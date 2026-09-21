using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI
{
    public static class DataGridViewStyleHelper
    {
        public static void ApplyStyle(DataGridView dgv)
        {
            if (dgv == null) return;

            DoubleBufferHelper.EnableDoubleBuffering(dgv);

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 44;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            UpdateColors(dgv);

            // Avoid duplicate subscriptions
            dgv.CellPainting -= Dgv_CellPainting;
            dgv.CellPainting += Dgv_CellPainting;

            dgv.CellMouseEnter -= Dgv_CellMouseEnter;
            dgv.CellMouseEnter += Dgv_CellMouseEnter;

            dgv.CellMouseLeave -= Dgv_CellMouseLeave;
            dgv.CellMouseLeave += Dgv_CellMouseLeave;
        }

        public static void UpdateColors(DataGridView dgv)
        {
            if (dgv == null) return;

            Color bg = ThemeManager.CardBackground;
            Color text = ThemeManager.TextPrimary;
            Color subText = ThemeManager.TextSecondary;
            Color gridLine = ThemeManager.GridLineColor;
            Color hoverBg = ThemeManager.HoverBackground;
            Color headerBg = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(243, 246, 251);

            dgv.BackgroundColor = bg;
            dgv.GridColor = gridLine;

            // Header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = subText;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Default cell style
            dgv.DefaultCellStyle.BackColor = bg;
            dgv.DefaultCellStyle.ForeColor = text;
            dgv.DefaultCellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Regular);
            dgv.DefaultCellStyle.SelectionBackColor = hoverBg;
            dgv.DefaultCellStyle.SelectionForeColor = text;
            dgv.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Alternating rows for subtle depth
            dgv.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.IsDark ? Color.FromArgb(30, 38, 60) : Color.FromArgb(250, 252, 255);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = text;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = hoverBg;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = text;

            dgv.Invalidate();
        }

        private static int _hoveredRow = -1;
        private static int _hoveredCol = -1;

        private static void Dgv_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView dgv && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var col = dgv.Columns[e.ColumnIndex];
                if (col is DataGridViewButtonColumn || IsActionColumn(col.HeaderText))
                {
                    dgv.Cursor = Cursors.Hand;
                    _hoveredRow = e.RowIndex;
                    _hoveredCol = e.ColumnIndex;
                    dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
                else
                {
                    dgv.Cursor = Cursors.Default;
                }
            }
        }

        private static void Dgv_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView dgv && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dgv.Cursor = Cursors.Default;
                if (_hoveredRow == e.RowIndex && _hoveredCol == e.ColumnIndex)
                {
                    _hoveredRow = -1;
                    _hoveredCol = -1;
                    dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
        }

        private static bool IsActionColumn(string headerText)
        {
            if (string.IsNullOrEmpty(headerText)) return false;
            string h = headerText.ToLowerInvariant();
            return h.Contains("edit") || h.Contains("delete") || h.Contains("action") || h.Contains("view");
        }

        private static void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || !(sender is DataGridView dgv)) return;

            string header = dgv.Columns[e.ColumnIndex].HeaderText?.Trim() ?? "";
            object val = e.Value;
            string valStr = val?.ToString()?.Trim() ?? "";

            // 1. Render Person Name Column with Initials Avatar (Users & Customers)
            bool isPersonName = header.Equals("Full Name", StringComparison.OrdinalIgnoreCase) ||
                                header.Equals("Customer Name", StringComparison.OrdinalIgnoreCase) ||
                                header.Equals("Staff Name", StringComparison.OrdinalIgnoreCase) ||
                                header.Equals(TranslationManager.T("FullName", "Full Name"), StringComparison.OrdinalIgnoreCase);

            if (isPersonName && !string.IsNullOrEmpty(valStr))
            {
                e.PaintBackground(e.CellBounds, true);

                int avatarSize = 28;
                int avatarX = e.CellBounds.X + 12;
                int avatarY = e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2;
                Rectangle avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

                GraphicsHelper.DrawInitialsAvatar(e.Graphics, avatarRect, valStr);

                // Draw Name Text
                int textX = avatarX + avatarSize + 10;
                int textY = e.CellBounds.Y + (e.CellBounds.Height - FontHelper.CreateFont(9F, FontStyle.Bold).Height) / 2;
                using (Font nameFont = FontHelper.CreateFont(9F, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    e.Graphics.DrawString(valStr, nameFont, textBrush, textX, textY);
                }

                e.Handled = true;
                return;
            }

            // 2. Render Status, Tier, and Role Columns as Modern Rounded Pill Badges
            if (header.Equals("Status", StringComparison.OrdinalIgnoreCase) || 
                header.Equals(TranslationManager.T("Status", "Status"), StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Tier", StringComparison.OrdinalIgnoreCase) ||
                header.Equals(TranslationManager.T("Tier", "Tier"), StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                header.Equals(TranslationManager.T("Role", "Role"), StringComparison.OrdinalIgnoreCase))
            {
                e.PaintBackground(e.CellBounds, true);

                Color pillBg;
                Color pillFg;
                GetStatusColors(valStr, out pillBg, out pillFg);

                using (Font font = FontHelper.CreateFont(8F, FontStyle.Bold))
                {
                    SizeF textSize = e.Graphics.MeasureString(valStr, font);
                    int pillWidth = (int)textSize.Width + 24;
                    int pillHeight = 24;
                    int pillX = e.CellBounds.X + 12;
                    int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;

                    Rectangle pillRect = new Rectangle(pillX, pillY, pillWidth, pillHeight);
                    using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(pillRect, 12))
                    using (SolidBrush bgBrush = new SolidBrush(pillBg))
                    using (SolidBrush fgBrush = new SolidBrush(pillFg))
                    {
                        GraphicsHelper.SetHighQuality(e.Graphics);
                        e.Graphics.FillPath(bgBrush, path);

                        // Draw indicator dot
                        int dotSize = 6;
                        int dotX = pillX + 8;
                        int dotY = pillY + (pillHeight - dotSize) / 2;
                        e.Graphics.FillEllipse(fgBrush, dotX, dotY, dotSize, dotSize);

                        // Draw status text
                        e.Graphics.DrawString(valStr, font, fgBrush, dotX + dotSize + 5, pillY + (pillHeight - font.Height) / 2);
                    }
                }

                e.Handled = true;
                return;
            }

            // 3. Render Action Buttons (Edit / Delete / View) as Flat Rounded Buttons
            var col = dgv.Columns[e.ColumnIndex];
            if (col is DataGridViewButtonColumn || IsActionColumn(header))
            {
                e.PaintBackground(e.CellBounds, true);

                bool isHovered = (_hoveredRow == e.RowIndex && _hoveredCol == e.ColumnIndex);
                bool isDelete = header.IndexOf("delete", StringComparison.OrdinalIgnoreCase) >= 0 || valStr.IndexOf("delete", StringComparison.OrdinalIgnoreCase) >= 0;
                bool isEdit = header.IndexOf("edit", StringComparison.OrdinalIgnoreCase) >= 0 || valStr.IndexOf("edit", StringComparison.OrdinalIgnoreCase) >= 0;

                Color btnBg;
                Color btnFg;
                Color btnBorder;
                string displayText;

                if (isDelete)
                {
                    displayText = "Delete";
                    btnBg = isHovered 
                        ? (ThemeManager.IsDark ? Color.FromArgb(90, 30, 30) : Color.FromArgb(254, 205, 205))
                        : (ThemeManager.IsDark ? Color.FromArgb(55, 25, 25) : Color.FromArgb(254, 226, 226));
                    btnFg = Color.FromArgb(239, 68, 68);
                    btnBorder = Color.FromArgb(248, 113, 113);
                }
                else if (isEdit)
                {
                    displayText = "Edit";
                    btnBg = isHovered
                        ? (ThemeManager.IsDark ? Color.FromArgb(35, 60, 110) : Color.FromArgb(219, 234, 254))
                        : (ThemeManager.IsDark ? Color.FromArgb(28, 44, 78) : Color.FromArgb(235, 242, 255));
                    btnFg = ThemeManager.AccentBlue;
                    btnBorder = Color.FromArgb(147, 197, 253);
                }
                else
                {
                    displayText = string.IsNullOrEmpty(valStr) ? "View" : valStr;
                    btnBg = isHovered ? ThemeManager.HoverBackground : ThemeManager.CardBackground;
                    btnFg = ThemeManager.TextPrimary;
                    btnBorder = ThemeManager.BorderColor;
                }

                int btnW = Math.Min(e.CellBounds.Width - 16, 76);
                int btnH = 26;
                int btnX = e.CellBounds.X + (e.CellBounds.Width - btnW) / 2;
                int btnY = e.CellBounds.Y + (e.CellBounds.Height - btnH) / 2;
                Rectangle btnRect = new Rectangle(btnX, btnY, btnW, btnH);

                GraphicsHelper.SetHighQuality(e.Graphics);

                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(btnRect, 7))
                using (SolidBrush bgBrush = new SolidBrush(btnBg))
                using (Pen borderPen = new Pen(btnBorder, 1f))
                using (SolidBrush fgBrush = new SolidBrush(btnFg))
                using (Font btnFont = FontHelper.CreateFont(8.5F, FontStyle.Bold))
                {
                    e.Graphics.FillPath(bgBrush, path);
                    e.Graphics.DrawPath(borderPen, path);

                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(displayText, btnFont, fgBrush, btnRect, sf);
                }

                e.Handled = true;
                return;
            }
        }

        private static void GetStatusColors(string status, out Color bg, out Color fg)
        {
            string s = status?.ToLowerInvariant() ?? "";

            if (s.Contains("administrator") || s.Contains("admin"))
            {
                bg = ThemeManager.IsDark ? Color.FromArgb(50, 30, 80) : Color.FromArgb(243, 232, 255);
                fg = Color.FromArgb(147, 51, 234); // Purple
            }
            else if (s.Contains("store manager") || s.Contains("manager"))
            {
                bg = ThemeManager.IsDark ? Color.FromArgb(58, 42, 20) : Color.FromArgb(254, 243, 199);
                fg = Color.FromArgb(217, 119, 6); // Amber
            }
            else if (s.Contains("cashier"))
            {
                bg = ThemeManager.IsDark ? Color.FromArgb(20, 45, 75) : Color.FromArgb(224, 242, 254);
                fg = Color.FromArgb(2, 132, 199); // Sky
            }
            else if (s.Contains("vip") || s.Contains("platinum"))
            {
                bg = ThemeManager.IsDark ? Color.FromArgb(50, 30, 80) : Color.FromArgb(245, 235, 255);
                fg = Color.FromArgb(139, 92, 246);
            }
            else if (s.Contains("in stock") || s.Contains("active") || s.Contains("completed") || s.Contains("paid"))
            {
                bg = ThemeManager.SuccessGreenBg;
                fg = ThemeManager.SuccessGreen;
            }
            else if (s.Contains("low stock") || s.Contains("pending") || s.Contains("gold"))
            {
                bg = ThemeManager.WarningYellowBg;
                fg = ThemeManager.WarningYellow;
            }
            else if (s.Contains("out of stock") || s.Contains("cancelled") || s.Contains("inactive") || s.Contains("suspended") || s.Contains("refunded"))
            {
                bg = ThemeManager.DangerRedBg;
                fg = ThemeManager.DangerRed;
            }
            else // Regular, New, Silver, General
            {
                bg = ThemeManager.IsDark ? Color.FromArgb(35, 45, 75) : Color.FromArgb(235, 242, 255);
                fg = ThemeManager.AccentBlue;
            }
        }
    }
}
