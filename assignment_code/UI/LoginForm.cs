using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;

namespace assignment_code.UI
{
    public partial class LoginForm : Form
    {
        // P/Invoke for dragging borderless window
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public AppUser AuthenticatedUser { get; private set; }

        // Top bar buttons
        private Rectangle _langRect;
        private Rectangle _themeRect;
        private Rectangle _minRect;
        private Rectangle _closeRect;
        private bool _isLangHovered = false;
        private bool _isThemeHovered = false;
        private bool _isMinHovered = false;
        private bool _isCloseHovered = false;
        private bool _isLoginHovered = false;

        // States & Animations
        private bool _isPasswordVisible = false;
        private bool _usernameFocused = false;
        private bool _passwordFocused = false;
        private bool _isLoading = false;
        private float _spinAngle = 0f;
        private string _loadingStatusText = "Logging in...";
        private Timer _spinnerTimer;

        // Blue branding colors matching primary system theme
        private static readonly Color BluePrimary = Color.FromArgb(37, 99, 235);       // #2563EB
        private static readonly Color BlueHover = Color.FromArgb(29, 78, 216);         // #1D4ED8
        private static readonly Color BlueDark = Color.FromArgb(30, 64, 175);          // #1E40AF
        private static readonly Color BorderNormal = Color.FromArgb(226, 232, 240);    // #E2E8F0
        private static readonly Color BorderFocused = Color.FromArgb(37, 99, 235);     // #2563EB

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public LoginForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            InitializeComponent();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            ComputeTopBarRects();
            InitializeSpinner();

            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            TranslationManager.LanguageChanged += (s, e) => ApplyTranslations();

            ApplyTheme();
            ApplyTranslations();

            // Default pre-fill
            _txtUsername.Text = "stephanie@pccfpistore.com";
            _txtPassword.Text = "admin123";
        }

        private void InitializeSpinner()
        {
            _spinnerTimer = new Timer { Interval = 16 };
            _spinnerTimer.Tick += (s, e) =>
            {
                _spinAngle = (_spinAngle + 12f) % 360f;
                _btnLogin?.Invalidate();
            };
        }

        private void UsernameWrapper_Click(object sender, EventArgs e) => _txtUsername.Focus();
        private void PasswordWrapper_Click(object sender, EventArgs e) => _txtPassword.Focus();
        private void TxtUsername_Enter(object sender, EventArgs e) { _usernameFocused = true; _usernameWrapper.Invalidate(); }
        private void TxtUsername_Leave(object sender, EventArgs e) { _usernameFocused = false; _usernameWrapper.Invalidate(); }
        private void TxtPassword_Enter(object sender, EventArgs e) { _passwordFocused = true; _passwordWrapper.Invalidate(); }
        private void TxtPassword_Leave(object sender, EventArgs e) { _passwordFocused = false; _passwordWrapper.Invalidate(); }

        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            _txtPassword.UseSystemPasswordChar = !_isPasswordVisible;
            _btnTogglePassword.Invalidate();
        }

        private void BtnLogin_MouseEnter(object sender, EventArgs e) { _isLoginHovered = true; _btnLogin.Invalidate(); }
        private void BtnLogin_MouseLeave(object sender, EventArgs e) { _isLoginHovered = false; _btnLogin.Invalidate(); }

        private void LnkSignUp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string info = "PCCFPI Store Portal:\n\nPlease enter your registered username and password to log in.";
            MessageBox.Show(info, "PCCFPI Store - Sign Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ComputeTopBarRects()
        {
            int right = Width - 16;
            int btnSize = 28;
            _closeRect = new Rectangle(right - btnSize, 12, btnSize, btnSize);
            _minRect = new Rectangle(_closeRect.Left - btnSize - 4, 12, btnSize, btnSize);
            _themeRect = new Rectangle(_minRect.Left - 32 - 6, 12, 32, 28);
            _langRect = new Rectangle(_themeRect.Left - 74 - 6, 12, 74, 28);
        }

        private async void TxtField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await PerformLoginAsync();
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            await PerformLoginAsync();
        }

        private async Task PerformLoginAsync()
        {
            if (_isLoading) return;

            string username = _txtUsername.Text.Trim();
            string password = _txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                ShowAlert(TranslationManager.T("InvalidCredentials", "Please enter your username or email."));
                _txtUsername.Focus();
                return;
            }

            // Start smooth animated loading state
            SetLoadingState(true, "Logging in...");
            _spinnerTimer.Start();

            await Task.Delay(250);
            SetLoadingProgress("Verifying credentials...");

            await Task.Delay(250);
            SetLoadingProgress("Connecting to PostgreSQL...");

            string errMsg;
            var user = StoreDataService.Instance.Authenticate(username, password, out errMsg);

            await Task.Delay(200);

            if (user != null)
            {
                AuthenticatedUser = user;
                SetLoadingProgress($"Welcome, {user.FullName}!");
                await Task.Delay(300);

                _spinnerTimer.Stop();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _spinnerTimer.Stop();
                SetLoadingState(false, "");
                ShowAlert(errMsg);
                _txtPassword.SelectAll();
                _txtPassword.Focus();
            }
        }

        private void SetLoadingState(bool loading, string status)
        {
            _isLoading = loading;
            _loadingStatusText = status;
            _txtUsername.Enabled = !loading;
            _txtPassword.Enabled = !loading;
            _lnkSignUp.Enabled = !loading;
            _btnLogin.Cursor = loading ? Cursors.WaitCursor : Cursors.Hand;
            _btnLogin.Invalidate();
        }

        private void SetLoadingProgress(string status)
        {
            _loadingStatusText = status;
            _btnLogin?.Invalidate();
        }

        private void ShowAlert(string message)
        {
            _lblAlertText.Text = message;
            _alertPanel.Visible = true;
            _lblUsername.Location = new Point(32, 108);
            _usernameWrapper.Location = new Point(32, 134);
            _lblPassword.Location = new Point(32, 188);
            _passwordWrapper.Location = new Point(32, 214);
            _btnLogin.Location = new Point(32, 274);
            _lblFooterText.Location = new Point(32, 330);
            _lnkSignUp.Location = new Point(32 + _lblFooterText.Width + 6, 330);
            _alertPanel.Invalidate();
            _cardPanel.Invalidate();
        }

        private void HideAlert()
        {
            if (_alertPanel.Visible)
            {
                _alertPanel.Visible = false;
                _lblUsername.Location = new Point(32, 80);
                _usernameWrapper.Location = new Point(32, 108);
                _lblPassword.Location = new Point(32, 162);
                _passwordWrapper.Location = new Point(32, 190);
                _btnLogin.Location = new Point(32, 252);
                _lblFooterText.Location = new Point(32, 310);
                _lnkSignUp.Location = new Point(32 + _lblFooterText.Width + 6, 310);
                _cardPanel.Invalidate();
            }
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDark;
            BackColor = isDark ? Color.FromArgb(15, 23, 42) : Color.FromArgb(243, 244, 246);

            _lblTitle.ForeColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30, 41, 59);
            _lblUsername.ForeColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);
            _lblPassword.ForeColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);

            _txtUsername.BackColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            _txtUsername.ForeColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);

            _txtPassword.BackColor = isDark ? Color.FromArgb(30, 41, 59) : Color.White;
            _txtPassword.ForeColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);

            _lblFooterText.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

            _usernameWrapper.Invalidate();
            _passwordWrapper.Invalidate();
            _cardPanel.Invalidate();
            Invalidate();
        }

        private void ApplyTranslations()
        {
            string titleText = TranslationManager.T("LoginTitle", "Login");
            _lblTitle.Text = titleText;
            _lblTitle.Font = FontHelper.CreateFontForText(titleText, 17F, FontStyle.Bold);

            string userText = TranslationManager.T("Username", "Username");
            _lblUsername.Text = userText;
            _lblUsername.Font = FontHelper.CreateFontForText(userText, 9.5F, FontStyle.Bold);

            string passText = TranslationManager.T("Password", "Password");
            _lblPassword.Text = passText;
            _lblPassword.Font = FontHelper.CreateFontForText(passText, 9.5F, FontStyle.Bold);

            string footerText = TranslationManager.T("DontHaveAccount", "Don't have an account?");
            _lblFooterText.Text = footerText;
            _lblFooterText.Font = FontHelper.CreateFontForText(footerText, 9F, FontStyle.Regular);

            string signUpText = TranslationManager.T("SignUp", "Sign up");
            _lnkSignUp.Text = signUpText;
            _lnkSignUp.Font = FontHelper.CreateFontForText(signUpText, 9F, FontStyle.Bold);
            _lnkSignUp.Location = new Point(32 + _lblFooterText.PreferredWidth + 6, _lblFooterText.Top);

            _btnLogin.Invalidate();
            Invalidate();
        }

        // ==========================================
        // GDI+ Custom Rendering
        // ==========================================

        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle bounds = new Rectangle(0, 0, _cardPanel.Width - 1, _cardPanel.Height - 1);

            // Draw Drop Shadow
            for (int i = 6; i >= 1; i--)
            {
                Rectangle shadowRect = new Rectangle(bounds.X - i, bounds.Y - i + 3, bounds.Width + (i * 2), bounds.Height + (i * 2));
                int alpha = (int)(18 / (float)i);
                using (var shadowPath = GraphicsHelper.GetRoundedRectanglePath(shadowRect, 18))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Draw Card Body
            using (var path = GraphicsHelper.GetRoundedRectanglePath(bounds, 16))
            {
                Color cardBg = ThemeManager.IsDark ? Color.FromArgb(24, 32, 53) : Color.White;
                using (var brush = new SolidBrush(cardBg))
                {
                    g.FillPath(brush, path);
                }

                Color borderCol = ThemeManager.IsDark ? Color.FromArgb(45, 60, 95) : Color.FromArgb(241, 245, 249);
                using (var pen = new Pen(borderCol, 1f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void UsernameWrapper_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle bounds = new Rectangle(0, 0, _usernameWrapper.Width - 1, _usernameWrapper.Height - 1);
            using (var path = GraphicsHelper.GetRoundedRectanglePath(bounds, 6))
            {
                Color bg = ThemeManager.IsDark ? Color.FromArgb(30, 41, 59) : Color.White;
                using (var brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                Color border = _usernameFocused ? BorderFocused : (ThemeManager.IsDark ? Color.FromArgb(51, 65, 85) : BorderNormal);
                using (var pen = new Pen(border, _usernameFocused ? 1.8f : 1.2f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void PasswordWrapper_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle bounds = new Rectangle(0, 0, _passwordWrapper.Width - 1, _passwordWrapper.Height - 1);
            using (var path = GraphicsHelper.GetRoundedRectanglePath(bounds, 6))
            {
                Color bg = ThemeManager.IsDark ? Color.FromArgb(30, 41, 59) : Color.White;
                using (var brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                Color border = _passwordFocused ? BorderFocused : (ThemeManager.IsDark ? Color.FromArgb(51, 65, 85) : BorderNormal);
                using (var pen = new Pen(border, _passwordFocused ? 1.8f : 1.2f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void BtnTogglePassword_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            int w = _btnTogglePassword.Width;
            int h = _btnTogglePassword.Height;
            Color eyeColor = _isPasswordVisible ? BluePrimary : Color.FromArgb(148, 163, 184);

            using (var pen = new Pen(eyeColor, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(eyeColor))
            {
                int cx = w / 2;
                int cy = h / 2;

                g.DrawArc(pen, cx - 8, cy - 5, 16, 10, 20, 140);
                g.DrawArc(pen, cx - 8, cy - 5, 16, 10, 200, 140);
                g.FillEllipse(brush, cx - 2.5f, cy - 2.5f, 5, 5);

                if (!_isPasswordVisible)
                {
                    using (var slashPen = new Pen(eyeColor, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLine(slashPen, cx - 7, cy + 6, cx + 7, cy - 6);
                    }
                }
            }
        }

        private void BtnLogin_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle rect = new Rectangle(0, 0, _btnLogin.Width - 1, _btnLogin.Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color btnColor = _isLoading ? BlueHover : (_isLoginHovered ? BlueHover : BluePrimary);

            using (var path = GraphicsHelper.GetRoundedRectanglePath(rect, 8))
            {
                using (var brush = new SolidBrush(btnColor))
                {
                    g.FillPath(brush, path);
                }

                if (_isLoading)
                {
                    // Draw Animated Rotating Spinner
                    int spinnerSize = 18;
                    int spinnerX = 24;
                    int spinnerY = (rect.Height - spinnerSize) / 2;
                    Rectangle spinnerRect = new Rectangle(spinnerX, spinnerY, spinnerSize, spinnerSize);

                    using (var trackPen = new Pen(Color.FromArgb(80, 255, 255, 255), 2.5f))
                    {
                        g.DrawEllipse(trackPen, spinnerRect);
                    }

                    using (var spinPen = new Pen(Color.White, 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawArc(spinPen, spinnerRect, _spinAngle, 110);
                    }

                    using (var font = FontHelper.CreateFontForText(_loadingStatusText, 10F, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString(_loadingStatusText, font, textBrush, spinnerX + spinnerSize + 12, (rect.Height - 20) / 2);
                    }
                }
                else
                {
                    string text = TranslationManager.T("LoginButton", "Login");
                    using (var font = FontHelper.CreateFontForText(text, 11F, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        SizeF sz = g.MeasureString(text, font);
                        float tx = (rect.Width - sz.Width) / 2;
                        float ty = (rect.Height - sz.Height) / 2;
                        g.DrawString(text, font, textBrush, tx, ty);
                    }
                }
            }
        }

        private void AlertPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle bounds = new Rectangle(0, 0, _alertPanel.Width - 1, _alertPanel.Height - 1);
            using (var path = GraphicsHelper.GetRoundedRectanglePath(bounds, 6))
            {
                using (var bg = new SolidBrush(Color.FromArgb(254, 242, 242)))
                {
                    g.FillPath(bg, path);
                }
                using (var border = new Pen(Color.FromArgb(248, 113, 113), 1f))
                {
                    g.DrawPath(border, path);
                }
            }

            using (var redBrush = new SolidBrush(Color.FromArgb(220, 38, 38)))
            {
                g.FillEllipse(redBrush, 8, 7, 14, 14);
            }
            using (var whitePen = new Pen(Color.White, 1.5f))
            {
                g.DrawLine(whitePen, 15, 10, 15, 14);
                g.DrawLine(whitePen, 15, 16, 15, 17);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            ComputeTopBarRects();

            // 1. Close button (✕)
            if (_isCloseHovered)
            {
                using (var bg = new SolidBrush(Color.FromArgb(239, 68, 68)))
                {
                    g.FillEllipse(bg, _closeRect);
                }
            }
            using (var pen = new Pen(_isCloseHovered ? Color.White : Color.FromArgb(100, 116, 139), 1.8f))
            {
                int cx = _closeRect.X + _closeRect.Width / 2;
                int cy = _closeRect.Y + _closeRect.Height / 2;
                g.DrawLine(pen, cx - 4, cy - 4, cx + 4, cy + 4);
                g.DrawLine(pen, cx + 4, cy - 4, cx - 4, cy + 4);
            }

            // 2. Minimize button (_)
            if (_isMinHovered)
            {
                using (var bg = new SolidBrush(Color.FromArgb(226, 232, 240)))
                {
                    g.FillEllipse(bg, _minRect);
                }
            }
            using (var pen = new Pen(Color.FromArgb(100, 116, 139), 1.8f))
            {
                int cx = _minRect.X + _minRect.Width / 2;
                int cy = _minRect.Y + _minRect.Height / 2;
                g.DrawLine(pen, cx - 5, cy + 3, cx + 5, cy + 3);
            }

            // 3. Theme Toggle Switch Pill
            if (_isThemeHovered)
            {
                using (var bg = new SolidBrush(Color.FromArgb(226, 232, 240)))
                using (var path = GraphicsHelper.GetRoundedRectanglePath(_themeRect, 6))
                {
                    g.FillPath(bg, path);
                }
            }
            Rectangle themeIconRect = new Rectangle(_themeRect.X + 7, _themeRect.Y + 4, 18, 18);
            GraphicsHelper.DrawIcon(g, ThemeManager.IsDark ? "sun" : "moon", themeIconRect, Color.FromArgb(100, 116, 139), 1.6f);

            // 4. Language Pill (EN / KH with Flag icon)
            using (var langPath = GraphicsHelper.GetRoundedRectanglePath(_langRect, 6))
            {
                Color langBg = _isLangHovered ? Color.FromArgb(226, 232, 240) : (ThemeManager.IsDark ? Color.FromArgb(30, 41, 59) : Color.White);
                using (var bg = new SolidBrush(langBg))
                {
                    g.FillPath(bg, langPath);
                }
                using (var pen = new Pen(Color.FromArgb(203, 213, 225), 1f))
                {
                    g.DrawPath(pen, langPath);
                }
            }

            string flagCode = TranslationManager.CurrentLanguage == AppLanguage.Khmer ? "kh" : "en";
            string langCode = TranslationManager.CurrentLanguage == AppLanguage.Khmer ? "KH" : "EN";

            Rectangle flagRect = new Rectangle(_langRect.X + 7, _langRect.Y + (_langRect.Height - 15) / 2, 22, 15);
            GraphicsHelper.DrawFlag(g, flagRect, flagCode);

            using (var flagBorderPen = new Pen(Color.FromArgb(203, 213, 225), 1f))
            {
                g.DrawRectangle(flagBorderPen, flagRect);
            }

            using (var font = FontHelper.CreateFont(8.5F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            {
                g.DrawString(langCode, font, brush, _langRect.X + 35, _langRect.Y + 5);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ComputeTopBarRects();

            bool prevClose = _isCloseHovered;
            bool prevMin = _isMinHovered;
            bool prevTheme = _isThemeHovered;
            bool prevLang = _isLangHovered;

            _isCloseHovered = _closeRect.Contains(e.Location);
            _isMinHovered = _minRect.Contains(e.Location);
            _isThemeHovered = _themeRect.Contains(e.Location);
            _isLangHovered = _langRect.Contains(e.Location);

            if (_isCloseHovered || _isMinHovered || _isThemeHovered || _isLangHovered)
                Cursor = Cursors.Hand;
            else
                Cursor = Cursors.Default;

            if (prevClose != _isCloseHovered || prevMin != _isMinHovered || prevTheme != _isThemeHovered || prevLang != _isLangHovered)
                Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            ComputeTopBarRects();

            if (_closeRect.Contains(e.Location))
            {
                Application.Exit();
            }
            else if (_minRect.Contains(e.Location))
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (_themeRect.Contains(e.Location))
            {
                ThemeManager.SetTheme(ThemeManager.IsDark ? AppThemeMode.Light : AppThemeMode.Dark);
            }
            else if (_langRect.Contains(e.Location))
            {
                TranslationManager.ToggleLanguage();
            }
        }

        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
