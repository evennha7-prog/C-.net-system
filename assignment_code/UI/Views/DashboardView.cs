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

        public DashboardView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponent();
            SetupEventHandlers();
            BindInitialData();
            ThemeManager.ThemeChanged += (s, e) => Invalidate(true);
            StoreDataService.Instance.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) =>
            {
                _barChartPostGrowth.Title = TranslationManager.T("ProductGrowth", "Product Growth");
                _splineChartCommentsTrend.Title = TranslationManager.T("ReportsTrend", "Reports Trend");
                RefreshView();
            };
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

            int containerW = Math.Max(700, ClientSize.Width);
            _contentWrapper.Width = containerW;

            int gap = 16;
            int startY = 14;

            // Row 1: KPI Cards
            int kpiW = (containerW - (gap * 3)) / 4;
            int kpiH = 126;

            _kpiPosts.SetBounds(0, startY, kpiW, kpiH);
            _kpiCategories.SetBounds(kpiW + gap, startY, kpiW, kpiH);
            _kpiMedia.SetBounds((kpiW + gap) * 2, startY, kpiW, kpiH);
            _kpiComments.SetBounds((kpiW + gap) * 3, startY, containerW - ((kpiW + gap) * 3), kpiH);

            // Row 2: Charts
            int row2Y = startY + kpiH + gap;
            int chartH = 250;
            int leftColW = (int)((containerW - gap) * 0.44f);
            int rightColW = containerW - leftColW - gap;

            _barChartPostGrowth.SetBounds(0, row2Y, leftColW, chartH);
            _splineChartCommentsTrend.SetBounds(leftColW + gap, row2Y, rightColW, chartH);

            // Row 3: Tables
            int row3Y = row2Y + chartH + gap;
            int tableH = 230;

            _latestPostsCard.SetBounds(0, row3Y, leftColW, tableH);
            _recentCommentsCard.SetBounds(leftColW + gap, row3Y, rightColW, tableH);

            _contentWrapper.Height = row3Y + tableH + 16;
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
