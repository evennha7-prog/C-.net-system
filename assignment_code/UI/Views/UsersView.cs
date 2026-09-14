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
        private Panel _contentWrapper;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Button _btnAddUser;
        private Button _btnRefresh;
        private string _searchFilter = "";

        public UsersView()
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

            _btnAddUser = new Button
            {
                Text = TranslationManager.T("AddStaffMember", "+ Add Staff Member"),
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(170, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnAddUser.FlatAppearance.BorderSize = 0;
            _btnAddUser.Click += BtnAddUser_Click;

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

            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("UserId", "User ID"), DataPropertyName = "Id", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("FullName", "Full Name"), DataPropertyName = "FullName", FillWeight = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("EmailAddress", "Email Address"), DataPropertyName = "Email", FillWeight = 140 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Role", "Role"), DataPropertyName = "Role", FillWeight = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("Status", "Status"), DataPropertyName = "Status", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = TranslationManager.T("LastLogin", "Last Login"), DataPropertyName = "LastLogin", FillWeight = 100 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 50 });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 50 });

            DataGridViewStyleHelper.ApplyStyle(_grid);

            _grid.CellContentClick += (s, e) =>
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
                    if (MessageBox.Show($"Are you sure you want to delete staff account '{user.FullName}' from PostgreSQL database?", "Confirm Delete Staff", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _dataService.DeleteUser(user.Id);
                        RefreshGrid();
                    }
                }
            };

            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var user = (AppUser)_grid.Rows[e.RowIndex].Tag;
                    if (user != null) ShowEditUserDialog(user);
                }
            };

            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 3 && e.Value != null)
                {
                    string role = e.Value.ToString();
                    if (role == "Administrator") e.CellStyle.ForeColor = Color.FromArgb(139, 92, 246);
                    else if (role == "Store Manager") e.CellStyle.ForeColor = ThemeManager.AccentBlue;
                    else e.CellStyle.ForeColor = ThemeManager.SuccessGreen;
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                }
                else if (e.ColumnIndex == 4 && e.Value != null)
                {
                    string st = e.Value.ToString();
                    e.CellStyle.ForeColor = (st == "Active" || st == TranslationManager.T("Active", "Active")) ? ThemeManager.SuccessGreen : ThemeManager.DangerRed;
                    e.CellStyle.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                }
            };

            _contentWrapper.Controls.Add(_txtSearch);
            _contentWrapper.Controls.Add(_btnRefresh);
            _contentWrapper.Controls.Add(_btnAddUser);
            _contentWrapper.Controls.Add(_grid);

            TranslationManager.LanguageChanged += (s, e) =>
            {
                _btnAddUser.Text = TranslationManager.T("AddStaffMember", "+ Add Staff Member");
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
            var filtered = _dataService.Users.AsEnumerable();

            if (!string.IsNullOrEmpty(_searchFilter))
                filtered = filtered.Where(u => u.FullName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               u.Email.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               u.Role.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var u in filtered)
            {
                int rowIdx = _grid.Rows.Add(u.Id, u.FullName, u.Email, u.Role, u.Status, u.LastLogin.ToString("dd MMM HH:mm"), "Edit", "Delete");
                _grid.Rows[rowIdx].Tag = u;
            }
        }

        private void ApplyTheme()
        {
            DataGridViewStyleHelper.UpdateColors(_grid);

            _btnAddUser.BackColor = ThemeManager.AccentBlue;
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
            _btnAddUser.Location = new Point(w - _btnAddUser.Width, startY);

            int gridY = startY + 44;
            _grid.Location = new Point(0, gridY);
            _grid.Size = new Size(w, Math.Max(400, ClientSize.Height - gridY - 20));
            _contentWrapper.Height = gridY + _grid.Height + 20;
        }
    }
}
