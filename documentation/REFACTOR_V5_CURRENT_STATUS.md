# refactor_v5 - Current Status & Next Actions

**Date:** 2024-04-12 (Updated)  
**Branch:** refactor_v5  
**Build Status:** ? Clean (0 errors, 0 warnings)  

---

## ? FEATURES ALREADY COMPLETE (Found in Code)

### 1. Feature B10: Error/Warning Navigation ?
**Location:** MainForm.cs lines 1183-1186, 2055-2136, 1728-1753  
**Status:** Fully implemented
- Error/warning line indexing during file load
- Navigation methods (Next/Prev Error/Warning)
- Keyboard shortcuts (F8, Shift+F8, Ctrl+F8, Ctrl+Shift+F8)
- Status bar shows counts
- Toolbar button handlers present

### 2. Feature D3: API Invocation Details Panel ?
**Location:** MainForm.cs lines 1005-1051  
**Status:** Fully implemented
- ShowApiDetails() method
- Displays in Log Details tab
- Shows: invocations, first line, all lines, ENTER/EXIT status

### 3. Feature D6: Sort APIs in Tree ? (Code Complete)
**Location:** MainForm.cs lines 736-750, 873-887  
**Status:** Code complete, UI needed
- SortApiTreeBy() method exists
- ApiSortMode enum (ByName, ByCount, ByFirstLine)
- PopulateApiTree() respects sort order
- **Missing:** Toolbar buttons to trigger sort (UI Designer work)

---

## ?? NEXT TO IMPLEMENT (Priority Order)

### 1. Feature D5: Cross-Reference Jump API ? CallTree
**Priority:** HIGH  
**Complexity:** SMALL  
**Status:** Context menu exists, needs enhancement  

**Current State:**
- "Show in Other Tree" context menu exists (line 2663)
- FindAndSelectApiTreeNode() exists (line 2676)
- FindAndSelectCallTreeNode() exists (line 2686)
- ShowApiTree() and ShowCallTree() helpers exist

**What's Needed:**
- Enhance to automatically cross-reference on selection
- Add event handlers for AfterSelect in both trees
- Auto-highlight matching nodes when clicking

**Implementation Plan:**
```csharp
// In ApiTree_AfterSelect - add cross-highlight
private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e)
{
    ScrollLogToLine(e.Node?.Tag);
    ShowApiDetails(e.Node);
    // NEW: Find and highlight in Call Tree (if visible and split mode)
    if (CallTree.Visible && !IsSingleTreeMode)
        HighlightInCallTree(GetMethodNameFromNode(e.Node));
}

// In CallTree_AfterSelect - add cross-highlight
private void CallTree_AfterSelect(object sender, TreeViewEventArgs e)
{
    ScrollLogToLine(e.Node?.Tag);
    // NEW: Find and highlight in API Tree (if visible and split mode)
    if (ApiTree.Visible && !IsSingleTreeMode)
        HighlightInApiTree(GetMethodNameFromNode(e.Node));
}
```

### 2. Feature A6: Merge Log Files (Time-Sorted)
**Priority:** MEDIUM  
**Complexity:** MEDIUM  
**Status:** Menu item exists, stubbed out

**Current State:**
- mergeLogsMenuItem_Click exists (line 1471)
- Shows "Coming Soon" message
- OpenFileDialog for multiple files ready

**What's Needed:**
- Create MergeLogService class
- Parse timestamps from each file
- Merge and sort by timestamp
- Handle different timestamp formats
- Display merged result

### 3. Feature C2: Lazy Loading for Large Logs
**Priority:** MEDIUM  
**Complexity:** MEDIUM  

**What's Needed:**
- Detect large trees (50k+ nodes)
- Add placeholder nodes "..."
- Load children only on expand
- Progress indication during expansion

---

## ?? SUMMARY

**Total Features:** 79  
**Already Implemented:** 66 (including B10, D3, D6 code)  
**High Priority Remaining:** 3 (D5, A6, C2)  
**Build Status:** ? Clean  

**Immediate Next Action:** Implement Feature D5 (Cross-Reference)

---

**Last Updated:** 2024-04-12 15:00
