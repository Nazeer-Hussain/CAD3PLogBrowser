# ?? FEATURE ACCESSIBILITY AUDIT - Complete Analysis

## Overview

Analyzing all 77 features to ensure each has at least one access method:
- Menu item
- Toolbar button  
- Keyboard shortcut
- Context menu
- Programmatic access

---

## ? TOOLBAR BUTTONS FOUND (19)

```
1. OpenButton - Open file
2. SaveButton - Save file
3. SaveToXLSButton - Export to XLS
4. RefreshButton - Reload file
5. CopyButton - Copy
6. FindButton - Find dialog
7. FindNextButton - Find next
8. FilterButton - Filter dialog
9. ExpandAllButton - Expand all nodes
10. CollapseAllButton - Collapse all nodes
11. JumpToMatchingButton - Jump to matching ENTER/EXIT
12. prevErrorButton - Previous error
13. nextErrorButton - Next error
14. prevWarningButton - Previous warning
15. nextWarningButton - Next warning
16. CallTreeButton - Show call tree
17. ApiTreeButton - Show API tree
18. SettingsButton - Settings
19. ShowHelpButton - Help
```

---

## ? MENU ITEMS FOUND

### File Menu (9 items)
```
- Open
- Save As
- Save to XLS
- Export Performance CSV
- Export Tree JSON
- Export Tree XML
- Export Timeline Image
- Export Flame Graph Image
- Reload
- Recent Files (submenu)
- Exit
```

### Edit Menu (17 items)
```
- Copy
- Copy with Headers
- Find
- Find Next
- Find All
- Filter
- Clear Filter
- Expand All
- Collapse All
- Jump to Matching
- Toggle Bookmark
- Next Bookmark
- Previous Bookmark
- Show All Bookmarks
- Clear All Bookmarks
- Jump to Line
```

### View Menu (9 items)
```
- Show Call Tree
- Show API Tree
- Show Tabs (submenu with 6 tabs)
  - LogView
  - Performance View
  - Details
  - CallGraph
  - Flame Graph
  - Timeline
- Select Font
- Show Toolbar
```

### Options Menu (1 item)
```
- Settings
```

### Help Menu (5 items)
```
- Help CHM
- Keyboard Shortcuts
- About
- Check for Updates
- Report Errors
```

---

## ?? FEATURES WITHOUT MENU/TOOLBAR ACCESS

Let me check if any features are missing...

### Features That Might Need Access:

1. **Export Branch** - I see code for this but no menu item found
2. **Copy Tree Node** - Context menu only?
3. **Search in Grok** - Context menu only?
4. **Export Call Graph** - I see code but no menu item?
5. **Auto-reload on file change** - Automatic feature
6. **Virtual list mode** - Automatic feature
7. **Syntax highlighting** - Automatic feature

Let me verify these systematically...

