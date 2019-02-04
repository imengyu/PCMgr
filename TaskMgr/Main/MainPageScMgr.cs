using PCMgr.Aero.TaskDialog;
using PCMgr.Helpers;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.Main.MainUtils;
using static PCMgr.NativeMethods;

namespace PCMgr.Main
{
    class MainPageScMgr : MainPage
    {
        private ListView listService;

        public MainPageScMgr(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageScCtl)
        {
            listService = formMain.listService;
        }

        protected override void OnLoadControlEvents()
        {
            FormMain.linkRebootAsAdmin.LinkClicked += linkRebootAsAdmin_LinkClicked;
            FormMain.linkOpenScMsc.LinkClicked += linkOpenScMsc_LinkClicked;
            listService.ColumnClick += listService_ColumnClick;
            listService.KeyDown += listService_KeyDown;
            listService.MouseClick += listService_MouseClick;

            base.OnLoadControlEvents();
        }
        protected override void OnLoad()
        {
            mainSettings = FormMain.MainSettings;

            updateScTimer.Interval = 1000;
            updateScTimer.Tick += UpdateScTimer_Tick;

            base.OnLoad();
        }



        //服务管理

        private class ListViewItemComparer : IComparer
        {
            public int SortColum { get; set; }
            public bool Asdening { get; set; } = false;

            public int Compare(object x, object y)
            {
                int returnVal = -1;
                if (((ListViewItem)x).SubItems[SortColum].Text == ((ListViewItem)y).SubItems[SortColum].Text) return -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[SortColum].Text, ((ListViewItem)y).SubItems[SortColum].Text);
                if (Asdening) returnVal = -returnVal;
                return returnVal;
            }
        }

        private MainSettings mainSettings = null;
        private ListViewItemComparer listViewItemComparerSc = new ListViewItemComparer();
        private List<uint> scValidPid = new List<uint>();
        private List<ScItem> runningSc = new List<ScItem>();
        private Icon icoSc = null;
        private Dictionary<string, string> scGroupFriendlyName = new Dictionary<string, string>();
        private bool scCanUse = false;
        private IntPtr hListHeaderSc = IntPtr.Zero;
        private Timer updateScTimer = new Timer();
        private ListViewItem updateScItem = null;

        public Icon IcoSc { get { return icoSc; } }
        public List<uint> ScValidPid { get { return scValidPid; } }
        public bool ScCanUse { get { return scCanUse; } }

        public string ScGroupNameToFriendlyName(string s)
        {
            string rs = s;
            if (LanuageMgr.IsChinese)
            {
                if (s != null)
                    if (!scGroupFriendlyName.TryGetValue(s, out rs))
                        rs = s;
            }
            return rs;
        }
        public bool ScMgrFindRunSc(PsItem p)
        {
            bool rs = false;
            if (p != null)
            {
                foreach (ScItem r in runningSc)
                {
                    if (r.pid == p.pid)
                    {
                        p.svcs.Add(r);
                        rs = true;
                    }
                }
            }
            return rs;
        }
        public string ScMgrFindDriverSc(string driverOrgPath)
        {
            string rs = "";
            foreach (ListViewItem li in listService.Items)
            {
                if (li.SubItems[7].Text == driverOrgPath)
                {
                    rs = li.Text;
                    break;
                }
            }
            return rs;
        }
        public void ScMgrInit()
        {
            if (!Inited)
            {
                Inited = true;

                if (!MIsRunasAdmin())
                {
                    listService.Hide();
                    FormMain.pl_ScNeedAdminTip.Show();
                }
                else
                {
                    scGroupFriendlyName.Add("localService", LanuageMgr.GetStr("LocalService"));
                    scGroupFriendlyName.Add("LocalService", LanuageMgr.GetStr("LocalService"));
                    scGroupFriendlyName.Add("LocalSystem", LanuageMgr.GetStr("LocalSystem"));
                    scGroupFriendlyName.Add("LocalSystemNetworkRestricted", LanuageMgr.GetStr("LocalSystemNetworkRestricted"));
                    scGroupFriendlyName.Add("LocalServiceNetworkRestricted", LanuageMgr.GetStr("LocalServiceNetworkRestricted"));
                    scGroupFriendlyName.Add("LocalServiceNoNetwork", LanuageMgr.GetStr("LocalServiceNoNetwork"));
                    scGroupFriendlyName.Add("LocalServiceAndNoImpersonation", LanuageMgr.GetStr("LocalServiceAndNoImpersonation"));
                    scGroupFriendlyName.Add("NetworkServiceAndNoImpersonation", LanuageMgr.GetStr("NetworkServiceAndNoImpersonation"));
                    scGroupFriendlyName.Add("NetworkService", LanuageMgr.GetStr("NetworkService"));
                    scGroupFriendlyName.Add("NetworkServiceNetworkRestricted", LanuageMgr.GetStr("NetworkServiceNetworkRestricted"));
                    scGroupFriendlyName.Add("UnistackSvcGroup", LanuageMgr.GetStr("UnistackSvcGroup"));
                    scGroupFriendlyName.Add("NetSvcs", LanuageMgr.GetStr("NetworkService"));
                    scGroupFriendlyName.Add("netsvcs", LanuageMgr.GetStr("NetworkService"));

                    MAppWorkCall3(182, listService.Handle, IntPtr.Zero);

                    if (!MSCM_Init())
                        TaskDialog.Show(LanuageMgr.GetStr("StartSCMFailed", false), LanuageFBuffers.Str_ErrTitle, "", TaskDialogButton.OK, TaskDialogIcon.Stop);

                    NativeBridge.scMgrEnumServicesCallBack = ScMgrIEnumServicesCallBack;
                    NativeBridge.scMgrEnumServicesCallBackPtr = Marshal.GetFunctionPointerForDelegate(NativeBridge.scMgrEnumServicesCallBack);

                    scCanUse = true;
                    ScMgrRefeshList();

                }

                icoSc = new Icon(Properties.Resources.icoService, 16, 16);

                listService.ListViewItemSorter = listViewItemComparerSc;
                hListHeaderSc = ComCtlApi.MListViewGetHeaderControl(listService.Handle, false);

                
            }
        }
        public void ScMgrRefeshList()
        {
            if (scCanUse)
            {
                scValidPid.Clear();
                runningSc.Clear();
                listService.Items.Clear();
                MEnumServices(NativeBridge.scMgrEnumServicesCallBackPtr);
                FormMain.lbServicesCount.Text = LanuageMgr.GetStr("ServiceCount") + " : " + (listService.Items.Count == 0 ? "--" : listService.Items.Count.ToString());
            }
        }
        private void ScMgrIEnumServicesCallBack(IntPtr dspName, IntPtr scName, uint scType, uint currentState, uint dwProcessId, bool syssc,
            uint dwStartType, IntPtr lpBinaryPathName, IntPtr lpLoadOrderGroup, bool add)
        {
            if (add)
            {
                ListViewItem li = new ListViewItem(Marshal.PtrToStringUni(scName));
                ScTag t = new ScTag();
                t.name = li.Text;
                t.runningState = currentState;
                t.startType = scType;
                t.binaryPathName = Marshal.PtrToStringUni(lpBinaryPathName);
                li.SubItems.Add(dwProcessId == 0 ? "" : dwProcessId.ToString());
                li.Tag = t;
                if (dwProcessId != 0)
                {
                    scValidPid.Add(dwProcessId);
                    runningSc.Add(new ScItem(Convert.ToInt32(dwProcessId), Marshal.PtrToStringUni(lpLoadOrderGroup), Marshal.PtrToStringUni(scName), Marshal.PtrToStringUni(dspName)));
                }
                li.SubItems.Add(Marshal.PtrToStringUni(dspName));
                switch (currentState)
                {
                    case 0x0001:
                    case 0x0003: li.SubItems.Add(LanuageFBuffers.Str_StatusStopped); break;
                    case 0x0002:
                    case 0x0004: li.SubItems.Add(LanuageFBuffers.Str_StatusRunning); break;
                    case 0x0006:
                    case 0x0007: li.SubItems.Add(LanuageFBuffers.Str_StatusPaused); break;
                    default: li.SubItems.Add(""); break;
                }
                li.SubItems.Add(Marshal.PtrToStringUni(lpLoadOrderGroup));
                switch (dwStartType)
                {
                    case 0x0000: li.SubItems.Add(LanuageFBuffers.Str_DriverLoad); break;
                    case 0x0001: li.SubItems.Add(LanuageFBuffers.Str_DriverLoad); break;
                    case 0x0002: li.SubItems.Add(LanuageFBuffers.Str_AutoStart); break;
                    case 0x0003: li.SubItems.Add(LanuageFBuffers.Str_DemandStart); break;
                    case 0x0004: li.SubItems.Add(LanuageFBuffers.Str_Disabled); break;
                    case 0x0080: li.SubItems.Add(""); break;
                    default: li.SubItems.Add(""); break;
                }
                switch (scType)
                {
                    case 0x0002: li.SubItems.Add(LanuageFBuffers.Str_FileSystem); break;
                    case 0x0001: li.SubItems.Add(LanuageFBuffers.Str_KernelDriver); break;
                    case 0x0010: li.SubItems.Add(LanuageFBuffers.Str_UserService); break;
                    case 0x0020: li.SubItems.Add(LanuageFBuffers.Str_SystemService); break;
                    default: li.SubItems.Add(""); break;
                }

                string path = Marshal.PtrToStringUni(lpBinaryPathName);
                if (!MFM_FileExist(path))
                {
                    StringBuilder spath = new StringBuilder(260);
                    if (MCommandLineToFilePath(path, spath, 260))
                        path = spath.ToString();
                }

                bool hightlight = false;
                if (!string.IsNullOrEmpty(path) && MFM_FileExist(path))
                {
                    li.SubItems.Add(path);
                    StringBuilder exeCompany = new StringBuilder(256);
                    if (MGetExeCompany(path, exeCompany, 256))
                    {
                        li.SubItems.Add(exeCompany.ToString());
                        if (mainSettings.HighlightNoSystem && exeCompany.ToString() != ConstVals.MICROSOFT)
                            hightlight = true;
                    }
                    else if (mainSettings.HighlightNoSystem) hightlight = true;
                }
                else
                {
                    li.SubItems.Add(path);
                    li.SubItems.Add(LanuageFBuffers.Str_FileNotExist);
                    if (mainSettings.HighlightNoSystem) hightlight = true;
                }
                if (hightlight)
                {
                    li.ForeColor = Color.Blue;
                    foreach (ListViewItem.ListViewSubItem s in li.SubItems)
                        s.ForeColor = Color.Blue;
                }
                listService.Items.Add(li);
            }
            else if (updateScItem != null)
            {
                ScTag t = (ScTag)updateScItem.Tag;
                t.runningState = currentState;
                t.startType = scType;
                if (dwProcessId != 0)
                {
                    scValidPid.Add(dwProcessId);
                    updateScItem.SubItems[1].Text = dwProcessId.ToString();
                }
                else
                {
                    if (updateScItem.SubItems[1].Text != "")
                    {
                        uint oldpid = UInt32.Parse(updateScItem.SubItems[1].Text);
                        if (scValidPid.Contains(oldpid)) scValidPid.Remove(oldpid);
                    }
                    updateScItem.SubItems[1].Text = "";
                }
                switch (currentState)
                {
                    case 0x0001:
                    case 0x0003: updateScItem.SubItems[3].Text = LanuageFBuffers.Str_StatusStopped; break;
                    case 0x0002:
                    case 0x0004: updateScItem.SubItems[3].Text = LanuageFBuffers.Str_StatusRunning; break;
                    case 0x0006:
                    case 0x0007: updateScItem.SubItems[3].Text = LanuageFBuffers.Str_StatusPaused; break;
                    default: updateScItem.SubItems[3].Text = ""; break;
                }
                switch (dwStartType)
                {
                    case 0x0000: updateScItem.SubItems[5].Text = LanuageFBuffers.Str_DriverLoad; break;
                    case 0x0001: updateScItem.SubItems[5].Text = LanuageFBuffers.Str_DriverLoad; break;
                    case 0x0002: updateScItem.SubItems[5].Text = LanuageFBuffers.Str_AutoStart; break;
                    case 0x0003: updateScItem.SubItems[5].Text = LanuageFBuffers.Str_DemandStart; break;
                    case 0x0004: updateScItem.SubItems[5].Text = LanuageFBuffers.Str_Disabled; break;
                    case 0x0080: updateScItem.SubItems[5].Text = ""; break;
                    default: updateScItem.SubItems[5].Text = ""; break;
                }
                updateScItem = null;
            }
        }
        public void ScMgrRemoveInvalidItem(string targetName)
        {
            if (Inited && targetName != "")
            {
                ListViewItem target = null;
                foreach (ListViewItem li in listService.Items)
                {
                    if (li.Text == targetName)
                    {
                        target = li;
                        break;
                    }
                }
                if (target != null)
                    listService.Items.Remove(target);
            }
        }
        public void ScMgrGoToService(string targetName)
        {
            FormMain.tabControlMain.SelectedTab = Page;
            foreach (ListViewItem it in listService.Items)
            {
                if (it.Text == targetName)
                {
                    int i = listService.Items.IndexOf(it);
                    listService.EnsureVisible(i);
                    it.Selected = true;
                }
                else it.Selected = false;
            }
        }
        public void ScMgrUpdateService(string targetName)
        {
            foreach (ListViewItem it in listService.Items)
            {
                if (it.Text == targetName)
                {
                    updateScItem = it;
                    updateScTimer.Start();
                    break;
                }
            }
        }

        private void UpdateScTimer_Tick(object sender, EventArgs e)
        {
            if (!MSCM_UpdateServiceStatus(updateScItem.Text, NativeBridge.scMgrEnumServicesCallBackPtr))
                LogApi.LogErr2("UpdateServiceStatus for service " + updateScItem.Text + " failed.");

            updateScTimer.Stop();
        }

        private void listService_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listService.SelectedItems.Count > 0)
                {
                    ListViewItem item = listService.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listService.PointToScreen(p);
                    ScTag t = item.Tag as ScTag;
                    MSCM_ShowMenu(Handle, t.name, t.runningState, t.startType, t.binaryPathName, p.X, p.Y);
                }
            }
        }
        private void listService_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listService.SelectedItems.Count > 0)
                {
                    ListViewItem item = listService.SelectedItems[0];
                    ScTag t = item.Tag as ScTag;
                    MSCM_ShowMenu(Handle, t.name, t.runningState, t.startType, t.binaryPathName, 0, 0);
                }
            }
        }
        private void listService_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparer)listService.ListViewItemSorter).Asdening = !((ListViewItemComparer)listService.ListViewItemSorter).Asdening;
            ((ListViewItemComparer)listService.ListViewItemSorter).SortColum = e.Column;
            ComCtlApi.MListViewSetColumnSortArrow(hListHeaderSc, ((ListViewItemComparer)listService.ListViewItemSorter).SortColum,
                         ((ListViewItemComparer)listService.ListViewItemSorter).Asdening, false);
            listService.Sort();
        }

        private void linkRebootAsAdmin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MAppRebotAdmin2("select services");
        }
        private void linkOpenScMsc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MFM_OpenFile("services.msc", Handle);
        }



    }
}
