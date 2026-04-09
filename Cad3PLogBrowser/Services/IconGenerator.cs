using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Generates modern flat icons for the application toolbar.
    /// Supports multiple sizes (Small=16x16, Medium=24x24, Large=32x32).
    /// </summary>
    public static class IconGenerator
    {
        public enum IconSize
        {
            Small = 16,   // 16x16
            Medium = 24,  // 24x24
            Large = 32    // 32x32
        }

        private static Color GetThemeAccentColor()
        {
            // Use a universal blue that works in both themes
            return Color.FromArgb(0, 122, 204);  // VS Blue
        }

        private static Color GetThemeForegroundColor()
        {
            // This will be determined by context, use neutral dark
            return Color.FromArgb(40, 40, 40);
        }

        private static Color GetThemeBackgroundColor()
        {
            return Color.Transparent;
        }

        // ?? Icon Generation Methods ??????????????????????????????????????????

        public static Bitmap CreateOpenIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 8;

                // Folder shape
                using (var brush = new SolidBrush(accentColor))
                using (var pen = new Pen(accentColor, Math.Max(1, s / 16f)))
                {
                    // Folder body
                    var rect = new Rectangle(padding, padding + s / 4, s - padding * 2, s - padding - s / 4);
                    g.FillRectangle(brush, rect);

                    // Folder tab
                    var tabRect = new Rectangle(padding, padding, s / 2, s / 5);
                    g.FillRectangle(brush, tabRect);
                }
            }
            return bmp;
        }

        public static Bitmap CreateSaveIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 8;

                // Floppy disk shape
                using (var brush = new SolidBrush(accentColor))
                using (var whiteBrush = new SolidBrush(Color.White))
                {
                    // Main body
                    var rect = new Rectangle(padding, padding, s - padding * 2, s - padding * 2);
                    g.FillRectangle(brush, rect);

                    // Label area (white rectangle at bottom)
                    var labelRect = new Rectangle(padding * 2, s - padding * 4, s - padding * 4, s / 4);
                    g.FillRectangle(whiteBrush, labelRect);

                    // Save button/notch at top
                    var notchRect = new Rectangle(s - padding * 3, padding, padding * 2, s / 3);
                    g.FillRectangle(whiteBrush, notchRect);
                }
            }
            return bmp;
        }

        public static Bitmap CreateRefreshIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 6;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 8f)))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.ArrowAnchor;
                    pen.CustomEndCap = new AdjustableArrowCap(s / 8f, s / 8f);

                    // Circular arrow
                    var rect = new Rectangle(padding, padding, s - padding * 2, s - padding * 2);
                    g.DrawArc(pen, rect, -45, 270);
                }
            }
            return bmp;
        }

        public static Bitmap CreateCopyIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 6;
                int offset = s / 5;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 12f)))
                {
                    // Back rectangle
                    g.DrawRectangle(pen, padding, padding + offset, s - padding * 2 - offset, s - padding * 2 - offset);

                    // Front rectangle (offset)
                    g.DrawRectangle(pen, padding + offset, padding, s - padding * 2 - offset, s - padding * 2 - offset);
                }
            }
            return bmp;
        }

        public static Bitmap CreateFindIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 6;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                {
                    // Magnifying glass circle
                    int circleSize = s * 2 / 3;
                    g.DrawEllipse(pen, padding, padding, circleSize, circleSize);

                    // Handle
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    int handleStart = padding + circleSize * 7 / 10;
                    g.DrawLine(pen, handleStart, handleStart, s - padding, s - padding);
                }
            }
            return bmp;
        }

        public static Bitmap CreateFilterIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 8;

                using (var brush = new SolidBrush(accentColor))
                {
                    // Funnel shape
                    var points = new Point[]
                    {
                        new Point(padding, padding),
                        new Point(s - padding, padding),
                        new Point(s * 2 / 3, s / 2),
                        new Point(s * 2 / 3, s - padding),
                        new Point(s / 3, s - padding),
                        new Point(s / 3, s / 2)
                    };
                    g.FillPolygon(brush, points);
                }
            }
            return bmp;
        }

        public static Bitmap CreateSettingsIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int centerX = s / 2;
                int centerY = s / 2;
                int outerRadius = s * 4 / 10;
                int innerRadius = s / 5;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                using (var brush = new SolidBrush(accentColor))
                {
                    // Gear teeth (8 teeth)
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = i * Math.PI / 4;
                        int x1 = centerX + (int)(outerRadius * 0.7 * Math.Cos(angle));
                        int y1 = centerY + (int)(outerRadius * 0.7 * Math.Sin(angle));
                        int x2 = centerX + (int)(outerRadius * Math.Cos(angle));
                        int y2 = centerY + (int)(outerRadius * Math.Sin(angle));
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }

                    // Center circle
                    g.FillEllipse(brush, centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2);
                }
            }
            return bmp;
        }

        public static Bitmap CreateHelpIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 8;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                {
                    // Circle
                    g.DrawEllipse(pen, padding, padding, s - padding * 2, s - padding * 2);

                    // Question mark
                    using (var font = new Font("Segoe UI", s * 0.5f, FontStyle.Bold))
                    using (var brush = new SolidBrush(accentColor))
                    {
                        var format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("?", font, brush, s / 2f, s / 2f, format);
                    }
                }
            }
            return bmp;
        }

        public static Bitmap CreateExpandIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 6;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    // Plus sign
                    int center = s / 2;
                    int lineLength = s - padding * 2;

                    // Horizontal line
                    g.DrawLine(pen, padding, center, s - padding, center);

                    // Vertical line
                    g.DrawLine(pen, center, padding, center, s - padding);
                }
            }
            return bmp;
        }

        public static Bitmap CreateCollapseIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 6;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    // Minus sign (horizontal line)
                    int center = s / 2;
                    g.DrawLine(pen, padding, center, s - padding, center);
                }
            }
            return bmp;
        }

        public static Bitmap CreateTreeIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 8;

                using (var pen = new Pen(accentColor, Math.Max(1, s / 12f)))
                using (var brush = new SolidBrush(accentColor))
                {
                    // Tree structure
                    int nodeSize = Math.Max(3, s / 8);

                    // Root node
                    g.FillEllipse(brush, padding, padding, nodeSize, nodeSize);

                    // Child nodes
                    int childY = s / 2;
                    int child1X = s / 3;
                    int child2X = s * 2 / 3;

                    g.FillEllipse(brush, child1X - nodeSize / 2, childY - nodeSize / 2, nodeSize, nodeSize);
                    g.FillEllipse(brush, child2X - nodeSize / 2, childY - nodeSize / 2, nodeSize, nodeSize);

                    // Grandchild nodes
                    int grandChildY = s - padding - nodeSize;
                    g.FillEllipse(brush, padding * 2, grandChildY, nodeSize, nodeSize);
                    g.FillEllipse(brush, s / 2 - nodeSize / 2, grandChildY, nodeSize, nodeSize);
                    g.FillEllipse(brush, s - padding * 2 - nodeSize, grandChildY, nodeSize, nodeSize);

                    // Connecting lines
                    g.DrawLine(pen, padding + nodeSize / 2, padding + nodeSize, child1X, childY - nodeSize / 2);
                    g.DrawLine(pen, padding + nodeSize / 2, padding + nodeSize, child2X, childY - nodeSize / 2);
                    g.DrawLine(pen, child1X, childY + nodeSize / 2, padding * 2 + nodeSize / 2, grandChildY);
                    g.DrawLine(pen, child1X, childY + nodeSize / 2, s / 2, grandChildY);
                    g.DrawLine(pen, child2X, childY + nodeSize / 2, s - padding * 2 - nodeSize / 2, grandChildY);
                }
            }
            return bmp;
        }

        public static Bitmap CreateExportIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = Color.FromArgb(34, 139, 34); // Green for export
                int padding = s / 8;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                using (var brush = new SolidBrush(accentColor))
                {
                    // Document outline
                    g.DrawRectangle(pen, padding, padding, s - padding * 2, s - padding * 2);

                    // Arrow pointing out (up and right)
                    int arrowX = s * 2 / 3;
                    int arrowY = s / 3;
                    int arrowSize = s / 4;

                    pen.CustomEndCap = new AdjustableArrowCap(s / 10f, s / 10f);
                    g.DrawLine(pen, s / 2, s / 2, arrowX + arrowSize, arrowY - arrowSize);
                }
            }
            return bmp;
        }

        public static Bitmap CreateJumpIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var accentColor = GetThemeAccentColor();
                int padding = s / 6;

                using (var pen = new Pen(accentColor, Math.Max(2, s / 10f)))
                {
                    pen.StartCap = LineCap.ArrowAnchor;
                    pen.EndCap = LineCap.ArrowAnchor;
                    pen.CustomStartCap = new AdjustableArrowCap(s / 12f, s / 12f);
                    pen.CustomEndCap = new AdjustableArrowCap(s / 12f, s / 12f);

                    // Double-headed arrow (horizontal)
                    int centerY = s / 2;
                    g.DrawLine(pen, padding, centerY, s - padding, centerY);
                }
            }
            return bmp;
        }

        public static Bitmap CreateErrorIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var errorColor = Color.FromArgb(220, 50, 50); // Red
                int padding = s / 8;

                using (var pen = new Pen(errorColor, Math.Max(2, s / 10f)))
                using (var brush = new SolidBrush(errorColor))
                {
                    // Circle
                    g.DrawEllipse(pen, padding, padding, s - padding * 2, s - padding * 2);

                    // X or exclamation mark
                    using (var font = new Font("Segoe UI", s * 0.5f, FontStyle.Bold))
                    {
                        var format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("!", font, brush, s / 2f, s / 2f, format);
                    }
                }
            }
            return bmp;
        }

        public static Bitmap CreateWarningIcon(IconSize size)
        {
            int s = (int)size;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var warningColor = Color.FromArgb(255, 165, 0); // Orange
                int padding = s / 8;

                using (var brush = new SolidBrush(warningColor))
                {
                    // Triangle
                    var points = new Point[]
                    {
                        new Point(s / 2, padding),
                        new Point(s - padding, s - padding),
                        new Point(padding, s - padding)
                    };
                    g.FillPolygon(brush, points);

                    // Exclamation mark
                    using (var font = new Font("Segoe UI", s * 0.4f, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.Black))
                    {
                        var format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("!", font, textBrush, s / 2f, s * 0.6f, format);
                    }
                }
            }
            return bmp;
        }

        // ?? Helper method to create all icons at once ????????????????????????
        public static void GenerateAllIcons(IconSize size, out Bitmap open, out Bitmap save, 
            out Bitmap refresh, out Bitmap copy, out Bitmap find, out Bitmap filter,
            out Bitmap settings, out Bitmap help, out Bitmap expand, out Bitmap collapse,
            out Bitmap tree, out Bitmap exportIcon, out Bitmap jump, out Bitmap error,
            out Bitmap warning)
        {
            open = CreateOpenIcon(size);
            save = CreateSaveIcon(size);
            refresh = CreateRefreshIcon(size);
            copy = CreateCopyIcon(size);
            find = CreateFindIcon(size);
            filter = CreateFilterIcon(size);
            settings = CreateSettingsIcon(size);
            help = CreateHelpIcon(size);
            expand = CreateExpandIcon(size);
            collapse = CreateCollapseIcon(size);
            tree = CreateTreeIcon(size);
            exportIcon = CreateExportIcon(size);
            jump = CreateJumpIcon(size);
            error = CreateErrorIcon(size);
            warning = CreateWarningIcon(size);
        }
    }
}
