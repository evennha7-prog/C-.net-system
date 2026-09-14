using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace assignment_code.UI.Controls
{
    public class NavItemButton : Control
    {
        private string _iconName = "dashboard";
        private bool _isActive = false;
        private bool _isHovered = false;
        private string _badgeText = null;
        private bool _showChevron = false;
        private bool _isExpanded = false;
        private bool _isSubItem = false;
        private int _borderRadius = 18;
        private string _translationKey = null;

        [Category("Appearance")]
        public string TranslationKey
        {
            get => _translationKey;
            set { _translationKey = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string IconName
        {
            get => _iconName;
            set { _iconName = value; Invalidate(); }
        }

        [Category("Appearance")]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value; Invalidate(); }
        }

        [Category("Appearance")]
        public bool ShowChevron
        {
            get => _showChevron;
            set { _showChevron = value; Invalidate(); }
        }

        [Category("Appearance")]
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; Invalidate(); }
        }

        private bool _isDangerAction = false;

        [Category("Appearance")]
        public bool IsDangerAction
        {
            get => _isDangerAction;
            set { _isDangerAction = value; Invalidate(); }
        }

        [Category("Appearance")]
        public bool IsSubItem
        {
            get => _isSubItem;
            set { _isSubItem = value; Invalidate(); }
        }

        public NavItemButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(200, 42);
            Font = FontHelper.CreateFont(9.5F, FontStyle.Regular);
            Cursor = Cursors.Hand;

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
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
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            int pillRadius = _isSubItem ? 12 : _borderRadius;

            // Draw Background Pill
            if (_isActive)
            {
                Color activeBg = _isDangerAction ? ThemeManager.DangerRed : ThemeManager.AccentBlue;
                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(rect, pillRadius))
                using (SolidBrush brush = new SolidBrush(activeBg))
                {
                    g.FillPath(brush, path);
                }
            }
            else if (_isHovered)
            {
                Color hoverBg = _isDangerAction 
                    ? (ThemeManager.IsDark ? Color.FromArgb(58, 24, 30) : Color.FromArgb(254, 242, 242)) 
                    : ThemeManager.HoverBackground;
                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(rect, pillRadius))
                using (SolidBrush brush = new SolidBrush(hoverBg))
                {
                    g.FillPath(brush, path);
                }
            }

            // Icon Color & Text Color
            Color iconColor;
            Color textColor;

            if (_isActive)
            {
                iconColor = Color.White;
                textColor = Color.White;
            }
            else if (_isDangerAction)
            {
                iconColor = ThemeManager.DangerRed;
                textColor = ThemeManager.DangerRed;
            }
            else if (_isHovered)
            {
                iconColor = ThemeManager.TextPrimary;
                textColor = ThemeManager.TextPrimary;
            }
            else
            {
                iconColor = ThemeManager.TextSecondary;
                textColor = ThemeManager.TextSecondary;
            }

            // Positioning based on IsSubItem
            int startX = _isSubItem ? 28 : 14;
            int textStartX = _isSubItem ? 52 : 44;
            int iconSize = _isSubItem ? 14 : 18;

            // Draw Icon
            Rectangle iconRect = new Rectangle(startX, (Height - iconSize) / 2, iconSize, iconSize);
            GraphicsHelper.DrawIcon(g, _iconName, iconRect, iconColor, _isSubItem ? 1.5f : 1.8f);

            // Draw Label
            float fontSize = _isSubItem ? 9F : 9.5F;
            FontStyle labelStyle = _isSubItem ? (_isActive ? FontStyle.Bold : FontStyle.Regular) : FontStyle.Bold;
            using (Font itemFont = FontHelper.CreateFontForText(Text, fontSize, labelStyle))
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(Text, itemFont, textBrush, textStartX, (Height - itemFont.Height) / 2);
            }

            // Draw Right Chevron if enabled
            if (_showChevron)
            {
                int chevSize = 14;
                Rectangle chevRect = new Rectangle(Width - 26, (Height - chevSize) / 2, chevSize, chevSize);
                string chevIcon = _isExpanded ? "chevronup" : "chevrondown";
                GraphicsHelper.DrawIcon(g, chevIcon, chevRect, ThemeManager.TextMuted, 1.6f);
            }

            // Draw Badge if set (e.g. "7")
            if (!string.IsNullOrEmpty(_badgeText))
            {
                int badgeSize = 18;
                Rectangle badgeRect = new Rectangle(Width - 30, (Height - badgeSize) / 2, badgeSize, badgeSize);

                using (SolidBrush bBrush = new SolidBrush(ThemeManager.DangerRed))
                {
                    g.FillEllipse(bBrush, badgeRect);
                }

                using (Font bFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
                using (SolidBrush bTextBrush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(_badgeText, bFont, bTextBrush, badgeRect, sf);
                }
            }
        }
    }
}
