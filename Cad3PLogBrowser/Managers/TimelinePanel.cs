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

        private const float ROW_HEIGHT = 24f;
        private const float MIN_ZOOM = 0.1f;
        private const float MAX_ZOOM = 10.0f;
        private const int LEFT_MARGIN = 150; // Space for labels
        private const int TOP_MARGIN = 40;   // Space for time scale

        private DateTime _startTime;
        private DateTime _endTime;
        private long _totalDurationMs;

        // ??????????????????????????????????????????????????????????????????????
        // Data Model
        // ??????????????????????????????????????????????????????????????????????

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
            DoubleBuffered = true;
            ResizeRedraw = true;
            BorderStyle = BorderStyle.None; // Modern flat design
            BackColor = ThemeManager.BackgroundColor;

            this.MouseWheel += TimelinePanel_MouseWheel;
            this.MouseDown += TimelinePanel_MouseDown;
            this.MouseMove += TimelinePanel_MouseMove;
            this.MouseUp += TimelinePanel_MouseUp;
            this.MouseClick += TimelinePanel_MouseClick;

            // Add tooltip for better UX
            var tooltip = new ToolTip();
            tooltip.SetToolTip(this, "?? Timeline: Scroll to zoom, drag to pan, click on events to jump to log");
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
                Invalidate();
                return;
            }

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
            Invalidate();
        }

        /// <summary>
        /// Resets view to default zoom and pan.
        /// </summary>
        public void ResetView()
        {
            _zoom = 1.0f;
            _panOffset = new PointF(0, 0);
            Invalidate();
        }

        /// <summary>
        /// Exports timeline as image.
        /// </summary>
        public Bitmap ExportAsImage(int width = 1920, int height = 1080)
        {
            var bitmap = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Recalculate for export size
                float oldZoom = _zoom;
                var oldPan = _panOffset;

                _zoom = 1.0f;
                _panOffset = new PointF(0, 0);

                // Draw
                DrawTimeline(g);

                // Restore
                _zoom = oldZoom;
                _panOffset = oldPan;
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
            if (durationMs < 100)
                return Color.FromArgb(144, 238, 144); // Light green
            else if (durationMs < 500)
                return Color.FromArgb(255, 222, 173); // Light amber
            else
                return Color.FromArgb(255, 182, 193); // Light red
        }

        // ??????????????????????????????????????????????????????????????????????
        // Layout
        // ??????????????????????????????????????????????????????????????????????

        private void CalculateLayout()
        {
            float availableWidth = this.ClientSize.Width - LEFT_MARGIN - 20;

            foreach (var entry in _entries)
            {
                // Calculate X position based on start time
                float offsetMs = (float)(entry.StartTime - _startTime).TotalMilliseconds;
                float x = LEFT_MARGIN + (offsetMs / _totalDurationMs) * availableWidth;

                // Calculate width based on duration
                float width = (entry.DurationMs / (float)_totalDurationMs) * availableWidth;
                width = Math.Max(2, width); // Minimum 2px width

                // Calculate Y position based on depth
                float y = TOP_MARGIN + (entry.Depth * ROW_HEIGHT);

                entry.Bounds = new RectangleF(x, y, width, ROW_HEIGHT - 2);
            }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Rendering
        // ??????????????????????????????????????????????????????????????????????

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_entries.Count == 0)
            {
                DrawEmptyState(g);
                return;
            }

            // Draw title/header first (not affected by zoom/pan)
            DrawTitle(g);

            // Apply transform for content area (below header)
            var headerHeight = 65; // Header + legend
            g.TranslateTransform(_panOffset.X, _panOffset.Y + headerHeight);
            g.ScaleTransform(_zoom, _zoom);

            DrawTimeline(g);
        }

        private void DrawTimeline(Graphics g)
        {
            // Draw time scale
            DrawTimeScale(g);

            // Draw depth labels
            DrawDepthLabels(g);

            // Draw timeline entries
            foreach (var entry in _entries)
            {
                DrawTimelineEntry(g, entry);
            }
        }

        private void DrawTimeScale(Graphics g)
        {
            g.ResetTransform();

            using (var pen = new Pen(ThemeManager.BorderColor, 1f))
            using (var font = new Font("Segoe UI", 7f))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                // Draw time markers every 100ms or appropriate interval
                float availableWidth = this.ClientSize.Width - LEFT_MARGIN - 20;
                int intervals = 10;
                float intervalWidth = availableWidth / intervals;
                long intervalMs = _totalDurationMs / intervals;

                for (int i = 0; i <= intervals; i++)
                {
                    float x = LEFT_MARGIN + (i * intervalWidth);
                    g.DrawLine(pen, x, TOP_MARGIN - 5, x, TOP_MARGIN);

                    string label = $"{i * intervalMs}ms";
                    var size = g.MeasureString(label, font);
                    g.DrawString(label, font, brush, x - size.Width / 2, TOP_MARGIN - 20);
                }

                // Draw baseline
                g.DrawLine(pen, LEFT_MARGIN, TOP_MARGIN, LEFT_MARGIN + availableWidth, TOP_MARGIN);
            }
        }

        private void DrawDepthLabels(Graphics g)
        {
            g.ResetTransform();

            int maxDepth = _entries.Count > 0 ? _entries.Max(e => e.Depth) : 0;

            using (var font = new Font("Segoe UI", 7f))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                for (int depth = 0; depth <= maxDepth; depth++)
                {
                    float y = TOP_MARGIN + (depth * ROW_HEIGHT) + ROW_HEIGHT / 2;
                    g.DrawString($"D{depth}", font, brush, 5, y - 6);
                }
            }
        }

        private void DrawTimelineEntry(Graphics g, TimelineEntry entry)
        {
            bool isHovered = entry == _hoveredEntry;
            bool isSelected = entry == _selectedEntry;

            Color fillColor = entry.Color;
            if (isHovered)
                fillColor = Color.FromArgb(Math.Min(255, fillColor.R + 30), 
                                          Math.Min(255, fillColor.G + 30), 
                                          Math.Min(255, fillColor.B + 30));

            Color borderColor = isSelected ? Color.Red : Color.FromArgb(80, 80, 80);
            float borderWidth = isSelected ? 2f : 1f;

            // Draw bar
            using (var brush = new SolidBrush(fillColor))
            using (var pen = new Pen(borderColor, borderWidth))
            {
                g.FillRectangle(brush, entry.Bounds);
                g.DrawRectangle(pen, Rectangle.Round(entry.Bounds));
            }

            // Draw label if wide enough
            if (entry.Bounds.Width > 50)
            {
                using (var font = new Font("Segoe UI", 7f))
                using (var brush = new SolidBrush(ThemeManager.CurrentTheme == ThemeManager.Theme.Dark ? Color.White : Color.Black))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    string label = $"{entry.MethodName} ({entry.DurationMs}ms)";
                    g.DrawString(label, font, brush, entry.Bounds, format);
                }
            }
        }

        private void DrawEmptyState(Graphics g)
        {
            g.ResetTransform();

            // Modern empty state with card design
            int cardWidth = 500;
            int cardHeight = 350;
            int cardX = (this.Width - cardWidth) / 2;
            int cardY = (this.Height - cardHeight) / 2;

            var cardRect = new Rectangle(cardX, cardY, cardWidth, cardHeight);

            // Draw card background with subtle shadow
            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                g.FillRoundedRectangle(shadowBrush, cardX + 4, cardY + 4, cardWidth, cardHeight, 12);
            }

            var cardColor = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                ? Color.FromArgb(45, 45, 48) : Color.FromArgb(250, 250, 250);

            using (var cardBrush = new SolidBrush(cardColor))
            using (var borderPen = new Pen(ThemeManager.BorderColor, 2))
            {
                g.FillRoundedRectangle(cardBrush, cardX, cardY, cardWidth, cardHeight, 12);
                g.DrawRoundedRectangle(borderPen, cardX, cardY, cardWidth, cardHeight, 12);
            }

            // Draw timeline icon
            using (var iconFont = new Font("Segoe UI", 48f))
            using (var iconBrush = new SolidBrush(Color.FromArgb(0, 122, 204))) // Blue
            {
                var iconSize = g.MeasureString("??", iconFont);
                g.DrawString("??", iconFont, iconBrush, 
                    cardX + (cardWidth - iconSize.Width) / 2, cardY + 20);
            }

            // Draw title
            using (var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                var titleText = "Timeline Visualization";
                var titleSize = g.MeasureString(titleText, titleFont);
                g.DrawString(titleText, titleFont, titleBrush,
                    cardX + (cardWidth - titleSize.Width) / 2, cardY + 100);
            }

            // Draw subtitle
            using (var subFont = new Font("Segoe UI", 10f))
            using (var subBrush = new SolidBrush(Color.FromArgb(180, ThemeManager.ForegroundColor)))
            {
                var subText = "No timeline data to visualize";
                var subSize = g.MeasureString(subText, subFont);
                g.DrawString(subText, subFont, subBrush,
                    cardX + (cardWidth - subSize.Width) / 2, cardY + 135);
            }

            // Draw instructions
            var instructions = new[]
            {
                "?? Open a log file with call data to get started",
                "",
                "?? What is a Timeline View?",
                "• Chronological visualization of API calls",
                "• X-axis = Time progression",
                "• Y-axis = Call stack depth",
                "• Bar length = Duration of call",
                "• Color = Performance (green/orange/red)",
                "",
                "??? How to Use:",
                "• Hover over bars to see call details",
                "• Click a bar to jump to that log line",
                "• Mouse wheel to zoom in/out",
                "• Drag to pan left/right",
                "• Right-click to reset view"
            };

            using (var font = new Font("Segoe UI", 9f))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                float y = cardY + 170;
                foreach (var line in instructions)
                {
                    var lineSize = g.MeasureString(line, font);
                    g.DrawString(line, font, brush, 
                        cardX + (cardWidth - lineSize.Width) / 2, y);
                    y += line == "" ? 10 : 22;
                }
            }
        }

        private void DrawTitle(Graphics g)
        {
            g.ResetTransform();

            // Draw modern header bar
            var headerHeight = 35;
            var headerColor = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                ? Color.FromArgb(37, 37, 38) : Color.FromArgb(240, 240, 240);

            using (var headerBrush = new SolidBrush(headerColor))
            {
                g.FillRectangle(headerBrush, 0, 0, this.Width, headerHeight);
            }

            // Draw subtle border at bottom of header
            using (var borderPen = new Pen(ThemeManager.BorderColor, 1))
            {
                g.DrawLine(borderPen, 0, headerHeight, this.Width, headerHeight);
            }

            // Draw icon and title
            string title = $"?? Timeline View — Method Execution Over Time";

            using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                g.DrawString(title, font, brush, 12, 8);
            }

            // Draw zoom level indicator
            if (_zoom != 1.0f)
            {
                var zoomText = $"Zoom: {_zoom:F1}x";
                using (var font = new Font("Segoe UI", 9f))
                using (var brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
                {
                    var size = g.MeasureString(zoomText, font);
                    g.DrawString(zoomText, font, brush, this.Width - size.Width - 180, 10);
                }
            }

            // Draw interactive instructions (right side of header)
            string instructions = "??? Scroll: Zoom • Drag: Pan • Click: Jump to Log • Right-Click: Reset";
            using (var font = new Font("Segoe UI", 8f))
            using (var brush = new SolidBrush(Color.FromArgb(150, ThemeManager.ForegroundColor)))
            {
                var size = g.MeasureString(instructions, font);
                g.DrawString(instructions, font, brush, this.Width - size.Width - 12, 10);
            }

            // Draw legend if data exists
            if (_entries.Count > 0)
            {
                var legendY = headerHeight + 10;
                DrawLegend(g, 12, legendY);
            }
        }

        /// <summary>
        /// Draws a color legend for the timeline.
        /// </summary>
        private void DrawLegend(Graphics g, int x, int y)
        {
            var legendItems = new[]
            {
                ("Length = Duration", Color.Empty),
                ("Position = Time", Color.Empty),
                ("", Color.Empty), // Spacer
                ("Fast (<100ms)", Color.FromArgb(76, 175, 80)),
                ("Medium (100-500ms)", Color.FromArgb(255, 152, 0)),
                ("Slow (>500ms)", Color.FromArgb(244, 67, 54))
            };

            using (var font = new Font("Segoe UI", 8f))
            {
                var currentX = x;
                foreach (var (text, color) in legendItems)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        currentX += 15; // Spacer
                        continue;
                    }

                    // Draw color box if color specified
                    if (color != Color.Empty)
                    {
                        using (var brush = new SolidBrush(color))
                        using (var pen = new Pen(ThemeManager.BorderColor))
                        {
                            g.FillRectangle(brush, currentX, y + 2, 12, 12);
                            g.DrawRectangle(pen, currentX, y + 2, 12, 12);
                        }
                        currentX += 18;
                    }

                    // Draw text
                    using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
                    {
                        g.DrawString(text, font, brush, currentX, y);
                        var size = g.MeasureString(text, font);
                        currentX += (int)size.Width + 15;
                    }
                }
            }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Mouse Events
        // ??????????????????????????????????????????????????????????????????????

        private void TimelinePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            float delta = e.Delta > 0 ? 1.2f : 0.8f;
            float newZoom = _zoom * delta;

            if (newZoom >= MIN_ZOOM && newZoom <= MAX_ZOOM)
            {
                _zoom = newZoom;
                CalculateLayout();
                Invalidate();
            }
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
                _panOffset.X += e.X - _lastMousePos.X;
                _panOffset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                Invalidate();
            }
            else
            {
                var entry = FindEntryAtPoint(e.Location);
                if (entry != _hoveredEntry)
                {
                    _hoveredEntry = entry;
                    Invalidate();

                    if (entry != null)
                    {
                        this.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
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
                    Invalidate();

                    // Raise event for line jump
                    OnTimelineEntrySelected(entry);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                ResetView();
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
            float graphX = (screenPoint.X - _panOffset.X) / _zoom;
            float graphY = (screenPoint.Y - _panOffset.Y) / _zoom;
            PointF graphPoint = new PointF(graphX, graphY);

            foreach (var entry in _entries)
            {
                if (entry.Bounds.Contains(graphPoint))
                    return entry;
            }

            return null;
        }
    }
}
