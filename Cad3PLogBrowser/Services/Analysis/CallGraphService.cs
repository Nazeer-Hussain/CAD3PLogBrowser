using System.Collections.Generic;

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

                        if (!graph.Edges.ContainsKey(edgeKey))
                            graph.Edges[edgeKey] = new CallGraphEdge(caller, callee);

                        graph.Edges[edgeKey].Weight++;
                    }
                    callStack.Push(entry.ApiName);
                }
                else if (entry.IsCallExit)
                {
                    // Walk the stack to find the matching ENTER (mirrors LogParserService).
                    // A simple Peek-and-pop fails when an inner ENTER had no matching EXIT.
                    var temp = new Stack<string>();
                    while (callStack.Count > 0)
                    {
                        string top = callStack.Pop();
                        if (top == entry.ApiName)
                            break;          // found the match — leave it popped
                        temp.Push(top);     // not a match — restore later
                    }
                    // Restore any intermediary frames that weren't the target
                    while (temp.Count > 0) callStack.Push(temp.Pop());
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
