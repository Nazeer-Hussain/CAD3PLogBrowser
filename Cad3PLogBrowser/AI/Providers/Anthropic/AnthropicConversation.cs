using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Providers.Anthropic
{
    /// <summary>
    /// Manages a conversation with Anthropic Claude.
    /// </summary>
    public class AnthropicConversation : IAIConversation
    {
        private readonly IAIProvider _provider;
        private readonly List<ChatMessage> _messages;
        private readonly Dictionary<string, object> _metadata;

        public AnthropicConversation(IAIProvider provider)
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

            // Build request
            var request = new AIRequest
            {
                Prompt = userMessage,
                SystemPrompt = SystemPrompt,
                ConversationHistory = _messages.Take(_messages.Count - 1).ToList(), // Exclude current message
                Model = null // Use provider's default
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
                    Timestamp = DateTime.UtcNow,
                    TokenCount = response.CompletionTokens
                };
                _messages.Add(assistantMsg);
            }

            return response;
        }

        public async Task StreamMessageAsync(string userMessage,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                onError?.Invoke(new ArgumentException("User message cannot be empty"));
                return;
            }

            // Add user message to history
            var userMsg = new ChatMessage
            {
                Role = "user",
                Content = userMessage,
                Timestamp = DateTime.UtcNow
            };
            _messages.Add(userMsg);

            // Build request
            var request = new AIRequest
            {
                Prompt = userMessage,
                SystemPrompt = SystemPrompt,
                ConversationHistory = _messages.Take(_messages.Count - 1).ToList(),
                Model = null,
                Stream = true
            };

            // Send streaming request
            await _provider.StreamRequestAsync(request,
                onChunkReceived,
                response =>
                {
                    if (response.Success)
                    {
                        var assistantMsg = new ChatMessage
                        {
                            Role = "assistant",
                            Content = response.Content,
                            Timestamp = DateTime.UtcNow,
                            TokenCount = response.CompletionTokens
                        };
                        _messages.Add(assistantMsg);
                    }
                    onComplete?.Invoke(response);
                },
                onError,
                cancellationToken);
        }

        public void AddMessage(ChatMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _messages.Add(message);
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public int EstimateTotalTokens()
        {
            int total = 0;

            // System prompt
            if (!string.IsNullOrEmpty(SystemPrompt))
            {
                total += _provider.EstimateTokenCount(SystemPrompt);
            }

            // All messages
            foreach (var msg in _messages)
            {
                if (msg.TokenCount.HasValue)
                {
                    total += msg.TokenCount.Value;
                }
                else
                {
                    total += _provider.EstimateTokenCount(msg.Content);
                }
            }

            return total;
        }

        public void TrimToTokenLimit(int maxTokens)
        {
            int currentTokens = EstimateTotalTokens();

            if (currentTokens <= maxTokens)
                return;

            // Remove oldest messages (keep system prompt)
            while (_messages.Count > 0 && EstimateTotalTokens() > maxTokens)
            {
                // Remove the oldest pair (user + assistant)
                if (_messages.Count >= 2)
                {
                    _messages.RemoveAt(0);
                    _messages.RemoveAt(0);
                }
                else
                {
                    _messages.RemoveAt(0);
                }
            }
        }
    }
}
