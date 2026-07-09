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

namespace Cad3PLogBrowser.AI.Providers.Ollama
{
    /// <summary>
    /// Ollama provider for self-hosted LLM inference.
    /// Ollama provides a simple API to run open-source LLMs locally or on a corporate server.
    /// Supports models like Llama 3, Mistral, CodeLlama, and many others.
    /// Free, open-source, and runs entirely on your infrastructure.
    /// API documentation: https://github.com/ollama/ollama/blob/main/docs/api.md
    /// </summary>
    public class OllamaProvider : IAIProvider
    {
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public string ProviderName => "Ollama";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_baseUrl);

        public IAIAuthentication Authentication => null; // Ollama doesn't require authentication by default

        public int MaxContextTokens => 8192; // Default, varies by model

        public int MaxOutputTokens => 4096;

        /// <summary>
        /// Creates a new Ollama provider instance.
        /// </summary>
        /// <param name="baseUrl">Base URL of your Ollama server (e.g., "http://localhost:11434" or "http://your-server:11434")</param>
        /// <param name="model">Model name (e.g., "llama3", "mistral", "codellama", "phi3")</param>
        public OllamaProvider(string baseUrl = "http://localhost:11434", string model = "llama3")
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Ollama base URL is required", nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model name is required", nameof(model));

            _baseUrl = baseUrl.TrimEnd('/');
            _model = model;

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5) // Longer timeout for local inference
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IAIResponse> SendRequestAsync(IAIRequest request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request, false);
                var jsonPayload = JsonHelper.SerializeDictionary(payload);

                // Debug: Log the payload
                System.Diagnostics.Debug.WriteLine($"Ollama Request Payload: {jsonPayload}");

                var content = new StringContent(
                    jsonPayload,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/api/chat",
                    content,
                    cancellationToken);

                var responseText = await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    return AIResponse.CreateError(
                        $"Ollama API error: {response.StatusCode} - {TryParseError(responseText)}");
                }

                return ParseResponse(responseText, stopwatch.Elapsed);
            }
            catch (HttpRequestException ex)
            {
                return AIResponse.CreateError(
                    $"Cannot connect to Ollama server at {_baseUrl}. Make sure Ollama is running. Error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return AIResponse.CreateError("Request was cancelled");
            }
            catch (Exception ex)
            {
                return AIResponse.CreateError($"Request failed: {ex.Message}");
            }
        }

        public async Task<IAIResponse> SendStreamingRequestAsync(
            IAIRequest request,
            Action<string> onChunkReceived,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request, true);
                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/chat")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(
                    httpRequestMessage,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return AIResponse.CreateError(
                        $"Ollama API error: {response.StatusCode} - {TryParseError(errorText)}");
                }

                var contentBuilder = new StringBuilder();
                int? promptTokens = null;
                int? completionTokens = null;

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return AIResponse.CreateError("Request was cancelled");

                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        try
                        {
                            // Debug: Log the raw JSON line
                            System.Diagnostics.Debug.WriteLine($"[Ollama] Raw Line: {line}");

                            // Each line is a complete JSON object
                            var messageContent = ExtractNestedValue(line, "message", "content");
                            if (!string.IsNullOrEmpty(messageContent))
                            {
                                System.Diagnostics.Debug.WriteLine($"[Ollama] Extracted (length={messageContent.Length}): '{messageContent}'");
                                System.Diagnostics.Debug.WriteLine($"[Ollama] First 50 chars: '{(messageContent.Length > 50 ? messageContent.Substring(0, 50) : messageContent)}'");

                                contentBuilder.Append(messageContent);
                                onChunkReceived?.Invoke(messageContent);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Ollama] No content extracted from line");
                            }

                            // Check if this is the final response with token counts
                            var doneStr = ExtractNestedValue(line, "done");
                            if (doneStr == "true")
                            {
                                var promptTokensStr = ExtractNestedValue(line, "prompt_eval_count");
                                var completionTokensStr = ExtractNestedValue(line, "eval_count");

                                if (!string.IsNullOrEmpty(promptTokensStr))
                                {
                                    int pt;
                                    if (int.TryParse(promptTokensStr, out pt))
                                        promptTokens = pt;
                                }
                                if (!string.IsNullOrEmpty(completionTokensStr))
                                {
                                    int ct;
                                    if (int.TryParse(completionTokensStr, out ct))
                                        completionTokens = ct;
                                }
                            }
                        }
                        catch
                        {
                            // Skip malformed JSON lines
                        }
                    }
                }

                stopwatch.Stop();

                return new AIResponse
                {
                    Success = true,
                    Content = contentBuilder.ToString(),
                    Model = _model,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0),
                    ElapsedTime = stopwatch.Elapsed,
                    FinishReason = "stop"
                };
            }
            catch (HttpRequestException ex)
            {
                return AIResponse.CreateError(
                    $"Cannot connect to Ollama server at {_baseUrl}. Error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return AIResponse.CreateError("Request was cancelled");
            }
            catch (Exception ex)
            {
                return AIResponse.CreateError($"Streaming request failed: {ex.Message}");
            }
        }

        public Task StreamRequestAsync(
            IAIRequest request,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var response = await SendStreamingRequestAsync(
                        request,
                        onChunkReceived,
                        cancellationToken);

                    onComplete?.Invoke(response);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }, cancellationToken);
        }

        public Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // First, check if Ollama is running
                    var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags", cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        return $"Cannot connect to Ollama server at {_baseUrl}. Status: {response.StatusCode}";
                    }

                    var responseText = await response.Content.ReadAsStringAsync();

                    // Check if the specified model exists
                    // Model names can be with or without tags (e.g., "llama3" or "llama3:latest")
                    bool modelFound = responseText.Contains($"\"{_model}\"") || 
                                     responseText.Contains($"\"{_model}:") ||
                                     responseText.Contains($"\"name\":\"{_model}") ||
                                     responseText.Contains($"\"name\":\"{_model}:");

                    if (!modelFound)
                    {
                        return $"Model '{_model}' is not available on the Ollama server. Please pull it first using: ollama pull {_model}";
                    }

                    // Test with a simple request
                    var testRequest = new AIRequest
                    {
                        Prompt = "Say 'Hello from Ollama!'",
                        MaxTokens = 50
                    };

                    var testResponse = await SendRequestAsync(testRequest, cancellationToken);

                    if (testResponse.Success)
                    {
                        return null; // Success
                    }
                    else
                    {
                        return testResponse.ErrorMessage;
                    }
                }
                catch (HttpRequestException ex)
                {
                    return $"Cannot connect to Ollama at {_baseUrl}. Make sure Ollama is installed and running. Error: {ex.Message}";
                }
                catch (Exception ex)
                {
                    return $"Connection test error: {ex.Message}";
                }
            }, cancellationToken);
        }

        public IAIConversation CreateConversation()
        {
            return new OllamaConversation(this);
        }

        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Rough estimation: ~4 characters per token (similar to GPT tokenization)
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        private Dictionary<string, object> BuildRequestPayload(IAIRequest request, bool stream)
        {
            var messages = new List<object>();

            // Add system message if provided
            if (!string.IsNullOrEmpty(request.SystemPrompt))
            {
                messages.Add(new Dictionary<string, string>
                {
                    ["role"] = "system",
                    ["content"] = request.SystemPrompt
                });
            }

            // Add current prompt
            messages.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = request.Prompt
            });

            var options = new Dictionary<string, object>();

            if (request.Temperature > 0)
                options["temperature"] = request.Temperature;
            else
                options["temperature"] = 0.7;

            if (request.MaxTokens > 0)
                options["num_predict"] = request.MaxTokens;

            var payload = new Dictionary<string, object>
            {
                ["model"] = string.IsNullOrEmpty(request.Model) ? _model : request.Model,
                ["messages"] = messages,
                ["stream"] = stream,
                ["options"] = options
            };

            return payload;
        }

        private IAIResponse ParseResponse(string responseText, TimeSpan elapsed)
        {
            try
            {
                var content = ExtractNestedValue(responseText, "message", "content");
                var model = ExtractNestedValue(responseText, "model") ?? _model;

                var promptTokensStr = ExtractNestedValue(responseText, "prompt_eval_count");
                var completionTokensStr = ExtractNestedValue(responseText, "eval_count");

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
                    FinishReason = "stop"
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
                var errorMessage = ExtractNestedValue(responseText, "error");
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
                    // Handle object key
                    var searchKey = $"\"{key}\":";
                    int keyIndex = current.IndexOf(searchKey);
                    if (keyIndex < 0) return null;

                    int valueStart = keyIndex + searchKey.Length;
                    current = ExtractValue(current, valueStart);
                    if (current == null) return null;
                }

                // Unescape JSON escape sequences
                // DON'T trim the content - spaces are significant!
                // Only trim quotes if present
                string unescaped = current;
                if (unescaped.StartsWith("\"") && unescaped.EndsWith("\"") && unescaped.Length >= 2)
                {
                    unescaped = unescaped.Substring(1, unescaped.Length - 2);
                }
                return UnescapeJsonString(unescaped);
            }
            catch
            {
                return null;
            }
        }

        private string UnescapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str
                .Replace("\\\"", "\"")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\\", "\\");
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

            // Handle boolean, number, or null
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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
