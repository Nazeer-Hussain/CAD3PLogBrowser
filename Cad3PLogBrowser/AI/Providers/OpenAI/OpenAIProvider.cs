using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Utilities;

namespace Cad3PLogBrowser.AI.Providers.OpenAI
{
    /// <summary>
    /// OpenAI provider using the Chat Completions API.
    /// Supports GPT-4o, GPT-4 Turbo, GPT-3.5 Turbo, and other chat models.
    /// API documentation: https://platform.openai.com/docs/api-reference/chat
    /// </summary>
    public class OpenAIProvider : IAIProvider
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _organization;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        public string ProviderName => "OpenAI";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public IAIAuthentication Authentication { get; }

        public int MaxContextTokens => 128000; // GPT-4o / GPT-4 Turbo context window

        public int MaxOutputTokens => 4096;

        /// <param name="apiKey">Your OpenAI API key (starts with "sk-...").</param>
        /// <param name="model">Model name (e.g., "gpt-4o", "gpt-4-turbo", "gpt-3.5-turbo").</param>
        /// <param name="organization">Optional OpenAI organization ID.</param>
        public OpenAIProvider(string apiKey, string model = "gpt-4o", string organization = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("OpenAI API key is required", nameof(apiKey));

            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gpt-4o" : model;
            _organization = organization;

            Authentication = new ApiKeyAuthentication(_apiKey);

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrWhiteSpace(_organization))
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _organization);
        }

        public async Task<IAIResponse> SendRequestAsync(IAIRequest request, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                return AIResponse.CreateError("OpenAI API key not configured");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request);
                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content };
                var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseText = await httpResponse.Content.ReadAsStringAsync();

                stopwatch.Stop();

                if (!httpResponse.IsSuccessStatusCode)
                    return AIResponse.CreateError($"OpenAI API error: {httpResponse.StatusCode} - {TryParseError(responseText)}");

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
                onError?.Invoke(new InvalidOperationException("OpenAI API key not configured"));
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

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content };

                var httpResponse = await _httpClient.SendAsync(httpRequest,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorText = await httpResponse.Content.ReadAsStringAsync();
                    onError?.Invoke(new Exception($"OpenAI API error: {httpResponse.StatusCode} - {TryParseError(errorText)}"));
                    return;
                }

                var contentBuilder = new StringBuilder();

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

                        string data = line.Substring(6).Trim();
                        if (data == "[DONE]")
                            break;

                        try
                        {
                            var deltaContent = ExtractNestedValue(data, "choices", "0", "delta", "content");
                            if (!string.IsNullOrEmpty(deltaContent))
                            {
                                contentBuilder.Append(deltaContent);
                                onChunkReceived?.Invoke(deltaContent);
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
                    FinishReason = "stop"
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
            return new OpenAIConversation(this);
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
                return response.Success ? null : response.ErrorMessage;
            }
            catch (Exception ex)
            {
                return $"Connection test failed: {ex.Message}";
            }
        }

        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Rough approximation: 1 token ~ 4 characters for GPT tokenization
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        private Dictionary<string, object> BuildRequestPayload(IAIRequest request)
        {
            var messages = new List<Dictionary<string, string>>();

            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new Dictionary<string, string>
                {
                    ["role"] = "system",
                    ["content"] = request.SystemPrompt
                });
            }

            if (request.ConversationHistory != null)
            {
                foreach (var msg in request.ConversationHistory)
                {
                    if (msg.Role != "system")
                    {
                        messages.Add(new Dictionary<string, string>
                        {
                            ["role"] = msg.Role,
                            ["content"] = msg.Content
                        });
                    }
                }
            }

            messages.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = request.Prompt
            });

            return new Dictionary<string, object>
            {
                ["model"] = string.IsNullOrEmpty(request.Model) ? _model : request.Model,
                ["messages"] = messages,
                ["temperature"] = request.Temperature > 0 ? request.Temperature : 0.7,
                ["max_tokens"] = request.MaxTokens > 0 ? request.MaxTokens : 4096
            };
        }

        private AIResponse ParseResponse(string responseText, TimeSpan elapsed)
        {
            try
            {
                var content = ExtractNestedValue(responseText, "choices", "0", "message", "content");
                var model = ExtractNestedValue(responseText, "model") ?? _model;

                var promptTokensStr = ExtractNestedValue(responseText, "usage", "prompt_tokens");
                var completionTokensStr = ExtractNestedValue(responseText, "usage", "completion_tokens");

                int? promptTokens = null;
                int? completionTokens = null;

                if (!string.IsNullOrEmpty(promptTokensStr))
                    promptTokens = int.Parse(promptTokensStr);
                if (!string.IsNullOrEmpty(completionTokensStr))
                    completionTokens = int.Parse(completionTokensStr);

                return new AIResponse
                {
                    Success = true,
                    Content = content ?? string.Empty,
                    Model = model,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0),
                    ElapsedTime = elapsed,
                    FinishReason = ExtractNestedValue(responseText, "choices", "0", "finish_reason")
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
                    if (int.TryParse(key, out int index))
                    {
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
                        var searchKey = $"\"{key}\":";
                        int keyIndex = current.IndexOf(searchKey);
                        if (keyIndex < 0) return null;

                        int valueStart = keyIndex + searchKey.Length;
                        current = ExtractValue(current, valueStart);
                        if (current == null) return null;
                    }
                }

                return current.Trim().Trim('"');
            }
            catch
            {
                return null;
            }
        }

        private string ExtractValue(string json, int startIndex)
        {
            while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
                startIndex++;

            if (startIndex >= json.Length)
                return null;

            char firstChar = json[startIndex];

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

            if (firstChar == '{')
            {
                int endIndex = FindMatchingBrace(json, startIndex);
                if (endIndex < 0) return null;
                return json.Substring(startIndex, endIndex - startIndex + 1);
            }

            if (firstChar == '[')
            {
                int endIndex = FindMatchingBracket(json, startIndex);
                if (endIndex < 0) return null;
                return json.Substring(startIndex, endIndex - startIndex + 1);
            }

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
            int depth = 0;
            int start = 0;
            bool inString = false;

            for (int i = 0; i < arrayContent.Length; i++)
            {
                char c = arrayContent[i];

                if (c == '"' && (i == 0 || arrayContent[i - 1] != '\\'))
                    inString = !inString;

                if (!inString)
                {
                    if (c == '{' || c == '[') depth++;
                    if (c == '}' || c == ']') depth--;

                    if (c == ',' && depth == 0)
                    {
                        elements.Add(arrayContent.Substring(start, i - start).Trim());
                        start = i + 1;
                    }
                }
            }

            if (start < arrayContent.Length)
                elements.Add(arrayContent.Substring(start).Trim());

            return elements;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
