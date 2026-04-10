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
            BorderStyle = BorderStyle.Fixed3D;
            BackColor = Color.White;

            // Setup mouse events
            this.MouseWheel += FlameGraphPanel_MouseWheel;
            this.MouseDown += FlameGraphPanel_MouseDown;
            this.MouseMove += FlameGraphPanel_MouseMove;
            this.MouseUp += FlameGraphPanel_MouseUp;
            this.MouseClick += FlameGraphPanel_MouseClick;
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

            float width = this.ClientSize.Width - 20; // Padding
            float x = 10; // Start X position

            // Layout root nodes or zoomed node
            var nodesToLayout = _zoomedNode != null 
                ? new List<FlameGraphNode> { _zoomedNode }
                : _rootNodes;

            foreach (var node in nodesToLayout)
            {
                LayoutNodeRecursive(node, x, 10, width, totalDuration);
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

            // Apply zoom and pan
            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            if (_rootNodes.Count == 0)
            {
                DrawEmptyState(g);
                return;
            }

            // Draw nodes
            var nodesToDraw = _zoomedNode != null 
                ? new List<FlameGraphNode> { _zoomedNode }
                : _rootNodes;

            foreach (var node in nodesToDraw)
            {
                DrawNodeRecursive(g, node);
            }

            // Draw title
            DrawTitle(g);
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
            g.ResetTransform();
            string message = "No performance data available.\nLoad a log file to see the flame graph.";

            using (var font = new Font("Segoe UI", 10f))
            using (var brush = new SolidBrush(Color.Gray))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(message, font, brush, 
                    new RectangleF(0, 0, this.Width, this.Height), format);
            }
        }

        private void DrawTitle(Graphics g)
        {
            g.ResetTransform();

            string title = _zoomedNode != null 
                ? $"Flame Graph - Zoomed: {_zoomedNode.Name}"
                : "Flame Graph - All Functions";

            using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                g.DrawString(title, font, brush, 10, 10);
            }

            // Draw instructions
            string instructions = "Hover: Details | Click: Zoom | Right-click: Reset | Wheel: Zoom | Drag: Pan";
            using (var font = new Font("Segoe UI", 7f))
            using (var brush = new SolidBrush(Color.Gray))
            {
                g.DrawString(instructions, font, brush, 10, this.Height - 20);
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
                // Pan
                _panOffset.X += e.X - _lastMousePos.X;
                _panOffset.Y += e.Y - _lastMousePos.Y;
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
}
