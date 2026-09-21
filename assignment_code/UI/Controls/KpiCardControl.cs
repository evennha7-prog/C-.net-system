using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Models;

namespace assignment_code.UI.Controls
{
    public class KpiCardControl : Control
    {
        private KpiStat _data;
        private int _borderRadius = 16;
        private bool _isHovered = false;

        [Category("Data")]
        public KpiStat Data
        {
            get => _data;
            set { _data = value; Invalidate(); }
        }

        public KpiCardControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(220, 130);
            Font = FontHelper.CreateFont(9F, FontStyle.Regular);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Bind(KpiStat stat)
        {
            _data = stat;
            Invalidate();
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

            // 0. Paint parent background first to eliminate white corner artifacts
            Color parentBg = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : ThemeManager.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Background & Border
            using (GraphicsPath cardPath = GraphicsHelper.GetRoundedRectanglePath(cardBounds, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(ThemeManager.CardBackground))
                {
                    g.FillPath(bgBrush, cardPath);
                }

                Color borderCol = _isHovered 
                    ? ThemeManager.AccentBlue 
                    : ThemeManager.BorderColor;
                using (Pen borderPen = new Pen(borderCol, 1.2f))
                {
                    borderPen.Alignment = PenAlignment.Inset;
                    g.DrawPath(borderPen, cardPath);
                }
            }

            if (_data == null) return;

            int padding = 16;

            // 2. Determine Color Palette based on IconType
            Color iconBg;
            Color iconColor;
            string iconName = "products";

            switch (_data.IconType)
            {
                case KpiIconType.Document:
                case KpiIconType.Product:
                    iconName = "products";
                    iconBg = ThemeManager.IsDark ? Color.FromArgb(32, 43, 85) : Color.FromArgb(238, 242, 255);
                    iconColor = Color.FromArgb(99, 102, 241);
                    break;
                case KpiIconType.Folder:
                    iconName = "folder";
                    iconBg = ThemeManager.IsDark ? Color.FromArgb(20, 56, 47) : Color.FromArgb(236, 253, 245);
                    iconColor = Color.FromArgb(16, 185, 129);
                    break;
                case KpiIconType.Media:
                case KpiIconType.Order:
                    iconName = "order";
                    iconBg = ThemeManager.IsDark ? Color.FromArgb(56, 44, 24) : Color.FromArgb(254, 243, 199);
                    iconColor = Color.FromArgb(245, 158, 11);
                    break;
                case KpiIconType.Report:
                case KpiIconType.Comments:
                default:
                    iconName = "dollar";
                    iconBg = ThemeManager.IsDark ? Color.FromArgb(26, 52, 90) : Color.FromArgb(234, 240, 255);
                    iconColor = Color.FromArgb(59, 130, 246);
                    break;
            }

            // 3. Icon Squircle (top-left)
            int iconBoxSize = 34;
            Rectangle iconBoxRect = new Rectangle(padding, padding, iconBoxSize, iconBoxSize);
            using (GraphicsPath iconPath = GraphicsHelper.GetRoundedRectanglePath(iconBoxRect, 9))
            using (SolidBrush iconBgBrush = new SolidBrush(iconBg))
            {
                g.FillPath(iconBgBrush, iconPath);
            }

            Rectangle iconInnerRect = new Rectangle(iconBoxRect.X + 8, iconBoxRect.Y + 8, iconBoxSize - 16, iconBoxSize - 16);
            GraphicsHelper.DrawIcon(g, iconName, iconInnerRect, iconColor, 1.8f);

            // 4. Title next to or below icon
            int titleX = iconBoxRect.Right + 10;
            int titleY = padding + 2;
            using (Font titleFont = FontHelper.CreateFontForText(_data.Title, 8.75F, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString(_data.Title, titleFont, titleBrush, titleX, titleY);
            }

            // 5. Value Number (bold prominent number)
            using (Font valFont = FontHelper.CreateFontForText(_data.Value, 16F, FontStyle.Bold))
            using (SolidBrush valBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_data.Value, valFont, valBrush, titleX - 1, titleY + 16);
            }

            // 6. Bottom Row: Subtitle + Trend Badge
            int bottomY = Height - padding - 18;

            using (Font subFont = FontHelper.CreateFontForText(_data.Subtitle ?? "", 8F, FontStyle.Regular))
            using (SolidBrush subBrush = new SolidBrush(ThemeManager.TextMuted))
            {
                g.DrawString(_data.Subtitle ?? "", subFont, subBrush, padding, bottomY + 2);
            }

            // 7. Pill Trend Badge
            string arrow = _data.IsPositive ? "↑ " : "↓ ";
            string badgeText = arrow + _data.PercentageChange.ToString("0.0") + "%";

            using (Font badgeFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
            {
                SizeF badgeTextSize = g.MeasureString(badgeText, badgeFont);
                int badgeW = (int)badgeTextSize.Width + 14;
                int badgeH = 20;
                int badgeX = Width - padding - badgeW;
                int badgeY = bottomY;

                Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);

                Color badgeBg = _data.IsPositive ? ThemeManager.SuccessGreenBg : ThemeManager.DangerRedBg;
                Color badgeFg = _data.IsPositive ? ThemeManager.SuccessGreen : ThemeManager.DangerRed;

                using (GraphicsPath badgePath = GraphicsHelper.GetRoundedRectanglePath(badgeRect, 10))
                {
                    using (SolidBrush badgeBgBrush = new SolidBrush(badgeBg))
                    {
                        g.FillPath(badgeBgBrush, badgePath);
                    }
                    using (Pen badgeBorder = new Pen(Color.FromArgb(40, badgeFg), 1f))
                    {
                        g.DrawPath(badgeBorder, badgePath);
                    }
                }

                using (SolidBrush badgeFgBrush = new SolidBrush(badgeFg))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(badgeText, badgeFont, badgeFgBrush, badgeRect, sf);
                }
            }
        }
    }
}
