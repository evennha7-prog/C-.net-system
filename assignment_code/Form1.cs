using System;
using System.Drawing;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI;
using assignment_code.UI.Controls;
using assignment_code.UI.Views;

namespace assignment_code
{
    public partial class Form1 : Form
    {
        private SidebarControl _sidebar;
        private Panel _mainContainer;
        private TopHeaderControl _topHeader;
        private Panel _viewsContainer;

        // Modular Views
        private DashboardView _dashboardView;
        private ProductsView _productsView;
        private CategoriesView _categoriesView;
        private PosView _posView;
        private SalesListView _salesListView;
        private OrdersView _ordersView;
        private CustomersView _customersView;
        private UsersView _usersView;
        private ReportsView _reportsView;
        private SettingsView _settingsView;
        private Control _activeView;


        public Form1() : this(null)
        {
        }

        public Form1(AppUser user)
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            if (user != null)
            {
                StoreDataService.Instance.CurrentUser = user;
            }

            BuildLayout();

            DoubleBufferHelper.EnableDoubleBufferingTree(this);

            ThemeManager.ThemeChanged += OnThemeChanged;
            TranslationManager.LanguageChanged += (s, e) =>
            {
                RefreshAllScreens();
            };
            ApplyTheme();
            FontHelper.ApplyFontHierarchy(this);
        }

        private void BuildLayout()
        {
            SuspendLayout();

            // Wire up Sidebar events
            _sidebar.MenuSelected += Sidebar_MenuSelected;

            // Wire up TopHeader events
            var curUser = StoreDataService.Instance.CurrentUser;
            if (curUser != null)
            {
                _topHeader.UserName = curUser.FullName;
            }
            _topHeader.SearchTextChanged += TopHeader_SearchTextChanged;
            _topHeader.ProfileClicked += (s, e) =>
            {
                var u = StoreDataService.Instance.CurrentUser ?? new AppUser { FullName = "Stephanie Sharkey", Role = "Administrator", Status = "Active", Email = "stephanie@pccfpistore.com" };
                string store = EnvLoader.Get("STORE_NAME", "PCCFPI STORE");
                MessageBox.Show($"Logged in as: {u.FullName}\nRole: {u.Role}\nEmail: {u.Email}\nStore: {store}\nStatus: {u.Status}", "User Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            _viewsContainer.BringToFront();

            // Initialize all modular views
            _dashboardView = new DashboardView { Dock = DockStyle.Fill };
            _productsView = new ProductsView { Dock = DockStyle.Fill, Visible = false };
            _categoriesView = new CategoriesView { Dock = DockStyle.Fill, Visible = false };
            _posView = new PosView { Dock = DockStyle.Fill, Visible = false };
            _salesListView = new SalesListView { Dock = DockStyle.Fill, Visible = false };
            _ordersView = new OrdersView { Dock = DockStyle.Fill, Visible = false };
            _customersView = new CustomersView { Dock = DockStyle.Fill, Visible = false };
            _usersView = new UsersView { Dock = DockStyle.Fill, Visible = false };
            _reportsView = new ReportsView { Dock = DockStyle.Fill, Visible = false };
            _settingsView = new SettingsView { Dock = DockStyle.Fill, Visible = false };

            _viewsContainer.Controls.AddRange(new Control[]
            {
                _dashboardView,
                _productsView,
                _categoriesView,
                _posView,
                _salesListView,
                _ordersView,
                _customersView,
                _usersView,
                _reportsView,
                _settingsView
            });

            foreach (Control v in new Control[]
            {
                _dashboardView, _productsView, _categoriesView, _posView,
                _salesListView, _ordersView, _customersView, _usersView,
                _reportsView, _settingsView
            })
            {
                v.Bounds = _viewsContainer.ClientRectangle;
            }

            _activeView = _dashboardView;

            ResumeLayout(true);
        }

        public void RefreshAllScreens()
        {
            try
            {
                foreach (Control c in _viewsContainer.Controls)
                {
                    if (c is IRefreshableView r)
                    {
                        r.RefreshView();
                    }
                }
                _topHeader?.Invalidate();
                _sidebar?.Invalidate();
                FontHelper.ApplyFontHierarchy(this);
                Invalidate(true);
            }
            catch { }
        }

        private void ShowView(Control targetView)
        {
            if (targetView == null) return;

            _viewsContainer.SuspendLayout();

            targetView.Dock = DockStyle.Fill;
            targetView.Bounds = _viewsContainer.ClientRectangle;
            targetView.Visible = true;
            targetView.BringToFront();

            if (_activeView != null && _activeView != targetView)
            {
                _activeView.Visible = false;
            }

            _activeView = targetView;

            _viewsContainer.ResumeLayout(true);
            _viewsContainer.PerformLayout();
            targetView.PerformLayout();

            (targetView as IRefreshableView)?.RefreshView();
        }

        private void TopHeader_SearchTextChanged(object sender, EventArgs e)
        {
            string query = _topHeader.SearchText;
            _dashboardView?.ApplySearch(query);
            _productsView?.ApplySearch(query);
            _salesListView?.ApplySearch(query);
            _customersView?.ApplySearch(query);
            _usersView?.ApplySearch(query);
            _posView?.ApplySearch(query);
        }

        private void Sidebar_MenuSelected(object sender, string menuName)
        {
            switch (menuName)
            {
                case "Dashboard":
                    ShowView(_dashboardView);
                    break;
                case "Products":
                    ShowView(_productsView);
                    break;
                case "Categories":
                    ShowView(_categoriesView);
                    break;
                case "Sale":
                case "POS":
                    ShowView(_posView);
                    break;
                case "List sale":
                    ShowView(_salesListView);
                    break;
                case "Order":
                    ShowView(_ordersView);
                    break;
                case "Customer":
                    ShowView(_customersView);
                    break;
                case "Users":
                    ShowView(_usersView);
                    break;
                case "Report":
                    ShowView(_reportsView);
                    break;
                case "Appearance":
                case "Settings":
                    ShowView(_settingsView);
                    break;
                case "Logout":
                    HandleLogout();
                    break;
                default:
                    ShowView(_dashboardView);
                    break;
            }
        }

        public bool IsLoggedOut { get; private set; }

        private void HandleLogout()
        {
            string title = TranslationManager.T("Logout", "Logout");
            string msg = TranslationManager.T("ConfirmLogout", "Are you sure you want to log out from PCCFPI STORE?");
            if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IsLoggedOut = true;
                StoreDataService.Instance.CurrentUser = null;
                Close();
            }
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_mainContainer != null) _mainContainer.BackColor = ThemeManager.Background;
            if (_viewsContainer != null) _viewsContainer.BackColor = ThemeManager.Background;
            if (_topHeader != null) _topHeader.BackColor = ThemeManager.Background;
            if (_sidebar != null) _sidebar.BackColor = ThemeManager.SidebarBackground;
            Invalidate(true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        private void _topHeader_Click(object sender, EventArgs e)
        {

        }

        private void _sidebar_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
