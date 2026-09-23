using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;
using assignment_code.Services.Database;

namespace assignment_code.UI.Controls
{
    public class NotificationItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string TimeText { get; set; }
        public string IconName { get; set; } = "bell";
        public Color IconColor { get; set; } = Color.FromArgb(59, 130, 246);
        public bool IsRead { get; set; } = false;
    }

    public class NotificationDropdownControl : UserControl
    {
        private List<NotificationItem> _items = new List<NotificationItem>();
        private Rectangle _clearButtonRect;
        private bool _isClearHovered = false;
        private int _hoveredItemIndex = -1;

        public event EventHandler ItemsChanged;

        public NotificationDropdownControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(330, 220);
            BackColor = ThemeManager.CardBackground;
            DoubleBuffered = true;

            LoadDefaultNotifications();

            ThemeManager.ThemeChanged += (s, e) =>
            {
                BackColor = ThemeManager.CardBackground;
                Invalidate();
            };
            TranslationManager.LanguageChanged += (s, e) => Invalidate();
        }

        private void LoadDefaultNotifications()
        {
            // Initial notification items (real-time store events)
            _items.Add(new NotificationItem
            {
                Id = "1",
                Title = TranslationManager.CurrentLanguage == AppLanguage.Khmer
                    ? "ទំនិញជិតអស់ពីស្តុក: Wireless Mouse (នៅសល់ 3)"
                    : "Low Stock Alert: Wireless Mouse (3 left)",
                TimeText = "10m ago",
                IconName = "products",
                IconColor = Color.FromArgb(239, 68, 68)
            });

            _items.Add(new NotificationItem
            {
                Id = "2",
                Title = TranslationManager.CurrentLanguage == AppLanguage.Khmer
                    ? "ការបញ្ជាទិញថ្មី: #ORD-1048 ទទួលបានជោគជ័យ"
                    : "New Order: #ORD-1048 completed",
                TimeText = "35m ago",
                IconName = "orders",
                IconColor = Color.FromArgb(34, 197, 94)
            });

            _items.Add(new NotificationItem
            {
                Id = "3",
                Title = TranslationManager.CurrentLanguage == AppLanguage.Khmer
                    ? $"បានភ្ជាប់ {DbConnectionHelper.ProviderDisplayName} មូលដ្ឋានទិន្នន័យជោគជ័យ"
                    : $"{DbConnectionHelper.ProviderDisplayName} database connected successfully",
                TimeText = "1h ago",
                IconName = "settings",
                IconColor = Color.FromArgb(59, 130, 246)
            });
        }

        public bool HasUnread => _items.Exists(x => !x.IsRead);

        public void ClearAll()
        {
            _items.Clear();
            ItemsChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool prevClear = _isClearHovered;
            int prevHoverIndex = _hoveredItemIndex;

            _isClearHovered = _clearButtonRect.Contains(e.Location);

            _hoveredItemIndex = -1;
            if (_items.Count > 0 && e.Y > 48)
            {
                int itemH = 50;
                int idx = (e.Y - 48) / itemH;
                if (idx >= 0 && idx < _items.Count)
                {
                    _hoveredItemIndex = idx;
                }
            }

            if (_isClearHovered || _hoveredItemIndex >= 0)
                Cursor = Cursors.Hand;
            else
                Cursor = Cursors.Default;

            if (prevClear != _isClearHovered || prevHoverIndex != _hoveredItemIndex)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isClearHovered = false;
            _hoveredItemIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_isClearHovered && _items.Count > 0)
            {
                ClearAll();
                return;
            }

            if (_hoveredItemIndex >= 0 && _hoveredItemIndex < _items.Count)
            {
                _items[_hoveredItemIndex].IsRead = true;
                ItemsChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // 1. Draw Drop Shadow
            for (int i = 5; i >= 1; i--)
            {
                Rectangle shadowRect = new Rectangle(bounds.X - i, bounds.Y - i + 2, bounds.Width + (i * 2), bounds.Height + (i * 2));
                int alpha = (int)(18 / (float)i);
                using (var shadowPath = GraphicsHelper.GetRoundedRectanglePath(shadowRect, 14))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // 2. Main Card Background
            Color cardBg = ThemeManager.IsDark ? Color.FromArgb(30, 41, 59) : Color.White;
            Color borderColor = ThemeManager.IsDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);

            using (var cardPath = GraphicsHelper.GetRoundedRectanglePath(bounds, 12))
            {
                using (var bgBrush = new SolidBrush(cardBg))
                {
                    g.FillPath(bgBrush, cardPath);
                }
                using (var borderPen = new Pen(borderColor, 1f))
                {
                    g.DrawPath(borderPen, cardPath);
                }
            }

            // 3. Header Title: "សេចក្តីជូនដំណឹង" / "Notifications"
            string headerTitle = TranslationManager.T("NotificationsTitle", "សេចក្តីជូនដំណឹង");
            using (Font titleFont = FontHelper.CreateFontForText(headerTitle, 10.5F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(30, 41, 59)))
            {
                g.DrawString(headerTitle, titleFont, titleBrush, 16, 14);
            }

            // Clear All button if items exist
            if (_items.Count > 0)
            {
                string clearText = TranslationManager.T("ClearAll", "Clear all");
                using (Font clearFont = FontHelper.CreateFontForText(clearText, 8.5F, FontStyle.Regular))
                {
                    SizeF clearSize = g.MeasureString(clearText, clearFont);
                    _clearButtonRect = new Rectangle(Width - (int)clearSize.Width - 20, 14, (int)clearSize.Width + 8, 20);

                    Color clearColor = _isClearHovered ? Color.FromArgb(239, 68, 68) : (ThemeManager.IsDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139));
                    using (SolidBrush clearBrush = new SolidBrush(clearColor))
                    {
                        g.DrawString(clearText, clearFont, clearBrush, _clearButtonRect.X, _clearButtonRect.Y);
                    }
                }
            }
            else
            {
                _clearButtonRect = Rectangle.Empty;
            }

            // 4. Divider Line under Header
            using (Pen dividerPen = new Pen(ThemeManager.IsDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249), 1f))
            {
                g.DrawLine(dividerPen, 0, 44, Width, 44);
            }

            // 5. Body Content
            if (_items.Count == 0)
            {
                // Empty state matching the user concept: "មិនទាន់មានសេចក្តីជូនដំណឹងនៅឡើយទេ"
                string emptyText = TranslationManager.T("NoNotificationsYet", "មិនទាន់មានសេចក្តីជូនដំណឹងនៅឡើយទេ");
                using (Font emptyFont = FontHelper.CreateFontForText(emptyText, 10F, FontStyle.Regular))
                using (SolidBrush emptyBrush = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(148, 163, 184)))
                {
                    SizeF textSize = g.MeasureString(emptyText, emptyFont);
                    float tx = (Width - textSize.Width) / 2f;
                    float ty = 44 + (Height - 44 - textSize.Height) / 2f;
                    g.DrawString(emptyText, emptyFont, emptyBrush, tx, ty);
                }
            }
            else
            {
                // Render List of Notifications
                int startY = 46;
                int itemH = 52;

                for (int i = 0; i < Math.Min(_items.Count, 3); i++)
                {
                    var item = _items[i];
                    Rectangle itemRect = new Rectangle(6, startY + (i * itemH), Width - 12, itemH - 4);

                    // Hover highlight
                    if (_hoveredItemIndex == i)
                    {
                        Color hoverBg = ThemeManager.IsDark ? Color.FromArgb(45, 58, 82) : Color.FromArgb(248, 250, 252);
                        using (var hPath = GraphicsHelper.GetRoundedRectanglePath(itemRect, 8))
                        using (var hBrush = new SolidBrush(hoverBg))
                        {
                            g.FillPath(hBrush, hPath);
                        }
                    }

                    // Item Icon Circle
                    Rectangle iconCircle = new Rectangle(itemRect.X + 8, itemRect.Y + 8, 28, 28);
                    using (SolidBrush iconBg = new SolidBrush(Color.FromArgb(25, item.IconColor)))
                    {
                        g.FillEllipse(iconBg, iconCircle);
                    }
                    Rectangle iconGlyph = new Rectangle(iconCircle.X + 6, iconCircle.Y + 6, 16, 16);
                    GraphicsHelper.DrawIcon(g, item.IconName, iconGlyph, item.IconColor, 1.4f);

                    // Title
                    int textX = iconCircle.Right + 10;
                    int textW = itemRect.Width - textX - 10;
                    using (Font itemFont = FontHelper.CreateFontForText(item.Title, 8.5F, item.IsRead ? FontStyle.Regular : FontStyle.Bold))
                    using (SolidBrush itemBrush = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59)))
                    {
                        g.DrawString(item.Title, itemFont, itemBrush, new RectangleF(textX, itemRect.Y + 6, textW, 20));
                    }

                    // Timestamp
                    using (Font timeFont = FontHelper.CreateFont(7.5F, FontStyle.Regular))
                    using (SolidBrush timeBrush = new SolidBrush(ThemeManager.IsDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(148, 163, 184)))
                    {
                        g.DrawString(item.TimeText, timeFont, timeBrush, textX, itemRect.Y + 24);
                    }

                    // Unread dot
                    if (!item.IsRead)
                    {
                        using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(59, 130, 246)))
                        {
                            g.FillEllipse(dotBrush, itemRect.Right - 14, itemRect.Y + 18, 6, 6);
                        }
                    }
                }
            }
        }
    }

    public class NotificationPopupHost : ToolStripDropDown
    {
        private ToolStripControlHost _host;
        private NotificationDropdownControl _notificationControl;

        public NotificationDropdownControl NotificationControl => _notificationControl;

        public NotificationPopupHost()
        {
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            AutoSize = false;
            DropShadowEnabled = false;
            BackColor = ThemeManager.CardBackground;

            _notificationControl = new NotificationDropdownControl();
            Size = _notificationControl.Size;

            _host = new ToolStripControlHost(_notificationControl)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = _notificationControl.Size
            };

            Items.Add(_host);
        }
    }
}
