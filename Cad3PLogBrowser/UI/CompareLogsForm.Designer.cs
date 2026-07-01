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
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.browseLeftToolButton = new System.Windows.Forms.ToolStripButton();
            this.leftFileLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.browseRightToolButton = new System.Windows.Forms.ToolStripButton();
            this.rightFileLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.compareToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.firstDiffToolButton = new System.Windows.Forms.ToolStripButton();
            this.prevDiffToolButton = new System.Windows.Forms.ToolStripButton();
            this.nextDiffToolButton = new System.Windows.Forms.ToolStripButton();
            this.lastDiffToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolButton = new System.Windows.Forms.ToolStripButton();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.bottomSplitContainer = new System.Windows.Forms.SplitContainer();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
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
            this.legendPanel = new System.Windows.Forms.Panel();
            this.legendLabel = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mainToolStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
            this.bottomSplitContainer.Panel1.SuspendLayout();
            this.bottomSplitContainer.Panel2.SuspendLayout();
            this.bottomSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.leftHeaderPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.rightHeaderPanel.SuspendLayout();
            this.legendPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseLeftToolButton,
            this.leftFileLabel,
            this.toolStripSeparator1,
            this.browseRightToolButton,
            this.rightFileLabel,
            this.toolStripSeparator2,
            this.compareToolButton,
            this.toolStripSeparator3,
            this.firstDiffToolButton,
            this.prevDiffToolButton,
            this.nextDiffToolButton,
            this.lastDiffToolButton,
            this.toolStripSeparator4,
            this.optionsToolButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Padding = new System.Windows.Forms.Padding(8, 4, 1, 4);
            this.mainToolStrip.Size = new System.Drawing.Size(1400, 35);
            this.mainToolStrip.TabIndex = 0;
            // 
            // browseLeftToolButton
            // 
            this.browseLeftToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.browseLeftToolButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.browseLeftToolButton.Name = "browseLeftToolButton";
            this.browseLeftToolButton.Size = new System.Drawing.Size(70, 24);
            this.browseLeftToolButton.Text = "Left File...";
            this.browseLeftToolButton.Click += new System.EventHandler(this.browseLeftButton_Click);
            // 
            // leftFileLabel
            // 
            this.leftFileLabel.ForeColor = System.Drawing.Color.Gray;
            this.leftFileLabel.Name = "leftFileLabel";
            this.leftFileLabel.Size = new System.Drawing.Size(300, 24);
            this.leftFileLabel.Text = "No file selected";
            this.leftFileLabel.AutoSize = false;
            this.leftFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // browseRightToolButton
            // 
            this.browseRightToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.browseRightToolButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.browseRightToolButton.Name = "browseRightToolButton";
            this.browseRightToolButton.Size = new System.Drawing.Size(75, 24);
            this.browseRightToolButton.Text = "Right File...";
            this.browseRightToolButton.Click += new System.EventHandler(this.browseRightButton_Click);
            // 
            // rightFileLabel
            // 
            this.rightFileLabel.ForeColor = System.Drawing.Color.Gray;
            this.rightFileLabel.Name = "rightFileLabel";
            this.rightFileLabel.Size = new System.Drawing.Size(300, 24);
            this.rightFileLabel.Text = "No file selected";
            this.rightFileLabel.AutoSize = false;
            this.rightFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // compareToolButton
            // 
            this.compareToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.compareToolButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.compareToolButton.ForeColor = System.Drawing.Color.DarkBlue;
            this.compareToolButton.Name = "compareToolButton";
            this.compareToolButton.Size = new System.Drawing.Size(70, 24);
            this.compareToolButton.Text = "Compare";
            this.compareToolButton.Click += new System.EventHandler(this.compareButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // firstDiffToolButton
            // 
            this.firstDiffToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.firstDiffToolButton.Enabled = false;
            this.firstDiffToolButton.Name = "firstDiffToolButton";
            this.firstDiffToolButton.Size = new System.Drawing.Size(40, 24);
            this.firstDiffToolButton.Text = "First";
            this.firstDiffToolButton.ToolTipText = "First Difference";
            this.firstDiffToolButton.Click += new System.EventHandler(this.firstDiffButton_Click);
            // 
            // prevDiffToolButton
            // 
            this.prevDiffToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.prevDiffToolButton.Enabled = false;
            this.prevDiffToolButton.Name = "prevDiffToolButton";
            this.prevDiffToolButton.Size = new System.Drawing.Size(55, 24);
            this.prevDiffToolButton.Text = "< Prev";
            this.prevDiffToolButton.ToolTipText = "Previous Difference";
            this.prevDiffToolButton.Click += new System.EventHandler(this.prevDiffButton_Click);
            // 
            // nextDiffToolButton
            // 
            this.nextDiffToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.nextDiffToolButton.Enabled = false;
            this.nextDiffToolButton.Name = "nextDiffToolButton";
            this.nextDiffToolButton.Size = new System.Drawing.Size(55, 24);
            this.nextDiffToolButton.Text = "Next >";
            this.nextDiffToolButton.ToolTipText = "Next Difference";
            this.nextDiffToolButton.Click += new System.EventHandler(this.nextDiffButton_Click);
            // 
            // lastDiffToolButton
            // 
            this.lastDiffToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lastDiffToolButton.Enabled = false;
            this.lastDiffToolButton.Name = "lastDiffToolButton";
            this.lastDiffToolButton.Size = new System.Drawing.Size(40, 24);
            this.lastDiffToolButton.Text = "Last";
            this.lastDiffToolButton.ToolTipText = "Last Difference";
            this.lastDiffToolButton.Click += new System.EventHandler(this.lastDiffButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // optionsToolButton
            // 
            this.optionsToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.optionsToolButton.Name = "optionsToolButton";
            this.optionsToolButton.Size = new System.Drawing.Size(65, 24);
            this.optionsToolButton.Text = "Options...";
            this.optionsToolButton.Click += new System.EventHandler(this.optionsButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statsStatusLabel,
            this.progressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 778);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1400, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // statsStatusLabel
            // 
            this.statsStatusLabel.Name = "statsStatusLabel";
            this.statsStatusLabel.Size = new System.Drawing.Size(1285, 17);
            this.statsStatusLabel.Spring = true;
            this.statsStatusLabel.Text = "Select files to compare";
            this.statsStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Visible = false;
            // 
            // bottomSplitContainer
            // 
            this.bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomSplitContainer.Location = new System.Drawing.Point(0, 35);
            this.bottomSplitContainer.Name = "bottomSplitContainer";
            this.bottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // bottomSplitContainer.Panel1
            // 
            this.bottomSplitContainer.Panel1.Controls.Add(this.mainSplitContainer);
            // 
            // bottomSplitContainer.Panel2
            // 
            this.bottomSplitContainer.Panel2.Controls.Add(this.differenceListView);
            this.bottomSplitContainer.Panel2.Controls.Add(this.legendPanel);
            this.bottomSplitContainer.Size = new System.Drawing.Size(1400, 743);
            this.bottomSplitContainer.SplitterDistance = 500;
            this.bottomSplitContainer.SplitterWidth = 5;
            this.bottomSplitContainer.TabIndex = 2;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.leftPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.rightPanel);
            this.mainSplitContainer.Size = new System.Drawing.Size(1400, 500);
            this.mainSplitContainer.SplitterDistance = 695;
            this.mainSplitContainer.SplitterWidth = 5;
            this.mainSplitContainer.TabIndex = 0;
            // 
            // leftPanel
            // 
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
            this.leftTreeView.HideSelection = false;
            this.leftTreeView.Location = new System.Drawing.Point(0, 32);
            this.leftTreeView.Name = "leftTreeView";
            this.leftTreeView.Size = new System.Drawing.Size(695, 468);
            this.leftTreeView.TabIndex = 1;
            // 
            // leftHeaderPanel
            // 
            this.leftHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.leftHeaderPanel.Controls.Add(this.leftTitleLabel);
            this.leftHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.leftHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.leftHeaderPanel.Name = "leftHeaderPanel";
            this.leftHeaderPanel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.leftHeaderPanel.Size = new System.Drawing.Size(695, 32);
            this.leftHeaderPanel.TabIndex = 0;
            // 
            // leftTitleLabel
            // 
            this.leftTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.leftTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.leftTitleLabel.Location = new System.Drawing.Point(10, 0);
            this.leftTitleLabel.Name = "leftTitleLabel";
            this.leftTitleLabel.Size = new System.Drawing.Size(685, 32);
            this.leftTitleLabel.TabIndex = 0;
            this.leftTitleLabel.Text = "Left File";
            this.leftTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.rightTreeView);
            this.rightPanel.Controls.Add(this.rightHeaderPanel);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(0, 0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(700, 500);
            this.rightPanel.TabIndex = 0;
            // 
            // rightTreeView
            // 
            this.rightTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rightTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTreeView.Font = new System.Drawing.Font("Consolas", 9F);
            this.rightTreeView.HideSelection = false;
            this.rightTreeView.Location = new System.Drawing.Point(0, 32);
            this.rightTreeView.Name = "rightTreeView";
            this.rightTreeView.Size = new System.Drawing.Size(700, 468);
            this.rightTreeView.TabIndex = 1;
            // 
            // rightHeaderPanel
            // 
            this.rightHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.rightHeaderPanel.Controls.Add(this.rightTitleLabel);
            this.rightHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.rightHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.rightHeaderPanel.Name = "rightHeaderPanel";
            this.rightHeaderPanel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.rightHeaderPanel.Size = new System.Drawing.Size(700, 32);
            this.rightHeaderPanel.TabIndex = 0;
            // 
            // rightTitleLabel
            // 
            this.rightTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.rightTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.rightTitleLabel.Location = new System.Drawing.Point(10, 0);
            this.rightTitleLabel.Name = "rightTitleLabel";
            this.rightTitleLabel.Size = new System.Drawing.Size(690, 32);
            this.rightTitleLabel.TabIndex = 0;
            this.rightTitleLabel.Text = "Right File";
            this.rightTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // differenceListView
            // 
            this.differenceListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
            this.differenceListView.Location = new System.Drawing.Point(0, 30);
            this.differenceListView.MultiSelect = false;
            this.differenceListView.Name = "differenceListView";
            this.differenceListView.Size = new System.Drawing.Size(1400, 208);
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
            // legendPanel
            // 
            this.legendPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.legendPanel.Controls.Add(this.legendLabel);
            this.legendPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.legendPanel.Location = new System.Drawing.Point(0, 0);
            this.legendPanel.Name = "legendPanel";
            this.legendPanel.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.legendPanel.Size = new System.Drawing.Size(1400, 30);
            this.legendPanel.TabIndex = 1;
            // 
            // legendLabel
            // 
            this.legendLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.legendLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.legendLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.legendLabel.Location = new System.Drawing.Point(10, 5);
            this.legendLabel.Name = "legendLabel";
            this.legendLabel.Size = new System.Drawing.Size(1380, 20);
            this.legendLabel.TabIndex = 0;
            this.legendLabel.Text = "Differences:";
            this.legendLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Log Files (*.log;*.txt)|*.log;*.txt|All Files (*.*)|*.*";
            this.openFileDialog.Title = "Select Log File";
            // 
            // CompareLogsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.bottomSplitContainer);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainToolStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "CompareLogsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Compare Logs";
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.bottomSplitContainer.Panel1.ResumeLayout(false);
            this.bottomSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
            this.bottomSplitContainer.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.leftHeaderPanel.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.rightHeaderPanel.ResumeLayout(false);
            this.legendPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton browseLeftToolButton;
        private System.Windows.Forms.ToolStripLabel leftFileLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton browseRightToolButton;
        private System.Windows.Forms.ToolStripLabel rightFileLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton compareToolButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton firstDiffToolButton;
        private System.Windows.Forms.ToolStripButton prevDiffToolButton;
        private System.Windows.Forms.ToolStripButton nextDiffToolButton;
        private System.Windows.Forms.ToolStripButton lastDiffToolButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton optionsToolButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statsStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.SplitContainer bottomSplitContainer;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
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
        private System.Windows.Forms.Panel legendPanel;
        private System.Windows.Forms.Label legendLabel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
