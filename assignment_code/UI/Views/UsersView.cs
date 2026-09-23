using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class UsersView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private string _searchFilter = "";
        private string _roleFilter = "All Roles";

        // Layout Containers
        private Panel _contentWrapper;
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private ModernButton _btnAddUser;

        // KPI Metric Cards
        private Panel _metricsPanel;
        private ManagementStatCard _cardTotalStaff;
        private ManagementStatCard _cardAdmins;
        private ManagementStatCard _cardActiveStaff;
        private ManagementStatCard _cardManagersCashiers;

        // Toolbar Card
        private RoundedPanel _toolbarCard;
        private ModernSearchBox _searchBox;
        private ModernComboBox _cboRole;
        private ModernButton _btnRefresh;
        private Label _lblCount;

        // Data Table Card
        private RoundedPanel _gridCard;
        private DataGridView _grid;
        private Label _lblEmptyState;

        public UsersView()
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

            PopulateRoleFilter();
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
                Text = "Staff & User Access",
                Font = FontHelper.CreateFont(15F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            _lblSubtitle = new Label
            {
                Text = "Manage employee accounts, security roles, system permissions, and login audit.",
                Font = FontHelper.CreateFont(8.75F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 26)
            };
            _btnAddUser = new ModernButton
            {
                Text = "Add Staff Member",
                IconName = "plus",
                ButtonType = ModernButtonType.Primary,
                Size = new Size(185, 38)
            };
            _btnAddUser.Click += BtnAddUser_Click;

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_btnAddUser);
            _contentWrapper.Controls.Add(_headerPanel);

            // 2. Metrics Row
            _metricsPanel = new Panel
            {
                BackColor = ThemeManager.Background,
                Height = 78
            };
            _cardTotalStaff = new ManagementStatCard { IconName = "users", AccentColor = Color.FromArgb(59, 130, 246) };
            _cardAdmins = new ManagementStatCard { IconName = "shield", AccentColor = Color.FromArgb(139, 92, 246) };
            _cardActiveStaff = new ManagementStatCard { IconName = "check", AccentColor = Color.FromArgb(16, 185, 129) };
            _cardManagersCashiers = new ManagementStatCard { IconName = "customer", AccentColor = Color.FromArgb(245, 158, 11) };

            _metricsPanel.Controls.AddRange(new Control[] { _cardTotalStaff, _cardAdmins, _cardActiveStaff, _cardManagersCashiers });
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
                PlaceholderText = "Search staff name, email, or ID...",
                Size = new Size(270, 36)
            };
            _searchBox.SearchTextChanged += (s, e) =>
            {
                _searchFilter = _searchBox.Text.Trim();
                RefreshGrid();
            };

            _cboRole = new ModernComboBox
            {
                Size = new Size(170, 34)
            };
            _cboRole.SelectedIndexChanged += (s, e) =>
            {
                _roleFilter = _cboRole.SelectedItem?.ToString() ?? "All Roles";
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
                Text = "Showing 0 accounts",
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                AutoSize = true
            };

            _toolbarCard.Controls.Add(_searchBox);
            _toolbarCard.Controls.Add(_cboRole);
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

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "User ID", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", FillWeight = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email Address", FillWeight = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Role", FillWeight = 95 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", FillWeight = 65 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Last Login", FillWeight = 95 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 45 });

            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _lblEmptyState = new Label
            {
                Text = "No staff members found matching your search.",
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

        private void PopulateRoleFilter()
        {
            _cboRole.Items.Clear();
            _cboRole.Items.Add("All Roles");
            _cboRole.Items.Add("Administrator");
            _cboRole.Items.Add("Store Manager");
            _cboRole.Items.Add("Cashier");
            _cboRole.SelectedIndex = 0;
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
            var user = (AppUser)_grid.Rows[e.RowIndex].Tag;
            if (user == null) return;

            if (e.ColumnIndex == 6) // Edit
            {
                ShowEditUserDialog(user);
            }
            else if (e.ColumnIndex == 7) // Delete
            {
                string confirmMsg = TranslationManager.T("ConfirmDeleteStaff", $"Are you sure you want to delete staff account '{user.FullName}' from the database?");
                if (MessageBox.Show(confirmMsg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _dataService.DeleteUser(user.Id);
                    RefreshGrid();
                }
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var user = (AppUser)_grid.Rows[e.RowIndex].Tag;
                if (user != null) ShowEditUserDialog(user);
            }
        }

        public void ApplySearch(string query)
        {
            _searchFilter = query;
            _searchBox.Text = query;
            RefreshGrid();
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            var newUser = ModernDialogHelper.ShowUserDialog(this);
            if (newUser != null)
            {
                _dataService.AddUser(newUser);
                RefreshGrid();
            }
        }

        private void ShowEditUserDialog(AppUser user)
        {
            if (user == null) return;
            var updated = ModernDialogHelper.ShowUserDialog(this, user);
            if (updated != null)
            {
                _dataService.UpdateUser(updated);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            var allUsers = _dataService.Users;
            var filtered = allUsers.AsEnumerable();

            if (!string.IsNullOrEmpty(_roleFilter) && _roleFilter != "All Roles")
            {
                filtered = filtered.Where(u => string.Equals(u.Role, _roleFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                filtered = filtered.Where(u => (u.FullName?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                               (u.Email?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                               (u.Id?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            var list = filtered.ToList();
            foreach (var u in list)
            {
                string lastLoginStr = u.LastLogin != default ? u.LastLogin.ToString("MMM dd, yyyy HH:mm") : "Never";
                int rowIdx = _grid.Rows.Add(u.Id, u.FullName, u.Email, u.Role, u.Status, lastLoginStr, "Edit", "Delete");
                _grid.Rows[rowIdx].Tag = u;
            }

            // Update KPI Metric Cards
            int totalCount = allUsers.Count;
            int adminCount = allUsers.Count(u => string.Equals(u.Role, "Administrator", StringComparison.OrdinalIgnoreCase));
            int activeCount = allUsers.Count(u => u.Status == "Active");
            int otherStaffCount = totalCount - adminCount;

            _cardTotalStaff.SetData(TranslationManager.T("TotalStaff", "Total Staff"), $"{totalCount}", $"{list.Count} shown", "users", Color.FromArgb(59, 130, 246));
            _cardAdmins.SetData(TranslationManager.T("Administrators", "Administrators"), $"{adminCount}", "Full Access", "shield", Color.FromArgb(139, 92, 246));
            _cardActiveStaff.SetData(TranslationManager.T("ActiveAccounts", "Active Status"), $"{activeCount}", $"{Math.Round((double)activeCount / Math.Max(1, totalCount) * 100)}%", "check", Color.FromArgb(16, 185, 129));
            _cardManagersCashiers.SetData(TranslationManager.T("StaffMembers", "Managers & Cashiers"), $"{otherStaffCount}", "Operational", "customer", Color.FromArgb(245, 158, 11));

            _lblCount.Text = $"Showing {list.Count} of {totalCount} accounts";

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
            _lblTitle.Text = TranslationManager.T("Users", "Staff & User Access");
            _lblSubtitle.Text = TranslationManager.T("UsersSubtitle", "Manage employee accounts, security roles, system permissions, and login audit.");
            _btnAddUser.Text = TranslationManager.T("AddStaffMember", "Add Staff Member");
            _btnRefresh.Text = TranslationManager.T("Refresh", "Refresh");

            if (_grid.Columns.Count >= 8)
            {
                _grid.Columns[0].HeaderText = TranslationManager.T("UserId", "User ID");
                _grid.Columns[1].HeaderText = TranslationManager.T("FullName", "Full Name");
                _grid.Columns[2].HeaderText = TranslationManager.T("EmailAddress", "Email Address");
                _grid.Columns[3].HeaderText = TranslationManager.T("Role", "Role");
                _grid.Columns[4].HeaderText = TranslationManager.T("Status", "Status");
                _grid.Columns[5].HeaderText = TranslationManager.T("LastLogin", "Last Login");
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
            _btnAddUser.Location = new Point(containerW - _btnAddUser.Width, 5);

            currentY += _headerPanel.Height + gap;

            // 2. Metrics Row (4 Cards)
            _metricsPanel.Location = new Point(0, currentY);
            _metricsPanel.Width = containerW;

            int cardGap = 12;
            int cardW = (containerW - (cardGap * 3)) / 4;
            _cardTotalStaff.SetBounds(0, 0, cardW, 78);
            _cardAdmins.SetBounds(cardW + cardGap, 0, cardW, 78);
            _cardActiveStaff.SetBounds((cardW + cardGap) * 2, 0, cardW, 78);
            _cardManagersCashiers.SetBounds((cardW + cardGap) * 3, 0, containerW - ((cardW + cardGap) * 3), 78);

            currentY += _metricsPanel.Height + gap;

            // 3. Toolbar Card
            _toolbarCard.Location = new Point(0, currentY);
            _toolbarCard.Width = containerW;

            _searchBox.Location = new Point(12, 10);
            _cboRole.Location = new Point(_searchBox.Right + 12, 11);
            _btnRefresh.Location = new Point(_cboRole.Right + 12, 10);

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
