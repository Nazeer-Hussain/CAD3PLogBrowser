using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Generates modern, crisp, theme-aware vector icons for toolbars, menus and status bars.
    /// All icons are rendered at 1x with anti-aliasing and clean geometric shapes.
    /// </summary>
    public static class IconGenerator
    {
        public enum IconSize { Small = 16, Medium = 24, Large = 32 }

        // ?? Theme colours ?????????????????????????????????????????????????????
        private static bool Dark => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

        private static Color Ink      => Dark ? Color.FromArgb(210, 215, 230) : Color.FromArgb(45,  55,  80);
        private static Color Accent   => Color.FromArgb(0,   122, 204);   // VS blue
        private static Color AccentAlt=> Color.FromArgb(16,  137, 62);    // green
        private static Color Warn     => Color.FromArgb(220, 140,  20);   // amber
        private static Color Danger   => Color.FromArgb(210,  45,  45);   // red
        private static Color Success  => Color.FromArgb(30,  170, 100);   // teal-green

        // ?? Convenience factory ???????????????????????????????????????????????
        private static Bitmap Make(IconSize sz, Action<Graphics, int> draw)
        {
            int s = (int)sz;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);
                draw(g, s);
            }
            return bmp;
        }

        private static float W(int s, float factor) => s * factor;
        private static Pen   P(Color c, int s, float f = 0.08f) => new Pen(c, Math.Max(1f, s * f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        private static Pen   PA(Color c, int s, float f = 0.08f)
        {
            var pen = P(c, s, f);
            pen.CustomEndCap = new AdjustableArrowCap(s * 0.18f, s * 0.18f, true);
            return pen;
        }

        // ?? Rounded-rect helper ???????????????????????????????????????????????
        private static GraphicsPath RRect(float x, float y, float w, float h, float r)
        {
            float d = r * 2;
            var p = new GraphicsPath();
            p.AddArc(x,     y,     d, d, 180, 90);
            p.AddArc(x+w-d, y,     d, d, 270, 90);
            p.AddArc(x+w-d, y+h-d, d, d,   0, 90);
            p.AddArc(x,     y+h-d, d, d,  90, 90);
            p.CloseFigure();
            return p;
        }

        // ??????????????????????????????????????????????????????????????????????
        // TOOLBAR ICONS
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Open folder icon — folder outline with opening tab.</summary>
        public static Bitmap CreateOpenIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s * 0.1f, t = s * 0.22f;
            using (var br = new SolidBrush(Accent))
            {
                // folder back panel
                using (var path = RRect(p, p + t, s - p*2, s - p - t, s*0.08f))
                    g.FillPath(br, path);
                // tab
                using (var path = RRect(p, p, s*0.45f, t + s*0.08f, s*0.08f))
                    g.FillPath(br, path);
                // arrow-up inside folder (open indicator)
                float ax = s * 0.58f, ay = s * 0.52f, aw = s * 0.26f;
                using (var wp = new Pen(Color.White, Math.Max(1.5f, s * 0.09f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(wp, ax, ay, ax, ay - aw);
                    g.DrawLine(wp, ax - aw*0.45f, ay - aw*0.5f, ax, ay - aw);
                    g.DrawLine(wp, ax + aw*0.45f, ay - aw*0.5f, ax, ay - aw);
                }
            }
        });

        /// <summary>Save icon — floppy disk with label area.</summary>
        public static Bitmap CreateSaveIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s * 0.08f, r = s * 0.1f;
            using (var body = RRect(p, p, s - p*2, s - p*2, r))
            using (var br = new SolidBrush(Accent))
                g.FillPath(br, body);
            // notch (top-right cut for write-protect)
            using (var nb = new SolidBrush(Color.Transparent))
            {
                float nx = s - p - s*0.22f, ny = p;
                var notch = new PointF[] {
                    new PointF(nx, ny), new PointF(s-p, ny), new PointF(s-p, ny + s*0.22f)
                };
                using (var eb = new SolidBrush(g.GetNearestColor(Color.FromArgb(0,0,0,0))))
                    g.FillPolygon(eb, notch);
            }
            // label band
            float ly = s*0.55f;
            using (var lb = new SolidBrush(Color.FromArgb(210, 235, 255)))
            using (var lp = RRect(p + s*0.1f, ly, s - p*2 - s*0.2f, s - p - ly, s*0.04f))
                g.FillPath(lb, lp);
            // shine
            using (var sh = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
            using (var sp = RRect(p+1, p+1, s-p*2-2, s*0.2f, r))
                g.FillPath(sh, sp);
        });

        /// <summary>Refresh/Reload icon — circular arrow.</summary>
        public static Bitmap CreateRefreshIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s * 0.12f;
            var rect = new RectangleF(p, p, s - p*2, s - p*2);
            using (var pen = new Pen(Accent, Math.Max(1.5f, s * 0.1f)) { StartCap = LineCap.Round })
            {
                pen.CustomEndCap = new AdjustableArrowCap(s * 0.2f, s * 0.2f, true);
                g.DrawArc(pen, rect, -50, 280);
            }
        });

        /// <summary>Copy icon — two overlapping document rectangles.</summary>
        public static Bitmap CreateCopyIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f, off = s*0.22f, r = s*0.08f;
            float w = s - p*2 - off, h = s - p*2 - off;
            // back
            using (var br = new SolidBrush(Color.FromArgb(Dark ? 70 : 180, Accent.R, Accent.G, Accent.B)))
            using (var path = RRect(p + off, p, w, h, r))
                g.FillPath(br, path);
            // front
            using (var br = new SolidBrush(Accent))
            using (var path = RRect(p, p + off, w, h, r))
                g.FillPath(br, path);
            // lines on front doc
            float lx1 = p + s*0.12f, lx2 = p + w - s*0.08f, ly = p + off + h*0.35f;
            using (var lp = new Pen(Color.White, Math.Max(1f, s*0.07f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(lp, lx1, ly,        lx2, ly);
                g.DrawLine(lp, lx1, ly+s*0.2f, lx2, ly+s*0.2f);
            }
        });

        /// <summary>Find/Search icon — magnifying glass.</summary>
        public static Bitmap CreateFindIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f, cs = s*0.55f;
            using (var pen = new Pen(Accent, Math.Max(1.5f, s*0.11f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawEllipse(pen, p, p, cs, cs);
                float hx = p + cs*0.72f, hy = p + cs*0.72f;
                g.DrawLine(pen, hx, hy, s*0.9f, s*0.9f);
            }
        });

        /// <summary>Filter icon — funnel.</summary>
        public static Bitmap CreateFilterIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.08f;
            var pts = new PointF[]
            {
                new PointF(p,        p),
                new PointF(s-p,      p),
                new PointF(s*0.63f,  s*0.52f),
                new PointF(s*0.63f,  s-p),
                new PointF(s*0.37f,  s-p),
                new PointF(s*0.37f,  s*0.52f),
            };
            using (var br = new SolidBrush(Accent))
                g.FillPolygon(br, pts);
        });

        /// <summary>Settings/Gear icon — cog wheel.</summary>
        public static Bitmap CreateSettingsIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float cx = s/2f, cy = s/2f;
            float outer = s*0.42f, inner = s*0.25f, tooth = s*0.12f;
            int teeth = 8;
            var outerPts = new PointF[teeth * 2];
            for (int i = 0; i < teeth; i++)
            {
                double a1 = 2*Math.PI*i/teeth - Math.PI/teeth/2;
                double a2 = a1 + Math.PI/teeth;
                outerPts[i*2]   = new PointF(cx + (float)((outer+tooth)*Math.Cos(a1)), cy + (float)((outer+tooth)*Math.Sin(a1)));
                outerPts[i*2+1] = new PointF(cx + (float)((outer)*Math.Cos(a2)),       cy + (float)((outer)*Math.Sin(a2)));
            }
            using (var br = new SolidBrush(Accent))
            {
                g.FillPolygon(br, outerPts);
                // hollow centre
                using (var ib = new SolidBrush(Color.Transparent))
                using (var eb = new SolidBrush(g.GetNearestColor(Color.FromArgb(0,0,0,0))))
                    g.FillEllipse(eb, cx-inner, cy-inner, inner*2, inner*2);
            }
            // ring outline for centre hole
            using (var pen = new Pen(Accent, Math.Max(1f, s*0.07f)))
                g.DrawEllipse(pen, cx-inner, cy-inner, inner*2, inner*2);
        });

        /// <summary>Help icon — ? in a circle.</summary>
        public static Bitmap CreateHelpIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.08f;
            using (var br = new SolidBrush(Accent))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            using (var font = new Font("Segoe UI", s*0.42f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var tb = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("?", font, tb, s/2f, s/2f, sf);
            }
        });

        /// <summary>Expand-all icon — plus with outward arrows.</summary>
        public static Bitmap CreateExpandIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.15f, c = s/2f;
            using (var pen = new Pen(Ink, Math.Max(1.5f, s*0.1f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, p, c, s-p, c);
                g.DrawLine(pen, c, p, c, s-p);
            }
        });

        /// <summary>Collapse-all icon — minus.</summary>
        public static Bitmap CreateCollapseIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.15f, c = s/2f;
            using (var pen = new Pen(Ink, Math.Max(1.5f, s*0.1f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(pen, p, c, s-p, c);
        });

        /// <summary>Tree/hierarchy icon — branching nodes.</summary>
        public static Bitmap CreateTreeIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.08f, ns = Math.Max(3f, s*0.2f);
            float rootX = s*0.25f, rootY = s*0.2f;
            float c1X = s*0.65f, c1Y = s*0.5f;
            float c2X = s*0.65f, c2Y = s*0.8f;
            using (var pen = new Pen(Accent, Math.Max(1f, s*0.08f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var br = new SolidBrush(Accent))
            {
                // trunk
                g.DrawLine(pen, rootX, rootY + ns/2, rootX, c2Y);
                // branches
                g.DrawLine(pen, rootX, c1Y, c1X - ns/2, c1Y);
                g.DrawLine(pen, rootX, c2Y, c2X - ns/2, c2Y);
                // nodes
                g.FillEllipse(br, rootX - ns/2, rootY - ns/2, ns, ns);
                g.FillEllipse(br, c1X  - ns/2, c1Y  - ns/2, ns, ns);
                g.FillEllipse(br, c2X  - ns/2, c2Y  - ns/2, ns, ns);
            }
        });

        /// <summary>Export icon — document with upward arrow.</summary>
        public static Bitmap CreateExportIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f, r = s*0.08f;
            float docW = s*0.55f, docH = s*0.7f;
            using (var br = new SolidBrush(Color.FromArgb(Dark?55:185, Accent.R, Accent.G, Accent.B)))
            using (var dp = RRect(p, p, docW, docH, r))
                g.FillPath(br, dp);
            // arrow up-right
            float ax = s*0.7f, ay = s*0.35f;
            using (var pen = new Pen(AccentAlt, Math.Max(1.5f, s*0.1f)) { StartCap = LineCap.Round })
            {
                pen.CustomEndCap = new AdjustableArrowCap(s*0.2f, s*0.2f, true);
                g.DrawLine(pen, ax - s*0.15f, ay + s*0.2f, ax + s*0.15f, ay - s*0.1f);
            }
        });

        /// <summary>Jump-to-line icon — right-pointing arrow entering a page.</summary>
        public static Bitmap CreateJumpIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f, c = s/2f;
            using (var pen = PA(Accent, s, 0.1f))
                g.DrawLine(pen, p, c, s-p, c);
        });

        /// <summary>Error nav icon — filled red circle with exclamation.</summary>
        public static Bitmap CreateErrorIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.08f;
            using (var br = new SolidBrush(Danger))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            using (var font = new Font("Segoe UI", s*0.45f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var tb = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("!", font, tb, s/2f, s/2f, sf);
            }
        });

        /// <summary>Warning nav icon — amber triangle with exclamation.</summary>
        public static Bitmap CreateWarningIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.06f;
            var tri = new PointF[] {
                new PointF(s/2f, p),
                new PointF(s-p,  s-p),
                new PointF(p,    s-p)
            };
            using (var br = new SolidBrush(Warn))
                g.FillPolygon(br, tri);
            using (var font = new Font("Segoe UI", s*0.38f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var tb = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("!", font, tb, s/2f, s*0.6f, sf);
            }
        });

        // ??????????????????????????????????????????????????????????????????????
        // STATUS BAR INDICATOR ICONS  (16x16 circles)
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Green circle — file loaded successfully.</summary>
        public static Bitmap CreateStatusOkIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f;
            // Gradient-filled circle
            using (var br = new LinearGradientBrush(
                new PointF(p, p), new PointF(s-p, s-p),
                Color.FromArgb(80, 210, 120), Color.FromArgb(20, 155, 75)))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            // Tick
            using (var pen = new Pen(Color.White, Math.Max(1.5f, s*0.12f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLines(pen, new PointF[] {
                    new PointF(s*0.28f, s*0.52f),
                    new PointF(s*0.44f, s*0.68f),
                    new PointF(s*0.72f, s*0.32f)
                });
        });

        /// <summary>Amber circle — file loading / processing.</summary>
        public static Bitmap CreateStatusLoadingIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f;
            using (var br = new LinearGradientBrush(
                new PointF(p, p), new PointF(s-p, s-p),
                Color.FromArgb(255, 210, 80), Color.FromArgb(215, 155, 20)))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            // hourglass dots
            using (var dp = new Pen(Color.White, Math.Max(1.5f, s*0.1f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(dp, p*2, p*2, s-p*4, s-p*4, -60, 210);
        });

        /// <summary>Red circle — error / no file.</summary>
        public static Bitmap CreateStatusErrorIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f;
            using (var br = new LinearGradientBrush(
                new PointF(p, p), new PointF(s-p, s-p),
                Color.FromArgb(240, 80, 80), Color.FromArgb(185, 35, 35)))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            // X
            float ip = s*0.28f;
            using (var xp = new Pen(Color.White, Math.Max(1.5f, s*0.12f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(xp, ip, ip, s-ip, s-ip);
                g.DrawLine(xp, s-ip, ip, ip, s-ip);
            }
        });

        // ??????????????????????????????????????????????????????????????????????
        // TREE-NODE CHECK ICONS  (for API tree matched/unmatched)
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Small green check — matched ENTER/EXIT pair.</summary>
        public static Bitmap CreateCheckIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f;
            using (var br = new SolidBrush(Success))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            using (var pen = new Pen(Color.White, Math.Max(1.5f, s*0.13f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLines(pen, new PointF[] {
                    new PointF(s*0.27f, s*0.52f),
                    new PointF(s*0.44f, s*0.70f),
                    new PointF(s*0.73f, s*0.30f)
                });
        });

        /// <summary>Small red X — unmatched API call.</summary>
        public static Bitmap CreateCrossIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s*0.1f;
            using (var br = new SolidBrush(Danger))
                g.FillEllipse(br, p, p, s-p*2, s-p*2);
            float ip = s*0.3f;
            using (var pen = new Pen(Color.White, Math.Max(1.5f, s*0.12f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, ip, ip, s-ip, s-ip);
                g.DrawLine(pen, s-ip, ip, ip, s-ip);
            }
        });

        // ??????????????????????????????????????????????????????????????????????
        // THEME TOGGLE ICONS  (sun = switch to light, moon = switch to dark)
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Sun icon — click to switch to light theme.</summary>
        public static Bitmap CreateSunIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float cx = s / 2f, cy = s / 2f;
            float cr = s * 0.22f;
            float rr = s * 0.42f;
            float rw = Math.Max(1.5f, s * 0.09f);
            Color sun = Color.FromArgb(250, 200, 40);
            using (var pen = new Pen(sun, rw) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4;
                    float x1 = cx + (float)((cr + s * 0.06f) * Math.Cos(a));
                    float y1 = cy + (float)((cr + s * 0.06f) * Math.Sin(a));
                    float x2 = cx + (float)(rr * Math.Cos(a));
                    float y2 = cy + (float)(rr * Math.Sin(a));
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            using (var br = new SolidBrush(sun))
                g.FillEllipse(br, cx - cr, cy - cr, cr * 2, cr * 2);
        });

        /// <summary>Moon icon — click to switch to dark theme.</summary>
        public static Bitmap CreateMoonIcon(IconSize sz) => Make(sz, (g, s) =>
        {
            float p = s * 0.08f;
            Color moon = Color.FromArgb(180, 200, 255);
            using (var br = new SolidBrush(moon))
                g.FillEllipse(br, p, p, s - p * 2, s - p * 2);
            float ox = s * 0.22f;
            var old = g.CompositingMode;
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            using (var cb = new SolidBrush(Color.FromArgb(0, 0, 0, 0)))
                g.FillEllipse(cb, p + ox, p - ox * 0.3f, s - p * 2, s - p * 2);
            g.CompositingMode = old;
        });

        /// <summary>Generates both theme-toggle icons in one call.</summary>
        public static void GenerateThemeIcons(IconSize sz, out Bitmap sunIcon, out Bitmap moonIcon)
        {
            sunIcon  = CreateSunIcon(sz);
            moonIcon = CreateMoonIcon(sz);
        }

        // ??????????????????????????????????????????????????????????????????????
        // BATCH GENERATOR  (called by MainForm.ApplyIconSize)
        // ??????????????????????????????????????????????????????????????????????

        public static void GenerateAllIcons(IconSize sz,
            out Bitmap openIcon,    out Bitmap saveIcon,    out Bitmap refreshIcon,
            out Bitmap copyIcon,    out Bitmap findIcon,    out Bitmap filterIcon,
            out Bitmap settingsIcon,out Bitmap helpIcon,    out Bitmap expandIcon,
            out Bitmap collapseIcon,out Bitmap treeIcon,    out Bitmap exportIcon,
            out Bitmap jumpIcon,    out Bitmap errorIcon,   out Bitmap warningIcon)
        {
            openIcon     = CreateOpenIcon(sz);
            saveIcon     = CreateSaveIcon(sz);
            refreshIcon  = CreateRefreshIcon(sz);
            copyIcon     = CreateCopyIcon(sz);
            findIcon     = CreateFindIcon(sz);
            filterIcon   = CreateFilterIcon(sz);
            settingsIcon = CreateSettingsIcon(sz);
            helpIcon     = CreateHelpIcon(sz);
            expandIcon   = CreateExpandIcon(sz);
            collapseIcon = CreateCollapseIcon(sz);
            treeIcon     = CreateTreeIcon(sz);
            exportIcon   = CreateExportIcon(sz);
            jumpIcon     = CreateJumpIcon(sz);
            errorIcon    = CreateErrorIcon(sz);
            warningIcon  = CreateWarningIcon(sz);
        }
    }
}
