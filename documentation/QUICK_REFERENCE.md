# AI Features - Quick Reference Guide

## ?? For End Users

### Setup (5 Minutes)

**1. Install Ollama** (Recommended - Free!)
```bash
# Download from: https://ollama.ai/download
# Then pull a model:
ollama pull llama3
```

**2. Configure CAD3PLogBrowser**
```
Settings (Ctrl+,) ? AI & Integration tab

? Enable AI Features
Provider: [Ollama (Self-Hosted) ?]
Server URL: [http://localhost:11434]
Model: [llama3 ?]

Click "Test AI Connection"
Click "OK" to save
```

### Usage (Quick Actions)

| Action | Steps |
|--------|-------|
| **Ask a question** | Open AI Assistant tab ? Type question ? Enter |
| **Explain error** | Select error line ? Right-click ? "Analyze with AI" |
| **Summarize log** | AI tab ? "Summarize this log" |
| **Find similar issues** | Select line ? AI tab ? "Find similar issues" |
| **Get fix suggestions** | AI tab ? "How do I fix [error]?" |

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+,` | Open Settings |
| `Ctrl+Alt+A` | Focus AI Assistant tab |
| `Ctrl+E` | Explain selected error |

### Providers Comparison

| Provider | Cost | Privacy | Setup |
|----------|------|---------|-------|
| **Ollama** | ? Free | ? Private | 10 min |
| Anthropic | ?? ~$0.01/req | ?? Cloud | 5 min |
| GitHub Copilot | ?? $19-39/mo | ?? Cloud | 5 min |
| Mock | ? Free | ? Private | 0 min (testing) |

### Common Questions

**Q: How do I ask a good question?**
```
? Bad: "Fix it"
? Good: "Why is there a NullReferenceException at line 42 when loading customers?"
```

**Q: How much context should I provide?**
- Errors: 5-10 lines around the error
- Performance: Full operation sequence
- Crashes: Last 20-30 lines

**Q: Is my data safe?**
- Ollama: Yes, 100% local
- Cloud providers: Data sent to their servers (but not stored)

**Q: How do I change providers?**
```
Settings ? AI & Integration ? Change Provider dropdown ? Save
```

---

## ?? For Developers

### Quick Integration

**1. Initialize AI Service**
```csharp
using Cad3PLogBrowser.AI.Services;
using Cad3PLogBrowser.AI.Security;

// In your MainForm or startup
var aiSettings = AISettingsService.Load();
var _aiService = new AIService(aiSettings);
```

**2. Simple AI Request**
```csharp
var request = new AIRequest
{
    SystemPrompt = "You are a log analyzer.",
    Prompt = "Analyze this: " + logContent,
    MaxTokens = 2000
};

var response = await _aiService.CurrentProvider.SendRequestAsync(request);

if (response.Success)
{
    MessageBox.Show(response.Content);
}
```

**3. Streaming Request**
```csharp
await _aiService.CurrentProvider.StreamRequestAsync(
    request,
    onChunkReceived: chunk => outputBox.AppendText(chunk),
    onComplete: resp => statusLabel.Text = "Done!",
    onError: err => MessageBox.Show(err.Message)
);
```

### Key Classes

| Class | Purpose | Usage |
|-------|---------|-------|
| `AIService` | Main coordinator | `new AIService(settings)` |
| `IAIProvider` | Provider interface | `_aiService.CurrentProvider` |
| `AIRequest` | Request data | `new AIRequest { Prompt = "..." }` |
| `AIResponse` | Response data | `if (response.Success) ...` |
| `IAIConversation` | Multi-turn chat | `provider.CreateConversation()` |

### Common Patterns

**Check if AI is available:**
```csharp
if (_aiService?.IsEnabled == true)
{
    // Use AI
}
else
{
    MessageBox.Show("AI not configured");
}
```

**Handle errors robustly:**
```csharp
try
{
    var response = await _aiService.CurrentProvider.SendRequestAsync(request);
    if (response.Success)
        ShowResult(response.Content);
    else
        ShowError(response.ErrorMessage);
}
catch (Exception ex)
{
    LogError(ex);
    ShowError("AI request failed");
}
```

**Test connection:**
```csharp
var (success, message) = await _aiService.TestConnectionAsync();
if (!success)
    MessageBox.Show($"Connection failed: {message}");
```

**Use conversations:**
```csharp
_aiService.StartConversation("You are a log expert.");

var r1 = await _aiService.SendConversationMessageAsync("What errors?");
var r2 = await _aiService.SendConversationMessageAsync("How to fix?");

_aiService.EndConversation();
```

### Provider-Specific Code

**Ollama:**
```csharp
var provider = new OllamaProvider(
    baseUrl: "http://localhost:11434",
    model: "llama3"
);
```

**Anthropic:**
```csharp
var provider = new AnthropicProvider(
    apiKey: "sk-ant-...",
    model: "claude-3-5-sonnet-20241022"
);
```

**GitHub Copilot:**
```csharp
var provider = new GitHubCopilotProvider(
    apiToken: "ghp_...",
    model: "gpt-4",
    apiEndpoint: "https://api.github.com/copilot/chat/completions"
);
```

**Mock (Testing):**
```csharp
var provider = new MockProvider();
```

### File Structure

```
AI/
??? Abstractions/        # Interfaces
??? Providers/           # Provider implementations
?   ??? Ollama/         # OllamaProvider
?   ??? Anthropic/      # AnthropicProvider
?   ??? GitHub/         # GitHubCopilotProvider
?   ??? Mock/           # MockProvider
??? Services/            # AIService, etc.
??? Models/              # Data classes
??? Security/            # Credentials, redaction
??? Prompts/             # Prompt templates

Your Code/
??? MainForm.cs         # Initialize AIService
??? SettingsForm.cs     # AI settings UI
??? Features/           # Your AI-powered features
```

### Testing

**Unit test with Mock:**
```csharp
[TestMethod]
public async Task TestAIFeature()
{
    var settings = new AISettings 
    { 
        EnableAI = true,
        SelectedProvider = AIProviderType.Mock 
    };
    var aiService = new AIService(settings);

    var response = await aiService.CurrentProvider.SendRequestAsync(
        new AIRequest { Prompt = "test" }
    );

    Assert.IsTrue(response.Success);
}
```

**Integration test with Ollama:**
```csharp
[TestMethod]
[TestCategory("Integration")]
public async Task TestOllama()
{
    var provider = new OllamaProvider("http://localhost:11434", "llama3");
    var result = await provider.TestConnectionAsync();
    Assert.IsNull(result); // null = success
}
```

### Best Practices Checklist

- [ ] Always check `_aiService.IsEnabled` before using
- [ ] Wrap AI calls in try-catch
- [ ] Show loading indicators
- [ ] Support cancellation (CancellationToken)
- [ ] Respect token limits
- [ ] Cache results when appropriate
- [ ] Test with Mock provider first
- [ ] Provide user-friendly error messages
- [ ] Document your AI features
- [ ] Log errors for debugging

### Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| "AI not enabled" | Check Settings ? AI & Integration ? Enable AI |
| "Cannot connect to Ollama" | Verify Ollama is running: `ollama list` |
| "Model not found" | Download model: `ollama pull llama3` |
| "API key invalid" | Regenerate API key from provider website |
| "Request too large" | Reduce log size or increase token limit |
| "Timeout" | Increase timeout or use smaller model |

### Example Features to Add

1. **"Explain Error" button**
   - Select error line
   - Send to AI with context
   - Show explanation in dialog

2. **"Find Similar Issues"**
   - Get pattern from selected line
   - Search log with AI
   - Highlight matches

3. **"Smart Search"**
   - Natural language search: "Find database errors"
   - AI translates to log patterns
   - Show matching lines

4. **"Auto-Summarize on Load"**
   - When log opens
   - AI generates quick summary
   - Show in panel

5. **"Compare Logs"**
   - Open two logs
   - AI identifies differences
   - Highlight key changes

### Code Snippets Library

**Add menu item:**
```csharp
var aiMenuItem = new ToolStripMenuItem("Ask AI");
aiMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.A;
aiMenuItem.Click += async (s, e) => await ShowAIDialog();
contextMenu.Items.Add(aiMenuItem);
```

**Show loading overlay:**
```csharp
var loading = new Form 
{ 
    Text = "AI Analyzing...", 
    Size = new Size(300, 100),
    StartPosition = FormStartPosition.CenterParent,
    FormBorderStyle = FormBorderStyle.FixedDialog
};
loading.Show();
try { await DoAI(); } 
finally { loading.Close(); }
```

**Estimate tokens:**
```csharp
int tokens = _aiService.CurrentProvider.EstimateTokenCount(text);
if (tokens > maxTokens)
{
    MessageBox.Show($"Text too large: {tokens} tokens (max: {maxTokens})");
    return;
}
```

---

## ?? Documentation Links

### End User Docs:
- **Complete Guide**: `AI/END_USER_GUIDE.md`
- **Settings Guide**: `AI/SETTINGS_UI_GUIDE.md`
- **Ollama Setup**: `AI/Providers/Ollama/SETUP_GUIDE.md`

### Developer Docs:
- **Integration Guide**: `AI/DEVELOPER_INTEGRATION_GUIDE.md`
- **Architecture**: `AI/SETTINGS_INTEGRATION_SUMMARY.md`
- **Code Examples**: `AI/Examples/OllamaUsageExample.cs`
- **Provider Details**: `AI/Providers/*/README.md`

### Quick Links:
- Ollama: https://ollama.ai
- Anthropic: https://console.anthropic.com
- GitHub Copilot: https://github.com/features/copilot

---

## ?? Quick Start Summary

### For End Users:
1. Install Ollama ? Pull llama3
2. Settings ? AI & Integration ? Configure
3. Test Connection ? Save
4. Open AI tab ? Start asking questions!

### For Developers:
1. Initialize `AIService` in your code
2. Check `IsEnabled` before using
3. Send requests with `SendRequestAsync`
4. Handle responses and errors
5. Add AI features to your UI

**That's it! You're ready to use AI in CAD3PLogBrowser!** ??
