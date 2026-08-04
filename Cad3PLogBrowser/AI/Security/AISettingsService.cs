using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Security;

namespace Cad3PLogBrowser.AI.Services
{
    /// <summary>
    /// Manages AI settings persistence with secure credential storage.
    /// API keys are stored in Windows Credential Manager (or encrypted files on non-Windows).
    /// Other settings are stored as JSON.
    /// </summary>
    public class AISettingsService
    {
        private const string SettingsFileName = "ai_settings.json";
        private static string SettingsFilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", SettingsFileName);

        /// <summary>
        /// Loads AI settings from disk.
        /// API keys are loaded from secure storage.
        /// </summary>
        public static AISettings Load()
        {
            try
            {
                AISettings settings;

                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath, Encoding.UTF8);

                    var serializer = new DataContractJsonSerializer(typeof(AISettings));
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        settings = (AISettings)serializer.ReadObject(ms);
                    }

                    if (settings == null)
                        settings = AISettings.CreateDefault();
                }
                else
                {
                    settings = AISettings.CreateDefault();
                }

                // Load API keys from secure storage
                LoadSecureCredentials(settings);

                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load AI settings: {ex.Message}");
                return AISettings.CreateDefault();
            }
        }

        /// <summary>
        /// Saves AI settings to disk.
        /// API keys are saved to secure storage.
        /// </summary>
        public static bool Save(AISettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            try
            {
                // Create a copy without API keys for JSON storage
                var settingsToSave = CloneWithoutApiKeys(settings);

                // Save non-sensitive settings as JSON
                var serializer = new DataContractJsonSerializer(typeof(AISettings));
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, settingsToSave);
                    string json = Encoding.UTF8.GetString(ms.ToArray());

                    string directory = Path.GetDirectoryName(SettingsFilePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllText(SettingsFilePath, json, Encoding.UTF8);
                }

                // Save API keys to secure storage
                SaveSecureCredentials(settings);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save AI settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes all AI settings including secure credentials.
        /// </summary>
        public static bool Delete()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    File.Delete(SettingsFilePath);
                }

                // Delete secure credentials
                CredentialManager.DeleteCredential("AI_OpenAI");
                CredentialManager.DeleteCredential("AI_AzureOpenAI");
                CredentialManager.DeleteCredential("AI_Anthropic");
                CredentialManager.DeleteCredential("AI_GoogleGemini");
                CredentialManager.DeleteCredential("AI_GitHubCopilot");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete AI settings: {ex.Message}");
                return false;
            }
        }

        private static void LoadSecureCredentials(AISettings settings)
        {
            try
            {
                settings.OpenAIApiKey = CredentialManager.RetrieveCredential("AI_OpenAI") ?? "";
                settings.AzureOpenAIApiKey = CredentialManager.RetrieveCredential("AI_AzureOpenAI") ?? "";
                settings.AnthropicApiKey = CredentialManager.RetrieveCredential("AI_Anthropic") ?? "";
                settings.GoogleApiKey = CredentialManager.RetrieveCredential("AI_GoogleGemini") ?? "";
                settings.GitHubCopilotApiToken = CredentialManager.RetrieveCredential("AI_GitHubCopilot") ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load secure credentials: {ex.Message}");
            }
        }

        private static void SaveSecureCredentials(AISettings settings)
        {
            try
            {
                // Only save non-empty API keys
                if (!string.IsNullOrWhiteSpace(settings.OpenAIApiKey))
                    CredentialManager.StoreCredential("AI_OpenAI", settings.OpenAIApiKey);

                if (!string.IsNullOrWhiteSpace(settings.AzureOpenAIApiKey))
                    CredentialManager.StoreCredential("AI_AzureOpenAI", settings.AzureOpenAIApiKey);

                if (!string.IsNullOrWhiteSpace(settings.AnthropicApiKey))
                    CredentialManager.StoreCredential("AI_Anthropic", settings.AnthropicApiKey);

                if (!string.IsNullOrWhiteSpace(settings.GoogleApiKey))
                    CredentialManager.StoreCredential("AI_GoogleGemini", settings.GoogleApiKey);

                if (!string.IsNullOrWhiteSpace(settings.GitHubCopilotApiToken))
                    CredentialManager.StoreCredential("AI_GitHubCopilot", settings.GitHubCopilotApiToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save secure credentials: {ex.Message}");
            }
        }

        private static AISettings CloneWithoutApiKeys(AISettings source)
        {
            // Manual cloning to avoid JSON serialization
            var clone = new AISettings
            {
                SelectedProvider = source.SelectedProvider,
                EnableAI = source.EnableAI,
                OpenAIModel = source.OpenAIModel,
                OpenAIOrganization = source.OpenAIOrganization,
                AzureOpenAIEndpoint = source.AzureOpenAIEndpoint,
                AzureOpenAIDeploymentName = source.AzureOpenAIDeploymentName,
                AzureOpenAIApiVersion = source.AzureOpenAIApiVersion,
                AnthropicModel = source.AnthropicModel,
                GoogleModel = source.GoogleModel,
                GitHubCopilotModel = source.GitHubCopilotModel,
                GitHubCopilotEndpoint = source.GitHubCopilotEndpoint,
                OllamaServerUrl = source.OllamaServerUrl,
                OllamaModel = source.OllamaModel,
                RedactionPatterns = new System.Collections.Generic.List<string>(source.RedactionPatterns ?? new System.Collections.Generic.List<string>()),
                Temperature = source.Temperature,
                MaxTokens = source.MaxTokens,
                EnableStreaming = source.EnableStreaming,
                TimeoutSeconds = source.TimeoutSeconds,
                RememberConversation = source.RememberConversation,
                MaxConversationMessages = source.MaxConversationMessages,
                AutoSaveConversations = source.AutoSaveConversations,
                RedactSensitiveData = source.RedactSensitiveData,
                MaxContextTokens = source.MaxContextTokens,
                AutoTruncateContext = source.AutoTruncateContext,
                ChunkingStrategy = source.ChunkingStrategy,
                ShowTokenUsage = source.ShowTokenUsage,
                ShowElapsedTime = source.ShowElapsedTime,
                LogRequests = source.LogRequests,
                RetryAttempts = source.RetryAttempts,
                RetryDelayMs = source.RetryDelayMs
            };

            // Clear API keys
            clone.OpenAIApiKey = "";
            clone.AzureOpenAIApiKey = "";
            clone.AnthropicApiKey = "";
            clone.GoogleApiKey = "";
            clone.GitHubCopilotApiToken = "";

            return clone;
        }
    }
}
