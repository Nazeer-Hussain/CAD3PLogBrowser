using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.AI.Context
{
    /// <summary>
    /// L1: provides the per-method call-count/duration breakdown the AI Log Summarizer
    /// spec calls for ("method list with call counts and durations"). Without this,
    /// the "Slowest methods" section of a summary has no real timing data to draw on
    /// and the model can only guess at method names — this supplies actual numbers.
    /// </summary>
    public class ApiPerformanceContextProvider : ContextProviderBase
    {
        private readonly List<ApiPerfStats> _stats;
        private readonly int _topN;

        public ApiPerformanceContextProvider(
            List<ApiPerfStats> stats,
            int topN = 15,
            ITokenEstimator tokenEstimator = null)
            : base(tokenEstimator)
        {
            _stats = stats;
            _topN = topN;
        }

        public override string ContextType => "ApiPerformance";

        public override string Description => "Method Performance Breakdown";

        public override bool HasContext => _stats != null && _stats.Count > 0;

        public override Task<string> GetContextAsync()
        {
            if (!HasContext)
                return Task.FromResult(string.Empty);

            var slowest = _stats
                .Where(s => s.TimedCallCount > 0)
                .OrderByDescending(s => s.TotalDurationMs)
                .Take(_topN)
                .ToList();

            if (slowest.Count == 0)
                return Task.FromResult(string.Empty);

            var sb = new StringBuilder();
            sb.AppendLine("### Method Performance (top " + slowest.Count + " by total duration)");
            sb.AppendLine("| Method | Calls | Avg (ms) | Max (ms) | Total (ms) |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var s in slowest)
            {
                sb.AppendLine($"| {s.ApiName} | {s.CallCount} | {s.AvgDurationMs:N0} | {s.MaxDurationMs:N0} | {s.TotalDurationMs:N0} |");
            }
            sb.AppendLine();

            return Task.FromResult(sb.ToString());
        }

        public override Task<string> GetSummaryAsync()
        {
            if (!HasContext)
                return Task.FromResult("No performance data available");

            var slowest = _stats.OrderByDescending(s => s.TotalDurationMs).FirstOrDefault();
            return Task.FromResult(slowest == null
                ? "No timed calls"
                : $"{_stats.Count} methods profiled, slowest: {slowest.ApiName} ({slowest.TotalDurationMs:N0} ms total)");
        }
    }
}
