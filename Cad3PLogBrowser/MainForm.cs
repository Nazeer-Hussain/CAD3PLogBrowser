using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Cad3PLogBrowser.Properties;

namespace Cad3PLogBrowser
{
    public partial class mainFrm : Form
    {
        public String activeFilename { get; set; }

        public mainFrm()
        {
            InitializeComponent();
            /*Application.UserAppDataRegistry.GetValue("InitialDir", "D:\UWGM\VC_LOG_DIR\cadapp");
            Application.UserAppDataRegistry.GetValue("InitialDirVar", "VC_LOG_DIR");

            Application.UserAppDataRegistry.GetValue("Suffix", "part");
            Application.UserAppDataRegistry.GetValue("Filters", "");
            
            Application.UserAppDataRegistry.GetValue("LastView", "Tree");
            Application.UserAppDataRegistry.GetValue("LastTab", "Log");*/
            splitContainer1.SplitterDistance = Convert.ToInt32(Application.UserAppDataRegistry.GetValue("LastSplitter"));
            //setZeroDocState(true);
        }

        public void OpenFilename(string filename)
        {
            if (filename != string.Empty)
            {
                if (File.Exists(filename))
                {
                logFileLoading = true;
                if (ReadLogFile(filename))
                {
                    setZeroDocState(false);
                    FileStatus.Image = Resources.green_ball;
                    logWatcher.Path = Path.GetDirectoryName(filename);
                    logWatcher.Filter = Path.GetFileName(filename);
                    logWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
                }
                else
                {
                    setZeroDocState(true);
                }}
                logFileLoading = false;
            }
        }

        private bool ReadLogFile(string filename)
        {
            bool isSuccess = false;
            try
            {
                FileInfo logFileInfo = new FileInfo(filename);
                double filesize = Convert.ToDouble (logFileInfo.Length);
                FileLoadProgress.Maximum = 100;
                FileLoadProgress.Visible = true;
                StreamReader logReader = new StreamReader(filename);
                string logentry;
                double percent = 0.0;
                int lineNo = 1;
                listView1.Items.Clear();
                while ((logentry = logReader.ReadLine()) != null)
                {
                    percent = percent + (logentry.Length / filesize * 100);
                    FileLoadProgress.Value = Convert.ToInt32(percent);
                    listView1.Items.Add(lineNo.ToString() + "      " + logentry);
                    lineNo++;
                }
                FileLoadProgress.Value = 100;
                isSuccess = true;
                FileLoadProgress.Visible = false;
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        private void setZeroDocState(bool status)
        {
            this.saveAsMenuItem.Enabled = this.SaveButton.Enabled = !status;
            this.refreshMenuItem.Enabled = this.RefreshButton.Enabled = !status;
            this.copyMenuItem.Enabled = this.CopyButton.Enabled = !status;
            this.findMenuItem.Enabled = this.FindButton.Enabled = !status;
            this.filterMenuItem.Enabled = this.FilterButton.Enabled = !status;
            this.reloadMenuItem.Enabled = !status;
            this.findNextMenuItem.Enabled = !status;
            this.FileStatus.Enabled = !status;
            this.FileLoadProgress.Enabled = !status;
            // throw new NotImplementedException();
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mainFrm_ResizeBegin(object sender, EventArgs e)
        {
            resizeItems();
        }

        private void mainFrm_ResizeEnd(object sender, EventArgs e)
        {
            resizeItems();
        }

        private void mainFrm_Resize(object sender, EventArgs e)
        {
            resizeItems();

        }

        public void resizeItems()
        {
            /*CallTree.Width = splitContainer1.Panel1.Width;
            CallTree.Height = splitContainer1.Panel1.Height-(statusStrip1.Height *2);
            tabControl1.Width = splitContainer1.Panel2.Width;
            tabControl1.Height = splitContainer1.Panel2.Height - (statusStrip1.Height*2);*/
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            resizeItems();
        }

        private void mainFrm_SizeChanged(object sender, EventArgs e)
        {
            resizeItems();
        }


        private void openMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //MessageBox.Show("File Selected is " + openFileDialog1.FileName);

                OpenFilename(openFileDialog1.FileName);
            }
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            openMenuItem_Click(sender, e);
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("File Selected is " + saveFileDialog1.FileName);
            }

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            saveAsMenuItem_Click(sender, e);
        }

        private void logWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (!logFileLoading)
                FileStatus.Image = Resources.red_ball;
        }


        public bool logFileLoading { get; set; }

        private void mainFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.UserAppDataRegistry.SetValue("InitialDir", "D:\\UWGM\\VC_LOG_DIR\\cadapp");
            Application.UserAppDataRegistry.SetValue("InitialDirVar", "VC_LOG_DIR");

            Application.UserAppDataRegistry.SetValue("Suffix", "part");
            Application.UserAppDataRegistry.SetValue("Filters", "");

            Application.UserAppDataRegistry.SetValue("LastView", "Tree");
            Application.UserAppDataRegistry.SetValue("LastTab", "Log");
            Application.UserAppDataRegistry.SetValue("LastSplitter", splitContainer1.SplitterDistance.ToString());
        }

        private void mainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /* if (MessageBox.Show("Do you want to exit?", Resources.TITLE, MessageBoxButtons.YesNo) == DialogResult.No)
             {
                 // Cancel the Closing event from closing the form.
                 e.Cancel = true;
             }*/
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            AbtBox abtbox = new AbtBox();
            abtbox.ShowDialog();
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void findMenuItem_Click(object sender, EventArgs e)
        {
            FindForm findFrm = new FindForm();
            findFrm.Left = mainFrm.ActiveForm.Right - findFrm.Width - 100;
            findFrm.Top = mainFrm.ActiveForm.Top + 100;
            findFrm.ShowDialog();
        }

        private void filterMenuItem_Click(object sender, EventArgs e)
        {
            FilterFrm filterFrm = new FilterFrm();
            filterFrm.ShowDialog();
        }

        private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void FilterButton_Click(object sender, EventArgs e)
        {
            filterMenuItem_Click(sender, e);
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            findMenuItem_Click(sender, e);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            refreshMenuItem_Click(sender, e);
        }

        private void refreshMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            copyMenuItem_Click(sender, e);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            settingsMenuItem_Click(sender, e);
        }

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            SettingsFrm settingsFrm = new SettingsFrm();
            settingsFrm.ShowDialog();
        }

        private void mainFrm_Load(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ApiTree_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void ApiTree_Click(object sender, EventArgs e)
        {

        }

        private void CallTree_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshMenuItem_Click(sender, e);
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            filterMenuItem_Click(sender, e);
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseUp_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(listView1, e.Location);
            }
        }

        private void ApiTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(ApiTree, e.Location);
            }
        }

        private void CallTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(CallTree, e.Location);
            }
        }

    }
}
