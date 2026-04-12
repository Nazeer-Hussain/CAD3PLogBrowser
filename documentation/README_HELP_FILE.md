# ?? Help File Creation Guide

## Overview

This folder contains the source files for creating the `Cad3PLogBrowser.chm` help file that integrates with the application.

---

## Files

- **UserGuide.html** - Complete user guide in HTML format (can be viewed directly in browser)
- **README_HELP_FILE.md** - This file (instructions for creating CHM)

---

## Quick Start (View Help Now)

**Option 1: View HTML directly**
```
1. Open UserGuide.html in any web browser
2. Fully functional, searchable, and formatted
3. No compilation needed
```

**Option 2: Create CHM file (Windows Help format)**
Follow the instructions below to compile into Cad3PLogBrowser.chm

---

## Creating CHM Help File

### Prerequisites

You need **Microsoft HTML Help Workshop** (free from Microsoft):

1. Download from: [Microsoft HTML Help Workshop](https://www.microsoft.com/en-us/download/details.aspx?id=21138)
2. Install hhw.exe
3. Or use command-line: `hhc.exe` (HTML Help Compiler)

### Method 1: Simple Single-File CHM

**Step 1: Create Project File**

Create `Cad3PLogBrowser.hhp` with this content:

```ini
[OPTIONS]
Compatibility=1.1 or later
Compiled file=Cad3PLogBrowser.chm
Contents file=Cad3PLogBrowser.hhc
Default Window=Main
Default topic=UserGuide.html
Display compile progress=Yes
Language=0x409 English (United States)
Title=CAD 3P Log Browser - User Guide

[WINDOWS]
Main="CAD 3P Log Browser - Help","Cad3PLogBrowser.hhc",,"UserGuide.html","UserGuide.html",,,,,0x63520,,0x387e,,,,,,,,0

[FILES]
UserGuide.html
```

**Step 2: Create Table of Contents**

Create `Cad3PLogBrowser.hhc` with this content:

```html
<!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML//EN">
<HTML>
<HEAD>
<meta name="GENERATOR" content="Microsoft&reg; HTML Help Workshop 4.1">
<!-- Sitemap 1.0 -->
</HEAD><BODY>
<OBJECT type="text/site properties">
    <param name="Window Styles" value="0x800025">
    <param name="ImageType" value="Folder">
</OBJECT>
<UL>
    <LI> <OBJECT type="text/sitemap">
        <param name="Name" value="CAD 3P Log Browser User Guide">
        <param name="Local" value="UserGuide.html">
        </OBJECT>
    <UL>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Introduction">
            <param name="Local" value="UserGuide.html#introduction">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Getting Started">
            <param name="Local" value="UserGuide.html#getting-started">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="File Operations">
            <param name="Local" value="UserGuide.html#file-operations">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Search & Filter">
            <param name="Local" value="UserGuide.html#search-filter">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Bookmarks">
            <param name="Local" value="UserGuide.html#bookmarks">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Navigation">
            <param name="Local" value="UserGuide.html#navigation">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Tree Views">
            <param name="Local" value="UserGuide.html#tree-views">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Performance Analysis">
            <param name="Local" value="UserGuide.html#performance">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Visualizations">
            <param name="Local" value="UserGuide.html#visualizations">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Export Features">
            <param name="Local" value="UserGuide.html#export">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Settings & Configuration">
            <param name="Local" value="UserGuide.html#settings">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Keyboard Shortcuts">
            <param name="Local" value="UserGuide.html#keyboard-shortcuts">
            </OBJECT>
        <LI> <OBJECT type="text/sitemap">
            <param name="Name" value="Tips & Tricks">
            <param name="Local" value="UserGuide.html#tips">
            </OBJECT>
    </UL>
</UL>
</BODY></HTML>
```

**Step 3: Compile**

```cmd
hhc.exe Cad3PLogBrowser.hhp
```

This creates `Cad3PLogBrowser.chm`

### Method 2: Using HTML Help Workshop GUI

1. Open **HTML Help Workshop**
2. File ? New ? Project
3. Click Next, enter "Cad3PLogBrowser" as name
4. Click Next, add UserGuide.html as existing file
5. Click Finish
6. Click "Compile"
7. Creates Cad3PLogBrowser.chm

---

## Installing Help File

**Option 1: Manual Installation**

```
1. Copy Cad3PLogBrowser.chm to the application directory
   (same folder as Cad3PLogBrowser.exe)

2. The application will automatically find it when user presses F1
```

**Option 2: Include in Build**

Add to Cad3PLogBrowser.csproj:

```xml
<ItemGroup>
  <Content Include="Cad3PLogBrowser.chm">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

## Testing Help Integration

**Test in Application:**

1. Run Cad3PLogBrowser.exe
2. Press **F1** or select **Help ? Help**
3. Help file should open
4. Test navigation using Table of Contents
5. Test search functionality

**If Help Doesn't Open:**

Check these:
- Cad3PLogBrowser.chm exists in application directory
- Windows hasn't blocked the file (Right-click ? Properties ? Unblock)
- File is not corrupted (try opening directly in Explorer)

---

## Command-Line Compilation (Automated)

Create `build-help.bat`:

```batch
@echo off
echo Building CAD 3P Log Browser Help File...

REM Check if HHC is installed
if not exist "C:\Program Files (x86)\HTML Help Workshop\hhc.exe" (
    echo ERROR: HTML Help Compiler not found!
    echo Please install Microsoft HTML Help Workshop
    exit /b 1
)

REM Compile help file
"C:\Program Files (x86)\HTML Help Workshop\hhc.exe" Cad3PLogBrowser.hhp

if exist Cad3PLogBrowser.chm (
    echo.
    echo SUCCESS: Cad3PLogBrowser.chm created!
    echo.
    echo Next steps:
    echo 1. Copy Cad3PLogBrowser.chm to application directory
    echo 2. Test by pressing F1 in the application
) else (
    echo.
    echo ERROR: Failed to create CHM file!
)

pause
```

Run: `build-help.bat`

---

## Alternative: PowerShell Script

Create `Build-HelpFile.ps1`:

```powershell
# Build CAD 3P Log Browser Help File
$hhcPath = "C:\Program Files (x86)\HTML Help Workshop\hhc.exe"
$projectFile = "Cad3PLogBrowser.hhp"
$outputFile = "Cad3PLogBrowser.chm"

Write-Host "Building CAD 3P Log Browser Help File..." -ForegroundColor Cyan

if (-not (Test-Path $hhcPath)) {
    Write-Host "ERROR: HTML Help Compiler not found!" -ForegroundColor Red
    Write-Host "Please install Microsoft HTML Help Workshop" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $projectFile)) {
    Write-Host "ERROR: $projectFile not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Compiling help file..." -ForegroundColor White
& $hhcPath $projectFile

if (Test-Path $outputFile) {
    Write-Host ""
    Write-Host "SUCCESS: $outputFile created!" -ForegroundColor Green
    Write-Host ""
    Write-Host "File size: $([math]::Round((Get-Item $outputFile).Length/1KB, 2)) KB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Copy $outputFile to application bin directory"
    Write-Host "2. Test by pressing F1 in the application"
} else {
    Write-Host ""
    Write-Host "ERROR: Failed to create CHM file!" -ForegroundColor Red
}
```

Run: `powershell -ExecutionPolicy Bypass -File Build-HelpFile.ps1`

---

## Current Status

? **UserGuide.html** - Created (comprehensive, 77 features documented)  
? **Cad3PLogBrowser.hhp** - Needs to be created (see instructions above)  
? **Cad3PLogBrowser.hhc** - Needs to be created (see instructions above)  
? **Cad3PLogBrowser.chm** - Needs to be compiled  

---

## Application Integration

The application already has complete CHM integration:

**Code Location:** `MainForm.cs` line 2181

```csharp
private void viewHelpMenuItem_Click(object sender, EventArgs e)
{
    try
    {
        // Try to open CHM help file if it exists
        string helpFilePath = Path.Combine(Application.StartupPath, "Cad3PLogBrowser.chm");

        if (File.Exists(helpFilePath))
        {
            System.Diagnostics.Process.Start(helpFilePath);
        }
        else
        {
            // If CHM doesn't exist, show fallback message
            MessageBox.Show(Resources.MSG_HELP_FILE_NOT_FOUND,
                Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(string.Format(Resources.ERR_OPEN_HELP_FAILED, ex.Message), 
            Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
```

**Menu Paths:**
- Help ? Help (F1)
- Toolbar ? Help button

---

## Updating Help File

When features are added or changed:

1. Edit `UserGuide.html`
2. Update relevant sections
3. Recompile CHM: `hhc.exe Cad3PLogBrowser.hhp`
4. Replace old .chm with new one
5. Test in application

---

## Tips

**Styling:**
- Current HTML uses clean, professional styling
- Colors match application theme
- Responsive design works on any screen size
- Print-friendly layout

**Search:**
- CHM format includes full-text search automatically
- No configuration needed
- Search index built during compilation

**Navigation:**
- Table of Contents auto-generated from .hhc file
- All sections are bookmarked with #anchors
- Easy keyboard navigation (Tab through links)

---

## Troubleshooting

**Problem:** CHM file won't open (Windows security)

**Solution:**
```
1. Right-click Cad3PLogBrowser.chm
2. Properties
3. Check "Unblock" checkbox
4. Click OK
```

**Problem:** HHC.exe not found

**Solution:**
- Install HTML Help Workshop from Microsoft
- Or use HTML directly (UserGuide.html works perfectly in browser)

**Problem:** Compilation fails

**Solution:**
- Check all files are in same directory
- Verify .hhp and .hhc files are correct
- Check for typos in filenames

---

## Summary

**What You Have:**
? Complete user guide (HTML format)
? All 77 features documented
? Keyboard shortcuts reference
? Tips & tricks
? Troubleshooting guide
? Application already integrated

**What's Needed:**
? Create .hhp and .hhc files (copy from this README)
? Compile with HHC.exe
? Copy .chm to application directory

**Total Time:** ~5 minutes to compile CHM

---

**Last Updated:** 2024-04-11  
**Status:** HTML ready, CHM compilation pending  
**Location:** documentation/UserGuide.html  
