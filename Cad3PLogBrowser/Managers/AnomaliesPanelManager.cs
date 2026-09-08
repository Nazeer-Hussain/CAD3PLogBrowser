namespace Cad3PLogBrowser.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Services;
    using Cad3PLogBrowser.Services.Analysis;
    using Cad3PLogBrowser.Services.Core;

    /// <summary>
    /// Manages the "Anomalies" tab (Feature L3): compares the current log's API
    /// timings/call counts against a saved baseline. Extracted from MainForm to keep
    /// the orchestrator focused on wiring, with all tab construction/population
    /// logic living here.
    /// </summary>
    public class AnomaliesPanelManager
    {
        private readonly TabControl _mainTabControl;
        private readonly ToolStripMenuItem _tabsMenuItem;
        private readonly AppSettings _appSettings;
        /// <summary>Callback invoked when the user double-clicks an anomaly row (jumps to that API in the API tree).</summary>
        private readonly Action<string> _onApiSelected;

        private TabPage  _anomaliesTab;
        private ToolStripMenuItem _showAnomaliesTabMenuItem;
        private ListView _anomaliesListView;
        private Label    _anomaliesStatusLabel;

        /// <summary>The TabPage hosting this panel (null until <see cref="Init"/> runs).</summary>
        public TabPage Tab => _anomaliesTab;

        /// <summary>The "Show Anomalies tab" menu item (null until <see cref="Init"/> runs).</summary>
        public ToolStripMenuItem ShowTabMenuItem => _showAnomaliesTabMenuItem;

        /// <summary>The anomalies computed by the last <see cref="UpdateFromStats"/> call.</summary>
        public List<AnomalyResult> LastAnomalies { get; private set; } = new List<AnomalyResult>();

        /// <param name="mainTabControl">The main tab control the Anomalies tab is added to/removed from.</param>
        /// <param name="tabsMenuItem">The "Tabs" menu the show/hide toggle is added to.</param>
        /// <param name="appSettings">Persisted app settings (for the tab-visibility flag).</param>
        /// <param name="onApiSelected">Callback fired with the API name when a row is double-clicked.</param>
        public AnomaliesPanelManager(
            TabControl mainTabControl,
            ToolStripMenuItem tabsMenuItem,
            AppSettings appSettings,
            Action<string> onApiSelected)
        {
            _mainTabControl = mainTabControl;
            _tabsMenuItem   = tabsMenuItem;
            _appSettings    = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _onApiSelected  = onApiSelected ?? throw new ArgumentNullException(nameof(onApiSelected));
        }

        /// <summary>Builds the tab UI and registers the show/hide menu item. Call once during startup.</summary>
        public void Init()
        {
            _anomaliesTab = new TabPage("Anomalies") { Name = "anomaliesTab", UseVisualStyleBackColor = true };

            _anomaliesListView = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = true, Font = new Font("Consolas", 9f)
            };
            _anomaliesListView.Columns.Add("Method", 260);
            _anomaliesListView.Columns.Add("Baseline Avg (ms)", 120);
            _anomaliesListView.Columns.Add("Current Avg (ms)", 120);
            _anomaliesListView.Columns.Add("Baseline Calls", 100);
            _anomaliesListView.Columns.Add("Current Calls", 100);
            _anomaliesListView.Columns.Add("Why Flagged", 260);
            _anomaliesListView.DoubleClick += (s, e) =>
            {
                if (_anomaliesListView.SelectedItems.Count == 0) return;
                if (_anomaliesListView.SelectedItems[0].Tag is string apiName)
                    _onApiSelected(apiName);
            };

            _anomaliesStatusLabel = new Label
            {
                Dock = DockStyle.Top, Height = 22, Padding = new Padding(4, 4, 0, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            _anomaliesTab.Controls.Add(_anomaliesListView);
            _anomaliesTab.Controls.Add(_anomaliesStatusLabel);

            if (_mainTabControl != null)
                _mainTabControl.TabPages.Add(_anomaliesTab);

            _showAnomaliesTabMenuItem = new ToolStripMenuItem("&Anomalies")
            {
                Name = "showAnomaliesTabMenuItem", CheckOnClick = true, Checked = _appSettings.ShowAnomaliesTab
            };
            if (!_appSettings.ShowAnomaliesTab && _mainTabControl != null && _mainTabControl.TabPages.Contains(_anomaliesTab))
                _mainTabControl.TabPages.Remove(_anomaliesTab);
            _showAnomaliesTabMenuItem.CheckedChanged += (s, e) =>
            {
                if (_anomaliesTab == null || _mainTabControl == null) return;
                _appSettings.ShowAnomaliesTab = _showAnomaliesTabMenuItem.Checked;
                _appSettings.Save();
                if (_showAnomaliesTabMenuItem.Checked)
                {
                    if (!_mainTabControl.TabPages.Contains(_anomaliesTab))
                        _mainTabControl.TabPages.Add(_anomaliesTab);
                }
                else if (_mainTabControl.TabPages.Contains(_anomaliesTab))
                {
                    _mainTabControl.TabPages.Remove(_anomaliesTab);
                }
            };
            _tabsMenuItem?.DropDownItems.Add(_showAnomaliesTabMenuItem);
        }

        /// <summary>Re-runs the baseline comparison against the freshly-loaded log's stats.</summary>
        public void UpdateFromStats(IEnumerable<ApiPerfStats> currentStats)
        {
            if (_anomaliesListView == null) return;
            _anomaliesListView.Items.Clear();

            var baseline = BaselineService.LoadBaseline();
            if (baseline == null)
            {
                _anomaliesStatusLabel.Text = "No baseline saved yet — Options > Set as Baseline Log to enable comparison.";
                LastAnomalies = new List<AnomalyResult>();
                return;
            }

            var anomalies = BaselineService.CompareToBaseline(currentStats, baseline);
            LastAnomalies = anomalies;
            _anomaliesStatusLabel.Text = string.Format(
                "Comparing against baseline saved {0:yyyy-MM-dd HH:mm} from \"{1}\" — {2} anomal{3} found.",
                baseline.SavedAtUtc.ToLocalTime(), baseline.SourceFileName,
                anomalies.Count, anomalies.Count == 1 ? "y" : "ies");

            foreach (var a in anomalies)
            {
                var item = new ListViewItem(new[]
                {
                    a.ApiName, a.BaselineAvgMs.ToString(), a.CurrentAvgMs.ToString(),
                    a.BaselineCalls.ToString(), a.CurrentCalls.ToString(), a.Reason
                })
                { Tag = a.ApiName };
                _anomaliesListView.Items.Add(item);
            }
        }

        /// <summary>Saves the given stats as the new baseline and refreshes the tab. Returns the method count saved.</summary>
        public int SaveBaseline(string sourceFileName, IEnumerable<ApiPerfStats> currentStats)
        {
            var list = new List<ApiPerfStats>(currentStats);
            BaselineService.SaveBaseline(sourceFileName, list);
            UpdateFromStats(list);
            return list.Count;
        }
    }
}
