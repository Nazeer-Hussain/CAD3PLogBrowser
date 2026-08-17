﻿using System;
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
        // File menu
        private const char IconOpen            = '\uE8E5'; // OpenFile
        private const char IconSave            = '\uE74E'; // Save
        private const char IconExportFile      = '\uE8A7'; // SaveAs (generic file export)
        private const char IconExportCsv       = '\uE8F1'; // Page
        private const char IconExportImage     = '\uE722'; // Pictures (export image)
        private const char IconExportJson      = '\uE943'; // Code
        private const char IconExportXml       = '\uE8A5'; // Document
        private const char IconMergeLogs       = '\uE8F0'; // MergeCall
        private const char IconReload          = '\uE72C'; // Refresh
        private const char IconExit            = '\uE7E8'; // ChromeClose
        private const char IconCompareLogs     = '\uE8AA'; // OpenPane (side-by-side compare)
        private const char IconExportAnalytics = '\uE9D9'; // AnalyticsReport
        private const char IconExportApiCsv    = '\uE8F1'; // Page (CSV export, list-like)
        private const char IconExportCallGraph = '\uE8EE'; // Relationship
        private const char IconExportHeatmap   = '\uE9D2'; // Heatmap / grid
        private const char IconSetBaseline     = '\uE735'; // Flag / mark baseline
        private const char IconScreenshot      = '\uE722'; // Pictures (screenshot/capture)
        private const char IconExportXls       = '\uE9D9'; // ExcelDocument (Save Selected as XLS)
        // Edit menu
        private const char IconCopy            = '\uE8C8'; // Copy
        private const char IconCopyHeaders     = '\uE8CF'; // ClipboardList
        private const char IconCopyAllVisible  = '\uE8C8'; // Copy (visible-scope copy)
        private const char IconFind            = '\uE721'; // Search
        private const char IconFindNext        = '\uE893'; // ChevronRight
        private const char IconFindAll         = '\uE773'; // SearchAndApps
        private const char IconFilter          = '\uE71C'; // Filter
        private const char IconClearFilter     = '\uE77A'; // ClearFilter
        private const char IconExpand          = ''; // ExpandAll
        private const char IconCollapse        = ''; // CollapseAll
        private const char IconJumpMatch       = '\uE7C5'; // GoToStart
        private const char IconJumpLine        = '\uE8AB'; // GoToLine
        private const char IconBookmark        = '\uE8D3'; // Bookmark
        private const char IconBookmarkNext    = '\uE893'; // ChevronRight (next bookmark)
        private const char IconBookmarkPrev    = '\uE892'; // ChevronLeft (prev bookmark)
        private const char IconBookmarkShow    = '\uE8A4'; // ShowAll
        private const char IconBookmarkClear   = '\uE8D4'; // BookmarkOutline
        private const char IconExpandToLevel   = '\uE8BB'; // NumberedListText (expand to specific level)
        // View menu
        private const char IconCallTree        = '\uE8AC'; // Hierarchy
        private const char IconApiTree         = '\uE71D'; // SortLines (API list)
        private const char IconFont            = '\uE8D2'; // Font
        private const char IconToolbar         = '\uE700'; // GlobalNavButton
        private const char IconTab             = '\uE74C'; // Tab (generic / submenu header)
        private const char IconHideTabs        = '\uE71A'; // FullScreen (hide/show tabs)
        private const char IconWatchFile       = '\uE890'; // View (watch for changes)
        private const char IconStatusBar       = '\uE73A'; // AlignBottom (status bar)
        // Per-tab icons
        private const char IconTabLog          = '\uE8F1'; // Page / document list  - Log view
        private const char IconTabRaw          = '\uE8A5'; // Document              - Raw text view
        private const char IconTabPerformance  = '\uE7EF'; // Org chart             ? Performance
        private const char IconTabLogDetails   = '\uE8A5'; // Document detail       ? Log Details
        private const char IconTabCallGraph    = '\uE8EE'; // Relationship          ? Call Graph
        private const char IconTabFlameGraph   = '\uE7C1'; // BulletedList2 (stacked bars)
        private const char IconTabTimeline     = '\uE81C'; // Timeline / clock      ? Timeline
        private const char IconTabAI           = '\uE8BD'; // Chat / message        ? AI Assistant
        private const char IconTabHeatmap      = '\uE9D2'; // Heatmap / grid        ? Heatmap tab
        // Navigation buttons
        private const char IconPrevError       = '\uE892'; // ChevronLeft
        private const char IconNextError       = '\uE893'; // ChevronRight
        private const char IconPrevWarning     = '\uE892'; // ChevronLeft (prev warning)
        private const char IconNextWarning     = '\uE893'; // ChevronRight (next warning)
        // Help menu
        private const char IconHelp            = '\uE897'; // Help
        private const char IconKeyboard        = '\uE765'; // Keyboard
        private const char IconAbout           = '\uE946'; // Info
        private const char IconUpdate          = '\uE895'; // Sync
        private const char IconReport          = '\uE730'; // ReportDocument
        private const char IconUpdateLog       = '\uE81C'; // Timeline / log history
        private const char IconProjectPage     = '\uE774'; // WebSearch (external link)
        // Tree context menu
        private const char IconSaveBranch      = '\uE78C'; // SaveLocal
        private const char IconGrok            = '\uE774'; // WebSearch
        private const char IconShowInTree      = '\uE8B0'; // ShowResults
        private const char IconSettings        = '\uE713'; // Settings gear
        private const char IconAskAi           = '\uE8BD'; // Chat / message (Ask AI)
        private const char IconRootCause       = '\uE9D9'; // AnalyticsReport (root cause analysis)
        // Line/context menu
        private const char IconInspectLine     = '\uE946'; // Info (inspect line)
        private const char IconOpenInEditor    = '\uE8E5'; // OpenFile (open in editor)
        private const char IconToggleBookmark  = '\uE8D3'; // Bookmark
        // Status bar indicators
        private const char IconStatusOk        = '\uE73E'; // Accept
        private const char IconStatusLoad      = '\uE72C'; // Refresh (loading indicator)
        private const char IconStatusErr       = '\uE783'; // ErrorBadge (error indicator)
        // Tree node state
        private const char IconCheck           = '\uE73E'; // Accept
        private const char IconCross           = '\uE894'; // Cancel
        // Navigation error/warning
        private const char IconError           = '\uE783'; // ErrorBadge
        private const char IconWarning         = '\uE7BA'; // Warning triangle
        // Theme toggle
        private const char IconSun             = '\uE706'; // Brightness
        private const char IconMoon            = '\uE793'; // ClearNight

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
        /// <summary>Orange — file changed on disk, reload required.</summary>
        private static Color StatusOrange  => Color.FromArgb(220, 100,   0);

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
        // FILE MENU
        // ??????????????????????????????????????????????????????????????????????
        public static Bitmap CreateOpenIcon(IconSize sz)         => Render(IconOpen,        sz, AccentBlue);
        public static Bitmap CreateSaveIcon(IconSize sz)         => Render(IconSave,        sz, AccentBlue);
        public static Bitmap CreateExportFileIcon(IconSize sz)   => Render(IconExportFile,  sz, AccentGreen);
        public static Bitmap CreateExportCsvIcon(IconSize sz)    => Render(IconExportCsv,   sz, AccentGreen);
        public static Bitmap CreateExportImageIcon(IconSize sz)  => Render(IconExportImage, sz, AccentGreen);
        public static Bitmap CreateExportJsonIcon(IconSize sz)   => Render(IconExportJson,  sz, AccentGreen);
        public static Bitmap CreateExportXmlIcon(IconSize sz)    => Render(IconExportXml,   sz, AccentGreen);
        public static Bitmap CreateMergeLogsIcon(IconSize sz)    => Render(IconMergeLogs,   sz, AccentBlue);
        public static Bitmap CreateReloadIcon(IconSize sz)       => Render(IconReload,      sz, GlyphColor);
        public static Bitmap CreateExitIcon(IconSize sz)         => Render(IconExit,        sz, AccentRed);
        public static Bitmap CreateExportAnalyticsIcon(IconSize sz)  => Render(IconExportAnalytics, sz, AccentGreen);
        public static Bitmap CreateExportApiCsvIcon(IconSize sz)     => Render(IconExportApiCsv,    sz, AccentGreen);
        public static Bitmap CreateExportCallGraphIcon(IconSize sz)  => Render(IconExportCallGraph, sz, AccentGreen);
        public static Bitmap CreateExportHeatmapIcon(IconSize sz)    => Render(IconExportHeatmap,   sz, AccentGreen);
        public static Bitmap CreateSetBaselineIcon(IconSize sz)      => Render(IconSetBaseline,     sz, AccentAmber);
        public static Bitmap CreateScreenshotIcon(IconSize sz)       => Render(IconScreenshot,      sz, AccentBlue);
        public static Bitmap CreateExportXlsIcon(IconSize sz)        => Render(IconExportXls,       sz, AccentGreen);

        // ??????????????????????????????????????????????????????????????????????
        // EDIT MENU
        // ??????????????????????????????????????????????????????????????????????
        public static Bitmap CreateCopyIcon(IconSize sz)          => Render(IconCopy,         sz, GlyphColor);
        public static Bitmap CreateCopyHeadersIcon(IconSize sz)   => Render(IconCopyHeaders,  sz, GlyphColor);
        public static Bitmap CreateFindIcon(IconSize sz)          => Render(IconFind,          sz, GlyphColor);
        public static Bitmap CreateFindNextIcon(IconSize sz)      => Render(IconFindNext,      sz, GlyphColor);
        public static Bitmap CreateFindAllIcon(IconSize sz)       => Render(IconFindAll,       sz, GlyphColor);
        public static Bitmap CreateFilterIcon(IconSize sz)        => Render(IconFilter,        sz, AccentBlue);
        public static Bitmap CreateClearFilterIcon(IconSize sz)   => Render(IconClearFilter,   sz, AccentAmber);
        public static Bitmap CreateExpandIcon(IconSize sz)        => Render(IconExpand,        sz, GlyphColor);
        public static Bitmap CreateCollapseIcon(IconSize sz)      => Render(IconCollapse,      sz, GlyphColor);
        public static Bitmap CreateJumpMatchIcon(IconSize sz)     => Render(IconJumpMatch,     sz, AccentBlue);
        public static Bitmap CreateJumpLineIcon(IconSize sz)      => Render(IconJumpLine,      sz, GlyphColor);
        public static Bitmap CreateBookmarkIcon(IconSize sz)      => Render(IconBookmark,      sz, AccentAmber);
        public static Bitmap CreateBookmarkNextIcon(IconSize sz)  => Render(IconBookmarkNext,  sz, AccentAmber);
        public static Bitmap CreateBookmarkPrevIcon(IconSize sz)  => Render(IconBookmarkPrev,  sz, AccentAmber);
        public static Bitmap CreateBookmarkShowIcon(IconSize sz)  => Render(IconBookmarkShow,  sz, AccentAmber);
        public static Bitmap CreateBookmarkClearIcon(IconSize sz) => Render(IconBookmarkClear, sz, GlyphColor);
        public static Bitmap CreateCopyAllVisibleIcon(IconSize sz) => Render(IconCopyAllVisible, sz, GlyphColor);
        public static Bitmap CreateExpandToLevelIcon(IconSize sz)  => Render(IconExpandToLevel,  sz, GlyphColor);

        // ??????????????????????????????????????????????????????????????????????
        // VIEW MENU
        // ??????????????????????????????????????????????????????????????????????
        public static Bitmap CreateCallTreeIcon(IconSize sz)  => Render(IconCallTree, sz, GlyphColor);
        public static Bitmap CreateApiTreeIcon(IconSize sz)   => Render(IconApiTree,  sz, GlyphColor);
        public static Bitmap CreateFontIcon(IconSize sz)      => Render(IconFont,     sz, GlyphColor);
        public static Bitmap CreateToolbarIcon(IconSize sz)   => Render(IconToolbar,  sz, GlyphColor);
        public static Bitmap CreateTabIcon(IconSize sz)       => Render(IconTab,      sz, GlyphColor);
        public static Bitmap CreateHideTabsIcon(IconSize sz)  => Render(IconHideTabs, sz, GlyphColor);
        public static Bitmap CreateWatchFileIcon(IconSize sz) => Render(IconWatchFile, sz, AccentBlue);
        public static Bitmap CreateStatusBarIcon(IconSize sz) => Render(IconStatusBar, sz, GlyphColor);

        // Per-tab icons — each tab gets a distinct, semantically matching glyph
        public static Bitmap CreateTabLogIcon(IconSize sz)         => Render(IconTabLog,         sz, AccentBlue);
        public static Bitmap CreateTabRawIcon(IconSize sz)         => Render(IconTabRaw,         sz, GlyphColor);
        public static Bitmap CreateTabPerformanceIcon(IconSize sz) => Render(IconTabPerformance, sz, AccentGreen);
        public static Bitmap CreateTabLogDetailsIcon(IconSize sz)  => Render(IconTabLogDetails,  sz, GlyphColor);
        public static Bitmap CreateTabCallGraphIcon(IconSize sz)   => Render(IconTabCallGraph,   sz, AccentBlue);
        public static Bitmap CreateTabFlameGraphIcon(IconSize sz)  => Render(IconTabFlameGraph,  sz, AccentRed);
        public static Bitmap CreateTabTimelineIcon(IconSize sz)    => Render(IconTabTimeline,    sz, AccentAmber);
        public static Bitmap CreateTabAiIcon(IconSize sz)          => Render(IconTabAI,          sz, AccentTeal);
        public static Bitmap CreateTabHeatmapIcon(IconSize sz)     => Render(IconTabHeatmap,     sz, AccentAmber);

        // ??????????????????????????????????????????????????????????????????????
        // NAVIGATION BUTTONS (prev/next error/warning)
        // ??????????????????????????????????????????????????????????????????????
        public static Bitmap CreatePrevErrorIcon(IconSize sz)   => Render(IconPrevError,   sz, AccentRed);
        public static Bitmap CreateNextErrorIcon(IconSize sz)   => Render(IconNextError,   sz, AccentRed);
        public static Bitmap CreatePrevWarningIcon(IconSize sz) => Render(IconPrevWarning, sz, AccentAmber);
        public static Bitmap CreateNextWarningIcon(IconSize sz) => Render(IconNextWarning, sz, AccentAmber);

        // ??????????????????????????????????????????????????????????????????????
        // HELP MENU
        // ??????????????????????????????????????????????????????????????????????
        public static Bitmap CreateHelpIcon(IconSize sz)          => Render(IconHelp,     sz, AccentBlue);
        public static Bitmap CreateKeyboardIcon(IconSize sz)      => Render(IconKeyboard, sz, GlyphColor);
        public static Bitmap CreateAboutIcon(IconSize sz)         => Render(IconAbout,    sz, AccentBlue);
        public static Bitmap CreateCheckUpdatesIcon(IconSize sz)  => Render(IconUpdate,   sz, AccentGreen);
        public static Bitmap CreateReportErrorsIcon(IconSize sz)  => Render(IconReport,   sz, AccentRed);
        public static Bitmap CreateUpdateLogIcon(IconSize sz)     => Render(IconUpdateLog,   sz, AccentBlue);
        public static Bitmap CreateProjectPageIcon(IconSize sz)   => Render(IconProjectPage, sz, AccentBlue);

        // ??????????????????????????????????????????????????????????????????????
        // TREE CONTEXT MENU
        // ??????????????????????????????????????????????????????????????????????
        public static Bitmap CreateSaveBranchIcon(IconSize sz)  => Render(IconSaveBranch, sz, AccentBlue);
        public static Bitmap CreateGrokIcon(IconSize sz)        => Render(IconGrok,       sz, GlyphColor);
        public static Bitmap CreateShowInTreeIcon(IconSize sz)  => Render(IconShowInTree, sz, GlyphColor);
        public static Bitmap CreateSettingsIcon(IconSize sz)    => Render(IconSettings,   sz, GlyphColor);
        public static Bitmap CreateRefreshIcon(IconSize sz)     => Render(IconReload,     sz, GlyphColor);
        public static Bitmap CreateAskAiIcon(IconSize sz)          => Render(IconAskAi,          sz, AccentTeal);
        public static Bitmap CreateRootCauseIcon(IconSize sz)       => Render(IconRootCause,      sz, AccentAmber);
        public static Bitmap CreateInspectLineIcon(IconSize sz)     => Render(IconInspectLine,    sz, AccentBlue);
        public static Bitmap CreateOpenInEditorIcon(IconSize sz)    => Render(IconOpenInEditor,   sz, GlyphColor);
        public static Bitmap CreateToggleBookmarkIcon(IconSize sz)  => Render(IconToggleBookmark, sz, AccentAmber);
        public static Bitmap CreateCompareLogsIcon(IconSize sz)     => CreateCompareIcon(sz);

        // ?? Legacy aliases so GenerateAllIcons signature is unchanged ?????????
        public static Bitmap CreateExportIcon(IconSize sz)  => CreateExportFileIcon(sz);
        public static Bitmap CreateJumpIcon(IconSize sz)    => CreateJumpMatchIcon(sz);
        public static Bitmap CreateTreeIcon(IconSize sz)    => CreateCallTreeIcon(sz);
        public static Bitmap CreateErrorIcon(IconSize sz)   => Render(IconError,   sz, AccentRed);
        public static Bitmap CreateWarningIcon(IconSize sz) => Render(IconWarning, sz, AccentAmber);

        // ??????????????????????????????????????????????????????????????????????
        // COMPARE LOGS DIALOG  — flat vector shapes (no icon-font dependency, so
        // these can never fall back to a missing-glyph "?" box on an odd system).
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Left-pointing triangle — "Previous Difference".</summary>
        public static Bitmap CreateDiffPrevIcon(IconSize sz) => RenderDiffArrow(sz, pointLeft: true, withBar: false);

        /// <summary>Right-pointing triangle — "Next Difference".</summary>
        public static Bitmap CreateDiffNextIcon(IconSize sz) => RenderDiffArrow(sz, pointLeft: false, withBar: false);

        /// <summary>Left-pointing triangle with a leading bar — "First Difference".</summary>
        public static Bitmap CreateDiffFirstIcon(IconSize sz) => RenderDiffArrow(sz, pointLeft: true, withBar: true);

        /// <summary>Right-pointing triangle with a trailing bar — "Last Difference".</summary>
        public static Bitmap CreateDiffLastIcon(IconSize sz) => RenderDiffArrow(sz, pointLeft: false, withBar: true);

        private static Bitmap RenderDiffArrow(IconSize sz, bool pointLeft, bool withBar)
        {
            int s = (int)sz;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float top = s * 0.22f, bottom = s * 0.78f;
                float triW = s * 0.30f;
                float barW = s * 0.11f;
                float barGap = withBar ? s * 0.14f : 0f;

                using (var brush = new SolidBrush(AccentBlue))
                {
                    if (withBar)
                    {
                        float barX = pointLeft ? s * 0.20f : s - s * 0.20f - barW;
                        g.FillRectangle(brush, barX, top, barW, bottom - top);
                    }

                    float triLeft = pointLeft
                        ? s * 0.20f + barGap
                        : s - s * 0.20f - triW - barGap;

                    PointF p1, p2, p3;
                    if (pointLeft)
                    {
                        p1 = new PointF(triLeft + triW, top);
                        p2 = new PointF(triLeft + triW, bottom);
                        p3 = new PointF(triLeft, (top + bottom) / 2f);
                    }
                    else
                    {
                        p1 = new PointF(triLeft, top);
                        p2 = new PointF(triLeft, bottom);
                        p3 = new PointF(triLeft + triW, (top + bottom) / 2f);
                    }
                    g.FillPolygon(brush, new[] { p1, p2, p3 });
                }
            }
            return bmp;
        }

        /// <summary>Two overlapping documents — "Compare Logs".</summary>
        public static Bitmap CreateCompareIcon(IconSize sz)
        {
            int s = (int)sz;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float w = s * 0.52f, h = s * 0.62f;
                float backX = s * 0.10f, backY = s * 0.12f;
                float frontX = s * 0.38f, frontY = s * 0.26f;

                using (var backBrush = new SolidBrush(Color.FromArgb(90, GlyphColor)))
                using (var backPen = new Pen(GlyphColor, 1.3f))
                {
                    g.FillRectangle(backBrush, backX, backY, w, h);
                    g.DrawRectangle(backPen, backX, backY, w, h);
                }

                using (var frontBrush = new SolidBrush(AccentBlue))
                using (var frontPen = new Pen(Color.White, 1.3f))
                {
                    g.FillRectangle(frontBrush, frontX, frontY, w, h);
                    g.DrawRectangle(frontPen, frontX, frontY, w, h);
                }
            }
            return bmp;
        }

        /// <summary>Two opposing horizontal arrows — "Swap Left/Right".</summary>
        public static Bitmap CreateSwapIcon(IconSize sz)
        {
            int s = (int)sz;
            var bmp = new Bitmap(s, s);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float topY = s * 0.36f, botY = s * 0.64f;
                float left = s * 0.18f, right = s * 0.82f;
                float penW = Math.Max(1.4f, s * 0.09f);

                using (var pen = new Pen(GlyphColor, penW) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, left, topY, right, topY);
                    g.DrawLine(pen, left, botY, right, botY);
                }
                using (var brush = new SolidBrush(GlyphColor))
                {
                    float ah = s * 0.13f;
                    g.FillPolygon(brush, new[]
                    {
                        new PointF(right, topY - ah * 0.7f),
                        new PointF(right, topY + ah * 0.7f),
                        new PointF(right + ah, topY)
                    });
                    g.FillPolygon(brush, new[]
                    {
                        new PointF(left, botY - ah * 0.7f),
                        new PointF(left, botY + ah * 0.7f),
                        new PointF(left - ah, botY)
                    });
                }
            }
            return bmp;
        }

        // ??????????????????????????????????????????????????????????????????????
        // STATUS BAR INDICATOR ICONS  — coloured circle + white glyph inside
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Green — file loaded successfully.</summary>
        public static Bitmap CreateStatusOkIcon(IconSize sz)      => RenderStatus(IconStatusOk,   sz, StatusGreen);

        /// <summary>Amber — file loading / processing.</summary>
        public static Bitmap CreateStatusLoadingIcon(IconSize sz)  => RenderStatus(IconStatusLoad, sz, StatusAmber);

        /// <summary>Red — error / no file loaded.</summary>
        public static Bitmap CreateStatusErrorIcon(IconSize sz)    => RenderStatus(IconStatusErr,  sz, StatusRed);

        /// <summary>Orange — file changed on disk, pending reload.</summary>
        public static Bitmap CreateStatusChangedIcon(IconSize sz)  => RenderStatus(IconStatusLoad, sz, StatusOrange);

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
