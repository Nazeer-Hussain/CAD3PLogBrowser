namespace Cad3PLogBrowser.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models;
    using Cad3PLogBrowser.Utilities;
    using Services = Cad3PLogBrowser.Services;

    /// <summary>
    /// Manages the Call Tree and API Tree views.
    /// Handles tree population, node selection, expand/collapse operations, and visual styling.
    /// </summary>
    /// <remarks>
    /// This manager separates tree-related UI logic from the MainForm.
    /// It owns the TreeView controls and coordinates their display and interaction.
    /// 
    /// Responsibilities:
    /// - Populate trees from data models
    /// - Apply visual styling (icons, colors, fonts)
    /// - Handle expand/collapse operations
    /// - Switch between Call Tree and API Tree views
    /// - Provide search/navigation within trees
    /// 
    /// The manager does NOT:
    /// - Parse log files (that's LogParserService)
    /// - Handle menu/toolbar clicks (that's MainForm)
    /// - Manage application state (that's MainForm)
    /// </remarks>
    public class TreeViewManager
    {
        // Tree view controls
        private readonly TreeView _callTreeView;
        private readonly TreeView _apiTreeView;
        private readonly ImageList _nodeIconList;

        /// <summary>
        /// Gets a value indicating whether the Call Tree is currently visible.
        /// </summary>
        public bool IsCallTreeVisible => _callTreeView.Visible;

        /// <summary>
        /// Gets a value indicating whether the API Tree is currently visible.
        /// </summary>
        public bool IsApiTreeVisible => _apiTreeView.Visible;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewManager"/> class.
        /// </summary>
        /// <param name="callTreeView">TreeView control for the Call Tree.</param>
        /// <param name="apiTreeView">TreeView control for the API Tree.</param>
        /// <param name="nodeIconList">ImageList containing tree node icons (checkmark, cross).</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public TreeViewManager(
            TreeView callTreeView,
            TreeView apiTreeView,
            ImageList nodeIconList)
        {
            _callTreeView = callTreeView ?? throw new ArgumentNullException(nameof(callTreeView));
            _apiTreeView = apiTreeView ?? throw new ArgumentNullException(nameof(apiTreeView));
            _nodeIconList = nodeIconList ?? throw new ArgumentNullException(nameof(nodeIconList));

            // Configure tree views
            ConfigureTreeViews();
        }

        /// <summary>
        /// Configures both tree views with optimal settings.
        /// </summary>
        private void ConfigureTreeViews()
        {
            foreach (var tree in new[] { _callTreeView, _apiTreeView })
            {
                tree.ImageList       = _nodeIconList;
                tree.ShowNodeToolTips = true;
                tree.HideSelection   = false;  // keep selection visible when unfocused
                // Issue 4 Fix: FullRowSelect + owner-draw prevents text clipping in dark theme
                tree.FullRowSelect   = true;
                tree.DrawMode        = TreeViewDrawMode.OwnerDrawText;
                tree.DrawNode       += TreeView_DrawNode;
            }
        }

        /// <summary>
        /// Issue 4 Fix: Custom draw ensures text is never truncated and always
        /// uses the correct foreground color regardless of system highlight color.
        /// </summary>
        private void TreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            bool isDark    = Services.ThemeManager.CurrentTheme == Services.ThemeManager.Theme.Dark;
            bool isSelected = (e.State & TreeNodeStates.Selected) != 0;

            // ── Background ────────────────────────────────────────────────────
            Color backColor;
            if (isSelected)
            {
                backColor = isDark
                    ? Color.FromArgb(50, 90, 150)     // dark-mode selection: blue tint
                    : SystemColors.Highlight;
            }
            else
            {
                backColor = e.Node.BackColor != Color.Empty
                    ? e.Node.BackColor
                    : (isDark ? Color.FromArgb(28, 30, 38) : SystemColors.Window);
            }

            using (var bgBrush = new System.Drawing.SolidBrush(backColor))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            // ── Focus rectangle ───────────────────────────────────────────────
            if ((e.State & TreeNodeStates.Focused) != 0)
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds,
                    isDark ? Color.White : SystemColors.ControlText, backColor);

            // ── Foreground text ───────────────────────────────────────────────
            Color foreColor;
            if (isSelected)
            {
                foreColor = isDark ? Color.White : SystemColors.HighlightText;
            }
            else
            {
                // Respect per-node color (duration coding) but fall back to theme color
                foreColor = e.Node.ForeColor != Color.Empty && e.Node.ForeColor != Color.Black
                    ? e.Node.ForeColor
                    : (isDark ? Color.FromArgb(210, 215, 230) : SystemColors.WindowText);
            }

            Font font = e.Node.NodeFont ?? (sender as TreeView)?.Font ?? SystemFonts.DefaultFont;

            // Measure text to ensure it never clips
            var textRect = new Rectangle(
                e.Bounds.X + 2,
                e.Bounds.Y,
                e.Bounds.Width - 2,
                e.Bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                font,
                textRect,
                foreColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Applies theme-aware colors to both tree views.
        /// Call this whenever the application theme changes.
        /// </summary>
        public void ApplyTheme()
        {
            bool isDark = Services.ThemeManager.CurrentTheme == Services.ThemeManager.Theme.Dark;

            Color back = isDark ? Color.FromArgb(28, 30, 38)    : SystemColors.Window;
            Color fore = isDark ? Color.FromArgb(210, 215, 230) : SystemColors.WindowText;
            Color line = isDark ? Color.FromArgb(60, 65, 80)    : SystemColors.GrayText;

            foreach (var tree in new[] { _callTreeView, _apiTreeView })
            {
                tree.BackColor  = back;
                tree.ForeColor  = fore;
                tree.LineColor  = line;
                tree.Invalidate();
            }
        }

        /// <summary>
        /// Populates the Call Tree with hierarchical call stack data.
        /// </summary>
        /// <param name="rootNodes">Root nodes of the call tree (top-level method calls).</param>
        /// <param name="expandFirstLevel">Whether to expand the first level after population.</param>
        /// <remarks>
        /// The Call Tree shows the execution flow in chronological order.
        /// Parent nodes represent callers, child nodes represent callees.
        /// 
        /// Visual elements applied:
        /// - ? icon for matched ENTER/EXIT pairs
        /// - ? icon for unmatched calls
        /// - Duration overlay: "[142 ms]"
        /// - Color coding: green (fast), amber (medium), red (slow)
        /// </remarks>
        /// <example>
        /// var roots = parser.BuildCallTree(logEntries);
        /// treeManager.PopulateCallTree(roots, expandFirstLevel: true);
        /// </example>
        public void PopulateCallTree(List<CallStackNode> rootNodes, bool expandFirstLevel = true)
        {
            if (rootNodes == null)
                rootNodes = new List<CallStackNode>();

            _callTreeView.BeginUpdate();
            _callTreeView.Nodes.Clear();

            // Create top-most root node labeled "Call Tree"
            var topRoot = new TreeNode("Call Tree")
            {
                Tag = -1, // Special tag indicating this is the UI root, not a log entry
                NodeFont = new Font(_callTreeView.Font, FontStyle.Bold)
            };

            // Add all call stack roots as children
            foreach (var root in rootNodes)
            {
                topRoot.Nodes.Add(BuildCallTreeNode(root));
            }

            _callTreeView.Nodes.Add(topRoot);

            // Expand root to show first level
            topRoot.Expand();

            // Optionally expand all first-level nodes
            if (expandFirstLevel)
            {
                foreach (TreeNode child in topRoot.Nodes)
                {
                    child.Expand();
                }
            }

            _callTreeView.EndUpdate();
        }

        /// <summary>
        /// Builds a TreeNode from a CallStackNode data model.
        /// Applies visual styling (icons, colors, duration overlay).
        /// </summary>
        /// <param name="callStackNode">The data model to convert.</param>
        /// <returns>A TreeNode ready to add to the tree view.</returns>
        /// <remarks>
        /// Visual elements applied:
        /// - Node text: "MethodName [duration]"
        /// - Icon: index 0 (?) if matched, index 1 (?) if unmatched
        /// - Color: based on duration (green/amber/red)
        /// - Tooltip: detailed information (API, source, line numbers, duration)
        /// </remarks>
        private TreeNode BuildCallTreeNode(CallStackNode callStackNode)
        {
            bool isMatched = callStackNode.IsMatched;

            // Build display text with duration overlay
            string displayText = callStackNode.GetDisplayText();

            // Build detailed tooltip
            string tooltip = $"API: {callStackNode.Label}\n" +
                           $"Source: {callStackNode.SourceFile ?? "-"}\n" +
                           $"ENTER line: {callStackNode.LineNumber}\n" +
                           $"EXIT line: {(isMatched ? callStackNode.ExitLineNumber.ToString() : "? (no EXIT found)")}\n" +
                           $"Duration: {callStackNode.DurationMs} ms\n" +
                           $"Self time: {callStackNode.SelfTimeMs} ms\n" +
                           $"Depth: {callStackNode.Depth}\n" +
                           $"Children: {callStackNode.Children.Count}";

            // Create tree node
            var treeNode = new TreeNode(displayText)
            {
                Tag = callStackNode.LineNumber, // Store line number for navigation
                ToolTipText = tooltip,
                ImageIndex = isMatched ? 0 : 1,        // 0 = ?, 1 = ?
                SelectedImageIndex = isMatched ? 0 : 1
            };

            // Apply color coding based on duration
            if (callStackNode.DurationMs > 0)
            {
                treeNode.ForeColor = callStackNode.DurationMs.GetDurationColor();
            }

            // Recursively add children
            foreach (var child in callStackNode.Children)
            {
                treeNode.Nodes.Add(BuildCallTreeNode(child));
            }

            return treeNode;
        }

        /// <summary>
        /// Populates the API Tree with unique API methods and their invocations.
        /// </summary>
        /// <param name="apiNodes">List of unique API methods with invocation details.</param>
        /// <param name="expandFirstLevel">Whether to expand root nodes to show invocations.</param>
        /// <remarks>
        /// The API Tree groups all calls to the same method under one root node.
        /// Root nodes show: "MethodName (N calls)"
        /// Child nodes show: "MethodName � Ln 42" (one per invocation)
        /// </remarks>
        public void PopulateApiTree(List<ApiCallNode> apiNodes, bool expandFirstLevel = true)
        {
            if (apiNodes == null)
                apiNodes = new List<ApiCallNode>();

            _apiTreeView.BeginUpdate();
            _apiTreeView.Nodes.Clear();

            // Create top-most root node labeled "API Tree"
            var topRoot = new TreeNode("API Tree")
            {
                Tag = -1,
                NodeFont = new Font(_apiTreeView.Font, FontStyle.Bold)
            };

            // Add each unique API as a root node
            foreach (var apiNode in apiNodes)
            {
                var rootNode = new TreeNode(apiNode.GetDisplayText())
                {
                    Tag = apiNode,
                    ImageIndex = apiNode.AllCallsMatched ? 0 : 1,
                    SelectedImageIndex = apiNode.AllCallsMatched ? 0 : 1,
                    ToolTipText = $"{apiNode.ApiName}\n" +
                                 $"Total calls: {apiNode.TotalCalls}\n" +
                                 $"Avg duration: {apiNode.AvgDurationMs:F1} ms\n" +
                                 $"Min: {apiNode.MinDurationMs} ms\n" +
                                 $"Max: {apiNode.MaxDurationMs} ms"
                };

                // Add child nodes for each invocation
                foreach (int lineNumber in apiNode.LineNumbers)
                {
                    var child = new TreeNode($"{apiNode.ApiName} � Ln {lineNumber}")
                    {
                        Tag = lineNumber,
                        ImageIndex = apiNode.AllCallsMatched ? 0 : 1,
                        SelectedImageIndex = apiNode.AllCallsMatched ? 0 : 1
                    };
                    rootNode.Nodes.Add(child);
                }

                topRoot.Nodes.Add(rootNode);
            }

            _apiTreeView.Nodes.Add(topRoot);
            topRoot.Expand();

            // Optionally expand all API nodes to show invocations
            if (expandFirstLevel)
            {
                foreach (TreeNode node in topRoot.Nodes)
                {
                    node.Expand();
                }
            }

            _apiTreeView.EndUpdate();
        }

        /// <summary>
        /// Expands all nodes in both trees with progress reporting and cancellation support.
        /// </summary>
        /// <param name="progressCallback">Callback for progress updates.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task representing the async operation.</returns>
        /// <remarks>
        /// This operation can be slow for large trees (thousands of nodes).
        /// Progress updates and cancellation make it user-friendly.
        /// </remarks>
        public async Task ExpandAllNodesAsync(
            Action<int, string> progressCallback,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                // Expand Call Tree
                progressCallback?.Invoke(0, "Expanding Call Tree...");
                ExpandTreeRecursive(_callTreeView.Nodes, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                // Expand API Tree
                progressCallback?.Invoke(50, "Expanding API Tree...");
                ExpandTreeRecursive(_apiTreeView.Nodes, cancellationToken);

                progressCallback?.Invoke(100, "Expand complete");
            }, cancellationToken);
        }

        /// <summary>
        /// Recursively expands all nodes in a tree.
        /// </summary>
        private void ExpandTreeRecursive(TreeNodeCollection nodes, CancellationToken cancellationToken)
        {
            foreach (TreeNode node in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Expand on UI thread
                if (_callTreeView.InvokeRequired)
                {
                    _callTreeView.Invoke((Action)(() => node.Expand()));
                }
                else
                {
                    node.Expand();
                }

                // Recursively expand children
                if (node.Nodes.Count > 0)
                {
                    ExpandTreeRecursive(node.Nodes, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Collapses all nodes except the root in both trees.
        /// </summary>
        public void CollapseAllNodes()
        {
            _callTreeView.BeginUpdate();
            _apiTreeView.BeginUpdate();

            // Collapse all except root
            foreach (TreeNode root in _callTreeView.Nodes)
            {
                CollapseAllExceptRoot(root);
            }

            foreach (TreeNode root in _apiTreeView.Nodes)
            {
                CollapseAllExceptRoot(root);
            }

            _callTreeView.EndUpdate();
            _apiTreeView.EndUpdate();
        }

        /// <summary>
        /// Recursively collapses all children but keeps the root expanded.
        /// </summary>
        private void CollapseAllExceptRoot(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Collapse();
                CollapseAllExceptRoot(child);
            }
        }

        /// <summary>
        /// Switches between Call Tree and API Tree visibility.
        /// Only one tree can be visible at a time.
        /// </summary>
        /// <param name="showCallTree">True to show Call Tree; false to show API Tree.</param>
        public void SwitchTreeView(bool showCallTree)
        {
            _callTreeView.Visible = showCallTree;
            _apiTreeView.Visible = !showCallTree;
        }

        /// <summary>
        /// Gets the currently selected node from whichever tree is visible.
        /// </summary>
        /// <returns>Selected TreeNode or null if none selected.</returns>
        public TreeNode GetSelectedNode()
        {
            if (_callTreeView.Visible && _callTreeView.SelectedNode != null)
                return _callTreeView.SelectedNode;

            if (_apiTreeView.Visible && _apiTreeView.SelectedNode != null)
                return _apiTreeView.SelectedNode;

            return null;
        }

        /// <summary>
        /// Extracts the clean method name from a tree node's display text.
        /// Removes duration overlay, call count, and line number suffixes.
        /// </summary>
        /// <param name="node">The tree node.</param>
        /// <returns>
        /// Clean method name without decorations.
        /// Example: "CADSystem::OpenFile [142 ms]" ? "CADSystem::OpenFile"
        /// </returns>
        public string GetMethodName(TreeNode node)
        {
            if (node == null)
                return string.Empty;

            string text = node.Text;

            // Remove duration suffix like " [142 ms]"
            int bracketIndex = text.IndexOf(" [");
            if (bracketIndex > 0)
                return text.Substring(0, bracketIndex).Trim();

            // Remove call count suffix like " (3 calls)"
            int parenIndex = text.IndexOf(" (");
            if (parenIndex > 0)
                return text.Substring(0, parenIndex).Trim();

            // Remove line number suffix like " � Ln 123"
            int dashIndex = text.IndexOf(" � ");
            if (dashIndex > 0)
                return text.Substring(0, dashIndex).Trim();

            return text.Trim();
        }

        /// <summary>
        /// Finds and selects a node by method name in the currently visible tree.
        /// </summary>
        /// <param name="methodName">The method name to search for.</param>
        /// <returns>True if node was found and selected.</returns>
        public bool FindAndSelectNode(string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return false;

            TreeView activeTree = _callTreeView.Visible ? _callTreeView : _apiTreeView;

            foreach (TreeNode root in activeTree.Nodes)
            {
                if (FindNodeRecursive(root, methodName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Recursively searches for a node by method name.
        /// </summary>
        private bool FindNodeRecursive(TreeNode node, string methodName)
        {
            if (GetMethodName(node).Equals(methodName, StringComparison.OrdinalIgnoreCase))
            {
                TreeView parentTree = node.TreeView;
                parentTree.SelectedNode = node;
                node.EnsureVisible();
                return true;
            }

            foreach (TreeNode child in node.Nodes)
            {
                if (FindNodeRecursive(child, methodName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Auto-selects the first meaningful node in the currently visible tree.
        /// Skips the UI root node and selects the first actual method node.
        /// </summary>
        /// <remarks>
        /// Called after populating a tree to provide immediate context.
        /// Makes the UI feel more responsive and helpful.
        /// </remarks>
        public void AutoSelectFirstNode()
        {
            TreeView activeTree = _callTreeView.Visible ? _callTreeView : _apiTreeView;

            if (activeTree.Nodes.Count > 0)
            {
                var root = activeTree.Nodes[0];
                if (root.Nodes.Count > 0)
                {
                    // Select first child of root (skip the "Call Tree"/"API Tree" label node)
                    activeTree.SelectedNode = root.Nodes[0];
                    root.Nodes[0].EnsureVisible();
                }
            }
        }

        /// <summary>
        /// Gets the line number stored in a tree node's Tag property.
        /// </summary>
        /// <param name="node">The tree node.</param>
        /// <returns>Line number, or -1 if not available.</returns>
        /// <remarks>
        /// Tree nodes store their associated log line number in the Tag property.
        /// This allows clicking a node to jump to that line in the log view.
        /// 
        /// Special values:
        /// - -1: UI root node ("Call Tree" or "API Tree" label)
        /// - 0: No line number available
        /// - > 0: Valid line number
        /// </remarks>
        public int GetLineNumber(TreeNode node)
        {
            if (node?.Tag == null)
                return -1;

            if (node.Tag is int lineNumber)
                return lineNumber;

            if (node.Tag is ApiCallNode apiNode && apiNode.LineNumbers.Count > 0)
                return apiNode.LineNumbers[0];

            return -1;
        }

        /// <summary>
        /// Copies the selected subtree as indented text to clipboard.
        /// </summary>
        /// <param name="node">Root of the subtree to copy.</param>
        /// <returns>Formatted text string of the subtree.</returns>
        /// <example>
        /// Output format:
        /// Application::Start [1000 ms]
        ///   Database::Connect [200 ms]
        ///     Connection::Open [150 ms]
        ///   UI::ShowWindow [750 ms]
        /// </example>
        public string GetSubtreeAsText(TreeNode node)
        {
            if (node == null)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            AppendSubtreeText(node, sb, 0);
            return sb.ToString();
        }

        /// <summary>
        /// Recursively builds indented text representation of a subtree.
        /// </summary>
        private void AppendSubtreeText(TreeNode node, System.Text.StringBuilder sb, int depth)
        {
            // Indent by depth (2 spaces per level)
            sb.Append("  ".Repeat(depth));
            sb.AppendLine(node.Text);

            foreach (TreeNode child in node.Nodes)
            {
                AppendSubtreeText(child, sb, depth + 1);
            }
        }

        /// <summary>
        /// Clears all nodes from both trees.
        /// </summary>
        public void ClearAllTrees()
        {
            _callTreeView.Nodes.Clear();
            _apiTreeView.Nodes.Clear();
        }

        /// <summary>
        /// Gets the total number of nodes in the currently visible tree (excluding UI root).
        /// </summary>
        /// <returns>Total node count.</returns>
        public int GetTotalNodeCount()
        {
            TreeView activeTree = _callTreeView.Visible ? _callTreeView : _apiTreeView;

            if (activeTree.Nodes.Count == 0)
                return 0;

            // Count all nodes except the UI root
            var root = activeTree.Nodes[0];
            return CountNodesRecursive(root);
        }

        /// <summary>
        /// Recursively counts all nodes in a subtree.
        /// </summary>
        private int CountNodesRecursive(TreeNode node)
        {
            int count = 1; // Count this node

            foreach (TreeNode child in node.Nodes)
            {
                count += CountNodesRecursive(child);
            }

            return count;
        }
    }
}
