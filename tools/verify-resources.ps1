# Verify Resource Usage
# This script checks which resources are actually used in the codebase

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Resource Usage Verification Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if Resources.Designer.cs exists (support running from tools/ or root)
$designerPath = if (Test-Path "Cad3PLogBrowser\Properties\Resources.Designer.cs") {
    "Cad3PLogBrowser\Properties\Resources.Designer.cs"
} elseif (Test-Path "..\Cad3PLogBrowser\Properties\Resources.Designer.cs") {
    "..\Cad3PLogBrowser\Properties\Resources.Designer.cs"
} else {
    $null
}

if (-not $designerPath) {
    Write-Host "ERROR: Resources.Designer.cs not found!" -ForegroundColor Red
    Write-Host "Please run this script from project root or tools folder." -ForegroundColor Yellow
    exit 1
}

Write-Host "Analyzing resource usage..." -ForegroundColor White
Write-Host ""

# Get all resource names from Resources.Designer.cs
$designerContent = Get-Content $designerPath -Raw

# Extract string resources
$stringResources = [regex]::Matches($designerContent, 'internal static string (\w+)') | 
    ForEach-Object { $_.Groups[1].Value }

# Extract image/bitmap resources
$imageResources = [regex]::Matches($designerContent, 'internal static System\.Drawing\.(?:Bitmap|Icon) (\w+)') | 
    ForEach-Object { $_.Groups[1].Value }

$allResources = $stringResources + $imageResources | Sort-Object -Unique

Write-Host "Found $($allResources.Count) resources:" -ForegroundColor Cyan
Write-Host "  - String resources: $($stringResources.Count)" -ForegroundColor White
Write-Host "  - Image resources: $($imageResources.Count)" -ForegroundColor White
Write-Host ""

$used = @()
$unused = @()

Write-Host "Checking usage in code files..." -ForegroundColor White

foreach ($name in $allResources) {
    # Search for usage (excluding Resources.Designer.cs itself)
    # Support running from tools/ or root folder
    $searchPaths = if (Test-Path "Cad3PLogBrowser") {
        @("Cad3PLogBrowser\*.cs","Cad3PLogBrowser\**\*.cs")
    } else {
        @("..\Cad3PLogBrowser\*.cs","..\Cad3PLogBrowser\**\*.cs")
    }

    $usage = Select-String -Path $searchPaths `
        -Pattern "Resources\.$name" -Exclude "Resources.Designer.cs" -ErrorAction SilentlyContinue

    if ($usage) {
        $used += [PSCustomObject]@{
            Name = $name
            UsageCount = $usage.Count
        }
    } else {
        $unused += $name
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total resources defined: $($allResources.Count)" -ForegroundColor White
Write-Host "Resources used: $($used.Count) ($([math]::Round(($used.Count / $allResources.Count) * 100, 1))%)" -ForegroundColor Green
Write-Host "Resources unused: $($unused.Count) ($([math]::Round(($unused.Count / $allResources.Count) * 100, 1))%)" -ForegroundColor $(if ($unused.Count -gt 0) { 'Yellow' } else { 'Green' })
Write-Host ""

if ($unused.Count -gt 0) {
    Write-Host "============================================" -ForegroundColor Yellow
    Write-Host "  UNUSED RESOURCES (SHOULD BE REMOVED)" -ForegroundColor Yellow
    Write-Host "============================================" -ForegroundColor Yellow
    Write-Host ""
    $unused | ForEach-Object { Write-Host "  ? $_" -ForegroundColor Yellow }
    Write-Host ""
    Write-Host "ACTION REQUIRED:" -ForegroundColor Yellow
    Write-Host "  1. Open Resources.resx in Visual Studio" -ForegroundColor White
    Write-Host "  2. For each unused resource listed above:" -ForegroundColor White
    Write-Host "     - Find it in the list" -ForegroundColor White
    Write-Host "     - Right-click ? Remove" -ForegroundColor White
    Write-Host "  3. Save the file" -ForegroundColor White
    Write-Host "  4. Rebuild the project" -ForegroundColor White
    Write-Host ""
    exit 1
} else {
    Write-Host "? EXCELLENT: All resources are being used!" -ForegroundColor Green
    Write-Host ""
    Write-Host "No cleanup needed - resource usage is 100%" -ForegroundColor Green
    Write-Host ""
}

# Show top 10 most used resources
if ($used.Count -gt 0) {
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  TOP 10 MOST USED RESOURCES" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
    $used | Sort-Object UsageCount -Descending | Select-Object -First 10 | 
        ForEach-Object { Write-Host "  $($_.Name): $($_.UsageCount) uses" -ForegroundColor Gray }
    Write-Host ""
}

exit 0
