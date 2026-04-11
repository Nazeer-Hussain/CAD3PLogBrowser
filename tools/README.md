# ??? Tools & Utilities

This folder contains all supporting scripts and utilities for the CAD 3P Log Browser project.

---

## ?? PowerShell Scripts (5)

### Verification Scripts

#### verify-strings.ps1
**Purpose:** Check for hard-coded strings in code  
**Usage:** `powershell -ExecutionPolicy Bypass -File .\verify-strings.ps1`  
**Output:** Lists any hard-coded MessageBox.Show strings  
**Expected:** 0 hard-coded strings (100% externalized)  

#### verify-resources.ps1
**Purpose:** Check resource utilization  
**Usage:** `powershell -ExecutionPolicy Bypass -File .\verify-resources.ps1`  
**Output:** Resource usage statistics  
**Expected:** 100% utilization (all resources used)  

### Resource Management Scripts

#### add-all-resources.ps1
**Purpose:** Automatically add resources to Resources.resx  
**Usage:** `powershell -ExecutionPolicy Bypass -File .\add-all-resources.ps1`  
**Action:** Adds predefined resources to .resx file  
**Note:** Backs up .resx before changes  

#### remove-unused-resources.ps1
**Purpose:** Remove unused resources from Resources.resx  
**Usage:** `powershell -ExecutionPolicy Bypass -File .\remove-unused-resources.ps1`  
**Action:** Removes specified unused resources  
**Note:** Backs up .resx before changes  

#### cleanup_resources.ps1
**Purpose:** General resource cleanup utility  
**Usage:** `powershell -ExecutionPolicy Bypass -File .\cleanup_resources.ps1`  
**Action:** Cleanup and optimization tasks  

---

## ?? Icon Generation Tools (4)

### IconGenerator.cs / IconGenerator.exe
**Purpose:** Generate simple icons for the application  
**Usage:** Run IconGenerator.exe or compile .cs file  
**Output:** Icon image files  

### ComprehensiveIconGenerator.cs / ComprehensiveIconGenerator.exe
**Purpose:** Generate comprehensive icon set  
**Usage:** Run ComprehensiveIconGenerator.exe  
**Output:** Complete set of application icons  

---

## ?? Implementation Helpers (1)

### NEW_FEATURES_IMPLEMENTATION.cs
**Purpose:** Template/helper for implementing new features  
**Usage:** Reference for feature implementation patterns  
**Type:** Source code template  

---

## ?? Other Files (1)

### 80
**Purpose:** Temporary file (likely test output)  
**Action:** Can be deleted if not needed  

---

## ?? USAGE GUIDE

### Before Making Changes:

**1. Verify Current State:**
```powershell
cd tools
.\verify-strings.ps1    # Check for hard-coded strings
.\verify-resources.ps1  # Check resource utilization
```

**2. Make Code Changes:**
```
(Edit your code in Visual Studio)
```

**3. Add Resources (if needed):**
```powershell
# Manually edit Resources.resx in Visual Studio
# OR use the script to add multiple resources:
.\add-all-resources.ps1
```

**4. Verify After Changes:**
```powershell
.\verify-strings.ps1    # Should show 0 hard-coded strings
.\verify-resources.ps1  # Should show 100% utilization
```

**5. Remove Unused (if any):**
```powershell
.\remove-unused-resources.ps1
```

---

## ?? CURRENT STATUS

All scripts verified working:
- ? verify-strings.ps1 ? 0 hard-coded strings
- ? verify-resources.ps1 ? 62/62 (100%) utilization
- ? All tools functional

---

## ?? MAINTENANCE

### Regular Checks (Before Each Release):

```powershell
cd tools
.\verify-strings.ps1
.\verify-resources.ps1
```

**Expected Results:**
- ? Strings: 0 hard-coded
- ? Resources: 100% utilized

### If Issues Found:

**Hard-coded strings detected:**
1. Add resource to Resources.resx
2. Update code to use Resources.XXX
3. Re-run verify-strings.ps1

**Unused resources detected:**
1. Review if truly unused
2. Run remove-unused-resources.ps1
3. Re-run verify-resources.ps1

---

**Last Updated:** 2024-04-11  
**Status:** All tools verified working  
**Location:** `D:\Projects\CAD3PLogBrowser\tools\`  
**Files:** 12 (5 scripts + 4 icon tools + 3 other)  
