using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using assignment_code.Services;

namespace assignment_code.UI
{
    public static class FontHelper
    {
        public static string KhmerFontFamily => _resolvedKhmerFamily;
        public static string EnglishFontFamily => _resolvedEnglishFamily;
        public static string DefaultFontFamily => _resolvedEnglishFamily;

        private static string _resolvedKhmerFamily = "Khmer OS Battambang";
        private static string _resolvedEnglishFamily = "Roboto";

        static FontHelper()
        {
            try
            {
                var families = FontFamily.Families.Select(f => f.Name).ToList();

                // 1. English Font Candidates (Primary: Roboto)
                string[] enCandidates = new[]
                {
                    "Roboto",
                    "Roboto Medium",
                    "Segoe UI",
                    "Inter",
                    "Arial"
                };

                foreach (var c in enCandidates)
                {
                    if (families.Any(f => f.Equals(c, StringComparison.OrdinalIgnoreCase)))
                    {
                        _resolvedEnglishFamily = c;
                        break;
                    }
                }

                // 2. Standard Unicode Khmer fonts installed on Windows
                string[] khCandidates = new[] 
                { 
                    "Khmer OS Battambang", 
                    "Khmer OS Siemreap", 
                    "Khmer OS", 
                    "Leelawadee UI", 
                    "Khmer UI", 
                    "Segoe UI",
                    "Kh Battambang"
                };

                foreach (var c in khCandidates)
                {
                    if (families.Any(f => f.Equals(c, StringComparison.OrdinalIgnoreCase)))
                    {
                        _resolvedKhmerFamily = c;
                        break;
                    }
                }
            }
            catch
            {
                _resolvedKhmerFamily = "Leelawadee UI";
                _resolvedEnglishFamily = "Roboto";
            }
        }

        public static string CurrentFontFamily
        {
            get
            {
                if (TranslationManager.CurrentLanguage == AppLanguage.Khmer)
                {
                    return _resolvedKhmerFamily;
                }
                return _resolvedEnglishFamily;
            }
        }

        private static FontStyle SanitizeStyle(FontStyle style)
        {
            // Strictly remove Italic style across the entire application
            return style & ~FontStyle.Italic;
        }

        public static Font CreateFont(float size, FontStyle style = FontStyle.Regular)
        {
            string family = CurrentFontFamily;
            FontStyle safeStyle = SanitizeStyle(style);
            try
            {
                return new Font(family, size, safeStyle);
            }
            catch
            {
                return new Font("Roboto", size, safeStyle);
            }
        }

        public static Font CreateEnglishFont(float size, FontStyle style = FontStyle.Regular)
        {
            FontStyle safeStyle = SanitizeStyle(style);
            try
            {
                return new Font(_resolvedEnglishFamily, size, safeStyle);
            }
            catch
            {
                return new Font("Roboto", size, safeStyle);
            }
        }

        public static Font CreateKhmerFont(float size, FontStyle style = FontStyle.Regular)
        {
            FontStyle safeStyle = SanitizeStyle(style);
            try
            {
                return new Font(_resolvedKhmerFamily, size, safeStyle);
            }
            catch
            {
                return new Font(_resolvedEnglishFamily, size, safeStyle);
            }
        }

        public static Font CreateFontForText(string text, float size, FontStyle style = FontStyle.Regular)
        {
            FontStyle safeStyle = SanitizeStyle(style);
            if (ContainsKhmer(text) || TranslationManager.CurrentLanguage == AppLanguage.Khmer)
            {
                return CreateKhmerFont(size, safeStyle);
            }
            return CreateEnglishFont(size, safeStyle);
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
                string targetFamily = CurrentFontFamily;
                if (parent.Font != null)
                {
                    FontStyle safeStyle = SanitizeStyle(parent.Font.Style);
                    if (!parent.Font.FontFamily.Name.Equals(targetFamily, StringComparison.OrdinalIgnoreCase) || parent.Font.Italic)
                    {
                        parent.Font = new Font(targetFamily, parent.Font.Size, safeStyle);
                    }
                }

                if (parent is DataGridView dgv)
                {
                    if (dgv.Font != null)
                        dgv.Font = new Font(targetFamily, dgv.Font.Size, SanitizeStyle(dgv.Font.Style));

                    if (dgv.ColumnHeadersDefaultCellStyle != null)
                    {
                        var cur = dgv.ColumnHeadersDefaultCellStyle.Font ?? dgv.Font;
                        if (cur != null)
                            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(targetFamily, cur.Size, FontStyle.Bold);
                    }

                    if (dgv.DefaultCellStyle != null)
                    {
                        var cur = dgv.DefaultCellStyle.Font ?? dgv.Font;
                        if (cur != null)
                            dgv.DefaultCellStyle.Font = new Font(targetFamily, cur.Size, SanitizeStyle(cur.Style));
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






































































































































































































































































