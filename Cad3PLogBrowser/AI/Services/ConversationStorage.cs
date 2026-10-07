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

                var ser = new DataContractJsonSerializer(typeof(List<ChatMessage>));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, messages ?? new List<ChatMessage>());
                    File.WriteAllBytes(filePath, ms.ToArray());
                }

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

                var bytes = File.ReadAllBytes(filePath);
                var ser = new DataContractJsonSerializer(typeof(List<ChatMessage>));
                using (var ms = new MemoryStream(bytes))
                {
                    var messages = (List<ChatMessage>)ser.ReadObject(ms);
                    return Task.FromResult(messages ?? new List<ChatMessage>());
                }
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
