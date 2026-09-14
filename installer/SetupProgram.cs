using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PCCFPI_Store_Installer
{
    static class SetupProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isSilent = false;
            string customDir = null;

            foreach (var rawArg in args)
            {
                var arg = rawArg.Trim();
                if (arg.Equals("/S", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("/SILENT", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-s", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
                {
                    isSilent = true;
                }
                else if (arg.StartsWith("/DIR=", StringComparison.OrdinalIgnoreCase) ||
                         arg.StartsWith("-dir=", StringComparison.OrdinalIgnoreCase))
                {
                    customDir = arg.Substring(5).Trim('"', '\'');
                }
            }

            if (isSilent)
            {
                string targetDir = string.IsNullOrWhiteSpace(customDir) 
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PCCFPI Store")
                    : customDir;

                bool ok = InstallerEngine.PerformInstallation(
                    targetDir, 
                    createDesktopShortcut: true, 
                    createStartMenuShortcut: true, 
                    progressCallback: null);

                Environment.Exit(ok ? 0 : 1);
                return;
            }

            Application.Run(new InstallerForm(customDir));
        }
    }

    public static class InstallerEngine
    {
        public const string AppName = "PCCFPI Store";
        public const string AppVersion = "1.0.0";
        public const string Publisher = "PCCFP Institute";
        public const string ExeFileName = "assignment_code.exe";
        public const string RegistryKeyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PCCFPI_Store";

        public static bool PerformInstallation(
            string targetDir, 
            bool createDesktopShortcut, 
            bool createStartMenuShortcut, 
            Action<int, string> progressCallback)
        {
            try
            {
                progressCallback?.Invoke(5, "Preparing installation directory...");
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // 1. Extract payload from embedded zip
                progressCallback?.Invoke(15, "Extracting application binaries and assets...");
                var asm = Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream("Payload.zip"))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException("Payload archive missing from installer executable.");
                    }

                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        int totalEntries = archive.Entries.Count;
                        int count = 0;
                        foreach (var entry in archive.Entries)
                        {
                            count++;
                            string completeFileName = Path.Combine(targetDir, entry.FullName);
                            string directory = Path.GetDirectoryName(completeFileName);
                            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                // Directory entry
                                continue;
                            }

                            int pct = 20 + (int)((count / (float)totalEntries) * 50);
                            progressCallback?.Invoke(pct, "Extracting: " + entry.Name);
                            entry.ExtractToFile(completeFileName, true);
                        }
                    }
                }

                // 2. Extract and write Uninstaller
                progressCallback?.Invoke(75, "Generating uninstaller...");
                using (var uninstStream = asm.GetManifestResourceStream("uninstall.exe"))
                {
                    if (uninstStream != null)
                    {
                        string uninstTarget = Path.Combine(targetDir, "uninstall.exe");
                        using (var fs = new FileStream(uninstTarget, FileMode.Create, FileAccess.Write))
                        {
                            uninstStream.CopyTo(fs);
                        }
                    }
                }

                string mainExePath = Path.Combine(targetDir, ExeFileName);
                string iconPath = Path.Combine(targetDir, "icons", "app.ico");
                if (!File.Exists(iconPath)) iconPath = mainExePath;

                // 3. Create Desktop Shortcut
                if (createDesktopShortcut)
                {
                    progressCallback?.Invoke(85, "Creating Desktop shortcut...");
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string shortcutPath = Path.Combine(desktopPath, AppName + ".lnk");
                    CreateShortcut(shortcutPath, mainExePath, targetDir, iconPath, "PCCFPI Store Management System");
                }

                // 4. Create Start Menu Shortcut
                if (createStartMenuShortcut)
                {
                    progressCallback?.Invoke(90, "Creating Start Menu shortcut...");
                    string programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    string startMenuFolder = Path.Combine(programsPath, "PCCFP Institute");
                    if (!Directory.Exists(startMenuFolder)) Directory.CreateDirectory(startMenuFolder);
                    string shortcutPath = Path.Combine(startMenuFolder, AppName + ".lnk");
                    CreateShortcut(shortcutPath, mainExePath, targetDir, iconPath, "PCCFPI Store Management System");
                }

                // 5. Register in Windows Add/Remove Programs
                progressCallback?.Invoke(95, "Registering application with Windows...");
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyName))
                    {
                        if (key != null)
                        {
                            key.SetValue("DisplayName", AppName);
                            key.SetValue("DisplayVersion", AppVersion);
                            key.SetValue("Publisher", Publisher);
                            key.SetValue("DisplayIcon", iconPath);
                            key.SetValue("InstallLocation", targetDir);
                            key.SetValue("UninstallString", "\"" + Path.Combine(targetDir, "uninstall.exe") + "\"");
                            key.SetValue("QuietUninstallString", "\"" + Path.Combine(targetDir, "uninstall.exe") + "\" /S");
                            key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                            key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                            key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        }
                    }
                }
                catch { }

                progressCallback?.Invoke(100, "Installation completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                progressCallback?.Invoke(-1, "Installation error: " + ex.Message);
                return false;
            }
        }

        public static void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string iconPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    dynamic shell = Activator.CreateInstance(shellType);
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = targetPath;
                    shortcut.WorkingDirectory = workingDir;
                    shortcut.Description = description;
                    if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                    {
                        shortcut.IconLocation = iconPath + ",0";
                    }
                    shortcut.Save();
                }
            }
            catch
            {
                // Fallback to URL shortcut or ignore
            }
        }
    }

    public class InstallerForm : Form
    {
        private TextBox txtPath;
        private Button btnBrowse;
        private CheckBox chkDesktop;
        private CheckBox chkStartMenu;
        private CheckBox chkLaunch;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnInstall;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlHeader;
        private Panel pnlBody;
        private Panel pnlFooter;

        private bool _isInstalled = false;

        public InstallerForm(string customDir)
        {
            InitializeComponent(customDir);
        }

        private void InitializeComponent(string customDir)
        {
            this.Text = "PCCFPI Store Setup — Installation Wizard";
            this.Size = new Size(580, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            try
            {
                var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico");
                if (iconStream != null)
                {
                    this.Icon = new Icon(iconStream);
                }
            }
            catch { }

            // 1. Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(17, 24, 39) // Deep Slate
            };

            lblTitle = new Label
            {
                Text = "Install PCCFPI Store",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 18),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Modern Store Management, POS & Analytics Portal (v1.0.0)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(25, 48),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Footer Panel
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(235, 237, 240)
            };

            btnInstall = new Button
            {
                Text = "Install Now",
                Size = new Size(120, 38),
                Location = new Point(310, 14),
                BackColor = Color.FromArgb(16, 185, 129), // Emerald Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(95, 38),
                Location = new Point(445, 14),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            pnlFooter.Controls.Add(btnInstall);
            pnlFooter.Controls.Add(btnCancel);

            // 3. Body Panel
            pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };

            var lblDestination = new Label
            {
                Text = "Destination Folder:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(24, 15),
                AutoSize = true
            };

            string defaultDir = string.IsNullOrWhiteSpace(customDir)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PCCFPI Store")
                : customDir;

            txtPath = new TextBox
            {
                Text = defaultDir,
                Location = new Point(24, 40),
                Size = new Size(415, 26),
                Font = new Font("Segoe UI", 9.5f)
            };

            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new Point(445, 38),
                Size = new Size(95, 29),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(31, 41, 55),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnBrowse.Click += (s, e) =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select Installation Folder for PCCFPI Store";
                    fbd.SelectedPath = txtPath.Text;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        txtPath.Text = Path.Combine(fbd.SelectedPath, "PCCFPI Store");
                    }
                }
            };

            var lblShortcuts = new Label
            {
                Text = "Shortcut Options:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(24, 85),
                AutoSize = true
            };

            chkDesktop = new CheckBox
            {
                Text = "Create Desktop Shortcut",
                Checked = true,
                Location = new Point(26, 110),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.Hand
            };

            chkStartMenu = new CheckBox
            {
                Text = "Create Start Menu Folder & Shortcut",
                Checked = true,
                Location = new Point(26, 135),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.Hand
            };

            chkLaunch = new CheckBox
            {
                Text = "Launch PCCFPI Store after installation",
                Checked = true,
                Location = new Point(26, 160),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.Hand
            };

            lblStatus = new Label
            {
                Text = "Ready to install. Click 'Install Now' to proceed.",
                Location = new Point(24, 195),
                Size = new Size(515, 20),
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9f, FontStyle.Italic)
            };

            progressBar = new ProgressBar
            {
                Location = new Point(24, 220),
                Size = new Size(515, 20),
                Style = ProgressBarStyle.Continuous,
                Value = 0
            };

            pnlBody.Controls.Add(lblDestination);
            pnlBody.Controls.Add(txtPath);
            pnlBody.Controls.Add(btnBrowse);
            pnlBody.Controls.Add(lblShortcuts);
            pnlBody.Controls.Add(chkDesktop);
            pnlBody.Controls.Add(chkStartMenu);
            pnlBody.Controls.Add(chkLaunch);
            pnlBody.Controls.Add(lblStatus);
            pnlBody.Controls.Add(progressBar);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            if (_isInstalled)
            {
                if (chkLaunch.Checked)
                {
                    try
                    {
                        string exePath = Path.Combine(txtPath.Text.Trim(), InstallerEngine.ExeFileName);
                        if (File.Exists(exePath))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = exePath,
                                WorkingDirectory = txtPath.Text.Trim()
                            });
                        }
                    }
                    catch { }
                }
                this.Close();
                return;
            }

            string targetDir = txtPath.Text.Trim();
            if (string.IsNullOrEmpty(targetDir))
            {
                MessageBox.Show("Please specify a valid installation directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtPath.Enabled = false;
            btnBrowse.Enabled = false;
            chkDesktop.Enabled = false;
            chkStartMenu.Enabled = false;
            btnInstall.Enabled = false;
            btnCancel.Enabled = false;

            bool desk = chkDesktop.Checked;
            bool start = chkStartMenu.Checked;

            bool success = false;
            await Task.Run(() =>
            {
                success = InstallerEngine.PerformInstallation(
                    targetDir,
                    desk,
                    start,
                    (pct, msg) =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            if (pct >= 0 && pct <= 100)
                            {
                                progressBar.Value = pct;
                            }
                            lblStatus.Text = msg;
                        }));
                    });
            });

            btnCancel.Enabled = true;
            btnInstall.Enabled = true;

            if (success)
            {
                _isInstalled = true;
                lblStatus.Text = "Installation successful! PCCFPI Store is ready to use.";
                lblStatus.ForeColor = Color.FromArgb(5, 150, 105);
                btnInstall.Text = "Finish";
                btnInstall.BackColor = Color.FromArgb(79, 70, 229); // Indigo
                btnCancel.Visible = false;
            }
            else
            {
                txtPath.Enabled = true;
                btnBrowse.Enabled = true;
                chkDesktop.Enabled = true;
                chkStartMenu.Enabled = true;
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                MessageBox.Show("Installation failed. Please check folder permissions and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
