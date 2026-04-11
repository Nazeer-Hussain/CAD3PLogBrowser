# STEP 1: Add String Resources to Resources.resx

## Instructions

Open Visual Studio and follow these steps:

1. **Open Resources.resx**
   - Solution Explorer ? Properties ? Resources.resx (double-click)

2. **Add String Resources**

Click "Add Resource" dropdown ? "Add String" for each entry below:

### Application Titles
```
Name: APP_TITLE
Value: CAD 3P Log Browser

Name: DIALOG_TITLE_BOOKMARKS  
Value: Bookmarks

Name: DIALOG_TITLE_JUMP_TO_LINE
Value: Jump to Line
```

### Error Messages (Copy these exactly)
```
Name: ERR_SAVE_CANCELLED
Value: Save operation was cancelled.

Name: ERR_NO_DATA_TO_EXPORT
Value: No log data to export.

Name: ERR_NO_FILE_LOADED
Value: No file loaded.

Name: ERR_INVALID_LINE_NUMBER
Value: Invalid line number.

Name: ERR_NOT_API_CALL
Value: Selected line is not an API call line.

Name: ERR_NO_MATCHING_PAIR
Value: No matching pair found.

Name: ERR_NO_ERRORS_FOUND
Value: No errors found in this log file.

Name: ERR_NO_WARNINGS_FOUND
Value: No warnings found in this log file.

Name: ERR_NO_BOOKMARKS
Value: No bookmarks set.\n\nPress Ctrl+B to bookmark the current line.

Name: ERR_NO_PERFORMANCE_DATA
Value: No performance data to export.\nLoad a log file first.

Name: ERR_NO_CALL_TREE_DATA
Value: No call tree data to export.\nLoad a log file first.

Name: ERR_NO_TIMELINE_DATA
Value: No timeline data to export.\nLoad a log file and view the Timeline tab first.

Name: ERR_NO_FLAME_GRAPH_DATA
Value: No flame graph data to export.\nLoad a log file and view the Flame Graph tab first.

Name: ERR_SAVE_FAILED
Value: Could not save file:\n{0}

Name: ERR_EXPORT_FAILED
Value: Could not export results:\n{0}

Name: ERR_LOAD_FAILED
Value: Could not open file:\n{0}

Name: ERR_HELP_FILE_NOT_FOUND
Value: Help file (Cad3PLogBrowser.chm) not found.\n\nPlease ensure the help file is in the application directory.

Name: ERR_GROK_NOT_CONFIGURED
Value: Please configure the Grok URL in Options > Settings first.\n\nThe Grok URL is used to search for method definitions and implementations.

Name: ERR_BROWSER_LAUNCH_FAILED
Value: Could not open updates page:\n{0}
```

### Success Messages
```
Name: MSG_FILE_SAVED
Value: {0} line(s) saved.

Name: MSG_RESULTS_EXPORTED
Value: Results exported to:\n{0}

Name: MSG_CALL_TREE_EXPORTED_JSON
Value: Call tree exported to:\n{0}

Name: MSG_CALL_TREE_EXPORTED_XML
Value: Call tree exported to:\n{0}

Name: MSG_TIMELINE_EXPORTED
Value: Timeline exported to:\n{0}

Name: MSG_FLAME_GRAPH_EXPORTED
Value: Flame graph exported to:\n{0}
```

3. **Update Existing TITLE Resource**
   - Find "TITLE" in the list
   - Change its value from "version 1.0.0.0" to "CAD 3P Log Browser"

4. **Save** (Ctrl+S)

5. **Build** (Ctrl+Shift+B) to regenerate Resources.Designer.cs

---

After completing this step, proceed to STEP_2_UPDATE_CODE.md
