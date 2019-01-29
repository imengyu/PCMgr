using PCMgr.Lanuages;
using System;
using System.Windows.Forms;
using static PCMgr.Main.MainUtils;

namespace PCMgr.WorkWindow
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
                    headers = headers + "#" + ((aa)listView1.Items[i].Tag).a + "-" + ((aa)listView1.Items[i].Tag).b;
            NativeMethods.SetConfig("MainHeaders", "AppSetting", headers);
            f.MainPageProcess.saveheader = false;
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

        private struct aa
        {
            public string a;
            public int b;
        }
        private void Add(string name, int defw, bool en = true)
        {
            ListViewItem li = new ListViewItem(LanuageMgr.GetStr(name));
            if (en)
            {
                itemheader i = f.MainPageProcess.listProcessGetListHeaderItem(name);
                if (i != null)
                {
                    aa a = new aa();
                    a.b = i.width;
                    a.a = name;
                    li.Checked = i.show;
                    li.Tag = a;
                    if (i.index > listView1.Items.Count)
                        listView1.Items.Insert(listView1.Items.Count, li);
                    else listView1.Items.Insert(i.index, li);
                }
                else
                {
                    aa a = new aa();
                    a.b = defw;
                    a.a = name;
                    li.Tag = a;
                    listView1.Items.Add(li);
                }
            }
        }

        private void FormMainListHeaders_Load(object sender, EventArgs e)
        {
            FormSettings.LoadFontSettingForUI(this);
            Add("TitleProcName", 170);
            Add("TitleType", 100);
            Add("TitlePublisher", 100);
            Add("TitleStatus", 70);
            Add("TitlePID", 50);
            Add("TitleCPU", 75);
            Add("TitleRam", 75);
            Add("TitleDisk", 75);
            Add("TitleNet", 75);
            Add("TitleProcPath", 240);
            Add("TitleCmdLine", 200);
            Add("TitleEProcess", 100);
            //DialogResult = DialogResult.Cancel;
        }
    }
}
