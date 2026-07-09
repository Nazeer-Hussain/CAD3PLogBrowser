using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Services
{
    /// <summary>
    /// Stores conversations as JSON files in the application data folder.
    /// </summary>
    public class FileConversationStorage : IConversationStorage
    {
        private readonly string _storageFolder;

        public FileConversationStorage()
        {
            _storageFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "Conversations");

            Directory.CreateDirectory(_storageFolder);
        }

        public Task SaveConversationAsync(string conversationId, List<ChatMessage> messages, 
            Dictionary<string, object> metadata = null)
        {
            try
            {
                string filePath = GetConversationFilePath(conversationId);

                // Simple JSON generation for conversation data
                var sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine($"  \"ConversationId\": \"{EscapeJson(conversationId)}\",");
                sb.AppendLine($"  \"Timestamp\": \"{DateTime.UtcNow:O}\",");
                sb.AppendLine("  \"Messages\": [");

                for (int i = 0; i < messages.Count; i++)
                {
                    var msg = messages[i];
                    sb.AppendLine("    {");
                    sb.AppendLine($"      \"Role\": \"{EscapeJson(msg.Role)}\",");
                    sb.AppendLine($"      \"Content\": \"{EscapeJson(msg.Content)}\",");
                    sb.AppendLine($"      \"Timestamp\": \"{msg.Timestamp:O}\"");
                    sb.AppendLine(i < messages.Count - 1 ? "    }," : "    }");
                }

                sb.AppendLine("  ]");
                sb.AppendLine("}");

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        public Task<List<ChatMessage>> LoadConversationAsync(string conversationId)
        {
            try
            {
                string filePath = GetConversationFilePath(conversationId);

                if (!File.Exists(filePath))
                    return Task.FromResult(new List<ChatMessage>());

                string json = File.ReadAllText(filePath, Encoding.UTF8);

                // Simple JSON parsing for messages
                var messages = new List<ChatMessage>();
                var lines = json.Split('\n');

                ChatMessage currentMsg = null;
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("\"Role\":"))
                    {
                        currentMsg = new ChatMessage();
                        currentMsg.Role = ExtractJsonValue(trimmed);
                    }
                    else if (trimmed.StartsWith("\"Content\":") && currentMsg != null)
                    {
                        currentMsg.Content = ExtractJsonValue(trimmed);
                    }
                    else if (trimmed.StartsWith("\"Timestamp\":") && currentMsg != null)
                    {
                        var timestampStr = ExtractJsonValue(trimmed);
                        if (DateTime.TryParse(timestampStr, out DateTime timestamp))
                            currentMsg.Timestamp = timestamp;
                        messages.Add(currentMsg);
                        currentMsg = null;
                    }
                }

                return Task.FromResult(messages);
            }
            catch (Exception ex)
            {
                return Task.FromException<List<ChatMessage>>(ex);
            }
        }

        public Task<List<ConversationMetadata>> ListConversationsAsync()
        {
            try
            {
                var conversations = new List<ConversationMetadata>();

                if (!Directory.Exists(_storageFolder))
                    return Task.FromResult(conversations);

                var files = Directory.GetFiles(_storageFolder, "*.json")
                    .OrderByDescending(f => File.GetLastWriteTimeUtc(f));

                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file, Encoding.UTF8);

                        var metadata = new ConversationMetadata
                        {
                            ConversationId = Path.GetFileNameWithoutExtension(file),
                            CreatedAt = File.GetCreationTimeUtc(file),
                            LastModified = File.GetLastWriteTimeUtc(file),
                            Title = "Conversation",
                            MessageCount = CountMessages(json)
                        };

                        conversations.Add(metadata);
                    }
                    catch
                    {
                        // Skip malformed conversation files
                    }
                }

                return Task.FromResult(conversations);
            }
            catch (Exception ex)
            {
                return Task.FromException<List<ConversationMetadata>>(ex);
            }
        }

        public Task DeleteConversationAsync(string conversationId)
        {
            try
            {
                string filePath = GetConversationFilePath(conversationId);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        public Task ClearAllAsync()
        {
            try
            {
                if (Directory.Exists(_storageFolder))
                {
                    foreach (var file in Directory.GetFiles(_storageFolder, "*.json"))
                    {
                        File.Delete(file);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        private string GetConversationFilePath(string conversationId)
        {
            // Sanitize conversation ID for use as filename
            string safeId = string.Join("_", conversationId.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_storageFolder, $"{safeId}.json");
        }

        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private string ExtractJsonValue(string line)
        {
            // Extract value from: "Key": "Value",
            int colonIndex = line.IndexOf(':');
            if (colonIndex < 0) return "";

            var value = line.Substring(colonIndex + 1).Trim();

            // Remove quotes and comma
            value = value.Trim('"', ',', ' ');

            // Unescape
            return value
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }

        private int CountMessages(string json)
        {
            int count = 0;
            int index = 0;
            while ((index = json.IndexOf("\"Role\":", index)) >= 0)
            {
                count++;
                index += 7;
            }
            return count;
        }
    }
}
