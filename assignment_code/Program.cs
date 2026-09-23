using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using assignment_code.Models;
using assignment_code.Services;
using assignment_code.Services.Database;
using assignment_code.UI;

namespace assignment_code
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        [DllImport("Shcore.dll")]
        private static extern int SetProcessDpiAwareness(int PROCESS_DPI_AWARENESS);

        private const int ATTACH_PARENT_PROCESS = -1;
        private const int STD_OUTPUT_HANDLE = -11;

        private static void InitConsoleOutput()
        {
            try
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                var handle = GetStdHandle(STD_OUTPUT_HANDLE);
                var safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, false);
                var stream = new System.IO.FileStream(safeHandle, System.IO.FileAccess.Write);
                var writer = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8) { AutoFlush = true };
                Console.SetOut(writer);
                Console.SetError(writer);
            }
            catch { }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Enable native High-DPI Per-Monitor V2 rendering
            try
            {
                SetProcessDpiAwareness(2);
            }
            catch
            {
                try { SetProcessDPIAware(); } catch { }
            }

            EnvLoader.Load();

            // 1. Check for CLI migration / DB flags
            if (args != null && args.Length > 0)
            {
                string cmd = args[0].ToLowerInvariant().Trim('-', '/', ':');
                if (cmd == "migrate" || cmd == "db:migrate" || cmd == "dbmigrate")
                {
                    InitConsoleOutput();
                    Console.WriteLine("\n========================================================");
                    Console.WriteLine($" PCCFPI STORE - {DbConnectionHelper.ProviderDisplayName} Database Migrator");
                    Console.WriteLine("========================================================");
                    Console.WriteLine($"Provider:    {DbConnectionHelper.ProviderDisplayName}");
                    Console.WriteLine($"Server/Host: {DbConnectionHelper.ServerDisplayName}");
                    Console.WriteLine($"Database:    {EnvLoader.Get("DB_DATABASE")}\n");
                    Console.WriteLine("Executing migrations...");

                    var result = DatabaseMigrator.Migrate(seedInitialData: true);

                    foreach (var step in result.ExecutedSteps)
                    {
                        Console.WriteLine($"  {step}");
                    }

                    Console.WriteLine("\n--------------------------------------------------------");
                    if (result.Success)
                    {
                        Console.WriteLine($"SUCCESS: {result.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"FAILED: {result.Message}");
                        if (result.Error != null)
                        {
                            Console.WriteLine($"Details: {result.Error.Message}");
                        }
                    }
                    Console.WriteLine("========================================================\n");
                    return;
                }
                else if (cmd == "test-db" || cmd == "testdb" || cmd == "check-db")
                {
                    InitConsoleOutput();
                    Console.WriteLine($"\nTesting {DbConnectionHelper.ProviderDisplayName} Connection...");
                    string msg;
                    long ms;
                    bool ok = DbConnectionHelper.TestConnection(out msg, out ms);
                    Console.WriteLine(ok ? $"SUCCESS: {msg}" : $"FAILED: {msg}");
                    return;
                }
            }

            // 2. Normal GUI Mode
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        break;
                    }

                    using (var mainForm = new Form1(loginForm.AuthenticatedUser))
                    {
                        Application.Run(mainForm);
                        if (!mainForm.IsLoggedOut)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
