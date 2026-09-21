using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class PosView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private Panel _leftProductArea;
        private FlowLayoutPanel _categoryPillsPanel;
        private FlowLayoutPanel _productsGrid;
        private Panel _rightCartPanel;
        private FlowLayoutPanel _cartItemsList;
        private Panel _emptyCartPanel;
        private Panel _summaryBoxPanel;
        private Label _lblSubtotal;
        private Label _lblTax;
        private Label _lblTotal;
        private Label _lblCust;
        private Label _lblPay;
        private ComboBox _cboPayment;
        private TextBox _txtCustomer;
        private Button _btnCheckout;
        private Button _btnClearCart;
        private string _selectedCategory = "All";

        public PosView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.Background;

            InitializeComponent();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            RefreshView();

            _dataService.CartChanged += (s, e) => UpdateCartUI();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void RefreshView()
        {
            UpdateTranslations();
            PopulateCategoryPills();
            PopulateProducts();
            UpdateCartUI();
            ApplyTheme();
            Invalidate(true);
        }

        private void InitializeComponent()
        {
            // 1. Left Product Area
            _leftProductArea = new Panel
            {
                Location = new Point(0, 10),
                BackColor = Color.Transparent
            };
            Controls.Add(_leftProductArea);

            _categoryPillsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Height = 44,
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoScroll = false
            };
            _leftProductArea.Controls.Add(_categoryPillsPanel);

            _productsGrid = new FlowLayoutPanel
            {
                Location = new Point(0, 50),
                BackColor = Color.Transparent,
                AutoScroll = true,
                WrapContents = true
            };
            _leftProductArea.Controls.Add(_productsGrid);

            // 2. Right Cart Panel
            _rightCartPanel = new Panel
            {
                BackColor = ThemeManager.CardBackground
            };
            Controls.Add(_rightCartPanel);

            _rightCartPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _rightCartPanel.Width - 1, _rightCartPanel.Height - 1);
                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(r, 14))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground))
                        g.FillPath(bg, path);
                    using (Pen p = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawPath(p, path);
                }

                // Cart Header (Clean vector cart icon + text, avoid emoji box)
                int cartIconSize = 18;
                Rectangle cartIconRect = new Rectangle(16, 16, cartIconSize, cartIconSize);
                GraphicsHelper.DrawIcon(g, "cart", cartIconRect, ThemeManager.AccentBlue, 1.8f);

                string headerTitle = TranslationManager.T("CurrentOrder", "Current Order");
                using (Font hFont = FontHelper.CreateFontForText(headerTitle, 10.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(headerTitle, hFont, sb, 40, 15);

                // Cart Item Count Badge
                int totalItems = _dataService.CurrentCart.Sum(i => i.Quantity);
                string countBadge = $"{totalItems} {(totalItems == 1 ? "item" : "items")}";
                using (Font bFont = FontHelper.CreateFont(8F, FontStyle.Bold))
                using (SolidBrush bBg = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(40, 52, 80) : Color.FromArgb(235, 242, 255)))
                using (SolidBrush bFg = new SolidBrush(ThemeManager.AccentBlue))
                {
                    SizeF bSize = g.MeasureString(countBadge, bFont);
                    int badgeX = _rightCartPanel.Width - (int)bSize.Width - 28;
                    Rectangle badgeRect = new Rectangle(badgeX, 14, (int)bSize.Width + 14, 22);
                    using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(badgeRect, 8))
                        g.FillPath(bBg, bp);
                    g.DrawString(countBadge, bFont, bFg, badgeX + 7, 17);
                }

                // Header separator line
                using (Pen sp = new Pen(ThemeManager.SubtleDivider, 1f))
                    g.DrawLine(sp, 16, 46, _rightCartPanel.Width - 16, 46);
            };

            // Empty Cart State Panel (Beautiful centered vector graphic without overlap)
            _emptyCartPanel = new Panel
            {
                BackColor = Color.Transparent,
                Visible = true
            };
            _emptyCartPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);

                int cx = _emptyCartPanel.Width / 2;
                int cy = Math.Max(70, _emptyCartPanel.Height / 2 - 10);

                // Centered circular backdrop for shopping bag
                int circleSize = 64;
                Rectangle circleRect = new Rectangle(cx - (circleSize / 2), cy - 50, circleSize, circleSize);
                Color circleBg = ThemeManager.IsDark ? Color.FromArgb(36, 48, 72) : Color.FromArgb(238, 244, 255);
                using (SolidBrush cb = new SolidBrush(circleBg))
                {
                    g.FillEllipse(cb, circleRect);
                }

                // Centered vector shopping bag icon
                int iconInnerSize = 30;
                Rectangle iconInnerRect = new Rectangle(circleRect.X + (circleSize - iconInnerSize) / 2, circleRect.Y + (circleSize - iconInnerSize) / 2, iconInnerSize, iconInnerSize);
                GraphicsHelper.DrawIcon(g, "shoppingbag", iconInnerRect, ThemeManager.AccentBlue, 2f);

                // Empty text Title (Bold)
                string emptyTitle = TranslationManager.T("CartIsEmpty", "Cart is Empty");
                using (Font tFont = FontHelper.CreateFontForText(emptyTitle, 10.5F, FontStyle.Bold))
                using (SolidBrush tb = new SolidBrush(ThemeManager.TextPrimary))
                {
                    SizeF tsz = g.MeasureString(emptyTitle, tFont);
                    g.DrawString(emptyTitle, tFont, tb, cx - (tsz.Width / 2), circleRect.Bottom + 12);
                }

                // Empty text Subtitle (Regular)
                string emptySub = TranslationManager.T("TapToAddToOrder", "Click any product on the left to add");
                using (Font sFont = FontHelper.CreateFontForText(emptySub, 8.5F, FontStyle.Regular))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                {
                    SizeF ssz = g.MeasureString(emptySub, sFont);
                    g.DrawString(emptySub, sFont, sb, cx - (ssz.Width / 2), circleRect.Bottom + 38);
                }
            };
            _rightCartPanel.Controls.Add(_emptyCartPanel);

            // Active Cart Items List
            _cartItemsList = new FlowLayoutPanel
            {
                Location = new Point(14, 52),
                AutoScroll = true,
                BackColor = Color.Transparent,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown
            };
            _rightCartPanel.Controls.Add(_cartItemsList);

            // Financial Summary Card Container
            _summaryBoxPanel = new Panel
            {
                BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254)
            };
            _summaryBoxPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _summaryBoxPanel.Width - 1, _summaryBoxPanel.Height - 1);
                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(r, 10))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254)))
                        g.FillPath(bg, path);
                    using (Pen p = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawPath(p, path);
                }

                // Divider line above total
                using (Pen dp = new Pen(ThemeManager.SubtleDivider, 1f))
                    g.DrawLine(dp, 12, 60, _summaryBoxPanel.Width - 12, 60);
            };

            _lblSubtotal = new Label { Font = FontHelper.CreateFont(8.75F, FontStyle.Regular), AutoSize = true, BackColor = Color.Transparent };
            _lblTax = new Label { Font = FontHelper.CreateFont(8.75F, FontStyle.Regular), AutoSize = true, BackColor = Color.Transparent };
            _lblTotal = new Label { Font = FontHelper.CreateFont(11.5F, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent, ForeColor = ThemeManager.AccentBlue };

            _summaryBoxPanel.Controls.AddRange(new Control[] { _lblSubtotal, _lblTax, _lblTotal });
            _rightCartPanel.Controls.Add(_summaryBoxPanel);

            // Customer & Payment controls (Stacked cleanly for proper label width and spacing)
            _lblCust = new Label { Text = TranslationManager.T("CustomerName", "Customer Name:"), Font = FontHelper.CreateFont(8.5F, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            _txtCustomer = new TextBox { Text = "Walk-in Customer", Font = FontHelper.CreateFont(9F, FontStyle.Regular) };

            _lblPay = new Label { Text = TranslationManager.T("PaymentMethod", "Payment Method:"), Font = FontHelper.CreateFont(8.5F, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
            _cboPayment = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = FontHelper.CreateFont(9F, FontStyle.Regular) };
            _cboPayment.Items.AddRange(new object[] { "Cash", "Credit Card", "QR Pay" });
            _cboPayment.SelectedIndex = 0;

            _btnCheckout = new Button
            {
                Text = TranslationManager.T("CompleteSale", "Complete Sale (Checkout)"),
                Font = FontHelper.CreateFont(9.5F, FontStyle.Bold),
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnCheckout.FlatAppearance.BorderSize = 0;
            _btnCheckout.Click += BtnCheckout_Click;

            _btnClearCart = new Button
            {
                Text = TranslationManager.T("ClearCart", "Clear Cart"),
                Font = FontHelper.CreateFont(8.5F, FontStyle.Bold),
                Height = 22,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ThemeManager.TextSecondary,
                Cursor = Cursors.Hand
            };
            _btnClearCart.FlatAppearance.BorderSize = 0;
            _btnClearCart.Click += (s, e) => _dataService.ClearCart();

            _rightCartPanel.Controls.AddRange(new Control[] { _lblCust, _txtCustomer, _lblPay, _cboPayment, _btnCheckout, _btnClearCart });

            Resize += (s, e) => RepositionContent();
            RepositionContent();
        }

        private void PopulateCategoryPills()
        {
            _categoryPillsPanel.Controls.Clear();
            var categories = new[] { "All" }.Concat(_dataService.Categories.Select(c => c.Name)).ToArray();

            foreach (var cat in categories)
            {
                string catName = cat == "All" ? TranslationManager.T("All", "All") : TranslationManager.T(cat, cat);
                string displayText = $"● {catName}";

                Button btn = new Button
                {
                    Text = displayText,
                    Font = FontHelper.CreateFontForText(displayText, 9F, FontStyle.Bold),
                    Height = 34,
                    AutoSize = true,
                    Margin = new Padding(0, 0, 8, 0),
                    Padding = new Padding(10, 2, 10, 2),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                bool isSelected = (cat == _selectedCategory);
                btn.BackColor = isSelected ? ThemeManager.AccentBlue : ThemeManager.CardBackground;
                btn.ForeColor = isSelected ? Color.White : (cat == "All" ? ThemeManager.TextPrimary : GetCategoryColor(cat));

                btn.Click += (s, e) =>
                {
                    _selectedCategory = cat;
                    PopulateCategoryPills();
                    PopulateProducts();
                };

                _categoryPillsPanel.Controls.Add(btn);
            }
        }

        private void PopulateProducts()
        {
            _productsGrid.Controls.Clear();
            var filtered = _dataService.Products.AsEnumerable();
            if (_selectedCategory != "All")
                filtered = filtered.Where(p => p.Category == _selectedCategory);

            foreach (var prd in filtered)
            {
                var card = CreateProductPosCard(prd);
                _productsGrid.Controls.Add(card);
            }
        }

        private Control CreateProductPosCard(Product prd)
        {
            Color catColor = GetCategoryColor(prd.Category);
            string emoji = GetProductEmoji(prd);

            Panel card = new Panel
            {
                Size = new Size(195, 246),
                Margin = new Padding(0, 0, 16, 16),
                BackColor = ThemeManager.CardBackground,
                Cursor = Cursors.Hand
            };
            DoubleBufferHelper.EnableDoubleBuffering(card);

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);

                // Card background and border
                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground))
                        g.FillPath(bg, path);
                    using (Pen p = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawPath(p, path);
                }

                // Top Hero Visual Banner
                Rectangle topBanner = new Rectangle(10, 10, card.Width - 20, 72);
                using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(topBanner, 10))
                {
                    Color bannerBg = ThemeManager.IsDark
                        ? Color.FromArgb(40, catColor.R / 4, catColor.G / 4, catColor.B / 4)
                        : Color.FromArgb(24, catColor);
                    using (SolidBrush sb = new SolidBrush(bannerBg))
                        g.FillPath(sb, bp);
                }

                // Emoji Icon in Hero Banner
                using (Font eFont = new Font("Segoe UI Emoji", 26F))
                using (SolidBrush eb = new SolidBrush(Color.Black))
                {
                    SizeF eSize = g.MeasureString(emoji, eFont);
                    g.DrawString(emoji, eFont, eb, 10 + (topBanner.Width - eSize.Width) / 2, 10 + (topBanner.Height - eSize.Height) / 2);
                }

                // Category Tag Pill (Translated)
                string catTrans = TranslationManager.T(prd.Category, prd.Category);
                using (Font fCat = FontHelper.CreateFontForText(catTrans, 7F, FontStyle.Bold))
                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(30, catColor)))
                using (SolidBrush cFg = new SolidBrush(catColor))
                {
                    SizeF tSize = g.MeasureString(catTrans, fCat);
                    Rectangle tagRect = new Rectangle(10, 90, (int)tSize.Width + 10, 18);
                    using (GraphicsPath tp = GraphicsHelper.GetRoundedRectanglePath(tagRect, 6))
                        g.FillPath(cBg, tp);
                    g.DrawString(catTrans, fCat, cFg, 15, 92);
                }

                // Product Name (2 full lines without vertical clipping)
                using (Font fName = FontHelper.CreateFontForText(prd.Name, 9.25F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                {
                    RectangleF nameRect = new RectangleF(10, 114, card.Width - 20, 44);
                    g.DrawString(prd.Name, fName, sb, nameRect);
                }

                // Stock Indicator Pill
                using (Font fStk = FontHelper.CreateFont(8F, FontStyle.Regular))
                {
                    Color stkCol = prd.Stock > 10 ? ThemeManager.SuccessGreen : (prd.Stock > 0 ? ThemeManager.WarningYellow : ThemeManager.DangerRed);
                    string stockSuffix = prd.Stock > 0 ? TranslationManager.T("InStock", "in stock") : TranslationManager.T("OutOfStock", "Out of stock");
                    string stkText = prd.Stock > 0 ? $"● {prd.Stock} {stockSuffix}" : $"● {stockSuffix}";

                    using (SolidBrush sb = new SolidBrush(stkCol))
                        g.DrawString(stkText, fStk, sb, 10, 164);
                }

                // Price (Bold & Crisp)
                using (Font fPrice = FontHelper.CreateFont(13F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.AccentBlue))
                    g.DrawString($"${prd.Price:N2}", fPrice, sb, 10, 196);
            };

            Button btnAdd = new Button
            {
                Text = TranslationManager.T("Add", "+ Add"),
                Font = FontHelper.CreateFont(8.5F, FontStyle.Bold),
                Size = new Size(66, 32),
                Location = new Point(card.Width - 76, 192),
                FlatStyle = FlatStyle.Flat,
                BackColor = prd.Stock > 0 ? ThemeManager.AccentBlue : Color.FromArgb(160, 160, 160),
                ForeColor = Color.White,
                Cursor = prd.Stock > 0 ? Cursors.Hand : Cursors.Default,
                Enabled = prd.Stock > 0
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Paint += (s, e) =>
            {
                Graphics bg = e.Graphics;
                GraphicsHelper.SetHighQuality(bg);
                Rectangle br = new Rectangle(0, 0, btnAdd.Width - 1, btnAdd.Height - 1);
                using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(br, 6))
                {
                    using (SolidBrush bgb = new SolidBrush(btnAdd.BackColor))
                        bg.FillPath(bgb, bp);
                }
                string bText = btnAdd.Text;
                using (Font bf = FontHelper.CreateFontForText(bText, 8.5F, FontStyle.Bold))
                using (SolidBrush tb = new SolidBrush(btnAdd.ForeColor))
                {
                    SizeF bsz = bg.MeasureString(bText, bf);
                    bg.DrawString(bText, bf, tb, (btnAdd.Width - bsz.Width) / 2f, (btnAdd.Height - bsz.Height) / 2f);
                }
            };
            btnAdd.Click += (s, e) => _dataService.AddToCart(prd);

            card.Controls.Add(btnAdd);
            card.Click += (s, e) => { if (prd.Stock > 0) _dataService.AddToCart(prd); };

            return card;
        }

        private void UpdateTranslations()
        {
            if (_lblCust != null) _lblCust.Text = TranslationManager.T("CustomerName", "Customer Name:");
            if (_lblPay != null) _lblPay.Text = TranslationManager.T("PaymentMethod", "Payment Method:");
            if (_btnCheckout != null) _btnCheckout.Text = TranslationManager.T("CompleteSale", "Complete Sale (Checkout)");
            if (_btnClearCart != null) _btnClearCart.Text = TranslationManager.T("ClearCart", "Clear Cart");
            PopulateCategoryPills();
        }

        private void UpdateCartUI()
        {
            _cartItemsList.Controls.Clear();
            decimal subtotal = 0m;

            bool isEmpty = _dataService.CurrentCart.Count == 0;
            _emptyCartPanel.Visible = isEmpty;
            _cartItemsList.Visible = !isEmpty;

            foreach (var item in _dataService.CurrentCart)
            {
                subtotal += item.Subtotal;
                var row = CreateCartRow(item);
                _cartItemsList.Controls.Add(row);
            }

            decimal tax = Math.Round(subtotal * (_dataService.Settings.TaxRatePercentage / 100m), 2);
            decimal total = subtotal + tax;

            string subText = TranslationManager.T("Subtotal", "Subtotal");
            string taxText = TranslationManager.T("Tax", "Tax");
            string totText = TranslationManager.T("Total", "Total");

            _lblSubtotal.Text = $"{subText}:   ${subtotal:N2}";
            _lblTax.Text = $"{taxText} ({_dataService.Settings.TaxRatePercentage}%):   ${tax:N2}";
            _lblTotal.Text = $"{totText}:   ${total:N2}";

            _btnCheckout.Enabled = !isEmpty;
            if (!isEmpty)
            {
                string btnText = TranslationManager.T("CompleteSale", "Complete Sale");
                _btnCheckout.Text = $"{btnText} (${total:N2})";
            }
            else
            {
                _btnCheckout.Text = TranslationManager.T("CompleteSale", "Complete Sale (Checkout)");
            }

            _rightCartPanel.Invalidate();
            _emptyCartPanel.Invalidate();
            _summaryBoxPanel.Invalidate();
        }

        private Control CreateCartRow(CartItem item)
        {
            Color catColor = GetCategoryColor(item.Product.Category);
            string emoji = GetProductEmoji(item.Product);

            Panel row = new Panel
            {
                Size = new Size(_rightCartPanel.Width - 32, 68),
                Margin = new Padding(0, 0, 0, 8),
                BackColor = ThemeManager.IsDark ? Color.FromArgb(28, 36, 56) : Color.FromArgb(245, 248, 255)
            };
            DoubleBufferHelper.EnableDoubleBuffering(row);

            row.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, row.Width - 1, row.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 8))
                {
                    using (SolidBrush bg = new SolidBrush(row.BackColor)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(bp, p);
                }

                // Category indicator line on left
                using (SolidBrush cb = new SolidBrush(catColor))
                    g.FillRectangle(cb, 2, 8, 4, 52);

                // Small emoji avatar
                using (Font eFont = new Font("Segoe UI Emoji", 14F))
                using (SolidBrush eb = new SolidBrush(Color.Black))
                    g.DrawString(emoji, eFont, eb, 10, 18);
            };

            Label lblTitle = new Label
            {
                Text = item.Product.Name,
                Font = FontHelper.CreateFontForText(item.Product.Name, 9F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(40, 6),
                Size = new Size(row.Width - 72, 32),
                BackColor = Color.Transparent
            };

            Label lblPrice = new Label
            {
                Text = $"${item.UnitPrice:N2}",
                Font = FontHelper.CreateFont(8.5F),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(40, 40),
                Size = new Size(75, 20),
                BackColor = Color.Transparent
            };

            Button btnMinus = new Button
            {
                Text = "-",
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(22, 22),
                Location = new Point(120, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.HoverBackground,
                ForeColor = ThemeManager.TextPrimary,
                Cursor = Cursors.Hand
            };
            btnMinus.FlatAppearance.BorderSize = 0;
            btnMinus.Click += (s, e) => _dataService.ChangeCartQuantity(item.Product.Id, -1);

            Label lblQty = new Label
            {
                Text = item.Quantity.ToString(),
                Font = FontHelper.CreateFont(8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(24, 22),
                Location = new Point(144, 38),
                BackColor = Color.Transparent
            };

            Button btnPlus = new Button
            {
                Text = "+",
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(22, 22),
                Location = new Point(170, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.HoverBackground,
                ForeColor = ThemeManager.TextPrimary,
                Cursor = Cursors.Hand
            };
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.Click += (s, e) => _dataService.ChangeCartQuantity(item.Product.Id, 1);

            Label lblItemSub = new Label
            {
                Text = $"${item.Subtotal:N2}",
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(100, 22),
                Location = new Point(row.Width - 110, 38),
                BackColor = Color.Transparent
            };

            Button btnRemove = new Button
            {
                Text = "✕",
                Font = FontHelper.CreateFont(7.5F, FontStyle.Bold),
                Size = new Size(20, 20),
                Location = new Point(row.Width - 26, 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ThemeManager.DangerRed,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) => _dataService.RemoveFromCart(item.Product.Id);

            row.Controls.AddRange(new Control[] { lblTitle, lblPrice, btnMinus, lblQty, btnPlus, lblItemSub, btnRemove });
            return row;
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_dataService.CurrentCart == null || _dataService.CurrentCart.Count == 0)
            {
                MessageBox.Show("Your cart is empty. Please add items before checking out.", "Cart Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string customer = _txtCustomer.Text.Trim();
            string payment = _cboPayment.SelectedItem?.ToString() ?? "Cash";

            var cartSnapshot = _dataService.CurrentCart.Select(c => new CartItem { Product = c.Product, Quantity = c.Quantity }).ToList();
            var sale = _dataService.CheckoutCart(customer, payment);
            if (sale != null)
            {
                ModernDialogHelper.ShowReceiptDialog(this, sale, cartSnapshot);
                PopulateProducts();
            }
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_leftProductArea != null) _leftProductArea.BackColor = ThemeManager.Background;
            if (_categoryPillsPanel != null) _categoryPillsPanel.BackColor = ThemeManager.Background;
            if (_productsGrid != null) _productsGrid.BackColor = ThemeManager.Background;

            if (_rightCartPanel != null) _rightCartPanel.BackColor = ThemeManager.CardBackground;
            if (_summaryBoxPanel != null) _summaryBoxPanel.BackColor = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(248, 250, 254);
            if (_lblSubtotal != null) _lblSubtotal.ForeColor = ThemeManager.TextSecondary;
            if (_lblTax != null) _lblTax.ForeColor = ThemeManager.TextSecondary;
            if (_lblTotal != null) _lblTotal.ForeColor = ThemeManager.AccentBlue;
            if (_btnCheckout != null) _btnCheckout.BackColor = ThemeManager.AccentBlue;
            PopulateCategoryPills();
            PopulateProducts();
            UpdateCartUI();
            Invalidate(true);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                RepositionContent();
            }
        }

        private void RepositionContent()
        {
            int totalW = Math.Max(ClientSize.Width, Parent != null ? Parent.ClientSize.Width : 0);
            int totalH = Math.Max(ClientSize.Height, Parent != null ? Parent.ClientSize.Height : 0);
            if (totalW < 400) return;

            int rightW = Math.Max(320, (int)(totalW * 0.30f));
            int leftW = totalW - rightW - 18;

            _leftProductArea.SetBounds(0, 10, leftW, totalH - 20);
            _categoryPillsPanel.Width = leftW;
            _productsGrid.SetBounds(0, 48, leftW, totalH - 78);

            _rightCartPanel.SetBounds(leftW + 14, 10, rightW, totalH - 20);

            int summaryH = 92;
            int custH = 26;
            int payH = 26;
            int chkH = 40;
            int clrH = 22;

            int bottomNeededH = summaryH + 10 + 20 + custH + 6 + 20 + payH + 10 + chkH + 4 + clrH + 14;
            int summaryY = Math.Max(120, _rightCartPanel.Height - bottomNeededH);
            int listH = Math.Max(60, summaryY - 60);

            _cartItemsList.SetBounds(14, 52, rightW - 28, listH);
            _emptyCartPanel.SetBounds(14, 52, rightW - 28, listH);

            _summaryBoxPanel.SetBounds(14, summaryY, rightW - 28, summaryH);
            _lblSubtotal.Location = new Point(14, 10);
            _lblTax.Location = new Point(14, 34);
            _lblTotal.Location = new Point(14, 64);

            int custLabelY = summaryY + summaryH + 10;
            _lblCust.Location = new Point(14, custLabelY);
            _txtCustomer.SetBounds(14, custLabelY + 20, rightW - 28, custH);

            int payLabelY = custLabelY + 20 + custH + 6;
            _lblPay.Location = new Point(14, payLabelY);
            _cboPayment.SetBounds(14, payLabelY + 20, rightW - 28, payH);

            int btnY = payLabelY + 20 + payH + 10;
            _btnCheckout.SetBounds(14, btnY, rightW - 28, chkH);
            _btnClearCart.SetBounds(14, btnY + chkH + 4, rightW - 28, clrH);
        }

        private string GetCategoryIcon(string category)
        {
            switch (category?.ToLower())
            {
                case "all": return "✨";
                case "electronics": return "💻";
                case "beverages": return "☕";
                case "snacks": return "🍿";
                case "apparel": return "👕";
                case "stationery": return "📝";
                case "home goods": return "🏠";
                default: return "📦";
            }
        }

        private Color GetCategoryColor(string category)
        {
            switch (category?.ToLower())
            {
                case "electronics": return Color.FromArgb(59, 130, 246);
                case "beverages": return Color.FromArgb(16, 185, 129);
                case "snacks": return Color.FromArgb(245, 158, 11);
                case "apparel": return Color.FromArgb(139, 92, 246);
                case "stationery": return Color.FromArgb(236, 72, 153);
                case "home goods": return Color.FromArgb(6, 182, 212);
                default: return ThemeManager.AccentBlue;
            }
        }

        private string GetProductEmoji(Product p)
        {
            if (p == null) return "📦";
            string sku = p.Sku?.ToUpper() ?? "";
            string name = p.Name?.ToLower() ?? "";
            if (sku.Contains("ELE-001") || name.Contains("headphone")) return "🎧";
            if (sku.Contains("ELE-002") || name.Contains("watch")) return "⌚";
            if (sku.Contains("ELE-003") || name.Contains("charger")) return "🔌";
            if (sku.Contains("BEV-001") || name.Contains("coffee") || name.Contains("cold brew")) return "☕";
            if (sku.Contains("BEV-002") || name.Contains("matcha") || name.Contains("tea")) return "🍵";
            if (sku.Contains("SNK-001") || name.Contains("almond") || name.Contains("nut")) return "🥜";
            if (sku.Contains("SNK-002") || name.Contains("chocolate")) return "🍫";
            if (sku.Contains("APP-001") || name.Contains("tee") || name.Contains("shirt")) return "👕";
            if (sku.Contains("APP-002") || name.Contains("cardholder") || name.Contains("wallet")) return "💳";
            if (sku.Contains("STA-001") || name.Contains("journal") || name.Contains("book")) return "📔";

            return GetCategoryIcon(p.Category);
        }
    }
}
