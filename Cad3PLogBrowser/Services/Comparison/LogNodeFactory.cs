namespace Cad3PLogBrowser.Services.Comparison
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models.Comparison;

    /// <summary>
    /// Factory class for converting between WinForms TreeView/TreeNode and LogNode.
    /// Provides bidirectional conversion to maintain source references.
    /// </summary>
    /// <remarks>
    /// This class serves as the bridge between the UI layer and the comparison engine.
    /// It ensures that the comparison engine remains UI-independent while still
    /// allowing results to be mapped back to the visual tree.
    /// </remarks>
    public static class LogNodeFactory
    {
        /// <summary>
        /// Creates a LogNode tree from a WinForms TreeView.
        /// </summary>
        /// <param name="treeView">The source TreeView. If null, returns an empty LogNode.</param>
        /// <returns>A LogNode tree mirroring the TreeView structure.</returns>
        /// <remarks>
        /// This method creates a complete snapshot of the TreeView at the time of the call.
        /// Changes to the TreeView after this call will not be reflected in the LogNode tree.
        /// 
        /// Each LogNode's Source property is set to the original TreeNode for back-reference.
        /// </remarks>
        public static LogNode FromTreeView(TreeView treeView)
        {
            if (treeView == null || treeView.Nodes.Count == 0)
            {
                return new LogNode("(empty)");
            }

            // Create a virtual root to hold all top-level nodes
            var root = new LogNode("Root");

            foreach (TreeNode treeNode in treeView.Nodes)
            {
                var logNode = FromTreeNode(treeNode);
                root.Children.Add(logNode);
            }

            return root;
        }

        /// <summary>
        /// Creates a LogNode from a WinForms TreeNode, recursively converting all children.
        /// </summary>
        /// <param name="treeNode">The source TreeNode. If null, returns null.</param>
        /// <returns>A LogNode mirroring the TreeNode structure.</returns>
        /// <remarks>
        /// This is a recursive method that preserves:
        /// - Node text
        /// - Child hierarchy
        /// - Child order
        /// - Reference to original TreeNode
        /// </remarks>
        public static LogNode FromTreeNode(TreeNode treeNode)
        {
            if (treeNode == null)
                return null;

            var logNode = new LogNode
            {
                Text = treeNode.Text,
                Source = treeNode // Store reference to original TreeNode
            };

            // Recursively convert children
            foreach (TreeNode childTreeNode in treeNode.Nodes)
            {
                var childLogNode = FromTreeNode(childTreeNode);
                if (childLogNode != null)
                {
                    logNode.Children.Add(childLogNode);
                }
            }

            return logNode;
        }

        /// <summary>
        /// Creates a LogNode tree from a list of CallStackNode objects.
        /// </summary>
        /// <param name="callStackNodes">The source list of CallStackNode objects.</param>
        /// <returns>A LogNode tree representing the call stack hierarchy.</returns>
        /// <remarks>
        /// This converter is specific to the call tree structure.
        /// Each LogNode's Source property is set to the original CallStackNode.
        /// </remarks>
        public static LogNode FromCallStackNodes(List<Cad3PLogBrowser.Services.CallStackNode> callStackNodes)
        {
            if (callStackNodes == null || callStackNodes.Count == 0)
            {
                return new LogNode("(empty)");
            }

            var root = new LogNode("Root");

            foreach (var callNode in callStackNodes)
            {
                var logNode = FromCallStackNode(callNode);
                root.Children.Add(logNode);
            }

            return root;
        }

        /// <summary>
        /// Creates a LogNode from a CallStackNode, recursively converting all children.
        /// </summary>
        /// <param name="callNode">The source CallStackNode. If null, returns null.</param>
        /// <returns>A LogNode mirroring the CallStackNode structure.</returns>
        private static LogNode FromCallStackNode(Cad3PLogBrowser.Services.CallStackNode callNode)
        {
            if (callNode == null)
                return null;

            var logNode = new LogNode
            {
                Text = callNode.Label,
                Source = callNode,
                Metadata = new Dictionary<string, object>
                {
                    { "LineNumber", callNode.LineNumber },
                    { "ExitLineNumber", callNode.ExitLineNumber },
                    { "DurationMs", callNode.DurationMs }
                }
            };

            // Recursively convert children
            if (callNode.Children != null)
            {
                foreach (var childCallNode in callNode.Children)
                {
                    var childLogNode = FromCallStackNode(childCallNode);
                    if (childLogNode != null)
                    {
                        logNode.Children.Add(childLogNode);
                    }
                }
            }

            return logNode;
        }

        /// <summary>
        /// Creates a LogNode tree from a list of ApiCallNode objects.
        /// </summary>
        /// <param name="apiNodes">The source list of ApiCallNode objects.</param>
        /// <returns>A LogNode tree representing the API call hierarchy.</returns>
        public static LogNode FromApiCallNodes(List<Models.ApiCallNode> apiNodes)
        {
            if (apiNodes == null || apiNodes.Count == 0)
            {
                return new LogNode("(empty)");
            }

            var root = new LogNode("Root");

            foreach (var apiNode in apiNodes)
            {
                var logNode = FromApiCallNode(apiNode);
                root.Children.Add(logNode);
            }

            return root;
        }

        /// <summary>
        /// Creates a LogNode from an ApiCallNode, recursively converting all children.
        /// </summary>
        /// <param name="apiNode">The source ApiCallNode. If null, returns null.</param>
        /// <returns>A LogNode mirroring the ApiCallNode structure.</returns>
        private static LogNode FromApiCallNode(Models.ApiCallNode apiNode)
        {
            if (apiNode == null)
                return null;

            var logNode = new LogNode
            {
                Text = apiNode.ApiName,
                Source = apiNode,
                Metadata = new Dictionary<string, object>
                {
                    { "TotalCalls", apiNode.TotalCalls },
                    { "TotalDurationMs", apiNode.TotalDurationMs },
                    { "MinDurationMs", apiNode.MinDurationMs },
                    { "MaxDurationMs", apiNode.MaxDurationMs }
                }
            };

            // ApiCallNode doesn't have children in the current model,
            // but we'll leave this structure in place for future extensibility
            // if (apiNode.Children != null) { ... }

            return logNode;
        }

        /// <summary>
        /// Updates TreeNode references in the difference list after comparison.
        /// </summary>
        /// <param name="differences">The list of differences to update.</param>
        /// <remarks>
        /// This method maps LogNode.Source back to TreeNode for UI operations.
        /// Call this after comparison to enable highlighting and navigation.
        /// </remarks>
        public static void MapTreeNodesToDifferences(List<TreeDifference> differences)
        {
            if (differences == null)
                return;

            foreach (var diff in differences)
            {
                if (diff.LeftNode?.Source is TreeNode leftTreeNode)
                {
                    diff.LeftTreeNode = leftTreeNode;
                }

                if (diff.RightNode?.Source is TreeNode rightTreeNode)
                {
                    diff.RightTreeNode = rightTreeNode;
                }
            }
        }
    }
}
