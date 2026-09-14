using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class CustomersView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private Panel _contentWrapper;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Button _btnAddCustomer;
        private Button _btnRefresh;
        private string _searchFilter = "";

        public CustomersView()
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
                Size = new Size(220, 26)
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

            _btnAddCustomer = new Button
            {
                Text = TranslationManager.T("AddNewCustomer", "+ Add New Customer"),
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(180, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnAddCustomer.FlatAppearance.BorderSize = 0;
            _btnAddCustomer.Click += BtnAddCustomer_Click;

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

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("CustomerId", "Customer ID"), DataPropertyName = "Id", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("FullName", "Full Name"), DataPropertyName = "FullName", FillWeight = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Email", "Email"), DataPropertyName = "Email", FillWeight = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Phone", "Phone"), DataPropertyName = "Phone", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Orders", "Orders"), DataPropertyName = "TotalOrders", FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("TotalSpent", "Total Spent"), DataPropertyName = "TotalSpent", FillWeight = 75 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Tier", "Tier"), DataPropertyName = "Tier", FillWeight = 55 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Status", "Status"), DataPropertyName = "Status", FillWeight = 55 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 45 });

            _grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var cust = (Customer)_grid.Rows[e.RowIndex].Tag;
                if (cust == null) return;

                if (e.ColumnIndex == 8) // Edit
                {
                    ShowEditCustomerDialog(cust);
                }
                else if (e.ColumnIndex == 9) // Delete
                {
                    if (MessageBox.Show($"Are you sure you want to delete customer '{cust.FullName}' from PostgreSQL database?", "Confirm Delete Customer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _dataService.DeleteCustomer(cust.Id);
                        RefreshGrid();
                    }
                }
            };

            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var cust = (Customer)_grid.Rows[e.RowIndex].Tag;
                    if (cust != null) ShowEditCustomerDialog(cust);
                }
            };

            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 6 && e.Value != null)
                {
                    string tier = e.Value.ToString();
                    if (tier == "VIP") e.CellStyle.ForeColor = Color.FromArgb(139, 92, 246);
                    else e.CellStyle.ForeColor = ThemeManager.AccentBlue;
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                }
                else if (e.ColumnIndex == 5 && e.Value != null)
                {
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                    e.CellStyle.ForeColor = ThemeManager.SuccessGreen;
                }
            };

            _contentWrapper.Controls.Add(_txtSearch);
            _contentWrapper.Controls.Add(_btnRefresh);
            _contentWrapper.Controls.Add(_btnAddCustomer);
            _contentWrapper.Controls.Add(_grid);

            TranslationManager.LanguageChanged += (s, e) =>
            {
                _btnAddCustomer.Text = TranslationManager.T("AddNewCustomer", "+ Add New Customer");
                if (_grid.Columns.Count >= 10)
                {
                    _grid.Columns[0].HeaderText = TranslationManager.T("CustomerId", "Customer ID");
                    _grid.Columns[1].HeaderText = TranslationManager.T("FullName", "Full Name");
                    _grid.Columns[2].HeaderText = TranslationManager.T("Email", "Email");
                    _grid.Columns[3].HeaderText = TranslationManager.T("Phone", "Phone");
                    _grid.Columns[4].HeaderText = TranslationManager.T("Orders", "Orders");
                    _grid.Columns[5].HeaderText = TranslationManager.T("TotalSpent", "Total Spent");
                    _grid.Columns[6].HeaderText = TranslationManager.T("Tier", "Tier");
                    _grid.Columns[7].HeaderText = TranslationManager.T("Status", "Status");
                    _grid.Columns[8].HeaderText = "Edit";
                    _grid.Columns[9].HeaderText = "Delete";
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

        private void BtnAddCustomer_Click(object sender, EventArgs e)
        {
            var newCust = ModernDialogHelper.ShowCustomerDialog(this);
            if (newCust != null)
            {
                _dataService.AddCustomer(newCust);
                RefreshGrid();
            }
        }

        private void ShowEditCustomerDialog(Customer cust)
        {
            if (cust == null) return;
            var updated = ModernDialogHelper.ShowCustomerDialog(this, cust);
            if (updated != null)
            {
                _dataService.UpdateCustomer(updated);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            var filtered = _dataService.Customers.AsEnumerable();

            if (!string.IsNullOrEmpty(_searchFilter))
                filtered = filtered.Where(c => c.FullName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               c.Email.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               c.Phone.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var c in filtered)
            {
                int rowIdx = _grid.Rows.Add(c.Id, c.FullName, c.Email, c.Phone, c.TotalOrders, $"${c.TotalSpent:N2}", c.Tier, c.Status, "Edit", "Delete");
                _grid.Rows[rowIdx].Tag = c;
            }
        }

        private void ApplyTheme()
        {
            DataGridViewStyleHelper.UpdateColors(_grid);

            _btnAddCustomer.BackColor = ThemeManager.AccentBlue;
            _btnRefresh.BackColor = ThemeManager.CardBackground;
            _btnRefresh.ForeColor = ThemeManager.TextPrimary;
            _btnRefresh.FlatAppearance.BorderColor = ThemeManager.BorderColor;

            Invalidate(true);
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null) return;
            int w = Math.Max(700, ClientSize.Width);
            _contentWrapper.Width = w;

            int startY = 14;
            _txtSearch.Location = new Point(0, startY + 4);
            _btnRefresh.Location = new Point(_txtSearch.Right + 12, startY);
            _btnAddCustomer.Location = new Point(w - _btnAddCustomer.Width, startY);

            int gridY = startY + 44;
            _grid.Location = new Point(0, gridY);
            _grid.Size = new Size(w, Math.Max(400, ClientSize.Height - gridY - 20));
            _contentWrapper.Height = gridY + _grid.Height + 20;
        }
    }
}
