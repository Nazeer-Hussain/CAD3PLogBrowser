namespace Cad3PLogBrowser.UI
{
    partial class CompareLogsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseLeftMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseRightMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.compareMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.swapFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.closeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstDiffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.prevDiffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextDiffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastDiffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.browseLeftButton = new System.Windows.Forms.ToolStripButton();
            this.browseRightButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.compareButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.firstDiffButton = new System.Windows.Forms.ToolStripButton();
            this.prevDiffButton = new System.Windows.Forms.ToolStripButton();
            this.nextDiffButton = new System.Windows.Forms.ToolStripButton();
            this.lastDiffButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsButton = new System.Windows.Forms.ToolStripButton();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.topSplitContainer = new System.Windows.Forms.SplitContainer();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.leftTreeView = new System.Windows.Forms.TreeView();
            this.leftHeaderPanel = new System.Windows.Forms.Panel();
            this.leftTitleLabel = new System.Windows.Forms.Label();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.rightTreeView = new System.Windows.Forms.TreeView();
            this.rightHeaderPanel = new System.Windows.Forms.Panel();
            this.rightTitleLabel = new System.Windows.Forms.Label();
            this.differenceListView = new System.Windows.Forms.ListView();
            this.colIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLeftValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRightValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bottomTabControl = new System.Windows.Forms.TabControl();
            this.structuralDiffTabPage = new System.Windows.Forms.TabPage();
            this.performanceDiffTabPage = new System.Windows.Forms.TabPage();
            this.perfDiffSummaryLabel = new System.Windows.Forms.Label();
            this.perfDiffListView = new System.Windows.Forms.ListView();
            this.colPerfMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPerfBaseline = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPerfCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPerfDelta = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPerfStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).BeginInit();
            this.topSplitContainer.Panel1.SuspendLayout();
            this.topSplitContainer.Panel2.SuspendLayout();
            this.topSplitContainer.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.leftHeaderPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.rightHeaderPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.navigationMenuItem,
            this.viewMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1400, 24);
            this.menuStrip.TabIndex = 0;
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseLeftMenuItem,
            this.browseRightMenuItem,
            this.toolStripSeparator1,
            this.compareMenuItem,
            this.toolStripSeparator2,
            this.swapFilesMenuItem,
            this.toolStripSeparator3,
            this.closeMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "&File";
            // 
            // browseLeftMenuItem
            // 
            this.browseLeftMenuItem.Name = "browseLeftMenuItem";
            this.browseLeftMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.L)));
            this.browseLeftMenuItem.Size = new System.Drawing.Size(250, 22);
            this.browseLeftMenuItem.Text = "Browse &Left File...";
            this.browseLeftMenuItem.Click += new System.EventHandler(this.browseLeftButton_Click);
            // 
            // browseRightMenuItem
            // 
            this.browseRightMenuItem.Name = "browseRightMenuItem";
            this.browseRightMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
            this.browseRightMenuItem.Size = new System.Drawing.Size(250, 22);
            this.browseRightMenuItem.Text = "Browse &Right File...";
            this.browseRightMenuItem.Click += new System.EventHandler(this.browseRightButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(247, 6);
            // 
            // compareMenuItem
            // 
            this.compareMenuItem.Enabled = false;
            this.compareMenuItem.Name = "compareMenuItem";
            this.compareMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.compareMenuItem.Size = new System.Drawing.Size(250, 22);
            this.compareMenuItem.Text = "&Compare";
            this.compareMenuItem.Click += new System.EventHandler(this.compareButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(247, 6);
            // 
            // swapFilesMenuItem
            // 
            this.swapFilesMenuItem.Name = "swapFilesMenuItem";
            this.swapFilesMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.swapFilesMenuItem.Size = new System.Drawing.Size(250, 22);
            this.swapFilesMenuItem.Text = "S&wap Left/Right";
            this.swapFilesMenuItem.Click += new System.EventHandler(this.swapFilesMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(247, 6);
            // 
            // closeMenuItem
            // 
            this.closeMenuItem.Name = "closeMenuItem";
            this.closeMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.closeMenuItem.Size = new System.Drawing.Size(250, 22);
            this.closeMenuItem.Text = "C&lose";
            this.closeMenuItem.Click += new System.EventHandler(this.closeMenuItem_Click);
            // 
            // navigationMenuItem
            // 
            this.navigationMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.firstDiffMenuItem,
            this.prevDiffMenuItem,
            this.nextDiffMenuItem,
            this.lastDiffMenuItem});
            this.navigationMenuItem.Name = "navigationMenuItem";
            this.navigationMenuItem.Size = new System.Drawing.Size(77, 20);
            this.navigationMenuItem.Text = "&Navigation";
            // 
            // firstDiffMenuItem
            // 
            this.firstDiffMenuItem.Enabled = false;
            this.firstDiffMenuItem.Name = "firstDiffMenuItem";
            this.firstDiffMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Home)));
            this.firstDiffMenuItem.Size = new System.Drawing.Size(280, 22);
            this.firstDiffMenuItem.Text = "&First Difference";
            this.firstDiffMenuItem.Click += new System.EventHandler(this.firstDiffButton_Click);
            // 
            // prevDiffMenuItem
            // 
            this.prevDiffMenuItem.Enabled = false;
            this.prevDiffMenuItem.Name = "prevDiffMenuItem";
            this.prevDiffMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.prevDiffMenuItem.Size = new System.Drawing.Size(280, 22);
            this.prevDiffMenuItem.Text = "&Previous Difference";
            this.prevDiffMenuItem.Click += new System.EventHandler(this.prevDiffButton_Click);
            // 
            // nextDiffMenuItem
            // 
            this.nextDiffMenuItem.Enabled = false;
            this.nextDiffMenuItem.Name = "nextDiffMenuItem";
            this.nextDiffMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.nextDiffMenuItem.Size = new System.Drawing.Size(280, 22);
            this.nextDiffMenuItem.Text = "&Next Difference";
            this.nextDiffMenuItem.Click += new System.EventHandler(this.nextDiffButton_Click);
            // 
            // lastDiffMenuItem
            // 
            this.lastDiffMenuItem.Enabled = false;
            this.lastDiffMenuItem.Name = "lastDiffMenuItem";
            this.lastDiffMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.End)));
            this.lastDiffMenuItem.Size = new System.Drawing.Size(280, 22);
            this.lastDiffMenuItem.Text = "&Last Difference";
            this.lastDiffMenuItem.Click += new System.EventHandler(this.lastDiffButton_Click);
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllMenuItem,
            this.collapseAllMenuItem,
            this.optionsMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewMenuItem.Text = "&View";
            // 
            // expandAllMenuItem
            // 
            this.expandAllMenuItem.Name = "expandAllMenuItem";
            this.expandAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.expandAllMenuItem.Size = new System.Drawing.Size(196, 22);
            this.expandAllMenuItem.Text = "&Expand All";
            this.expandAllMenuItem.Click += new System.EventHandler(this.expandAllMenuItem_Click);
            // 
            // collapseAllMenuItem
            // 
            this.collapseAllMenuItem.Name = "collapseAllMenuItem";
            this.collapseAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.collapseAllMenuItem.Size = new System.Drawing.Size(196, 22);
            this.collapseAllMenuItem.Text = "&Collapse All";
            this.collapseAllMenuItem.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Size = new System.Drawing.Size(196, 22);
            this.optionsMenuItem.Text = "&Options...";
            this.optionsMenuItem.Click += new System.EventHandler(this.optionsButton_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseLeftButton,
            this.browseRightButton,
            this.toolStripSeparator4,
            this.compareButton,
            this.toolStripSeparator5,
            this.firstDiffButton,
            this.prevDiffButton,
            this.nextDiffButton,
            this.lastDiffButton,
            this.toolStripSeparator6,
            this.optionsButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(4, 2, 1, 2);
            this.toolStrip.Size = new System.Drawing.Size(1400, 27);
            this.toolStrip.TabIndex = 1;
            // 
            // browseLeftButton
            // 
            this.browseLeftButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.browseLeftButton.Name = "browseLeftButton";
            this.browseLeftButton.Size = new System.Drawing.Size(64, 20);
            this.browseLeftButton.Text = "Left File...";
            this.browseLeftButton.Click += new System.EventHandler(this.browseLeftButton_Click);
            // 
            // browseRightButton
            // 
            this.browseRightButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.browseRightButton.Name = "browseRightButton";
            this.browseRightButton.Size = new System.Drawing.Size(72, 20);
            this.browseRightButton.Text = "Right File...";
            this.browseRightButton.Click += new System.EventHandler(this.browseRightButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            // 
            // compareButton
            // 
            this.compareButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.compareButton.Enabled = false;
            this.compareButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.compareButton.Name = "compareButton";
            this.compareButton.Size = new System.Drawing.Size(63, 20);
            this.compareButton.Text = "Compare";
            this.compareButton.Click += new System.EventHandler(this.compareButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 23);
            // 
            // firstDiffButton
            // 
            this.firstDiffButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.firstDiffButton.Enabled = false;
            this.firstDiffButton.Name = "firstDiffButton";
            this.firstDiffButton.Size = new System.Drawing.Size(36, 20);
            this.firstDiffButton.Text = "|?";
            this.firstDiffButton.ToolTipText = "First Difference";
            this.firstDiffButton.Click += new System.EventHandler(this.firstDiffButton_Click);
            // 
            // prevDiffButton
            // 
            this.prevDiffButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.prevDiffButton.Enabled = false;
            this.prevDiffButton.Name = "prevDiffButton";
            this.prevDiffButton.Size = new System.Drawing.Size(23, 20);
            this.prevDiffButton.Text = "?";
            this.prevDiffButton.ToolTipText = "Previous Difference (F7)";
            this.prevDiffButton.Click += new System.EventHandler(this.prevDiffButton_Click);
            // 
            // nextDiffButton
            // 
            this.nextDiffButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.nextDiffButton.Enabled = false;
            this.nextDiffButton.Name = "nextDiffButton";
            this.nextDiffButton.Size = new System.Drawing.Size(23, 20);
            this.nextDiffButton.Text = "?";
            this.nextDiffButton.ToolTipText = "Next Difference (F8)";
            this.nextDiffButton.Click += new System.EventHandler(this.nextDiffButton_Click);
            // 
            // lastDiffButton
            // 
            this.lastDiffButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lastDiffButton.Enabled = false;
            this.lastDiffButton.Name = "lastDiffButton";
            this.lastDiffButton.Size = new System.Drawing.Size(36, 20);
            this.lastDiffButton.Text = "?|";
            this.lastDiffButton.ToolTipText = "Last Difference";
            this.lastDiffButton.Click += new System.EventHandler(this.lastDiffButton_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 23);
            // 
            // optionsButton
            // 
            this.optionsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.optionsButton.Name = "optionsButton";
            this.optionsButton.Size = new System.Drawing.Size(62, 20);
            this.optionsButton.Text = "Options...";
            this.optionsButton.Click += new System.EventHandler(this.optionsButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 778);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1400, 22);
            this.statusStrip.TabIndex = 2;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(1285, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Select files to compare";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Visible = false;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 51);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.topSplitContainer);
            // 
            // mainSplitContainer.Panel2
            //
            this.mainSplitContainer.Panel2.Controls.Add(this.bottomTabControl);
            this.mainSplitContainer.Size = new System.Drawing.Size(1400, 727);
            this.mainSplitContainer.SplitterDistance = 500;
            this.mainSplitContainer.SplitterWidth = 6;
            this.mainSplitContainer.TabIndex = 3;
            // 
            // topSplitContainer
            // 
            this.topSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.topSplitContainer.Name = "topSplitContainer";
            // 
            // topSplitContainer.Panel1
            // 
            this.topSplitContainer.Panel1.Controls.Add(this.leftPanel);
            // 
            // topSplitContainer.Panel2
            // 
            this.topSplitContainer.Panel2.Controls.Add(this.rightPanel);
            this.topSplitContainer.Size = new System.Drawing.Size(1400, 500);
            this.topSplitContainer.SplitterDistance = 695;
            this.topSplitContainer.SplitterWidth = 6;
            this.topSplitContainer.TabIndex = 0;
            // 
            // leftPanel
            // 
            this.leftPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.leftPanel.Controls.Add(this.leftTreeView);
            this.leftPanel.Controls.Add(this.leftHeaderPanel);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(695, 500);
            this.leftPanel.TabIndex = 0;
            // 
            // leftTreeView
            // 
            this.leftTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.leftTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftTreeView.Font = new System.Drawing.Font("Consolas", 9F);
            this.leftTreeView.FullRowSelect = false;
            this.leftTreeView.HideSelection = false;
            this.leftTreeView.Location = new System.Drawing.Point(0, 28);
            this.leftTreeView.Name = "leftTreeView";
            this.leftTreeView.ShowLines = true;
            this.leftTreeView.ShowPlusMinus = true;
            this.leftTreeView.ShowRootLines = true;
            this.leftTreeView.Size = new System.Drawing.Size(693, 470);
            this.leftTreeView.TabIndex = 1;
            // 
            // leftHeaderPanel
            // 
            this.leftHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.leftHeaderPanel.Controls.Add(this.leftTitleLabel);
            this.leftHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.leftHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.leftHeaderPanel.Name = "leftHeaderPanel";
            this.leftHeaderPanel.Size = new System.Drawing.Size(693, 28);
            this.leftHeaderPanel.TabIndex = 0;
            // 
            // leftTitleLabel
            // 
            this.leftTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.leftTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.leftTitleLabel.Name = "leftTitleLabel";
            this.leftTitleLabel.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.leftTitleLabel.Size = new System.Drawing.Size(693, 28);
            this.leftTitleLabel.TabIndex = 0;
            this.leftTitleLabel.Text = "Left File";
            this.leftTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rightPanel
            // 
            this.rightPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rightPanel.Controls.Add(this.rightTreeView);
            this.rightPanel.Controls.Add(this.rightHeaderPanel);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(0, 0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(699, 500);
            this.rightPanel.TabIndex = 0;
            // 
            // rightTreeView
            // 
            this.rightTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rightTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTreeView.Font = new System.Drawing.Font("Consolas", 9F);
            this.rightTreeView.FullRowSelect = false;
            this.rightTreeView.HideSelection = false;
            this.rightTreeView.Location = new System.Drawing.Point(0, 28);
            this.rightTreeView.Name = "rightTreeView";
            this.rightTreeView.ShowLines = true;
            this.rightTreeView.ShowPlusMinus = true;
            this.rightTreeView.ShowRootLines = true;
            this.rightTreeView.Size = new System.Drawing.Size(697, 470);
            this.rightTreeView.TabIndex = 1;
            // 
            // rightHeaderPanel
            // 
            this.rightHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.rightHeaderPanel.Controls.Add(this.rightTitleLabel);
            this.rightHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.rightHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.rightHeaderPanel.Name = "rightHeaderPanel";
            this.rightHeaderPanel.Size = new System.Drawing.Size(697, 28);
            this.rightHeaderPanel.TabIndex = 0;
            // 
            // rightTitleLabel
            // 
            this.rightTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.rightTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.rightTitleLabel.Name = "rightTitleLabel";
            this.rightTitleLabel.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.rightTitleLabel.Size = new System.Drawing.Size(697, 28);
            this.rightTitleLabel.TabIndex = 0;
            this.rightTitleLabel.Text = "Right File";
            this.rightTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // differenceListView
            // 
            this.differenceListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.differenceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIndex,
            this.colType,
            this.colPath,
            this.colLeftValue,
            this.colRightValue});
            this.differenceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.differenceListView.Font = new System.Drawing.Font("Consolas", 9F);
            this.differenceListView.FullRowSelect = true;
            this.differenceListView.GridLines = true;
            this.differenceListView.HideSelection = false;
            this.differenceListView.Location = new System.Drawing.Point(0, 0);
            this.differenceListView.MultiSelect = false;
            this.differenceListView.Name = "differenceListView";
            this.differenceListView.Size = new System.Drawing.Size(1400, 221);
            this.differenceListView.TabIndex = 0;
            this.differenceListView.UseCompatibleStateImageBehavior = false;
            this.differenceListView.View = System.Windows.Forms.View.Details;
            this.differenceListView.DoubleClick += new System.EventHandler(this.differenceListView_DoubleClick);
            // 
            // colIndex
            // 
            this.colIndex.Text = "#";
            this.colIndex.Width = 50;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 150;
            // 
            // colPath
            // 
            this.colPath.Text = "Path";
            this.colPath.Width = 400;
            // 
            // colLeftValue
            // 
            this.colLeftValue.Text = "Left Value";
            this.colLeftValue.Width = 400;
            // 
            // colRightValue
            // 
            this.colRightValue.Text = "Right Value";
            this.colRightValue.Width = 400;
            //
            // bottomTabControl
            //
            this.bottomTabControl.Controls.Add(this.structuralDiffTabPage);
            this.bottomTabControl.Controls.Add(this.performanceDiffTabPage);
            this.bottomTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomTabControl.Location = new System.Drawing.Point(0, 0);
            this.bottomTabControl.Name = "bottomTabControl";
            this.bottomTabControl.SelectedIndex = 0;
            this.bottomTabControl.Size = new System.Drawing.Size(1400, 221);
            this.bottomTabControl.TabIndex = 1;
            //
            // structuralDiffTabPage
            //
            this.structuralDiffTabPage.Controls.Add(this.differenceListView);
            this.structuralDiffTabPage.Location = new System.Drawing.Point(4, 22);
            this.structuralDiffTabPage.Name = "structuralDiffTabPage";
            this.structuralDiffTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.structuralDiffTabPage.Size = new System.Drawing.Size(1392, 195);
            this.structuralDiffTabPage.TabIndex = 0;
            this.structuralDiffTabPage.Text = "Structural Differences";
            this.structuralDiffTabPage.UseVisualStyleBackColor = true;
            //
            // performanceDiffTabPage
            //
            this.performanceDiffTabPage.Controls.Add(this.perfDiffListView);
            this.performanceDiffTabPage.Controls.Add(this.perfDiffSummaryLabel);
            this.performanceDiffTabPage.Location = new System.Drawing.Point(4, 22);
            this.performanceDiffTabPage.Name = "performanceDiffTabPage";
            this.performanceDiffTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.performanceDiffTabPage.Size = new System.Drawing.Size(1392, 195);
            this.performanceDiffTabPage.TabIndex = 1;
            this.performanceDiffTabPage.Text = "Performance Delta";
            this.performanceDiffTabPage.UseVisualStyleBackColor = true;
            //
            // perfDiffSummaryLabel
            //
            this.perfDiffSummaryLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.perfDiffSummaryLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.perfDiffSummaryLabel.Location = new System.Drawing.Point(3, 3);
            this.perfDiffSummaryLabel.Name = "perfDiffSummaryLabel";
            this.perfDiffSummaryLabel.Padding = new System.Windows.Forms.Padding(4);
            this.perfDiffSummaryLabel.Size = new System.Drawing.Size(1386, 40);
            this.perfDiffSummaryLabel.TabIndex = 0;
            this.perfDiffSummaryLabel.Text = "Compare two logs to see performance deltas.";
            //
            // perfDiffListView
            //
            this.perfDiffListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.perfDiffListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPerfMethod,
            this.colPerfBaseline,
            this.colPerfCurrent,
            this.colPerfDelta,
            this.colPerfStatus});
            this.perfDiffListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.perfDiffListView.Font = new System.Drawing.Font("Consolas", 9F);
            this.perfDiffListView.FullRowSelect = true;
            this.perfDiffListView.GridLines = true;
            this.perfDiffListView.HideSelection = false;
            this.perfDiffListView.Location = new System.Drawing.Point(3, 3);
            this.perfDiffListView.MultiSelect = false;
            this.perfDiffListView.Name = "perfDiffListView";
            this.perfDiffListView.Size = new System.Drawing.Size(1386, 189);
            this.perfDiffListView.TabIndex = 1;
            this.perfDiffListView.UseCompatibleStateImageBehavior = false;
            this.perfDiffListView.View = System.Windows.Forms.View.Details;
            //
            // colPerfMethod
            //
            this.colPerfMethod.Text = "Method";
            this.colPerfMethod.Width = 500;
            //
            // colPerfBaseline
            //
            this.colPerfBaseline.Text = "Baseline Avg (ms)";
            this.colPerfBaseline.Width = 150;
            //
            // colPerfCurrent
            //
            this.colPerfCurrent.Text = "Current Avg (ms)";
            this.colPerfCurrent.Width = 150;
            //
            // colPerfDelta
            //
            this.colPerfDelta.Text = "Delta (ms)";
            this.colPerfDelta.Width = 150;
            //
            // colPerfStatus
            //
            this.colPerfStatus.Text = "Status";
            this.colPerfStatus.Width = 150;
            //
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Log Files (*.log;*.txt)|*.log;*.txt|All Files (*.*)|*.*";
            this.openFileDialog.Title = "Select Log File";
            // 
            // CompareLogsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "CompareLogsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Compare Logs";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.topSplitContainer.Panel1.ResumeLayout(false);
            this.topSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).EndInit();
            this.topSplitContainer.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.leftHeaderPanel.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.rightHeaderPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseLeftMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseRightMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem compareMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem swapFilesMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem navigationMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstDiffMenuItem;
        private System.Windows.Forms.ToolStripMenuItem prevDiffMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextDiffMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lastDiffMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton browseLeftButton;
        private System.Windows.Forms.ToolStripButton browseRightButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton compareButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton firstDiffButton;
        private System.Windows.Forms.ToolStripButton prevDiffButton;
        private System.Windows.Forms.ToolStripButton nextDiffButton;
        private System.Windows.Forms.ToolStripButton lastDiffButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton optionsButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer topSplitContainer;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.TreeView leftTreeView;
        private System.Windows.Forms.Panel leftHeaderPanel;
        private System.Windows.Forms.Label leftTitleLabel;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.TreeView rightTreeView;
        private System.Windows.Forms.Panel rightHeaderPanel;
        private System.Windows.Forms.Label rightTitleLabel;
        private System.Windows.Forms.ListView differenceListView;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colPath;
        private System.Windows.Forms.ColumnHeader colLeftValue;
        private System.Windows.Forms.ColumnHeader colRightValue;
        private System.Windows.Forms.TabControl bottomTabControl;
        private System.Windows.Forms.TabPage structuralDiffTabPage;
        private System.Windows.Forms.TabPage performanceDiffTabPage;
        private System.Windows.Forms.Label perfDiffSummaryLabel;
        private System.Windows.Forms.ListView perfDiffListView;
        private System.Windows.Forms.ColumnHeader colPerfMethod;
        private System.Windows.Forms.ColumnHeader colPerfBaseline;
        private System.Windows.Forms.ColumnHeader colPerfCurrent;
        private System.Windows.Forms.ColumnHeader colPerfDelta;
        private System.Windows.Forms.ColumnHeader colPerfStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
