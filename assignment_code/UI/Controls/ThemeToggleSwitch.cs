using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace assignment_code.UI.Controls
{
    public class ThemeToggleSwitch : Control
    {
        private bool _isDark = false;
        private int _borderRadius = 16;

        public event EventHandler ThemeToggled;

        [Category("Appearance")]
        public bool IsDark
        {
            get => _isDark;
            set
            {
                if (_isDark != value)
                {
                    _isDark = value;
                    ThemeManager.SetTheme(_isDark ? AppThemeMode.Dark : AppThemeMode.Light);
                    ThemeToggled?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public ThemeToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(180, 36);
            Cursor = Cursors.Hand;

            ThemeManager.ThemeChanged += (s, e) =>
            {
                _isDark = ThemeManager.IsDark;
                Invalidate();
            };
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.X < Width / 2)
            {
                IsDark = false;
            }
            else
            {
                IsDark = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle fullRect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (fullRect.Width <= 0 || fullRect.Height <= 0) return;

            // Pill Container Background
            Color containerBg = ThemeManager.IsDark ? Color.FromArgb(20, 26, 42) : Color.FromArgb(240, 244, 250);
            using (GraphicsPath containerPath = GraphicsHelper.GetRoundedRectanglePath(fullRect, _borderRadius))
            using (SolidBrush containerBrush = new SolidBrush(containerBg))
            {
                g.FillPath(containerBrush, containerPath);
            }

            int halfW = (Width - 4) / 2;
            Rectangle leftPill = new Rectangle(2, 2, halfW, Height - 5);
            Rectangle rightPill = new Rectangle(Width / 2, 2, halfW, Height - 5);

            // Draw Active Selected Pill
            Rectangle activePill = _isDark ? rightPill : leftPill;
            using (GraphicsPath activePath = GraphicsHelper.GetRoundedRectanglePath(activePill, _borderRadius - 2))
            using (SolidBrush activeBrush = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(40, 52, 82) : Color.White))
            {
                g.FillPath(activeBrush, activePath);
            }

            // Draw "Light" button
            Color lightFg = !_isDark ? ThemeManager.TextPrimary : ThemeManager.TextMuted;
            Rectangle sunRect = new Rectangle(leftPill.X + 8, leftPill.Y + (leftPill.Height - 14) / 2, 14, 14);
            GraphicsHelper.DrawIcon(g, "sun", sunRect, lightFg, 1.4f);

            using (Font font = FontHelper.CreateFont(8.5F, !_isDark ? FontStyle.Bold : FontStyle.Regular))
            using (SolidBrush textBrush = new SolidBrush(lightFg))
            {
                g.DrawString("Light", font, textBrush, sunRect.Right + 6, leftPill.Y + (leftPill.Height - font.Height) / 2);
            }

            // Draw "Dark" button
            Color darkFg = _isDark ? ThemeManager.TextPrimary : ThemeManager.TextMuted;
            Rectangle moonRect = new Rectangle(rightPill.X + 8, rightPill.Y + (rightPill.Height - 14) / 2, 14, 14);
            GraphicsHelper.DrawIcon(g, "moon", moonRect, darkFg, 1.4f);

            using (Font font = FontHelper.CreateFont(8.5F, _isDark ? FontStyle.Bold : FontStyle.Regular))
            using (SolidBrush textBrush = new SolidBrush(darkFg))
            {
                g.DrawString("Dark", font, textBrush, moonRect.Right + 6, rightPill.Y + (rightPill.Height - font.Height) / 2);
            }
        }
    }
}
