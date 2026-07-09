using System;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Providers.Ollama;

namespace Cad3PLogBrowser.AI.Examples
{
    /// <summary>
    /// Example usage of Ollama provider for log analysis.
    /// This demonstrates how to integrate Ollama into your CAD3PLogBrowser application.
    /// </summary>
    public class OllamaUsageExample
    {
        /// <summary>
        /// Example: Simple log analysis query
        /// </summary>
        public static async Task SimpleLogAnalysisExample()
        {
            // Configure Ollama provider
            // For local testing: "http://localhost:11434"
            // For production: "http://your-ollama-server.corp.local:11434"
            var provider = new OllamaProvider(
                baseUrl: "http://localhost:11434",
                model: "llama3"
            );

            // Test connection first
            var connectionTest = await provider.TestConnectionAsync();
            if (connectionTest != null)
            {
                Console.WriteLine($"Connection failed: {connectionTest}");
                return;
            }

            Console.WriteLine("Connected to Ollama successfully!");

            // Create a request
            var request = new AIRequest
            {
                SystemPrompt = "You are a log analysis expert. Analyze the following log entries and provide insights.",
                Prompt = @"
[2024-01-15 10:23:45] ERROR: Database connection timeout after 30s
[2024-01-15 10:23:46] WARN: Retrying connection (attempt 1/3)
[2024-01-15 10:23:51] ERROR: Database connection timeout after 30s
[2024-01-15 10:23:52] WARN: Retrying connection (attempt 2/3)
[2024-01-15 10:23:57] ERROR: Database connection timeout after 30s
[2024-01-15 10:23:58] ERROR: Max retry attempts reached. Failing request.

What is the problem and how can I fix it?",
                Temperature = 0.7,
                MaxTokens = 2000
            };

            // Send request
            var response = await provider.SendRequestAsync(request);

            if (response.Success)
            {
                Console.WriteLine("\n=== AI Response ===");
                Console.WriteLine(response.Content);
                Console.WriteLine($"\nTokens used: {response.TotalTokens}");
                Console.WriteLine($"Time taken: {response.ElapsedTime.TotalSeconds:F2}s");
            }
            else
            {
                Console.WriteLine($"Error: {response.ErrorMessage}");
            }
        }

        /// <summary>
        /// Example: Streaming response for real-time feedback
        /// </summary>
        public static async Task StreamingLogAnalysisExample()
        {
            var provider = new OllamaProvider(
                baseUrl: "http://localhost:11434",
                model: "llama3"
            );

            var request = new AIRequest
            {
                SystemPrompt = "You are a helpful log analysis assistant.",
                Prompt = "Explain what this error means: NullReferenceException at line 42",
                Temperature = 0.7,
                MaxTokens = 1000
            };

            Console.WriteLine("Streaming response:\n");

            // Stream the response (useful for UI to show progress)
            var response = await provider.SendStreamingRequestAsync(
                request,
                chunk => Console.Write(chunk) // Print each chunk as it arrives
            );

            Console.WriteLine($"\n\nComplete! Tokens: {response.TotalTokens}");
        }

        /// <summary>
        /// Example: Multi-turn conversation for interactive log analysis
        /// </summary>
        public static async Task ConversationExample()
        {
            var provider = new OllamaProvider(
                baseUrl: "http://localhost:11434",
                model: "llama3"
            );

            var conversation = provider.CreateConversation();
            conversation.SystemPrompt = "You are a log analysis expert. Help the user debug their application.";

            // First question
            Console.WriteLine("User: What could cause high memory usage?");
            var response1 = await conversation.SendMessageAsync(
                "What could cause high memory usage in a .NET application?"
            );
            Console.WriteLine($"AI: {response1.Content}\n");

            // Follow-up question (maintains context)
            Console.WriteLine("User: How do I profile it?");
            var response2 = await conversation.SendMessageAsync(
                "How do I profile it to find the exact cause?"
            );
            Console.WriteLine($"AI: {response2.Content}\n");

            // Show conversation history
            Console.WriteLine("\n=== Conversation History ===");
            foreach (var msg in conversation.Messages)
            {
                Console.WriteLine($"{msg.Role}: {msg.Content.Substring(0, Math.Min(50, msg.Content.Length))}...");
            }
        }

        /// <summary>
        /// Example: Using different models for different tasks
        /// </summary>
        public static async Task ModelComparisonExample()
        {
            // Use llama3 for complex analysis
            var llama3 = new OllamaProvider("http://localhost:11434", "llama3");

            // Use phi3 for quick, simple queries (faster, less RAM)
            var phi3 = new OllamaProvider("http://localhost:11434", "phi3");

            var simpleQuery = new AIRequest
            {
                Prompt = "What does HTTP 404 mean?",
                MaxTokens = 100
            };

            Console.WriteLine("Testing phi3 (fast, lightweight)...");
            var quickResponse = await phi3.SendRequestAsync(simpleQuery);
            Console.WriteLine($"Response time: {quickResponse.ElapsedTime.TotalSeconds:F2}s");
            Console.WriteLine($"Answer: {quickResponse.Content}\n");

            Console.WriteLine("Testing llama3 (more capable)...");
            var betterResponse = await llama3.SendRequestAsync(simpleQuery);
            Console.WriteLine($"Response time: {betterResponse.ElapsedTime.TotalSeconds:F2}s");
            Console.WriteLine($"Answer: {betterResponse.Content}");
        }

        /// <summary>
        /// Example: Error handling
        /// </summary>
        public static async Task ErrorHandlingExample()
        {
            // Intentionally wrong URL to demonstrate error handling
            var provider = new OllamaProvider(
                baseUrl: "http://wrong-server:11434",
                model: "llama3"
            );

            var connectionTest = await provider.TestConnectionAsync();
            if (connectionTest != null)
            {
                Console.WriteLine($"Expected error: {connectionTest}");
                Console.WriteLine("This is how you should handle connection failures in your UI.");
            }

            // Test with a model that doesn't exist
            var provider2 = new OllamaProvider(
                baseUrl: "http://localhost:11434",
                model: "nonexistent-model"
            );

            var test2 = await provider2.TestConnectionAsync();
            if (test2 != null)
            {
                Console.WriteLine($"\nExpected error: {test2}");
                Console.WriteLine("Show this message to users so they know which model to install.");
            }
        }

        /// <summary>
        /// Example: Integration with your log viewer
        /// </summary>
        public static async Task LogViewerIntegrationExample(string[] selectedLogLines)
        {
            var provider = new OllamaProvider(
                baseUrl: "http://your-server:11434",
                model: "llama3"
            );

            // User selects lines in the log viewer and clicks "Analyze with AI"
            var logContent = string.Join("\n", selectedLogLines);

            var request = new AIRequest
            {
                SystemPrompt = @"You are an expert at analyzing application logs. 
When analyzing logs:
1. Identify errors and their root causes
2. Suggest specific fixes
3. Highlight patterns or anomalies
4. Keep explanations concise and actionable",
                Prompt = $"Analyze these log entries:\n\n{logContent}",
                Temperature = 0.7,
                MaxTokens = 2000
            };

            // Show a loading indicator in your UI here
            var response = await provider.SendRequestAsync(request);

            if (response.Success)
            {
                // Display response in a dialog or panel in your log viewer
                // ShowAnalysisDialog(response.Content);
                Console.WriteLine(response.Content);
            }
            else
            {
                // Show error message to user
                // MessageBox.Show(response.ErrorMessage);
                Console.WriteLine($"Error: {response.ErrorMessage}");
            }
        }
    }
}
