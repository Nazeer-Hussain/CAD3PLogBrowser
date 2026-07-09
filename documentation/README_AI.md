# CAD3PLogBrowser AI Integration Framework

## Overview

This framework provides enterprise-grade AI capabilities for log analysis in the CAD3PLogBrowser application. It supports multiple AI providers with a provider-agnostic architecture, secure credential storage, and extensive customization options.

## Architecture

```
???????????????????????????????????????????????????????????????
?                    Application Layer                         ?
?  (MainForm, AiAssistantPanel, AISettingsDialog, etc.)       ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
?                   AIService                                  ?
?  (Orchestrates all AI functionality)                         ?
??????????????????????????????????????????????????????????????
         ?                       ?
???????????????????   ??????????????????????
? Context         ?   ?  Prompt Builder    ?
? Providers       ?   ?  & Templates       ?
???????????????????   ??????????????????????
         ?                       ?
         ?????????????????????????
                     ?
        ???????????????????????????
        ?    IAIProvider          ?
        ?    (Interface)          ?
        ???????????????????????????
                     ?
    ???????????????????????????????????
    ?                ?                ?
????????????  ??????????????  ??????????????
? Anthropic?  ?   OpenAI   ?  ?   Mock     ?
? Provider ?  ?  Provider  ?  ?  Provider  ?
????????????  ??????????????  ??????????????
```

## Core Components

### 1. Abstractions (`AI/Abstractions/`)

- **IAIProvider**: Main provider interface
- **IAIConversation**: Multi-turn conversation management
- **IAIRequest**: Request structure
- **IAIResponse**: Response structure
- **IAIAuthentication**: Authentication handling
- **IContextProvider**: Context injection interface
- **IPromptBuilder**: Prompt construction interface
- **ITokenEstimator**: Token counting interface
- **IConversationStorage**: Conversation persistence

### 2. Models (`AI/Models/`)

- **AIProviderType**: Enum of supported providers
- **AISettings**: Configuration settings
- **AnalysisResult**: Analysis operation results
- **ChatMessage**: Conversation message structure

### 3. Providers (`AI/Providers/`)

#### Anthropic (Claude)
- Full implementation with streaming support
- Supports Claude 3.x models
- 200K context window

#### Mock Provider
- Testing without API keys
- Generates realistic synthetic responses
- No internet required

#### Planned Providers
- OpenAI (GPT-4, GPT-4o, GPT-5)
- Azure OpenAI
- Google Gemini

### 4. Context Providers (`AI/Context/`)

Inject relevant context into AI prompts:

- **CurrentLogContextProvider**: Current log statistics
- **SelectedLinesContextProvider**: User-selected text
- **SelectedNodesContextProvider**: Tree node selections
- **SearchResultsContextProvider**: Search results
- **ComparisonContextProvider**: Log comparison data

### 5. Security (`AI/Security/`)

- **CredentialManager**: Secure API key storage using Windows DPAPI
- **DataRedactor**: PII/sensitive data redaction
- **AISettingsService**: Secure settings persistence

### 6. Services (`AI/Services/`)

- **AIService**: Main orchestration service
- **TokenEstimationService**: Token counting
- **ConversationStorage**: Conversation persistence

## Supported Providers

### GitHub Copilot
**Status**: ? Not Supported

GitHub Copilot is **not available** as an embeddable API for third-party applications. It is only accessible through:
- Visual Studio Code extension
- Visual Studio extension
- GitHub.com chat interface
- JetBrains IDEs

**Reason**: GitHub Copilot uses proprietary infrastructure and is tightly integrated with Microsoft/GitHub's authentication and billing systems. There is no public API for embedding Copilot into custom applications.

**Recommendation**: Use **Azure OpenAI** (GPT-4) which provides similar capabilities and is officially supported for enterprise integration.

### Anthropic Claude ?
**Status**: Fully Supported

- **Authentication**: API Key
- **Models**: 
  - claude-3-5-sonnet-20241022 (recommended)
  - claude-3-opus-latest
  - claude-3-haiku-latest
- **Context Window**: 200,000 tokens
- **Pricing**: Pay-per-token
- **Setup**:
  1. Get API key from https://console.anthropic.com
  2. Enter in AI Settings dialog
  3. Select Anthropic provider
  4. Choose model

### OpenAI ??
**Status**: Planned

- **Authentication**: API Key or OAuth
- **Models**: GPT-4, GPT-4o, GPT-4-turbo, GPT-3.5-turbo
- **Context Window**: Up to 128K tokens
- **Pricing**: Pay-per-token

### Azure OpenAI ??
**Status**: Planned

- **Authentication**: 
  - API Key
  - Azure Active Directory (Entra ID)
  - Managed Identity
- **Models**: Same as OpenAI (deployed in Azure)
- **Context Window**: Up to 128K tokens
- **Advantages**:
  - Enterprise SLA
  - Data stays in your Azure tenant
  - RBAC integration
  - Compliance certifications

### Google Gemini ??
**Status**: Planned

- **Authentication**: API Key
- **Models**: gemini-pro, gemini-pro-vision
- **Context Window**: Up to 1M tokens
- **Pricing**: Pay-per-token

## Usage Examples

### Basic Analysis

```csharp
// Initialize service
var settings = AISettingsService.Load();
var aiService = new AIService(settings);

// Create context providers
var contextProviders = new List<IContextProvider>
{
    new CurrentLogContextProvider(aggregateStats, logFilePath),
    new SelectedLinesContextProvider(() => richTextBox.SelectedText)
};

// Perform analysis
var result = await aiService.AnalyzeAsync(
    AnalysisType.Summarize,
    contextProviders,
    cancellationToken: cancellationToken);

if (result.Success)
{
    MessageBox.Show(result.Content);
}
```

### Streaming Analysis

```csharp
await aiService.AnalyzeStreamingAsync(
    AnalysisType.RootCause,
    contextProviders,
    onChunkReceived: chunk => {
        // Update UI incrementally
        richTextBox.AppendText(chunk);
    },
    onComplete: result => {
        // Analysis complete
        statusLabel.Text = $"Completed in {result.ElapsedTime.TotalSeconds:F1}s";
    },
    onError: ex => {
        MessageBox.Show($"Error: {ex.Message}");
    });
```

### Multi-Turn Conversation

```csharp
// Start conversation
aiService.StartConversation(SystemPrompts.General);

// Send first message with context
var response1 = await aiService.SendConversationMessageAsync(
    "What errors are in this log?",
    contextProviders);

// Follow-up questions don't need context again
var response2 = await aiService.SendConversationMessageAsync(
    "Explain the first error in detail");

var response3 = await aiService.SendConversationMessageAsync(
    "How can I fix it?");

// Clear conversation
aiService.ClearConversation();
```

### Comparing Logs

```csharp
string oldLogSummary = GetLogSummary(oldStats);
string newLogSummary = GetLogSummary(newStats);

var result = await aiService.CompareLogsAsync(
    oldLogSummary,
    newLogSummary,
    "What changed between these two runs?");

Console.WriteLine(result.Content);
```

## Security Best Practices

### API Key Storage

? **DO**:
- Store API keys in Windows Credential Manager
- Use `CredentialManager.StoreCredential()`
- Never log or display full API keys

? **DON'T**:
- Store keys in plain text files
- Include keys in source code
- Log API keys in debug output
- Store keys in settings.json

### Data Redaction

Enable redaction to protect sensitive information:

```csharp
settings.RedactSensitiveData = true;
settings.RedactionPatterns = new List<string>
{
    "UserName",
    "EmailAddress",
    "IPAddress",
    "FilePath",
    "ComputerName"
};
```

Redaction automatically removes:
- Email addresses
- IP addresses
- File paths
- Computer names
- API keys in log text
- UUIDs/GUIDs

### Token Limits

Prevent excessive costs:

```csharp
settings.MaxTokens = 4096;  // Limit response size
settings.MaxContextTokens = 100000;  // Limit input size
settings.AutoTruncateContext = true;  // Auto-truncate large logs
```

## Configuration

### AI Settings Dialog

The application provides a UI for configuring AI settings:

1. **Provider Selection**: Choose from OpenAI, Azure OpenAI, Anthropic, Gemini, or Mock
2. **Authentication**: Enter API keys (stored securely)
3. **Model Selection**: Choose specific models
4. **Parameters**:
   - Temperature (0.0 - 2.0)
   - Max Tokens
   - Timeout
5. **Privacy**:
   - Enable/disable data redaction
   - Configure redaction patterns
6. **Conversation**:
   - Remember conversation history
   - Max conversation messages
   - Auto-save conversations

### Settings Persistence

Settings are saved to:
- **Non-sensitive**: `%AppData%\CAD3PLogBrowser\ai_settings.json`
- **API Keys**: Windows Credential Manager or encrypted file

## Analysis Types

The framework supports these analysis operations:

1. **Summarize**: Executive summary of log
2. **Root Cause**: Identify failure causes
3. **Performance**: Find bottlenecks
4. **Timeline**: Chronological event sequence
5. **Compare Logs**: Diff two log files
6. **Find Errors**: List and explain errors
7. **Find Warnings**: List and explain warnings
8. **Explain Crash**: Crash analysis
9. **Suggest Fix**: Recommended fixes
10. **Custom**: User-defined analysis

## Prompt Templates

System prompts are defined in `SystemPrompts.cs`:

- **General**: Default assistant behavior
- **Summarize**: Executive summary generation
- **RootCause**: Failure analysis
- **Performance**: Performance analysis
- **Comparison**: Log comparison
- **Timeline**: Event sequencing
- **ExplainException**: Exception explanation
- **SuggestFix**: Fix recommendations
- **CrashAnalysis**: Crash investigation

## Token Management

### Estimation

```csharp
var estimator = new SmartTokenEstimator();
int tokens = estimator.EstimateTokenCount(text);
```

### Truncation

```csharp
string truncated = estimator.TruncateToTokenLimit(text, maxTokens: 10000);
```

### Context Window Management

For large logs:
1. Send aggregated statistics instead of raw lines
2. Use chunking for very large contexts
3. Implement semantic summarization
4. Use truncated context providers

## Error Handling

All AI operations include comprehensive error handling:

```csharp
var result = await aiService.AnalyzeAsync(...);

if (!result.Success)
{
    // result.ErrorMessage contains error details
    HandleError(result.ErrorMessage);
}
```

Common errors:
- **API key not configured**: Enable AI in settings
- **Rate limit exceeded**: Retry with exponential backoff
- **Token limit exceeded**: Reduce context size
- **Network timeout**: Increase timeout setting
- **Invalid model**: Update model name

## Testing

### Unit Tests

Test without API calls:

```csharp
var mockProvider = new MockProvider();
var conversation = mockProvider.CreateConversation();

var response = await conversation.SendMessageAsync("Test message");
Assert.IsTrue(response.Success);
```

### Integration Tests

Test with real providers:

```csharp
var provider = new AnthropicProvider(apiKey, "claude-3-5-sonnet-20241022");
var testResult = await provider.TestConnectionAsync();

Assert.IsNull(testResult); // Null means success
```

## Extending the Framework

### Adding a New Provider

1. Create class implementing `IAIProvider`
2. Implement required methods
3. Add to `AIProviderType` enum
4. Update `AIService.InitializeProvider()`
5. Add UI controls in settings dialog

Example skeleton:

```csharp
public class MyProvider : IAIProvider
{
    public string ProviderName => "My Provider";
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public async Task<IAIResponse> SendRequestAsync(
        IAIRequest request, 
        CancellationToken cancellationToken)
    {
        // Implementation
    }

    // Implement other interface members...
}
```

### Adding a New Context Provider

```csharp
public class MyContextProvider : ContextProviderBase
{
    public override string ContextType => "MyContext";
    public override string Description => "My custom context";
    public override bool HasContext => /* check if data available */;

    public override async Task<string> GetContextAsync()
    {
        // Return context text
    }
}
```

### Adding a New Analysis Type

1. Add to `AnalysisType` enum
2. Create system prompt in `SystemPrompts.cs`
3. Add UI button/menu item
4. Call `AIService.AnalyzeAsync()` with new type

## Performance Considerations

### Token Usage

- Typical summarization: 500-2000 tokens
- Root cause analysis: 1000-4000 tokens
- Chat message: 100-500 tokens
- Large context: 10,000-100,000 tokens

### Cost Estimation

Claude 3.5 Sonnet pricing (as of 2024):
- Input: $3 / million tokens
- Output: $15 / million tokens

Typical analysis costs:
- Quick summary: $0.01 - $0.05
- Deep analysis: $0.10 - $0.50
- Large log comparison: $0.50 - $2.00

### Optimization Tips

1. Use aggregated statistics instead of raw logs
2. Enable context truncation
3. Cache analysis results
4. Use cheaper models for simple tasks
5. Implement rate limiting

## Future Enhancements

### Planned Features

- [ ] RAG (Retrieval-Augmented Generation) support
- [ ] Vector embeddings for semantic search
- [ ] Local LLM support (llama.cpp, Ollama)
- [ ] Batch processing for multiple logs
- [ ] Analysis templates and presets
- [ ] Integration with Azure Cognitive Search
- [ ] Multi-language support
- [ ] Export analysis to PDF/Word
- [ ] Scheduled analysis jobs
- [ ] API usage dashboard

### RAG Implementation Plan

1. **Embedding Generation**
   - Use OpenAI embeddings or local models
   - Store in vector database (Chroma, Pinecone)

2. **Semantic Search**
   - Index log entries
   - Find relevant context for queries

3. **Context Retrieval**
   - Top-K similarity search
   - Rerank results
   - Inject into prompts

### Local LLM Support

For air-gapped environments:

1. **llama.cpp Integration**
   - Run models locally (Llama, Mistral, etc.)
   - No internet required
   - Complete privacy

2. **Ollama Integration**
   - Simple local model management
   - Compatible API

## Troubleshooting

### Connection Issues

**Problem**: "Failed to connect to AI provider"

**Solutions**:
- Check internet connection
- Verify API key is correct
- Check if provider service is down
- Try Test Connection in settings

### Token Limit Exceeded

**Problem**: "Context too large"

**Solutions**:
- Enable auto-truncation
- Reduce `MaxContextTokens`
- Use aggregated stats instead of raw logs
- Send smaller context

### Rate Limiting

**Problem**: "Rate limit exceeded"

**Solutions**:
- Wait and retry
- Increase retry delay
- Reduce request frequency
- Upgrade provider plan

### API Key Issues

**Problem**: "Invalid API key"

**Solutions**:
- Verify key in provider console
- Check for extra spaces
- Regenerate key
- Ensure key has required permissions

## Support

For issues or questions:
- GitHub: https://github.com/Nazeer-Hussain/CAD3PLogBrowser
- Email: [maintainer email]

## License

[Your License]

## Credits

Developed for CAD3PLogBrowser by [Your Name]
