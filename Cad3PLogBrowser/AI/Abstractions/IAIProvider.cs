using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Core abstraction for AI service providers.
    /// Enables swapping between OpenAI, Azure OpenAI, Anthropic Claude, Google Gemini, or mock implementations.
    /// </summary>
    public interface IAIProvider
    {
        /// <summary>Provider identifier (e.g., "OpenAI", "AzureOpenAI", "Anthropic", "Gemini").</summary>
        string ProviderName { get; }

        /// <summary>Indicates whether this provider is properly configured and ready for use.</summary>
        bool IsConfigured { get; }

        /// <summary>Authentication interface for this provider.</summary>
        IAIAuthentication Authentication { get; }

        /// <summary>
        /// Sends a single request to the AI provider.
        /// </summary>
        Task<IAIResponse> SendRequestAsync(IAIRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a streaming request where responses arrive incrementally.
        /// </summary>
        Task StreamRequestAsync(IAIRequest request, 
            Action<string> onChunkReceived, 
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new conversation context for multi-turn interactions.
        /// </summary>
        IAIConversation CreateConversation();

        /// <summary>
        /// Tests connectivity and authentication with the provider.
        /// Returns null on success, or an error message on failure.
        /// </summary>
        Task<string> TestConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Estimates the number of tokens for the given text using provider-specific tokenization.
        /// </summary>
        int EstimateTokenCount(string text);

        /// <summary>
        /// Maximum context window size in tokens for the configured model.
        /// </summary>
        int MaxContextTokens { get; }

        /// <summary>
        /// Maximum output tokens supported by the configured model.
        /// </summary>
        int MaxOutputTokens { get; }
    }
}
