using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;

namespace assignment_code.UI.Controls
{
    public class LatestPostsCard : Control
    {
        private List<PostItem> _posts = new List<PostItem>();
        private int _borderRadius = 16;
        private int _hoveredRow = -1;

        public event EventHandler<PostItem> PostActionClicked;

        public LatestPostsCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(380, 240);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Bind(List<PostItem> posts)
        {
            _posts = posts ?? new List<PostItem>();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int headerH = 80;
            int rowH = 36;

            int prev = _hoveredRow;
            _hoveredRow = -1;

            if (e.Y >= headerH)
            {
                int idx = (e.Y - headerH) / rowH;
                if (idx >= 0 && idx < _posts.Count)
                {
                    _hoveredRow = idx;
                }
            }

            if (prev != _hoveredRow)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredRow = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_hoveredRow >= 0 && _hoveredRow < _posts.Count)
            {
                PostActionClicked?.Invoke(this, _posts[_hoveredRow]);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Container Background & Border
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

            // 2. Title ("Latest Products")
            string titleText = TranslationManager.T("LatestProducts", "Latest Products");
            using (Font titleFont = FontHelper.CreateFontForText(titleText, 12F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(titleText, titleFont, titleBrush, padding, padding);
            }

            // 3. Table Column Headers
            int colHeaderY = 54;
            int col1X = padding;
            int col2X = (int)(Width * 0.55f);
            int col3X = (int)(Width * 0.78f);
            int col4X = Width - padding - 20;

            string hPrd = TranslationManager.T("Product", "Product");
            string hSts = TranslationManager.T("Status", "Status");
            string hDat = TranslationManager.T("Date", "Date");

            using (Font headFont = FontHelper.CreateFont(9F, FontStyle.Bold))
            using (SolidBrush headBrush = new SolidBrush(ThemeManager.TextMuted))
            {
                g.DrawString(hPrd, headFont, headBrush, col1X, colHeaderY);
                g.DrawString(hSts, headFont, headBrush, col2X, colHeaderY);
                g.DrawString(hDat, headFont, headBrush, col3X, colHeaderY);
            }

            // 4. Data Rows
            int startY = 82;
            int rowHeight = 36;

            using (Font bodyFont = FontHelper.CreateFont(9F, FontStyle.Regular))
            using (Font boldFont = FontHelper.CreateFont(9F, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextPrimary))
            using (SolidBrush dateBrush = new SolidBrush(ThemeManager.TextSecondary))
            using (Pen divPen = new Pen(ThemeManager.SubtleDivider, 1f))
            {
                for (int i = 0; i < _posts.Count; i++)
                {
                    var post = _posts[i];
                    int rowY = startY + (i * rowHeight);

                    // Row hover background
                    if (i == _hoveredRow)
                    {
                        Rectangle hoverRect = new Rectangle(padding - 6, rowY - 4, Width - (padding * 2) + 12, rowHeight);
                        using (GraphicsPath hPath = GraphicsHelper.GetRoundedRectanglePath(hoverRect, 6))
                        using (SolidBrush hBrush = new SolidBrush(ThemeManager.HoverBackground))
                        {
                            g.FillPath(hBrush, hPath);
                        }
                    }

                    // Divider line
                    if (i > 0)
                    {
                        g.DrawLine(divPen, padding, rowY - 4, Width - padding, rowY - 4);
                    }

                    // Column 1: Title (Bold)
                    g.DrawString(post.Title, boldFont, textBrush, col1X, rowY);

                    // Column 2: Status Pill/Text
                    bool isPublished = post.Status == PostStatus.Published;
                    Color statusColor = isPublished ? ThemeManager.SuccessGreen : ThemeManager.WarningYellow;
                    using (SolidBrush statusBrush = new SolidBrush(statusColor))
                    {
                        g.DrawString(post.Status.ToString(), boldFont, statusBrush, col2X, rowY);
                    }

                    // Column 3: Date
                    g.DrawString(post.Date, bodyFont, dateBrush, col3X, rowY);

                    // Column 4: Dots action `•••`
                    Rectangle dotsRect = new Rectangle(col4X, rowY + 3, 16, 12);
                    GraphicsHelper.DrawIcon(g, "dots", dotsRect, ThemeManager.TextMuted, 1.5f);
                }
            }
        }
    }
}
