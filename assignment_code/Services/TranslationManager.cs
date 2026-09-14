using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace assignment_code.Services
{
    public enum AppLanguage
    {
        English,
        Khmer
    }

    public static class TranslationManager
    {
        public static AppLanguage CurrentLanguage { get; private set; } = AppLanguage.Khmer;
        public static event EventHandler LanguageChanged;

        private static readonly Dictionary<string, string> _enTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> _khTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static TranslationManager()
        {
            LoadTranslations();
        }

        public static void SetLanguage(AppLanguage language)
        {
            if (CurrentLanguage != language)
            {
                CurrentLanguage = language;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == AppLanguage.English ? AppLanguage.Khmer : AppLanguage.English);
        }

        public static string T(string key, string fallback = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return fallback ?? "";

            var dict = (CurrentLanguage == AppLanguage.Khmer) ? _khTranslations : _enTranslations;
            if (dict.TryGetValue(key, out string translated) && !string.IsNullOrWhiteSpace(translated))
            {
                return translated;
            }

            return fallback ?? key;
        }

        public static void LoadTranslations()
        {
            _enTranslations.Clear();
            _khTranslations.Clear();

            string asmDir = "";
            try { asmDir = Path.GetDirectoryName(typeof(TranslationManager).Assembly.Location); } catch { }
            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? "";
            string curDir = Directory.GetCurrentDirectory();

            var searchDirs = new List<string>();
            if (!string.IsNullOrEmpty(asmDir)) searchDirs.Add(asmDir);
            if (!string.IsNullOrEmpty(baseDir)) searchDirs.Add(baseDir);
            if (!string.IsNullOrEmpty(curDir)) searchDirs.Add(curDir);

            var possiblePaths = new List<string>();
            foreach (var dir in searchDirs)
            {
                possiblePaths.Add(Path.Combine(dir, "teanslations.txt"));
                possiblePaths.Add(Path.Combine(dir, "translations.txt"));
                possiblePaths.Add(Path.Combine(dir, "..", "..", "teanslations.txt"));
                possiblePaths.Add(Path.Combine(dir, "..", "..", "translations.txt"));
                possiblePaths.Add(Path.Combine(dir, "bin", "Debug", "teanslations.txt"));
                possiblePaths.Add(Path.Combine(dir, "assignment_code", "bin", "Debug", "teanslations.txt"));
            }

            string foundPath = null;
            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                {
                    foundPath = p;
                    break;
                }
            }

            if (foundPath != null)
            {
                try
                {
                    var lines = File.ReadAllLines(foundPath, Encoding.UTF8);
                    foreach (var rawLine in lines)
                    {
                        var line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith(";"))
                            continue;

                        int eqIdx = line.IndexOf('=');
                        if (eqIdx > 0)
                        {
                            string key = line.Substring(0, eqIdx).Trim();
                            string val = line.Substring(eqIdx + 1).Trim();

                            if (val.Contains("|"))
                            {
                                var parts = val.Split('|');
                                string en = parts[0].Trim();
                                string kh = parts.Length > 1 ? parts[1].Trim() : en;

                                _enTranslations[key] = en;
                                _khTranslations[key] = kh;
                            }
                            else
                            {
                                _enTranslations[key] = val;
                                _khTranslations[key] = val;
                            }
                        }
                    }
                }
                catch { }
            }

            // Fallback hardcoded defaults if file was missing or empty
            AddDefault("Dashboard", "Dashboard", "ផ្ទាំងគ្រប់គ្រង");
            AddDefault("Products", "Products", "ទំនិញ");
            AddDefault("Categories", "Categories", "ប្រភេទ");
            AddDefault("Sale", "Sale", "ការលក់");
            AddDefault("POS", "POS", "ប្រព័ន្ធ POS");
            AddDefault("List sale", "List sale", "បញ្ជីការលក់");
            AddDefault("Order", "Order", "ការបញ្ជាទិញ");
            AddDefault("Customer", "Customer", "អតិថិជន");
            AddDefault("Users", "Users", "អ្នកប្រើប្រាស់");
            AddDefault("Report", "Report", "របាយការណ៍");
            AddDefault("Appearance", "Appearance", "រូបរាង");
            AddDefault("Settings", "Settings", "ការកំណត់");
            AddDefault("SYSTEM", "SYSTEM", "ប្រព័ន្ធ");
            AddDefault("Welcome", "Welcome", "សូមស្វាគមន៍");
            AddDefault("SignIn", "Sign In", "ចូលប្រើប្រាស់");
            AddDefault("WelcomeBack", "Welcome Back", "សូមស្វាគមន៍មកវិញ");
            AddDefault("LoginSubtitle", "Enter your credentials to access your store portal", "សូមបញ្ចូលព័ត៌មានគណនីរបស់អ្នកដើម្បីចូលប្រើប្រាស់");
            AddDefault("EmailOrUsername", "Email or Username", "អ៊ីមែល ឬ ឈ្មោះគណនី");
            AddDefault("Password", "Password", "ពាក្យសម្ងាត់");
            AddDefault("RememberMe", "Remember me", "ចងចាំខ្ញុំ");
            AddDefault("ForgotPassword", "Forgot Password?", "ភ្លេចពាក្យសម្ងាត់?");
            AddDefault("QuickLoginAs", "Quick demo accounts:", "គណនីគំរូរហ័ស:");
            AddDefault("InvalidCredentials", "Invalid credentials. Please verify your email and password.", "ព័ត៌មានមិនត្រឹមត្រូវទេ។ សូមពិនិត្យមើលអ៊ីមែល និងពាក្យសម្ងាត់ឡើងវិញ។");
            AddDefault("AccountSuspended", "This account is suspended. Please contact administrator.", "គណនីនេះត្រូវបានផ្អាក។ សូមទាក់ទងអ្នកគ្រប់គ្រង។");
            AddDefault("LoginSuccess", "Login successful! Loading dashboard...", "ការចូលបានជោគជ័យ! កំពុងដំណើរការ...");
            AddDefault("DatabaseConnected", "PostgreSQL Connected", "បានភ្ជាប់ PostgreSQL");
            AddDefault("DatabaseSettings", "Database & Migration", "ការកំណត់មូលដ្ឋានទិន្នន័យ & Migration");
            AddDefault("RunMigration", "Migrate Database Now", "ដំណើរការ Migrate មូលដ្ឋានទិន្នន័យ");
            AddDefault("TestConnection", "Test DB Connection", "តេស្តការភ្ជាប់ DB");
            AddDefault("DatabaseStatus", "Database Status:", "ស្ថានភាពមូលដ្ឋានទិន្នន័យ:");
            AddDefault("Logout", "Logout", "ចាកចេញ");
            AddDefault("ConfirmLogout", "Are you sure you want to log out from PCCFPI STORE?", "តើអ្នកពិតជាចង់ចាកចេញពីប្រព័ន្ធ PCCFPI STORE មែនទេ?");
        }

        private static void AddDefault(string key, string en, string kh)
        {
            if (!_enTranslations.ContainsKey(key)) _enTranslations[key] = en;
            if (!_khTranslations.ContainsKey(key)) _khTranslations[key] = kh;
        }
    }
}
