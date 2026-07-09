# Quick Start Guide - AI Framework Integration

## 5-Minute Integration Guide

This guide helps you integrate the AI framework into CAD3PLogBrowser quickly.

---

## Step 1: Verify Files (1 minute)

Check that all AI framework files are in your project:

```
Cad3PLogBrowser/
??? AI/
    ??? Abstractions/      ? 8 files
    ??? Context/           ? 3 files
    ??? Models/            ? 3 files
    ??? Providers/         ? 3 files
    ??? Prompts/           ? 2 files
    ??? Security/          ? 3 files
    ??? Services/          ? 3 files
```

**Total: 25 source files + 3 documentation files**

---

## Step 2: Compile and Test (2 minutes)

### 2.1 Build the project

```bash
# In Visual Studio
Build > Build Solution
# or
dotnet build
```

### 2.2 Check for errors

All files should compile without errors. If you see any errors:
- Ensure `Newtonsoft.Json` NuGet package is installed
- Check target framework is .NET Framework 4.8
- Verify all files are included in project

---

## Step 3: Test with Mock Provider (2 minutes)

### 3.1 Create a test method

```csharp
private async void TestAI()
{
    // Create settings with Mock provider (no API key needed)
    var settings = new AISettings
    {
        EnableAI = true,
        SelectedProvider = AIProviderType.Mock
    };

    // Initialize AI service
    var aiService = new AIService(settings);

    // Create dummy context
    var stats = new AggregateStats
    {
        TotalLines = 1000,
        ErrorCount = 5,
        WarningCount = 12,
        TotalApiCalls = 250
    };

    var contextProviders = new List<IContextProvider>
    {
        new CurrentLogContextProvider(stats, "test.log")
    };

    // Test analysis
    var result = await aiService.AnalyzeAsync(
        AnalysisType.Summarize,
        contextProviders);

    // Show result
    MessageBox.Show(
        result.Success 
            ? $"Success!\n\n{result.Content}" 
            : $"Error: {result.ErrorMessage}",
        "AI Test Result");
}
```

### 3.2 Run the test

- Add a button to your form: "Test AI"
- Wire it to `TestAI()` method
- Click the button
- You should see a mock analysis result

? **Success**: Mock provider works!

---

## Step 4: Configure Real AI Provider

### Option A: Anthropic Claude (Recommended)

1. **Get API Key**
   - Go to https://console.anthropic.com
   - Sign up / Log in
   - Navigate to "API Keys"
   - Click "Create Key"
   - Copy the key (starts with `sk-ant-api03-...`)

2. **Configure in Code**

```csharp
var settings = new AISettings
{
    EnableAI = true,
    SelectedProvider = AIProviderType.Anthropic,
    AnthropicApiKey = "sk-ant-api03-...", // Your key
    AnthropicModel = "claude-3-5-sonnet-20241022"
};

// Save securely
AISettingsService.Save(settings);
```

3. **Test Connection**

```csharp
var aiService = new AIService(settings);
var (success, message) = await aiService.TestConnectionAsync();

if (success)
    MessageBox.Show("Connected successfully!");
else
    MessageBox.Show($"Connection failed: {message}");
```

### Option B: Azure OpenAI (Enterprise) - Coming Soon

Will be available in Phase 2.

---

## Step 5: Basic Usage Examples

### Example 1: Summarize Current Log

```csharp
private async void OnSummarizeLog(object sender, EventArgs e)
{
    if (_aiService == null || !_aiService.IsEnabled)
    {
        MessageBox.Show("Please configure AI in Settings first.");
        return;
    }

    // Create context from current log
    var contextProviders = new List<IContextProvider>
    {
        new CurrentLogContextProvider(_aggregateStats, _currentFilePath)
    };

    // Show progress
    statusLabel.Text = "Analyzing log...";
    this.Cursor = Cursors.WaitCursor;

    try
    {
        // Analyze
        var result = await _aiService.AnalyzeAsync(
            AnalysisType.Summarize,
            contextProviders,
            cancellationToken: CancellationToken.None);

        if (result.Success)
        {
            // Show result in a message box or text box
            MessageBox.Show(result.Content, "Log Summary");

            // Show metrics
            statusLabel.Text = 
                $"Analysis complete. Tokens: {result.TokensUsed}, " +
                $"Time: {result.ElapsedTime.TotalSeconds:F1}s";
        }
        else
        {
            MessageBox.Show($"Analysis failed: {result.ErrorMessage}", "Error");
        }
    }
    finally
    {
        this.Cursor = Cursors.Default;
    }
}
```

### Example 2: Streaming Analysis (Better UX)

```csharp
private async void OnAnalyzeStreaming(object sender, EventArgs e)
{
    var contextProviders = new List<IContextProvider>
    {
        new CurrentLogContextProvider(_aggregateStats, _currentFilePath)
    };

    // Clear previous result
    aiResponseTextBox.Clear();
    statusLabel.Text = "Analyzing...";

    await _aiService.AnalyzeStreamingAsync(
        AnalysisType.RootCause,
        contextProviders,

        // Chunk received - update UI in real-time
        onChunkReceived: chunk => {
            aiResponseTextBox.AppendText(chunk);
            aiResponseTextBox.ScrollToCaret();
        },

        // Complete
        onComplete: result => {
            statusLabel.Text = $"Complete ({result.TokensUsed} tokens)";
        },

        // Error
        onError: ex => {
            MessageBox.Show($"Error: {ex.Message}");
            statusLabel.Text = "Analysis failed";
        });
}
```

### Example 3: Interactive Chat

```csharp
private IAIConversation _conversation;

private void OnStartChat(object sender, EventArgs e)
{
    // Start new conversation
    _aiService.StartConversation();

    // Add context on first message
    var contextProviders = new List<IContextProvider>
    {
        new CurrentLogContextProvider(_aggregateStats, _currentFilePath)
    };

    chatHistoryTextBox.Clear();
    chatInputTextBox.Enabled = true;
    chatInputTextBox.Focus();
}

private async void OnSendChatMessage(object sender, EventArgs e)
{
    string userMessage = chatInputTextBox.Text.Trim();
    if (string.IsNullOrEmpty(userMessage))
        return;

    // Add user message to UI
    chatHistoryTextBox.AppendText($"You: {userMessage}\n\n");
    chatInputTextBox.Clear();

    // Get AI response
    chatHistoryTextBox.AppendText("AI: ");

    await _aiService.SendConversationMessageStreamingAsync(
        userMessage,
        onChunkReceived: chunk => {
            chatHistoryTextBox.AppendText(chunk);
            chatHistoryTextBox.ScrollToCaret();
        },
        onComplete: response => {
            chatHistoryTextBox.AppendText("\n\n");
        },
        onError: ex => {
            MessageBox.Show($"Error: {ex.Message}");
        },
        contextProviders: null); // Context sent on first message only
}
```

### Example 4: Compare Two Logs

```csharp
private async void OnCompareLogs(object sender, EventArgs e)
{
    // Get summaries of both logs
    string oldLogSummary = GetLogSummary(_oldLogStats);
    string newLogSummary = GetLogSummary(_newLogStats);

    statusLabel.Text = "Comparing logs...";

    var result = await _aiService.CompareLogsAsync(
        oldLogSummary,
        newLogSummary,
        "What are the key differences between these logs?");

    if (result.Success)
    {
        // Show comparison in UI
        comparisonTextBox.Text = result.Content;
        statusLabel.Text = "Comparison complete";
    }
}

private string GetLogSummary(AggregateStats stats)
{
    return $@"
Total Lines: {stats.TotalLines}
Errors: {stats.ErrorCount}
Warnings: {stats.WarningCount}
API Calls: {stats.TotalApiCalls}
Duration: {stats.SessionDurationMs} ms
    ";
}
```

---

## Step 6: Replace Existing AI Implementation

### Current Code (AiLogService)

```csharp
var aiService = new AiLogService(
    apiKey: settings.ClaudeApiKey,
    useClaudeApi: settings.UseClaudeApi,
    model: settings.ClaudeModel
);

string summary = await aiService.SummarizeAsync(stats, perfStats);
```

### New Code (AIService Framework)

```csharp
// Initialize once (e.g., in constructor)
var aiSettings = AISettingsService.Load();
_aiService = new AIService(aiSettings);

// Use anywhere
var contextProviders = new List<IContextProvider>
{
    new CurrentLogContextProvider(stats, logFilePath)
};

var result = await _aiService.AnalyzeAsync(
    AnalysisType.Summarize,
    contextProviders);

string summary = result.Success ? result.Content : result.ErrorMessage;
```

### Migration Steps

1. **Update MainForm**:
   ```csharp
   private AIService _aiService;

   private void MainForm_Load(object sender, EventArgs e)
   {
       // Initialize AI
       var aiSettings = AISettingsService.Load();
       _aiService = new AIService(aiSettings);
   }
   ```

2. **Update AiAssistantPanel**:
   - Replace event handlers to use `_aiService`
   - Use `AnalysisType` enum instead of method names
   - Add streaming support for better UX

3. **Add Settings UI**:
   - Create `AISettingsDialog.cs` form
   - Add controls for provider selection, API key, model
   - Add "Test Connection" button
   - Wire to Settings menu

4. **Test thoroughly**:
   - Test with Mock provider first
   - Configure real provider
   - Test all analysis types
   - Test error cases

---

## Common Issues & Solutions

### Issue: "AIService is not enabled"

**Solution**: 
```csharp
var settings = AISettingsService.Load();
settings.EnableAI = true;
settings.SelectedProvider = AIProviderType.Anthropic;
settings.AnthropicApiKey = "your-key";
AISettingsService.Save(settings);
```

### Issue: "API key not configured"

**Solution**: Check that API key is stored securely:
```csharp
// Verify key is stored
string key = CredentialManager.RetrieveCredential("AI_Anthropic");
Console.WriteLine($"Key exists: {!string.IsNullOrEmpty(key)}");
```

### Issue: "Connection timeout"

**Solution**: Increase timeout:
```csharp
settings.TimeoutSeconds = 120; // Increase from 60
AISettingsService.Save(settings);
```

### Issue: "Token limit exceeded"

**Solution**: Enable auto-truncation:
```csharp
settings.AutoTruncateContext = true;
settings.MaxContextTokens = 50000; // Reduce if needed
AISettingsService.Save(settings);
```

---

## Next Steps

1. ? Integration complete
2. ?? Create AI Settings UI dialog
3. ?? Update existing AiAssistantPanel
4. ?? Add menu items and toolbar buttons
5. ?? User testing
6. ?? Documentation for end users

---

## Need Help?

- **Architecture**: See `AI/ARCHITECTURE.md`
- **Full Documentation**: See `AI/README.md`
- **Provider Comparison**: See `AI/GITHUB_COPILOT_ANALYSIS.md`
- **Code Examples**: See this guide and README

---

## Quick Reference

### Initialize AI Service

```csharp
var settings = AISettingsService.Load();
var aiService = new AIService(settings);
```

### Single Analysis

```csharp
var result = await aiService.AnalyzeAsync(
    AnalysisType.Summarize,
    contextProviders);
```

### Streaming Analysis

```csharp
await aiService.AnalyzeStreamingAsync(
    AnalysisType.RootCause,
    contextProviders,
    onChunkReceived: chunk => { /* update UI */ },
    onComplete: result => { /* done */ },
    onError: ex => { /* handle error */ });
```

### Start Conversation

```csharp
aiService.StartConversation();
var response = await aiService.SendConversationMessageAsync(
    "Your question",
    contextProviders);
```

### Test Connection

```csharp
var (success, message) = await aiService.TestConnectionAsync();
```

### Save Settings

```csharp
var settings = new AISettings { /* configure */ };
AISettingsService.Save(settings);
```

---

**You're ready to go! ??**

Start with the Mock provider for testing, then configure a real provider when ready.
