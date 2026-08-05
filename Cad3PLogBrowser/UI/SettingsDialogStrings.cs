using System;

namespace Cad3PLogBrowser.UI
{
    /// <summary>
    /// String constants for the Settings Dialog.
    /// Centralizes all UI strings for easier maintenance and potential localization.
    /// </summary>
    public static class SettingsDialogStrings
    {
        // ?? Dialog Title ??????????????????????????????????????????????????????
        public const string DialogTitle = "Settings";

        // ?? Button Labels ?????????????????????????????????????????????????????
        public const string ButtonOk = "&OK";
        public const string ButtonCancel = "&Cancel";
        public const string ButtonResetDefaults = "Reset to Defaults";
        public const string ButtonBrowse = "Browse…";
        public const string ButtonPreviewFont = "Preview Font...";
        public const string ButtonShow = "Show";
        public const string ButtonHide = "Hide";
        public const string ButtonTestConnection = "Test Connection";
        public const string ButtonTestingConnection = "Testing...";
        public const string ButtonCheckNow = "Check Now";
        public const string ButtonClearSkippedVersion = "Clear Skipped Version";
        public const string ButtonDefaultPreset = "Default (Recommended for Logs)";
        public const string ButtonStrictPreset = "Strict (Consider Everything)";

        // ?? Tab Names ?????????????????????????????????????????????????????????
        public const string TabAppearance = "Appearance";
        public const string TabTabsAndLayout = "Tabs & Layout";
        public const string TabLogFont = "Log Font";
        public const string TabFilesAndBehavior = "Files & Behavior";
        public const string TabPerformance = "Performance";
        public const string TabAIAndIntegration = "AI & Integration";
        public const string TabComparison = "Comparison";
        public const string TabUpdates = "Updates";

        // ?? Appearance Tab ????????????????????????????????????????????????????
        public const string LabelTheme = "Theme:";
        public const string LabelToolbarIconSize = "Toolbar icon size:";
        public const string LabelToolbarVisible = "Toolbar visible:";
        public const string LabelHighlightColor = "Highlight colour:";
        public const string CheckboxShowToolbar = "Show toolbar";

        public const string ThemeLight = "Light";
        public const string ThemeDark = "Dark";

        public const string IconSizeSmall = "Small";
        public const string IconSizeMedium = "Medium";
        public const string IconSizeLarge = "Large";

        // ?? Tabs & Layout Tab ?????????????????????????????????????????????????
        public const string GroupVisibleTabs = "Visible Tabs";
        public const string CheckboxLogView = "Log View";
        public const string CheckboxPerformance = "Performance";
        public const string CheckboxLogDetails = "Log Details";
        public const string CheckboxCallGraph = "Call Graph";
        public const string CheckboxFlameGraph = "Flame Graph";
        public const string CheckboxTimeline = "Timeline";
        public const string CheckboxAIAssistant = "AI Assistant";

        public const string LabelStartupTab = "Start-up tab:";
        public const string LabelDefaultTreeView = "Default tree view:";

        public const string ViewLog = "Log";
        public const string ViewRaw = "Raw";
        public const string ViewPerformance = "Performance";
        public const string ViewLogDetails = "Log Details";
        public const string ViewCallGraph = "Call Graph";
        public const string ViewFlameGraph = "Flame Graph";
        public const string ViewTimeline = "Timeline";
        public const string ViewAIAssistant = "AI Assistant";

        public const string TreeViewCall = "Call Tree";
        public const string TreeViewAPI = "API Tree";

        // ?? Log Font Tab ??????????????????????????????????????????????????????
        public const string LabelFontFamily = "Font family:";
        public const string LabelFontSize = "Font size (pt):";
        public const string CheckboxBold = "Bold";
        public const string CheckboxItalic = "Italic";

        public const string FontConsolas = "Consolas";
        public const string FontCourierNew = "Courier New";
        public const string FontLucidaConsole = "Lucida Console";
        public const string FontDejaVuSansMono = "DejaVu Sans Mono";
        public const string FontSourceCodePro = "Source Code Pro";

        // ?? Files & Behavior Tab ??????????????????????????????????????????????
        public const string LabelDefaultOpenFolder = "Default open folder:";
        public const string LabelMaxRecentFiles = "Max recent files:";
        public const string LabelSnippetFileSuffix = "Snippet file suffix:";
        public const string DefaultSnippetSuffix = "_snippet";

        // ?? Performance Tab ???????????????????????????????????????????????????
        public const string LabelFastCallThreshold = "Fast call threshold:";
        public const string LabelSlowCallThreshold = "Slow call threshold:";
        public const string LabelSkipListViewIfFileGreater = "Skip list view if file >";
        public const string LabelLazyLoadThreshold = "Lazy-load tree above:";
        public const string CheckboxAutoFilterPerformance = "Auto-filter Performance tab when a Call Tree node is selected";
        public const string CheckboxRestoreSession = "Restore last session on startup";
        public const string CheckboxWatchFileChanges = "Watch for file changes (auto-reload)";
        public const string LabelAutoReloadDelay = "Auto-reload after (seconds):";
        public const string HintAutoReloadDelay = "0 = ask before reloading";

        public const string HintFastCallMs = "ms  (below = green)";
        public const string HintSlowCallMs = "ms  (above = red, else amber)";
        public const string HintMaxFileMb = "MB   (use Raw tab for very large files)";
        public const string HintLazyLoadThreshold = "nodes  (loads children on demand)";
        public const string HintAutoFilterOff = "When OFF, use the Call Tree right-click menu to filter manually.";

        // ?? AI & Integration Tab ??????????????????????????????????????????????
        public const string GroupAIProvider = "AI Provider";
        public const string GroupModelConfiguration = "Model Configuration";
        public const string GroupPrivacyAndConversation = "Privacy & Conversation";
        public const string GroupSourceIntegration = "Source Integration";
        public const string GroupLegacyIntegration = "Legacy Integration (Deprecated)";

        public const string CheckboxEnableAI = "Enable AI Features";
        public const string LabelProvider = "Provider:";
        public const string LabelAPIKey = "API Key:";
        public const string LabelServerURL = "Server URL:";
        public const string LabelModel = "Model:";
        public const string LabelTemperature = "Temperature:";
        public const string LabelMaxTokens = "Max Tokens:";
        public const string LabelMaxMessages = "Max messages:";
        public const string LabelGrokURL = "Grok URL:";
        public const string LabelClaudeKey = "Claude Key:";

        public const string CheckboxEnableStreaming = "Enable streaming";
        public const string CheckboxRedactSensitiveData = "Redact sensitive data (emails, IPs, paths)";
        public const string CheckboxRememberConversation = "Remember conversation history";
        public const string CheckboxEnableLegacyClaude = "Enable legacy Claude integration";

        public const string HintTemperature = "Lower = focused, Higher = creative";
        public const string HintLegacyWarning = "WARNING: Use 'Anthropic Claude' provider above instead";

        public const string ProviderMock = "Mock (Testing)";
        public const string ProviderAnthropic = "Anthropic Claude";
        public const string ProviderGitHubCopilot = "GitHub Copilot";
        public const string ProviderOllama = "Ollama (Self-Hosted)";
        public const string ProviderOpenAI = "OpenAI (Coming Soon)";
        public const string ProviderAzureOpenAI = "Azure OpenAI (Coming Soon)";
        public const string ProviderGoogleGemini = "Google Gemini (Coming Soon)";

        public const string DefaultOllamaServerUrl = "http://localhost:11434";
        public const string ModelPlaceholder = "(Select provider first)";
        public const string ModelMock = "mock-model-1.0";
        public const string ModelComingSoon = "(Coming soon)";

        public const string ModelClaude35Sonnet = "claude-3-5-sonnet-20241022";
        public const string ModelClaude3OpusLatest = "claude-3-opus-latest";
        public const string ModelClaude3HaikuLatest = "claude-3-haiku-latest";

        public const string ModelGPT4 = "gpt-4";
        public const string ModelGPT4Turbo = "gpt-4-turbo";
        public const string ModelGPT35Turbo = "gpt-3.5-turbo";

        public const string ModelLlama3 = "llama3";
        public const string ModelCodeLlama = "codellama";
        public const string ModelMistral = "mistral";
        public const string ModelPhi3 = "phi3";

        public const string StatusAIDisabled = "? AI is disabled or not configured";
        public const string StatusTestingConnection = "Testing connection...";
        public const string StatusConnectionSuccessful = "? Connection successful!";
        public const string StatusConnectionFailedFormat = "? Connection failed: {0}";
        public const string StatusErrorFormat = "? Error: {0}";

        // ?? Comparison Tab ????????????????????????????????????????????????????
        public const string GroupComparisonOptions = "Comparison Options";
        public const string GroupPresets = "Presets";

        public const string CheckboxIgnoreCase = "Ignore case (case-insensitive comparison)";
        public const string CheckboxIgnoreWhitespace = "Ignore whitespace differences (normalize spaces)";
        public const string CheckboxIgnoreTimestamps = "Ignore timestamps and durations (essential for log file comparison)";
        public const string CheckboxIgnoreGuids = "Ignore GUIDs (useful for session IDs, transaction IDs)";
        public const string CheckboxTrimText = "Trim leading and trailing whitespace before comparing";
        public const string CheckboxUseRegex = "Use custom regex pattern to ignore text";
        public const string LabelRegexPattern = "Regex Pattern:";

        public const string HintComparisonSettings = "These settings control how log files are compared when using the Difference functionality.\nThe default preset is recommended for comparing log files with timestamps and session IDs.";

        // ?? Updates Tab ???????????????????????????????????????????????????????
        public const string CheckboxCheckOnStartup = "Check for updates automatically on startup";
        public const string LabelCheckInterval = "Check interval (days):";
        public const string LabelManifestURL = "Manifest URL:";
        public const string HintCheckInterval = "  0 = every launch";
        public const string HintManifestURL = "Leave as default unless you host your own update server.";
        public const string LabelLastCheckedFormat = "Last checked:  {0:yyyy-MM-dd HH:mm} UTC";
        public const string LabelLastCheckedNever = "Last checked:  never";
        public const string LabelSkippedVersionFormat = "Skipped version:  {0}";
        public const string LabelSkippedVersionNone = "Skipped version:  (none)";

        // ?? Dialog Messages ???????????????????????????????????????????????????
        public const string MessageResetToDefaults = "Reset all settings to their default values?";
        public const string MessageResetToDefaultsTitle = "Reset to Defaults";

        public const string MessageBrowseFolderDescription = "Select default folder for opening log files";

        public const string MessageFontPreviewTitle = "Font Preview - {0}";
        public const string MessageFontPreviewText = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstuvwxyz\n0123456789\n{}[]()<>+-*/=";
        public const string MessageCannotCreateFontFormat = "Cannot create font: {0}";
        public const string MessageCannotCreateFontTitle = "Error";

        // ?? Color Names ???????????????????????????????????????????????????????
        public const string ColorYellow = "Yellow";
        public const string ColorCyan = "Cyan";
        public const string ColorLimeGreen = "LimeGreen";
        public const string ColorOrange = "Orange";
        public const string ColorHotPink = "HotPink";
        public const string ColorLightBlue = "LightBlue";
        public const string ColorPlum = "Plum";
        public const string ColorGold = "Gold";
    }
}
