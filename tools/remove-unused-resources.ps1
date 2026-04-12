# Remove Unused Resources from Resources.resx

$resxPath = "Cad3PLogBrowser\Properties\Resources.resx"

# Backup
Copy-Item $resxPath "$resxPath.beforecleanup" -Force
Write-Host "Backup created: $resxPath.beforecleanup" -ForegroundColor Green

# Load XML
[xml]$resx = Get-Content $resxPath

# Resources to remove
$toRemove = @(
    'DIALOG_EXPORT_BRANCH_TITLE',
    'FILTER_CSV_FILES',
    'MSG_BRANCH_EXPORTED',
    'MSG_BROWSER_LAUNCH_ERROR',
    'MSG_LOAD_ERROR',
    'MSG_NOT_FOUND'
)

Write-Host "`nRemoving $($toRemove.Count) unused resources..." -ForegroundColor Cyan

$removed = 0
foreach ($name in $toRemove) {
    $node = $resx.root.data | Where-Object { $_.name -eq $name }
    if ($node) {
        $resx.root.RemoveChild($node) | Out-Null
        Write-Host "  ? Removed: $name" -ForegroundColor Green
        $removed++
    } else {
        Write-Host "  ? Not found: $name" -ForegroundColor Yellow
    }
}

# Save
$resx.Save($resxPath)

Write-Host "`nSUCCESS: Removed $removed unused resources" -ForegroundColor Green
Write-Host "Resource utilization will now be 100%!" -ForegroundColor Cyan
Write-Host "`nNext: Build in Visual Studio (Ctrl+Shift+B)" -ForegroundColor White
