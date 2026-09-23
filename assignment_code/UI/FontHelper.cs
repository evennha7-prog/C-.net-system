using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI
{
    public static class FontHelper
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceExW", SetLastError = true)]
        private static extern int AddFontResourceEx([In][MarshalAs(UnmanagedType.LPWStr)] string lpszFilename, uint fl, IntPtr pdv);

        private const uint FR_PRIVATE = 0x10;

        public static string PrimaryFontFamily => _resolvedFamily;
        public static string KhmerFontFamily => _resolvedFamily;
        public static string EnglishFontFamily => _resolvedFamily;
        public static string DefaultFontFamily => _resolvedFamily;
        public static string CurrentFontFamily => _resolvedFamily;

        private static string _resolvedFamily = "Hanuman";
        private static readonly PrivateFontCollection _privateFonts = new PrivateFontCollection();
        private static FontFamily _hanumanPrivateFamily = null;
        private static bool _hanumanLoaded = false;

        static FontHelper()
        {
            try
            {
                // 1. Try to load Hanuman TTF from local Fonts directory or Windows user font directory
                string[] searchDirs = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Fonts"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts"),
                    @"C:\Windows\Fonts"
                };

                string[] fontFiles = new[] { "Hanuman-Regular.ttf", "Hanuman-Bold.ttf" };

                foreach (var dir in searchDirs)
                {
                    try
                    {
                        if (Directory.Exists(dir))
                        {
                            foreach (var f in fontFiles)
                            {
                                string fullPath = Path.Combine(dir, f);
                                if (File.Exists(fullPath))
                                {
                                    try
                                    {
                                        _privateFonts.AddFontFile(fullPath);
                                        AddFontResourceEx(fullPath, FR_PRIVATE, IntPtr.Zero);
                                        _hanumanLoaded = true;
                                    }
                                    catch { }
                                }
                            }
                            if (_hanumanLoaded) break;
                        }
                    }
                    catch { }
                }

                // 2. Check if Hanuman is in PrivateFontCollection or System Families
                _hanumanPrivateFamily = _privateFonts.Families.FirstOrDefault(f => f.Name.Equals("Hanuman", StringComparison.OrdinalIgnoreCase));
                var installedFamilies = FontFamily.Families.Select(f => f.Name).ToList();

                if (_hanumanPrivateFamily != null || installedFamilies.Any(f => f.Equals("Hanuman", StringComparison.OrdinalIgnoreCase)))
                {
                    _resolvedFamily = "Hanuman";
                    _hanumanLoaded = true;
                }
                else
                {
                    // Fallback search if Hanuman is not found on machine
                    string[] candidates = new[]
                    {
                        "Khmer OS Battambang",
                        "Khmer OS Siemreap",
                        "Khmer OS",
                        "Leelawadee UI",
                        "Segoe UI",
                        "Roboto",
                        "Arial"
                    };

                    foreach (var c in candidates)
                    {
                        if (installedFamilies.Any(f => f.Equals(c, StringComparison.OrdinalIgnoreCase)))
                        {
                            _resolvedFamily = c;
                            break;
                        }
                    }
                }
            }
            catch
            {
                _resolvedFamily = "Hanuman";
            }
        }

        private static FontStyle SanitizeStyle(FontStyle style)
        {
            // Strictly remove Italic style across the application for crisp Khmer and English rendering
            return style & ~FontStyle.Italic;
        }

        public static Font CreateFont(float size, FontStyle style = FontStyle.Regular)
        {
            FontStyle safeStyle = SanitizeStyle(style);
            try
            {
                if (_hanumanPrivateFamily != null)
                {
                    return new Font(_hanumanPrivateFamily, size, safeStyle);
                }
                return new Font(_resolvedFamily, size, safeStyle);
            }
            catch
            {
                try
                {
                    return new Font(_resolvedFamily, size, safeStyle);
                }
                catch
                {
                    try
                    {
                        return new Font("Segoe UI", size, safeStyle);
                    }
                    catch
                    {
                        return SystemFonts.DefaultFont;
                    }
                }
            }
        }

        public static Font CreateEnglishFont(float size, FontStyle style = FontStyle.Regular)
        {
            return CreateFont(size, style);
        }

        public static Font CreateKhmerFont(float size, FontStyle style = FontStyle.Regular)
        {
            return CreateFont(size, style);
        }

        public static Font CreateFontForText(string text, float size, FontStyle style = FontStyle.Regular)
        {
            return CreateFont(size, style);
        }

        public static bool ContainsKhmer(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= '\u1780' && c <= '\u17FF')
                    return true;
            }
            return false;
        }

        public static void ApplyFontHierarchy(Control parent)
        {
            if (parent == null) return;
            try
            {
                string targetFamily = _resolvedFamily;
                if (parent.Font != null)
                {
                    FontStyle safeStyle = SanitizeStyle(parent.Font.Style);
                    if (!parent.Font.FontFamily.Name.Equals(targetFamily, StringComparison.OrdinalIgnoreCase) || parent.Font.Italic)
                    {
                        parent.Font = CreateFont(parent.Font.Size, safeStyle);
                    }
                }

                if (parent is DataGridView dgv)
                {
                    if (dgv.Font != null)
                        dgv.Font = CreateFont(dgv.Font.Size, SanitizeStyle(dgv.Font.Style));

                    if (dgv.ColumnHeadersDefaultCellStyle != null)
                    {
                        var cur = dgv.ColumnHeadersDefaultCellStyle.Font ?? dgv.Font;
                        if (cur != null)
                            dgv.ColumnHeadersDefaultCellStyle.Font = CreateFont(cur.Size, FontStyle.Bold);
                    }

                    if (dgv.DefaultCellStyle != null)
                    {
                        var cur = dgv.DefaultCellStyle.Font ?? dgv.Font;
                        if (cur != null)
                            dgv.DefaultCellStyle.Font = CreateFont(cur.Size, SanitizeStyle(cur.Style));
                    }
                }

                foreach (Control child in parent.Controls)
                {
                    ApplyFontHierarchy(child);
                }
            }
            catch { }
        }
    }
}
