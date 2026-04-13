# ?? Help File Images - Guide

This document lists all images needed for the comprehensive help file.

---

## ?? IMAGES NEEDED (Total: 25 images)

### Section 1: Introduction & Getting Started (5 images)

#### 1. main-window.png
**Where:** Introduction section  
**What to capture:** Full application window showing:
- Menu bar at top
- Toolbar below menu
- Log view (main list) on left
- Tree panel on right
- Tab panel at bottom
- Status bar at very bottom

**Purpose:** Give users overview of interface  
**Annotations needed:** Label each major area with arrows/text

---

#### 2. file-open-menu.png
**Where:** Getting Started ? Opening Files  
**What to capture:** File menu dropdown with "Open" option highlighted  
**Purpose:** Show where to find Open command

---

#### 3. toolbar-open.png
**Where:** Getting Started ? Opening Files  
**What to capture:** Toolbar with Open button (folder icon) circled/highlighted  
**Purpose:** Show toolbar shortcut

---

#### 4. interface-overview.png
**Where:** Getting Started ? Understanding Interface  
**What to capture:** Same as main-window.png but with labeled annotations:
- "Menu Bar" (with arrow)
- "Toolbar" (with arrow)
- "Log View" (with arrow)
- "Tree Panel" (with arrow)
- "Tab Panel" (with arrow)
- "Status Bar" (with arrow)

**Purpose:** Detailed interface walkthrough  
**Tool needed:** Image editor to add arrows and labels

---

#### 5. first-analysis-workflow.png
**Where:** Getting Started ? Your First Analysis  
**What to capture:** 4-panel sequence showing:
1. File loading (progress bar)
2. Log view with entries
3. Performance tab active
4. Call tree visible

**Purpose:** Show typical first steps  
**Can be:** 4 separate small images combined into one

---

### Section 2: File Operations (2 images)

#### 6. open-dialog.png
**Where:** File Operations ? Opening Files  
**What to capture:** Windows Open File dialog showing .log file selection  
**Purpose:** Show file dialog interface

---

#### 7. excel-export-result.png
**Where:** File Operations ? Export to Excel  
**What to capture:** Excel spreadsheet showing exported log data with columns  
**Purpose:** Show what exported data looks like

---

#### 8. recent-files-menu.png
**Where:** File Operations ? Recent Files  
**What to capture:** File menu with Recent Files submenu expanded showing list of recent .log files  
**Purpose:** Show recent files feature

---

### Section 3: Search & Filter (4 images)

#### 9. find-dialog.png
**Where:** Search & Filter ? Basic Search  
**What to capture:** Find dialog with:
- Search text box
- Case sensitivity checkbox
- Use regex checkbox
- Search history dropdown

**Purpose:** Show search dialog options

---

#### 10. find-all-results.png
**Where:** Search & Filter ? Find All  
**What to capture:** Find All results window showing list of matches with line numbers  
**Purpose:** Show Find All feature

---

#### 11. filter-dialog.png
**Where:** Search & Filter ? Advanced Filtering  
**What to capture:** Filter dialog showing ALL criteria options:
- Text filter
- Duration filter
- Time range (From/To)
- Thread ID
- Log Level dropdown

**Purpose:** Show complete filter interface

---

#### 12. filter-slow-database.png
**Where:** Search & Filter ? Filter Scenarios  
**What to capture:** Filtered log view showing only database operations > 1000ms  
**Status bar should show:** "Filter: Active - 234 of 50,000 lines match"  
**Purpose:** Show filter results example

---

### Section 4: Bookmarks (2 images)

#### 13. bookmarks-in-logview.png
**Where:** Bookmarks ? Introduction  
**What to capture:** Log view with several lines bookmarked (showing bookmark icons)  
**Purpose:** Show what bookmarks look like in log view

---

#### 14. bookmarks-dialog.png
**Where:** Bookmarks ? Show All Bookmarks  
**What to capture:** Show All Bookmarks dialog with list of bookmarked lines  
**Purpose:** Show bookmarks management dialog

---

### Section 5: Navigation (2 images)

#### 15. navigation-error.png
**Where:** Navigation ? Error Navigation  
**What to capture:** Log view with ERROR line highlighted/selected after pressing F8  
**Purpose:** Show error navigation feature

---

#### 16. enter-exit-matching.png
**Where:** Navigation ? Jump to Matching  
**What to capture:** Two-panel view or split showing:
- ENTER line highlighted
- Matching EXIT line highlighted
- Arrow or line connecting them

**Purpose:** Show ENTER/EXIT matching  
**Can be:** Single screenshot with annotations added

---

#### 17. jump-to-line-dialog.png
**Where:** Navigation ? Jump to Line  
**What to capture:** Jump to Line dialog with input field  
**Purpose:** Show line number navigation

---

### Section 6: Tree Views (3 images)

#### 18. call-tree-view.png
**Where:** Tree Views ? Call Tree  
**What to capture:** Call Tree panel showing:
- Hierarchical structure
- Method names
- Durations next to methods
- Color coding (green/amber/red)
- Expanded and collapsed nodes

**Purpose:** Show Call Tree feature

---

#### 19. api-tree-view.png
**Where:** Tree Views ? API Tree  
**What to capture:** API Tree panel showing methods grouped by API  
**Purpose:** Show API Tree organization

---

#### 20. tree-search.png
**Where:** Tree Views ? Tree Search  
**What to capture:** Tree view with search box at top, showing filtered results  
**Purpose:** Show tree filtering feature

---

#### 21. tree-context-menu.png
**Where:** Tree Views ? Tree Context Menu  
**What to capture:** Right-click context menu on tree node showing all options:
- Copy Node Name
- Copy Subtree
- Expand/Collapse All
- Jump to Matching
- Save Branch
- Export Branch as CSV
- Search in Grok
- Show in Other Tree

**Purpose:** Show available tree operations

---

### Section 7: Performance Analysis (2 images)

#### 22. performance-tab.png
**Where:** Performance Analysis ? Introduction  
**What to capture:** Performance tab showing statistics table with:
- Method column
- Calls column
- Total (ms) column
- Avg, Min, Max columns
- Self (ms) column
- Color-coded heatmap (green/amber/red cells)

**Purpose:** Show performance analysis table

---

#### 23. performance-heatmap.png
**Where:** Performance Analysis ? Color-Coded Heatmap  
**What to capture:** Close-up of performance table with clear color coding:
- Green cells (< 100ms)
- Amber cells (100-500ms)
- Red cells (> 500ms)

**Purpose:** Demonstrate color coding system

---

### Section 8: Visualizations (3 images)

#### 24. flame-graph.png
**Where:** Visualizations ? Flame Graph  
**What to capture:** Flame Graph tab showing:
- Hierarchical colored bars
- Wider sections indicating more time
- Color coding (green/amber/red)
- Multiple levels of call stack

**Purpose:** Show flame graph visualization

---

#### 25. timeline-view.png
**Where:** Visualizations ? Timeline View  
**What to capture:** Timeline tab showing:
- Horizontal bars representing method calls
- Time axis (horizontal)
- Depth/thread axis (vertical)
- Different colored bars for different methods

**Purpose:** Show timeline visualization

---

#### 26. call-graph.png
**Where:** Visualizations ? Call Graph  
**What to capture:** Call Graph tab showing:
- Nodes (circles/boxes) representing methods
- Arrows/edges showing call relationships
- Graph layout

**Purpose:** Show call graph visualization

---

### Section 9: Other (3 images)

#### 27. export-menu.png
**Where:** Export Features  
**What to capture:** File menu with export options visible:
- Save to XLS
- Export Performance to CSV
- Export Tree as JSON
- Export Tree as XML
- Export Timeline as Image
- Export Flame Graph as Image

**Purpose:** Show all export options

---

#### 28. settings-dialog.png
**Where:** Settings & Configuration  
**What to capture:** Settings dialog showing all tabs:
- General tab
- Performance tab
- Display tab
- Advanced tab
(With one tab active showing its contents)

**Purpose:** Show settings interface

---

## ?? IMAGE REQUIREMENTS

### Technical Specs:
- **Format:** PNG preferred (good quality, small size)
- **Resolution:** 1920x1080 or higher (will be scaled down in help file)
- **Color:** Full color (screenshots should match application theme)
- **Compression:** Light compression OK (maintain readability)

### Quality Guidelines:
- **Clear and Sharp:** Text must be readable
- **No Personal Data:** Ensure log files don't contain sensitive information
- **Consistent Size:** Try to capture similar-sized windows for uniformity
- **Good Contrast:** Ensure highlights/selections are visible

### Annotations:
Some images need annotations (arrows, labels, circles):
- Use image editing tool (Paint, Snagit, Photoshop, etc.)
- Use consistent colors (red for highlights, blue for labels)
- Keep annotations clear but not overwhelming
- Font size should be readable when image is scaled

---

## ?? HOW TO CAPTURE

### Recommended Process:

1. **Prepare Test Log File:**
   - Use a medium-sized log (10,000-50,000 lines)
   - Ensure it has errors, warnings, method calls
   - Include performance data (durations)

2. **Set Up Application:**
   - Load the test log file
   - Ensure all features are working
   - Have some bookmarks set

3. **Capture Screenshots:**
   - Use Windows Snipping Tool (Win+Shift+S)
   - Or use Snagit, ShareX, or similar tools
   - Capture full windows where possible
   - For dialogs, include surrounding context

4. **Save Files:**
   - Name exactly as shown above (e.g., "main-window.png")
   - Save to `Help\images\` folder
   - PNG format preferred

5. **Annotate (if needed):**
   - Open in image editor
   - Add arrows, labels, circles as described
   - Use consistent styling
   - Save annotated version

---

## ?? FILE ORGANIZATION

```
Help/
??? UserGuide.html (already created)
??? images/
?   ??? main-window.png
?   ??? file-open-menu.png
?   ??? toolbar-open.png
?   ??? interface-overview.png
?   ??? first-analysis-workflow.png
?   ??? open-dialog.png
?   ??? excel-export-result.png
?   ??? recent-files-menu.png
?   ??? find-dialog.png
?   ??? find-all-results.png
?   ??? filter-dialog.png
?   ??? filter-slow-database.png
?   ??? bookmarks-in-logview.png
?   ??? bookmarks-dialog.png
?   ??? navigation-error.png
?   ??? enter-exit-matching.png
?   ??? jump-to-line-dialog.png
?   ??? call-tree-view.png
?   ??? api-tree-view.png
?   ??? tree-search.png
?   ??? tree-context-menu.png
?   ??? performance-tab.png
?   ??? performance-heatmap.png
?   ??? flame-graph.png
?   ??? timeline-view.png
?   ??? call-graph.png
?   ??? export-menu.png
?   ??? settings-dialog.png
??? README_IMAGES.md (this file)
```

---

## ? AFTER IMAGES ARE ADDED

Once you provide the images:

1. I'll replace all `screenshot-placeholder` divs with actual `<img>` tags
2. Add proper alt text for accessibility
3. Ensure captions match the images
4. Test that all images load correctly
5. Optimize image sizes if needed

---

## ?? PROVIDING IMAGES

You can provide images in any of these ways:

1. **Create folder:** `Help\images\` and place all PNG files there
2. **Provide as ZIP:** Compress all images and provide
3. **Share individually:** Provide one at a time as you capture them

**Priority Images** (most important, capture these first):
1. main-window.png (overview)
2. performance-tab.png (key feature)
3. flame-graph.png (key feature)
4. filter-dialog.png (frequently used)
5. call-tree-view.png (key feature)

---

**Total Images Needed:** 28  
**Estimated Time to Capture:** 2-3 hours (including annotations)  
**Impact:** Professional, complete help documentation  

Let me know when you're ready to provide images, and I'll integrate them into the help file!
