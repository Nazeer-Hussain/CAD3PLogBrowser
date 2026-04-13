using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services.Analysis;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// L1–L6: AI Assistant tab panel.
    /// Provides buttons for AI Summarize, Anomaly Detection, Root Cause,
    /// Bug Report, and a chat interface for conversational log analysis.
    /// IMPORTANT: Only structured summaries are sent to the API — never raw logs.
    /// </summary>
    public class AiAssistantPanel : Panel
    {
        // ── Events ────────────────────────────────────────────────────────────
        public event EventHandler<AiRequest> RequestSubmitted;

        // ── Controls ─────────────────────────────────────────────────────────
        private readonly RichTextBox _outputBox;
        private readonly TextBox     _chatInput;
        private readonly Button      _summarizeBtn, _anomalyBtn, _rootCauseBtn,
                                     _bugReportBtn,  _sendChatBtn, _clearBtn;
        private readonly Label       _noKeyLabel;
        private readonly Panel       _buttonBar;

        public AiAssistantPanel()
        {
            BackColor = Color.FromArgb(30, 32, 38);

            // Warning label shown when no API key
            _noKeyLabel = new Label
            {
                Text      = "⚠  No Claude API key — add it in Options → Settings → AI tab.",
                ForeColor = Color.FromArgb(255, 200, 80),
                Dock      = DockStyle.Top,
                Height    = 24,
                Padding   = new Padding(4, 4, 0, 0),
                Font      = new Font("Segoe UI", 8.5f)
            };

            // Output area
            _outputBox = new RichTextBox
            {
                Dock      = DockStyle.Fill,
                ReadOnly  = true,
                BackColor = Color.FromArgb(22, 24, 30),
                ForeColor = Color.FromArgb(210, 220, 235),
                Font      = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical
            };

            // Chat input
            var chatPanel = new Panel { Dock = DockStyle.Bottom, Height = 36 };
            _chatInput = new TextBox
            {
                Dock        = DockStyle.Fill,
                PlaceholderText = "Ask a question about this log session...",
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = Color.FromArgb(40, 44, 54),
                ForeColor   = Color.FromArgb(210, 220, 235),
                BorderStyle = BorderStyle.FixedSingle
            };
            _chatInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; SendChat(); }
            };
            _sendChatBtn = MakeButton("Send", 80, DockStyle.Right);
            _sendChatBtn.Click += (s, e) => SendChat();
            chatPanel.Controls.Add(_chatInput);
            chatPanel.Controls.Add(_sendChatBtn);

            // Button bar
            _buttonBar = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.FromArgb(35, 38, 46) };
            _summarizeBtn = MakeButton("📋 Summarize",   110, DockStyle.Left);
            _anomalyBtn   = MakeButton("🔍 Anomalies",   100, DockStyle.Left);
            _rootCauseBtn = MakeButton("🎯 Root Cause",  100, DockStyle.Left);
            _bugReportBtn = MakeButton("🐛 Bug Report",  100, DockStyle.Left);
            _clearBtn     = MakeButton("🗑 Clear",         70, DockStyle.Right);
            _summarizeBtn.Click += (s, e) => Raise(AiRequestType.Summarize);
            _anomalyBtn.Click   += (s, e) => Raise(AiRequestType.Anomaly);
            _rootCauseBtn.Click += (s, e) => Raise(AiRequestType.RootCause);
            _bugReportBtn.Click += (s, e) => Raise(AiRequestType.BugReport);
            _clearBtn.Click     += (s, e) => { _outputBox.Clear(); _chatHistory.Clear(); };
            foreach (var btn in new[] { _summarizeBtn, _anomalyBtn, _rootCauseBtn, _bugReportBtn, _clearBtn })
                _buttonBar.Controls.Add(btn);

            Controls.Add(_outputBox);
            Controls.Add(_noKeyLabel);
            Controls.Add(chatPanel);
            Controls.Add(_buttonBar);
        }

        // ── Chat history ──────────────────────────────────────────────────────
        private readonly List<(string role, string content)> _chatHistory = new List<(string, string)>();

        private void SendChat()
        {
            string q = _chatInput.Text.Trim();
            if (string.IsNullOrEmpty(q)) return;
            _chatInput.Clear();
            Raise(AiRequestType.Chat, q);
        }

        private void Raise(AiRequestType type, string extra = null) =>
            RequestSubmitted?.Invoke(this, new AiRequest { Type = type, UserMessage = extra });

        // ── Public methods called by MainForm ─────────────────────────────────
        public void ShowApiKeyWarning(bool missing) => _noKeyLabel.Visible = missing;

        public void SetBusy(bool busy)
        {
            foreach (var btn in new[] { _summarizeBtn, _anomalyBtn, _rootCauseBtn, _bugReportBtn, _sendChatBtn })
                btn.Enabled = !busy;
            _chatInput.Enabled = !busy;
            if (busy) AppendOutput("\n⏳ Thinking...");
        }

        public void AppendOutput(string text)
        {
            if (InvokeRequired) { Invoke((Action<string>)AppendOutput, text); return; }
            _outputBox.AppendText(text + "\n");
            _outputBox.ScrollToCaret();
        }

        public void AppendChatTurn(string role, string text)
        {
            _chatHistory.Add((role, text));
            string label = role == "user" ? "You" : "Claude";
            Color  color = role == "user" ? Color.FromArgb(130, 180, 255) : Color.FromArgb(160, 230, 160);
            _outputBox.SelectionStart  = _outputBox.TextLength;
            _outputBox.SelectionLength = 0;
            _outputBox.SelectionColor  = color;
            _outputBox.AppendText($"\n[{label}] ");
            _outputBox.SelectionColor = Color.FromArgb(210, 220, 235);
            _outputBox.AppendText(text + "\n");
            _outputBox.ScrollToCaret();
        }

        public List<(string, string)> ChatHistory => _chatHistory;

        // ── Helper ────────────────────────────────────────────────────────────
        private static Button MakeButton(string text, int width, DockStyle dock) => new Button
        {
            Text      = text,
            Width     = width,
            Dock      = dock,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 60, 80),
            ForeColor = Color.FromArgb(210, 220, 240),
            Font      = new Font("Segoe UI", 8.5f),
            FlatAppearance = { BorderColor = Color.FromArgb(70, 90, 120) }
        };
    }

    public enum AiRequestType { Summarize, Anomaly, RootCause, BugReport, Chat }
    public class AiRequest
    {
        public AiRequestType Type        { get; set; }
        public string        UserMessage { get; set; }
    }
}
