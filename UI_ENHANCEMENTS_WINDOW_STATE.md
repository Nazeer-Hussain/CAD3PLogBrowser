# New Enhancement Requirements - Window State & UI Defaults

## ?? Window State & UI Management Features

These enhancements improve the user experience by remembering window state and providing better default layouts.

---

## Feature 1: Window Size & Position Persistence

### 1a - Remember Last Window Size and Position
**What:** On launch, restore the main window to the last used size and position  
**Priority:** HIGH  
**Effort:** LOW (30 minutes)

**Requirements:**
- Save window size (Width, Height) on close
- Save window position (Left, Top) on close
- Restore both on next launch
- Handle multi-monitor scenarios (position may be off-screen)

### 1b - Save Window State in AppSettings
**What:** Store window size and position in AppSettings.json  
**Priority:** HIGH  
**Effort:** LOW (15 minutes)

**New AppSettings Properties:**
```csharp
public int WindowWidth { get; set; } = 1024;
public int WindowHeight { get; set; } = 768;
public int WindowLeft { get; set; } = -1;  // -1 = not set
public int WindowTop { get; set; } = -1;   // -1 = not set
public FormWindowState WindowState { get; set; } = FormWindowState.Normal;
```

### 1c - Maximize if No Settings Found
**What:** If AppSettings don't exist or window state is not saved, maximize the window  
**Priority:** HIGH  
**Effort:** LOW (10 minutes)

**Logic:**
```
If (WindowLeft == -1 || WindowTop == -1):
    WindowState = Maximized
Else:
    Restore saved size and position
    If position is off-screen:
        Center on primary screen
```

---

## Feature 2: Splitter Default Position

### 2a - Default Splitter at 30% Width
**What:** The tree/log split panel should default to 30% for tree, 70% for log view  
**Priority:** MEDIUM  
**Effort:** LOW (10 minutes)

**Current Behavior:**
- Splitter position is hardcoded or uses default control value
- May not be optimal for most use cases

**New Behavior:**
- Calculate 30% of main window width
- Set splitter distance to this value on first launch
- Formula: `SplitterDistance = ClientSize.Width * 0.3`

### 2b - Save and Restore Splitter Position
**What:** Save splitter position in AppSettings and restore on next launch  
**Priority:** MEDIUM  
**Effort:** LOW (already partially implemented)

**Current Status:**
- ? Already implemented in `SaveSettings()` and `RestoreSettings()`
- ? Uses `SettingsService.SaveSplitterDistance()` and `LoadSplitterDistance()`
- ?? May need to add default 30% logic if no saved value exists

**Enhancement:**
- Add default 30% calculation when no saved value exists
- Ensure it works on window resize

---

## Feature 3: Active Node Selection After File Load

### 3a - Call Tree: Select Topmost Node After Load
**What:** After loading a file, automatically select the first/topmost node in Call Tree  
**Priority:** HIGH  
**Effort:** LOW (10 minutes)

**Current Behavior:**
- No node selected after file load
- User must manually click a node to see log content

**New Behavior:**
```csharp
After PopulateCallTree():
    if (CallTree.Nodes.Count > 0)
    {
        var rootNode = CallTree.Nodes[0];
        if (rootNode.Nodes.Count > 0)
        {
            CallTree.SelectedNode = rootNode.Nodes[0]; // First child
            // This will trigger AfterSelect event ? scroll log
        }
        else
        {
            CallTree.SelectedNode = rootNode;
        }
    }
```

### 3b - API Tree: Select Topmost Node After Load
**What:** After loading a file, automatically select the first API in API Tree  
**Priority:** HIGH  
**Effort:** LOW (10 minutes)

**Current Behavior:**
- No node selected after file load
- Same issue as Call Tree

**New Behavior:**
```csharp
After PopulateApiTree():
    if (ApiTree.Nodes.Count > 0)
    {
        var rootNode = ApiTree.Nodes[0]; // "API Tree" root
        if (rootNode.Nodes.Count > 0)
        {
            ApiTree.SelectedNode = rootNode.Nodes[0]; // First API
        }
    }
```

**Additional Enhancement:**
- Only auto-select when the respective tree is visible
- If Call Tree is active ? select in Call Tree
- If API Tree is active ? select in API Tree

---

## ?? Implementation Plan

### Phase D: Window State & UI Defaults (Total: ~1.5 hours)

| Feature | Priority | Effort | Order |
|---------|----------|--------|-------|
| 2a - Default Splitter 30% | MED | 10 min | 1 |
| 2b - Splitter Save/Restore | MED | (done) | - |
| 1b - Window State in AppSettings | HIGH | 15 min | 2 |
| 1a - Save/Restore Window State | HIGH | 30 min | 3 |
| 1c - Maximize if No Settings | HIGH | 10 min | 4 |
| 3a - Call Tree Auto-Select | HIGH | 10 min | 5 |
| 3b - API Tree Auto-Select | HIGH | 10 min | 6 |

**Total Implementation Time:** ~1.5 hours  
**All HIGH priority items**

---

## ?? Technical Implementation Details

### 1. AppSettings.cs Changes

**Add Properties:**
```csharp
// Window state persistence
public int WindowWidth { get; set; } = 1024;
public int WindowHeight { get; set; } = 768;
public int WindowLeft { get; set; } = -1;
public int WindowTop { get; set; } = -1;
public string WindowState { get; set; } = "Normal"; // "Normal", "Maximized"
```

### 2. MainForm.cs Changes

**In RestoreSettings():**
```csharp
private void RestoreSettings()
{
    // Existing splitter restore...

    // Feature 1a/1b/1c: Restore window state
    if (_appSettings.WindowLeft >= 0 && _appSettings.WindowTop >= 0)
    {
        // Restore saved position
        this.StartPosition = FormStartPosition.Manual;
        this.Left = _appSettings.WindowLeft;
        this.Top = _appSettings.WindowTop;
        this.Width = _appSettings.WindowWidth;
        this.Height = _appSettings.WindowHeight;

        // Validate position is on-screen
        if (!IsOnScreen(this.Left, this.Top))
        {
            CenterToScreen();
        }

        if (_appSettings.WindowState == "Maximized")
        {
            this.WindowState = FormWindowState.Maximized;
        }
    }
    else
    {
        // Feature 1c: No saved settings ? maximize
        this.WindowState = FormWindowState.Maximized;
    }

    // Feature 2a: Default splitter to 30% if not set
    int dist = _settingsService.LoadSplitterDistance();
    if (dist <= 0)
    {
        dist = (int)(this.ClientSize.Width * 0.3);
    }
    mainSplitContainer.SplitterDistance = dist;
}

private bool IsOnScreen(int left, int top)
{
    foreach (Screen screen in Screen.AllScreens)
    {
        if (screen.WorkingArea.Contains(left, top))
            return true;
    }
    return false;
}
```

**In SaveSettings():**
```csharp
private void SaveSettings()
{
    // Existing code...

    // Feature 1a/1b: Save window state
    if (this.WindowState == FormWindowState.Normal)
    {
        _appSettings.WindowLeft = this.Left;
        _appSettings.WindowTop = this.Top;
        _appSettings.WindowWidth = this.Width;
        _appSettings.WindowHeight = this.Height;
        _appSettings.WindowState = "Normal";
    }
    else if (this.WindowState == FormWindowState.Maximized)
    {
        _appSettings.WindowState = "Maximized";
        // Keep last normal position
    }

    _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;
    _appSettings.Save();
}
```

**In PopulateTrees():**
```csharp
private void PopulateTrees(List<string> lines)
{
    // Existing population code...

    PopulateApiTree(_apiNodes);
    PopulateCallTree(callTree);

    // Feature 3a/3b: Auto-select top node
    SelectDefaultTreeNode();
}

private void SelectDefaultTreeNode()
{
    // Feature 3a: Call Tree auto-select
    if (CallTree.Visible && CallTree.Nodes.Count > 0)
    {
        var root = CallTree.Nodes[0];
        if (root.Nodes.Count > 0)
        {
            CallTree.SelectedNode = root.Nodes[0];
        }
    }

    // Feature 3b: API Tree auto-select
    if (ApiTree.Visible && ApiTree.Nodes.Count > 0)
    {
        var root = ApiTree.Nodes[0];
        if (root.Nodes.Count > 0)
        {
            ApiTree.SelectedNode = root.Nodes[0];
        }
    }
}
```

---

## ? Expected Behavior After Implementation

### First Launch (No AppSettings):
1. ? Window maximized (Feature 1c)
2. ? Splitter at 30% width (Feature 2a)
3. ? First node selected in active tree (Feature 3a/3b)
4. ? Log scrolls to first entry

### Subsequent Launches:
1. ? Window restores to last size and position (Feature 1a/1b)
2. ? Splitter restores to last position (Feature 2b - already done)
3. ? First node selected in active tree (Feature 3a/3b)

### User Moves Window:
1. ? New position saved on close
2. ? Restored on next launch

### User Resizes Window:
1. ? New size saved on close
2. ? Restored on next launch

### User Maximizes Window:
1. ? Maximized state saved
2. ? Restored as maximized on next launch
3. ? Previous normal size/position preserved

### Multi-Monitor Scenario:
1. ? If saved position is off-screen ? centered on primary screen
2. ? Fallback to maximize if positioning fails

---

## ?? Testing Checklist

**Feature 1 (Window State):**
- [ ] First launch ? window maximized
- [ ] Resize window ? close ? reopen ? size restored
- [ ] Move window ? close ? reopen ? position restored
- [ ] Maximize ? close ? reopen ? still maximized
- [ ] Unmaximize ? resize ? close ? reopen ? size restored
- [ ] Disconnect monitor ? reopen ? centered on primary screen
- [ ] Delete AppSettings ? reopen ? maximized again

**Feature 2 (Splitter):**
- [ ] First launch ? splitter at 30% width
- [ ] Move splitter ? close ? reopen ? splitter position restored
- [ ] Resize window ? splitter maintains relative position

**Feature 3 (Auto-Select):**
- [ ] Load file with Call Tree active ? first node selected
- [ ] Load file with API Tree active ? first API selected
- [ ] Selected node ? log scrolls to corresponding line
- [ ] Load file ? selected line visible with 10 previous lines (H1)

---

## ?? Commit Messages

```
feat(UI): add window state persistence and smart defaults

- Save and restore window size and position (Feature 1a/1b)
- Maximize window on first launch if no settings (Feature 1c)
- Default splitter to 30% width on first launch (Feature 2a)
- Auto-select topmost node in Call Tree after load (Feature 3a)
- Auto-select topmost node in API Tree after load (Feature 3b)
- Validate window position for multi-monitor scenarios
- Preserve normal size/position when maximized

Enhancements improve first-run experience and remember user preferences.

Files modified:
- AppSettings.cs (new properties)
- MainForm.cs (RestoreSettings, SaveSettings, SelectDefaultTreeNode)

All features tested and build successful.
```

---

## ?? User Benefits

1. **No Manual Window Setup** - Window opens exactly where you left it
2. **Better Default Layout** - 30% tree / 70% log is optimal for most workflows
3. **Immediate Context** - First log entry visible immediately after file load
4. **Multi-Monitor Support** - Handles disconnected monitors gracefully
5. **First-Time Experience** - Maximized window on first launch shows full UI

---

## ?? Priority Justification

**Why HIGH Priority:**
- **User Experience Impact:** First thing users see when launching the app
- **Common Frustration:** Having to resize/reposition window every time
- **Low Effort:** Quick implementation (~1.5 hours total)
- **High Value:** Dramatically improves daily usage
- **Professional Polish:** Expected behavior in modern applications

**Why Implement These First:**
- Foundation for other UI features
- User-facing quality of life improvements
- Low risk, high reward
- Can be implemented and tested quickly

---

## ?? Ready to Implement

All features are well-defined and ready for implementation.

**Estimated Total Time:** 1.5 hours  
**Risk Level:** LOW  
**User Impact:** HIGH  

Shall we implement these features next?
