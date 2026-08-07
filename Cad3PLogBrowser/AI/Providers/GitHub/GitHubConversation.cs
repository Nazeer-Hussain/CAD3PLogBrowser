using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Providers.GitHub
{
    /// <summary>
    /// Manages a conversation with GitHub Copilot.
    /// </summary>
    public class GitHubConversation : IAIConversation
    {
        private readonly IAIProvider _provider;
        private readonly List<ChatMessage> _messages;
        private readonly Dictionary<string, object> _metadata;

        public GitHubConversation(IAIProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _messages = new List<ChatMessage>();
            _metadata = new Dictionary<string, object>();
            ConversationId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }

        public string ConversationId { get; }

        public DateTime CreatedAt { get; }

        public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

        public string SystemPrompt { get; set; }

        public Dictionary<string, object> Metadata => _metadata;

        /// <summary>
        /// L2/L6: history budget passed to <see cref="TrimToTokenLimit"/> before every
        /// send. Reserves half the model's context window for prior turns, leaving room
        /// for the system prompt (which may include injected log context), the new
        /// message, and the response.
        /// </summary>
        private int MaxHistoryTokens => _provider.MaxContextTokens / 2;

        public async Task<IAIResponse> SendMessageAsync(string userMessage,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));

            // Add user message to history
            var userMsg = new ChatMessage
            {
                Role = "user",
                Content = userMessage,
                Timestamp = DateTime.UtcNow
            };
            _messages.Add(userMsg);

            // L2/L6: cap history growth instead of sending the entire unbounded
            // conversation every turn.
            TrimToTokenLimit(MaxHistoryTokens);

            // Build request with conversation history
            var request = new AIRequest
            {
                Prompt = userMessage,
                SystemPrompt = SystemPrompt,
                ConversationHistory = _messages.ToList()
            };

            // Send request
            var response = await _provider.SendRequestAsync(request, cancellationToken);

            // Add assistant response to history
            if (response.Success)
            {
                var assistantMsg = new ChatMessage
                {
                    Role = "assistant",
                    Content = response.Content,
                    Timestamp = DateTime.UtcNow
                };
                _messages.Add(assistantMsg);
            }

            return response;
        }

        public async Task SendMessageStreamingAsync(
            string userMessage,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));

            try
            {
                // Add user message to history
                var userMsg = new ChatMessage
                {
                    Role = "user",
                    Content = userMessage,
                    Timestamp = DateTime.UtcNow
                };
                _messages.Add(userMsg);

                // L2/L6: cap history growth instead of sending the entire unbounded
                // conversation every turn.
                TrimToTokenLimit(MaxHistoryTokens);

                // Build request with conversation history
                var request = new AIRequest
                {
                    Prompt = userMessage,
                    SystemPrompt = SystemPrompt,
                    ConversationHistory = _messages.ToList(),
                    Stream = true
                };

                // Send streaming request
                await _provider.StreamRequestAsync(
                    request,
                    onChunkReceived,
                    response =>
                    {
                        // Add assistant response to history
                        if (response.Success)
                        {
                            var assistantMsg = new ChatMessage
                            {
                                Role = "assistant",
                                Content = response.Content,
                                Timestamp = DateTime.UtcNow
                            };
                            _messages.Add(assistantMsg);
                        }
                        onComplete?.Invoke(response);
                    },
                    onError,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        public Task StreamMessageAsync(
            string userMessage,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            return SendMessageStreamingAsync(userMessage, onChunkReceived, onComplete, onError, cancellationToken);
        }

        public void AddMessage(ChatMessage message)
        {
            if (message != null)
            {
                _messages.Add(message);
            }
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public void ClearHistory()
        {
            Clear();
        }

        public int EstimateTotalTokens()
        {
            int total = 0;

            if (!string.IsNullOrEmpty(SystemPrompt))
            {
                total += _provider.EstimateTokenCount(SystemPrompt);
            }

            foreach (var message in _messages)
            {
                total += _provider.EstimateTokenCount(message.Content);
            }

            return total;
        }

        /// <summary>L6: spec requires a hard cap of the last 6 turns, enforced
        /// alongside the token-based trim below so history never grows unbounded
        /// even when individual messages are short enough to stay under budget.</summary>
        private const int MaxHistoryMessages = 12; // 6 turns x (user + assistant)

        public void TrimToTokenLimit(int maxTokens)
        {
            while (_messages.Count > MaxHistoryMessages)
                _messages.RemoveAt(0);

            while (_messages.Count > 1 && EstimateTotalTokens() > maxTokens)
            {
                // Remove oldest user-assistant pair
                _messages.RemoveAt(0);
            }
        }

        public void SetSystemPrompt(string systemPrompt)
        {
            SystemPrompt = systemPrompt;
        }
    }
}
