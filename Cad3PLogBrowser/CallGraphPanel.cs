using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Custom panel that renders the API call graph.
    /// Supports mouse-wheel zoom and drag-to-pan.
    /// Nodes are laid out in a circular arrangement; edges are drawn as arrows
    /// with weight-scaled thickness.
    /// </summary>
    public class CallGraphPanel : Panel
    {
        // ── State ─────────────────────────────────────────────────────────────
        private CallGraph _graph;
        private float     _zoom         = 1.0f;
        private PointF    _panOffset    = new PointF(0, 0);
        private Point     _lastMousePos;
        private bool      _isDragging   = false;
        private string    _hoveredNode  = null;

        // ── Layout constants ──────────────────────────────────────────────────
        private const float NodeWidth    = 120f;
        private const float NodeHeight   = 32f;
        private const float NodeRadius   = NodeHeight / 2f;
        private const float MinZoom      = 0.2f;
        private const float MaxZoom      = 4.0f;

        // ── Colours (Theme-aware) ─────────────────────────────────────────────
        private Color NodeFill      => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(60, 60, 65) : Color.FromArgb(220, 235, 255);
        private Color NodeBorder    => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(120, 120, 130) : Color.FromArgb(80, 130, 200);
        private Color NodeHover     => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(80, 80, 90) : Color.FromArgb(180, 210, 255);
        private Color NodeText      => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(220, 220, 220) : Color.FromArgb(20, 40, 80);
        private Color EdgeColour    => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(100, 100, 110) : Color.FromArgb(120, 140, 170);
        private Color EdgeHighlight => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(200, 100, 80) : Color.FromArgb(200, 80, 60);
        private Color GraphBackground => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
            ? Color.FromArgb(30, 30, 30) : Color.FromArgb(245, 248, 252);

        public CallGraphPanel()
        {
            DoubleBuffered       = true;
            ResizeRedraw         = true;
            BorderStyle          = BorderStyle.None;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(GraphBackground);
        }

        // ── Public API ────────────────────────────────────────────────────────
        public void LoadGraph(CallGraph graph)
        {
            _graph     = graph;

            if (graph != null && graph.Nodes.Count > 0)
            {
                LayoutNodes();
                AutoFitZoom();
                _panOffset = new PointF(0, 0);
            }
            else
            {
                _zoom = 1.0f;
                _panOffset = new PointF(0, 0);
            }

            Invalidate();
        }

        // Issue Fix: Calculate optimal zoom to fit all nodes on screen
        private void AutoFitZoom()
        {
            if (_graph == null || _graph.Nodes.Count == 0)
            {
                _zoom = 1.0f;
                return;
            }

            // Find bounding box of all nodes
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var node in _graph.Nodes.Values)
            {
                minX = Math.Min(minX, node.X - NodeWidth / 2f);
                maxX = Math.Max(maxX, node.X + NodeWidth / 2f);
                minY = Math.Min(minY, node.Y - NodeHeight / 2f);
                maxY = Math.Max(maxY, node.Y + NodeHeight / 2f);
            }

            float graphWidth = maxX - minX;
            float graphHeight = maxY - minY;

            if (graphWidth < 1f || graphHeight < 1f)
            {
                _zoom = 1.0f;
                return;
            }

            // Calculate zoom to fit with some padding
            float panelWidth = this.Width - 40;  // 20px padding on each side
            float panelHeight = this.Height - 60; // Extra padding for legend

            float zoomX = panelWidth / graphWidth;
            float zoomY = panelHeight / graphHeight;

            _zoom = Math.Min(zoomX, zoomY);
            _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * 0.9f)); // 90% to add margin
        }

        public void ResetView()
        {
            _panOffset = new PointF(0, 0);
            AutoFitZoom();
            Invalidate();
        }

        // ── Layout ────────────────────────────────────────────────────────────
        private void LayoutNodes()
        {
            if (_graph == null || _graph.Nodes.Count == 0) return;

            var nodes = new List<CallGraphNode>(_graph.Nodes.Values);
            int count = nodes.Count;

            // Circular layout — radius scales with node count
            float radius = Math.Max(150f, count * 45f);
            float cx = 0f;
            float cy = 0f;

            for (int i = 0; i < count; i++)
            {
                double angle = 2 * Math.PI * i / count - Math.PI / 2;
                nodes[i].X = cx + radius * (float)Math.Cos(angle);
                nodes[i].Y = cy + radius * (float)Math.Sin(angle);
            }
        }

        // ── Painting ──────────────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (_graph == null || _graph.Nodes.Count == 0)
            {
                DrawEmptyMessage(g);
                return;
            }

            // Save original state for legend drawing
            var state = g.Save();

            // Apply pan + zoom transform for graph elements
            g.TranslateTransform(
                Width  / 2f + _panOffset.X,
                Height / 2f + _panOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            // Draw edges first (behind nodes)
            DrawEdges(g);

            // Draw center crosshair for reference (helps with debugging positioning)
            using (var pen = new Pen(ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                ? Color.FromArgb(60, 60, 65) 
                : Color.FromArgb(200, 200, 210), 1f))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawLine(pen, -20, 0, 20, 0);  // Horizontal
                g.DrawLine(pen, 0, -20, 0, 20);  // Vertical
            }

            // Draw nodes on top
            DrawNodes(g);

            // Restore transform for legend (draw in screen space)
            g.Restore(state);

            // Draw legend and status info
            DrawLegend(g);
            DrawDebugInfo(g);
        }

        private void DrawDebugInfo(Graphics g)
        {
            if (_graph == null) return;

            // Show graph statistics in top-left corner
            using (var font = new Font("Consolas", 8f))
            using (var brush = new SolidBrush(ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                ? Color.FromArgb(180, 180, 190) 
                : Color.FromArgb(80, 80, 90)))
            using (var bgBrush = new SolidBrush(ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                ? Color.FromArgb(200, 40, 40, 45)
                : Color.FromArgb(200, 255, 255, 255)))
            {
                var lines = new List<string>
                {
                    $"Call Graph Debug Info:",
                    $"├─ Nodes: {_graph.Nodes.Count}",
                    $"├─ Edges: {_graph.Edges.Count}",
                    $"├─ Zoom: {_zoom:F2}x ({_zoom * 100:F0}%)",
                    $"├─ Pan: X={_panOffset.X:F0}, Y={_panOffset.Y:F0}",
                    $"├─ Panel: {Width}x{Height}",
                    $"└─ Status: " + (_graph.Nodes.Count > 0 ? "✓ Graph loaded" : "⚠ No data")
                };

                // Draw background
                float lineHeight = font.Height;
                var bgRect = new RectangleF(5, 5, 280, lines.Count * lineHeight + 10);
                g.FillRectangle(bgBrush, bgRect);

                // Draw text
                float y = 8;
                foreach (var line in lines)
                {
                    g.DrawString(line, font, brush, 8, y);
                    y += lineHeight;
                }
            }
        }

        private void DrawEmptyMessage(Graphics g)
        {
            using (var titleFont = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (var textFont = new Font("Segoe UI", 9.5f))
            using (var brush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // Title
                string title = "📊 Call Graph Visualization";
                g.DrawString(title, titleFont, brush, Width / 2f, Height / 2f - 80, format);

                // Main message
                string msg = "No call graph data available.\n\n" +
                             "To generate a call graph, your log file must contain:\n" +
                             "• API calls marked with [ENTER] and [EXIT]\n" +
                             "• Format: [ENTER] FunctionName() or [EXIT] FunctionName()\n\n" +
                             "The graph will show:\n" +
                             "• Nodes = API functions\n" +
                             "• Arrows = Calls between APIs\n" +
                             "• Thickness = Call frequency\n\n" +
                             "Once loaded, you can:\n" +
                             "• Scroll to zoom in/out\n" +
                             "• Drag to pan the graph\n" +
                             "• Click nodes for details\n" +
                             "• Hover to highlight connections";

                var rect = new RectangleF(50, Height / 2f - 40, Width - 100, Height / 2f);
                g.DrawString(msg, textFont, brush, rect, format);
            }
        }

        private void DrawEdges(Graphics g)
        {
            if (_graph.Edges.Count == 0) return;

            // Find max weight for scaling
            int maxWeight = 1;
            foreach (var edge in _graph.Edges.Values)
                if (edge.Weight > maxWeight) maxWeight = edge.Weight;

            foreach (var edge in _graph.Edges.Values)
            {
                if (!_graph.Nodes.TryGetValue(edge.Caller, out var callerNode)) continue;
                if (!_graph.Nodes.TryGetValue(edge.Callee, out var calleeNode)) continue;

                bool isHighlighted = edge.Caller == _hoveredNode || edge.Callee == _hoveredNode;
                float thickness    = 1f + 3f * edge.Weight / maxWeight;
                Color colour       = isHighlighted ? EdgeHighlight : EdgeColour;

                DrawArrow(g, callerNode, calleeNode, thickness, colour, edge.Weight);
            }
        }

        private void DrawArrow(Graphics g, CallGraphNode from, CallGraphNode to,
            float thickness, Color colour, int weight)
        {
            // Calculate edge start/end at node boundaries
            float dx   = to.X - from.X;
            float dy   = to.Y - from.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1f) return;

            float nx = dx / dist;
            float ny = dy / dist;

            // Start at edge of source node, end at edge of target node
            float startX = from.X + nx * (NodeWidth / 2f);
            float startY = from.Y + ny * (NodeHeight / 2f);
            float endX   = to.X   - nx * (NodeWidth / 2f + 6f);
            float endY   = to.Y   - ny * (NodeHeight / 2f + 6f);

            // Self-loop
            if (from.Name == to.Name)
            {
                DrawSelfLoop(g, from, colour, thickness);
                return;
            }

            using (var pen = new Pen(colour, thickness))
            {
                pen.CustomEndCap = new AdjustableArrowCap(5, 5);
                g.DrawLine(pen, startX, startY, endX, endY);
            }

            // Weight label on edge midpoint
            if (weight > 1)
            {
                float midX = (startX + endX) / 2f;
                float midY = (startY + endY) / 2f;
                using (var font  = new Font("Segoe UI", 7f))
                using (var brush = new SolidBrush(colour))
                    g.DrawString(weight.ToString(), font, brush, midX + 3, midY - 10);
            }
        }

        private void DrawSelfLoop(Graphics g, CallGraphNode node, Color colour, float thickness)
        {
            float loopSize = 30f;
            var rect = new RectangleF(node.X + NodeWidth / 2f, node.Y - loopSize,
                loopSize, loopSize);
            using (var pen = new Pen(colour, thickness))
                g.DrawEllipse(pen, rect);
        }

        private void DrawNodes(Graphics g)
        {
            using (var borderPen  = new Pen(NodeBorder, 1.5f))
            using (var normalBrush = new SolidBrush(NodeFill))
            using (var hoverBrush  = new SolidBrush(NodeHover))
            using (var textBrush   = new SolidBrush(NodeText))
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            {
                var sf = new StringFormat
                {
                    Alignment     = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming      = StringTrimming.EllipsisCharacter
                };

                foreach (var node in _graph.Nodes.Values)
                {
                    bool isHovered = node.Name == _hoveredNode;
                    var rect = new RectangleF(
                        node.X - NodeWidth  / 2f,
                        node.Y - NodeHeight / 2f,
                        NodeWidth, NodeHeight);

                    // Shadow
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                    {
                        var shadowRect = rect;
                        shadowRect.Offset(2, 2);
                        FillRoundedRect(g, shadowBrush, shadowRect, NodeRadius);
                    }

                    // Node fill
                    FillRoundedRect(g, isHovered ? hoverBrush : normalBrush, rect, NodeRadius);
                    DrawRoundedRect(g, borderPen, rect, NodeRadius);

                    // Label — truncate to fit
                    g.DrawString(node.Name, font, textBrush, rect, sf);
                }
            }
        }

        private static void FillRoundedRect(Graphics g, Brush brush, RectangleF rect, float r)
        {
            using (var path = RoundedRectPath(rect, r))
                g.FillPath(brush, path);
        }

        private static void DrawRoundedRect(Graphics g, Pen pen, RectangleF rect, float r)
        {
            using (var path = RoundedRectPath(rect, r))
                g.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRectPath(RectangleF rect, float r)
        {
            float d = r * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DrawLegend(Graphics g)
        {
            using (var font = new Font("Segoe UI", 8f))
            using (var brush = new SolidBrush(ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                ? Color.FromArgb(160, 160, 170)   // Light gray for dark theme
                : Color.FromArgb(100, 100, 120))) // Dark gray for light theme
            {
                int nodeCount = _graph?.Nodes.Count ?? 0;
                int edgeCount = _graph?.Edges.Count ?? 0;

                string legend = nodeCount > 0 
                    ? $"{nodeCount} APIs, {edgeCount} Calls  •  Scroll=Zoom  •  Drag=Pan  •  Click=Info  •  DblClick=Focus"
                    : "Scroll to zoom  •  Drag to pan  •  Click/Double-click nodes for details";
                g.DrawString(legend, font, brush, 8, Height - 20);

                string zoomLabel = string.Format("Zoom: {0:P0}", _zoom);
                var sz = g.MeasureString(zoomLabel, font);
                g.DrawString(zoomLabel, font, brush, Width - sz.Width - 8, Height - 20);
            }
        }

        // ── Mouse interactions ────────────────────────────────────────────────
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            float delta = e.Delta > 0 ? 1.1f : 0.9f;
            _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * delta));
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                // Check if clicking on a node
                string hitNode = HitTestNode(e.Location);
                if (hitNode != null)
                {
                    // Show node info
                    ShowNodeInfo(hitNode, e.Location);
                    return;
                }

                _isDragging   = true;
                _lastMousePos = e.Location;
                Cursor        = Cursors.SizeAll;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // Double-click to zoom and center on node
            string hitNode = HitTestNode(e.Location);
            if (hitNode != null && _graph != null && _graph.Nodes.TryGetValue(hitNode, out var node))
            {
                // Center on this node
                _panOffset.X = -node.X * _zoom;
                _panOffset.Y = -node.Y * _zoom;

                // Zoom in a bit
                _zoom = Math.Min(MaxZoom, _zoom * 1.5f);

                Invalidate();
            }
        }

        private void ShowNodeInfo(string nodeName, Point location)
        {
            if (_graph == null || !_graph.Nodes.ContainsKey(nodeName)) return;

            // Count incoming and outgoing calls
            int incomingCalls = 0;
            int outgoingCalls = 0;
            var callers = new List<string>();
            var callees = new List<string>();

            foreach (var edge in _graph.Edges.Values)
            {
                if (edge.Callee == nodeName)
                {
                    incomingCalls += edge.Weight;
                    if (!callers.Contains(edge.Caller))
                        callers.Add(edge.Caller);
                }
                if (edge.Caller == nodeName)
                {
                    outgoingCalls += edge.Weight;
                    if (!callees.Contains(edge.Callee))
                        callees.Add(edge.Callee);
                }
            }

            string info = $"API: {nodeName}\n\n" +
                         $"Calls TO this API: {incomingCalls} (from {callers.Count} APIs)\n" +
                         $"Calls FROM this API: {outgoingCalls} (to {callees.Count} APIs)";

            if (callers.Count > 0)
                info += $"\n\nCallers: {string.Join(", ", callers.ToArray())}";
            if (callees.Count > 0)
                info += $"\n\nCalls: {string.Join(", ", callees.ToArray())}";

            MessageBox.Show(info, "Call Graph Node Details", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
            Cursor      = Cursors.Default;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                _panOffset.X += e.X - _lastMousePos.X;
                _panOffset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                Invalidate();
                return;
            }

            // Hover detection — convert mouse to graph coords
            string hitNode = HitTestNode(e.Location);
            if (hitNode != _hoveredNode)
            {
                _hoveredNode = hitNode;
                Cursor       = hitNode != null ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredNode = null;
            Invalidate();
        }

        private string HitTestNode(Point screenPt)
        {
            if (_graph == null) return null;

            // Convert screen coords → graph coords
            float gx = (screenPt.X - Width  / 2f - _panOffset.X) / _zoom;
            float gy = (screenPt.Y - Height / 2f - _panOffset.Y) / _zoom;

            foreach (var node in _graph.Nodes.Values)
            {
                if (Math.Abs(gx - node.X) <= NodeWidth  / 2f &&
                    Math.Abs(gy - node.Y) <= NodeHeight / 2f)
                    return node.Name;
            }
            return null;
        }
    }
}
