using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Context;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Security;
using Cad3PLogBrowser.AI.Services;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// AI Assistant Panel with modern AI framework integration.
    /// Supports multiple AI providers (Anthropic, OpenAI, Azure OpenAI, etc.)
    /// </summary>
    public class AiAssistantPanel : Panel
    {
        // ?? AI Service ????????????????????????????????????????????????????????
        private AIService _aiService;
        private AISettings _aiSettings;
        private Func<Models.AggregateStats> _getStats;
        private Func<List<Services.ApiPerfStats>> _getPerfStats;
        private Func<string> _getCurrentFilePath;
        private Func<string> _getSelectedText;
        private CancellationTokenSource _cancellationTokenSource;

        // ?? Events ????????????????????????????????????????????????????????????
        public event EventHandler SettingsRequested;

        // ?? Controls ?????????????????????????????????????????????????????????
        private Label      _statusLabel;
        private Label      _apiModeLabel;
        private Panel      _buttonPanel;
        private Panel      _inputPanel;
        private Button     _summarizeBtn, _rootCauseBtn, _findErrorsBtn,
                           _findWarningsBtn, _perfBtn, _timelineBtn;
        private TextBox    _chatInputBox;
        private Button     _sendBtn, _clearBtn, _copyBtn, _settingsBtn;
        private RichTextBox _responseBox;
        private ProgressBar _progressBar;
        private Label       _tokenLabel;

        public AiAssistantPanel(
            Func<Models.AggregateStats> getStats,
            Func<List<Services.ApiPerfStats>> getPerfStats,
            Func<string> getCurrentFilePath,
            Func<string> getSelectedText)
        {
            _getStats = getStats;
            _getPerfStats = getPerfStats;
            _getCurrentFilePath = getCurrentFilePath;
            _getSelectedText = getSelectedText;

            InitializeAIService();
            BuildUI();
        }

        private void InitializeAIService()
        {
            try
            {
                var settings = AISettingsService.Load();
                _aiSettings = settings;
                _aiService = new AIService(settings);
                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize AI service: {ex.Message}");
            }
        }

        public void RefreshAIService()
        {
            InitializeAIService();
            UpdateStatusLabel();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // ?? API mode indicator ?????????????????????????????????????????
            _apiModeLabel = new Label
            {
                Text      = GetProviderStatus(),
                Dock      = DockStyle.Top,
                Height    = 22,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(130, 140, 160),
                Padding   = new Padding(8, 3, 0, 0),
                BackColor = Color.FromArgb(40, 44, 54)
            };

            // ?? Title with Settings button ????????????????????????????????
            var titlePanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                BackColor = Color.FromArgb(35, 38, 48)
            };

            _statusLabel = new Label
            {
                Text      = "AI Assistant",
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 215, 240),
                Padding   = new Padding(8, 7, 0, 0),
                BackColor = Color.Transparent
            };

            _settingsBtn = new Button
            {
                Text      = "Settings",
                Dock      = DockStyle.Right,
                Width     = 100,
                Height    = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 50, 62),
                ForeColor = Color.FromArgb(200, 215, 240),
                Margin    = new Padding(0, 2, 5, 2),
                Cursor    = Cursors.Hand
            };
            _settingsBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 65, 77);
            _settingsBtn.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

            titlePanel.Controls.AddRange(new Control[] { _statusLabel, _settingsBtn });

            // ?? Analysis Buttons ??????????????????????????????????????????
            _buttonPanel = new Panel
            {
                Height    = 70,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(35, 38, 48),
                Padding   = new Padding(6, 4, 6, 4)
            };

            _summarizeBtn     = MakeBtn("Summarize",       0);
            _rootCauseBtn     = MakeBtn("Root Cause",      1);
            _findErrorsBtn    = MakeBtn("Find Errors",     2);
            _findWarningsBtn  = MakeBtn("Warnings",   3);
            _perfBtn          = MakeBtn("Performance",     4);
            _timelineBtn      = MakeBtn("Timeline",        5);

            _summarizeBtn.Click     += async (s, e) => await RunAnalysisAsync(AnalysisType.Summarize);
            _rootCauseBtn.Click     += async (s, e) => await RunAnalysisAsync(AnalysisType.RootCause);
            _findErrorsBtn.Click    += async (s, e) => await RunAnalysisAsync(AnalysisType.FindErrors);
            _findWarningsBtn.Click  += async (s, e) => await RunAnalysisAsync(AnalysisType.FindWarnings);
            _perfBtn.Click          += async (s, e) => await RunAnalysisAsync(AnalysisType.Performance);
            _timelineBtn.Click      += async (s, e) => await RunAnalysisAsync(AnalysisType.Timeline);

            _buttonPanel.Controls.AddRange(new Control[]
            {
                _summarizeBtn, _rootCauseBtn, _findErrorsBtn,
                _findWarningsBtn, _perfBtn, _timelineBtn
            });

            // Response Display
            _responseBox = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = Color.FromArgb(30, 33, 43),
                ForeColor   = Color.FromArgb(210, 220, 235),
                BorderStyle = BorderStyle.None,
                ReadOnly    = true,
                Padding     = new Padding(10),
                Text        = "Welcome to AI Assistant!\n\n" +
                             "� Click any analysis button above to analyze the log\n" +
                             "� Or type a question below for interactive chat\n" +
                             "� Configure AI provider in Settings if not done yet",
                DetectUrls  = true,
                EnableAutoDragDrop = false,
                ShortcutsEnabled = true  // Enable Ctrl+C for copying
            };

            // ?? Progress Bar ??????????????????????????????????????????????
            _progressBar = new ProgressBar
            {
                Dock    = DockStyle.Bottom,
                Height  = 3,
                Style   = ProgressBarStyle.Marquee,
                Visible = false
            };

            // ?? Token Usage Display ???????????????????????????????????????
            _tokenLabel = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(130, 140, 160),
                Padding   = new Padding(8, 3, 0, 0),
                BackColor = Color.FromArgb(35, 38, 48),
                Text      = "Ready"
            };

            // ?? Chat Input ????????????????????????????????????????????????
            _inputPanel = new Panel
            {
                Height    = 40,
                Dock      = DockStyle.Bottom,
                BackColor = Color.FromArgb(35, 38, 48),
                Padding   = new Padding(6, 5, 6, 5)
            };

            _chatInputBox = new TextBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = Color.FromArgb(48, 52, 64),
                ForeColor   = Color.FromArgb(210, 220, 235),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline   = false,
                Height      = 30
            };
            _chatInputBox.KeyDown += ChatInput_KeyDown;

            _sendBtn = MakeSmallBtn("Send", DockStyle.Right, 70);
            _sendBtn.Click += async (s, e) => await SendChatMessageAsync();

            _copyBtn = MakeSmallBtn("Copy", DockStyle.Right, 70);
            _copyBtn.Click += (s, e) => CopyResponse();

            _clearBtn = MakeSmallBtn("Clear", DockStyle.Right, 70);
            _clearBtn.Click += (s, e) => ClearResponse();

            _inputPanel.Controls.AddRange(new Control[] { _chatInputBox, _sendBtn, _copyBtn, _clearBtn });

            // ?? Add all to panel ??????????????????????????????????????????
            Controls.AddRange(new Control[]
            {
                _responseBox,
                _progressBar,
                _tokenLabel,
                _inputPanel,
                _buttonPanel,
                titlePanel,
                _apiModeLabel
            });

            ResumeLayout();
        }

        // ?? Button Factory ????????????????????????????????????????????????????
        private Button MakeBtn(string text, int col)
        {
            int w = 90, h = 28, gap = 4;
            int rowHeight = h + gap;
            int row = col / 3;
            int colInRow = col % 3;

            return new Button
            {
                Text      = text,
                Location  = new Point(gap + colInRow * (w + gap), gap + row * rowHeight),
                Size      = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 50, 62),
                ForeColor = Color.FromArgb(200, 215, 240),
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
        }

        private Button MakeSmallBtn(string text, DockStyle dock, int width)
        {
            var btn = new Button
            {
                Text      = text,
                Dock      = dock,
                Width     = width,
                Height    = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 50, 62),
                ForeColor = Color.FromArgb(200, 215, 240),
                Font      = new Font("Segoe UI", 9f),
                Margin    = new Padding(3, 0, 0, 0),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, 65, 77);
            return btn;
        }

        // ?? Analysis Execution ????????????????????????????????????????????????
        private async Task RunAnalysisAsync(AnalysisType analysisType)
        {
            if (!CheckAIAvailable()) return;

            // Cancel any ongoing operation
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            // Create context providers
            var contextProviders = CreateContextProviders();

            // Clear previous response
            _responseBox.Clear();
            ShowProgress($"Running {analysisType} analysis...");

            try
            {
                await _aiService.AnalyzeStreamingAsync(
                    analysisType,
                    contextProviders,
                    onChunkReceived: chunk => AppendText(chunk),
                    onComplete: result => OnAnalysisCompleteAnalysis(result),
                    onError: ex => OnAnalysisError(ex),
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                OnAnalysisError(ex);
            }
        }

        private async Task SendChatMessageAsync()
        {
            string message = _chatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            if (!CheckAIAvailable()) return;

            // Start conversation if not already started
            if (_aiService.ActiveConversation == null)
            {
                _aiService.StartConversation();
                _responseBox.Clear();
            }

            // Display user message
            AppendText($"\n\nYou: {message}\n\n");
            _chatInputBox.Clear();

            // Context for first message only
            var contextProviders = _aiService.ActiveConversation.Messages.Count == 0
                ? CreateContextProviders()
                : null;

            // Cancel any ongoing operation
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            ShowProgress("AI is thinking...");
            AppendText("AI: ");

            try
            {
                await _aiService.SendConversationMessageStreamingAsync(
                    message,
                    onChunkReceived: chunk => AppendText(chunk),
                    onComplete: result => OnAnalysisComplete(result),
                    onError: ex => OnAnalysisError(ex),
                    contextProviders: contextProviders,
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                OnAnalysisError(ex);
            }
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                _ = SendChatMessageAsync();
            }
        }

        // ?? Context Providers ?????????????????????????????????????????????????
        private List<IContextProvider> CreateContextProviders()
        {
            var providers = new List<IContextProvider>();

            try
            {
                // DEF-AI01: raw log text (selected lines) must never reach the AI API
                // un-redacted, regardless of which provider constructs it or how it's
                // later combined into a prompt — redact at the source, not just once
                // centrally in AIService, so this stays safe even if a future caller
                // consumes these providers directly.
                bool redact = _aiSettings?.RedactSensitiveData ?? true;

                var stats = _getStats?.Invoke();
                if (stats != null)
                {
                    providers.Add(new CurrentLogContextProvider(
                        stats,
                        _getCurrentFilePath?.Invoke(),
                        _getSelectedText,
                        redactSensitiveData: redact));
                }

                var selectedText = _getSelectedText?.Invoke();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    providers.Add(new SelectedLinesContextProvider(_getSelectedText, redactSensitiveData: redact));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating context providers: {ex.Message}");
            }

            return providers;
        }

        // ?? UI Updates ????????????????????????????????????????????????????????
        private void ShowProgress(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowProgress(message)));
                return;
            }

            _progressBar.Visible = true;
            _tokenLabel.Text = message;
            _sendBtn.Enabled = false;
            _summarizeBtn.Enabled = false;
            _rootCauseBtn.Enabled = false;
            _findErrorsBtn.Enabled = false;
            _findWarningsBtn.Enabled = false;
            _perfBtn.Enabled = false;
            _timelineBtn.Enabled = false;
        }

        private void HideProgress()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(HideProgress));
                return;
            }

            _progressBar.Visible = false;
            _sendBtn.Enabled = true;
            _summarizeBtn.Enabled = true;
            _rootCauseBtn.Enabled = true;
            _findErrorsBtn.Enabled = true;
            _findWarningsBtn.Enabled = true;
            _perfBtn.Enabled = true;
            _timelineBtn.Enabled = true;
        }

        private void AppendText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendText(text)));
                return;
            }

            // Text should already be unescaped by the provider
            // Just apply markdown formatting directly
            ApplyMarkdownFormatting(text);

            _responseBox.SelectionStart = _responseBox.Text.Length;
            _responseBox.ScrollToCaret();
        }

        private void ApplyMarkdownFormatting(string text)
        {
            // Save current position
            int startPos = _responseBox.Text.Length;

            // Append the text first
            _responseBox.AppendText(text);

            // Temporarily allow editing for formatting
            bool wasReadOnly = _responseBox.ReadOnly;
            _responseBox.ReadOnly = false;

            try
            {
                // Get the full text to check for complete markdown patterns
                string fullText = _responseBox.Text;

            // Process markdown formatting for the newly added text
            // We need to check if we have complete markdown patterns

            // Find and format bold text (**text**)
            // Start from the beginning of the newly added text, but look backwards a bit
            // in case a markdown pattern spans across chunks
            int searchStart = Math.Max(0, startPos - 100); // Look back 100 chars

            while (searchStart < fullText.Length)
            {
                int boldStart = fullText.IndexOf("**", searchStart);
                if (boldStart == -1 || boldStart >= _responseBox.Text.Length) break;

                int boldEnd = fullText.IndexOf("**", boldStart + 2);
                if (boldEnd == -1 || boldEnd >= _responseBox.Text.Length) break;

                // Only process if both markers are present
                if (boldEnd > boldStart + 2)
                {
                    // Check if this region was already formatted
                    // by seeing if the markers still exist
                    _responseBox.Select(boldStart, 2);
                    if (_responseBox.SelectedText == "**")
                    {
                        // Extract the bold text
                        string boldText = fullText.Substring(boldStart + 2, boldEnd - boldStart - 2);

                        // Remove opening **
                        _responseBox.Select(boldStart, 2);
                        _responseBox.SelectedText = "";

                        // Remove closing ** (now at boldStart + boldText.Length)
                        _responseBox.Select(boldStart + boldText.Length, 2);
                        _responseBox.SelectedText = "";

                        // Make the text bold and colored
                        _responseBox.Select(boldStart, boldText.Length);
                        _responseBox.SelectionFont = new Font(_responseBox.Font, FontStyle.Bold);
                        _responseBox.SelectionColor = Color.FromArgb(255, 200, 100); // Orange/gold

                        // Update fullText since we removed characters
                        fullText = _responseBox.Text;
                        searchStart = boldStart + boldText.Length;
                    }
                    else
                    {
                        searchStart = boldEnd + 2;
                    }
                }
                else
                {
                    searchStart = boldEnd + 2;
                }
            }

            // Find and format headers (### text)
            searchStart = Math.Max(0, startPos - 100);
            fullText = _responseBox.Text;

            while (searchStart < fullText.Length)
            {
                int lineStart = fullText.IndexOf("###", searchStart);
                if (lineStart == -1 || lineStart >= fullText.Length) break;

                // Check if ### is at the start of a line
                if (lineStart > 0 && fullText[lineStart - 1] != '\n' && fullText[lineStart - 1] != '\r')
                {
                    searchStart = lineStart + 3;
                    continue;
                }

                // Check if we already processed this (no ### markers left)
                _responseBox.Select(lineStart, 3);
                if (_responseBox.SelectedText != "###")
                {
                    searchStart = lineStart + 3;
                    continue;
                }

                int lineEnd = fullText.IndexOf('\n', lineStart);
                if (lineEnd == -1) lineEnd = fullText.Length;

                // Format the entire header line (including ###)
                _responseBox.Select(lineStart, lineEnd - lineStart);
                _responseBox.SelectionFont = new Font(_responseBox.Font.FontFamily, _responseBox.Font.Size, FontStyle.Bold);
                _responseBox.SelectionColor = Color.FromArgb(100, 150, 255); // Light blue

                searchStart = lineEnd + 1;
            }

            // Reset selection
            _responseBox.SelectionStart = _responseBox.Text.Length;
            _responseBox.SelectionLength = 0;
            _responseBox.SelectionFont = new Font(_responseBox.Font.FontFamily, _responseBox.Font.Size, FontStyle.Regular);
            _responseBox.SelectionColor = _responseBox.ForeColor;
            }
            finally
            {
                // Restore read-only state
                _responseBox.ReadOnly = wasReadOnly;
            }
        }

        private void OnAnalysisComplete(IAIResponse result)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAnalysisComplete(result)));
                return;
            }

            // Reformat all text now that streaming is complete
            ReformatAllMarkdown();

            HideProgress();

            if (result.Success)
            {
                var tokenInfo = result.TotalTokens.HasValue
                    ? $" � {result.TotalTokens.Value} tokens"
                    : "";
                var timeInfo = result.ElapsedTime.TotalSeconds > 0
                    ? $" � {result.ElapsedTime.TotalSeconds:F1}s"
                    : "";
                _tokenLabel.Text = $"Complete{tokenInfo}{timeInfo}";
            }
            else
            {
                _tokenLabel.Text = "Failed";
            }
        }

        private void OnAnalysisCompleteAnalysis(AnalysisResult result)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAnalysisCompleteAnalysis(result)));
                return;
            }

            // Reformat all text now that streaming is complete
            ReformatAllMarkdown();

            HideProgress();

            if (result.Success)
            {
                var tokenInfo = result.TokensUsed.HasValue
                    ? $" � {result.TokensUsed.Value} tokens"
                    : "";
                var timeInfo = result.ElapsedTime.TotalSeconds > 0
                    ? $" � {result.ElapsedTime.TotalSeconds:F1}s"
                    : "";
                _tokenLabel.Text = $"Complete{tokenInfo}{timeInfo}";
            }
            else
            {
                _tokenLabel.Text = "Failed";
            }
        }

        private void ReformatAllMarkdown()
        {
            if (_responseBox.Text.Length == 0) return;

            try
            {
                // Temporarily allow editing for formatting
                _responseBox.ReadOnly = false;
                _responseBox.SuspendLayout();

                string fullText = _responseBox.Text;

                // Process all bold text (**text**)
                int searchStart = 0;
                int boldCount = 0;
                while (searchStart < fullText.Length)
                {
                    int boldStart = fullText.IndexOf("**", searchStart);
                    if (boldStart == -1) break;

                    int boldEnd = fullText.IndexOf("**", boldStart + 2);
                    if (boldEnd == -1) break;

                    if (boldEnd > boldStart + 2)
                    {
                        // Check if markers still exist
                        _responseBox.Select(boldStart, 2);
                        if (_responseBox.SelectedText == "**")
                        {
                            string boldText = fullText.Substring(boldStart + 2, boldEnd - boldStart - 2);

                            // Remove opening **
                            _responseBox.Select(boldStart, 2);
                            _responseBox.SelectedText = "";

                            // Remove closing **
                            _responseBox.Select(boldStart + boldText.Length, 2);
                            _responseBox.SelectedText = "";

                            // Format as bold with color
                            _responseBox.Select(boldStart, boldText.Length);
                            var boldFont = new Font(_responseBox.Font.FontFamily, _responseBox.Font.Size, FontStyle.Bold);
                            _responseBox.SelectionFont = boldFont;
                            _responseBox.SelectionColor = Color.FromArgb(255, 200, 100);

                            boldCount++;
                            fullText = _responseBox.Text;
                            searchStart = boldStart + boldText.Length;
                        }
                        else
                        {
                            searchStart = boldEnd + 2;
                        }
                    }
                    else
                    {
                        searchStart = boldEnd + 2;
                    }
                }

                // Process all headers (### text)
                searchStart = 0;
                fullText = _responseBox.Text;
                int headerCount = 0;

                while (searchStart < fullText.Length)
                {
                    int lineStart = fullText.IndexOf("###", searchStart);
                    if (lineStart == -1) break;

                    // Check if ### is at start of line
                    if (lineStart > 0 && fullText[lineStart - 1] != '\n' && fullText[lineStart - 1] != '\r')
                    {
                        searchStart = lineStart + 3;
                        continue;
                    }

                    int lineEnd = fullText.IndexOf('\n', lineStart);
                    if (lineEnd == -1) lineEnd = fullText.Length;

                    // Format header line
                    _responseBox.Select(lineStart, lineEnd - lineStart);
                    var headerFont = new Font(_responseBox.Font.FontFamily, _responseBox.Font.Size, FontStyle.Bold);
                    _responseBox.SelectionFont = headerFont;
                    _responseBox.SelectionColor = Color.FromArgb(100, 150, 255);

                    headerCount++;
                    searchStart = lineEnd + 1;
                }

                // Reset selection to end
                _responseBox.SelectionStart = _responseBox.Text.Length;
                _responseBox.SelectionLength = 0;
                var defaultFont = new Font(_responseBox.Font.FontFamily, _responseBox.Font.Size, FontStyle.Regular);
                _responseBox.SelectionFont = defaultFont;
                _responseBox.SelectionColor = Color.FromArgb(210, 220, 235);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Markdown] Error: {ex.Message}");
            }
            finally
            {
                _responseBox.ReadOnly = true;  // Restore read-only
                _responseBox.ResumeLayout();
            }
        }

        private void OnAnalysisError(Exception ex)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAnalysisError(ex)));
                return;
            }

            HideProgress();
            AppendText($"\n\nError: {ex.Message}");
            _tokenLabel.Text = "Error occurred";
        }

        private void ClearResponse()
        {
            _responseBox.Clear();
            _aiService?.EndConversation();
            _tokenLabel.Text = "Ready";
            _responseBox.Text = "Conversation cleared. Start a new chat or run an analysis.";
        }

        private void CopyResponse()
        {
            if (string.IsNullOrWhiteSpace(_responseBox.Text))
            {
                MessageBox.Show("No content to copy.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Clipboard.SetText(_responseBox.Text);
                _tokenLabel.Text = "Copied to clipboard";

                // Reset status after 2 seconds
                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    _tokenLabel.Text = "Ready";
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy: {ex.Message}", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusLabel()
        {
            if (_apiModeLabel != null)
            {
                _apiModeLabel.Text = GetProviderStatus();
            }
        }

        private string GetProviderStatus()
        {
            if (_aiService == null || !_aiService.IsEnabled)
                return "AI Disabled - Click Settings to configure";

            var provider = _aiService.CurrentProvider;
            if (provider == null)
                return "No provider configured";

            return $"{provider.ProviderName} ready";
        }

        private bool CheckAIAvailable()
        {
            if (_aiService == null || !_aiService.IsEnabled)
            {
                var result = MessageBox.Show(
                    "AI features are not configured.\n\nWould you like to configure them now?",
                    "AI Not Configured",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SettingsRequested?.Invoke(this, EventArgs.Empty);
                }

                return false;
            }

            return true;
        }

        // ?? Theme Support ?????????????????????????????????????????????????????
        public void UpdateTheme()
        {
            // This method can be called from MainForm when theme changes
            // Color scheme already set in BuildUI, but can be updated here if needed
        }

        public void SetApiMode(bool enabled)
        {
            // Legacy method for compatibility
            UpdateStatusLabel();
        }

        public void AppendResponse(string text)
        {
            AppendText(text);
        }

        public void SetStatus(string text)
        {
            if (_tokenLabel != null)
            {
                _tokenLabel.Text = text;
            }
        }
    }
}
