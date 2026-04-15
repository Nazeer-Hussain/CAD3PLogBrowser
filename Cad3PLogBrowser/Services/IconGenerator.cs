using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Renders professional icons from the Windows-built-in Segoe MDL2 Assets font
    /// (the same icon set used by File Explorer, Settings, Taskbar on Windows 10/11).
    /// Falls back to Segoe UI Symbol if MDL2 is unavailable.
    /// All icons are rendered with ClearType + AntiAlias at the requested size.
    /// Codepoint reference: https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
    /// </summary>
    public static class IconGenerator
    {
        public enum IconSize { Small = 16, Medium = 24, Large = 32 }

        // ?? Font names (in preference order) ?????????????????????????????????
        private const string FluentFont = "Segoe Fluent Icons";   // Windows 11
        private const string Mdl2Font   = "Segoe MDL2 Assets";    // Windows 10
        private const string FallbackFont = "Segoe UI Symbol";    // older Windows

        // ?? Segoe MDL2 / Fluent codepoints ???????????????????????????????????
        // Reference: https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
        private const char IconOpen       = '\uE8E5'; // OpenFile
        private const char IconSave       = '\uE74E'; // Save
        private const char IconRefresh    = '\uE72C'; // Refresh
        private const char IconCopy       = '\uE8C8'; // Copy
        private const char IconFind       = '\uE721'; // Search
        private const char IconFilter     = '\uE71C'; // Filter
        private const char IconSettings   = '\uE713'; // Settings gear
        private const char IconHelp       = '\uE897'; // Help
        private const char IconExpand     = '\uE74B'; // Add / Plus
        private const char IconCollapse   = '\uE74D'; // Remove / Minus
        private const char IconTree       = '\uE9F9'; // Org / Hierarchy
        private const char IconExport     = '\uEDE1'; // Export
        private const char IconJump       = '\uE7C5'; // GoToStart (right arrow)
        private const char IconError      = '\uE783'; // ErrorBadge
        private const char IconWarning    = '\uE7BA'; // Warning triangle
        private const char IconStatusOk   = '\uE73E'; // Accept / tick
        private const char IconStatusLoad = '\uE9F5'; // Sync / Processing
        private const char IconStatusErr  = '\uEB90'; // ErrorCircle / blocked
        private const char IconCheck      = '\uE73E'; // Accept
        private const char IconCross      = '\uE894'; // Cancel / X
        private const char IconSun        = '\uE706'; // Brightness / Sun
        private const char IconMoon       = '\uE793'; // ClearNight / Moon

        // ?? Colour helpers ????????????????????????????????????????????????????
        private static bool Dark => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

        // Normal toolbar glyph colour — matches VS toolbar conventions
        private static Color GlyphColor    => Dark ? Color.FromArgb(208, 212, 220) : Color.FromArgb(50,  60,  80);
        private static Color AccentBlue    => Color.FromArgb(0,   122, 204);
        private static Color AccentGreen   => Color.FromArgb(16,  137,  62);
        private static Color AccentAmber   => Color.FromArgb(215, 140,  20);
        private static Color AccentRed     => Color.FromArgb(205,  40,  40);
        private static Color AccentTeal    => Color.FromArgb(0,   151, 135);
        private static Color StatusGreen   => Color.FromArgb(30,  170,  80);
        private static Color StatusAmber   => Color.FromArgb(225, 155,  10);
        private static Color StatusRed     => Color.FromArgb(200,  35,  35);

        // ?? Cached font name ??????????????????????????????????????????????????
        private static string _fontName;
        private static string FontName
        {
            get
            {
                if (_fontName != null) return _fontName;
                // Pick the best available icon font
                foreach (var name in new[] { FluentFont, Mdl2Font, FallbackFont })
                {
                    using (var test = new Font(name, 12f))
                        if (test.Name == name) { _fontName = name; return name; }
                }
                _fontName = FallbackFont;
                return _fontName;
            }
        }

        // ?? Core renderer ?????????????????????????????????????????????????????
        /// <summary>
        /// Renders a single icon glyph from Segoe MDL2 / Fluent Icons at the given
        /// pixel size, centred on a transparent background.
        /// </summary>
        /// <param name="glyph">Unicode codepoint from the icon font.</param>
        /// <param name="sz">Target icon size.</param>
        /// <param name="color">Glyph fill colour.</param>
        /// <param name="glyphScale">
        /// Fraction of the bitmap the glyph should fill (0.0–1.0).
        /// 0.72 gives comfortable padding like VS toolbar icons.
        /// </param>
        private static Bitmap Render(char glyph, IconSize sz, Color color, float glyphScale = 0.72f)
        {
            int s = (int)sz;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Transparent);

                float fontSize = s * glyphScale;
                using (var font = new Font(FontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(color))
                {
                    var sf = new StringFormat
                    {
                        Alignment     = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        FormatFlags   = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
                    };
                    g.DrawString(glyph.ToString(), font, brush, s / 2f, s / 2f, sf);
                }
            }
            return bmp;
        }

        /// <summary>
        /// Renders a status-indicator icon: a solid-filled circle with a glyph inside.
        /// Used for the FileStatus toolbar label (green/amber/red).
        /// </summary>
        private static Bitmap RenderStatus(char glyph, IconSize sz, Color circleColor)
        {
            int s = (int)sz;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Transparent);

                // Filled circle background
                float p = s * 0.06f;
                using (var br = new SolidBrush(circleColor))
                    g.FillEllipse(br, p, p, s - p * 2, s - p * 2);

                // White glyph centred inside
                float fontSize = s * 0.58f;
                using (var font = new Font(FontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat
                    {
                        Alignment     = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        FormatFlags   = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
                    };
                    g.DrawString(glyph.ToString(), font, brush, s / 2f, s / 2f, sf);
                }
            }
            return bmp;
        }

        // ??????????????????????????????????????????????????????????????????????
        // TOOLBAR / MENU ICONS  — neutral glyph colour, matches VS conventions
        // ??????????????????????????????????????????????????????????????????????

        public static Bitmap CreateOpenIcon(IconSize sz)     => Render(IconOpen,     sz, AccentBlue);
        public static Bitmap CreateSaveIcon(IconSize sz)     => Render(IconSave,     sz, AccentBlue);
        public static Bitmap CreateRefreshIcon(IconSize sz)  => Render(IconRefresh,  sz, GlyphColor);
        public static Bitmap CreateCopyIcon(IconSize sz)     => Render(IconCopy,     sz, GlyphColor);
        public static Bitmap CreateFindIcon(IconSize sz)     => Render(IconFind,     sz, GlyphColor);
        public static Bitmap CreateFilterIcon(IconSize sz)   => Render(IconFilter,   sz, AccentBlue);
        public static Bitmap CreateSettingsIcon(IconSize sz) => Render(IconSettings, sz, GlyphColor);
        public static Bitmap CreateHelpIcon(IconSize sz)     => Render(IconHelp,     sz, AccentBlue);
        public static Bitmap CreateExpandIcon(IconSize sz)   => Render(IconExpand,   sz, GlyphColor);
        public static Bitmap CreateCollapseIcon(IconSize sz) => Render(IconCollapse, sz, GlyphColor);
        public static Bitmap CreateTreeIcon(IconSize sz)     => Render(IconTree,     sz, GlyphColor);
        public static Bitmap CreateExportIcon(IconSize sz)   => Render(IconExport,   sz, AccentGreen);
        public static Bitmap CreateJumpIcon(IconSize sz)     => Render(IconJump,     sz, GlyphColor);
        public static Bitmap CreateErrorIcon(IconSize sz)    => Render(IconError,    sz, AccentRed);
        public static Bitmap CreateWarningIcon(IconSize sz)  => Render(IconWarning,  sz, AccentAmber);

        // ??????????????????????????????????????????????????????????????????????
        // STATUS BAR INDICATOR ICONS  — coloured circle + white glyph inside
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Green — file loaded successfully.</summary>
        public static Bitmap CreateStatusOkIcon(IconSize sz)      => RenderStatus(IconStatusOk,   sz, StatusGreen);

        /// <summary>Amber — file loading / processing.</summary>
        public static Bitmap CreateStatusLoadingIcon(IconSize sz)  => RenderStatus(IconStatusLoad, sz, StatusAmber);

        /// <summary>Red — error / no file loaded.</summary>
        public static Bitmap CreateStatusErrorIcon(IconSize sz)    => RenderStatus(IconStatusErr,  sz, StatusRed);

        // ??????????????????????????????????????????????????????????????????????
        // TREE-NODE ICONS  — matched / unmatched API pairs
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Small teal circle with tick — matched ENTER/EXIT pair.</summary>
        public static Bitmap CreateCheckIcon(IconSize sz)  => RenderStatus(IconCheck, sz, AccentTeal);

        /// <summary>Small red circle with X — unmatched API call.</summary>
        public static Bitmap CreateCrossIcon(IconSize sz)  => RenderStatus(IconCross, sz, AccentRed);

        // ??????????????????????????????????????????????????????????????????????
        // THEME TOGGLE ICONS
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Sun glyph — click to switch to light theme.</summary>
        public static Bitmap CreateSunIcon(IconSize sz)  => Render(IconSun,  sz, Color.FromArgb(230, 185, 30), 0.78f);

        /// <summary>Moon glyph — click to switch to dark theme.</summary>
        public static Bitmap CreateMoonIcon(IconSize sz) => Render(IconMoon, sz, Color.FromArgb(140, 160, 220), 0.78f);

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
