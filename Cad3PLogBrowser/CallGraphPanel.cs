using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Professional call-graph panel with two view modes:
    ///   Weighted   — edge thickness and node heat colour scale with call frequency
    ///   Structural — flat colours, uniform edges (who-calls-whom, no weights)
    /// Features: dot-grid background, gradient nodes, heat-map colouring,
    ///           arrowhead edges with weight badges, zoom/pan, hover highlight,
    ///           status bar, legend.
    /// </summary>
    public class CallGraphPanel : Panel
    {
        // ?? State ?????????????????????????????????????????????????????????????
        private CallGraph _graph;
        private float     _zoom      = 1.0f;
        private PointF    _pan       = new PointF(0, 0);
        private Point     _lastMouse;
        private bool      _dragging;
        private string    _hoveredNode;
        private bool      _structuralView = false;

        // ?? Layout constants ??????????????????????????????????????????????????
        private const float NW      = 140f;
        private const float NH      = 34f;
        private const float NR      = NH / 2f;
        private const float MinZoom = 0.15f;
        private const float MaxZoom = 5.0f;

        // ?? Theme helpers ?????????????????????????????????????????????????????
        private bool Dark => ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
        private Color BgColor   => Dark ? Color.FromArgb(22, 24, 30)    : Color.FromArgb(243, 246, 251);
        private Color GridColor => Dark ? Color.FromArgb(38, 42, 52)    : Color.FromArgb(218, 224, 238);
        private Color TextColor => Dark ? Color.FromArgb(228, 232, 240) : Color.FromArgb(25, 35, 65);
        private Color EdgeBase  => Dark ? Color.FromArgb(85, 95, 120)   : Color.FromArgb(145, 162, 200);
        private Color EdgeHot   => Color.FromArgb(255, 145, 40);
        private Color BorderSel => Color.FromArgb(0, 150, 230);

        // Heat palette: cool (few calls) ? hot (many calls)
        private static readonly Color[] Heat =
        {
            Color.FromArgb(50,  95, 170),   // cool  – blue
            Color.FromArgb(30, 165, 125),   // warm  – teal
            Color.FromArgb(205,175,  28),   // hot   – amber
            Color.FromArgb(215,  55,  50),  // very hot – red
        };

        // ?? Welcome panel ?????????????????????????????????????????????????????
        private Managers.VisualizationWelcomePanel _welcomePanel;

        public bool IsStructuralView => _structuralView;

        // ?? Constructor ???????????????????????????????????????????????????????
        public CallGraphPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw   = true;
            BorderStyle    = BorderStyle.None;

            _welcomePanel = new Managers.VisualizationWelcomePanel(
                "Call Graph",
                "Visualize API call relationships and frequencies",
                new[]
                {
                    "? Nodes represent API functions",
                    "? Arrows show caller ? callee direction",
                    "? Node colour = call heat  (blue ? red)",
                    "? Edge thickness = call frequency",
                    "? Scroll to zoom  |  Drag to pan",
                    "? Hover to highlight connections",
                    "? Click a node for details",
                    "? Toggle Weighted / Structural view"
                },
                Color.FromArgb(0, 140, 220));
            _welcomePanel.Visible = false;
            Controls.Add(_welcomePanel);
        }

        // ?? Public API ????????????????????????????????????????????????????????
        public void LoadGraph(CallGraph graph)
        {
            _graph = graph;
            bool hasData = graph != null && graph.Nodes.Count > 0;
            _welcomePanel.Visible = !hasData;
            if (hasData) { LayoutNodes(); FitToWindow(); }
            Invalidate();
        }

        public void ResetView() { FitToWindow(); Invalidate(); }

        public void ToggleViewMode() { _structuralView = !_structuralView; Invalidate(); }

        // ?? Layout ????????????????????????????????????????????????????????????
        private void LayoutNodes()
        {
            if (_graph == null || _graph.Nodes.Count == 0) return;
            var nodes = new List<CallGraphNode>(_graph.Nodes.Values);
            int n = nodes.Count;
            float radius = Math.Max(180f, n * 52f);
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * i / n - Math.PI / 2;
                nodes[i].X = radius * (float)Math.Cos(a);
                nodes[i].Y = radius * (float)Math.Sin(a);
            }
        }

        private void FitToWindow()
        {
            if (_graph == null || _graph.Nodes.Count == 0) { _zoom = 1f; _pan = PointF.Empty; return; }
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var nd in _graph.Nodes.Values)
            {
                minX = Math.Min(minX, nd.X - NW / 2); maxX = Math.Max(maxX, nd.X + NW / 2);
                minY = Math.Min(minY, nd.Y - NH / 2); maxY = Math.Max(maxY, nd.Y + NH / 2);
            }
            float gw = maxX - minX, gh = maxY - minY;
            if (gw < 1 || gh < 1) { _zoom = 1f; _pan = PointF.Empty; return; }
            float zx = (Width  - 60) / gw;
            float zy = (Height - 80) / gh;
            _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, Math.Min(zx, zy) * 0.88f));
            _pan  = PointF.Empty;
        }

        // ?? Painting ??????????????????????????????????????????????????????????
        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(BgColor);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_graph == null || _graph.Nodes.Count == 0) return;

            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            DrawGrid(g);

            var state = g.Save();
            g.TranslateTransform(Width / 2f + _pan.X, Height / 2f + _pan.Y);
            g.ScaleTransform(_zoom, _zoom);
            DrawEdges(g);
            DrawNodes(g);
            g.Restore(state);

            DrawStatusBar(g);
            DrawLegend(g);
        }

        // ?? Grid ??????????????????????????????????????????????????????????????
        private void DrawGrid(Graphics g)
        {
            float step = Math.Max(18f, 60f * _zoom);
            float ox = (Width  / 2f + _pan.X) % step;
            float oy = (Height / 2f + _pan.Y) % step;
            using (var pen = new Pen(GridColor, 1f))
            {
                for (float x = ox; x < Width;  x += step) g.DrawLine(pen, x, 0, x, Height);
                for (float y = oy; y < Height; y += step) g.DrawLine(pen, 0, y, Width, y);
            }
        }

        // ?? Edges ?????????????????????????????????????????????????????????????
        private static readonly AdjustableArrowCap Arrow = new AdjustableArrowCap(5f, 5f);

        private void DrawEdges(Graphics g)
        {
            if (_graph.Edges.Count == 0) return;
            int maxW = _graph.Edges.Values.Max(ed => ed.Weight);

            foreach (var ed in _graph.Edges.Values)
            {
                if (!_graph.Nodes.TryGetValue(ed.Caller, out var src)) continue;
                if (!_graph.Nodes.TryGetValue(ed.Callee, out var dst)) continue;
                if (src.Name == dst.Name) { DrawSelfLoop(g, src, ed.Weight, maxW); continue; }

                bool hot  = ed.Caller == _hoveredNode || ed.Callee == _hoveredNode;
                float t   = _structuralView ? 1.5f : Math.Max(1f, 4f * ed.Weight / maxW);
                Color col = hot ? EdgeHot : BlendColor(EdgeBase, Color.FromArgb(175, 198, 228), (float)ed.Weight / maxW);

                float dx = dst.X - src.X, dy = dst.Y - src.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist < 1f) continue;
                float nx = dx / dist, ny = dy / dist;

                float x1 = src.X + nx * (NW / 2f + 2);
                float y1 = src.Y + ny * (NH / 2f + 2);
                float x2 = dst.X - nx * (NW / 2f + 11);
                float y2 = dst.Y - ny * (NH / 2f + 11);

                using (var pen = new Pen(col, t) { CustomEndCap = Arrow })
                    g.DrawLine(pen, x1, y1, x2, y2);

                if (!_structuralView && ed.Weight > 1)
                    DrawEdgeBadge(g, (x1 + x2) / 2f, (y1 + y2) / 2f, ed.Weight, col);
            }
        }

        private void DrawEdgeBadge(Graphics g, float mx, float my, int weight, Color col)
        {
            string lbl = weight.ToString();
            using (var f = new Font("Segoe UI", 7f, FontStyle.Bold))
            {
                var sz = g.MeasureString(lbl, f);
                var br = new RectangleF(mx - sz.Width / 2 - 2, my - sz.Height / 2 - 1, sz.Width + 4, sz.Height + 2);
                using (var bg = new SolidBrush(Color.FromArgb(210, BgColor.R, BgColor.G, BgColor.B)))
                    g.FillRectangle(bg, br);
                using (var pen = new Pen(Color.FromArgb(80, col), 0.5f))
                    g.DrawRectangle(pen, br.X, br.Y, br.Width, br.Height);
                using (var tb = new SolidBrush(col))
                    g.DrawString(lbl, f, tb, mx - sz.Width / 2, my - sz.Height / 2);
            }
        }

        private void DrawSelfLoop(Graphics g, CallGraphNode nd, int weight, int maxW)
        {
            bool hot  = nd.Name == _hoveredNode;
            float t   = _structuralView ? 1.5f : Math.Max(1f, 3f * weight / maxW);
            Color col = hot ? EdgeHot : EdgeBase;
            using (var pen = new Pen(col, t))
                g.DrawArc(pen, nd.X + NW / 2f - 10, nd.Y - NH - 10, 28, 28, 0, 270);
        }

        // ?? Nodes ?????????????????????????????????????????????????????????????
        private void DrawNodes(Graphics g)
        {
            if (_graph == null) return;

            // Per-node call heat
            var heatMap = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var ed in _graph.Edges.Values)
            {
                heatMap.TryGetValue(ed.Caller, out int hc); heatMap[ed.Caller] = hc + ed.Weight;
                heatMap.TryGetValue(ed.Callee, out int hd); heatMap[ed.Callee] = hd + ed.Weight;
            }
            int maxHeat = heatMap.Count > 0 ? heatMap.Values.Max() : 1;

            using (var labelFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var subFont   = new Font("Segoe UI", 6.5f))
            using (var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming      = StringTrimming.EllipsisCharacter
            })
            {
                foreach (var nd in _graph.Nodes.Values)
                {
                    bool hov = nd.Name == _hoveredNode;
                    heatMap.TryGetValue(nd.Name, out int h);
                    float t  = maxHeat > 0 ? (float)h / maxHeat : 0f;

                    Color top, bot, border, textCol;
                    if (_structuralView)
                    {
                        top    = Dark ? Color.FromArgb(55, 65, 100) : Color.FromArgb(200, 215, 248);
                        bot    = Dark ? Color.FromArgb(36, 44,  72) : Color.FromArgb(172, 192, 238);
                        border = hov  ? BorderSel : (Dark ? Color.FromArgb(90, 115, 170) : Color.FromArgb(100, 140, 215));
                        textCol = Dark ? Color.FromArgb(210, 220, 242) : Color.FromArgb(28, 48, 105);
                    }
                    else
                    {
                        Color h1 = HeatColor(t);
                        Color h2 = DarkenColor(h1, 0.72f);
                        top    = hov ? BlendColor(h1, Color.White, 0.28f) : h1;
                        bot    = hov ? BlendColor(h2, Color.White, 0.18f) : h2;
                        border = hov ? Color.White : LightenColor(h1, 1.35f);
                        textCol = Color.White;
                    }

                    var rect = new RectangleF(nd.X - NW / 2f, nd.Y - NH / 2f, NW, NH);

                    // Drop shadow
                    using (var sb = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    { var sr = rect; sr.Offset(3, 3); FillRounded(g, sb, sr, NR); }

                    // Gradient body
                    using (var gb = new LinearGradientBrush(
                        new PointF(rect.X, rect.Y), new PointF(rect.X, rect.Bottom), top, bot))
                        FillRounded(g, gb, rect, NR);

                    // Top gloss strip
                    using (var gloss = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                        FillRounded(g, gloss, new RectangleF(rect.X + 2, rect.Y + 2, rect.Width - 4, 6), 3);

                    // Border
                    using (var pen = new Pen(border, hov ? 2.5f : 1.5f))
                        StrokeRounded(g, pen, rect, NR);

                    // Function name
                    var labelRect = new RectangleF(rect.X + 4, rect.Y + 1, rect.Width - 8,
                        h > 0 && !_structuralView ? rect.Height - 13 : rect.Height);
                    using (var tb = new SolidBrush(textCol))
                        g.DrawString(nd.Name, labelFont, tb, labelRect, sf);

                    // Call-count sub-label (weighted mode only)
                    if (!_structuralView && h > 0)
                    {
                        using (var sb2 = new SolidBrush(Color.FromArgb(190, textCol)))
                        using (var sf2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
                            g.DrawString($"{h} call{(h == 1 ? "" : "s")}", subFont, sb2,
                                new RectangleF(rect.X, rect.Y, rect.Width, rect.Height - 2), sf2);
                    }
                }
            }
        }

        // ?? Overlays ??????????????????????????????????????????????????????????
        private void DrawStatusBar(Graphics g)
        {
            const int H = 22;
            int y0 = Height - H;
            using (var bg = new SolidBrush(Color.FromArgb(210, Dark ? 28 : 238, Dark ? 31 : 241, Dark ? 40 : 252)))
                g.FillRectangle(bg, 0, y0, Width, H);
            using (var sep = new Pen(GridColor))
                g.DrawLine(sep, 0, y0, Width, y0);

            string mode = _structuralView ? "Structural" : "Weighted";
            string txt  = $"  {_graph.Nodes.Count} nodes  •  {_graph.Edges.Count} edges  •  Mode: {mode}" +
                          $"  •  Zoom {_zoom * 100:F0}%  •  Scroll=Zoom  Drag=Pan  Click=Info  Dbl-Click=Center";
            using (var f = new Font("Segoe UI", 7.5f))
            using (var b = new SolidBrush(Color.FromArgb(145, Dark ? 195 : 75, Dark ? 200 : 80, Dark ? 218 : 105)))
                g.DrawString(txt, f, b, 0, y0 + 4);
        }

        private void DrawLegend(Graphics g)
        {
            if (_structuralView) return;
            const int LW = 135, LH = 20, LX = 10, LY = 10;
            using (var bg = new SolidBrush(Color.FromArgb(185, Dark ? 28 : 246, Dark ? 31 : 247, Dark ? 40 : 254)))
                g.FillRectangle(bg, LX, LY, LW, LH);
            using (var pen = new Pen(GridColor))
                g.DrawRectangle(pen, LX, LY, LW, LH);
            using (var f = new Font("Segoe UI", 7f))
            using (var b = new SolidBrush(TextColor))
            {
                g.DrawString("Low", f, b, LX + 2, LY + 4);
                g.DrawString("High", f, b, LX + LW - 26, LY + 4);
            }
            var bar = new Rectangle(LX + 26, LY + 5, LW - 52, 10);
            using (var gb = new LinearGradientBrush(bar, Heat[0], Heat[3], LinearGradientMode.Horizontal))
                g.FillRectangle(gb, bar);
        }

        // ?? Geometry helpers ??????????????????????????????????????????????????
        private static void FillRounded(Graphics g, Brush br, RectangleF r, float rad)
        {
            using (var p = RoundPath(r, rad)) g.FillPath(br, p);
        }
        private static void StrokeRounded(Graphics g, Pen pen, RectangleF r, float rad)
        {
            using (var p = RoundPath(r, rad)) g.DrawPath(pen, p);
        }
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

        // ?? Colour math ???????????????????????????????????????????????????????
        private static Color HeatColor(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            float seg = t * (Heat.Length - 1);
            int lo = (int)seg, hi = Math.Min(lo + 1, Heat.Length - 1);
            return BlendColor(Heat[hi], Heat[lo], seg - lo);
        }
        private static Color BlendColor(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R * t + b.R * (1 - t)),
                (int)(a.G * t + b.G * (1 - t)),
                (int)(a.B * t + b.B * (1 - t)));
        }
        private static Color DarkenColor(Color c, float f) =>
            Color.FromArgb(Math.Max(0, (int)(c.R * f)), Math.Max(0, (int)(c.G * f)), Math.Max(0, (int)(c.B * f)));
        private static Color LightenColor(Color c, float f) =>
            Color.FromArgb(Math.Min(255, (int)(c.R * f)), Math.Min(255, (int)(c.G * f)), Math.Min(255, (int)(c.B * f)));

        // ?? Mouse ?????????????????????????????????????????????????????????????
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * (e.Delta > 0 ? 1.12f : 0.89f)));
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            string hit = HitTest(e.Location);
            if (hit != null) { ShowNodeInfo(hit); return; }
            _dragging = true; _lastMouse = e.Location; Cursor = Cursors.SizeAll;
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            string hit = HitTest(e.Location);
            if (hit == null || !_graph.Nodes.TryGetValue(hit, out var nd)) return;
            _pan.X = -nd.X * _zoom; _pan.Y = -nd.Y * _zoom;
            _zoom = Math.Min(MaxZoom, _zoom * 1.5f);
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)   { base.OnMouseUp(e); _dragging = false; Cursor = Cursors.Default; }
        protected override void OnMouseLeave(EventArgs e)     { base.OnMouseLeave(e); _hoveredNode = null; Invalidate(); }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                _pan.X += e.X - _lastMouse.X; _pan.Y += e.Y - _lastMouse.Y;
                _lastMouse = e.Location; Invalidate(); return;
            }
            string hit = HitTest(e.Location);
            if (hit != _hoveredNode) { _hoveredNode = hit; Cursor = hit != null ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_welcomePanel != null) _welcomePanel.Bounds = ClientRectangle;
        }

        private string HitTest(Point pt)
        {
            if (_graph == null) return null;
            float gx = (pt.X - Width  / 2f - _pan.X) / _zoom;
            float gy = (pt.Y - Height / 2f - _pan.Y) / _zoom;
            foreach (var nd in _graph.Nodes.Values)
                if (Math.Abs(gx - nd.X) <= NW / 2f && Math.Abs(gy - nd.Y) <= NH / 2f)
                    return nd.Name;
            return null;
        }

        private void ShowNodeInfo(string name)
        {
            if (_graph == null) return;
            int incoming = 0, outgoing = 0;
            var callers = new List<string>();
            var callees = new List<string>();
            foreach (var ed in _graph.Edges.Values)
            {
                if (ed.Callee == name) { incoming += ed.Weight; if (!callers.Contains(ed.Caller)) callers.Add(ed.Caller); }
                if (ed.Caller == name) { outgoing += ed.Weight; if (!callees.Contains(ed.Callee)) callees.Add(ed.Callee); }
            }
            string info = $"Function:  {name}\n\n" +
                          $"Incoming calls : {incoming}  ({callers.Count} caller{(callers.Count == 1 ? "" : "s")})\n" +
                          $"Outgoing calls : {outgoing}  ({callees.Count} callee{(callees.Count == 1 ? "" : "s")})";
            if (callers.Count > 0) info += $"\n\nCallers:\n  {string.Join("\n  ", callers)}";
            if (callees.Count > 0) info += $"\n\nCallees:\n  {string.Join("\n  ", callees)}";
            MessageBox.Show(info, "Call Graph — Node Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
