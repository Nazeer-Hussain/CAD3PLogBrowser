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

        public async Task<IAIResponse> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            var userMsg = new ChatMessage
            {
                Role = "user",
                Content = userMessage
            };
            _messages.Add(userMsg);

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

        public void TrimToTokenLimit(int maxTokens)
        {
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
