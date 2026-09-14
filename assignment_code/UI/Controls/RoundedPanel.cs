using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace assignment_code.UI.Controls
{
    public class RoundedPanel : Panel
    {
        private int _borderRadius = 16;
        private Color _borderColor = Color.FromArgb(235, 240, 247);
        private int _borderWidth = 1;
        private Color _fillColor = Color.White;
        private bool _useThemeColors = true;

        [Category("Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public int BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(12);

            ThemeManager.ThemeChanged += (s, e) =>
            {
                if (_useThemeColors)
                {
                    _fillColor = ThemeManager.CardBackground;
                    _borderColor = ThemeManager.BorderColor;
                    Invalidate();
                }
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Color currentFill = _useThemeColors ? ThemeManager.CardBackground : _fillColor;
            Color currentBorder = _useThemeColors ? ThemeManager.BorderColor : _borderColor;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(rect, _borderRadius))
            {
                using (SolidBrush brush = new SolidBrush(currentFill))
                {
                    g.FillPath(brush, path);
                }

                if (_borderWidth > 0)
                {
                    using (Pen pen = new Pen(currentBorder, _borderWidth))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        g.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}
