using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// F4 — Dependency Graph (who calls whom).
    /// Shows a directed graph where each node is a unique API and each
    /// directed edge means "caller → callee". Edge thickness = call frequency.
    /// Supports pan (drag), zoom (scroll wheel), and hover highlighting.
    /// </summary>
    public class DependencyGraphPanel : Panel
    {
        // ── Data ──────────────────────────────────────────────────────────────
        private List<DepNode> _nodes = new List<DepNode>();
        private List<DepEdge> _edges = new List<DepEdge>();

        // ── Interaction ───────────────────────────────────────────────────────
        private float  _zoom      = 1f;
        private PointF _pan       = new PointF(0, 0);
        private Point  _lastMouse;
        private bool   _dragging;
        private string _hoveredApi;

        private const float MinZoom = 0.15f, MaxZoom = 5f;
        private const float NodeW = 130f, NodeH = 28f, NodeR = 6f;

        public DependencyGraphPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw   = true;
            BackColor      = Color.FromArgb(24, 26, 32);
        }

        // ── Public API ────────────────────────────────────────────────────────
        public void Load(Dictionary<string, HashSet<string>> callerToCallees)
        {
            _nodes.Clear();
            _edges.Clear();

            // Build nodes
            var allApis = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in callerToCallees)
            {
                allApis.Add(kv.Key);
                foreach (var callee in kv.Value) allApis.Add(callee);
            }

            // Edge weights
            var weights = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var kv in callerToCallees)
                foreach (var callee in kv.Value)
                {
                    string key = kv.Key + "→" + callee;
                    weights[key] = weights.ContainsKey(key) ? weights[key] + 1 : 1;
                }

            // Circular layout
            var apiList = allApis.OrderBy(a => a).ToList();
            int n = apiList.Count;
            if (n == 0) { Invalidate(); return; }
            float radius = Math.Max(160f, n * 40f);

            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n - Math.PI / 2;
                _nodes.Add(new DepNode
                {
                    Name = apiList[i],
                    X    = radius * (float)Math.Cos(angle),
                    Y    = radius * (float)Math.Sin(angle)
                });
            }

            // Build edges
            var nodeMap = _nodes.ToDictionary(nd => nd.Name, StringComparer.Ordinal);
            foreach (var kv in callerToCallees)
            {
                if (!nodeMap.TryGetValue(kv.Key, out var callerNode)) continue;
                foreach (var callee in kv.Value)
                {
                    if (!nodeMap.TryGetValue(callee, out var calleeNode)) continue;
                    string key = kv.Key + "→" + callee;
                    _edges.Add(new DepEdge
                    {
                        Caller  = callerNode,
                        Callee  = calleeNode,
                        Weight  = weights.ContainsKey(key) ? weights[key] : 1
                    });
                }
            }

            ResetView();
        }

        public void ResetView() { _zoom = 1f; _pan = new PointF(0, 0); Invalidate(); }

        // ── Painting ──────────────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (_nodes.Count == 0)
            {
                using (var f = new Font("Segoe UI", 10))
                using (var b = new SolidBrush(Color.FromArgb(100, 110, 130)))
                    g.DrawString("No dependency data.\nOpen a log file to build the graph.",
                        f, b, 20, Height / 2 - 20);
                return;
            }

            g.TranslateTransform(Width / 2f + _pan.X, Height / 2f + _pan.Y);
            g.ScaleTransform(_zoom, _zoom);

            DrawEdges(g);
            DrawNodes(g);
        }

        private static readonly AdjustableArrowCap Arrow = new AdjustableArrowCap(5, 5);

        private void DrawEdges(Graphics g)
        {
            int maxW = _edges.Count > 0 ? _edges.Max(ed => ed.Weight) : 1;
            foreach (var ed in _edges)
            {
                bool hot = ed.Caller.Name == _hoveredApi || ed.Callee.Name == _hoveredApi;
                Color col = hot ? Color.FromArgb(255, 160, 50)
                               : Color.FromArgb(80, 100, 140);
                float thick = 1f + 3f * ed.Weight / maxW;

                // Vector from caller → callee
                float dx = ed.Callee.X - ed.Caller.X;
                float dy = ed.Callee.Y - ed.Caller.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist < 1) continue;
                float nx = dx / dist, ny = dy / dist;

                float x1 = ed.Caller.X + nx * NodeW / 2;
                float y1 = ed.Caller.Y + ny * NodeH / 2;
                float x2 = ed.Callee.X - nx * (NodeW / 2 + 8);
                float y2 = ed.Callee.Y - ny * (NodeH / 2 + 8);

                using (var pen = new Pen(col, thick) { CustomEndCap = Arrow })
                    g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        private void DrawNodes(Graphics g)
        {
            using (var font = new Font("Segoe UI", 8f))
            using (var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center,
                  Trimming = StringTrimming.EllipsisCharacter })
            {
                foreach (var nd in _nodes)
                {
                    bool hot = nd.Name == _hoveredApi;
                    var rect = new RectangleF(nd.X - NodeW / 2, nd.Y - NodeH / 2, NodeW, NodeH);

                    Color fill = hot ? Color.FromArgb(70, 130, 200)
                                     : Color.FromArgb(40, 55, 80);
                    Color border = hot ? Color.FromArgb(130, 180, 255)
                                       : Color.FromArgb(70, 100, 150);

                    using (var b = new SolidBrush(fill))
                    using (var path = RoundedRect(rect, NodeR))
                        g.FillPath(b, path);

                    using (var pen = new Pen(border, hot ? 2f : 1f))
                    using (var path = RoundedRect(rect, NodeR))
                        g.DrawPath(pen, path);

                    using (var tb = new SolidBrush(Color.FromArgb(220, 230, 245)))
                        g.DrawString(nd.Name, font, tb, rect, sf);
                }
            }
        }

        private static GraphicsPath RoundedRect(RectangleF r, float rad)
        {
            float d = rad * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ── Interaction ───────────────────────────────────────────────────────
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            float f = e.Delta > 0 ? 1.12f : 0.88f;
            _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * f));
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) { _dragging = true; _lastMouse = e.Location; Cursor = Cursors.SizeAll; }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        { base.OnMouseUp(e); _dragging = false; Cursor = Cursors.Default; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                _pan.X += e.X - _lastMouse.X;
                _pan.Y += e.Y - _lastMouse.Y;
                _lastMouse = e.Location;
                Invalidate();
                return;
            }
            string hit = HitTest(e.Location);
            if (hit != _hoveredApi) { _hoveredApi = hit; Cursor = hit != null ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hoveredApi = null; Invalidate(); }

        private string HitTest(Point p)
        {
            float gx = (p.X - Width / 2f - _pan.X) / _zoom;
            float gy = (p.Y - Height / 2f - _pan.Y) / _zoom;
            foreach (var nd in _nodes)
                if (Math.Abs(gx - nd.X) < NodeW / 2 && Math.Abs(gy - nd.Y) < NodeH / 2)
                    return nd.Name;
            return null;
        }

        private class DepNode { public string Name; public float X, Y; }
        private class DepEdge { public DepNode Caller, Callee; public int Weight; }
    }
}
