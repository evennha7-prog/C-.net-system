using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI.Controls
{
    public class ManagementStatCard : Control
    {
        private string _title = "Metric";
        private string _value = "0";
        private string _badgeText = "";
        private string _iconName = "products";
        private Color _accentColor = Color.FromArgb(59, 130, 246); // Blue
        private int _borderRadius = 12;

        [Category("Appearance")]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string IconName
        {
            get => _iconName;
            set { _iconName = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public ManagementStatCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(180, 78);
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void SetData(string title, string value, string badgeText, string iconName, Color accent)
        {
            _title = title;
            _value = value;
            _badgeText = badgeText;
            _iconName = iconName;
            _accentColor = accent;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle cardRect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardRect.Width <= 0 || cardRect.Height <= 0) return;

            // 0. Paint parent background first
            Color parentBg = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : ThemeManager.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            // 1. Background & Border
            using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(cardRect, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(ThemeManager.CardBackground))
                {
                    g.FillPath(bgBrush, path);
                }

                using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1.0f))
                {
                    borderPen.Alignment = PenAlignment.Inset;
                    g.DrawPath(borderPen, path);
                }
            }

            // 2. Icon Squircle
            int iconBoxSize = 42;
            int iconBoxX = 14;
            int iconBoxY = (Height - iconBoxSize) / 2;
            Rectangle iconBoxRect = new Rectangle(iconBoxX, iconBoxY, iconBoxSize, iconBoxSize);

            Color iconBgColor = ThemeManager.IsDark
                ? Color.FromArgb(40, _accentColor.R, _accentColor.G, _accentColor.B)
                : Color.FromArgb(24, _accentColor.R, _accentColor.G, _accentColor.B);

            using (GraphicsPath iconBoxPath = GraphicsHelper.GetRoundedRectanglePath(iconBoxRect, 10))
            {
                using (SolidBrush ibBrush = new SolidBrush(iconBgColor))
                {
                    g.FillPath(ibBrush, iconBoxPath);
                }
            }

            // Vector Icon inside squircle
            int iconSize = 20;
            Rectangle iconRect = new Rectangle(
                iconBoxX + (iconBoxSize - iconSize) / 2,
                iconBoxY + (iconBoxSize - iconSize) / 2,
                iconSize,
                iconSize
            );
            GraphicsHelper.DrawIcon(g, _iconName, iconRect, _accentColor, 2.0f);

            // 3. Text layout (Value + Title)
            int textX = iconBoxX + iconBoxSize + 12;
            int textAvailableWidth = Width - textX - 12;

            // Title (Muted smaller label)
            using (Font titleFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString(_title, titleFont, titleBrush, textX, 15);
            }

            // Value (Bold prominent number)
            using (Font valFont = FontHelper.CreateFont(13F, FontStyle.Bold))
            using (SolidBrush valBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_value, valFont, valBrush, textX, 33);
            }

            // 4. Optional Badge Pill (Top Right or Bottom Right)
            if (!string.IsNullOrEmpty(_badgeText))
            {
                using (Font badgeFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
                {
                    SizeF bSize = g.MeasureString(_badgeText, badgeFont);
                    int bW = (int)bSize.Width + 12;
                    int bH = 18;
                    int bX = Width - bW - 12;
                    int bY = 14;

                    Rectangle bRect = new Rectangle(bX, bY, bW, bH);
                    using (GraphicsPath bPath = GraphicsHelper.GetRoundedRectanglePath(bRect, 9))
                    using (SolidBrush bBg = new SolidBrush(iconBgColor))
                    using (SolidBrush bFg = new SolidBrush(_accentColor))
                    {
                        g.FillPath(bBg, bPath);
                        g.DrawString(_badgeText, badgeFont, bFg, bX + 6, bY + 2);
                    }
                }
            }
        }
    }
}
