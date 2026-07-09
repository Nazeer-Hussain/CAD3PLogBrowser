using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Stores and retrieves conversation history.
    /// Supports persisting conversations across sessions.
    /// </summary>
    public interface IConversationStorage
    {
        /// <summary>
        /// Saves a conversation.
        /// </summary>
        Task SaveConversationAsync(string conversationId, List<ChatMessage> messages, 
            Dictionary<string, object> metadata = null);

        /// <summary>
        /// Loads a conversation by ID.
        /// </summary>
        Task<List<ChatMessage>> LoadConversationAsync(string conversationId);

        /// <summary>
        /// Lists all saved conversation IDs with their metadata.
        /// </summary>
        Task<List<ConversationMetadata>> ListConversationsAsync();

        /// <summary>
        /// Deletes a conversation.
        /// </summary>
        Task DeleteConversationAsync(string conversationId);

        /// <summary>
        /// Clears all saved conversations.
        /// </summary>
        Task ClearAllAsync();
    }

    /// <summary>
    /// Metadata about a saved conversation.
    /// </summary>
    public class ConversationMetadata
    {
        public string ConversationId { get; set; }
        public string Title { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime LastModified { get; set; }
        public int MessageCount { get; set; }
        public Dictionary<string, object> CustomData { get; set; }

        public ConversationMetadata()
        {
            CustomData = new Dictionary<string, object>();
        }
    }
}
