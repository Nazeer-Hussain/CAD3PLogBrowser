using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ComprehensiveIconGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDir = @"D:\Projects\CAD3PLogBrowser\Cad3PLogBrowser\images_blue";

            Console.WriteLine("Generating 20x20px toolbar icons...");

            // File operations
            CreateOpenIcon(outputDir);
            CreateSaveIcon(outputDir);
            CreateRefreshIcon(outputDir);

            // Edit operations
            CreateCopyIcon(outputDir);
            CreateFindIcon(outputDir);
            CreateFilterIcon(outputDir);

            // Navigation icons
            CreatePrevErrorIcon(outputDir);
            CreateNextErrorIcon(outputDir);
            CreatePrevWarningIcon(outputDir);
            CreateNextWarningIcon(outputDir);

            // View operations
            CreateCallTreeIcon(outputDir);
            CreateApiTreeIcon(outputDir);
            CreateTabsIcon(outputDir);
            CreatePerformanceIcon(outputDir);
            CreateDetailsIcon(outputDir);
            CreateGraphIcon(outputDir);

            // Settings and Help
            CreateSettingsIcon(outputDir);
            CreateHelpIcon(outputDir);
            CreateToolsIcon(outputDir);

            // Additional icons
            CreateCheckIcon(outputDir);
            CreateCrossIcon(outputDir);
            CreateRemoveIcon(outputDir);

            Console.WriteLine("All icons generated successfully!");
            Console.WriteLine($"Location: {outputDir}");
        }

        // === FILE OPERATIONS ===

        static void CreateOpenIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Folder shape
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    // Folder back
                    g.FillRectangle(brush, 3, 7, 14, 10);
                    // Folder tab
                    Point[] tab = { new Point(3, 7), new Point(3, 5), new Point(9, 5), new Point(10, 7) };
                    g.FillPolygon(brush, tab);
                }

                // Folder outline
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 1f))
                {
                    g.DrawRectangle(pen, 3, 7, 14, 10);
                    g.DrawLines(pen, new Point[] { new Point(3, 7), new Point(3, 5), new Point(9, 5), new Point(10, 7) });
                }

                bmp.Save(System.IO.Path.Combine(dir, "open.png"), ImageFormat.Png);
            }
        }

        static void CreateSaveIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Floppy disk body
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    g.FillRectangle(brush, 4, 3, 12, 14);
                }

                // Top label area (darker)
                using (var brush = new SolidBrush(Color.FromArgb(33, 115, 176)))
                {
                    g.FillRectangle(brush, 4, 3, 12, 5);
                }

                // Metal plate (bottom)
                using (var brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                {
                    g.FillRectangle(brush, 4, 13, 12, 4);
                }

                // Notch
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, 6, 5, 3, 3);
                }

                bmp.Save(System.IO.Path.Combine(dir, "save.png"), ImageFormat.Png);
            }
        }

        static void CreateRefreshIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Circular arrow
                using (var pen = new Pen(Color.FromArgb(52, 144, 220), 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.ArrowAnchor;
                    g.DrawArc(pen, 4, 4, 12, 12, 45, 270);
                }

                // Arrow tip (additional emphasis)
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    Point[] arrow = { new Point(15, 3), new Point(17, 5), new Point(13, 5) };
                    g.FillPolygon(brush, arrow);
                }

                bmp.Save(System.IO.Path.Combine(dir, "refresh.png"), ImageFormat.Png);
            }
        }

        // === EDIT OPERATIONS ===

        static void CreateCopyIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Back document
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 1f))
                {
                    g.FillRectangle(brush, 7, 5, 9, 11);
                    g.DrawRectangle(pen, 7, 5, 9, 11);
                }

                // Front document
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 1f))
                {
                    g.FillRectangle(brush, 4, 3, 9, 11);
                    g.DrawRectangle(pen, 4, 3, 9, 11);
                }

                // Lines on front document
                using (var pen = new Pen(Color.White, 1f))
                {
                    g.DrawLine(pen, 6, 6, 11, 6);
                    g.DrawLine(pen, 6, 8, 11, 8);
                    g.DrawLine(pen, 6, 10, 11, 10);
                }

                bmp.Save(System.IO.Path.Combine(dir, "copy.png"), ImageFormat.Png);
            }
        }

        static void CreateFindIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Magnifying glass circle
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 2f))
                {
                    g.DrawEllipse(pen, 4, 4, 9, 9);
                }

                // Handle
                using (var pen = new Pen(Color.FromArgb(52, 144, 220), 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 11, 11, 16, 16);
                }

                bmp.Save(System.IO.Path.Combine(dir, "find.png"), ImageFormat.Png);
            }
        }

        static void CreateFilterIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Funnel shape
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    Point[] funnel = {
                        new Point(3, 4),
                        new Point(17, 4),
                        new Point(12, 10),
                        new Point(12, 17),
                        new Point(8, 17),
                        new Point(8, 10)
                    };
                    g.FillPolygon(brush, funnel);
                }

                // Outline
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 1f))
                {
                    Point[] funnel = {
                        new Point(3, 4),
                        new Point(17, 4),
                        new Point(12, 10),
                        new Point(12, 17),
                        new Point(8, 17),
                        new Point(8, 10),
                        new Point(3, 4)
                    };
                    g.DrawLines(pen, funnel);
                }

                bmp.Save(System.IO.Path.Combine(dir, "filter.png"), ImageFormat.Png);
            }
        }

        // === NAVIGATION ICONS ===

        static void CreatePrevErrorIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Red circle
                using (var brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
                {
                    g.FillEllipse(brush, 3, 3, 14, 14);
                }

                // White left arrow
                using (var pen = new Pen(Color.White, 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[] {
                        new Point(12, 6),
                        new Point(8, 10),
                        new Point(12, 14)
                    });
                }

                bmp.Save(System.IO.Path.Combine(dir, "error_prev.png"), ImageFormat.Png);
            }
        }

        static void CreateNextErrorIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Red circle
                using (var brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
                {
                    g.FillEllipse(brush, 3, 3, 14, 14);
                }

                // White right arrow
                using (var pen = new Pen(Color.White, 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[] {
                        new Point(8, 6),
                        new Point(12, 10),
                        new Point(8, 14)
                    });
                }

                bmp.Save(System.IO.Path.Combine(dir, "error_next.png"), ImageFormat.Png);
            }
        }

        static void CreatePrevWarningIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Amber triangle
                using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                {
                    Point[] triangle = {
                        new Point(10, 2),
                        new Point(17, 17),
                        new Point(3, 17)
                    };
                    g.FillPolygon(brush, triangle);
                }

                // Dark left arrow
                using (var pen = new Pen(Color.FromArgb(80, 50, 0), 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[] {
                        new Point(12, 7),
                        new Point(8, 10),
                        new Point(12, 13)
                    });
                }

                bmp.Save(System.IO.Path.Combine(dir, "warning_prev.png"), ImageFormat.Png);
            }
        }

        static void CreateNextWarningIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Amber triangle
                using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                {
                    Point[] triangle = {
                        new Point(10, 2),
                        new Point(17, 17),
                        new Point(3, 17)
                    };
                    g.FillPolygon(brush, triangle);
                }

                // Dark right arrow
                using (var pen = new Pen(Color.FromArgb(80, 50, 0), 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[] {
                        new Point(8, 7),
                        new Point(12, 10),
                        new Point(8, 13)
                    });
                }

                bmp.Save(System.IO.Path.Combine(dir, "warning_next.png"), ImageFormat.Png);
            }
        }

        // === VIEW ICONS ===

        static void CreateCallTreeIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(Color.FromArgb(52, 144, 220), 2f))
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    // Root node
                    g.FillRectangle(brush, 2, 2, 6, 4);

                    // Connecting lines
                    g.DrawLine(pen, 5, 6, 5, 10);
                    g.DrawLine(pen, 5, 10, 8, 10);
                    g.DrawLine(pen, 5, 10, 8, 14);

                    // Child nodes
                    g.FillRectangle(brush, 8, 8, 6, 4);
                    g.FillRectangle(brush, 8, 12, 6, 4);
                }

                bmp.Save(System.IO.Path.Combine(dir, "treeview.png"), ImageFormat.Png);
            }
        }

        static void CreateApiTreeIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    // API symbol - curly braces
                    using (var pen = new Pen(Color.FromArgb(52, 144, 220), 2f))
                    {
                        // Left brace
                        g.DrawArc(pen, 4, 4, 4, 6, 90, 180);
                        g.DrawLine(pen, 4, 7, 2, 7);
                        g.DrawArc(pen, 4, 10, 4, 6, 180, 180);

                        // Right brace
                        g.DrawArc(pen, 12, 4, 4, 6, -90, -180);
                        g.DrawLine(pen, 16, 7, 18, 7);
                        g.DrawArc(pen, 12, 10, 4, 6, 0, -180);
                    }
                }

                bmp.Save(System.IO.Path.Combine(dir, "apiview.png"), ImageFormat.Png);
            }
        }

        static void CreateTabsIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 1f))
                {
                    // Back tab
                    g.FillRectangle(brush, 10, 6, 6, 3);
                    g.DrawRectangle(pen, 10, 6, 6, 3);

                    // Front tab (active)
                    g.FillRectangle(brush, 4, 4, 6, 3);
                    g.DrawRectangle(pen, 4, 4, 6, 3);

                    // Content area
                    g.FillRectangle(brush, 4, 7, 12, 9);
                    g.DrawRectangle(pen, 4, 7, 12, 9);
                }

                bmp.Save(System.IO.Path.Combine(dir, "tabs.png"), ImageFormat.Png);
            }
        }

        static void CreatePerformanceIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Bar chart
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    g.FillRectangle(brush, 3, 11, 3, 6);
                    g.FillRectangle(brush, 7, 7, 3, 10);
                    g.FillRectangle(brush, 11, 4, 3, 13);
                    g.FillRectangle(brush, 15, 9, 3, 8);
                }

                bmp.Save(System.IO.Path.Combine(dir, "performance.png"), ImageFormat.Png);
            }
        }

        static void CreateDetailsIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Document with lines
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                using (var pen = new Pen(Color.FromArgb(33, 115, 176), 1f))
                {
                    g.FillRectangle(brush, 4, 3, 12, 14);
                    g.DrawRectangle(pen, 4, 3, 12, 14);
                }

                // Text lines
                using (var pen = new Pen(Color.White, 1f))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        g.DrawLine(pen, 6, 6 + (i * 2), 14, 6 + (i * 2));
                    }
                }

                bmp.Save(System.IO.Path.Combine(dir, "details.png"), ImageFormat.Png);
            }
        }

        static void CreateGraphIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Nodes
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    g.FillEllipse(brush, 3, 3, 5, 5);
                    g.FillEllipse(brush, 12, 3, 5, 5);
                    g.FillEllipse(brush, 3, 12, 5, 5);
                    g.FillEllipse(brush, 12, 12, 5, 5);
                }

                // Connecting lines
                using (var pen = new Pen(Color.FromArgb(52, 144, 220), 1.5f))
                {
                    g.DrawLine(pen, 8, 5, 12, 5);
                    g.DrawLine(pen, 5, 8, 5, 12);
                    g.DrawLine(pen, 8, 14, 12, 14);
                    g.DrawLine(pen, 14, 8, 14, 12);
                }

                bmp.Save(System.IO.Path.Combine(dir, "graph1.png"), ImageFormat.Png);
            }
        }

        // === SETTINGS AND HELP ===

        static void CreateSettingsIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Gear teeth
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    // Outer gear shape (simplified)
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = i * Math.PI / 4;
                        int x = 10 + (int)(7 * Math.Cos(angle)) - 1;
                        int y = 10 + (int)(7 * Math.Sin(angle)) - 1;
                        g.FillRectangle(brush, x, y, 2, 2);
                    }

                    // Inner circle
                    g.FillEllipse(brush, 6, 6, 8, 8);
                }

                // Center hole
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillEllipse(brush, 8, 8, 4, 4);
                }

                bmp.Save(System.IO.Path.Combine(dir, "settings.png"), ImageFormat.Png);
            }
        }

        static void CreateHelpIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Circle
                using (var brush = new SolidBrush(Color.FromArgb(52, 144, 220)))
                {
                    g.FillEllipse(brush, 3, 3, 14, 14);
                }

                // Question mark
                using (var font = new Font("Arial", 11, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("?", font, brush, new RectangleF(0, 0, 20, 20), sf);
                }

                bmp.Save(System.IO.Path.Combine(dir, "help.png"), ImageFormat.Png);
            }
        }

        static void CreateToolsIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Wrench
                using (var pen = new Pen(Color.FromArgb(52, 144, 220), 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    // Handle
                    g.DrawLine(pen, 5, 15, 10, 10);

                    // Head
                    g.DrawArc(pen, 9, 3, 6, 6, 180, 180);
                }

                // Screwdriver crossed
                using (var pen = new Pen(Color.FromArgb(52, 144, 220), 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 15, 5, 10, 10);
                }

                bmp.Save(System.IO.Path.Combine(dir, "tools.png"), ImageFormat.Png);
            }
        }

        // === ADDITIONAL ICONS ===

        static void CreateCheckIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Green checkmark
                using (var pen = new Pen(Color.FromArgb(40, 167, 69), 3f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[] {
                        new Point(4, 10),
                        new Point(8, 14),
                        new Point(16, 6)
                    });
                }

                bmp.Save(System.IO.Path.Combine(dir, "check1.png"), ImageFormat.Png);
            }
        }

        static void CreateCrossIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Red X
                using (var pen = new Pen(Color.FromArgb(220, 53, 69), 3f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 5, 5, 15, 15);
                    g.DrawLine(pen, 15, 5, 5, 15);
                }

                bmp.Save(System.IO.Path.Combine(dir, "cross.png"), ImageFormat.Png);
            }
        }

        static void CreateRemoveIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Trash can
                using (var brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
                using (var pen = new Pen(Color.FromArgb(180, 40, 55), 1f))
                {
                    // Can body
                    Point[] can = {
                        new Point(6, 8),
                        new Point(7, 16),
                        new Point(13, 16),
                        new Point(14, 8)
                    };
                    g.FillPolygon(brush, can);
                    g.DrawPolygon(pen, can);

                    // Lid
                    g.FillRectangle(brush, 5, 6, 10, 2);
                    g.DrawRectangle(pen, 5, 6, 10, 2);

                    // Handle
                    g.DrawArc(pen, 8, 3, 4, 4, 180, 180);
                }

                bmp.Save(System.IO.Path.Combine(dir, "remove.png"), ImageFormat.Png);
            }
        }
    }
}
