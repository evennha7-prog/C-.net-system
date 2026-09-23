using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class CategoriesView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;

        public CategoriesView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponent();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            PopulateCards();
            ThemeManager.ThemeChanged += (s, e) => RefreshView();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) =>
            {
                _btnAddCategory.Text = TranslationManager.T("AddNewCategory", "Add New Category");
                RefreshView();
            };
        }

        public void RefreshView()
        {
            ApplyTheme();
            PopulateCards();
            RepositionContent();
            Invalidate(true);
        }

        private void CategoriesView_Resize(object sender, EventArgs e)
        {
            RepositionContent();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _dataService.LoadData();
            PopulateCards();
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            var newCat = ModernDialogHelper.ShowCategoryDialog(this);
            if (newCat != null)
            {
                _dataService.AddCategory(newCat);
                PopulateCards();
            }
        }

        private void ShowEditCategoryDialog(Category cat)
        {
            var updated = ModernDialogHelper.ShowCategoryDialog(this, cat);
            if (updated != null)
            {
                _dataService.UpdateCategory(updated);
                PopulateCards();
            }
        }

        private void PopulateCards()
        {
            _cardsPanel.Controls.Clear();
            foreach (var cat in _dataService.Categories)
            {
                var card = CreateCategoryCard(cat);
                _cardsPanel.Controls.Add(card);
            }
        }

        private Control CreateCategoryCard(Category cat)
        {
            Panel card = new Panel
            {
                Size = new Size(280, 185),
                Margin = new Padding(0, 0, 16, 16),
                BackColor = ThemeManager.CardBackground
            };
            DoubleBufferHelper.EnableDoubleBuffering(card);

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                GraphicsHelper.SetHighQuality(g);
                Rectangle r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);

                using (GraphicsPath path = GraphicsHelper.GetRoundedRectanglePath(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground))
                        g.FillPath(bg, path);
                    using (Pen p = new Pen(ThemeManager.BorderColor, 1f))
                        g.DrawPath(p, path);
                }

                // Color accent bar on top
                Color accent = ThemeManager.AccentBlue;
                try { accent = ColorTranslator.FromHtml(cat.ColorHex); } catch { }
                using (SolidBrush ab = new SolidBrush(accent))
                    g.FillRectangle(ab, 18, 16, 6, 22);

                // Category name
                using (Font fName = FontHelper.CreateFontForText(cat.Name, 12F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(cat.Name, fName, sb, 32, 14);

                // Description
                using (Font fDesc = FontHelper.CreateFontForText(cat.Description, 8.5F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString(cat.Description, fDesc, sb, 18, 46);

                // Divider line
                using (Pen dp = new Pen(ThemeManager.SubtleDivider, 1f))
                    g.DrawLine(dp, 18, 84, card.Width - 18, 84);

                // Product count
                int actualCount = _dataService.Products.Count(p => string.Equals(p.Category, cat.Name, StringComparison.OrdinalIgnoreCase));
                using (Font fCount = FontHelper.CreateFont(11F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString($"{actualCount}", fCount, sb, 18, 96);

                string prodLbl = TranslationManager.T("Products", "Products");
                using (Font fLbl = FontHelper.CreateFontForText(prodLbl, 8F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString(prodLbl, fLbl, sb, 18, 120);

                // Revenue
                using (Font fRev = FontHelper.CreateFont(11F, FontStyle.Bold))
                using (SolidBrush sb = new SolidBrush(ThemeManager.SuccessGreen))
                    g.DrawString($"${cat.TotalRevenue:N0}", fRev, sb, 140, 96);

                string revLbl = TranslationManager.T("Revenue", "Revenue");
                using (Font fLbl = FontHelper.CreateFontForText(revLbl, 8F))
                using (SolidBrush sb = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString(revLbl, fLbl, sb, 140, 120);
            };

            // Edit button
            Button btnEdit = new Button
            {
                Text = "Edit",
                Font = FontHelper.CreateFont(8.5F, FontStyle.Bold),
                Size = new Size(80, 28),
                Location = new Point(95, 143),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.HoverBackground,
                ForeColor = ThemeManager.TextPrimary,
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            btnEdit.Click += (s, e) => ShowEditCategoryDialog(cat);

            // Delete button
            Button btnDel = new Button
            {
                Text = "Delete",
                Font = FontHelper.CreateFont(8.5F, FontStyle.Bold),
                Size = new Size(80, 28),
                Location = new Point(182, 143),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 239, 68, 68),
                ForeColor = ThemeManager.DangerRed,
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderColor = ThemeManager.DangerRed;
            btnDel.Click += (s, e) =>
            {
                if (MessageBox.Show($"Are you sure you want to delete category '{cat.Name}' from the database?", "Confirm Delete Category", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _dataService.DeleteCategory(cat.Id);
                    PopulateCards();
                }
            };

            card.Controls.Add(btnEdit);
            card.Controls.Add(btnDel);

            return card;
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_contentWrapper != null) _contentWrapper.BackColor = ThemeManager.Background;
            if (_cardsPanel != null) _cardsPanel.BackColor = ThemeManager.Background;

            _btnAddCategory.BackColor = ThemeManager.AccentBlue;
            _btnRefresh.BackColor = ThemeManager.CardBackground;
            _btnRefresh.ForeColor = ThemeManager.TextPrimary;
            _btnRefresh.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            PopulateCards();
            Invalidate(true);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                RepositionContent();
            }
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null) return;
            int w = Math.Max(700, Math.Max(ClientSize.Width, Parent != null ? Parent.ClientSize.Width : 0));
            _contentWrapper.Location = new Point(0, 0);
            _contentWrapper.Width = w;

            int startY = 14;
            _btnRefresh.Location = new Point(0, startY);
            _btnAddCategory.Location = new Point(w - _btnAddCategory.Width, startY);

            int cardsY = startY + 44;
            _cardsPanel.Location = new Point(0, cardsY);
            _cardsPanel.Size = new Size(w, Math.Max(500, ClientSize.Height - cardsY - 20));
            _contentWrapper.Height = cardsY + _cardsPanel.Height + 20;
        }
    }
}
