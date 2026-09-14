using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI.Controls
{
    public class TopHeaderControl : Control
    {
        private string _storeName = "PCCFPI STORE";
        private string _userName = "Stephanie Sharkey";
        private ModernSearchBox _searchBox;
        private Rectangle _avatarRect;
        private Rectangle _bellRect;
        private Rectangle _themeRect;
        private Rectangle _langRect;
        private bool _hasUnreadNotifications = true;
        private bool _isBellHovered = false;
        private bool _isThemeHovered = false;
        private bool _isLangHovered = false;
        private NotificationPopupHost _notificationPopup;

        public event EventHandler SearchTextChanged;
        public event EventHandler NotificationClicked;
        public event EventHandler ProfileClicked;

        [Category("Appearance")]
        public string StoreName
        {
            get => _storeName;
            set { _storeName = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string UserName
        {
            get => _userName;
            set { _userName = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string SearchText => _searchBox?.Text ?? string.Empty;

        public TopHeaderControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Height = 68;

            _searchBox = new ModernSearchBox
            {
                Size = new Size(230, 36),
                IsHeaderStyle = true,
                PlaceholderText = TranslationManager.T("SearchPlaceholder", "Search for find products ...")
            };
            _searchBox.SearchTextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
            Controls.Add(_searchBox);

            _notificationPopup = new NotificationPopupHost();
            _notificationPopup.NotificationControl.ItemsChanged += (s, e) =>
            {
                _hasUnreadNotifications = _notificationPopup.NotificationControl.HasUnread;
                Invalidate();
            };

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
            TranslationManager.LanguageChanged += (s, e) =>
            {
                _searchBox.PlaceholderText = TranslationManager.T("SearchPlaceholder", "Search for find products ...");
                Invalidate();
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            int right = Width - 16;

            // 1. Avatar on far right
            int avatarSize = 38;
            _avatarRect = new Rectangle(right - avatarSize, (Height - avatarSize) / 2, avatarSize, avatarSize);

            // 2. Bell icon next to avatar
            int bellSize = 34;
            _bellRect = new Rectangle(_avatarRect.Left - bellSize - 10, (Height - bellSize) / 2, bellSize, bellSize);

            // 3. Dark/Light Theme Toggle Switch next to Bell
            int themeSize = 34;
            _themeRect = new Rectangle(_bellRect.Left - themeSize - 10, (Height - themeSize) / 2, themeSize, themeSize);

            // 4. Language Toggle Pill next to Theme Toggle
            int langW = 76;
            int langH = 32;
            _langRect = new Rectangle(_themeRect.Left - langW - 10, (Height - langH) / 2, langW, langH);

            // 5. Search box next to Language Pill
            if (_searchBox != null)
            {
                int searchW = Math.Min(260, Math.Max(150, Width / 4));
                _searchBox.Size = new Size(searchW, 36);
                _searchBox.Location = new Point(_langRect.Left - searchW - 14, (Height - _searchBox.Height) / 2);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool prevBell = _isBellHovered;
            bool prevTheme = _isThemeHovered;
            bool prevLang = _isLangHovered;

            _isBellHovered = _bellRect.Contains(e.Location);
            _isThemeHovered = _themeRect.Contains(e.Location);
            _isLangHovered = _langRect.Contains(e.Location);

            if (_isBellHovered || _isThemeHovered || _isLangHovered || _avatarRect.Contains(e.Location))
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (prevBell != _isBellHovered || prevTheme != _isThemeHovered || prevLang != _isLangHovered)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isBellHovered = false;
            _isThemeHovered = false;
            _isLangHovered = false;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_themeRect.Contains(e.Location))
            {
                ThemeManager.ToggleTheme();
                Invalidate();
            }
            else if (_langRect.Contains(e.Location))
            {
                TranslationManager.ToggleLanguage();
                Invalidate();
            }
            else if (_bellRect.Contains(e.Location))
            {
                _hasUnreadNotifications = false;
                if (_notificationPopup != null)
                {
                    Point screenPt = PointToScreen(new Point(_bellRect.Right - _notificationPopup.Width + 4, _bellRect.Bottom + 6));
                    _notificationPopup.Show(screenPt);
                }
                NotificationClicked?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
            else if (_avatarRect.Contains(e.Location))
            {
                ProfileClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Blue Header Background Gradient
            Color blue1 = ThemeManager.IsDark ? Color.FromArgb(28, 52, 105) : Color.FromArgb(37, 99, 235);
            Color blue2 = ThemeManager.IsDark ? Color.FromArgb(18, 34, 72) : Color.FromArgb(29, 78, 216);

            using (GraphicsPath headerPath = GraphicsHelper.GetRoundedRectanglePath(cardBounds, 14))
            {
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(cardBounds, blue1, blue2, LinearGradientMode.Horizontal))
                {
                    g.FillPath(bgBrush, headerPath);
                }

                Color borderCol = ThemeManager.IsDark ? Color.FromArgb(45, 75, 140) : Color.FromArgb(59, 130, 246);
                using (Pen borderPen = new Pen(borderCol, 1f))
                {
                    borderPen.Alignment = PenAlignment.Inset;
                    g.DrawPath(borderPen, headerPath);
                }
            }

            int startX = 16;
            string displayStoreName = TranslationManager.T("StoreName", _storeName);

            // 2. Store Name Badge ("🏪 PCCFPI STORE")
            using (Font storeFont = FontHelper.CreateFontForText(displayStoreName, 8.5F, FontStyle.Bold))
            {
                SizeF storeSz = g.MeasureString(displayStoreName, storeFont);
                int badgeW = (int)storeSz.Width + 30;
                int badgeH = 22;
                Rectangle storeBadgeRect = new Rectangle(startX, 11, badgeW, badgeH);

                using (GraphicsPath bPath = GraphicsHelper.GetRoundedRectanglePath(storeBadgeRect, 5))
                using (SolidBrush bBg = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
                using (Pen bPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
                {
                    g.FillPath(bBg, bPath);
                    g.DrawPath(bPen, bPath);
                }

                Rectangle storeIconRect = new Rectangle(startX + 6, 15, 14, 14);
                GraphicsHelper.DrawIcon(g, "store", storeIconRect, Color.White, 1.5f);

                using (SolidBrush storeTextBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(displayStoreName, storeFont, storeTextBrush, startX + 23, 14);
                }
            }

            // 3. Welcome Greeting & Wave Icon + User Name
            int greetY = 38;
            string welcomeText = TranslationManager.T("Welcome", "Welcome");
            using (Font greetFont = FontHelper.CreateFontForText(welcomeText, 9F, FontStyle.Regular))
            using (SolidBrush greetBrush = new SolidBrush(Color.FromArgb(220, 235, 255)))
            {
                g.DrawString(welcomeText, greetFont, greetBrush, startX, greetY);
                SizeF sz = g.MeasureString(welcomeText, greetFont);
                Rectangle waveRect = new Rectangle((int)(startX + sz.Width + 2), greetY + 1, 13, 13);
                GraphicsHelper.DrawIcon(g, "wave", waveRect, Color.Empty);

                int nameX = (int)(startX + sz.Width + 18);
                using (Font nameFont = FontHelper.CreateFontForText(_userName, 9.5F, FontStyle.Bold))
                using (SolidBrush nameBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(_userName, nameFont, nameBrush, nameX, greetY);
                }
            }

            // 4. Language Selector Pill ("[🇰🇭 KH]" / "[🇬🇧 EN]")
            if (_langRect.Width > 0)
            {
                using (GraphicsPath langPath = GraphicsHelper.GetRoundedRectanglePath(_langRect, 8))
                {
                    Color langBg = _isLangHovered ? Color.FromArgb(70, 255, 255, 255) : Color.FromArgb(40, 255, 255, 255);
                    using (SolidBrush bgBrush = new SolidBrush(langBg))
                    {
                        g.FillPath(bgBrush, langPath);
                    }
                    using (Pen borderPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
                    {
                        g.DrawPath(borderPen, langPath);
                    }
                }

                string langCode = (TranslationManager.CurrentLanguage == AppLanguage.English) ? "en" : "kh";
                string langText = (TranslationManager.CurrentLanguage == AppLanguage.English) ? "EN" : "KH";

                // Draw Flag Image
                Rectangle flagRect = new Rectangle(_langRect.Left + 8, _langRect.Top + (_langRect.Height - 16) / 2, 22, 16);
                GraphicsHelper.DrawFlag(g, flagRect, langCode);

                using (Pen flagBorder = new Pen(Color.FromArgb(100, 255, 255, 255), 1f))
                {
                    g.DrawRectangle(flagBorder, flagRect);
                }

                using (Font lFont = FontHelper.CreateFont(9F, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(langText, lFont, textBrush, _langRect.Left + 35, _langRect.Top + 7);
                }
            }

            // 5. Dark/Light Mode Toggle Switch Button
            if (_themeRect.Width > 0)
            {
                using (GraphicsPath themeCircle = new GraphicsPath())
                {
                    themeCircle.AddEllipse(_themeRect);
                    Color themeBg = _isThemeHovered ? Color.FromArgb(70, 255, 255, 255) : Color.FromArgb(40, 255, 255, 255);
                    using (SolidBrush bgBrush = new SolidBrush(themeBg))
                    {
                        g.FillPath(bgBrush, themeCircle);
                    }
                    using (Pen borderPen = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                    {
                        g.DrawPath(borderPen, themeCircle);
                    }
                }

                Rectangle themeIconRect = new Rectangle(_themeRect.X + 8, _themeRect.Y + 8, 18, 18);
                GraphicsHelper.DrawIcon(g, ThemeManager.IsDark ? "sun" : "moon", themeIconRect, Color.White, 1.6f);
            }

            // 7. Bell Notification Button
            if (_bellRect.Width > 0)
            {
                using (GraphicsPath bellCircle = new GraphicsPath())
                {
                    bellCircle.AddEllipse(_bellRect);
                    Color bellBg = _isBellHovered ? Color.FromArgb(70, 255, 255, 255) : Color.FromArgb(40, 255, 255, 255);
                    using (SolidBrush bgBrush = new SolidBrush(bellBg))
                    {
                        g.FillPath(bgBrush, bellCircle);
                    }
                    using (Pen borderPen = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                    {
                        g.DrawPath(borderPen, bellCircle);
                    }
                }

                Rectangle bellIconRect = new Rectangle(_bellRect.X + 8, _bellRect.Y + 8, 18, 18);
                GraphicsHelper.DrawIcon(g, "bell", bellIconRect, Color.White, 1.6f);

                // Red notification badge dot
                if (_hasUnreadNotifications)
                {
                    int dotSize = 8;
                    Rectangle dotRect = new Rectangle(_bellRect.Right - 11, _bellRect.Top + 7, dotSize, dotSize);
                    using (SolidBrush dotBrush = new SolidBrush(ThemeManager.DangerRed))
                    {
                        g.FillEllipse(dotBrush, dotRect);
                    }
                    using (Pen dotRingPen = new Pen(Color.White, 1f))
                    {
                        g.DrawEllipse(dotRingPen, dotRect);
                    }
                }
            }

            // 8. Avatar
            if (_avatarRect.Width > 0)
            {
                GraphicsHelper.DrawAvatar(g, _avatarRect);
            }
        }
    }
}
