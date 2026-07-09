using System;
using System.Threading;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Providers.Mock
{
    /// <summary>
    /// Mock AI provider for testing without requiring an API key or internet connection.
    /// Generates synthetic responses based on the request type.
    /// </summary>
    public class MockProvider : IAIProvider
    {
        private readonly Random _random = new Random();

        public string ProviderName => "Mock Provider (Testing)";

        public bool IsConfigured => true;

        public IAIAuthentication Authentication { get; } = new NoAuthentication();

        public int MaxContextTokens => 100000;

        public int MaxOutputTokens => 4096;

        public Task<IAIResponse> SendRequestAsync(IAIRequest request, 
            CancellationToken cancellationToken = default)
        {
            // Simulate network delay
            Thread.Sleep(_random.Next(500, 1500));

            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult<IAIResponse>(AIResponse.CreateError("Request cancelled"));

            string response = GenerateMockResponse(request);
            int promptTokens = EstimateTokenCount(request.Prompt);
            int completionTokens = EstimateTokenCount(response);

            return Task.FromResult<IAIResponse>(new AIResponse
            {
                Success = true,
                Content = response,
                Model = "mock-model-1.0",
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = promptTokens + completionTokens,
                ElapsedTime = TimeSpan.FromMilliseconds(_random.Next(500, 1500)),
                FinishReason = "stop"
            });
        }

        public async Task StreamRequestAsync(IAIRequest request,
            Action<string> onChunkReceived,
            Action<IAIResponse> onComplete,
            Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string fullResponse = GenerateMockResponse(request);

                // Stream word by word
                var words = fullResponse.Split(' ');
                var contentBuilder = new System.Text.StringBuilder();

                foreach (var word in words)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    string chunk = word + " ";
                    contentBuilder.Append(chunk);
                    onChunkReceived?.Invoke(chunk);

                    await Task.Delay(_random.Next(50, 150), cancellationToken);
                }

                int promptTokens = EstimateTokenCount(request.Prompt);
                int completionTokens = EstimateTokenCount(fullResponse);

                var response = new AIResponse
                {
                    Success = true,
                    Content = contentBuilder.ToString().Trim(),
                    Model = "mock-model-1.0",
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = promptTokens + completionTokens,
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
            return new Anthropic.AnthropicConversation(this);
        }

        public Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string>(null); // Always succeeds
        }

        public int EstimateTokenCount(string text)
        {
            return text.Length / 4;
        }

        private string GenerateMockResponse(IAIRequest request)
        {
            string prompt = request.Prompt?.ToLowerInvariant() ?? "";

            // Generate contextual mock responses
            if (prompt.Contains("summarize") || prompt.Contains("summary"))
            {
                return GenerateSummary();
            }
            else if (prompt.Contains("root cause") || prompt.Contains("error"))
            {
                return GenerateRootCauseAnalysis();
            }
            else if (prompt.Contains("performance") || prompt.Contains("slow"))
            {
                return GeneratePerformanceAnalysis();
            }
            else if (prompt.Contains("compare") || prompt.Contains("difference"))
            {
                return GenerateComparisonAnalysis();
            }
            else if (prompt.Contains("timeline") || prompt.Contains("sequence"))
            {
                return GenerateTimeline();
            }
            else if (prompt.Contains("crash") || prompt.Contains("crash"))
            {
                return GenerateCrashAnalysis();
            }
            else
            {
                return GenerateGenericResponse();
            }
        }

        private string GenerateSummary()
        {
            return @"## Log Summary

**Status**: WARNING

**Overview**: The log file contains 15,432 lines spanning a session of approximately 45 minutes. The application executed 1,247 API calls across 156 unique functions.

**Key Findings**:
- 3 critical errors detected
- 28 warnings logged
- Maximum call depth of 12 levels
- Performance concern: DatabaseQuery operations averaging 850ms

**Top Issues**:
1. NULL reference exception in UserProfileService
2. Timeout on external API call to ConfigurationService
3. Memory pressure warning at 15:23:41

**Recommendations**:
- Investigate the NULL reference in UserProfileService (appears 3 times)
- Review database query optimization for slow operations
- Consider increasing timeout for ConfigurationService calls";
        }

        private string GenerateRootCauseAnalysis()
        {
            return @"## Root Cause Analysis

**Primary Issue**: Application crash due to unhandled NullReferenceException

**Root Cause**: The UserProfileService attempted to access the Email property of a User object that was null.

**Causal Chain**:
1. LoginController.AuthenticateUser() successfully authenticated
2. UserProfileService.LoadProfile() was called with userId
3. Database query returned null (user not found in profile table)
4. Code attempted to access user.Email without null check
5. NullReferenceException thrown
6. No exception handler at call site
7. Application crashed

**Confidence**: HIGH

**Evidence**:
- Line 1247: ""User profile not found for ID: 12345""
- Line 1248: NullReferenceException stack trace
- User exists in authentication table but not in profile table (data inconsistency)

**Recommended Fix**:
Add null check in UserProfileService.LoadProfile() and handle missing profiles gracefully.";
        }

        private string GeneratePerformanceAnalysis()
        {
            return @"## Performance Analysis

**Overall Assessment**: Application shows moderate performance issues with specific bottlenecks.

**Slow Operations** (>1000ms):
1. **DatabaseQuery.ExecuteComplexJoin**: 2,450ms average (5 calls)
   - Impact: HIGH - blocks user operations

2. **ExternalAPIClient.SyncConfiguration**: 1,850ms average (2 calls)
   - Impact: MEDIUM - startup delay

3. **ReportGenerator.CreatePDF**: 1,200ms average (3 calls)
   - Impact: LOW - background operation

**Repeated Operations**:
- ValidateSession called 342 times (avg 15ms each = 5.1s total)
- Potential for caching

**Recommendations**:
1. Optimize DatabaseQuery join - consider indexing or query restructuring
2. Implement async loading for ExternalAPIClient
3. Add caching for ValidateSession results (use sliding expiration)
4. Consider pagination for ReportGenerator to reduce load";
        }

        private string GenerateComparisonAnalysis()
        {
            return @"## Log Comparison Analysis

**Overall Assessment**: REGRESSION - New issues introduced

**New Errors** (appeared in current log):
- ConfigurationService timeout (3 occurrences)
- FileNotFoundException in ReportModule

**Fixed Issues** (resolved from baseline):
- DatabaseConnectionPool exhaustion no longer occurs
- CacheInvalidation errors resolved

**Performance Changes**:
- LoginService: 15% SLOWER (225ms ? 260ms)
- DatabaseQuery: 30% FASTER (1200ms ? 840ms) ?
- API response time: 8% SLOWER overall

**Behavioral Differences**:
- Current log shows different initialization sequence
- New dependency on ConfigurationService introduced
- Report module now attempts file I/O (not present in baseline)

**Verdict**: Mixed results. Database optimization improved but new ConfigurationService dependency introduced regressions. Recommend investigating timeout issues before deployment.";
        }

        private string GenerateTimeline()
        {
            return @"## Event Timeline

**00:00.000** - Application startup
- Initialized dependency injection container
- Loaded configuration

**00:01.250** - Database connection established
- Connected to primary database
- Connection pool: 10 connections

**00:02.100** - User authentication began
- LoginController.AuthenticateUser invoked
- User credentials validated

**00:02.350** - [ERROR] User profile load failed
- UserProfileService.LoadProfile returned null
- NullReferenceException thrown

**00:02.351** - Exception propagated through call stack
- No handler in LoginController
- No handler in AuthenticationModule
- Reached top-level exception handler

**00:02.352** - Application crash
- Unhandled exception terminated process
- Exit code: -1

**Key Insights**:
- Total execution time: 2.35 seconds
- Crash occurred during user authentication
- No retry logic implemented
- Error handling gaps in authentication flow";
        }

        private string GenerateCrashAnalysis()
        {
            return @"## Crash Analysis

**Crash Type**: Unhandled NullReferenceException

**Location**: UserProfileService.cs, line 142

**Immediate Cause**: 
Attempted to access `user.Email` where `user` was null.

**Stack Trace Summary**:
```
NullReferenceException: Object reference not set
  at UserProfileService.LoadProfile(Int32 userId)
  at LoginController.AuthenticateUser(LoginRequest request)
  at AuthenticationModule.ProcessLogin(HttpContext context)
```

**Application State Before Crash**:
- User successfully authenticated
- Session token generated
- Database connection active
- 247MB memory usage (normal range)

**Why It Crashed**:
1. User profile missing from database (data integrity issue)
2. No null check after database query
3. No exception handling at call site
4. No global exception filter for this endpoint

**Reproducibility**: HIGH
- Occurs for any user without profile record
- Consistent crash pattern

**Prevention Strategy**:
1. Add defensive null checks in UserProfileService
2. Implement global exception handler
3. Add database constraint to ensure profile exists for all users
4. Return meaningful error response instead of crashing

**Immediate Fix**:
```csharp
if (user == null)
{
    _logger.LogWarning($""Profile not found for user {userId}"");
    throw new UserProfileNotFoundException(userId);
}
```";
        }

        private string GenerateGenericResponse()
        {
            return @"I've analyzed the log file. Here are my observations:

The log contains various API calls and operations typical of a CAD/CAM application session. I can see initialization sequences, user operations, and some warning messages.

To provide more specific insights, I can help you with:
- Summarizing the overall log activity
- Identifying errors and their root causes
- Analyzing performance bottlenecks
- Explaining specific exceptions
- Comparing two log files
- Creating a timeline of events

Please let me know what specific aspect you'd like me to focus on, or ask a more specific question about the log file.";
        }

        private class NoAuthentication : IAIAuthentication
        {
            public AuthenticationType AuthType => AuthenticationType.None;
            public bool IsAuthenticated => true;
            public Task<string> ValidateAsync(CancellationToken cancellationToken = default) => 
                Task.FromResult<string>(null);
            public string GetAuthenticationToken() => string.Empty;
            public Task RefreshAsync(CancellationToken cancellationToken = default) => 
                Task.CompletedTask;
        }
    }
}
