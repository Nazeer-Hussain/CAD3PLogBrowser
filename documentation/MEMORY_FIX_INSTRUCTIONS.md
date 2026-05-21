# Memory Safety Fix for OutOfMemoryException in CAD3PLogBrowser

## Problem
The application crashes with `OutOfMemoryException` when loading large log files into RichTextBox controls.

## Root Cause Analysis
The stack trace shows:
```
System.OutOfMemoryException: Insufficient memory to continue the execution of the program.
   at System.Runtime.InteropServices.Marshal.AllocCoTaskMem(Int32 cb)
   at System.Windows.Forms.UnsafeNativeMethods.GetWindowText(HandleRef hWnd, StringBuilder lpString, Int32 nMaxCount)
   at System.Windows.Forms.Control.get_WindowText()
   at System.Windows.Forms.TextBoxBase.get_WindowText()
```

This occurs when:
1. Large log files (>50MB) are loaded into `rawTextBox` RichTextBox control
2. RichTextBox has theoretical 2GB limit but becomes unstable >10-50MB
3. No size checking before loading content

## Files Already Modified

### 1. Cad3PLogBrowser\Models\AppSettings.cs ?
Added memory safety configuration:
```csharp
/// <summary>
/// Maximum number of characters to load into RichTextBox controls.
/// Default: 10 million characters (~10MB of text).
/// </summary>
public int MaxRichTextBoxChars { get; set; } = 10_000_000;

/// <summary>
/// Maximum file size (in bytes) that will be loaded into RichTextBox controls.
/// Files larger than this will show a placeholder message instead.
/// Default: 50 MB.
/// </summary>
public long MaxRichTextBoxFileSizeBytes { get; set; } = 50 * 1024 * 1024;
```

### 2. Cad3PLogBrowser\MainForm.Designer.cs ?
Added MaxLength property to rawTextBox:
```csharp
this.rawTextBox.MaxLength = 10_000_000; // Memory safety: prevent OutOfMemoryException
```

## Remaining Changes Needed

### 3. Cad3PLogBrowser\MainForm.cs - PopulateRawView Method

**Location:** Line 2638

**Current Code:**
```csharp
private void PopulateRawView(IList<string> lines)
{
    if (rawTextBox == null) return;
    const int MaxRawLines = 50_000;
    bool truncated = lines.Count > MaxRawLines;
    int count = Math.Min(lines.Count, MaxRawLines);

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
```

**Required Fix:**
Add size checking and OutOfMemoryException handling:

```csharp
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
```

## Testing Instructions

1. Create or obtain a large log file (>100MB)
2. Open it in the application
3. Navigate to the Raw tab
4. Verify:
   - If file > 50MB: Placeholder message is shown instead of crash
   - If file < 50MB but > character limit: Truncation message is shown
   - No OutOfMemoryException occurs

## Additional Recommendations

1. Consider adding the same protection to other Rich TextBox controls:
   - `_apiDetailsBox` (probably safe - shows small summaries)
   - Any other text display controls

2. Add settings UI to allow users to adjust:
   - `MaxRichTextBoxChars`
   - `MaxRichTextBoxFileSizeBytes`

3. Consider using a different control for very large files:
   - ScintillaNET or other high-performance text editor control
   - Virtual text viewer with line-by-line rendering

## Summary

The fix prevents OutOfMemoryException by:
1. Pre-checking estimated file size before loading into RichTextBox
2. Checking character count during StringBuilder creation
3. Catching OutOfMemoryException as last resort
4. Setting MaxLength property on RichTextBox controls
5. Providing user-configurable limits via AppSettings

This ensures the application gracefully handles large files instead of crashing.
