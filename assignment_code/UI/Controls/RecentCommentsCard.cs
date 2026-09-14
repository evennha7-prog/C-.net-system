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
        private int _hoveredBtnIndex = -1;

        public event EventHandler<CommentItem> ViewCommentClicked;

        public RecentCommentsCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(420, 240);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Bind(List<CommentItem> comments)
        {
            _comments = comments ?? new List<CommentItem>();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int headerH = 80;
            int rowH = 36;

            int prevRow = _hoveredRow;
            int prevBtn = _hoveredBtnIndex;

            _hoveredRow = -1;
            _hoveredBtnIndex = -1;

            int col4X = Width - 20 - 55;

            if (e.Y >= headerH)
            {
                int idx = (e.Y - headerH) / rowH;
                if (idx >= 0 && idx < _comments.Count)
                {
                    _hoveredRow = idx;

                    // Check if hovering specifically over the View button
                    int rowY = 82 + (idx * rowH);
                    Rectangle btnRect = new Rectangle(col4X, rowY - 2, 50, 24);
                    if (btnRect.Contains(e.Location))
                    {
                        _hoveredBtnIndex = idx;
                        Cursor = Cursors.Hand;
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (prevRow != _hoveredRow || prevBtn != _hoveredBtnIndex)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredRow = -1;
            _hoveredBtnIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_hoveredBtnIndex >= 0 && _hoveredBtnIndex < _comments.Count)
            {
                ViewCommentClicked?.Invoke(this, _comments[_hoveredBtnIndex]);
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

            // 2. Title ("Recent Reports")
            string titleText = TranslationManager.T("RecentReports", "Recent Reports");
            using (Font titleFont = FontHelper.CreateFontForText(titleText, 12F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(titleText, titleFont, titleBrush, padding, padding);
            }

            // 3. Table Column Headers
            int colHeaderY = 54;
            int col1X = padding;
            int col2X = (int)(Width * 0.22f);
            int col3X = (int)(Width * 0.72f);
            int col4X = Width - padding - 50;

            string hAuth = TranslationManager.T("Author", "Author");
            string hPrev = TranslationManager.T("ReportPreview", "Report Preview");
            string hDate = TranslationManager.T("Date", "Date");
            string hAct = TranslationManager.T("Action", "Action");

            using (Font headFont = FontHelper.CreateFont(9F, FontStyle.Bold))
            using (SolidBrush headBrush = new SolidBrush(ThemeManager.TextMuted))
            {
                g.DrawString(hAuth, headFont, headBrush, col1X, colHeaderY);
                g.DrawString(hPrev, headFont, headBrush, col2X, colHeaderY);
                g.DrawString(hDate, headFont, headBrush, col3X, colHeaderY);
                g.DrawString(hAct, headFont, headBrush, col4X + 8, colHeaderY);
            }

            // 4. Data Rows
            int startY = 82;
            int rowHeight = 36;

            using (Font authorFont = FontHelper.CreateFont(9F, FontStyle.Bold))
            using (Font previewFont = FontHelper.CreateFont(9F, FontStyle.Regular))
            using (Font dateFont = FontHelper.CreateFont(9F, FontStyle.Regular))
            using (Font btnFont = FontHelper.CreateFont(8.25F, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextPrimary))
            using (SolidBrush dateBrush = new SolidBrush(ThemeManager.TextSecondary))
            using (Pen divPen = new Pen(ThemeManager.SubtleDivider, 1f))
            {
                for (int i = 0; i < _comments.Count; i++)
                {
                    var comment = _comments[i];
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

                    // Column 1: Author
                    g.DrawString(comment.Author, authorFont, textBrush, col1X, rowY);

                    // Column 2: Comment Preview (Truncated if too long)
                    Rectangle previewRect = new Rectangle(col2X, rowY, col3X - col2X - 10, rowHeight);
                    if (comment.Preview.Contains("🔥"))
                    {
                        string cleanText = comment.Preview.Replace("🔥", "").TrimEnd('”', '\"', ' ') + " ";
                        g.DrawString(cleanText, previewFont, textBrush, col2X, rowY);
                        SizeF txtSz = g.MeasureString(cleanText, previewFont);
                        Rectangle fireRect = new Rectangle((int)(col2X + txtSz.Width - 2), rowY + 1, 14, 14);
                        GraphicsHelper.DrawIcon(g, "fire", fireRect, Color.Empty);
                        g.DrawString("”", previewFont, textBrush, fireRect.Right + 1, rowY);
                    }
                    else
                    {
                        var sf = new StringFormat
                        {
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        };
                        g.DrawString(comment.Preview, previewFont, textBrush, previewRect, sf);
                    }

                    // Column 3: Date
                    g.DrawString(comment.Date, dateFont, dateBrush, col3X, rowY);

                    // Column 4: [View] Button
                    Rectangle btnRect = new Rectangle(col4X, rowY - 2, 50, 22);
                    bool isBtnHovered = (i == _hoveredBtnIndex);

                    using (GraphicsPath btnPath = GraphicsHelper.GetRoundedRectanglePath(btnRect, 6))
                    using (SolidBrush btnBg = new SolidBrush(isBtnHovered ? ThemeManager.AccentBlue : ThemeManager.ViewButtonBg))
                    {
                        g.FillPath(btnBg, btnPath);
                    }

                    Color btnFg = isBtnHovered ? Color.White : ThemeManager.ViewButtonText;
                    using (SolidBrush btnTextBrush = new SolidBrush(btnFg))
                    {
                        var btnSf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("View", btnFont, btnTextBrush, btnRect, btnSf);
                    }
                }
            }
        }
    }
}
