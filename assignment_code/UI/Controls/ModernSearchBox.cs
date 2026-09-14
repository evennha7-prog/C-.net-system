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
        private int _borderRadius = 10;
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
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
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
                ApplyColors();
                UpdateCueBanner();
                Invalidate();
            };
        }

        private void ApplyColors()
        {
            if (_innerTextBox == null) return;

            if (_isHeaderStyle)
            {
                _innerTextBox.BackColor = Color.White;
                _innerTextBox.ForeColor = Color.FromArgb(15, 23, 42);
            }
            else
            {
                _innerTextBox.BackColor = ThemeManager.SearchBoxBackground;
                _innerTextBox.ForeColor = ThemeManager.TextPrimary;
            }
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
                _innerTextBox.Location = new Point(34, (Height - _innerTextBox.Height) / 2);
                _innerTextBox.Width = Width - 44;
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

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color bgCol = _isHeaderStyle ? Color.White : ThemeManager.SearchBoxBackground;
            Color borderCol = _isHeaderStyle 
                ? (_isFocused ? Color.FromArgb(37, 99, 235) : Color.FromArgb(219, 234, 254))
                : (_isFocused ? ThemeManager.AccentBlue : ThemeManager.SearchBoxBorder);
            Color iconCol = _isHeaderStyle ? Color.FromArgb(37, 99, 235) : ThemeManager.TextMuted;
            Color placeCol = _isHeaderStyle ? Color.FromArgb(148, 163, 184) : ThemeManager.TextMuted;

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

            // Draw Placeholder if text is empty and not focused
            if (string.IsNullOrEmpty(_innerTextBox.Text) && !_isFocused)
            {
                using (Font pFont = FontHelper.CreateFont(9F, FontStyle.Regular))
                using (SolidBrush pBrush = new SolidBrush(placeCol))
                {
                    g.DrawString(_placeholderText, pFont, pBrush, 34, (Height - pFont.Height) / 2);
                }
            }
        }
    }
}
