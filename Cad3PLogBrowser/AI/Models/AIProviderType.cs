namespace Cad3PLogBrowser.AI.Models
{
    /// <summary>
    /// Supported AI provider types.
    /// </summary>
    public enum AIProviderType
    {
        None = 0,
        OpenAI = 1,
        AzureOpenAI = 2,
        Anthropic = 3,
        GoogleGemini = 4,
        GitHubCopilot = 5,
        Ollama = 6,
        Mock = 99
    }
}
