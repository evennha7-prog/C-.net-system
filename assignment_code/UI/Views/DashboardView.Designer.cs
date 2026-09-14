namespace assignment_code.UI.Views
{
    partial class DashboardView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
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
            // 
            // _contentWrapper
            // 
            this._contentWrapper.BackColor = System.Drawing.Color.Transparent;
            this._contentWrapper.Controls.Add(this._kpiPosts);
            this._contentWrapper.Controls.Add(this._kpiCategories);
            this._contentWrapper.Controls.Add(this._kpiMedia);
            this._contentWrapper.Controls.Add(this._kpiComments);
            this._contentWrapper.Controls.Add(this._barChartPostGrowth);
            this._contentWrapper.Controls.Add(this._splineChartCommentsTrend);
            this._contentWrapper.Controls.Add(this._latestPostsCard);
            this._contentWrapper.Controls.Add(this._recentCommentsCard);
            this._contentWrapper.Location = new System.Drawing.Point(0, 0);
            this._contentWrapper.Name = "_contentWrapper";
            this._contentWrapper.Size = new System.Drawing.Size(1012, 720);
            this._contentWrapper.TabIndex = 0;
            // 
            // _kpiPosts
            // 
            this._kpiPosts.BackColor = System.Drawing.Color.White;
            this._kpiPosts.Location = new System.Drawing.Point(0, 14);
            this._kpiPosts.Name = "_kpiPosts";
            this._kpiPosts.Size = new System.Drawing.Size(235, 126);
            this._kpiPosts.TabIndex = 0;
            // 
            // _kpiCategories
            // 
            this._kpiCategories.BackColor = System.Drawing.Color.White;
            this._kpiCategories.Location = new System.Drawing.Point(251, 14);
            this._kpiCategories.Name = "_kpiCategories";
            this._kpiCategories.Size = new System.Drawing.Size(235, 126);
            this._kpiCategories.TabIndex = 1;
            // 
            // _kpiMedia
            // 
            this._kpiMedia.BackColor = System.Drawing.Color.White;
            this._kpiMedia.Location = new System.Drawing.Point(502, 14);
            this._kpiMedia.Name = "_kpiMedia";
            this._kpiMedia.Size = new System.Drawing.Size(235, 126);
            this._kpiMedia.TabIndex = 2;
            // 
            // _kpiComments
            // 
            this._kpiComments.BackColor = System.Drawing.Color.White;
            this._kpiComments.Location = new System.Drawing.Point(753, 14);
            this._kpiComments.Name = "_kpiComments";
            this._kpiComments.Size = new System.Drawing.Size(259, 126);
            this._kpiComments.TabIndex = 3;
            // 
            // _barChartPostGrowth
            // 
            this._barChartPostGrowth.BackColor = System.Drawing.Color.White;
            this._barChartPostGrowth.Location = new System.Drawing.Point(0, 156);
            this._barChartPostGrowth.Name = "_barChartPostGrowth";
            this._barChartPostGrowth.Size = new System.Drawing.Size(438, 250);
            this._barChartPostGrowth.TabIndex = 4;
            this._barChartPostGrowth.Title = "Product Growth";
            // 
            // _splineChartCommentsTrend
            // 
            this._splineChartCommentsTrend.BackColor = System.Drawing.Color.White;
            this._splineChartCommentsTrend.Location = new System.Drawing.Point(454, 156);
            this._splineChartCommentsTrend.Name = "_splineChartCommentsTrend";
            this._splineChartCommentsTrend.Size = new System.Drawing.Size(558, 250);
            this._splineChartCommentsTrend.TabIndex = 5;
            this._splineChartCommentsTrend.Title = "Reports Trend";
            // 
            // _latestPostsCard
            // 
            this._latestPostsCard.BackColor = System.Drawing.Color.White;
            this._latestPostsCard.Location = new System.Drawing.Point(0, 422);
            this._latestPostsCard.Name = "_latestPostsCard";
            this._latestPostsCard.Size = new System.Drawing.Size(438, 230);
            this._latestPostsCard.TabIndex = 6;
            // 
            // _recentCommentsCard
            // 
            this._recentCommentsCard.BackColor = System.Drawing.Color.White;
            this._recentCommentsCard.Location = new System.Drawing.Point(454, 422);
            this._recentCommentsCard.Name = "_recentCommentsCard";
            this._recentCommentsCard.Size = new System.Drawing.Size(558, 230);
            this._recentCommentsCard.TabIndex = 7;
            // 
            // DashboardView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this._contentWrapper);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
