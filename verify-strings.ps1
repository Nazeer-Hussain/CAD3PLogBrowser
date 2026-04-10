# Verify Hard-coded Strings
# This script finds remaining hard-coded strings in the codebase

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Hard-coded String Verification Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$files = Get-ChildItem -Path "Cad3PLogBrowser" -Filter "*.cs" `
    -Exclude "*Designer.cs","*AssemblyInfo.cs","Resources.Designer.cs" -Recurse

Write-Host "Scanning $($files.Count) files..." -ForegroundColor White
Write-Host ""

$hardcoded = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw

    # Pattern 1: MessageBox.Show with literal strings
    $matches1 = [regex]::Matches($content, 'MessageBox\.Show\s*\(\s*"[^"]*"')

    # Pattern 2: MessageBox.Show with string interpolation
    $matches2 = [regex]::Matches($content, 'MessageBox\.Show\s*\(\s*\$"[^"]*"')

    # Pattern 3: Status bar assignments with literal strings
    $matches3 = [regex]::Matches($content, 'Status\w+\.Text\s*=\s*"[^"]*"')

    foreach ($match in ($matches1 + $matches2 + $matches3)) {
        $lineNum = ($content.Substring(0, $match.Index) -split "`n").Count
        $code = $match.Value
        if ($code.Length > 80) {
            $code = $code.Substring(0, 77) + "..."
        }

        $hardcoded += [PSCustomObject]@{
            File = $file.Name
            Line = $lineNum
            Code = $code
        }
    }
}

if ($hardcoded.Count -gt 0) {
    Write-Host "WARNING: Found $($hardcoded.Count) hard-coded strings:" -ForegroundColor Yellow
    Write-Host ""
    $hardcoded | Format-Table -AutoSize -Wrap
    Write-Host ""
    Write-Host "ACTION REQUIRED:" -ForegroundColor Yellow
    Write-Host "  1. Review each hard-coded string above" -ForegroundColor White
    Write-Host "  2. Add appropriate resource to Resources.resx" -ForegroundColor White
    Write-Host "  3. Replace hard-coded string with Resources.XXX" -ForegroundColor White
    Write-Host ""
    exit 1
} else {
    Write-Host "? SUCCESS: No hard-coded strings found!" -ForegroundColor Green
    Write-Host ""
    Write-Host "All strings have been externalized to Resources.resx" -ForegroundColor Green
    Write-Host ""
    exit 0
}
