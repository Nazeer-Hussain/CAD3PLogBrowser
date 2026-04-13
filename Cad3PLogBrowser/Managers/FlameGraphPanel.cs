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
    /// Flame Graph visualization for performance analysis.
    /// Displays hierarchical call stack data as stacked horizontal bars,
    /// where the width represents time/duration and height represents call depth.
    /// </summary>
    /// <remarks>
    /// Flame graphs are an excellent way to visualize:
    /// - Which functions consume the most time
    /// - Call stack depth and hierarchy
    /// - Performance hotspots at a glance
    /// 
    /// The wider the bar, the more time spent in that function.
    /// Colors are generated based on function name for visual distinction.
    /// Hover to see details, click to zoom into a specific call stack.
    /// </remarks>
    public class FlameGraphPanel : Panel
    {
        // ??????????????????????????????????????????????????????????????????????
        // Fields and State
        // ??????????????????????????????????????????????????????????????????????

        private List<FlameGraphNode> _rootNodes = new List<FlameGraphNode>();
        private FlameGraphNode _selectedNode = null;
        private FlameGraphNode _hoveredNode = null;
        private FlameGraphNode _zoomedNode = null; // Root node when zoomed in

        private float _zoom = 1.0f;
        private PointF _panOffset = new PointF(0, 0);
        private Point _lastMousePos;
        private bool _isDragging = false;

        private const float MIN_ZOOM = 0.5f;
        private const float MAX_ZOOM = 4.0f;
        private const float BAR_HEIGHT = 24f; // Height of each flame bar
        private const float MIN_WIDTH_TO_SHOW_TEXT = 40f; // Minimum width to show label

        // Theme colors as properties
        private Color NodeHover => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(80, 80, 90) : Color.FromArgb(180, 210, 255);
        private Color NodeBorder => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(120, 120, 130) : Color.FromArgb(80, 130, 200);
        private Color NodeText => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(220, 220, 220) : Color.FromArgb(20, 40, 80);

        // ??????????????????????????????????????????????????????????????????????
        // Constructor
        // ??????????????????????????????????????????????????????????????????????

        public FlameGraphPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BorderStyle = BorderStyle.None; // Modern flat design
            BackColor = ThemeManager.BackgroundColor;

            // Setup mouse events
            this.MouseWheel += FlameGraphPanel_MouseWheel;
            this.MouseDown += FlameGraphPanel_MouseDown;
            this.MouseMove += FlameGraphPanel_MouseMove;
            this.MouseUp += FlameGraphPanel_MouseUp;
            this.MouseClick += FlameGraphPanel_MouseClick;

            // Add tooltip for better UX
            var tooltip = new ToolTip();
            tooltip.SetToolTip(this, "[Flame Graph] Scroll to zoom, drag to pan, click to focus on a function");
        }

        // ??????????????????????????????????????????????????????????????????????
        // Public API
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Loads call stack data and builds the flame graph.
        /// </summary>
        /// <param name="callStack">Root nodes of the call stack.</param>
        public void LoadCallStack(List<CallStackNode> callStack)
        {
            _rootNodes.Clear();
            _selectedNode = null;
            _hoveredNode = null;
            _zoomedNode = null;
            _zoom = 1.0f;
            _panOffset = new PointF(0, 0);

            if (callStack == null || callStack.Count == 0)
            {
                Invalidate();
                return;
            }

            // Convert CallStackNode to FlameGraphNode
            foreach (var root in callStack)
            {
                var flameNode = ConvertToFlameNode(root);
                if (flameNode != null)
                    _rootNodes.Add(flameNode);
            }

            // Calculate positions
            CalculateLayout();
            Invalidate();
        }

        /// <summary>
        /// Resets the view to show all nodes at default zoom.
        /// </summary>
        public void ResetView()
        {
            _zoom = 1.0f;
            _panOffset = new PointF(0, 0);
            _zoomedNode = null;
            _selectedNode = null;
            Invalidate();
        }

        /// <summary>
        /// Zooms into a specific call stack branch.
        /// </summary>
        public void ZoomToNode(FlameGraphNode node)
        {
            _zoomedNode = node;
            _panOffset = new PointF(0, 0);
            CalculateLayout();
            Invalidate();
        }

        // ??????????????????????????????????????????????????????????????????????
        // Data Model
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Represents a node in the flame graph.
        /// </summary>
        public class FlameGraphNode
        {
            public string Name { get; set; }
            public long DurationMs { get; set; }
            public long SelfDurationMs { get; set; }
            public int Depth { get; set; }
            public List<FlameGraphNode> Children { get; set; } = new List<FlameGraphNode>();

            // Layout properties
            public RectangleF Bounds { get; set; }
            public Color Color { get; set; }

            // Source data
            public int LineNumber { get; set; }
            public CallStackNode SourceNode { get; set; }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Conversion from CallStackNode
        // ??????????????????????????????????????????????????????????????????????

        private FlameGraphNode ConvertToFlameNode(CallStackNode csNode, int depth = 0)
        {
            var node = new FlameGraphNode
            {
                Name = csNode.Label,
                DurationMs = csNode.DurationMs,
                SelfDurationMs = csNode.DurationMs, // Will calculate later
                Depth = depth,
                LineNumber = csNode.LineNumber,
                SourceNode = csNode,
                Color = GenerateColorFromName(csNode.Label)
            };

            // Convert children
            long childrenTotalDuration = 0;
            foreach (var child in csNode.Children)
            {
                var flameChild = ConvertToFlameNode(child, depth + 1);
                node.Children.Add(flameChild);
                childrenTotalDuration += flameChild.DurationMs;
            }

            // Calculate self duration (time spent in this function excluding children)
            node.SelfDurationMs = Math.Max(0, node.DurationMs - childrenTotalDuration);

            return node;
        }

        // ??????????????????????????????????????????????????????????????????????
        // Layout Calculation
        // ??????????????????????????????????????????????????????????????????????

        private void CalculateLayout()
        {
            if (_rootNodes.Count == 0)
                return;

            // Calculate total duration
            long totalDuration = _zoomedNode != null 
                ? _zoomedNode.DurationMs 
                : _rootNodes.Sum(n => n.DurationMs);

            if (totalDuration == 0)
                totalDuration = 1; // Avoid division by zero

            const float headerHeight = 65; // Header + legend space
            float width = this.ClientSize.Width - 20; // Padding
            float x = 10; // Start X position
            float startY = headerHeight + 10; // Start below header with padding

            // Layout root nodes or zoomed node
            var nodesToLayout = _zoomedNode != null 
                ? new List<FlameGraphNode> { _zoomedNode }
                : _rootNodes;

            foreach (var node in nodesToLayout)
            {
                LayoutNodeRecursive(node, x, startY, width, totalDuration);
                x += (node.DurationMs / (float)totalDuration) * width;
            }
        }

        private void LayoutNodeRecursive(FlameGraphNode node, float x, float y, float totalWidth, long totalDuration)
        {
            // Calculate width based on duration
            float nodeWidth = (node.DurationMs / (float)totalDuration) * totalWidth;

            // Set node bounds
            node.Bounds = new RectangleF(x, y, nodeWidth, BAR_HEIGHT);

            // Layout children below this node
            if (node.Children.Count > 0)
            {
                float childX = x;
                long childTotalDuration = node.Children.Sum(c => c.DurationMs);

                if (childTotalDuration == 0)
                    childTotalDuration = 1;

                foreach (var child in node.Children)
                {
                    float childWidth = (child.DurationMs / (float)childTotalDuration) * nodeWidth;
                    LayoutNodeRecursive(child, childX, y + BAR_HEIGHT, childWidth, childTotalDuration);
                    childX += childWidth;
                }
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
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (_rootNodes.Count == 0)
            {
                DrawEmptyState(g);
                return;
            }

            // Draw title/header first (fixed, no transform)
            DrawTitle(g);

            // Save original transform
            var originalTransform = g.Transform.Clone();

            // Apply zoom and pan for content area
            // Content starts at Y=65 (header 35px + legend 30px)
            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            // Draw nodes
            var nodesToDraw = _zoomedNode != null 
                ? new List<FlameGraphNode> { _zoomedNode }
                : _rootNodes;

            foreach (var node in nodesToDraw)
            {
                DrawNodeRecursive(g, node);
            }

            // Restore transform for any post-drawing
            g.Transform = originalTransform;
        }

        private void DrawNodeRecursive(Graphics g, FlameGraphNode node)
        {
            // Determine colors
            bool isHovered = node == _hoveredNode;
            bool isSelected = node == _selectedNode;

            Color fillColor = isHovered ? NodeHover : node.Color;
            Color borderColor = isSelected ? Color.Red : NodeBorder;
            float borderWidth = isSelected ? 2f : 1f;

            // Draw rectangle
            using (var brush = new SolidBrush(fillColor))
            using (var pen = new Pen(borderColor, borderWidth))
            {
                g.FillRectangle(brush, node.Bounds);
                g.DrawRectangle(pen, Rectangle.Round(node.Bounds));
            }

            // Draw text if wide enough
            if (node.Bounds.Width > MIN_WIDTH_TO_SHOW_TEXT)
            {
                string label = node.Name;
                string durationText = $"{node.DurationMs}ms";

                // Truncate if too long
                using (var font = new Font("Segoe UI", 8f))
                using (var brush = new SolidBrush(NodeText))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    // Draw function name
                    var textRect = new RectangleF(
                        node.Bounds.X + 4, 
                        node.Bounds.Y, 
                        node.Bounds.Width - 60, 
                        node.Bounds.Height);

                    g.DrawString(label, font, brush, textRect, format);

                    // Draw duration (right-aligned)
                    var durationRect = new RectangleF(
                        node.Bounds.Right - 56,
                        node.Bounds.Y,
                        50,
                        node.Bounds.Height);

                    format.Alignment = StringAlignment.Far;
                    g.DrawString(durationText, font, brush, durationRect, format);
                }
            }

            // Draw children
            foreach (var child in node.Children)
            {
                DrawNodeRecursive(g, child);
            }
        }

        private void DrawEmptyState(Graphics g)
        {
            // Note: Already called without transform

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

            // Draw flame icon
            using (var iconFont = new Font("Segoe UI", 48f, FontStyle.Bold))
            using (var iconBrush = new SolidBrush(Color.FromArgb(255, 140, 0))) // Orange
            {
                var iconText = "[FLAME]";
                var iconSize = g.MeasureString(iconText, iconFont);
                g.DrawString(iconText, iconFont, iconBrush, 
                    cardX + (cardWidth - iconSize.Width) / 2, cardY + 20);
            }

            // Draw title
            using (var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                var titleText = "Flame Graph Visualization";
                var titleSize = g.MeasureString(titleText, titleFont);
                g.DrawString(titleText, titleFont, titleBrush,
                    cardX + (cardWidth - titleSize.Width) / 2, cardY + 100);
            }

            // Draw subtitle
            using (var subFont = new Font("Segoe UI", 10f))
            using (var subBrush = new SolidBrush(Color.FromArgb(180, ThemeManager.ForegroundColor)))
            {
                var subText = "No performance data to visualize";
                var subSize = g.MeasureString(subText, subFont);
                g.DrawString(subText, subFont, subBrush,
                    cardX + (cardWidth - subSize.Width) / 2, cardY + 135);
            }

            // Draw instructions
            var instructions = new[]
            {
                ">> Open a log file with performance data to get started",
                "",
                "[?] What is a Flame Graph?",
                "- Visual profiling: See where time is spent",
                "- Width = Time spent in function",
                "- Height = Call stack depth",
                "- Color = Different functions (for distinction)",
                "",
                "[>] How to Use:",
                "- Hover over bars to see details",
                "- Click a bar to zoom into that function",
                "- Mouse wheel to zoom in/out",
                "- Drag to pan around",
                "- Right-click to reset view"
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
            // Note: Already called without transform

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
            string title = _zoomedNode != null 
                ? $"?? Flame Graph - Zoomed: {_zoomedNode.Name}"
                : "?? Flame Graph - Performance Profiling";

            using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                g.DrawString(title, font, brush, 12, 8);
            }

            // Draw interactive instructions (top right of header)
            string instructions = "??? Scroll: Zoom - Drag: Pan - Click: Focus - Right-Click: Reset";
            using (var font = new Font("Segoe UI", 8f))
            using (var brush = new SolidBrush(Color.FromArgb(150, ThemeManager.ForegroundColor)))
            {
                var size = g.MeasureString(instructions, font);
                g.DrawString(instructions, font, brush, this.Width - size.Width - 12, 10);
            }

            // Draw zoom level indicator (below title on left - no overlap)
            if (_zoom != 1.0f)
            {
                var zoomText = $"[ZOOM] {_zoom:F1}x";
                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
                {
                    g.DrawString(zoomText, font, brush, 350, 25);
                }
            }

            // Draw legend if data exists
            if (_rootNodes.Count > 0)
            {
                var legendY = 40;
                DrawLegend(g, 12, legendY);
            }
        }

        /// <summary>
        /// Draws a color legend for the flame graph.
        /// </summary>
        private void DrawLegend(Graphics g, int x, int y)
        {
            var legendItems = new[]
            {
                ("Width = Time", Color.Empty),
                ("Height = Depth", Color.Empty),
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
        // Color Generation
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Generates a consistent color for a function name.
        /// Same function always gets the same color.
        /// </summary>
        private Color GenerateColorFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Color.LightGray;

            // Use hash code to generate consistent color
            int hash = name.GetHashCode();
            Random rnd = new Random(hash);

            // Generate pastel colors for better readability
            int r = rnd.Next(150, 255);
            int g = rnd.Next(150, 255);
            int b = rnd.Next(150, 255);

            // Adjust for dark theme
            if (ThemeManager.CurrentTheme == ThemeManager.Theme.Dark)
            {
                r = rnd.Next(60, 180);
                g = rnd.Next(60, 180);
                b = rnd.Next(60, 180);
            }

            return Color.FromArgb(r, g, b);
        }

        // ??????????????????????????????????????????????????????????????????????
        // Mouse Events
        // ??????????????????????????????????????????????????????????????????????

        private void FlameGraphPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom in/out
            float delta = e.Delta > 0 ? 1.1f : 0.9f;
            float newZoom = _zoom * delta;

            if (newZoom >= MIN_ZOOM && newZoom <= MAX_ZOOM)
            {
                _zoom = newZoom;
                Invalidate();
            }
        }

        private void FlameGraphPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePos = e.Location;
                Cursor = Cursors.Hand;
            }
        }

        private void FlameGraphPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Pan with boundaries to prevent graphics showing in wrong places
                float deltaX = e.X - _lastMousePos.X;
                float deltaY = e.Y - _lastMousePos.Y;

                float newPanX = _panOffset.X + deltaX;
                float newPanY = _panOffset.Y + deltaY;

                // Constrain pan to reasonable boundaries
                // Allow some panning but keep content mostly visible
                float maxPanX = this.Width * 0.3f;
                float maxPanY = 100f; // Keep header area clear
                float minPanX = -this.Width * Math.Max(0, _zoom - 1);
                float minPanY = -this.Height * Math.Max(0, _zoom - 1);

                _panOffset.X = Math.Max(minPanX, Math.Min(maxPanX, newPanX));
                _panOffset.Y = Math.Max(minPanY, Math.Min(maxPanY, newPanY));

                _lastMousePos = e.Location;
                Invalidate();
            }
            else
            {
                // Check for hover
                var node = FindNodeAtPoint(e.Location);
                if (node != _hoveredNode)
                {
                    _hoveredNode = node;
                    Invalidate();

                    // Show tooltip
                    if (node != null)
                    {
                        string tooltip = $"{node.Name}\n" +
                                       $"Duration: {node.DurationMs}ms\n" +
                                       $"Self: {node.SelfDurationMs}ms\n" +
                                       $"Line: {node.LineNumber}";
                        this.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void FlameGraphPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            Cursor = Cursors.Default;
        }

        private void FlameGraphPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var node = FindNodeAtPoint(e.Location);
                if (node != null)
                {
                    // Zoom into this node
                    ZoomToNode(node);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Reset view
                ResetView();
            }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Hit Testing
        // ??????????????????????????????????????????????????????????????????????

        private FlameGraphNode FindNodeAtPoint(Point screenPoint)
        {
            // Convert screen point to graph space
            float graphX = (screenPoint.X - _panOffset.X) / _zoom;
            float graphY = (screenPoint.Y - _panOffset.Y) / _zoom;
            PointF graphPoint = new PointF(graphX, graphY);

            var nodesToSearch = _zoomedNode != null 
                ? new List<FlameGraphNode> { _zoomedNode }
                : _rootNodes;

            foreach (var node in nodesToSearch)
            {
                var found = FindNodeAtPointRecursive(node, graphPoint);
                if (found != null)
                    return found;
            }

            return null;
        }

        private FlameGraphNode FindNodeAtPointRecursive(FlameGraphNode node, PointF point)
        {
            if (node.Bounds.Contains(point))
                return node;

            foreach (var child in node.Children)
            {
                var found = FindNodeAtPointRecursive(child, point);
                if (found != null)
                    return found;
            }

            return null;
        }

        // ??????????????????????????????????????????????????????????????????????
        // Export
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Exports the flame graph as an image.
        /// </summary>
        public Bitmap ExportAsImage(int width = 1920, int height = 1080)
        {
            var bitmap = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Temporarily adjust zoom to fit
                float oldZoom = _zoom;
                var oldPan = _panOffset;

                _zoom = 1.0f;
                _panOffset = new PointF(0, 0);

                // Recalculate layout for export size
                float oldWidth = this.Width;
                typeof(Control).GetProperty("Width").SetValue(this, width);
                CalculateLayout();

                // Draw
                var nodesToDraw = _zoomedNode != null 
                    ? new List<FlameGraphNode> { _zoomedNode }
                    : _rootNodes;

                foreach (var node in nodesToDraw)
                {
                    DrawNodeRecursive(g, node);
                }

                DrawTitle(g);

                // Restore
                _zoom = oldZoom;
                _panOffset = oldPan;
                typeof(Control).GetProperty("Width").SetValue(this, oldWidth);
                CalculateLayout();
            }

            return bitmap;
        }
    }

    /// <summary>
    /// Extension methods for Graphics to support rounded rectangles.
    /// </summary>
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using (var path = GetRoundedRectPath(x, y, width, height, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using (var path = GetRoundedRectPath(x, y, width, height, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundedRectPath(float x, float y, float width, float height, float radius)
        {
            var path = new GraphicsPath();
            float diameter = radius * 2;
            var arc = new RectangleF(x, y, diameter, diameter);

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = x + width - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = y + height - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = x;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}

