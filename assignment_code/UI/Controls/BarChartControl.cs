using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;

namespace assignment_code.UI.Controls
{
    public class BarChartControl : Control
    {
        private List<BarChartPoint> _data = new List<BarChartPoint>();
        private string _title = "Product Growth";
        private string _selectedPeriod = "6 months";
        private int _hoveredIndex = -1;
        private Rectangle _periodButtonRect;
        private int _borderRadius = 16;
        private SplineChartControl splineChartControl1;
        private double _maxAxisValue = 50;

        public event EventHandler<string> PeriodChanged;

        [Category("Appearance")]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set { _selectedPeriod = value; Invalidate(); }
        }

        public BarChartControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(380, 260);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Bind(List<BarChartPoint> data, string period = "6 months")
        {
            _data = data ?? new List<BarChartPoint>();
            _selectedPeriod = period;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int previousHover = _hoveredIndex;
            _hoveredIndex = -1;

            // Check if hovering over period button
            if (_periodButtonRect.Contains(e.Location))
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            // Check bar hit testing
            int paddingLeft = 45;
            int paddingRight = 20;
            int chartTop = 110;
            int chartBottom = Height - 35;
            int plotWidth = Width - paddingLeft - paddingRight;

            if (_data.Count > 0 && e.Y >= chartTop && e.Y <= chartBottom + 10)
            {
                float slotWidth = (float)plotWidth / _data.Count;
                int idx = (int)((e.X - paddingLeft) / slotWidth);
                if (idx >= 0 && idx < _data.Count)
                {
                    _hoveredIndex = idx;
                }
            }

            if (previousHover != _hoveredIndex)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_periodButtonRect.Contains(e.Location))
            {
                // Toggle period or show menu
                var menu = new ContextMenuStrip();
                var item1 = menu.Items.Add("6 months");
                var item2 = menu.Items.Add("12 months");
                var item3 = menu.Items.Add("30 days");

                item1.Click += (s, ev) => ChangePeriod("6 months");
                item2.Click += (s, ev) => ChangePeriod("12 months");
                item3.Click += (s, ev) => ChangePeriod("30 days");

                menu.Show(this, new Point(_periodButtonRect.Left, _periodButtonRect.Bottom + 2));
            }
        }

        private void ChangePeriod(string period)
        {
            _selectedPeriod = period;
            PeriodChanged?.Invoke(this, period);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Card Container
            using (GraphicsPath cardPath = GraphicsHelper.GetRoundedRectanglePath(cardBounds, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(ThemeManager.CardBackground))
                {
                    g.FillPath(bgBrush, cardPath);
                }

                using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
                {
                    borderPen.Alignment = PenAlignment.Inset;
                    g.DrawPath(borderPen, cardPath);
                }
            }

            int padding = 20;

            // 2. Header: Title ("Post Growth")
            using (Font titleFont = FontHelper.CreateFontForText(_title, 12F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_title, titleFont, titleBrush, padding, padding);
            }

            // 3. Period Dropdown Button ("6 months ∨")
            string periodText = _selectedPeriod + "  ▾";
            using (Font periodFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
            {
                SizeF periodSize = g.MeasureString(periodText, periodFont);
                int btnW = (int)periodSize.Width + 16;
                int btnH = 26;
                int btnX = Width - padding - btnW;
                int btnY = padding - 2;

                _periodButtonRect = new Rectangle(btnX, btnY, btnW, btnH);

                using (GraphicsPath btnPath = GraphicsHelper.GetRoundedRectanglePath(_periodButtonRect, 6))
                {
                    using (SolidBrush btnBg = new SolidBrush(ThemeManager.HoverBackground))
                    {
                        g.FillPath(btnBg, btnPath);
                    }
                    using (Pen btnBorder = new Pen(ThemeManager.BorderColor, 1f))
                    {
                        g.DrawPath(btnBorder, btnPath);
                    }
                }

                using (SolidBrush btnTextBrush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(periodText, periodFont, btnTextBrush, _periodButtonRect, sf);
                }
            }

            // 4. Legend: Blue square + "Total number of posts"
            int legendY = padding + 32;
            int sqSize = 10;
            Rectangle sqRect = new Rectangle(padding, legendY + 3, sqSize, sqSize);
            using (GraphicsPath sqPath = GraphicsHelper.GetRoundedRectanglePath(sqRect, 2))
            using (SolidBrush sqBrush = new SolidBrush(ThemeManager.AccentBlue))
            {
                g.FillPath(sqBrush, sqPath);
            }

            string legendText = TranslationManager.T("Total number of products", "Total number of products");
            using (Font legendFont = FontHelper.CreateFontForText(legendText, 8.75F, FontStyle.Regular))
            using (SolidBrush legendBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString(legendText, legendFont, legendBrush, padding + sqSize + 8, legendY);
            }

            // 5. Chart Grid & Plot
            int chartLeft = 45;
            int chartRight = Width - 20;
            int chartTop = 95;
            int chartBottom = Height - 35;
            int plotH = chartBottom - chartTop;
            int plotW = chartRight - chartLeft;

            // Y-Axis labels and horizontal dashed lines (0, 10, 20, 30, 40, 50)
            int ySteps = 5;
            using (Font axisFont = FontHelper.CreateFont(8.25F, FontStyle.Regular))
            using (SolidBrush axisBrush = new SolidBrush(ThemeManager.TextMuted))
            using (Pen gridPen = new Pen(ThemeManager.GridLineColor, 1f) { DashStyle = DashStyle.Dash, DashPattern = new float[] { 3, 3 } })
            {
                for (int i = 0; i <= ySteps; i++)
                {
                    int val = (int)(_maxAxisValue * i / ySteps);
                    float y = chartBottom - (float)i / ySteps * plotH;

                    // Label
                    string label = val.ToString();
                    SizeF s = g.MeasureString(label, axisFont);
                    g.DrawString(label, axisFont, axisBrush, chartLeft - s.Width - 8, y - s.Height / 2f);

                    // Dashed gridline
                    g.DrawLine(gridPen, chartLeft, y, chartRight, y);
                }
            }

            if (_data == null || _data.Count == 0) return;

            // 6. Draw Bars
            float slotWidth = (float)plotW / _data.Count;
            float barWidth = Math.Min(34f, slotWidth * 0.48f);

            int tooltipIndexToDraw = -1;
            PointF tooltipPoint = PointF.Empty;
            string tooltipString = "";

            using (Font xFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
            using (SolidBrush xBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    var pt = _data[i];
                    float centerX = chartLeft + i * slotWidth + slotWidth / 2f;
                    float barHeight = (float)(pt.Value / _maxAxisValue * plotH);
                    float barX = centerX - barWidth / 2f;
                    float barY = chartBottom - barHeight;

                    bool isHighlighted = pt.IsHighlighted || (i == _hoveredIndex);

                    Color barColor = isHighlighted ? ThemeManager.BarActive : ThemeManager.BarLight;

                    // Draw rounded bar
                    RectangleF barRect = new RectangleF(barX, barY, barWidth, barHeight);
                    if (barHeight > 4)
                    {
                        using (GraphicsPath barPath = GraphicsHelper.GetRoundedRectanglePath(Rectangle.Round(barRect), 6))
                        using (SolidBrush barBrush = new SolidBrush(barColor))
                        {
                            g.FillPath(barBrush, barPath);
                        }
                    }
                    else
                    {
                        using (SolidBrush barBrush = new SolidBrush(barColor))
                        {
                            g.FillRectangle(barBrush, barRect);
                        }
                    }

                    // X-Axis Label
                    SizeF xSize = g.MeasureString(pt.Label, xFont);
                    g.DrawString(pt.Label, xFont, xBrush, centerX - xSize.Width / 2f, chartBottom + 6);

                    // Check if should record tooltip
                    if (isHighlighted && (!string.IsNullOrEmpty(pt.TooltipText) || i == _hoveredIndex))
                    {
                        tooltipIndexToDraw = i;
                        tooltipPoint = new PointF(centerX, barY);
                        tooltipString = !string.IsNullOrEmpty(pt.TooltipText) ? pt.TooltipText : $"{pt.Value} products";
                    }
                }
            }

            // 7. Draw Tooltip if active
            if (tooltipIndexToDraw >= 0 && !string.IsNullOrEmpty(tooltipString))
            {
                using (Font ttFont = FontHelper.CreateFont(8.25F, FontStyle.Bold))
                {
                    SizeF ttSize = g.MeasureString(tooltipString, ttFont);
                    int ttW = (int)ttSize.Width + 16;
                    int ttH = 24;
                    int ttX = (int)(tooltipPoint.X - ttW / 2f);
                    int ttY = (int)(tooltipPoint.Y - ttH - 7);

                    Rectangle ttRect = new Rectangle(ttX, ttY, ttW, ttH);

                    // Dark pill container
                    using (GraphicsPath ttPath = GraphicsHelper.GetRoundedRectanglePath(ttRect, 6))
                    using (SolidBrush ttBg = new SolidBrush(ThemeManager.DarkTooltipBg))
                    {
                        g.FillPath(ttBg, ttPath);

                        // Down pointer triangle
                        PointF[] triangle = new PointF[]
                        {
                            new PointF(tooltipPoint.X - 5, ttRect.Bottom),
                            new PointF(tooltipPoint.X + 5, ttRect.Bottom),
                            new PointF(tooltipPoint.X, ttRect.Bottom + 5)
                        };
                        g.FillPolygon(ttBg, triangle);
                    }

                    using (SolidBrush ttFg = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(tooltipString, ttFont, ttFg, ttRect, sf);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.splineChartControl1 = new assignment_code.UI.Controls.SplineChartControl();
            this.SuspendLayout();
            // 
            // splineChartControl1
            // 
            this.splineChartControl1.BackColor = System.Drawing.Color.Transparent;
            this.splineChartControl1.Location = new System.Drawing.Point(0, 0);
            this.splineChartControl1.Name = "splineChartControl1";
            this.splineChartControl1.SelectedPeriod = "Last 15 days";
            this.splineChartControl1.Size = new System.Drawing.Size(420, 260);
            this.splineChartControl1.TabIndex = 0;
            this.splineChartControl1.Text = "splineChartControl1";
            this.splineChartControl1.Title = "Reports Trend";
            this.ResumeLayout(false);

        }
    }
}
