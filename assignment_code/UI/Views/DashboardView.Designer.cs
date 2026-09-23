namespace assignment_code.UI.Views
{
    partial class DashboardView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this._contentWrapper = new System.Windows.Forms.Panel();
            this._kpiPosts = new assignment_code.UI.Controls.KpiCardControl();
            this._kpiCategories = new assignment_code.UI.Controls.KpiCardControl();
            this._kpiMedia = new assignment_code.UI.Controls.KpiCardControl();
            this._kpiComments = new assignment_code.UI.Controls.KpiCardControl();
            this._barChartPostGrowth = new assignment_code.UI.Controls.BarChartControl();
            this._splineChartCommentsTrend = new assignment_code.UI.Controls.SplineChartControl();
            this._latestPostsCard = new assignment_code.UI.Controls.LatestPostsCard();
            this._recentCommentsCard = new assignment_code.UI.Controls.RecentCommentsCard();
            this._contentWrapper.SuspendLayout();
            this.SuspendLayout();

            this._contentWrapper.Location = new System.Drawing.Point(0, 0);
            this._contentWrapper.Name = "_contentWrapper";
            this._contentWrapper.Size = new System.Drawing.Size(1012, 720);
            this._contentWrapper.TabIndex = 0;

            this._kpiPosts.Location = new System.Drawing.Point(0, 16);
            this._kpiPosts.Name = "_kpiPosts";
            this._kpiPosts.Size = new System.Drawing.Size(235, 120);
            this._kpiPosts.TabIndex = 0;

            this._kpiCategories.Location = new System.Drawing.Point(251, 16);
            this._kpiCategories.Name = "_kpiCategories";
            this._kpiCategories.Size = new System.Drawing.Size(235, 120);
            this._kpiCategories.TabIndex = 1;

            this._kpiMedia.Location = new System.Drawing.Point(502, 16);
            this._kpiMedia.Name = "_kpiMedia";
            this._kpiMedia.Size = new System.Drawing.Size(235, 120);
            this._kpiMedia.TabIndex = 2;

            this._kpiComments.Location = new System.Drawing.Point(753, 16);
            this._kpiComments.Name = "_kpiComments";
            this._kpiComments.Size = new System.Drawing.Size(259, 120);
            this._kpiComments.TabIndex = 3;

            this._barChartPostGrowth.Location = new System.Drawing.Point(0, 300);
            this._barChartPostGrowth.Name = "_barChartPostGrowth";
            this._barChartPostGrowth.Size = new System.Drawing.Size(438, 260);
            this._barChartPostGrowth.TabIndex = 4;
            this._barChartPostGrowth.Title = "Product Growth";

            this._splineChartCommentsTrend.Location = new System.Drawing.Point(454, 300);
            this._splineChartCommentsTrend.Name = "_splineChartCommentsTrend";
            this._splineChartCommentsTrend.Size = new System.Drawing.Size(558, 260);
            this._splineChartCommentsTrend.TabIndex = 5;
            this._splineChartCommentsTrend.Title = "Sales & Order Trends";

            this._latestPostsCard.Location = new System.Drawing.Point(0, 576);
            this._latestPostsCard.Name = "_latestPostsCard";
            this._latestPostsCard.Size = new System.Drawing.Size(438, 240);
            this._latestPostsCard.TabIndex = 6;

            this._recentCommentsCard.Location = new System.Drawing.Point(454, 576);
            this._recentCommentsCard.Name = "_recentCommentsCard";
            this._recentCommentsCard.Size = new System.Drawing.Size(558, 240);
            this._recentCommentsCard.TabIndex = 7;

            this.AutoScroll = true;
            this.Controls.Add(this._contentWrapper);
            this.Font = new System.Drawing.Font("Roboto", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DashboardView";
            this.Size = new System.Drawing.Size(1012, 720);
            this.Resize += new System.EventHandler(this.DashboardView_Resize);
            this._contentWrapper.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel _contentWrapper;
        private assignment_code.UI.Controls.KpiCardControl _kpiPosts;
        private assignment_code.UI.Controls.KpiCardControl _kpiCategories;
        private assignment_code.UI.Controls.KpiCardControl _kpiMedia;
        private assignment_code.UI.Controls.KpiCardControl _kpiComments;
        private assignment_code.UI.Controls.BarChartControl _barChartPostGrowth;
        private assignment_code.UI.Controls.SplineChartControl _splineChartCommentsTrend;
        private assignment_code.UI.Controls.LatestPostsCard _latestPostsCard;
        private assignment_code.UI.Controls.RecentCommentsCard _recentCommentsCard;
    }
}
