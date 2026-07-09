# CAD3PLogBrowser AI Features - Developer Integration Guide

## ?? Overview

This guide explains how to integrate and use the AI features in your CAD3PLogBrowser application code. Whether you're adding new AI-powered features or customizing existing ones, this document covers everything you need to know.

---

## ?? Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Quick Start Integration](#quick-start-integration)
3. [Adding AI to Your Features](#adding-ai-to-your-features)
4. [Working with AI Providers](#working-with-ai-providers)
5. [Advanced Usage](#advanced-usage)
6. [Creating Custom Analysis](#creating-custom-analysis)
7. [Error Handling](#error-handling)
8. [Best Practices](#best-practices)
9. [Testing](#testing)
10. [Examples](#examples)

---

## ??? Architecture Overview

### Component Structure

```
AI/
??? Abstractions/           # Interfaces and contracts
?   ??? IAIProvider.cs     # Provider interface
?   ??? IAIRequest.cs      # Request interface
?   ??? IAIResponse.cs     # Response interface
?   ??? IAIConversation.cs # Conversation interface
?   ??? IContextProvider.cs# Context provider interface
?
??? Providers/             # AI provider implementations
?   ??? Ollama/           # Self-hosted Ollama
?   ??? Anthropic/        # Claude API
?   ??? GitHub/           # GitHub Copilot
?   ??? Mock/             # Testing provider
?
??? Services/              # Core services
?   ??? AIService.cs      # Main AI service coordinator
?   ??? ConversationStorage.cs
?   ??? TokenEstimationService.cs
?
??? Models/                # Data models
?   ??? AISettings.cs     # Configuration
?   ??? AIProviderType.cs # Provider enum
?   ??? AnalysisResult.cs # Analysis results
?
??? Context/               # Context providers
?   ??? CurrentLogContextProvider.cs
?   ??? SelectedLinesContextProvider.cs
?
??? Security/              # Security features
?   ??? AISettingsService.cs
?   ??? CredentialManager.cs
?   ??? DataRedactor.cs
?
??? Prompts/               # Prompt engineering
    ??? SystemPrompts.cs
    ??? PromptBuilder.cs
```

### Key Interfaces

```csharp
// Core provider interface
public interface IAIProvider
{
    Task<IAIResponse> SendRequestAsync(IAIRequest request, CancellationToken ct);
    Task StreamRequestAsync(IAIRequest request, ...);
    IAIConversation CreateConversation();
    Task<string> TestConnectionAsync(CancellationToken ct);
}

// Request/Response
public interface IAIRequest
{
    string Prompt { get; set; }
    string SystemPrompt { get; set; }
    string Model { get; set; }
    double Temperature { get; set; }
    int MaxTokens { get; set; }
}

public interface IAIResponse
{
    bool Success { get; }
    string Content { get; }
    string ErrorMessage { get; }
    int? TotalTokens { get; }
    TimeSpan ElapsedTime { get; }
}
```

---

## ?? Quick Start Integration

### Step 1: Initialize AI Service

In your MainForm or startup code:

```csharp
using Cad3PLogBrowser.AI.Services;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Security;

public class MainForm : Form
{
    private AIService _aiService;

    private void InitializeAI()
    {
        // Load AI settings
        var aiSettings = AISettingsService.Load();

        // Create AI service
        _aiService = new AIService(aiSettings);

        // Check if AI is enabled and configured
        if (_aiService.IsEnabled)
        {
            Debug.WriteLine("AI is ready!");
        }
        else
        {
            Debug.WriteLine("AI is disabled or not configured");
        }
    }
}
```

### Step 2: Simple AI Request

```csharp
private async Task AnalyzeLogWithAI()
{
    // Get log content
    string logContent = GetSelectedLogLines();

    // Create request
    var request = new AIRequest
    {
        SystemPrompt = "You are an expert log analyzer.",
        Prompt = $"Analyze this log and identify errors:\n\n{logContent}",
        MaxTokens = 2000
    };

    // Send to AI
    var response = await _aiService.CurrentProvider.SendRequestAsync(request);

    // Handle response
    if (response.Success)
    {
        MessageBox.Show(response.Content, "AI Analysis");
    }
    else
    {
        MessageBox.Show(response.ErrorMessage, "Error");
    }
}
```

### Step 3: Add UI Integration

```csharp
private void AddAIMenuItem()
{
    var menuItem = new ToolStripMenuItem("Analyze with AI");
    menuItem.Click += async (s, e) => await AnalyzeLogWithAI();
    contextMenuStrip.Items.Add(menuItem);
}
```

---

## ?? Adding AI to Your Features

### Example 1: Add "Explain Error" Feature

```csharp
// In your log viewer context menu
private void AddExplainErrorMenuItem()
{
    var explainMenuItem = new ToolStripMenuItem("Explain Error");
    explainMenuItem.ShortcutKeys = Keys.Control | Keys.E;
    explainMenuItem.Click += async (s, e) => await ExplainSelectedError();

    logContextMenu.Items.Add(explainMenuItem);
}

private async Task ExplainSelectedError()
{
    // Get selected log line
    string errorLine = GetSelectedLogLine();

    if (string.IsNullOrEmpty(errorLine))
    {
        MessageBox.Show("Please select an error line.");
        return;
    }

    // Show loading indicator
    ShowLoadingOverlay("AI is analyzing the error...");

    try
    {
        // Create context providers
        var contextProviders = new List<IContextProvider>
        {
            new SelectedLinesContextProvider(new[] { errorLine })
        };

        // Analyze with AI
        var result = await _aiService.AnalyzeAsync(
            AnalysisType.ExplainError,
            contextProviders,
            userQuery: "Explain this error in simple terms and suggest how to fix it."
        );

        if (result.Success)
        {
            // Show result in a nice dialog
            ShowAIResultDialog("Error Explanation", result.Content);
        }
        else
        {
            MessageBox.Show(result.ErrorMessage, "AI Error");
        }
    }
    finally
    {
        HideLoadingOverlay();
    }
}
```

### Example 2: Add "Find Similar Issues" Feature

```csharp
private async Task FindSimilarIssues()
{
    string selectedLine = GetSelectedLogLine();

    var request = new AIRequest
    {
        SystemPrompt = "You are an expert at pattern matching in logs.",
        Prompt = $@"Given this log line:
{selectedLine}

Search through the following log and find similar issues:
{GetAllLogContent()}

List each similar issue with its line number.",
        MaxTokens = 3000
    };

    var response = await _aiService.CurrentProvider.SendRequestAsync(request);

    if (response.Success)
    {
        // Parse AI response and highlight matching lines
        HighlightMatchingLines(response.Content);
    }
}
```

### Example 3: Add Streaming Analysis with Progress

```csharp
private async Task AnalyzeWithStreaming()
{
    // Clear output
    analysisTextBox.Clear();
    analysisTextBox.Visible = true;

    // Create request
    var request = new AIRequest
    {
        SystemPrompt = "You are a log analysis expert.",
        Prompt = $"Analyze this log:\n\n{GetLogContent()}",
        MaxTokens = 4000,
        Stream = true
    };

    // Send streaming request
    await _aiService.CurrentProvider.StreamRequestAsync(
        request,
        onChunkReceived: chunk =>
        {
            // Update UI as text arrives (must be on UI thread)
            if (InvokeRequired)
            {
                Invoke(new Action(() => 
                {
                    analysisTextBox.AppendText(chunk);
                    analysisTextBox.ScrollToCaret();
                }));
            }
        },
        onComplete: response =>
        {
            Invoke(new Action(() =>
            {
                statusLabel.Text = $"Analysis complete! ({response.TotalTokens} tokens)";
            }));
        },
        onError: error =>
        {
            Invoke(new Action(() =>
            {
                MessageBox.Show($"Error: {error.Message}");
            }));
        }
    );
}
```

---

## ?? Working with AI Providers

### Checking Provider Status

```csharp
private void CheckAIStatus()
{
    if (_aiService == null || !_aiService.IsEnabled)
    {
        statusLabel.Text = "AI: Disabled";
        statusLabel.ForeColor = Color.Gray;
        return;
    }

    var provider = _aiService.CurrentProvider;
    statusLabel.Text = $"AI: {provider.ProviderName}";
    statusLabel.ForeColor = Color.Green;
}
```

### Testing Connection

```csharp
private async Task<bool> TestAIConnection()
{
    if (_aiService == null)
        return false;

    var (success, message) = await _aiService.TestConnectionAsync();

    if (success)
    {
        MessageBox.Show("AI connection successful!", "Success");
        return true;
    }
    else
    {
        MessageBox.Show($"AI connection failed:\n{message}", "Error");
        return false;
    }
}
```

### Handling Provider Changes

```csharp
private void OnAISettingsChanged()
{
    // Reload AI settings
    var newSettings = AISettingsService.Load();

    // Recreate AI service
    _aiService?.Dispose();
    _aiService = new AIService(newSettings);

    // Update UI
    UpdateAIStatusDisplay();

    // Notify AI panel to refresh
    _aiPanel?.RefreshAIService();
}
```

---

## ?? Advanced Usage

### Multi-Turn Conversations

```csharp
private async Task StartAIConversation()
{
    // Create a conversation
    _aiService.StartConversation(
        systemPrompt: "You are helping analyze CAD application logs."
    );

    // First question
    var response1 = await _aiService.SendConversationMessageAsync(
        "What are the main errors in this log?"
    );
    ShowInChat("Assistant", response1.Content);

    // Follow-up question (AI remembers context)
    var response2 = await _aiService.SendConversationMessageAsync(
        "How do I fix the first error?"
    );
    ShowInChat("Assistant", response2.Content);

    // Another follow-up
    var response3 = await _aiService.SendConversationMessageAsync(
        "Can you show me example code?"
    );
    ShowInChat("Assistant", response3.Content);

    // End conversation when done
    _aiService.EndConversation();
}
```

### Using Context Providers

Context providers help you provide relevant log data to the AI:

```csharp
// Example 1: Selected lines context
var selectedContext = new SelectedLinesContextProvider(
    GetSelectedLogLines()
);

// Example 2: Current log context
var currentLogContext = new CurrentLogContextProvider(
    logFileName: "app.log",
    logContent: GetCurrentLogContent(),
    selectedLineNumber: GetCurrentLineNumber()
);

// Example 3: Multiple contexts
var contexts = new List<IContextProvider>
{
    selectedContext,
    currentLogContext,
    // Add more as needed
};

// Use in analysis
var result = await _aiService.AnalyzeAsync(
    AnalysisType.General,
    contexts,
    userQuery: "Analyze these logs"
);
```

### Custom Analysis Types

```csharp
public enum CustomAnalysisType
{
    FindRootCause,
    CompareWithBaseline,
    PerformanceBottleneck,
    SecurityAudit
}

private async Task<AnalysisResult> PerformCustomAnalysis(
    CustomAnalysisType analysisType,
    string logContent)
{
    string systemPrompt = analysisType switch
    {
        CustomAnalysisType.FindRootCause => 
            "You are an expert at root cause analysis. Trace errors back to their origin.",
        CustomAnalysisType.PerformanceBottleneck => 
            "You are a performance optimization expert. Identify slow operations.",
        CustomAnalysisType.SecurityAudit => 
            "You are a security expert. Look for security issues and vulnerabilities.",
        _ => "You are a log analysis expert."
    };

    var request = new AIRequest
    {
        SystemPrompt = systemPrompt,
        Prompt = $"Analyze this log:\n\n{logContent}",
        Temperature = 0.5, // More focused for analysis
        MaxTokens = 3000
    };

    var response = await _aiService.CurrentProvider.SendRequestAsync(request);

    return response.Success
        ? AnalysisResult.CreateSuccess(response.Content, AnalysisType.General)
        : AnalysisResult.CreateError(response.ErrorMessage, AnalysisType.General);
}
```

### Batch Processing

```csharp
private async Task BatchAnalyzeMultipleLogs()
{
    var logFiles = Directory.GetFiles(logDirectory, "*.log");
    var results = new List<(string fileName, string analysis)>();

    progressBar.Maximum = logFiles.Length;
    progressBar.Value = 0;

    foreach (var logFile in logFiles)
    {
        try
        {
            var logContent = File.ReadAllText(logFile);

            var request = new AIRequest
            {
                SystemPrompt = "Provide a brief summary of this log.",
                Prompt = logContent,
                MaxTokens = 500
            };

            var response = await _aiService.CurrentProvider.SendRequestAsync(request);

            if (response.Success)
            {
                results.Add((Path.GetFileName(logFile), response.Content));
            }

            progressBar.Value++;
            Application.DoEvents(); // Keep UI responsive
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error analyzing {logFile}: {ex.Message}");
        }
    }

    // Show batch results
    ShowBatchResults(results);
}
```

---

## ??? Creating Custom Analysis

### Custom Prompt Templates

```csharp
public class CustomPromptTemplates
{
    public static string ErrorAnalysis(string errorType) => 
        $@"You are analyzing a {errorType} error in a CAD application.
Focus on:
1. What caused the error
2. Which component failed
3. How to fix it
4. How to prevent it

Be specific and provide code examples if relevant.";

    public static string PerformanceAnalysis() =>
        @"Analyze the performance data and identify:
1. Slowest operations (with timings)
2. Resource bottlenecks
3. Optimization opportunities
4. Expected vs actual performance

Provide specific recommendations with estimated impact.";

    public static string ComparisonAnalysis() =>
        @"Compare these two logs and identify:
1. Key differences
2. What changed between them
3. Potential causes for any issues
4. Which version performs better

Focus on actionable insights.";
}
```

### Usage:

```csharp
private async Task AnalyzePerformance()
{
    var request = new AIRequest
    {
        SystemPrompt = CustomPromptTemplates.PerformanceAnalysis(),
        Prompt = GetPerformanceLog(),
        Temperature = 0.3, // More focused
        MaxTokens = 2500
    };

    var response = await _aiService.CurrentProvider.SendRequestAsync(request);
    ShowAnalysisResults(response);
}
```

---

## ?? Error Handling

### Robust Error Handling Pattern

```csharp
private async Task<string> SafeAIRequest(string prompt)
{
    const int maxRetries = 3;
    int attempt = 0;

    while (attempt < maxRetries)
    {
        try
        {
            var request = new AIRequest
            {
                Prompt = prompt,
                MaxTokens = 2000
            };

            var response = await _aiService.CurrentProvider.SendRequestAsync(
                request,
                new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token
            );

            if (response.Success)
            {
                return response.Content;
            }
            else
            {
                Debug.WriteLine($"AI request failed: {response.ErrorMessage}");
                attempt++;

                if (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt); // Exponential backoff
                }
            }
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("AI request timed out");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"Network error: {ex.Message}");
            attempt++;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    throw new Exception($"AI request failed after {maxRetries} attempts");
}
```

### User-Friendly Error Messages

```csharp
private void HandleAIError(Exception ex)
{
    string userMessage = ex switch
    {
        TimeoutException => 
            "The AI request took too long. Try with a smaller log file or increase the timeout.",
        HttpRequestException => 
            "Cannot connect to AI service. Check your internet connection and try again.",
        UnauthorizedAccessException => 
            "Invalid API key. Please check your AI settings.",
        _ => 
            $"AI error: {ex.Message}\n\nPlease check your settings or try again later."
    };

    MessageBox.Show(userMessage, "AI Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

---

## ?? Best Practices

### 1. Always Check AI Availability

```csharp
private async Task UseAIFeature()
{
    if (_aiService == null || !_aiService.IsEnabled)
    {
        MessageBox.Show(
            "AI features are not enabled.\n\n" +
            "Go to Settings > AI & Integration to configure.",
            "AI Not Available"
        );
        return;
    }

    // Proceed with AI feature
    await DoAIOperation();
}
```

### 2. Provide User Feedback

```csharp
private async Task AnalyzeWithFeedback()
{
    // Show loading
    var loadingForm = new LoadingForm("AI is analyzing...");
    loadingForm.Show();

    try
    {
        var result = await _aiService.AnalyzeAsync(...);

        loadingForm.Close();

        if (result.Success)
        {
            // Show success
            ShowResults(result.Content);

            // Show stats
            statusLabel.Text = $"Analysis complete ({result.TokensUsed} tokens, {result.ElapsedTime.TotalSeconds:F1}s)";
        }
    }
    catch (Exception ex)
    {
        loadingForm.Close();
        HandleAIError(ex);
    }
}
```

### 3. Respect Token Limits

```csharp
private async Task AnalyzeLogRespectingLimits()
{
    string logContent = GetLogContent();

    // Estimate tokens
    int estimatedTokens = _aiService.CurrentProvider.EstimateTokenCount(logContent);
    int maxContext = _aiService.CurrentProvider.MaxContextTokens;

    if (estimatedTokens > maxContext * 0.8) // Use 80% of limit
    {
        var result = MessageBox.Show(
            $"This log is very large ({estimatedTokens:N0} tokens).\n" +
            "Would you like to analyze selected portions instead?",
            "Large Log",
            MessageBoxButtons.YesNo
        );

        if (result == DialogResult.Yes)
        {
            // Show selection dialog
            ShowLogSelectionDialog();
            return;
        }
        else
        {
            // Truncate
            logContent = TruncateToTokenLimit(logContent, maxContext);
        }
    }

    // Proceed with analysis
    await AnalyzeLog(logContent);
}
```

### 4. Cache Results

```csharp
private Dictionary<string, string> _aiCache = new Dictionary<string, string>();

private async Task<string> GetAIResponseWithCache(string prompt)
{
    // Check cache
    string cacheKey = GetHash(prompt);
    if (_aiCache.ContainsKey(cacheKey))
    {
        Debug.WriteLine("Using cached AI response");
        return _aiCache[cacheKey];
    }

    // Get fresh response
    var response = await GetAIResponse(prompt);

    // Cache it
    _aiCache[cacheKey] = response;

    // Limit cache size
    if (_aiCache.Count > 100)
    {
        _aiCache.Remove(_aiCache.Keys.First());
    }

    return response;
}
```

### 5. Implement Cancellation

```csharp
private CancellationTokenSource _cancellationTokenSource;

private async Task CancellableAIRequest()
{
    _cancellationTokenSource = new CancellationTokenSource();
    cancelButton.Enabled = true;

    try
    {
        var response = await _aiService.CurrentProvider.SendRequestAsync(
            request,
            _cancellationTokenSource.Token
        );

        // Handle response
    }
    catch (OperationCanceledException)
    {
        MessageBox.Show("AI request was cancelled.");
    }
    finally
    {
        cancelButton.Enabled = false;
    }
}

private void CancelButton_Click(object sender, EventArgs e)
{
    _cancellationTokenSource?.Cancel();
}
```

---

## ?? Testing

### Unit Testing AI Features

```csharp
[TestClass]
public class AIFeatureTests
{
    [TestMethod]
    public async Task TestMockProvider()
    {
        // Arrange
        var settings = new AISettings
        {
            EnableAI = true,
            SelectedProvider = AIProviderType.Mock
        };
        var aiService = new AIService(settings);

        // Act
        var response = await aiService.CurrentProvider.SendRequestAsync(
            new AIRequest { Prompt = "Test" }
        );

        // Assert
        Assert.IsTrue(response.Success);
        Assert.IsFalse(string.IsNullOrEmpty(response.Content));
    }

    [TestMethod]
    public async Task TestConnectionFailure()
    {
        // Arrange
        var settings = new AISettings
        {
            EnableAI = true,
            SelectedProvider = AIProviderType.Ollama,
            OllamaServerUrl = "http://invalid:9999"
        };
        var aiService = new AIService(settings);

        // Act
        var (success, message) = await aiService.TestConnectionAsync();

        // Assert
        Assert.IsFalse(success);
        Assert.IsNotNull(message);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class AIIntegrationTests
{
    [TestMethod]
    [TestCategory("Integration")]
    public async Task TestOllamaIntegration()
    {
        // Requires Ollama running locally
        var provider = new OllamaProvider("http://localhost:11434", "llama3");

        var testResult = await provider.TestConnectionAsync();
        Assert.IsNull(testResult, "Ollama should be available for integration tests");

        var response = await provider.SendRequestAsync(new AIRequest
        {
            Prompt = "Say hello",
            MaxTokens = 50
        });

        Assert.IsTrue(response.Success);
        Assert.IsFalse(string.IsNullOrEmpty(response.Content));
    }
}
```

---

## ?? Examples

### Example 1: Complete "Ask AI" Feature

```csharp
public class AskAIDialog : Form
{
    private TextBox questionBox;
    private RichTextBox answerBox;
    private Button askButton;
    private AIService _aiService;

    public AskAIDialog(AIService aiService)
    {
        _aiService = aiService;
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = "Ask AI";
        Size = new Size(600, 400);

        // Question input
        var lblQuestion = new Label 
        { 
            Text = "Your Question:", 
            Location = new Point(10, 10), 
            AutoSize = true 
        };

        questionBox = new TextBox
        {
            Location = new Point(10, 30),
            Size = new Size(560, 60),
            Multiline = true,
            PlaceholderText = "e.g., What caused this crash?"
        };

        // Ask button
        askButton = new Button
        {
            Text = "Ask AI",
            Location = new Point(10, 100),
            Size = new Size(100, 30)
        };
        askButton.Click += async (s, e) => await AskAI();

        // Answer display
        var lblAnswer = new Label 
        { 
            Text = "AI Response:", 
            Location = new Point(10, 140), 
            AutoSize = true 
        };

        answerBox = new RichTextBox
        {
            Location = new Point(10, 160),
            Size = new Size(560, 180),
            ReadOnly = true,
            Font = new Font("Segoe UI", 9F)
        };

        Controls.AddRange(new Control[] 
        { 
            lblQuestion, questionBox, askButton, 
            lblAnswer, answerBox 
        });
    }

    private async Task AskAI()
    {
        if (string.IsNullOrWhiteSpace(questionBox.Text))
        {
            MessageBox.Show("Please enter a question.");
            return;
        }

        askButton.Enabled = false;
        answerBox.Text = "Thinking...";

        try
        {
            var request = new AIRequest
            {
                SystemPrompt = "You are a helpful assistant analyzing CAD logs.",
                Prompt = questionBox.Text,
                MaxTokens = 2000
            };

            var response = await _aiService.CurrentProvider.SendRequestAsync(request);

            if (response.Success)
            {
                answerBox.Text = response.Content;
            }
            else
            {
                answerBox.Text = $"Error: {response.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            answerBox.Text = $"Error: {ex.Message}";
        }
        finally
        {
            askButton.Enabled = true;
        }
    }
}
```

### Example 2: Real-time Streaming UI

```csharp
public class StreamingAnalysisPanel : UserControl
{
    private RichTextBox outputBox;
    private Button analyzeButton;
    private Button stopButton;
    private ProgressBar progressBar;
    private CancellationTokenSource _cts;

    public async Task StartAnalysis(string logContent, AIService aiService)
    {
        outputBox.Clear();
        analyzeButton.Enabled = false;
        stopButton.Enabled = true;
        progressBar.Style = ProgressBarStyle.Marquee;

        _cts = new CancellationTokenSource();

        try
        {
            var request = new AIRequest
            {
                SystemPrompt = "Analyze this log thoroughly.",
                Prompt = logContent,
                MaxTokens = 4000
            };

            await aiService.CurrentProvider.StreamRequestAsync(
                request,
                onChunkReceived: chunk =>
                {
                    // Append text as it arrives
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            outputBox.AppendText(chunk);
                            outputBox.ScrollToCaret();
                        }));
                    }
                },
                onComplete: response =>
                {
                    Invoke(new Action(() =>
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        analyzeButton.Enabled = true;
                        stopButton.Enabled = false;
                    }));
                },
                onError: error =>
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Error: {error.Message}");
                        analyzeButton.Enabled = true;
                        stopButton.Enabled = false;
                    }));
                },
                _cts.Token
            );
        }
        catch (OperationCanceledException)
        {
            outputBox.AppendText("\n\n[Analysis stopped by user]");
        }
        finally
        {
            analyzeButton.Enabled = true;
            stopButton.Enabled = false;
            progressBar.Style = ProgressBarStyle.Continuous;
        }
    }

    private void StopButton_Click(object sender, EventArgs e)
    {
        _cts?.Cancel();
    }
}
```

---

## ?? Summary Checklist

When adding AI features to your application:

- [ ] Initialize `AIService` on startup
- [ ] Check `_aiService.IsEnabled` before using AI
- [ ] Handle errors gracefully with try-catch
- [ ] Provide user feedback (loading indicators)
- [ ] Support cancellation for long operations
- [ ] Test with Mock provider first
- [ ] Respect token limits
- [ ] Cache results when appropriate
- [ ] Implement proper error messages
- [ ] Add keyboard shortcuts for AI features
- [ ] Document your AI features
- [ ] Test with real AI providers

## ?? Next Steps

1. Review the example implementations
2. Start with simple features (explain error, summarize log)
3. Test thoroughly with Mock provider
4. Add more advanced features (conversations, comparisons)
5. Optimize for performance (caching, token limits)
6. Gather user feedback and iterate

## ?? Support

- Check `AI/Examples/OllamaUsageExample.cs` for more examples
- Review existing AI features in `Managers/AiAssistantPanel.cs`
- Read provider-specific docs in `AI/Providers/*/`

Happy coding! ??
