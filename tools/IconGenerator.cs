using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IconGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDir = @"D:\Projects\CAD3PLogBrowser\Cad3PLogBrowser\images_blue";

            // Create error navigation icons (red)
            CreatePrevErrorIcon(outputDir);
            CreateNextErrorIcon(outputDir);

            // Create warning navigation icons (amber/orange)
            CreatePrevWarningIcon(outputDir);
            CreateNextWarningIcon(outputDir);

            Console.WriteLine("Icons created successfully!");
        }

        static void CreatePrevErrorIcon(string dir)
        {
            using (var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Red circle background
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

                // Red circle background
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

                // Orange/amber triangle background
                using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                {
                    Point[] triangle = new Point[] {
                        new Point(10, 2),
                        new Point(17, 17),
                        new Point(3, 17)
                    };
                    g.FillPolygon(brush, triangle);
                }

                // White left arrow
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

                // Orange/amber triangle background
                using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                {
                    Point[] triangle = new Point[] {
                        new Point(10, 2),
                        new Point(17, 17),
                        new Point(3, 17)
                    };
                    g.FillPolygon(brush, triangle);
                }

                // White right arrow
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
    }
}
