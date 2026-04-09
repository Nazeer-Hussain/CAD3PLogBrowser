using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Feature B7: Find All Results Window
    /// Shows all search results in a list, allowing quick navigation by clicking.
    /// </summary>
    public partial class FindAllResultsForm : Form
    {
        private readonly MainForm _mainForm;
        private ListView resultsListView;
        private Label lblResults;
        private Button btnClose;
        private Button btnExport;

        public FindAllResultsForm(MainForm mainForm, List<FindResult> results, string searchTerm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            ThemeManager.ApplyTheme(this);

            Text = $"Find All Results - '{searchTerm}'";
            PopulateResults(results, searchTerm);
        }

        private void InitializeComponent()
        {
            this.resultsListView = new ListView();
            this.lblResults = new Label();
            this.btnClose = new Button();
            this.btnExport = new Button();

            SuspendLayout();

            // lblResults
            lblResults.AutoSize = true;
            lblResults.Location = new Point(12, 12);
            lblResults.Name = "lblResults";
            lblResults.Size = new Size(100, 17);
            lblResults.TabIndex = 0;
            lblResults.Text = "Search Results:";

            // resultsListView
            resultsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            resultsListView.FullRowSelect = true;
            resultsListView.GridLines = true;
            resultsListView.Location = new Point(12, 35);
            resultsListView.MultiSelect = false;
            resultsListView.Name = "resultsListView";
            resultsListView.Size = new Size(760, 400);
            resultsListView.TabIndex = 1;
            resultsListView.View = View.Details;
            resultsListView.DoubleClick += ResultsListView_DoubleClick;

            resultsListView.Columns.Add("Line #", 80);
            resultsListView.Columns.Add("Text", 670);

            // btnExport
            btnExport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExport.Location = new Point(575, 445);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(95, 30);
            btnExport.TabIndex = 2;
            btnExport.Text = "&Export CSV...";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += BtnExport_Click;

            // btnClose
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(677, 445);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(95, 30);
            btnClose.TabIndex = 3;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += (s, e) => Close();

            // FindAllResultsForm
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(784, 487);
            Controls.Add(btnClose);
            Controls.Add(btnExport);
            Controls.Add(resultsListView);
            Controls.Add(lblResults);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimizeBox = false;
            MaximizeBox = true;
            Name = "FindAllResultsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Find All Results";

            ResumeLayout(false);
            PerformLayout();
        }

        private void PopulateResults(List<FindResult> results, string searchTerm)
        {
            lblResults.Text = $"Found {results.Count} match(es) for '{searchTerm}':";

            resultsListView.BeginUpdate();
            resultsListView.Items.Clear();

            foreach (var result in results)
            {
                var item = new ListViewItem(result.LineNumber.ToString());
                item.SubItems.Add(result.LineText);
                item.Tag = result.LineNumber;
                resultsListView.Items.Add(item);
            }

            resultsListView.EndUpdate();
        }

        private void ResultsListView_DoubleClick(object sender, EventArgs e)
        {
            if (resultsListView.SelectedItems.Count == 0) return;

            var item = resultsListView.SelectedItems[0];
            if (item.Tag is int lineNumber)
            {
                _mainForm.JumpToLine(lineNumber);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FileName = "search_results.csv";

                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var writer = new System.IO.StreamWriter(dialog.FileName))
                    {
                        writer.WriteLine("Line Number,Text");

                        foreach (ListViewItem item in resultsListView.Items)
                        {
                            string text = item.SubItems[1].Text;
                            if (text.Contains(",") || text.Contains("\""))
                                text = "\"" + text.Replace("\"", "\"\"") + "\"";
                            writer.WriteLine($"{item.Text},{text}");
                        }
                    }

                    MessageBox.Show($"Results exported to:\n{dialog.FileName}",
                        "Find All Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not export results:\n{ex.Message}",
                        "Find All Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public class FindResult
    {
        public int LineNumber { get; set; }
        public string LineText { get; set; }
    }
}
