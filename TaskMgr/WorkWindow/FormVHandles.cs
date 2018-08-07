using PCMgr.Lanuages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormVHandles : Form
    {
        public FormVHandles(uint pid, string name)
        {
            InitializeComponent();
            currentPid = pid;
            currentName = name;

        }

        private string currentName = "";
        private uint currentPid = 0;

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_CloseHandleWithProcess(uint pid, IntPtr handle);
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_EH_CloseHandle(uint pid, IntPtr handle);
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MEnumProcessHandles(uint pid, IntPtr callback);

        private IntPtr CallbackPtr = IntPtr.Zero;
        private EHCALLBACK Callback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EHCALLBACK(IntPtr handle, IntPtr type, IntPtr name, IntPtr address, IntPtr objaddr, int refcount, int typeindex);

        private void FormVHandles_Load(object sender, EventArgs e)
        {
            FormMain.MAppWorkCall3(182, listView1.Handle, IntPtr.Zero);
            listView1.ListViewItemSorter = new ListViewItemComparer();

            Callback = _EHCALLBACK;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);
        }
        private void _EHCALLBACK(IntPtr handle, IntPtr type, IntPtr name, IntPtr address, IntPtr objaddr, int refcount, int typeindex)
        {
            ListViewItem li = new ListViewItem(Marshal.PtrToStringUni(type));
            li.Tag = handle;
            li.SubItems.Add(Marshal.PtrToStringUni(name));
            li.SubItems.Add(Marshal.PtrToStringUni(address));
            li.SubItems.Add(Marshal.PtrToStringUni(objaddr));
            li.SubItems.Add(refcount.ToString());
            li.SubItems.Add(typeindex.ToString());
            listView1.Items.Add(li);
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentPid >= 4)
            {
                listView1.Items.Clear();
                MEnumProcessHandles(currentPid, CallbackPtr);
                Text = string.Format(LanuageMgr.GetStr("VHandleTitle"), currentName, currentPid, listView1.Items.Count);
            }
        }
        private void 关闭句柄ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                if (M_EH_CloseHandle(currentPid, (IntPtr)listView1.SelectedItems[0].Tag))
                    MessageBox.Show(LanuageMgr.GetStr("OpSuccess"));
                else MessageBox.Show(LanuageMgr.GetStr("OpFailed"));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listView1.SelectedItems.Count > 0)
                contextMenuStrip1.Show(MousePosition);
        }

        public class ListViewItemComparer : IComparer
        {
            private int col;
            private bool asdening = false;

            public int SortColum { get { return col; } set { col = value; } }
            public bool Asdening { get { return asdening; } set { asdening = value; } }

            public int Compare(object x, object y)
            {
                int returnVal = -1;
                if (((ListViewItem)x).SubItems[col].Text == ((ListViewItem)y).SubItems[col].Text) return -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
                if (asdening) returnVal = -returnVal;
                return returnVal;
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparer)listView1.ListViewItemSorter).Asdening = !((ListViewItemComparer)listView1.ListViewItemSorter).Asdening;
            ((ListViewItemComparer)listView1.ListViewItemSorter).SortColum = e.Column;
            listView1.Sort();
        }

        private void 强制关闭句柄ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                if (M_SU_CloseHandleWithProcess(currentPid, (IntPtr)listView1.SelectedItems[0].Tag))
                    MessageBox.Show(LanuageMgr.GetStr("OpSuccess"));
                else MessageBox.Show(LanuageMgr.GetStr("OpFailed"));
        }

        private void FormVHandles_Shown(object sender, EventArgs e)
        {
            刷新ToolStripMenuItem_Click(sender, e);
            labelEnuming.Hide();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ListViewItem item = listView1.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listView1.PointToScreen(p);
                    contextMenuStrip1.Show(p);
                }
            }
        }
    }
}
