# Feature B10 Implementation Plan

## Feature B10 - Quick Navigation (Next/Prev Error/Warning)

### Requirements
- Toolbar buttons: ? Prev Warning, ? Next Warning, ? Prev Error, ? Next Error
- Index all ERROR and WARN lines on file open
- Navigate through error/warning lines with wrap-around
- Status bar shows: "N errors, M warnings"
- Keyboard shortcuts: F8 (Next Error), Shift+F8 (Prev Error)

### Implementation Steps

1. **Add fields to MainForm.cs:**
   - `List<int> _errorLines`
   - `List<int> _warningLines`
   - `int _currentErrorIndex`
   - `int _currentWarningIndex`

2. **Index errors/warnings in PopulateVirtualListView():**
   - Scan for 'E' and 'W' level indicators
   - Store line indices in respective lists

3. **Add toolbar buttons:**
   - Previous Warning button
   - Next Warning button
   - Previous Error button
   - Next Error button

4. **Add navigation methods:**
   - `NavigateToNextError()`
   - `NavigateToPreviousError()`
   - `NavigateToNextWarning()`
   - `NavigateToPreviousWarning()`

5. **Update status bar:**
   - Show error and warning counts
   - Update on file load

6. **Add keyboard shortcuts:**
   - F8 ? Next Error
   - Shift+F8 ? Prev Error
   - Ctrl+F8 ? Next Warning
   - Ctrl+Shift+F8 ? Prev Warning

Let's implement!
