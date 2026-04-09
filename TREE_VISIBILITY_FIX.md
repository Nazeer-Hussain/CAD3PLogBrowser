# Tree Visibility Fix - Mutually Exclusive Trees

## ? Issue Fixed

Changed tree visibility behavior so that only Call Tree OR API Tree is visible at any time, making them mutually exclusive.

---

## ?? Problem

**Before:**
- Both Call Tree and API Tree could be visible simultaneously
- Confusing UX - two trees stacked vertically
- Unclear which tree was active
- Space wasted showing both

---

## ? Solution

**After:**
- Only ONE tree visible at any time
- Clicking Call Tree ? hides API Tree
- Clicking API Tree ? hides Call Tree
- Always at least one tree visible (defaults to Call Tree)

---

## ?? Behavior

### Toggle Logic

**Scenario 1: Switch from Call Tree to API Tree**
```
Current: Call Tree ? visible
Action: Click "API Tree" button
Result: API Tree ? visible, Call Tree ? hidden
```

**Scenario 2: Switch from API Tree to Call Tree**
```
Current: API Tree ? visible
Action: Click "Call Tree" button
Result: Call Tree ? visible, API Tree ? hidden
```

**Scenario 3: Try to Uncheck Current Tree**
```
Current: Call Tree ? visible
Action: Click "Call Tree" button to uncheck
Result: Call Tree ? remains visible (at least one must be visible)
```

**Scenario 4: Context Menu "Show in Other Tree"**
```
Current: Call Tree ? visible
Action: Right-click node ? "Show in API Tree"
Result: Switches to API Tree ?, Call Tree ? hidden
```

---

## ?? Implementation Details

### SyncTreeVisibility() - Enhanced

**New Logic:**
```csharp
1. Check if both trees are requested
   if (showCall && showApi):
     Determine which was just clicked
     Uncheck the other one

2. Ensure at least one tree is visible
   if (!showCall && !showApi):
     Default to Call Tree

3. Update visibility and checkmarks
   CallTree.Visible = showCall
   ApiTree.Visible = showApi
   Sync menu and toolbar checkmarks
```

### ShowApiTree() - Updated
```csharp
// Explicitly hide Call Tree
CallTreeButton.Checked = false
showCallTreeMenuItem.Checked = false

// Show API Tree
ApiTreeButton.Checked = true
showApiListMenuItem.Checked = true
```

### ShowCallTree() - Updated
```csharp
// Explicitly hide API Tree
ApiTreeButton.Checked = false
showApiListMenuItem.Checked = false

// Show Call Tree
CallTreeButton.Checked = true
showCallTreeMenuItem.Checked = true
```

---

## ?? User Experience

### Before (Both Trees Could Be Visible)
```
???????????????????????
? API Tree            ?
? ?? Method1 (3)      ?
? ?? Method2 (5)      ?
? ?? Method3 (2)      ?
??????????????????????? ? Split point
? Call Tree           ?
? ?? Method1 [10ms]   ?
? ?? Method2 [20ms]   ?
? ?? Method3 [15ms]   ?
???????????????????????

Problem: Confusing, both trees visible
```

### After (Only One Tree Visible)
```
Option 1: Call Tree Selected
???????????????????????
? Call Tree           ?
? ?? Method1 [10ms]   ?
? ?  ?? SubCall [5ms] ?
? ?? Method2 [20ms]   ?
? ?? Method3 [15ms]   ?
?                     ?
?   (full height)     ?
???????????????????????

Option 2: API Tree Selected
???????????????????????
? API Tree            ?
? ?? Method1 (3)      ?
? ?? Method2 (5)      ?
? ?? Method3 (2)      ?
?                     ?
?   (full height)     ?
???????????????????????

Result: Clear, focused view
```

---

## ?? Toggle Methods

### Via Toolbar Buttons
```
[Call Tree] [API Tree]
    ?           ?        ? Call Tree visible
    ?           ?        ? API Tree visible

Click API Tree ? switches to API Tree
Click Call Tree ? switches to Call Tree
```

### Via View Menu
```
View
 ?? Show Call Tree    ?
 ?? Show API List     ?
```

### Via Context Menu
```
Right-click tree node
 ?? Show in API Tree
    ?? Switches to API Tree and finds method
```

---

## ? Testing Checklist

### Mutual Exclusivity
- [x] Click Call Tree ? API Tree hides
- [x] Click API Tree ? Call Tree hides
- [x] Only one tree visible at a time
- [x] Cannot uncheck current tree (always one visible)

### Synchronization
- [x] Toolbar button syncs with View menu
- [x] View menu syncs with toolbar button
- [x] Both update together

### Context Menu Integration
- [x] "Show in API Tree" switches trees
- [x] "Show in Call Tree" switches trees
- [x] Finds and selects matching node
- [x] Tree switching works seamlessly

### Default Behavior
- [x] App starts with Call Tree visible by default
- [x] If both somehow get unchecked ? defaults to Call Tree
- [x] LayoutTrees() called after visibility change

---

## ?? Code Changes

### Modified Method: SyncTreeVisibility()
**Lines Added:** ~20  
**Logic:** Mutual exclusivity + always-one-visible guarantee

### Modified Methods: ShowApiTree(), ShowCallTree()
**Enhancement:** Explicitly hide the other tree  
**Benefit:** Clear, predictable behavior

---

## ?? Benefits

### User Experience
- ? Clear focus on one tree at a time
- ? No confusion about which tree is active
- ? Full height for the active tree
- ? Simple toggle behavior

### Screen Real Estate
- ? Tree panel uses full height for one tree
- ? No split-panel confusion
- ? More nodes visible without scrolling
- ? Better use of space

### Navigation
- ? Context menu "Show in Other Tree" works perfectly
- ? Clear visual indication of active tree
- ? Easy switching between views
- ? Predictable toggle behavior

---

## ?? Build Status

? **Build successful**  
? **No compilation errors**  
? **Logic tested**  
? **Ready for commit**

---

## ?? Result

**Before:** Both trees could be visible (confusing)  
**After:** Only one tree visible at a time (clear)  

Trees now behave as **mutually exclusive toggle options** as intended.
