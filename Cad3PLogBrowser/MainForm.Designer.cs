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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.findMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.filterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCallTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showApiListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tabsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTab4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.FileStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.FileLoadProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.StatusFileName = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLineCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusSelection = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ApiTree = new System.Windows.Forms.TreeView();
            this.CallTree = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.OpenButton = new System.Windows.Forms.ToolStripButton();
            this.SaveButton = new System.Windows.Forms.ToolStripButton();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripSeparator();
            this.CopyButton = new System.Windows.Forms.ToolStripButton();
            this.FindButton = new System.Windows.Forms.ToolStripButton();
            this.FilterButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SettingsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.CallTreeButton = new System.Windows.Forms.ToolStripButton();
            this.ApiTreeButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton11 = new System.Windows.Forms.ToolStripSeparator();
            this.HideTabsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton13 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowHelpButton = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.logWatcher = new System.IO.FileSystemWatcher();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logWatcher)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.editMenuItem,
            this.optionsMenuItem,
            this.viewMenuItem,
            this.helpMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(987, 26);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenuItem,
            this.saveAsMenuItem,
            this.toolStripSeparator5,
            this.refreshMenuItem,
            this.reloadMenuItem,
            this.toolStripSeparator4,
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
            this.saveAsMenuItem.Text = "&Save As...";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(257, 6);
            // 
            // refreshMenuItem
            // 
            this.refreshMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.refresh;
            this.refreshMenuItem.Name = "refreshMenuItem";
            this.refreshMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshMenuItem.Size = new System.Drawing.Size(260, 22);
            this.refreshMenuItem.Text = "&Refresh";
            this.refreshMenuItem.Click += new System.EventHandler(this.refreshMenuItem_Click);
            // 
            // reloadMenuItem
            // 
            this.reloadMenuItem.Name = "reloadMenuItem";
            this.reloadMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.reloadMenuItem.Size = new System.Drawing.Size(260, 22);
            this.reloadMenuItem.Text = "R&eload File from Disk";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(257, 6);
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
            this.toolStripSeparator6,
            this.findMenuItem,
            this.findNextMenuItem,
            this.toolStripSeparator7,
            this.filterMenuItem});
            this.editMenuItem.Name = "editMenuItem";
            this.editMenuItem.Size = new System.Drawing.Size(43, 22);
            this.editMenuItem.Text = "&Edit";
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.copy;
            this.copyMenuItem.Name = "copyMenuItem";
            this.copyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyMenuItem.Size = new System.Drawing.Size(158, 22);
            this.copyMenuItem.Text = "&Copy";
            this.copyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(155, 6);
            // 
            // findMenuItem
            // 
            this.findMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.find;
            this.findMenuItem.Name = "findMenuItem";
            this.findMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findMenuItem.Size = new System.Drawing.Size(158, 22);
            this.findMenuItem.Text = "&Find";
            this.findMenuItem.Click += new System.EventHandler(this.findMenuItem_Click);
            // 
            // findNextMenuItem
            // 
            this.findNextMenuItem.Name = "findNextMenuItem";
            this.findNextMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findNextMenuItem.Size = new System.Drawing.Size(158, 22);
            this.findNextMenuItem.Text = "Find&Next";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(155, 6);
            // 
            // filterMenuItem
            // 
            this.filterMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.filter;
            this.filterMenuItem.Name = "filterMenuItem";
            this.filterMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.filterMenuItem.Size = new System.Drawing.Size(158, 22);
            this.filterMenuItem.Text = "Fil&ter";
            this.filterMenuItem.Click += new System.EventHandler(this.filterMenuItem_Click);
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
            this.settingsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.settingsMenuItem.Size = new System.Drawing.Size(175, 22);
            this.settingsMenuItem.Text = "&Settings";
            this.settingsMenuItem.Click += new System.EventHandler(this.settingsMenuItem_Click);
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showCallTreeMenuItem,
            this.showApiListMenuItem,
            this.toolStripSeparator8,
            this.tabsMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(49, 22);
            this.viewMenuItem.Text = "‪&View";
            // 
            // showCallTreeMenuItem
            // 
            this.showCallTreeMenuItem.Checked = true;
            this.showCallTreeMenuItem.CheckOnClick = true;
            this.showCallTreeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCallTreeMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.treeview;
            this.showCallTreeMenuItem.Name = "showCallTreeMenuItem";
            this.showCallTreeMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.showCallTreeMenuItem.Size = new System.Drawing.Size(178, 22);
            this.showCallTreeMenuItem.Text = "&CallTree";
            // 
            // showApiListMenuItem
            // 
            this.showApiListMenuItem.CheckOnClick = true;
            this.showApiListMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.apiview2;
            this.showApiListMenuItem.Name = "showApiListMenuItem";
            this.showApiListMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.showApiListMenuItem.Size = new System.Drawing.Size(178, 22);
            this.showApiListMenuItem.Text = "&ApiList";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(175, 6);
            // 
            // tabsMenuItem
            // 
            this.tabsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideAllMenuItem,
            this.showTab1MenuItem,
            this.showTab2MenuItem,
            this.showTab3MenuItem,
            this.showTab4MenuItem});
            this.tabsMenuItem.Name = "tabsMenuItem";
            this.tabsMenuItem.Size = new System.Drawing.Size(178, 22);
            this.tabsMenuItem.Text = "&Tabs";
            // 
            // hideAllMenuItem
            // 
            this.hideAllMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.tabs;
            this.hideAllMenuItem.Name = "hideAllMenuItem";
            this.hideAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.hideAllMenuItem.Size = new System.Drawing.Size(167, 22);
            this.hideAllMenuItem.Text = "&HideAll";
            // 
            // showTab1MenuItem
            // 
            this.showTab1MenuItem.Name = "showTab1MenuItem";
            this.showTab1MenuItem.Size = new System.Drawing.Size(167, 22);
            this.showTab1MenuItem.Text = "Show Tab&1";
            // 
            // showTab2MenuItem
            // 
            this.showTab2MenuItem.Name = "showTab2MenuItem";
            this.showTab2MenuItem.Size = new System.Drawing.Size(167, 22);
            this.showTab2MenuItem.Text = "Show Tab&2";
            // 
            // showTab3MenuItem
            // 
            this.showTab3MenuItem.Name = "showTab3MenuItem";
            this.showTab3MenuItem.Size = new System.Drawing.Size(167, 22);
            this.showTab3MenuItem.Text = "Show Tab&3";
            // 
            // showTab4MenuItem
            // 
            this.showTab4MenuItem.Name = "showTab4MenuItem";
            this.showTab4MenuItem.Size = new System.Drawing.Size(167, 22);
            this.showTab4MenuItem.Text = "Show Tab&4";
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewHelpMenuItem,
            this.toolStripSeparator9,
            this.aboutMenuItem});
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(48, 22);
            this.helpMenuItem.Text = "&Help";
            this.helpMenuItem.Click += new System.EventHandler(this.helpMenuItem_Click);
            // 
            // viewHelpMenuItem
            // 
            this.viewHelpMenuItem.Image = global::Cad3PLogBrowser.Properties.Resources.help;
            this.viewHelpMenuItem.Name = "viewHelpMenuItem";
            this.viewHelpMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.viewHelpMenuItem.Size = new System.Drawing.Size(162, 22);
            this.viewHelpMenuItem.Text = "View &Help";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(159, 6);
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(162, 22);
            this.aboutMenuItem.Text = "&About";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileStatus,
            this.StatusFileName,
            this.StatusLineCount,
            this.StatusSelection,
            this.FileLoadProgress});
            this.statusStrip1.Location = new System.Drawing.Point(3, 469);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(682, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
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
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(0, 54);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AccessibleName = "TreePanel";
            this.splitContainer1.Panel1.Controls.Add(this.ApiTree);
            this.splitContainer1.Panel1.Controls.Add(this.CallTree);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AccessibleName = "TabPanel";
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(987, 525);
            this.splitContainer1.SplitterDistance = 285;
            this.splitContainer1.TabIndex = 3;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // ApiTree
            // 
            this.ApiTree.Location = new System.Drawing.Point(-1, 0);
            this.ApiTree.Name = "ApiTree";
            this.ApiTree.Size = new System.Drawing.Size(406, 234);
            this.ApiTree.TabIndex = 2;
            this.ApiTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ApiTree_AfterSelect);
            this.ApiTree.Click += new System.EventHandler(this.ApiTree_Click);
            this.ApiTree.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ApiTree_MouseClick);
            this.ApiTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ApiTree_MouseUp);
            // 
            // CallTree
            // 
            this.CallTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CallTree.Location = new System.Drawing.Point(-1, 333);
            this.CallTree.Name = "CallTree";
            this.CallTree.Size = new System.Drawing.Size(406, 186);
            this.CallTree.TabIndex = 0;
            this.CallTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.CallTree_AfterSelect);
            this.CallTree.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CallTree_MouseClick);
            this.CallTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CallTree_MouseUp);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(696, 523);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listView1);
            this.tabPage1.Controls.Add(this.statusStrip1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(688, 494);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(682, 466);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp_1);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 645;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(688, 494);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3 — Performance
            // 
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.performanceView = new System.Windows.Forms.ListView();
            this.perfColName = new System.Windows.Forms.ColumnHeader();
            this.perfColCalls = new System.Windows.Forms.ColumnHeader();
            this.perfColFirst = new System.Windows.Forms.ColumnHeader();
            this.tabPage3.Controls.Add(this.performanceView);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(688, 494);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Performance";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.performanceView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performanceView.FullRowSelect = true;
            this.performanceView.View = System.Windows.Forms.View.Details;
            this.performanceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.perfColName, this.perfColCalls, this.perfColFirst });
            this.perfColName.Text = "API Name";
            this.perfColName.Width = 300;
            this.perfColCalls.Text = "Call Count";
            this.perfColCalls.Width = 100;
            this.perfColFirst.Text = "First Line";
            this.perfColFirst.Width = 100;
            // 
            // tabPage4 — Log Details
            // 
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.logDetailBox = new System.Windows.Forms.RichTextBox();
            this.tabPage4.Controls.Add(this.logDetailBox);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(688, 494);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Log Details";
            this.tabPage4.UseVisualStyleBackColor = true;
            this.logDetailBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logDetailBox.ReadOnly = true;
            this.logDetailBox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.logDetailBox.WordWrap = true;
            this.logDetailBox.BackColor = System.Drawing.SystemColors.Window;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenButton,
            this.SaveButton,
            this.RefreshButton,
            this.toolStripButton4,
            this.CopyButton,
            this.FindButton,
            this.FilterButton,
            this.toolStripSeparator1,
            this.SettingsButton,
            this.toolStripSeparator2,
            this.CallTreeButton,
            this.ApiTreeButton,
            this.toolStripButton11,
            this.HideTabsButton,
            this.toolStripButton13,
            this.toolStripSeparator3,
            this.ShowHelpButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 26);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(987, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
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
            this.SaveButton.Text = "Save As";
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = global::Cad3PLogBrowser.Properties.Resources.refresh;
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(6, 25);
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // SettingsButton
            // 
            this.SettingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SettingsButton.Image = global::Cad3PLogBrowser.Properties.Resources.settings;
            this.SettingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(23, 22);
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
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
            // 
            // toolStripButton11
            // 
            this.toolStripButton11.Name = "toolStripButton11";
            this.toolStripButton11.Size = new System.Drawing.Size(6, 25);
            // 
            // HideTabsButton
            // 
            this.HideTabsButton.CheckOnClick = true;
            this.HideTabsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.HideTabsButton.Image = global::Cad3PLogBrowser.Properties.Resources.tabs;
            this.HideTabsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.HideTabsButton.Name = "HideTabsButton";
            this.HideTabsButton.Size = new System.Drawing.Size(23, 22);
            this.HideTabsButton.Text = "Hide Tabs";
            // 
            // toolStripButton13
            // 
            this.toolStripButton13.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton13.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton13.Image")));
            this.toolStripButton13.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton13.Name = "toolStripButton13";
            this.toolStripButton13.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton13.Text = "toolStripButton13";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // ShowHelpButton
            // 
            this.ShowHelpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ShowHelpButton.Image = global::Cad3PLogBrowser.Properties.Resources.help;
            this.ShowHelpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ShowHelpButton.Name = "ShowHelpButton";
            this.ShowHelpButton.Size = new System.Drawing.Size(23, 22);
            this.ShowHelpButton.Text = "toolStripButton14";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "log files (*.log) | *.log |log files (*.log.*)|*.log.*|All files (*.*)|*.*";
            this.openFileDialog1.FilterIndex = 2;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "log files (*.log.*)|*.log.*|All files (*.*)|*.*";
            // 
            // logWatcher
            // 
            this.logWatcher.EnableRaisingEvents = true;
            this.logWatcher.NotifyFilter = System.IO.NotifyFilters.Attributes;
            this.logWatcher.SynchronizingObject = this;
            this.logWatcher.Changed += new System.IO.FileSystemEventHandler(this.logWatcher_Changed);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.filterToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.filterToolStripMenuItem.Text = "Filter";
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.filterToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 574);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
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
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logWatcher)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem refreshMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView CallTree;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.ListView performanceView;
        private System.Windows.Forms.ColumnHeader perfColName;
        private System.Windows.Forms.ColumnHeader perfColCalls;
        private System.Windows.Forms.ColumnHeader perfColFirst;
        private System.Windows.Forms.RichTextBox logDetailBox;
        private System.Windows.Forms.ToolStripStatusLabel StatusFileName;
        private System.Windows.Forms.ToolStripStatusLabel StatusLineCount;
        private System.Windows.Forms.ToolStripStatusLabel StatusSelection;
        private System.Windows.Forms.ToolStripMenuItem editMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHelpMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton OpenButton;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.IO.FileSystemWatcher logWatcher;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripStatusLabel FileStatus;
        private System.Windows.Forms.TreeView ApiTree;
        private System.Windows.Forms.ToolStripButton RefreshButton;
        private System.Windows.Forms.ToolStripSeparator toolStripButton4;
        private System.Windows.Forms.ToolStripButton CopyButton;
        private System.Windows.Forms.ToolStripButton FindButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton SettingsButton;
        private System.Windows.Forms.ToolStripMenuItem filterMenuItem;
        private System.Windows.Forms.ToolStripButton FilterButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton CallTreeButton;
        private System.Windows.Forms.ToolStripButton ApiTreeButton;
        private System.Windows.Forms.ToolStripButton HideTabsButton;
        private System.Windows.Forms.ToolStripButton toolStripButton13;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton ShowHelpButton;
        private System.Windows.Forms.ToolStripMenuItem viewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCallTreeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showApiListMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tabsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab2MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab3MenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTab4MenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripButton11;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripProgressBar FileLoadProgress;
    }
}

