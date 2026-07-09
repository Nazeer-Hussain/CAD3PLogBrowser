using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Represents a multi-turn conversation with an AI provider.
    /// Manages message history and context across multiple requests.
    /// </summary>
    public interface IAIConversation
    {
        /// <summary>Unique identifier for this conversation.</summary>
        string ConversationId { get; }

        /// <summary>Timestamp when the conversation was created.</summary>
        DateTime CreatedAt { get; }

        /// <summary>All messages in this conversation (user + assistant).</summary>
        IReadOnlyList<ChatMessage> Messages { get; }

        /// <summary>System prompt that provides context for the entire conversation.</summary>
        string SystemPrompt { get; set; }

        /// <summary>Custom metadata for this conversation (e.g., log file name, analysis type).</summary>
        Dictionary<string, object> Metadata { get; }

        /// <summary>
        /// Sends a user message and receives a response, automatically maintaining history.
        /// </summary>
        Task<IAIResponse> SendMessageAsync(string userMessage, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a user message with streaming response.
        /// </summary>
        Task StreamMessageAsync(string userMessage,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a message to the conversation history without sending a request.
        /// Useful for loading previous conversations.
        /// </summary>
        void AddMessage(ChatMessage message);

        /// <summary>
        /// Clears all messages from the conversation history.
        /// </summary>
        void Clear();

        /// <summary>
        /// Estimates total tokens used in the entire conversation.
        /// </summary>
        int EstimateTotalTokens();

        /// <summary>
        /// Removes oldest messages to fit within the token limit, keeping system prompt intact.
        /// </summary>
        void TrimToTokenLimit(int maxTokens);
    }

    /// <summary>
    /// Represents a single message in a conversation.
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; }        // "user", "assistant", "system"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int? TokenCount { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public ChatMessage()
        {
            Timestamp = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }
    }
}
