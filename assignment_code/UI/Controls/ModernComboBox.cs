using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace assignment_code.UI.Controls
{
    public class ModernComboBox : ComboBox
    {
        private const int WM_PAINT = 0x000F;

        public ModernComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ItemHeight = 26;
            Font = FontHelper.CreateFont(9.25F, FontStyle.Regular);
            FlatStyle = FlatStyle.Flat;

            ThemeManager.ThemeChanged += (s, e) =>
            {
                ApplyTheme();
                Invalidate();
            };
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.SearchBoxBackground;
            ForeColor = ThemeManager.TextPrimary;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? ThemeManager.HoverBackground : (ThemeManager.IsDark ? Color.FromArgb(27, 34, 54) : Color.White);
            Color fg = isSelected ? ThemeManager.AccentBlue : ThemeManager.TextPrimary;

            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, e.Bounds);
            }

            string text = Items[e.Index]?.ToString() ?? "";
            using (Font f = FontHelper.CreateFont(9F, isSelected ? FontStyle.Bold : FontStyle.Regular))
            using (SolidBrush fgBrush = new SolidBrush(fg))
            {
                float textY = e.Bounds.Y + (e.Bounds.Height - f.Height) / 2f;
                g.DrawString(text, f, fgBrush, e.Bounds.X + 8, textY);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_PAINT)
            {
                try
                {
                    using (Graphics g = Graphics.FromHwnd(Handle))
                    {
                        GraphicsHelper.SetHighQuality(g);

                        // Draw sleek modern border
                        Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                        using (Pen borderPen = new Pen(ThemeManager.SearchBoxBorder, 1.0f))
                        {
                            g.DrawRectangle(borderPen, rect);
                        }

                        // Draw sleek Chevron Down Arrow
                        int arrowSize = 12;
                        int arrowX = Width - arrowSize - 10;
                        int arrowY = (Height - arrowSize) / 2;
                        Rectangle arrowRect = new Rectangle(arrowX, arrowY, arrowSize, arrowSize);

                        // Clear native button area
                        using (SolidBrush bgBrush = new SolidBrush(ThemeManager.SearchBoxBackground))
                        {
                            g.FillRectangle(bgBrush, Width - 24, 1, 23, Height - 2);
                        }

                        GraphicsHelper.DrawIcon(g, "chevrondown", arrowRect, ThemeManager.TextSecondary, 1.6f);
                    }
                }
                catch { }
            }
        }
    }
}
