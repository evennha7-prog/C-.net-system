using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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
        private double _maxAxisValue = 20;

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
                     ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.Background;
            Size = new Size(380, 260);

            ThemeManager.ThemeChanged += (s, e) =>
            {
                BackColor = ThemeManager.Background;
                Invalidate();
            };
        }

        public void Bind(List<BarChartPoint> data, string period = "6 months")
        {
            _data = data ?? new List<BarChartPoint>();
            _selectedPeriod = period;

            // Dynamically scale max axis value for optimal visual proportions
            if (_data.Count > 0)
            {
                double maxVal = _data.Max(p => p.Value);
                if (maxVal <= 5) _maxAxisValue = 10;
                else if (maxVal <= 10) _maxAxisValue = 15;
                else if (maxVal <= 25) _maxAxisValue = 30;
                else if (maxVal <= 50) _maxAxisValue = 60;
                else _maxAxisValue = Math.Ceiling(maxVal * 1.25 / 10.0) * 10;
            }
            else
            {
                _maxAxisValue = 20;
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int previousHover = _hoveredIndex;
            _hoveredIndex = -1;

            if (_periodButtonRect.Contains(e.Location))
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            int paddingLeft = 45;
            int paddingRight = 20;
            int chartTop = 100;
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

            // 0. Paint parent background first to eliminate white corner artifacts
            Color parentBg = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : ThemeManager.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Card Container
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

            // 2. Header: Title
            using (Font titleFont = FontHelper.CreateFontForText(_title, 11F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_title, titleFont, titleBrush, padding, padding);
            }

            // 3. Period Dropdown Pill ("6 months ▾")
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

            // 4. Legend: Indigo dot + "Total number of products"
            int legendY = padding + 28;
            int dotSize = 8;
            Rectangle dotRect = new Rectangle(padding, legendY + 4, dotSize, dotSize);
            using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(99, 102, 241)))
            {
                g.FillEllipse(dotBrush, dotRect);
            }

            string legendText = TranslationManager.T("Total number of products", "Total number of products");
            using (Font legendFont = FontHelper.CreateFontForText(legendText, 8.5F, FontStyle.Regular))
            using (SolidBrush legendBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString(legendText, legendFont, legendBrush, padding + dotSize + 6, legendY);
            }

            // 5. Chart Grid & Plot Area
            int chartLeft = 45;
            int chartRight = Width - 20;
            int chartTop = 85;
            int chartBottom = Height - 35;
            int plotH = chartBottom - chartTop;
            int plotW = chartRight - chartLeft;

            // Y-Axis labels and horizontal dashed lines
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

            if (_data == null || _data.Count == 0) return;

            // 6. Draw Gradient Capsule Bars
            float slotWidth = (float)plotW / _data.Count;
            float barWidth = Math.Min(32f, slotWidth * 0.52f);

            int tooltipIndexToDraw = -1;
            PointF tooltipPoint = PointF.Empty;
            string tooltipString = "";

            using (Font xFont = FontHelper.CreateFont(8.25F, FontStyle.Regular))
            using (SolidBrush xBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    var pt = _data[i];
                    float centerX = chartLeft + i * slotWidth + slotWidth / 2f;
                    float barHeight = (float)(pt.Value / _maxAxisValue * plotH);
                    barHeight = Math.Max(4f, barHeight); // minimum height so bars are always visible
                    float barX = centerX - barWidth / 2f;
                    float barY = chartBottom - barHeight;

                    bool isHighlighted = pt.IsHighlighted || (i == _hoveredIndex);

                    RectangleF barRect = new RectangleF(barX, barY, barWidth, barHeight);
                    Rectangle intRect = Rectangle.Round(barRect);

                    if (isHighlighted)
                    {
                        Color topCol = Color.FromArgb(99, 102, 241);
                        Color botCol = Color.FromArgb(59, 130, 246);
                        using (LinearGradientBrush barBrush = new LinearGradientBrush(intRect, topCol, botCol, LinearGradientMode.Vertical))
                        using (GraphicsPath barPath = GraphicsHelper.GetRoundedRectanglePath(intRect, 6))
                        {
                            g.FillPath(barBrush, barPath);
                        }
                    }
                    else
                    {
                        Color barCol = ThemeManager.IsDark ? Color.FromArgb(42, 54, 88) : Color.FromArgb(218, 228, 247);
                        using (SolidBrush barBrush = new SolidBrush(barCol))
                        using (GraphicsPath barPath = GraphicsHelper.GetRoundedRectanglePath(intRect, 6))
                        {
                            g.FillPath(barBrush, barPath);
                        }
                    }

                    // X-Axis Label
                    SizeF xSize = g.MeasureString(pt.Label, xFont);
                    g.DrawString(pt.Label, xFont, xBrush, centerX - xSize.Width / 2f, chartBottom + 6);

                    // Record tooltip
                    if (isHighlighted && (!string.IsNullOrEmpty(pt.TooltipText) || i == _hoveredIndex))
                    {
                        tooltipIndexToDraw = i;
                        tooltipPoint = new PointF(centerX, barY);
                        tooltipString = !string.IsNullOrEmpty(pt.TooltipText) ? pt.TooltipText : $"{pt.Value} items";
                    }
                }
            }

            // 7. Draw Floating Tooltip
            if (tooltipIndexToDraw >= 0 && !string.IsNullOrEmpty(tooltipString))
            {
                using (Font ttFont = FontHelper.CreateFont(8F, FontStyle.Bold))
                {
                    SizeF ttSize = g.MeasureString(tooltipString, ttFont);
                    int ttW = (int)ttSize.Width + 18;
                    int ttH = 24;
                    int ttX = (int)(tooltipPoint.X - ttW / 2f);
                    int ttY = (int)(tooltipPoint.Y - ttH - 8);

                    Rectangle ttRect = new Rectangle(ttX, ttY, ttW, ttH);

                    using (GraphicsPath ttPath = GraphicsHelper.GetRoundedRectanglePath(ttRect, 6))
                    using (SolidBrush ttBg = new SolidBrush(ThemeManager.DarkTooltipBg))
                    {
                        g.FillPath(ttBg, ttPath);

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
    }
}
