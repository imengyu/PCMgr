using System;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
#if _X64_
            labelOsVer.Text = "64 Bit Version";
#else
            labelOsVer.Text = "32 Bit Version" + (NativeMethods.MIs64BitOS() ? " (But in 64 OS)" : "");
#endif
        }
    }
}
