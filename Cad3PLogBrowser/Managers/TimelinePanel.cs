using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;
using CallStackNode = Cad3PLogBrowser.Services.CallStackNode;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// Timeline/Gantt chart visualization showing method execution over time.
    /// Displays API calls as horizontal bars on a timeline, where:
    /// - X-axis = Time (chronological order)
    /// - Y-axis = Call depth (stack level)
    /// - Bar length = Duration
    /// - Color = Performance (green/amber/red)
    /// </summary>
    public class TimelinePanel : Panel
    {
        // ??????????????????????????????????????????????????????????????????????
        // Fields
        // ??????????????????????????????????????????????????????????????????????

        private List<TimelineEntry> _entries = new List<TimelineEntry>();
        private TimelineEntry _hoveredEntry = null;
        private TimelineEntry _selectedEntry = null;

        private float _zoom = 1.0f;
        private PointF _panOffset = new PointF(0, 0);
        private Point _lastMousePos;
        private bool _isDragging = false;

        private const float ROW_HEIGHT  = 28f;
        private const float BAR_RADIUS  = 4f;
        private const float MIN_ZOOM    = 0.1f;
        private const float MAX_ZOOM    = 10.0f;
        private const int   LEFT_MARGIN = 155;
        private const int   TOP_MARGIN  = 42;

        private bool Dark => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
        private Color BgColor    => Dark ? Color.FromArgb(20, 22, 28)    : Color.FromArgb(245, 247, 252);
        private Color GridColor  => Dark ? Color.FromArgb(35, 39, 50)    : Color.FromArgb(215, 222, 238);
        private Color AxisColor  => Dark ? Color.FromArgb(90, 96, 120)   : Color.FromArgb(140, 150, 180);
        private Color LabelColor => Dark ? Color.FromArgb(185, 190, 210) : Color.FromArgb( 55,  65,  95);

        private DateTime _startTime;
        private DateTime _endTime;
        private long _totalDurationMs;

        private VisualizationWelcomePanel _welcomePanel;
        private VisualizationToolbar _toolbar;
        private VisualizationStatusBar _statusBar;
        private Panel _contentPanel;
        private ToolTip _tooltip;

        // ======================================================================
        // Events
        // ======================================================================

        /// <summary>Raised when the user chooses "Export as Image..." from the context menu.</summary>
        public event EventHandler ExportImageRequested;

        // ======================================================================
        // Data Model
        // ======================================================================

        /// <summary>
        /// Represents a single timeline entry (method call).
        /// </summary>
        public class TimelineEntry
        {
            public string MethodName { get; set; }
            public int Depth { get; set; }
            public DateTime StartTime { get; set; }
            public long DurationMs { get; set; }
            public int LineNumber { get; set; }
            public RectangleF Bounds { get; set; }
            public Color Color { get; set; }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Constructor
        // ??????????????????????????????????????????????????????????????????????

        public TimelinePanel()
        {
            // Professional panel setup
            this.BorderStyle = BorderStyle.None;
            this.BackColor = ThemeManager.BackgroundColor;

            // Create content panel for the actual timeline
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.BackgroundColor
            };
            typeof(Panel).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(_contentPanel, true, null);

            _contentPanel.Paint += ContentPanel_Paint;
            _contentPanel.MouseWheel += TimelinePanel_MouseWheel;
            _contentPanel.MouseDown += TimelinePanel_MouseDown;
            _contentPanel.MouseMove += TimelinePanel_MouseMove;
            _contentPanel.MouseUp += TimelinePanel_MouseUp;
            _contentPanel.MouseClick += TimelinePanel_MouseClick;

            // Create toolbar
            _toolbar = new VisualizationToolbar();
            _toolbar.Dock = DockStyle.Top;
            _toolbar.ResetClicked += (s, e) => ResetView();
            _toolbar.ZoomInClicked += (s, e) => ZoomBy(1.2f);
            _toolbar.ZoomOutClicked += (s, e) => ZoomBy(0.8f);
            _toolbar.FitToWindowClicked += (s, e) => FitToWindow();

            // Create status bar
            _statusBar = new VisualizationStatusBar();
            _statusBar.Dock = DockStyle.Bottom;

            // Create welcome panel
            _welcomePanel = new VisualizationWelcomePanel(
                "Timeline View",
                "Visualize API call execution over time",
                new[]
                {
                    "? X-axis shows time progression",
                    "? Y-axis shows call stack depth",
                    "? Bar length shows call duration",
                    "? Use toolbar buttons for zoom",
                    "? Drag to pan through timeline",
                    "? Click any bar to jump to that log line"
                },
                Color.FromArgb(0, 120, 215)
            );
            _welcomePanel.Dock = DockStyle.Fill;
            _welcomePanel.Visible = true;

            // Tooltip
            _tooltip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 200
            };

            // Context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Reset View", null, (s, e) => ResetView());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Zoom In", null, (s, e) => ZoomBy(1.2f));
            contextMenu.Items.Add("Zoom Out", null, (s, e) => ZoomBy(0.8f));
            contextMenu.Items.Add("Fit to Window", null, (s, e) => FitToWindow());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Export as Image...", null, (s, e) => ExportImageRequested?.Invoke(this, EventArgs.Empty));
            _contentPanel.ContextMenuStrip = contextMenu;

            // Add controls in correct order
            this.Controls.Add(_contentPanel);
            this.Controls.Add(_welcomePanel);
            this.Controls.Add(_toolbar);
            this.Controls.Add(_statusBar);
        }

        /// <summary>
        /// Updates theme colors for toolbar, status bar, and content.
        /// Called by ThemeManager when theme changes.
        /// </summary>
        public void UpdateTheme()
        {
            // EMERGENCY FIX: Add guards to prevent loops
            try
            {
                this.BackColor = ThemeManager.BackgroundColor;

                if (_contentPanel != null)
                    _contentPanel.BackColor = ThemeManager.BackgroundColor;

                if (_welcomePanel != null)
                    _welcomePanel.BackColor = ThemeManager.BackgroundColor;

                if (_toolbar != null && this.Controls.Contains(_toolbar))
                    _toolbar.ApplyTheme();

                if (_statusBar != null && this.Controls.Contains(_statusBar))
                    _statusBar.ApplyTheme();

                if (_contentPanel != null && _entries.Count > 0)
                    _contentPanel.Invalidate();
            }
            catch
            {
                // Silently ignore theme update errors to prevent freeze
            }
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.Clear(BgColor);
            if (_entries.Count == 0) return;

            // Translate for pan (zoom is baked into Bounds by CalculateLayout)
            g.TranslateTransform(_panOffset.X, _panOffset.Y);

            foreach (var entry in _entries)
                DrawTimelineEntry(g, entry);

            g.ResetTransform();
            DrawTimeScale(g);
            DrawDepthLabels(g);
            DrawStatusOverlay(g);
        }

        private void DrawStatusOverlay(Graphics g)
        {
            const int H = 22;
            int y0 = _contentPanel.Height - H;
            Color bg = Dark ? Color.FromArgb(200, 24, 26, 35) : Color.FromArgb(200, 238, 241, 252);
            using (var bgBrush = new SolidBrush(bg))
                g.FillRectangle(bgBrush, 0, y0, _contentPanel.Width, H);
            using (var sep = new Pen(AxisColor))
                g.DrawLine(sep, 0, y0, _contentPanel.Width, y0);
            string txt = $"  {_entries.Count} calls  •  {_totalDurationMs}ms total" +
                         $"  •  Zoom {_zoom*100:F0}%  •  Scroll=Zoom  Drag=Pan  Click=Jump";
            using (var f = new Font("Segoe UI", 7.5f))
            using (var b = new SolidBrush(Color.FromArgb(140, Dark ? 185 : 65, Dark ? 192 : 72, Dark ? 215 : 105)))
                g.DrawString(txt, f, b, 0, y0 + 4);
        }

        private void ZoomBy(float factor)
        {
            float newZoom = _zoom * factor;
            if (newZoom >= MIN_ZOOM && newZoom <= MAX_ZOOM)
            {
                _zoom = newZoom;
                _toolbar.UpdateZoomLevel(_zoom);
                _statusBar.UpdateZoom(_zoom);
                CalculateLayout();
                _contentPanel.Invalidate();
            }
        }

        private void FitToWindow()
        {
            _zoom = 1.0f;
            _panOffset = new PointF(0, 0);
            _toolbar.UpdateZoomLevel(_zoom);
            _statusBar.UpdateZoom(_zoom);
            CalculateLayout();
            _contentPanel.Invalidate();
        }

        public void ResetView()
        {
            _zoom = 1.0f;
            _panOffset = new PointF(0, 0);
            _selectedEntry = null;
            _toolbar.UpdateZoomLevel(_zoom);
            _statusBar.UpdateZoom(_zoom);
            _statusBar.UpdateSelectedNode(null, 0);
            CalculateLayout();
            _contentPanel.Invalidate();
        }

        // ??????????????????????????????????????????????????????????????????????
        // Public API
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Loads timeline data from call stack.
        /// </summary>
        public void LoadCallStack(List<CallStackNode> callStack)
        {
            _entries.Clear();
            _selectedEntry = null;
            _hoveredEntry = null;

            if (callStack == null || callStack.Count == 0)
            {
                _welcomePanel.Visible = true;
                _welcomePanel.BringToFront();
                _statusBar.UpdateNodeCount(0);
                _statusBar.UpdateTotalDuration(0);
                _statusBar.UpdateSelectedNode(null, 0);
                _contentPanel.Invalidate();
                return;
            }

            _welcomePanel.Visible = false;

            // Convert to timeline entries
            _startTime = DateTime.Now;
            _endTime = DateTime.Now;

            DateTime currentTime = DateTime.Now;

            foreach (var node in callStack)
            {
                ConvertToTimelineRecursive(node, 0, ref currentTime);
            }

            _totalDurationMs = (long)(_endTime - _startTime).TotalMilliseconds;
            if (_totalDurationMs == 0) _totalDurationMs = 1;

            // Calculate layout
            CalculateLayout();

            // Update status bar
            _statusBar.UpdateNodeCount(_entries.Count);
            _statusBar.UpdateTotalDuration(_totalDurationMs);
            _toolbar.UpdateZoomLevel(_zoom);
            _statusBar.UpdateZoom(_zoom);

            _contentPanel.Invalidate();
        }

        /// <summary>
        /// Exports timeline as image.
        /// </summary>
        public Bitmap ExportAsImage(int width = 1920, int height = 1080)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(ThemeManager.BackgroundColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Save state, set export dimensions
                float oldZoom = _zoom;
                PointF oldPan = _panOffset;
                _zoom = 1.0f;
                _panOffset = PointF.Empty;

                // Temporarily override ClientSize for layout calculation
                float baseWidth = width - LEFT_MARGIN - 20;
                float availableWidth = baseWidth;
                foreach (var entry in _entries)
                {
                    float offsetMs = (float)(entry.StartTime - _startTime).TotalMilliseconds;
                    float x = LEFT_MARGIN + (offsetMs / _totalDurationMs) * availableWidth;
                    float bw = Math.Max(2f, (entry.DurationMs / (float)_totalDurationMs) * availableWidth);
                    float y = TOP_MARGIN + (entry.Depth * ROW_HEIGHT);
                    entry.Bounds = new RectangleF(x, y, bw, ROW_HEIGHT - 2);
                }

                foreach (var entry in _entries)
                    DrawTimelineEntry(g, entry);

                // Draw axis in screen space
                using (var pen = new Pen(ThemeManager.BorderColor, 1f))
                using (var font = new Font("Segoe UI", 7f))
                using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
                {
                    int intervals = 10;
                    float iw = availableWidth / intervals;
                    long ims = _totalDurationMs / intervals;
                    for (int i = 0; i <= intervals; i++)
                    {
                        float x = LEFT_MARGIN + i * iw;
                        g.DrawLine(pen, x, TOP_MARGIN - 5, x, TOP_MARGIN);
                        string lbl = $"{i * ims}ms";
                        var sz = g.MeasureString(lbl, font);
                        g.DrawString(lbl, font, brush, x - sz.Width / 2, TOP_MARGIN - 20);
                    }
                    g.DrawLine(pen, LEFT_MARGIN, TOP_MARGIN, LEFT_MARGIN + availableWidth, TOP_MARGIN);
                }

                // Restore live layout
                _zoom = oldZoom;
                _panOffset = oldPan;
                CalculateLayout();
            }
            return bitmap;
        }

        // ??????????????????????????????????????????????????????????????????????
        // Conversion
        // ??????????????????????????????????????????????????????????????????????

        private void ConvertToTimelineRecursive(CallStackNode node, int depth, ref DateTime currentTime)
        {
            var entry = new TimelineEntry
            {
                MethodName = node.Label,
                Depth = depth,
                StartTime = currentTime,
                DurationMs = node.DurationMs,
                LineNumber = node.LineNumber,
                Color = GetColorForDuration(node.DurationMs)
            };

            _entries.Add(entry);

            // Update time range
            if (_startTime == DateTime.Now || entry.StartTime < _startTime)
                _startTime = entry.StartTime;

            DateTime endTime = entry.StartTime.AddMilliseconds(entry.DurationMs);
            if (endTime > _endTime)
                _endTime = endTime;

            // Process children
            DateTime childTime = currentTime;
            foreach (var child in node.Children)
            {
                ConvertToTimelineRecursive(child, depth + 1, ref childTime);
                childTime = childTime.AddMilliseconds(child.DurationMs);
            }

            // Advance time
            currentTime = currentTime.AddMilliseconds(node.DurationMs);
        }

        private Color GetColorForDuration(long durationMs)
        {
            if (durationMs <= 0)   return Color.FromArgb( 75, 190, 165);  // teal  – no data
            if (durationMs <  50)  return Color.FromArgb( 60, 195, 130);  // emerald – very fast
            if (durationMs <  200) return Color.FromArgb(100, 185, 225);  // sky blue – fast
            if (durationMs <  500) return Color.FromArgb(240, 185,  55);  // amber – medium
            if (durationMs < 1000) return Color.FromArgb(240, 130,  60);  // orange – slow
            return                        Color.FromArgb(225,  65,  60);  // red – very slow
        }

        // ??????????????????????????????????????????????????????????????????????
        // Layout
        // ??????????????????????????????????????????????????????????????????????

        private void CalculateLayout()
        {
            // Scale the total available width by _zoom so bars expand when zooming in
            // and compress when zooming out, giving a true horizontal zoom effect.
            float baseWidth = _contentPanel.ClientSize.Width > 0
                ? _contentPanel.ClientSize.Width - LEFT_MARGIN - 20
                : this.ClientSize.Width - LEFT_MARGIN - 20;
            float availableWidth = baseWidth * _zoom;

            foreach (var entry in _entries)
            {
                float offsetMs = (float)(entry.StartTime - _startTime).TotalMilliseconds;
                float x = LEFT_MARGIN + (offsetMs / _totalDurationMs) * availableWidth;
                float width = Math.Max(2f, (entry.DurationMs / (float)_totalDurationMs) * availableWidth);
                float y = TOP_MARGIN + (entry.Depth * ROW_HEIGHT);
                entry.Bounds = new RectangleF(x, y, width, ROW_HEIGHT - 2);
            }
        }

        // ======================================================================
        // Rendering
        // ======================================================================

        // ContentPanel_Paint handles all rendering

        private void DrawTimeScale(Graphics g)
        {
            // Header band
            Color hdrBg = Dark ? Color.FromArgb(30, 33, 42) : Color.FromArgb(235, 238, 250);
            using (var bg = new SolidBrush(hdrBg))
                g.FillRectangle(bg, 0, 0, _contentPanel.Width, TOP_MARGIN);
            using (var sep = new Pen(AxisColor, 1f))
                g.DrawLine(sep, LEFT_MARGIN, TOP_MARGIN - 1, _contentPanel.Width, TOP_MARGIN - 1);

            float baseWidth    = _contentPanel.ClientSize.Width - LEFT_MARGIN - 20;
            float availableWidth = baseWidth * _zoom;
            int   intervals    = 10;
            float intervalWidth = availableWidth / intervals;
            long  intervalMs   = _totalDurationMs / intervals;

            using (var tickPen = new Pen(AxisColor, 1f))
            using (var font    = new Font("Segoe UI", 7f))
            using (var brush   = new SolidBrush(LabelColor))
            {
                for (int i = 0; i <= intervals; i++)
                {
                    float x = LEFT_MARGIN + _panOffset.X + (i * intervalWidth);
                    if (x < LEFT_MARGIN - 40 || x > _contentPanel.Width + 40) continue;

                    // Major tick
                    g.DrawLine(tickPen, x, TOP_MARGIN - 8, x, TOP_MARGIN);
                    // Minor tick halfway
                    if (i < intervals)
                    {
                        float mx = x + intervalWidth / 2;
                        if (mx > LEFT_MARGIN && mx < _contentPanel.Width)
                            g.DrawLine(tickPen, mx, TOP_MARGIN - 4, mx, TOP_MARGIN);
                    }

                    string lbl = $"{i * intervalMs}ms";
                    var    sz  = g.MeasureString(lbl, font);
                    g.DrawString(lbl, font, brush, x - sz.Width / 2, TOP_MARGIN - 20);
                }

                // Baseline
                float x0 = Math.Max(LEFT_MARGIN, LEFT_MARGIN + _panOffset.X);
                float x1 = Math.Min(_contentPanel.Width, LEFT_MARGIN + _panOffset.X + availableWidth);
                using (var basePen = new Pen(AxisColor, 1.5f))
                    g.DrawLine(basePen, x0, TOP_MARGIN, x1, TOP_MARGIN);
            }
        }

        private void DrawDepthLabels(Graphics g)
        {
            int maxDepth = _entries.Count > 0 ? _entries.Max(e => e.Depth) : 0;

            // Left-margin band
            Color bandBg = Dark ? Color.FromArgb(28, 30, 40) : Color.FromArgb(238, 241, 252);
            using (var bg = new SolidBrush(bandBg))
                g.FillRectangle(bg, 0, TOP_MARGIN, LEFT_MARGIN - 4, _contentPanel.Height - TOP_MARGIN);
            using (var sep = new Pen(AxisColor, 1f))
                g.DrawLine(sep, LEFT_MARGIN - 1, TOP_MARGIN, LEFT_MARGIN - 1, _contentPanel.Height);

            // Alternating row shading
            for (int depth = 0; depth <= maxDepth; depth++)
            {
                float rowY = TOP_MARGIN + _panOffset.Y + depth * ROW_HEIGHT;
                if (rowY + ROW_HEIGHT < TOP_MARGIN || rowY > _contentPanel.Height) continue;
                if (depth % 2 == 1)
                    using (var rb = new SolidBrush(Color.FromArgb(Dark ? 10 : 8,
                        Dark ? 255 : 0, Dark ? 255 : 0, Dark ? 255 : 0)))
                        g.FillRectangle(rb, LEFT_MARGIN, rowY, _contentPanel.Width - LEFT_MARGIN, ROW_HEIGHT);
            }

            using (var font  = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(LabelColor))
            {
                for (int depth = 0; depth <= maxDepth; depth++)
                {
                    float y = TOP_MARGIN + _panOffset.Y + depth * ROW_HEIGHT + ROW_HEIGHT / 2f;
                    if (y < TOP_MARGIN || y > _contentPanel.Height) continue;

                    string lbl = $"D{depth}";
                    var    sz  = g.MeasureString(lbl, font);
                    float  lx  = LEFT_MARGIN - sz.Width - 10;
                    float  ly  = y - sz.Height / 2;

                    // Pill background
                    var pill = new RectangleF(lx - 3, ly - 1, sz.Width + 6, sz.Height + 2);
                    Color pillBg = Dark ? Color.FromArgb(45, 50, 68) : Color.FromArgb(210, 218, 245);
                    using (var pb = new SolidBrush(pillBg))
                        FillRounded(g, pb, pill, 4);

                    g.DrawString(lbl, font, brush, lx, ly);
                }
            }
        }

        private void DrawTimelineEntry(Graphics g, TimelineEntry entry)
        {
            bool isHov = entry == _hoveredEntry;
            bool isSel = entry == _selectedEntry;

            Color fill    = isHov ? Lighten(entry.Color, 1.22f) : entry.Color;
            Color fillBot = Darken(fill, 0.75f);
            var   r       = entry.Bounds;

            // Shadow
            using (var sb = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            { var sr = r; sr.Offset(1, 2); FillRounded(g, sb, sr, BAR_RADIUS); }

            // Gradient body
            using (var gb = new LinearGradientBrush(
                new PointF(r.X, r.Y), new PointF(r.X, r.Bottom), fill, fillBot))
                FillRounded(g, gb, r, BAR_RADIUS);

            // Gloss strip
            using (var gloss = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
                FillRounded(g, gloss, new RectangleF(r.X + 1, r.Y + 1, r.Width - 2, 7), 3);

            // Border
            Color border = isSel ? Color.FromArgb(0, 150, 230) :
                           isHov ? Color.FromArgb(255, 255, 255, 80) :
                                   Color.FromArgb(0, 0, 0, 45);
            using (var pen = new Pen(border, isSel ? 2f : 0.8f))
                StrokeRounded(g, pen, r, BAR_RADIUS);

            // Selection pulse ring
            if (isSel)
                using (var gp = new Pen(Color.FromArgb(100, 0, 150, 230), 1f))
                    StrokeRounded(g, gp, RectangleF.Inflate(r, 2, 2), BAR_RADIUS + 2);

            // Label
            if (r.Width > 36)
            {
                using (var font = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                using (var durFont = new Font("Segoe UI", 6.5f))
                using (var tb = new SolidBrush(Dark ? Color.FromArgb(238, 240, 248) : Color.FromArgb(20, 28, 55)))
                {
                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming      = StringTrimming.EllipsisCharacter,
                        FormatFlags   = StringFormatFlags.NoWrap
                    };

                    if (r.Width > 110)
                    {
                        string dur   = $"{entry.DurationMs}ms";
                        var    dSz   = g.MeasureString(dur, durFont);
                        sf.Alignment = StringAlignment.Center;
                        using (var db = new SolidBrush(Color.FromArgb(160, Dark ? Color.FromArgb(238, 240, 248) : Color.FromArgb(20, 28, 55))))
                            g.DrawString(dur, durFont, db,
                                new RectangleF(r.Right - dSz.Width - 6, r.Y, dSz.Width + 4, r.Height), sf);
                        sf.Alignment = StringAlignment.Near;
                        g.DrawString(entry.MethodName, font, tb,
                            new RectangleF(r.X + 5, r.Y, r.Width - dSz.Width - 14, r.Height), sf);
                    }
                    else
                    {
                        sf.Alignment = StringAlignment.Near;
                        g.DrawString(entry.MethodName, font, tb,
                            new RectangleF(r.X + 4, r.Y, r.Width - 8, r.Height), sf);
                    }
                }
            }
        }

        // Geometry + colour helpers
        private static void FillRounded(Graphics g, Brush br, RectangleF r, float rad)
        { using (var p = RoundPath(r, rad)) g.FillPath(br, p); }
        private static void StrokeRounded(Graphics g, Pen pen, RectangleF r, float rad)
        { using (var p = RoundPath(r, rad)) g.DrawPath(pen, p); }
        private static GraphicsPath RoundPath(RectangleF r, float rad)
        {
            float d = rad * 2f;
            var p = new GraphicsPath();
            p.AddArc(r.X,         r.Y,          d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            p.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            p.CloseFigure();
            return p;
        }
        private static Color Darken(Color c, float f) =>
            Color.FromArgb(Math.Max(0,(int)(c.R*f)), Math.Max(0,(int)(c.G*f)), Math.Max(0,(int)(c.B*f)));
        private static Color Lighten(Color c, float f) =>
            Color.FromArgb(Math.Min(255,(int)(c.R*f)), Math.Min(255,(int)(c.G*f)), Math.Min(255,(int)(c.B*f)));

            // ======================================================================
            // Mouse Events
            // ======================================================================

        private void TimelinePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            float delta = e.Delta > 0 ? 1.2f : 0.8f;
            ZoomBy(delta);
        }

        private void TimelinePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePos = e.Location;
                Cursor = Cursors.Hand;
            }
        }

        private void TimelinePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Pan
                _panOffset.X += e.X - _lastMousePos.X;
                _panOffset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                _contentPanel.Invalidate();
            }
            else
            {
                var entry = FindEntryAtPoint(e.Location);
                if (entry != _hoveredEntry)
                {
                    _hoveredEntry = entry;
                    _contentPanel.Invalidate();

                    if (entry != null)
                    {
                        string tooltipText = $"{entry.MethodName}\n" +
                                           $"Duration: {entry.DurationMs}ms\n" +
                                           $"Start: {entry.StartTime:HH:mm:ss.fff}\n" +
                                           $"Depth: {entry.Depth}";
                        _tooltip.SetToolTip(_contentPanel, tooltipText);
                        _statusBar.UpdateInfo($"Hover: {entry.MethodName} - {entry.DurationMs}ms");
                        _contentPanel.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        _tooltip.SetToolTip(_contentPanel, "");
                        _statusBar.UpdateInfo("Scroll: Zoom | Drag: Pan | Click: Jump to Log");
                        _contentPanel.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void TimelinePanel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            Cursor = Cursors.Default;
        }

        private void TimelinePanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var entry = FindEntryAtPoint(e.Location);
                if (entry != null)
                {
                    _selectedEntry = entry;
                    _statusBar.UpdateSelectedNode(entry.MethodName, entry.DurationMs);
                    _contentPanel.Invalidate();

                    // Raise event for line jump
                    OnTimelineEntrySelected(entry);
                }
            }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Events
        // ??????????????????????????????????????????????????????????????????????

        public event EventHandler<TimelineEntry> TimelineEntrySelected;

        protected virtual void OnTimelineEntrySelected(TimelineEntry entry)
        {
            TimelineEntrySelected?.Invoke(this, entry);
        }

        // ??????????????????????????????????????????????????????????????????????
        // Hit Testing
        // ??????????????????????????????????????????????????????????????????????

        private TimelineEntry FindEntryAtPoint(Point screenPoint)
        {
            // Bounds are in layout space (zoom already baked in).
            // Only subtract pan offset to convert from screen to layout space.
            float lx = screenPoint.X - _panOffset.X;
            float ly = screenPoint.Y - _panOffset.Y;
            var layoutPoint = new PointF(lx, ly);

            foreach (var entry in _entries)
            {
                if (entry.Bounds.Contains(layoutPoint))
                    return entry;
            }
            return null;
        }
    }
}
