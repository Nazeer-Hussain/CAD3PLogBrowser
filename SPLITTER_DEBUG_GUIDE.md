# ?? SPLITTER DISTANCE DEBUG GUIDE

**Issue:** Splitter distance still not persisting correctly  
**Current Saved Value:** 498 pixels  

---

## ?? DEBUG STEPS

### Step 1: Verify Event is Firing
Add this temporary code to `splitContainer1_SplitterMoved`:

```csharp
private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
{
    LayoutTrees();

    // DEBUG: Show message box when splitter moves
    System.Diagnostics.Debug.WriteLine($"Splitter moved to: {mainSplitContainer.SplitterDistance}");

    if (_appSettings != null && mainSplitContainer != null)
    {
        _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;
        System.Diagnostics.Debug.WriteLine($"Saved to _appSettings: {_appSettings.SplitterDistance}");
    }
}
```

### Step 2: Verify Settings are Saved on Close
Add this to `SaveSettings()`:

```csharp
private void SaveSettings()
{
    try
    {
        // Update all settings
        _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;

        // DEBUG
        System.Diagnostics.Debug.WriteLine($"SaveSettings: Splitter = {_appSettings.SplitterDistance}");

        // ... rest of code ...

        _appSettings.Save();

        // DEBUG: Verify it was written
        System.Diagnostics.Debug.WriteLine($"Settings saved to disk");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"ERROR saving settings: {ex.Message}");
    }
}
```

### Step 3: Verify Settings are Loaded on Start
Add this to `RestoreSettings()`:

```csharp
private void RestoreSettings()
{
    // DEBUG
    System.Diagnostics.Debug.WriteLine($"RestoreSettings START");
    System.Diagnostics.Debug.WriteLine($"  Loaded SplitterDistance from file: {_appSettings.SplitterDistance}");

    // ... window state code ...

    // Feature 2a/2b: Default splitter to 30% if not set
    int dist = _settingsService.LoadSplitterDistance();
    System.Diagnostics.Debug.WriteLine($"  LoadSplitterDistance returned: {dist}");

    if (dist > 0)
    {
        mainSplitContainer.SplitterDistance = dist;
        System.Diagnostics.Debug.WriteLine($"  Set mainSplitContainer.SplitterDistance to: {dist}");
    }

    // ... rest of code ...
}
```

### Step 4: Verify MainForm_Load Doesn't Override
Add this to `MainForm_Load()`:

```csharp
private void MainForm_Load(object sender, EventArgs e)
{
    SetDocumentLoaded(false);
    LayoutTrees();

    // DEBUG
    System.Diagnostics.Debug.WriteLine($"MainForm_Load START");
    System.Diagnostics.Debug.WriteLine($"  _appSettings.SplitterDistance = {_appSettings.SplitterDistance}");
    System.Diagnostics.Debug.WriteLine($"  mainSplitContainer.SplitterDistance = {mainSplitContainer.SplitterDistance}");

    if (_appSettings.SplitterDistance <= 0)
    {
        int defaultSplitter = (int)(this.ClientSize.Width * 0.3);
        System.Diagnostics.Debug.WriteLine($"  Applying default splitter: {defaultSplitter}");
        mainSplitContainer.SplitterDistance = defaultSplitter;
    }
    else
    {
        System.Diagnostics.Debug.WriteLine($"  Using saved splitter value");
    }

    System.Diagnostics.Debug.WriteLine($"  FINAL mainSplitContainer.SplitterDistance = {mainSplitContainer.SplitterDistance}");

    // ... rest of code ...
}
```

---

## ?? EXPECTED DEBUG OUTPUT

### On Startup:
```
RestoreSettings START
  Loaded SplitterDistance from file: 498
  LoadSplitterDistance returned: 498
  Set mainSplitContainer.SplitterDistance to: 498
MainForm_Load START
  _appSettings.SplitterDistance = 498
  mainSplitContainer.SplitterDistance = 498
  Using saved splitter value
  FINAL mainSplitContainer.SplitterDistance = 498
```

### When Moving Splitter:
```
Splitter moved to: 350
Saved to _appSettings: 350
Splitter moved to: 355
Saved to _appSettings: 355
... (many lines as you drag)
Splitter moved to: 400
Saved to _appSettings: 400
```

### On Close:
```
SaveSettings: Splitter = 400
Settings saved to disk
```

---

## ?? COMMON ISSUES & FIXES

### Issue 1: Event Not Firing
**Symptom:** No "Splitter moved to" messages in debug output  
**Cause:** Event not wired in Designer  
**Fix:** Check MainForm.Designer.cs line 530 for:
```csharp
this.mainSplitContainer.SplitterMoved += new 
    System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
```

### Issue 2: Value Resets in MainForm_Load
**Symptom:** Splitter value changes in MainForm_Load  
**Cause:** Logic checking `_appSettings.SplitterDistance <= 0` when it's actually > 0  
**Fix:** Already applied - removed `|| == 285` check

### Issue 3: Settings Not Saved to Disk
**Symptom:** No "Settings saved to disk" message  
**Cause:** Exception in Save() or FormClosed not firing  
**Fix:** Check for exceptions, ensure app closes normally

### Issue 4: File Being Read But Wrong Value
**Symptom:** File shows correct value but app loads different value  
**Cause:** Multiple AppSettings instances (should be fixed now)  
**Fix:** Verify only one AppSettings instance created

---

## ?? MANUAL TEST PROCEDURE

### Test 1: Basic Save/Load
1. Close app if running
2. Delete settings file: `Remove-Item (Join-Path $env:APPDATA "CAD3PLogBrowser\settings.json")`
3. Start app - should use 30% default
4. Move splitter to FAR LEFT (e.g., 200 pixels)
5. Close app normally
6. Check file: `(Get-Content (Join-Path $env:APPDATA "CAD3PLogBrowser\settings.json") | ConvertFrom-Json).SplitterDistance`
7. Should show ~200
8. Start app - splitter should be at FAR LEFT

### Test 2: Update Existing
1. Start app (should be at saved position from Test 1)
2. Move splitter to FAR RIGHT (e.g., 800 pixels)
3. Close app
4. Check file - should show ~800
5. Start app - should be at FAR RIGHT

### Test 3: Maximized Window
1. Start app
2. Maximize window
3. Move splitter
4. Close app
5. Start app (should open maximized)
6. Splitter should be at saved position

---

## ?? QUICK FIX OPTIONS

### Option A: Force Save on Every Splitter Move
Uncomment in `splitContainer1_SplitterMoved`:
```csharp
_appSettings.Save();  // Uncomment this line
```

**Pros:** Guaranteed to save  
**Cons:** Many disk writes (slow for SSDs)

### Option B: Debounced Save (Recommended)
Add timer to save 2 seconds after user stops moving:
```csharp
private System.Windows.Forms.Timer _settingsSaveTimer;

private void InitSettingsSaveTimer()
{
    _settingsSaveTimer = new Timer();
    _settingsSaveTimer.Interval = 2000; // 2 seconds
    _settingsSaveTimer.Tick += (s, e) =>
    {
        _appSettings.Save();
        _settingsSaveTimer.Stop();
        System.Diagnostics.Debug.WriteLine("Settings auto-saved");
    };
}

private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
{
    LayoutTrees();
    if (_appSettings != null)
    {
        _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;
        _settingsSaveTimer.Stop();
        _settingsSaveTimer.Start(); // Restart timer
    }
}
```

### Option C: Current Method (Save on Close Only)
Should work if FormClosed fires correctly

---

## ?? CURRENT STATUS

**Settings File Location:**
```
%APPDATA%\CAD3PLogBrowser\settings.json
```

**Current Saved Value:** 498 pixels

**Expected Behavior:**
- Splitter should restore to 498 pixels on startup
- Moving splitter should update in-memory value
- Closing app should save to disk
- Next startup should show saved value

---

## ?? NEXT STEPS

1. ? **Add debug output** (temporary)
2. ? **Run app and move splitter**
3. ? **Check Debug output window**
4. ? **Identify where value is lost**
5. ? **Apply specific fix**

---

**To view debug output:**
- In Visual Studio: View > Output
- Select "Debug" from dropdown
- Look for Debug.WriteLine messages

---

**Created:** 2025-01-15  
**Purpose:** Diagnose splitter distance persistence issue  
**Status:** Debugging in progress  
