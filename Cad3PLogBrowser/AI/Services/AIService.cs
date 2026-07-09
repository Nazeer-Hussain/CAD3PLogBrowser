using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Providers.Anthropic;
using Cad3PLogBrowser.AI.Providers.Mock;
using Cad3PLogBrowser.AI.Providers.GitHub;
using Cad3PLogBrowser.AI.Providers.Ollama;
using Cad3PLogBrowser.AI.Prompts;
using Cad3PLogBrowser.AI.Security;
using Cad3PLogBrowser.AI.Services;

namespace Cad3PLogBrowser.AI.Services
{
    /// <summary>
    /// Main AI service that coordinates all AI functionality.
    /// Provides a simplified interface for the application to interact with AI providers.
    /// </summary>
    public class AIService
    {
        private IAIProvider _currentProvider;
        private readonly AISettings _settings;
        private readonly IPromptBuilder _promptBuilder;
        private readonly ITokenEstimator _tokenEstimator;
        private readonly DataRedactor _dataRedactor;
        private IAIConversation _activeConversation;

        public AIService(AISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _tokenEstimator = new SmartTokenEstimator();
            _promptBuilder = new PromptBuilder(_tokenEstimator);
            _dataRedactor = new DataRedactor();

            InitializeProvider();
        }

        /// <summary>
        /// Indicates whether AI features are enabled and properly configured.
        /// </summary>
        public bool IsEnabled => _settings.EnableAI && _currentProvider != null && _currentProvider.IsConfigured;

        /// <summary>
        /// Gets the currently active provider.
        /// </summary>
        public IAIProvider CurrentProvider => _currentProvider;

        /// <summary>
        /// Gets the current conversation (null if no conversation is active).
        /// </summary>
        public IAIConversation ActiveConversation => _activeConversation;

        /// <summary>
        /// Reinitializes the provider after settings changes.
        /// </summary>
        public void RefreshProvider()
        {
            InitializeProvider();
        }

        /// <summary>
        /// Tests connection to the current AI provider.
        /// </summary>
        public async Task<(bool success, string message)> TestConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            if (!_settings.EnableAI)
                return (false, "AI features are disabled in settings");

            if (_currentProvider == null)
                return (false, "No AI provider configured");

            string error = await _currentProvider.TestConnectionAsync(cancellationToken);

            if (error == null)
                return (true, "Connection successful");
            else
                return (false, error);
        }

        /// <summary>
        /// Performs a one-shot analysis on the provided context.
        /// </summary>
        public async Task<AnalysisResult> AnalyzeAsync(
            AnalysisType analysisType,
            IEnumerable<IContextProvider> contextProviders,
            string userQuery = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
            {
                return AnalysisResult.CreateError("AI features are not enabled", analysisType);
            }

            try
            {
                var startTime = DateTime.UtcNow;

                // Build the prompt
                string systemPrompt = _promptBuilder.GetSystemPrompt(analysisType.ToString());
                string userPrompt = _promptBuilder.BuildAnalysisPrompt(
                    analysisType.ToString(), 
                    userQuery, 
                    contextProviders);

                // Redact sensitive data if enabled
                if (_settings.RedactSensitiveData)
                {
                    userPrompt = _dataRedactor.Redact(userPrompt);
                }

                // Create request
                var request = new AIRequest
                {
                    SystemPrompt = systemPrompt,
                    Prompt = userPrompt,
                    Model = _settings.GetCurrentModel(),
                    Temperature = _settings.Temperature,
                    MaxTokens = _settings.MaxTokens
                };

                // Send request
                var response = await _currentProvider.SendRequestAsync(request, cancellationToken);

                if (!response.Success)
                {
                    return AnalysisResult.CreateError(response.ErrorMessage, analysisType);
                }

                var result = AnalysisResult.CreateSuccess(response.Content, analysisType);
                result.ElapsedTime = DateTime.UtcNow - startTime;
                result.TokensUsed = response.TotalTokens;

                return result;
            }
            catch (Exception ex)
            {
                return AnalysisResult.CreateError($"Analysis failed: {ex.Message}", analysisType);
            }
        }

        /// <summary>
        /// Performs a streaming analysis with incremental results.
        /// </summary>
        public async Task AnalyzeStreamingAsync(
            AnalysisType analysisType,
            IEnumerable<IContextProvider> contextProviders,
            Action<string> onChunkReceived,
            Action<AnalysisResult> onComplete,
            Action<Exception> onError,
            string userQuery = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
            {
                onError?.Invoke(new InvalidOperationException("AI features are not enabled"));
                return;
            }

            try
            {
                var startTime = DateTime.UtcNow;

                // Build the prompt
                string systemPrompt = _promptBuilder.GetSystemPrompt(analysisType.ToString());
                string userPrompt = _promptBuilder.BuildAnalysisPrompt(
                    analysisType.ToString(), 
                    userQuery, 
                    contextProviders);

                // Redact sensitive data if enabled
                if (_settings.RedactSensitiveData)
                {
                    userPrompt = _dataRedactor.Redact(userPrompt);
                }

                // Create request
                var request = new AIRequest
                {
                    SystemPrompt = systemPrompt,
                    Prompt = userPrompt,
                    Model = _settings.GetCurrentModel(),
                    Temperature = _settings.Temperature,
                    MaxTokens = _settings.MaxTokens,
                    Stream = true
                };

                // Send streaming request
                await _currentProvider.StreamRequestAsync(
                    request,
                    onChunkReceived,
                    response =>
                    {
                        var result = response.Success
                            ? AnalysisResult.CreateSuccess(response.Content, analysisType)
                            : AnalysisResult.CreateError(response.ErrorMessage, analysisType);

                        result.ElapsedTime = DateTime.UtcNow - startTime;
                        result.TokensUsed = response.TotalTokens;

                        onComplete?.Invoke(result);
                    },
                    onError,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// Starts a new conversation for multi-turn interactions.
        /// </summary>
        public void StartConversation(string systemPrompt = null)
        {
            if (!IsEnabled)
                throw new InvalidOperationException("AI features are not enabled");

            _activeConversation = _currentProvider.CreateConversation();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                _activeConversation.SystemPrompt = systemPrompt;
            }
            else
            {
                _activeConversation.SystemPrompt = _promptBuilder.GetSystemPrompt("general");
            }
        }

        /// <summary>
        /// Sends a message in the active conversation.
        /// </summary>
        public async Task<IAIResponse> SendConversationMessageAsync(
            string userMessage,
            IEnumerable<IContextProvider> contextProviders = null,
            CancellationToken cancellationToken = default)
        {
            if (_activeConversation == null)
                throw new InvalidOperationException("No active conversation. Call StartConversation first.");

            // Build message with context if this is the first message
            string message = userMessage;

            if (_activeConversation.Messages.Count == 0 && contextProviders != null)
            {
                message = _promptBuilder.BuildChatPrompt(userMessage, null, contextProviders);
            }

            // Redact sensitive data if enabled
            if (_settings.RedactSensitiveData)
            {
                message = _dataRedactor.Redact(message);
            }

            return await _activeConversation.SendMessageAsync(message, cancellationToken);
        }

        /// <summary>
        /// Sends a streaming message in the active conversation.
        /// </summary>
        public async Task SendConversationMessageStreamingAsync(
            string userMessage,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            IEnumerable<IContextProvider> contextProviders = null,
            CancellationToken cancellationToken = default)
        {
            if (_activeConversation == null)
            {
                onError?.Invoke(new InvalidOperationException("No active conversation"));
                return;
            }

            // Build message with context if this is the first message
            string message = userMessage;

            if (_activeConversation.Messages.Count == 0 && contextProviders != null)
            {
                message = _promptBuilder.BuildChatPrompt(userMessage, null, contextProviders);
            }

            // Redact sensitive data if enabled
            if (_settings.RedactSensitiveData)
            {
                message = _dataRedactor.Redact(message);
            }

            await _activeConversation.StreamMessageAsync(
                message, 
                onChunkReceived, 
                onComplete, 
                onError, 
                cancellationToken);
        }

        /// <summary>
        /// Clears the active conversation.
        /// </summary>
        public void ClearConversation()
        {
            _activeConversation?.Clear();
        }

        /// <summary>
        /// Ends the current conversation.
        /// </summary>
        public void EndConversation()
        {
            _activeConversation = null;
        }

        /// <summary>
        /// Compares two logs using AI analysis.
        /// </summary>
        public async Task<AnalysisResult> CompareLogsAsync(
            string oldLogContext,
            string newLogContext,
            string userQuery = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
            {
                return AnalysisResult.CreateError("AI features are not enabled", AnalysisType.CompareLog);
            }

            try
            {
                var startTime = DateTime.UtcNow;

                string systemPrompt = _promptBuilder.GetSystemPrompt("comparison");
                string userPrompt = _promptBuilder.BuildComparisonPrompt(oldLogContext, newLogContext, userQuery);

                if (_settings.RedactSensitiveData)
                {
                    userPrompt = _dataRedactor.Redact(userPrompt);
                }

                var request = new AIRequest
                {
                    SystemPrompt = systemPrompt,
                    Prompt = userPrompt,
                    Model = _settings.GetCurrentModel(),
                    Temperature = _settings.Temperature,
                    MaxTokens = _settings.MaxTokens
                };

                var response = await _currentProvider.SendRequestAsync(request, cancellationToken);

                if (!response.Success)
                {
                    return AnalysisResult.CreateError(response.ErrorMessage, AnalysisType.CompareLog);
                }

                var result = AnalysisResult.CreateSuccess(response.Content, AnalysisType.CompareLog);
                result.ElapsedTime = DateTime.UtcNow - startTime;
                result.TokensUsed = response.TotalTokens;

                return result;
            }
            catch (Exception ex)
            {
                return AnalysisResult.CreateError($"Comparison failed: {ex.Message}", AnalysisType.CompareLog);
            }
        }

        private void InitializeProvider()
        {
            if (!_settings.EnableAI || _settings.SelectedProvider == AIProviderType.None)
            {
                _currentProvider = null;
                return;
            }

            try
            {
                switch (_settings.SelectedProvider)
                {
                    case AIProviderType.Anthropic:
                        string apiKey = LoadApiKey("Anthropic") ?? _settings.AnthropicApiKey;
                        if (!string.IsNullOrWhiteSpace(apiKey))
                        {
                            _currentProvider = new AnthropicProvider(apiKey, _settings.AnthropicModel);
                        }
                        break;

                    case AIProviderType.GitHubCopilot:
                        string ghToken = LoadApiKey("GitHubCopilot") ?? _settings.GitHubCopilotApiToken;
                        if (!string.IsNullOrWhiteSpace(ghToken))
                        {
                            _currentProvider = new GitHubCopilotProvider(
                                ghToken, 
                                _settings.GitHubCopilotModel,
                                _settings.GitHubCopilotEndpoint);
                        }
                        break;

                    case AIProviderType.Ollama:
                        string ollamaUrl = _settings.OllamaServerUrl ?? "http://localhost:11434";
                        string ollamaModel = _settings.OllamaModel ?? "llama3";
                        _currentProvider = new OllamaProvider(ollamaUrl, ollamaModel);
                        break;

                    case AIProviderType.Mock:
                        _currentProvider = new MockProvider();
                        break;

                    // TODO: Implement other providers (OpenAI, Azure OpenAI, Google Gemini)

                    default:
                        _currentProvider = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize AI provider: {ex.Message}");
                _currentProvider = null;
            }
        }

        private string LoadApiKey(string providerName)
        {
            try
            {
                return CredentialManager.RetrieveCredential($"AI_{providerName}");
            }
            catch
            {
                return null;
            }
        }
    }
}
