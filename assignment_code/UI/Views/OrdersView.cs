using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class OrdersView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private Panel _contentWrapper;
        private FlowLayoutPanel _filterTabsPanel;
        private ModernButton _modernBtnRefresh;
        private DataGridView _grid;
        private string _statusFilter = "All";

        public OrdersView()
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
            PopulateFilterTabs();
            RefreshGrid();
            ThemeManager.ThemeChanged += (s, e) => RefreshView();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void RefreshView()
        {
            PopulateFilterTabs();
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

            _filterTabsPanel = new FlowLayoutPanel
            {
                Height = 34,
                BackColor = Color.Transparent,
                WrapContents = false
            };

            _modernBtnRefresh = new ModernButton
            {
                Text = "Refresh",
                IconName = "refresh",
                ButtonType = ModernButtonType.Secondary,
                Size = new Size(110, 34),
                TranslationKey = "Refresh"
            };
            _modernBtnRefresh.Click += (s, e) =>
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

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("OrderId", "Order ID"), DataPropertyName = "OrderId", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Customer", "Customer"), DataPropertyName = "CustomerName", FillWeight = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("ItemsSummary", "Items Summary"), DataPropertyName = "ItemsSummary", FillWeight = 160 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Total", "Total ($)"), DataPropertyName = "TotalAmount", FillWeight = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("OrderDate", "Order Date"), DataPropertyName = "OrderDate", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Status", "Status"), DataPropertyName = "Status", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Payment", "Payment"), DataPropertyName = "PaymentStatus", FillWeight = 60 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = TranslationManager.T("Action", "Action"), Text = "Change Status", UseColumnTextForButtonValue = true, FillWeight = 80 });

            _grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 7)
                {
                    var ord = (Order)_grid.Rows[e.RowIndex].Tag;
                    if (ord != null)
                    {
                        string[] statuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                        int nextIdx = (Array.IndexOf(statuses, ord.Status) + 1) % statuses.Length;
                        string newStatus = statuses[nextIdx];
                        _dataService.UpdateOrderStatus(ord.OrderId, newStatus);
                        RefreshGrid();
                    }
                }
            };

            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 5 && e.Value != null)
                {
                    string st = e.Value.ToString();
                    if (st == "Delivered") e.CellStyle.ForeColor = ThemeManager.SuccessGreen;
                    else if (st == "Shipped" || st == "Processing") e.CellStyle.ForeColor = ThemeManager.AccentBlue;
                    else if (st == "Pending") e.CellStyle.ForeColor = ThemeManager.WarningYellow;
                    else e.CellStyle.ForeColor = ThemeManager.DangerRed;
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                }
            };

            _contentWrapper.Controls.Add(_filterTabsPanel);
            _contentWrapper.Controls.Add(_modernBtnRefresh);
            _contentWrapper.Controls.Add(_grid);

            TranslationManager.LanguageChanged += (s, e) =>
            {
                if (_grid.Columns.Count >= 8)
                {
                    _grid.Columns[0].HeaderText = TranslationManager.T("OrderId", "Order ID");
                    _grid.Columns[1].HeaderText = TranslationManager.T("Customer", "Customer");
                    _grid.Columns[2].HeaderText = TranslationManager.T("ItemsSummary", "Items Summary");
                    _grid.Columns[3].HeaderText = TranslationManager.T("Total", "Total ($)");
                    _grid.Columns[4].HeaderText = TranslationManager.T("OrderDate", "Order Date");
                    _grid.Columns[5].HeaderText = TranslationManager.T("Status", "Status");
                    _grid.Columns[6].HeaderText = TranslationManager.T("Payment", "Payment");
                    _grid.Columns[7].HeaderText = TranslationManager.T("Action", "Action");
                }
                PopulateFilterTabs();
                ApplyTheme();
                RefreshGrid();
            };

            ApplyTheme();
            RepositionContent();
        }

        private void PopulateFilterTabs()
        {
            _filterTabsPanel.Controls.Clear();
            string[] tabs = new[] { "All", "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            foreach (var tab in tabs)
            {
                string displayText = TranslationManager.T(tab, tab);
                Button btn = new Button
                {
                    Text = displayText,
                    Font = FontHelper.CreateFontForText(displayText, 8.5F, FontStyle.Bold),
                    Height = 32,
                    AutoSize = true,
                    Margin = new Padding(0, 0, 8, 0),
                    Padding = new Padding(8, 2, 8, 2),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                bool isSelected = (tab == _statusFilter);
                btn.BackColor = isSelected ? ThemeManager.AccentBlue : ThemeManager.CardBackground;
                btn.ForeColor = isSelected ? Color.White : ThemeManager.TextSecondary;

                btn.Click += (s, e) =>
                {
                    _statusFilter = tab;
                    PopulateFilterTabs();
                    RefreshGrid();
                };

                _filterTabsPanel.Controls.Add(btn);
            }
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            var filtered = _dataService.Orders.AsEnumerable();

            if (_statusFilter != "All")
                filtered = filtered.Where(o => string.Equals(o.Status, _statusFilter, StringComparison.OrdinalIgnoreCase));

            foreach (var ord in filtered)
            {
                int rowIdx = _grid.Rows.Add(ord.OrderId, ord.CustomerName, ord.ItemsSummary, $"${ord.TotalAmount:N2}", ord.OrderDate.ToString("dd MMM yyyy"), ord.Status, ord.PaymentStatus, "Change Status");
                _grid.Rows[rowIdx].Tag = ord;
            }
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_contentWrapper != null) _contentWrapper.BackColor = ThemeManager.Background;

            DataGridViewStyleHelper.UpdateColors(_grid);

            _modernBtnRefresh.BackColor = ThemeManager.CardBackground;
            _modernBtnRefresh.ForeColor = ThemeManager.TextPrimary;
            _modernBtnRefresh.Invalidate();

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
            _filterTabsPanel.Location = new Point(0, startY);
            _filterTabsPanel.Width = w - 130;
            _modernBtnRefresh.Location = new Point(w - _modernBtnRefresh.Width, startY);

            int gridY = startY + 44;
            _grid.Location = new Point(0, gridY);
            _grid.Size = new Size(w, Math.Max(400, ClientSize.Height - gridY - 20));
            _contentWrapper.Height = gridY + _grid.Height + 20;
        }
    }
}
