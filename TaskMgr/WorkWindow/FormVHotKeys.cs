using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;

using System.Runtime.InteropServices;
using System.Text;

using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormVHotKeys : Form
    {
        public FormVHotKeys()
        {
            InitializeComponent();
        }
        public FormVHotKeys(uint processId)
        {
            InitializeComponent();
            this.processId = processId;
        }

        private uint processId = 0;

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_GetProcessHotKeys(uint pid, IntPtr callback);

        private IntPtr CallbackPtr = IntPtr.Zero;
        private EnumProcessHotKeyCallBack Callback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumProcessHotKeyCallBack(IntPtr structPtr, IntPtr objectStr, uint keyID, IntPtr keyStr, uint pid, uint tid, IntPtr procName);

        private void _EnumProcessHotKeyCallBack(IntPtr structPtr, IntPtr objectStr, uint keyID, IntPtr keyStr, uint pid, uint tid, IntPtr procName)
        {
            ListViewItem li = new ListViewItem(Marshal.PtrToStringUni(objectStr));
            li.SubItems.Add(keyID.ToString());
            li.SubItems.Add(Marshal.PtrToStringUni(keyStr));
            li.SubItems.Add(pid.ToString());
            li.SubItems.Add(tid.ToString());
            li.SubItems.Add(Marshal.PtrToStringUni(procName));

            listViewHotKeys.Items.Add(li);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewHotKeys.SelectedItems.Count > 0)
            {

            }
        }
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewHotKeys.Items.Clear();

            M_SU_GetProcessHotKeys(processId, CallbackPtr);
        }
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewHotKeys.SelectedItems.Count > 0)
            {
                string s = "";
                for (int i = 0; i < listViewHotKeys.Columns.Count; i++)
                {
                    s += " " + listViewHotKeys.Columns[i].Text + " : ";
                    s += listViewHotKeys.SelectedItems[0].SubItems[i];
                }
                NativeMethods.MCopyToClipboard2(s);
            }
        }

        private void FormVHotKeys_Load(object sender, EventArgs e)
        {
            Callback = _EnumProcessHotKeyCallBack;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);

            刷新ToolStripMenuItem_Click(sender, e);
        }

        private void listViewHotKeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listViewHotKeys.SelectedItems.Count > 0)
                {
                    ListViewItem item = listViewHotKeys.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listViewHotKeys.PointToScreen(p);
                    contextMenuStrip.Show(p);
                }
            }
        }
    }
}
