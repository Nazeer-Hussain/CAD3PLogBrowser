using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Cad3PLogBrowser.Services.Analysis;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// F5 — Heatmap visualisation of API call frequency and duration.
    /// Each cell = one unique API. Colour intensity = total time (hot=slow, cool=fast).
    /// Size = call count. Click a cell to navigate to that API.
    /// </summary>
    public class HeatmapPanel : Panel
    {
        public event EventHandler<string> ApiSelected;

        private List<HeatCell> _cells = new List<HeatCell>();
        private HeatCell        _hovered;
        private readonly ToolTip _tip = new ToolTip();

        public HeatmapPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw   = true;
            BackColor      = Color.FromArgb(30, 30, 35);
        }

        public void LoadData(List<PerformanceStatistics> stats)
        {
            _cells.Clear();
            if (stats == null || stats.Count == 0) { Invalidate(); return; }

            long maxTotal = stats.Max(s => s.TotalDurationMs);
            int  maxCalls = stats.Max(s => s.CallCount);
            if (maxTotal == 0) maxTotal = 1;
            if (maxCalls == 0) maxCalls = 1;

            // Simple grid layout
            int cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(stats.Count)));
            int rows = (int)Math.Ceiling((double)stats.Count / cols);
            int cellW = Math.Max(40, (Width  - 20) / cols);
            int cellH = Math.Max(30, (Height - 20) / rows);

            for (int i = 0; i < stats.Count; i++)
            {
                var s = stats[i];
                int col = i % cols;
                int row = i / cols;

                // Heat: 0..1 based on total duration
                float heat = (float)s.TotalDurationMs / maxTotal;
                // Size: proportional to call count
                float sizeFactor = 0.4f + 0.6f * (float)s.CallCount / maxCalls;

                int x = 10 + col * cellW;
                int y = 10 + row * cellH;
                int w = (int)(cellW * sizeFactor) - 4;
                int h = (int)(cellH * sizeFactor) - 4;

                _cells.Add(new HeatCell
                {
                    ApiName = s.MethodName,
                    Bounds  = new Rectangle(x, y, w, h),
                    Heat    = heat,
                    Stats   = s
                });
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_cells.Count == 0)
            {
                using (var f = new Font("Segoe UI", 11))
                using (var b = new SolidBrush(Color.FromArgb(120, 120, 140)))
                    g.DrawString("No data — open a log file to generate heatmap.",
                        f, b, 20, Height / 2 - 12);
                return;
            }

            using (var font = new Font("Segoe UI", 7f))
            {
                foreach (var cell in _cells)
                {
                    var color = HeatColor(cell.Heat);
                    using (var fill = new SolidBrush(color))
                        g.FillRectangle(fill, cell.Bounds);

                    bool hot = cell == _hovered;
                    using (var pen = new Pen(hot ? Color.White : Color.FromArgb(80, 80, 90), hot ? 2 : 1))
                        g.DrawRectangle(pen, cell.Bounds);

                    // Label — truncate to fit
                    string label = cell.ApiName;
                    var sz = g.MeasureString(label, font);
                    while (sz.Width > cell.Bounds.Width - 4 && label.Length > 3)
                    {
                        label = label.Substring(0, label.Length - 4) + "...";
                        sz = g.MeasureString(label, font);
                    }
                    using (var tb = new SolidBrush(cell.Heat > 0.5f ? Color.White : Color.Black))
                        g.DrawString(label, font, tb, cell.Bounds.X + 2, cell.Bounds.Y + 2);
                }
            }

            // Legend
            DrawLegend(g);
        }

        private void DrawLegend(Graphics g)
        {
            int lx = Width - 130, ly = Height - 30;
            using (var font = new Font("Segoe UI", 7.5f))
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    float h = i / 100f;
                    using (var b = new SolidBrush(HeatColor(h)))
                        g.FillRectangle(b, lx + i, ly, 10, 14);
                }
                using (var tb = new SolidBrush(Color.LightGray))
                {
                    g.DrawString("Fast", font, tb, lx, ly + 15);
                    g.DrawString("Slow", font, tb, lx + 80, ly + 15);
                }
            }
        }

        private static Color HeatColor(float heat)
        {
            // Cool (blue) → warm (green) → hot (red)
            heat = Math.Max(0, Math.Min(1, heat));
            if (heat < 0.5f)
            {
                float t = heat * 2;
                return Color.FromArgb(
                    (int)(0   + t * 100),
                    (int)(120 + t * 80),
                    (int)(200 - t * 180));
            }
            else
            {
                float t = (heat - 0.5f) * 2;
                return Color.FromArgb(
                    (int)(100 + t * 155),
                    (int)(200 - t * 200),
                    (int)(20  - t * 20));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var hit = _cells.FirstOrDefault(c => c.Bounds.Contains(e.Location));
            if (hit != _hovered)
            {
                _hovered = hit;
                if (hit != null)
                    _tip.SetToolTip(this, string.Format(
                        "{0}\nCalls: {1}  Total: {2} ms  Avg: {3} ms",
                        hit.ApiName, hit.Stats.CallCount,
                        hit.Stats.TotalDurationMs, hit.Stats.AverageMs));
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var hit = _cells.FirstOrDefault(c => c.Bounds.Contains(e.Location));
            if (hit != null) ApiSelected?.Invoke(this, hit.ApiName);
        }

        private class HeatCell
        {
            public string              ApiName { get; set; }
            public Rectangle           Bounds  { get; set; }
            public float               Heat    { get; set; }
            public PerformanceStatistics Stats { get; set; }
        }
    }
}
