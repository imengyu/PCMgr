using System;
using System.Windows.Forms;

namespace TaskMgr.WorkWindow
{
    public partial class FormMainListHeaders : Form
    {
        public FormMainListHeaders(FormMain f)
        {
            InitializeComponent();
            this.f = f;
        }

        FormMain f = null;
        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string headers = "";
            for (int i = 0; i < listView1.Items.Count; i++)
                if (listView1.Items[i].Checked)
                    headers = headers + "#" + listView1.Items[i].Text + "-" + (int)listView1.Items[i].Tag;
            FormMain.SetConfig("MainHeaders", "AppSetting", headers);
            f.saveheader = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem li = listView1.SelectedItems[0];
                int i = listView1.Items.IndexOf(li);
                if(i>0)
                {
                    listView1.Items.Remove(li);
                    listView1.Items.Insert(i - 1, li);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem li = listView1.SelectedItems[0];
                int i = listView1.Items.IndexOf(li);
                if (i < listView1.Items.Count - 1)
                {
                    listView1.Items.Remove(li);
                    listView1.Items.Insert(i + 1, li);
                }
            }
        }

        private void Add(string name, int defw, bool en = true)
        {
            ListViewItem li = new ListViewItem(name);
            if (en)
            {
                FormMain.itemheader i = f.listProcessGetListHeaderItem(name);
                if (i != null)
                {
                    li.Checked = i.show;
                    li.Tag = i.width;
                    if (i.index > listView1.Items.Count)
                        listView1.Items.Insert(listView1.Items.Count, li);
                    else listView1.Items.Insert(i.index, li);
                }
                else
                {
                    li.Tag = defw;
                    listView1.Items.Add(li);
                }
            }
        }

        private void FormMainListHeaders_Load(object sender, EventArgs e)
        {
            Add("进程名称", 170);
            Add("发布者", 100);
            Add("状态", 70);
            Add("PID", 50);
            Add("CPU", 75);
            Add("内存", 75);
            Add("磁盘", 75);
            Add("网络", 75);
            Add("进程路径", 240);
            Add("命令行", 200);
            //DialogResult = DialogResult.Cancel;
        }
    }
}
