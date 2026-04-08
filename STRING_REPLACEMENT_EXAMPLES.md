# ?? STRING REPLACEMENT EXAMPLES - CODE CHANGES

**Reference:** Quick guide for replacing hardcoded strings  
**File:** MainForm.cs  
**Status:** Examples ready for implementation  

---

## ?? STEP 1: ADD USING STATEMENT

**At the top of MainForm.cs**, add:

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cad3PLogBrowser.Properties;  // ? ADD THIS LINE
using Cad3PLogBrowser.Services;
```

---

## ?? STEP 2: REPLACE STRINGS

### LoadFileAsync Method

**Line 919:** Status message
```csharp
// BEFORE:
StatusFileName.Text = "Loading...";

// AFTER:
StatusFileName.Text = Strings.Status_Loading;
```

**Line 929:** Processing message
```csharp
// BEFORE:
StatusFileName.Text = "Processing log data...";

// AFTER:
StatusFileName.Text = Strings.Status_ProcessingLogData;
```

**Line 936:** Building tree message
```csharp
// BEFORE:
StatusFileName.Text = "Building call tree...";

// AFTER:
StatusFileName.Text = Strings.Status_BuildingCallTree;
```

---

### saveAsMenuItem_Click Method

**Line 1109:** Preparing message
```csharp
// BEFORE:
StatusFileName.Text = "Preparing to save...";

// AFTER:
StatusFileName.Text = Strings.Status_PreparingToSave;
```

**Line 1117:** Collecting selected
```csharp
// BEFORE:
StatusFileName.Text = "Collecting selected lines...";

// AFTER:
StatusFileName.Text = Strings.Status_CollectingSelectedLines;
```

**Line 1122:** Collecting all
```csharp
// BEFORE:
StatusFileName.Text = "Collecting all lines...";

// AFTER:
StatusFileName.Text = Strings.Status_CollectingAllLines;
```

**Line 1135:** Success message
```csharp
// BEFORE:
MessageBox.Show(string.Format("{0} line(s) saved.", lines.Count),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(string.Format(Strings.Msg_LinesSaved, lines.Count),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
```

**Line 1142:** Error message
```csharp
// BEFORE:
MessageBox.Show(string.Format("Could not save file:\n{0}", ex.Message),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);

// AFTER:
MessageBox.Show(string.Format(Strings.Msg_CouldNotSaveFile, ex.Message),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
```

---

### exportFilteredLogsMenuItem_Click Method

**Line 1191:** No data message
```csharp
// BEFORE:
MessageBox.Show("No log data to export.", Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(Strings.Msg_NoLogData, Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);
```

**Line 1197:** Dialog title and filter
```csharp
// BEFORE:
dlg.Title = "Export Filtered Logs";
dlg.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";

// AFTER:
dlg.Title = Strings.Dialog_ExportFilteredLogs;
dlg.Filter = Strings.FileFilter_LogFiles;
```

**Line 1209:** Exporting message
```csharp
// BEFORE:
StatusFileName.Text = "Exporting filtered logs...";

// AFTER:
StatusFileName.Text = Strings.Status_ExportingFilteredLogs;
```

**Line 1227:** Filter info
```csharp
// BEFORE:
string filterInfo = string.IsNullOrEmpty(_activeFilterText) 
    ? "all lines" 
    : string.Format("filtered lines (filter: '{0}')", _activeFilterText);

// AFTER:
string filterInfo = string.IsNullOrEmpty(_activeFilterText) 
    ? Strings.Filter_AllLines 
    : string.Format(Strings.Filter_FilteredLines, _activeFilterText);
```

**Line 1234:** Success message
```csharp
// BEFORE:
MessageBox.Show(
    string.Format("{0:N0} {1} exported successfully.\n\nFile: {2}",
        lines.Count, filterInfo, dlg.FileName),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(
    string.Format(Strings.Msg_ExportSuccess, 
        lines.Count, filterInfo, dlg.FileName),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
```

**Line 1243:** Error message
```csharp
// BEFORE:
MessageBox.Show(string.Format("Failed to export file:\n{0}", ex.Message),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);

// AFTER:
MessageBox.Show(string.Format(Strings.Msg_ExportFailed, ex.Message),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
```

---

### FindNext Method

**Line 1298:** Not found message
```csharp
// BEFORE:
MessageBox.Show(string.Format("'{0}' not found.", searchTerm),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(string.Format(Strings.Msg_NotFound, searchTerm),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### JumpToMatchingPair Method

**Line 1386:** Not API call message
```csharp
// BEFORE:
if (current == null) { 
    MessageBox.Show("Selected line is not an API call line.", Resources.TITLE, 
        MessageBoxButtons.OK, MessageBoxIcon.Information); 
    return; 
}

// AFTER:
if (current == null) { 
    MessageBox.Show(Strings.Msg_NotApiCallLine, Resources.TITLE, 
        MessageBoxButtons.OK, MessageBoxIcon.Information); 
    return; 
}
```

**Line 1418:** No matching pair
```csharp
// BEFORE:
MessageBox.Show("No matching pair found.", Resources.TITLE, 
    MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(Strings.Msg_NoMatchingPair, Resources.TITLE, 
    MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### NavigateToNextError Method

**Line 1452:** No errors message
```csharp
// BEFORE:
MessageBox.Show("No errors found in this log file.", Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(Strings.Msg_NoErrors, Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### NavigateToPreviousError Method

**Line 1470:** No errors message
```csharp
// BEFORE:
MessageBox.Show("No errors found in this log file.", Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(Strings.Msg_NoErrors, Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### NavigateToNextWarning Method

**Line 1489:** No warnings message
```csharp
// BEFORE:
MessageBox.Show("No warnings found in this log file.", Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(Strings.Msg_NoWarnings, Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### NavigateToPreviousWarning Method

**Line 1507:** No warnings message
```csharp
// BEFORE:
MessageBox.Show("No warnings found in this log file.", Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(Strings.Msg_NoWarnings, Resources.TITLE,
    MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### MainForm_Load Method

**Lines 1723-1726:** Tab names
```csharp
// BEFORE:
logTab.Text = "Log";
performanceTab.Text = "Performance";
logDetailTab.Text = "Log Details";
callGraphTab.Text = "Call Graph";

// AFTER:
logTab.Text = Strings.Tab_Log;
performanceTab.Text = Strings.Tab_Performance;
logDetailTab.Text = Strings.Tab_LogDetails;
callGraphTab.Text = Strings.Tab_CallGraph;
```

---

### treeContextExportBranchCsvMenuItem_Click Method

**Line 1883:** Dialog title and filter
```csharp
// BEFORE:
dlg.Title = "Export Branch to CSV";
dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

// AFTER:
dlg.Title = Strings.Dialog_ExportBranchToCSV;
dlg.Filter = Strings.FileFilter_CSVFiles;
```

**Line 1945:** Success message
```csharp
// BEFORE:
MessageBox.Show($"Branch exported to:\n{dlg.FileName}",
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);

// AFTER:
MessageBox.Show(string.Format(Strings.Msg_BranchExported, dlg.FileName),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
```

---

### treeContextSearchInGrokMenuItem_Click Method

**Line 1897:** Browser error message
```csharp
// BEFORE:
MessageBox.Show($"Failed to open browser:\n{ex.Message}",
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);

// AFTER:
MessageBox.Show(string.Format(Strings.Msg_BrowserFailed, ex.Message),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
```

---

## ?? LOGFILESERVICE.CS CHANGES

### ReadLinesAsync Method

**Line 36:**
```csharp
// BEFORE:
progressCallback(progress, $"Reading: {lineCount:N0} lines");

// AFTER:
progressCallback(progress, string.Format(Strings.Status_Reading, lineCount));
```

---

### WriteLines Method

**Line 80:**
```csharp
// BEFORE:
progressCallback(progress, $"Writing: {linesWritten:N0}/{totalLines:N0} lines");

// AFTER:
progressCallback(progress, string.Format(Strings.Status_Writing, linesWritten, totalLines));
```

**Line 87:**
```csharp
// BEFORE:
progressCallback(95, "Saving to disk...");

// AFTER:
progressCallback(95, Strings.Status_SavingToDisk);
```

**Line 91:**
```csharp
// BEFORE:
progressCallback(100, "Complete");

// AFTER:
progressCallback(100, Strings.Status_Complete);
```

---

## ?? SHOWLOADERROR METHOD

**Line 1010-1018:**
```csharp
// BEFORE:
private void ShowLoadError(string filePath, string reason, string detail)
{
    _isLoading = false;
    FileLoadProgress.Visible = false;
    SetDocumentLoaded(false);
    FileStatus.Image = Resources.red_ball;
    MessageBox.Show(
        string.Format("{0}:\n{1}\n\nFile: {2}", reason, detail, filePath),
        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
}

// AFTER:
private void ShowLoadError(string filePath, string reason, string detail)
{
    _isLoading = false;
    FileLoadProgress.Visible = false;
    SetDocumentLoaded(false);
    FileStatus.Image = Resources.red_ball;
    MessageBox.Show(
        string.Format(Strings.Error_LoadError, reason, detail, filePath),
        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

**And update the callers (Lines 926-928):**
```csharp
// BEFORE:
catch (UnauthorizedAccessException ex) { ShowLoadError(filePath, "Access denied", ex.Message); }
catch (IOException ex)                 { ShowLoadError(filePath, "File read error", ex.Message); }
catch (Exception ex)                   { ShowLoadError(filePath, "Unexpected error", ex.Message); }

// AFTER:
catch (UnauthorizedAccessException ex) { ShowLoadError(filePath, Strings.Error_AccessDenied, ex.Message); }
catch (IOException ex)                 { ShowLoadError(filePath, Strings.Error_FileReadError, ex.Message); }
catch (Exception ex)                   { ShowLoadError(filePath, Strings.Error_UnexpectedError, ex.Message); }
```

---

## ? SUMMARY

**Total Replacements:**
- **MainForm.cs:** ~30 replacements
- **LogFileService.cs:** 4 replacements

**Pattern:**
1. Simple strings ? `Strings.ResourceName`
2. Format strings ? `string.Format(Strings.ResourceName, args)`
3. Interpolated strings ? `string.Format(Strings.ResourceName, args)`

**Benefits:**
- ? All strings in one place (Strings.resx)
- ? Easy to update and maintain
- ? Ready for localization
- ? IntelliSense support
- ? Compile-time checking

---

**Status:** ? READY FOR IMPLEMENTATION  
**Effort:** ~1-2 hours  
**Testing:** Test after each section  

**Happy string externalizing!** ??

