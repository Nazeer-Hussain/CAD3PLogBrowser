## Resource Cleanup Analysis

### Resources Currently Defined (47 items)
```
About, ABOUT_DESCRIPTION, apiview, apiview2, Bitmap1, blue_ball, cad3plog, 
check1, check2, Color1, copy, cross, details, DIALOG_EXPORT_BRANCH_TITLE, 
filter, FILTER_CSV_FILES, find, graph1, graph2, green_ball, help, Icon1, 
MSG_BRANCH_EXPORTED, MSG_BROWSER_LAUNCH_ERROR, MSG_FILE_SAVED, 
MSG_GROK_NOT_CONFIGURED, MSG_LOAD_ERROR, MSG_NO_ERRORS, MSG_NO_MATCHING_PAIR, 
MSG_NO_WARNINGS, MSG_NOT_API_CALL, MSG_NOT_FOUND, MSG_SAVE_ERROR, Name1, 
open, performance, red_ball, refresh, remove, save, settings, tabs, TITLE, 
tools, treeview, yellow
```

### Resources Actually Used (14 items)
```
TITLE, apiview2, copy, filter, find, green_ball, help, open, red_ball, 
refresh, save, settings, treeview, yellow
```

### Unused Resources to Remove (33 items)
```
About, ABOUT_DESCRIPTION, apiview, Bitmap1, blue_ball, cad3plog, check1, 
check2, Color1, cross, details, DIALOG_EXPORT_BRANCH_TITLE, 
FILTER_CSV_FILES, graph1, graph2, Icon1, MSG_BRANCH_EXPORTED, 
MSG_BROWSER_LAUNCH_ERROR, MSG_FILE_SAVED, MSG_GROK_NOT_CONFIGURED, 
MSG_LOAD_ERROR, MSG_NO_ERRORS, MSG_NO_MATCHING_PAIR, MSG_NO_WARNINGS, 
MSG_NOT_API_CALL, MSG_NOT_FOUND, MSG_SAVE_ERROR, Name1, performance, 
remove, tabs, tools
```

### Hard-coded Strings Found (Sample)
```
- "Call Graph Node Details"
- "Results exported to:\n{0}"
- "Could not export results:\n{0}"
- "Save operation was cancelled."
- "Could not save file:\n{0}"
- "No log data to export."
- "Failed to export file:\n{0}"
- "'{0}' not found."
- "Invalid regular expression:\n{0}"
- "Selected line is not an API call line."
- "No matching pair found."
- "No errors found in this log file."
- "No warnings found in this log file."
- "Could not open updates page:\n{0}"
... and many more
```

### Recommended Actions
1. ? Remove 33 unused resources
2. ? Externalize all hard-coded strings to Resources.resx
3. ? Organize strings by category (Titles, Messages, Errors, Labels)
4. ? Update all code files to use Resources.XXX
5. ? Remove unused image files from Properties folder
