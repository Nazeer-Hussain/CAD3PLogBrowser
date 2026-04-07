# Feature C1 Implementation - Expand/Collapse All + Jump to Matching

## ? Feature C1 Complete

### What Was Added

**Edit Menu Enhancements:**
```
Edit
  ?? Copy                         Ctrl+C
  ??????????????????????????????????????
  ?? Find                         Ctrl+F
  ?? Find Next                    F3
  ??????????????????????????????????????
  ?? Filter                       Ctrl+I
  ??????????????????????????????????????
  ?? Expand All                   Ctrl+E    ??? NEW
  ?? Collapse All                 Ctrl+W    ??? NEW
  ?? Jump to Matching ENTER/EXIT  Ctrl+G    ??? NEW
```

**Note:** Settings shortcut changed from Ctrl+E to Ctrl+Shift+S to avoid conflict.

### Implementation Details

**Files Modified:**
1. `MainForm.Designer.cs` - Added 3 new menu items + 1 separator
2. `MainForm.cs` - Added 3 menu event handlers

**Menu Items Added:**
- `expandAllMenuItem` - Expands all nodes in both Call Tree and API Tree
- `collapseAllMenuItem` - Collapses all nodes (keeps root nodes expanded)
- `jumpToMatchingMenuItem` - Wires existing JumpToMatchingPair() method

**Keyboard Shortcuts:**
- Ctrl+E - Expand All
- Ctrl+W - Collapse All
- Ctrl+G - Jump to Matching ENTER/EXIT

### Code Changes

**MainForm.cs:**
```csharp
// Feature C1: Menu event handlers
private void expandAllMenuItem_Click(object sender, EventArgs e) =>
    ExpandAllTrees();

private void collapseAllMenuItem_Click(object sender, EventArgs e) =>
    CollapseAllTrees();

private void jumpToMatchingMenuItem_Click(object sender, EventArgs e) =>
    JumpToMatchingPair();
```

**Existing Methods Wired:**
- `ExpandAllTrees()` - Already existed, now accessible via menu
- `CollapseAllTrees()` - Already existed, now accessible via menu
- `JumpToMatchingPair()` - Already existed (B9), now accessible via menu

### Testing

? Build successful (no errors)
? Menu items added to Edit menu
? Keyboard shortcuts assigned
? Existing functionality wired correctly

### Spec Compliance

| Requirement | Status | Notes |
|-------------|--------|-------|
| Expand All menu item | ? | Edit > Expand All (Ctrl+E) |
| Collapse All menu item | ? | Edit > Collapse All (Ctrl+W) |
| Keyboard shortcuts | ? | All shortcuts assigned |
| Wire existing methods | ? | No duplication, reuses existing code |

---

**Ready for Git commit.**
