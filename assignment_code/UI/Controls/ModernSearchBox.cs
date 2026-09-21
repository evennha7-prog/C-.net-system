using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace assignment_code.UI.Controls
{
    public class ModernSearchBox : Control
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private TextBox _innerTextBox;
        private string _placeholderText = "Enter keywords ...";
        private int _borderRadius = 9;
        private bool _isFocused = false;

        public event EventHandler SearchTextChanged;

        [Category("Appearance")]
        public string PlaceholderText
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value;
                UpdateCueBanner();
                Invalidate();
            }
        }

        [Category("Appearance")]
        public override string Text
        {
            get => _innerTextBox?.Text ?? string.Empty;
            set
            {
                if (_innerTextBox != null)
                {
                    _innerTextBox.Text = value;
                    Invalidate();
                }
            }
        }

        private bool _isHeaderStyle = false;

        [Category("Appearance")]
        public bool IsHeaderStyle
        {
            get => _isHeaderStyle;
            set
            {
                _isHeaderStyle = value;
                ApplyColors();
                Invalidate();
            }
        }

        public ModernSearchBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.CardBackground;
            Size = new Size(240, 36);

            _innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = FontHelper.CreateFont(9.25F, FontStyle.Regular),
                Location = new Point(34, 9),
                Size = new Size(Width - 44, 20),
                BackColor = ThemeManager.SearchBoxBackground,
                ForeColor = ThemeManager.TextPrimary
            };

            _innerTextBox.HandleCreated += (s, e) => UpdateCueBanner();
            _innerTextBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _innerTextBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _innerTextBox.TextChanged += (s, e) =>
            {
                SearchTextChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            };

            Controls.Add(_innerTextBox);

            ThemeManager.ThemeChanged += (s, e) =>
            {
                BackColor = _isHeaderStyle ? ThemeManager.CardBackground : ThemeManager.Background;
                ApplyColors();
                UpdateCueBanner();
                Invalidate();
            };

            ApplyColors();
        }

        private void ApplyColors()
        {
            if (_innerTextBox == null) return;

            Color bgCol = ThemeManager.IsDark
                ? ThemeManager.SearchBoxBackground
                : (_isHeaderStyle ? Color.FromArgb(246, 248, 252) : Color.FromArgb(243, 244, 246));

            _innerTextBox.BackColor = bgCol;
            _innerTextBox.ForeColor = ThemeManager.TextPrimary;
        }

        private void UpdateCueBanner()
        {
            if (_innerTextBox != null && _innerTextBox.IsHandleCreated)
            {
                SendMessage(_innerTextBox.Handle, EM_SETCUEBANNER, (IntPtr)1, _placeholderText);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_innerTextBox != null)
            {
                int rightPad = (_isHeaderStyle && Width > 220) ? 68 : 40;
                _innerTextBox.Location = new Point(34, (Height - _innerTextBox.Height) / 2);
                _innerTextBox.Width = Math.Max(50, Width - 34 - rightPad);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            _innerTextBox.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            // Clear parent background matching header card or container
            Color parentBg = _isHeaderStyle
                ? ThemeManager.CardBackground
                : ((Parent != null && Parent.BackColor != Color.Transparent) ? Parent.BackColor : ThemeManager.Background);

            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color bgCol = ThemeManager.IsDark
                ? ThemeManager.SearchBoxBackground
                : (_isHeaderStyle ? Color.FromArgb(246, 248, 252) : Color.FromArgb(243, 244, 246));

            Color borderCol = _isFocused
                ? ThemeManager.AccentBlue
                : (ThemeManager.IsDark ? ThemeManager.SearchBoxBorder : Color.FromArgb(226, 232, 240));

            Color iconCol = _isFocused ? ThemeManager.AccentBlue : ThemeManager.TextMuted;

            using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(rect, _borderRadius))
            {
                // Background
                using (SolidBrush bgBrush = new SolidBrush(bgCol))
                {
                    g.FillPath(bgBrush, path);
                }

                // Border
                using (Pen borderPen = new Pen(borderCol, _isFocused ? 1.5f : 1.0f))
                {
                    borderPen.Alignment = PenAlignment.Inset;
                    g.DrawPath(borderPen, path);
                }
            }

            // Draw Search Icon
            Rectangle iconRect = new Rectangle(11, (Height - 16) / 2, 16, 16);
            GraphicsHelper.DrawIcon(g, "search", iconRect, iconCol, 1.6f);

            // Draw "Ctrl K" shortcut hint pill on right
            if (_isHeaderStyle && Width > 210 && !_isFocused && string.IsNullOrEmpty(_innerTextBox.Text))
            {
                int kBadgeW = 48;
                int kBadgeH = 20;
                int kBadgeX = Width - kBadgeW - 8;
                int kBadgeY = (Height - kBadgeH) / 2;
                Rectangle kRect = new Rectangle(kBadgeX, kBadgeY, kBadgeW, kBadgeH);

                using (GraphicsPath kp = GraphicsHelper.GetRoundedRectanglePath(kRect, 5))
                {
                    Color kBg = ThemeManager.IsDark ? Color.FromArgb(28, 36, 56) : Color.FromArgb(235, 240, 248);
                    using (SolidBrush kb = new SolidBrush(kBg)) g.FillPath(kb, kp);
                    using (Pen kpen = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(kpen, kp);
                }

                using (Font kFont = FontHelper.CreateFont(7.5F, FontStyle.Bold))
                using (SolidBrush kt = new SolidBrush(ThemeManager.TextMuted))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("Ctrl K", kFont, kt, kRect, sf);
                }
            }
        }
    }
}
