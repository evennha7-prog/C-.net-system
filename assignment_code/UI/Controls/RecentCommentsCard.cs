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
    public class RecentCommentsCard : Control
    {
        private List<CommentItem> _comments = new List<CommentItem>();
        private int _borderRadius = 16;
        private int _hoveredRow = -1;

        public event EventHandler<CommentItem> ViewCommentClicked;

        public RecentCommentsCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.Background;
            Size = new Size(550, 240);

            ThemeManager.ThemeChanged += (s, e) =>
            {
                BackColor = ThemeManager.Background;
                Invalidate();
            };
        }

        public void Bind(List<CommentItem> comments)
        {
            _comments = comments ?? new List<CommentItem>();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int headerH = 76;
            int rowH = 36;

            int prevRow = _hoveredRow;
            _hoveredRow = -1;

            if (e.Y >= headerH && _comments.Count > 0)
            {
                int idx = (e.Y - headerH) / rowH;
                if (idx >= 0 && idx < _comments.Count)
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

            if (prevRow != _hoveredRow)
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
            if (_hoveredRow >= 0 && _hoveredRow < _comments.Count)
            {
                ViewCommentClicked?.Invoke(this, _comments[_hoveredRow]);
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

            // 2. Title ("Recent Transactions")
            string titleText = TranslationManager.T("RecentTransactions", "Recent Transactions");
            using (Font titleFont = FontHelper.CreateFontForText(titleText, 11F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(titleText, titleFont, titleBrush, padding, padding);
            }

            // 3. Table Column Headers
            int colHeaderY = 50;
            int col1X = padding;
            int col2X = (int)(Width * 0.38f);
            int col3X = (int)(Width * 0.72f);

            string hAuthor = TranslationManager.T("Customer", "Customer");
            string hPreview = TranslationManager.T("Details", "Transaction Details");
            string hDate = TranslationManager.T("Date", "Date");

            using (Font headFont = FontHelper.CreateFont(8.5F, FontStyle.Bold))
            using (SolidBrush headBrush = new SolidBrush(ThemeManager.TextMuted))
            {
                g.DrawString(hAuthor, headFont, headBrush, col1X, colHeaderY);
                g.DrawString(hPreview, headFont, headBrush, col2X, colHeaderY);
                g.DrawString(hDate, headFont, headBrush, col3X, colHeaderY);
            }

            // 4. Check for Empty State
            if (_comments.Count == 0)
            {
                int iconY = 96;
                int iconBoxSize = 36;
                Rectangle iconRect = new Rectangle((Width - iconBoxSize) / 2, iconY, iconBoxSize, iconBoxSize);

                Color emptyBg = ThemeManager.IsDark ? Color.FromArgb(28, 38, 65) : Color.FromArgb(238, 242, 255);
                using (GraphicsPath ipath = GraphicsHelper.GetRoundedRectanglePath(iconRect, 10))
                using (SolidBrush ibrush = new SolidBrush(emptyBg))
                {
                    g.FillPath(ibrush, ipath);
                }

                Rectangle innerIconRect = new Rectangle(iconRect.X + 8, iconRect.Y + 8, iconBoxSize - 16, iconBoxSize - 16);
                GraphicsHelper.DrawIcon(g, "report", innerIconRect, ThemeManager.AccentBlue, 1.6f);

                string emptyTitle = TranslationManager.T("NoRecentTransactions", "No Recent Transactions");
                string emptyDesc = TranslationManager.T("EmptyTransDesc", "Completed checkout sales will appear here automatically.");

                using (Font eTitleFont = FontHelper.CreateFont(9.5F, FontStyle.Bold))
                using (SolidBrush eTitleBrush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(emptyTitle, eTitleFont, eTitleBrush, Width / 2f, iconY + iconBoxSize + 10, sf);
                }

                using (Font eDescFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
                using (SolidBrush eDescBrush = new SolidBrush(ThemeManager.TextMuted))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(emptyDesc, eDescFont, eDescBrush, Width / 2f, iconY + iconBoxSize + 30, sf);
                }

                return;
            }

            // 5. Data Rows
            int startY = 76;
            int rowHeight = 36;

            using (Font authorFont = FontHelper.CreateFont(8.75F, FontStyle.Bold))
            using (Font previewFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
            using (Font dateFont = FontHelper.CreateFont(8.5F, FontStyle.Regular))
            using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextPrimary))
            using (SolidBrush detailBrush = new SolidBrush(ThemeManager.TextSecondary))
            using (SolidBrush dateBrush = new SolidBrush(ThemeManager.TextMuted))
            using (Pen divPen = new Pen(ThemeManager.SubtleDivider, 1f))
            {
                int maxRows = Math.Min(4, _comments.Count);
                for (int i = 0; i < maxRows; i++)
                {
                    var comment = _comments[i];
                    int rowY = startY + (i * rowHeight);

                    // Row hover
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

                    // Column 1: Customer Initials + Name
                    Rectangle avatarRect = new Rectangle(col1X, rowY + 5, 24, 24);
                    GraphicsHelper.DrawInitialsAvatar(g, avatarRect, comment.Author);

                    g.DrawString(comment.Author, authorFont, textBrush, col1X + 30, rowY + 6);

                    // Column 2: Details / Preview
                    Rectangle previewRect = new Rectangle(col2X, rowY + 6, col3X - col2X - 10, 20);
                    var sf = new StringFormat
                    {
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    g.DrawString(comment.Preview, previewFont, detailBrush, previewRect, sf);

                    // Column 3: Date
                    g.DrawString(comment.Date, dateFont, dateBrush, col3X, rowY + 6);
                }
            }
        }
    }
}
