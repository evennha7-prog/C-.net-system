using System;
using System.Drawing;

namespace assignment_code.UI
{
    public enum AppThemeMode
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        public static AppThemeMode CurrentTheme { get; private set; } = AppThemeMode.Light;

        public static event EventHandler ThemeChanged;

        public static void SetTheme(AppThemeMode theme)
        {
            if (CurrentTheme != theme)
            {
                CurrentTheme = theme;
                if (ThemeChanged != null)
                {
                    foreach (EventHandler handler in ThemeChanged.GetInvocationList())
                    {
                        try
                        {
                            handler(null, EventArgs.Empty);
                        }
                        catch { }
                    }
                }
            }
        }

        public static void ToggleTheme()
        {
            SetTheme(IsDark ? AppThemeMode.Light : AppThemeMode.Dark);
        }

        public static bool IsDark => CurrentTheme == AppThemeMode.Dark;

        public static Color Background => IsDark ? Color.FromArgb(17, 22, 37) : Color.FromArgb(245, 247, 251);
        public static Color CardBackground => IsDark ? Color.FromArgb(27, 34, 54) : Color.FromArgb(255, 255, 255);
        public static Color SidebarBackground => IsDark ? Color.FromArgb(24, 30, 48) : Color.FromArgb(255, 255, 255);
        public static Color BorderColor => IsDark ? Color.FromArgb(40, 50, 78) : Color.FromArgb(235, 240, 247);
        public static Color SubtleDivider => IsDark ? Color.FromArgb(35, 44, 69) : Color.FromArgb(240, 243, 248);

        public static Color TextPrimary => IsDark ? Color.FromArgb(245, 247, 255) : Color.FromArgb(30, 34, 56);
        public static Color TextSecondary => IsDark ? Color.FromArgb(154, 166, 191) : Color.FromArgb(140, 147, 164);
        public static Color TextMuted => IsDark ? Color.FromArgb(104, 117, 148) : Color.FromArgb(163, 174, 194);

        public static Color AccentBlue => IsDark ? Color.FromArgb(62, 114, 255) : Color.FromArgb(48, 98, 249);
        public static Color AccentBlueHover => IsDark ? Color.FromArgb(75, 125, 255) : Color.FromArgb(60, 110, 255);
        public static Color AccentBlueLight => IsDark ? Color.FromArgb(30, 43, 77) : Color.FromArgb(234, 240, 255);

        public static Color BarLight => IsDark ? Color.FromArgb(44, 59, 99) : Color.FromArgb(220, 230, 255);
        public static Color BarActive => AccentBlue;

        public static Color SuccessGreen => Color.FromArgb(16, 185, 129);
        public static Color SuccessGreenBg => IsDark ? Color.FromArgb(20, 56, 47) : Color.FromArgb(230, 249, 240);

        public static Color WarningYellow => Color.FromArgb(245, 158, 11);
        public static Color WarningYellowBg => IsDark ? Color.FromArgb(61, 46, 20) : Color.FromArgb(254, 243, 199);

        public static Color DangerRed => Color.FromArgb(239, 68, 68);
        public static Color DangerRedBg => IsDark ? Color.FromArgb(61, 27, 27) : Color.FromArgb(254, 226, 226);

        public static Color GridLineColor => IsDark ? Color.FromArgb(37, 46, 71) : Color.FromArgb(238, 242, 246);
        public static Color HoverBackground => IsDark ? Color.FromArgb(36, 46, 73) : Color.FromArgb(240, 244, 250);

        public static Color SearchBoxBackground => IsDark ? Color.FromArgb(21, 27, 43) : Color.FromArgb(245, 247, 251);
        public static Color SearchBoxBorder => IsDark ? Color.FromArgb(40, 51, 78) : Color.FromArgb(228, 233, 242);
        
        public static Color DarkTooltipBg => IsDark ? Color.FromArgb(11, 15, 25) : Color.FromArgb(30, 34, 56);
        public static Color ViewButtonBg => IsDark ? Color.FromArgb(35, 48, 85) : Color.FromArgb(235, 242, 255);
        public static Color ViewButtonText => AccentBlue;
    }
}
