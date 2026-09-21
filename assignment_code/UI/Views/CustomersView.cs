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
        private string _searchFilter = "";
        private string _tierFilter = "All Tiers";

        // Layout Containers
        private Panel _contentWrapper;
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private ModernButton _btnAddCustomer;

        // KPI Metric Cards
        private Panel _metricsPanel;
        private ManagementStatCard _cardTotalCust;
        private ManagementStatCard _cardActiveCust;
        private ManagementStatCard _cardVipCust;
        private ManagementStatCard _cardTotalSpent;

        // Toolbar Card
        private RoundedPanel _toolbarCard;
        private ModernSearchBox _searchBox;
        private ModernComboBox _cboTier;
        private ModernButton _btnRefresh;
        private Label _lblCount;

        // Data Table Card
        private RoundedPanel _gridCard;
        private DataGridView _grid;
        private Label _lblEmptyState;

        public CustomersView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            AutoScroll = true;

            InitializeLayout();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            DataGridViewStyleHelper.ApplyStyle(_grid);

            PopulateTierFilter();
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
                BackColor = Color.Transparent
            };
            Controls.Add(_contentWrapper);
            Resize += (s, e) => RepositionContent();

            // 1. Header Area
            _headerPanel = new Panel
            {
                BackColor = Color.Transparent,
                Height = 48
            };
            _lblTitle = new Label
            {
                Text = "Customer Directory",
                Font = FontHelper.CreateFont(15F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            _lblSubtitle = new Label
            {
                Text = "View client profiles, transaction history, contact info, and loyalty tiers.",
                Font = FontHelper.CreateFont(8.75F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 26)
            };
            _btnAddCustomer = new ModernButton
            {
                Text = "+ Add New Customer",
                IconName = "plus",
                ButtonType = ModernButtonType.Primary,
                Size = new Size(185, 38)
            };
            _btnAddCustomer.Click += BtnAddCustomer_Click;

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_btnAddCustomer);
            _contentWrapper.Controls.Add(_headerPanel);

            // 2. Metrics Row
            _metricsPanel = new Panel
            {
                BackColor = Color.Transparent,
                Height = 78
            };
            _cardTotalCust = new ManagementStatCard { IconName = "customer", AccentColor = Color.FromArgb(59, 130, 246) };
            _cardActiveCust = new ManagementStatCard { IconName = "check", AccentColor = Color.FromArgb(16, 185, 129) };
            _cardVipCust = new ManagementStatCard { IconName = "star", AccentColor = Color.FromArgb(139, 92, 246) };
            _cardTotalSpent = new ManagementStatCard { IconName = "dollar", AccentColor = Color.FromArgb(245, 158, 11) };

            _metricsPanel.Controls.AddRange(new Control[] { _cardTotalCust, _cardActiveCust, _cardVipCust, _cardTotalSpent });
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
                PlaceholderText = "Search customer name, email, or phone...",
                Size = new Size(270, 36)
            };
            _searchBox.SearchTextChanged += (s, e) =>
            {
                _searchFilter = _searchBox.Text.Trim();
                RefreshGrid();
            };

            _cboTier = new ModernComboBox
            {
                Size = new Size(160, 34)
            };
            _cboTier.SelectedIndexChanged += (s, e) =>
            {
                _tierFilter = _cboTier.SelectedItem?.ToString() ?? "All Tiers";
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
                Text = "Showing 0 customers",
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                AutoSize = true
            };

            _toolbarCard.Controls.Add(_searchBox);
            _toolbarCard.Controls.Add(_cboTier);
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
                RowTemplate = { Height = 46 }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Customer ID", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", FillWeight = 145 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", FillWeight = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", FillWeight = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Spent", FillWeight = 75 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tier", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 45 });

            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _lblEmptyState = new Label
            {
                Text = "No customers found matching your search.",
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

        private void PopulateTierFilter()
        {
            _cboTier.Items.Clear();
            _cboTier.Items.Add("All Tiers");
            _cboTier.Items.Add("VIP");
            _cboTier.Items.Add("Regular");
            _cboTier.Items.Add("New");
            _cboTier.SelectedIndex = 0;
        }

        public void RefreshView()
        {
            RefreshGrid();
            ApplyTheme();
            UpdateTranslations();
            RepositionContent();
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
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
                string confirmMsg = TranslationManager.T("ConfirmDeleteCustomer", $"Are you sure you want to delete customer '{cust.FullName}' from PostgreSQL database?");
                if (MessageBox.Show(confirmMsg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _dataService.DeleteCustomer(cust.Id);
                    RefreshGrid();
                }
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var cust = (Customer)_grid.Rows[e.RowIndex].Tag;
                if (cust != null) ShowEditCustomerDialog(cust);
            }
        }

        public void ApplySearch(string query)
        {
            _searchFilter = query;
            _searchBox.Text = query;
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
            var allCustomers = _dataService.Customers;
            var filtered = allCustomers.AsEnumerable();

            if (!string.IsNullOrEmpty(_tierFilter) && _tierFilter != "All Tiers")
            {
                filtered = filtered.Where(c => string.Equals(c.Tier, _tierFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                filtered = filtered.Where(c => (c.FullName?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                               (c.Email?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                               (c.Phone?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                               (c.Id?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            var list = filtered.ToList();
            foreach (var c in list)
            {
                int rowIdx = _grid.Rows.Add(c.Id, c.FullName, c.Email, c.Phone, $"{c.TotalOrders} orders", $"${c.TotalSpent:N2}", c.Tier, c.Status, "Edit", "Delete");
                _grid.Rows[rowIdx].Tag = c;
            }

            // Update KPI Metric Cards
            int totalCount = allCustomers.Count;
            int activeCount = allCustomers.Count(c => c.Status == "Active");
            int vipCount = allCustomers.Count(c => string.Equals(c.Tier, "VIP", StringComparison.OrdinalIgnoreCase));
            decimal totalRevenue = allCustomers.Sum(c => c.TotalSpent);

            _cardTotalCust.SetData(TranslationManager.T("TotalCustomers", "Total Clients"), $"{totalCount}", $"{list.Count} shown", "customer", Color.FromArgb(59, 130, 246));
            _cardActiveCust.SetData(TranslationManager.T("ActiveAccounts", "Active Clients"), $"{activeCount}", $"{Math.Round((double)activeCount / Math.Max(1, totalCount) * 100)}%", "check", Color.FromArgb(16, 185, 129));
            _cardVipCust.SetData(TranslationManager.T("VipMembers", "VIP Members"), $"{vipCount}", $"{Math.Round((double)vipCount / Math.Max(1, totalCount) * 100)}%", "star", Color.FromArgb(139, 92, 246));
            _cardTotalSpent.SetData(TranslationManager.T("LifetimeRevenue", "Lifetime Spent"), $"${totalRevenue:N0}", "Total", "dollar", Color.FromArgb(245, 158, 11));

            _lblCount.Text = $"Showing {list.Count} of {totalCount} clients";

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
            _lblTitle.Text = TranslationManager.T("Customers", "Customer Directory");
            _lblSubtitle.Text = TranslationManager.T("CustomersSubtitle", "View client profiles, transaction history, contact info, and loyalty tiers.");
            _btnAddCustomer.Text = TranslationManager.T("AddNewCustomer", "+ Add New Customer");
            _btnRefresh.Text = TranslationManager.T("Refresh", "Refresh");

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
        }

        private void ApplyTheme()
        {
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
            _btnAddCustomer.Location = new Point(containerW - _btnAddCustomer.Width, 5);

            currentY += _headerPanel.Height + gap;

            // 2. Metrics Row (4 Cards)
            _metricsPanel.Location = new Point(0, currentY);
            _metricsPanel.Width = containerW;

            int cardGap = 12;
            int cardW = (containerW - (cardGap * 3)) / 4;
            _cardTotalCust.SetBounds(0, 0, cardW, 78);
            _cardActiveCust.SetBounds(cardW + cardGap, 0, cardW, 78);
            _cardVipCust.SetBounds((cardW + cardGap) * 2, 0, cardW, 78);
            _cardTotalSpent.SetBounds((cardW + cardGap) * 3, 0, containerW - ((cardW + cardGap) * 3), 78);

            currentY += _metricsPanel.Height + gap;

            // 3. Toolbar Card
            _toolbarCard.Location = new Point(0, currentY);
            _toolbarCard.Width = containerW;

            _searchBox.Location = new Point(12, 10);
            _cboTier.Location = new Point(_searchBox.Right + 12, 11);
            _btnRefresh.Location = new Point(_cboTier.Right + 12, 10);

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
