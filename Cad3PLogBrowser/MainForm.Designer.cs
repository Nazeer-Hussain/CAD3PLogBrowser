namespace Cad3PLogBrowser
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFilteredLogsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportPerformanceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTreeJsonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTreeXmlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTimelineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFlameGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSeparatorAfterSave = new System.Windows.Forms.ToolStripSeparator();
            this.reloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeLogsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSeparatorBeforeExit = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSeparatorAfterFilter = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSeparatorAfterCollapse = new System.Windows.Forms.ToolStripSeparator();
            this.jumpToMatchingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSeparatorBeforeBookmarks = new System.Windows.Forms.ToolStripSeparator();
            this.toggleBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBookmarksMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearBookmarksMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSeparatorAfterBookmarks = new System.Windows.Forms.ToolStripSeparator();
            this.copyWithHeadersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSeparatorBeforeJumpToLine = new System.Windows.Forms.ToolStripSeparator();
            this.jumpToLineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCallTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showApiTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showToolbarMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFlameGraphTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTimelineTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyboardShortcutsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportErrorsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.FileStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.FileLoadProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.StatusFileName = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLineCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusSelection = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.treeSearchTextBox = new System.Windows.Forms.TextBox();
            this.ApiTree = new System.Windows.Forms.TreeView();
            this.CallTree = new System.Windows.Forms.TreeView();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.logTab = new System.Windows.Forms.TabPage();
            this.logListView = new System.Windows.Forms.ListView();
            this.colLineNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLogText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rawTab = new System.Windows.Forms.TabPage();
            this.performanceTab = new System.Windows.Forms.TabPage();
            this.logDetailTab = new System.Windows.Forms.TabPage();
            this.callGraphTab = new System.Windows.Forms.TabPage();
            this.flameGraphTab = new System.Windows.Forms.TabPage();
            this.timelineTab = new System.Windows.Forms.TabPage();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.OpenButton = new System.Windows.Forms.ToolStripButton();
            this.SaveButton = new System.Windows.Forms.ToolStripButton();
            this.SaveToXLSButton = new System.Windows.Forms.ToolStripButton();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.separatorAfterRefresh = new System.Windows.Forms.ToolStripSeparator();
            this.CopyButton = new System.Windows.Forms.ToolStripButton();
            this.FindButton = new System.Windows.Forms.ToolStripButton();
            this.FindNextButton = new System.Windows.Forms.ToolStripButton();
            this.FilterButton = new System.Windows.Forms.ToolStripButton();
            this.separatorAfterFilter = new System.Windows.Forms.ToolStripSeparator();
            this.ExpandAllButton = new System.Windows.Forms.ToolStripButton();
            this.CollapseAllButton = new System.Windows.Forms.ToolStripButton();
            this.JumpToMatchingButton = new System.Windows.Forms.ToolStripButton();
            this.separatorAfterTreeOps = new System.Windows.Forms.ToolStripSeparator();
            this.prevErrorButton = new System.Windows.Forms.ToolStripButton();
            this.nextErrorButton = new System.Windows.Forms.ToolStripButton();
            this.prevWarningButton = new System.Windows.Forms.ToolStripButton();
            this.nextWarningButton = new System.Windows.Forms.ToolStripButton();
            this.separatorAfterNavigation = new System.Windows.Forms.ToolStripSeparator();
            this.CallTreeButton = new System.Windows.Forms.ToolStripButton();
            this.ApiTreeButton = new System.Windows.Forms.ToolStripButton();
            this.separatorAfterTreeView = new System.Windows.Forms.ToolStripSeparator();
            this.SettingsButton = new System.Windows.Forms.ToolStripButton();
            this.separatorAfterSettings = new System.Windows.Forms.ToolStripSeparator();
            this.ShowHelpButton = new System.Windows.Forms.ToolStripButton();
            this.openLogFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveLogFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.logFontDialog = new System.Windows.Forms.FontDialog();
            this.logWatcher = new System.IO.FileSystemWatcher();
            this.fileWatchTimer = new System.Windows.Forms.Timer(this.components);
            this.logContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextCopyWithHeadersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.contextFindMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.contextExpandAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextCollapseAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextJumpToMatchingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.contextRefreshMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.treeContextCopyNodeNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextCopySubtreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.treeContextExpandAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextCollapseAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextJumpToMatchingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.treeContextSaveBranchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextExportBranchCsvMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextSearchInGrokMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeContextShowInOtherTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeIconList = new System.Windows.Forms.ImageList(this.components);
            this.treeIconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.treeIconList.ImageSize = new System.Drawing.Size(16, 16);
            this.treeIconList.TransparentColor = System.Drawing.Color.Transparent;
            this.mainMenuStrip.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.logTab.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logWatcher)).BeginInit();
            this.logContextMenu.SuspendLayout();
            this.treeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.editMenuItem,
            this.optionsMenuItem,
            this.viewMenuItem,
            this.helpMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(987, 26);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "mainMenuStrip";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenuItem,
            this.mergeLogsMenuItem,
            this.saveAsMenuItem,
            this.exportFilteredLogsMenuItem,
            this.exportPerformanceMenuItem,
            this.exportTreeJsonMenuItem,
            this.exportTreeXmlMenuItem,
            this.exportTimelineMenuItem,
            this.exportFlameGraphMenuItem,
            this.fileSeparatorAfterSave,
            this.reloadMenuItem,
            this.fileSeparatorBeforeExit,
            this.exitMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(40, 22);
            this.fileMenuItem.Text = "&File";
            // 
            // openMenuItem
            // 
            this.openMenuItem.AutoToolTip = true;
            this.openMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.open;
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.ShortcutKeyDisplayString = "";
            this.openMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openMenuItem.Size = new System.Drawing.Size(260, 22);
            this.openMenuItem.Text = "&Open ...";
            this.openMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.openMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
            // 
            // saveAsMenuItem
            // 
            this.saveAsMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.save;
            this.saveAsMenuItem.Name = "saveAsMenuItem";
            this.saveAsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsMenuItem.Size = new System.Drawing.Size(260, 22);
            this.saveAsMenuItem.Text = "Save &Selected...";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // exportFilteredLogsMenuItem
            // 
            this.exportFilteredLogsMenuItem.Name = "exportFilteredLogsMenuItem";
            this.exportFilteredLogsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.E)));
            this.exportFilteredLogsMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exportFilteredLogsMenuItem.Text = "Save to &XLS...";
            this.exportFilteredLogsMenuItem.Click += new System.EventHandler(this.exportFilteredLogsMenuItem_Click);
            // 
            // exportPerformanceMenuItem
            // 
            this.exportPerformanceMenuItem.Name = "exportPerformanceMenuItem";
            this.exportPerformanceMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exportPerformanceMenuItem.Text = "Export &Performance to CSV...";
            this.exportPerformanceMenuItem.Click += new System.EventHandler(this.exportPerformanceMenuItem_Click);
            // 
            // exportTreeJsonMenuItem
            // 
            this.exportTreeJsonMenuItem.Name = "exportTreeJsonMenuItem";
            this.exportTreeJsonMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exportTreeJsonMenuItem.Text = "Export Tree as &JSON...";
            this.exportTreeJsonMenuItem.Click += new System.EventHandler(this.exportTreeJsonMenuItem_Click);
            // 
            // exportTreeXmlMenuItem
            // 
            this.exportTreeXmlMenuItem.Name = "exportTreeXmlMenuItem";
            this.exportTreeXmlMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exportTreeXmlMenuItem.Text = "Export Tree as X&ML...";
            this.exportTreeXmlMenuItem.Click += new System.EventHandler(this.exportTreeXmlMenuItem_Click);
            // 
            // exportTimelineMenuItem
            // 
            this.exportTimelineMenuItem.Name = "exportTimelineMenuItem";
            this.exportTimelineMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exportTimelineMenuItem.Text = "Export &Timeline as Image...";
            this.exportTimelineMenuItem.Click += new System.EventHandler(this.exportTimelineMenuItem_Click);
            // 
            // exportFlameGraphMenuItem
            // 
            this.exportFlameGraphMenuItem.Name = "exportFlameGraphMenuItem";
            this.exportFlameGraphMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exportFlameGraphMenuItem.Text = "Export &Flame Graph as Image...";
            this.exportFlameGraphMenuItem.Click += new System.EventHandler(this.exportFlameGraphMenuItem_Click);
            // 
            // fileSeparatorAfterSave
            // 
            this.fileSeparatorAfterSave.Name = "fileSeparatorAfterSave";
            this.fileSeparatorAfterSave.Size = new System.Drawing.Size(257, 6);
            // 
            // mergeLogsMenuItem
            // 
            this.mergeLogsMenuItem.Name = "mergeLogsMenuItem";
            this.mergeLogsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.mergeLogsMenuItem.Size = new System.Drawing.Size(260, 22);
            this.mergeLogsMenuItem.Text = "&Merge Log Files...";
            this.mergeLogsMenuItem.Click += new System.EventHandler(this.mergeLogsMenuItem_Click);
            // 
            // reloadMenuItem
            // 
            this.reloadMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.refresh;
            this.reloadMenuItem.Name = "reloadMenuItem";
            this.reloadMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.reloadMenuItem.Size = new System.Drawing.Size(260, 22);
            this.reloadMenuItem.Text = "&Reload from Disk";
            this.reloadMenuItem.Click += new System.EventHandler(this.refreshMenuItem_Click);
            // 
            // fileSeparatorBeforeExit
            // 
            this.fileSeparatorBeforeExit.Name = "fileSeparatorBeforeExit";
            this.fileSeparatorBeforeExit.Size = new System.Drawing.Size(257, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitMenuItem.Size = new System.Drawing.Size(260, 22);
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // editMenuItem
            // 
            this.editMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyMenuItem,
            this.copyWithHeadersMenuItem,
            this.findMenuItem,
            this.findNextMenuItem,
            this.findAllMenuItem,
            this.filterMenuItem,
            this.clearFilterMenuItem,
            this.editSeparatorAfterFilter,
            this.expandAllMenuItem,
            this.collapseAllMenuItem,
            this.editSeparatorAfterCollapse,
            this.jumpToMatchingMenuItem,
            this.editSeparatorBeforeBookmarks,
            this.toggleBookmarkMenuItem,
            this.nextBookmarkMenuItem,
            this.previousBookmarkMenuItem,
            this.showBookmarksMenuItem,
            this.clearBookmarksMenuItem,
            this.editSeparatorAfterBookmarks,
            this.editSeparatorBeforeJumpToLine,
            this.jumpToLineMenuItem});
            this.editMenuItem.Name = "editMenuItem";
            this.editMenuItem.Size = new System.Drawing.Size(43, 22);
            this.editMenuItem.Text = "&Edit";
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.copy;
            this.copyMenuItem.Name = "copyMenuItem";
            this.copyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyMenuItem.Size = new System.Drawing.Size(258, 22);
            this.copyMenuItem.Text = "&Copy";
            this.copyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // findMenuItem
            // 
            this.findMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.find;
            this.findMenuItem.Name = "findMenuItem";
            this.findMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findMenuItem.Size = new System.Drawing.Size(258, 22);
            this.findMenuItem.Text = "&Find";
            this.findMenuItem.Click += new System.EventHandler(this.findMenuItem_Click);
            // 
            // findNextMenuItem
            // 
            this.findNextMenuItem.Name = "findNextMenuItem";
            this.findNextMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findNextMenuItem.Size = new System.Drawing.Size(258, 22);
            this.findNextMenuItem.Text = "Find &Next";
            // 
            // findAllMenuItem
            // 
            this.findAllMenuItem.Name = "findAllMenuItem";
            this.findAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) | System.Windows.Forms.Keys.F));
            this.findAllMenuItem.Size = new System.Drawing.Size(258, 22);
            this.findAllMenuItem.Text = "Find &All...";
            this.findAllMenuItem.Click += new System.EventHandler(this.findAllMenuItem_Click);
            // 
            // filterMenuItem
            // 
            this.filterMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.filter;
            this.filterMenuItem.Name = "filterMenuItem";
            this.filterMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.filterMenuItem.Size = new System.Drawing.Size(258, 22);
            this.filterMenuItem.Text = "Fil&ter...";
            this.filterMenuItem.Click += new System.EventHandler(this.filterMenuItem_Click);
            // 
            // editSeparatorAfterFilter
            // 
            this.editSeparatorAfterFilter.Name = "editSeparatorAfterFilter";
            this.editSeparatorAfterFilter.Size = new System.Drawing.Size(255, 6);
            // 
            // expandAllMenuItem
            // 
            this.expandAllMenuItem.Name = "expandAllMenuItem";
            this.expandAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.expandAllMenuItem.Size = new System.Drawing.Size(258, 22);
            this.expandAllMenuItem.Text = "&Expand All";
            this.expandAllMenuItem.Click += new System.EventHandler(this.expandAllMenuItem_Click);
            // 
            // collapseAllMenuItem
            // 
            this.collapseAllMenuItem.Name = "collapseAllMenuItem";
            this.collapseAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.collapseAllMenuItem.Size = new System.Drawing.Size(258, 22);
            this.collapseAllMenuItem.Text = "&Collapse All";
            this.collapseAllMenuItem.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
            // 
            // editSeparatorAfterCollapse
            // 
            this.editSeparatorAfterCollapse.Name = "editSeparatorAfterCollapse";
            this.editSeparatorAfterCollapse.Size = new System.Drawing.Size(255, 6);
            // 
            // jumpToMatchingMenuItem
            // 
            this.jumpToMatchingMenuItem.Name = "jumpToMatchingMenuItem";
            this.jumpToMatchingMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.jumpToMatchingMenuItem.Size = new System.Drawing.Size(258, 22);
            this.jumpToMatchingMenuItem.Text = "&Jump to Matching Enter/Exit";
            this.jumpToMatchingMenuItem.Click += new System.EventHandler(this.jumpToMatchingMenuItem_Click);
            // 
            // editSeparatorBeforeBookmarks
            // 
            this.editSeparatorBeforeBookmarks.Name = "editSeparatorBeforeBookmarks";
            this.editSeparatorBeforeBookmarks.Size = new System.Drawing.Size(255, 6);
            // 
            // toggleBookmarkMenuItem
            // 
            this.toggleBookmarkMenuItem.Name = "toggleBookmarkMenuItem";
            this.toggleBookmarkMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.toggleBookmarkMenuItem.Size = new System.Drawing.Size(258, 22);
            this.toggleBookmarkMenuItem.Text = "Toggle &Bookmark";
            this.toggleBookmarkMenuItem.Click += new System.EventHandler(this.toggleBookmarkMenuItem_Click);
            // 
            // nextBookmarkMenuItem
            // 
            this.nextBookmarkMenuItem.Name = "nextBookmarkMenuItem";
            this.nextBookmarkMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.nextBookmarkMenuItem.Size = new System.Drawing.Size(258, 22);
            this.nextBookmarkMenuItem.Text = "&Next Bookmark";
            this.nextBookmarkMenuItem.Click += new System.EventHandler(this.nextBookmarkMenuItem_Click);
            // 
            // previousBookmarkMenuItem
            // 
            this.previousBookmarkMenuItem.Name = "previousBookmarkMenuItem";
            this.previousBookmarkMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F2)));
            this.previousBookmarkMenuItem.Size = new System.Drawing.Size(258, 22);
            this.previousBookmarkMenuItem.Text = "&Previous Bookmark";
            this.previousBookmarkMenuItem.Click += new System.EventHandler(this.previousBookmarkMenuItem_Click);
            // 
            // showBookmarksMenuItem
            // 
            this.showBookmarksMenuItem.Name = "showBookmarksMenuItem";
            this.showBookmarksMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.B)));
            this.showBookmarksMenuItem.Size = new System.Drawing.Size(258, 22);
            this.showBookmarksMenuItem.Text = "Show &All Bookmarks";
            this.showBookmarksMenuItem.Click += new System.EventHandler(this.showBookmarksMenuItem_Click);
            // 
            // clearBookmarksMenuItem
            // 
            this.clearBookmarksMenuItem.Name = "clearBookmarksMenuItem";
            this.clearBookmarksMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.Delete)));
            this.clearBookmarksMenuItem.Size = new System.Drawing.Size(258, 22);
            this.clearBookmarksMenuItem.Text = "&Clear All Bookmarks";
            this.clearBookmarksMenuItem.Click += new System.EventHandler(this.clearBookmarksMenuItem_Click);
            // 
            // editSeparatorAfterBookmarks
            // 
            this.editSeparatorAfterBookmarks.Name = "editSeparatorAfterBookmarks";
            this.editSeparatorAfterBookmarks.Size = new System.Drawing.Size(255, 6);
            // 
            // copyWithHeadersMenuItem
            // 
            this.copyWithHeadersMenuItem.Name = "copyWithHeadersMenuItem";
            this.copyWithHeadersMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.C)));
            this.copyWithHeadersMenuItem.Size = new System.Drawing.Size(258, 22);
            this.copyWithHeadersMenuItem.Text = "Copy with &Headers";
            this.copyWithHeadersMenuItem.Click += new System.EventHandler(this.copyWithHeadersMenuItem_Click);
            // 
            // clearFilterMenuItem
            // 
            this.clearFilterMenuItem.Name = "clearFilterMenuItem";
            this.clearFilterMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.F)));
            this.clearFilterMenuItem.Size = new System.Drawing.Size(258, 22);
            this.clearFilterMenuItem.Text = "C&lear Filter";
            this.clearFilterMenuItem.Click += new System.EventHandler(this.clearFilterMenuItem_Click);
            // 
            // editSeparatorBeforeJumpToLine
            // 
            this.editSeparatorBeforeJumpToLine.Name = "editSeparatorBeforeJumpToLine";
            this.editSeparatorBeforeJumpToLine.Size = new System.Drawing.Size(255, 6);
            // 
            // jumpToLineMenuItem
            // 
            this.jumpToLineMenuItem.Name = "jumpToLineMenuItem";
            this.jumpToLineMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.jumpToLineMenuItem.Size = new System.Drawing.Size(258, 22);
            this.jumpToLineMenuItem.Text = "Jump to &Line...";
            this.jumpToLineMenuItem.Click += new System.EventHandler(this.jumpToLineMenuItem_Click);
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsMenuItem});
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Size = new System.Drawing.Size(69, 22);
            this.optionsMenuItem.Text = "&Options";
            // 
            // settingsMenuItem
            // 
            this.settingsMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.settings;
            this.settingsMenuItem.Name = "settingsMenuItem";
            this.settingsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.S));
            this.settingsMenuItem.Size = new System.Drawing.Size(218, 22);
            this.settingsMenuItem.Text = "&Settings";
            this.settingsMenuItem.Click += new System.EventHandler(this.settingsMenuItem_Click);
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showCallTreeMenuItem,
            this.showApiTreeMenuItem,
            this.tabsMenuItem,
            this.viewSeparator1,
            this.selectFontMenuItem,
            this.showToolbarMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(49, 22);
            this.viewMenuItem.Text = "&View";
            // 
            // showCallTreeMenuItem
            // 
            this.showCallTreeMenuItem.CheckOnClick = true;
            this.showCallTreeMenuItem.Checked = true;
            this.showCallTreeMenuItem.Name = "showCallTreeMenuItem";
            this.showCallTreeMenuItem.Size = new System.Drawing.Size(178, 22);
            this.showCallTreeMenuItem.Text = "Show &Call Tree";
            this.showCallTreeMenuItem.CheckedChanged += new System.EventHandler(this.showCallTreeMenuItem_CheckedChanged);
            // 
            // showApiTreeMenuItem
            // 
            this.showApiTreeMenuItem.CheckOnClick = true;
            this.showApiTreeMenuItem.Checked = false;
            this.showApiTreeMenuItem.Name = "showApiTreeMenuItem";
            this.showApiTreeMenuItem.Size = new System.Drawing.Size(178, 22);
            this.showApiTreeMenuItem.Text = "Show &API Tree";
            this.showApiTreeMenuItem.CheckedChanged += new System.EventHandler(this.showApiTreeMenuItem_CheckedChanged);
            // 
            // tabsMenuItem
            // 
            this.tabsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTab1MenuItem,
            this.showTab2MenuItem,
            this.showTab3MenuItem,
            this.showTab4MenuItem,
            this.showFlameGraphTabMenuItem,
            this.showTimelineTabMenuItem});
            this.tabsMenuItem.Name = "tabsMenuItem";
            this.tabsMenuItem.Size = new System.Drawing.Size(178, 22);
            this.tabsMenuItem.Text = "Show &Tabs";
            // 
            // showTab1MenuItem - LogView
            // 
            this.showTab1MenuItem.CheckOnClick = true;
            this.showTab1MenuItem.Checked = true;
            this.showTab1MenuItem.Name = "showTab1MenuItem";
            this.showTab1MenuItem.Size = new System.Drawing.Size(180, 22);
            this.showTab1MenuItem.Text = "&LogView";
            this.showTab1MenuItem.CheckedChanged += new System.EventHandler(this.showTab1MenuItem_CheckedChanged);
            // 
            // showTab2MenuItem - Performance View
            // 
            this.showTab2MenuItem.CheckOnClick = true;
            this.showTab2MenuItem.Checked = true;
            this.showTab2MenuItem.Name = "showTab2MenuItem";
            this.showTab2MenuItem.Size = new System.Drawing.Size(180, 22);
            this.showTab2MenuItem.Text = "&Performance View";
            this.showTab2MenuItem.CheckedChanged += new System.EventHandler(this.showTab2MenuItem_CheckedChanged);
            // 
            // showTab3MenuItem - Details
            // 
            this.showTab3MenuItem.CheckOnClick = true;
            this.showTab3MenuItem.Checked = true;
            this.showTab3MenuItem.Name = "showTab3MenuItem";
            this.showTab3MenuItem.Size = new System.Drawing.Size(180, 22);
            this.showTab3MenuItem.Text = "&Details";
            this.showTab3MenuItem.CheckedChanged += new System.EventHandler(this.showTab3MenuItem_CheckedChanged);
            // 
            // showTab4MenuItem - CallGraph
            // 
            this.showTab4MenuItem.CheckOnClick = true;
            this.showTab4MenuItem.Checked = true;
            this.showTab4MenuItem.Name = "showTab4MenuItem";
            this.showTab4MenuItem.Size = new System.Drawing.Size(180, 22);
            this.showTab4MenuItem.Text = "&CallGraph";
            this.showTab4MenuItem.CheckedChanged += new System.EventHandler(this.showTab4MenuItem_CheckedChanged);
            // 
            // showFlameGraphTabMenuItem
            // 
            this.showFlameGraphTabMenuItem.CheckOnClick = true;
            this.showFlameGraphTabMenuItem.Checked = true;
            this.showFlameGraphTabMenuItem.Name = "showFlameGraphTabMenuItem";
            this.showFlameGraphTabMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showFlameGraphTabMenuItem.Text = "&Flame Graph";
            this.showFlameGraphTabMenuItem.CheckedChanged += new System.EventHandler(this.showFlameGraphTabMenuItem_CheckedChanged);
            // 
            // showTimelineTabMenuItem
            // 
            this.showTimelineTabMenuItem.CheckOnClick = true;
            this.showTimelineTabMenuItem.Checked = true;
            this.showTimelineTabMenuItem.Name = "showTimelineTabMenuItem";
            this.showTimelineTabMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showTimelineTabMenuItem.Text = "&Timeline";
            this.showTimelineTabMenuItem.CheckedChanged += new System.EventHandler(this.showTimelineTabMenuItem_CheckedChanged);
            // 
            // viewSeparator1
            // 
            this.viewSeparator1.Name = "viewSeparator1";
            this.viewSeparator1.Size = new System.Drawing.Size(175, 6);
            // 
            // selectFontMenuItem
            // 
            this.selectFontMenuItem.Name = "selectFontMenuItem";
            this.selectFontMenuItem.Size = new System.Drawing.Size(178, 22);
            this.selectFontMenuItem.Text = "Select &Font...";
            this.selectFontMenuItem.Click += new System.EventHandler(this.selectFontMenuItem_Click);
            // 
            // showToolbarMenuItem
            // 
            this.showToolbarMenuItem.Checked = true;
            this.showToolbarMenuItem.CheckOnClick = true;
            this.showToolbarMenuItem.Name = "showToolbarMenuItem";
            this.showToolbarMenuItem.Size = new System.Drawing.Size(178, 22);
            this.showToolbarMenuItem.Text = "Show &Toolbar";
            this.showToolbarMenuItem.CheckedChanged += new System.EventHandler(this.showToolbarMenuItem_CheckedChanged);
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewHelpMenuItem,
            this.keyboardShortcutsMenuItem,
            this.helpSeparator1,
            this.aboutMenuItem,
            this.checkForUpdatesMenuItem,
            this.reportErrorsMenuItem});
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(48, 22);
            this.helpMenuItem.Text = "&Help";
            // 
            // viewHelpMenuItem
            // 
            this.viewHelpMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.help;
            this.viewHelpMenuItem.Name = "viewHelpMenuItem";
            this.viewHelpMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.viewHelpMenuItem.Size = new System.Drawing.Size(200, 22);
            this.viewHelpMenuItem.Text = "&Help CHM";
            this.viewHelpMenuItem.Click += new System.EventHandler(this.viewHelpMenuItem_Click);
            // 
            // keyboardShortcutsMenuItem
            // 
            this.keyboardShortcutsMenuItem.Name = "keyboardShortcutsMenuItem";
            this.keyboardShortcutsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
            this.keyboardShortcutsMenuItem.Size = new System.Drawing.Size(220, 22);
            this.keyboardShortcutsMenuItem.Text = "&Keyboard Shortcuts";
            this.keyboardShortcutsMenuItem.Click += new System.EventHandler(this.keyboardShortcutsMenuItem_Click);
            // 
            // helpSeparator1
            // 
            this.helpSeparator1.Name = "helpSeparator1";
            this.helpSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(200, 22);
            this.aboutMenuItem.Text = "&About";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // checkForUpdatesMenuItem
            // 
            this.checkForUpdatesMenuItem.Name = "checkForUpdatesMenuItem";
            this.checkForUpdatesMenuItem.Size = new System.Drawing.Size(200, 22);
            this.checkForUpdatesMenuItem.Text = "&Check for Updates";
            this.checkForUpdatesMenuItem.Click += new System.EventHandler(this.checkForUpdatesMenuItem_Click);
            // 
            // reportErrorsMenuItem
            // 
            this.reportErrorsMenuItem.Name = "reportErrorsMenuItem";
            this.reportErrorsMenuItem.Size = new System.Drawing.Size(200, 22);
            this.reportErrorsMenuItem.Text = "&Report Errors...";
            this.reportErrorsMenuItem.Click += new System.EventHandler(this.reportErrorsMenuItem_Click);
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileStatus,
            this.StatusFileName,
            this.StatusLineCount,
            this.StatusSelection,
            this.FileLoadProgress});
            this.mainStatusStrip.Location = new System.Drawing.Point(3, 469);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Size = new System.Drawing.Size(682, 22);
            this.mainStatusStrip.TabIndex = 1;
            this.mainStatusStrip.Text = "mainStatusStrip";
            // 
            // FileStatus
            // 
            this.FileStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FileStatus.Image = global::Cad3PLogBrowser.Properties.Resources.yellow;
            this.FileStatus.Name = "FileStatus";
            this.FileStatus.Size = new System.Drawing.Size(16, 17);
            // 
            // FileLoadProgress
            // 
            this.FileLoadProgress.AutoSize = false;
            this.FileLoadProgress.Name = "FileLoadProgress";
            this.FileLoadProgress.Size = new System.Drawing.Size(200, 17);
            this.FileLoadProgress.Visible = false;
            // 
            // StatusFileName
            // 
            this.StatusFileName.Name = "StatusFileName";
            this.StatusFileName.Size = new System.Drawing.Size(300, 17);
            this.StatusFileName.Spring = true;
            this.StatusFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.StatusFileName.Text = "";
            this.StatusFileName.IsLink = true;
            this.StatusFileName.Click += new System.EventHandler(this.StatusFileName_Click);
            // 
            // StatusLineCount
            // 
            this.StatusLineCount.Name = "StatusLineCount";
            this.StatusLineCount.Size = new System.Drawing.Size(120, 17);
            this.StatusLineCount.Text = "";
            // 
            // StatusSelection
            // 
            this.StatusSelection.Name = "StatusSelection";
            this.StatusSelection.Size = new System.Drawing.Size(100, 17);
            this.StatusSelection.Text = "";
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.AccessibleName = "TreePanel";
            this.mainSplitContainer.Panel1.Controls.Add(this.CallTree);
            this.mainSplitContainer.Panel1.Controls.Add(this.ApiTree);
            this.mainSplitContainer.Panel1.Controls.Add(this.treeSearchTextBox);
            this.mainSplitContainer.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.AccessibleName = "TabPanel";
            this.mainSplitContainer.Panel2.Controls.Add(this.mainTabControl);
            this.mainSplitContainer.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.mainSplitContainer.SplitterDistance = 285;
            this.mainSplitContainer.TabIndex = 3;
            this.mainSplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // treeSearchTextBox
            // 
            this.treeSearchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.treeSearchTextBox.Name = "treeSearchTextBox";
            this.treeSearchTextBox.Height = 28;
            this.treeSearchTextBox.TabIndex = 3;
            this.treeSearchTextBox.Text = "";
            this.treeSearchTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.treeSearchTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.treeSearchTextBox.Enter += new System.EventHandler(this.treeSearchTextBox_Enter);
            this.treeSearchTextBox.Leave += new System.EventHandler(this.treeSearchTextBox_Leave);
            this.treeSearchTextBox.TextChanged += new System.EventHandler(this.treeSearchTextBox_TextChanged);
            // 
            // ApiTree
            // 
            this.ApiTree.ContextMenuStrip = this.treeContextMenu;
            this.ApiTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ApiTree.Name = "ApiTree";
            this.ApiTree.TabIndex = 2;
            this.ApiTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ApiTree_AfterSelect);
            this.ApiTree.Click += new System.EventHandler(this.ApiTree_Click);
            this.ApiTree.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ApiTree_MouseClick);
            this.ApiTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ApiTree_MouseUp);
            // 
            // CallTree
            // 
            this.CallTree.ContextMenuStrip = this.treeContextMenu;
            this.CallTree.ImageList = this.treeIconList;
            this.CallTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CallTree.Name = "CallTree";
            this.CallTree.TabIndex = 0;
            this.CallTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.CallTree_AfterSelect);
            this.CallTree.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CallTree_MouseClick);
            this.CallTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CallTree_MouseUp);
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.logTab);
            this.mainTabControl.Controls.Add(this.performanceTab);
            this.mainTabControl.Controls.Add(this.logDetailTab);
            this.mainTabControl.Controls.Add(this.callGraphTab);
            this.mainTabControl.Controls.Add(this.flameGraphTab);
            this.mainTabControl.Controls.Add(this.timelineTab);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(696, 523);
            this.mainTabControl.TabIndex = 1;
            // 
            // logTab
            // 
            this.logTab.Controls.Add(this.logListView);
            this.logTab.Controls.Add(this.mainStatusStrip);
            this.logTab.Location = new System.Drawing.Point(4, 25);
            this.logTab.Name = "logTab";
            this.logTab.Padding = new System.Windows.Forms.Padding(3);
            this.logTab.Size = new System.Drawing.Size(688, 494);
            this.logTab.TabIndex = 0;
            this.logTab.Text = "logTab";
            this.logTab.UseVisualStyleBackColor = true;
            // 
            // logListView
            // 
            this.logListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colLineNumber,
            this.colLogText});
            this.logListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logListView.FullRowSelect = true;
            this.logListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.logListView.Location = new System.Drawing.Point(3, 3);
            this.logListView.Name = "logListView";
            this.logListView.ShowItemToolTips = true;
            this.logListView.VirtualMode = true;
            this.logListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView1_RetrieveVirtualItem);
            this.logListView.Size = new System.Drawing.Size(682, 466);
            this.logListView.TabIndex = 3;
            this.logListView.UseCompatibleStateImageBehavior = false;
            this.logListView.View = System.Windows.Forms.View.Details;
            this.logListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.logListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp_1);
            this.logListView.Resize += new System.EventHandler(this.logListView_Resize);
            // 
            // colLineNumber
            // 
            this.colLineNumber.Text = "Line #";
            this.colLineNumber.Width = 80;
            // 
            // colLogText
            // 
            this.colLogText.Text = "Log Text";
            this.colLogText.Width = 600;
            // 
            // rawTab
            // 
            this.rawTab.Location = new System.Drawing.Point(4, 25);
            this.rawTab.Name = "rawTab";
            this.rawTab.Padding = new System.Windows.Forms.Padding(3);
            this.rawTab.Size = new System.Drawing.Size(688, 494);
            this.rawTab.TabIndex = 1;
            this.rawTab.Text = "rawTab";
            this.rawTab.UseVisualStyleBackColor = true;
            // 
            // performanceTab � Performance
            // 
            this.performanceView = new System.Windows.Forms.ListView();
            this.perfColName = new System.Windows.Forms.ColumnHeader();
            this.perfColCalls = new System.Windows.Forms.ColumnHeader();
            this.perfColFirst = new System.Windows.Forms.ColumnHeader();
            this.performanceTab.Controls.Add(this.performanceView);
            this.performanceTab.Location = new System.Drawing.Point(4, 25);
            this.performanceTab.Name = "performanceTab";
            this.performanceTab.Padding = new System.Windows.Forms.Padding(3);
            this.performanceTab.Size = new System.Drawing.Size(688, 494);
            this.performanceTab.TabIndex = 2;
            this.performanceTab.Text = "Performance";
            this.performanceTab.UseVisualStyleBackColor = true;
            this.performanceView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performanceView.FullRowSelect = true;
            this.performanceView.View = System.Windows.Forms.View.Details;
            this.perfColTotal = new System.Windows.Forms.ColumnHeader();
            this.perfColAvg = new System.Windows.Forms.ColumnHeader();
            this.perfColMin = new System.Windows.Forms.ColumnHeader();
            this.perfColMax = new System.Windows.Forms.ColumnHeader();
            this.perfColSource = new System.Windows.Forms.ColumnHeader();
            this.perfColSelf = new System.Windows.Forms.ColumnHeader();
            this.performanceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.perfColName, this.perfColCalls, this.perfColTotal,
                this.perfColAvg, this.perfColMin, this.perfColMax, this.perfColSelf, this.perfColSource });
            this.performanceView.FullRowSelect = true;
            this.performanceView.GridLines = true;
            this.perfColName.Text = "API Name";
            this.perfColName.Width = 260;
            this.perfColCalls.Text = "Calls";
            this.perfColCalls.Width = 55;
            this.perfColTotal.Text = "Total (ms)";
            this.perfColTotal.Width = 90;
            this.perfColAvg.Text = "Avg (ms)";
            this.perfColAvg.Width = 80;
            this.perfColMin.Text = "Min (ms)";
            this.perfColMin.Width = 80;
            this.perfColMax.Text = "Max (ms)";
            this.perfColMax.Width = 80;
            this.perfColSource.Text = "Source File";
            this.perfColSource.Width = 200;
            this.perfColSelf.Text = "Self (ms)";
            this.perfColSelf.Width = 80;
            // 
            // logDetailTab � Log Details
            // 
            this.logDetailBox = new System.Windows.Forms.RichTextBox();
            this.logDetailTab.Controls.Add(this.logDetailBox);
            this.logDetailTab.Location = new System.Drawing.Point(4, 25);
            this.logDetailTab.Name = "logDetailTab";
            this.logDetailTab.Padding = new System.Windows.Forms.Padding(3);
            this.logDetailTab.Size = new System.Drawing.Size(688, 494);
            this.logDetailTab.TabIndex = 3;
            this.logDetailTab.Text = "Log Details";
            this.logDetailTab.UseVisualStyleBackColor = true;
            this.logDetailBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logDetailBox.ReadOnly = true;
            this.logDetailBox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.logDetailBox.WordWrap = true;
            this.logDetailBox.BackColor = System.Drawing.SystemColors.Window;
            // 
            // callGraphTab � Call Graph
            // 
            this.callGraphPanel = new Cad3PLogBrowser.CallGraphPanel();
            this.callGraphResetButton = new System.Windows.Forms.Button();
            this.callGraphExportButton = new System.Windows.Forms.Button();
            this.callGraphTab.Controls.Add(this.callGraphPanel);
            this.callGraphTab.Controls.Add(this.callGraphResetButton);
            this.callGraphTab.Controls.Add(this.callGraphExportButton);
            this.callGraphTab.Location = new System.Drawing.Point(4, 25);
            this.callGraphTab.Name = "callGraphTab";
            this.callGraphTab.Padding = new System.Windows.Forms.Padding(3);
            this.callGraphTab.Size = new System.Drawing.Size(688, 494);
            this.callGraphTab.TabIndex = 4;
            this.callGraphTab.Text = "Call Graph";
            this.callGraphTab.UseVisualStyleBackColor = true;
            // 
            // flameGraphTab
            // 
            this.flameGraphPanel = new Cad3PLogBrowser.Managers.FlameGraphPanel();
            this.flameGraphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flameGraphTab.Controls.Add(this.flameGraphPanel);
            this.flameGraphTab.Location = new System.Drawing.Point(4, 25);
            this.flameGraphTab.Name = "flameGraphTab";
            this.flameGraphTab.Padding = new System.Windows.Forms.Padding(3);
            this.flameGraphTab.Size = new System.Drawing.Size(688, 494);
            this.flameGraphTab.TabIndex = 5;
            this.flameGraphTab.Text = "Flame Graph";
            this.flameGraphTab.UseVisualStyleBackColor = true;
            // 
            // timelineTab
            // 
            this.timelinePanel = new Cad3PLogBrowser.Managers.TimelinePanel();
            this.timelinePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timelineTab.Controls.Add(this.timelinePanel);
            this.timelineTab.Location = new System.Drawing.Point(4, 25);
            this.timelineTab.Name = "timelineTab";
            this.timelineTab.Padding = new System.Windows.Forms.Padding(3);
            this.timelineTab.Size = new System.Drawing.Size(688, 494);
            this.timelineTab.TabIndex = 6;
            this.timelineTab.Text = "Timeline";
            this.timelineTab.UseVisualStyleBackColor = true;
            this.callGraphResetButton.Text = "Reset View";
            this.callGraphResetButton.Location = new System.Drawing.Point(6, 6);
            this.callGraphResetButton.Size = new System.Drawing.Size(85, 26);
            this.callGraphResetButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.callGraphResetButton.Click += new System.EventHandler(this.callGraphResetButton_Click);
            this.callGraphExportButton.Text = "Export PNG...";
            this.callGraphExportButton.Location = new System.Drawing.Point(97, 6);
            this.callGraphExportButton.Size = new System.Drawing.Size(100, 26);
            this.callGraphExportButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.callGraphExportButton.Click += new System.EventHandler(this.callGraphExportButton_Click);
            this.callGraphPanel.Location = new System.Drawing.Point(3, 38);
            this.callGraphPanel.Size = new System.Drawing.Size(682, 453);
            this.callGraphPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenButton,
            this.SaveButton,
            this.SaveToXLSButton,
            this.RefreshButton,
            this.separatorAfterRefresh,
            this.CopyButton,
            this.FindButton,
            this.FindNextButton,
            this.FilterButton,
            this.separatorAfterFilter,
            this.ExpandAllButton,
            this.CollapseAllButton,
            this.JumpToMatchingButton,
            this.separatorAfterTreeOps,
            this.prevErrorButton,
            this.nextErrorButton,
            this.prevWarningButton,
            this.nextWarningButton,
            this.separatorAfterNavigation,
            this.CallTreeButton,
            this.ApiTreeButton,
            this.separatorAfterTreeView,
            this.SettingsButton,
            this.separatorAfterSettings,
            this.ShowHelpButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 26);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainToolStrip.Size = new System.Drawing.Size(987, 27);
            this.mainToolStrip.TabIndex = 4;
            this.mainToolStrip.Text = "mainToolStrip";
            // 
            // OpenButton
            // 
            this.OpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenButton.Image = global::Cad3PLogBrowser.Properties.Resources.open;
            this.OpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(23, 22);
            this.OpenButton.Text = "Open";
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveButton.Image = global::Cad3PLogBrowser.Properties.Resources.save;
            this.SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(23, 22);
            this.SaveButton.Text = "Save Selected";
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // SaveToXLSButton
            // 
            this.SaveToXLSButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveToXLSButton.Image = global::Cad3PLogBrowser.Properties.Resources.save;
            this.SaveToXLSButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveToXLSButton.Name = "SaveToXLSButton";
            this.SaveToXLSButton.Size = new System.Drawing.Size(23, 22);
            this.SaveToXLSButton.Text = "Save to XLS";
            this.SaveToXLSButton.Click += new System.EventHandler(this.exportFilteredLogsMenuItem_Click);
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = global::Cad3PLogBrowser.Properties.Resources.refresh;
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.Text = "Reload from Disk";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // separatorAfterRefresh
            // 
            this.separatorAfterRefresh.Name = "separatorAfterRefresh";
            this.separatorAfterRefresh.Size = new System.Drawing.Size(6, 25);
            // 
            // CopyButton
            // 
            this.CopyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CopyButton.Image = global::Cad3PLogBrowser.Properties.Resources.copy;
            this.CopyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(23, 22);
            this.CopyButton.Text = "Copy";
            this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // FindButton
            // 
            this.FindButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FindButton.Image = global::Cad3PLogBrowser.Properties.Resources.find;
            this.FindButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(23, 22);
            this.FindButton.Text = "Find";
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // FindNextButton
            // 
            this.FindNextButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FindNextButton.Image = global::Cad3PLogBrowser.Properties.Resources.find;
            this.FindNextButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FindNextButton.Name = "FindNextButton";
            this.FindNextButton.Size = new System.Drawing.Size(23, 22);
            this.FindNextButton.Text = "Find Next (F3)";
            this.FindNextButton.Click += new System.EventHandler(this.FindNextButton_Click);
            // 
            // FilterButton
            // 
            this.FilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterButton.Image = global::Cad3PLogBrowser.Properties.Resources.filter;
            this.FilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterButton.Name = "FilterButton";
            this.FilterButton.Size = new System.Drawing.Size(23, 22);
            this.FilterButton.Text = "Filter";
            this.FilterButton.Click += new System.EventHandler(this.FilterButton_Click);
            // 
            // separatorAfterFilter
            // 
            this.separatorAfterFilter.Name = "separatorAfterFilter";
            this.separatorAfterFilter.Size = new System.Drawing.Size(6, 25);
            // 
            // ExpandAllButton
            // 
            this.ExpandAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ExpandAllButton.Name = "ExpandAllButton";
            this.ExpandAllButton.Size = new System.Drawing.Size(70, 22);
            this.ExpandAllButton.Text = "Expand All";
            this.ExpandAllButton.ToolTipText = "Expand All Nodes (Ctrl+E)";
            this.ExpandAllButton.Click += new System.EventHandler(this.expandAllMenuItem_Click);
            // 
            // CollapseAllButton
            // 
            this.CollapseAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.CollapseAllButton.Name = "CollapseAllButton";
            this.CollapseAllButton.Size = new System.Drawing.Size(75, 22);
            this.CollapseAllButton.Text = "Collapse All";
            this.CollapseAllButton.ToolTipText = "Collapse All Nodes (Ctrl+W)";
            this.CollapseAllButton.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
            // 
            // JumpToMatchingButton
            // 
            this.JumpToMatchingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.JumpToMatchingButton.Name = "JumpToMatchingButton";
            this.JumpToMatchingButton.Size = new System.Drawing.Size(95, 22);
            this.JumpToMatchingButton.Text = "Jump ?";
            this.JumpToMatchingButton.ToolTipText = "Jump to Matching Enter/Exit (Ctrl+G)";
            this.JumpToMatchingButton.Click += new System.EventHandler(this.jumpToMatchingMenuItem_Click);
            // 
            // separatorAfterTreeOps
            // 
            this.separatorAfterTreeOps.Name = "separatorAfterTreeOps";
            this.separatorAfterTreeOps.Size = new System.Drawing.Size(6, 25);
            // 
            // prevErrorButton - Feature B10
            // 
            this.prevErrorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.prevErrorButton.Name = "prevErrorButton";
            this.prevErrorButton.Size = new System.Drawing.Size(23, 22);
            this.prevErrorButton.Text = "?E";
            this.prevErrorButton.ToolTipText = "Previous Error (Shift+F8)";
            this.prevErrorButton.Click += new System.EventHandler(this.prevErrorButton_Click);
            // 
            // nextErrorButton � Feature B10
            // 
            this.nextErrorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.nextErrorButton.Name = "nextErrorButton";
            this.nextErrorButton.Size = new System.Drawing.Size(23, 22);
            this.nextErrorButton.Text = "E?";
            this.nextErrorButton.ToolTipText = "Next Error (F8)";
            this.nextErrorButton.Click += new System.EventHandler(this.nextErrorButton_Click);
            // 
            // prevWarningButton � Feature B10
            // 
            this.prevWarningButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.prevWarningButton.Name = "prevWarningButton";
            this.prevWarningButton.Size = new System.Drawing.Size(23, 22);
            this.prevWarningButton.Text = "?W";
            this.prevWarningButton.ToolTipText = "Previous Warning (Ctrl+Shift+F8)";
            this.prevWarningButton.Click += new System.EventHandler(this.prevWarningButton_Click);
            // 
            // nextWarningButton � Feature B10
            // 
            this.nextWarningButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.nextWarningButton.Name = "nextWarningButton";
            this.nextWarningButton.Size = new System.Drawing.Size(23, 22);
            this.nextWarningButton.Text = "W?";
            this.nextWarningButton.ToolTipText = "Next Warning (Ctrl+F8)";
            this.nextWarningButton.Click += new System.EventHandler(this.nextWarningButton_Click);
            // 
            // separatorAfterNavigation
            // 
            this.separatorAfterNavigation.Name = "separatorAfterNavigation";
            this.separatorAfterNavigation.Size = new System.Drawing.Size(6, 25);
            // 
            // CallTreeButton
            // 
            this.SettingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SettingsButton.Image = global::Cad3PLogBrowser.Properties.Resources.settings;
            this.SettingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(23, 22);
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // separatorAfterSettings
            // 
            this.separatorAfterSettings.Name = "separatorAfterSettings";
            this.separatorAfterSettings.Size = new System.Drawing.Size(6, 25);
            // 
            // CallTreeButton
            // 
            this.CallTreeButton.Checked = true;
            this.CallTreeButton.CheckOnClick = true;
            this.CallTreeButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CallTreeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CallTreeButton.Image = global::Cad3PLogBrowser.Properties.Resources.treeview;
            this.CallTreeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CallTreeButton.Name = "CallTreeButton";
            this.CallTreeButton.Size = new System.Drawing.Size(23, 22);
            this.CallTreeButton.Text = "CallTree";
            this.CallTreeButton.ToolTipText = "Show Call Tree";
            // 
            // ApiTreeButton
            // 
            this.ApiTreeButton.CheckOnClick = true;
            this.ApiTreeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ApiTreeButton.Image = global::Cad3PLogBrowser.Properties.Resources.apiview2;
            this.ApiTreeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ApiTreeButton.Name = "ApiTreeButton";
            this.ApiTreeButton.Size = new System.Drawing.Size(23, 22);
            this.ApiTreeButton.Text = "ApiList";
            this.ApiTreeButton.ToolTipText = "Show API Tree";
            // 
            // separatorAfterTreeView
            // 
            this.separatorAfterTreeView.Name = "separatorAfterTreeView";
            this.separatorAfterTreeView.Size = new System.Drawing.Size(6, 25);
            // 
            // SettingsButton
            // 
            this.SettingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SettingsButton.Image = global::Cad3PLogBrowser.Properties.Resources.settings;
            this.SettingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(23, 22);
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.ToolTipText = "Settings (Ctrl+Shift+S)";
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // separatorAfterSettings
            // 
            this.separatorAfterSettings.Name = "separatorAfterSettings";
            this.separatorAfterSettings.Size = new System.Drawing.Size(6, 25);
            // 
            // ShowHelpButton
            // 
            this.ShowHelpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowHelpButton.Image = global::Cad3PLogBrowser.Properties.Resources.help;
            this.ShowHelpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowHelpButton.Name = "ShowHelpButton";
            this.ShowHelpButton.Size = new System.Drawing.Size(23, 22);
            this.ShowHelpButton.Text = "Help";
            this.ShowHelpButton.ToolTipText = "Help (F1)";
            this.ShowHelpButton.Click += new System.EventHandler(this.viewHelpMenuItem_Click);
            // 
            // openLogFileDialog
            // 
            this.openLogFileDialog.Filter = "log files (*.log) | *.log |log files (*.log.*)|*.log.*|All files (*.*)|*.*";
            this.openLogFileDialog.FilterIndex = 2;
            // 
            // saveLogFileDialog
            // 
            this.saveLogFileDialog.Filter = "log files (*.log.*)|*.log.*|All files (*.*)|*.*";
            // 
            // logWatcher
            // 
            this.logWatcher.EnableRaisingEvents = true;
            this.logWatcher.NotifyFilter = System.IO.NotifyFilters.Attributes;
            this.logWatcher.SynchronizingObject = this;
            this.logWatcher.Changed += new System.IO.FileSystemEventHandler(this.logWatcher_Changed);
            // 
            // logContextMenu
            // 
            this.logContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextCopyMenuItem,
            this.contextCopyWithHeadersMenuItem,
            this.contextSeparator1,
            this.contextFindMenuItem,
            this.contextFilterMenuItem,
            this.contextSeparator2,
            this.contextExpandAllMenuItem,
            this.contextCollapseAllMenuItem,
            this.contextJumpToMatchingMenuItem,
            this.contextSeparator3,
            this.contextRefreshMenuItem});
            this.logContextMenu.Name = "logContextMenu";
            this.logContextMenu.Size = new System.Drawing.Size(280, 220);
            // 
            // contextCopyMenuItem
            // 
            this.contextCopyMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.copy;
            this.contextCopyMenuItem.Name = "contextCopyMenuItem";
            this.contextCopyMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            this.contextCopyMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextCopyMenuItem.Text = "&Copy";
            this.contextCopyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // contextCopyWithHeadersMenuItem
            // 
            this.contextCopyWithHeadersMenuItem.Name = "contextCopyWithHeadersMenuItem";
            this.contextCopyWithHeadersMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextCopyWithHeadersMenuItem.Text = "Copy with &Headers";
            this.contextCopyWithHeadersMenuItem.Click += new System.EventHandler(this.contextCopyWithHeadersMenuItem_Click);
            // 
            // contextSeparator1
            // 
            this.contextSeparator1.Name = "contextSeparator1";
            this.contextSeparator1.Size = new System.Drawing.Size(277, 6);
            // 
            // contextFindMenuItem
            // 
            this.contextFindMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.find;
            this.contextFindMenuItem.Name = "contextFindMenuItem";
            this.contextFindMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
            this.contextFindMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextFindMenuItem.Text = "&Find...";
            this.contextFindMenuItem.Click += new System.EventHandler(this.findMenuItem_Click);
            // 
            // contextFilterMenuItem
            // 
            this.contextFilterMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.filter;
            this.contextFilterMenuItem.Name = "contextFilterMenuItem";
            this.contextFilterMenuItem.ShortcutKeyDisplayString = "Ctrl+I";
            this.contextFilterMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextFilterMenuItem.Text = "Fil&ter...";
            this.contextFilterMenuItem.Click += new System.EventHandler(this.filterMenuItem_Click);
            // 
            // contextSeparator2
            // 
            this.contextSeparator2.Name = "contextSeparator2";
            this.contextSeparator2.Size = new System.Drawing.Size(277, 6);
            // 
            // contextExpandAllMenuItem
            // 
            this.contextExpandAllMenuItem.Name = "contextExpandAllMenuItem";
            this.contextExpandAllMenuItem.ShortcutKeyDisplayString = "Ctrl+E";
            this.contextExpandAllMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextExpandAllMenuItem.Text = "&Expand All";
            this.contextExpandAllMenuItem.Click += new System.EventHandler(this.expandAllMenuItem_Click);
            // 
            // contextCollapseAllMenuItem
            // 
            this.contextCollapseAllMenuItem.Name = "contextCollapseAllMenuItem";
            this.contextCollapseAllMenuItem.ShortcutKeyDisplayString = "Ctrl+W";
            this.contextCollapseAllMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextCollapseAllMenuItem.Text = "&Collapse All";
            this.contextCollapseAllMenuItem.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
            // 
            // contextJumpToMatchingMenuItem
            // 
            this.contextJumpToMatchingMenuItem.Name = "contextJumpToMatchingMenuItem";
            this.contextJumpToMatchingMenuItem.ShortcutKeyDisplayString = "Ctrl+G";
            this.contextJumpToMatchingMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextJumpToMatchingMenuItem.Text = "&Jump to Matching ENTER/EXIT";
            this.contextJumpToMatchingMenuItem.Click += new System.EventHandler(this.jumpToMatchingMenuItem_Click);
            // 
            // contextSeparator3
            // 
            this.contextSeparator3.Name = "contextSeparator3";
            this.contextSeparator3.Size = new System.Drawing.Size(277, 6);
            // 
            // contextRefreshMenuItem
            // 
            this.contextRefreshMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.refresh;
            this.contextRefreshMenuItem.Name = "contextRefreshMenuItem";
            this.contextRefreshMenuItem.ShortcutKeyDisplayString = "F5";
            this.contextRefreshMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextRefreshMenuItem.Text = "&Refresh";
            this.contextRefreshMenuItem.Click += new System.EventHandler(this.refreshMenuItem_Click);
            // 
            // treeContextMenu
            // 
            this.treeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.treeContextCopyNodeNameMenuItem,
            this.treeContextCopySubtreeMenuItem,
            this.treeContextSeparator1,
            this.treeContextExpandAllMenuItem,
            this.treeContextCollapseAllMenuItem,
            this.treeContextJumpToMatchingMenuItem,
            this.treeContextSeparator2,
            this.treeContextSaveBranchMenuItem,
            this.treeContextExportBranchCsvMenuItem,
            this.treeContextSearchInGrokMenuItem,
            this.treeContextShowInOtherTreeMenuItem});
            this.treeContextMenu.Name = "treeContextMenu";
            this.treeContextMenu.Size = new System.Drawing.Size(280, 200);
            // 
            // treeContextCopyNodeNameMenuItem
            // 
            this.treeContextCopyNodeNameMenuItem.Name = "treeContextCopyNodeNameMenuItem";
            this.treeContextCopyNodeNameMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextCopyNodeNameMenuItem.Text = "Copy &Node Name";
            this.treeContextCopyNodeNameMenuItem.Click += new System.EventHandler(this.treeContextCopyNodeNameMenuItem_Click);
            // 
            // treeContextCopySubtreeMenuItem
            // 
            this.treeContextCopySubtreeMenuItem.Name = "treeContextCopySubtreeMenuItem";
            this.treeContextCopySubtreeMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextCopySubtreeMenuItem.Text = "Copy &Subtree as Text";
            this.treeContextCopySubtreeMenuItem.Click += new System.EventHandler(this.treeContextCopySubtreeMenuItem_Click);
            // 
            // treeContextSeparator1
            // 
            this.treeContextSeparator1.Name = "treeContextSeparator1";
            this.treeContextSeparator1.Size = new System.Drawing.Size(277, 6);
            // 
            // treeContextExpandAllMenuItem
            // 
            this.treeContextExpandAllMenuItem.Name = "treeContextExpandAllMenuItem";
            this.treeContextExpandAllMenuItem.ShortcutKeyDisplayString = "Ctrl+E";
            this.treeContextExpandAllMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextExpandAllMenuItem.Text = "&Expand All";
            this.treeContextExpandAllMenuItem.Click += new System.EventHandler(this.expandAllMenuItem_Click);
            // 
            // treeContextCollapseAllMenuItem
            // 
            this.treeContextCollapseAllMenuItem.Name = "treeContextCollapseAllMenuItem";
            this.treeContextCollapseAllMenuItem.ShortcutKeyDisplayString = "Ctrl+W";
            this.treeContextCollapseAllMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextCollapseAllMenuItem.Text = "&Collapse All";
            this.treeContextCollapseAllMenuItem.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
            // 
            // treeContextJumpToMatchingMenuItem
            // 
            this.treeContextJumpToMatchingMenuItem.Name = "treeContextJumpToMatchingMenuItem";
            this.treeContextJumpToMatchingMenuItem.ShortcutKeyDisplayString = "Ctrl+G";
            this.treeContextJumpToMatchingMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextJumpToMatchingMenuItem.Text = "&Jump to Matching ENTER/EXIT";
            this.treeContextJumpToMatchingMenuItem.Click += new System.EventHandler(this.jumpToMatchingMenuItem_Click);
            // 
            // treeContextSeparator2
            // 
            this.treeContextSeparator2.Name = "treeContextSeparator2";
            this.treeContextSeparator2.Size = new System.Drawing.Size(277, 6);
            // 
            // treeContextSaveBranchMenuItem
            // 
            this.treeContextSaveBranchMenuItem.Name = "treeContextSaveBranchMenuItem";
            this.treeContextSaveBranchMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextSaveBranchMenuItem.Text = "&Save Branch to File...";
            this.treeContextSaveBranchMenuItem.Click += new System.EventHandler(this.treeContextSaveBranchMenuItem_Click);
            // 
            // treeContextExportBranchCsvMenuItem
            // 
            this.treeContextExportBranchCsvMenuItem.Name = "treeContextExportBranchCsvMenuItem";
            this.treeContextExportBranchCsvMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextExportBranchCsvMenuItem.Text = "E&xport Branch to CSV...";
            this.treeContextExportBranchCsvMenuItem.Click += new System.EventHandler(this.treeContextExportBranchCsvMenuItem_Click);
            // 
            // treeContextSearchInGrokMenuItem
            // 
            this.treeContextSearchInGrokMenuItem.Name = "treeContextSearchInGrokMenuItem";
            this.treeContextSearchInGrokMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextSearchInGrokMenuItem.Text = "Search in &Grok";
            this.treeContextSearchInGrokMenuItem.Click += new System.EventHandler(this.treeContextSearchInGrokMenuItem_Click);
            // 
            // treeContextShowInOtherTreeMenuItem
            // 
            this.treeContextShowInOtherTreeMenuItem.Name = "treeContextShowInOtherTreeMenuItem";
            this.treeContextShowInOtherTreeMenuItem.Size = new System.Drawing.Size(280, 22);
            this.treeContextShowInOtherTreeMenuItem.Text = "Show in &API Tree";
            this.treeContextShowInOtherTreeMenuItem.Click += new System.EventHandler(this.treeContextShowInOtherTreeMenuItem_Click);
            // 
            // contextFilterMenuItem
            // 
            this.contextFilterMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.filter;
            this.contextFilterMenuItem.Name = "contextFilterMenuItem";
            this.contextFilterMenuItem.ShortcutKeyDisplayString = "Ctrl+I";
            this.contextFilterMenuItem.Size = new System.Drawing.Size(280, 22);
            this.contextFilterMenuItem.Text = "Fil&ter...";
            this.contextFilterMenuItem.Click += new System.EventHandler(this.filterMenuItem_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 574);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "MainForm";
            this.Text = "WWGM 3P Log Browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.ResizeBegin += new System.EventHandler(this.MainForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.mainTabControl.ResumeLayout(false);
            this.logTab.ResumeLayout(false);
            this.logTab.PerformLayout();
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logWatcher)).EndInit();
            this.logContextMenu.ResumeLayout(false);
            this.treeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFilteredLogsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportPerformanceMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTreeJsonMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTreeXmlMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTimelineMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFlameGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.ToolStripMenuItem reloadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeLogsMenuItem;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.TreeView CallTree;
        private System.Windows.Forms.TextBox treeSearchTextBox;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage flameGraphTab;
        private System.Windows.Forms.TabPage timelineTab;
        private Managers.FlameGraphPanel flameGraphPanel;
        private Managers.TimelinePanel timelinePanel;
        private System.Windows.Forms.TabPage logTab;
        private System.Windows.Forms.TabPage rawTab;
        private System.Windows.Forms.TabPage performanceTab;
        private System.Windows.Forms.TabPage logDetailTab;
        private System.Windows.Forms.TabPage callGraphTab;
        private Cad3PLogBrowser.CallGraphPanel callGraphPanel;
        private System.Windows.Forms.Button callGraphResetButton;
        private System.Windows.Forms.Button callGraphExportButton;
        private System.Windows.Forms.ListView performanceView;
        private System.Windows.Forms.ColumnHeader perfColName;
        private System.Windows.Forms.ColumnHeader perfColCalls;
        private System.Windows.Forms.ColumnHeader perfColFirst;
        private System.Windows.Forms.ColumnHeader perfColTotal;
        private System.Windows.Forms.ColumnHeader perfColAvg;
        private System.Windows.Forms.ColumnHeader perfColMin;
        private System.Windows.Forms.ColumnHeader perfColMax;
        private System.Windows.Forms.ColumnHeader perfColSelf;
        private System.Windows.Forms.ColumnHeader perfColSource;
        private System.Windows.Forms.ImageList treeIconList;
        private System.Windows.Forms.RichTextBox logDetailBox;
        private System.Windows.Forms.ToolStripStatusLabel StatusFileName;
        private System.Windows.Forms.ToolStripStatusLabel StatusLineCount;
        private System.Windows.Forms.ToolStripStatusLabel StatusSelection;
        private System.Windows.Forms.ToolStripMenuItem editMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findAllMenuItem;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton OpenButton;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.OpenFileDialog openLogFileDialog;
        private System.Windows.Forms.SaveFileDialog saveLogFileDialog;
        private System.Windows.Forms.FontDialog logFontDialog;
        private System.IO.FileSystemWatcher logWatcher;
        private System.Windows.Forms.Timer fileWatchTimer;
        private System.Windows.Forms.ToolStripStatusLabel FileStatus;
        private System.Windows.Forms.TreeView ApiTree;
        private System.Windows.Forms.ToolStripButton RefreshButton;
        private System.Windows.Forms.ToolStripSeparator separatorAfterRefresh;
        private System.Windows.Forms.ToolStripButton CopyButton;
        private System.Windows.Forms.ToolStripButton FindButton;
        private System.Windows.Forms.ToolStripSeparator separatorAfterFilter;
        private System.Windows.Forms.ToolStripButton prevErrorButton;
        private System.Windows.Forms.ToolStripButton nextErrorButton;
        private System.Windows.Forms.ToolStripButton prevWarningButton;
        private System.Windows.Forms.ToolStripButton nextWarningButton;
        private System.Windows.Forms.ToolStripSeparator separatorAfterNavigation;
        private System.Windows.Forms.ToolStripButton SettingsButton;
        private System.Windows.Forms.ToolStripMenuItem filterMenuItem;
        private System.Windows.Forms.ToolStripButton FilterButton;
        private System.Windows.Forms.ToolStripSeparator separatorAfterSettings;
        private System.Windows.Forms.ToolStripButton CallTreeButton;
        private System.Windows.Forms.ToolStripButton ApiTreeButton;
        private System.Windows.Forms.ToolStripSeparator separatorAfterTreeView;
        private System.Windows.Forms.ToolStripButton ShowHelpButton;
        private System.Windows.Forms.ToolStripButton SaveToXLSButton;
        private System.Windows.Forms.ToolStripButton FindNextButton;
        private System.Windows.Forms.ToolStripButton ExpandAllButton;
        private System.Windows.Forms.ToolStripButton CollapseAllButton;
        private System.Windows.Forms.ToolStripButton JumpToMatchingButton;
        private System.Windows.Forms.ToolStripSeparator separatorAfterTreeOps;
        private System.Windows.Forms.ToolStripMenuItem viewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCallTreeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showApiTreeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tabsMenuItem;
        private System.Windows.Forms.ToolStripSeparator viewSeparator1;
        private System.Windows.Forms.ToolStripMenuItem selectFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showToolbarMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab2MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab3MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab4MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFlameGraphTabMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTimelineTabMenuItem;
        private System.Windows.Forms.ToolStripSeparator fileSeparatorAfterSave;
        private System.Windows.Forms.ToolStripSeparator fileSeparatorBeforeExit;
        private System.Windows.Forms.ToolStripSeparator editSeparatorAfterFilter;
        private System.Windows.Forms.ToolStripSeparator editSeparatorAfterCollapse;
        private System.Windows.Forms.ToolStripMenuItem expandAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpToMatchingMenuItem;
        private System.Windows.Forms.ToolStripSeparator editSeparatorBeforeJumpToLine;
        private System.Windows.Forms.ToolStripMenuItem jumpToLineMenuItem;
        private System.Windows.Forms.ToolStripSeparator editSeparatorBeforeBookmarks;
        private System.Windows.Forms.ToolStripMenuItem toggleBookmarkMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextBookmarkMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previousBookmarkMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBookmarksMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearBookmarksMenuItem;
        private System.Windows.Forms.ToolStripSeparator editSeparatorAfterBookmarks;
        private System.Windows.Forms.ToolStripMenuItem copyWithHeadersMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearFilterMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keyboardShortcutsMenuItem;
        private System.Windows.Forms.ToolStripSeparator helpSeparator1;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportErrorsMenuItem;
        private System.Windows.Forms.ListView logListView;
        private System.Windows.Forms.ContextMenuStrip logContextMenu;
        private System.Windows.Forms.ToolStripMenuItem contextCopyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextCopyWithHeadersMenuItem;
        private System.Windows.Forms.ToolStripSeparator contextSeparator1;
        private System.Windows.Forms.ToolStripMenuItem contextFindMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextFilterMenuItem;
        private System.Windows.Forms.ToolStripSeparator contextSeparator2;
        private System.Windows.Forms.ToolStripMenuItem contextExpandAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextCollapseAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextJumpToMatchingMenuItem;
        private System.Windows.Forms.ToolStripSeparator contextSeparator3;
        private System.Windows.Forms.ToolStripMenuItem contextRefreshMenuItem;
        private System.Windows.Forms.ContextMenuStrip treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem treeContextCopyNodeNameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeContextCopySubtreeMenuItem;
        private System.Windows.Forms.ToolStripSeparator treeContextSeparator1;
        private System.Windows.Forms.ToolStripMenuItem treeContextExpandAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeContextCollapseAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeContextJumpToMatchingMenuItem;
        private System.Windows.Forms.ToolStripSeparator treeContextSeparator2;
        private System.Windows.Forms.ToolStripMenuItem treeContextSaveBranchMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeContextExportBranchCsvMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeContextSearchInGrokMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeContextShowInOtherTreeMenuItem;
        private System.Windows.Forms.ColumnHeader colLineNumber;
        private System.Windows.Forms.ColumnHeader colLogText;
        private System.Windows.Forms.ToolStripProgressBar FileLoadProgress;
    }
}

