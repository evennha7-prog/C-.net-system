using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class AppearanceView : UserControl, IRefreshableView
    {
        private Panel _contentWrapper;
        private Panel _cardLight;
        private Panel _cardDark;
        private Panel _cardLangEn;
        private Panel _cardLangKh;
        private Panel _previewCard;

        public AppearanceView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            AutoScroll = true;

            InitializeComponent();
            ThemeManager.ThemeChanged += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void RefreshView()
        {
            ApplyTheme();
            RepositionContent();
            Invalidate(true);
        }

        private void InitializeComponent()
        {
            _contentWrapper = new Panel
            {
                Location = new Point(0, 0),
                Width = ClientSize.Width,
                Height = 720,
                BackColor = Color.Transparent
            };
            Controls.Add(_contentWrapper);
            Resize += (s, e) => RepositionContent();

            // 1. Light Theme Selection Card
            _cardLight = new Panel
            {
                Size = new Size(240, 130),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            _cardLight.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _cardLight.Width - 1, _cardLight.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(Color.White)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(!ThemeManager.IsDark ? ThemeManager.AccentBlue : Color.FromArgb(220, 225, 235), !ThemeManager.IsDark ? 2.5f : 1f))
                        g.DrawPath(bp, p);
                }

                // Mini preview bar
                using (SolidBrush hb = new SolidBrush(Color.FromArgb(37, 99, 235)))
                    g.FillRectangle(hb, 16, 14, _cardLight.Width - 32, 20);

                using (Font f = FontHelper.CreateFont(10.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(30, 41, 59)))
                    g.DrawString("Light Theme", f, sb, 16, 44);

                using (Font fDesc = FontHelper.CreateFont(8F))
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(100, 116, 139)))
                    g.DrawString("Crisp clean white background for high daylight clarity.", fDesc, sb, new RectangleF(16, 70, _cardLight.Width - 32, 48));
            };
            _cardLight.Click += (s, e) => ThemeManager.SetTheme(AppThemeMode.Light);

            // 2. Dark Theme Selection Card
            _cardDark = new Panel
            {
                Size = new Size(240, 130),
                BackColor = Color.FromArgb(27, 34, 54),
                Cursor = Cursors.Hand
            };
            _cardDark.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _cardDark.Width - 1, _cardDark.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(Color.FromArgb(27, 34, 54))) g.FillPath(bg, p);
                    using (Pen bp = new Pen(ThemeManager.IsDark ? ThemeManager.AccentBlue : Color.FromArgb(50, 65, 95), ThemeManager.IsDark ? 2.5f : 1f))
                        g.DrawPath(bp, p);
                }

                // Mini preview bar
                using (SolidBrush hb = new SolidBrush(Color.FromArgb(28, 52, 105)))
                    g.FillRectangle(hb, 16, 14, _cardDark.Width - 32, 20);

                using (Font f = FontHelper.CreateFont(10.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(Color.White))
                    g.DrawString("Dark Theme", f, sb, 16, 44);

                using (Font fDesc = FontHelper.CreateFont(8F))
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(154, 166, 191)))
                    g.DrawString("Deep navy OLED-friendly palette for low eye strain.", fDesc, sb, new RectangleF(16, 70, _cardDark.Width - 32, 48));
            };
            _cardDark.Click += (s, e) => ThemeManager.SetTheme(AppThemeMode.Dark);

            // 3. Language English Selection Card
            _cardLangEn = new Panel
            {
                Size = new Size(240, 95),
                BackColor = ThemeManager.CardBackground,
                Cursor = Cursors.Hand
            };
            _cardLangEn.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                bool isSelected = (TranslationManager.CurrentLanguage == AppLanguage.English);
                Rectangle r = new Rectangle(0, 0, _cardLangEn.Width - 1, _cardLangEn.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(isSelected ? ThemeManager.AccentBlue : ThemeManager.BorderColor, isSelected ? 2.5f : 1f))
                        g.DrawPath(bp, p);
                }

                Rectangle flagRect = new Rectangle(16, 20, 26, 18);
                GraphicsHelper.DrawFlag(g, flagRect, "en");
                using (Pen fBorder = new Pen(ThemeManager.BorderColor, 1f))
                {
                    g.DrawRectangle(fBorder, flagRect);
                }

                using (Font f = FontHelper.CreateEnglishFont(10.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString("English (EN)", f, sb, 50, 17);

                using (Font fDesc = FontHelper.CreateEnglishFont(8.5F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString("Default store language", fDesc, sb, 50, 42);
            };
            _cardLangEn.Click += (s, e) => TranslationManager.SetLanguage(AppLanguage.English);

            // 4. Language Khmer Selection Card
            _cardLangKh = new Panel
            {
                Size = new Size(240, 95),
                BackColor = ThemeManager.CardBackground,
                Cursor = Cursors.Hand
            };
            _cardLangKh.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                bool isSelected = (TranslationManager.CurrentLanguage == AppLanguage.Khmer);
                Rectangle r = new Rectangle(0, 0, _cardLangKh.Width - 1, _cardLangKh.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(isSelected ? ThemeManager.AccentBlue : ThemeManager.BorderColor, isSelected ? 2.5f : 1f))
                        g.DrawPath(bp, p);
                }

                Rectangle flagRect = new Rectangle(16, 20, 26, 18);
                GraphicsHelper.DrawFlag(g, flagRect, "kh");
                using (Pen fBorder = new Pen(ThemeManager.BorderColor, 1f))
                {
                    g.DrawRectangle(fBorder, flagRect);
                }

                using (Font f = FontHelper.CreateKhmerFont(10.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString("áž—áž¶ážŸáž¶ážáŸ’áž˜áŸ‚ážš (Khmer - KH)", f, sb, 50, 17);

                using (Font fDesc = FontHelper.CreateKhmerFont(8.5F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString("áž”áž€áž”áŸ’ážšáŸ‚áž‚áŸ’ážšáž”áŸ‹áž¢áŸáž€áŸ’ážšáž„áŸ‹áž‘áž¶áŸ†áž„áž¢ážŸáŸ‹", fDesc, sb, 50, 42);
            };
            _cardLangKh.Click += (s, e) => TranslationManager.SetLanguage(AppLanguage.Khmer);

            // 5. Live Preview Card
            _previewCard = new Panel { BackColor = ThemeManager.CardBackground };
            _previewCard.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, _previewCard.Width - 1, _previewCard.Height - 1);
                using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                    using (Pen bp = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(bp, p);
                }

                using (Font hF = FontHelper.CreateFont(11.5F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString("Live UI Preview", hF, sb, 20, 16);

                string langName = (TranslationManager.CurrentLanguage == AppLanguage.Khmer) ? "áž—áž¶ážŸáž¶ážáŸ’áž˜áŸ‚ážš (Khmer)" : "English (EN)";
                using (Font dF = FontHelper.CreateFontForText(langName, 9F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString($"Active Theme: {(ThemeManager.IsDark ? "Dark Mode" : "Light Mode")}  |  Language: {langName}", dF, sb, 20, 44);

                // Sample button preview
                Rectangle btnR = new Rectangle(20, 76, 140, 36);
                using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(btnR, 6))
                using (SolidBrush bb = new SolidBrush(ThemeManager.AccentBlue))
                {
                    g.FillPath(bb, bp);
                    using (Font bf = FontHelper.CreateFont(9F, FontStyle.Bold))
                    using (SolidBrush btb = new SolidBrush(Color.White))
                        g.DrawString("Primary Accent", bf, btb, 32, 84);
                }

                // Sample success badge
                Rectangle badgeR = new Rectangle(180, 76, 110, 36);
                using (GraphicsPath bp = GraphicsHelper.GetRoundedRectanglePath(badgeR, 6))
                using (SolidBrush bb = new SolidBrush(ThemeManager.SuccessGreenBg))
                {
                    g.FillPath(bb, bp);
                    using (Font bf = FontHelper.CreateFont(9F, FontStyle.Bold))
                    using (SolidBrush btb = new SolidBrush(ThemeManager.SuccessGreen))
                        g.DrawString("Active Store", bf, btb, 196, 84);
                }
            };

            _contentWrapper.Controls.AddRange(new Control[] { _cardLight, _cardDark, _cardLangEn, _cardLangKh, _previewCard });

            RepositionContent();
        }

        private void ApplyTheme()
        {
            _cardLangEn.BackColor = ThemeManager.CardBackground;
            _cardLangKh.BackColor = ThemeManager.CardBackground;
            _previewCard.BackColor = ThemeManager.CardBackground;
            _cardLight.Invalidate();
            _cardDark.Invalidate();
            _cardLangEn.Invalidate();
            _cardLangKh.Invalidate();
            _previewCard.Invalidate();
            Invalidate(true);
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null) return;
            int w = Math.Max(700, ClientSize.Width);
            _contentWrapper.Width = w;

            int startY = 14;
            _cardLight.Location = new Point(0, startY);
            _cardDark.Location = new Point(260, startY);

            int langY = startY + 146;
            _cardLangEn.Location = new Point(0, langY);
            _cardLangKh.Location = new Point(260, langY);

            int previewY = langY + 110;
            _previewCard.Location = new Point(0, previewY);
            _previewCard.Size = new Size(Math.Min(w, 520), 130);

            _contentWrapper.Height = previewY + _previewCard.Height + 20;
        }
    }
}
