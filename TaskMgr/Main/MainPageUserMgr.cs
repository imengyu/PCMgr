using PCMgr.Ctls;
using PCMgr.Lanuages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static PCMgr.Main.MainUtils;
using static PCMgr.NativeMethods;

namespace PCMgr.Main
{
    class MainPageUserMgr : MainPage
    {
        private TaskMgrList listUsers;

        private MainPageProcess mainPageProcess = null;

        public MainPageUserMgr(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageUsers)
        {
            listUsers = formMain.listUsers;
        }

        protected override void OnLoad()
        {
            mainPageProcess = FormMain.MainPageProcess;
            base.OnLoad();
        }
        protected override void OnLoadControlEvents()
        {
            listUsers.KeyDown += listUsers_KeyDown;
            listUsers.MouseClick += listUsers_MouseClick;
            listUsers.Header.CloumClick += listUsers_Header_CloumClick;

            base.OnLoadControlEvents();
        }

        //用户页代码

        public void UsersListInit()
        {
            if (!Inited)
            {
                NativeBridge.enumUsersCallBack = UsersListEnumUsersCallBack;
                NativeBridge.enumUsersCallBackCallBack_ptr = Marshal.GetFunctionPointerForDelegate(NativeBridge.enumUsersCallBack);

                if (mainPageProcess.Inited) mainPageProcess.ProcessListRefesh2();

                listViewItemCompareUsers = new ListViewItemComparerUsers();
                listViewItemCompareUsers.Order = SortOrder.Descending;

                UsersListLoadCols();

                listUsers.ListViewItemSorter = listViewItemCompareUsers;
                listUsers.Colunms[0].ArrowType = TaskMgrListHeaderSortArrow.Descending;
                listUsers.Header.Height = 36;
                listUsers.ReposVscroll();
                listUsers.DrawIcon = true;

                Inited = true;

                UsersListLoad();
            }
        }
        public void UsersListUnInit()
        {
            if (Inited)
            {
                listUsers.Items.Clear();

                listViewItemCompareUsers = null;
                Inited = false;
            }
        }
        public void UsersListLoad()
        {
            listUsers.Items.Clear();
            NativeMethods.M_User_EnumUsers(NativeBridge.enumUsersCallBackCallBack_ptr, IntPtr.Zero);
            listUsers.Sort();
        }
        private void UsersListLoadCols() {
            TaskMgrListHeaderItem li13 = new TaskMgrListHeaderItem();
            li13.TextSmall = LanuageMgr.GetStr("TitleName", false);
            li13.Width = 550;
            listUsers.Colunms.Add(li13);
            TaskMgrListHeaderItem li14 = new TaskMgrListHeaderItem();
            li14.TextSmall = "ID";
            li14.Width = 50;
            listUsers.Colunms.Add(li14);
            TaskMgrListHeaderItem li15 = new TaskMgrListHeaderItem();
            li15.TextSmall = LanuageMgr.GetStr("TitleSessionID", false);
            li15.Width = 50;
            listUsers.Colunms.Add(li15);
            TaskMgrListHeaderItem li16 = new TaskMgrListHeaderItem();
            li16.TextSmall = LanuageMgr.GetStr("TitleDomainName", false);
            li16.Width = 130;
            listUsers.Colunms.Add(li16);
        }
        private void UsersListAddProcess(TaskMgrListItem li, string userName)
        {
            //添加用户对应的所有进程

            List<PsItem> loadedPs = mainPageProcess.GetLoadedPs();
            foreach (PsItem p in loadedPs)
            {
                if (p.username == userName)
                {
                    TaskMgrListItemChild child = new TaskMgrListItemChild(p.item.Text, p.item.Icon);
                    if (p.item.IsUWPICO) child.IsUWPICO = true;
                    li.Childs.Add(child);
                }
            }

            li.Childs.Sort(listViewItemCompareUsers);
        }


        private void listUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listUsers.SelectedItem != null && !listUsers.SelectedItem.IsChildItem && listUsers.SelectedChildItem == null)
                {
                    Point p = listUsers.GetItemPoint(listUsers.SelectedItem);
                    p = listUsers.PointToScreen(p);

                    MAppWorkCall3(212, new IntPtr(p.X), new IntPtr(p.Y));
                    IntPtr str = Marshal.StringToHGlobalUni(listUsers.SelectedItem.SubItems[0].Text);
                    MAppWorkCall3(170, Handle, str);
                    Marshal.FreeHGlobal(str);
                    MAppWorkCall3(175, Handle, new IntPtr((int)(uint)listUsers.SelectedItem.Tag));
                }
            }
        }
        private void listUsers_MouseClick(object sender, MouseEventArgs e)
        {
            if (listUsers.SelectedItem != null && !listUsers.SelectedItem.IsChildItem && listUsers.SelectedChildItem == null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    MAppWorkCall3(212, new IntPtr(MousePosition.X), new IntPtr(MousePosition.Y));
                    IntPtr str = Marshal.StringToHGlobalUni(listUsers.SelectedItem.SubItems[0].Text);
                    MAppWorkCall3(170, Handle, str);
                    Marshal.FreeHGlobal(str);
                    MAppWorkCall3(175, Handle, new IntPtr((int)(uint)listUsers.SelectedItem.Tag));
                }
            }
        }
        private void listUsers_Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left && e.Index == 0)
            {
                listUsers.Locked = true;
                if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.None)
                    listViewItemCompareUsers.Asdening = true;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                    listViewItemCompareUsers.Asdening = true;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    listViewItemCompareUsers.Asdening = false;

                foreach (TaskMgrListItem li in listUsers.Items)
                    if (li.Childs.Count > 0)
                        li.Childs.Sort(listViewItemCompareUsers);

                listUsers.Sort();
                listUsers.Locked = false;
                listUsers.Invalidate();
            }
        }

        private ListViewItemComparerUsers listViewItemCompareUsers = null;
        private class ListViewItemComparerUsers : ListViewColumnSorter
        {
            public bool Asdening
            {
                get
                {
                    return Order == SortOrder.Ascending;
                }
                set
                {
                    if (value) Order = SortOrder.Ascending;
                    else Order = SortOrder.Descending;
                }
            }


            public override int Compare(TaskMgrListItem x, TaskMgrListItem y)
            {
                int returnVal = -1;
                if (x is TaskMgrListItemChild && y is TaskMgrListItemChild)
                    returnVal = String.Compare(x.Text, y.Text);
                else
                {
                    UInt64 xi, yi;
                    if (UInt64.TryParse(x.SubItems[0].Text, out xi) && UInt64.TryParse(y.SubItems[0].Text, out yi))
                    {
                        if (x.SubItems[0].Text == y.SubItems[0].Text) returnVal = 0;
                        else if (xi > yi) returnVal = 1;
                        else if (xi < yi) returnVal = -1;
                    }
                    else returnVal = String.Compare(((ListViewItem)x).SubItems[0].Text, ((ListViewItem)y).SubItems[0].Text);
                }
                if (Asdening) returnVal = -returnVal;
                return returnVal;
            }
        }

        private bool UsersListEnumUsersCallBack(IntPtr userName, uint sessionId, uint userId, IntPtr domain, IntPtr customData)
        {
            string username = Marshal.PtrToStringUni(userName);
            string domainStr = Marshal.PtrToStringUni(domain);
            TaskMgrListItem li = new TaskMgrListItem(username);
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems[0].Text = username;
            li.SubItems[1].Text = userId.ToString();
            li.SubItems[2].Text = sessionId.ToString();
            li.SubItems[3].Text = domainStr;
            li.SubItems[0].Font = listUsers.Font;
            li.SubItems[1].Font = listUsers.Font;
            li.SubItems[2].Font = listUsers.Font;
            li.SubItems[3].Font = listUsers.Font;
            li.Tag = sessionId;
            li.DisplayChildCount = false;
            li.IsUWPICO = true;

            string userFullName, userIcoPath;
            if (UsersListEnumGetUserInfos(username, out userIcoPath, out userFullName))
            {
                li.Text = userFullName + " (" + username + ")";
                li.SubItems[0].Text = li.Text;
            }

            UsersListAddProcess(li, username);
            if (li.Childs.Count == 0) li.DisplayChildCount = false;

            listUsers.Items.Add(li);
            return true;
        }

        private bool UsersListEnumGetUserInfos(string username, out string userIcoPath, out string userFullName)
        {
            IntPtr userIcoPathBuf = Marshal.AllocHGlobal(520);
            IntPtr userFullNameBuf = Marshal.AllocHGlobal(512);
            if (NativeMethods.M_User_GetUserInfo(username, userIcoPathBuf, userFullNameBuf, 256))
            {
                userFullName = Marshal.PtrToStringUni(userFullNameBuf);
                userIcoPath = Marshal.PtrToStringUni(userIcoPathBuf);
                Marshal.FreeHGlobal(userFullNameBuf);
                Marshal.FreeHGlobal(userIcoPathBuf);
                return true;
            }
            else
            {
                userFullName = "";
                userIcoPath = "";
                return false;
            }
        }
    }
}
