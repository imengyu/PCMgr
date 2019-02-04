using PCMgr.Aero.TaskDialog;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.NativeMethods;

namespace PCMgr.Main
{
    class MainPageFileMgr : MainPage
    {
        private ListView listFm;
        private TreeView treeFmLeft;
        private ImageList imageListFileMgrLeft;
        private ImageList imageListFileTypeList;
        private Label lbFileMgrStatus;
        private TextBox textBoxFmCurrent;
        private FileSystemWatcher fileSystemWatcher;

        public MainPageFileMgr(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageFileCtl)
        {
            listFm = formMain.listFm;
            treeFmLeft = formMain.treeFmLeft;
            imageListFileMgrLeft = formMain.imageListFileMgrLeft;
            imageListFileTypeList = formMain.imageListFileTypeList;
            lbFileMgrStatus = formMain.lbFileMgrStatus;
            textBoxFmCurrent = formMain.textBoxFmCurrent;
            fileSystemWatcher = formMain.fileSystemWatcher;
        }

        protected override void OnLoadControlEvents()
        {
            treeFmLeft.BeforeExpand += new TreeViewCancelEventHandler(treeFmLeft_BeforeExpand);
            treeFmLeft.AfterSelect += new TreeViewEventHandler(treeFmLeft_AfterSelect);
            treeFmLeft.NodeMouseClick += new TreeNodeMouseClickEventHandler(treeFmLeft_NodeMouseClick);
            treeFmLeft.KeyDown += new KeyEventHandler(treeFmLeft_KeyDown);
            treeFmLeft.MouseClick += new MouseEventHandler(treeFmLeft_MouseClick);

            listFm.AfterLabelEdit += new LabelEditEventHandler(listFm_AfterLabelEdit);
            listFm.SelectedIndexChanged += new EventHandler(listFm_SelectedIndexChanged);
            listFm.MouseClick += new MouseEventHandler(listFm_MouseClick);
            listFm.MouseDoubleClick += new MouseEventHandler(listFm_MouseDoubleClick);

            fileSystemWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
            fileSystemWatcher.Created += new FileSystemEventHandler(fileSystemWatcher_Created);
            fileSystemWatcher.Deleted += new FileSystemEventHandler(fileSystemWatcher_Deleted);
            fileSystemWatcher.Renamed += new RenamedEventHandler(fileSystemWatcher_Renamed);

            textBoxFmCurrent.KeyDown += textBoxFmCurrent_KeyDown;

            FormMain.btnFmAddGoto.Click += new EventHandler(btnFmAddGoto_Click);

            base.OnLoadControlEvents();
        }

        //文件管理器页代码

        private Dictionary<string, string> fileTypeNames = new Dictionary<string, string>();
        private TreeNode lastClickTreeNode = null;
        private string lastShowDir = "";
        private bool lastRightClicked = false;

        public void FileMgrInit()
        {
            if (!Inited)
            {
                FormMain.DelingDialogInit();

                NativeBridge.fileMgrCallBack = FileMgrCallBack;
                MFM_SetCallBack(Marshal.GetFunctionPointerForDelegate(NativeBridge.fileMgrCallBack));

                imageListFileMgrLeft.Images.Add("folder", Icon.FromHandle(MFM_GetFolderIcon()));
                imageListFileMgrLeft.Images.Add("mycp", Icon.FromHandle(MFM_GetMyComputerIcon()));

                imageListFileTypeList.Images.Add("folder", Icon.FromHandle(MFM_GetFolderIcon()));

                MAppWorkCall3(182, treeFmLeft.Handle, IntPtr.Zero);
                MAppWorkCall3(182, listFm.Handle, IntPtr.Zero);

                Inited = true;

                string smycp = Marshal.PtrToStringAuto(MFM_GetMyComputerName());
                treeFmLeft.Nodes.Add("mycp", smycp, "mycp", "mycp").Tag = "mycp";
                MFM_GetRoots();
            }
        }
        
        public void FileMgrShowFiles(string path)
        {
            if (path == null)
            {
                path = lastShowDir;
                lastShowDir = null;
            }
            if (lastShowDir != path)
            {
                fileSystemWatcher.EnableRaisingEvents = true;
                lastShowDir = path;
                listFm.Items.Clear();
                if (lastShowDir == "mycp" || lastShowDir == "\\\\")
                {
                    for (int i = 0; i < treeFmLeft.Nodes[0].Nodes.Count; i++)
                        listFm.Items.Add(new ListViewItem(treeFmLeft.Nodes[0].Nodes[i].Text, treeFmLeft.Nodes[0].Nodes[i].ImageKey) { Tag = "..\\ROOT\\" + treeFmLeft.Nodes[0].Nodes[i].Tag });
                    textBoxFmCurrent.Text = treeFmLeft.Nodes[0].Text;
                }
                else
                {
                    MFM_GetFiles(lastShowDir);
                    textBoxFmCurrent.Text = lastShowDir;
                }
                if (path == "mycp") fileSystemWatcher.EnableRaisingEvents = false;
                else fileSystemWatcher.Path = path;

                FileMgrUpdateStatus(1);
            }
        }
        public void FileMgrUpdateStatus(int i)
        {
            FileMgrCallBack(15, new IntPtr(i), IntPtr.Zero);
        }
        public void FileMgrTreeOpenItem(TreeNode n)
        {
            if (n.Nodes.Count == 0 || n.Nodes[0].Text == LanuageFBuffers.Str_Loading && n.Tag != null)
            {
                lastClickTreeNode = n;
                string s = n.Tag.ToString();
                if (MFM_GetFolders(s))
                    lastClickTreeNode.Nodes.Remove(lastClickTreeNode.Nodes[0]);
            }
        }

        private IntPtr FileMgrCallBack(int msg, IntPtr lParam, IntPtr wParam)
        {
            switch (msg)
            {
                case 2:
                    {
                        string s = Marshal.PtrToStringAuto(lParam);
                        string path = Marshal.PtrToStringAuto(wParam);
                        Icon icon = Icon.FromHandle(MFM_GetFileIcon(path, null, 0));
                        imageListFileMgrLeft.Images.Add(path, icon);
                        imageListFileTypeList.Images.Add(path, icon);
                        TreeNode n = treeFmLeft.Nodes[0].Nodes.Add(path, s, path, path);
                        n.Tag = path;
                        n.Nodes.Add("loading", LanuageFBuffers.Str_Loading, "loading", "loading");
                        break;
                    }
                case 3:
                    {
                        if (wParam.ToInt32() == -1)
                        {
                            lastClickTreeNode.Nodes[0].Text = LanuageFBuffers.Str_VisitFolderFailed;
                            lastClickTreeNode.Nodes[0].ImageKey = "err";
                        }
                        else
                        {
                            string s = Marshal.PtrToStringAuto(lParam);
                            string path = Marshal.PtrToStringAuto(wParam);
                            TreeNode n = lastClickTreeNode.Nodes.Add(s, s, "folder", "folder");
                            if (path.EndsWith("\\"))
                                n.Tag = path + s;
                            else n.Tag = path + "\\" + s;
                            n.Nodes.Add("loading", LanuageFBuffers.Str_Loading, "loading", "loading");
                        }
                        break;
                    }
                case 5:
                    {
                        string s = Marshal.PtrToStringAuto(lParam);
                        string path = Marshal.PtrToStringAuto(wParam);
                        listFm.Items.Add(new ListViewItem(s, "folder") { Tag = path.EndsWith("\\") ? path + s : path + "\\" + s });
                        break;
                    }
                case 6:
                case 26:
                    {
                        if (wParam.ToInt32() == -1)
                        {
                            listFm.Items.Clear();
                            string path = Marshal.PtrToStringAuto(lParam);
                            listFm.Items.Add(new ListViewItem("..", "folder") { Tag = "..\\back\\" + path });
                            ListViewItem lvi = listFm.Items.Add(LanuageFBuffers.Str_VisitFolderFailed, "err");
                        }
                        else
                        {
                            ListViewItem it = null;
                            WIN32_FIND_DATA data = default(WIN32_FIND_DATA);
                            data = (WIN32_FIND_DATA)Marshal.PtrToStructure(lParam, data.GetType());
                            string s = data.cFileName;
                            string path = Marshal.PtrToStringAuto(wParam);
                            string fpath = path + "\\" + s;
                            fpath = fpath.Replace("\\\\", "\\");
                            string fext = "*" + Path.GetExtension(fpath);
                            if (fext == "") fext = "*.*";
                            if (fext == "*.exe")
                            {
                                if (!imageListFileTypeList.Images.ContainsKey(fpath) && MFM_FileExist(fpath))
                                {
                                    StringBuilder sb0 = new StringBuilder(260);
                                    IntPtr h = MGetExeIcon(fpath);
                                    if (h != IntPtr.Zero)
                                        imageListFileTypeList.Images.Add(fpath, Icon.FromHandle(h));
                                    if (!fileTypeNames.ContainsKey(fpath))
                                    {
                                        MGetExeDescribe(fpath, sb0, 260);
                                        fileTypeNames.Add(fpath, sb0.ToString());
                                    }
                                    sb0 = null;
                                }
                                if (msg == 26)
                                {
                                    foreach (ListViewItem i in listFm.Items)
                                    {
                                        if (i.Tag.ToString() == fpath)
                                        {
                                            it = i;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    it = listFm.Items.Add(new ListViewItem(s, fpath) { Tag = fpath });
                                }
                                string typeName = "";
                                if (fileTypeNames.TryGetValue(fpath, out typeName))
                                    it.SubItems.Add(typeName);
                                else it.SubItems.Add("");
                            }
                            else
                            {
                                if (!imageListFileTypeList.Images.ContainsKey(fext))
                                {
                                    StringBuilder sb0 = new StringBuilder(80);
                                    imageListFileTypeList.Images.Add(fext, Icon.FromHandle(MFM_GetFileIcon(fext, sb0, 80)));
                                    if (!fileTypeNames.ContainsKey(fext))
                                        fileTypeNames.Add(fext, sb0.ToString());
                                    else imageListFileTypeList.Images.Add(fext, Icon.FromHandle(MFM_GetFileIcon(fext, null, 0)));
                                    sb0 = null;
                                }
                                if (msg == 26)
                                {
                                    foreach (ListViewItem i in listFm.Items)
                                    {
                                        if (i.Tag.ToString() == fpath)
                                        {
                                            it = i;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    it = listFm.Items.Add(new ListViewItem(s, fext) { Tag = fpath });
                                }

                                string typeName = "";
                                if (fileTypeNames.TryGetValue(fext, out typeName))
                                    it.SubItems.Add(typeName);
                                else it.SubItems.Add("");
                            }

                            long size = (data.nFileSizeHigh * 0xffffffff + 1) + data.nFileSizeLow;
                            it.SubItems.Add(FormatFileSize(size));

                            StringBuilder sb = new StringBuilder(26);
                            if (MFM_GetFileTime(ref data.ftCreationTime, sb, 26))
                                it.SubItems.Add(sb.ToString());
                            else it.SubItems.Add("Unknow");

                            StringBuilder sb2 = new StringBuilder(26);
                            if (MFM_GetFileTime(ref data.ftLastWriteTime, sb2, 26))
                                it.SubItems.Add(sb2.ToString());
                            else it.SubItems.Add("Unknow");

                            StringBuilder sb3 = new StringBuilder(32);
                            bool hidden = false;
                            if (MFM_GetFileAttr(data.dwFileAttributes, sb3, 32, ref hidden))
                            {
                                if (hidden)
                                {
                                    it.ForeColor = Color.Gray;
                                    it.SubItems[0].ForeColor = Color.Gray;
                                    it.SubItems[1].ForeColor = Color.Gray;
                                    it.SubItems[2].ForeColor = Color.Gray;
                                    it.SubItems[3].ForeColor = Color.Gray;
                                }
                                it.SubItems.Add(sb3.ToString());
                            }
                            else it.SubItems.Add("");

                        }
                        break;
                    }
                case 7:
                    {
                        string path = Marshal.PtrToStringAuto(wParam);
                        listFm.Items.Add(new ListViewItem("..", "folder") { Tag = "..\\back\\" + path });
                        break;
                    }
                case 8:
                    FileMgrShowFiles(null);
                    break;
                case 9:
                    {
                        if (listFm.SelectedItems.Count > 0)
                        {
                            ListViewItem listViewItem = listFm.SelectedItems[0];
                            string path = listViewItem.Tag.ToString();
                            listViewItem.BeginEdit();
                            currEditingItem = listViewItem;
                        }
                        break;
                    }
                case 10:
                    {
                        ListViewItem listViewItem = listFm.Items.Add(LanuageMgr.GetStr("NewFolder"), "folder");
                        listViewItem.Tag = "newfolder";
                        listViewItem.BeginEdit();
                        currEditingItem = listViewItem;
                        break;
                    }
                case 11:
                    {
                        foreach (ListViewItem i in listFm.Items)
                            i.Selected = true;
                        break;
                    }
                case 12:
                    {
                        foreach (ListViewItem i in listFm.Items)
                            i.Selected = false;
                        break;
                    }
                case 13:
                    {
                        foreach (ListViewItem i in listFm.Items)
                            i.Selected = !i.Selected;
                        break;
                    }
                case 14:
                    lbFileMgrStatus.Text = Marshal.PtrToStringAuto(lParam);
                    break;
                case 15:
                    switch (lParam.ToInt32())
                    {
                        case 0: lbFileMgrStatus.Text = LanuageFBuffers.Str_Ready; break;
                        case 1:
                            {
                                if (listFm.SelectedItems.Count > 0)
                                    lbFileMgrStatus.Text = LanuageFBuffers.Str_ReadyStatus + listFm.Items.Count + LanuageFBuffers.Str_ReadyStatusEnd2 + listFm.SelectedItems.Count + LanuageFBuffers.Str_ReadyStatusEnd;
                                else lbFileMgrStatus.Text = LanuageFBuffers.Str_ReadyStatus + listFm.Items.Count + LanuageFBuffers.Str_ReadyStatusEnd;
                                break;
                            }
                        case 2: lbFileMgrStatus.Text = ""; break;
                        case 3: lbFileMgrStatus.Text = ""; break;
                        case 4: lbFileMgrStatus.Text = ""; break;
                        case 5: lbFileMgrStatus.Text = LanuageMgr.GetStr("FileCuted"); break;
                        case 6: lbFileMgrStatus.Text = LanuageMgr.GetStr("FileCopyed"); break;
                        case 7: lbFileMgrStatus.Text = LanuageMgr.GetStr("NewFolderFailed"); break;
                        case 8: lbFileMgrStatus.Text = LanuageMgr.GetStr("NewFolderSuccess"); break;
                        case 9: lbFileMgrStatus.Text = LanuageMgr.GetStr("PathCopyed"); break;
                        case 10: lbFileMgrStatus.Text = LanuageMgr.GetStr("FolderCuted"); break;
                        case 11: lbFileMgrStatus.Text = LanuageMgr.GetStr("FolderCopyed"); break;
                    }

                    break;
                case 16:
                    int index = lParam.ToInt32();
                    if (index > 0 && index < listFm.SelectedItems.Count)
                        return Marshal.StringToHGlobalAuto(listFm.SelectedItems[index].Tag.ToString());
                    break;
                case 17:
                    if (lParam != IntPtr.Zero)
                        Marshal.FreeHGlobal(wParam);
                    break;
                case 18:
                    return FormMain.MainSettings.ShowHiddenFiles ? new IntPtr(1) : new IntPtr(0);
                case 19:
                    FileMgrShowFiles(Marshal.PtrToStringAuto(lParam));
                    break;
                case 20:
                    {
                        new FormCheckFileUse(Marshal.PtrToStringAuto(lParam)).ShowDialog();
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        private void textBoxFmCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnFmAddGoto_Click(sender, e);
        }
        private void btnFmAddGoto_Click(object sender, EventArgs e)
        {

            if (textBoxFmCurrent.Text == "")
                TaskDialog.Show(LanuageMgr.GetStr("PleaseEnterPath"), LanuageFBuffers.Str_TipTitle);
            else
            {
                if (textBoxFmCurrent.Text.StartsWith("\"") && textBoxFmCurrent.Text.EndsWith("\""))
                {
                    textBoxFmCurrent.Text = textBoxFmCurrent.Text.Remove(textBoxFmCurrent.Text.Length - 1, 1);
                    textBoxFmCurrent.Text = textBoxFmCurrent.Text.Remove(0, 1);
                }
                if (Directory.Exists(textBoxFmCurrent.Text))
                    FileMgrShowFiles(textBoxFmCurrent.Text);
                else if (MFM_FileExist(textBoxFmCurrent.Text))
                {
                    string d = Path.GetDirectoryName(textBoxFmCurrent.Text);
                    string f = Path.GetFileName(textBoxFmCurrent.Text);
                    FileMgrShowFiles(d);
                    ListViewItem[] lis = listFm.Items.Find(f, false);
                    if (lis.Length > 0) lis[0].Selected = true;
                }
                else TaskDialog.Show(LanuageMgr.GetStr("PathUnExists"), LanuageFBuffers.Str_TipTitle);
            }
        }
        private void treeFmLeft_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            FileMgrTreeOpenItem(e.Node);
        }
        private void treeFmLeft_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

        }
        private void treeFmLeft_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode n = treeFmLeft.SelectedNode;
            if (n != null && n.Tag != null)
            {
                if (e.Button == MouseButtons.Left)
                    lastRightClicked = false;
                else if (e.Button == MouseButtons.Right)
                {
                    lastRightClicked = true;
                    MAppWorkShowMenuFMF(n.Tag.ToString());
                }
            }
        }
        private void treeFmLeft_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse)
            {
                if (!lastRightClicked)
                {
                    lastClickTreeNode = e.Node;
                    FileMgrShowFiles(lastClickTreeNode.Tag.ToString());
                }
            }
        }
        private void treeFmLeft_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                FileMgrTreeOpenItem(treeFmLeft.SelectedNode);
        }

        private ListViewItem currEditingItem = null;
        private void listFm_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (currEditingItem != null && e.Item == 0)
            {
                string path = currEditingItem.Tag.ToString();
                string targetName = e.Label;
                //Folder
                if (path == "newfolder")
                {
                    if (targetName == "")
                    {
                        targetName = LanuageMgr.GetStr("NewFolder");
                        int ix = 1;
                        string spt = lastShowDir + "\\" + targetName + (ix == 1 ? "" : (" (" + ix + ")"));
                        bool finded = false;
                        while (!finded)
                        {
                            if (Directory.Exists(spt))
                                ix++;
                            else
                            {
                                finded = true;
                                break;
                            }
                        }
                        if (!MFM_CreateDir(spt))
                        {
                            e.CancelEdit = true;
                            listFm.Items.Remove(currEditingItem);
                            FileMgrUpdateStatus(7);
                        }
                        else FileMgrUpdateStatus(8);
                    }
                    else if (MFM_IsValidateFolderFileName(targetName))
                    {
                        string spt = lastShowDir + "\\" + targetName;
                        if (Directory.Exists(spt))
                        {
                            e.CancelEdit = true;
                            listFm.Items.Remove(currEditingItem);
                            TaskDialog.Show(LanuageMgr.GetStr("FolderHasExist"));
                        }
                        else
                        {
                            if (!MFM_CreateDir(spt))
                            {
                                e.CancelEdit = true;
                                listFm.Items.Remove(currEditingItem);
                                FileMgrUpdateStatus(7);
                            }
                            else FileMgrUpdateStatus(8);
                        }
                    }
                    else
                    {
                        e.CancelEdit = true;
                        listFm.Items.Remove(currEditingItem);
                        TaskDialog.Show(LanuageMgr.GetStr("InvalidFileName"));
                    }
                }
                else
                {

                }
            }
            else e.CancelEdit = true;
        }
        private void listFm_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileMgrUpdateStatus(1);
        }
        private void listFm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (listFm.SelectedItems.Count > 0)
                {
                    ListViewItem listViewItem = listFm.SelectedItems[0];
                    string path = listViewItem.Tag.ToString();
                    if (path.StartsWith("..\\back\\"))
                    {
                        path = path.Remove(0, 8);
                        int ix = path.LastIndexOf('\\');
                        if (ix > 0 && ix < path.Length)
                        {
                            path = path.Remove(ix);
                            FileMgrShowFiles(path);
                        }
                    }
                    else
                    {
                        if (listViewItem.ImageKey == "folder" && Directory.Exists(path))
                            FileMgrShowFiles(path);
                        else if (path.StartsWith("..\\ROOT\\"))
                        {
                            path = path.Remove(0, 8);
                            FileMgrShowFiles(path);
                        }
                        else if (MFM_FileExist(path))
                        {
                            if (path.EndsWith(".exe"))
                            {
                                if (TaskDialog.Show(LanuageMgr.GetStr("OpenAsk"), LanuageFBuffers.Str_AskTitle, LanuageMgr.GetStr("PathStart") + path, TaskDialogButton.Yes | TaskDialogButton.No) == Result.Yes)
                                    MFM_OpenFile(path, Handle);
                            }
                            else MFM_OpenFile(path, Handle);
                        }
                    }
                }
            }
        }
        private void listFm_MouseClick(object sender, MouseEventArgs e)
        {
            if (listFm.SelectedItems.Count > 0)
            {
                ListViewItem listViewItem = listFm.SelectedItems[0];
                string path = listViewItem.Tag.ToString();
                if (e.Button == MouseButtons.Right)
                    MAppWorkShowMenuFM(path, listFm.SelectedItems.Count > 1, listFm.SelectedItems.Count);
            }
        }


        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fullpath = e.FullPath;
            MFM_UpdateFile(fullpath, Path.GetDirectoryName(fullpath));
        }
        private void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            string fullpath = e.FullPath;
            MFM_ReUpdateFile(fullpath, Path.GetDirectoryName(fullpath));
            FileMgrUpdateStatus(1);
        }
        private void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            //Remove 
            string fullpath = e.FullPath;
            ListViewItem ii = null;
            foreach (ListViewItem i in listFm.Items)
            {
                if (i.Tag.ToString() == fullpath)
                {
                    ii = i;
                    break;
                }
            }
            listFm.Items.Remove(ii);
            FileMgrUpdateStatus(1);
        }
        private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            string oldfullpath = e.OldFullPath;
            string fullpath = e.FullPath;
            //Rename 
            ListViewItem ii = null;
            foreach (ListViewItem i in listFm.Items)
            {
                if (i.Tag.ToString() == oldfullpath)
                {
                    ii = i;
                    break;
                }
            }
            ii.Tag = fullpath;
            ii.Text = e.Name;
            ii.ImageKey = "*" + Path.GetExtension(fullpath);
        }


    }
}
