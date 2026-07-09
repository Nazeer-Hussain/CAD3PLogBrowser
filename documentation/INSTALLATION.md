# AI Framework - Installation & Setup Instructions

## Prerequisites

The AI framework has been successfully created, but requires the following NuGet packages to be installed:

### Required NuGet Packages

#### 1. Newtonsoft.Json (JSON.NET)
**Status**: ? Missing (Required)  
**Version**: 12.0.3 or later  
**Purpose**: JSON serialization/deserialization for API communication and settings storage

**Install via NuGet Package Manager:**
```powershell
Install-Package Newtonsoft.Json
```

**Or via .NET CLI:**
```bash
dotnet add package Newtonsoft.Json
```

**Or via Visual Studio:**
1. Right-click project ? Manage NuGet Packages
2. Click "Browse"
3. Search for "Newtonsoft.Json"
4. Click "Install"

#### 2. System.Security.Cryptography.ProtectedData
**Status**: ? Missing (Required for .NET Framework 4.8)  
**Version**: 4.7.0 or later  
**Purpose**: Secure credential storage using Windows DPAPI

**Install via NuGet Package Manager:**
```powershell
Install-Package System.Security.Cryptography.ProtectedData
```

**Or via Visual Studio:**
1. Right-click project ? Manage NuGet Packages
2. Click "Browse"
3. Search for "System.Security.Cryptography.ProtectedData"
4. Click "Install"

---

## Quick Installation Steps

### Option 1: Via Visual Studio (Recommended)

1. Open CAD3PLogBrowser solution in Visual Studio
2. Right-click on the "Cad3PLogBrowser" project
3. Select "Manage NuGet Packages..."
4. Click the "Browse" tab
5. Search for and install:
   - `Newtonsoft.Json`
   - `System.Security.Cryptography.ProtectedData`
6. Build the solution (Ctrl+Shift+B)

### Option 2: Via Package Manager Console

1. Open Visual Studio
2. Go to: Tools ? NuGet Package Manager ? Package Manager Console
3. Run these commands:

```powershell
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package System.Security.Cryptography.ProtectedData -Version 7.0.0
```

4. Build the solution

### Option 3: Modify .csproj File

Add these lines to your `Cad3PLogBrowser.csproj` file inside the `<ItemGroup>` section:

```xml
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="7.0.0" />
</ItemGroup>
```

Then restore packages:
```bash
dotnet restore
```

---

## Post-Installation Verification

### Step 1: Build the Project

After installing packages, build the project:

```bash
dotnet build
```

Or in Visual Studio: `Build ? Build Solution` (Ctrl+Shift+B)

### Step 2: Verify No Errors

All AI framework files should now compile without errors.

### Step 3: Run Initial Test

Add this test code to verify the framework is working:

```csharp
private async void TestAIFramework()
{
    try
    {
        // Test with Mock provider (no API key needed)
        var settings = new AISettings
        {
            EnableAI = true,
            SelectedProvider = AIProviderType.Mock
        };

        var aiService = new AIService(settings);

        // Test connection
        var (success, message) = await aiService.TestConnectionAsync();

        if (success)
        {
            MessageBox.Show("? AI Framework installed successfully!", "Success");
        }
        else
        {
            MessageBox.Show($"? Test failed: {message}", "Error");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"? Exception: {ex.Message}\n\n{ex.StackTrace}", "Error");
    }
}
```

---

## Common Issues & Solutions

### Issue 1: "Newtonsoft.Json not found"

**Cause**: NuGet package not installed

**Solution**:
```powershell
Install-Package Newtonsoft.Json
```

### Issue 2: "ProtectedData does not exist"

**Cause**: Missing System.Security.Cryptography.ProtectedData package

**Solution**:
```powershell
Install-Package System.Security.Cryptography.ProtectedData
```

### Issue 3: Build still fails after installing packages

**Cause**: Solution needs to be cleaned and rebuilt

**Solution**:
1. Close Visual Studio
2. Delete `bin` and `obj` folders
3. Reopen solution
4. Rebuild: `Build ? Rebuild Solution`

### Issue 4: Package restore failed

**Cause**: NuGet cache issue

**Solution**:
```powershell
dotnet nuget locals all --clear
dotnet restore
```

---

## Package Versions Compatibility

### Recommended Versions

| Package | Minimum Version | Recommended Version | Latest Tested |
|---------|----------------|---------------------|---------------|
| Newtonsoft.Json | 11.0.2 | 13.0.3 | 13.0.3 |
| System.Security.Cryptography.ProtectedData | 4.7.0 | 7.0.0 | 7.0.0 |

### Framework Requirements

- **.NET Framework**: 4.8 (already required by project)
- **C# Language Version**: 7.3 (already configured)
- **Platform**: Windows (for Credential Manager support)

---

## Alternative: Manual File Additions (If NuGet Fails)

If NuGet packages cannot be installed for some reason, you can:

1. **Newtonsoft.Json**: Download DLL from [NuGet.org](https://www.nuget.org/packages/Newtonsoft.Json/)
2. **ProtectedData**: This is part of .NET Framework and should be available

Add manual references:
1. Right-click "References" in project
2. Click "Add Reference..."
3. Click "Browse..."
4. Navigate to downloaded DLLs
5. Add them to the project

**Note**: Using NuGet is strongly recommended over manual references.

---

## Verification Checklist

After installation, verify:

- [ ] Solution builds without errors
- [ ] No missing references
- [ ] Mock provider test works
- [ ] Settings can be saved and loaded
- [ ] Credential manager works (test storing/retrieving a dummy key)

**Test Script:**

```csharp
// Test 1: Settings persistence
var settings = AISettings.CreateDefault();
settings.EnableAI = true;
settings.SelectedProvider = AIProviderType.Mock;
bool saved = AISettingsService.Save(settings);
Console.WriteLine($"Settings saved: {saved}");

var loaded = AISettingsService.Load();
Console.WriteLine($"Settings loaded: {loaded.EnableAI}");

// Test 2: Credential storage
bool stored = CredentialManager.StoreCredential("test_key", "test_value");
Console.WriteLine($"Credential stored: {stored}");

string retrieved = CredentialManager.RetrieveCredential("test_key");
Console.WriteLine($"Credential retrieved: {retrieved == "test_value"}");

CredentialManager.DeleteCredential("test_key");

// Test 3: AI Service initialization
var aiService = new AIService(loaded);
Console.WriteLine($"AI Service enabled: {aiService.IsEnabled}");

// Test 4: Mock provider test
var result = await aiService.AnalyzeAsync(
    AnalysisType.Summarize,
    new List<IContextProvider>());
Console.WriteLine($"Mock analysis success: {result.Success}");
```

---

## Next Steps After Installation

Once packages are installed and the solution builds:

1. ? **Review Documentation**
   - Read `AI/README.md`
   - Read `AI/QUICKSTART.md`
   - Read `AI/ARCHITECTURE.md`

2. ? **Test with Mock Provider**
   - No API key required
   - Verifies framework is working
   - See QUICKSTART.md for examples

3. ? **Configure Real Provider**
   - Get Anthropic API key (recommended)
   - Or plan for Azure OpenAI (enterprise)
   - See QUICKSTART.md for configuration

4. ? **Integrate into UI**
   - Create AI Settings dialog
   - Update AiAssistantPanel
   - Add menu items/toolbar buttons

5. ? **Test End-to-End**
   - Load a log file
   - Run AI analysis
   - Verify results

---

## Support

If you encounter issues during installation:

1. Check this document for solutions
2. Review build output for specific errors
3. Ensure .NET Framework 4.8 SDK is installed
4. Verify NuGet package manager is working
5. Try cleaning and rebuilding the solution

---

## Summary

**Required Actions:**

1. Install `Newtonsoft.Json` NuGet package
2. Install `System.Security.Cryptography.ProtectedData` NuGet package
3. Build the solution
4. Run verification tests
5. Proceed with integration

**Estimated Time**: 5-10 minutes

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Author**: CAD3PLogBrowser AI Framework
