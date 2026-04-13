using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// AI Assistant Panel - provides intelligent log analysis capabilities.
    /// Features: Natural language queries, anomaly detection, performance insights, pattern recognition.
    /// </summary>
    public class AiAssistantPanel : Panel
    {
        private TextBox _queryTextBox;
        private Button _askButton;
        private Button _summarizeButton;
        private Button _detectAnomaliesButton;
        private Button _analyzePerformanceButton;
        private Button _findPatternsButton;
        private RichTextBox _responseTextBox;
        private Label _statusLabel;
        private Panel _buttonPanel;
        private bool _isThinking;

        public event EventHandler<string> QuerySubmitted;
        public event EventHandler SummarizeRequested;
        public event EventHandler DetectAnomaliesRequested;
        public event EventHandler AnalyzePerformanceRequested;
        public event EventHandler FindPatternsRequested;

        public AiAssistantPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Status label
            _statusLabel = new Label
            {
                Text = "?? AI Assistant - Ask questions about your log file",
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };

            // Button panel
            _buttonPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            // Quick action buttons
            _summarizeButton = new Button
            {
                Text = "?? Summarize",
                Width = 120,
                Height = 30,
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 9f)
            };
            _summarizeButton.Click += (s, e) => SummarizeRequested?.Invoke(this, EventArgs.Empty);

            _detectAnomaliesButton = new Button
            {
                Text = "?? Detect Anomalies",
                Width = 140,
                Height = 30,
                Location = new Point(140, 10),
                Font = new Font("Segoe UI", 9f)
            };
            _detectAnomaliesButton.Click += (s, e) => DetectAnomaliesRequested?.Invoke(this, EventArgs.Empty);

            _analyzePerformanceButton = new Button
            {
                Text = "? Performance",
                Width = 120,
                Height = 30,
                Location = new Point(290, 10),
                Font = new Font("Segoe UI", 9f)
            };
            _analyzePerformanceButton.Click += (s, e) => AnalyzePerformanceRequested?.Invoke(this, EventArgs.Empty);

            _findPatternsButton = new Button
            {
                Text = "?? Find Patterns",
                Width = 120,
                Height = 30,
                Location = new Point(420, 10),
                Font = new Font("Segoe UI", 9f)
            };
            _findPatternsButton.Click += (s, e) => FindPatternsRequested?.Invoke(this, EventArgs.Empty);

            _buttonPanel.Controls.AddRange(new Control[] { 
                _summarizeButton, _detectAnomaliesButton, _analyzePerformanceButton, _findPatternsButton 
            });

            // Query input panel
            var queryPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var queryLabel = new Label
            {
                Text = "Ask a question:",
                AutoSize = true,
                Location = new Point(10, 5),
                Font = new Font("Segoe UI", 9f)
            };

            _queryTextBox = new TextBox
            {
                Width = 550,
                Height = 25,
                Location = new Point(10, 25),
                Font = new Font("Segoe UI", 9f),
                Text = "e.g., What caused the errors? Which APIs are slowest?",
                ForeColor = SystemColors.GrayText
            };
            _queryTextBox.GotFocus += (s, e) =>
            {
                if (_queryTextBox.ForeColor == SystemColors.GrayText)
                {
                    _queryTextBox.Text = "";
                    _queryTextBox.ForeColor = SystemColors.WindowText;
                }
            };
            _queryTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_queryTextBox.Text))
                {
                    _queryTextBox.Text = "e.g., What caused the errors? Which APIs are slowest?";
                    _queryTextBox.ForeColor = SystemColors.GrayText;
                }
            };
            _queryTextBox.KeyDown += QueryTextBox_KeyDown;

            _askButton = new Button
            {
                Text = "Ask",
                Width = 80,
                Height = 25,
                Location = new Point(570, 25),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _askButton.Click += AskButton_Click;

            queryPanel.Controls.AddRange(new Control[] { queryLabel, _queryTextBox, _askButton });

            // Response text box
            _responseTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Text = "Welcome to AI Assistant!\n\n" +
                       "Quick Actions:\n" +
                       "  • Summarize - Get an overview of the log session\n" +
                       "  • Detect Anomalies - Find unusual patterns or issues\n" +
                       "  • Performance - Analyze slow operations\n" +
                       "  • Find Patterns - Identify recurring issues\n\n" +
                       "Or ask a question in natural language:\n" +
                       "  • \"What caused the errors?\"\n" +
                       "  • \"Which APIs are the slowest?\"\n" +
                       "  • \"Are there any performance issues?\"\n" +
                       "  • \"Show me anomalies\"\n\n" +
                       "Note: AI analysis is based on log statistics and patterns, " +
                       "not external API calls (works offline)."
            };

            // Add controls to panel
            this.Controls.Add(_responseTextBox);
            this.Controls.Add(queryPanel);
            this.Controls.Add(_buttonPanel);
            this.Controls.Add(_statusLabel);

            this.ResumeLayout();
        }

        private void QueryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                AskButton_Click(sender, e);
            }
        }

        private void AskButton_Click(object sender, EventArgs e)
        {
            string query = _queryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            QuerySubmitted?.Invoke(this, query);
        }

        public void ShowThinking(bool thinking)
        {
            _isThinking = thinking;

            if (thinking)
            {
                _statusLabel.Text = "?? Analyzing...";
                _askButton.Enabled = false;
                _summarizeButton.Enabled = false;
                _detectAnomaliesButton.Enabled = false;
                _analyzePerformanceButton.Enabled = false;
                _findPatternsButton.Enabled = false;
                _responseTextBox.AppendText("\n\n?? Analyzing...\n");
            }
            else
            {
                _statusLabel.Text = "?? AI Assistant - Ask questions about your log file";
                _askButton.Enabled = true;
                _summarizeButton.Enabled = true;
                _detectAnomaliesButton.Enabled = true;
                _analyzePerformanceButton.Enabled = true;
                _findPatternsButton.Enabled = true;
            }
        }

        public void ShowResponse(string response)
        {
            _responseTextBox.SelectionStart = _responseTextBox.TextLength;
            _responseTextBox.SelectionLength = 0;
            _responseTextBox.SelectionFont = new Font("Consolas", 9f);
            _responseTextBox.SelectionColor = Color.Black;
            _responseTextBox.AppendText("\n");
            _responseTextBox.AppendText(response);
            _responseTextBox.AppendText("\n");
            _responseTextBox.ScrollToCaret();
        }

        public void ShowError(string error)
        {
            _responseTextBox.SelectionStart = _responseTextBox.TextLength;
            _responseTextBox.SelectionLength = 0;
            _responseTextBox.SelectionFont = new Font("Consolas", 9f, FontStyle.Bold);
            _responseTextBox.SelectionColor = Color.Red;
            _responseTextBox.AppendText("\n? ERROR: ");
            _responseTextBox.SelectionFont = new Font("Consolas", 9f);
            _responseTextBox.AppendText(error);
            _responseTextBox.AppendText("\n");
            _responseTextBox.ScrollToCaret();
        }

        public void ShowAnalysis(string analysis)
        {
            ShowResponse(analysis);
        }

        public void ClearResponse()
        {
            _responseTextBox.Clear();
            _responseTextBox.Text = "Ready for your questions...\n";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(this.BackColor);
        }
    }
}
