# String Externalization Implementation Guide

## Overview
This guide provides step-by-step instructions to externalize all hard-coded strings to Resources.resx.

## Phase 1: Add String Resources to Resources.resx

Open Visual Studio ? Solution Explorer ? Properties ? Resources.resx

### Application Titles (10 strings)
```
APP_TITLE = CAD 3P Log Browser
DIALOG_TITLE_FILTER = Filter Log
DIALOG_TITLE_FIND = Find
DIALOG_TITLE_SETTINGS = Settings
DIALOG_TITLE_ABOUT = About CAD 3P Log Browser
DIALOG_TITLE_KEYBOARD_SHORTCUTS = Keyboard Shortcuts
DIALOG_TITLE_EXPORT = Export Results
DIALOG_TITLE_BOOKMARKS = Bookmarks
DIALOG_TITLE_CALL_GRAPH = Call Graph Node Details
DIALOG_TITLE_JUMP_TO_LINE = Jump to Line
```

### Error Messages (21 strings)
```
ERR_NO_FILE_LOADED = No file loaded.
ERR_SAVE_CANCELLED = Save operation was cancelled.
ERR_NO_DATA_TO_EXPORT = No log data to export.
ERR_SAVE_FAILED = Could not save file:\n{0}
ERR_EXPORT_FAILED = Could not export results:\n{0}
ERR_LOAD_FAILED = Could not open file:\n{0}
ERR_INVALID_REGEX = Invalid regular expression:\n{0}
ERR_NOT_FOUND = '{0}' not found.
ERR_NOT_API_CALL = Selected line is not an API call line.
ERR_NO_MATCHING_PAIR = No matching pair found.
ERR_NO_ERRORS_FOUND = No errors found in this log file.
ERR_NO_WARNINGS_FOUND = No warnings found in this log file.
ERR_NO_BOOKMARKS = No bookmarks set.\n\nPress Ctrl+B to bookmark the current line.
ERR_INVALID_LINE_NUMBER = Invalid line number.
ERR_HELP_FILE_NOT_FOUND = Help file (Cad3PLogBrowser.chm) not found.\n\nPlease ensure the help file is in the application directory.
ERR_GROK_NOT_CONFIGURED = Please configure the Grok URL in Options > Settings first.\n\nThe Grok URL is used to search for method definitions and implementations.
ERR_BROWSER_LAUNCH_FAILED = Could not open updates page:\n{0}
ERR_NO_PERFORMANCE_DATA = No performance data to export.\nLoad a log file first.
ERR_NO_CALL_TREE_DATA = No call tree data to export.\nLoad a log file first.
ERR_NO_TIMELINE_DATA = No timeline data to export.\nLoad a log file and view the Timeline tab first.
ERR_NO_FLAME_GRAPH_DATA = No flame graph data to export.\nLoad a log file and view the Flame Graph tab first.
```

### Success Messages (9 strings)
```
MSG_RESULTS_EXPORTED = Results exported to:\n{0}
MSG_FILE_SAVED = {0} line(s) saved.
MSG_CALL_TREE_EXPORTED_JSON = Call tree exported to:\n{0}
MSG_CALL_TREE_EXPORTED_XML = Call tree exported to:\n{0}
MSG_TIMELINE_EXPORTED = Timeline exported to:\n{0}
MSG_FLAME_GRAPH_EXPORTED = Flame graph exported to:\n{0}
MSG_COPIED_TO_CLIPBOARD = Text copied to clipboard.
MSG_BOOKMARK_ADDED = Bookmark added at line {0}.
MSG_BOOKMARK_REMOVED = Bookmark removed from line {0}.
```

### Format Strings (13 strings)
```
FMT_LINES_LOADED = {0:N0} lines loaded.
FMT_LINES_FILTERED = Filter applied: {0:N0} of {1:N0} lines match.
FMT_MATCHES_FOUND = {0:N0} matches found.
FMT_MATCH_POSITION = Match {0} of {1}
FMT_ERROR_COUNT = {0:N0} errors found.
FMT_WARNING_COUNT = {0:N0} warnings found.
FMT_BOOKMARK_COUNT = Bookmarks ({0})
FMT_PROGRESS_PERCENT = {0}% complete...
FMT_DURATION_MS = [{0} ms]
FMT_CALL_COUNT = ({0} calls)
FMT_LINE_NUMBER = Line {0}
FMT_ERROR_AT_LINE = Error {0} of {1}
FMT_WARNING_AT_LINE = Warning {0} of {1}
```

### Placeholders (5 strings)
```
PH_SEARCH_TEXT = Search...
PH_FILTER_TEXT = Filter text...
PH_TREE_SEARCH = Search tree nodes...
PH_THREAD_ID = Enter thread ID...
PH_LINE_NUMBER = Line number...
```

## Phase 2: Code Replacements

### Step-by-Step Process

1. **Open Find and Replace in Visual Studio** (Ctrl+H)
2. **Set scope to** "Entire Solution"
3. **Enable** "Use Regular Expressions"
4. **For each pattern below**, do Find and Replace

### Pattern 1: Simple MessageBox with literal string

**Find:**
```
MessageBox\.Show\("([^"]+)", Resources\.TITLE
```

**Replace:**
```
MessageBox.Show(Resources.XXX, Resources.APP_TITLE
```
*(Replace XXX with appropriate resource name)*

### Pattern 2: MessageBox with string interpolation

**Find:**
```
MessageBox\.Show\(\$"([^"]+)", Resources\.TITLE
```

**Replace:**
```
MessageBox.Show(string.Format(Resources.XXX, ...), Resources.APP_TITLE
```

### Specific Replacements for MainForm.cs

#### Replace #1
**Find:**
```csharp
MessageBox.Show("Save operation was cancelled.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_SAVE_CANCELLED, Resources.APP_TITLE,
```

#### Replace #2
**Find:**
```csharp
MessageBox.Show("No log data to export.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_NO_DATA_TO_EXPORT, Resources.APP_TITLE,
```

#### Replace #3
**Find:**
```csharp
MessageBox.Show($"Could not save file:\n{ex.Message}", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(string.Format(Resources.ERR_SAVE_FAILED, ex.Message), Resources.APP_TITLE,
```

#### Replace #4
**Find:**
```csharp
MessageBox.Show($"Results exported to:\n{dialog.FileName}",
```
**Replace:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_RESULTS_EXPORTED, dialog.FileName),
```

#### Replace #5
**Find:**
```csharp
MessageBox.Show($"{lines.Count:N0} line(s) saved.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(string.Format(Resources.MSG_FILE_SAVED, lines.Count), Resources.APP_TITLE,
```

#### Replace #6
**Find:**
```csharp
MessageBox.Show("Selected line is not an API call line.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_NOT_API_CALL, Resources.APP_TITLE,
```

#### Replace #7
**Find:**
```csharp
MessageBox.Show("No matching pair found.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_NO_MATCHING_PAIR, Resources.APP_TITLE,
```

#### Replace #8
**Find:**
```csharp
MessageBox.Show("No errors found in this log file.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_NO_ERRORS_FOUND, Resources.APP_TITLE,
```

#### Replace #9
**Find:**
```csharp
MessageBox.Show("No warnings found in this log file.", Resources.TITLE,
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_NO_WARNINGS_FOUND, Resources.APP_TITLE,
```

#### Replace #10
**Find:**
```csharp
MessageBox.Show("No bookmarks set.\n\nPress Ctrl+B to bookmark the current line.", "Bookmarks",
```
**Replace:**
```csharp
MessageBox.Show(Resources.ERR_NO_BOOKMARKS, Resources.DIALOG_TITLE_BOOKMARKS,
```

## Phase 3: Update Resources.TITLE

Since existing code uses `Resources.TITLE`, update its value in Resources.resx:

**Old:** `TITLE = version 1.0.0.0`  
**New:** `TITLE = CAD 3P Log Browser`  

Or create a new `APP_TITLE` and do a global replace:

**Find:** `Resources.TITLE`  
**Replace:** `Resources.APP_TITLE`

## Phase 4: Clean Up Unused Resources

Remove these 33 unused resources from Resources.resx:

### To Remove via Visual Studio:
1. Open Resources.resx
2. Find each item in the list
3. Right-click ? Remove
4. Save file

### List of resources to remove:
```
About, ABOUT_DESCRIPTION, apiview, Bitmap1, blue_ball, cad3plog, 
check1, check2, Color1, cross, details, DIALOG_EXPORT_BRANCH_TITLE, 
FILTER_CSV_FILES, graph1, graph2, Icon1, MSG_BRANCH_EXPORTED, 
MSG_BROWSER_LAUNCH_ERROR, MSG_FILE_SAVED (old one), 
MSG_GROK_NOT_CONFIGURED (old one), MSG_LOAD_ERROR, MSG_NO_ERRORS, 
MSG_NO_MATCHING_PAIR, MSG_NO_WARNINGS, MSG_NOT_API_CALL, MSG_NOT_FOUND, 
MSG_SAVE_ERROR, Name1, performance, remove, tabs, tools
```

## Phase 5: Testing Checklist

After all changes:

### Build
- ? Build solution (Ctrl+Shift+B)
- ? Verify no build errors
- ? Verify no missing resource warnings

### Functional Testing
- ? Open a log file
- ? Try to open non-existent file ? Check error message
- ? Save file ? Check success message
- ? Apply filter ? Check status message
- ? Search for text ? Check "not found" message
- ? Toggle bookmark ? Check bookmark messages
- ? Navigate errors/warnings ? Check navigation messages
- ? Export data ? Check export messages
- ? Open Help (when file missing) ? Check help error message
- ? Try Grok (when not configured) ? Check Grok error message

### Verification Script

Run this PowerShell to find any remaining hard-coded strings:

```powershell
$files = Get-ChildItem -Path "Cad3PLogBrowser" -Filter "*.cs" `
    -Exclude "*Designer.cs","*AssemblyInfo.cs","Resources.Designer.cs" -Recurse

$hardcoded = @()
foreach ($file in $files) {
    $matches = Select-String -Path $file.FullName `
        -Pattern 'MessageBox\.Show\s*\(\s*"[^"]*"' -AllMatches
    if ($matches) {
        $hardcoded += $matches
    }
}

if ($hardcoded.Count -gt 0) {
    Write-Host "WARNING: Found $($hardcoded.Count) hard-coded strings:" -ForegroundColor Yellow
    $hardcoded | ForEach-Object { Write-Host "  $($_.Filename):$($_.LineNumber)" }
} else {
    Write-Host "SUCCESS: No hard-coded strings found!" -ForegroundColor Green
}
```

## Summary

**Total strings to add:** ~60  
**Total resources to remove:** ~33  
**Files to modify:** ~10  
**Estimated time:** 2-3 hours  

**Benefits:**
- ? Centralized string management
- ? Localization ready
- ? Consistent messaging
- ? Easier maintenance
- ? No magic strings in code

