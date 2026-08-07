using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Cad3PLogBrowser.Services.Core;

namespace Cad3PLogBrowser.Services.Analysis
{
    [DataContract]
    public class BaselineApiStat
    {
        [DataMember] public string ApiName { get; set; }
        [DataMember] public long AvgDurationMs { get; set; }
        [DataMember] public int CallCount { get; set; }
    }

    [DataContract]
    public class BaselineData
    {
        [DataMember] public DateTime SavedAtUtc { get; set; }
        [DataMember] public string SourceFileName { get; set; }
        [DataMember] public List<BaselineApiStat> Stats { get; set; } = new List<BaselineApiStat>();
    }

    public class AnomalyResult
    {
        public string ApiName { get; set; }
        public long BaselineAvgMs { get; set; }
        public long CurrentAvgMs { get; set; }
        public int BaselineCalls { get; set; }
        public int CurrentCalls { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// L3: compares the current log's per-API stats against a saved baseline and
    /// flags methods with &gt;2x baseline avg duration or &gt;50% call-count change.
    /// </summary>
    public static class BaselineService
    {
        private static string BaselineFilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "baseline.json");

        public static bool HasBaseline => File.Exists(BaselineFilePath);

        public static void SaveBaseline(string sourceFileName, IEnumerable<ApiPerfStats> stats)
        {
            var data = new BaselineData
            {
                SavedAtUtc = DateTime.UtcNow,
                SourceFileName = sourceFileName ?? "",
            };
            foreach (var s in stats)
            {
                if (s.TimedCallCount <= 0) continue;
                data.Stats.Add(new BaselineApiStat
                {
                    ApiName = s.ApiName,
                    AvgDurationMs = s.AvgDurationMs,
                    CallCount = s.CallCount
                });
            }

            string dir = Path.GetDirectoryName(BaselineFilePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var ser = new DataContractJsonSerializer(typeof(BaselineData));
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, data);
                File.WriteAllBytes(BaselineFilePath, ms.ToArray());
            }
        }

        public static BaselineData LoadBaseline()
        {
            try
            {
                if (!File.Exists(BaselineFilePath)) return null;
                var bytes = File.ReadAllBytes(BaselineFilePath);
                var ser = new DataContractJsonSerializer(typeof(BaselineData));
                using (var ms = new MemoryStream(bytes))
                    return (BaselineData)ser.ReadObject(ms);
            }
            catch
            {
                return null;
            }
        }

        public static List<AnomalyResult> CompareToBaseline(IEnumerable<ApiPerfStats> current, BaselineData baseline)
        {
            var results = new List<AnomalyResult>();
            if (baseline == null || baseline.Stats == null || baseline.Stats.Count == 0 || current == null)
                return results;

            var baselineByName = baseline.Stats.ToDictionary(s => s.ApiName, s => s, StringComparer.OrdinalIgnoreCase);

            foreach (var cur in current)
            {
                if (cur.TimedCallCount <= 0) continue;
                if (!baselineByName.TryGetValue(cur.ApiName, out var b)) continue;

                var reasons = new List<string>();
                if (b.AvgDurationMs > 0 && cur.AvgDurationMs > b.AvgDurationMs * 2)
                    reasons.Add(string.Format("avg time {0:F1}x baseline",
                        cur.AvgDurationMs / (double)b.AvgDurationMs));

                if (b.CallCount > 0)
                {
                    double changePct = Math.Abs(cur.CallCount - b.CallCount) / (double)b.CallCount * 100;
                    if (changePct > 50)
                        reasons.Add(string.Format("call count changed {0:F0}%", changePct));
                }

                if (reasons.Count > 0)
                {
                    results.Add(new AnomalyResult
                    {
                        ApiName = cur.ApiName,
                        BaselineAvgMs = b.AvgDurationMs,
                        CurrentAvgMs = cur.AvgDurationMs,
                        BaselineCalls = b.CallCount,
                        CurrentCalls = cur.CallCount,
                        Reason = string.Join("; ", reasons)
                    });
                }
            }

            return results.OrderByDescending(r => r.CurrentAvgMs).ToList();
        }
    }
}
