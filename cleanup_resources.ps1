# Resource Cleanup PowerShell Script
# This script removes unused resources from Resources.resx

$resxFile = "Cad3PLogBrowser\Properties\Resources.resx"

# Resources to KEEP (actively used in code)
$keepResources = @(
    'TITLE',
    'apiview2',
    'copy',
    'filter',
    'find',
    'green_ball',
    'help',
    'open',
    'red_ball',
    'refresh',
    'save',
    'settings',
    'treeview',
    'yellow'
)

# Resources to REMOVE (unused)
$removeResources = @(
    'About', 'ABOUT_DESCRIPTION', 'apiview', 'Bitmap1', 'blue_ball', 
    'cad3plog', 'check1', 'check2', 'Color1', 'cross', 'details',
    'DIALOG_EXPORT_BRANCH_TITLE', 'FILTER_CSV_FILES', 'graph1', 'graph2',
    'Icon1', 'MSG_BRANCH_EXPORTED', 'MSG_BROWSER_LAUNCH_ERROR',
    'MSG_FILE_SAVED', 'MSG_GROK_NOT_CONFIGURED', 'MSG_LOAD_ERROR',
    'MSG_NO_ERRORS', 'MSG_NO_MATCHING_PAIR', 'MSG_NO_WARNINGS',
    'MSG_NOT_API_CALL', 'MSG_NOT_FOUND', 'MSG_SAVE_ERROR', 'Name1',
    'performance', 'remove', 'tabs', 'tools'
)

Write-Host "Resource Cleanup Summary:" -ForegroundColor Cyan
Write-Host "  Keeping: $($keepResources.Count) resources" -ForegroundColor Green
Write-Host "  Removing: $($removeResources.Count) resources" -ForegroundColor Yellow
Write-Host ""

Write-Host "Resources to remove:" -ForegroundColor Yellow
$removeResources | ForEach-Object { Write-Host "  - $_" }

Write-Host ""
Write-Host "To complete cleanup:" -ForegroundColor Cyan
Write-Host "1. Open Resources.resx in Visual Studio" -ForegroundColor White
Write-Host "2. For each resource to remove, right-click and select 'Remove'" -ForegroundColor White
Write-Host "3. Save the file" -ForegroundColor White
Write-Host "4. Rebuild the project" -ForegroundColor White
Write-Host ""
Write-Host "OR use Visual Studio's Resource Editor UI to manage resources" -ForegroundColor Cyan
