using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

using System.Windows.Forms;

namespace PCMgr
{
    public partial class FormSL : Form
    {
        public FormSL(string[] agrs)
        {
            this.agrs = agrs;
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

        }

        private void FormSL_Shown(object sender, EventArgs e)
        {
            NativeMethods.MAppWorkCall3(200, Handle);
            timer1.Start();
        }

        private string[] agrs;

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            new FormMain(agrs).ShowDialog(this);

            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
