using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Models;

namespace Cad3PLogBrowser.AI.Providers.Ollama
{
    /// <summary>
    /// Represents a multi-turn conversation with Ollama.
    /// Maintains conversation history for context-aware responses.
    /// </summary>
    public class OllamaConversation : IAIConversation
    {
        private readonly OllamaProvider _provider;
        private readonly List<ChatMessage> _messages;
        private string _systemPrompt;

        public string ConversationId { get; }
        public DateTime CreatedAt { get; }
        public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

        public string SystemPrompt
        {
            get => _systemPrompt;
            set => _systemPrompt = value;
        }

        public Dictionary<string, object> Metadata { get; }

        public OllamaConversation(OllamaProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _messages = new List<ChatMessage>();
            ConversationId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }

        public void AddMessage(ChatMessage message)
        {
            if (message != null)
                _messages.Add(message);
        }

        public void Clear()
        {
            _messages.Clear();
        }

        /// <summary>
        /// L2/L6: history budget passed to <see cref="TrimToTokenLimit"/> before every
        /// send. Reserves half the model's context window for prior turns, leaving room
        /// for the system prompt, the new message, and the response.
        /// </summary>
        private int MaxHistoryTokens => _provider.MaxContextTokens / 2;

        public async Task<IAIResponse> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            var userMsg = new ChatMessage
            {
                Role = "user",
                Content = userMessage
            };
            _messages.Add(userMsg);

            // L2/L6: cap history growth instead of letting it grow unbounded for the
            // life of the conversation.
            TrimToTokenLimit(MaxHistoryTokens);

            var request = new AIRequest
            {
                SystemPrompt = _systemPrompt,
                Prompt = BuildConversationPrompt(),
                Temperature = 0.7,
                MaxTokens = 4096
            };

            var response = await _provider.SendRequestAsync(request, cancellationToken);

            if (response.Success)
            {
                var assistantMsg = new ChatMessage
                {
                    Role = "assistant",
                    Content = response.Content
                };
                _messages.Add(assistantMsg);
            }

            return response;
        }

        public Task StreamMessageAsync(
            string userMessage,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var userMsg = new ChatMessage
                    {
                        Role = "user",
                        Content = userMessage
                    };
                    _messages.Add(userMsg);

                    // L2/L6: cap history growth instead of letting it grow unbounded for
                    // the life of the conversation.
                    TrimToTokenLimit(MaxHistoryTokens);

                    var request = new AIRequest
                    {
                        SystemPrompt = _systemPrompt,
                        Prompt = BuildConversationPrompt(),
                        Temperature = 0.7,
                        MaxTokens = 4096
                    };

                    var response = await _provider.SendStreamingRequestAsync(request, onChunkReceived, cancellationToken);

                    if (response.Success)
                    {
                        var assistantMsg = new ChatMessage
                        {
                            Role = "assistant",
                            Content = response.Content
                        };
                        _messages.Add(assistantMsg);
                    }

                    onComplete?.Invoke(response);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }, cancellationToken);
        }

        public int EstimateTotalTokens()
        {
            int total = 0;
            if (!string.IsNullOrEmpty(_systemPrompt))
                total += _provider.EstimateTokenCount(_systemPrompt);

            foreach (var msg in _messages)
            {
                total += _provider.EstimateTokenCount(msg.Content);
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

            while (EstimateTotalTokens() > maxTokens && _messages.Count > 1)
            {
                _messages.RemoveAt(0); // Remove oldest messages
            }
        }

        private string BuildConversationPrompt()
        {
            // For Ollama, we'll format the conversation history as a single prompt
            // The provider will handle converting this to the messages format
            if (_messages.Count == 0)
                return string.Empty;

            // Return just the last user message since we're using the messages API
            var lastUserMessage = _messages.LastOrDefault(m => m.Role == "user");
            return lastUserMessage?.Content ?? string.Empty;
        }
    }
}
