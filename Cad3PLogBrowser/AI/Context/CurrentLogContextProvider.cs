using System;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.AI.Context
{
    /// <summary>
    /// Provides context from the currently loaded log file.
    /// Includes summary statistics rather than raw log lines to stay within token limits.
    /// </summary>
    public class CurrentLogContextProvider : ContextProviderBase
    {
        private readonly AggregateStats _stats;
        private readonly string _logFilePath;
        private readonly Func<string> _getSelectedText;

        public CurrentLogContextProvider(
            AggregateStats stats, 
            string logFilePath = null,
            Func<string> getSelectedText = null,
            ITokenEstimator tokenEstimator = null) 
            : base(tokenEstimator)
        {
            _stats = stats;
            _logFilePath = logFilePath;
            _getSelectedText = getSelectedText;
        }

        public override string ContextType => "CurrentLog";

        public override string Description => "Current Log Summary";

        public override bool HasContext => _stats != null;

        public override Task<string> GetContextAsync()
        {
            if (!HasContext)
                return Task.FromResult(string.Empty);

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_logFilePath))
            {
                sb.AppendLine($"**File**: {System.IO.Path.GetFileName(_logFilePath)}");
                sb.AppendLine();
            }

            sb.AppendLine("### Log Statistics");
            sb.AppendLine($"- Total Lines: {_stats.TotalLines:N0}");
            sb.AppendLine($"- Errors: {_stats.ErrorCount}");
            sb.AppendLine($"- Warnings: {_stats.WarningCount}");
            sb.AppendLine($"- API Calls: {_stats.TotalApiCalls:N0} ({_stats.UniqueApiCount} unique)");
            sb.AppendLine($"- Max Call Depth: {_stats.MaxCallDepth}");
            sb.AppendLine($"- Session Duration: {_stats.SessionDurationMs:N0} ms");
            sb.AppendLine();

            // Include selected text if available
            if (_getSelectedText != null)
            {
                string selected = _getSelectedText();
                if (!string.IsNullOrWhiteSpace(selected))
                {
                    sb.AppendLine("### Selected Content");
                    sb.AppendLine("```");
                    sb.AppendLine(selected);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }

            return Task.FromResult(sb.ToString());
        }

        public override Task<string> GetSummaryAsync()
        {
            if (!HasContext)
                return Task.FromResult("No log loaded");

            return Task.FromResult($"Log with {_stats.TotalLines:N0} lines, " +
                $"{_stats.ErrorCount} errors, {_stats.WarningCount} warnings");
        }
    }
}
