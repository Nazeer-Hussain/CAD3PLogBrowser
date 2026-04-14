using System;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// Professional status bar for visualization panels.
    /// Shows real-time information about the current view.
    /// </summary>
    public class VisualizationStatusBar : StatusStrip
    {
        private ToolStripStatusLabel _nodeCountLabel;
        private ToolStripStatusLabel _selectedNodeLabel;
        private ToolStripStatusLabel _zoomLabel;
        private ToolStripStatusLabel _totalDurationLabel;
        private ToolStripStatusLabel _infoLabel;

        public VisualizationStatusBar()
        {
            this.SizingGrip = false;
            this.Renderer = new ToolStripProfessionalRenderer();

            InitializeLabels();
            ApplyTheme();
        }

        private void InitializeLabels()
        {
            // Node count
            _nodeCountLabel = new ToolStripStatusLabel
            {
                Text = "Nodes: 0",
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Width = 100
            };

            // Selected node info
            _selectedNodeLabel = new ToolStripStatusLabel
            {
                Text = "No selection",
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Zoom level
            _zoomLabel = new ToolStripStatusLabel
            {
                Text = "Zoom: 100%",
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Width = 100
            };

            // Total duration
            _totalDurationLabel = new ToolStripStatusLabel
            {
                Text = "Duration: 0ms",
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Width = 120
            };

            // Info/help text
            _infoLabel = new ToolStripStatusLabel
            {
                Text = "Scroll: Zoom | Drag: Pan | Click: Select",
                ForeColor = Color.FromArgb(100, 100, 100),
                Width = 250
            };

            this.Items.Add(_nodeCountLabel);
            this.Items.Add(_selectedNodeLabel);
            this.Items.Add(_zoomLabel);
            this.Items.Add(_totalDurationLabel);
            this.Items.Add(_infoLabel);
        }

        public void UpdateNodeCount(int count)
        {
            _nodeCountLabel.Text = $"Nodes: {count:N0}";
        }

        public void UpdateSelectedNode(string nodeName, long durationMs)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                _selectedNodeLabel.Text = "No selection";
                _selectedNodeLabel.ForeColor = Color.FromArgb(100, 100, 100);
            }
            else
            {
                _selectedNodeLabel.Text = $"Selected: {nodeName} ({durationMs}ms)";
                _selectedNodeLabel.ForeColor = Color.FromArgb(0, 120, 215);
            }
        }

        public void UpdateZoom(float zoom)
        {
            _zoomLabel.Text = $"Zoom: {zoom * 100:F0}%";
        }

        public void UpdateTotalDuration(long durationMs)
        {
            _totalDurationLabel.Text = $"Duration: {durationMs:N0}ms";
        }

        public void UpdateInfo(string info)
        {
            _infoLabel.Text = info;
        }

        public void ApplyTheme()
        {
            bool isDark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

            this.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.FromArgb(240, 240, 240);
            this.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(30, 30, 30);

            foreach (ToolStripItem item in this.Items)
            {
                if (item != _infoLabel && item != _selectedNodeLabel)
                {
                    item.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(30, 30, 30);
                }
            }
        }
    }
}
