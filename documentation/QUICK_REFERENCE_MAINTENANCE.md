# ?? QUICK REFERENCE - Maintenance & Best Practices

## For Future Development

### When Adding New Features

#### If you need to show a user message:

**DON'T DO THIS:** ?
```csharp
MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK);
```

**DO THIS:** ?
```csharp
// 1. Add to Resources.resx:
//    Name: ERR_FILE_NOT_FOUND
//    Value: File not found.

// 2. In code:
MessageBox.Show(Resources.ERR_FILE_NOT_FOUND, Resources.TITLE, 
    MessageBoxButtons.OK, MessageBoxIcon.Error);
```

#### If message has parameters:

**DON'T DO THIS:** ?
```csharp
MessageBox.Show($"Could not load {filename}", "Error");
```

**DO THIS:** ?
```csharp
// 1. Add to Resources.resx:
//    Name: ERR_FILE_LOAD_FAILED
//    Value: Could not load {0}

// 2. In code:
MessageBox.Show(string.Format(Resources.ERR_FILE_LOAD_FAILED, filename),
    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
```

---

## Resource Naming Conventions

### Error Messages:
```
ERR_{CONTEXT}_{ACTION}_FAILED
Examples:
- ERR_FILE_LOAD_FAILED
- ERR_EXPORT_DATA_FAILED
- ERR_INVALID_INPUT
```

### Success Messages:
```
MSG_{CONTEXT}_{ACTION}_{STATUS}
Examples:
- MSG_FILE_SAVED_TO
- MSG_DATA_EXPORTED_TO
- MSG_OPERATION_COMPLETE
```

### Dialog Titles:
```
DIALOG_TITLE_{PURPOSE}
Examples:
- DIALOG_TITLE_BOOKMARKS
- DIALOG_TITLE_SETTINGS
```

### Info Messages:
```
MSG_{CONTEXT}
Examples:
- MSG_NO_DATA_AVAILABLE
- MSG_PROCESSING_COMPLETE
```

---

## Verification Commands

### Check for hard-coded strings:
```powershell
powershell -ExecutionPolicy Bypass -File .\verify-strings.ps1
```
**Expected:** 0 hard-coded MessageBox strings

### Check resource utilization:
```powershell
powershell -ExecutionPolicy Bypass -File .\verify-resources.ps1
```
**Expected:** 100% utilization

### Build verification:
```powershell
# In Visual Studio: Ctrl+Shift+B
# Or PowerShell:
msbuild Cad3PLogBrowser\Cad3PLogBrowser.csproj
```
**Expected:** 0 errors, 0 warnings

---

## Adding Menu Items for New Features

### Step 1: Add to MainForm.Designer.cs
```csharp
// In InitializeComponent():
this.myNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
this.myNewMenuItem.Name = "myNewMenuItem";
this.myNewMenuItem.Text = "&My New Feature";
this.myNewMenuItem.ShortcutKeys = Keys.Control | Keys.N;
this.myNewMenuItem.Click += new EventHandler(this.myNewMenuItem_Click);

// Add to parent menu:
this.editMenuItem.DropDownItems.Add(this.myNewMenuItem);
```

### Step 2: Add handler in MainForm.cs
```csharp
private void myNewMenuItem_Click(object sender, EventArgs e)
{
    // Feature implementation
}
```

---

## Adding Toolbar Buttons

```csharp
// In MainForm.Designer.cs:
this.myNewButton = new System.Windows.Forms.ToolStripButton();
this.myNewButton.Image = Resources.my_icon;
this.myNewButton.ToolTipText = "My New Feature (Ctrl+N)";
this.myNewButton.Click += new EventHandler(this.myNewMenuItem_Click);

// Add to toolbar:
this.mainToolStrip.Items.Add(this.myNewButton);
```

---

## Current Resource Summary

### String Resources: 47 (100% used)
Organized by prefix:
- `ERR_*` (29) - Error messages
- `MSG_*` (16) - Success/info messages  
- `DIALOG_*` (1) - Dialog titles
- `TITLE` (1) - Application title

### Image Resources: 15 (100% used)
All toolbar icons and status indicators

---

## Maintenance Checklist

### Before Each Release:
```
? Run verify-strings.ps1 (ensure no new hard-coded strings)
? Run verify-resources.ps1 (ensure 100% utilization)
? Build solution (ensure clean build)
? Test major features
? Update version number
? Update documentation if needed
```

### Monthly:
```
? Review and remove any new unused resources
? Check for code quality issues
? Update dependencies (Newtonsoft.Json)
? Review GitHub issues
```

---

## Common Tasks

### Add new language:
```
1. Copy Resources.resx ? Resources.{lang}.resx
2. Translate all values
3. Build and test
```

### Add new resource:
```
1. Open Resources.resx in Visual Studio
2. Add Resource ? Add String
3. Enter name (follow conventions)
4. Enter value
5. Save and build
6. Use in code: Resources.YOUR_RESOURCE_NAME
```

### Remove unused resource:
```
1. Run verify-resources.ps1 to find unused
2. Open Resources.resx
3. Find and remove unused resources
4. Save and build
5. Verify 100% utilization
```

---

## ?? PROJECT STATUS

**All Requirements:** ? 100% Complete  
**Build:** ? Clean  
**Production:** ? Ready  
**Quality:** ? Professional  

**Last Updated:** 2024-04-10  
**Branch:** refactor_v4  
**Version:** 2.0 (Post-Refactoring & Cleanup)  

---

**Use this document for future reference and maintenance!** ??
