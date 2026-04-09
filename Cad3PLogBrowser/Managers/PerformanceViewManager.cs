namespace Cad3PLogBrowser.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models;
    using Cad3PLogBrowser.Utilities;

    /// <summary>
    /// Manages the Performance tab's ListView display.
    /// Handles population, sorting, color coding, and interaction with performance statistics.
    /// </summary>
    /// <remarks>
    /// The Performance tab shows a grid of all API methods with their execution statistics:
    /// - API Name
    /// - Call Count
    /// - Total Duration
    /// - Average Duration
    /// - Min/Max Duration
    /// - Self Time
    /// - Source File
    /// 
    /// Users can click column headers to sort by any metric.
    /// Slow methods are color-coded in red for easy identification.
    /// </remarks>
    public class PerformanceViewManager
    {
        private readonly ListView _performanceListView;
        private List<PerformanceStatistics> _statistics;
        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;

        /// <summary>
        /// Gets the total number of unique API methods in the performance view.
        /// </summary>
        public int TotalMethods
        {
            get { return _statistics != null ? _statistics.Count : 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceViewManager"/> class.
        /// </summary>
        /// <param name="performanceListView">The ListView control for displaying performance statistics.</param>
        /// <exception cref="ArgumentNullException">Thrown if performanceListView is null.</exception>
        public PerformanceViewManager(ListView performanceListView)
        {
            _performanceListView = performanceListView ?? throw new ArgumentNullException(nameof(performanceListView));
            _statistics = new List<PerformanceStatistics>();

            ConfigureListView();
        }

        /// <summary>
        /// Configures the performance list view with columns and settings.
        /// </summary>
        private void ConfigureListView()
        {
            _performanceListView.View = View.Details;
            _performanceListView.FullRowSelect = true;
            _performanceListView.GridLines = true;
            _performanceListView.MultiSelect = false;
            _performanceListView.Sorting = SortOrder.None;

            // Create columns if they don't exist
            if (_performanceListView.Columns.Count == 0)
            {
                _performanceListView.Columns.Add("API Name", 250);
                _performanceListView.Columns.Add("Calls", 80, HorizontalAlignment.Right);
                _performanceListView.Columns.Add("Total (ms)", 100, HorizontalAlignment.Right);
                _performanceListView.Columns.Add("Avg (ms)", 90, HorizontalAlignment.Right);
                _performanceListView.Columns.Add("Min (ms)", 90, HorizontalAlignment.Right);
                _performanceListView.Columns.Add("Max (ms)", 90, HorizontalAlignment.Right);
                _performanceListView.Columns.Add("Self (ms)", 90, HorizontalAlignment.Right);
                _performanceListView.Columns.Add("Source File", 200);
            }

            // Subscribe to column click for sorting
            _performanceListView.ColumnClick += OnColumnClick;
        }

        /// <summary>
        /// Populates the performance view with statistics.
        /// </summary>
        /// <param name="statistics">List of performance statistics to display.</param>
        /// <remarks>
        /// This method clears the existing view and adds all statistics as list items.
        /// Each row is color-coded based on average duration (red for slow calls).
        /// </remarks>
        public void PopulatePerformanceView(List<PerformanceStatistics> statistics)
        {
            if (statistics == null)
                statistics = new List<PerformanceStatistics>();

            _statistics = statistics;

            _performanceListView.BeginUpdate();
            _performanceListView.Items.Clear();

            foreach (var stat in statistics)
            {
                var item = CreateListViewItem(stat);
                _performanceListView.Items.Add(item);
            }

            _performanceListView.EndUpdate();
        }

        /// <summary>
        /// Creates a ListViewItem from a PerformanceStatistics object.
        /// </summary>
        private ListViewItem CreateListViewItem(PerformanceStatistics stat)
        {
            var item = new ListViewItem(stat.ApiName);
            item.SubItems.Add(stat.CallCount.ToString("N0"));
            item.SubItems.Add(stat.TotalDurationMs.ToString("N0"));
            item.SubItems.Add(stat.AvgDurationMs.ToString("N0"));
            item.SubItems.Add(stat.MinDurationMs.ToString("N0"));
            item.SubItems.Add(stat.MaxDurationMs.ToString("N0"));
            item.SubItems.Add(stat.SelfDurationMs.ToString("N0"));
            item.SubItems.Add(stat.SourceFile ?? string.Empty);

            // Color code based on average duration
            if (stat.AvgDurationMs > Constants.Performance.SlowCallThresholdMs)
            {
                item.ForeColor = Color.Red;
            }
            else if (stat.AvgDurationMs > Constants.Performance.FastCallThresholdMs)
            {
                item.ForeColor = Color.DarkOrange;
            }
            else
            {
                item.ForeColor = Color.Green;
            }

            // Store the statistics object in the tag for later access
            item.Tag = stat;

            return item;
        }

        /// <summary>
        /// Handles column click events for sorting.
        /// </summary>
        private void OnColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine sort order
            if (e.Column == _sortColumn)
            {
                // Toggle sort order
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortColumn = e.Column;
                _sortOrder = SortOrder.Ascending;
            }

            // Sort the statistics
            SortStatistics(_sortColumn, _sortOrder);

            // Repopulate with sorted data
            PopulatePerformanceView(_statistics);
        }

        /// <summary>
        /// Sorts the statistics list by the specified column.
        /// </summary>
        private void SortStatistics(int columnIndex, SortOrder order)
        {
            if (_statistics == null || _statistics.Count == 0)
                return;

            bool ascending = order == SortOrder.Ascending;

            switch (columnIndex)
            {
                case 0: // API Name
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.ApiName).ToList()
                        : _statistics.OrderByDescending(s => s.ApiName).ToList();
                    break;

                case 1: // Calls
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.CallCount).ToList()
                        : _statistics.OrderByDescending(s => s.CallCount).ToList();
                    break;

                case 2: // Total (ms)
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.TotalDurationMs).ToList()
                        : _statistics.OrderByDescending(s => s.TotalDurationMs).ToList();
                    break;

                case 3: // Avg (ms)
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.AvgDurationMs).ToList()
                        : _statistics.OrderByDescending(s => s.AvgDurationMs).ToList();
                    break;

                case 4: // Min (ms)
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.MinDurationMs).ToList()
                        : _statistics.OrderByDescending(s => s.MinDurationMs).ToList();
                    break;

                case 5: // Max (ms)
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.MaxDurationMs).ToList()
                        : _statistics.OrderByDescending(s => s.MaxDurationMs).ToList();
                    break;

                case 6: // Self (ms)
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.SelfDurationMs).ToList()
                        : _statistics.OrderByDescending(s => s.SelfDurationMs).ToList();
                    break;

                case 7: // Source File
                    _statistics = ascending
                        ? _statistics.OrderBy(s => s.SourceFile ?? string.Empty).ToList()
                        : _statistics.OrderByDescending(s => s.SourceFile ?? string.Empty).ToList();
                    break;
            }
        }

        /// <summary>
        /// Gets the currently selected performance statistic.
        /// </summary>
        /// <returns>Selected PerformanceStatistics or null if none selected.</returns>
        public PerformanceStatistics GetSelectedStatistic()
        {
            if (_performanceListView.SelectedItems.Count == 0)
                return null;

            var item = _performanceListView.SelectedItems[0];
            return item.Tag as PerformanceStatistics;
        }

        /// <summary>
        /// Clears all items from the performance view.
        /// </summary>
        public void Clear()
        {
            _statistics.Clear();
            _performanceListView.Items.Clear();
        }

        /// <summary>
        /// Finds and selects a performance statistic by API name.
        /// </summary>
        /// <param name="apiName">The API name to find.</param>
        /// <returns>True if found and selected.</returns>
        public bool FindAndSelectByApiName(string apiName)
        {
            if (string.IsNullOrWhiteSpace(apiName))
                return false;

            for (int i = 0; i < _performanceListView.Items.Count; i++)
            {
                var item = _performanceListView.Items[i];
                var stat = item.Tag as PerformanceStatistics;

                if (stat != null && stat.ApiName.Equals(apiName, StringComparison.OrdinalIgnoreCase))
                {
                    _performanceListView.SelectedIndices.Clear();
                    _performanceListView.SelectedIndices.Add(i);
                    item.EnsureVisible();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets summary statistics for display.
        /// </summary>
        /// <returns>Formatted string with summary information.</returns>
        public string GetSummary()
        {
            if (_statistics == null || _statistics.Count == 0)
                return "No performance data available";

            int totalMethods = _statistics.Count;
            long totalCalls = 0;
            long totalDuration = 0;

            foreach (var stat in _statistics)
            {
                totalCalls += stat.CallCount;
                totalDuration += stat.TotalDurationMs;
            }

            return string.Format("{0} methods, {1:N0} calls, {2} total time",
                totalMethods,
                totalCalls,
                totalDuration.FormatDuration());
        }

        /// <summary>
        /// Exports performance statistics to a CSV-formatted string.
        /// </summary>
        /// <returns>CSV string with all statistics.</returns>
        public string ExportToCsvString()
        {
            var sb = new System.Text.StringBuilder();

            // Header
            sb.AppendLine("API Name,Calls,Total (ms),Avg (ms),Min (ms),Max (ms),Self (ms),Source File");

            // Data rows
            foreach (var stat in _statistics)
            {
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    stat.ApiName.EscapeCsv(),
                    stat.CallCount,
                    stat.TotalDurationMs,
                    stat.AvgDurationMs,
                    stat.MinDurationMs,
                    stat.MaxDurationMs,
                    stat.SelfDurationMs,
                    (stat.SourceFile ?? string.Empty).EscapeCsv()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Highlights slow methods (average > threshold) in the view.
        /// </summary>
        public void HighlightSlowMethods()
        {
            foreach (ListViewItem item in _performanceListView.Items)
            {
                var stat = item.Tag as PerformanceStatistics;
                if (stat != null && stat.AvgDurationMs > Constants.Performance.SlowCallThresholdMs)
                {
                    item.BackColor = Color.LightCoral;
                }
            }
        }

        /// <summary>
        /// Clears background highlighting from all items.
        /// </summary>
        public void ClearHighlighting()
        {
            foreach (ListViewItem item in _performanceListView.Items)
            {
                item.BackColor = SystemColors.Window;
            }
        }
    }
}
