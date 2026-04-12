// ????????????????????????????????????????????????????????????????????????????
// NEW FEATURES IMPLEMENTATION - Add to MainForm.cs
// ????????????????????????????????????????????????????????????????????????????

// Add these using statements at the top if not present:
// using System.Linq;
// using System.Text;
// using Newtonsoft.Json; // For search history persistence

namespace Cad3PLogBrowser
{
    public partial class MainForm
    {
        // ???????????????????????????????????????????????????????????????????????
        // FEATURE 1: Copy Menu Item Handlers (CRITICAL)
        // ???????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Handles copy menu item click - copies selected log lines to clipboard.
        /// </summary>
        private void copyMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: false);
        }

        /// <summary>
        /// Handles context menu copy click.
        /// </summary>
        private void contextCopyMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: false);
        }

        /// <summary>
        /// Handles toolbar copy button click.
        /// </summary>
        private void CopyButton_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: false);
        }

        // ???????????????????????????????????????????????????????????????????????
        // FEATURE 3: Copy with Headers (I4)
        // ???????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Handles copy with headers menu item click.
        /// Copies selected lines with column headers in tab-separated format.
        /// </summary>
        private void contextCopyWithHeadersMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: true);
        }

        /// <summary>
        /// Copies selected log lines to clipboard.
        /// </summary>
        /// <param name="includeHeaders">If true, includes "Line #\tLog Text" header.</param>
        private void CopySelectedLinesToClipboard(bool includeHeaders)
        {
            try
            {
                if (logListView.SelectedIndices.Count == 0)
                {
                    StatusFileName.Text = "No lines selected";
                    return;
                }

                var sb = new StringBuilder();

                // Add header if requested
                if (includeHeaders)
                {
                    sb.AppendLine("Line #\tLog Text");
                    sb.AppendLine("------\t--------");
                }

                // Copy selected indices to array and sort
                var indices = new int[logListView.SelectedIndices.Count];
                logListView.SelectedIndices.CopyTo(indices, 0);
                Array.Sort(indices);

                // Get text for each selected line
                foreach (int index in indices)
                {
                    if (index >= 0 && index < logListView.VirtualListSize)
                    {
                        var item = logListView.Items[index];
                        if (includeHeaders)
                        {
                            // Tab-separated format: Line# \t Text
                            sb.AppendLine($"{item.Text}\t{item.SubItems[1].Text}");
                        }
                        else
                        {
                            // Just the log text
                            sb.AppendLine(item.SubItems[1].Text);
                        }
                    }
                }

                // Copy to clipboard
                if (sb.Length > 0)
                {
                    Clipboard.SetText(sb.ToString());

                    // Update status bar
                    string message = includeHeaders 
                        ? $"Copied {indices.Length} line(s) with headers to clipboard"
                        : $"Copied {indices.Length} line(s) to clipboard";

                    StatusFileName.Text = message;

                    // Clear status after 3 seconds
                    var timer = new Timer();
                    timer.Interval = 3000;
                    timer.Tick += (s, args) =>
                    {
                        StatusFileName.Text = Path.GetFileName(_currentFilePath);
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard:\n{ex.Message}", 
                    "Copy Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        // ???????????????????????????????????????????????????????????????????????
        // FEATURE 2: Search History Persistence (B6)
        // ???????????????????????????????????????????????????????????????????????

        private const string SEARCH_HISTORY_FILE = "search_history.json";
        private const int MAX_SEARCH_HISTORY = 20;

        /// <summary>
        /// Saves search history to JSON file.
        /// Call this when FindForm is closing or app is exiting.
        /// </summary>
        private void SaveSearchHistory(FindForm findForm)
        {
            try
            {
                // Get search history from FindForm combobox
                var searchHistory = new List<string>();

                if (findForm != null && findForm.SearchTextBox != null)
                {
                    foreach (var item in findForm.SearchTextBox.Items)
                    {
                        if (item != null)
                            searchHistory.Add(item.ToString());
                    }
                }

                // Limit to MAX_SEARCH_HISTORY items
                if (searchHistory.Count > MAX_SEARCH_HISTORY)
                    searchHistory = searchHistory.Take(MAX_SEARCH_HISTORY).ToList();

                // Save to JSON file in application directory
                string filePath = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    SEARCH_HISTORY_FILE);

                string json = System.Text.Json.JsonSerializer.Serialize(searchHistory, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                // Don't show error to user - just log it
                System.Diagnostics.Debug.WriteLine($"Failed to save search history: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads search history from JSON file.
        /// Call this when FindForm is created.
        /// </summary>
        private void LoadSearchHistory(FindForm findForm)
        {
            try
            {
                string filePath = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    SEARCH_HISTORY_FILE);

                if (!File.Exists(filePath))
                    return;

                string json = File.ReadAllText(filePath);
                var searchHistory = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);

                if (searchHistory != null && searchHistory.Count > 0 && findForm.SearchTextBox != null)
                {
                    findForm.SearchTextBox.Items.Clear();
                    foreach (var term in searchHistory)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                            findForm.SearchTextBox.Items.Add(term);
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't show error to user - just log it
                System.Diagnostics.Debug.WriteLine($"Failed to load search history: {ex.Message}");
            }
        }

        // ???????????????????????????????????????????????????????????????????????
        // FEATURE 4: Tree Search/Filter (C5)
        // ???????????????????????????????????????????????????????????????????????

        private string _treeSearchText = string.Empty;

        /// <summary>
        /// Filters tree nodes based on search text.
        /// </summary>
        /// <param name="searchText">Text to search for in node names.</param>
        private void FilterTreeNodes(string searchText)
        {
            _treeSearchText = searchText.ToLowerInvariant();

            TreeView activeTree = CallTree.Visible ? CallTree : ApiTree;

            if (string.IsNullOrWhiteSpace(_treeSearchText))
            {
                // Show all nodes
                ShowAllTreeNodes(activeTree);
                return;
            }

            // Filter nodes
            activeTree.BeginUpdate();

            foreach (TreeNode rootNode in activeTree.Nodes)
            {
                FilterTreeNodeRecursive(rootNode);
            }

            activeTree.EndUpdate();
        }

        /// <summary>
        /// Recursively filters tree nodes.
        /// A node is visible if it or any of its children match the search text.
        /// </summary>
        /// <returns>True if this node or any child matches.</returns>
        private bool FilterTreeNodeRecursive(TreeNode node)
        {
            bool hasMatch = false;
            bool nodeMatches = node.Text.ToLowerInvariant().Contains(_treeSearchText);

            // Check children first
            foreach (TreeNode child in node.Nodes)
            {
                if (FilterTreeNodeRecursive(child))
                    hasMatch = true;
            }

            // Show node if it matches or has matching children
            if (nodeMatches || hasMatch)
            {
                node.BackColor = nodeMatches ? Color.Yellow : Color.Transparent;
                // Note: TreeNode doesn't have Visible property
                // We'll expand matching nodes instead
                if (hasMatch && !nodeMatches)
                    node.Expand();
                return true;
            }
            else
            {
                node.BackColor = Color.Transparent;
                node.Collapse();
                return false;
            }
        }

        /// <summary>
        /// Shows all tree nodes (clears filter).
        /// </summary>
        private void ShowAllTreeNodes(TreeView tree)
        {
            tree.BeginUpdate();

            foreach (TreeNode rootNode in tree.Nodes)
            {
                ClearTreeNodeFilter(rootNode);
            }

            tree.EndUpdate();
        }

        /// <summary>
        /// Recursively clears filter highlighting from nodes.
        /// </summary>
        private void ClearTreeNodeFilter(TreeNode node)
        {
            node.BackColor = Color.Transparent;

            foreach (TreeNode child in node.Nodes)
            {
                ClearTreeNodeFilter(child);
            }
        }

        /// <summary>
        /// Handler for tree search textbox.
        /// Add this textbox above the tree in the designer.
        /// </summary>
        private void treeSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                FilterTreeNodes(textBox.Text);
            }
        }

        // ???????????????????????????????????????????????????????????????????????
        // FEATURE 5: Font Selection (H5)
        // ???????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Shows font selection dialog and applies chosen font to log view.
        /// </summary>
        private void SelectLogFont()
        {
            try
            {
                // Set current font in dialog
                logFontDialog.Font = logListView.Font;

                if (logFontDialog.ShowDialog() == DialogResult.OK)
                {
                    // Apply font to log list view
                    logListView.Font = logFontDialog.Font;

                    // Save to settings
                    if (_appSettings != null)
                    {
                        _appSettings.LogFontFamily = logFontDialog.Font.FontFamily.Name;
                        _appSettings.LogFontSize = logFontDialog.Font.Size;
                        _appSettings.LogFontStyle = logFontDialog.Font.Style;

                        // Save settings
                        _settingsService.SaveSettings(_appSettings);
                    }

                    StatusFileName.Text = $"Font changed to {logFontDialog.Font.Name} {logFontDialog.Font.Size}pt";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change font:\n{ex.Message}", 
                    "Font Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads saved font from settings.
        /// Call this during form initialization.
        /// </summary>
        private void LoadLogFont()
        {
            try
            {
                if (_appSettings != null && 
                    !string.IsNullOrEmpty(_appSettings.LogFontFamily))
                {
                    var font = new Font(
                        _appSettings.LogFontFamily,
                        _appSettings.LogFontSize > 0 ? _appSettings.LogFontSize : 9.0f,
                        _appSettings.LogFontStyle);

                    logListView.Font = font;
                }
            }
            catch (Exception ex)
            {
                // Use default font if loading fails
                System.Diagnostics.Debug.WriteLine($"Failed to load font: {ex.Message}");
            }
        }
    }
}

// ????????????????????????????????????????????????????????????????????????????
// APPSETINGS.CS ADDITIONS
// ????????????????????????????????????????????????????????????????????????????

// Add these properties to AppSettings class:

/// <summary>Font family for log view.</summary>
public string LogFontFamily { get; set; } = "Consolas";

/// <summary>Font size for log view.</summary>
public float LogFontSize { get; set; } = 9.0f;

/// <summary>Font style for log view.</summary>
public FontStyle LogFontStyle { get; set; } = FontStyle.Regular;

// ????????????????????????????????????????????????????????????????????????????
// FINDFORM.CS MODIFICATIONS
// ????????????????????????????????????????????????????????????????????????????

// Make SearchTextBox public so MainForm can access it:
// Change: internal ComboBox SearchTextBox;
// To:     public ComboBox SearchTextBox;

// Or add a public property:
public ComboBox SearchComboBox => SearchTextBox;

// ????????????????????????????????????????????????????????????????????????????
// MAINFORM CONSTRUCTOR MODIFICATIONS
// ????????????????????????????????????????????????????????????????????????????

// Add these calls in MainForm constructor after InitializeComponent():

public MainForm()
{
    InitializeComponent();

    // ... existing initialization code ...

    // Load saved font
    LoadLogFont();

    // Wire up event handlers
    WireUpNewEventHandlers();
}

private void WireUpNewEventHandlers()
{
    // Copy handlers
    copyMenuItem.Click += copyMenuItem_Click;
    contextCopyMenuItem.Click += contextCopyMenuItem_Click;
    contextCopyWithHeadersMenuItem.Click += contextCopyWithHeadersMenuItem_Click;
    CopyButton.Click += CopyButton_Click;
}

// ????????????????????????????????????????????????????????????????????????????
// MAINFORM_FORMCLOSING MODIFICATIONS
// ????????????????????????????????????????????????????????????????????????????

// Add this to save search history on exit:

protected override void OnFormClosing(FormClosingEventArgs e)
{
    // Save search history if FindForm exists
    if (_findForm != null)
    {
        SaveSearchHistory(_findForm);
    }

    base.OnFormClosing(e);
}

// ????????????????????????????????????????????????????????????????????????????
// SETTINGSFORM ADDITIONS (for Font Selection)
// ????????????????????????????????????????????????????????????????????????????

// Add a button in SettingsForm for font selection:
// <Button Name="selectFontButton" Text="Select Log Font..." />

private void selectFontButton_Click(object sender, EventArgs e)
{
    _mainForm.SelectLogFont(); // Expose SelectLogFont as public or internal
}

