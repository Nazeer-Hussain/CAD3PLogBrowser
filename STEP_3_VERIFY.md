# STEP 3: Verify and Test

## Run Verification Scripts

### 1. Verify No Hard-coded Strings Remain

```powershell
powershell -ExecutionPolicy Bypass -File .\verify-strings.ps1
```

**Expected Output:**
```
? SUCCESS: No hard-coded strings found!
```

**If it finds any:** Go back to STEP_2 and externalize them

### 2. Verify Resource Usage

```powershell
powershell -ExecutionPolicy Bypass -File .\verify-resources.ps1
```

**Expected Output:**
```
Total resources used: XX (100%)
Unused: 0 (0%)
? All resources are being used!
```

---

## Build and Test

### 1. Clean Build
```
Ctrl+Shift+B (Build Solution)
```

**Expected:** 
- Build succeeded
- 0 Errors
- 0 Warnings

**If build fails:** Check the error message and fix the resource reference

### 2. Run Application
Press F5 to run

### 3. Test All Messages

**File Operations:**
- [ ] Try to save when no file loaded ? Check "No file loaded" message
- [ ] Cancel a save operation ? Check "Save operation cancelled" message
- [ ] Try to export with no data ? Check "No log data to export" message

**Navigation:**
- [ ] Click on non-API line and jump ? Check "Selected line is not an API call line"
- [ ] Try to find matching when none exists ? Check "No matching pair found"
- [ ] Navigate errors when none exist ? Check "No errors found"
- [ ] Navigate warnings when none exist ? Check "No warnings found"

**Bookmarks:**
- [ ] Show bookmarks when empty ? Check "No bookmarks set" message
- [ ] Jump to line with invalid number ? Check "Invalid line number"

**Export:**
- [ ] Try to export performance with no data ? Check error message
- [ ] Try to export call tree with no data ? Check error message
- [ ] Try to export timeline with no data ? Check error message
- [ ] Try to export flame graph with no data ? Check error message
- [ ] Successfully export something ? Check success message

**Help:**
- [ ] Try to open help file (if missing) ? Check help file error
- [ ] Try Grok when not configured ? Check Grok configuration message

---

## Final Checks

### Visual Verification
- [ ] All messages display correctly
- [ ] No "MissingResourceException" errors
- [ ] No empty MessageBox dialogs
- [ ] All dialog titles correct

### Code Verification
- [ ] No hard-coded strings in MessageBox.Show
- [ ] All Resources.XXX references exist
- [ ] All string.Format calls have correct number of parameters

---

## If All Tests Pass

### Commit Changes

```powershell
git add .
git commit -m "feat: externalize hard-coded strings to Resources.resx

- Added 26 string resources for error/success messages
- Updated MainForm.cs to use Resources.XXX instead of literals
- All MessageBox.Show calls now use externalized strings
- Application is now localization-ready

BREAKING CHANGES: None
BUILD: Clean
TESTS: All messages verified"
```

---

## If Tests Fail

### Common Issues and Fixes

**Issue:** Build error "Resources.XXX does not exist"  
**Fix:** 
1. Check Resources.resx for typos in resource name
2. Rebuild solution (Ctrl+Shift+B)
3. Check that Resources.Designer.cs was regenerated

**Issue:** MessageBox shows "{0}" literally  
**Fix:** 
1. Make sure you used `string.Format(Resources.XXX, parameter)`
2. Check that the resource string contains {0} placeholder

**Issue:** Wrong message displays  
**Fix:**
1. Check that you used the correct Resources.XXX constant
2. Verify the value in Resources.resx

**Issue:** Application crashes on MessageBox  
**Fix:**
1. Check for null reference
2. Ensure all Resources.XXX exist in Resources.Designer.cs
3. Rebuild solution

---

After verification and successful commit, proceed to STEP_4_REMOVE_UNUSED.md
