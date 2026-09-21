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
            int headerH = 76;
            int rowH = 36;

            int prev = _hoveredRow;
            _hoveredRow = -1;

            if (e.Y >= headerH)
            {
                int idx = (e.Y - headerH) / rowH;
                if (idx >= 0 && idx < _posts.Count)
                {
                    _hoveredRow = idx;
                    Cursor = Cursors.Hand;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (prev != _hoveredRow)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredRow = -1;
            Cursor = Cursors.Default;
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

            // 0. Paint parent background first to eliminate white corner artifacts
            Color parentBg = (Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : ThemeManager.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Container Background & Border
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

            // 2. Title ("Latest Products")
            string titleText = TranslationManager.T("LatestProducts", "Latest Products");
            using (Font titleFont = FontHelper.CreateFontForText(titleText, 11F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(titleText, titleFont, titleBrush, padding, padding);
            }

            // 3. Table Column Headers
            int colHeaderY = 50;
            int col1X = padding;
            int col2X = (int)(Width * 0.54f);
            int col3X = (int)(Width * 0.78f);

            string hPrd = TranslationManager.T("Product", "Product");
            string hSts = TranslationManager.T("Status", "Status");
            string hDat = TranslationManager.T("Date", "Date");

            using (Font headFont = FontHelper.CreateFont(8.5F, FontStyle.Bold))
            using (SolidBrush headBrush = new SolidBrush(ThemeManager.TextMuted))
            {
                g.DrawString(hPrd, headFont, headBrush, col1X, colHeaderY);
                g.DrawString(hSts, headFont, headBrush, col2X, colHeaderY);
                g.DrawString(hDat, headFont, headBrush, col3X, colHeaderY);
            }

            // 4. Data Rows or Empty State
            if (_posts.Count == 0)
            {
                int emptyY = 110;
                string emptyText = TranslationManager.T("NoProductsFound", "No products available in inventory yet.");
                using (Font emptyFont = FontHelper.CreateFont(9F, FontStyle.Regular))
                using (SolidBrush emptyBrush = new SolidBrush(ThemeManager.TextMuted))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(emptyText, emptyFont, emptyBrush, Width / 2f, emptyY, sf);
                }
                return;
            }

            int startY = 76;
            int rowHeight = 36;

            using (Font bodyFont = FontHelper.CreateFont(8.75F, FontStyle.Regular))
            using (Font boldFont = FontHelper.CreateFont(8.75F, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextPrimary))
            using (SolidBrush dateBrush = new SolidBrush(ThemeManager.TextSecondary))
            using (Pen divPen = new Pen(ThemeManager.SubtleDivider, 1f))
            {
                int maxRows = Math.Min(4, _posts.Count);
                for (int i = 0; i < maxRows; i++)
                {
                    var post = _posts[i];
                    int rowY = startY + (i * rowHeight);

                    // Row hover background
                    if (i == _hoveredRow)
                    {
                        Rectangle hRect = new Rectangle(padding - 6, rowY - 2, Width - (padding * 2) + 12, rowHeight);
                        using (GraphicsPath hPath = GraphicsHelper.GetRoundedRectanglePath(hRect, 6))
                        using (SolidBrush hBrush = new SolidBrush(ThemeManager.HoverBackground))
                        {
                            g.FillPath(hBrush, hPath);
                        }
                    }

                    // Divider line
                    if (i > 0)
                    {
                        g.DrawLine(divPen, padding, rowY - 2, Width - padding, rowY - 2);
                    }

                    // Column 1: Product Title (Truncate if needed)
                    Rectangle titleRect = new Rectangle(col1X, rowY + 6, col2X - col1X - 10, 20);
                    var sf = new StringFormat
                    {
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    g.DrawString(post.Title, (i == _hoveredRow) ? boldFont : bodyFont, textBrush, titleRect, sf);

                    // Column 2: Status Pill Badge
                    string statusLabel = post.Status == PostStatus.Published ? "In Stock" : "Low Stock";
                    Color statusBg = post.Status == PostStatus.Published ? ThemeManager.SuccessGreenBg : ThemeManager.WarningYellowBg;
                    Color statusFg = post.Status == PostStatus.Published ? ThemeManager.SuccessGreen : ThemeManager.WarningYellow;

                    using (Font pillFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
                    {
                        SizeF pillSz = g.MeasureString(statusLabel, pillFont);
                        int pillW = (int)pillSz.Width + 14;
                        int pillH = 18;
                        Rectangle pillRect = new Rectangle(col2X, rowY + 6, pillW, pillH);

                        using (GraphicsPath pillPath = GraphicsHelper.GetRoundedRectanglePath(pillRect, 9))
                        {
                            using (SolidBrush pillBgBrush = new SolidBrush(statusBg))
                            {
                                g.FillPath(pillBgBrush, pillPath);
                            }
                            using (Pen pillBorder = new Pen(Color.FromArgb(40, statusFg), 1f))
                            {
                                g.DrawPath(pillBorder, pillPath);
                            }
                        }

                        using (SolidBrush pillFgBrush = new SolidBrush(statusFg))
                        {
                            var pillSf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(statusLabel, pillFont, pillFgBrush, pillRect, pillSf);
                        }
                    }

                    // Column 3: Date
                    g.DrawString(post.Date, bodyFont, dateBrush, col3X, rowY + 6);
                }
            }
        }
    }
}
