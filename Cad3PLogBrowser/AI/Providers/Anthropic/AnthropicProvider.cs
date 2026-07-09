using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Utilities;

namespace Cad3PLogBrowser.AI.Providers.Anthropic
{
    /// <summary>
    /// Anthropic Claude AI provider implementation.
    /// Supports Claude 3.x models including Opus, Sonnet, and Haiku.
    /// </summary>
    public class AnthropicProvider : IAIProvider
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string ApiVersion = "2023-06-01";

        public AnthropicProvider(string apiKey, string model = "claude-3-5-sonnet-20241022")
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120)
            };

            Authentication = new ApiKeyAuthentication(_apiKey);
        }

        public string ProviderName => "Anthropic Claude";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public IAIAuthentication Authentication { get; }

        public int MaxContextTokens => 200000; // Claude 3 supports 200k context

        public int MaxOutputTokens => 4096;

        public async Task<IAIResponse> SendRequestAsync(IAIRequest request, 
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                return AIResponse.CreateError("Anthropic API key not configured");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request);
                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Content = content
                };

                httpRequest.Headers.Add("x-api-key", _apiKey);
                httpRequest.Headers.Add("anthropic-version", ApiVersion);

                var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                stopwatch.Stop();

                var responseText = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var error = TryParseError(responseText);
                    return AIResponse.CreateError($"API error: {error}");
                }

                return ParseResponse(responseText, stopwatch.Elapsed);
            }
            catch (TaskCanceledException)
            {
                return AIResponse.CreateError("Request timed out");
            }
            catch (Exception ex)
            {
                return AIResponse.CreateError($"Request failed: {ex.Message}");
            }
        }

        public async Task StreamRequestAsync(IAIRequest request,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
            {
                onError?.Invoke(new InvalidOperationException("Anthropic API key not configured"));
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request);
                payload["stream"] = true;

                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Content = content
                };

                httpRequest.Headers.Add("x-api-key", _apiKey);
                httpRequest.Headers.Add("anthropic-version", ApiVersion);

                var httpResponse = await _httpClient.SendAsync(httpRequest, 
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorText = await httpResponse.Content.ReadAsStringAsync();
                    var error = TryParseError(errorText);
                    onError?.Invoke(new Exception($"API error: {error}"));
                    return;
                }

                var contentBuilder = new StringBuilder();
                int? promptTokens = null;
                int? completionTokens = null;

                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                            continue;

                        string data = line.Substring(6); // Remove "data: " prefix

                        if (data == "[DONE]")
                            break;

                        try
                        {
                            var parsed = JsonHelper.ParseSimpleJson(data);
                            var type = parsed.ContainsKey("type") ? parsed["type"] : null;

                            if (type == "content_block_delta")
                            {
                                // Extract nested text from delta.text
                                var deltaText = ExtractNestedValue(data, "delta", "text");
                                if (!string.IsNullOrEmpty(deltaText))
                                {
                                    contentBuilder.Append(deltaText);
                                    onChunkReceived?.Invoke(deltaText);
                                }
                            }
                            else if (type == "message_start")
                            {
                                var inputTokens = ExtractNestedValue(data, "message", "usage", "input_tokens");
                                if (!string.IsNullOrEmpty(inputTokens))
                                    int.TryParse(inputTokens, out int tokens);
                            }
                            else if (type == "message_delta")
                            {
                                var outputTokens = ExtractNestedValue(data, "usage", "output_tokens");
                                if (!string.IsNullOrEmpty(outputTokens))
                                    int.TryParse(outputTokens, out int tokens);
                            }
                        }
                        catch
                        {
                            // Skip malformed SSE events
                        }
                    }
                }

                stopwatch.Stop();

                var response = new AIResponse
                {
                    Success = true,
                    Content = contentBuilder.ToString(),
                    Model = _model,
                    ElapsedTime = stopwatch.Elapsed,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0)
                };

                onComplete?.Invoke(response);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        public IAIConversation CreateConversation()
        {
            return new AnthropicConversation(this);
        }

        public async Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var testRequest = new AIRequest
                {
                    Prompt = "Hello",
                    SystemPrompt = "You are a helpful assistant. Respond with just 'OK'.",
                    Model = _model,
                    MaxTokens = 10
                };

                var response = await SendRequestAsync(testRequest, cancellationToken);

                if (!response.Success)
                    return response.ErrorMessage;

                return null; // Success
            }
            catch (Exception ex)
            {
                return $"Connection test failed: {ex.Message}";
            }
        }

        public int EstimateTokenCount(string text)
        {
            // Claude uses a similar tokenization to GPT models
            // Rough approximation: 1 token ? 4 characters
            return text.Length / 4;
        }

        private Dictionary<string, object> BuildRequestPayload(IAIRequest request)
        {
            var messages = new List<Dictionary<string, string>>();

            // Add conversation history
            if (request.ConversationHistory != null)
            {
                foreach (var msg in request.ConversationHistory)
                {
                    if (msg.Role != "system") // System prompt goes separately
                    {
                        messages.Add(new Dictionary<string, string>
                        {
                            ["role"] = msg.Role,
                            ["content"] = msg.Content
                        });
                    }
                }
            }

            // Add current user message
            messages.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = request.Prompt
            });

            var payload = new Dictionary<string, object>
            {
                ["model"] = request.Model ?? _model,
                ["messages"] = messages,
                ["max_tokens"] = request.MaxTokens,
                ["temperature"] = request.Temperature
            };

            // Add system prompt if provided
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                payload["system"] = request.SystemPrompt;
            }

            return payload;
        }

        private AIResponse ParseResponse(string responseText, TimeSpan elapsed)
        {
            try
            {
                var parsed = JsonHelper.ParseSimpleJson(responseText);

                var content = ExtractNestedValue(responseText, "content", "0", "text");
                var model = parsed.ContainsKey("model") ? parsed["model"] : _model;

                var inputTokens = ExtractNestedValue(responseText, "usage", "input_tokens");
                var outputTokens = ExtractNestedValue(responseText, "usage", "output_tokens");

                int? promptTokens = null;
                int? completionTokens = null;

                if (!string.IsNullOrEmpty(inputTokens))
                    promptTokens = int.Parse(inputTokens);
                if (!string.IsNullOrEmpty(outputTokens))
                    completionTokens = int.Parse(outputTokens);

                return new AIResponse
                {
                    Success = true,
                    Content = content ?? string.Empty,
                    Model = model,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0),
                    ElapsedTime = elapsed,
                    FinishReason = parsed.ContainsKey("stop_reason") ? parsed["stop_reason"] : null
                };
            }
            catch (Exception ex)
            {
                return AIResponse.CreateError($"Failed to parse response: {ex.Message}");
            }
        }

        private string TryParseError(string responseText)
        {
            try
            {
                var errorMessage = ExtractNestedValue(responseText, "error", "message");
                return errorMessage ?? responseText;
            }
            catch
            {
                return responseText;
            }
        }

        private string ExtractNestedValue(string json, params string[] path)
        {
            if (string.IsNullOrEmpty(json) || path == null || path.Length == 0)
                return null;

            try
            {
                string current = json;
                foreach (var key in path)
                {
                    // Handle array index
                    if (int.TryParse(key, out int index))
                    {
                        // Find array and extract element at index
                        int arrayStart = current.IndexOf('[');
                        if (arrayStart < 0) return null;

                        int arrayEnd = FindMatchingBracket(current, arrayStart);
                        if (arrayEnd < 0) return null;

                        var arrayContent = current.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
                        var elements = SplitArrayElements(arrayContent);

                        if (index >= 0 && index < elements.Count)
                            current = elements[index];
                        else
                            return null;
                    }
                    else
                    {
                        // Handle object key
                        var searchKey = $"\"{key}\":";
                        int keyIndex = current.IndexOf(searchKey);
                        if (keyIndex < 0) return null;

                        int valueStart = keyIndex + searchKey.Length;
                        current = ExtractValue(current, valueStart);
                        if (current == null) return null;
                    }
                }

                // Clean up the final value
                return current.Trim().Trim('"');
            }
            catch
            {
                return null;
            }
        }

        private string ExtractValue(string json, int startIndex)
        {
            // Skip whitespace
            while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
                startIndex++;

            if (startIndex >= json.Length)
                return null;

            char firstChar = json[startIndex];

            // String value
            if (firstChar == '"')
            {
                int endIndex = startIndex + 1;
                while (endIndex < json.Length)
                {
                    if (json[endIndex] == '"' && json[endIndex - 1] != '\\')
                        return json.Substring(startIndex + 1, endIndex - startIndex - 1);
                    endIndex++;
                }
                return null;
            }

            // Object value
            if (firstChar == '{')
            {
                int endIndex = FindMatchingBrace(json, startIndex);
                if (endIndex < 0) return null;
                return json.Substring(startIndex, endIndex - startIndex + 1);
            }

            // Array value
            if (firstChar == '[')
            {
                int endIndex = FindMatchingBracket(json, startIndex);
                if (endIndex < 0) return null;
                return json.Substring(startIndex, endIndex - startIndex + 1);
            }

            // Number, boolean, or null
            int commaOrBrace = json.IndexOfAny(new[] { ',', '}', ']' }, startIndex);
            if (commaOrBrace < 0)
                return json.Substring(startIndex).Trim();
            return json.Substring(startIndex, commaOrBrace - startIndex).Trim();
        }

        private int FindMatchingBrace(string text, int openIndex)
        {
            int depth = 0;
            for (int i = openIndex; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private int FindMatchingBracket(string text, int openIndex)
        {
            int depth = 0;
            for (int i = openIndex; i < text.Length; i++)
            {
                if (text[i] == '[') depth++;
                if (text[i] == ']')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private List<string> SplitArrayElements(string arrayContent)
        {
            var elements = new List<string>();
            var current = new StringBuilder();
            int depth = 0;
            bool inString = false;
            bool escaped = false;

            foreach (char c in arrayContent)
            {
                if (escaped)
                {
                    current.Append(c);
                    escaped = false;
                    continue;
                }

                if (c == '\\' && inString)
                {
                    escaped = true;
                    current.Append(c);
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    current.Append(c);
                    continue;
                }

                if (!inString)
                {
                    if (c == '{' || c == '[') depth++;
                    if (c == '}' || c == ']') depth--;

                    if (c == ',' && depth == 0)
                    {
                        elements.Add(current.ToString().Trim());
                        current.Clear();
                        continue;
                    }
                }

                current.Append(c);
            }

            if (current.Length > 0)
                elements.Add(current.ToString().Trim());

            return elements;
        }
    }
}
