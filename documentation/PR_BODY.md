## Summary

This PR merges the BugFixing04 branch into master, delivering bug fixes, performance improvements, and new features across the CAD 3P Log Browser.

## Bug Fixes

- D1/P1 PerformanceAnalyzer: Replace FindTopSlowestCalls O(N^2) scan with O(N) stack-based LIFO matching; fixes recursive call pairing
- D2/P2 FilterService: Add Regex cache to avoid recompiling on every filtered log entry
- D3 MergeLogService: Cap estimatedLines at 5,000,000 to prevent OutOfMemoryException on large files
- D4 ExportService: Write entry.RawText so exported files contain original log lines; fix corrupt unicode header
- D9 CallGraphService: Handle mismatched EXIT frames; prevents unbounded stack growth on partial/corrupt logs
- D10 Program: Use timestamped crash log filename (DATE_TIME.err) so crashes never overwrite each other

## Performance Improvements

- P3 MergeLogService: Extract static TimestampComparison to avoid delegate allocation on every Sort
- P5 AiLogService: Remove redundant OrderByDescending on already-sorted perfStats in OfflineNlSearch
- P7 TreeViewManager: Add SolidBrush cache keyed by ARGB; add IDisposable.Dispose to release brushes on shutdown

## New Features / Enhancements

- MainForm.cs: +202 lines of new functionality
- FlameGraphPanel.cs: updated flame graph rendering
- TimelinePanel.cs: minor timeline adjustments
- AppSettings.cs: new settings added
- AiLogService.cs: AI analysis improvements
- LogParserService.cs: parser fixes
- ExportService.cs: extended export capabilities
- SettingsForm.cs: settings UI updates
- LineInspectorPanel.cs: inspector panel improvements
- Extensions.cs: new extension methods

## Code Quality

- Q2 BookmarkService: Replace corrupt unicode separators with standard dash-style comments
- Models/LogEntry: Add RawText property to support correct export behaviour

## Files Changed

10 files changed across the full branch (+514 insertions / -136 deletions)
