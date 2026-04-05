using System.Collections.Generic;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Builds a directed call graph from parsed log entries.
    /// Each node is a unique API name; each edge means "A called B".
    /// Edge weight = number of times A called B.
    /// </summary>
    public class CallGraphService
    {
        public CallGraph Build(List<LogEntry> entries)
        {
            var graph = new CallGraph();
            var callStack = new Stack<string>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.ApiName)) continue;

                // Ensure caller node exists
                if (!graph.Nodes.ContainsKey(entry.ApiName))
                    graph.Nodes[entry.ApiName] = new CallGraphNode(entry.ApiName);

                if (entry.IsCallEnter)
                {
                    // Record edge from current top of stack → this API
                    if (callStack.Count > 0)
                    {
                        string caller = callStack.Peek();
                        string callee = entry.ApiName;
                        string edgeKey = caller + "→" + callee;

                        if (!graph.Edges.ContainsKey(edgeKey))
                            graph.Edges[edgeKey] = new CallGraphEdge(caller, callee);

                        graph.Edges[edgeKey].Weight++;
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
        // Layout position (set by the renderer)
        public float X { get; set; }
        public float Y { get; set; }

        public CallGraphNode(string name) { Name = name; }
    }

    public class CallGraphEdge
    {
        public string Caller { get; }
        public string Callee { get; }
        public int Weight { get; set; }

        public CallGraphEdge(string caller, string callee)
        {
            Caller = caller;
            Callee = callee;
        }
    }
}
