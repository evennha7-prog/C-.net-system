using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class ProductsView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private string _searchFilter = "";
        private string _categoryFilter = "All Categories";

        // Layout Containers
        private Panel _contentWrapper;
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private ModernButton _btnAddProduct;

        // KPI Metric Cards
        private Panel _metricsPanel;
        private ManagementStatCard _cardTotalPrd;
        private ManagementStatCard _cardInStock;
        private ManagementStatCard _cardLowStock;
        private ManagementStatCard _cardTotalVal;

        // Toolbar Card
        private RoundedPanel _toolbarCard;
        private ModernSearchBox _searchBox;
        private ModernComboBox _cboCategory;
        private ModernButton _btnRefresh;
        private Label _lblCount;

        // Data Table Card
        private RoundedPanel _gridCard;
        private DataGridView _grid;
        private Label _lblEmptyState;

        public ProductsView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = ThemeManager.Background;
            AutoScroll = true;

            InitializeLayout();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            DataGridViewStyleHelper.ApplyStyle(_grid);

            PopulateCategories();
            RefreshGrid();

            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) =>
            {
                UpdateTranslations();
                RefreshGrid();
            };

            ApplyTheme();
            UpdateTranslations();
        }

        private void InitializeLayout()
        {
            _contentWrapper = new Panel
            {
                Location = new Point(0, 0),
                Width = ClientSize.Width,
                Height = 850,
                BackColor = ThemeManager.Background
            };
            Controls.Add(_contentWrapper);
            Resize += (s, e) => RepositionContent();

            // 1. Header Area
            _headerPanel = new Panel
            {
                BackColor = ThemeManager.Background,
                Height = 48
            };
            _lblTitle = new Label
            {
                Text = "Products & Inventory",
                Font = FontHelper.CreateFont(15F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            _lblSubtitle = new Label
            {
                Text = "Manage store catalog, stock quantities, categories, and retail pricing.",
                Font = FontHelper.CreateFont(8.75F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 26)
            };
            _btnAddProduct = new ModernButton
            {
                Text = "Add New Product",
                IconName = "plus",
                ButtonType = ModernButtonType.Primary,
                Size = new Size(185, 38)
            };
            _btnAddProduct.Click += BtnAddProduct_Click;

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_btnAddProduct);
            _contentWrapper.Controls.Add(_headerPanel);

            // 2. Metrics Row
            _metricsPanel = new Panel
            {
                BackColor = ThemeManager.Background,
                Height = 78
            };
            _cardTotalPrd = new ManagementStatCard { IconName = "products", AccentColor = Color.FromArgb(59, 130, 246) };
            _cardInStock = new ManagementStatCard { IconName = "check", AccentColor = Color.FromArgb(16, 185, 129) };
            _cardLowStock = new ManagementStatCard { IconName = "box", AccentColor = Color.FromArgb(245, 158, 11) };
            _cardTotalVal = new ManagementStatCard { IconName = "dollar", AccentColor = Color.FromArgb(139, 92, 246) };

            _metricsPanel.Controls.AddRange(new Control[] { _cardTotalPrd, _cardInStock, _cardLowStock, _cardTotalVal });
            _contentWrapper.Controls.Add(_metricsPanel);

            // 3. Toolbar Card
            _toolbarCard = new RoundedPanel
            {
                BorderRadius = 12,
                Height = 56,
                Padding = new Padding(12, 9, 12, 9)
            };

            _searchBox = new ModernSearchBox
            {
                PlaceholderText = "Search products or SKU...",
                Size = new Size(260, 36)
            };
            _searchBox.SearchTextChanged += (s, e) =>
            {
                _searchFilter = _searchBox.Text.Trim();
                RefreshGrid();
            };

            _cboCategory = new ModernComboBox
            {
                Size = new Size(180, 34)
            };
            _cboCategory.SelectedIndexChanged += (s, e) =>
            {
                _categoryFilter = _cboCategory.SelectedItem?.ToString() ?? "All Categories";
                RefreshGrid();
            };

            _btnRefresh = new ModernButton
            {
                Text = "Refresh",
                IconName = "refresh",
                ButtonType = ModernButtonType.Secondary,
                Size = new Size(105, 36)
            };
            _btnRefresh.Click += (s, e) =>
            {
                _dataService.LoadData();
                RefreshGrid();
            };

            _lblCount = new Label
            {
                Text = "Showing 0 products",
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                AutoSize = true
            };

            _toolbarCard.Controls.Add(_searchBox);
            _toolbarCard.Controls.Add(_cboCategory);
            _toolbarCard.Controls.Add(_btnRefresh);
            _toolbarCard.Controls.Add(_lblCount);
            _contentWrapper.Controls.Add(_toolbarCard);

            // 4. Grid Container Card
            _gridCard = new RoundedPanel
            {
                BorderRadius = 12,
                Padding = new Padding(1)
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 44 }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SKU", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product Name", FillWeight = 160 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Price ($)", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", FillWeight = 55 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", FillWeight = 75 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 45 });

            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _lblEmptyState = new Label
            {
                Text = "No products found matching your filter criteria.",
                Font = FontHelper.CreateFont(10F, FontStyle.Regular),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                Dock = DockStyle.Fill
            };

            _gridCard.Controls.Add(_lblEmptyState);
            _gridCard.Controls.Add(_grid);
            _contentWrapper.Controls.Add(_gridCard);
        }

        public void RefreshView()
        {
            try
            {
                string currentCat = _cboCategory.SelectedItem?.ToString() ?? "All Categories";
                PopulateCategories();
                if (_cboCategory.Items.Contains(currentCat))
                    _cboCategory.SelectedItem = currentCat;
                else if (_cboCategory.Items.Count > 0)
                    _cboCategory.SelectedIndex = 0;
            }
            catch { }

            RefreshGrid();
            ApplyTheme();
            UpdateTranslations();
            RepositionContent();
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var prd = (Product)_grid.Rows[e.RowIndex].Tag;
            if (prd == null) return;

            if (e.ColumnIndex == 6) // Edit
            {
                ShowEditProductDialog(prd);
            }
            else if (e.ColumnIndex == 7) // Delete
            {
                string confirmMsg = TranslationManager.T("ConfirmDeleteProduct", $"Are you sure you want to delete '{prd.Name}' from the database?");
                if (MessageBox.Show(confirmMsg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _dataService.DeleteProduct(prd.Id);
                    RefreshGrid();
                }
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var prd = (Product)_grid.Rows[e.RowIndex].Tag;
                if (prd != null) ShowEditProductDialog(prd);
            }
        }

        private void PopulateCategories()
        {
            _cboCategory.Items.Clear();
            _cboCategory.Items.Add("All Categories");
            foreach (var cat in _dataService.Categories)
            {
                _cboCategory.Items.Add(cat.Name);
            }
            if (_cboCategory.Items.Count > 0)
            {
                _cboCategory.SelectedIndex = 0;
            }
        }

        public void ApplySearch(string query)
        {
            _searchFilter = query;
            _searchBox.Text = query;
            RefreshGrid();
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            var newPrd = ModernDialogHelper.ShowProductDialog(this);
            if (newPrd != null)
            {
                _dataService.AddProduct(newPrd);
                RefreshGrid();
            }
        }

        private void ShowEditProductDialog(Product prd)
        {
            if (prd == null) return;
            var updated = ModernDialogHelper.ShowProductDialog(this, prd);
            if (updated != null)
            {
                _dataService.UpdateProduct(updated);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            var allProducts = _dataService.Products;
            var filtered = allProducts.AsEnumerable();

            if (!string.IsNullOrEmpty(_categoryFilter) && _categoryFilter != "All Categories")
            {
                filtered = filtered.Where(p => p.Category == _categoryFilter);
            }

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                filtered = filtered.Where(p => p.Name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               p.Sku.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var list = filtered.ToList();
            foreach (var p in list)
            {
                int rowIdx = _grid.Rows.Add(p.Sku, p.Name, p.Category, $"${p.Price:N2}", $"{p.Stock} pcs", p.Status, "Edit", "Delete");
                _grid.Rows[rowIdx].Tag = p;
            }

            // Update KPI Metric Cards
            int totalCount = allProducts.Count;
            int inStockCount = allProducts.Count(p => p.Status == "In Stock" || p.Stock > 5);
            int lowStockCount = allProducts.Count(p => p.Status == "Low Stock" || (p.Stock > 0 && p.Stock <= 5) || p.Status == "Out of Stock" || p.Stock == 0);
            decimal totalCatalogValue = allProducts.Sum(p => p.Price * p.Stock);

            _cardTotalPrd.SetData(TranslationManager.T("TotalProducts", "Total Products"), $"{totalCount}", $"{list.Count} shown", "products", Color.FromArgb(59, 130, 246));
            _cardInStock.SetData(TranslationManager.T("InStock", "In Stock"), $"{inStockCount}", $"{Math.Round((double)inStockCount / Math.Max(1, totalCount) * 100)}%", "check", Color.FromArgb(16, 185, 129));
            _cardLowStock.SetData(TranslationManager.T("LowStock", "Low / Alert"), $"{lowStockCount}", lowStockCount > 0 ? "Needs restock" : "Healthy", "box", lowStockCount > 0 ? Color.FromArgb(245, 158, 11) : Color.FromArgb(16, 185, 129));
            _cardTotalVal.SetData(TranslationManager.T("InventoryValue", "Inventory Value"), $"${totalCatalogValue:N0}", "Retail", "dollar", Color.FromArgb(139, 92, 246));

            _lblCount.Text = $"Showing {list.Count} of {totalCount} items";

            bool isEmpty = list.Count == 0;
            _lblEmptyState.Visible = isEmpty;
            _grid.Visible = !isEmpty;
            if (isEmpty)
            {
                _lblEmptyState.BringToFront();
            }
        }

        private void UpdateTranslations()
        {
            _lblTitle.Text = TranslationManager.T("Products", "Products & Inventory");
            _lblSubtitle.Text = TranslationManager.T("ProductsSubtitle", "Manage store catalog, stock quantities, categories, and retail pricing.");
            _btnAddProduct.Text = TranslationManager.T("AddNewProduct", "Add New Product");
            _btnRefresh.Text = TranslationManager.T("Refresh", "Refresh");

            if (_grid.Columns.Count >= 8)
            {
                _grid.Columns[0].HeaderText = TranslationManager.T("SKU", "SKU");
                _grid.Columns[1].HeaderText = TranslationManager.T("ProductName", "Product Name");
                _grid.Columns[2].HeaderText = TranslationManager.T("Category", "Category");
                _grid.Columns[3].HeaderText = TranslationManager.T("Price", "Price ($)");
                _grid.Columns[4].HeaderText = TranslationManager.T("Stock", "Stock");
                _grid.Columns[5].HeaderText = TranslationManager.T("Status", "Status");
                _grid.Columns[6].HeaderText = "Edit";
                _grid.Columns[7].HeaderText = "Delete";
            }
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_contentWrapper != null) _contentWrapper.BackColor = ThemeManager.Background;
            if (_headerPanel != null) _headerPanel.BackColor = ThemeManager.Background;
            if (_metricsPanel != null) _metricsPanel.BackColor = ThemeManager.Background;

            DataGridViewStyleHelper.UpdateColors(_grid);

            _lblTitle.ForeColor = ThemeManager.TextPrimary;
            _lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            _lblCount.ForeColor = ThemeManager.TextSecondary;
            _lblEmptyState.ForeColor = ThemeManager.TextMuted;
            _lblEmptyState.BackColor = ThemeManager.CardBackground;

            RepositionContent();
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionContent();
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null || _headerPanel == null) return;

            int containerW = Math.Max(720, Math.Max(ClientSize.Width, Parent != null ? Parent.ClientSize.Width : 0));
            _contentWrapper.Location = new Point(0, 0);
            _contentWrapper.Width = containerW;

            int gap = 14;
            int currentY = 14;

            // 1. Header
            _headerPanel.Location = new Point(0, currentY);
            _headerPanel.Width = containerW;
            _btnAddProduct.Location = new Point(containerW - _btnAddProduct.Width, 5);

            currentY += _headerPanel.Height + gap;

            // 2. Metrics Row (4 Cards)
            _metricsPanel.Location = new Point(0, currentY);
            _metricsPanel.Width = containerW;

            int cardGap = 12;
            int cardW = (containerW - (cardGap * 3)) / 4;
            _cardTotalPrd.SetBounds(0, 0, cardW, 78);
            _cardInStock.SetBounds(cardW + cardGap, 0, cardW, 78);
            _cardLowStock.SetBounds((cardW + cardGap) * 2, 0, cardW, 78);
            _cardTotalVal.SetBounds((cardW + cardGap) * 3, 0, containerW - ((cardW + cardGap) * 3), 78);

            currentY += _metricsPanel.Height + gap;

            // 3. Toolbar Card
            _toolbarCard.Location = new Point(0, currentY);
            _toolbarCard.Width = containerW;

            _searchBox.Location = new Point(12, 10);
            _cboCategory.Location = new Point(_searchBox.Right + 12, 11);
            _btnRefresh.Location = new Point(_cboCategory.Right + 12, 10);

            _lblCount.Location = new Point(containerW - _lblCount.Width - 16, 20);

            currentY += _toolbarCard.Height + gap;

            // 4. Data Grid Card
            _gridCard.Location = new Point(0, currentY);
            int remainingH = Math.Max(400, ClientSize.Height - currentY - 20);
            _gridCard.Size = new Size(containerW, remainingH);

            _contentWrapper.Height = currentY + remainingH + 20;
        }
    }
}
