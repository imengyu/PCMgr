using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PCMgrNetMon
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            PCMgrUWP.Caller.ShowAboutDlg();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PCMgrUWP.Caller.ShowAboutDlg();
        }
    }
}
