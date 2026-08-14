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
using Cad3PLogBrowser.Services;
using Cad3PLogBrowser.Services.Analysis;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// AI Assistant Panel with modern AI framework integration.
    /// Supports multiple AI providers (Anthropic, OpenAI, Azure OpenAI, Ollama, etc.).
    ///
    /// Merges two response sources into one experience:
    ///  - Real analysis: when a configured AI provider is enabled and reachable,
    ///    responses stream from <see cref="AIService"/> exactly as before.
    ///  - Sample/canned analysis: when AI is disabled, unconfigured, or the live
    ///    call fails (e.g. offline provider), a deterministic, rule-based response
    ///    is generated locally by <see cref="AiLogService"/> and clearly labeled
    ///    as a SAMPLE response so it is never mistaken for real AI output.
    /// </summary>
    public class AiAssistantPanel : Panel
    {
        // ?? AI Service ????????????????????????????????????????????????????????
        private AIService _aiService;
        private AISettings _aiSettings;
        // Offline/rule-based fallback used whenever the real provider is disabled,
        // unconfigured, or a live request fails — output is always clearly labeled
        // as a SAMPLE response so users can never confuse it with real AI analysis.
        private readonly AiLogService _canned = new AiLogService();
        private Func<Models.AggregateStats> _getStats;
        private Func<List<Services.ApiPerfStats>> _getPerfStats;
        private Func<string> _getCurrentFilePath;
        private Func<string> _getSelectedText;
        private CancellationTokenSource _cancellationTokenSource;

        private const string SampleBanner =
            "?????????????????????????????????????????????????????????\n" +
            "?? SAMPLE RESPONSE — not real AI analysis.\n" +
            "AI is disabled, unconfigured, or unreachable. This is a\n" +
            "deterministic, rule-based summary generated locally.\n" +
            "Configure and enable a provider in Settings for real analysis.\n" +
            "?????????????????????????????????????????????????????????\n\n";

        // ?? Events ????????????????????????????????????????????????????????????
        public event EventHandler SettingsRequested;

        // ?? Controls ?????????????????????????????????????????????????????????
        private Panel      _titlePanel;
        private Label      _statusLabel;
        private Label      _apiModeLabel;
        private FlowLayoutPanel _buttonPanel;
        private Panel      _inputPanel;
        private Button     _summarizeBtn, _rootCauseBtn, _findErrorsBtn,
                           _findWarningsBtn, _perfBtn, _timelineBtn;
        private FlowLayoutPanel _promptChipsPanel;
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

        // ── Theme-aware styling helpers ──────────────────────────────────────
        // AiAssistantPanel used to hardcode a fixed dark palette independent of
        // ThemeManager, so it looked out of place whenever the app itself was in
        // Light theme. These mirror the exact rules ThemeManager.ApplyThemeToControls
        // applies to ordinary buttons/panels elsewhere in the app, so the AI tab
        // renders identically to the rest of the UI in both themes.
        private static void StyleButton(Button btn)
        {
            bool dark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
            btn.ForeColor = ThemeManager.ControlForegroundColor;
            btn.BackColor = ThemeManager.ButtonBackgroundColor;
            btn.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
            if (dark)
            {
                btn.FlatAppearance.BorderColor = ThemeManager.BorderColor;
                btn.FlatAppearance.MouseOverBackColor = ThemeManager.ButtonHoverColor;
            }
        }

        // Secondary/status text (API mode strip, token counter) reads softer than
        // primary text everywhere else in the app; ThemeManager has no dedicated
        // "muted" color, so blend foreground toward background instead of a
        // literal gray that would only look right in one theme.
        private static Color MutedForeground =>
            Blend(ThemeManager.ForegroundColor, ThemeManager.BackgroundColor, 0.45);

        private static Color Blend(Color a, Color b, double t) => Color.FromArgb(
            (int)(a.R * (1 - t) + b.R * t),
            (int)(a.G * (1 - t) + b.G * t),
            (int)(a.B * (1 - t) + b.B * t));

        // Markdown-style accents in AI responses (bold/headers) need enough
        // contrast against BackgroundColor in both themes -- the original literal
        // gold/blue only read correctly on the panel's old fixed dark background.
        private static Color BoldAccentColor =>
            ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                ? Color.FromArgb(255, 200, 100)
                : Color.FromArgb(150, 90, 0);

        private static Color HeaderAccentColor =>
            ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                ? Color.FromArgb(100, 150, 255)
                : Color.FromArgb(20, 80, 190);

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
                ForeColor = MutedForeground,
                Padding   = new Padding(8, 3, 0, 0),
                BackColor = ThemeManager.ControlBackgroundColor
            };

            // ?? Title with Settings button ????????????????????????????????
            _titlePanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                BackColor = ThemeManager.ControlBackgroundColor
            };

            _statusLabel = new Label
            {
                Text      = "AI Assistant",
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ThemeManager.ForegroundColor,
                Padding   = new Padding(8, 7, 0, 0),
                BackColor = Color.Transparent
            };

            _settingsBtn = new Button
            {
                Text      = "Settings",
                Dock      = DockStyle.Right,
                Width     = 100,
                Height    = 28,
                Margin    = new Padding(0, 2, 5, 2),
                Cursor    = Cursors.Hand
            };
            StyleButton(_settingsBtn);
            _settingsBtn.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

            _titlePanel.Controls.AddRange(new Control[] { _statusLabel, _settingsBtn });

            // ?? Analysis Buttons ??????????????????????????????????????????
            // FlowLayoutPanel with WrapContents lets the buttons reflow to fit the
            // available width instead of relying on hardcoded pixel Location/Size
            // math, which previously misaligned buttons whenever the panel was
            // resized or the font/DPI differed from the assumed 90x28 grid cell.
            _buttonPanel = new FlowLayoutPanel
            {
                AutoSize      = true,
                Dock          = DockStyle.Top,
                BackColor     = ThemeManager.ControlBackgroundColor,
                Padding       = new Padding(6, 6, 6, 6),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true
            };

            _summarizeBtn     = MakeBtn("Summarize");
            _rootCauseBtn     = MakeBtn("Root Cause");
            _findErrorsBtn    = MakeBtn("Find Errors");
            _findWarningsBtn  = MakeBtn("Warnings");
            _perfBtn          = MakeBtn("Performance");
            _timelineBtn      = MakeBtn("Timeline");

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
                BackColor   = ThemeManager.BackgroundColor,
                ForeColor   = ThemeManager.ForegroundColor,
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
                ForeColor = MutedForeground,
                Padding   = new Padding(8, 3, 0, 0),
                BackColor = ThemeManager.ControlBackgroundColor,
                Text      = "Ready"
            };

            // ?? Chat Input ????????????????????????????????????????????????
            _inputPanel = new Panel
            {
                Height    = 40,
                Dock      = DockStyle.Bottom,
                BackColor = ThemeManager.ControlBackgroundColor,
                Padding   = new Padding(6, 5, 6, 5)
            };

            _chatInputBox = new TextBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = ThemeManager.InputBackgroundColor,
                ForeColor   = ThemeManager.ControlForegroundColor,
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

            // ?? Example Prompt Chips (L6) ?????????????????????????????????
            // Clickable suggestions that send a ready-made question through the
            // same conversational chat path as typing + Send, so first-time users
            // discover what natural-language questions the assistant supports.
            _promptChipsPanel = new FlowLayoutPanel
            {
                Height        = 34,
                Dock          = DockStyle.Bottom,
                BackColor     = ThemeManager.ControlBackgroundColor,
                Padding       = new Padding(6, 4, 6, 4),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };
            _promptChipsPanel.Controls.AddRange(new Control[]
            {
                MakeChip("Summarize this log"),
                MakeChip("Slowest methods?"),
                MakeChip("Any errors?")
            });

            BackColor = ThemeManager.BackgroundColor;

            // ?? Add all to panel ??????????????????????????????????????????
            Controls.AddRange(new Control[]
            {
                _responseBox,
                _progressBar,
                _tokenLabel,
                _inputPanel,
                _promptChipsPanel,
                _buttonPanel,
                _titlePanel,
                _apiModeLabel
            });

            ResumeLayout();
        }

        // ?? Button Factory ????????????????????????????????????????????????????
        private Button MakeBtn(string text)
        {
            var btn = new Button
            {
                Text      = text,
                AutoSize  = true,
                MinimumSize = new Size(90, 28),
                Padding   = new Padding(10, 0, 10, 0),
                Margin    = new Padding(0, 0, 6, 6),
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            StyleButton(btn);
            return btn;
        }

        private Button MakeSmallBtn(string text, DockStyle dock, int width)
        {
            var btn = new Button
            {
                Text      = text,
                Dock      = dock,
                Width     = width,
                Height    = 28,
                Font      = new Font("Segoe UI", 9f),
                Margin    = new Padding(3, 0, 0, 0),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            StyleButton(btn);
            return btn;
        }

        // L6: example prompt chip — clicking it sends the given text as a chat message.
        private Button MakeChip(string promptText)
        {
            var chip = new Button
            {
                Text      = promptText,
                AutoSize  = true,
                Height    = 24,
                Padding   = new Padding(8, 0, 8, 0),
                Font      = new Font("Segoe UI", 8f),
                Margin    = new Padding(0, 0, 6, 0),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            StyleButton(chip);
            chip.Click += async (s, e) =>
            {
                _chatInputBox.Text = promptText;
                await SendChatMessageAsync();
            };
            return chip;
        }

        /// <summary>L2: entry point for asking a question from outside this panel
        /// (e.g. an "Ask AI about this method" command near the Call Tree) — sends it
        /// through the exact same conversational path as typing into the chat box
        /// directly, so it gets the same context redaction, history capping, etc.</summary>
        public async Task AskQuestion(string question)
        {
            if (string.IsNullOrWhiteSpace(question)) return;
            _chatInputBox.Text = question;
            await SendChatMessageAsync();
        }

        // L4: keyed by method + its specific call chain, so asking about the same
        // node twice (e.g. re-opening the context menu) doesn't re-call the AI.
        private readonly Dictionary<string, string> _rootCauseCache = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>L4: node-specific Root Cause — unlike the generic Root Cause button
        /// (whole-log aggregate stats only), this analyzes one specific call using its
        /// actual parent-chain context (what called it, at what depth, with what
        /// timing), and caches the result per method+chain.</summary>
        public async Task AnalyzeNodeRootCause(string methodName, string parentChainContext)
        {
            string cacheKey = methodName + "␟" + parentChainContext;
            if (_rootCauseCache.TryGetValue(cacheKey, out string cached))
            {
                _responseBox.Clear();
                AppendText(cached);
                ReformatAllMarkdown();
                _tokenLabel.Text = "Root cause (cached)";
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            _responseBox.Clear();

            if (!IsRealAiAvailable)
            {
                await RunCannedAnalysisAsync(AnalysisType.RootCause);
                return;
            }

            var providers = new List<IContextProvider>
            {
                new PlainTextContextProvider("Specific Call Chain", parentChainContext)
            };

            ShowProgress(string.Format("Analyzing root cause for {0}...", methodName));

            var captured = new System.Text.StringBuilder();
            try
            {
                await _aiService.AnalyzeStreamingAsync(
                    AnalysisType.RootCause,
                    providers,
                    onChunkReceived: chunk => { captured.Append(chunk); AppendText(chunk); },
                    onComplete: result =>
                    {
                        if (result.Success) _rootCauseCache[cacheKey] = captured.ToString();
                        OnAnalysisCompleteAnalysis(result);
                    },
                    onError: async ex =>
                    {
                        _responseBox.Clear();
                        await RunCannedAnalysisAsync(AnalysisType.RootCause, realAiError: ex.Message);
                    },
                    userQuery: string.Format(
                        "Analyze the likely root cause for '{0}' using ONLY the specific call chain below " +
                        "(not general log statistics) — what called it, at what depth, and with what timing.",
                        methodName),
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _responseBox.Clear();
                await RunCannedAnalysisAsync(AnalysisType.RootCause, realAiError: ex.Message);
            }
        }

        /// <summary>Minimal IContextProvider wrapping a single pre-built block of text —
        /// used by AnalyzeNodeRootCause to inject the specific call chain instead of the
        /// generic whole-log context CreateContextProviders() builds.</summary>
        private class PlainTextContextProvider : Cad3PLogBrowser.AI.Context.ContextProviderBase
        {
            private readonly string _title;
            private readonly string _text;
            public PlainTextContextProvider(string title, string text) { _title = title; _text = text; }
            public override string ContextType => "PlainText";
            public override string Description => _title;
            public override bool HasContext => !string.IsNullOrEmpty(_text);
            public override Task<string> GetContextAsync() =>
                Task.FromResult(string.Format("### {0}\n\n{1}\n", _title, _text));
        }

        // ?? Analysis Execution ????????????????????????????????????????????????
        private async Task RunAnalysisAsync(AnalysisType analysisType)
        {
            // Cancel any ongoing operation
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            // Clear previous response
            _responseBox.Clear();

            if (!IsRealAiAvailable)
            {
                await RunCannedAnalysisAsync(analysisType);
                return;
            }

            // Create context providers
            var contextProviders = CreateContextProviders();

            ShowProgress($"Running {analysisType} analysis...");

            try
            {
                await _aiService.AnalyzeStreamingAsync(
                    analysisType,
                    contextProviders,
                    onChunkReceived: chunk => AppendText(chunk),
                    onComplete: result => OnAnalysisCompleteAnalysis(result),
                    onError: async ex =>
                    {
                        // Real provider failed at runtime (e.g. offline/unreachable) —
                        // gracefully fall back to a clearly-labeled sample response
                        // instead of just showing an error.
                        _responseBox.Clear();
                        await RunCannedAnalysisAsync(analysisType, realAiError: ex.Message);
                    },
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _responseBox.Clear();
                await RunCannedAnalysisAsync(analysisType, realAiError: ex.Message);
            }
        }

        /// <summary>Runs the local rule-based fallback and clearly labels the output as a sample.</summary>
        private async Task RunCannedAnalysisAsync(AnalysisType analysisType, string realAiError = null)
        {
            ShowProgress($"Generating sample {analysisType} response (AI unavailable)...");
            try
            {
                string content = await GetCannedAnalysisAsync(analysisType);
                string banner = realAiError != null
                    ? SampleBanner.Replace("AI is disabled, unconfigured, or unreachable.",
                        $"Real AI request failed ({realAiError}).")
                    : SampleBanner;

                AppendText(banner + content);
                OnAnalysisCompleteAnalysis(AnalysisResult.CreateSuccess(content, analysisType));
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

            _chatInputBox.Clear();

            if (!IsRealAiAvailable)
            {
                AppendText($"\n\nYou: {message}\n\n");
                await RunCannedChatAsync(message);
                return;
            }

            // Start conversation if not already started
            if (_aiService.ActiveConversation == null)
            {
                _aiService.StartConversation();
                _responseBox.Clear();
            }

            // Display user message
            AppendText($"\n\nYou: {message}\n\n");

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
                    onError: async ex =>
                    {
                        // Real provider failed mid-conversation — fall back to a
                        // clearly-labeled sample answer rather than a bare error.
                        await RunCannedChatAsync(message, realAiError: ex.Message);
                    },
                    contextProviders: contextProviders,
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                await RunCannedChatAsync(message, realAiError: ex.Message);
            }
        }

        /// <summary>Runs the local rule-based fallback for chat and clearly labels the output as a sample.</summary>
        private async Task RunCannedChatAsync(string message, string realAiError = null)
        {
            ShowProgress("Generating sample response (AI unavailable)...");
            AppendText("AI: ");
            try
            {
                var stats = _getStats?.Invoke() ?? new Models.AggregateStats();
                var perfStats = _getPerfStats?.Invoke() ?? new List<Services.ApiPerfStats>();
                string content = await _canned.NlSearchAsync(message, stats, perfStats);
                string banner = realAiError != null
                    ? SampleBanner.Replace("AI is disabled, unconfigured, or unreachable.",
                        $"Real AI request failed ({realAiError}).")
                    : SampleBanner;

                AppendText(banner + content);
                OnAnalysisCompleteAnalysis(AnalysisResult.CreateSuccess(content, AnalysisType.Custom));
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

                // L1: feed the per-method call-count/duration breakdown that the AI Log
                // Summarizer spec calls for. _getPerfStats was already being collected and
                // passed into this panel but never actually used — without it, a "Slowest
                // methods" analysis had no real timing data to draw on.
                var perfStats = _getPerfStats?.Invoke();
                if (perfStats != null && perfStats.Count > 0)
                {
                    providers.Add(new ApiPerformanceContextProvider(perfStats));
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
                        _responseBox.SelectionColor = BoldAccentColor;

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
                _responseBox.SelectionColor = HeaderAccentColor;

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
                            _responseBox.SelectionColor = BoldAccentColor;

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
                    _responseBox.SelectionColor = HeaderAccentColor;

                    headerCount++;
                    searchStart = lineEnd + 1;
                }

                // Reset selection to end
                _responseBox.SelectionStart = _responseBox.Text.Length;
                _responseBox.SelectionLength = 0;
                var defaultFont = new Font(_responseBox.Font.FontFamily, _responseBox.Font.Size, FontStyle.Regular);
                _responseBox.SelectionFont = defaultFont;
                _responseBox.SelectionColor = _responseBox.ForeColor;

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
            if (_aiService != null && _aiService.IsEnabled && _aiService.CurrentProvider != null)
                return $"{_aiService.CurrentProvider.ProviderName} ready — real analysis";

            return "Sample Mode (AI not configured) — Click Settings to enable real analysis";
        }

        /// <summary>True when a real, configured AI provider is available for live requests.</summary>
        private bool IsRealAiAvailable => _aiService != null && _aiService.IsEnabled && _aiService.CurrentProvider != null;

        /// <summary>
        /// Generates a deterministic, rule-based "sample" response for the given analysis
        /// type using only locally-available aggregate statistics — no network call. Used
        /// whenever the real AI provider is disabled, unconfigured, or fails at runtime.
        /// </summary>
        private async Task<string> GetCannedAnalysisAsync(AnalysisType analysisType)
        {
            var stats = _getStats?.Invoke() ?? new Models.AggregateStats();
            var perfStats = _getPerfStats?.Invoke() ?? new List<Services.ApiPerfStats>();

            switch (analysisType)
            {
                case AnalysisType.Summarize:
                    return await _canned.SummarizeAsync(stats, perfStats);
                case AnalysisType.RootCause:
                    return await _canned.SuggestRootCauseAsync(stats, perfStats, stats.ErrorCount, stats.WarningCount);
                case AnalysisType.FindErrors:
                    return await _canned.NlSearchAsync("find errors", stats, perfStats);
                case AnalysisType.FindWarnings:
                    return await _canned.NlSearchAsync("find warnings", stats, perfStats);
                case AnalysisType.Performance:
                    return await _canned.AnalyzePerformanceAsync(perfStats);
                default:
                    return await _canned.NlSearchAsync(analysisType.ToString(), stats, perfStats);
            }
        }

        /// <summary>
        /// Legacy gate kept for compatibility — no longer blocks usage. Real vs. sample
        /// routing is now decided per-call via <see cref="IsRealAiAvailable"/> so users
        /// always get a response, clearly labeled when it's a sample.
        /// </summary>
        private bool CheckAIAvailable() => true;

        // ?? Theme Support ?????????????????????????????????????????????????????
        // Called from MainForm whenever the app's Light/Dark theme changes (same
        // hook HeatmapPanel/FlameGraphPanel/TimelinePanel use) -- re-applies the
        // exact colors BuildUI assigned at construction so the AI tab stays in
        // sync with a live theme switch instead of freezing at whatever theme was
        // active when the app started.
        public void UpdateTheme()
        {
            BackColor = ThemeManager.BackgroundColor;

            if (_apiModeLabel != null)
            {
                _apiModeLabel.ForeColor = MutedForeground;
                _apiModeLabel.BackColor = ThemeManager.ControlBackgroundColor;
            }
            if (_titlePanel != null) _titlePanel.BackColor = ThemeManager.ControlBackgroundColor;
            if (_statusLabel != null) _statusLabel.ForeColor = ThemeManager.ForegroundColor;
            if (_buttonPanel != null) _buttonPanel.BackColor = ThemeManager.ControlBackgroundColor;
            if (_responseBox != null)
            {
                _responseBox.BackColor = ThemeManager.BackgroundColor;
                _responseBox.ForeColor = ThemeManager.ForegroundColor;
            }
            if (_tokenLabel != null)
            {
                _tokenLabel.ForeColor = MutedForeground;
                _tokenLabel.BackColor = ThemeManager.ControlBackgroundColor;
            }
            if (_inputPanel != null) _inputPanel.BackColor = ThemeManager.ControlBackgroundColor;
            if (_chatInputBox != null)
            {
                _chatInputBox.BackColor = ThemeManager.InputBackgroundColor;
                _chatInputBox.ForeColor = ThemeManager.ControlForegroundColor;
            }
            if (_promptChipsPanel != null) _promptChipsPanel.BackColor = ThemeManager.ControlBackgroundColor;

            // The 6 analysis buttons, Settings, Send/Copy/Clear, and the 3 example
            // chips all share one StyleButton() rule -- restyle whichever ones
            // exist by walking their containers rather than listing every field
            // (the chips in particular are anonymous, created only as Controls).
            RestyleButtonsIn(_buttonPanel);
            RestyleButtonsIn(_inputPanel);
            RestyleButtonsIn(_promptChipsPanel);
            if (_settingsBtn != null) StyleButton(_settingsBtn);
        }

        private static void RestyleButtonsIn(Control container)
        {
            if (container == null) return;
            foreach (Control child in container.Controls)
            {
                if (child is Button btn)
                    StyleButton(btn);
            }
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
