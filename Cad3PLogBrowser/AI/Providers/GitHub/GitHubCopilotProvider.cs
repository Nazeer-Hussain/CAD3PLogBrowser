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

namespace Cad3PLogBrowser.AI.Providers.GitHub
{
    /// <summary>
    /// GitHub Copilot provider for enterprise customers.
    /// Uses GitHub Copilot Chat API for AI-powered log analysis.
    /// Requires GitHub Copilot Business or Enterprise license.
    /// </summary>
    public class GitHubCopilotProvider : IAIProvider
    {
        private readonly string _apiToken;
        private readonly string _apiEndpoint;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public string ProviderName => "GitHub Copilot";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiToken);

        public IAIAuthentication Authentication => null; // GitHub uses Bearer token authentication

        public int MaxContextTokens => 128000; // GPT-4 Turbo context window

        public int MaxOutputTokens => 4096;

        public GitHubCopilotProvider(string apiToken, string model = "gpt-4", string apiEndpoint = null)
        {
            if (string.IsNullOrWhiteSpace(apiToken))
                throw new ArgumentException("GitHub API token is required", nameof(apiToken));

            _apiToken = apiToken;
            _model = model;
            // Use GitHub's Copilot Chat API endpoint
            // For PTC internal GitHub: https://api.github.ptc.com/copilot/chat/completions
            // For public GitHub: https://api.github.com/copilot/chat/completions
            _apiEndpoint = apiEndpoint ?? "https://api.github.com/copilot/chat/completions";

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CAD3PLogBrowser/1.0");
        }

        public async Task<IAIResponse> SendRequestAsync(IAIRequest request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request);
                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiEndpoint)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync();

                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    return AIResponse.CreateError(
                        $"GitHub Copilot API error: {response.StatusCode} - {TryParseError(responseText)}");
                }

                return ParseResponse(responseText, stopwatch.Elapsed);
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
                var payload = BuildRequestPayload(request);
                payload["stream"] = true;

                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiEndpoint)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return AIResponse.CreateError(
                        $"GitHub Copilot API error: {response.StatusCode} - {TryParseError(errorText)}");
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

                        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                            continue;

                        var data = line.Substring(6).Trim();

                        if (data == "[DONE]")
                            break;

                        try
                        {
                            var parsed = JsonHelper.ParseSimpleJson(data);

                            // Extract content from choices[0].delta.content
                            var deltaContent = ExtractNestedValue(data, "choices", "0", "delta", "content");
                            if (!string.IsNullOrEmpty(deltaContent))
                            {
                                contentBuilder.Append(deltaContent);
                                onChunkReceived?.Invoke(deltaContent);
                            }

                            // Extract usage information
                            var usagePromptTokens = ExtractNestedValue(data, "usage", "prompt_tokens");
                            var usageCompletionTokens = ExtractNestedValue(data, "usage", "completion_tokens");

                            if (!string.IsNullOrEmpty(usagePromptTokens))
                                int.TryParse(usagePromptTokens, out int pt);
                            if (!string.IsNullOrEmpty(usageCompletionTokens))
                                int.TryParse(usageCompletionTokens, out int ct);
                        }
                        catch
                        {
                            // Skip malformed SSE events
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
            catch (TaskCanceledException)
            {
                return AIResponse.CreateError("Request was cancelled");
            }
            catch (Exception ex)
            {
                return AIResponse.CreateError($"Streaming request failed: {ex.Message}");
            }
        }

        public Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Test with a simple request
                    var testRequest = new AIRequest
                    {
                        Prompt = "Say 'Hello from GitHub Copilot!'",
                        MaxTokens = 50
                    };

                    var response = await SendRequestAsync(testRequest, cancellationToken);

                    if (response.Success)
                    {
                        return null; // Success
                    }
                    else
                    {
                        return response.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    return $"Connection test error: {ex.Message}";
                }
            }, cancellationToken);
        }

        private Dictionary<string, object> BuildRequestPayload(IAIRequest request)
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

            // Add conversation history (messages from IAIRequest don't exist, skip for now)
            // GitHub Copilot will be used for single-shot requests

            // Add current prompt
            messages.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = request.Prompt
            });

            var payload = new Dictionary<string, object>
            {
                ["model"] = string.IsNullOrEmpty(request.Model) ? _model : request.Model,
                ["messages"] = messages,
                ["temperature"] = request.Temperature > 0 ? request.Temperature : 0.7,
                ["max_tokens"] = request.MaxTokens > 0 ? request.MaxTokens : 4096
            };

            return payload;
        }

        private IAIResponse ParseResponse(string responseText, TimeSpan elapsed)
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
                    // Handle array index
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
                        // Handle object key
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

        public IAIConversation CreateConversation()
        {
            return new GitHubConversation(this);
        }

        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Rough estimation: ~4 characters per token (GPT-4 tokenization)
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
