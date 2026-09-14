using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace assignment_code.Services
{
    public static class EnvLoader
    {
        private static readonly Dictionary<string, string> _envVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static bool IsLoaded { get; private set; } = false;

        static EnvLoader()
        {
            Load();
        }

        public static void Load(string customPath = null)
        {
            _envVars.Clear();

            string foundPath = customPath;
            if (string.IsNullOrEmpty(foundPath) || !File.Exists(foundPath))
            {
                foundPath = LocateEnvFile();
            }

            if (foundPath != null && File.Exists(foundPath))
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

                            // Strip surrounding single or double quotes
                            if ((val.StartsWith("\"") && val.EndsWith("\"")) ||
                                (val.StartsWith("'") && val.EndsWith("'")))
                            {
                                if (val.Length >= 2)
                                    val = val.Substring(1, val.Length - 2);
                            }

                            _envVars[key] = val;
                            try
                            {
                                Environment.SetEnvironmentVariable(key, val);
                            }
                            catch { }
                        }
                    }
                    IsLoaded = true;
                }
                catch { }
            }
        }

        private static string LocateEnvFile()
        {
            string asmDir = "";
            try { asmDir = Path.GetDirectoryName(typeof(EnvLoader).Assembly.Location); } catch { }
            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? "";
            string curDir = Directory.GetCurrentDirectory();

            var searchDirs = new List<string>();
            if (!string.IsNullOrEmpty(asmDir)) searchDirs.Add(asmDir);
            if (!string.IsNullOrEmpty(baseDir)) searchDirs.Add(baseDir);
            if (!string.IsNullOrEmpty(curDir)) searchDirs.Add(curDir);

            var possiblePaths = new List<string>();
            foreach (var dir in searchDirs)
            {
                possiblePaths.Add(Path.Combine(dir, ".env"));
                possiblePaths.Add(Path.Combine(dir, "..", "..", ".env"));
                possiblePaths.Add(Path.Combine(dir, "bin", "Debug", ".env"));
                possiblePaths.Add(Path.Combine(dir, "assignment_code", ".env"));
                possiblePaths.Add(Path.Combine(dir, "assignment_code", "bin", "Debug", ".env"));
            }

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p)) return p;
            }

            return null;
        }

        public static string Get(string key, string fallback = "")
        {
            if (string.IsNullOrWhiteSpace(key)) return fallback;
            if (_envVars.TryGetValue(key, out string val)) return val;
            string envVal = Environment.GetEnvironmentVariable(key);
            return envVal ?? fallback;
        }

        public static bool GetBool(string key, bool fallback = false)
        {
            string val = Get(key, null);
            if (val == null) return fallback;
            if (bool.TryParse(val, out bool b)) return b;
            if (val.Equals("1", StringComparison.OrdinalIgnoreCase) || val.Equals("yes", StringComparison.OrdinalIgnoreCase) || val.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (val.Equals("0", StringComparison.OrdinalIgnoreCase) || val.Equals("no", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
            return fallback;
        }

        public static int GetInt(string key, int fallback = 0)
        {
            string val = Get(key, null);
            if (val != null && int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int res))
                return res;
            return fallback;
        }

        public static decimal GetDecimal(string key, decimal fallback = 0m)
        {
            string val = Get(key, null);
            if (val != null && decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal res))
                return res;
            return fallback;
        }
    }
}
