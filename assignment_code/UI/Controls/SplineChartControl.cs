using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;

namespace assignment_code.UI.Controls
{
    public class SplineChartControl : Control
    {
        private SplineChartDataset _dataset = new SplineChartDataset();
        private string _title = "Sales & Order Trends";
        private string _selectedPeriod = "Last 15 days";
        private Rectangle _periodButtonRect;
        private int _borderRadius = 16;
        private double _maxAxisValue = 20;
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
                     ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.Background;
            Size = new Size(420, 260);

            ThemeManager.ThemeChanged += (s, e) =>
            {
                BackColor = ThemeManager.Background;
                Invalidate();
            };
        }

        public void Bind(SplineChartDataset dataset, string period = "Last 15 days")
        {
            _dataset = dataset ?? new SplineChartDataset();
            _selectedPeriod = period;

            // Dynamically scale max axis value
            double maxVal = 0;
            if (_dataset.Series != null)
            {
                foreach (var s in _dataset.Series)
                {
                    if (s.Values != null && s.Values.Count > 0)
                    {
                        double m = s.Values.Max();
                        if (m > maxVal) maxVal = m;
                    }
                }
            }

            if (maxVal <= 5) _maxAxisValue = 10;
            else if (maxVal <= 10) _maxAxisValue = 15;
            else if (maxVal <= 25) _maxAxisValue = 30;
            else if (maxVal <= 50) _maxAxisValue = 60;
            else _maxAxisValue = Math.Ceiling(maxVal * 1.25 / 10.0) * 10;

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
                var item1 = menu.Items.Add("Last 7 days");
                var item2 = menu.Items.Add("Last 15 days");
                var item3 = menu.Items.Add("Last 30 days");

                item1.Click += (s, ev) => ChangePeriod("Last 7 days");
                item2.Click += (s, ev) => ChangePeriod("Last 15 days");
                item3.Click += (s, ev) => ChangePeriod("Last 30 days");

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

            // 0. Paint parent background first to eliminate white corner artifacts
            Color parentBg = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : ThemeManager.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Container
            using (GraphicsPath cardPath = GraphicsHelper.GetRoundedRectanglePath(cardBounds, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(ThemeManager.CardBackground))
                {
                    g.FillPath(bgBrush, cardPath);
                }

                using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1.2f))
                {
                    borderPen.Alignment = PenAlignment.Inset;
                    g.DrawPath(borderPen, cardPath);
                }
            }

            int padding = 20;

            // 2. Title
            using (Font titleFont = FontHelper.CreateFontForText(_title, 11F, FontStyle.Bold))
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

            // 4. Legend Items
            int legendY = padding + 28;
            int currentLegendX = padding;
            int dotSize = 8;

            using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                foreach (var series in _dataset.Series)
                {
                    Rectangle dotRect = new Rectangle(currentLegendX, legendY + 4, dotSize, dotSize);
                    using (SolidBrush dotBrush = new SolidBrush(series.LineColor))
                    {
                        g.FillEllipse(dotBrush, dotRect);
                    }

                    using (Font legFont = FontHelper.CreateFontForText(series.Name, 8.5F, FontStyle.Regular))
                    {
                        g.DrawString(series.Name, legFont, textBrush, currentLegendX + dotSize + 6, legendY);
                        SizeF sz = g.MeasureString(series.Name, legFont);
                        currentLegendX += dotSize + 6 + (int)sz.Width + 16;
                    }
                }
            }

            // 5. Gridlines and Y-Axis
            int chartLeft = 40;
            int chartRight = Width - 20;
            int chartTop = 85;
            int chartBottom = Height - 35;
            int plotH = chartBottom - chartTop;
            int plotW = chartRight - chartLeft;

            int ySteps = 4;
            using (Font axisFont = FontHelper.CreateFont(8F, FontStyle.Regular))
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

            // 6. X-Axis Labels
            using (Font xFont = FontHelper.CreateFont(8F, FontStyle.Regular))
            using (SolidBrush xBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                // Only draw every 2nd or 3rd label if many items to prevent overlapping
                int stepInterval = _dataset.XLabels.Count > 10 ? 2 : 1;
                for (int i = 0; i < _dataset.XLabels.Count; i += stepInterval)
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
                using (Pen guidePen = new Pen(ThemeManager.BorderColor, 1.2f) { DashStyle = DashStyle.Dash })
                {
                    g.DrawLine(guidePen, hx, chartTop, hx, chartBottom);
                }
            }

            // 8. Draw Smooth Spline Curves with Soft Gradient Area Fills
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

                // Gradient area fill underneath curve
                if (points.Count >= 2 && plotH > 10)
                {
                    using (GraphicsPath areaPath = new GraphicsPath())
                    {
                        areaPath.AddCurve(points.ToArray(), 0.45f);
                        areaPath.AddLine(points.Last(), new PointF(points.Last().X, chartBottom));
                        areaPath.AddLine(new PointF(points.Last().X, chartBottom), new PointF(points.First().X, chartBottom));
                        areaPath.AddLine(new PointF(points.First().X, chartBottom), points.First());
                        areaPath.CloseFigure();

                        Rectangle areaBounds = new Rectangle(chartLeft, chartTop, plotW, plotH);
                        if (areaBounds.Width > 0 && areaBounds.Height > 0)
                        {
                            Color topColor = Color.FromArgb(38, series.LineColor);
                            Color bottomColor = Color.FromArgb(0, series.LineColor);
                            using (LinearGradientBrush areaBrush = new LinearGradientBrush(areaBounds, topColor, bottomColor, LinearGradientMode.Vertical))
                            {
                                g.FillPath(areaBrush, areaPath);
                            }
                        }
                    }
                }

                // Solid Anti-Aliased Spline Curve
                using (Pen curvePen = new Pen(series.LineColor, 2.2f)
                {
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    g.DrawCurve(curvePen, points.ToArray(), 0.45f);
                }

                // Highlight dot on hover
                if (_hoveredPointIndex >= 0 && _hoveredPointIndex < points.Count)
                {
                    var pt = points[_hoveredPointIndex];
                    using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(60, series.LineColor)))
                    {
                        g.FillEllipse(glowBrush, pt.X - 8, pt.Y - 8, 16, 16);
                    }
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
