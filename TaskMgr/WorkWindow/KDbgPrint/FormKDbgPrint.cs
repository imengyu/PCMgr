using PCMgr.WorkWindow.KDbgPrint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormKDbgPrint : Form
    {
        public FormKDbgPrint()
        {
            InitializeComponent();
        }

        private FormFind formFind = null;

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private extern static void MOnCloseMyDbgView();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private extern static bool MSaveFileSingal(IntPtr hWnd, string startDir, string title, string fileFilter, string fileName, string defExt, StringBuilder strrs, uint bufsize);


        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(260);
            if (MSaveFileSingal(Handle, null, "Save to..", "文本文件\0*.txt\0所有文件\0*.*\0", null, ".txt", sb, 260))
                saveAll(sb.ToString());
        }
        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            listViewOutPut.Items.Clear();
        }
        private void toolStripButtonFilter_Click(object sender, EventArgs e)
        {

        }
        private void toolStripButtonFind_Click(object sender, EventArgs e)
        {
            if (formFind == null)
            {
                formFind = new FormFind();
                formFind.Find += FormFind_Find;
                formFind.FormClosed += FormFind_FormClosed;
                formFind.Show(this);
            }
            else formFind.Show();
        }

        private void FormFind_FormClosed(object sender, FormClosedEventArgs e)
        {
            formFind = null;
        }
        private void FormFind_Find(object sender, EventArgs e)
        {
            bool full = formFind.FullSearch;
            bool casesenstive = formFind.CaseSensitive;
            string keyword = formFind.KeyWord;
            if (casesenstive) keyword = keyword.ToLower();
            bool up = formFind.Up;

            ListViewItem nextItem = null;        
            if (up)
            {
                int indexstart = listViewOutPut.Items.Count - 1;
                if (listViewOutPut.SelectedItems.Count > 0)
                    indexstart = listViewOutPut.Items.IndexOf(listViewOutPut.SelectedItems[0]);
                for (int i = indexstart; i >= 0; i--)
                {
                    ListViewItem li = listViewOutPut.Items[i];
                    string str = li.SubItems[2].Text;
                    if (casesenstive)
                        str = str.ToLower();

                    if (full && str == keyword)
                        nextItem = li;
                    else if (!full && str.Contains(keyword))
                        nextItem = li;
                }
            }
            else
            {
                int indexstart = 0;
                if (listViewOutPut.SelectedItems.Count > 0)
                    indexstart = listViewOutPut.Items.IndexOf(listViewOutPut.SelectedItems[0]);
                for (int i = indexstart; i < listViewOutPut.Items.Count; i++)
                {
                    ListViewItem li = listViewOutPut.Items[i];
                    string str = li.SubItems[2].Text;
                    if (casesenstive)
                        str = str.ToLower();

                    if (full && str == keyword)
                        nextItem = li;
                    else if (!full && str.Contains(keyword))
                        nextItem = li;
                }
            }

            if (nextItem != null)
            {
                foreach (ListViewItem li in listViewOutPut.SelectedItems)
                    li.Selected = false;
                nextItem.Selected = true;
                listViewOutPut.EnsureVisible(listViewOutPut.Items.IndexOf(nextItem));
                formFind.Hide();
                FormMain.MAppWorkCall3(213, Handle, IntPtr.Zero);
            }
            else MessageBox.Show(FormMain.str_CantFind + " \"" + keyword + "\"");
        }

        private void addItem(string s)
        {
            ListViewItem li = new ListViewItem(listViewOutPut.Items.Count.ToString());
            li.SubItems.Add(DateTime.Now.ToString());
            li.SubItems.Add(s);

            listViewOutPut.Items.Add(li);
            if (toolStripButtonAutoScroll.CheckState == CheckState.Checked)
                listViewOutPut.EnsureVisible(listViewOutPut.Items.Count - 1);
        }
        private void saveAll(string fileName)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName);
            for (int i = 0; i < listViewOutPut.Items.Count; i++)
            {
                ListViewItem li = listViewOutPut.Items[i];
                sw.WriteLine(li.SubItems[1].Text + "  " + li.SubItems[2].Text);
            }
            sw.Close();
            sw = null;
        }

        public void Add(string s)
        {
            Invoke(new Action(delegate {
                addItem(s);
            }));
        }

        private void FormKDbgPrint_Load(object sender, EventArgs e)
        {
            string set = FormMain.GetConfig("DbgPrintViewPosSize", "AppSetting", "");
            if (set != "" && set.Contains("#"))
            {
                string[] vals = set.Split('#');
                if(vals.Length==4)
                {
                    try
                    {
                        int x = int.Parse(vals[0]),
                            y = int.Parse(vals[1]);
                        if (x > 0 && x < Screen.PrimaryScreen.Bounds.Width - 200)
                            Left = x;
                        if (y > 0 && y < Screen.PrimaryScreen.Bounds.Height - 100)
                            Top = y;
                    }
                    catch
                    {

                    }
                    try
                    {
                        int w = int.Parse(vals[2]), h = int.Parse(vals[3]);
                        if (w > 0 && w + Left < Screen.PrimaryScreen.Bounds.Width)
                            Width = w;
                        if (h > 0 && h + Top < Screen.PrimaryScreen.Bounds.Height)
                            Height = h;
                    }
                    catch { }
                }
            }
        }
        private void FormKDbgPrint_FormClosing(object sender, FormClosingEventArgs e)
        {
            MOnCloseMyDbgView();
            FormMain.SetConfig("DbgPrintViewPosSize", "AppSetting", Left + "#" + Top + "#" + Width + "#" + Height);
        }
    }
}
