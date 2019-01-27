using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using System.Text;

using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormAlwaysOnTop : Form
    {
        public FormAlwaysOnTop()
        {
            InitializeComponent();
        }
        public void UpdateText(string s)
        {
            label1.Text = s;
        }
        private void FormAlwaysOnTop_Load(object sender, EventArgs e)
        {
            Location = new Point(15, 15);
            if(NativeMethods.MAppWorkCall3(223)==1) label1.Text = "AOP Timer started.";
            else label1.Text = "AOP Timer not start.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NativeMethods.MAppWorkCall3(224);
        }
    }
}
