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

namespace Cad3PLogBrowser.AI.Providers.Google
{
    /// <summary>
    /// Google Gemini provider using the Generative Language API.
    /// API documentation: https://ai.google.dev/api/generate-content
    /// </summary>
    public class GoogleGeminiProvider : IAIProvider
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

        public string ProviderName => "Google Gemini";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public IAIAuthentication Authentication { get; }

        public int MaxContextTokens => 1000000; // Gemini 1.5 Pro context window

        public int MaxOutputTokens => 8192;

        /// <param name="apiKey">Your Google AI Studio / Gemini API key.</param>
        /// <param name="model">Model name (e.g., "gemini-pro", "gemini-1.5-pro", "gemini-1.5-flash").</param>
        public GoogleGeminiProvider(string apiKey, string model = "gemini-pro")
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("Google API key is required", nameof(apiKey));

            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gemini-pro" : model;

            Authentication = new ApiKeyAuthentication(_apiKey);

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string BuildUrl(bool stream = false)
        {
            string action = stream ? "streamGenerateContent" : "generateContent";
            return $"{ApiBaseUrl}/{_model}:{action}?key={_apiKey}";
        }

        public async Task<IAIResponse> SendRequestAsync(IAIRequest request, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                return AIResponse.CreateError("Google API key not configured");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request);
                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl()) { Content = content };
                var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseText = await httpResponse.Content.ReadAsStringAsync();

                stopwatch.Stop();

                if (!httpResponse.IsSuccessStatusCode)
                    return AIResponse.CreateError($"Google Gemini API error: {httpResponse.StatusCode} - {TryParseError(responseText)}");

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
                onError?.Invoke(new InvalidOperationException("Google API key not configured"));
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var payload = BuildRequestPayload(request);

                var content = new StringContent(
                    JsonHelper.SerializeDictionary(payload),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl(stream: true)) { Content = content };

                var httpResponse = await _httpClient.SendAsync(httpRequest,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorText = await httpResponse.Content.ReadAsStringAsync();
                    onError?.Invoke(new Exception($"Google Gemini API error: {httpResponse.StatusCode} - {TryParseError(errorText)}"));
                    return;
                }

                // The streaming endpoint returns a JSON array of response chunks. Since we
                // read incrementally, buffer the full text and extract "text" fields as we go.
                var contentBuilder = new StringBuilder();
                var rawBuilder = new StringBuilder();
                int lastExtractedLength = 0;

                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    char[] buffer = new char[4096];
                    int read;
                    while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        rawBuilder.Append(buffer, 0, read);

                        // Try to extract any new complete "text" fields available so far.
                        string rawSoFar = rawBuilder.ToString();
                        var texts = ExtractAllTextFields(rawSoFar);
                        if (texts.Count > lastExtractedLength)
                        {
                            for (int i = lastExtractedLength; i < texts.Count; i++)
                            {
                                contentBuilder.Append(texts[i]);
                                onChunkReceived?.Invoke(texts[i]);
                            }
                            lastExtractedLength = texts.Count;
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
            return new GoogleGeminiConversation(this);
        }

        public async Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var testRequest = new AIRequest
                {
                    Prompt = "Hello",
                    SystemPrompt = "You are a helpful assistant. Respond with just 'OK'.",
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

            return (int)Math.Ceiling(text.Length / 4.0);
        }

        private Dictionary<string, object> BuildRequestPayload(IAIRequest request)
        {
            var contents = new List<Dictionary<string, object>>();

            if (request.ConversationHistory != null)
            {
                foreach (var msg in request.ConversationHistory)
                {
                    if (msg.Role == "system")
                        continue;

                    // Gemini uses "user" and "model" roles (no "assistant").
                    string role = msg.Role == "assistant" ? "model" : "user";

                    contents.Add(new Dictionary<string, object>
                    {
                        ["role"] = role,
                        ["parts"] = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> { ["text"] = msg.Content }
                        }
                    });
                }
            }

            contents.Add(new Dictionary<string, object>
            {
                ["role"] = "user",
                ["parts"] = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string> { ["text"] = request.Prompt }
                }
            });

            var payload = new Dictionary<string, object>
            {
                ["contents"] = contents,
                ["generationConfig"] = new Dictionary<string, object>
                {
                    ["temperature"] = request.Temperature > 0 ? request.Temperature : 0.7,
                    ["maxOutputTokens"] = request.MaxTokens > 0 ? request.MaxTokens : 4096
                }
            };

            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                payload["systemInstruction"] = new Dictionary<string, object>
                {
                    ["parts"] = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string> { ["text"] = request.SystemPrompt }
                    }
                };
            }

            return payload;
        }

        private AIResponse ParseResponse(string responseText, TimeSpan elapsed)
        {
            try
            {
                var content = ExtractNestedValue(responseText, "candidates", "0", "content", "parts", "0", "text");

                var promptTokensStr = ExtractNestedValue(responseText, "usageMetadata", "promptTokenCount");
                var completionTokensStr = ExtractNestedValue(responseText, "usageMetadata", "candidatesTokenCount");

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
                    Model = _model,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0),
                    ElapsedTime = elapsed,
                    FinishReason = ExtractNestedValue(responseText, "candidates", "0", "finishReason")
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

        /// <summary>
        /// Extracts every "text":"..." field value found in a raw (possibly partial) JSON
        /// array response, used for incremental streaming parsing.
        /// </summary>
        private List<string> ExtractAllTextFields(string json)
        {
            var results = new List<string>();
            if (string.IsNullOrEmpty(json))
                return results;

            const string key = "\"text\":";
            int searchStart = 0;
            while (true)
            {
                int keyIndex = json.IndexOf(key, searchStart);
                if (keyIndex < 0) break;

                int valueStart = keyIndex + key.Length;
                var value = ExtractValue(json, valueStart);
                if (value == null) break;

                results.Add(UnescapeJsonString(value));
                searchStart = valueStart + value.Length;
            }

            return results;
        }

        private string UnescapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
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
    }
}
