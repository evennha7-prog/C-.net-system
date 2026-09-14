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

        public ProductsView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponent();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            DataGridViewStyleHelper.ApplyStyle(_grid);
            PopulateCategories();
            RefreshGrid();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) =>
            {
                _btnAddProduct.Text = TranslationManager.T("AddNewProduct", "+ Add New Product");
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
                ApplyTheme();
                RefreshGrid();
            };

            ApplyTheme();
            RepositionContent();
        }

        public void RefreshView()
        {
            try
            {
                string currentCat = _cboCategory.SelectedItem?.ToString() ?? "All Categories";
                PopulateCategories();
                if (_cboCategory.Items.Contains(currentCat))
                    _cboCategory.SelectedItem = currentCat;
                else _cboCategory.SelectedIndex = 0;
            }
            catch { }

            RefreshGrid();
            ApplyTheme();
        }

        private void ProductsView_Resize(object sender, EventArgs e)
        {
            RepositionContent();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchFilter = _txtSearch.Text.Trim();
            RefreshGrid();
        }

        private void CboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            _categoryFilter = _cboCategory.SelectedItem?.ToString() ?? "All Categories";
            RefreshGrid();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _dataService.LoadData();
            RefreshGrid();
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
                if (MessageBox.Show($"Are you sure you want to delete '{prd.Name}' from PostgreSQL database?", "Confirm Delete Product", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 5 && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "In Stock" || status == TranslationManager.T("InStock", "In Stock")) e.CellStyle.ForeColor = ThemeManager.SuccessGreen;
                else if (status == "Low Stock" || status == TranslationManager.T("LowStock", "Low Stock")) e.CellStyle.ForeColor = ThemeManager.WarningYellow;
                else e.CellStyle.ForeColor = ThemeManager.DangerRed;
                e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
            }
        }

        private void PopulateCategories()
        {
            _cboCategory.Items.Clear();
            _cboCategory.Items.Add("All Categories");
            foreach (var cat in _dataService.Categories)
                _cboCategory.Items.Add(cat.Name);
            if (_cboCategory.Items.Count > 0)
                _cboCategory.SelectedIndex = 0;
        }

        public void ApplySearch(string query)
        {
            _searchFilter = query;
            _txtSearch.Text = query;
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
            var filtered = _dataService.Products.AsEnumerable();

            if (!string.IsNullOrEmpty(_categoryFilter) && _categoryFilter != "All Categories")
                filtered = filtered.Where(p => p.Category == _categoryFilter);

            if (!string.IsNullOrEmpty(_searchFilter))
                filtered = filtered.Where(p => p.Name.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               p.Sku.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var p in filtered)
            {
                int rowIdx = _grid.Rows.Add(p.Sku, p.Name, p.Category, $"${p.Price:N2}", p.Stock, p.Status, "Edit", "Delete");
                _grid.Rows[rowIdx].Tag = p;
            }
        }

        private void ApplyTheme()
        {
            DataGridViewStyleHelper.UpdateColors(_grid);

            _btnAddProduct.BackColor = ThemeManager.AccentBlue;
            _btnAddProduct.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
            _btnRefresh.BackColor = ThemeManager.CardBackground;
            _btnRefresh.ForeColor = ThemeManager.TextPrimary;
            _btnRefresh.FlatAppearance.BorderColor = ThemeManager.BorderColor;

            Invalidate(true);
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null || _txtSearch == null) return;
            int w = Math.Max(700, ClientSize.Width);
            _contentWrapper.Width = w;

            int startY = 14;
            _txtSearch.Location = new Point(0, startY + 4);
            _cboCategory.Location = new Point(_txtSearch.Right + 12, startY + 4);
            _btnRefresh.Location = new Point(_cboCategory.Right + 12, startY);
            _btnAddProduct.Location = new Point(w - _btnAddProduct.Width, startY);

            int gridY = startY + 44;
            _grid.Location = new Point(0, gridY);
            _grid.Size = new Size(w, Math.Max(400, ClientSize.Height - gridY - 20));
            _contentWrapper.Height = gridY + _grid.Height + 20;
        }
    }
}
