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
            Height = 72;

            _searchBox = new ModernSearchBox
            {
                Size = new Size(240, 36),
                IsHeaderStyle = true,
                PlaceholderText = TranslationManager.T("SearchPlaceholder", "Search for products, orders ...")
            };
            _searchBox.SearchTextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
            Controls.Add(_searchBox);

            _notificationPopup = new NotificationPopupHost();
            _notificationPopup.NotificationControl.ItemsChanged += (s, e) =>
            {
                _hasUnreadNotifications = _notificationPopup.NotificationControl.HasUnread;
                Invalidate();
            };

            ThemeManager.ThemeChanged += (s, e) =>
            {
                BackColor = ThemeManager.Background;
                Invalidate();
            };

            TranslationManager.LanguageChanged += (s, e) =>
            {
                _searchBox.PlaceholderText = TranslationManager.T("SearchPlaceholder", "Search for products, orders ...");
                Invalidate();
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            int right = Width - 16;

            // 1. Avatar on far right (38x38)
            int avatarSize = 38;
            _avatarRect = new Rectangle(right - avatarSize, (Height - avatarSize) / 2, avatarSize, avatarSize);

            // 2. Bell icon next to avatar (34x34)
            int bellSize = 34;
            _bellRect = new Rectangle(_avatarRect.Left - bellSize - 10, (Height - bellSize) / 2, bellSize, bellSize);

            // 3. Dark/Light Theme Toggle Switch next to Bell (34x34)
            int themeSize = 34;
            _themeRect = new Rectangle(_bellRect.Left - themeSize - 10, (Height - themeSize) / 2, themeSize, themeSize);

            // 4. Language Toggle Pill next to Theme Toggle (78x32)
            int langW = 78;
            int langH = 32;
            _langRect = new Rectangle(_themeRect.Left - langW - 10, (Height - langH) / 2, langW, langH);

            // 5. Search box next to Language Pill
            if (_searchBox != null)
            {
                int searchRight = _langRect.Left - 14;
                int searchW = Math.Min(300, Math.Max(170, searchRight - 360));
                _searchBox.Size = new Size(searchW, 36);
                _searchBox.Location = new Point(searchRight - searchW, (Height - _searchBox.Height) / 2);
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

            if (_isBellHovered || _isThemeHovered || _isLangHovered || _isAvatarHovered)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (prevBell != _isBellHovered || prevTheme != _isThemeHovered || prevLang != _isLangHovered || prevAvatar != _isAvatarHovered)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isBellHovered = false;
            _isThemeHovered = false;
            _isLangHovered = false;
            _isAvatarHovered = false;
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

            // 0. Paint parent background first to eliminate clipping artifacts
            using (var parentBrush = new SolidBrush(ThemeManager.Background))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle cardBounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0) return;

            // 1. Sleek Modern Card Background
            using (GraphicsPath headerPath = GraphicsHelper.GetRoundedRectanglePath(cardBounds, 12))
            {
                if (ThemeManager.IsDark)
                {
                    Color cardBg1 = Color.FromArgb(28, 36, 58);
                    Color cardBg2 = Color.FromArgb(22, 28, 46);
                    using (LinearGradientBrush bgBrush = new LinearGradientBrush(cardBounds, cardBg1, cardBg2, LinearGradientMode.Vertical))
                    {
                        g.FillPath(bgBrush, headerPath);
                    }

                    using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
                    {
                        borderPen.Alignment = PenAlignment.Inset;
                        g.DrawPath(borderPen, headerPath);
                    }

                    // Subtle glass highlight on top inside edge
                    using (Pen glowPen = new Pen(Color.FromArgb(22, 255, 255, 255), 1f))
                    {
                        g.DrawLine(glowPen, 14, 1, Width - 15, 1);
                    }
                }
                else
                {
                    using (SolidBrush bgBrush = new SolidBrush(Color.White))
                    {
                        g.FillPath(bgBrush, headerPath);
                    }

                    using (Pen borderPen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                    {
                        borderPen.Alignment = PenAlignment.Inset;
                        g.DrawPath(borderPen, headerPath);
                    }
                }
            }

            int startX = 18;
            string displayStoreName = TranslationManager.T("StoreName", _storeName);

            // 2. Store Live Status Pill ("🏪 PCCFPI STORE • ● Live")
            using (Font storeFont = FontHelper.CreateFontForText(displayStoreName, 8.5F, FontStyle.Bold))
            using (Font statusFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
            {
                SizeF storeSz = g.MeasureString(displayStoreName, storeFont);
                string statusText = TranslationManager.CurrentLanguage == AppLanguage.Khmer ? "សកម្ម" : "Live";
                SizeF statusSz = g.MeasureString(statusText, statusFont);

                int badgeW = 28 + (int)storeSz.Width + 14 + (int)statusSz.Width + 10;
                int badgeH = 24;
                int badgeY = 10;
                Rectangle storeBadgeRect = new Rectangle(startX, badgeY, badgeW, badgeH);

                using (GraphicsPath bPath = GraphicsHelper.GetRoundedRectanglePath(storeBadgeRect, 7))
                {
                    Color bBg = ThemeManager.IsDark ? Color.FromArgb(24, 32, 54) : Color.FromArgb(243, 246, 254);
                    Color bBorder = ThemeManager.IsDark ? Color.FromArgb(42, 56, 90) : Color.FromArgb(218, 230, 248);

                    using (SolidBrush bBrush = new SolidBrush(bBg))
                    {
                        g.FillPath(bBrush, bPath);
                    }
                    using (Pen bPen = new Pen(bBorder, 1f))
                    {
                        g.DrawPath(bPen, bPath);
                    }
                }

                // Store icon
                Rectangle storeIconRect = new Rectangle(startX + 8, badgeY + 5, 14, 14);
                GraphicsHelper.DrawIcon(g, "store", storeIconRect, ThemeManager.AccentBlue, 1.6f);

                // Store name text
                using (SolidBrush storeTextBrush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(displayStoreName, storeFont, storeTextBrush, startX + 26, badgeY + 4);
                }

                // Separator dot
                int dotX = startX + 26 + (int)storeSz.Width + 4;
                using (SolidBrush dotBrush = new SolidBrush(ThemeManager.TextMuted))
                {
                    g.DrawString("•", storeFont, dotBrush, dotX, badgeY + 3);
                }

                // Live green status dot & text
                int liveX = dotX + 10;
                using (SolidBrush greenDot = new SolidBrush(ThemeManager.SuccessGreen))
                {
                    g.FillEllipse(greenDot, liveX, badgeY + 9, 6, 6);
                    g.DrawString(statusText, statusFont, greenDot, liveX + 8, badgeY + 5);
                }
            }

            // 3. User Greeting Row ("Welcome back, Stephanie Sharkey  [ Administrator ]")
            int greetY = 38;
            string welcomeText = TranslationManager.CurrentLanguage == AppLanguage.Khmer ? "សូមស្វាគមន៍," : "Welcome back,";
            using (Font greetFont = FontHelper.CreateFontForText(welcomeText, 8.5F, FontStyle.Regular))
            using (SolidBrush greetBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString(welcomeText, greetFont, greetBrush, startX, greetY);
                SizeF sz = g.MeasureString(welcomeText, greetFont);

                int nameX = (int)(startX + sz.Width + 4);
                using (Font nameFont = FontHelper.CreateFontForText(_userName, 9.5F, FontStyle.Bold))
                using (SolidBrush nameBrush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(_userName, nameFont, nameBrush, nameX, greetY - 1);
                    SizeF nameSz = g.MeasureString(_userName, nameFont);

                    // Role pill badge next to user name
                    string role = StoreDataService.Instance.CurrentUser?.Role ?? "Administrator";
                    using (Font roleFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
                    {
                        SizeF roleSz = g.MeasureString(role, roleFont);
                        int roleW = (int)roleSz.Width + 12;
                        int roleH = 18;
                        int roleX = (int)(nameX + nameSz.Width + 8);
                        int roleY = greetY;

                        Rectangle roleRect = new Rectangle(roleX, roleY, roleW, roleH);
                        using (GraphicsPath rPath = GraphicsHelper.GetRoundedRectanglePath(roleRect, 9))
                        {
                            using (SolidBrush rBg = new SolidBrush(ThemeManager.AccentBlueLight))
                            {
                                g.FillPath(rBg, rPath);
                            }
                            using (Pen rPen = new Pen(Color.FromArgb(40, ThemeManager.AccentBlue), 1f))
                            {
                                g.DrawPath(rPen, rPath);
                            }
                        }

                        using (SolidBrush rText = new SolidBrush(ThemeManager.AccentBlue))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(role, roleFont, rText, roleRect, sf);
                        }
                    }
                }
            }

            // 4. Language Selector Pill ("[🇰🇭 KH ▾]" / "[🇬🇧 EN ▾]")
            if (_langRect.Width > 0)
            {
                using (GraphicsPath langPath = GraphicsHelper.GetRoundedRectanglePath(_langRect, 8))
                {
                    Color langBg = _isLangHovered
                        ? ThemeManager.HoverBackground
                        : (ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.FromArgb(245, 247, 251));
                    Color langBorder = _isLangHovered ? ThemeManager.AccentBlue : ThemeManager.BorderColor;

                    using (SolidBrush bgBrush = new SolidBrush(langBg))
                    {
                        g.FillPath(bgBrush, langPath);
                    }
                    using (Pen borderPen = new Pen(langBorder, 1f))
                    {
                        g.DrawPath(borderPen, langPath);
                    }
                }

                string langCode = (TranslationManager.CurrentLanguage == AppLanguage.English) ? "en" : "kh";
                string langText = (TranslationManager.CurrentLanguage == AppLanguage.English) ? "EN" : "KH";

                // Draw Flag
                Rectangle flagRect = new Rectangle(_langRect.Left + 8, _langRect.Top + (_langRect.Height - 14) / 2, 20, 14);
                GraphicsHelper.DrawFlag(g, flagRect, langCode);

                using (Pen flagBorder = new Pen(ThemeManager.BorderColor, 1f))
                {
                    g.DrawRectangle(flagBorder, flagRect);
                }

                using (Font lFont = FontHelper.CreateFont(8.5F, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(langText, lFont, textBrush, _langRect.Left + 33, _langRect.Top + 8);
                }

                // Small chevron indicator
                using (Font chFont = FontHelper.CreateFont(7F, FontStyle.Regular))
                using (SolidBrush chBrush = new SolidBrush(ThemeManager.TextMuted))
                {
                    g.DrawString("▾", chFont, chBrush, _langRect.Right - 15, _langRect.Top + 8);
                }
            }

            // 5. Dark/Light Mode Toggle Switch Button (Squircle)
            if (_themeRect.Width > 0)
            {
                using (GraphicsPath themePath = GraphicsHelper.GetRoundedRectanglePath(_themeRect, 8))
                {
                    Color themeBg = _isThemeHovered
                        ? ThemeManager.HoverBackground
                        : (ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.FromArgb(245, 247, 251));
                    Color themeBorder = _isThemeHovered
                        ? (ThemeManager.IsDark ? Color.FromArgb(245, 158, 11) : ThemeManager.AccentBlue)
                        : ThemeManager.BorderColor;

                    using (SolidBrush bgBrush = new SolidBrush(themeBg))
                    {
                        g.FillPath(bgBrush, themePath);
                    }
                    using (Pen borderPen = new Pen(themeBorder, 1f))
                    {
                        g.DrawPath(borderPen, themePath);
                    }
                }

                Rectangle themeIconRect = new Rectangle(_themeRect.X + 8, _themeRect.Y + 8, 18, 18);
                Color iconCol = ThemeManager.IsDark ? Color.FromArgb(245, 158, 11) : Color.FromArgb(99, 102, 241);
                GraphicsHelper.DrawIcon(g, ThemeManager.IsDark ? "sun" : "moon", themeIconRect, iconCol, 1.6f);
            }

            // 6. Bell Notification Button (Squircle with unread dot)
            if (_bellRect.Width > 0)
            {
                using (GraphicsPath bellPath = GraphicsHelper.GetRoundedRectanglePath(_bellRect, 8))
                {
                    Color bellBg = _isBellHovered
                        ? ThemeManager.HoverBackground
                        : (ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.FromArgb(245, 247, 251));
                    Color bellBorder = _isBellHovered ? ThemeManager.AccentBlue : ThemeManager.BorderColor;

                    using (SolidBrush bgBrush = new SolidBrush(bellBg))
                    {
                        g.FillPath(bgBrush, bellPath);
                    }
                    using (Pen borderPen = new Pen(bellBorder, 1f))
                    {
                        g.DrawPath(borderPen, bellPath);
                    }
                }

                Rectangle bellIconRect = new Rectangle(_bellRect.X + 8, _bellRect.Y + 8, 18, 18);
                Color bellColor = _isBellHovered ? ThemeManager.AccentBlue : ThemeManager.TextSecondary;
                GraphicsHelper.DrawIcon(g, "bell", bellIconRect, bellColor, 1.6f);

                // Red notification badge dot
                if (_hasUnreadNotifications)
                {
                    int dotSize = 8;
                    Rectangle dotRect = new Rectangle(_bellRect.Right - 10, _bellRect.Top + 6, dotSize, dotSize);
                    using (SolidBrush dotBrush = new SolidBrush(ThemeManager.DangerRed))
                    {
                        g.FillEllipse(dotBrush, dotRect);
                    }
                    using (Pen dotRingPen = new Pen(ThemeManager.IsDark ? Color.FromArgb(23, 30, 48) : Color.White, 1.2f))
                    {
                        g.DrawEllipse(dotRingPen, dotRect);
                    }
                }
            }

            // 7. Avatar Capsule with Gradient Ring & Online Status Dot
            if (_avatarRect.Width > 0)
            {
                Rectangle avRect = _avatarRect;

                // Gradient ring around avatar
                using (var ringPath = new GraphicsPath())
                {
                    ringPath.AddEllipse(avRect);
                    using (var ringGrad = new LinearGradientBrush(avRect, ThemeManager.AccentBlue, Color.FromArgb(139, 92, 246), LinearGradientMode.ForwardDiagonal))
                    using (var ringPen = new Pen(_isAvatarHovered ? ThemeManager.AccentBlueHover : ThemeManager.AccentBlue, 1.6f))
                    {
                        g.DrawPath(ringPen, ringPath);
                    }
                }

                // Inner avatar circle
                Rectangle innerAv = new Rectangle(avRect.X + 2, avRect.Y + 2, avRect.Width - 4, avRect.Height - 4);
                GraphicsHelper.DrawAvatar(g, innerAv);

                // Online indicator green dot
                int stDotSize = 9;
                Rectangle stDotRect = new Rectangle(avRect.Right - stDotSize - 1, avRect.Bottom - stDotSize - 1, stDotSize, stDotSize);
                using (SolidBrush stBrush = new SolidBrush(ThemeManager.SuccessGreen))
                {
                    g.FillEllipse(stBrush, stDotRect);
                }
                using (Pen stRingPen = new Pen(ThemeManager.IsDark ? Color.FromArgb(24, 32, 54) : Color.White, 1.5f))
                {
                    g.DrawEllipse(stRingPen, stDotRect);
                }
            }
        }
    }
}
