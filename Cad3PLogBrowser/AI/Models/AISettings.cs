using System;
using System.Collections.Generic;

namespace Cad3PLogBrowser.AI.Models
{
    /// <summary>
    /// Configuration settings for AI providers.
    /// Stored separately from AppSettings for better security and modularity.
    /// </summary>
    public class AISettings
    {
        // ?? Provider Selection ????????????????????????????????????????????????
        public AIProviderType SelectedProvider { get; set; } = AIProviderType.None;
        public bool EnableAI { get; set; } = false;

        // ?? OpenAI Settings ???????????????????????????????????????????????????
        public string OpenAIApiKey { get; set; } = "";
        public string OpenAIModel { get; set; } = "gpt-4o";
        public string OpenAIOrganization { get; set; } = "";

        // ?? Azure OpenAI Settings ?????????????????????????????????????????????
        public string AzureOpenAIEndpoint { get; set; } = "";
        public string AzureOpenAIApiKey { get; set; } = "";
        public string AzureOpenAIDeploymentName { get; set; } = "";
        public string AzureOpenAIApiVersion { get; set; } = "2024-02-15-preview";

        // ?? Anthropic (Claude) Settings ???????????????????????????????????????
        public string AnthropicApiKey { get; set; } = "";
        public string AnthropicModel { get; set; } = "claude-3-5-sonnet-20241022";

        // ?? Google Gemini Settings ????????????????????????????????????????????
        public string GoogleApiKey { get; set; } = "";
        public string GoogleModel { get; set; } = "gemini-pro";

        // ?? GitHub Copilot Settings ??????????????????????????????????????????
        public string GitHubCopilotApiToken { get; set; } = "";
        public string GitHubCopilotModel { get; set; } = "gpt-4";
        public string GitHubCopilotEndpoint { get; set; } = "https://api.github.com/copilot/chat/completions";

        // ?? Ollama Settings (Self-Hosted) ???????????????????????????????????????
        public string OllamaServerUrl { get; set; } = "http://localhost:11434";
        public string OllamaModel { get; set; } = "llama3";

        // ?? Request Parameters ????????????????????????????????????????????????
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 4096;
        public bool EnableStreaming { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 60;

        // ?? Conversation Settings ?????????????????????????????????????????????
        public bool RememberConversation { get; set; } = true;
        public int MaxConversationMessages { get; set; } = 20;
        public bool AutoSaveConversations { get; set; } = true;

        // ?? Privacy & Security ????????????????????????????????????????????????
        public bool RedactSensitiveData { get; set; } = true;
        public List<string> RedactionPatterns { get; set; } = new List<string>
        {
            "UserName",
            "ComputerName",
            "EmailAddress",
            "IPAddress",
            "FilePath"
        };

        // ?? Context Management ????????????????????????????????????????????????
        public int MaxContextTokens { get; set; } = 100000;
        public bool AutoTruncateContext { get; set; } = true;
        public string ChunkingStrategy { get; set; } = "Smart"; // "Smart", "Fixed", "Semantic"

        // ?? Advanced Settings ?????????????????????????????????????????????????
        public bool ShowTokenUsage { get; set; } = true;
        public bool ShowElapsedTime { get; set; } = true;
        public bool LogRequests { get; set; } = false; // For debugging - never log sensitive content
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;

        // ?? Helper Methods ????????????????????????????????????????????????????

        /// <summary>
        /// Returns the API key for the currently selected provider.
        /// </summary>
        public string GetCurrentApiKey()
        {
            switch (SelectedProvider)
            {
                case AIProviderType.OpenAI:
                    return OpenAIApiKey;
                case AIProviderType.AzureOpenAI:
                    return AzureOpenAIApiKey;
                case AIProviderType.Anthropic:
                    return AnthropicApiKey;
                case AIProviderType.GoogleGemini:
                    return GoogleApiKey;
                case AIProviderType.GitHubCopilot:
                    return GitHubCopilotApiToken;
                case AIProviderType.Ollama:
                    return ""; // Ollama doesn't need an API key
                default:
                    return "";
            }
        }

        /// <summary>
        /// Returns the model name for the currently selected provider.
        /// </summary>
        public string GetCurrentModel()
        {
            switch (SelectedProvider)
            {
                case AIProviderType.OpenAI:
                    return OpenAIModel;
                case AIProviderType.AzureOpenAI:
                    return AzureOpenAIDeploymentName;
                case AIProviderType.Anthropic:
                    return AnthropicModel;
                case AIProviderType.GoogleGemini:
                    return GoogleModel;
                case AIProviderType.GitHubCopilot:
                    return GitHubCopilotModel;
                case AIProviderType.Ollama:
                    return OllamaModel;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Validates that the current provider has necessary configuration.
        /// </summary>
        public bool IsCurrentProviderConfigured()
        {
            if (!EnableAI || SelectedProvider == AIProviderType.None)
                return false;

            switch (SelectedProvider)
            {
                case AIProviderType.OpenAI:
                    return !string.IsNullOrWhiteSpace(OpenAIApiKey);

                case AIProviderType.AzureOpenAI:
                    return !string.IsNullOrWhiteSpace(AzureOpenAIEndpoint) &&
                           !string.IsNullOrWhiteSpace(AzureOpenAIApiKey) &&
                           !string.IsNullOrWhiteSpace(AzureOpenAIDeploymentName);

                case AIProviderType.Anthropic:
                    return !string.IsNullOrWhiteSpace(AnthropicApiKey);

                case AIProviderType.GoogleGemini:
                    return !string.IsNullOrWhiteSpace(GoogleApiKey);

                case AIProviderType.GitHubCopilot:
                    return !string.IsNullOrWhiteSpace(GitHubCopilotApiToken);

                case AIProviderType.Ollama:
                    return !string.IsNullOrWhiteSpace(OllamaServerUrl);

                case AIProviderType.Mock:
                    return true; // Mock provider needs no configuration

                default:
                    return false;
            }
        }

        /// <summary>
        /// Creates a default settings instance.
        /// </summary>
        public static AISettings CreateDefault()
        {
            return new AISettings
            {
                EnableAI = false,
                SelectedProvider = AIProviderType.None,
                Temperature = 0.7,
                MaxTokens = 4096,
                EnableStreaming = true,
                RememberConversation = true,
                RedactSensitiveData = true
            };
        }

        /// <summary>
        /// Clears all API keys (for security when disabling AI features).
        /// </summary>
        public void ClearAllApiKeys()
        {
            OpenAIApiKey = "";
            AzureOpenAIApiKey = "";
            AnthropicApiKey = "";
            GoogleApiKey = "";
        }
    }
}
