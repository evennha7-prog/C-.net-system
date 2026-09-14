using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PCCFPI_Store_Installer
{
    static class UninstallProgram
    {
        private const string AppName = "PCCFPI Store";
        private const string RegistryKeyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PCCFPI_Store";

        [STAThread]
        static void Main(string[] args)
        {
            bool isSilent = false;
            foreach (var arg in args)
            {
                if (arg.Equals("/S", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("/SILENT", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-s", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
                {
                    isSilent = true;
                }
            }

            if (!isSilent)
            {
                DialogResult dr = MessageBox.Show(
                    "Are you sure you want to uninstall " + AppName + " and remove all its components from this computer?",
                    AppName + " Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr != DialogResult.Yes)
                {
                    return;
                }
            }

            // 1. Close running instances of the app
            try
            {
                foreach (var proc in Process.GetProcessesByName("assignment_code"))
                {
                    try { proc.Kill(); proc.WaitForExit(2000); } catch { }
                }
            }
            catch { }

            string installDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');

            // 2. Remove shortcuts
            try
            {
                // Desktop
                string userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string desktopLnk = Path.Combine(userDesktop, AppName + ".lnk");
                if (File.Exists(desktopLnk)) File.Delete(desktopLnk);

                string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                string commonDesktopLnk = Path.Combine(commonDesktop, AppName + ".lnk");
                if (File.Exists(commonDesktopLnk)) File.Delete(commonDesktopLnk);

                // Start Menu
                string userPrograms = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string startMenuFolder = Path.Combine(userPrograms, "PCCFP Institute");
                string startMenuLnk = Path.Combine(startMenuFolder, AppName + ".lnk");
                if (File.Exists(startMenuLnk)) File.Delete(startMenuLnk);
                if (Directory.Exists(startMenuFolder) && Directory.GetFileSystemEntries(startMenuFolder).Length == 0)
                {
                    Directory.Delete(startMenuFolder, true);
                }
            }
            catch { }

            // 3. Remove Registry keys
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyName, false);
            }
            catch { }

            try
            {
                using (var hklm = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true))
                {
                    if (hklm != null)
                    {
                        hklm.DeleteSubKeyTree("PCCFPI_Store", false);
                    }
                }
            }
            catch { }

            if (!isSilent)
            {
                MessageBox.Show(
                    AppName + " has been successfully uninstalled from your computer.",
                    AppName + " Uninstall",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            // 4. Self-deletion script for remaining directory contents
            try
            {
                string tempBat = Path.Combine(Path.GetTempPath(), "uninst_" + Guid.NewGuid().ToString("N") + ".bat");
                string batContent = 
                    "@echo off\r\n" +
                    "ping 127.0.0.1 -n 2 > nul\r\n" +
                    "rd /s /q \"" + installDir + "\"\r\n" +
                    "del \"%~f0\"\r\n";
                File.WriteAllText(tempBat, batContent);

                var psi = new ProcessStartInfo
                {
                    FileName = tempBat,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch { }
        }
    }
}
