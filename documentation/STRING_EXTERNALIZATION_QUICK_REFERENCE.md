# String Externalization - Quick Reference Guide

## ? What's Done

### UpdateService - 100% Complete
```csharp
// File: Services/Update/UpdateServiceStrings.cs
UpdateServiceStrings.LogFetchManifestStarting
UpdateServiceStrings.ErrorManifestUrlNullOrEmpty
UpdateServiceStrings.BatchScriptTemplate
```

### SettingsForm - 70% Complete
```csharp
// File: UI/SettingsDialogStrings.cs
SettingsDialogStrings.DialogTitle
SettingsDialogStrings.ButtonOk
SettingsDialogStrings.TabAppearance
SettingsDialogStrings.CheckboxEnableAI
```

## ?? Quick Start - Add String Constants to New Form

### Step 1: Create Strings Class (2 minutes)
```csharp
// File: UI/YourFormStrings.cs
namespace Cad3PLogBrowser.UI
{
    public static class YourFormStrings
    {
        public const string DialogTitle = "Your Dialog";
        public const string ButtonOk = "&OK";
        public const string MessageFormat = "Message: {0}";
    }
}
```

### Step 2: Add Using (10 seconds)
```csharp
// At top of your form file
using Cad3PLogBrowser.UI;
```

### Step 3: Replace Strings (5 minutes)
```csharp
// Before
this.Text = "Your Dialog";
btnOk.Text = "&OK";
MessageBox.Show("Message: " + text);

// After
this.Text = YourFormStrings.DialogTitle;
btnOk.Text = YourFormStrings.ButtonOk;
MessageBox.Show(string.Format(YourFormStrings.MessageFormat, text));
```

### Step 4: Build & Test (1 minute)
```bash
# Build project
dotnet build

# Run and verify
# Test dialog opens
# Check all text displays correctly
```

## ?? Naming Cheat Sheet

| Element | Pattern | Example |
|---------|---------|---------|
| Dialog Title | `DialogTitle` | `DialogTitle = "Settings"` |
| Button | `Button[Action]` | `ButtonOk = "&OK"` |
| Label | `Label[Purpose]` | `LabelUserName = "User name:"` |
| Checkbox | `Checkbox[Option]` | `CheckboxMatchCase = "Match case"` |
| Message | `Message[Purpose]` | `MessageNotFound = "Not found"` |
| Format | `[Purpose]Format` | `MessageNotFoundFormat = "'{0}' not found"` |
| Status | `Status[State]` | `StatusSearching = "Searching..."` |
| ToolTip | `ToolTip[Control]` | `ToolTipOpen = "Open file (Ctrl+O)"` |
| Menu | `Menu[Item]` | `MenuFileOpen = "&Open..."` |
| Tab | `Tab[Name]` | `TabGeneral = "General"` |
| Group | `Group[Name]` | `GroupOptions = "Options"` |

## ?? Priority Order

### Do These First ??
1. **MainForm** - Most visible to users
2. **FindForm** - Core functionality
3. **FilterForm** - Core functionality

### Do These Next ??
4. **UpdateAvailableForm** - User experience
5. **AboutForm** - Professional appearance
6. Finish **SettingsForm** (30% remaining)

### Do These Last ??
7. **CompareLogsForm** - Advanced feature
8. **LineInspectorPanel** - Utility
9. Other dialogs as needed

## ?? Common Patterns

### Pattern 1: Simple Text
```csharp
// Constant
public const string ButtonSave = "&Save";

// Usage
btnSave.Text = YourFormStrings.ButtonSave;
```

### Pattern 2: Format String
```csharp
// Constant
public const string MessageLoadedFormat = "Loaded {0} lines in {1:F2}s";

// Usage
string msg = string.Format(
    YourFormStrings.MessageLoadedFormat, 
    lineCount, 
    elapsed
);
```

### Pattern 3: MessageBox
```csharp
// Constants
public const string MessageCannotOpen = "Cannot open file: {0}";
public const string MessageCannotOpenTitle = "Error";

// Usage
MessageBox.Show(
    string.Format(YourFormStrings.MessageCannotOpen, fileName),
    YourFormStrings.MessageCannotOpenTitle,
    MessageBoxButtons.OK,
    MessageBoxIcon.Error
);
```

### Pattern 4: Menu Items
```csharp
// Constants
public const string MenuFile = "&File";
public const string MenuFileOpen = "&Open...";
public const string MenuFileExit = "E&xit";

// Usage
fileToolStripMenuItem.Text = YourFormStrings.MenuFile;
openToolStripMenuItem.Text = YourFormStrings.MenuFileOpen;
exitToolStripMenuItem.Text = YourFormStrings.MenuFileExit;
```

### Pattern 5: ToolTips
```csharp
// Constant
public const string ToolTipOpen = "Open file (Ctrl+O)";

// Usage
toolTip1.SetToolTip(btnOpen, YourFormStrings.ToolTipOpen);
```

## ?? Finding Strings to Externalize

### Search for Common Patterns
```csharp
// In Visual Studio, search for:
= "              // Direct string assignment
.Text = "       // Control text property
MessageBox.Show(" // MessageBox strings
.Format("       // Format strings
throw new .*(")  // Exception messages (regex)
```

### Files to Check
1. *.cs (all form files)
2. InitializeComponent() methods
3. Event handlers
4. MessageBox.Show() calls
5. String.Format() calls
6. throw new Exception() calls

## ? Verification Checklist

After externalizing strings:
- [ ] Build succeeds
- [ ] No hardcoded strings in file
- [ ] All UI elements display correctly
- [ ] All buttons work
- [ ] All messages show correctly
- [ ] All tooltips work
- [ ] All menu items work
- [ ] Constants are well-named
- [ ] Constants are grouped logically
- [ ] Format strings use {0}, {1}, etc. correctly

## ?? Progress Tracker

```
Project-Wide String Externalization Progress
???????????????????????????????????????????

??????????????????????????????????????? 20%

Completed:
  ? UpdateService         ???????????????????? 100%
  ? SettingsForm          ????????????????????  70%

In Progress:
  ?? MainForm             ????????????????????   0%
  ?? FindForm             ????????????????????   0%

Pending:
  ? FilterForm
  ? UpdateAvailableForm
  ? AboutForm
  ? CompareLogsForm
  ? LineInspectorPanel
```

## ?? Pro Tips

### Tip 1: Group Related Constants
```csharp
// Good - grouped by section
public static class FormStrings
{
    // ?? Dialog Title ??????????????????
    public const string DialogTitle = "...";

    // ?? Buttons ???????????????????????
    public const string ButtonOk = "...";
    public const string ButtonCancel = "...";
}
```

### Tip 2: Use Format Strings
```csharp
// Good - reusable
public const string MessageFormat = "Found {0} matches in {1:F2}s";

// Bad - hardcoded values
public const string Message1 = "Found 5 matches in 0.25s";
public const string Message2 = "Found 10 matches in 0.50s";
```

### Tip 3: Include Context in Names
```csharp
// Good - clear purpose
public const string MessageFileNotFound = "File not found: {0}";
public const string MessageFileNotFoundTitle = "Error";

// Bad - ambiguous
public const string Message1 = "File not found: {0}";
public const string Title1 = "Error";
```

### Tip 4: Follow Existing Patterns
```csharp
// Look at completed files for patterns:
// - UpdateServiceStrings.cs
// - SettingsDialogStrings.cs

// Use similar structure and naming
```

### Tip 5: Test Immediately
```csharp
// After externalizing each form:
// 1. Build
// 2. Run application
// 3. Open the dialog
// 4. Test all buttons
// 5. Trigger all messages
```

## ?? Common Issues & Solutions

### Issue 1: Build Error - Constant Not Found
```csharp
// Problem
Text = FormStrings.DialogTitle;
// Error: 'FormStrings' does not exist

// Solution
using Cad3PLogBrowser.UI; // Add using statement
```

### Issue 2: Format String Error
```csharp
// Problem
string.Format(FormStrings.Message, a, b, c);
// Error: Index (0) must be >= 0 and < 2

// Solution
// Check format string has enough placeholders
public const string Message = "a={0} b={1} c={2}"; // Add {2}
```

### Issue 3: String Not Displaying
```csharp
// Problem
btnOk.Text = FormStrings.ButtonOk; // Shows empty

// Solution  
// Check constant value is not empty
public const string ButtonOk = "&OK"; // Should have value
```

### Issue 4: MessageBox Title Missing
```csharp
// Problem
MessageBox.Show(message); // No title

// Solution
MessageBox.Show(
    message,
    FormStrings.MessageTitle // Add title constant
);
```

## ?? Quick Help

### Where to Find Examples
1. `UpdateServiceStrings.cs` - Complete example
2. `SettingsDialogStrings.cs` - Large dialog example
3. Documentation folder - Detailed guides

### Best Practices
1. ? Use descriptive constant names
2. ? Group constants by function
3. ? Add XML comments for complex formats
4. ? Use format strings for dynamic content
5. ? Test after every change

### What NOT to Do
1. ? Don't use generic names (String1, Text2)
2. ? Don't embed values in constant names
3. ? Don't mix concerns (UI + business logic)
4. ? Don't skip testing
5. ? Don't forget using statements

## ?? Success Criteria

You've successfully externalized strings when:
- ? No hardcoded strings in code
- ? Build succeeds
- ? Application runs correctly
- ? All UI text displays correctly
- ? Constants are well-organized
- ? Code is more maintainable
- ? Ready for localization

---

**Remember:** String externalization is an investment in code quality. Take your time, follow the patterns, and test thoroughly!

**Current Status:** 20% Complete  
**Next Goal:** 60% Complete (Phase 1 done)  
**Final Goal:** 100% Complete
