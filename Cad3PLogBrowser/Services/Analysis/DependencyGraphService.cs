using System;
using System.Collections.Generic;
using System.Linq;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// Builds a dependency graph showing caller/callee relationships.
    /// Feature F4: Dependency Graph - Who calls whom with directional arrows.
    /// </summary>
    public class DependencyGraphService
    {
        /// <summary>
        /// Builds a dependency graph from log entries showing API calling relationships.
        /// </summary>
        /// <param name="entries">Parsed log entries containing API calls.</param>
        /// <returns>Dictionary mapping caller API to set of callee APIs.</returns>
        public Dictionary<string, HashSet<string>> Build(List<LogEntry> entries)
        {
            var dependencies = new Dictionary<string, HashSet<string>>(); // caller -> callees

            if (entries == null || entries.Count == 0)
                return dependencies;

            // Stack to track the call hierarchy
            var callStack = new Stack<string>();

            foreach (var entry in entries)
            {
                if (!entry.IsApiCall || string.IsNullOrEmpty(entry.ApiName))
                    continue;

                if (entry.IsCallEnter)
                {
                    // If there's a caller on the stack, record the dependency
                    if (callStack.Count > 0)
                    {
                        string caller = callStack.Peek();

                        if (!dependencies.ContainsKey(caller))
                            dependencies[caller] = new HashSet<string>();

                        dependencies[caller].Add(entry.ApiName);
                    }

                    callStack.Push(entry.ApiName);
                }
                else if (entry.IsCallExit)
                {
                    // Pop the stack on EXIT
                    if (callStack.Count > 0 && callStack.Peek() == entry.ApiName)
                    {
                        callStack.Pop();
                    }
                }
            }

            return dependencies;
        }
    }
}
