# STEP 2: Update MainForm.cs Code

## Instructions

Open `Cad3PLogBrowser\MainForm.cs` in Visual Studio

Use Find & Replace (Ctrl+H) with these exact replacements:

## Replacements (Do these in order)

### 1. Save Cancelled
**Find:** `"Save operation was cancelled.", Resources.TITLE`  
**Replace:** `Resources.ERR_SAVE_CANCELLED, Resources.TITLE`

### 2. No Data to Export
**Find:** `"No log data to export.", Resources.TITLE`  
**Replace:** `Resources.ERR_NO_DATA_TO_EXPORT, Resources.TITLE`

### 3. Not API Call
**Find:** `"Selected line is not an API call line.", Resources.TITLE`  
**Replace:** `Resources.ERR_NOT_API_CALL, Resources.TITLE`

### 4. No Matching Pair
**Find:** `"No matching pair found.", Resources.TITLE`  
**Replace:** `Resources.ERR_NO_MATCHING_PAIR, Resources.TITLE`

### 5. No Errors Found (appears twice)
**Find:** `"No errors found in this log file.", Resources.TITLE`  
**Replace:** `Resources.ERR_NO_ERRORS_FOUND, Resources.TITLE`  
**Replace All**

### 6. No Warnings Found (appears twice)
**Find:** `"No warnings found in this log file.", Resources.TITLE`  
**Replace:** `Resources.ERR_NO_WARNINGS_FOUND, Resources.TITLE`  
**Replace All**

### 7. No File Loaded (appears twice)
**Find:** `"No file loaded.", Resources.TITLE`  
**Replace:** `Resources.ERR_NO_FILE_LOADED, Resources.TITLE`  
**Replace All**

### 8. Invalid Line Number
**Find:** `"Invalid line number.", Resources.TITLE`  
**Replace:** `Resources.ERR_INVALID_LINE_NUMBER, Resources.TITLE`

### 9. No Bookmarks
**Find:** `"No bookmarks set.\n\nPress Ctrl+B to bookmark the current line.", "Bookmarks"`  
**Replace:** `Resources.ERR_NO_BOOKMARKS, Resources.DIALOG_TITLE_BOOKMARKS`

### 10. No Performance Data
**Find:** `"No performance data to export.\nLoad a log file first.",`  
**Replace:** `Resources.ERR_NO_PERFORMANCE_DATA,`

### 11. No Call Tree Data (appears twice)
**Find:** `"No call tree data to export.\nLoad a log file first.",`  
**Replace:** `Resources.ERR_NO_CALL_TREE_DATA,`  
**Replace All**

### 12. No Timeline Data
**Find:** `"No timeline data to export.\nLoad a log file and view the Timeline tab first.",`  
**Replace:** `Resources.ERR_NO_TIMELINE_DATA,`

### 13. No Flame Graph Data
**Find:** `"No flame graph data to export.\nLoad a log file and view the Flame Graph tab first.",`  
**Replace:** `Resources.ERR_NO_FLAME_GRAPH_DATA,`

---

## Manual Updates (String Interpolation)

Find and manually replace these:

### Line ~1335 - Save Failed
**Find:**
```csharp
MessageBox.Show($"Could not save file:\n{ex.Message}", Resources.TITLE,
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.ERR_SAVE_FAILED, ex.Message), Resources.TITLE,
```

### Line ~1391 - Export Failed
**Find:**
```csharp
MessageBox.Show($"Could not export results:\n{ex.Message}",
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.ERR_EXPORT_FAILED, ex.Message),
```

### Line ~1321 - File Saved
**Find:**
```csharp
MessageBox.Show($"{lines.Count:N0} line(s) saved.", Resources.TITLE,
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_FILE_SAVED, lines.Count), Resources.TITLE,
```

### Line ~1363 - Results Exported
**Find:**
```csharp
MessageBox.Show($"Results exported to:\n{dialog.FileName}",
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_RESULTS_EXPORTED, dialog.FileName),
```

### Line ~3558 - Call Tree JSON
**Find:**
```csharp
MessageBox.Show(string.Format("Call tree exported to:\n{0}", dlg.FileName),
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_CALL_TREE_EXPORTED_JSON, dlg.FileName),
```

### Line ~3598 - Call Tree XML
**Find:**
```csharp
MessageBox.Show(string.Format("Call tree exported to:\n{0}", dlg.FileName),
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_CALL_TREE_EXPORTED_XML, dlg.FileName),
```

### Line ~3636 - Timeline Exported
**Find:**
```csharp
MessageBox.Show(string.Format("Timeline exported to:\n{0}", dlg.FileName),
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_TIMELINE_EXPORTED, dlg.FileName),
```

### Line ~3686 - Flame Graph Exported
**Find:**
```csharp
MessageBox.Show(string.Format("Flame graph exported to:\n{0}", dlg.FileName),
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_FLAME_GRAPH_EXPORTED, dlg.FileName),
```

### Help File Not Found (around line 2979)
**Find:**
```csharp
MessageBox.Show("Help file (Cad3PLogBrowser.chm) not found.\n\n
```

**Replace with:**
```csharp
MessageBox.Show(Resources.ERR_HELP_FILE_NOT_FOUND + 
```

### Grok Not Configured (around line 3002)
**Find:**
```csharp
MessageBox.Show("Please configure the Grok URL in Options > Settings first.\n\n
```

**Replace with:**
```csharp
MessageBox.Show(Resources.ERR_GROK_NOT_CONFIGURED + 
```

### Browser Launch Failed (around line 3036)
**Find:**
```csharp
MessageBox.Show($"Could not open updates page:\n{ex.Message}",
```

**Replace with:**
```csharp
MessageBox.Show(string.Format(Resources.ERR_BROWSER_LAUNCH_FAILED, ex.Message),
```

---

## After All Replacements

1. **Build** (Ctrl+Shift+B)
2. **Fix any errors** (check line numbers if needed)
3. **Save** (Ctrl+S)

---

After completing this step, proceed to STEP_3_VERIFY.md
