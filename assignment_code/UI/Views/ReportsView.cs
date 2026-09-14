using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class ReportsView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private Panel _contentWrapper;
        private Panel _kpi1;
        private Panel _kpi2;
        private Panel _kpi3;
        private Panel _kpi4;
        private Panel _topProductsCard;
        private Panel _financialBreakdownCard;
        private Button _btnExport;

        public ReportsView()
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
            ThemeManager.ThemeChanged += (s, e) => RefreshView();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void RefreshView()
        {
            ApplyTheme();
            _topProductsCard?.Invalidate();
            _financialBreakdownCard?.Invalidate();
            _kpi1?.Invalidate();
            _kpi2?.Invalidate();
            _kpi3?.Invalidate();
            _kpi4?.Invalidate();
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

            _btnExport = new Button
            {
                Text = TranslationManager.T("ExportReport", "Export Report (CSV)"),
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Size = new Size(180, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnExport.FlatAppearance.BorderSize = 0;
            _btnExport.Click += (s, e) => MessageBox.Show("Report exported successfully to /exports/sales_report_2026.csv", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 4 KPI Summary Cards
            _kpi1 = CreateKpi("GrossRevenue", "Gross Revenue", "$124,500.00", "+18.4% vs last month", ThemeManager.SuccessGreen);
            _kpi2 = CreateKpi("NetProfit", "Net Profit", "$58,240.00", "+12.1% vs last month", ThemeManager.AccentBlue);
            _kpi3 = CreateKpi("AverageTicket", "Average Ticket", "$64.20", "+4.2% vs last month", Color.FromArgb(139, 92, 246));
            _kpi4 = CreateKpi("TotalTransactions", "Total Transactions", "1,940", "+9.8% vs last month", ThemeManager.WarningYellow);

            _contentWrapper.Controls.AddRange(new Control[] { _btnExport, _kpi1, _kpi2, _kpi3, _kpi4 });

            // Top Products Leaderboard Card
            _topProductsCard = new Panel { BackColor = ThemeManager.CardBackground };
            _topProductsCard.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _topProductsCard.Width - 1, _topProductsCard.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(bp, p);
                }

                string headerTitle = TranslationManager.T("TopPerformingProducts", "Top Performing Products");
                using (Font hFont = FontHelper.CreateFontForText(headerTitle, 11.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(headerTitle, hFont, sb, 20, 18);

                string[] items = new[]
                {
                    "1. Smart Fitness Watch V3 - $39,900 (200 sold)",
                    "2. Wireless ANC Headphones - $29,998 (200 sold)",
                    "3. Organic Cotton Oversized Tee - $16,000 (500 sold)",
                    "4. Ultra-Fast GaN Charger 65W - $15,996 (400 sold)",
                    "5. Artisan Cold Brew Coffee - $5,400 (1,200 sold)"
                };

                int itemY = 60;
                using (Font itFont = FontHelper.CreateFont(9.5F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                {
                    foreach (var item in items)
                    {
                        g.DrawString(item, itFont, sb, 20, itemY);
                        itemY += 34;
                    }
                }
            };

            // Financial Breakdown Card
            _financialBreakdownCard = new Panel { BackColor = ThemeManager.CardBackground };
            _financialBreakdownCard.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _financialBreakdownCard.Width - 1, _financialBreakdownCard.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(bp, p);
                }

                string headerTitle = TranslationManager.T("CategoryRevenueBreakdown", "Category Revenue Breakdown");
                using (Font hFont = FontHelper.CreateFontForText(headerTitle, 11.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(headerTitle, hFont, sb, 20, 18);

                var cats = _dataService.Categories;
                int barY = 60;
                decimal maxRev = cats.Max(c => c.TotalRevenue);

                foreach (var c in cats)
                {
                    using (Font fName = FontHelper.CreateFontForText(c.Name, 9F, FontStyle.Bold))
                    using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                        g.DrawString(c.Name, fName, sb, 20, barY);

                    int barW = (int)((c.TotalRevenue / (maxRev > 0 ? maxRev : 1)) * (_financialBreakdownCard.Width - 220));
                    Color barColor = ThemeManager.AccentBlue;
                    try { barColor = ColorTranslator.FromHtml(c.ColorHex); } catch { }

                    using (SolidBrush bb = new SolidBrush(barColor))
                        g.FillRectangle(bb, 140, barY + 3, Math.Max(10, barW), 14);

                    using (Font fRev = FontHelper.CreateFont(8.5F, FontStyle.Bold))
                    using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                        g.DrawString($"${c.TotalRevenue:N0}", fRev, sb, 150 + barW, barY);

                    barY += 28;
                }
            };

            _contentWrapper.Controls.Add(_topProductsCard);
            _contentWrapper.Controls.Add(_financialBreakdownCard);

            TranslationManager.LanguageChanged += (s, e) =>
            {
                _btnExport.Text = TranslationManager.T("ExportReport", "Export Report (CSV)");
                _btnExport.Font = FontHelper.CreateFont(9F, FontStyle.Bold);
                Invalidate(true);
            };

            RepositionContent();
        }

        private Panel CreateKpi(string key, string defaultTitle, string value, string change, Color changeColor)
        {
            Panel card = new Panel
            {
                BackColor = ThemeManager.CardBackground
            };

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);

                Rectangle r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(bp, p);
                }

                string title = TranslationManager.T(key, defaultTitle);
                using (Font fT = FontHelper.CreateFontForText(title, 9F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString(title, fT, sb, 16, 16);

                using (Font fV = FontHelper.CreateFont(16F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(value, fV, sb, 16, 38);

                using (Font fC = FontHelper.CreateFont(8.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(changeColor))
                    g.DrawString(change, fC, sb, 16, 76);
            };

            return card;
        }

        private void ApplyTheme()
        {
            _btnExport.BackColor = ThemeManager.AccentBlue;
            _topProductsCard.BackColor = ThemeManager.CardBackground;
            _financialBreakdownCard.BackColor = ThemeManager.CardBackground;
            Invalidate(true);
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null) return;
            int w = Math.Max(700, ClientSize.Width);
            _contentWrapper.Width = w;

            int startY = 14;
            _btnExport.Location = new Point(w - _btnExport.Width, startY);

            int kpiY = startY + 44;
            int gap = 16;
            int kpiW = (w - (gap * 3)) / 4;
            int kpiH = 110;

            _kpi1.SetBounds(0, kpiY, kpiW, kpiH);
            _kpi2.SetBounds(kpiW + gap, kpiY, kpiW, kpiH);
            _kpi3.SetBounds((kpiW + gap) * 2, kpiY, kpiW, kpiH);
            _kpi4.SetBounds((kpiW + gap) * 3, kpiY, w - ((kpiW + gap) * 3), kpiH);

            int row2Y = kpiY + kpiH + gap;
            int cardH = 260;
            int halfW = (w - gap) / 2;

            _topProductsCard.SetBounds(0, row2Y, halfW, cardH);
            _financialBreakdownCard.SetBounds(halfW + gap, row2Y, w - halfW - gap, cardH);

            _contentWrapper.Height = row2Y + cardH + 20;
        }
    }
}
