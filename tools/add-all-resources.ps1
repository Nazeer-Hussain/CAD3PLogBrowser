# Add All 24 Resources to Resources.resx Automatically
# This script adds all string resources programmatically

$resxPath = "Cad3PLogBrowser\Properties\Resources.resx"

# Backup the file first
Copy-Item $resxPath "$resxPath.backup" -Force
Write-Host "Backup created: $resxPath.backup" -ForegroundColor Green

# Load the resx file as XML
[xml]$resx = Get-Content $resxPath

# Resources to add
$resourcesToAdd = @(
    @{ Name = "ERR_EXPORT_FILE_FAILED"; Value = "Failed to export file:\n{0}" }
    @{ Name = "ERR_NOT_FOUND"; Value = "'{0}' not found." }
    @{ Name = "ERR_INVALID_REGEX"; Value = "Invalid regular expression:\n{0}" }
    @{ Name = "ERR_OPEN_UPDATES_FAILED"; Value = "Could not open updates page:\n{0}" }
    @{ Name = "ERR_OPEN_ISSUES_FAILED"; Value = "Could not open issues page:\n{0}" }
    @{ Name = "ERR_OPEN_HELP_FAILED"; Value = "Could not open help file:\n{0}" }
    @{ Name = "ERR_BROWSER_FAILED"; Value = "Failed to open browser:\n{0}" }
    @{ Name = "ERR_EXPORT_PERFORMANCE_FAILED"; Value = "Could not export performance data:\n{0}" }
    @{ Name = "ERR_LINE_NUMBER_OUT_OF_RANGE"; Value = "Line number must be between 1 and {0}." }
    @{ Name = "ERR_NO_ENTER_EXIT_PAIR"; Value = "Could not find ENTER/EXIT pair for '{0}'." }
    @{ Name = "ERR_SAVE_BRANCH_FAILED"; Value = "Could not save branch:\n{0}" }
    @{ Name = "ERR_NO_MATCHES_FOUND"; Value = "No matches found for '{0}'." }
    @{ Name = "ERR_EXPORT_CALL_GRAPH_FAILED"; Value = "Could not export Call Graph:\n{0}" }
    @{ Name = "ERR_COPY_FAILED"; Value = "Failed to copy to clipboard:\n{0}" }
    @{ Name = "ERR_EXPORT_TREE_FAILED"; Value = "Failed to export tree:\n{0}" }
    @{ Name = "ERR_EXPORT_TIMELINE_FAILED"; Value = "Failed to export timeline:\n{0}" }
    @{ Name = "ERR_EXPORT_FLAME_GRAPH_FAILED"; Value = "Failed to export flame graph:\n{0}" }
    @{ Name = "ERR_FONT_CHANGE_FAILED"; Value = "Failed to change font:\n{0}" }
    @{ Name = "MSG_BRANCH_EXPORTED_TO"; Value = "Branch exported to:\n{0}" }
    @{ Name = "MSG_PERFORMANCE_EXPORTED_TO"; Value = "Performance data exported to:\n{0}" }
    @{ Name = "MSG_BRANCH_SAVED_TO"; Value = "{0} line(s) saved to:\n{1}" }
    @{ Name = "MSG_CALL_GRAPH_EXPORTED_TO"; Value = "Call Graph exported to:\n{0}" }
    @{ Name = "DIALOG_TITLE_BOOKMARKS"; Value = "Bookmarks" }
    @{ Name = "MSG_HELP_FILE_NOT_FOUND"; Value = "Help file (Cad3PLogBrowser.chm) not found.\n\nPress Ctrl+K to view keyboard shortcuts,\nor visit the GitHub repository for documentation." }
)

Write-Host "`nAdding $($resourcesToAdd.Count) resources..." -ForegroundColor Cyan

$added = 0
foreach ($resource in $resourcesToAdd) {
    # Check if resource already exists
    $existing = $resx.root.data | Where-Object { $_.name -eq $resource.Name }

    if ($existing) {
        Write-Host "  ? $($resource.Name) already exists - skipping" -ForegroundColor Yellow
    } else {
        # Create new data element
        $dataNode = $resx.CreateElement("data")
        $dataNode.SetAttribute("name", $resource.Name)
        $dataNode.SetAttribute("xml:space", "preserve")

        $valueNode = $resx.CreateElement("value")
        $valueNode.InnerText = $resource.Value

        $dataNode.AppendChild($valueNode) | Out-Null
        $resx.root.AppendChild($dataNode) | Out-Null

        Write-Host "  ? Added: $($resource.Name)" -ForegroundColor Green
        $added++
    }
}

# Save the modified resx file
$resx.Save($resxPath)

Write-Host "`nSUCCESS: Added $added new resources to Resources.resx" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Build solution in Visual Studio (Ctrl+Shift+B)" -ForegroundColor White
Write-Host "2. Verify Resources.Designer.cs was regenerated" -ForegroundColor White
Write-Host "3. Let Copilot know to update the code" -ForegroundColor White
