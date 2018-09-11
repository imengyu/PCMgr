using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static PCMgrRegedit.NativeMethods;

namespace PCMgrRegedit
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            HKEY_CLASSES_ROOT = MREG_GetROOTKEY(0);
            HKEY_CURRENT_USER = MREG_GetROOTKEY(1);
            HKEY_LOCAL_MACHINE = MREG_GetROOTKEY(2);
            HKEY_USERS = MREG_GetROOTKEY(3);

            HKEY_CURRENT_CONFIG = MREG_GetROOTKEY(7);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static IntPtr HKEY_CLASSES_ROOT = IntPtr.Zero;
        public static IntPtr HKEY_CURRENT_USER = IntPtr.Zero;
        public static IntPtr HKEY_LOCAL_MACHINE = IntPtr.Zero;
        public static IntPtr HKEY_USERS = IntPtr.Zero;
        public static IntPtr HKEY_CURRENT_CONFIG = IntPtr.Zero;

        private TreeNode hkcr = null;
        private TreeNode hkcu = null;
        private TreeNode hklm = null;
        private TreeNode hkus = null;
        private TreeNode hkcc = null;

        private struct KEYSTRO
        {
            public KEYSTRO(IntPtr rootKey)
            {
                NewItem = false;
                KeyPath = "";
                RootKey = rootKey;
                IsRootKey = true;
            }
            public KEYSTRO(IntPtr rootKey, string path)
            {
                NewItem = false;
                IsRootKey = false;
                KeyPath = path;
                RootKey = rootKey;
            }

            public bool NewItem;
            public bool IsRootKey;
            public IntPtr RootKey;
            public string KeyPath;
        }
        private struct VALSTRO
        {
            public VALSTRO(KEYSTRO k, string valname, uint type)
            {
                NewItem = false;
                Key = k;
                ValueName = valname;
                Type = type;
            }
            public VALSTRO(KEYSTRO k, string valname, uint type, bool newitem)
            {
                NewItem = newitem;
                Key = k;
                ValueName = valname;
                Type = type;
            }

            public bool NewItem;
            public KEYSTRO Key;
            public string ValueName;
            public uint Type;
        }

        private ENUMKEYVALECALLBACK eNUMKEYVALECALLBACK;
        private ENUMKEYSCALLBACK eNUMKEYSCALLBACK;
        private IntPtr eNUMKEYVALECALLBACK_Ptr;
        private IntPtr eNUMKEYSCALLBACK_Ptr;

        private bool enumChildKeysHasDefault = false;
        private bool enumChildKeysShowStatus = false;
        private TreeNode currentEnumItem = null;
        private TreeNode lastShowItem = null;
        private TreeNode myComputer = null;

        private void FormMain_Load(object sender, EventArgs e)
        {
            eNUMKEYVALECALLBACK = EnumKeyValuesCallback;
            eNUMKEYSCALLBACK = EnumKeysCallback;

            eNUMKEYVALECALLBACK_Ptr = Marshal.GetFunctionPointerForDelegate(eNUMKEYVALECALLBACK);
            eNUMKEYSCALLBACK_Ptr = Marshal.GetFunctionPointerForDelegate(eNUMKEYSCALLBACK);

            imageList.Images.Add("MyComputer", new Icon(Icon.FromHandle(MFM_GetMyComputerIcon()), 16, 16));
            imageList.Images.Add("Folder", new Icon(Icon.FromHandle(MFM_GetFolderIcon()), 16, 16));

            myComputer = treeView.Nodes.Add("MyComputer", MFM_GetMyComputerName(), "MyComputer", "MyComputer");

            hkcr = AddItemToList("HKEY_CLASSES_ROOT", myComputer, true, HKEY_CLASSES_ROOT);
            hkcu = AddItemToList("HKEY_CURRENT_USER", myComputer, true, HKEY_CURRENT_USER);
            hklm = AddItemToList("HKEY_LOCAL_MACHINE", myComputer, true, HKEY_LOCAL_MACHINE);
            hkus = AddItemToList("HKEY_USERS", myComputer, true, HKEY_USERS);
            hkcc = AddItemToList("HKEY_CURRENT_CONFIG", myComputer, true, HKEY_CURRENT_CONFIG);

            textBoxAddress.Text = myComputer.Text;

            myComputer.Expand();
        }

        private TreeNode AddItemToList(string name, TreeNode parent, bool hasChild, IntPtr keytag, string keyPath = null)
        {
            TreeNode t = parent.Nodes.Add(name, name, "Folder", "Folder");

            if (keyPath != null) t.Tag = new KEYSTRO(keytag, keyPath);
            else t.Tag = new KEYSTRO(keytag);

            if (hasChild)
            {
                TreeNode t1 = t.Nodes.Add("Loading", "加载中", "ItemLoading");
                t1.Name = "Loading";
            }
            return t;
        }
        private bool LoadFolder(TreeNode t)
        {
            enumChildKeysHasDefault = false;
            KEYSTRO kEYSTRO = (KEYSTRO)t.Tag;
            bool rs = MREG_EnumKeys(kEYSTRO.RootKey, kEYSTRO.KeyPath, eNUMKEYSCALLBACK_Ptr);
            return rs;
        }
        private void LoadAllItems(TreeNode t)
        {
            if (lastShowItem != t)
            {
                if (t.Tag == null) return;

                listView.Items.Clear();
                lastShowItem = t;
                currentEnumItem = t;
                KEYSTRO kEYSTRO = (KEYSTRO)currentEnumItem.Tag;
                textBoxAddress.Text = MREG_ROOTKEYToStr(kEYSTRO.RootKey) + "\\" + kEYSTRO.KeyPath;
                bool rs = MREG_EnumKeyVaules(kEYSTRO.RootKey, kEYSTRO.KeyPath, eNUMKEYVALECALLBACK_Ptr);

                if (!enumChildKeysHasDefault)
                    AddDataItemToList(kEYSTRO, "", 1, "");
            }
        }
        private bool ContainsKey(TreeNode t, string name)
        {
            foreach(TreeNode t2 in t.Nodes)
            {
                if (t2.Text == name)
                    return true;
            }
            return false;
        }
        private bool ContainsValue(string name)
        {
            foreach (ListViewItem li in listView.Items)
            {
                if (li.Text == name)
                    return true;
            }
            return false;
        }
        private void AddDataItemToList(KEYSTRO parent, string name, uint type, string data)
        {
            if (name == "")
            {
                name = "(默认)";
                enumChildKeysHasDefault = true;
            }

            ListViewItem li = new ListViewItem(name);
            li.SubItems.Add(MREG_RegTypeToStr(type));

            if (data == "" && MREG_RegTypeIsSz(type))
                data = "(数值未设置)";

            li.Tag = new VALSTRO(parent, name, type);
            li.SubItems.Add(data);
            li.ImageKey = MREG_RegTypeToIcon(type);

            listView.Items.Add(li);
        }
        private void SetStatusText(string s)
        {
            Invoke(new Action(delegate { statusLabel.Text = s; }));
        }
        private void SetStatusProcressVisible(bool visible)
        {
            Invoke(new Action(delegate { statusProgressBar.Visible = visible; }));
        }
        private void SetStatusProcressMaquree(bool maquree)
        {
            Invoke(new Action(delegate { statusProgressBar.Style = maquree ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks; }));
        }
        private void SetStatusProcress(int val)
        {
            Invoke(new Action(delegate {
                statusProgressBar.Value = val;
            }));
        }

        private bool EnumKeyValuesCallback(IntPtr hRootKey, string path, string valueName, uint dataType, uint dataSize, string dataSample, uint index, uint allCount)
        {
            if (currentEnumItem != null)
            {
                AddDataItemToList((KEYSTRO)currentEnumItem.Tag, valueName, dataType, dataSample);
                return true;
            }
            return false;
        }
        private bool EnumKeysCallback(IntPtr hRootKey, string path, string childKeyName, bool hasChild, uint index, uint allCount)
        {
            Invoke(new Action(delegate
            {
                if (index == 100)
                {
                    enumChildKeysShowStatus = true;
                    SetStatusText("正在展开...");
                    SetStatusProcressVisible(true);
                }

                AddItemToList(childKeyName, currentEnumItem, hasChild, hRootKey,
                    path == "" ? childKeyName : path + "\\" + childKeyName);

                if (enumChildKeysShowStatus)
                {
                    statusProgressBar.Value = (int)(index / (double)allCount * 100);
                    statusLabel.Text = "正在展开 (" + index + "/" + allCount + ")";
                }
            }));
            return true;
        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNode t = e.Node;
            if (t.Nodes.Count == 1 && t.Nodes[0].Name == "Loading")
            {
                t.Nodes.Clear();
                currentEnumItem = t;

                new Thread(new ThreadStart(delegate
                {
                    enumChildKeysShowStatus = false;
                    if (!LoadFolder(t))
                    {
                        Invoke(new Action(delegate
                        {
                            t.Nodes.Add("Error", "加载失败", "ItemError", "ItemError");
                        }));
                    }

                    SetStatusProcressVisible(false);
                    SetStatusText("就绪");
                    currentEnumItem = null;
                })).Start();
            }
        }
        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == myComputer)
                textBoxAddress.Text = myComputer.Text;
            else LoadAllItems(e.Node);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
        }

        private void 复制项名称ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
            {
                KEYSTRO eYSTRO = (KEYSTRO)t.Tag;
                Clipboard.SetText(MREG_ROOTKEYToStr(eYSTRO.RootKey) + "\\" + eYSTRO.KeyPath);
            }
        }
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
            {
                if (MessageBox.Show("真的要删除此项目和其所有子项吗?", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    KEYSTRO eYSTRO = (KEYSTRO)t.Tag;
                    if (!MREG_DeleteKey(eYSTRO.RootKey, eYSTRO.KeyPath))
                        MessageBox.Show("无法删除项\n" + MREG_GetLastErrString(), "删除项目", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    else t.Remove();
                }
            }
        }
        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                t.BeginEdit();
        }
        private void 权限ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
            {
                KEYSTRO eYSTRO = (KEYSTRO)t.Tag;

            }
        }

        private void NewSubKey(KEYSTRO parent, TreeNode tparent)
        {
            if (!tparent.IsExpanded)
                tparent.Expand();

            int newid = 1;
            string name = "新项 #" + newid;
            while (ContainsKey(tparent, name))
            {
                newid++;
                name = "新项 #" + newid;
            }

            parent.NewItem = true;
            TreeNode newT = tparent.Nodes.Add(name, name, "Folder", "Folder");
            newT.Name = name;
            newT.Tag = parent;
            newT.BeginEdit();
        }
        private void NewValue(KEYSTRO parent, TreeNode tparent, uint type)
        {
            int newid = 1;
            string name = "新值  #" + newid;
            while (ContainsValue(name))
            {
                newid++;
                name = "新项  #" + newid;
            }

            ListViewItem linew = new ListViewItem(name);
            linew.SubItems.Add(MREG_RegTypeToStr(type));
            linew.SubItems.Add("");
            linew.ImageKey = MREG_RegTypeToIcon(type);
            linew.Name = name;
            linew.Tag = new VALSTRO(parent, name, type, true);
            listView.Items.Add(linew);
            linew.BeginEdit();
        }

        private void 项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewSubKey((KEYSTRO)t.Tag, t);
        }
        private void 字符串值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewValue((KEYSTRO)t.Tag, t, REG_SZ);
        }
        private void dWORD32位值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewValue((KEYSTRO)t.Tag, t, REG_DWORD);
        }
        private void qWORD64位值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewValue((KEYSTRO)t.Tag, t, REG_QWORD);
        }
        private void 多字符串值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewValue((KEYSTRO)t.Tag, t, REG_MULTI_SZ);
        }
        private void 可扩充字符串值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewValue((KEYSTRO)t.Tag, t, REG_EXPAND_SZ);
        }
        private void 二进制值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView.SelectedNode;
            if (t != null && t.Tag != null)
                NewValue((KEYSTRO)t.Tag, t, REG_BINARY);
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null)
            {
                KEYSTRO eYSTRO = (KEYSTRO)e.Node.Tag;
                if (eYSTRO.NewItem)
                {
                    eYSTRO.NewItem = false;
                    string newText = e.Label == null ? e.Node.Name : e.Label;
                    if (MREG_CreateSubKey(eYSTRO.RootKey, eYSTRO.KeyPath, newText))
                    {
                        e.Node.Tag = new KEYSTRO(eYSTRO.RootKey, eYSTRO.KeyPath + "\\" + newText);
                        e.Node.Name = newText;
                    }
                    else
                    {
                        e.Node.Remove();
                        MessageBox.Show("无法创建新项\n" + MREG_GetLastErrString(), "创建新项", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
                else if(e.Label != null)
                {
                    if (!MREG_RenameKey(eYSTRO.RootKey, eYSTRO.KeyPath, e.Label))
                    {
                        e.CancelEdit = true;
                        MessageBox.Show("无法重命名项\n" + MREG_GetLastErrString(), "重命名", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
            }
        }
        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem li = listView.Items[e.Item];
            VALSTRO vALSTRO = (VALSTRO)li.Tag;
            if (vALSTRO.NewItem)
            {
                string newValueName = e.Label == null ? vALSTRO.ValueName : e.Label;
                if (!MREG_CreateValue(vALSTRO.Key.RootKey, vALSTRO.Key.KeyPath, newValueName))
                {
                    listView.Items.Remove(li);
                    MessageBox.Show("无法创建新值\n" + MREG_GetLastErrString(), "创建新值", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    vALSTRO.ValueName = newValueName;
                    vALSTRO.NewItem = false;
                }
            }
            else if (e.Label != null && e.Label != "")
            {
                if (!MREG_RenameValue(vALSTRO.Key.RootKey, vALSTRO.Key.KeyPath, vALSTRO.ValueName, e.Label))
                {
                    e.CancelEdit = true;
                    MessageBox.Show("无法重命名值\n" + MREG_GetLastErrString(), "重命名值", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void treeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                if (e.KeyCode == Keys.Delete)
                    删除ToolStripMenuItem_Click(sender, e);
                else if (e.KeyCode == Keys.F2)
                    重命名ToolStripMenuItem_Click(sender, e);
            }
        }
        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                if (e.KeyCode == Keys.Delete)
                    删除ToolStripMenuItem1_Click(sender, e);
                else if (e.KeyCode == Keys.F2)
                    重命名ToolStripMenuItem1_Click(sender, e);
            }
        }

        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("删除某些注册表值会引起系统不稳定。确实要永久删除此数值吗?", "确认数值删除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    VALSTRO vALSTRO = (VALSTRO)listView.SelectedItems[0].Tag;
                    if(MREG_DeleteKeyValue(vALSTRO.Key.RootKey, vALSTRO.Key.KeyPath, vALSTRO.ValueName))                  
                        listView.Items.Remove(listView.SelectedItems[0]);                   
                    else                  
                        MessageBox.Show("无法删除数值\n" + MREG_GetLastErrString(), "数值删除", MessageBoxButtons.OK, MessageBoxIcon.Hand);                    
                }
            }
        }
        private void 重命名ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
                listView.SelectedItems[0].BeginEdit();
        }
        private void 修改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem li = listView.SelectedItems[0];
                VALSTRO vALSTRO = (VALSTRO)li.Tag;

            }
        }
        private void 修改二进制数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {

            }
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PCMgrUWP.Caller.ShowAboutDlg();
        }
    }
}
