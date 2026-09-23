using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI.Controls
{
    public class PosCategoryPillButton : Control
    {
        private string _categoryKey = "All";
        private string _categoryTitle = "All";
        private int _productCount = 0;
        private Color _categoryColor = Color.FromArgb(59, 130, 246);
        private bool _isSelected = false;
        private bool _isHovered = false;
        private bool _isPressed = false;

        public string CategoryKey
        {
            get => _categoryKey;
            set { _categoryKey = value; Invalidate(); }
        }

        public string CategoryTitle
        {
            get => _categoryTitle;
            set { _categoryTitle = value; UpdateSize(); Invalidate(); }
        }

        public int ProductCount
        {
            get => _productCount;
            set { _productCount = value; UpdateSize(); Invalidate(); }
        }

        public Color CategoryColor
        {
            get => _categoryColor;
            set { _categoryColor = value; Invalidate(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Invalidate(); }
        }

        public PosCategoryPillButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Height = 36;
            UpdateSize();
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void UpdateSize()
        {
            try
            {
                using (Graphics g = CreateGraphics())
                {
                    GraphicsHelper.SetHighQuality(g);
                    using (Font font = FontHelper.CreateFontForText(_categoryTitle ?? "", 9.25F, FontStyle.Bold))
                    using (Font badgeFont = FontHelper.CreateFont(8F, FontStyle.Bold))
                    {
                        SizeF textSize = g.MeasureString(_categoryTitle ?? "", font);
                        SizeF badgeSize = g.MeasureString(_productCount.ToString(), badgeFont);

                        int dotWidth = 8;
                        int dotMarginRight = 8;
                        int textMarginRight = 8;
                        int badgeInnerPadding = 12;
                        int badgeW = Math.Max(18, (int)Math.Ceiling(badgeSize.Width) + badgeInnerPadding);
                        int leftPadding = 14;
                        int rightPadding = 12;

                        int totalW = leftPadding + dotWidth + dotMarginRight + (int)Math.Ceiling(textSize.Width) + textMarginRight + badgeW + rightPadding;
                        Size = new Size(Math.Max(76, totalW), 36);
                    }
                }
            }
            catch
            {
                Size = new Size(110, 36);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            int radius = 17; // Perfect capsule shape for Height = 36

            using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(rect, radius))
            {
                if (_isSelected)
                {
                    // Active Pill: Vibrant primary accent gradient
                    Color topCol = _isPressed ? Color.FromArgb(29, 78, 216) : Color.FromArgb(37, 99, 235);
                    Color botCol = _isPressed ? Color.FromArgb(30, 64, 175) : Color.FromArgb(29, 78, 216);
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, topCol, botCol, 90f))
                    {
                        g.FillPath(lgb, path);
                    }

                    using (Pen bp = new Pen(Color.FromArgb(29, 78, 216), 1.5f))
                    {
                        bp.Alignment = PenAlignment.Inset;
                        g.DrawPath(bp, path);
                    }
                }
                else
                {
                    // Inactive Pill: Card background with subtle hover
                    Color bgCol = _isHovered
                        ? (ThemeManager.IsDark ? Color.FromArgb(38, 48, 72) : Color.FromArgb(241, 245, 249))
                        : ThemeManager.CardBackground;

                    using (SolidBrush sbg = new SolidBrush(bgCol))
                    {
                        g.FillPath(sbg, path);
                    }

                    Color borderCol = _isHovered ? ThemeManager.AccentBlue : ThemeManager.BorderColor;
                    using (Pen bp = new Pen(borderCol, 1.2f))
                    {
                        bp.Alignment = PenAlignment.Inset;
                        g.DrawPath(bp, path);
                    }
                }
            }

            // 1. Category Indicator Dot
            int dotSize = 8;
            int dotX = 14;
            int dotY = (Height - dotSize) / 2;
            Rectangle dotRect = new Rectangle(dotX, dotY, dotSize, dotSize);

            Color dotColor = _isSelected ? Color.White : _categoryColor;
            using (SolidBrush dotBrush = new SolidBrush(dotColor))
            {
                g.FillEllipse(dotBrush, dotRect);
            }

            // 2. Category Title Text
            int textX = dotX + dotSize + 8;
            string title = _categoryTitle ?? "";
            using (Font titleFont = FontHelper.CreateFontForText(title, 9.25F, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(title, titleFont);
                int textW = (int)Math.Ceiling(textSize.Width) + 4;
                Rectangle textRect = new Rectangle(textX, 0, textW, Height);

                Color textColor = _isSelected ? Color.White : ThemeManager.TextPrimary;
                using (SolidBrush textBrush = new SolidBrush(textColor))
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                })
                {
                    g.DrawString(title, titleFont, textBrush, textRect, sf);
                }

                // 3. Count Badge Bubble
                int badgeX = textX + textW + 4;
                string countStr = _productCount.ToString();
                using (Font badgeFont = FontHelper.CreateFont(8F, FontStyle.Bold))
                {
                    SizeF badgeTextSize = g.MeasureString(countStr, badgeFont);
                    int badgeInnerPad = 12;
                    int badgeW = Math.Max(18, (int)Math.Ceiling(badgeTextSize.Width) + badgeInnerPad);
                    int badgeH = 20;
                    int badgeY = (Height - badgeH) / 2;
                    Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);

                    using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(badgeRect, 9))
                    {
                        Color badgeBg = _isSelected
                            ? Color.FromArgb(50, 255, 255, 255)
                            : (ThemeManager.IsDark ? Color.FromArgb(42, 53, 78) : Color.FromArgb(233, 238, 246));

                        using (SolidBrush bgb = new SolidBrush(badgeBg))
                        {
                            g.FillPath(bgb, bp);
                        }
                    }

                    Color badgeFg = _isSelected ? Color.White : ThemeManager.TextSecondary;
                    using (SolidBrush bfb = new SolidBrush(badgeFg))
                    using (StringFormat bsf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                    {
                        g.DrawString(countStr, badgeFont, bfb, badgeRect, bsf);
                    }
                }
            }
        }
    }
}
