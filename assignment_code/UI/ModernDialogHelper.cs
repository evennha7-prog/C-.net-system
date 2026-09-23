using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.Services.Security;

namespace assignment_code.UI
{
    public static class ModernDialogHelper
    {
        public static Form CreateBaseDialog(string title, int width, int height, IWin32Window owner = null)
        {
            var form = new Form
            {
                Text = title,
                ClientSize = new Size(width, height),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = ThemeManager.CardBackground,
                ForeColor = ThemeManager.TextPrimary,
                Font = FontHelper.CreateFont(9.5F, FontStyle.Regular)
            };

            DoubleBufferHelper.EnableDoubleBuffering(form);
            return form;
        }

        public static Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = FontHelper.CreateFont(8.75F, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent
            };
        }

        public static TextBox CreateTextBox(string text, int x, int y, int width, bool isPassword = false)
        {
            var tb = new TextBox
            {
                Text = text ?? "",
                Location = new Point(x, y),
                Width = width,
                Font = FontHelper.CreateFont(9.5F, FontStyle.Regular),
                BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254),
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = isPassword
            };
            return tb;
        }

        public static Button CreatePrimaryButton(string text, int x, int y, int width, int height = 36)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        public static Button CreateSecondaryButton(string text, int x, int y, int width, int height = 36)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = ThemeManager.IsDark ? Color.FromArgb(40, 50, 75) : Color.FromArgb(241, 245, 249),
                ForeColor = ThemeManager.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font = FontHelper.CreateFont(9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            return btn;
        }

        // 1. Modern Product Dialog
        public static Product ShowProductDialog(IWin32Window owner, Product existing = null)
        {
            bool isEdit = existing != null;
            string title = isEdit ? $"Edit Product: {existing.Name}" : "Add New Product";

            using (Form f = CreateBaseDialog(title, 460, 480, owner))
            {
                int x = 24;
                int fullW = f.ClientSize.Width - (x * 2);
                int halfW = (fullW - 16) / 2;

                // Name
                f.Controls.Add(CreateFieldLabel("Product Name *", x, 18));
                var txtName = CreateTextBox(existing?.Name ?? "", x, 38, fullW);
                f.Controls.Add(txtName);

                // Category
                f.Controls.Add(CreateFieldLabel("Category *", x, 74));
                var cboCat = new ComboBox
                {
                    Location = new Point(x, 94),
                    Width = fullW,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FontHelper.CreateFont(9.5F, FontStyle.Regular),
                    BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254),
                    ForeColor = ThemeManager.TextPrimary
                };
                foreach (var c in StoreDataService.Instance.Categories) cboCat.Items.Add(c.Name);
                if (existing != null) cboCat.SelectedItem = existing.Category;
                if (cboCat.SelectedIndex < 0 && cboCat.Items.Count > 0) cboCat.SelectedIndex = 0;
                f.Controls.Add(cboCat);

                // SKU & Stock (side by side)
                f.Controls.Add(CreateFieldLabel("SKU Code *", x, 130));
                var txtSku = CreateTextBox(existing?.Sku ?? $"PRD-{new Random().Next(100, 999)}", x, 150, halfW);
                f.Controls.Add(txtSku);

                f.Controls.Add(CreateFieldLabel("Initial Stock Qty *", x + halfW + 16, 130));
                var txtStock = CreateTextBox(existing != null ? existing.Stock.ToString() : "50", x + halfW + 16, 150, halfW);
                f.Controls.Add(txtStock);

                // Selling Price & Cost Price (side by side)
                f.Controls.Add(CreateFieldLabel("Selling Price ($) *", x, 186));
                var txtPrice = CreateTextBox(existing != null ? existing.Price.ToString("N2") : "19.99", x, 206, halfW);
                f.Controls.Add(txtPrice);

                f.Controls.Add(CreateFieldLabel("Cost Price ($)", x + halfW + 16, 186));
                var txtCost = CreateTextBox(existing != null ? existing.Cost.ToString("N2") : "12.00", x + halfW + 16, 206, halfW);
                f.Controls.Add(txtCost);

                // Buttons
                int btnY = f.ClientSize.Height - 54;
                var btnCancel = CreateSecondaryButton("Cancel", x + fullW - 200, btnY, 90);
                btnCancel.DialogResult = DialogResult.Cancel;

                var btnSave = CreatePrimaryButton(isEdit ? "Update Product" : "Create Product", x + fullW - 100, btnY, 100);
                btnSave.DialogResult = DialogResult.OK;

                f.Controls.AddRange(new Control[] { btnCancel, btnSave });
                f.AcceptButton = btnSave;
                f.CancelButton = btnCancel;

                if (f.ShowDialog(owner) == DialogResult.OK)
                {
                    string name = txtName.Text.Trim();
                    if (string.IsNullOrEmpty(name)) return null;

                    decimal.TryParse(txtPrice.Text, out decimal price);
                    decimal.TryParse(txtCost.Text, out decimal cost);
                    int.TryParse(txtStock.Text, out int stock);

                    var p = existing ?? new Product();
                    p.Name = name;
                    p.Category = cboCat.SelectedItem?.ToString() ?? "General";
                    p.Sku = string.IsNullOrWhiteSpace(txtSku.Text) ? $"PRD-{new Random().Next(100, 999)}" : txtSku.Text.Trim();
                    p.Price = price > 0 ? price : 10.00m;
                    p.Cost = cost > 0 ? cost : 5.00m;
                    p.Stock = stock >= 0 ? stock : 0;
                    p.Status = p.Stock > 10 ? "In Stock" : (p.Stock > 0 ? "Low Stock" : "Out of Stock");

                    return p;
                }
            }
            return null;
        }

        // 2. Modern Receipt / Invoice Modal
        public static void ShowReceiptDialog(IWin32Window owner, SaleTransaction sale, List<CartItem> items)
        {
            if (sale == null) return;

            using (Form f = CreateBaseDialog($"Receipt - Invoice #{sale.InvoiceNo}", 440, 620, owner))
            {
                Panel paper = new Panel
                {
                    Location = new Point(18, 18),
                    Size = new Size(404, 530),
                    BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.White
                };
                DoubleBufferHelper.EnableDoubleBuffering(paper);

                paper.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    GraphicsHelper.SetHighQuality(g);

                    Rectangle r = new Rectangle(0, 0, paper.Width - 1, paper.Height - 1);
                    using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(r, 10))
                    {
                        using (SolidBrush bg = new SolidBrush(paper.BackColor)) g.FillPath(bg, path);
                        using (Pen p = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(p, path);
                    }

                    // Store Brand Header
                    string store = EnvLoader.Get("STORE_NAME", "PCCFPI STORE");
                    using (Font hFont = FontHelper.CreateFont(13F, FontStyle.Bold))
                    using (SolidBrush tb = new SolidBrush(ThemeManager.TextPrimary))
                    {
                        SizeF ssz = g.MeasureString(store, hFont);
                        g.DrawString(store, hFont, tb, (paper.Width - ssz.Width) / 2f, 16);
                    }

                    string sub = "Official POS Customer Receipt";
                    using (Font sFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
                    using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    {
                        SizeF ssz = g.MeasureString(sub, sFont);
                        g.DrawString(sub, sFont, sb, (paper.Width - ssz.Width) / 2f, 38);
                    }

                    // Divider
                    using (Pen dp = new Pen(ThemeManager.SubtleDivider, 1f) { DashStyle = DashStyle.Dash })
                        g.DrawLine(dp, 20, 60, paper.Width - 20, 60);

                    // Metadata
                    string cashier = StoreDataService.Instance.CurrentUser?.FullName ?? "Cashier";
                    using (Font mFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
                    using (SolidBrush mb = new SolidBrush(ThemeManager.TextSecondary))
                    {
                        g.DrawString($"Invoice: {sale.InvoiceNo}", mFont, mb, 20, 70);
                        g.DrawString($"Date: {sale.Timestamp:yyyy-MM-dd HH:mm}", mFont, mb, 20, 88);
                        g.DrawString($"Cashier: {cashier}", mFont, mb, 20, 106);
                        g.DrawString($"Customer: {sale.CustomerName}", mFont, mb, 20, 124);
                    }

                    // Table Header
                    int tableY = 150;
                    using (Pen dp = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawLine(dp, 20, tableY, paper.Width - 20, tableY);

                    using (Font thFont = FontHelper.CreateFont(8.5F, FontStyle.Bold))
                    using (SolidBrush thb = new SolidBrush(ThemeManager.TextPrimary))
                    {
                        g.DrawString("ITEM", thFont, thb, 20, tableY + 6);
                        g.DrawString("QTY", thFont, thb, 240, tableY + 6);
                        g.DrawString("PRICE", thFont, thb, 290, tableY + 6);
                        g.DrawString("TOTAL", thFont, thb, 345, tableY + 6);
                    }

                    using (Pen dp = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawLine(dp, 20, tableY + 26, paper.Width - 20, tableY + 26);

                    // Item rows
                    int rowY = tableY + 34;
                    using (Font itFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
                    using (SolidBrush itb = new SolidBrush(ThemeManager.TextPrimary))
                    {
                        var list = items != null && items.Count > 0 ? items : new List<CartItem>();
                        int count = 0;
                        foreach (var it in list)
                        {
                            if (count >= 5)
                            {
                                g.DrawString($"... and {list.Count - count} more item(s)", itFont, itb, 20, rowY);
                                rowY += 20;
                                break;
                            }
                            string name = it.Product?.Name ?? "Item";
                            if (name.Length > 25) name = name.Substring(0, 22) + "...";

                            g.DrawString(name, itFont, itb, 20, rowY);
                            g.DrawString($"x{it.Quantity}", itFont, itb, 240, rowY);
                            g.DrawString($"${it.UnitPrice:N2}", itFont, itb, 290, rowY);
                            g.DrawString($"${it.Subtotal:N2}", itFont, itb, 345, rowY);
                            rowY += 22;
                            count++;
                        }
                    }

                    // Divider before totals
                    int totalY = Math.Max(rowY + 10, 310);
                    using (Pen dp = new Pen(ThemeManager.SubtleDivider, 1f))
                        g.DrawLine(dp, 20, totalY, paper.Width - 20, totalY);

                    using (Font totLblFont = FontHelper.CreateFont(9F, FontStyle.Regular))
                    using (Font totValFont = FontHelper.CreateFont(9F, FontStyle.Bold))
                    using (Font grandFont = FontHelper.CreateFont(13F, FontStyle.Bold))
                    using (SolidBrush lb = new SolidBrush(ThemeManager.TextSecondary))
                    using (SolidBrush vb = new SolidBrush(ThemeManager.TextPrimary))
                    using (SolidBrush gb = new SolidBrush(ThemeManager.AccentBlue))
                    {
                        g.DrawString("Subtotal:", totLblFont, lb, 240, totalY + 8);
                        g.DrawString($"${sale.Subtotal:N2}", totValFont, vb, 340, totalY + 8);

                        g.DrawString("Tax (10%):", totLblFont, lb, 240, totalY + 28);
                        g.DrawString($"${sale.TaxAmount:N2}", totValFont, vb, 340, totalY + 28);

                        g.DrawString("Payment:", totLblFont, lb, 20, totalY + 8);
                        g.DrawString(sale.PaymentMethod, totValFont, vb, 90, totalY + 8);

                        using (Pen dp2 = new Pen(ThemeManager.BorderColor, 1.5f))
                            g.DrawLine(dp2, 20, totalY + 54, paper.Width - 20, totalY + 54);

                        g.DrawString("TOTAL PAID:", totValFont, vb, 20, totalY + 66);
                        g.DrawString($"${sale.TotalAmount:N2}", grandFont, gb, 290, totalY + 62);
                    }

                    // Barcode simulation
                    int barY = totalY + 110;
                    int barH = 34;
                    using (SolidBrush bb = new SolidBrush(ThemeManager.TextPrimary))
                    {
                        int bx = (paper.Width - 220) / 2;
                        int[] widths = new int[] { 2, 4, 1, 3, 2, 5, 1, 2, 4, 2, 1, 3, 4, 1, 2, 3, 5, 2, 1, 4, 2, 3, 1, 4, 2, 5, 1, 3, 2, 4 };
                        for (int i = 0; i < widths.Length; i++)
                        {
                            if (i % 2 == 0)
                            {
                                g.FillRectangle(bb, bx, barY, widths[i], barH);
                            }
                            bx += widths[i] + 3;
                        }

                        using (Font bcFont = FontHelper.CreateFont(8F, FontStyle.Regular))
                        {
                            string code = $"* {sale.InvoiceNo} *";
                            SizeF csz = g.MeasureString(code, bcFont);
                            g.DrawString(code, bcFont, bb, (paper.Width - csz.Width) / 2f, barY + barH + 4);
                        }
                    }
                };

                f.Controls.Add(paper);

                // Bottom actions
                var btnClose = CreatePrimaryButton("Done & Close", f.ClientSize.Width - 140, f.ClientSize.Height - 48, 120);
                btnClose.DialogResult = DialogResult.OK;

                var btnPrint = CreateSecondaryButton("🖨️ Print", f.ClientSize.Width - 260, f.ClientSize.Height - 48, 110);
                btnPrint.Click += (s, e) =>
                {
                    MessageBox.Show("Receipt sent to thermal printer.", "Print Job", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                f.Controls.AddRange(new Control[] { btnPrint, btnClose });
                f.AcceptButton = btnClose;

                f.ShowDialog(owner);
            }
        }

        // 3. Modern Category Dialog
        public static Category ShowCategoryDialog(IWin32Window owner, Category existing = null)
        {
            bool isEdit = existing != null;
            string title = isEdit ? $"Edit Category: {existing.Name}" : "Add New Category";

            using (Form f = CreateBaseDialog(title, 420, 340, owner))
            {
                int x = 24;
                int fullW = f.ClientSize.Width - (x * 2);

                f.Controls.Add(CreateFieldLabel("Category Name *", x, 18));
                var txtName = CreateTextBox(existing?.Name ?? "", x, 38, fullW);
                f.Controls.Add(txtName);

                f.Controls.Add(CreateFieldLabel("Description", x, 74));
                var txtDesc = CreateTextBox(existing?.Description ?? "", x, 94, fullW);
                f.Controls.Add(txtDesc);

                f.Controls.Add(CreateFieldLabel("Accent Color Hex (e.g. #3B82F6)", x, 130));
                var txtColor = CreateTextBox(existing?.ColorHex ?? "#3B82F6", x, 150, 160);
                f.Controls.Add(txtColor);

                int btnY = f.ClientSize.Height - 54;
                var btnCancel = CreateSecondaryButton("Cancel", x + fullW - 200, btnY, 90);
                btnCancel.DialogResult = DialogResult.Cancel;

                var btnSave = CreatePrimaryButton(isEdit ? "Update Category" : "Save Category", x + fullW - 100, btnY, 100);
                btnSave.DialogResult = DialogResult.OK;

                f.Controls.AddRange(new Control[] { btnCancel, btnSave });
                f.AcceptButton = btnSave;
                f.CancelButton = btnCancel;

                if (f.ShowDialog(owner) == DialogResult.OK)
                {
                    string name = txtName.Text.Trim();
                    if (string.IsNullOrEmpty(name)) return null;

                    var cat = existing ?? new Category();
                    cat.Name = name;
                    cat.Description = txtDesc.Text.Trim();
                    cat.ColorHex = string.IsNullOrWhiteSpace(txtColor.Text) ? "#3B82F6" : txtColor.Text.Trim();
                    return cat;
                }
            }
            return null;
        }

        // 4. Modern Customer Dialog
        public static Customer ShowCustomerDialog(IWin32Window owner, Customer existing = null)
        {
            bool isEdit = existing != null;
            string title = isEdit ? $"Edit Customer: {existing.FullName}" : "Add New Customer";

            using (Form f = CreateBaseDialog(title, 440, 420, owner))
            {
                int x = 24;
                int fullW = f.ClientSize.Width - (x * 2);
                int halfW = (fullW - 16) / 2;

                f.Controls.Add(CreateFieldLabel("Full Name *", x, 18));
                var txtName = CreateTextBox(existing?.FullName ?? "", x, 38, fullW);
                f.Controls.Add(txtName);

                f.Controls.Add(CreateFieldLabel("Email Address", x, 74));
                var txtEmail = CreateTextBox(existing?.Email ?? "", x, 94, fullW);
                f.Controls.Add(txtEmail);

                f.Controls.Add(CreateFieldLabel("Phone Number", x, 130));
                var txtPhone = CreateTextBox(existing?.Phone ?? "", x, 150, fullW);
                f.Controls.Add(txtPhone);

                // Tier & Status side by side
                f.Controls.Add(CreateFieldLabel("Membership Tier", x, 186));
                var cboTier = new ComboBox
                {
                    Location = new Point(x, 206),
                    Width = halfW,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FontHelper.CreateFont(9.5F, FontStyle.Regular),
                    BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254),
                    ForeColor = ThemeManager.TextPrimary
                };
                cboTier.Items.AddRange(new object[] { "Regular", "Silver", "Gold", "VIP" });
                cboTier.SelectedItem = existing?.Tier ?? "Regular";
                if (cboTier.SelectedIndex < 0) cboTier.SelectedIndex = 0;
                f.Controls.Add(cboTier);

                f.Controls.Add(CreateFieldLabel("Account Status", x + halfW + 16, 186));
                var cboStatus = new ComboBox
                {
                    Location = new Point(x + halfW + 16, 206),
                    Width = halfW,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FontHelper.CreateFont(9.5F, FontStyle.Regular),
                    BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254),
                    ForeColor = ThemeManager.TextPrimary
                };
                cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });
                cboStatus.SelectedItem = existing?.Status ?? "Active";
                if (cboStatus.SelectedIndex < 0) cboStatus.SelectedIndex = 0;
                f.Controls.Add(cboStatus);

                int btnY = f.ClientSize.Height - 54;
                var btnCancel = CreateSecondaryButton("Cancel", x + fullW - 200, btnY, 90);
                btnCancel.DialogResult = DialogResult.Cancel;

                var btnSave = CreatePrimaryButton(isEdit ? "Update Customer" : "Save Customer", x + fullW - 100, btnY, 100);
                btnSave.DialogResult = DialogResult.OK;

                f.Controls.AddRange(new Control[] { btnCancel, btnSave });
                f.AcceptButton = btnSave;
                f.CancelButton = btnCancel;

                if (f.ShowDialog(owner) == DialogResult.OK)
                {
                    string name = txtName.Text.Trim();
                    if (string.IsNullOrEmpty(name)) return null;

                    var c = existing ?? new Customer();
                    c.FullName = name;
                    c.Email = txtEmail.Text.Trim();
                    c.Phone = txtPhone.Text.Trim();
                    c.Tier = cboTier.SelectedItem?.ToString() ?? "Regular";
                    c.Status = cboStatus.SelectedItem?.ToString() ?? "Active";
                    return c;
                }
            }
            return null;
        }

        // 5. Modern User Dialog
        public static AppUser ShowUserDialog(IWin32Window owner, AppUser existing = null)
        {
            bool isEdit = existing != null;
            string title = isEdit ? $"Edit Staff: {existing.FullName}" : "Add New Staff Member";

            using (Form f = CreateBaseDialog(title, 440, 420, owner))
            {
                int x = 24;
                int fullW = f.ClientSize.Width - (x * 2);
                int halfW = (fullW - 16) / 2;

                f.Controls.Add(CreateFieldLabel("Full Name *", x, 18));
                var txtName = CreateTextBox(existing?.FullName ?? "", x, 38, fullW);
                f.Controls.Add(txtName);

                f.Controls.Add(CreateFieldLabel("Email Address *", x, 74));
                var txtEmail = CreateTextBox(existing?.Email ?? "", x, 94, fullW);
                f.Controls.Add(txtEmail);

                // Role & Status
                f.Controls.Add(CreateFieldLabel("Staff Role *", x, 130));
                var cboRole = new ComboBox
                {
                    Location = new Point(x, 150),
                    Width = halfW,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FontHelper.CreateFont(9.5F, FontStyle.Regular),
                    BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254),
                    ForeColor = ThemeManager.TextPrimary
                };
                cboRole.Items.AddRange(new object[] { "Cashier", "Store Manager", "Administrator", "Staff" });
                cboRole.SelectedItem = existing?.Role ?? "Cashier";
                if (cboRole.SelectedIndex < 0) cboRole.SelectedIndex = 0;
                f.Controls.Add(cboRole);

                f.Controls.Add(CreateFieldLabel("Account Status", x + halfW + 16, 130));
                var cboStatus = new ComboBox
                {
                    Location = new Point(x + halfW + 16, 150),
                    Width = halfW,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = FontHelper.CreateFont(9.5F, FontStyle.Regular),
                    BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254),
                    ForeColor = ThemeManager.TextPrimary
                };
                cboStatus.Items.AddRange(new object[] { "Active", "Suspended" });
                cboStatus.SelectedItem = existing?.Status ?? "Active";
                if (cboStatus.SelectedIndex < 0) cboStatus.SelectedIndex = 0;
                f.Controls.Add(cboStatus);

                f.Controls.Add(CreateFieldLabel(isEdit ? "Password (leave blank to keep unchanged)" : "Password *", x, 186));
                var txtPass = CreateTextBox(isEdit ? "" : "password123", x, 206, fullW, true);
                f.Controls.Add(txtPass);

                int btnY = f.ClientSize.Height - 54;
                var btnCancel = CreateSecondaryButton("Cancel", x + fullW - 200, btnY, 90);
                btnCancel.DialogResult = DialogResult.Cancel;

                var btnSave = CreatePrimaryButton(isEdit ? "Update Staff" : "Save Staff", x + fullW - 100, btnY, 100);
                btnSave.DialogResult = DialogResult.OK;

                f.Controls.AddRange(new Control[] { btnCancel, btnSave });
                f.AcceptButton = btnSave;
                f.CancelButton = btnCancel;

                if (f.ShowDialog(owner) == DialogResult.OK)
                {
                    string name = txtName.Text.Trim();
                    string email = txtEmail.Text.Trim();
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email)) return null;

                    var u = existing ?? new AppUser { Id = $"USR-{new Random().Next(100, 999)}", LastLogin = DateTime.Now };
                    u.FullName = name;
                    u.Email = email;
                    u.Role = cboRole.SelectedItem?.ToString() ?? "Cashier";
                    u.Status = cboStatus.SelectedItem?.ToString() ?? "Active";

                    string pass = txtPass.Text.Trim();
                    if (!string.IsNullOrEmpty(pass))
                    {
                        u.RawPassword = pass;
                        u.PasswordHash = PasswordSecurityHelper.HashPassword(pass);
                    }
                    return u;
                }
            }
            return null;
        }
    }
}
