# Quick Reference: Using AppStrings

## Common Patterns

### Dialog Titles
```csharp
// Simple title
this.Text = UI.AppStrings.FindFormTitle;  // "Find"

// Formatted title
this.Text = string.Format(UI.AppStrings.AboutFormTitle, appName);  // "About {0}"
```

### Menu Items
```csharp
// Main menus
fileMenuItem.Text = UI.AppStrings.MenuFile;  // "&File"
editMenuItem.Text = UI.AppStrings.MenuEdit;  // "&Edit"

// Submenu items
openMenuItem.Text = UI.AppStrings.MenuFileOpen;  // "&Open..."
copyMenuItem.Text = UI.AppStrings.MenuEditCopy;  // "&Copy"
```

### Buttons
```csharp
okButton.Text = UI.AppStrings.ButtonOK;  // "&OK"
cancelButton.Text = UI.AppStrings.ButtonCancel;  // "&Cancel"
applyButton.Text = UI.AppStrings.ButtonApply;  // "&Apply"
```

### Labels
```csharp
searchLabel.Text = UI.AppStrings.FindLabelSearchFor;  // "Search for:"
threadLabel.Text = UI.AppStrings.FilterLabelThreadId;  // "&Thread ID:"
```

### Status Messages
```csharp
// Simple status
statusLabel.Text = UI.AppStrings.StatusReady;

// Formatted status
statusLabel.Text = string.Format(UI.AppStrings.StatusFileLoaded, lineCount);
// "File loaded: {0} lines"

statusLabel.Text = string.Format(UI.AppStrings.StatusFilterActive, filtered, total);
// "Filter active: {0} of {1} lines"
```

### MessageBox
```csharp
// Error message
MessageBox.Show(
    string.Format(UI.AppStrings.MsgErrorLoadingFile, filename),
    UI.AppStrings.ErrorTitle,
    MessageBoxButtons.OK,
    MessageBoxIcon.Error);

// Info message
MessageBox.Show(
    UI.AppStrings.MsgFileSaved,
    UI.AppStrings.InformationTitle,
    MessageBoxButtons.OK,
    MessageBoxIcon.Information);

// Confirmation
var result = MessageBox.Show(
    UI.AppStrings.MsgConfirmExit,
    UI.AppStrings.AppTitle,
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question);
```

### Tooltips
```csharp
openButton.ToolTipText = UI.AppStrings.ToolTipOpen;  // "Open log file"
saveButton.ToolTipText = UI.AppStrings.ToolTipSave;  // "Save log file"
findButton.ToolTipText = UI.AppStrings.ToolTipFind;  // "Find text"
```

### Column Headers
```csharp
columnLine.Text = UI.AppStrings.ColumnLineNumber;  // "Line #"
columnTime.Text = UI.AppStrings.ColumnTimestamp;   // "Timestamp"
columnMsg.Text = UI.AppStrings.ColumnMessage;      // "Message"
```

## All Available Categories

- **Application** - `AppTitle`, `AppTitleWithFile`
- **Menus** - `Menu*` (File, Edit, Options, View, Help)
- **Tooltips** - `ToolTip*`
- **Status Bar** - `Status*`
- **Tabs** - `Tab*`
- **Context Menus** - `Context*`
- **Messages** - `Msg*`
- **Find Form** - `Find*`
- **Filter Form** - `Filter*`
- **About Form** - `About*`
- **Update Form** - `Update*`
- **Compare Logs** - `CompareLogs*`
- **AI Settings** - `AISettings*`
- **Compare Options** - `CompareOptions*`
- **Buttons** - `Button*`
- **File Dialogs** - `FileDialog*`
- **Errors** - `Error*`, `Warning*`, `Information*`
- **Performance** - `Perf*`
- **Bookmarks** - `Bookmark*`
- **Hints** - `Hint*`
- **Columns** - `Column*`
- **Visualization** - `Viz*`
- **Update Check** - `UpdateCheck*`

## IntelliSense Tips

1. Type `UI.AppStrings.` and IntelliSense will show all available strings
2. Use Ctrl+Space to trigger IntelliSense
3. Constants are organized alphabetically within each category
4. Look for prefixes matching your component (e.g., `Find*`, `Filter*`, `Menu*`)

## Format String Examples

| Pattern | Example | Result |
|---------|---------|--------|
| `{0}` | `string.Format(UI.AppStrings.AboutFormTitle, "MyApp")` | "About MyApp" |
| `{0}, {1}` | `string.Format(UI.AppStrings.StatusFilterActive, 50, 100)` | "Filter active: 50 of 100 lines" |
| `{0:F2}` | `string.Format(UI.AppStrings.PerfAverageDuration, 123.456)` | "Average duration: 123.46 ms" |

## Best Practices

? **DO**
- Always use `UI.AppStrings.*` for any user-visible text
- Use formatted strings for dynamic content
- Keep accelerator keys (&) in button/menu text
- Use descriptive variable names matching the constant

? **DON'T**
- Don't hardcode strings in UI code
- Don't concatenate strings - use `string.Format` instead
- Don't duplicate strings - reuse existing constants
- Don't remove accelerator keys from menu items

## Migration Helper

**Find hardcoded strings:**
```powershell
# Search for potential hardcoded UI strings
Get-ChildItem -Recurse -Filter *.cs | Select-String -Pattern '\.Text = "' -CaseSensitive
Get-ChildItem -Recurse -Filter *.cs | Select-String -Pattern 'MessageBox\.Show\("' -CaseSensitive
```

**Replace pattern:**
```csharp
// Before:
this.Text = "Find";
button.Text = "&OK";
label.Text = "Search for:";

// After:
this.Text = UI.AppStrings.FindFormTitle;
button.Text = UI.AppStrings.ButtonOK;
label.Text = UI.AppStrings.FindLabelSearchFor;
```

---

**File Location**: `Cad3PLogBrowser\UI\AppStrings.cs`  
**Namespace**: `Cad3PLogBrowser.UI`  
**Usage**: Add `using Cad3PLogBrowser.UI;` or use fully qualified names
