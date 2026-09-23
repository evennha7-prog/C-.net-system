using System;
using System.Drawing;
using System.Windows.Forms;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class DashboardView : UserControl, IRefreshableView
    {
        private DashboardService _dashboardService = new DashboardService();
        private Label _lblTitle;
        private Label _lblSubtitle;

        public DashboardView()
        {
            SetStyle(ControlStyles.UserPaint |
                      ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.ResizeRedraw, true);

            InitializeComponent();
            InitializeHeader();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            SetupEventHandlers();
            BindInitialData();
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => { ApplyTheme(); Invalidate(true); };
            StoreDataService.Instance.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) =>
            {
                if (_lblTitle != null) _lblTitle.Text = TranslationManager.T("DashboardOverview", "Store Overview & Analytics");
                if (_lblSubtitle != null) _lblSubtitle.Text = TranslationManager.T("DashboardSubtitle", "Real-time revenue, inventory status, and order performance.");
                if (_barChartPostGrowth != null) _barChartPostGrowth.Title = TranslationManager.T("ProductGrowth", "Product Growth");
                if (_splineChartCommentsTrend != null) _splineChartCommentsTrend.Title = TranslationManager.T("SalesOrderTrends", "Sales & Order Trends");
                RefreshView();
            };
        }

        private void InitializeHeader()
        {
            _lblTitle = new Label
            {
                Text = TranslationManager.T("DashboardOverview", "Store Overview & Analytics"),
                Font = FontHelper.CreateFont(13.5F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _lblSubtitle = new Label
            {
                Text = TranslationManager.T("DashboardSubtitle", "Real-time revenue, inventory status, and order performance."),
                Font = FontHelper.CreateFont(8.75F, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            if (_contentWrapper != null)
            {
                _contentWrapper.Controls.Add(_lblTitle);
                _contentWrapper.Controls.Add(_lblSubtitle);
            }
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_contentWrapper != null) _contentWrapper.BackColor = ThemeManager.Background;
            if (_lblTitle != null) _lblTitle.ForeColor = ThemeManager.TextPrimary;
            if (_lblSubtitle != null) _lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            Invalidate(true);
        }

        private void SetupEventHandlers()
        {
            _barChartPostGrowth.PeriodChanged += (s, period) =>
            {
                _barChartPostGrowth.Bind(_dashboardService.GetPostGrowthData(period), period);
            };
            _splineChartCommentsTrend.PeriodChanged += (s, period) =>
            {
                _splineChartCommentsTrend.Bind(_dashboardService.GetCommentsTrendData(period), period);
            };
            _latestPostsCard.PostActionClicked += (s, post) =>
            {
                MessageBox.Show($"Product: {post.Title}\nStatus: {post.Status}\nPublished: {post.Date}", "Product Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            _recentCommentsCard.ViewCommentClicked += (s, comment) =>
            {
                MessageBox.Show($"Author: {comment.Author}\nReport: {comment.Preview}\nDate: {comment.Date}", "Report Inspection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void DashboardView_Resize(object sender, EventArgs e)
        {
            RepositionContent();
        }

        public void RefreshView()
        {
            BindInitialData();
            RepositionContent();
            Invalidate(true);
        }

        public void ApplySearch(string query)
        {
            _latestPostsCard?.Bind(_dashboardService.GetLatestPosts(query));
            _recentCommentsCard?.Bind(_dashboardService.GetRecentComments(query));
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null || _kpiPosts == null) return;

            int containerW = Math.Max(720, ClientSize.Width);
            _contentWrapper.Width = containerW;

            int pad = 20;
            int gap = 16;
            int contentW = containerW - (pad * 2);

            int startY = 16;
            if (_lblTitle != null) _lblTitle.Location = new Point(pad, startY);
            if (_lblSubtitle != null) _lblSubtitle.Location = new Point(pad, startY + 28);

            // Row 1: KPI Cards
            int kpiY = startY + 54;
            int kpiW = (contentW - (gap * 3)) / 4;
            int kpiH = 120;

            _kpiPosts.SetBounds(pad, kpiY, kpiW, kpiH);
            _kpiCategories.SetBounds(pad + kpiW + gap, kpiY, kpiW, kpiH);
            _kpiMedia.SetBounds(pad + (kpiW + gap) * 2, kpiY, kpiW, kpiH);
            _kpiComments.SetBounds(pad + (kpiW + gap) * 3, kpiY, contentW - ((kpiW + gap) * 3), kpiH);

            // Row 2: Charts
            int row2Y = kpiY + kpiH + gap;
            int chartH = 260;
            int leftColW = (int)((contentW - gap) * 0.44f);
            int rightColW = contentW - leftColW - gap;

            _barChartPostGrowth.SetBounds(pad, row2Y, leftColW, chartH);
            _splineChartCommentsTrend.SetBounds(pad + leftColW + gap, row2Y, rightColW, chartH);

            // Row 3: Tables
            int row3Y = row2Y + chartH + gap;
            int tableH = 250;

            _latestPostsCard.SetBounds(pad, row3Y, leftColW, tableH);
            _recentCommentsCard.SetBounds(pad + leftColW + gap, row3Y, rightColW, tableH);

            _contentWrapper.Height = row3Y + tableH + 30;
        }

        private void BindInitialData()
        {
            var stats = _dashboardService.GetKpiStats();
            if (stats.Count >= 4)
            {
                _kpiPosts.Bind(stats[0]);
                _kpiCategories.Bind(stats[1]);
                _kpiMedia.Bind(stats[2]);
                _kpiComments.Bind(stats[3]);
            }

            _barChartPostGrowth.Bind(_dashboardService.GetPostGrowthData("6 months"), "6 months");
            _splineChartCommentsTrend.Bind(_dashboardService.GetCommentsTrendData("Last 15 days"), "Last 15 days");
            _latestPostsCard.Bind(_dashboardService.GetLatestPosts());
            _recentCommentsCard.Bind(_dashboardService.GetRecentComments());
        }
    }
}
