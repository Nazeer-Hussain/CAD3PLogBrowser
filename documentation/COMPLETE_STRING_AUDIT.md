# COMPREHENSIVE STRING EXTERNALIZATION - ALL REMAINING STRINGS

## ?? COMPLETE AUDIT

### Hard-coded Strings Found: 23 in MainForm.cs

1. `"Failed to export file:\n{0}"` - Line ~1390
2. `"'{0}' not found."` - Line ~1412
3. `"Invalid regular expression:\n{0}"` - Line ~1560
4. `"Could not open updates page:\n{0}"` - Line ~2162
5. `"Could not open issues page:\n{0}"` - Line ~2176
6. `"Could not open help file:\n{0}"` - Line ~2206
7. `"Failed to open browser:\n{0}"` - Line ~2523
8. `"Branch exported to:\n{0}"` - Line ~2571
9. `"Performance data exported to:\n{0}"` - Line ~2696
10. `"Could not export performance data:\n{0}"` - Line ~2701
11. `"Line number must be between 1 and {0}."` - Line ~2745
12. `"Could not find ENTER/EXIT pair for '{0}'."` - Line ~2796
13. `"{0} line(s) saved to:\n{1}"` - Line ~2814
14. `"Could not save branch:\n{0}"` - Line ~2819
15. `"No matches found for '{0}'."` - Line ~2868
16. `"Call Graph exported to:\n{0}"` - Line ~2927
17. `"Could not export Call Graph:\n{0}"` - Line ~2932
18. `"Failed to copy to clipboard:\n{0}"` - Line ~3084
19. `"Bookmarks"` - Line ~3507 (dialog title)
20. `"Failed to export tree:\n{0}"` - Line ~3563 & ~3602
21. `"Failed to export timeline:\n{0}"` - Line ~3651
22. `"Failed to export flame graph:\n{0}"` - Line ~3700
23. `"Failed to change font:\n{0}"` - Line ~3846

Plus the const string for help message.

---

## ?? RESOURCES TO ADD (23 new resources)

Add these to Resources.resx in Visual Studio:

```
Name: ERR_EXPORT_FILE_FAILED
Value: Failed to export file:\n{0}

Name: ERR_NOT_FOUND
Value: '{0}' not found.

Name: ERR_INVALID_REGEX
Value: Invalid regular expression:\n{0}

Name: ERR_OPEN_UPDATES_FAILED
Value: Could not open updates page:\n{0}

Name: ERR_OPEN_ISSUES_FAILED
Value: Could not open issues page:\n{0}

Name: ERR_OPEN_HELP_FAILED
Value: Could not open help file:\n{0}

Name: ERR_BROWSER_FAILED
Value: Failed to open browser:\n{0}

Name: ERR_EXPORT_PERFORMANCE_FAILED
Value: Could not export performance data:\n{0}

Name: ERR_LINE_NUMBER_OUT_OF_RANGE
Value: Line number must be between 1 and {0}.

Name: ERR_NO_ENTER_EXIT_PAIR
Value: Could not find ENTER/EXIT pair for '{0}'.

Name: ERR_SAVE_BRANCH_FAILED
Value: Could not save branch:\n{0}

Name: ERR_NO_MATCHES_FOUND
Value: No matches found for '{0}'.

Name: ERR_EXPORT_CALL_GRAPH_FAILED
Value: Could not export Call Graph:\n{0}

Name: ERR_COPY_FAILED
Value: Failed to copy to clipboard:\n{0}

Name: ERR_EXPORT_TREE_FAILED
Value: Failed to export tree:\n{0}

Name: ERR_EXPORT_TIMELINE_FAILED
Value: Failed to export timeline:\n{0}

Name: ERR_EXPORT_FLAME_GRAPH_FAILED
Value: Failed to export flame graph:\n{0}

Name: ERR_FONT_CHANGE_FAILED
Value: Failed to change font:\n{0}

Name: MSG_BRANCH_EXPORTED_TO
Value: Branch exported to:\n{0}

Name: MSG_PERFORMANCE_EXPORTED_TO
Value: Performance data exported to:\n{0}

Name: MSG_BRANCH_SAVED_TO
Value: {0} line(s) saved to:\n{1}

Name: MSG_CALL_GRAPH_EXPORTED_TO
Value: Call Graph exported to:\n{0}

Name: DIALOG_TITLE_BOOKMARKS
Value: Bookmarks

Name: MSG_HELP_FILE_NOT_FOUND
Value: Help file (Cad3PLogBrowser.chm) not found.\n\nPress Ctrl+K to view keyboard shortcuts,\nor visit the GitHub repository for documentation.
```

---

## ? COMPLETE REPLACEMENT LIST

I'll provide the comprehensive solution in the next file with ALL replacements.

