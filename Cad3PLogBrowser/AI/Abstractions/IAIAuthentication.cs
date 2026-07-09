using System.Threading;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Handles authentication for AI providers.
    /// Different providers support different authentication mechanisms.
    /// </summary>
    public interface IAIAuthentication
    {
        /// <summary>Type of authentication (ApiKey, OAuth, AzureAD, etc.).</summary>
        AuthenticationType AuthType { get; }

        /// <summary>Indicates whether authentication credentials are configured.</summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Validates the current credentials with the provider.
        /// Returns null on success, or an error message on failure.
        /// </summary>
        Task<string> ValidateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the authentication header or token for API requests.
        /// </summary>
        string GetAuthenticationToken();

        /// <summary>
        /// Refreshes authentication if using OAuth or time-limited tokens.
        /// </summary>
        Task RefreshAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Types of authentication supported by AI providers.
    /// </summary>
    public enum AuthenticationType
    {
        None,
        ApiKey,
        OAuth,
        AzureActiveDirectory,
        PersonalAccessToken,
        DeviceCode,
        ManagedIdentity
    }

    /// <summary>
    /// Simple API key authentication.
    /// </summary>
    public class ApiKeyAuthentication : IAIAuthentication
    {
        private readonly string _apiKey;
        private readonly string _headerName;

        public ApiKeyAuthentication(string apiKey, string headerName = "Authorization")
        {
            _apiKey = apiKey;
            _headerName = headerName;
        }

        public AuthenticationType AuthType => AuthenticationType.ApiKey;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_apiKey);

        public Task<string> ValidateAsync(CancellationToken cancellationToken = default)
        {
            // Basic validation - actual validation happens on first API call
            if (string.IsNullOrWhiteSpace(_apiKey))
                return Task.FromResult("API key is empty");

            if (_apiKey.Length < 10)
                return Task.FromResult("API key appears invalid (too short)");

            return Task.FromResult<string>(null);
        }

        public string GetAuthenticationToken()
        {
            return _apiKey;
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            // API keys don't need refresh
            return Task.CompletedTask;
        }
    }
}
