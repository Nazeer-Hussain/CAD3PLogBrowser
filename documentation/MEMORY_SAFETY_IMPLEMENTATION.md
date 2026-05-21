# Memory Safety Implementation for CAD3PLogBrowser

## Problem
OutOfMemoryException when loading large log files into RichTextBox controls (rawTextBox, logDetailBox, _apiDetailsBox).

## Root Cause
RichTextBox controls have ~2GB theoretical limit but become unstable above 10-50MB of text. The application was loading entire files without size checks.

## Solution
1. Added MaxRichTextBoxChars and MaxRichTextBoxFileSizeBytes settings to AppSettings
2. Need to implement size checks before populating RichTextBox controls
3. Show placeholder message for files exceeding limits

## Files Modified
- Cad3PLogBrowser\Models\AppSettings.cs: Added memory safety settings

## Files That Need Updates
- Cad3PLogBrowser\MainForm.cs: 
  - Check file size before populating rawTextBox
  - Truncate or show placeholder for large files
  - Apply same logic to other RichTextBox controls (_apiDetailsBox)

## Implementation Strategy
Instead of loading entire file into rawTextBox.Text, we should:
1. Check if file size > MaxRichTextBoxFileSizeBytes
2. If too large, show a message like "File too large to display in raw view (XX MB). Use the Log tab for better performance."
3. Never call string.Join or Text = on RichTextBox with huge strings
