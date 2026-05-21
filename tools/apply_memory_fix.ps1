# PowerShell script to apply memory safety fix to MainForm.cs
# This script applies the fix documented in MEMORY_FIX_INSTRUCTIONS.md

$filePath = "Cad3PLogBrowser\MainForm.cs"

Write-Host "Applying memory safety fix to $filePath..." -ForegroundColor Green

# Read the file
$content = Get-Content $filePath -Raw

# Find and replace the PopulateRawView method
$oldMethod = @'
        private void PopulateRawView(IList<string> lines)
        {
            if (rawTextBox == null) return;
            const int MaxRawLines = 50_000;
            bool truncated = lines.Count > MaxRawLines;
            int count = Math.Min(lines.Count, MaxRawLines);

            // P-03: build the text then load into the RTB with WM_SETREDRAW suppressed
            // and use AppendText instead of .Text= so the control parses the RTF once
            // rather than re-parsing the whole document on every incremental append.
            var sb = new System.Text.StringBuilder(count * 80);
            for (int i = 0; i < count; i++) sb.AppendLine(lines[i]);
            if (truncated)
                sb.AppendLine(string.Format("[... {0:N0} more lines not shown — file exceeds raw view limit ...]",
                              lines.Count - MaxRawLines));

            Services.NativeMethods.SuppressRedraw(rawTextBox);
            try
            {
                rawTextBox.Clear();
                rawTextBox.AppendText(sb.ToString());
            }
            finally
            {
                Services.NativeMethods.ResumeRedraw(rawTextBox);
            }
        }
'@

$newMethod = @'
        private void PopulateRawView(IList<string> lines)
        {
            if (rawTextBox == null) return;

            // MEMORY SAFETY FIX: Estimate total file size before loading
            long estimatedChars = 0;
            const long maxSampleLines = 1000;
            int sampleCount = (int)Math.Min(lines.Count, maxSampleLines);
            for (int i = 0; i < sampleCount; i++)
            {
                estimatedChars += (lines[i]?.Length ?? 0) + 2; // +2 for \r\n
            }
            if (lines.Count > sampleCount)
                estimatedChars = (estimatedChars / sampleCount) * lines.Count;

            // If file exceeds size limit, show placeholder
            long maxBytes = _appSettings?.MaxRichTextBoxFileSizeBytes ?? (50 * 1024 * 1024);
            if (estimatedChars > maxBytes)
            {
                Services.NativeMethods.SuppressRedraw(rawTextBox);
                try
                {
                    rawTextBox.Clear();
                    long sizeMB = estimatedChars / (1024 * 1024);
                    long limitMB = maxBytes / (1024 * 1024);
                    rawTextBox.Text = string.Format(
                        "File too large for Raw view ({0} MB > {1} MB limit).\r\n\r\n" +
                        "Use the Log tab for better performance with large files.\r\n\r\n" +
                        "Adjust MaxRichTextBoxFileSizeBytes in Settings if needed.",
                        sizeMB, limitMB);
                }
                finally { Services.NativeMethods.ResumeRedraw(rawTextBox); }
                return;
            }

            const int MaxRawLines = 50_000;
            bool truncated = lines.Count > MaxRawLines;
            int count = Math.Min(lines.Count, MaxRawLines);

            // P-03: build the text then load into the RTB with WM_SETREDRAW suppressed
            // and use AppendText instead of .Text= so the control parses the RTF once
            // rather than re-parsing the whole document on every incremental append.
            var sb = new System.Text.StringBuilder(count * 80);
            int charCount = 0;
            int maxChars = _appSettings?.MaxRichTextBoxChars ?? 10_000_000;

            for (int i = 0; i < count; i++)
            {
                int lineLen = (lines[i]?.Length ?? 0) + Environment.NewLine.Length;
                // MEMORY SAFETY: Check character limit
                if (charCount + lineLen > maxChars)
                {
                    sb.AppendLine(string.Format("[... {0:N0} more lines not shown — exceeded character limit ...]",
                              lines.Count - i));
                    truncated = true;
                    break;
                }
                sb.AppendLine(lines[i]);
                charCount += lineLen;
            }

            if (truncated && !sb.ToString().Contains("exceeded character limit"))
                sb.AppendLine(string.Format("[... {0:N0} more lines not shown — file exceeds raw view limit ...]",
                              lines.Count - MaxRawLines));

            Services.NativeMethods.SuppressRedraw(rawTextBox);
            try
            {
                rawTextBox.Clear();
                // MEMORY SAFETY: Wrap in try-catch for OOM
                try
                {
                    rawTextBox.AppendText(sb.ToString());
                }
                catch (OutOfMemoryException)
                {
                    rawTextBox.Clear();
                    rawTextBox.Text = "ERROR: Out of memory. File is too large for Raw view.\r\n\r\n" +
                                    "Use the Log tab instead.";
                }
            }
            finally
            {
                Services.NativeMethods.ResumeRedraw(rawTextBox);
            }
        }
'@

# Apply the replacement
$newContent = $content -replace [regex]::Escape($oldMethod), $newMethod

if ($newContent -eq $content)
{
    Write-Host "ERROR: Could not find the method to replace!" -ForegroundColor Red
    Write-Host "The method signature or content may have changed." -ForegroundColor Red
    exit 1
}

# Write the updated content back
Set-Content -Path $filePath -Value $newContent -NoNewline

Write-Host "Successfully applied memory safety fix!" -ForegroundColor Green
Write-Host ""
Write-Host "Changes made:" -ForegroundColor Cyan
Write-Host "  - Added file size estimation before loading into RichTextBox" -ForegroundColor White
Write-Host "  - Added character count limit checking during StringBuilder creation" -ForegroundColor White
Write-Host "  - Added OutOfMemoryException handling as last resort" -ForegroundColor White
Write-Host ""
Write-Host "Please rebuild the project to verify the fix compiles correctly." -ForegroundColor Yellow
