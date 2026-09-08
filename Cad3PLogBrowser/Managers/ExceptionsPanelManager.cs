namespace Cad3PLogBrowser.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Services;
    using Cad3PLogBrowser.Services.Analysis;

    /// <summary>
    /// Manages the "Exceptions" tab (Features K2/K3): exception-block grouping and
    /// correlation/request-ID tracking. Extracted from MainForm to keep the orchestrator
    /// focused on wiring, with all tab construction/population logic living here.
    /// </summary>
    public class ExceptionsPanelManager
    {
        private readonly TabControl _mainTabControl;
        private readonly ToolStripMenuItem _tabsMenuItem;
        private readonly AppSettings _appSettings;
        private readonly Func<List<string>> _getAllLines;
        private readonly Action<int> _scrollLogToLine;
        private readonly ExceptionGroupingService _exceptionGroupingService = new ExceptionGroupingService();

        private TabPage  _exceptionsTab;
        private ToolStripMenuItem _showExceptionsTabMenuItem;
        private ListView _exceptionGroupsListView;
        private ListView _correlationIdsListView;
        private ListView _correlationOccurrencesListView;

        /// <summary>The TabPage hosting this panel (null until <see cref="Init"/> runs).</summary>
        public TabPage Tab => _exceptionsTab;

        /// <summary>The "Show Exceptions tab" menu item (null until <see cref="Init"/> runs).</summary>
        public ToolStripMenuItem ShowTabMenuItem => _showExceptionsTabMenuItem;

        /// <param name="mainTabControl">The main tab control the Exceptions tab is added to/removed from.</param>
        /// <param name="tabsMenuItem">The "Tabs" menu the show/hide toggle is added to.</param>
        /// <param name="appSettings">Persisted app settings (for the tab-visibility flag).</param>
        /// <param name="getAllLines">Callback returning the current file's raw lines (for occurrence text).</param>
        /// <param name="scrollLogToLine">Callback that scrolls the main log view to a given 1-based line number.</param>
        public ExceptionsPanelManager(
            TabControl mainTabControl,
            ToolStripMenuItem tabsMenuItem,
            AppSettings appSettings,
            Func<List<string>> getAllLines,
            Action<int> scrollLogToLine)
        {
            _mainTabControl  = mainTabControl;
            _tabsMenuItem    = tabsMenuItem;
            _appSettings     = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _getAllLines     = getAllLines ?? throw new ArgumentNullException(nameof(getAllLines));
            _scrollLogToLine = scrollLogToLine ?? throw new ArgumentNullException(nameof(scrollLogToLine));
        }

        /// <summary>Builds the tab UI and registers the show/hide menu item. Call once during startup.</summary>
        public void Init()
        {
            _exceptionsTab = new TabPage("Exceptions") { Name = "exceptionsTab", UseVisualStyleBackColor = true };

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };

            // K2: Exception Groups (top)
            _exceptionGroupsListView = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = true, Font = new Font("Consolas", 9f)
            };
            _exceptionGroupsListView.Columns.Add("Type", 220);
            _exceptionGroupsListView.Columns.Add("Count", 70);
            _exceptionGroupsListView.Columns.Add("First Seen (Line)", 130);
            _exceptionGroupsListView.Columns.Add("Last Seen (Line)", 130);
            _exceptionGroupsListView.DoubleClick += (s, e) =>
            {
                if (_exceptionGroupsListView.SelectedItems.Count == 0) return;
                if (_exceptionGroupsListView.SelectedItems[0].Tag is ExceptionGroup grp)
                    _scrollLogToLine(grp.StartLine);
            };

            var groupsPanel = new Panel { Dock = DockStyle.Fill };
            var groupsHeader = new Label { Dock = DockStyle.Top, Height = 20, Text = "Exception Groups",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Padding = new Padding(4, 2, 0, 0) };
            groupsPanel.Controls.Add(_exceptionGroupsListView);
            groupsPanel.Controls.Add(groupsHeader);
            split.Panel1.Controls.Add(groupsPanel);

            // K3: Correlation IDs (bottom) — ID list on the left, occurrences on the right
            var correlationSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };

            _correlationIdsListView = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = true, Font = new Font("Consolas", 9f)
            };
            _correlationIdsListView.Columns.Add("Correlation / Request ID", 220);
            _correlationIdsListView.Columns.Add("Occurrences", 90);
            _correlationIdsListView.SelectedIndexChanged += (s, e) => PopulateCorrelationOccurrences();

            _correlationOccurrencesListView = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = true, Font = new Font("Consolas", 9f)
            };
            _correlationOccurrencesListView.Columns.Add("Line #", 80);
            _correlationOccurrencesListView.Columns.Add("Log Text", 800);
            _correlationOccurrencesListView.DoubleClick += (s, e) =>
            {
                if (_correlationOccurrencesListView.SelectedItems.Count == 0) return;
                if (_correlationOccurrencesListView.SelectedItems[0].Tag is int lineNo)
                    _scrollLogToLine(lineNo);
            };

            correlationSplit.Panel1.Controls.Add(_correlationIdsListView);
            correlationSplit.Panel2.Controls.Add(_correlationOccurrencesListView);
            correlationSplit.SplitterDistance = 260;

            var correlationPanel = new Panel { Dock = DockStyle.Fill };
            var correlationHeader = new Label { Dock = DockStyle.Top, Height = 20, Text = "Correlation / Request IDs — double-click a line to jump to it",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Padding = new Padding(4, 2, 0, 0) };
            correlationPanel.Controls.Add(correlationSplit);
            correlationPanel.Controls.Add(correlationHeader);
            split.Panel2.Controls.Add(correlationPanel);

            split.SplitterDistance = 200;
            _exceptionsTab.Controls.Add(split);

            if (_mainTabControl != null)
                _mainTabControl.TabPages.Add(_exceptionsTab);

            _showExceptionsTabMenuItem = new ToolStripMenuItem("E&xceptions")
            {
                Name = "showExceptionsTabMenuItem", CheckOnClick = true, Checked = _appSettings.ShowExceptionsTab
            };
            if (!_appSettings.ShowExceptionsTab && _mainTabControl != null && _mainTabControl.TabPages.Contains(_exceptionsTab))
                _mainTabControl.TabPages.Remove(_exceptionsTab);
            _showExceptionsTabMenuItem.CheckedChanged += (s, e) =>
            {
                if (_exceptionsTab == null || _mainTabControl == null) return;
                _appSettings.ShowExceptionsTab = _showExceptionsTabMenuItem.Checked;
                _appSettings.Save();
                if (_showExceptionsTabMenuItem.Checked)
                {
                    if (!_mainTabControl.TabPages.Contains(_exceptionsTab))
                        _mainTabControl.TabPages.Add(_exceptionsTab);
                }
                else if (_mainTabControl.TabPages.Contains(_exceptionsTab))
                {
                    _mainTabControl.TabPages.Remove(_exceptionsTab);
                }
            };
            _tabsMenuItem?.DropDownItems.Add(_showExceptionsTabMenuItem);
        }

        /// <summary>Refreshes the K2/K3 tab from the freshly-loaded log entries.</summary>
        public void UpdateFromEntries(List<Services.LogEntry> entries)
        {
            _exceptionGroupsListView.Items.Clear();
            _correlationIdsListView.Items.Clear();
            _correlationOccurrencesListView.Items.Clear();

            var groups = _exceptionGroupingService.GroupExceptions(entries);
            foreach (var grp in groups)
            {
                var item = new ListViewItem(new[]
                {
                    grp.ExceptionType, grp.Count.ToString(), grp.StartLine.ToString(), grp.EndLine.ToString()
                })
                { Tag = grp };
                _exceptionGroupsListView.Items.Add(item);
            }

            var correlations = _exceptionGroupingService.GroupByCorrelationId(entries);
            foreach (var kv in correlations)
            {
                var item = new ListViewItem(new[] { kv.Key, kv.Value.Count.ToString() }) { Tag = kv.Value };
                _correlationIdsListView.Items.Add(item);
            }
        }

        private void PopulateCorrelationOccurrences()
        {
            _correlationOccurrencesListView.Items.Clear();
            if (_correlationIdsListView.SelectedItems.Count == 0) return;
            if (!(_correlationIdsListView.SelectedItems[0].Tag is List<int> lines)) return;

            var allLines = _getAllLines();
            foreach (int lineNo in lines)
            {
                string text = (allLines != null && lineNo - 1 >= 0 && lineNo - 1 < allLines.Count) ? allLines[lineNo - 1] : "";
                _correlationOccurrencesListView.Items.Add(new ListViewItem(new[] { lineNo.ToString(), text }) { Tag = lineNo });
            }
        }
    }
}
