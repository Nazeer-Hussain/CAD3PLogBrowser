# Missing Features Implementation Plan

## STATUS SUMMARY
Based on analysis of the codebase, here are the **truly missing** features:

### Already Implemented (Previously marked as "missing")
- ? **A2** - PTC_LOG_DIR environment variable support (implemented in RestoreSettings)
- ? **B2** - Regex Search (UseRegexCheckBox exists in FindForm)
- ? **B7** - Find All Results Window (FindAllResultsForm already implemented)
- ? **F6** - Call Graph Export as Image (callGraphExportButton_Click implemented)

### Truly Missing Features

#### Phase A (File Operations)
- ? **A5** - Multi-file Tabs (0% - Complex feature, requires TabControl wrapper)

#### Phase B (Search, Filter & Navigation)
- ? **B4** - Time Range Filter (0% - needs new dialog)
- ? **B5** - Duration Threshold Filter (0% - needs UI)
- ? **B6** - Search History (0% - combo box already has Items, needs persistence)
- ? **B9** - Jump to Line Number (0% - needs InputBox dialog)

#### Phase C (Tree Operations)
- ? **C5** - Tree Search/Filter (0% - needs search box above tree)

#### Phase H (Log Display)
- ? **H5** - Font Selection (0% - needs Settings UI)

#### Phase I (Export & Save)
- ? **I2** - Save Selected Branch (0% - right-click menu item exists but needs implementation)
- ? **I3** - Export Performance to CSV (0% - menu item exists, needs implementation)
- ? **I4** - Copy with Headers (0% - needs context menu)

---

## IMPLEMENTATION PRIORITY

### HIGH PRIORITY (Quick Wins - 1-2 hours each)
1. **B9** - Jump to Line Number ? Already has JumpToLine method, just needs menu item
2. **I3** - Export Performance to CSV ? performanceView already exists
3. **B6** - Search History ? SearchTextBox ComboBox just needs persistence
4. **I4** - Copy with Headers ? Simple clipboard enhancement

### MEDIUM PRIORITY (2-3 hours each)
5. **B5** - Duration Threshold Filter ? InputBox + filter logic
6. **B4** - Time Range Filter ? New dialog with DateTimePicker
7. **I2** - Save Selected Branch ? Export subtree log lines
8. **C5** - Tree Search/Filter ? TextBox + filter tree nodes

### LOW PRIORITY (Complex - 4+ hours)
9. **H5** - Font Selection ? Settings dialog update + persistence
10. **A5** - Multi-file Tabs ? Major architectural change

---

## IMPLEMENTATION ORDER FOR THIS SESSION

### Batch 1: Quick Wins (30 minutes)
1. ? **B9** - Jump to Line Number
2. ? **I4** - Copy with Headers
3. ? **B6** - Search History Persistence

### Batch 2: Export Features (45 minutes)
4. ? **I3** - Export Performance to CSV
5. ? **I2** - Save Selected Branch

### Batch 3: Filtering (1 hour)
6. ? **B5** - Duration Threshold Filter
7. ? **B4** - Time Range Filter

### Batch 4: Advanced (1 hour)
8. ? **C5** - Tree Search/Filter
9. ? **H5** - Font Selection

### Batch 5: Optional (If time permits)
10. ? **A5** - Multi-file Tabs (deferred - too complex)

---

## COMMIT STRATEGY

Each batch will be a single commit:
- `feat: add jump to line, copy with headers, and search history`
- `feat: add export performance to CSV and save selected branch`
- `feat: add time range and duration threshold filters`
- `feat: add tree search and font selection`

---

## ESTIMATED COMPLETION TIME
- **Total:** ~3.5 hours
- **Completion Rate:** 95%+ of all planned features (excluding multi-file tabs)

