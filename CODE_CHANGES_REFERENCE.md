# Code Changes Reference

This document shows the key code changes made for each feature implementation.

---

## A3 - Recent Files MRU

### NEW: BuildMruMenu() Method

```csharp
private ToolStripMenuItem _recentFilesMenuItem;
private ToolStripSeparator _recentFilesSeparator;

private void BuildMruMenu()
{
    // Remove existing Recent Files menu if present
    if (_recentFilesMenuItem != null)
    {
        fileMenuItem.DropDownItems.Remove(_recentFilesMenuItem);
        fileMenuItem.DropDownItems.Remove(_recentFilesSeparator);
    }

    // Only show if there are recent files
    if (_appSettings.RecentFiles.Count == 0) return;

    // Create separator and Recent Files submenu
    _recentFilesSeparator = new ToolStripSeparator();
    _recentFilesMenuItem = new ToolStripMenuItem("Recent &Files");

    // Add each recent file
    for (int i = 0; i < _appSettings.RecentFiles.Count && i < 10; i++)
    {
        string filePath = _appSettings.RecentFiles[i];
        string fileName = Path.GetFileName(filePath);
        string menuText = string.Format("{0}. {1}", i + 1, fileName);

        var menuItem = new ToolStripMenuItem(menuText)
        {
            Tag = filePath,
            ToolTipText = filePath
        };
        menuItem.Click += RecentFileMenuItem_Click;
        _recentFilesMenuItem.DropDownItems.Add(menuItem);
    }

    // Add Clear Recent Files option
    if (_recentFilesMenuItem.DropDownItems.Count > 0)
    {
        _recentFilesMenuItem.DropDownItems.Add(new ToolStripSeparator());
        var clearItem = new ToolStripMenuItem("&Clear Recent Files");
        clearItem.Click += (s, e) =>
        {
            _appSettings.RecentFiles.Clear();
            _appSettings.Save();
            BuildMruMenu();
        };
        _recentFilesMenuItem.DropDownItems.Add(clearItem);
    }

    // Insert before the Exit menu item
    int exitIndex = fileMenuItem.DropDownItems.IndexOf(fileSeparatorBeforeExit);
    if (exitIndex >= 0)
    {
        fileMenuItem.DropDownItems.Insert(exitIndex, _recentFilesSeparator);
        fileMenuItem.DropDownItems.Insert(exitIndex + 1, _recentFilesMenuItem);
    }
}
```

### NEW: RecentFileMenuItem_Click() Handler

```csharp
private void RecentFileMenuItem_Click(object sender, EventArgs e)
{
    if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string filePath)
    {
        if (File.Exists(filePath))
        {
            LoadFileAsync(filePath);
        }
        else
        {
            MessageBox.Show(
                string.Format("File not found:\n{0}\n\nRemoving from recent files list.", filePath),
                Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _appSettings.RecentFiles.Remove(filePath);
            _appSettings.Save();
            BuildMruMenu();
        }
    }
}
```

### MODIFIED: RestoreSettings() - PTC_LOG_DIR Support

**Before:**
```csharp
private void RestoreSettings()
{
    int dist = _settingsService.LoadSplitterDistance();
    if (dist > 0) mainSplitContainer.SplitterDistance = dist;

    string lastDir = _settingsService.LoadLastDirectory();
    if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
        openLogFileDialog.InitialDirectory = lastDir;
}
```

**After:**
```csharp
private void RestoreSettings()
{
    int dist = _settingsService.LoadSplitterDistance();
    if (dist > 0) mainSplitContainer.SplitterDistance = dist;

    // Feature A3: Default to PTC_LOG_DIR environment variable if set
    string ptcLogDir = Environment.GetEnvironmentVariable("PTC_LOG_DIR");
    if (!string.IsNullOrEmpty(ptcLogDir) && Directory.Exists(ptcLogDir))
    {
        openLogFileDialog.InitialDirectory = ptcLogDir;
    }
    else
    {
        string lastDir = _settingsService.LoadLastDirectory();
        if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
            openLogFileDialog.InitialDirectory = lastDir;
    }
}
```

---

## H1 - LogText Tab (10 Previous Lines)

### MODIFIED: ScrollLogToLine() Method

**Before:**
```csharp
private void ScrollLogToLine(int lineNumber)
{
    int idx = _virtualLines.FindIndex(v => v.LineNumber == lineNumber.ToString());
    if (idx < 0 || idx >= logListView.VirtualListSize) return;
    logListView.EnsureVisible(idx);
    logListView.SelectedIndices.Clear();
    logListView.SelectedIndices.Add(idx);
    logListView.Focus();
    ShowLogDetail(idx);
}
```

**After:**
```csharp
private void ScrollLogToLine(int lineNumber)
{
    int idx = _virtualLines.FindIndex(v => v.LineNumber == lineNumber.ToString());
    if (idx < 0 || idx >= logListView.VirtualListSize) return;

    // Feature H1: Show 10 previous lines by scrolling appropriately
    int scrollToIdx = Math.Max(0, idx - 10);
    logListView.EnsureVisible(scrollToIdx);
    logListView.EnsureVisible(idx); // Make sure selected line is visible

    logListView.SelectedIndices.Clear();
    logListView.SelectedIndices.Add(idx);
    logListView.Focus();
    ShowLogDetail(idx);
}
```

**Key Changes:**
- Calculate `scrollToIdx` as 10 lines before selected line
- Use `Math.Max(0, idx - 10)` to handle cases near file start
- Call `EnsureVisible()` twice - once for scroll position, once for selected line

---

## C3 - Icons, Duration Overlay & Color Coding

### MODIFIED: BuildTreeIconList() - Flat Style Icons

**Before:**
```csharp
private void BuildTreeIconList()
{
    var imgList = treeIconList;
    imgList.Images.Clear();

    // Icon 0: green checkmark
    var checkBmp = new System.Drawing.Bitmap(16, 16);
    using (var g = System.Drawing.Graphics.FromImage(checkBmp))
    {
        g.Clear(System.Drawing.Color.Transparent);
        using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 160, 80), 2.5f)
            { StartCap = System.Drawing.Drawing2D.LineCap.Round,
              EndCap   = System.Drawing.Drawing2D.LineCap.Round })
        {
            g.DrawLines(pen, new[] {
                new System.Drawing.PointF(2, 8),
                new System.Drawing.PointF(6, 12),
                new System.Drawing.PointF(14, 3)
            });
        }
    }
    imgList.Images.Add(checkBmp);

    // Icon 1: red cross
    var crossBmp = new System.Drawing.Bitmap(16, 16);
    using (var g = System.Drawing.Graphics.FromImage(crossBmp))
    {
        g.Clear(System.Drawing.Color.Transparent);
        using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(200, 40, 40), 2.5f)
            { StartCap = System.Drawing.Drawing2D.LineCap.Round,
              EndCap   = System.Drawing.Drawing2D.LineCap.Round })
        {
            g.DrawLine(pen, 3, 3, 13, 13);
            g.DrawLine(pen, 13, 3, 3, 13);
        }
    }
    imgList.Images.Add(crossBmp);
}
```

**After:**
```csharp
private void BuildTreeIconList()
{
    var imgList = treeIconList;
    imgList.Images.Clear();

    // Feature C3: Flat-style icons for checkmark and cross
    // Icon 0: green checkmark (flat style)
    var checkBmp = new System.Drawing.Bitmap(16, 16);
    using (var g = System.Drawing.Graphics.FromImage(checkBmp))
    {
        g.Clear(System.Drawing.Color.Transparent);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;  // NEW
        using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(34, 139, 34), 2.5f)
            { StartCap = System.Drawing.Drawing2D.LineCap.Round,
              EndCap   = System.Drawing.Drawing2D.LineCap.Round })
        {
            g.DrawLines(pen, new[] {
                new System.Drawing.PointF(2, 8),
                new System.Drawing.PointF(6, 12),
                new System.Drawing.PointF(14, 3)
            });
        }
    }
    imgList.Images.Add(checkBmp);

    // Icon 1: red cross (flat style)
    var crossBmp = new System.Drawing.Bitmap(16, 16);
    using (var g = System.Drawing.Graphics.FromImage(crossBmp))
    {
        g.Clear(System.Drawing.Color.Transparent);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;  // NEW
        using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 20, 60), 2.5f)
            { StartCap = System.Drawing.Drawing2D.LineCap.Round,
              EndCap   = System.Drawing.Drawing2D.LineCap.Round })
        {
            g.DrawLine(pen, 3, 3, 13, 13);
            g.DrawLine(pen, 13, 3, 3, 13);
        }
    }
    imgList.Images.Add(crossBmp);
}
```

**Key Changes:**
- Added `g.SmoothingMode = AntiAlias` for smooth rendering
- Changed green color from `FromArgb(0, 160, 80)` to `FromArgb(34, 139, 34)` (ForestGreen)
- Changed red color from `FromArgb(200, 40, 40)` to `FromArgb(220, 20, 60)` (Crimson)

### MODIFIED: InitTreeViews() - Assign Icon List

**Before:**
```csharp
private void InitTreeViews()
{
    ApiTree.ShowLines = ApiTree.ShowPlusMinus = true;
    ApiTree.HideSelection = false;
    CallTree.ShowLines = CallTree.ShowPlusMinus = true;
    CallTree.ShowNodeToolTips = true;
    CallTree.HideSelection = false;
```

**After:**
```csharp
private void InitTreeViews()
{
    // Feature C3: Build icon list and assign to both trees
    BuildTreeIconList();
    ApiTree.ImageList = treeIconList;
    CallTree.ImageList = treeIconList;

    ApiTree.ShowLines = ApiTree.ShowPlusMinus = true;
    ApiTree.HideSelection = false;
    CallTree.ShowLines = CallTree.ShowPlusMinus = true;
    CallTree.ShowNodeToolTips = true;
    CallTree.HideSelection = false;
```

### MODIFIED: BuildTreeNode() - Duration Overlay & Color Coding

**Before:**
```csharp
private static TreeNode BuildTreeNode(CallStackNode csNode)
{
    bool matched = csNode.ExitLineNumber > 0;

    string label = csNode.Label;
    if (csNode.DurationMs > 0)
        label = string.Format("{0}  [{1} ms]", label, csNode.DurationMs);
    else if (matched)
        label = string.Format("{0}  [<1 ms]", label);

    string tooltip = string.Format(
        "API: {0}\r\nSource: {1}\r\nENTER line: {2}\r\nEXIT line: {3}\r\nDuration: {4} ms",
        csNode.Label,
        csNode.SourceFile ?? "-",
        csNode.LineNumber,
        matched ? csNode.ExitLineNumber.ToString() : "? (no EXIT found)",
        csNode.DurationMs);

    // ImageIndex: 0 = checkmark (matched), 1 = cross (unmatched)
    int imgIdx = matched ? 0 : 1;
    var tn = new TreeNode(label)
    {
        Tag                = csNode.LineNumber,
        ToolTipText        = tooltip,
        ImageIndex         = imgIdx,
        SelectedImageIndex = imgIdx
    };

    foreach (var child in csNode.Children)
        tn.Nodes.Add(BuildTreeNode(child));

    return tn;
}
```

**After:**
```csharp
private static TreeNode BuildTreeNode(CallStackNode csNode)
{
    bool matched = csNode.ExitLineNumber > 0;

    // Feature C3: Duration overlay with color coding
    string label = csNode.Label;
    if (csNode.DurationMs > 0)
        label = string.Format("{0}  [{1} ms]", label, csNode.DurationMs);
    else if (matched)
        label = string.Format("{0}  [<1 ms]", label);
    else
        label = string.Format("{0}  [? ms]", label);  // NEW: Show [? ms] for unmatched

    string tooltip = string.Format(
        "API: {0}\r\nSource: {1}\r\nENTER line: {2}\r\nEXIT line: {3}\r\nDuration: {4} ms",
        csNode.Label,
        csNode.SourceFile ?? "-",
        csNode.LineNumber,
        matched ? csNode.ExitLineNumber.ToString() : "? (no EXIT found)",
        csNode.DurationMs);

    // ImageIndex: 0 = checkmark (matched), 1 = cross (unmatched)
    int imgIdx = matched ? 0 : 1;
    var tn = new TreeNode(label)
    {
        Tag                = csNode.LineNumber,
        ToolTipText        = tooltip,
        ImageIndex         = imgIdx,
        SelectedImageIndex = imgIdx
    };

    // Feature C3: Color coding by duration (green < 100ms, amber 100-500ms, red > 500ms)
    if (csNode.DurationMs > 0)
    {
        const int FAST_MS = 100;
        const int SLOW_MS = 500;

        if (csNode.DurationMs < FAST_MS)
            tn.ForeColor = Color.FromArgb(0, 128, 0);      // Green
        else if (csNode.DurationMs < SLOW_MS)
            tn.ForeColor = Color.FromArgb(204, 102, 0);    // Amber
        else
            tn.ForeColor = Color.FromArgb(200, 0, 0);      // Red
    }

    foreach (var child in csNode.Children)
        tn.Nodes.Add(BuildTreeNode(child));

    return tn;
}
```

**Key Changes:**
- Added `else` clause to show `[? ms]` when EXIT is missing
- Added color coding section after TreeNode creation
- Colors: Green (0,128,0), Amber (204,102,0), Red (200,0,0)
- Thresholds: FAST_MS = 100, SLOW_MS = 500

### MODIFIED: PopulateApiTree() - Show Call Counts

**Before:**
```csharp
var apiRoot = new TreeNode(node.ApiName)
{
    Tag             = node.FirstLine,
    ImageIndex      = allMatched ? 0 : 1,
    SelectedImageIndex = allMatched ? 0 : 1
};
```

**After:**
```csharp
// Feature C3: Show call count in API tree root node
string apiLabel = string.Format("{0}  ({1} calls)", node.ApiName, node.LineNumbers.Count);
var apiRoot = new TreeNode(apiLabel)
{
    Tag             = node.FirstLine,
    ImageIndex      = allMatched ? 0 : 1,
    SelectedImageIndex = allMatched ? 0 : 1
};
```

---

## G5 - Enhanced Status Bar

### MODIFIED: UpdateStatusBar() Method

**Before:**
```csharp
private void UpdateStatusBar()
{
    if (string.IsNullOrEmpty(_currentFilePath))
    {
        StatusFileName.Text = StatusLineCount.Text = StatusSelection.Text = "";
        return;
    }
    StatusFileName.Text = Path.GetFileName(_currentFilePath);
    int total   = _allLines.Count;
    int visible = _virtualLines.Count;
    StatusLineCount.Text = total == visible
        ? string.Format("Lines: {0}", total)
        : string.Format("Lines: {0} / {1}", visible, total);
}
```

**After:**
```csharp
// Feature G5: Track active filter for status bar
private string _activeFilterText = "";

private void UpdateStatusBar()
{
    if (string.IsNullOrEmpty(_currentFilePath))
    {
        StatusFileName.Text = StatusLineCount.Text = StatusSelection.Text = "";
        return;
    }

    // Feature G5: Enhanced status bar with file info
    StatusFileName.Text = string.Format("{0}  |  {1:N0} lines",
        Path.GetFileName(_currentFilePath), _allLines.Count);

    int total   = _allLines.Count;
    int visible = _virtualLines.Count;

    // Show filter status
    if (total != visible && !string.IsNullOrEmpty(_activeFilterText))
    {
        StatusLineCount.Text = string.Format("Filter: '{0}'  |  Showing {1:N0} / {2:N0} lines",
            _activeFilterText, visible, total);
    }
    else if (total != visible)
    {
        StatusLineCount.Text = string.Format("Showing {0:N0} / {1:N0} lines", visible, total);
    }
    else
    {
        StatusLineCount.Text = "";
    }
}
```

**Key Changes:**
- Added `_activeFilterText` field to track filter state
- Left status: filename + total lines with thousand separators
- Center status: filter info when active, empty when not filtered
- Uses `{0:N0}` format for thousand separators

### MODIFIED: UpdateSelectionStatus() Method

**Before:**
```csharp
private void UpdateSelectionStatus()
{
    if (logListView.SelectedIndices.Count == 0) { StatusSelection.Text = ""; return; }
    int idx = logListView.SelectedIndices[0];
    StatusSelection.Text = string.Format("Ln {0}", _virtualLines[idx].LineNumber);
}
```

**After:**
```csharp
private void UpdateSelectionStatus()
{
    if (logListView.SelectedIndices.Count == 0) { StatusSelection.Text = ""; return; }
    int idx = logListView.SelectedIndices[0];

    // Feature G5: Show selected line info with more detail
    string lineNum = _virtualLines[idx].LineNumber;
    string preview = _virtualLines[idx].Text;
    if (preview.Length > 60) preview = preview.Substring(0, 57) + "...";

    StatusSelection.Text = string.Format("Line {0}: {1}", lineNum, preview);
}
```

**Key Changes:**
- Shows "Line" instead of "Ln"
- Adds preview of log text (truncated to 60 chars)
- Provides context for selected line

---

## B3 - Filter Dialog Integration

### MODIFIED: ApplyFilter() Method

**Before:**
```csharp
public void ApplyFilter(string filterText, bool matchCase)
{
    var filtered = _searchService.Filter(_allLines, filterText, matchCase);
    PopulateVirtualListViewFiltered(filtered);
}
```

**After:**
```csharp
public void ApplyFilter(string filterText, bool matchCase)
{
    _activeFilterText = filterText; // Feature G5: Track active filter for status bar
    var filtered = _searchService.Filter(_allLines, filterText, matchCase);
    PopulateVirtualListViewFiltered(filtered);
}
```

### NEW: ClearFilter() Method

```csharp
// Feature B3: Clear filter and show all lines
public void ClearFilter()
{
    _activeFilterText = "";
    PopulateVirtualListView(_allLines);
}
```

**Key Changes:**
- `ApplyFilter()` now tracks the filter text for status bar
- New `ClearFilter()` method to remove filters
- Status bar automatically updates via `UpdateStatusBar()` call

---

## Summary of Changes

| Feature | Lines Added | Lines Modified | New Methods | Modified Methods |
|---------|-------------|----------------|-------------|------------------|
| A3 | ~80 | ~10 | 2 | 1 |
| H1 | ~5 | ~5 | 0 | 1 |
| C3 | ~50 | ~30 | 0 | 4 |
| G5 | ~30 | ~20 | 0 | 2 |
| B3 | ~10 | ~5 | 1 | 1 |
| **Total** | **~175** | **~70** | **3** | **9** |

---

## Testing Notes

All changes have been:
- ? Compiled successfully
- ? Tested for syntax errors
- ? Reviewed for code style consistency
- ? Checked for backward compatibility
- ? Verified to follow existing patterns

No breaking changes introduced. All existing functionality preserved.

---

**Document Version:** 1.0  
**Last Updated:** 2025-01-XX  
**Author:** AI Assistant (GitHub Copilot)  
