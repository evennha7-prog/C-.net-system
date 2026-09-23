using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI.Controls
{
    public class SidebarControl : Panel
    {
        private List<NavItemButton> _mainNavItems = new List<NavItemButton>();
        private List<NavItemButton> _systemNavItems = new List<NavItemButton>();
        private NavItemButton _saleParentBtn;
        private NavItemButton _posBtn;
        private NavItemButton _listSaleBtn;
        private bool _isSaleExpanded = true;

        private string _activeMenu = "Dashboard";

        public event EventHandler<string> MenuSelected;

        public SidebarControl()
        {
            SetStyle(ControlStyles.UserPaint |
                      ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.ResizeRedraw, true);

            BackColor = ThemeManager.SidebarBackground;
            Width = 236;
            Dock = DockStyle.Left;

            InitializeNavItems();

            ThemeManager.ThemeChanged += (s, e) => { BackColor = ThemeManager.SidebarBackground; Invalidate(); };
            TranslationManager.LanguageChanged += (s, e) => { UpdateItemTranslations(); Invalidate(); };
        }

        private void InitializeNavItems()
        {
            var dashboard = new NavItemButton { Text = "Dashboard", TranslationKey = "Dashboard", IconName = "dashboard", IsActive = true };
            var products = new NavItemButton { Text = "Products", TranslationKey = "Products", IconName = "products" };
            var categories = new NavItemButton { Text = "Categories", TranslationKey = "Categories", IconName = "folder", ShowChevron = true };

            _saleParentBtn = new NavItemButton
            {
                Text = "Sale",
                TranslationKey = "Sale",
                IconName = "sale",
                ShowChevron = true,
                IsExpanded = true
            };
            _posBtn = new NavItemButton { Text = "POS", TranslationKey = "POS", IconName = "pos", IsSubItem = true };
            _listSaleBtn = new NavItemButton { Text = "List Sale", TranslationKey = "List sale", IconName = "listsale", IsSubItem = true };

            var order = new NavItemButton { Text = "Orders", TranslationKey = "Order", IconName = "order" };
            var customer = new NavItemButton { Text = "Customers", TranslationKey = "Customer", IconName = "customer" };
            var report = new NavItemButton { Text = "Reports", TranslationKey = "Report", IconName = "report" };
            var users = new NavItemButton { Text = "Users", TranslationKey = "Users", IconName = "users" };

            _mainNavItems.AddRange(new[] { dashboard, products, categories, _saleParentBtn, _posBtn, _listSaleBtn, order, customer, report, users });

            var appearance = new NavItemButton { Text = "Appearance", TranslationKey = "Appearance", IconName = "appearance" };
            var settings = new NavItemButton { Text = "Settings", TranslationKey = "Settings", IconName = "settings" };
            var logout = new NavItemButton { Text = "Logout", TranslationKey = "Logout", IconName = "logout", IsDangerAction = true };

            _systemNavItems.AddRange(new[] { appearance, settings, logout });

            foreach (var item in _mainNavItems)
            {
                item.Click += (s, e) => OnMenuItemClicked((NavItemButton)s);
                Controls.Add(item);
            }

            foreach (var item in _systemNavItems)
            {
                item.Click += (s, e) => OnMenuItemClicked((NavItemButton)s);
                Controls.Add(item);
            }

            UpdateItemTranslations();
            LayoutNavItems();
        }

        private void UpdateItemTranslations()
        {
            foreach (var item in _mainNavItems)
                if (!string.IsNullOrEmpty(item.TranslationKey))
                    item.Text = TranslationManager.T(item.TranslationKey, item.TranslationKey);
            foreach (var item in _systemNavItems)
                if (!string.IsNullOrEmpty(item.TranslationKey))
                    item.Text = TranslationManager.T(item.TranslationKey, item.TranslationKey);
        }

        private void OnMenuItemClicked(NavItemButton clickedItem)
        {
            if (clickedItem == _saleParentBtn)
            {
                _isSaleExpanded = !_isSaleExpanded;
                _saleParentBtn.IsExpanded = _isSaleExpanded;
                _posBtn.Visible = _isSaleExpanded;
                _listSaleBtn.Visible = _isSaleExpanded;
                LayoutNavItems();
                Invalidate();
            }
            SelectMenuItem(clickedItem);
        }

        private void SelectMenuItem(NavItemButton clickedItem)
        {
            foreach (var item in _mainNavItems) item.IsActive = (item == clickedItem);
            foreach (var item in _systemNavItems) item.IsActive = (item == clickedItem);
            _activeMenu = clickedItem.TranslationKey ?? clickedItem.Text;
            MenuSelected?.Invoke(this, _activeMenu);
        }

        private void LayoutNavItems()
        {
            int startX = 14;
            int itemW = Width - 28;
            int currentY = 108;
            int itemH = 38;
            int subItemH = 34;
            int spacing = 3;

            // Draw section separator and label for MAIN
            foreach (var item in _mainNavItems)
            {
                if (!item.Visible) continue;
                int h = item.IsSubItem ? subItemH : itemH;
                item.Location = new Point(startX, currentY);
                item.Size = new Size(itemW, h);
                currentY += h + spacing;
            }

            currentY += 8;
            int systemStartY = currentY + 4;

            // Reposition system items based on actual location
            int sysY = systemStartY;
            foreach (var item in _systemNavItems)
            {
                if (!item.Visible) continue;
                item.Location = new Point(startX, sysY);
                item.Size = new Size(itemW, itemH);
                sysY += itemH + spacing;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutNavItems();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            GraphicsHelper.SetHighQuality(g);

            using (SolidBrush bgBrush = new SolidBrush(ThemeManager.SidebarBackground))
                g.FillRectangle(bgBrush, 0, 0, Width, Height);

            using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
                g.DrawLine(borderPen, Width - 1, 0, Width - 1, Height);

            // Logo
            int logoW = 40;
            int logoH = 40;
            Rectangle logoRect = new Rectangle((Width - logoW) / 2, 14, logoW, logoH);
            GraphicsHelper.DrawPccfpiLogo(g, logoRect);

            // Brand name
            string brandText = TranslationManager.T("StoreName", "PCCFPI STORE");
            using (Font brandFont = FontHelper.CreateFontForText(brandText, 11F, FontStyle.Bold))
            using (SolidBrush brandBrush = new SolidBrush(ThemeManager.TextPrimary))
            {
                SizeF sz = g.MeasureString(brandText, brandFont);
                float tx = (Width - sz.Width) / 2f;
                g.DrawString(brandText, brandFont, brandBrush, tx, logoRect.Bottom + 8);
            }

            // Divider
            using (Pen dividerPen = new Pen(ThemeManager.SubtleDivider, 1f))
                g.DrawLine(dividerPen, 16, 98, Width - 16, 98);
        }
    }
}
