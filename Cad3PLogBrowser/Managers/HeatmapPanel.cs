using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// F5 — API activity heatmap: APIs as rows, time buckets as columns, cell colour
    /// depth = time spent in that API during that window. This is deliberately NOT a
    /// re-sorted duplicate of the Performance tab — Performance answers "which API is
    /// slowest overall"; this answers "when did each API get hot", a time dimension
    /// neither Performance (aggregate-only) nor Timeline (per-call, not aggregated)
    /// shows on its own.
    /// </summary>
    public class HeatmapPanel : Panel
    {
        public event EventHandler<string> ApiSelected;
        /// <summary>F6: raised when the user clicks "Export PNG...". MainForm owns the
        /// SaveFileDialog and file I/O, matching how Timeline/FlameGraph export works.</summary>
        public event EventHandler ExportImageRequested;

        private const int RowHeight    = 24;
        private const int HeaderHeight = 46;
        private const int LabelWidth   = 170;
        private const int RightPad     = 12;
        private const int BucketCount  = 50;

        private static readonly Color HeatDeep = Color.FromArgb(0, 122, 204); // IconGenerator.AccentBlue

        private class ApiRow
        {
            public string ApiName;
            public long[] BucketDurationMs;
            public int[]  BucketCallCount;
            public long   TotalDurationMs;
            public int    TotalCalls;
        }

        private List<ApiRow> _rows = new List<ApiRow>();
        private long _maxCellDuration = 1;
        private int  _maxCellCallCount = 1;
        // F5: toggles cell colour depth between time-spent (default) and call-count.
        private bool _colorByCount = false;
        private long _startEpochMs, _endEpochMs;
        private int _hoveredRow = -1, _hoveredBucket = -1;
        private readonly ToolTip _tip = new ToolTip();
        private readonly VScrollBar _scrollBar;
        private readonly Button _modeToggle;
        private readonly Button _exportButton;

        // Active-tab Find support (Ctrl+F): API-name search over heatmap rows.
        private string _searchText = string.Empty;
        private bool _searchMatchCase = false;
        private bool _searchUseRegex = false;
        private readonly List<int> _searchMatches = new List<int>();
        private readonly HashSet<int> _searchMatchSet = new HashSet<int>();
        private int _searchMatchIndex = -1;

        public HeatmapPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw   = true;
            BackColor      = ThemeManager.BackgroundColor;

            _scrollBar = new VScrollBar { Dock = DockStyle.Right, SmallChange = RowHeight, LargeChange = RowHeight * 5, Visible = false };
            _scrollBar.ValueChanged += (s, e) => Invalidate();
            Controls.Add(_scrollBar);

            _modeToggle = new Button
            {
                Text      = "Colour: Time",
                Size      = new Size(110, 22),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            _modeToggle.Click += (s, e) =>
            {
                _colorByCount = !_colorByCount;
                _modeToggle.Text = _colorByCount ? "Colour: Count" : "Colour: Time";
                Invalidate();
            };
            Controls.Add(_modeToggle);

            _exportButton = new Button
            {
                Text      = "Export PNG...",
                Size      = new Size(100, 22),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            _exportButton.Click += (s, e) => ExportImageRequested?.Invoke(this, EventArgs.Empty);
            Controls.Add(_exportButton);

            // F6: right-click "Export as Image..." to match Flame Graph/Timeline's
            // context menu, in addition to the in-panel button above.
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Export as Image...", null, (s, e) => ExportImageRequested?.Invoke(this, EventArgs.Empty));
            this.ContextMenuStrip = contextMenu;

            PositionModeToggle();
            UpdateTheme();
        }

        private void PositionModeToggle()
        {
            _modeToggle.Location = new Point(Math.Max(0, GridAreaWidth - _modeToggle.Width - RightPad), 2);
            _exportButton.Location = new Point(_modeToggle.Left - _exportButton.Width - 6, 2);
        }

        /// <summary>F6: Heatmap previously had no export at all. Renders every row
        /// (ignoring the current scroll position, unlike a plain screenshot would) at
        /// the panel's current width — there's no zoom/pan to reset here, just the
        /// vertical scroll that would otherwise crop everything below the fold.</summary>
        public Bitmap ExportAsImage()
        {
            int width  = Math.Max(1, GridAreaWidth);
            int height = Math.Max(1, HeaderHeight + _rows.Count * RowHeight);
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(ThemeManager.BackgroundColor);
                for (int i = 0; i < _rows.Count; i++)
                    DrawRow(g, i, HeaderHeight + i * RowHeight, width);
                DrawHeader(g, width);
            }
            return bmp;
        }

        /// <summary>Re-reads theme colours after a Light/Dark toggle.</summary>
        public void UpdateTheme()
        {
            BackColor = ThemeManager.BackgroundColor;
            _modeToggle.BackColor = ThemeManager.ButtonBackgroundColor;
            _modeToggle.ForeColor = ThemeManager.ForegroundColor;
            _modeToggle.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            _exportButton.BackColor = ThemeManager.ButtonBackgroundColor;
            _exportButton.ForeColor = ThemeManager.ForegroundColor;
            _exportButton.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            Invalidate();
        }

        /// <summary>
        /// Builds the time-bucketed activity matrix from the call tree. Each node's
        /// full duration is attributed to the bucket containing its start time — a
        /// deliberate simplification (vs. splitting duration across every bucket a
        /// long call spans) that keeps the picture readable and still answers "when
        /// did this fire", which is the point of this view.
        /// </summary>
        public void LoadData(List<CallStackNode> callStack)
        {
            _rows.Clear();
            _maxCellDuration = 1;
            _hoveredRow = -1;
            _hoveredBucket = -1;

            if (callStack == null || callStack.Count == 0)
            {
                UpdateScrollBar();
                Invalidate();
                return;
            }

            // Merged logs carry synthetic file-group roots — flatten past them so the
            // heatmap reflects real call activity, same as Timeline/FlameGraph do.
            var effectiveRoots = new List<CallStackNode>();
            foreach (var node in callStack)
            {
                if (node.IsFileGroupRoot) effectiveRoots.AddRange(node.Children);
                else effectiveRoots.Add(node);
            }

            long minEpoch = long.MaxValue, maxEpoch = long.MinValue;
            WalkRange(effectiveRoots, ref minEpoch, ref maxEpoch);

            if (minEpoch == long.MaxValue || maxEpoch <= minEpoch)
            {
                UpdateScrollBar();
                Invalidate();
                return;
            }

            _startEpochMs = minEpoch;
            _endEpochMs   = maxEpoch;
            long span = Math.Max(1, maxEpoch - minEpoch);

            var byApi = new Dictionary<string, ApiRow>(StringComparer.Ordinal);
            foreach (var root in effectiveRoots)
                WalkAccumulate(root, minEpoch, span, byApi);

            _rows = byApi.Values.OrderByDescending(r => r.TotalDurationMs).ToList();

            _maxCellDuration = 1;
            _maxCellCallCount = 1;
            foreach (var row in _rows)
            {
                foreach (var d in row.BucketDurationMs)
                    if (d > _maxCellDuration) _maxCellDuration = d;
                foreach (var c in row.BucketCallCount)
                    if (c > _maxCellCallCount) _maxCellCallCount = c;
            }

            UpdateScrollBar();
            PositionModeToggle();
            Invalidate();
        }

        private static void WalkRange(List<CallStackNode> nodes, ref long minEpoch, ref long maxEpoch)
        {
            foreach (var n in nodes)
            {
                if (n.EpochMs > 0)
                {
                    if (n.EpochMs < minEpoch) minEpoch = n.EpochMs;
                    long end = n.ExitEpochMs > 0 ? n.ExitEpochMs : n.EpochMs;
                    if (end > maxEpoch) maxEpoch = end;
                }
                WalkRange(n.Children, ref minEpoch, ref maxEpoch);
            }
        }

        private static void WalkAccumulate(CallStackNode n, long minEpoch, long span, Dictionary<string, ApiRow> byApi)
        {
            if (!string.IsNullOrEmpty(n.Label) && n.EpochMs > 0 && n.DurationMs > 0)
            {
                if (!byApi.TryGetValue(n.Label, out var row))
                {
                    row = new ApiRow
                    {
                        ApiName          = n.Label,
                        BucketDurationMs = new long[BucketCount],
                        BucketCallCount  = new int[BucketCount]
                    };
                    byApi[n.Label] = row;
                }

                int bucket = (int)(((n.EpochMs - minEpoch) * BucketCount) / span);
                if (bucket < 0) bucket = 0;
                if (bucket >= BucketCount) bucket = BucketCount - 1;

                row.BucketDurationMs[bucket] += n.DurationMs;
                row.BucketCallCount[bucket]++;
                row.TotalDurationMs += n.DurationMs;
                row.TotalCalls++;
            }

            foreach (var child in n.Children)
                WalkAccumulate(child, minEpoch, span, byApi);
        }

        private void UpdateScrollBar()
        {
            int contentHeight = HeaderHeight + _rows.Count * RowHeight;
            if (contentHeight <= Height || _rows.Count == 0)
            {
                _scrollBar.Visible = false;
                return;
            }

            _scrollBar.Visible     = true;
            _scrollBar.Minimum     = 0;
            _scrollBar.Maximum     = contentHeight - HeaderHeight;
            _scrollBar.LargeChange = Math.Max(1, Height - HeaderHeight);
            _scrollBar.SmallChange = RowHeight;
            if (_scrollBar.Value > _scrollBar.Maximum - _scrollBar.LargeChange + 1)
                _scrollBar.Value = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange + 1);
        }

        private bool RowMatches(ApiRow row, string term, bool useRegex,
            System.Text.RegularExpressions.Regex rx, StringComparison comparison)
        {
            if (row == null || string.IsNullOrEmpty(row.ApiName)) return false;
            return useRegex ? rx.IsMatch(row.ApiName) : row.ApiName.IndexOf(term, comparison) >= 0;
        }

        private void RebuildSearchMatches(string term, bool matchCase, bool useRegex)
        {
            _searchText = term ?? string.Empty;
            _searchMatchCase = matchCase;
            _searchUseRegex = useRegex;
            _searchMatches.Clear();
            _searchMatchSet.Clear();
            _searchMatchIndex = -1;

            if (_rows.Count == 0 || string.IsNullOrEmpty(_searchText)) return;

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var rx = useRegex
                ? new System.Text.RegularExpressions.Regex(
                    _searchText,
                    matchCase ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                : null;

            for (int i = 0; i < _rows.Count; i++)
            {
                if (!RowMatches(_rows[i], _searchText, useRegex, rx, comparison))
                    continue;

                _searchMatches.Add(i);
                _searchMatchSet.Add(i);
            }
        }

        private void EnsureRowVisible(int rowIndex)
        {
            if (!_scrollBar.Visible || rowIndex < 0) return;

            int rowTop = rowIndex * RowHeight;
            int viewportTop = _scrollBar.Value;
            int viewportHeight = Math.Max(1, Height - HeaderHeight);
            int viewportBottom = viewportTop + viewportHeight - RowHeight;

            if (rowTop < viewportTop)
                _scrollBar.Value = Math.Max(_scrollBar.Minimum, rowTop);
            else if (rowTop > viewportBottom)
                _scrollBar.Value = Math.Min(_scrollBar.Maximum - _scrollBar.LargeChange + 1, rowTop - viewportHeight + RowHeight * 2);
        }

        public int FindApi(string term, bool matchCase, bool useRegex, bool forward)
        {
            if (_rows.Count == 0 || string.IsNullOrEmpty(term)) return -1;

            bool rebuild = !string.Equals(term, _searchText, matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)
                || matchCase != _searchMatchCase
                || useRegex != _searchUseRegex
                || _searchMatches.Count == 0;

            if (rebuild)
                RebuildSearchMatches(term, matchCase, useRegex);

            if (_searchMatches.Count == 0) return -1;

            _searchMatchIndex = forward
                ? (_searchMatchIndex + 1) % _searchMatches.Count
                : (_searchMatchIndex - 1 + _searchMatches.Count) % _searchMatches.Count;

            int row = _searchMatches[_searchMatchIndex];
            _hoveredRow = row;
            _hoveredBucket = -1;
            EnsureRowVisible(row);
            Invalidate();
            return _searchMatchIndex;
        }

        public int CountApiMatches(string term, bool matchCase, bool useRegex, int currentIndex, out int rank)
        {
            rank = 0;
            if (_rows.Count == 0 || string.IsNullOrEmpty(term)) return 0;

            bool rebuild = !string.Equals(term, _searchText, matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)
                || matchCase != _searchMatchCase
                || useRegex != _searchUseRegex;

            if (rebuild)
                RebuildSearchMatches(term, matchCase, useRegex);

            if (_searchMatches.Count == 0) return 0;

            rank = _searchMatchIndex >= 0 ? _searchMatchIndex + 1 : 0;
            return _searchMatches.Count;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
            PositionModeToggle();
            Invalidate();
        }

        private int GridAreaWidth => Width - (_scrollBar.Visible ? _scrollBar.Width : 0);
        private int ScrollY => _scrollBar.Visible ? _scrollBar.Value : 0;
        private float CellWidth => Math.Max(1f, (GridAreaWidth - LabelWidth - RightPad) / (float)BucketCount);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(ThemeManager.BackgroundColor);

            int areaWidth = GridAreaWidth;

            if (_rows.Count == 0)
            {
                using (var f = new Font("Segoe UI", 10f))
                using (var b = new SolidBrush(ThemeManager.ForegroundColor))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("Open a log file to see API activity over time.", f, b,
                        new RectangleF(0, 0, areaWidth, Height), sf);
                }
                return;
            }

            var clip = g.Clip;
            g.SetClip(new Rectangle(0, HeaderHeight, areaWidth, Math.Max(0, Height - HeaderHeight)));
            int top = HeaderHeight - ScrollY;
            for (int i = 0; i < _rows.Count; i++)
            {
                int rowTop = top + i * RowHeight;
                if (rowTop + RowHeight < HeaderHeight || rowTop > Height) continue;
                DrawRow(g, i, rowTop, areaWidth);
            }
            g.Clip = clip;

            DrawHeader(g, areaWidth);
        }

        private void DrawHeader(Graphics g, int areaWidth)
        {
            using (var bg = new SolidBrush(ThemeManager.ControlBackgroundColor))
                g.FillRectangle(bg, 0, 0, areaWidth, HeaderHeight);

            using (var titleFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(ThemeManager.ForegroundColor))
                g.DrawString("API Activity Over Time", titleFont, titleBrush, 14, 5);

            using (var hintFont = new Font("Segoe UI", 8f))
            using (var hintBrush = new SolidBrush(MutedInk))
            {
                string colourBy = _colorByCount ? "call count" : "time spent";
                g.DrawString(string.Format("{0} APIs — colour depth = {1} in that window", _rows.Count, colourBy),
                    hintFont, hintBrush, 14, 21);
            }

            // Time-axis ticks under the title, spanning the grid area.
            float gridLeft  = LabelWidth;
            float gridRight = areaWidth - RightPad;
            long totalSpan  = Math.Max(1, _endEpochMs - _startEpochMs);
            using (var tickFont = new Font("Segoe UI", 7f))
            using (var tickBrush = new SolidBrush(MutedInk))
            using (var tickPen = new Pen(ThemeManager.BorderColor))
            {
                for (int i = 0; i <= 4; i++)
                {
                    float frac = i / 4f;
                    float x = gridLeft + (gridRight - gridLeft) * frac;
                    string lbl = FormatElapsed((long)(totalSpan * frac));
                    var sz = g.MeasureString(lbl, tickFont);
                    float lx = i == 4 ? x - sz.Width : (i == 0 ? x : x - sz.Width / 2);
                    g.DrawString(lbl, tickFont, tickBrush, lx, 28);
                    g.DrawLine(tickPen, x, 40, x, HeaderHeight - 1);
                }
            }

            using (var pen = new Pen(ThemeManager.BorderColor))
                g.DrawLine(pen, 0, HeaderHeight - 1, areaWidth, HeaderHeight - 1);
        }

        private void DrawRow(Graphics g, int index, int rowTop, int areaWidth)
        {
            var row = _rows[index];
            bool hoveredRow = index == _hoveredRow;
            bool isSearchMatch = _searchMatchSet.Contains(index);
            var rowRect = new Rectangle(0, rowTop, areaWidth, RowHeight);

            if (index % 2 == 1)
                using (var zebra = new SolidBrush(ZebraTint))
                    g.FillRectangle(zebra, rowRect);
            if (hoveredRow)
                using (var hoverBrush = new SolidBrush(Color.FromArgb(20, HeatDeep)))
                    g.FillRectangle(hoverBrush, rowRect);
            if (isSearchMatch)
                using (var matchBrush = new SolidBrush(Color.FromArgb(24, 255, 170, 0)))
                    g.FillRectangle(matchBrush, rowRect);

            using (var nameFont = new Font("Segoe UI", 8.5f))
            using (var nameBrush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                string label = row.ApiName;
                float maxW = LabelWidth - 10;
                var sz = g.MeasureString(label, nameFont);
                while (sz.Width > maxW && label.Length > 4)
                {
                    label = label.Substring(0, label.Length - 4) + "…";
                    sz = g.MeasureString(label, nameFont);
                }
                var rect = new RectangleF(8, rowTop, LabelWidth - 10, RowHeight);
                g.DrawString(label, nameFont, nameBrush, rect,
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            }

            float cellW = CellWidth;
            float cellH = RowHeight - 6;
            float cellY = rowTop + 3;

            for (int b = 0; b < BucketCount; b++)
            {
                float cellX = LabelWidth + b * cellW;
                var cellRect = new RectangleF(cellX, cellY, Math.Max(1f, cellW - 1), cellH);

                // F5: colour depth is either time-spent (default) or call-count,
                // toggled by _modeToggle — both dimensions were always tracked per
                // bucket, but only time was ever drawn.
                long dur   = row.BucketDurationMs[b];
                int  count = row.BucketCallCount[b];
                float t = _colorByCount
                    ? count / (float)_maxCellCallCount
                    : dur   / (float)_maxCellDuration;
                bool hasActivity = _colorByCount ? count > 0 : dur > 0;
                bool hoveredCell = hoveredRow && b == _hoveredBucket;

                using (var brush = new SolidBrush(hasActivity ? HeatColor(t) : ThemeManager.ControlBackgroundColor))
                    g.FillRectangle(brush, cellRect);

                if (hoveredCell)
                    using (var pen = new Pen(ThemeManager.ForegroundColor, 1.5f))
                        g.DrawRectangle(pen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
            }

            using (var pen = new Pen(ThemeManager.BorderColor))
                g.DrawLine(pen, 0, rowTop + RowHeight - 1, areaWidth, rowTop + RowHeight - 1);

            if (isSearchMatch)
            {
                using (var accent = new SolidBrush(Color.FromArgb(255, 150, 0)))
                    g.FillRectangle(accent, 0, rowTop, 3, RowHeight - 1);
            }
        }

        /// <summary>Single-hue sequential ramp: pale tint of the app's accent blue → fully saturated.</summary>
        private static Color HeatColor(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            var bg = ThemeManager.BackgroundColor;
            int r = (int)(bg.R + (HeatDeep.R - bg.R) * (0.15f + 0.85f * t));
            int gg = (int)(bg.G + (HeatDeep.G - bg.G) * (0.15f + 0.85f * t));
            int b = (int)(bg.B + (HeatDeep.B - bg.B) * (0.15f + 0.85f * t));
            return Color.FromArgb(255, Clamp(r), Clamp(gg), Clamp(b));
        }

        private static int Clamp(int v) => Math.Max(0, Math.Min(255, v));

        private static Color MutedInk =>
            ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                ? Color.FromArgb(150, 156, 168)
                : Color.FromArgb(110, 116, 128);

        private static Color ZebraTint =>
            ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                ? Color.FromArgb(10, 255, 255, 255)
                : Color.FromArgb(14, 0, 0, 0);

        private static string FormatElapsed(long ms)
        {
            if (ms >= 60_000) return string.Format("{0}m{1:00}s", ms / 60_000, (ms % 60_000) / 1000);
            if (ms >= 1_000)  return string.Format("{0:F1}s", ms / 1000.0);
            return string.Format("{0}ms", ms);
        }

        private (int row, int bucket) HitTest(Point pt)
        {
            if (pt.Y < HeaderHeight || _rows.Count == 0) return (-1, -1);
            int row = (pt.Y - HeaderHeight + ScrollY) / RowHeight;
            if (row < 0 || row >= _rows.Count) return (-1, -1);

            if (pt.X < LabelWidth) return (row, -1);
            int bucket = (int)((pt.X - LabelWidth) / CellWidth);
            if (bucket < 0 || bucket >= BucketCount) return (row, -1);
            return (row, bucket);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var (row, bucket) = HitTest(e.Location);
            if (row != _hoveredRow || bucket != _hoveredBucket)
            {
                _hoveredRow = row;
                _hoveredBucket = bucket;

                if (row >= 0)
                {
                    var r = _rows[row];
                    if (bucket >= 0)
                    {
                        long span = Math.Max(1, _endEpochMs - _startEpochMs);
                        long bucketStartMs = span * bucket / BucketCount;
                        long bucketEndMs   = span * (bucket + 1) / BucketCount;
                        _tip.SetToolTip(this, string.Format(
                            "{0}\n{1} – {2}\nDuration: {3:N0} ms   Calls: {4}",
                            r.ApiName, FormatElapsed(bucketStartMs), FormatElapsed(bucketEndMs),
                            r.BucketDurationMs[bucket], r.BucketCallCount[bucket]));
                    }
                    else
                    {
                        _tip.SetToolTip(this, string.Format(
                            "{0}\nTotal: {1:N0} ms   Calls: {2}", r.ApiName, r.TotalDurationMs, r.TotalCalls));
                    }
                }
                else
                {
                    _tip.SetToolTip(this, "");
                }
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredRow != -1) { _hoveredRow = -1; _hoveredBucket = -1; Invalidate(); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var (row, _) = HitTest(e.Location);
            if (row >= 0) ApiSelected?.Invoke(this, _rows[row].ApiName);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_scrollBar.Visible) return;
            int delta = -(e.Delta / 120) * RowHeight * 3;
            int newValue = Math.Max(_scrollBar.Minimum, Math.Min(_scrollBar.Maximum - _scrollBar.LargeChange + 1, _scrollBar.Value + delta));
            _scrollBar.Value = newValue;
        }
    }
}
