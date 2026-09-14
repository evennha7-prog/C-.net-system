using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Models;

namespace assignment_code.UI.Controls
{
    public class SplineChartControl : Control
    {
        private SplineChartDataset _dataset = new SplineChartDataset();
        private string _title = "Reports Trend";
        private string _selectedPeriod = "Last 15 days";
        private Rectangle _periodButtonRect;
        private int _borderRadius = 16;
        private double _maxAxisValue = 50;
        private int _hoveredPointIndex = -1;

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

        public SplineChartControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(420, 260);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Bind(SplineChartDataset dataset, string period = "Last 15 days")
        {
            _dataset = dataset ?? new SplineChartDataset();
            _selectedPeriod = period;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int prev = _hoveredPointIndex;
            _hoveredPointIndex = -1;

            if (_periodButtonRect.Contains(e.Location))
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            int chartLeft = 40;
            int chartRight = Width - 20;
            int plotW = chartRight - chartLeft;

            if (_dataset.XLabels.Count > 1 && e.X >= chartLeft && e.X <= chartRight)
            {
                float step = (float)plotW / (_dataset.XLabels.Count - 1);
                int idx = (int)Math.Round((e.X - chartLeft) / step);
                if (idx >= 0 && idx < _dataset.XLabels.Count)
                {
                    _hoveredPointIndex = idx;
                }
            }

            if (prev != _hoveredPointIndex)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredPointIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_periodButtonRect.Contains(e.Location))
            {
                var menu = new ContextMenuStrip();
                var item1 = menu.Items.Add("Last 15 days");
                var item2 = menu.Items.Add("Last 30 days");
                var item3 = menu.Items.Add("Last 7 days");

                item1.Click += (s, ev) => ChangePeriod("Last 15 days");
                item2.Click += (s, ev) => ChangePeriod("Last 30 days");
                item3.Click += (s, ev) => ChangePeriod("Last 7 days");

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

            // 1. Container
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

            // 2. Title ("Comments Trend")
            using (Font titleFont = FontHelper.CreateFontForText(_title, 12F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_title, titleFont, titleBrush, padding, padding);
            }

            // 3. Period Dropdown Button ("Last 15 days ▾")
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

            // 4. Legend Items (Approved, Pending, Spam/Rejected)
            int legendY = padding + 32;
            int currentLegendX = padding;
            int sqSize = 10;

            using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                foreach (var series in _dataset.Series)
                {
                    Rectangle sqRect = new Rectangle(currentLegendX, legendY + 3, sqSize, sqSize);
                    using (GraphicsPath sqPath = GraphicsHelper.GetRoundedRectanglePath(sqRect, 2))
                    using (SolidBrush sqBrush = new SolidBrush(series.LineColor))
                    {
                        g.FillPath(sqBrush, sqPath);
                    }

                    using (Font legFont = FontHelper.CreateFontForText(series.Name, 8.5F, FontStyle.Regular))
                    {
                        g.DrawString(series.Name, legFont, textBrush, currentLegendX + sqSize + 6, legendY);
                        SizeF sz = g.MeasureString(series.Name, legFont);
                        currentLegendX += sqSize + 6 + (int)sz.Width + 16;
                    }
                }
            }

            // 5. Gridlines and Y-Axis
            int chartLeft = 40;
            int chartRight = Width - 20;
            int chartTop = 95;
            int chartBottom = Height - 35;
            int plotH = chartBottom - chartTop;
            int plotW = chartRight - chartLeft;

            int ySteps = 5;
            using (Font axisFont = FontHelper.CreateFont(8.25F, FontStyle.Regular))
            using (SolidBrush axisBrush = new SolidBrush(ThemeManager.TextMuted))
            using (Pen gridPen = new Pen(ThemeManager.GridLineColor, 1f) { DashStyle = DashStyle.Dash, DashPattern = new float[] { 3, 3 } })
            {
                for (int i = 0; i <= ySteps; i++)
                {
                    int val = (int)(_maxAxisValue * i / ySteps);
                    float y = chartBottom - (float)i / ySteps * plotH;

                    string label = val.ToString();
                    SizeF s = g.MeasureString(label, axisFont);
                    g.DrawString(label, axisFont, axisBrush, chartLeft - s.Width - 8, y - s.Height / 2f);

                    g.DrawLine(gridPen, chartLeft, y, chartRight, y);
                }
            }

            if (_dataset.XLabels.Count < 2) return;

            float stepX = (float)plotW / (_dataset.XLabels.Count - 1);

            // 6. X-Axis Labels (1..15)
            using (Font xFont = FontHelper.CreateFont(8.25F, FontStyle.Regular))
            using (SolidBrush xBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                for (int i = 0; i < _dataset.XLabels.Count; i++)
                {
                    float x = chartLeft + i * stepX;
                    string lbl = _dataset.XLabels[i];
                    SizeF sz = g.MeasureString(lbl, xFont);
                    g.DrawString(lbl, xFont, xBrush, x - sz.Width / 2f, chartBottom + 6);
                }
            }

            // 7. Hover Guide Line
            if (_hoveredPointIndex >= 0 && _hoveredPointIndex < _dataset.XLabels.Count)
            {
                float hx = chartLeft + _hoveredPointIndex * stepX;
                using (Pen guidePen = new Pen(ThemeManager.BorderColor, 1.5f) { DashStyle = DashStyle.Dot })
                {
                    g.DrawLine(guidePen, hx, chartTop, hx, chartBottom);
                }
            }

            // 8. Draw Spline Curves
            foreach (var series in _dataset.Series)
            {
                if (series.Values == null || series.Values.Count < 2) continue;

                var points = new List<PointF>();
                int count = Math.Min(series.Values.Count, _dataset.XLabels.Count);
                for (int i = 0; i < count; i++)
                {
                    float x = chartLeft + i * stepX;
                    float val = (float)Math.Max(0, Math.Min(_maxAxisValue, series.Values[i]));
                    float y = chartBottom - (val / (float)_maxAxisValue * plotH);
                    points.Add(new PointF(x, y));
                }

                using (Pen curvePen = new Pen(series.LineColor, 2.0f)
                {
                    DashStyle = DashStyle.Dash,
                    DashPattern = new float[] { 3.5f, 2.5f },
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    g.DrawCurve(curvePen, points.ToArray(), 0.5f);
                }

                // Draw highlighted point if hovered
                if (_hoveredPointIndex >= 0 && _hoveredPointIndex < points.Count)
                {
                    var pt = points[_hoveredPointIndex];
                    using (SolidBrush dotBrush = new SolidBrush(series.LineColor))
                    using (SolidBrush innerBrush = new SolidBrush(ThemeManager.CardBackground))
                    {
                        g.FillEllipse(dotBrush, pt.X - 5, pt.Y - 5, 10, 10);
                        g.FillEllipse(innerBrush, pt.X - 2.5f, pt.Y - 2.5f, 5, 5);
                    }
                }
            }
        }
    }
}
