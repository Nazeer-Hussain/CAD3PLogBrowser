# PowerShell script to add DependencyGraphPanel and AiAssistantPanel to project
# Run this script to automatically add the panel files to the project

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Add Dependency Graph & AI Panels" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectFile = "Cad3PLogBrowser\Cad3PLogBrowser.csproj"

# Check if project file exists
if (!(Test-Path $projectFile)) {
    Write-Host "ERROR: Project file not found: $projectFile" -ForegroundColor Red
    Write-Host "Make sure you're running this from the solution root directory." -ForegroundColor Yellow
    exit 1
}

# Read project file
Write-Host "Reading project file..." -ForegroundColor Yellow
$content = Get-Content $projectFile -Raw

# Check if already added
if ($content -match "DependencyGraphPanel.cs") {
    Write-Host "DependencyGraphPanel.cs is already in the project!" -ForegroundColor Green
    $depGraphExists = $true
} else {
    Write-Host "DependencyGraphPanel.cs not found in project. Will add..." -ForegroundColor Yellow
    $depGraphExists = $false
}

if ($content -match "AiAssistantPanel.cs") {
    Write-Host "AiAssistantPanel.cs is already in the project!" -ForegroundColor Green
    $aiPanelExists = $true
} else {
    Write-Host "AiAssistantPanel.cs not found in project. Will add..." -ForegroundColor Yellow
    $aiPanelExists = $false
}

if ($depGraphExists -and $aiPanelExists) {
    Write-Host ""
    Write-Host "Both files are already in the project. Nothing to do!" -ForegroundColor Green
    Write-Host "You can now uncomment the code in MainForm.cs." -ForegroundColor Cyan
    exit 0
}

# Find the insertion point (after FlameGraphPanel)
$pattern = '(<Compile Include="Managers\\FlameGraphPanel\.cs">[\s\S]*?</Compile>)'

if (!($content -match $pattern)) {
    Write-Host "ERROR: Could not find FlameGraphPanel entry in project file." -ForegroundColor Red
    Write-Host "Manual edit required." -ForegroundColor Yellow
    exit 1
}

# Create the new entries
$newEntries = ""

if (!$depGraphExists) {
    $newEntries += @"

    <Compile Include="Managers\DependencyGraphPanel.cs">
      <SubType>Component</SubType>
    </Compile>
"@
}

if (!$aiPanelExists) {
    # Find TimelinePanel for AI panel insertion
    if ($content -match '(<Compile Include="Managers\\TimelinePanel\.cs">[\s\S]*?</Compile>)') {
        $aiInsertAfter = $matches[1]
        $newEntriesAi = @"

    <Compile Include="Managers\AiAssistantPanel.cs">
      <SubType>Component</SubType>
    </Compile>
"@
    }
}

# Create backup
$backupFile = $projectFile + ".backup"
Write-Host ""
Write-Host "Creating backup: $backupFile" -ForegroundColor Yellow
Copy-Item $projectFile $backupFile -Force

# Apply modifications
try {
    if (!$depGraphExists) {
        $content = $content -replace $pattern, "`$1$newEntries"
        Write-Host "? Added DependencyGraphPanel.cs to project" -ForegroundColor Green
    }

    if (!$aiPanelExists -and $aiInsertAfter) {
        $content = $content -replace [regex]::Escape($aiInsertAfter), "$aiInsertAfter$newEntriesAi"
        Write-Host "? Added AiAssistantPanel.cs to project" -ForegroundColor Green
    }

    # Write modified content
    Set-Content -Path $projectFile -Value $content -NoNewline

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host " SUCCESS! Files added to project" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Open solution in Visual Studio" -ForegroundColor White
    Write-Host "2. Go to MainForm.cs" -ForegroundColor White
    Write-Host "3. Uncomment the field declarations (lines ~48-51)" -ForegroundColor White
    Write-Host "4. Uncomment InitDependencyGraphPanel() call in constructor" -ForegroundColor White
    Write-Host "5. Uncomment InitDependencyGraphPanel() method body" -ForegroundColor White
    Write-Host "6. Uncomment ShowDependencyGraphMenuItem_CheckedChanged() method" -ForegroundColor White
    Write-Host "7. Uncomment dependency graph loading in PopulateTrees()" -ForegroundColor White
    Write-Host "8. Build and run!" -ForegroundColor White
    Write-Host ""
    Write-Host "Backup saved as: $backupFile" -ForegroundColor Yellow
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to modify project file" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Restoring from backup..." -ForegroundColor Yellow
    Copy-Item $backupFile $projectFile -Force
    Write-Host "Project file restored. Please edit manually." -ForegroundColor Yellow
    exit 1
}
