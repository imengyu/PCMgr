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
            pb_plat.Image=Properties.Resources.img_x64;
#else
            bool is64 = NativeMethods.MIs64BitOS();
            labelOsVer.Text = "32 Bit Version" + (is64 ? " (But in 64 OS)" : "");
            if(is64) pb_plat.Image = Properties.Resources.img_x32inx64;
            else pb_plat.Image = Properties.Resources.img_x32;
#endif
        }

        private void linkLabelDiag_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            NativeMethods.MAppWorkCall3(172, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
