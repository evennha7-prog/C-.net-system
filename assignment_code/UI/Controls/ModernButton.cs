using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace assignment_code.UI.Controls
{
    public enum ModernButtonType
    {
        Primary,
        Secondary,
        Danger,
        Success,
        Ghost
    }

    public class ModernButton : Control
    {
        private ModernButtonType _buttonType = ModernButtonType.Primary;
        private string _iconName = null;
        private int _borderRadius = 8;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private string _translationKey = null;

        [Category("Appearance")]
        public ModernButtonType ButtonType
        {
            get => _buttonType;
            set { _buttonType = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string IconName
        {
            get => _iconName;
            set { _iconName = value; Invalidate(); }
        }

        [Category("Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string TranslationKey
        {
            get => _translationKey;
            set { _translationKey = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(140, 38);
            Font = FontHelper.CreateFont(9F, FontStyle.Bold);

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color bgColor;
            Color fgColor;
            Color borderColor = Color.Transparent;
            float borderWidth = 1.0f;

            switch (_buttonType)
            {
                case ModernButtonType.Primary:
                    bgColor = _isPressed
                        ? Color.FromArgb(29, 78, 216)
                        : (_isHovered ? ThemeManager.AccentBlueHover : ThemeManager.AccentBlue);
                    fgColor = Color.White;
                    break;

                case ModernButtonType.Secondary:
                    bgColor = _isPressed
                        ? (ThemeManager.IsDark ? Color.FromArgb(42, 53, 82) : Color.FromArgb(226, 232, 240))
                        : (_isHovered ? ThemeManager.HoverBackground : ThemeManager.CardBackground);
                    fgColor = ThemeManager.TextPrimary;
                    borderColor = _isHovered ? ThemeManager.AccentBlue : ThemeManager.BorderColor;
                    borderWidth = 1.0f;
                    break;

                case ModernButtonType.Danger:
                    bgColor = _isPressed
                        ? Color.FromArgb(185, 28, 28)
                        : (_isHovered ? Color.FromArgb(220, 38, 38) : ThemeManager.DangerRed);
                    fgColor = Color.White;
                    break;

                case ModernButtonType.Success:
                    bgColor = _isPressed
                        ? Color.FromArgb(4, 120, 87)
                        : (_isHovered ? Color.FromArgb(5, 150, 105) : ThemeManager.SuccessGreen);
                    fgColor = Color.White;
                    break;

                case ModernButtonType.Ghost:
                default:
                    bgColor = _isHovered ? ThemeManager.HoverBackground : Color.Transparent;
                    fgColor = _isHovered ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
                    break;
            }

            if (!Enabled)
            {
                bgColor = ThemeManager.IsDark ? Color.FromArgb(35, 42, 60) : Color.FromArgb(226, 232, 240);
                fgColor = ThemeManager.TextMuted;
                borderColor = Color.Transparent;
            }

            using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(rect, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }

                if (borderColor != Color.Transparent && borderWidth > 0)
                {
                    using (Pen pen = new Pen(borderColor, borderWidth))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Text & Icon Calculation
            string displayText = !string.IsNullOrEmpty(_translationKey)
                ? Services.TranslationManager.T(_translationKey, Text)
                : Text;

            bool hasIcon = !string.IsNullOrEmpty(_iconName);
            if (hasIcon && !string.IsNullOrEmpty(displayText))
            {
                if (displayText.StartsWith("+ "))
                    displayText = displayText.Substring(2).TrimStart();
                else if (displayText.StartsWith("+"))
                    displayText = displayText.Substring(1).TrimStart();
            }

            using (Font f = FontHelper.CreateFont(Font.Size, FontStyle.Bold))
            using (SolidBrush fgBrush = new SolidBrush(fgColor))
            {
                int iconSize = 16;
                int gap = 8;
                SizeF textSize = g.MeasureString(displayText, f);

                int totalContentWidth = (int)textSize.Width + (hasIcon ? (iconSize + gap) : 0);
                int startX = Math.Max(6, (Width - totalContentWidth) / 2);

                if (hasIcon)
                {
                    int iconY = (Height - iconSize) / 2;
                    Rectangle iconRect = new Rectangle(startX, iconY, iconSize, iconSize);
                    GraphicsHelper.DrawIcon(g, _iconName, iconRect, fgColor, 1.8f);
                    startX += iconSize + gap;
                }

                float textY = (Height - textSize.Height) / 2f;
                g.DrawString(displayText, f, fgBrush, startX, textY);
            }
        }
    }
}
