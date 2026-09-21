using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class SalesListView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private Panel _contentWrapper;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Button _btnRefresh;
        private string _searchFilter = "";

        public SalesListView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            AutoScroll = true;

            InitializeComponent();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            DataGridViewStyleHelper.ApplyStyle(_grid);
            RefreshGrid();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void RefreshView()
        {
            RefreshGrid();
            ApplyTheme();
            Invalidate(true);
        }

        private void InitializeComponent()
        {
            _contentWrapper = new Panel
            {
                Location = new Point(0, 0),
                Width = ClientSize.Width,
                Height = 720,
                BackColor = Color.Transparent
            };
            Controls.Add(_contentWrapper);
            Resize += (s, e) => RepositionContent();

            _txtSearch = new TextBox
            {
                Font = FontHelper.CreateFont(9.5F),
                Size = new Size(240, 26)
            };
            _txtSearch.TextChanged += (s, e) =>
            {
                _searchFilter = _txtSearch.Text.Trim();
                RefreshGrid();
            };

            _btnRefresh = new Button
            {
                Text = "Refresh",
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(110, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.CardBackground,
                ForeColor = ThemeManager.TextPrimary,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            _btnRefresh.Click += (s, e) =>
            {
                _dataService.LoadData();
                RefreshGrid();
            };

            _grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = ThemeManager.CardBackground,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 40 }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("InvoiceNo", "Invoice #"), DataPropertyName = "InvoiceNo", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Customer", "Customer"), DataPropertyName = "CustomerName", FillWeight = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("DateTime", "Date & Time"), DataPropertyName = "Timestamp", FillWeight = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Items", "Items"), DataPropertyName = "ItemsCount", FillWeight = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Payment", "Payment"), DataPropertyName = "PaymentMethod", FillWeight = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Total", "Total ($)"), DataPropertyName = "TotalAmount", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Status", "Status"), DataPropertyName = "Status", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = TranslationManager.T("Receipt", "Receipt"), Text = "Receipt", UseColumnTextForButtonValue = true, FillWeight = 80 });

            _grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 7)
                {
                    var sale = (SaleTransaction)_grid.Rows[e.RowIndex].Tag;
                    if (sale != null)
                    {
                        ModernDialogHelper.ShowReceiptDialog(this, sale, sale.Items);
                    }
                }
            };

            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 6 && e.Value != null)
                {
                    e.CellStyle.ForeColor = ThemeManager.SuccessGreen;
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                }
                else if (e.ColumnIndex == 5 && e.Value != null)
                {
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                    e.CellStyle.ForeColor = ThemeManager.AccentBlue;
                }
            };

            _contentWrapper.Controls.Add(_txtSearch);
            _contentWrapper.Controls.Add(_btnRefresh);
            _contentWrapper.Controls.Add(_grid);

            TranslationManager.LanguageChanged += (s, e) =>
            {
                if (_grid.Columns.Count >= 8)
                {
                    _grid.Columns[0].HeaderText = TranslationManager.T("InvoiceNo", "Invoice #");
                    _grid.Columns[1].HeaderText = TranslationManager.T("Customer", "Customer");
                    _grid.Columns[2].HeaderText = TranslationManager.T("DateTime", "Date & Time");
                    _grid.Columns[3].HeaderText = TranslationManager.T("Items", "Items");
                    _grid.Columns[4].HeaderText = TranslationManager.T("Payment", "Payment");
                    _grid.Columns[5].HeaderText = TranslationManager.T("Total", "Total ($)");
                    _grid.Columns[6].HeaderText = TranslationManager.T("Status", "Status");
                    _grid.Columns[7].HeaderText = TranslationManager.T("Receipt", "Receipt");
                }
                ApplyTheme();
                RefreshGrid();
            };

            ApplyTheme();
            RepositionContent();
        }

        public void ApplySearch(string query)
        {
            _searchFilter = query;
            _txtSearch.Text = query;
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            var filtered = _dataService.Sales.AsEnumerable();

            if (!string.IsNullOrEmpty(_searchFilter))
                filtered = filtered.Where(s => s.InvoiceNo.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               s.CustomerName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               s.PaymentMethod.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var s in filtered)
            {
                int rowIdx = _grid.Rows.Add(s.InvoiceNo, s.CustomerName, s.Timestamp.ToString("dd MMM yyyy HH:mm"), s.Items?.Count ?? 1, s.PaymentMethod, $"${s.TotalAmount:N2}", s.Status, "Receipt");
                _grid.Rows[rowIdx].Tag = s;
            }
        }

        private void ApplyTheme()
        {
            DataGridViewStyleHelper.UpdateColors(_grid);

            _btnRefresh.BackColor = ThemeManager.CardBackground;
            _btnRefresh.ForeColor = ThemeManager.TextPrimary;
            _btnRefresh.FlatAppearance.BorderColor = ThemeManager.BorderColor;

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
            if (_contentWrapper == null) return;
            int w = Math.Max(700, Math.Max(ClientSize.Width, Parent != null ? Parent.ClientSize.Width : 0));
            _contentWrapper.Location = new Point(0, 0);
            _contentWrapper.Width = w;

            int startY = 14;
            _txtSearch.Location = new Point(0, startY + 4);
            _btnRefresh.Location = new Point(w - _btnRefresh.Width, startY);

            int gridY = startY + 44;
            _grid.Location = new Point(0, gridY);
            _grid.Size = new Size(w, Math.Max(400, ClientSize.Height - gridY - 20));
            _contentWrapper.Height = gridY + _grid.Height + 20;
        }
    }
}
