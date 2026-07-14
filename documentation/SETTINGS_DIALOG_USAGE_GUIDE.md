# Settings Dialog Integration - Usage Guide

## Quick Reference

### Opening Settings Dialog with Specific Tab

```csharp
using (var settingsDialog = new SettingsForm(this, SettingsForm.TabIndexComparison))
{
    if (settingsDialog.ShowDialog(this) == DialogResult.OK)
    {
        // Settings saved - apply changes
    }
}
```

### Tab Index Constants

```csharp
SettingsForm.TabIndexAppearance      // 0 - Theme, colors, toolbar
SettingsForm.TabIndexTabsLayout      // 1 - Visible tabs, initial view
SettingsForm.TabIndexLogFont         // 2 - Font settings
SettingsForm.TabIndexFiles           // 3 - File paths, recent files
SettingsForm.TabIndexPerformance     // 4 - Performance thresholds
SettingsForm.TabIndexAIIntegration   // 5 - AI provider, API keys
SettingsForm.TabIndexComparison      // 6 - Comparison options
SettingsForm.TabIndexUpdates         // 7 - Update settings
```

## Integration Examples

### Example 1: AI Assistant Settings Button

```csharp
// In MainForm.cs where the AI panel is created:
_aiPanel.SettingsRequested += (s, e) =>
{
    using (var settingsDialog = new SettingsForm(this, SettingsForm.TabIndexAIIntegration))
    {
        if (settingsDialog.ShowDialog(this) == DialogResult.OK)
        {
            // Apply any theme or font changes
            ApplySettings();

            // Refresh AI service with new settings
            _aiPanel.RefreshAIService();
        }
    }
};
```

### Example 2: Comparison/Difference Settings

```csharp
// In CompareLogsForm.cs (already implemented):
private void optionsButton_Click(object sender, EventArgs e)
{
    var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
    if (mainForm != null)
    {
        using (var settingsDialog = new SettingsForm(mainForm, SettingsForm.TabIndexComparison))
        {
            if (settingsDialog.ShowDialog(this) == DialogResult.OK)
            {
                _compareOptions = settingsDialog.CompareOptions;

                if (leftTreeView.Nodes.Count > 0 && rightTreeView.Nodes.Count > 0)
                {
                    PerformComparison();
                }
            }
        }
    }
}
```

### Example 3: General Settings (Menu Item)

```csharp
// In MainForm.cs - menu item click handler:
private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
{
    using (var settingsDialog = new SettingsForm(this))
    {
        if (settingsDialog.ShowDialog(this) == DialogResult.OK)
        {
            ApplySettings();
        }
    }
}

private void ApplySettings()
{
    // Apply theme changes
    ThemeManager.ApplyTheme(this);

    // Apply font changes to log views
    ApplyFontSettings();

    // Apply toolbar visibility
    ApplyToolbarVisibility();

    // Refresh tab visibility
    UpdateTabVisibility();

    // Refresh any other components that depend on settings
    _aiPanel?.RefreshAIService();
}
```

### Example 4: Toolbar Settings Button

```csharp
// Add settings button to toolbar:
var btnSettings = new ToolStripButton
{
    Text = "Settings",
    DisplayStyle = ToolStripItemDisplayStyle.Text,
    ToolTipText = "Open Settings"
};
btnSettings.Click += (s, e) =>
{
    using (var settingsDialog = new SettingsForm(this))
    {
        if (settingsDialog.ShowDialog(this) == DialogResult.OK)
        {
            ApplySettings();
        }
    }
};
mainToolStrip.Items.Add(btnSettings);
```

## Getting Comparison Options

If you need to access the comparison options from the dialog:

```csharp
using (var settingsDialog = new SettingsForm(mainForm, SettingsForm.TabIndexComparison))
{
    if (settingsDialog.ShowDialog(this) == DialogResult.OK)
    {
        // Get the configured comparison options
        var compareOptions = settingsDialog.CompareOptions;

        // Use them for comparison
        _comparer.Options = compareOptions;
    }
}
```

## Checking for Settings Changes

The dialog returns `DialogResult.OK` when the user clicks OK, and `DialogResult.Cancel` when cancelled:

```csharp
using (var settingsDialog = new SettingsForm(this, SettingsForm.TabIndexAIIntegration))
{
    var result = settingsDialog.ShowDialog(this);

    if (result == DialogResult.OK)
    {
        // User clicked OK - settings were saved
        ApplySettings();
    }
    else
    {
        // User cancelled - no changes were made
    }
}
```

## Best Practices

### 1. Always Dispose Properly
```csharp
using (var settingsDialog = new SettingsForm(this))
{
    // Use dialog
} // Automatically disposed
```

### 2. Apply Settings After OK
```csharp
if (settingsDialog.ShowDialog(this) == DialogResult.OK)
{
    ApplySettings(); // Apply changes to UI
    SaveSettings();  // Settings are auto-saved, but apply them
}
```

### 3. Pass MainForm Reference
```csharp
// Always pass MainForm for full functionality
var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
if (mainForm != null)
{
    using (var settingsDialog = new SettingsForm(mainForm))
    {
        // ...
    }
}
```

### 4. Refresh Components After Settings Change
```csharp
if (settingsDialog.ShowDialog(this) == DialogResult.OK)
{
    ApplySettings();
    _aiPanel?.RefreshAIService();
    _treeViewManager?.RefreshView();
    // Refresh any other components that depend on settings
}
```

## Migration from Old Dialogs

### Replacing AISettingsDialog

**Old Code:**
```csharp
using (var aiDialog = new AISettingsDialog())
{
    if (aiDialog.ShowDialog(this) == DialogResult.OK)
    {
        _aiPanel.RefreshAIService();
    }
}
```

**New Code:**
```csharp
using (var settingsDialog = new SettingsForm(this, SettingsForm.TabIndexAIIntegration))
{
    if (settingsDialog.ShowDialog(this) == DialogResult.OK)
    {
        ApplySettings();
        _aiPanel.RefreshAIService();
    }
}
```

### Replacing CompareOptionsDialog

**Old Code:**
```csharp
using (var optionsDialog = new CompareOptionsDialog(_compareOptions))
{
    if (optionsDialog.ShowDialog(this) == DialogResult.OK)
    {
        _compareOptions = optionsDialog.Options;
        PerformComparison();
    }
}
```

**New Code:**
```csharp
var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
if (mainForm != null)
{
    using (var settingsDialog = new SettingsForm(mainForm, SettingsForm.TabIndexComparison))
    {
        if (settingsDialog.ShowDialog(this) == DialogResult.OK)
        {
            _compareOptions = settingsDialog.CompareOptions;
            PerformComparison();
        }
    }
}
```

## Common Scenarios

### Scenario 1: User clicks AI Settings button
? Opens Settings dialog on AI & Integration tab

### Scenario 2: User clicks Options in Compare form
? Opens Settings dialog on Comparison tab

### Scenario 3: User selects Settings from menu
? Opens Settings dialog on first tab (Appearance)

### Scenario 4: User presses Ctrl+, keyboard shortcut
? Opens Settings dialog on first tab (Appearance)

## Properties and Methods

### Public Properties
```csharp
public Models.Comparison.CompareOptions CompareOptions { get; }
```

### Public Constants
```csharp
public const int TabIndexAppearance = 0;
public const int TabIndexTabsLayout = 1;
public const int TabIndexLogFont = 2;
public const int TabIndexFiles = 3;
public const int TabIndexPerformance = 4;
public const int TabIndexAIIntegration = 5;
public const int TabIndexComparison = 6;
public const int TabIndexUpdates = 7;
```

### Constructors
```csharp
public SettingsForm(MainForm mainForm)
public SettingsForm(MainForm mainForm, int initialTabIndex)
```

## Testing Your Integration

```csharp
// Test opening each tab
for (int i = 0; i < 8; i++)
{
    using (var dialog = new SettingsForm(this, i))
    {
        Console.WriteLine($"Opening tab {i}");
        dialog.ShowDialog(this);
    }
}
```

## Troubleshooting

### Dialog doesn't open on correct tab
- Check that you're using the correct tab index constant
- Verify the tab index is within range (0-7)
- Make sure `_tabControl` is properly initialized

### Settings not persisting
- Ensure `DialogResult.OK` is returned
- Check that `OkButton_Click()` is being called
- Verify `_settings.Save()` is called

### Theme not applying
- Call `ApplySettings()` after dialog closes with OK
- Ensure `ThemeManager.ApplyTheme(this)` is called
- Check that theme name is valid

### AI service not refreshing
- Call `_aiPanel.RefreshAIService()` after settings change
- Ensure AISettings are being saved properly
- Verify AI provider is configured correctly

## Complete Example

```csharp
// Complete integration example for MainForm.cs
public partial class MainForm : Form
{
    private void InitializeComponent()
    {
        // ... other initialization

        // Wire up AI panel settings
        _aiPanel.SettingsRequested += OnAISettingsRequested;

        // Wire up menu item
        settingsToolStripMenuItem.Click += OnSettingsMenuItemClick;
    }

    private void OnAISettingsRequested(object sender, EventArgs e)
    {
        ShowSettings(SettingsForm.TabIndexAIIntegration);
    }

    private void OnSettingsMenuItemClick(object sender, EventArgs e)
    {
        ShowSettings();
    }

    private void ShowSettings(int tabIndex = -1)
    {
        using (var settingsDialog = new SettingsForm(this, tabIndex))
        {
            if (settingsDialog.ShowDialog(this) == DialogResult.OK)
            {
                ApplyAllSettings();
            }
        }
    }

    private void ApplyAllSettings()
    {
        // Apply theme
        ThemeManager.ApplyTheme(this);

        // Apply fonts
        ApplyFontSettings();

        // Apply toolbar visibility
        ApplyToolbarVisibility();

        // Update tab visibility
        UpdateTabVisibility();

        // Refresh AI service
        _aiPanel?.RefreshAIService();

        // Refresh any other components
        Refresh();
    }
}
```

## Summary

The unified Settings dialog provides:
- ? Single location for all settings
- ? Context-aware tab opening
- ? Easy integration with existing code
- ? Consistent user experience
- ? Simplified maintenance

Use the tab index constants to open specific tabs based on context, and always apply settings after the dialog closes with OK.
