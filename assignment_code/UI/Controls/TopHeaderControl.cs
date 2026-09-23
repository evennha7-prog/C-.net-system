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
        private bool _isAvatarHovered = false;
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
                      ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.Background;
            Height = 64;

            _searchBox = new ModernSearchBox
            {
                Size = new Size(260, 34),
                IsHeaderStyle = true,
                PlaceholderText = TranslationManager.T("SearchPlaceholder", "Search products, orders...")
            };
            _searchBox.SearchTextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
            Controls.Add(_searchBox);

            _notificationPopup = new NotificationPopupHost();
            _notificationPopup.NotificationControl.ItemsChanged += (s, e) =>
            {
                _hasUnreadNotifications = _notificationPopup.NotificationControl.HasUnread;
                Invalidate();
            };

            ThemeManager.ThemeChanged += (s, e) => { BackColor = ThemeManager.Background; Invalidate(); };
            TranslationManager.LanguageChanged += (s, e) =>
            {
                _searchBox.PlaceholderText = TranslationManager.T("SearchPlaceholder", "Search products, orders...");
                Invalidate();
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int right = Width - 12;
            int avatarSize = 34;
            _avatarRect = new Rectangle(right - avatarSize, (Height - avatarSize) / 2, avatarSize, avatarSize);
            int bellSize = 32;
            _bellRect = new Rectangle(_avatarRect.Left - bellSize - 8, (Height - bellSize) / 2, bellSize, bellSize);
            int themeSize = 32;
            _themeRect = new Rectangle(_bellRect.Left - themeSize - 8, (Height - themeSize) / 2, themeSize, themeSize);
            int langW = 72;
            int langH = 30;
            _langRect = new Rectangle(_themeRect.Left - langW - 8, (Height - langH) / 2, langW, langH);
            if (_searchBox != null)
            {
                int searchRight = _langRect.Left - 12;
                int searchW = Math.Min(280, Math.Max(160, searchRight - 320));
                _searchBox.Size = new Size(searchW, 34);
                _searchBox.Location = new Point(searchRight - searchW, (Height - 34) / 2);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool prevBell = _isBellHovered;
            bool prevTheme = _isThemeHovered;
            bool prevLang = _isLangHovered;
            bool prevAvatar = _isAvatarHovered;
            _isBellHovered = _bellRect.Contains(e.Location);
            _isThemeHovered = _themeRect.Contains(e.Location);
            _isLangHovered = _langRect.Contains(e.Location);
            _isAvatarHovered = _avatarRect.Contains(e.Location);
            Cursor = (_isBellHovered || _isThemeHovered || _isLangHovered || _isAvatarHovered) ? Cursors.Hand : Cursors.Default;
            if (prevBell != _isBellHovered || prevTheme != _isThemeHovered || prevLang != _isLangHovered || prevAvatar != _isAvatarHovered)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isBellHovered = false; _isThemeHovered = false; _isLangHovered = false; _isAvatarHovered = false;
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

            using (var parentBrush = new SolidBrush(ThemeManager.Background))
                g.FillRectangle(parentBrush, ClientRectangle);

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            using (GraphicsPath headerPath = GraphicsHelper.GetRoundedRectanglePath(cardBounds, 10))
            {
                if (ThemeManager.IsDark)
                {
                    using (var bgBrush = new SolidBrush(Color.FromArgb(28, 36, 58)))
                        g.FillPath(bgBrush, headerPath);
                    using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawPath(borderPen, headerPath);
                    using (Pen glowPen = new Pen(Color.FromArgb(22, 255, 255, 255), 1f))
                        g.DrawLine(glowPen, 14, 1, Width - 15, 1);
                }
                else
                {
                    using (var bgBrush = new SolidBrush(Color.White))
                        g.FillPath(bgBrush, headerPath);
                    using (Pen borderPen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                        g.DrawPath(borderPen, headerPath);
                }
            }

            int startX = 16;
            string displayStoreName = TranslationManager.T("StoreName", _storeName);

            using (Font storeFont = FontHelper.CreateFontForText(displayStoreName, 8.5F, FontStyle.Bold))
            using (Font statusFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
            {
                SizeF storeSz = g.MeasureString(displayStoreName, storeFont);
                string statusText = TranslationManager.CurrentLanguage == AppLanguage.Khmer ? "សកម្ម" : "Live";
                SizeF statusSz = g.MeasureString(statusText, statusFont);
                int badgeW = 28 + (int)storeSz.Width + 14 + (int)statusSz.Width + 10;
                int badgeH = 22;
                int badgeY = 8;
                Rectangle storeBadgeRect = new Rectangle(startX, badgeY, badgeW, badgeH);
                using (GraphicsPath bPath = GraphicsHelper.GetRoundedRectanglePath(storeBadgeRect, 6))
                {
                    Color bBg = ThemeManager.IsDark ? Color.FromArgb(24, 32, 54) : Color.FromArgb(243, 246, 254);
                    Color bBorder = ThemeManager.IsDark ? Color.FromArgb(42, 56, 90) : Color.FromArgb(218, 230, 248);
                    using (var bBrush = new SolidBrush(bBg)) g.FillPath(bBrush, bPath);
                    using (var bPen = new Pen(bBorder, 1f)) g.DrawPath(bPen, bPath);
                }
                Rectangle storeIconRect = new Rectangle(startX + 7, badgeY + 4, 13, 13);
                GraphicsHelper.DrawIcon(g, "store", storeIconRect, ThemeManager.AccentBlue, 1.5f);
                using (var storeTextBrush = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(displayStoreName, storeFont, storeTextBrush, startX + 24, badgeY + 3);
                int dotX = startX + 24 + (int)storeSz.Width + 4;
                using (var dotBrush = new SolidBrush(ThemeManager.TextMuted))
                    g.DrawString("•", storeFont, dotBrush, dotX, badgeY + 2);
                int liveX = dotX + 8;
                using (var greenDot = new SolidBrush(ThemeManager.SuccessGreen))
                {
                    g.FillEllipse(greenDot, liveX, badgeY + 7, 5, 5);
                    g.DrawString(statusText, statusFont, greenDot, liveX + 7, badgeY + 4);
                }
            }

            int greetY = 36;
            string welcomeText = TranslationManager.CurrentLanguage == AppLanguage.Khmer ? "សូមស្វាគមន៍," : "Welcome back,";
            Font greetFont = FontHelper.CreateFontForText(welcomeText, 8.5F, FontStyle.Regular);
            using (var greetBrush = new SolidBrush(ThemeManager.TextSecondary))
                g.DrawString(welcomeText, greetFont, greetBrush, startX, greetY);
            SizeF sz = g.MeasureString(welcomeText, greetFont);
            int nameX = (int)(startX + sz.Width + 4);
            using (Font nameFont = FontHelper.CreateFontForText(_userName, 9.5F, FontStyle.Bold))
            using (var nameBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                g.DrawString(_userName, nameFont, nameBrush, nameX, greetY - 1);
                SizeF nameSz = g.MeasureString(_userName, nameFont);
                string role = StoreDataService.Instance.CurrentUser?.Role ?? "Administrator";
                using (Font roleFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
                {
                    SizeF roleSz = g.MeasureString(role, roleFont);
                    int roleW = (int)roleSz.Width + 10;
                    int roleH = 16;
                    int roleX = (int)(nameX + nameSz.Width + 6);
                    int roleY = greetY;
                    Rectangle roleRect = new Rectangle(roleX, roleY, roleW, roleH);
                    using (GraphicsPath rPath = GraphicsHelper.GetRoundedRectanglePath(roleRect, 8))
                    {
                        using (var rBg = new SolidBrush(ThemeManager.AccentBlueLight)) g.FillPath(rBg, rPath);
                        using (var rPen = new Pen(Color.FromArgb(40, ThemeManager.AccentBlue), 1f)) g.DrawPath(rPen, rPath);
                    }
                    using (var rText = new SolidBrush(ThemeManager.AccentBlue))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(role, roleFont, rText, roleRect, sf);
                    }
                }
            }

            if (_langRect.Width > 0)
            {
                using (GraphicsPath langPath = GraphicsHelper.GetRoundedRectanglePath(_langRect, 6))
                {
                    Color langBg = _isLangHovered ? ThemeManager.HoverBackground : (ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.FromArgb(245, 247, 251));
                    Color langBorder = _isLangHovered ? ThemeManager.AccentBlue : ThemeManager.BorderColor;
                    using (var bgBrush = new SolidBrush(langBg)) g.FillPath(bgBrush, langPath);
                    using (var borderPen = new Pen(langBorder, 1f)) g.DrawPath(borderPen, langPath);
                }
                string langCode = TranslationManager.CurrentLanguage == AppLanguage.English ? "en" : "kh";
                string langText = TranslationManager.CurrentLanguage == AppLanguage.English ? "EN" : "KH";
                Rectangle flagRect = new Rectangle(_langRect.Left + 6, _langRect.Top + (_langRect.Height - 13) / 2, 18, 13);
                GraphicsHelper.DrawFlag(g, flagRect, langCode);
                using (var flagBorder = new Pen(ThemeManager.BorderColor, 1f))
                    g.DrawRectangle(flagBorder, flagRect);
                using (Font lFont = FontHelper.CreateFont(8F, FontStyle.Bold))
                using (var textBrush = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(langText, lFont, textBrush, _langRect.Left + 28, _langRect.Top + 7);
            }

            if (_themeRect.Width > 0)
            {
                using (GraphicsPath themePath = GraphicsHelper.GetRoundedRectanglePath(_themeRect, 6))
                {
                    Color themeBg = _isThemeHovered ? ThemeManager.HoverBackground : (ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.FromArgb(245, 247, 251));
                    Color themeBorder = _isThemeHovered ? (ThemeManager.IsDark ? Color.FromArgb(245, 158, 11) : ThemeManager.AccentBlue) : ThemeManager.BorderColor;
                    using (var bgBrush = new SolidBrush(themeBg)) g.FillPath(bgBrush, themePath);
                    using (var borderPen = new Pen(themeBorder, 1f)) g.DrawPath(borderPen, themePath);
                }
                Rectangle themeIconRect = new Rectangle(_themeRect.X + 6, _themeRect.Y + 6, 18, 18);
                Color iconCol = ThemeManager.IsDark ? Color.FromArgb(245, 158, 11) : Color.FromArgb(99, 102, 241);
                GraphicsHelper.DrawIcon(g, ThemeManager.IsDark ? "sun" : "moon", themeIconRect, iconCol, 1.5f);
            }

            if (_bellRect.Width > 0)
            {
                using (GraphicsPath bellPath = GraphicsHelper.GetRoundedRectanglePath(_bellRect, 6))
                {
                    Color bellBg = _isBellHovered ? ThemeManager.HoverBackground : (ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.FromArgb(245, 247, 251));
                    Color bellBorder = _isBellHovered ? ThemeManager.AccentBlue : ThemeManager.BorderColor;
                    using (var bgBrush = new SolidBrush(bellBg)) g.FillPath(bgBrush, bellPath);
                    using (var borderPen = new Pen(bellBorder, 1f)) g.DrawPath(borderPen, bellPath);
                }
                Rectangle bellIconRect = new Rectangle(_bellRect.X + 6, _bellRect.Y + 6, 18, 18);
                Color bellColor = _isBellHovered ? ThemeManager.AccentBlue : ThemeManager.TextSecondary;
                GraphicsHelper.DrawIcon(g, "bell", bellIconRect, bellColor, 1.5f);
                if (_hasUnreadNotifications)
                {
                    using (var dotBrush = new SolidBrush(ThemeManager.DangerRed))
                        g.FillEllipse(dotBrush, _bellRect.Right - 8, _bellRect.Top + 4, 7, 7);
                    using (var dotRing = new Pen(ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.White, 1.2f))
                        g.DrawEllipse(dotRing, _bellRect.Right - 8, _bellRect.Top + 4, 7, 7);
                }
            }

            if (_avatarRect.Width > 0)
            {
                Rectangle avRect = _avatarRect;
                using (var ringPath = new GraphicsPath())
                {
                    ringPath.AddEllipse(avRect);
                    using (var ringGrad = new LinearGradientBrush(avRect, ThemeManager.AccentBlue, Color.FromArgb(139, 92, 246), LinearGradientMode.ForwardDiagonal))
                    using (var ringPen = new Pen(_isAvatarHovered ? ThemeManager.AccentBlueHover : ThemeManager.AccentBlue, 1.5f))
                        g.DrawPath(ringPen, ringPath);
                }
                Rectangle innerAv = new Rectangle(avRect.X + 2, avRect.Y + 2, avRect.Width - 4, avRect.Height - 4);
                GraphicsHelper.DrawAvatar(g, innerAv);
                using (var stBrush = new SolidBrush(ThemeManager.SuccessGreen))
                    g.FillEllipse(stBrush, avRect.Right - 7, avRect.Bottom - 7, 7, 7);
                using (var stRing = new Pen(ThemeManager.IsDark ? Color.FromArgb(24, 32, 54) : Color.White, 1.5f))
                    g.DrawEllipse(stRing, avRect.Right - 7, avRect.Bottom - 7, 7, 7);
            }
        }
    }
}
