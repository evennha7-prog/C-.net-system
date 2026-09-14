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
        private int _borderRadius = 14;

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
            Size = new Size(180, 140);
            Font = FontHelper.CreateFont(9F, FontStyle.Regular);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Bind(KpiStat stat)
        {
            _data = stat;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Background & Border
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

            if (_data == null) return;

            int padding = 14;

            // 2. Icon Box (top-left)
            int iconBoxSize = 28;
            Rectangle iconBoxRect = new Rectangle(padding, padding, iconBoxSize, iconBoxSize);
            using (GraphicsPath iconPath = GraphicsHelper.GetRoundedRectanglePath(iconBoxRect, 7))
            {
                using (SolidBrush iconBgBrush = new SolidBrush(ThemeManager.AccentBlueLight))
                {
                    g.FillPath(iconBgBrush, iconPath);
                }
            }

            string iconName = "document";
            switch (_data.IconType)
            {
                case KpiIconType.Document:
                case KpiIconType.Product: iconName = "products"; break;
                case KpiIconType.Folder: iconName = "folder"; break;
                case KpiIconType.Media:
                case KpiIconType.Order: iconName = "order"; break;
                case KpiIconType.Customer: iconName = "customer"; break;
                case KpiIconType.Report:
                case KpiIconType.Comments: iconName = "report"; break;
            }

            Rectangle iconInnerRect = new Rectangle(iconBoxRect.X + 6, iconBoxRect.Y + 6, iconBoxSize - 12, iconBoxSize - 12);
            GraphicsHelper.DrawIcon(g, iconName, iconInnerRect, ThemeManager.AccentBlue, 1.8f);

            // 3. Title (e.g. "Total Posts" / "ទំនិញសរុប")
            int contentY = iconBoxRect.Bottom + 8;
            using (Font titleFont = FontHelper.CreateFontForText(_data.Title, 9F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString(_data.Title, titleFont, titleBrush, padding, contentY);
            }

            // 4. Value Number (e.g. "560")
            contentY += 18;
            using (Font valFont = FontHelper.CreateFontForText(_data.Value, 15F, FontStyle.Bold))
            using (SolidBrush valBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_data.Value, valFont, valBrush, padding - 1, contentY);
            }

            // 5. Bottom Line: Subtitle ("Last 30 days") + Trend Badge ("↑ 40.35%")
            int bottomY = Height - padding - 16;

            using (Font subFont = FontHelper.CreateFontForText(_data.Subtitle ?? "Last 30 days", 8F, FontStyle.Regular))
            using (SolidBrush subBrush = new SolidBrush(ThemeManager.TextMuted))
            {
                g.DrawString(_data.Subtitle ?? "Last 30 days", subFont, subBrush, padding, bottomY + 2);
            }

            // Trend Badge
            string arrow = _data.IsPositive ? "↑ " : "↓ ";
            string badgeText = arrow + _data.PercentageChange.ToString("0.00") + "%";

            using (Font badgeFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
            {
                SizeF badgeTextSize = g.MeasureString(badgeText, badgeFont);
                int badgeW = (int)badgeTextSize.Width + 10;
                int badgeH = 18;
                int badgeX = Width - padding - badgeW;
                int badgeY = bottomY;

                Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);

                Color badgeBg = _data.IsPositive ? ThemeManager.SuccessGreenBg : ThemeManager.DangerRedBg;
                Color badgeFg = _data.IsPositive ? ThemeManager.SuccessGreen : ThemeManager.DangerRed;

                using (GraphicsPath badgePath = GraphicsHelper.GetRoundedRectanglePath(badgeRect, 5))
                {
                    using (SolidBrush badgeBgBrush = new SolidBrush(badgeBg))
                    {
                        g.FillPath(badgeBgBrush, badgePath);
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
