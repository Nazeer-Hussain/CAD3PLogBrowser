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
    /// Manages the log list view display and interactions.
    /// Handles virtual mode, scrolling, selection, highlighting, and context display.
    /// </summary>
    /// <remarks>
    /// This manager owns the ListView control that displays log entries in a two-column format:
    /// [Line #] [Log Text]
    /// 
    /// Key features:
    /// - Virtual mode for handling large files (500k+ lines)
    /// - Automatic color coding (errors, warnings, search highlights)
    /// - Context display (shows 10 lines before selected line)
    /// - Smart scrolling and selection
    /// - Column auto-sizing
    /// 
    /// Virtual mode means list items are created on-demand, not all upfront.
    /// This keeps memory usage low even for huge log files.
    /// </remarks>
    public class LogViewManager
    {
        // UI controls
        private readonly ListView _logListView;

        // Data
        private List<VirtualLogLine> _virtualLines;

        /// <summary>
        /// Gets the total number of lines currently in the view.
        /// </summary>
        public int TotalLines
        {
            get { return _virtualLines != null ? _virtualLines.Count : 0; }
        }

        /// <summary>
        /// Gets the currently selected line index (0-based), or -1 if none selected.
        /// </summary>
        public int SelectedLineIndex
        {
            get
            {
                if (_logListView.SelectedIndices.Count > 0)
                    return _logListView.SelectedIndices[0];
                return -1;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogViewManager"/> class.
        /// </summary>
        /// <param name="logListView">The ListView control for displaying logs.</param>
        /// <exception cref="ArgumentNullException">Thrown if logListView is null.</exception>
        public LogViewManager(ListView logListView)
        {
            _logListView = logListView ?? throw new ArgumentNullException(nameof(logListView));
            _virtualLines = new List<VirtualLogLine>();

            ConfigureListView();
        }

        /// <summary>
        /// Configures the list view for virtual mode and optimal performance.
        /// </summary>
        private void ConfigureListView()
        {
            _logListView.VirtualMode = true;
            _logListView.View = View.Details;
            _logListView.FullRowSelect = true;
            _logListView.GridLines = false;
            _logListView.MultiSelect = true;

            // Ensure columns exist (should be created in designer)
            if (_logListView.Columns.Count < 2)
            {
                _logListView.Columns.Add("Line #", Constants.UI.LineNumberColumnWidth);
                _logListView.Columns.Add("Log Text", 600);
            }

            // Subscribe to virtual item event
            _logListView.RetrieveVirtualItem += OnRetrieveVirtualItem;
        }

        /// <summary>
        /// Handles the RetrieveVirtualItem event to provide list items on-demand.
        /// This is the core of virtual mode - items are created only when visible.
        /// </summary>
        private void OnRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _virtualLines.Count)
            {
                e.Item = new ListViewItem();
                return;
            }

            var virtualLine = _virtualLines[e.ItemIndex];

            // Create list view item with two columns
            var item = new ListViewItem(virtualLine.LineNumber);
            item.SubItems.Add(virtualLine.Text);
            item.BackColor = virtualLine.BackgroundColor;

            e.Item = item;
        }

        /// <summary>
        /// Populates the list view with log entries.
        /// </summary>
        /// <param name="entries">Log entries to display.</param>
        public void PopulateLogView(List<LogEntry> entries)
        {
            if (entries == null)
                entries = new List<LogEntry>();

            _virtualLines = new List<VirtualLogLine>(entries.Count);

            foreach (var entry in entries)
            {
                _virtualLines.Add(VirtualLogLine.FromLogEntry(entry));
            }

            _logListView.VirtualListSize = _virtualLines.Count;
            _logListView.Invalidate();
        }

        /// <summary>
        /// Populates the list view with pre-built virtual lines (used for filtered views).
        /// </summary>
        public void PopulateLogView(List<VirtualLogLine> virtualLines)
        {
            _virtualLines = virtualLines ?? new List<VirtualLogLine>();
            _logListView.VirtualListSize = _virtualLines.Count;
            _logListView.Invalidate();
        }

        /// <summary>
        /// Highlights all lines matching the search term.
        /// </summary>
        public void HighlightSearchResults(
            string searchTerm,
            Color highlightColor,
            bool caseSensitive,
            bool useRegex)
        {
            if (string.IsNullOrEmpty(searchTerm) || _virtualLines.Count == 0)
            {
                ClearHighlighting();
                return;
            }

            try
            {
                if (useRegex)
                {
                    var options = caseSensitive ? System.Text.RegularExpressions.RegexOptions.None
                        : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                    var regex = new System.Text.RegularExpressions.Regex(searchTerm, options);

                    for (int i = 0; i < _virtualLines.Count; i++)
                    {
                        if (regex.IsMatch(_virtualLines[i].Text))
                        {
                            _virtualLines[i] = _virtualLines[i].WithBackgroundColor(highlightColor);
                        }
                    }
                }
                else
                {
                    var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                    for (int i = 0; i < _virtualLines.Count; i++)
                    {
                        if (_virtualLines[i].Text.IndexOf(searchTerm, comparison) >= 0)
                        {
                            _virtualLines[i] = _virtualLines[i].WithBackgroundColor(highlightColor);
                        }
                    }
                }

                _logListView.Invalidate();
            }
            catch (Exception)
            {
                // Ignore regex errors
            }
        }

        /// <summary>
        /// Clears all highlighting and restores original background colors.
        /// </summary>
        public void ClearHighlighting()
        {
            if (_virtualLines == null || _virtualLines.Count == 0)
                return;

            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var line = _virtualLines[i];
                Color originalColor = GetOriginalLineColor(line.Text);
                _virtualLines[i] = line.WithBackgroundColor(originalColor);
            }

            _logListView.Invalidate();
        }

        /// <summary>
        /// Determines the original background color for a log line based on its level indicator.
        /// </summary>
        private Color GetOriginalLineColor(string lineText)
        {
            if (string.IsNullOrEmpty(lineText))
                return SystemColors.Window;

            int firstColon = lineText.IndexOf(": ", StringComparison.Ordinal);
            if (firstColon >= 0 && firstColon + 3 < lineText.Length)
            {
                char level = lineText[firstColon + 2];

                if (level == Constants.Parsing.ErrorLevel)
                    return Color.LightCoral;

                if (level == Constants.Parsing.WarningLevel)
                    return Color.LightGoldenrodYellow;
            }

            return SystemColors.Window;
        }

        /// <summary>
        /// Jumps to and selects a specific line number.
        /// </summary>
        /// <param name="lineNumber">1-based line number to jump to.</param>
        /// <returns>True if jump was successful.</returns>
        public bool JumpToLine(int lineNumber)
        {
            if (lineNumber < 1 || lineNumber > _virtualLines.Count)
                return false;

            int index = lineNumber - 1;

            _logListView.SelectedIndices.Clear();
            _logListView.SelectedIndices.Add(index);
            _logListView.EnsureVisible(index);

            if (_logListView.Items.Count > index)
            {
                _logListView.FocusedItem = _logListView.Items[index];
            }

            return true;
        }

        /// <summary>
        /// Gets the text of selected lines.
        /// </summary>
        public List<string> GetSelectedLines()
        {
            var selectedLines = new List<string>();

            if (_logListView.SelectedIndices.Count == 0)
                return selectedLines;

            var indices = new int[_logListView.SelectedIndices.Count];
            _logListView.SelectedIndices.CopyTo(indices, 0);
            Array.Sort(indices);

            foreach (int index in indices)
            {
                if (index >= 0 && index < _virtualLines.Count)
                {
                    selectedLines.Add(_virtualLines[index].Text);
                }
            }

            return selectedLines;
        }

        /// <summary>
        /// Gets selected lines with line numbers (for "Copy with Headers" feature).
        /// </summary>
        public string GetSelectedLinesWithHeaders()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Line #\tLog Text");
            sb.AppendLine("------\t--------");

            if (_logListView.SelectedIndices.Count == 0)
                return sb.ToString();

            var indices = new int[_logListView.SelectedIndices.Count];
            _logListView.SelectedIndices.CopyTo(indices, 0);
            Array.Sort(indices);

            foreach (int index in indices)
            {
                if (index >= 0 && index < _virtualLines.Count)
                {
                    var line = _virtualLines[index];
                    sb.AppendLine(string.Format("{0}\t{1}", line.LineNumber, line.Text));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clears all lines from the view.
        /// </summary>
        public void Clear()
        {
            _virtualLines.Clear();
            _logListView.VirtualListSize = 0;
            _logListView.Invalidate();
        }

        /// <summary>
        /// Selects all lines in the view.
        /// </summary>
        public void SelectAll()
        {
            _logListView.SelectedIndices.Clear();

            for (int i = 0; i < _virtualLines.Count; i++)
            {
                _logListView.SelectedIndices.Add(i);
            }
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            _logListView.SelectedIndices.Clear();
        }

        /// <summary>
        /// Gets the selected line number (1-based) for status display.
        /// </summary>
        public int GetSelectedLineNumber()
        {
            int selectedIndex = SelectedLineIndex;
            if (selectedIndex < 0)
                return 0;

            return _virtualLines[selectedIndex].LineNumber.ToIntOrDefault(0);
        }

        /// <summary>
        /// Finds the next occurrence of a search term starting from current selection.
        /// </summary>
        public int FindNext(string searchTerm, bool caseSensitive, bool wrapAround)
        {
            if (string.IsNullOrEmpty(searchTerm) || _virtualLines.Count == 0)
                return -1;

            int startIndex = SelectedLineIndex + 1;
            if (startIndex >= _virtualLines.Count)
                startIndex = wrapAround ? 0 : _virtualLines.Count - 1;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            for (int i = startIndex; i < _virtualLines.Count; i++)
            {
                if (_virtualLines[i].Text.IndexOf(searchTerm, comparison) >= 0)
                    return i;
            }

            if (wrapAround && startIndex > 0)
            {
                for (int i = 0; i < startIndex; i++)
                {
                    if (_virtualLines[i].Text.IndexOf(searchTerm, comparison) >= 0)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Exports all visible lines to a list of strings.
        /// </summary>
        public List<string> ExportAllLines()
        {
            return _virtualLines.Select(vl => vl.Text).ToList();
        }
    }
}
