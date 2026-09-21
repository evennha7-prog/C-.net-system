using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace assignment_code.UI
{
    public static class GraphicsHelper
    {
        private static readonly Dictionary<string, Image> _flagCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image GetFlagImage(string langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode)) return null;
            string key = langCode.Trim().ToLowerInvariant();
            if (key == "english") key = "en";
            if (key == "khmer") key = "kh";

            if (_flagCache.TryGetValue(key, out Image cached) && cached != null)
            {
                return cached;
            }

            string asmDir = "";
            try { asmDir = Path.GetDirectoryName(typeof(GraphicsHelper).Assembly.Location) ?? ""; } catch { }

            string[] candidatePaths = new[]
            {
                Path.Combine(asmDir, "icons", $"{key}.png"),
                Path.Combine(asmDir, "icons", (key == "en" ? "uk.png" : $"{key}.png")),
                @"D:\PCCFP Institute\ppccfpi_files\cd-cs\assignment_code\assignment_code\icons\" + $"{key}.png",
                @"D:\PCCFP Institute\ppccfpi_files\cd-cs\assignment_code\assignment_code\icons\" + (key == "en" ? "uk.png" : $"{key}.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons", $"{key}.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons", (key == "en" ? "uk.png" : $"{key}.png")),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "icons", $"{key}.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "icons", (key == "en" ? "uk.png" : $"{key}.png")),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assignment_code", "icons", $"{key}.png")
            };

            foreach (var p in candidatePaths)
            {
                try
                {
                    if (File.Exists(p))
                    {
                        using (var temp = Image.FromFile(p))
                        {
                            var bitmap = new Bitmap(temp);
                            _flagCache[key] = bitmap;
                            return bitmap;
                        }
                    }
                }
                catch { }
            }

            return null;
        }

        public static void DrawFlag(Graphics g, Rectangle destRect, string langCode)
        {
            var flagImg = GetFlagImage(langCode);
            if (flagImg != null)
            {
                SetHighQuality(g);
                g.DrawImage(flagImg, destRect);
            }
            else
            {
                // Fallback to stylized globe if image not on disk
                DrawIcon(g, "globe", destRect, Color.White, 1.4f);
            }
        }

        private static Image _pccfpiLogoImage = null;

        public static Image GetPccfpiLogoImage()
        {
            if (_pccfpiLogoImage != null) return _pccfpiLogoImage;

            string[] candidatePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons", "pccfpi.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "icons", "pccfpi.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assignment_code", "icons", "pccfpi.png"),
                @"D:\PCCFP Institute\ppccfpi_files\cd-cs\assignment_code\assignment_code\icons\pccfpi.png"
            };

            foreach (var p in candidatePaths)
            {
                try
                {
                    if (File.Exists(p))
                    {
                        using (var temp = Image.FromFile(p))
                        {
                            _pccfpiLogoImage = new Bitmap(temp);
                            return _pccfpiLogoImage;
                        }
                    }
                }
                catch { }
            }

            return null;
        }

        public static void DrawPccfpiLogo(Graphics g, Rectangle destRect)
        {
            var logo = GetPccfpiLogoImage();
            if (logo != null)
            {
                SetHighQuality(g);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(logo, destRect);
            }
            else
            {
                DrawLogo(g, destRect, ThemeManager.AccentBlue);
            }
        }

        public static GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static GraphicsPath GetTopRoundedRectanglePath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right corner straight down
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            path.CloseFigure();
            return path;
        }

        public static void SetHighQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }

        // Draw 9-dots Logo
        public static void DrawLogo(Graphics g, Rectangle bounds, Color dotColor)
        {
            SetHighQuality(g);
            int dotSize = Math.Max(3, bounds.Width / 5);
            int spacing = (bounds.Width - (3 * dotSize)) / 2;

            using (var brush = new SolidBrush(dotColor))
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        int x = bounds.X + col * (dotSize + spacing);
                        int y = bounds.Y + row * (dotSize + spacing);
                        g.FillEllipse(brush, x, y, dotSize, dotSize);
                    }
                }
            }
        }

        // Vector Icon Drawer
        public static void DrawIcon(Graphics g, string iconName, Rectangle bounds, Color color, float penWidth = 2.0f)
        {
            SetHighQuality(g);
            using (var pen = new Pen(color, penWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(color))
            {
                int x = bounds.X;
                int y = bounds.Y;
                int w = bounds.Width;
                int h = bounds.Height;

                switch (iconName.ToLowerInvariant())
                {
                    case "home":
                    case "dashboard":
                        // House icon
                        var housePoints = new PointF[]
                        {
                            new PointF(x + w / 2f, y + 2),
                            new PointF(x + w - 2, y + h * 0.45f),
                            new PointF(x + w - 2, y + h - 2),
                            new PointF(x + 2, y + h - 2),
                            new PointF(x + 2, y + h * 0.45f)
                        };
                        g.DrawPolygon(pen, housePoints);
                        // Door
                        g.DrawLine(pen, x + w * 0.38f, y + h - 2, x + w * 0.38f, y + h * 0.6f);
                        g.DrawLine(pen, x + w * 0.38f, y + h * 0.6f, x + w * 0.62f, y + h * 0.6f);
                        g.DrawLine(pen, x + w * 0.62f, y + h * 0.6f, x + w * 0.62f, y + h - 2);
                        break;

                    case "posts":
                    case "products":
                    case "product":
                    case "document":
                        // Product box / tag icon
                        g.DrawRectangle(pen, x + 3, y + 2, w - 6, h - 4);
                        g.DrawLine(pen, x + 6, y + 6, x + w - 6, y + 6);
                        g.DrawLine(pen, x + 6, y + 10, x + w - 6, y + 10);
                        g.DrawLine(pen, x + 6, y + 14, x + w * 0.6f, y + 14);
                        break;

                    case "folder":
                    case "categories":
                        // Folder icon
                        var folderPoints = new PointF[]
                        {
                            new PointF(x + 2, y + 4),
                            new PointF(x + w * 0.45f, y + 4),
                            new PointF(x + w * 0.55f, y + 7),
                            new PointF(x + w - 2, y + 7),
                            new PointF(x + w - 2, y + h - 3),
                            new PointF(x + 2, y + h - 3)
                        };
                        g.DrawPolygon(pen, folderPoints);
                        break;

                    case "store":
                    case "shop":
                        // Storefront building with awning
                        g.DrawRectangle(pen, x + 3, y + 8, w - 6, h - 10);
                        // Roof / awning
                        g.DrawLine(pen, x + 2, y + 8, x + w / 2f, y + 3);
                        g.DrawLine(pen, x + w / 2f, y + 3, x + w - 2, y + 8);
                        // Door
                        g.DrawRectangle(pen, x + w / 2f - 3, y + 10, 6, h - 12);
                        break;

                    case "media":
                    case "image":
                    case "order":
                    case "orders":
                    case "shoppingbag":
                    case "bag":
                        // Shopping bag icon
                        g.DrawRectangle(pen, x + 3, y + 6, w - 6, h - 8);
                        g.DrawArc(pen, x + w / 2f - 4, y + 2, 8, 8, 180, 180);
                        g.DrawLine(pen, x + 7, y + 10, x + 7, y + 13);
                        g.DrawLine(pen, x + w - 7, y + 10, x + w - 7, y + 13);
                        break;

                    case "cart":
                    case "shoppingcart":
                        // Shopping cart icon
                        g.DrawLine(pen, x + 2, y + 4, x + 5, y + 4);
                        g.DrawLine(pen, x + 5, y + 4, x + 7, y + h - 8);
                        g.DrawLine(pen, x + 7, y + h - 8, x + w - 3, y + h - 8);
                        g.DrawLine(pen, x + w - 3, y + h - 8, x + w - 1, y + 7);
                        g.DrawLine(pen, x + w - 1, y + 7, x + 6, y + 7);
                        g.FillEllipse(brush, x + 6.5f, y + h - 6, 4, 4);
                        g.FillEllipse(brush, x + w - 5.5f, y + h - 6, 4, 4);
                        break;

                    case "sale":
                    case "sales":
                        // Price tag / discount badge
                        var tagPoints = new PointF[]
                        {
                            new PointF(x + 3, y + 4),
                            new PointF(x + w * 0.55f, y + 4),
                            new PointF(x + w - 3, y + h * 0.55f),
                            new PointF(x + w * 0.55f, y + h - 3),
                            new PointF(x + 3, y + h - 3)
                        };
                        g.DrawPolygon(pen, tagPoints);
                        g.FillEllipse(brush, x + 6, y + 7, 3, 3);
                        break;

                    case "pos":
                        // POS Terminal / Screen with keypad
                        g.DrawRectangle(pen, x + 2, y + 2, w - 4, h - 8);
                        g.DrawLine(pen, x + 4, y + 6, x + w - 4, y + 6);
                        g.DrawLine(pen, x + 5, y + 9, x + 8, y + 9);
                        g.DrawLine(pen, x + 10, y + 9, x + 13, y + 9);
                        g.DrawLine(pen, x + w / 2f - 3, y + h - 6, x + w / 2f + 3, y + h - 6);
                        g.DrawLine(pen, x + 4, y + h - 3, x + w - 4, y + h - 3);
                        break;

                    case "listsale":
                    case "receipt":
                        // List with lines
                        g.DrawRectangle(pen, x + 3, y + 2, w - 6, h - 4);
                        g.DrawLine(pen, x + 6, y + 6, x + w - 6, y + 6);
                        g.DrawLine(pen, x + 6, y + 10, x + w - 6, y + 10);
                        g.DrawLine(pen, x + 6, y + 14, x + w - 9, y + 14);
                        break;

                    case "dot":
                        // Subitem bullet dot
                        g.FillEllipse(brush, x + w / 2f - 2.5f, y + h / 2f - 2.5f, 5, 5);
                        break;

                    case "pages":
                    case "customer":
                    case "customers":
                        // Customer user profile icon
                        g.DrawEllipse(pen, x + w / 2f - 4, y + 2, 8, 8);
                        var userArc = new RectangleF(x + 2, y + 11, w - 4, 10);
                        g.DrawArc(pen, userArc, 180, 180);
                        break;

                    case "users":
                    case "user":
                        // Group of users icon
                        g.DrawEllipse(pen, x + 5, y + 3, 7, 7);
                        g.DrawArc(pen, x + 1, y + 11, 14, 9, 180, 180);
                        g.DrawEllipse(pen, x + w - 9, y + 2, 6, 6);
                        g.DrawArc(pen, x + w - 12, y + 9, 11, 8, 220, 130);
                        break;

                    case "comments":
                    case "chat":
                        // Chat bubble
                        var chatPoints = new PointF[]
                        {
                            new PointF(x + 2, y + 3),
                            new PointF(x + w - 2, y + 3),
                            new PointF(x + w - 2, y + h - 6),
                            new PointF(x + 9, y + h - 6),
                            new PointF(x + 4, y + h - 2),
                            new PointF(x + 5, y + h - 6),
                            new PointF(x + 2, y + h - 6)
                        };
                        g.DrawPolygon(pen, chatPoints);
                        break;

                    case "report":
                    case "reports":
                        // Report clipboard / graph icon
                        g.DrawRectangle(pen, x + 3, y + 4, w - 6, h - 6);
                        g.DrawLine(pen, x + 7, y + 8, x + w - 7, y + 8);
                        g.DrawLine(pen, x + 7, y + 11, x + w - 10, y + 11);
                        g.DrawLine(pen, x + 7, y + 14, x + w - 7, y + 14);
                        g.DrawArc(pen, x + w / 2f - 3, y + 1, 6, 5, 180, 180);
                        break;

                    case "appearance":
                    case "pen":
                        // Pen / brush edit icon
                        g.DrawLine(pen, x + 3, y + h - 3, x + 6, y + h - 6);
                        g.DrawLine(pen, x + 6, y + h - 6, x + w - 5, y + 5);
                        g.DrawLine(pen, x + w - 5, y + 5, x + w - 3, y + 7);
                        g.DrawLine(pen, x + w - 3, y + 7, x + 8, y + h - 4);
                        g.DrawLine(pen, x + 8, y + h - 4, x + 3, y + h - 3);
                        break;

                    case "settings":
                    case "gear":
                        // Gear cog icon
                        int gearR = Math.Max(4, w - 12);
                        g.DrawEllipse(pen, x + (w - gearR) / 2, y + (h - gearR) / 2, gearR, gearR);
                        for (int i = 0; i < 8; i++)
                        {
                            double angle = i * Math.PI / 4.0;
                            float cx = x + w / 2f;
                            float cy = y + h / 2f;
                            float r1 = gearR / 2f;
                            float r2 = r1 + 3f;
                            g.DrawLine(pen, cx + (float)(Math.Cos(angle) * r1), cy + (float)(Math.Sin(angle) * r1),
                                            cx + (float)(Math.Cos(angle) * r2), cy + (float)(Math.Sin(angle) * r2));
                        }
                        break;

                    case "globe":
                    case "lang":
                        // Globe icon
                        g.DrawEllipse(pen, x + 2, y + 2, w - 4, h - 4);
                        g.DrawLine(pen, x + 2, y + h / 2f, x + w - 2, y + h / 2f);
                        g.DrawEllipse(pen, x + w / 2f - (w - 4) / 4f, y + 2, (w - 4) / 2f, h - 4);
                        break;

                    case "search":
                        // Magnifying glass
                        int glassR = (int)(w * 0.45f);
                        g.DrawEllipse(pen, x + 3, y + 3, glassR, glassR);
                        g.DrawLine(pen, x + 3 + glassR - 1, y + 3 + glassR - 1, x + w - 3, y + h - 3);
                        break;

                    case "bell":
                    case "notification":
                    case "notifications":
                        // Real solid curved notification bell icon
                        using (var bellBody = new GraphicsPath())
                        {
                            float cx = x + w / 2f;
                            float topY = y + 2f;
                            float domeW = w * 0.46f;
                            float baseW = w * 0.76f;
                            float botY = y + h - 5.5f;

                            // Top small ring
                            g.FillEllipse(brush, cx - 1.5f, topY, 3f, 3f);

                            // Dome and flared bell body
                            bellBody.AddArc(cx - domeW / 2f, topY + 2.5f, domeW, domeW * 0.85f, 180, 180);
                            bellBody.AddBezier(
                                cx + domeW / 2f, topY + 2.5f + domeW * 0.42f,
                                cx + domeW / 2f + 1f, botY - 3f,
                                cx + baseW / 2f - 0.5f, botY - 1f,
                                cx + baseW / 2f, botY
                            );
                            bellBody.AddLine(cx + baseW / 2f, botY, cx - baseW / 2f, botY);
                            bellBody.AddBezier(
                                cx - baseW / 2f, botY,
                                cx - baseW / 2f + 0.5f, botY - 1f,
                                cx - domeW / 2f - 1f, botY - 3f,
                                cx - domeW / 2f, topY + 2.5f + domeW * 0.42f
                            );
                            bellBody.CloseFigure();
                            g.FillPath(brush, bellBody);

                            // Bottom clapper bead
                            g.FillEllipse(brush, cx - 2f, botY - 0.5f, 4f, 4f);
                        }
                        break;

                    case "sun":
                        // Sun icon
                        int sunR = Math.Max(4, w - 10);
                        g.DrawEllipse(pen, x + (w - sunR) / 2, y + (h - sunR) / 2, sunR, sunR);
                        for (int i = 0; i < 8; i++)
                        {
                            double angle = i * Math.PI / 4.0;
                            float cx = x + w / 2f;
                            float cy = y + h / 2f;
                            float r1 = sunR / 2f + 1f;
                            float r2 = r1 + 2.5f;
                            g.DrawLine(pen, cx + (float)(Math.Cos(angle) * r1), cy + (float)(Math.Sin(angle) * r1),
                                            cx + (float)(Math.Cos(angle) * r2), cy + (float)(Math.Sin(angle) * r2));
                        }
                        break;

                    case "moon":
                        // Moon crescent icon
                        var moonPath = new GraphicsPath();
                        moonPath.AddArc(x + 3, y + 3, w - 6, h - 6, 90, 270);
                        moonPath.AddArc(x + 7, y + 3, w - 10, h - 6, 270, -180);
                        moonPath.CloseFigure();
                        g.DrawPath(pen, moonPath);
                        break;

                    case "chevrondown":
                        // Down arrow
                        g.DrawLine(pen, x + 4, y + h * 0.38f, x + w / 2f, y + h * 0.62f);
                        g.DrawLine(pen, x + w / 2f, y + h * 0.62f, x + w - 4, y + h * 0.38f);
                        break;

                    case "chevronup":
                        // Up arrow
                        g.DrawLine(pen, x + 4, y + h * 0.62f, x + w / 2f, y + h * 0.38f);
                        g.DrawLine(pen, x + w / 2f, y + h * 0.38f, x + w - 4, y + h * 0.62f);
                        break;

                    case "chevronright":
                        // Right arrow
                        g.DrawLine(pen, x + w * 0.38f, y + 4, x + w * 0.62f, y + h / 2f);
                        g.DrawLine(pen, x + w * 0.62f, y + h / 2f, x + w * 0.38f, y + h - 4);
                        break;

                    case "refresh":
                    case "reload":
                    case "sync":
                        // Circular arrow
                        RectangleF circleRect = new RectangleF(x + 2, y + 2, w - 4, h - 4);
                        g.DrawArc(pen, circleRect, 45, 270);
                        float ax = x + w - 3;
                        float ay = y + h * 0.35f;
                        g.DrawLine(pen, ax, ay, ax, ay - 4);
                        g.DrawLine(pen, ax, ay, ax + 4, ay);
                        break;

                    case "menu":
                        // Hamburger 3 horizontal bars
                        g.DrawLine(pen, x + 3, y + h * 0.3f, x + w - 3, y + h * 0.3f);
                        g.DrawLine(pen, x + 3, y + h * 0.5f, x + w - 3, y + h * 0.5f);
                        g.DrawLine(pen, x + 3, y + h * 0.7f, x + w - 3, y + h * 0.7f);
                        break;

                    case "more":
                    case "dots":
                        // 3 horizontal dots
                        int dSize = 3;
                        g.FillEllipse(brush, x + w * 0.2f, y + h / 2f - dSize / 2f, dSize, dSize);
                        g.FillEllipse(brush, x + w * 0.5f - dSize / 2f, y + h / 2f - dSize / 2f, dSize, dSize);
                        g.FillEllipse(brush, x + w * 0.8f - dSize, y + h / 2f - dSize / 2f, dSize, dSize);
                        break;

                    case "wave":
                        // Waving hand icon (yellow/gold)
                        using (var goldBrush = new SolidBrush(Color.FromArgb(245, 185, 66)))
                        {
                            g.FillEllipse(goldBrush, x + 2, y + 4, w - 4, h - 5);
                            g.FillEllipse(goldBrush, x + w - 6, y + 1, 5, 7);
                            g.FillEllipse(goldBrush, x + w - 3, y + 3, 4, 6);
                        }
                        break;

                    case "fire":
                        // Flame icon (orange/red gradient)
                        using (var fireBrush = new SolidBrush(Color.FromArgb(255, 107, 53)))
                        {
                            var flamePoints = new PointF[]
                            {
                                new PointF(x + w / 2f, y + 1),
                                new PointF(x + w - 2, y + h * 0.55f),
                                new PointF(x + w * 0.75f, y + h - 1),
                                new PointF(x + w * 0.25f, y + h - 1),
                                new PointF(x + 2, y + h * 0.55f)
                            };
                            g.FillPolygon(fireBrush, flamePoints);
                        }
                        using (var innerFlameBrush = new SolidBrush(Color.FromArgb(255, 209, 102)))
                        {
                            g.FillEllipse(innerFlameBrush, x + w * 0.3f, y + h * 0.5f, w * 0.4f, h * 0.45f);
                        }
                        break;

                    case "logout":
                    case "exit":
                        // Door bracket: top, left, bottom
                        g.DrawLine(pen, x + w * 0.55f, y + 2, x + 3, y + 2);
                        g.DrawLine(pen, x + 3, y + 2, x + 3, y + h - 2);
                        g.DrawLine(pen, x + 3, y + h - 2, x + w * 0.55f, y + h - 2);
                        // Arrow pointing right
                        g.DrawLine(pen, x + w * 0.35f, y + h / 2f, x + w - 2, y + h / 2f);
                        g.DrawLine(pen, x + w - 5, y + h / 2f - 3, x + w - 2, y + h / 2f);
                        g.DrawLine(pen, x + w - 5, y + h / 2f + 3, x + w - 2, y + h / 2f);
                        break;

                    case "plus":
                    case "add":
                        g.DrawLine(pen, x + w / 2f, y + 3, x + w / 2f, y + h - 3);
                        g.DrawLine(pen, x + 3, y + h / 2f, x + w - 3, y + h / 2f);
                        break;

                    case "check":
                        g.DrawLine(pen, x + 3, y + h * 0.52f, x + w * 0.42f, y + h - 3);
                        g.DrawLine(pen, x + w * 0.42f, y + h - 3, x + w - 3, y + 3);
                        break;

                    case "filter":
                        var filterPoints = new PointF[]
                        {
                            new PointF(x + 2, y + 3),
                            new PointF(x + w - 2, y + 3),
                            new PointF(x + w * 0.6f, y + h * 0.55f),
                            new PointF(x + w * 0.6f, y + h - 2),
                            new PointF(x + w * 0.4f, y + h - 2),
                            new PointF(x + w * 0.4f, y + h * 0.55f)
                        };
                        g.DrawPolygon(pen, filterPoints);
                        break;

                    case "shield":
                        var shieldPoints = new PointF[]
                        {
                            new PointF(x + 3, y + 3),
                            new PointF(x + w - 3, y + 3),
                            new PointF(x + w - 3, y + h * 0.55f),
                            new PointF(x + w / 2f, y + h - 2),
                            new PointF(x + 3, y + h * 0.55f)
                        };
                        g.DrawPolygon(pen, shieldPoints);
                        break;

                    case "star":
                        var starPoints = new PointF[10];
                        float rx = w / 2f;
                        float ry = h / 2f;
                        float cxS = x + rx;
                        float cyS = y + ry;
                        for (int s = 0; s < 10; s++)
                        {
                            float r = (s % 2 == 0) ? Math.Min(rx, ry) - 2 : (Math.Min(rx, ry) - 2) * 0.48f;
                            double angle = s * Math.PI / 5.0 - Math.PI / 2.0;
                            starPoints[s] = new PointF(cxS + (float)(Math.Cos(angle) * r), cyS + (float)(Math.Sin(angle) * r));
                        }
                        g.DrawPolygon(pen, starPoints);
                        break;

                    case "dollar":
                    case "money":
                        g.DrawLine(pen, x + w / 2f, y + 2, x + w / 2f, y + h - 2);
                        g.DrawArc(pen, x + 4, y + 4, w - 8, (h - 8) / 2f, 90, 180);
                        g.DrawArc(pen, x + 4, y + h / 2f - 2, w - 8, (h - 8) / 2f, 270, 180);
                        break;

                    case "phone":
                        g.DrawRectangle(pen, x + 4, y + 2, w - 8, h - 4);
                        g.DrawLine(pen, x + 7, y + h - 5, x + w - 7, y + h - 5);
                        break;

                    case "mail":
                    case "email":
                    case "envelope":
                        // Envelope icon
                        g.DrawRectangle(pen, x + 2, y + 4, w - 4, h - 8);
                        g.DrawLine(pen, x + 2, y + 4, x + w / 2f, y + h / 2f + 1);
                        g.DrawLine(pen, x + w - 2, y + 4, x + w / 2f, y + h / 2f + 1);
                        break;

                    case "lock":
                        // Padlock icon
                        int shackleW = (int)(w * 0.44f);
                        int shackleH = (int)(h * 0.38f);
                        int shackleX = x + (w - shackleW) / 2;
                        int shackleY = y + 2;
                        g.DrawArc(pen, shackleX, shackleY, shackleW, shackleH * 2, 180, 180);
                        int bodyY = y + shackleH + 2;
                        int bodyH = h - bodyY - 2;
                        using (var lockPath = GetRoundedRectanglePath(new Rectangle(x + 3, bodyY, w - 6, bodyH), 3))
                        {
                            g.DrawPath(pen, lockPath);
                        }
                        g.DrawLine(pen, x + w / 2f, bodyY + 3, x + w / 2f, bodyY + bodyH - 4);
                        break;
                }
            }
        }

        public static void DrawAvatar(Graphics g, Rectangle bounds)
        {
            SetHighQuality(g);
            using (var brush = new LinearGradientBrush(bounds, Color.FromArgb(243, 144, 79), Color.FromArgb(59, 130, 246), LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(brush, bounds);
            }

            // Inner friendly stylized person silhouette
            using (var clipPath = new GraphicsPath())
            {
                clipPath.AddEllipse(bounds);
                g.SetClip(clipPath);

                using (var headBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
                {
                    int hw = bounds.Width * 38 / 100;
                    int hh = bounds.Height * 38 / 100;
                    int hx = bounds.X + (bounds.Width - hw) / 2;
                    int hy = bounds.Y + bounds.Height * 18 / 100;
                    g.FillEllipse(headBrush, hx, hy, hw, hh);

                    // Body
                    int bw = bounds.Width * 70 / 100;
                    int bh = bounds.Height * 50 / 100;
                    int bx = bounds.X + (bounds.Width - bw) / 2;
                    int by = bounds.Y + bounds.Height * 58 / 100;
                    g.FillEllipse(headBrush, bx, by, bw, bh);
                }

                g.ResetClip();
            }

            // Soft border
            using (var pen = new Pen(Color.FromArgb(220, 230, 245), 1.5f))
            {
                g.DrawEllipse(pen, bounds);
            }
        }

        private static readonly Color[] AvatarPalette = new[]
        {
            Color.FromArgb(99, 102, 241),  // Indigo
            Color.FromArgb(16, 185, 129),  // Emerald
            Color.FromArgb(245, 158, 11),  // Amber
            Color.FromArgb(239, 68, 68),   // Rose
            Color.FromArgb(139, 92, 246),  // Purple
            Color.FromArgb(14, 165, 233),  // Sky
            Color.FromArgb(236, 72, 153),  // Pink
            Color.FromArgb(20, 184, 166),  // Teal
        };

        public static void DrawInitialsAvatar(Graphics g, Rectangle bounds, string name, Color? customBg = null)
        {
            SetHighQuality(g);
            string initials = "??";
            if (!string.IsNullOrWhiteSpace(name))
            {
                var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    initials = $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
                }
                else if (parts.Length == 1 && parts[0].Length >= 2)
                {
                    initials = parts[0].Substring(0, 2).ToUpper();
                }
                else if (parts.Length == 1 && parts[0].Length == 1)
                {
                    initials = parts[0].ToUpper();
                }
            }

            int hash = Math.Abs((name ?? "").GetHashCode());
            Color bg = customBg ?? AvatarPalette[hash % AvatarPalette.Length];

            using (var brush = new SolidBrush(bg))
            {
                g.FillEllipse(brush, bounds);
            }

            // Subtle border
            using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1.2f))
            {
                g.DrawEllipse(pen, bounds);
            }

            using (var font = FontHelper.CreateFont(Math.Max(7.5f, bounds.Height * 0.36f), FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(initials, font, textBrush, bounds, sf);
            }
        }
    }
}
