using System;
using Cad3PLogBrowser.AI.Providers.Ollama;

namespace Cad3PLogBrowser.AI.Configuration
{
    /// <summary>
    /// Helper class to create and configure AI providers.
    /// Provides default configurations and factory methods for Ollama providers.
    /// </summary>
    public static class OllamaConfigurationHelper
    {
        // Default configuration values
        private const string DefaultServerUrl = "http://localhost:11434";
        private const string DefaultModel = "llama3";
        private const double DefaultTemperature = 0.7;
        private const int DefaultMaxTokens = 4096;

        /// <summary>
        /// Creates an Ollama provider with default local settings.
        /// Use this for quick testing with a local Ollama instance.
        /// </summary>
        public static OllamaProvider CreateLocalProvider(string model = DefaultModel)
        {
            return new OllamaProvider(DefaultServerUrl, model);
        }

        /// <summary>
        /// Creates an Ollama provider with explicit configuration.
        /// Use this in your application startup or settings dialog.
        /// </summary>
        public static OllamaProvider CreateProvider(string serverUrl, string model = DefaultModel)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException("Server URL is required", nameof(serverUrl));

            return new OllamaProvider(serverUrl, model);
        }

        /// <summary>
        /// Gets the default system prompt for log analysis.
        /// </summary>
        public static string GetDefaultSystemPrompt()
        {
            return @"You are an expert at analyzing CAD application logs. 
When analyzing logs:
1. Identify errors and their root causes
2. Suggest specific fixes with code examples if applicable
3. Highlight patterns or anomalies
4. Focus on actionable insights
5. Keep explanations concise but thorough";
        }

        /// <summary>
        /// Gets the default temperature setting.
        /// </summary>
        public static double GetDefaultTemperature()
        {
            return DefaultTemperature;
        }

        /// <summary>
        /// Gets the default max tokens setting.
        /// </summary>
        public static int GetDefaultMaxTokens()
        {
            return DefaultMaxTokens;
        }
    }
}
