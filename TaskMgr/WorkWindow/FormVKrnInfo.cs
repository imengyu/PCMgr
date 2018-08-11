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


            FormMain.PEOCESSKINFO info = new FormMain.PEOCESSKINFO();
            if (FormMain.MCanUseKernel())
            {
                if (FormMain.MGetProcessEprocess(currentPid, ref info))
                {
                    AddItem("Eprocess", info.Eprocess);
                    AddItem("Peb", info.PebAddress);
                    AddItem("Job", info.JobAddress);
                    AddItem("ImageFileName", info.ImageFileName);
                }

                AddItem("ImageFileName", info.ImageFileName);
            }
        }

        private void AddItem(string name, string value)
        {
            ListViewItem li = new ListViewItem(name);
            li.SubItems.Add(value);
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
                FormMain.MCopyToClipboard2(listView1.SelectedItems[0].Text + ":" + listView1.SelectedItems[0].SubItems[1].Text);
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

        }
    }
}
