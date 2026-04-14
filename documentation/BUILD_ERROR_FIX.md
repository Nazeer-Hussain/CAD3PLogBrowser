# Build Error Fix - Cad3PLogBrowser.csproj

## Problem
The file `Cad3PLogBrowser\Cad3PLogBrowser.csproj` has a syntax error on line 61:

```xml
<Reference Include="System" />`r`n    <Reference Include="System.Net.Http" />
```

The backticks (`` `r`n ``) are literal text instead of a newline.

## Solution

**Please close Visual Studio and apply this fix**:

### Option 1: Manual Edit (Recommended)

1. Close Visual Studio
2. Open `Cad3PLogBrowser\Cad3PLogBrowser.csproj` in a text editor (Notepad, VS Code)
3. Find line 61 (around the `<ItemGroup>` with references)
4. Replace this line:
   ```xml
   <Reference Include="System" />`r`n    <Reference Include="System.Net.Http" />
   ```

5. With these TWO separate lines:
   ```xml
   <Reference Include="System" />
   <Reference Include="System.Net.Http" />
   ```

6. Save the file
7. Reopen Visual Studio
8. Rebuild solution

### Option 2: Use PowerShell (after closing VS)

1. Close Visual Studio
2. Run this PowerShell command:

```powershell
$file = "Cad3PLogBrowser\Cad3PLogBrowser.csproj"
$content = Get-Content $file -Raw
$content = $content -replace '<Reference Include="System" />`r`n    <Reference Include="System.Net.Http" />', @'
    <Reference Include="System" />
    <Reference Include="System.Net.Http" />
'@
Set-Content $file -Value $content -NoNewline
```

3. Reopen Visual Studio
4. Rebuild solution

### Option 3: Git Restore and Manual Add

1. Close Visual Studio
2. Run:
   ```powershell
   git checkout Cad3PLogBrowser\Cad3PLogBrowser.csproj
   ```
3. Open `Cad3PLogBrowser.csproj` in text editor
4. Find the line with `<Reference Include="System" />`
5. Add a new line after it:
   ```xml
   <Reference Include="System.Net.Http" />
   ```
6. Save
7. Reopen Visual Studio

## Why This Happened

My PowerShell command used backticks which were interpreted as literal text instead of escape sequences, causing invalid XML in the project file.

## Current Status

? AiLogService.cs - Fixed (using WebClient instead of HttpClient)
? Cad3PLogBrowser.csproj - Needs manual fix (Visual Studio has file locked)

After fixing the csproj file, the build will succeed!
