using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using assignment_code.Services;
using assignment_code.Services.Database;
using assignment_code.UI.Controls;

namespace assignment_code.UI.Views
{
    public partial class SettingsView : UserControl, IRefreshableView
    {
        private StoreDataService _dataService = StoreDataService.Instance;
        private Panel _contentWrapper;
        private Panel _storeProfileCard;
        private Panel _posConfigCard;
        private Panel _databaseCard;

        private TextBox _txtName;
        private TextBox _txtPhone;
        private TextBox _txtEmail;
        private TextBox _txtAddress;
        private TextBox _txtCurrency;
        private TextBox _txtTax;
        private TextBox _txtReceiptHeader;
        private TextBox _txtReceiptFooter;
        private Button _btnSave;

        // Database Card Controls
        private Label _lblDbHost;
        private Label _lblDbName;
        private Label _lblDbStatus;
        private Button _btnTestDb;
        private Button _btnMigrateDb;
        private TextBox _txtDbLog;

        public SettingsView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            AutoScroll = true;

            InitializeComponent();
            DoubleBufferHelper.EnableDoubleBufferingTree(this);
            LoadSettings();
            ThemeManager.ThemeChanged += (s, e) => RefreshView();
            _dataService.DataRefreshed += (s, e) => RefreshView();
            TranslationManager.LanguageChanged += (s, e) => RefreshView();
        }

        public void RefreshView()
        {
            LoadSettings();
            ApplyTheme();
            Invalidate(true);
        }

        private Label _lblName;
        private Label _lblPhone;
        private Label _lblEmail;
        private Label _lblAddr;
        private Label _lblCur;
        private Label _lblTax;
        private Label _lblHdr;
        private Label _lblFtr;

        private void InitializeComponent()
        {
            _contentWrapper = new Panel
            {
                Location = new Point(0, 0),
                Width = ClientSize.Width,
                Height = 850,
                BackColor = Color.Transparent
            };
            Controls.Add(_contentWrapper);
            Resize += (s, e) => RepositionContent();

            // 1. Store Profile Card
            _storeProfileCard = new Panel { BackColor = ThemeManager.CardBackground };
            _storeProfileCard.Paint += (s, e) => DrawCardBackground(e.Graphics, _storeProfileCard, TranslationManager.T("StoreProfileAndContact", "Store Profile & Contact"));

            _lblName = new Label { Text = TranslationManager.T("StoreName:", "Store Name:"), Left = 20, Top = 50, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtName = new TextBox { Left = 20, Top = 72, Width = 320, Font = FontHelper.CreateFont(9.5F) };

            _lblPhone = new Label { Text = TranslationManager.T("Phone:", "Phone:"), Left = 20, Top = 108, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtPhone = new TextBox { Left = 20, Top = 130, Width = 320, Font = FontHelper.CreateFont(9.5F) };

            _lblEmail = new Label { Text = TranslationManager.T("Email:", "Email:"), Left = 20, Top = 166, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtEmail = new TextBox { Left = 20, Top = 188, Width = 320, Font = FontHelper.CreateFont(9.5F) };

            _lblAddr = new Label { Text = TranslationManager.T("StoreAddress:", "Store Address:"), Left = 20, Top = 224, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtAddress = new TextBox { Left = 20, Top = 246, Width = 320, Font = FontHelper.CreateFont(9.5F) };

            _storeProfileCard.Controls.AddRange(new Control[] { _lblName, _txtName, _lblPhone, _txtPhone, _lblEmail, _txtEmail, _lblAddr, _txtAddress });

            // 2. POS Config Card
            _posConfigCard = new Panel { BackColor = ThemeManager.CardBackground };
            _posConfigCard.Paint += (s, e) => DrawCardBackground(e.Graphics, _posConfigCard, TranslationManager.T("PosAndFinancialConfig", "POS & Financial Configuration"));

            _lblCur = new Label { Text = TranslationManager.T("CurrencySymbol:", "Currency Symbol:"), Left = 20, Top = 50, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtCurrency = new TextBox { Left = 20, Top = 72, Width = 150, Font = FontHelper.CreateFont(9.5F) };

            _lblTax = new Label { Text = TranslationManager.T("SalesTaxRate:", "Sales Tax Rate (%):"), Left = 190, Top = 50, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtTax = new TextBox { Left = 190, Top = 72, Width = 150, Font = FontHelper.CreateFont(9.5F) };

            _lblHdr = new Label { Text = TranslationManager.T("ReceiptHeader:", "Receipt Header Text:"), Left = 20, Top = 108, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtReceiptHeader = new TextBox { Left = 20, Top = 130, Width = 320, Font = FontHelper.CreateFont(9.5F) };

            _lblFtr = new Label { Text = TranslationManager.T("ReceiptFooter:", "Receipt Footer Message:"), Left = 20, Top = 166, AutoSize = true, Font = FontHelper.CreateFont(9F) };
            _txtReceiptFooter = new TextBox { Left = 20, Top = 188, Width = 320, Font = FontHelper.CreateFont(9.5F) };

            _btnSave = new Button
            {
                Text = TranslationManager.T("SaveSettings", "Save Store Settings"),
                Font = FontHelper.CreateFont(9.5F, FontStyle.Bold),
                Left = 20,
                Top = 236,
                Width = 320,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;

            _posConfigCard.Controls.AddRange(new Control[] { _lblCur, _txtCurrency, _lblTax, _txtTax, _lblHdr, _txtReceiptHeader, _lblFtr, _txtReceiptFooter, _btnSave });

            // 3. Database Migration Card
            _databaseCard = new Panel { BackColor = ThemeManager.CardBackground };
            _databaseCard.Paint += (s, e) => DrawCardBackground(e.Graphics, _databaseCard, TranslationManager.T("DatabaseSettings", "Database & Migration"));

            string host = EnvLoader.Get("DB_HOST", "pg-2718aec6-evennha7-6e4c.j.aivencloud.com");
            string port = EnvLoader.Get("DB_PORT", "14495");
            string db = EnvLoader.Get("DB_DATABASE", "codeassignment_db");

            _lblDbHost = new Label
            {
                Text = $"Host: {host}:{port}",
                Left = 20,
                Top = 48,
                Width = 450,
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary
            };

            _lblDbName = new Label
            {
                Text = $"Database: {db} (PostgreSQL)",
                Left = 20,
                Top = 72,
                Width = 450,
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary
            };

            _lblDbStatus = new Label
            {
                Text = "● Status: Ready / Connected",
                Left = 20,
                Top = 96,
                Width = 450,
                Font = FontHelper.CreateFont(8.5F, FontStyle.Regular),
                ForeColor = ThemeManager.SuccessGreen
            };

            _btnTestDb = new Button
            {
                Text = TranslationManager.T("TestConnection", "Test DB Connection"),
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Left = 20,
                Top = 126,
                Width = 180,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.HoverBackground,
                ForeColor = ThemeManager.TextPrimary,
                Cursor = Cursors.Hand
            };
            _btnTestDb.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            _btnTestDb.Click += async (s, e) => await TestConnectionAsync();

            _btnMigrateDb = new Button
            {
                Text = TranslationManager.T("RunMigration", "Migrate Database Now"),
                Font = FontHelper.CreateFont(9F, FontStyle.Bold),
                Left = 210,
                Top = 126,
                Width = 200,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.AccentBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnMigrateDb.FlatAppearance.BorderSize = 0;
            _btnMigrateDb.Click += async (s, e) => await RunMigrationAsync();

            _txtDbLog = new TextBox
            {
                Left = 20,
                Top = 172,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8.5F),
                BackColor = ThemeManager.SearchBoxBackground,
                ForeColor = ThemeManager.TextPrimary,
                Text = "Click 'Migrate Database Now' or 'Test DB Connection' to execute PostgreSQL operations."
            };

            _databaseCard.Controls.AddRange(new Control[] { _lblDbHost, _lblDbName, _lblDbStatus, _btnTestDb, _btnMigrateDb, _txtDbLog });

            _contentWrapper.Controls.AddRange(new Control[] { _storeProfileCard, _posConfigCard, _databaseCard });

            TranslationManager.LanguageChanged += (s, e) =>
            {
                _lblName.Text = TranslationManager.T("StoreName:", "Store Name:");
                _lblPhone.Text = TranslationManager.T("Phone:", "Phone:");
                _lblEmail.Text = TranslationManager.T("Email:", "Email:");
                _lblAddr.Text = TranslationManager.T("StoreAddress:", "Store Address:");
                _lblCur.Text = TranslationManager.T("CurrencySymbol:", "Currency Symbol:");
                _lblTax.Text = TranslationManager.T("SalesTaxRate:", "Sales Tax Rate (%):");
                _lblHdr.Text = TranslationManager.T("ReceiptHeader:", "Receipt Header Text:");
                _lblFtr.Text = TranslationManager.T("ReceiptFooter:", "Receipt Footer Message:");
                _btnSave.Text = TranslationManager.T("SaveSettings", "Save Store Settings");
                _btnSave.Font = FontHelper.CreateFont(9.5F, FontStyle.Bold);
                _btnTestDb.Text = TranslationManager.T("TestConnection", "Test DB Connection");
                _btnMigrateDb.Text = TranslationManager.T("RunMigration", "Migrate Database Now");
                _storeProfileCard.Invalidate();
                _posConfigCard.Invalidate();
                _databaseCard.Invalidate();
            };

            RepositionContent();
        }

        private async Task TestConnectionAsync()
        {
            _btnTestDb.Enabled = false;
            _lblDbStatus.Text = "â³ Testing connection...";
            _lblDbStatus.ForeColor = ThemeManager.WarningYellow;

            string msg;
            long elapsed;
            bool ok = false;

            await Task.Run(() =>
            {
                ok = DbConnectionHelper.TestConnection(out msg, out elapsed);
            });

            _btnTestDb.Enabled = true;
            if (ok)
            {
                _lblDbStatus.Text = "● Status: Connected (Online)";
                _lblDbStatus.ForeColor = ThemeManager.SuccessGreen;
                _txtDbLog.Text = $"[{DateTime.Now:HH:mm:ss}] Connection successful!\n" +
                                 $"Host: {EnvLoader.Get("DB_HOST")}\n" +
                                 $"Database: {EnvLoader.Get("DB_DATABASE")}\n" +
                                 $"Port: {EnvLoader.Get("DB_PORT")}";
            }
            else
            {
                _lblDbStatus.Text = "● Status: Connection Failed";
                _lblDbStatus.ForeColor = ThemeManager.DangerRed;
                _txtDbLog.Text = $"[{DateTime.Now:HH:mm:ss}] Connection failed ({elapsed}ms):\n{msg}\n\n" +
                                 $"Host: {EnvLoader.Get("DB_HOST")}\n" +
                                 $"Port: {EnvLoader.Get("DB_PORT")}\n" +
                                 $"User: {EnvLoader.Get("DB_USERNAME")}";
            }
        }

        private async Task RunMigrationAsync()
        {
            _btnMigrateDb.Enabled = false;
            _lblDbStatus.Text = "Testing/Running migration...";
            _lblDbStatus.ForeColor = ThemeManager.WarningYellow;
            _txtDbLog.Text = $"[{DateTime.Now:HH:mm:ss}] Starting database migration...\n";

            MigrationResult res = null;
            await Task.Run(() =>
            {
                res = DatabaseMigrator.Migrate(seedInitialData: true);
            });

            _btnMigrateDb.Enabled = true;
            if (res != null && res.Success)
            {
                _lblDbStatus.Text = "● Status: Migrated & Up to Date";
                _lblDbStatus.ForeColor = ThemeManager.SuccessGreen;
                _txtDbLog.Text = $"[{DateTime.Now:HH:mm:ss}] {res.Message}\n\nExecution Log:\n" + string.Join("\r\n", res.ExecutedSteps);
            }
            else
            {
                _lblDbStatus.Text = "● Status: Migration Failed";
                _lblDbStatus.ForeColor = ThemeManager.DangerRed;
                _txtDbLog.Text = $"[{DateTime.Now:HH:mm:ss}] {res?.Message}\n\n" + (res?.Error != null ? res.Error.ToString() : "");
            }
        }

        private void DrawCardBackground(Graphics g, Panel panel, string title)
        {
            GraphicsHelper.SetHighQuality(g);
            using (SolidBrush parentBrush = new SolidBrush(ThemeManager.Background))
                g.FillRectangle(parentBrush, panel.ClientRectangle);

            Rectangle r = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (GraphicsPath p = GraphicsHelper.GetRoundedRectanglePath(r, 12))
            {
                using (SolidBrush bg = new SolidBrush(ThemeManager.CardBackground)) g.FillPath(bg, p);
                using (Pen bp = new Pen(ThemeManager.BorderColor, 1f)) g.DrawPath(bp, p);
            }

            using (Font f = FontHelper.CreateFontForText(title, 11.5F, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(ThemeManager.TextPrimary))
                g.DrawString(title, f, sb, 20, 18);
        }

        private void LoadSettings()
        {
            var s = _dataService.Settings;
            _txtName.Text = s.StoreName;
            _txtPhone.Text = s.Phone;
            _txtEmail.Text = s.Email;
            _txtAddress.Text = s.Address;
            _txtCurrency.Text = s.CurrencySymbol;
            _txtTax.Text = s.TaxRatePercentage.ToString("N1");
            _txtReceiptHeader.Text = s.ReceiptHeader;
            _txtReceiptFooter.Text = s.ReceiptFooter;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var s = _dataService.Settings;
            s.StoreName = _txtName.Text.Trim();
            s.Phone = _txtPhone.Text.Trim();
            s.Email = _txtEmail.Text.Trim();
            s.Address = _txtAddress.Text.Trim();
            s.CurrencySymbol = _txtCurrency.Text.Trim();
            if (decimal.TryParse(_txtTax.Text, out decimal tax)) s.TaxRatePercentage = tax;
            s.ReceiptHeader = _txtReceiptHeader.Text.Trim();
            s.ReceiptFooter = _txtReceiptFooter.Text.Trim();

            MessageBox.Show("Settings saved successfully!", "Store Settings Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;
            if (_contentWrapper != null) _contentWrapper.BackColor = ThemeManager.Background;

            _storeProfileCard.BackColor = ThemeManager.CardBackground;
            _posConfigCard.BackColor = ThemeManager.CardBackground;
            _databaseCard.BackColor = ThemeManager.CardBackground;
            _btnSave.BackColor = ThemeManager.AccentBlue;
            _txtDbLog.BackColor = ThemeManager.SearchBoxBackground;
            _txtDbLog.ForeColor = ThemeManager.TextPrimary;

            _storeProfileCard.Invalidate();
            _posConfigCard.Invalidate();
            _databaseCard.Invalidate();
            Invalidate(true);
        }

        private void RepositionContent()
        {
            if (_contentWrapper == null) return;
            int w = Math.Max(700, ClientSize.Width);
            _contentWrapper.Width = w;

            int startY = 14;
            int cardW = Math.Min(360, (w - 36) / 2);
            int cardH = 300;

            _storeProfileCard.SetBounds(0, startY, cardW, cardH);
            _posConfigCard.SetBounds(cardW + 16, startY, cardW, cardH);

            int dbCardW = Math.Min(736, cardW * 2 + 16);
            int dbCardY = startY + cardH + 16;
            int dbCardH = 290;
            _databaseCard.SetBounds(0, dbCardY, dbCardW, dbCardH);

            _txtDbLog.SetBounds(20, 172, dbCardW - 40, 100);

            _contentWrapper.Height = dbCardY + dbCardH + 30;
        }
    }
}
