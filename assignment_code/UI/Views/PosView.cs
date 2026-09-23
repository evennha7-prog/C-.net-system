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
        private Panel _posFilterRowPanel;
        private ModernSearchBox _txtPosSearch;
        private ComboBox _cboStockFilter;
        private Label _lblAvailableCount;
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
        private string _searchQuery = "";
        private string _stockFilter = "All";

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

            _dataService.CartChanged += (s, e) =>
            {
                UpdateCartUI();
                _productsGrid?.Invalidate(true);
            };
            _dataService.DataRefreshed += (s, e) => RefreshView();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void ApplySearch(string query)
        {
            _searchQuery = query ?? "";
            if (_txtPosSearch != null && _txtPosSearch.Text != _searchQuery)
            {
                _txtPosSearch.Text = _searchQuery;
            }
            PopulateProducts();
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
                WrapContents = true,
                AutoScroll = false,
                Margin = new Padding(0),
                Padding = new Padding(0, 2, 0, 4)
            };
            _leftProductArea.Controls.Add(_categoryPillsPanel);

            // Filter & Search Toolbar Row
            _posFilterRowPanel = new Panel
            {
                Location = new Point(0, 42),
                Height = 36,
                BackColor = Color.Transparent
            };
            _leftProductArea.Controls.Add(_posFilterRowPanel);

            _txtPosSearch = new ModernSearchBox
            {
                PlaceholderText = TranslationManager.T("SearchProductsPos", "Search products or SKU..."),
                Size = new Size(220, 32),
                Location = new Point(0, 2)
            };
            _txtPosSearch.SearchTextChanged += (s, e) =>
            {
                _searchQuery = _txtPosSearch.Text.Trim();
                PopulateProducts();
            };
            _posFilterRowPanel.Controls.Add(_txtPosSearch);

            _cboStockFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                Size = new Size(130, 28),
                Location = new Point(228, 4),
                BackColor = ThemeManager.CardBackground,
                ForeColor = ThemeManager.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _cboStockFilter.Items.AddRange(new object[] {
                TranslationManager.T("All", "All"),
                TranslationManager.T("InStock", "In Stock"),
                TranslationManager.T("LowStock", "Low Stock"),
                TranslationManager.T("OutOfStock", "Out of Stock")
            });
            _cboStockFilter.SelectedIndex = 0;
            _cboStockFilter.SelectedIndexChanged += (s, e) =>
            {
                switch (_cboStockFilter.SelectedIndex)
                {
                    case 1: _stockFilter = "InStock"; break;
                    case 2: _stockFilter = "LowStock"; break;
                    case 3: _stockFilter = "OutOfStock"; break;
                    default: _stockFilter = "All"; break;
                }
                PopulateProducts();
            };
            _posFilterRowPanel.Controls.Add(_cboStockFilter);

            _lblAvailableCount = new Label
            {
                Text = "",
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                AutoSize = true,
                Location = new Point(368, 8),
                BackColor = Color.Transparent
            };
            _posFilterRowPanel.Controls.Add(_lblAvailableCount);

            _productsGrid = new FlowLayoutPanel
            {
                Location = new Point(0, 82),
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
            if (_categoryPillsPanel == null) return;
            _categoryPillsPanel.SuspendLayout();
            _categoryPillsPanel.Controls.Clear();
            var categories = new[] { "All" }.Concat(_dataService.Categories.Select(c => c.Name)).ToArray();

            int allCount = _dataService.Products.Count;

            foreach (var cat in categories)
            {
                int count = cat == "All" ? allCount : _dataService.Products.Count(p => string.Equals(p.Category, cat, StringComparison.OrdinalIgnoreCase));
                string catName = cat == "All" ? TranslationManager.T("All", "All") : TranslationManager.T(cat, cat);

                var pill = new PosCategoryPillButton
                {
                    CategoryKey = cat,
                    CategoryTitle = catName,
                    ProductCount = count,
                    CategoryColor = GetCategoryColor(cat),
                    IsSelected = (cat == _selectedCategory),
                    Margin = new Padding(0, 0, 8, 6)
                };

                pill.Click += (s, e) =>
                {
                    _selectedCategory = cat;
                    PopulateCategoryPills();
                    PopulateProducts();
                };

                _categoryPillsPanel.Controls.Add(pill);
            }
            _categoryPillsPanel.ResumeLayout(true);
            AdjustLeftProductAreaLayout();
        }

        private void PopulateProducts()
        {
            if (_productsGrid == null) return;
            _productsGrid.SuspendLayout();
            _productsGrid.Controls.Clear();

            var filtered = _dataService.Products.AsEnumerable();

            // 1. Category Filter
            if (_selectedCategory != "All")
                filtered = filtered.Where(p => string.Equals(p.Category, _selectedCategory, StringComparison.OrdinalIgnoreCase));

            // 2. Stock Filter
            if (_stockFilter == "InStock")
                filtered = filtered.Where(p => p.Stock > 0);
            else if (_stockFilter == "LowStock")
                filtered = filtered.Where(p => p.Stock > 0 && p.Stock <= 10);
            else if (_stockFilter == "OutOfStock")
                filtered = filtered.Where(p => p.Stock <= 0);

            // 3. Search Filter
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.Trim().ToLowerInvariant();
                filtered = filtered.Where(p =>
                    (p.Name != null && p.Name.ToLowerInvariant().Contains(q)) ||
                    (p.Sku != null && p.Sku.ToLowerInvariant().Contains(q)) ||
                    (p.Category != null && p.Category.ToLowerInvariant().Contains(q)) ||
                    TranslationManager.T(p.Name, p.Name).ToLowerInvariant().Contains(q) ||
                    TranslationManager.T(p.Category, p.Category).ToLowerInvariant().Contains(q) ||
                    p.Price.ToString("N2").Contains(q)
                );
            }

            // 4. Logical Sort: In-Stock items first, out-of-stock items at the end
            var productList = filtered
                .OrderByDescending(p => p.Stock > 0)
                .ThenBy(p => p.Category)
                .ThenBy(p => p.Sku)
                .ToList();

            if (_lblAvailableCount != null)
            {
                string countSuffix = TranslationManager.T("ItemsAvailable", "products available");
                _lblAvailableCount.Text = $"{productList.Count} {countSuffix}";
            }

            if (productList.Count == 0)
            {
                ShowEmptyProductsState();
            }
            else
            {
                foreach (var prd in productList)
                {
                    var card = CreateProductPosCard(prd);
                    _productsGrid.Controls.Add(card);
                }
            }

            _productsGrid.ResumeLayout(true);
        }

        private void ShowEmptyProductsState()
        {
            Panel emptyPanel = new Panel
            {
                Size = new Size(Math.Max(300, _productsGrid.Width - 30), 220),
                BackColor = Color.Transparent,
                Margin = new Padding(10, 30, 10, 10)
            };

            emptyPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);

                Rectangle iconRect = new Rectangle((emptyPanel.Width - 48) / 2, 20, 48, 48);
                GraphicsHelper.DrawIcon(g, "search", iconRect, ThemeManager.TextSecondary, 2.2f);

                string msg = TranslationManager.T("NoProductsFound", "No products found matching your search.");
                using (Font f = FontHelper.CreateFontForText(msg, 10F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                {
                    SizeF sz = g.MeasureString(msg, f);
                    g.DrawString(msg, f, sb, (emptyPanel.Width - sz.Width) / 2, 80);
                }
            };

            Button btnReset = new Button
            {
                Text = TranslationManager.T("ResetFilter", "Reset Filter"),
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(150, 34),
                Location = new Point((emptyPanel.Width - 150) / 2, 120),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) =>
            {
                _searchQuery = "";
                _stockFilter = "All";
                _selectedCategory = "All";
                if (_txtPosSearch != null) _txtPosSearch.Text = "";
                if (_cboStockFilter != null) _cboStockFilter.SelectedIndex = 0;
                PopulateCategoryPills();
                PopulateProducts();
            };
            emptyPanel.Controls.Add(btnReset);

            _productsGrid.Controls.Add(emptyPanel);
        }

        private Control CreateProductPosCard(Product prd)
        {
            Color catColor = GetCategoryColor(prd.Category);
            bool inStock = prd.Stock > 0;

            Panel card = new Panel
            {
                Size = new Size(198, 252),
                Margin = new Padding(0, 0, 16, 16),
                BackColor = ThemeManager.CardBackground,
                Cursor = inStock ? Cursors.Hand : Cursors.Default
            };
            DoubleBufferHelper.EnableDoubleBuffering(card);

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);

                // Card background and border (muted background if out of stock)
                Color cardBg = inStock
                    ? ThemeManager.CardBackground
                    : (ThemeManager.IsDark ? Color.FromArgb(24, 28, 38) : Color.FromArgb(246, 248, 252));

                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(cardBg))
                        g.FillPath(bg, path);
                    using (Pen p = new Pen(inStock ? ThemeManager.BorderColor : ThemeManager.SubtleDivider, 1f))
                        g.DrawPath(p, path);
                }

                // Top Hero Visual Banner with Vector Illustration
                Rectangle topBanner = new Rectangle(10, 10, card.Width - 20, 72);
                DrawProductIllustration(g, prd, topBanner, catColor);

                // Cart badge if product is already in cart
                var inCart = _dataService.CurrentCart.FirstOrDefault(c => c.Product.Id == prd.Id);
                int cartQty = inCart?.Quantity ?? 0;
                if (cartQty > 0)
                {
                    string cartBadge = $"✓ {cartQty} {TranslationManager.T("InCartBadge", "in cart")}";
                    using (Font bFont = FontHelper.CreateFontForText(cartBadge, 7.5F, FontStyle.Bold))
                    {
                        SizeF bSize = g.MeasureString(cartBadge, bFont);
                        int bW = (int)bSize.Width + 12;
                        Rectangle bRect = new Rectangle(card.Width - 10 - bW - 4, 14, bW, 20);
                        using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(bRect, 6))
                        using (SolidBrush bgBrush = new SolidBrush(ThemeManager.AccentBlue))
                        using (SolidBrush fgBrush = new SolidBrush(Color.White))
                        {
                            g.FillPath(bgBrush, bp);
                            g.DrawString(cartBadge, bFont, fgBrush, bRect.X + 6, bRect.Y + 2);
                        }
                    }
                }

                // Category Tag Pill (Translated)
                string catTrans = TranslationManager.T(prd.Category, prd.Category);
                using (Font fCat = FontHelper.CreateFontForText(catTrans, 7F, FontStyle.Bold))
                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(30, catColor)))
                using (SolidBrush cFg = new SolidBrush(catColor))
                {
                    SizeF tSize = g.MeasureString(catTrans, fCat);
                    Rectangle tagRect = new Rectangle(10, 88, (int)tSize.Width + 10, 18);
                    using (GraphicsPath tp = GraphicsHelper.GetRoundedRectanglePath(tagRect, 6))
                        g.FillPath(cBg, tp);
                    g.DrawString(catTrans, fCat, cFg, 15, 90);
                }

                // Product Name (Bilingual Hanuman Typography)
                bool isKhmer = TranslationManager.CurrentLanguage == AppLanguage.Khmer;
                string khmerName = TranslationManager.T(prd.Name, prd.Name);
                string mainTitle = isKhmer ? khmerName : prd.Name;
                string subTitle = isKhmer ? $"{prd.Sku} • {prd.Name}" : $"{prd.Sku} • {catTrans}";

                using (Font fMain = FontHelper.CreateFontForText(mainTitle, 9.5F, FontStyle.Bold))
                using (SolidBrush sbMain = new SolidBrush(inStock ? ThemeManager.TextPrimary : ThemeManager.TextSecondary))
                {
                    RectangleF titleRect = new RectangleF(10, 110, card.Width - 20, 22);
                    StringFormat sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                    g.DrawString(mainTitle, fMain, sbMain, titleRect, sf);
                }

                using (Font fSub = FontHelper.CreateFontForText(subTitle, 7.75F, FontStyle.Regular))
                using (SolidBrush sbSub = new SolidBrush(ThemeManager.TextSecondary))
                {
                    RectangleF subRect = new RectangleF(10, 134, card.Width - 20, 18);
                    StringFormat sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                    g.DrawString(subTitle, fSub, sbSub, subRect, sf);
                }

                // Stock Indicator Pill
                using (Font fStk = FontHelper.CreateFont(8F, FontStyle.Regular))
                {
                    Color stkCol = prd.Stock > 10 ? ThemeManager.SuccessGreen : (prd.Stock > 0 ? ThemeManager.WarningYellow : ThemeManager.DangerRed);
                    string stockSuffix = prd.Stock > 10 ? TranslationManager.T("InStock", "in stock") : (prd.Stock > 0 ? TranslationManager.T("LowStock", "Low Stock") : TranslationManager.T("OutOfStock", "Out of stock"));
                    string stkText = prd.Stock > 0 ? $"● {prd.Stock} {stockSuffix}" : $"● {stockSuffix}";

                    using (SolidBrush sb = new SolidBrush(stkCol))
                        g.DrawString(stkText, fStk, sb, 10, 160);
                }

                // Price (Bold & Crisp)
                using (Font fPrice = FontHelper.CreateFont(13F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(inStock ? ThemeManager.AccentBlue : ThemeManager.TextSecondary))
                    g.DrawString($"${prd.Price:N2}", fPrice, sb, 10, 196);
            };

            string btnText = inStock ? ("+ " + TranslationManager.T("Add", "Add")) : TranslationManager.T("OutOfStockBtn", "Out of Stock");
            Button btnAdd = new Button
            {
                Text = btnText,
                Font = FontHelper.CreateFontForText(btnText, 8.5F, FontStyle.Bold),
                Size = new Size(76, 32),
                Location = new Point(card.Width - 86, 194),
                FlatStyle = FlatStyle.Flat,
                BackColor = inStock ? ThemeManager.AccentBlue : Color.FromArgb(160, 165, 175),
                ForeColor = Color.White,
                Cursor = inStock ? Cursors.Hand : Cursors.Default,
                Enabled = inStock
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

            Action handleAdd = () =>
            {
                if (!inStock) return;
                var existing = _dataService.CurrentCart.FirstOrDefault(c => c.Product.Id == prd.Id);
                if (existing != null && existing.Quantity >= prd.Stock)
                {
                    MessageBox.Show(TranslationManager.T("MaxStockReached", "Maximum stock reached in cart."), "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _dataService.AddToCart(prd);
                card.Invalidate(true);
            };

            btnAdd.Click += (s, e) => handleAdd();
            card.Click += (s, e) => handleAdd();

            card.Controls.Add(btnAdd);
            return card;
        }

        private void UpdateTranslations()
        {
            if (_lblCust != null) _lblCust.Text = TranslationManager.T("CustomerName", "Customer Name:");
            if (_lblPay != null) _lblPay.Text = TranslationManager.T("PaymentMethod", "Payment Method:");
            if (_btnCheckout != null) _btnCheckout.Text = TranslationManager.T("CompleteSale", "Complete Sale (Checkout)");
            if (_btnClearCart != null) _btnClearCart.Text = TranslationManager.T("ClearCart", "Clear Cart");
            if (_txtPosSearch != null) _txtPosSearch.PlaceholderText = TranslationManager.T("SearchProductsPos", "Search products or SKU...");

            if (_cboStockFilter != null)
            {
                int curIdx = _cboStockFilter.SelectedIndex;
                _cboStockFilter.Items.Clear();
                _cboStockFilter.Items.AddRange(new object[] {
                    TranslationManager.T("All", "All"),
                    TranslationManager.T("InStock", "In Stock"),
                    TranslationManager.T("LowStock", "Low Stock"),
                    TranslationManager.T("OutOfStock", "Out of Stock")
                });
                _cboStockFilter.SelectedIndex = (curIdx >= 0 && curIdx < _cboStockFilter.Items.Count) ? curIdx : 0;
            }
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
            if (_posFilterRowPanel != null) _posFilterRowPanel.BackColor = ThemeManager.Background;
            if (_productsGrid != null) _productsGrid.BackColor = ThemeManager.Background;
            if (_cboStockFilter != null)
            {
                _cboStockFilter.BackColor = ThemeManager.CardBackground;
                _cboStockFilter.ForeColor = ThemeManager.TextPrimary;
            }
            if (_lblAvailableCount != null) _lblAvailableCount.ForeColor = ThemeManager.TextSecondary;

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
            AdjustLeftProductAreaLayout();

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

        private void AdjustLeftProductAreaLayout()
        {
            if (_categoryPillsPanel == null || _leftProductArea == null) return;

            int leftW = _leftProductArea.Width;
            if (leftW <= 0) return;

            _categoryPillsPanel.Width = leftW;
            _categoryPillsPanel.PerformLayout();

            int maxBottom = 0;
            foreach (Control c in _categoryPillsPanel.Controls)
            {
                if (c.Bottom > maxBottom) maxBottom = c.Bottom;
            }
            int pillsHeight = Math.Max(44, maxBottom + 4);
            _categoryPillsPanel.Height = pillsHeight;

            int filterY = _categoryPillsPanel.Bottom + 6;
            if (_posFilterRowPanel != null)
            {
                _posFilterRowPanel.SetBounds(0, filterY, leftW, 36);
                if (_lblAvailableCount != null)
                {
                    _lblAvailableCount.Location = new Point(Math.Max(380, Math.Min(leftW - 170, 380)), 8);
                }
            }

            int gridY = (_posFilterRowPanel != null ? _posFilterRowPanel.Bottom : _categoryPillsPanel.Bottom) + 6;
            int gridH = Math.Max(100, _leftProductArea.Height - gridY);
            if (_productsGrid != null)
            {
                _productsGrid.SetBounds(0, gridY, leftW, gridH);
            }
        }

        private void DrawProductIllustration(Graphics g, Product prd, Rectangle bounds, Color catColor)
        {
            GraphicsHelper.SetHighQuality(g);

            // 1. Hero background banner
            using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(bounds, 10))
            {
                Color bannerBg = ThemeManager.IsDark
                    ? Color.FromArgb(45, catColor.R / 4, catColor.G / 4, catColor.B / 4)
                    : Color.FromArgb(28, catColor);
                using (SolidBrush sb = new SolidBrush(bannerBg))
                    g.FillPath(sb, bp);
            }

            // 2. Central circular badge
            int circleSize = 44;
            int cx = bounds.X + (bounds.Width - circleSize) / 2;
            int cy = bounds.Y + (bounds.Height - circleSize) / 2;
            Rectangle badgeRect = new Rectangle(cx, cy, circleSize, circleSize);

            using (SolidBrush cBrush = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(60, catColor.R / 3, catColor.G / 3, catColor.B / 3) : Color.White))
            {
                g.FillEllipse(cBrush, badgeRect);
            }
            using (Pen cPen = new Pen(Color.FromArgb(80, catColor), 1.5f))
            {
                g.DrawEllipse(cPen, badgeRect);
            }

            // 3. Crisp Vector Icon inside the badge
            Rectangle iconRect = new Rectangle(cx + 10, cy + 10, 24, 24);
            string sku = prd.Sku?.ToUpper() ?? "";
            string name = prd.Name?.ToLower() ?? "";

            using (Pen pen = new Pen(catColor, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (SolidBrush brush = new SolidBrush(catColor))
            {
                int x = iconRect.X;
                int y = iconRect.Y;
                int w = iconRect.Width;
                int h = iconRect.Height;

                if (sku.Contains("ELE-001") || name.Contains("headphone"))
                {
                    // Headphones: headband + ear pads
                    g.DrawArc(pen, x + 2, y + 2, w - 4, h - 6, 180, 180);
                    using (GraphicsPath lp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + 1, y + 10, 5, 11), 2))
                        g.FillPath(brush, lp);
                    using (GraphicsPath rp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + w - 6, y + 10, 5, 11), 2))
                        g.FillPath(brush, rp);
                }
                else if (sku.Contains("ELE-002") || name.Contains("watch"))
                {
                    // Smartwatch: watch body + top & bottom straps
                    g.DrawLine(pen, x + 7, y + 1, x + 7, y + 5);
                    g.DrawLine(pen, x + w - 7, y + 1, x + w - 7, y + 5);
                    g.DrawLine(pen, x + 7, y + h - 5, x + 7, y + h - 1);
                    g.DrawLine(pen, x + w - 7, y + h - 5, x + w - 7, y + h - 1);
                    using (GraphicsPath wp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + 4, y + 5, w - 8, h - 10), 4))
                    {
                        g.DrawPath(pen, wp);
                    }
                    g.FillEllipse(brush, x + w / 2f - 2, y + h / 2f - 2, 4, 4);
                }
                else if (sku.Contains("ELE-003") || name.Contains("charger"))
                {
                    // GaN Fast Charger: Plug prongs + square + lightning
                    g.DrawLine(pen, x + 6, y + 2, x + 6, y + 7);
                    g.DrawLine(pen, x + w - 6, y + 2, x + w - 6, y + 7);
                    using (GraphicsPath cp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + 3, y + 7, w - 6, h - 9), 3))
                    {
                        g.DrawPath(pen, cp);
                    }
                    var bolt = new PointF[] {
                        new PointF(x + w * 0.55f, y + 9),
                        new PointF(x + w * 0.40f, y + 14),
                        new PointF(x + w * 0.52f, y + 14),
                        new PointF(x + w * 0.45f, y + 20),
                        new PointF(x + w * 0.62f, y + 13),
                        new PointF(x + w * 0.50f, y + 13)
                    };
                    g.FillPolygon(brush, bolt);
                }
                else if (sku.Contains("BEV-001") || name.Contains("coffee") || name.Contains("cold brew"))
                {
                    // Coffee cup + steam
                    g.DrawLine(pen, x + 4, y + 8, x + 5, y + h - 4);
                    g.DrawLine(pen, x + 5, y + h - 4, x + w - 8, y + h - 4);
                    g.DrawLine(pen, x + w - 8, y + h - 4, x + w - 7, y + 8);
                    g.DrawLine(pen, x + 4, y + 8, x + w - 7, y + 8);
                    g.DrawArc(pen, x + w - 8, y + 9, 6, 7, -90, 180);
                    g.DrawLine(pen, x + 8, y + 5, x + 8, y + 2);
                    g.DrawLine(pen, x + 13, y + 5, x + 13, y + 2);
                }
                else if (sku.Contains("BEV-002") || name.Contains("matcha") || name.Contains("tea"))
                {
                    // Matcha bowl / teacup + leaf
                    g.DrawArc(pen, x + 3, y + 6, w - 6, h - 10, 0, 180);
                    g.DrawLine(pen, x + 3, y + 11, x + w - 3, y + 11);
                    g.DrawLine(pen, x + 6, y + h - 4, x + w - 6, y + h - 4);
                    g.DrawArc(pen, x + w / 2f - 4, y + 2, 8, 6, 180, 180);
                }
                else if (sku.Contains("SNK-001") || name.Contains("almond") || name.Contains("nut"))
                {
                    // Almond snack bowl / nut
                    g.DrawEllipse(pen, x + 3, y + 6, 8, 12);
                    g.DrawEllipse(pen, x + 12, y + 6, 8, 12);
                    g.DrawArc(pen, x + 2, y + 12, w - 4, 9, 0, 180);
                }
                else if (sku.Contains("SNK-002") || name.Contains("chocolate"))
                {
                    // Chocolate Bar
                    using (GraphicsPath chp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + 4, y + 3, w - 8, h - 6), 2))
                    {
                        g.DrawPath(pen, chp);
                    }
                    g.DrawLine(pen, x + 4, y + h / 2f, x + w - 4, y + h / 2f);
                    g.DrawLine(pen, x + w / 2f, y + 3, x + w / 2f, y + h - 3);
                }
                else if (sku.Contains("APP-001") || name.Contains("tee") || name.Contains("shirt"))
                {
                    // T-shirt silhouette
                    var tPoints = new PointF[] {
                        new PointF(x + 7, y + 3),
                        new PointF(x + 2, y + 8),
                        new PointF(x + 5, y + 11),
                        new PointF(x + 7, y + 9),
                        new PointF(x + 7, y + h - 3),
                        new PointF(x + w - 7, y + h - 3),
                        new PointF(x + w - 7, y + 9),
                        new PointF(x + w - 5, y + 11),
                        new PointF(x + w - 2, y + 8),
                        new PointF(x + w - 7, y + 3),
                        new PointF(x + w * 0.62f, y + 3),
                        new PointF(x + w * 0.50f, y + 6),
                        new PointF(x + w * 0.38f, y + 3)
                    };
                    g.DrawPolygon(pen, tPoints);
                }
                else if (sku.Contains("APP-002") || name.Contains("cardholder") || name.Contains("wallet"))
                {
                    // Cardholder: wallet body + card
                    g.DrawRectangle(pen, x + 5, y + 4, w - 10, 6);
                    using (GraphicsPath vp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + 3, y + 8, w - 6, h - 11), 3))
                    {
                        g.DrawPath(pen, vp);
                    }
                    g.DrawLine(pen, x + 3, y + 14, x + w - 3, y + 14);
                }
                else if (sku.Contains("STA-001") || name.Contains("journal") || name.Contains("book"))
                {
                    // Hardcover Journal
                    using (GraphicsPath jp = GraphicsHelper.GetRoundedRectanglePath(new Rectangle(x + 4, y + 3, w - 8, h - 6), 2))
                    {
                        g.DrawPath(pen, jp);
                    }
                    g.DrawLine(pen, x + 8, y + 3, x + 8, y + h - 3);
                    g.DrawLine(pen, x + 11, y + 7, x + w - 7, y + 7);
                    g.DrawLine(pen, x + 11, y + 11, x + w - 7, y + 11);
                    g.DrawLine(pen, x + 11, y + 15, x + w - 9, y + 15);
                }
                else
                {
                    GraphicsHelper.DrawIcon(g, "products", iconRect, catColor, 2f);
                }
            }
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

