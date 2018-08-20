using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormVTimers : Form
    {
        public FormVTimers()
        {
            InitializeComponent();
        }
        public FormVTimers(uint processId)
        {
            InitializeComponent();
            this.processId = processId;
        }

        private uint processId = 0;

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_GetProcessTimers(uint pid, IntPtr callback);

        private IntPtr CallbackPtr = IntPtr.Zero;
        private EnumProcessHotKeyCallBack Callback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumProcessHotKeyCallBack(IntPtr structPtr, IntPtr objectStr, IntPtr funStr, IntPtr moduleStr, IntPtr hwndStr, IntPtr hwnd, uint tid, uint nID, uint interval, uint pid);

        private void FormVTimers_Load(object sender, EventArgs e)
        {
            Callback = _EnumProcessHotKeyCallBack;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);

            刷新ToolStripMenuItem_Click(sender, e);
        }

        private void _EnumProcessHotKeyCallBack(IntPtr structPtr, IntPtr objectStr, IntPtr funStr, IntPtr moduleStr, IntPtr hwndStr, IntPtr hwnd, uint tid, uint nID, uint interval, uint pid)
        {
            ListViewItem li = new ListViewItem(Marshal.PtrToStringUni(objectStr));
            li.SubItems.Add(interval.ToString());
            li.SubItems.Add(pid.ToString());
            li.SubItems.Add(tid.ToString());
            li.SubItems.Add(Marshal.PtrToStringUni(moduleStr));
            li.SubItems.Add(Marshal.PtrToStringUni(hwndStr));

            StringBuilder sbText = new StringBuilder(128);
            NativeMethods.Win32.GetWindowText(hwnd, sbText, 128);
            li.SubItems.Add(sbText.ToString());
            li.SubItems.Add(nID.ToString());
            li.SubItems.Add(Marshal.PtrToStringUni(funStr));
            li.SubItems.Add(nID.ToString());
            listViewTimers.Items.Add(li);
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewTimers.Items.Clear();

            M_SU_GetProcessTimers(processId, CallbackPtr);
        }
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewTimers.SelectedItems.Count > 0)
            {
                string s = "";
                for (int i = 0; i < listViewTimers.Columns.Count; i++)
                {
                    s += " " + listViewTimers.Columns[i].Text + " : ";
                    s += listViewTimers.SelectedItems[0].SubItems[i];
                }
                NativeMethods.MCopyToClipboard2(s);
            }
        }
        private void 移除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewTimers.SelectedItems.Count > 0)
            {

            }
        }

        private void listViewTimers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listViewTimers.SelectedItems.Count > 0)
                {
                    ListViewItem item = listViewTimers.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listViewTimers.PointToScreen(p);
                    contextMenuStrip.Show(p);
                }
            }
        }
    }
}
