# Layout Issues Audit - Comprehensive Analysis

## Systematic Search for Layout Problems

Searching for patterns that cause layout overlap issues:
1. Controls with Dock.Fill or Dock.Top
2. Hardcoded Location with Dock
3. Incorrect Controls.Add order
4. Manual Size with Dock

## Files to Audit

### Primary Targets
- [x] MainForm.Designer.cs - **FIXED**
- [ ] AiAssistantPanel.cs
- [ ] CallGraphPanel.cs
- [ ] DependencyGraphPanel.cs
- [ ] FlameGraphPanel.cs - Simple panel, no nested controls
- [ ] TimelinePanel.cs - Simple panel, no nested controls

### Secondary (Dialogs - likely OK)
- AboutForm.Designer.cs - Uses TableLayoutPanel (OK)
- FilterForm.Designer.cs - Fixed dialog with manual positioning (OK)
- FindForm.Designer.cs - Fixed dialog with manual positioning (OK)
- SettingsForm.Designer.cs - Fixed dialog with manual positioning (OK)

## Issues Found

### 1. MainForm.Designer.cs ? FIXED
**Issue**: 
- MenuStrip had `Location = (0, 0)` hardcoded
- ToolStrip had `Location = (0, 26)` and `Size = (987, 27)` hardcoded
- Wrong Controls.Add order
- mainSplitContainer.Panel1 had trees directly with Dock.Fill

**Fix Applied**:
- Removed Location and Size from MenuStrip and ToolStrip
- Reversed Controls.Add order
- Trees use Anchor with manual positioning via LayoutTrees()
- Added OnLoad/OnShown/Resize handlers

### 2. AiAssistantPanel.cs - POTENTIAL ISSUE
**Issue**:
```csharp
// Add order - MIGHT BE WRONG
Controls.Add(_responseTextBox);  // Dock.Fill - added FIRST
Controls.Add(queryPanel);        // Dock.Top
Controls.Add(_buttonPanel);      // Dock.Top
Controls.Add(_statusLabel);      // Dock.Bottom
```

**Expected Behavior**:
```
Process order (reverse):
1. _statusLabel (Dock.Bottom) ? Takes bottom space
2. _buttonPanel (Dock.Top) ? Takes top space
3. queryPanel (Dock.Top) ? Takes top space (below buttonPanel)
4. _responseTextBox (Dock.Fill) ? Fills remaining
```

**Correct Order Should Be**:
```csharp
Controls.Add(_responseTextBox);  // Dock.Fill - OK (processed last)
Controls.Add(_statusLabel);      // Dock.Bottom - Should be first!
Controls.Add(_buttonPanel);      // Dock.Top
Controls.Add(queryPanel);        // Dock.Top - Should be last!
```

**Need to verify if this causes issues in practice**

### 3. CallGraphPanel.cs - TO CHECK

### 4. DependencyGraphPanel.cs - TO CHECK

## Analysis Complete

Checking remaining files...
