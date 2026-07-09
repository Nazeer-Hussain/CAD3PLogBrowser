using System.Collections.Generic;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Represents a request sent to an AI provider.
    /// </summary>
    public interface IAIRequest
    {
        /// <summary>The main prompt or user message.</summary>
        string Prompt { get; set; }

        /// <summary>System prompt that sets the AI's behavior and context.</summary>
        string SystemPrompt { get; set; }

        /// <summary>Previous conversation history (for multi-turn conversations).</summary>
        List<ChatMessage> ConversationHistory { get; set; }

        /// <summary>Model identifier (e.g., "gpt-4", "claude-3-sonnet", "gemini-pro").</summary>
        string Model { get; set; }

        /// <summary>
        /// Controls randomness (0.0-2.0). Lower is more deterministic.
        /// Default: 0.7
        /// </summary>
        double Temperature { get; set; }

        /// <summary>
        /// Maximum number of tokens to generate in the response.
        /// </summary>
        int MaxTokens { get; set; }

        /// <summary>
        /// Whether to stream the response incrementally.
        /// </summary>
        bool Stream { get; set; }

        /// <summary>
        /// Custom parameters specific to the provider.
        /// </summary>
        Dictionary<string, object> ProviderSpecificSettings { get; set; }
    }

    /// <summary>
    /// Default implementation of IAIRequest.
    /// </summary>
    public class AIRequest : IAIRequest
    {
        public string Prompt { get; set; }
        public string SystemPrompt { get; set; }
        public List<ChatMessage> ConversationHistory { get; set; }
        public string Model { get; set; }
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
        public bool Stream { get; set; }
        public Dictionary<string, object> ProviderSpecificSettings { get; set; }

        public AIRequest()
        {
            ConversationHistory = new List<ChatMessage>();
            Temperature = 0.7;
            MaxTokens = 4096;
            Stream = false;
            ProviderSpecificSettings = new Dictionary<string, object>();
        }
    }
}
