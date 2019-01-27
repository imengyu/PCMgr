using System;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormWindowKillAsk : Form
    {
        public FormWindowKillAsk()
        {
            InitializeComponent();
        }
        public FormWindowKillAsk(string info, IntPtr target)
        {
            InitializeComponent();
            lb_wndinfo.Text = info;
            targetWnd = target;
        }

        private IntPtr targetWnd = IntPtr.Zero;
        private void FormWindowKillAsk_Deactivate(object sender, EventArgs e)
        {
            NativeMethods.MAppWorkCall3(213, Handle);
        }

        private void btnKill_Click(object sender, EventArgs e)
        {
            NativeMethods.MAppWorkCall4(102, targetWnd, IntPtr.Zero);
        }
        private void btnWndize_Click(object sender, EventArgs e)
        {
            NativeMethods.MAppWorkCall4(103, targetWnd, IntPtr.Zero);
        }
        private void btnNoTop_Click(object sender, EventArgs e)
        {
            NativeMethods.MAppWorkCall4(104, targetWnd, IntPtr.Zero);
        }
    }
}
