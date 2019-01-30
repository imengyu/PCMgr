using PCMgr.Ctls;
using PCMgr.Lanuages;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.NativeMethods;
using PCMgr.Helpers;

namespace PCMgr.Main
{
    class MainPageStartMgr : MainPage
    {
        private TaskMgrList listStartup;
        private MainSettings mainSettings = null;

        public MainPageStartMgr(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageStartCtl)
        {
            listStartup = formMain.listStartup;
        }

        protected override void OnLoadControlEvents()
        {
            listStartup.MouseClick += listStartup_MouseClick;
            listStartup.KeyDown += listStartup_KeyDown;
            listStartup.Header.CloumClick += listStartup_Header_CloumClick;

            base.OnLoadControlEvents();
        }
        protected override void OnLoad()
        {
            mainSettings = FormMain.MainSettings;
            base.OnLoad();
        }

        //启动项 页面（借鉴了PCHunter）

        private TaskMgrListItemGroup knowDlls = null;
        private TaskMgrListItemGroup rightMenu1 = null;
        private TaskMgrListItemGroup rightMenu2 = null;
        private TaskMgrListItemGroup rightMenu3 = null;
        private TaskMgrListItemGroup printMonitors = null;
        private TaskMgrListItemGroup printProviders = null;

        private MainPageUwpMgr.TaskListViewUWPColumnSorter startColumnSorter = new MainPageUwpMgr.TaskListViewUWPColumnSorter();
        private static uint startId = 0;

        private struct startitem
        {
            public uint id;
            public startitem(string s, IntPtr rootregpath, string path, string valuename)
            {
                this.filepath = s; this.rootregpath = rootregpath;
                this.path = path;
                this.valuename = valuename;
                id = startId++;
            }
            public string valuename;
            public string path;
            public string filepath;
            public IntPtr rootregpath;
        }
        public void StartMListInit()
        {
            if (!Inited)
            {
                StartMListLoadCols();

                listStartup.Header.Height = 36;
                listStartup.ReposVscroll();
                listStartup.ListViewItemSorter = startColumnSorter;

                NativeBridge.enumStartupsCallBack = StartMList_CallBack;
                NativeBridge.enumStartupsCallBackPtr = Marshal.GetFunctionPointerForDelegate(NativeBridge.enumStartupsCallBack);

                knowDlls = new TaskMgrListItemGroup("Know Dlls");
                knowDlls.Text = "Know Dlls";
                knowDlls.Icon = Properties.Resources.icoFiles;
                for (int i = 0; i < 5; i++)
                    knowDlls.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                knowDlls.Type = TaskMgrListItemType.ItemGroup;
                knowDlls.DisplayChildCount = true;
                rightMenu1 = new TaskMgrListItemGroup("RightMenu 1");
                rightMenu1.Text = "RightMenu 1";
                for (int i = 0; i < 5; i++)
                    rightMenu1.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                rightMenu1.Type = TaskMgrListItemType.ItemGroup;
                rightMenu1.DisplayChildCount = true;
                rightMenu1.Image = Properties.Resources.iconContextMenu;
                rightMenu2 = new TaskMgrListItemGroup("RightMenu 2");
                rightMenu2.Text = "RightMenu 2";
                for (int i = 0; i < 5; i++)
                    rightMenu2.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                rightMenu2.Type = TaskMgrListItemType.ItemGroup;
                rightMenu2.DisplayChildCount = true;
                rightMenu2.Image = Properties.Resources.iconContextMenu;
                rightMenu3 = new TaskMgrListItemGroup("RightMenu 3");
                rightMenu3.Text = "RightMenu 3";
                for (int i = 0; i < 5; i++)
                    rightMenu3.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                rightMenu3.Type = TaskMgrListItemType.ItemGroup;
                rightMenu3.DisplayChildCount = true;
                rightMenu3.Image = Properties.Resources.iconContextMenu;

                printMonitors = new TaskMgrListItemGroup("PrintMonitors");
                printMonitors.Text = "PrintMonitors";
                for (int i = 0; i < 5; i++)
                    printMonitors.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                printMonitors.Type = TaskMgrListItemType.ItemGroup;
                printMonitors.DisplayChildCount = true;
                printMonitors.Icon = Properties.Resources.icoWins;

                printProviders = new TaskMgrListItemGroup("PrintProviders");
                printProviders.Text = "PrintProviders";
                for (int i = 0; i < 5; i++)
                    printProviders.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                printProviders.Type = TaskMgrListItemType.ItemGroup;
                printProviders.DisplayChildCount = true;
                printProviders.Icon = Properties.Resources.icoWins;

                StringBuilder sbBIOSTime = new StringBuilder(6);
                if (MREG_GetBIOSTime(sbBIOSTime))
                {
                    FormMain.pl_bios_time.Show();
                    FormMain.lbBiosTime.Text = sbBIOSTime.ToString() + " " + LanuageFBuffers.Str_Second;
                    listStartup.Top = 30;
                    listStartup.Height -= 30;
                }

                Inited = true;

                StartMListRefesh();
                
            }
        }
        public void StartMListRefesh()
        {
            knowDlls.Childs.Clear();
            rightMenu2.Childs.Clear();
            rightMenu1.Childs.Clear();
            listStartup.Items.Clear();
            startId = 0;
            
            MEnumStartups(NativeBridge.enumStartupsCallBackPtr);

            if (knowDlls.Childs.Count > 0) listStartup.Items.Add(knowDlls);
            if (rightMenu1.Childs.Count > 0) listStartup.Items.Add(rightMenu1);
            if (rightMenu2.Childs.Count > 0) listStartup.Items.Add(rightMenu2);
            if (rightMenu3.Childs.Count > 0) listStartup.Items.Add(rightMenu3);
            if (printMonitors.Childs.Count > 0) listStartup.Items.Add(printMonitors);
            if (printProviders.Childs.Count > 0) listStartup.Items.Add(printProviders);
        }
        private void StartMList_CallBack(IntPtr name, IntPtr type, IntPtr path, IntPtr rootregpath, IntPtr regpath, IntPtr regvalue)
        {
            bool settoblue = false;
            TaskMgrListItem li = new TaskMgrListItem(Marshal.PtrToStringUni(name));
            for (int i = 0; i < 5; i++) li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listStartup.Font });
            li.IsFullData = true;
            li.SubItems[0].Text = li.Text;
            // li.SubItems[1].Text = Marshal.PtrToStringUni(type);
            li.Type = TaskMgrListItemType.ItemMain;
            StringBuilder filePath = null;
            if (path != IntPtr.Zero)
            {
                string pathstr = Marshal.PtrToStringUni(path);
                if (!pathstr.StartsWith("\"")) { pathstr = "\"" + pathstr + "\""; }
                li.SubItems[1].Text = (pathstr);
                filePath = new StringBuilder(260);
                if (MCommandLineToFilePath(pathstr, filePath, 260))
                {
                    li.SubItems[2].Text = filePath.ToString();
                    pathstr = filePath.ToString();
                    if (MFM_FileExist(pathstr))
                    {
                        li.Icon = Icon.FromHandle(MGetExeIcon(pathstr));
                        StringBuilder exeCompany = new StringBuilder(256);
                        if (MGetExeCompany(pathstr, exeCompany, 256))
                        {
                            li.SubItems[3].Text = exeCompany.ToString();
                            if (mainSettings.HighlightNoSystem && li.SubItems[3].Text != ConstVals.MICROSOFT)
                                settoblue = true;
                        }
                        else if (mainSettings.HighlightNoSystem)
                            settoblue = true;
                    }
                    else if (MFM_FileExist("C:\\WINDOWS\\system32\\" + pathstr))
                    {
                        if (pathstr.EndsWith(".exe"))
                            li.Icon = Icon.FromHandle(MGetExeIcon(@"C:\Windows\System32\" + pathstr));
                        StringBuilder exeCompany = new StringBuilder(256);
                        if (MGetExeCompany(@"C:\Windows\System32\" + pathstr, exeCompany, 256))
                        {
                            li.SubItems[3].Text = exeCompany.ToString();
                            if (mainSettings.HighlightNoSystem && li.SubItems[3].Text != ConstVals.MICROSOFT)
                                settoblue = true;
                        }
                        else if (mainSettings.HighlightNoSystem)
                            settoblue = true;
                    }
                    else if (MFM_FileExist("C:\\WINDOWS\\SysWOW64\\" + pathstr))
                    {
                        if (pathstr.EndsWith(".exe"))
                            li.Icon = Icon.FromHandle(MGetExeIcon(@"C:\Windows\SysWOW64\" + pathstr));
                        StringBuilder exeCompany = new StringBuilder(256);
                        if (MGetExeCompany(@"C:\Windows\SysWOW64\" + pathstr, exeCompany, 256))
                        {
                            li.SubItems[3].Text = exeCompany.ToString();
                            if (mainSettings.HighlightNoSystem && li.SubItems[3].Text != ConstVals.MICROSOFT)
                                settoblue = true;
                        }
                        else if (mainSettings.HighlightNoSystem)
                            settoblue = true;
                    }
                    else if (pathstr.StartsWith("wow64") && pathstr.EndsWith(".dll"))
                    {
#if !_X64_
                        if (!MIs64BitOS())
                        {
                            if (mainSettings.HighlightNoSystem)
                                settoblue = true;
                            li.SubItems[3].Text = LanuageFBuffers.Str_FileNotExist;
                        }
#endif
                        if (pathstr != "wow64.dll" && pathstr != "wow64cpu.dll" && pathstr != "wow64win.dll")
                        {
                            if (mainSettings.HighlightNoSystem)
                                settoblue = true;
                            li.SubItems[3].Text = LanuageFBuffers.Str_FileNotExist;
                        }
                    }
                    else
                    {
                        if (mainSettings.HighlightNoSystem)
                            settoblue = true;
                        li.SubItems[3].Text = LanuageFBuffers.Str_FileNotExist;
                    }
                }
            }

            string rootkey = Marshal.PtrToStringUni(MREG_ROOTKEYToStr(rootregpath));
            string regkey = rootkey + "\\" + Marshal.PtrToStringUni(regpath);
            string regvalues = Marshal.PtrToStringUni(regvalue);
            li.SubItems[4].Text = regkey + "\\" + regvalues;
            li.Tag = new startitem(filePath == null ? null : filePath.ToString(), rootregpath, Marshal.PtrToStringUni(regpath), regvalues);

            string typestr = Marshal.PtrToStringUni(type);
            if (typestr == "KnownDLLs")
            {
                li.Image = FormMain.imageListFileTypeList.Images[".dll"];
                knowDlls.Childs.Add(li);
            }
            else if (typestr == "RightMenu1")
                rightMenu1.Childs.Add(li);
            else if (typestr == "RightMenu2")
                rightMenu2.Childs.Add(li);
            else if (typestr == "RightMenu3")
                rightMenu3.Childs.Add(li);
            else if (typestr == "PrintMonitors")
                printMonitors.Childs.Add(li);
            else if (typestr == "PrintProviders")
                printProviders.Childs.Add(li);

            else listStartup.Items.Add(li);
            if (settoblue)
                for (int i = 0; i < 5; i++)
                    li.SubItems[i].ForeColor = Color.Blue;
        }
        public void StartMListRemoveItem(uint id)
        {
            TaskMgrListItem target = null;
            foreach (TaskMgrListItem li in listStartup.Items)
            {
                if (li.Tag != null)
                {
                    startitem item = (startitem)li.Tag;
                    if (item.id == id)
                    {
                        target = li;
                        break;
                    }
                }
            }
            if (target != null)
            {
                listStartup.Items.Remove(target);
                listStartup.SyncItems(true);
            }
        }
        private void StartMListLoadCols()
        {
            TaskMgrListHeaderItem li = new TaskMgrListHeaderItem();
            li.TextSmall = LanuageMgr.GetStr("TitleName", false);
            li.Width = 200;
            listStartup.Colunms.Add(li);
            TaskMgrListHeaderItem li2 = new TaskMgrListHeaderItem();
            li2.TextSmall = LanuageMgr.GetStr("TitleCmdLine", false);
            li2.Width = 200;
            listStartup.Colunms.Add(li2);
            TaskMgrListHeaderItem li3 = new TaskMgrListHeaderItem();
            li3.TextSmall = LanuageMgr.GetStr("TitleFilePath", false);
            li3.Width = 200;
            listStartup.Colunms.Add(li3);
            TaskMgrListHeaderItem li4 = new TaskMgrListHeaderItem();
            li4.TextSmall = LanuageMgr.GetStr("TitlePublisher", false);
            li4.Width = 100;
            listStartup.Colunms.Add(li4);
            TaskMgrListHeaderItem li5 = new TaskMgrListHeaderItem();
            li5.TextSmall = LanuageMgr.GetStr("TitleRegPath", false);
            li5.Width = 200;
            listStartup.Colunms.Add(li5);

        }

        public void StartMListExpandAll()
        {
            listStartup.Locked = true;
            foreach (TaskMgrListItem li in listStartup.Items)
            {
                if (li.Childs.Count > 0 && !li.ChildsOpened)
                    li.ChildsOpened = true;
            }
            listStartup.Locked = false;
            listStartup.SyncItems(true);
        }
        public void StartMListCollapseAll()
        {
            listStartup.Locked = true;
            foreach (TaskMgrListItem li in listStartup.Items)
            {
                if (li.Childs.Count > 0 && li.ChildsOpened)
                    li.ChildsOpened = false;
            }
            listStartup.Locked = false;
            listStartup.SyncItems(true);
        }

        private void listStartup_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listStartup.SelectedItem != null)
                {
                    TaskMgrListItem selectedItem = listStartup.SelectedItem.OldSelectedItem == null ?
                 listStartup.SelectedItem : listStartup.SelectedItem.OldSelectedItem;
                    if (selectedItem.Type == TaskMgrListItemType.ItemMain)
                    {
                        startitem item = (startitem)selectedItem.Tag;
                        MStartupsMgr_ShowMenu(item.rootregpath, item.path, item.filepath, item.valuename, item.id, 0, 0);
                    }
                }
            }
        }
        private void listStartup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listStartup.SelectedItem != null)
                {
                    Point p = listStartup.GetiItemPoint(listStartup.SelectedItem);
                    p = listStartup.PointToScreen(p);
                    startitem item = (startitem)listStartup.SelectedItem.Tag;
                    MStartupsMgr_ShowMenu(item.rootregpath, item.path, item.filepath, item.valuename, item.id, p.X, p.Y);
                }
            }
        }
        private void listStartup_Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left)
            {
                listStartup.Locked = true;
                if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.None)
                    startColumnSorter.Order = SortOrder.Ascending;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                    startColumnSorter.Order = SortOrder.Ascending;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    startColumnSorter.Order = SortOrder.Descending;
                startColumnSorter.SortColumn = e.Index;
                listStartup.Locked = false;
                listStartup.Sort();
            }
        }
    }
}
