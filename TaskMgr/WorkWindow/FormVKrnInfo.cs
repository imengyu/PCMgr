using System;
using System.Collections;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormVKrnInfo : Form
    {
        public FormVKrnInfo(uint pid,string name)
        {
            InitializeComponent();
            currentPid = pid;
            oldText = Text;
            procName = name;
        }

        private string procName = "";
        private string oldText = "";
        private uint currentPid = 0;

        private void FormKrnInfo_Load(object sender, EventArgs e)
        {
            Text = oldText + " Process : " + procName + " [" + currentPid + "]";

            LoadItems();
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

        private void LoadItems()
        {
            AddItem("ProcessName", procName);
            AddItem("ProcessId", currentPid.ToString());
            AddEmeptyItem();


            NativeMethods.PEOCESSKINFO info = new NativeMethods.PEOCESSKINFO();
            if (NativeMethods.MCanUseKernel())
            {
                if (NativeMethods.MGetProcessEprocess(currentPid, ref info))
                {
                    AddItem("Eprocess", info.Eprocess);
                    AddItem("Peb", info.PebAddress);
                    AddItem("Job", info.JobAddress);
                    AddItem("ImageFileName", info.ImageFileName);
                }

                AddItem(Lanuages.LanuageMgr.GetStr("DblCklShow_EPROCESS", false), "", "_EPROCESS");
                AddItem(Lanuages.LanuageMgr.GetStr("DblCklShow_PEB", false), "", "_PEB");
                AddItem(Lanuages.LanuageMgr.GetStr("DblCklShow_KPROCESS", false), "", "_KPROCES");
                AddItem(Lanuages.LanuageMgr.GetStr("DblCklShow_RTL_USER_PROCESS_PARAMETERS", false), "", "_RTL_USER_PROCESS_PARAMETERS");
            }
            else AddItem("", Lanuages.LanuageFBuffers.Str_DriverNotLoad);
        }

        private void AddItem(string name, string value, object tag = null)
        {
            ListViewItem li = new ListViewItem(name);
            li.SubItems.Add(value);
            li.Tag = tag;
            listView1.Items.Add(li);
        }
        private void AddEmeptyItem()
        {
            ListViewItem li = new ListViewItem();
            listView1.Items.Add(li);
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                NativeMethods.MCopyToClipboard2(listView1.SelectedItems[0].Text + ":" + listView1.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void 重新加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            LoadItems();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                if (listView1.SelectedItems.Count > 0)
                    contextMenuStrip1.Show(MousePosition);
        }
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparer)listView1.ListViewItemSorter).Asdening = !((ListViewItemComparer)listView1.ListViewItemSorter).Asdening;
            ((ListViewItemComparer)listView1.ListViewItemSorter).SortColum = e.Column;
            listView1.Sort();
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {

            }
        }
    }
}
