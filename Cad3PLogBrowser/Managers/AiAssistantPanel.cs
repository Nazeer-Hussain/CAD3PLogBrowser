using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// AI Assistant Panel — Option B hybrid approach.
    ///
    /// Shows offline analysis by default. When Claude API is enabled in Settings,
    /// all buttons silently upgrade to real API calls with no UI change needed.
    ///
    /// Buttons: Summarize | Detect Anomalies | Root Cause | Bug Report | Performance | Patterns
    /// Chat:    Multi-turn text input that routes to L6 (offline keyword or Claude API).
    /// </summary>
    public class AiAssistantPanel : Panel
    {
        // ── Events ────────────────────────────────────────────────────────────
        public event EventHandler<string>  QuerySubmitted;
        public event EventHandler          SummarizeRequested;
        public event EventHandler          DetectAnomaliesRequested;
        public event EventHandler          AnalyzePerformanceRequested;
        public event EventHandler          FindPatternsRequested;
        public event EventHandler          RootCauseRequested;       // L4
        public event EventHandler          BugReportRequested;       // L5
        public event EventHandler<string>  ChatMessageSubmitted;     // L6

        // ── Controls ─────────────────────────────────────────────────────────
        private Label      _statusLabel;
        private Label      _apiModeLabel;
        private Panel      _buttonPanel;
        private Button     _summarizeBtn, _anomalyBtn, _perfBtn,
                           _patternsBtn, _rootCauseBtn, _bugReportBtn;
        private TextBox    _queryBox;
        private Button     _askBtn, _chatBtn, _clearBtn;
        private RichTextBox _responseBox;

        // Chat history for L6
        private readonly List<(string role, string content)> _chatHistory
            = new List<(string, string)>();
        public List<(string role, string content)> ChatHistory => _chatHistory;

        public AiAssistantPanel()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // ── API mode indicator ─────────────────────────────────────────
            _apiModeLabel = new Label
            {
                Text      = "Mode: Offline (enable Claude API in Settings > AI)",
                Dock      = DockStyle.Top,
                Height    = 22,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(130, 140, 160),
                Padding   = new Padding(8, 3, 0, 0),
                BackColor = Color.FromArgb(40, 44, 54)
            };

            // ── Title ──────────────────────────────────────────────────────
            _statusLabel = new Label
            {
                Text      = "AI Assistant — Log Analysis",
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 215, 240),
                Padding   = new Padding(8, 5, 0, 0),
                BackColor = Color.FromArgb(35, 38, 48)
            };

            // ── Button rows ────────────────────────────────────────────────
            _buttonPanel = new Panel
            {
                Height    = 70,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(35, 38, 48),
                Padding   = new Padding(6, 4, 6, 4)
            };

            _summarizeBtn  = MakeBtn("Summarize",       0);
            _anomalyBtn    = MakeBtn("Anomalies",       1);
            _rootCauseBtn  = MakeBtn("Root Cause",      2);
            _bugReportBtn  = MakeBtn("Bug Report",      3);
            _perfBtn       = MakeBtn("Performance",     4);
            _patternsBtn   = MakeBtn("Patterns",        5);

            _summarizeBtn.Click  += (s, e) => SummarizeRequested?.Invoke(this, EventArgs.Empty);
            _anomalyBtn.Click    += (s, e) => DetectAnomaliesRequested?.Invoke(this, EventArgs.Empty);
            _rootCauseBtn.Click  += (s, e) => RootCauseRequested?.Invoke(this, EventArgs.Empty);
            _bugReportBtn.Click  += (s, e) => BugReportRequested?.Invoke(this, EventArgs.Empty);
            _perfBtn.Click       += (s, e) => AnalyzePerformanceRequested?.Invoke(this, EventArgs.Empty);
            _patternsBtn.Click   += (s, e) => FindPatternsRequested?.Invoke(this, EventArgs.Empty);

            _buttonPanel.Controls.AddRange(new Control[]
                { _summarizeBtn, _anomalyBtn, _rootCauseBtn, _bugReportBtn, _perfBtn, _patternsBtn });

            // ── Chat / query input ─────────────────────────────────────────
            var inputPanel = new Panel
            {
                Height    = 36,
                Dock      = DockStyle.Bottom,
                BackColor = Color.FromArgb(35, 38, 48)
            };

            _queryBox = new TextBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = Color.FromArgb(48, 52, 64),
                ForeColor   = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,
                Text        = "Ask a question or type a chat message..."
            };

            // Implement placeholder behavior manually (PlaceholderText not available in .NET Framework 4.8)
            _queryBox.GotFocus += (s, e) =>
            {
                if (_queryBox.ForeColor == Color.Gray && _queryBox.Text == "Ask a question or type a chat message...")
                {
                    _queryBox.Text = "";
                    _queryBox.ForeColor = Color.FromArgb(210, 220, 235);
                }
            };
            _queryBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_queryBox.Text))
                {
                    _queryBox.Text = "Ask a question or type a chat message...";
                    _queryBox.ForeColor = Color.Gray;
                }
            };

            _queryBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; Submit(); }
            };

            _askBtn = MakeSmallBtn("Ask", DockStyle.Right, 55);
            _askBtn.Click += (s, e) => Submit();

            _chatBtn = MakeSmallBtn("Chat", DockStyle.Right, 55);
            _chatBtn.Click += (s, e) => SubmitChat();

            _clearBtn = MakeSmallBtn("Clear", DockStyle.Right, 50);
            _clearBtn.BackColor = Color.FromArgb(60, 40, 40);
            _clearBtn.Click += (s, e) => { _responseBox.Clear(); _chatHistory.Clear(); };

            inputPanel.Controls.Add(_queryBox);
            inputPanel.Controls.Add(_askBtn);
            inputPanel.Controls.Add(_chatBtn);
            inputPanel.Controls.Add(_clearBtn);

            // ── Response area ──────────────────────────────────────────────
            _responseBox = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                ReadOnly    = true,
                Font        = new Font("Consolas", 9f),
                BackColor   = Color.FromArgb(22, 24, 30),
                ForeColor   = Color.FromArgb(200, 215, 235),
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                Text        = "Welcome to AI Assistant\n\n" +
                              "Quick Actions (top buttons):\n" +
                              "  Summarize    — overall health and session overview\n" +
                              "  Anomalies    — statistical outliers and unusual patterns\n" +
                              "  Root Cause   — likely causes of errors and warnings\n" +
                              "  Bug Report   — structured bug report from log data\n" +
                              "  Performance  — top time consumers and call metrics\n" +
                              "  Patterns     — repeated errors, escalation, hotspots\n\n" +
                              "Chat / Ask:\n" +
                              "  Type a question and press Ask or Chat (multi-turn)\n" +
                              "  e.g. \"What caused the errors?\"\n" +
                              "       \"Which APIs are the slowest?\"\n\n" +
                              "Tip: Enable Claude API in Settings > AI for enhanced answers.\n"
            };

            // Add in reverse Dock order (Fill goes first)
            Controls.Add(_responseBox);
            Controls.Add(inputPanel);
            Controls.Add(_buttonPanel);
            Controls.Add(_apiModeLabel);
            Controls.Add(_statusLabel);

            ResumeLayout();
        }

        // ── Submit helpers ────────────────────────────────────────────────────
        private void Submit()
        {
            string q = _queryBox.Text.Trim();
            if (string.IsNullOrEmpty(q)) return;
            _queryBox.Clear();
            QuerySubmitted?.Invoke(this, q);
        }

        private void SubmitChat()
        {
            string q = _queryBox.Text.Trim();
            if (string.IsNullOrEmpty(q)) return;
            _queryBox.Clear();
            AppendChatTurn("user", q);
            ChatMessageSubmitted?.Invoke(this, q);
        }

        // ── Public API called by MainForm ─────────────────────────────────────
        public void SetApiMode(bool apiEnabled)
        {
            if (_apiModeLabel == null) return;
            _apiModeLabel.Text      = apiEnabled
                ? "Mode: Claude API (enhanced AI responses)"
                : "Mode: Offline (enable Claude API in Settings > AI)";
            _apiModeLabel.ForeColor = apiEnabled
                ? Color.FromArgb(100, 200, 120)
                : Color.FromArgb(130, 140, 160);
        }

        public void ShowThinking(bool thinking)
        {
            _statusLabel.Text = thinking ? "AI Assistant — Thinking..." : "AI Assistant — Log Analysis";
            bool e = !thinking;
            foreach (var btn in new[] { _summarizeBtn, _anomalyBtn, _rootCauseBtn,
                                        _bugReportBtn, _perfBtn, _patternsBtn, _askBtn, _chatBtn })
                btn.Enabled = e;
            _queryBox.Enabled = e;
            if (thinking) AppendText("\n[Analysing...]\n", Color.FromArgb(140, 160, 200));
        }

        public void ShowResponse(string response) => AppendText("\n" + response + "\n", Color.FromArgb(200, 215, 235));
        public void ShowError(string error)        => AppendText("\nERROR: " + error + "\n", Color.FromArgb(255, 100, 100));
        public void ShowAnalysis(string text)      => ShowResponse(text);

        public void AppendChatTurn(string role, string text)
        {
            _chatHistory.Add((role, text));
            Color color = role == "user" ? Color.FromArgb(130, 180, 255) : Color.FromArgb(120, 220, 150);
            string label = role == "user" ? "You" : "Claude";
            AppendText($"\n[{label}] ", color);
            AppendText(text + "\n", Color.FromArgb(200, 215, 235));
        }

        public void ClearResponse() { _responseBox.Clear(); _chatHistory.Clear(); }

        // ── Rendering helpers ─────────────────────────────────────────────────
        private void AppendText(string text, Color color)
        {
            if (InvokeRequired) { Invoke((Action<string, Color>)AppendText, text, color); return; }
            _responseBox.SelectionStart  = _responseBox.TextLength;
            _responseBox.SelectionLength = 0;
            _responseBox.SelectionColor  = color;
            _responseBox.AppendText(text);
            _responseBox.SelectionColor  = _responseBox.ForeColor;
            _responseBox.ScrollToCaret();
        }

        private static Button MakeBtn(string text, int index)
        {
            int col = index % 3, row = index / 3;
            return new Button
            {
                Text      = text,
                Location  = new Point(6 + col * 110, 4 + row * 30),
                Size      = new Size(106, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 60, 85),
                ForeColor = Color.FromArgb(200, 215, 240),
                Font      = new Font("Segoe UI", 8.5f),
                FlatAppearance = { BorderColor = Color.FromArgb(70, 90, 130) }
            };
        }

        private static Button MakeSmallBtn(string text, DockStyle dock, int width) => new Button
        {
            Text      = text,
            Width     = width,
            Dock      = dock,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 60, 85),
            ForeColor = Color.FromArgb(200, 215, 240),
            Font      = new Font("Segoe UI", 8.5f),
            FlatAppearance = { BorderColor = Color.FromArgb(70, 90, 130) }
        };

        /// <summary>
        /// Updates the panel colors based on current theme.
        /// Call this after theme changes.
        /// </summary>
        public void UpdateTheme()
        {
            bool isDark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

            if (isDark)
            {
                // Dark theme colors
                _apiModeLabel.BackColor = Color.FromArgb(40, 44, 54);
                _apiModeLabel.ForeColor = _apiModeLabel.Text.Contains("Claude API") 
                    ? Color.FromArgb(100, 200, 120) 
                    : Color.FromArgb(130, 140, 160);

                _statusLabel.BackColor = Color.FromArgb(35, 38, 48);
                _statusLabel.ForeColor = Color.FromArgb(200, 215, 240);

                _buttonPanel.BackColor = Color.FromArgb(35, 38, 48);

                foreach (Control ctrl in _buttonPanel.Controls)
                {
                    if (ctrl is Button btn)
                    {
                        btn.BackColor = Color.FromArgb(50, 60, 85);
                        btn.ForeColor = Color.FromArgb(200, 215, 240);
                        btn.FlatAppearance.BorderColor = Color.FromArgb(70, 90, 130);
                    }
                }

                _queryBox.BackColor = Color.FromArgb(48, 52, 64);
                _queryBox.ForeColor = _queryBox.Text == "Ask a question or type a chat message..." 
                    ? Color.Gray 
                    : Color.FromArgb(210, 220, 235);

                _responseBox.BackColor = Color.FromArgb(22, 24, 30);
                _responseBox.ForeColor = Color.FromArgb(200, 215, 235);

                foreach (Control ctrl in Controls)
                {
                    if (ctrl is Panel panel && panel != _buttonPanel)
                        panel.BackColor = Color.FromArgb(35, 38, 48);
                }

                _clearBtn.BackColor = Color.FromArgb(60, 40, 40);
            }
            else
            {
                // Light theme colors
                _apiModeLabel.BackColor = Color.FromArgb(240, 240, 245);
                _apiModeLabel.ForeColor = _apiModeLabel.Text.Contains("Claude API") 
                    ? Color.FromArgb(0, 120, 40) 
                    : Color.FromArgb(100, 100, 120);

                _statusLabel.BackColor = Color.FromArgb(230, 235, 245);
                _statusLabel.ForeColor = Color.FromArgb(40, 50, 80);

                _buttonPanel.BackColor = Color.FromArgb(235, 240, 250);

                foreach (Control ctrl in _buttonPanel.Controls)
                {
                    if (ctrl is Button btn)
                    {
                        btn.BackColor = Color.FromArgb(200, 210, 230);
                        btn.ForeColor = Color.FromArgb(40, 50, 70);
                        btn.FlatAppearance.BorderColor = Color.FromArgb(150, 160, 180);
                    }
                }

                _queryBox.BackColor = Color.White;
                _queryBox.ForeColor = _queryBox.Text == "Ask a question or type a chat message..." 
                    ? Color.Gray 
                    : Color.Black;

                _responseBox.BackColor = Color.White;
                _responseBox.ForeColor = Color.Black;

                foreach (Control ctrl in Controls)
                {
                    if (ctrl is Panel panel && panel != _buttonPanel)
                        panel.BackColor = Color.FromArgb(235, 240, 250);
                }

                _clearBtn.BackColor = Color.FromArgb(220, 200, 200);
            }

            Invalidate(true);
        }
    }
}
