using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    public partial class FindForm : Form
    {
        public FindForm()
        {
            InitializeComponent();
        }

        private void FindFrm_Load(object sender, EventArgs e)
        {

        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            //this.Hide();
        }
    }
}
