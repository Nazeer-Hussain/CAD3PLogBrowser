using System;
using System.Collections.Generic;
using System.Linq;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Builds a directed call graph from parsed log entries.
    /// Uses the accurate ENTER/EXIT pairs from <see cref="LogParserService"/>.
    /// Edge weight = number of times caller→callee pair appears.
    /// </summary>
    public class CallGraphService
    {
        public CallGraph Build(List<LogEntry> entries)
        {
            var graph     = new CallGraph();
            var callStack = new Stack<string>();

            foreach (var entry in entries)
            {
                if (!entry.IsApiCall) continue;

                // Ensure node exists
                if (!graph.Nodes.ContainsKey(entry.ApiName))
                    graph.Nodes[entry.ApiName] = new CallGraphNode(entry.ApiName);

                if (entry.IsCallEnter)
                {
                    if (callStack.Count > 0)
                    {
                        string caller  = callStack.Peek();
                        string callee  = entry.ApiName;
                        string edgeKey = caller + "→" + callee;

                        if (!graph.Edges.TryGetValue(edgeKey, out var edge))
                        {
                            edge = new CallGraphEdge(caller, callee);
                            graph.Edges[edgeKey] = edge;
                        }
                        edge.Weight++;
                    }
                    callStack.Push(entry.ApiName);
                }
                else if (entry.IsCallExit)
                {
                    if (callStack.Count > 0 && callStack.Peek() == entry.ApiName)
                        callStack.Pop();
                }
            }

            return graph;
        }

        /// <summary>
        /// Builds one independent <see cref="CallGraph"/> per source log file.
        /// Entries are grouped by <see cref="LogEntry.SourceLogFile"/> so that
        /// cross-file caller→callee edges are never created.
        /// Falls back to a single-entry list wrapping <see cref="Build"/> when all
        /// entries share the same (or empty) source file tag.
        /// </summary>
        /// <returns>
        /// Ordered list of (fileName, graph) pairs; the fileName is the tag
        /// added by <see cref="MergeLogService"/> (e.g. "log1.txt").
        /// </returns>
        public List<(string FileName, CallGraph Graph)> BuildGroupedByFile(List<LogEntry> entries)
        {
            var fileOrder  = new List<string>();
            var fileGroups = new Dictionary<string, List<LogEntry>>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                string key = string.IsNullOrEmpty(entry.SourceLogFile) ? string.Empty : entry.SourceLogFile;
                if (!fileGroups.TryGetValue(key, out var group))
                {
                    group = new List<LogEntry>();
                    fileGroups[key] = group;
                    fileOrder.Add(key);
                }
                group.Add(entry);
            }

            var result = new List<(string, CallGraph)>(fileOrder.Count);
            foreach (var fileName in fileOrder)
                result.Add((fileName, Build(fileGroups[fileName])));

            return result;
        }
    }

    public class CallGraph
    {
        public Dictionary<string, CallGraphNode> Nodes { get; } =
            new Dictionary<string, CallGraphNode>();
        public Dictionary<string, CallGraphEdge> Edges { get; } =
            new Dictionary<string, CallGraphEdge>();
    }

    public class CallGraphNode
    {
        public string Name { get; }
        public float  X    { get; set; }
        public float  Y    { get; set; }
        public CallGraphNode(string name) { Name = name; }
    }

    public class CallGraphEdge
    {
        public string Caller { get; }
        public string Callee { get; }
        public int    Weight { get; set; }
        public CallGraphEdge(string caller, string callee)
        {
            Caller = caller;
            Callee = callee;
        }
    }
}
