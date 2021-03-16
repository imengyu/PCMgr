using PCMgr.Ctls;
using PCMgr.Helpers;
using PCMgr.Lanuages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.Win32;
using static PCMgr.NativeMethods.LogApi;
using static PCMgr.Main.MainUtils;

namespace PCMgr.Main
{
    class MainPageProcess : MainPage
    {
        private TaskMgrList listProcess;
        private TaskMgrList listApps;
        private Button btnEndTaskSimple;
        private Button btnEndProcess;
        private Label lbProcessCount;

        public MainPageProcess(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageProcCtl)
        {
            listProcess = formMain.listProcess;
            listApps = formMain.listApps;
            btnEndTaskSimple = formMain.btnEndTaskSimple;
            btnEndProcess = formMain.btnEndProcess;
            lbProcessCount = formMain.lbProcessCount;
        }

        protected override void OnLoad()
        {
            mainPageScMgr = FormMain.MainPageScMgr;
            mainPageUwpMgr = FormMain.MainPageUwpMgr;

            base.OnLoad();
        }
        protected override void OnLoadControlEvents()
        {
            FormMain.baseProcessRefeshTimerLow.Interval = 10000;
            FormMain.baseProcessRefeshTimerLow.Tick += BaseProcessRefeshTimerLow_Tick;
            FormMain.baseProcessRefeshTimerLowSc.Interval = 120000;
            FormMain.baseProcessRefeshTimerLowSc.Tick += BaseProcessRefeshTimerLowSc_Tick;
            FormMain.baseProcessRefeshTimerLowUWP.Interval = 5000;
            FormMain.baseProcessRefeshTimerLowUWP.Tick += BaseProcessRefeshTimerLowUWP_Tick;
            startLoadDealyTimer.Interval = 1300;
            startLoadDealyTimer.Tick += StartLoadDealyTimer_Tick;

            FormMain.check_showAllProcess.CheckedChanged += check_showAllProcess_CheckedChanged;
            FormMain.expandFewerDetals.Click += expandFewerDetals_Click;
            FormMain.expandMoreDetals.Click += expandMoreDetals_Click;
            FormMain.btnEndProcess.Click += btnEndProcess_Click;
            FormMain.btnEndTaskSimple.Click += btnEndTaskSimple_Click;

            listProcess.Header.CloumClick += listProcess_Header_CloumClick;
            listProcess.SelectItemChanged += listProcess_SelectItemChanged;
            listProcess.KeyDown +=listProcess_KeyDown;
            listProcess.MouseClick += listProcess_MouseClick;
            listProcess.MouseDoubleClick += listProcess_MouseDoubleClick;
            listProcess.MouseDown +=  listProcess_MouseDown;
            listProcess.DrawUWPPausedIconIndexGetCallback = ProcessListGetSatateIndex;

            listApps.SelectItemChanged +=listApps_SelectItemChanged;
            listApps.KeyDown += listApps_KeyDown;
            listApps.MouseClick +=listApps_MouseClick;
            listApps.MouseDoubleClick += listApps_MouseDoubleClick;

            FormMain.百分比ToolStripMenuItemRam.Click += 百分比ToolStripMenuItemRam_Click;
            FormMain.值ToolStripMenuItemRam.Click += 值ToolStripMenuItemRam_Click;
            FormMain.百分比ToolStripMenuItemDisk.Click += 百分比ToolStripMenuItemDisk_Click;
            FormMain.值ToolStripMenuItemDisk.Click += 值ToolStripMenuItemDisk_Click;
            FormMain.百分比ToolStripMenuItemNet.Click += 百分比ToolStripMenuItemNet_Click;
            FormMain.值ToolStripMenuItemNet.Click += 值ToolStripMenuItemNet_Click;


            base.OnLoadControlEvents();
        }

        private MainPageScMgr mainPageScMgr = null;
        private MainPageUwpMgr mainPageUwpMgr = null;

        //主页进程页面代码

        private const double PERF_LIMIT_MIN_DATA_DISK = 0.005;
        private const double PERF_LIMIT_MIN_DATA_NETWORK = 0.001;

        private bool refeshLowLock = false;
        private Size lastSimpleSize { get => FormMain.LastSimpleSize; set { FormMain.LastSimpleSize = value; } }
        private Size lastSize { get => FormMain.LastSize; set { FormMain.LastSize = value; } }
        private int nextSecType = -1;
        private int sortitem = -1;
        private bool sorta = false;
        private bool isFirstLoad = true;
        private bool isLoadFull = false;
        private bool mergeApps = true;

        private bool isGlobalBadDataLock = true;

        private TaskListViewColumnSorter lvwColumnSorter = null;

        private bool isGlobalRefeshing = false;
        private bool isGlobalRefeshingAll = false;

        private bool isRamPercentage = false;
        private bool isDiskPercentage = false;
        private bool isNetPercentage = false;

        private bool isSelectExplorer = false;
        private uint currentProcessPid = 0;

        internal List<PsItem> GetLoadedPs()
        {
            return loadedPs;
        }

        private List<UwpHostItem> uwpHostPid = new List<UwpHostItem>();
        private List<PsItem> loadedPs = new List<PsItem>();
        private List<UwpItem> uwps = new List<UwpItem>();
        private List<UwpWinItem> uwpwins = new List<UwpWinItem>();
        private List<string> windowsProcess = new List<string>();
        private List<string> veryimporantProcess = new List<string>();
        private Color dataGridZeroColor = Color.FromArgb(255, 244, 196);

        private string system32Path = "";
        private string csrssPath = "";
        private string ntoskrnlPath = "";
        private string systemRootPath = "";
        private string svchostPath = "";
        private string svchostPathwow = "";

        private TaskMgrListItem nextKillItem = null;
        private Font smallListFont = null;
        private TaskMgrListItem thisLoadItem = null;
        private Timer startLoadDealyTimer = new Timer();

        private int maxMem = 1024;
        private double allMem = 0;
        private double maxDiskRate = 100;
        private int maxNetRate = 100;

        private void MainGetWinsCallBack(IntPtr hWnd, IntPtr data, int i)
        {
            if (i == 1)
            {
                if (IsWindowVisible(hWnd))
                {
                    UwpWinItem item = new UwpWinItem();
                    item.hWnd = hWnd;
                    item.ownerPid = (uint)data.ToInt32();
                    uwpwins.Add(item);
                }
            }
            else
            {
                if (thisLoadItem != null)
                {
                    if (((PsItem)thisLoadItem.Tag).exepath.ToLower() != @"c:\windows\system32\dwm.exe")
                    {
                        if (!thisLoadItem.HasWindowChild(hWnd))
                        {
                            if (IsWindowVisible(hWnd))
                            {
                                IntPtr icon = MGetWindowIcon(hWnd);
                                TaskMgrListItemChild c = new TaskMgrListItemChild(Marshal.PtrToStringAuto(data), icon != IntPtr.Zero ? Icon.FromHandle(icon) : PCMgr.Properties.Resources.icoShowedWindow);
                                c.Tag = hWnd;
                                c.Type = TaskMgrListItemType.ItemWindow;
                                thisLoadItem.Childs.Add(c);
                            }
                        }
                    }
                }
            }
        }
        private void MainEnumWinsCallBack(IntPtr hWnd, IntPtr hWndParent)
        {
            WorkWindow.FormSpyWindow f = new WorkWindow.FormSpyWindow(hWnd);
            Control fp = Control.FromHandle(hWndParent);
            f.ShowDialog(fp);
        }

        private bool IsVeryImporant(PsItem p)
        {
            if (p.exepath != null)
            {
                string str = p.exepath.ToLower();
                foreach (string s in veryimporantProcess)
                    if (s == str) return true;
            }
            return false;
        }
        private bool IsImporant(PsItem p)
        {
            /*if (p.exepath != null)
            {
                if (p.exepath.ToLower() == @"c:\windows\system32\svchost.exe") return true;
                if (p.exepath.ToLower() == @"c:\windows\system32\cssrs.exe") return true;
                if (p.exepath.ToLower() == @"c:\windows\system32\smss.exe") return true;
                if (p.exepath.ToLower() == @"c:\windows\system32\lsass.exe") return true;
                if (p.exepath.ToLower() == @"c:\windows\system32\sihost.exe") return true;
                if (p.exepath.ToLower() == @"c:\windows\system32\cssrs.exe") return true;
               
            }*/
            if (p.exepath != null)
            {
                if (p.exepath.ToLower() == @"c:\windows\system32\svchost.exe") return true;
                return IsWindowsProcess(p.exepath);
            }
            return false;
        }
        private bool IsExplorer(PsItem p)
        {
            if (p.exename != null && p.exename.ToLower() == "explorer.exe") return true;
            if (p.exepath != null && p.exepath.ToLower() == @"c:\windows\explorer.exe") return true;
            return false;
        }
        private bool IsWindowsProcess(string str)
        {
            //检测是不是Windows进程
            if (str != null)
            {
                str = str.ToLower();
                foreach (string s in windowsProcess)
                    if (s == str) return true;
            }
            return false;
        }
        private bool IsUnAccessableWindowsProcess(string exename, IntPtr hprocess, ref StringBuilder stringBuilder)
        {
            if (hprocess == Nullptr)
                return exename == "csrss.exe" || exename == "wininit.exe" || exename == "services.exe" || exename == "smss.exe";
            return false;
        }
        private bool IsEndable(PsItem p)
        {
            if (p.pid <= 4) return false;
            if (p.exename == "Registry" || p.exename == "Memory Compression") return false;
            return true;
        }

        private bool ProcessListGetUwpIsRunning(TaskMgrListItem uwpHostItem, out IntPtr itsHwnd)
        {
            bool rs = false;
            foreach (UwpWinItem u in uwpwins)
            {
                foreach (TaskMgrListItem uwpprocess in uwpHostItem.Childs)
                {
                    if (uwpprocess.Type == TaskMgrListItemType.ItemUWPProcess)
                    {
                        if (uwpprocess.PID == u.ownerPid)
                        {
                            itsHwnd = u.hWnd;
                            rs = true;
                            return rs;
                        }
                    }
                }
            }
            itsHwnd = IntPtr.Zero;
            return rs;
        }
        private Color ProcessListGetColorFormValue(double v, double maxv)
        {
            //数值百分百转为颜色
            double d = v / maxv;
            if (d <= 0)
                return Color.FromArgb(255, 244, 196);
            else if (d > 0 && d <= 0.1)
                return Color.FromArgb(249, 236, 168);
            else if (d > 0.1 && d <= 0.3)
                return Color.FromArgb(255, 228, 135);
            else if (d > 0.3 && d <= 0.6)
                return Color.FromArgb(252, 207, 23);
            else if (d > 0.6 && d <= 0.8)
                return Color.FromArgb(252, 184, 22);
            else if (d > 0.8 && d <= 0.9)
                return Color.FromArgb(255, 167, 29);
            else if (d > 0.9)
                return Color.FromArgb(255, 160, 19);
            return Color.FromArgb(255, 249, 228);
        }
        private string ProcessListGetPrecentValue(double v, double maxv)
        {
            double d = v / maxv;
            return d.ToString("0.0") + " %";
        }

        //查找条目
        private UwpHostItem ProcessListFindUWPItemWithHostId(uint pid)
        {
            UwpHostItem rs = null;
            foreach (UwpHostItem i in uwpHostPid)
            {
                if (i.pid == pid)
                {
                    rs = i;
                    break;
                }
            }
            return rs;
        }
        private UwpItem ProcessListFindUWPItem(string fullName)
        {
            UwpItem rs = null;
            foreach (UwpItem i in uwps)
            {
                if (i.uwpFullName == fullName)
                {
                    rs = i;
                    break;
                }
            }
            return rs;
        }
        private PsItem ProcessListFindPsItem(uint pid)
        {
            PsItem rs = null;
            foreach (PsItem i in loadedPs)
            {
                if (i.pid == pid)
                {
                    rs = i;
                    return rs;
                }
            }
            return rs;
        }
        private TaskMgrListItem ProcessListFindItem(uint pid)
        {
            TaskMgrListItem rs = null;
            foreach (TaskMgrListItem i in listProcess.Items)
            {
                if (i.PID == pid)
                {
                    rs = i;
                    return rs;
                }
                if (i.Type == TaskMgrListItemType.ItemProcessHost
                    || i.Type == TaskMgrListItemType.ItemUWPProcess)
                {
                    foreach (TaskMgrListItem ix in i.Childs)
                    {
                        if (ix.PID == pid)
                        {
                            rs = ix;
                            return rs;
                        }
                    }
                }
            }
            return rs;
        }
        private bool ProcessListIsProcessLoaded(uint pid, out PsItem item)
        {
            bool rs = false;
            foreach (PsItem f in loadedPs)
            {
                if (f.pid == pid)
                {
                    item = f;
                    rs = true;
                    return rs;
                }
            }
            item = null;
            return rs;
        }
        internal string ProcessListFindProcessName(uint pid)
        {
            string rs = null;
            foreach (TaskMgrListItem i in listProcess.Items)
            {
                if (i.PID == pid)
                {
                    rs = i.Text;
                    return rs;
                }
                if (i.Type == TaskMgrListItemType.ItemProcessHost
                    || i.Type == TaskMgrListItemType.ItemUWPProcess)
                {
                    foreach (TaskMgrListItem ix in i.Childs)
                    {
                        if (ix.PID == pid)
                        {
                            rs = i.Text;
                            return rs;
                        }
                    }
                }
            }
            return rs;
        }
        internal void ProcessListSelectProcess(uint pid)
        {
            foreach (TaskMgrListItem i in listProcess.Items)
            {
                if (i.PID == pid)
                {
                    listProcess.SelectedItem = i;
                    listProcess.ScrollToItem(i);

                    if (FormMain.tabControlMain.SelectedTab != Page)
                        FormMain.tabControlMain.SelectedTab = Page;

                    return;
                }
                if (i.Type == TaskMgrListItemType.ItemProcessHost
                    || i.Type == TaskMgrListItemType.ItemUWPProcess)
                {
                    foreach (TaskMgrListItem ix in i.Childs)
                    {
                        if (ix.PID == pid)
                        {
                            i.ChildsOpened = true;
                            i.OldSelectedItem = ix;

                            listProcess.ScrollToItem(i);

                            if (FormMain.tabControlMain.SelectedTab != Page)
                                FormMain.tabControlMain.SelectedTab = Page;

                            return;
                        }
                    }
                }
            }
        }

        private IntPtr processMonitor = IntPtr.Zero;

        private bool perfMainInited = false;
        private bool perfMainInitFailed = false;

        private int ProcessListGetSatateIndex()
        {
            return stateindex;
        }
        private void ProcessListLoadCallBacks()
        {
            NativeBridge.enumWinsCallBack = MainEnumWinsCallBack;
            NativeBridge.getWinsCallBack = MainGetWinsCallBack;

            NativeBridge.ProcessNewItemCallBack = ProcessListNewItemCallBack;
            NativeBridge.ProcessRemoveItemCallBack = ProcessListRemoveItemCallBack;

            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(NativeBridge.enumWinsCallBack), 3);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(NativeBridge.getWinsCallBack), 4);

            NativeBridge.ptrProcessNewItemCallBack = Marshal.GetFunctionPointerForDelegate(NativeBridge.ProcessNewItemCallBack);
            NativeBridge.ptrProcessRemoveItemCallBack = Marshal.GetFunctionPointerForDelegate(NativeBridge.ProcessRemoveItemCallBack);
        }
        private void ProcessListLoadSettings()
        {
            mergeApps = GetConfigBool("MergeApps", "Configure", true);
            isRamPercentage = GetConfigBool("RamPercentage", "Configure", false);
            isDiskPercentage = GetConfigBool("DiskPercentage", "Configure", false);
            isNetPercentage = GetConfigBool("NetPercentage", "Configure", false);

            if (isRamPercentage) FormMain.百分比ToolStripMenuItemRam.Checked = true;
            else FormMain.值ToolStripMenuItemRam.Checked = true;

            if (isDiskPercentage) FormMain.百分比ToolStripMenuItemDisk.Checked = true;
            else FormMain.值ToolStripMenuItemDisk.Checked = true;

            if (isNetPercentage) FormMain.百分比ToolStripMenuItemNet.Checked = true;
            else FormMain.值ToolStripMenuItemNet.Checked = true;

            sorta = GetConfigBool("ListSortDk", "AppSetting", true);
            string sortitemxx = GetConfig("ListSortIndex", "AppSetting", "0");
            if (sortitemxx != "" && sortitemxx != "-1")
                int.TryParse(sortitemxx, out sortitem);
           
        }
        private void ProcessListSaveSettings()
        {
            SetConfig("ListSortIndex", "AppSetting", sortitem.ToString());
            if (sorta) SetConfig("ListSortDk", "AppSetting", "TRUE");
            else SetConfig("ListSortDk", "AppSetting", "FALSE");

            SetConfigBool("RamPercentage", "Configure", isRamPercentage);
            SetConfigBool("DiskPercentage", "Configure", isDiskPercentage);
            SetConfigBool("NetPercentage", "Configure", isNetPercentage);
        }
        private void ProcessListLoadCols()
        {
            TaskMgrListGroup lg = new TaskMgrListGroup(LanuageMgr.GetStr("TitleApp", false));
            listProcess.Groups.Add(lg);
            TaskMgrListGroup lg2 = new TaskMgrListGroup(LanuageMgr.GetStr("TitleBackGround", false));
            listProcess.Groups.Add(lg2);
            TaskMgrListGroup lg3 = new TaskMgrListGroup(LanuageMgr.GetStr("TitleWinApp", false));
            listProcess.Groups.Add(lg3);
            listProcess.Header.CanMoveCloum = true;

            //Net no admin
            if (!FormMain.IsAdmin) headerTips[3].name = "TipNetNoAdmin";

            listProcessAddHeaderMenu("TitleProcName");
            listProcessAddHeaderMenu("TitleType");
            listProcessAddHeaderMenu("TitlePublisher");
            listProcessAddHeaderMenu("TitleStatus");
            listProcessAddHeaderMenu("TitlePID");
            listProcessAddHeaderMenu("TitleCPU");
            listProcessAddHeaderMenu("TitleRam");
            listProcessAddHeaderMenu("TitleDisk");
            listProcessAddHeaderMenu("TitleNet");
            listProcessAddHeaderMenu("TitleProcPath");
            listProcessAddHeaderMenu("TitleCmdLine");

            string s1 = GetConfig("MainHeaders1", "AppSetting");
            if (s1 != "") listProcessAddHeader("TitleName", int.Parse(s1));
            else listProcessAddHeader("TitleName", 200);
            string headers = GetConfig("MainHeaders", "AppSetting");
            if (headers.Contains("#"))
            {
                string[] headersv = headers.Split('#');
                for (int i = 0; i < headersv.Length; i++)
                {
                    if (headersv[i].Contains("-"))
                    {
                        int width = 0;
                        string[] headersvx = headersv[i].Split('-');
                        if (headersv.Length >= 2)
                        {
                            if (!int.TryParse(headersvx[1], out width) || width < 0 || width > 512)
                                width = listProcessTryGetHeaderDefaultWidth(headersvx[0]);
                            listProcessAddHeader(headersvx[0], width);
                            listProcessCheckHeaderMenu(headersvx[0], true);
                        }
                    }
                }
            }
            else if (headers == "")
            {
                listProcessAddHeader("TitleStatus", listProcessTryGetHeaderDefaultWidth("TitleStatus"));
                listProcessAddHeader("TitlePID", listProcessTryGetHeaderDefaultWidth("TitlePID"));
                listProcessAddHeader("TitleProcName", listProcessTryGetHeaderDefaultWidth("TitleProcName"));
                listProcessAddHeader("TitleCPU", listProcessTryGetHeaderDefaultWidth("TitleCPU"));
                listProcessAddHeader("TitleRam", listProcessTryGetHeaderDefaultWidth("TitleRam"));
                listProcessAddHeader("TitleDisk", listProcessTryGetHeaderDefaultWidth("TitleDisk"));
                listProcessAddHeader("TitleNet", listProcessTryGetHeaderDefaultWidth("TitleNet"));
            }

            listProcessGetAllHeaderIndexs();

            if (pidindex != -1) listProcess.Header.Items[pidindex].Alignment = StringAlignment.Far;
            if (cpuindex != -1)
            {
                listProcess.Header.Items[cpuindex].IsNum = true;
                listProcess.Header.Items[cpuindex].Alignment = StringAlignment.Far;
            }
            if (ramindex != -1)
            {
                listProcess.Header.Items[ramindex].IsNum = true;
                listProcess.Header.Items[ramindex].Alignment = StringAlignment.Far;
            }
            if (diskindex != -1)
            {
                listProcess.Header.Items[diskindex].IsNum = true;
                listProcess.Header.Items[diskindex].Alignment = StringAlignment.Far;
            }
            if (netindex != -1)
            {
                listProcess.Header.Items[netindex].IsNum = true;
                listProcess.Header.Items[netindex].Alignment = StringAlignment.Far;
            }
        }
        private void ProcessListSaveCols()
        {
            if (saveheader)
            {
                string headers = "";
                for (int i = 1; i < listProcess.Header.SortedItems.Count; i++)
                    headers = headers + "#" + listProcess.Header.SortedItems[i].Identifier + "-" + listProcess.Header.SortedItems[i].Width;
                SetConfig("MainHeaders", "AppSetting", headers);
            }
            SetConfig("MainHeaders1", "AppSetting", listProcess.Colunms[0].Width.ToString());
        }
        private void ProcessListInitPerfs()
        {
            if (!perfMainInitFailed &&!perfMainInited)
            {
                //初始化整体性能计数器
                MPERF_Init3PerformanceCounters();
                ProcessListForceRefeshAll();
                perfMainInited = true;
            }
        }
        private void ProcessListUnInitPerfs()
        {
            if (perfMainInited)
            {
                //释放计数器
                MPERF_Destroy3PerformanceCounters();
            }
        }
        public void ProcessListInit()
        {
            //初始化
            if (!Inited)
            {
                lvwColumnSorter = new TaskListViewColumnSorter(this);

                ProcessListLoadCols();
                ProcessListLoadSettings();
                ProcessListLoadCallBacks();

                currentProcessPid = (uint)MAppWorkCall3(180, IntPtr.Zero, IntPtr.Zero);

                processMonitor = MProcessMonitor.CreateProcessMonitor(NativeBridge.ptrProcessRemoveItemCallBack, NativeBridge.ptrProcessNewItemCallBack, Nullptr);

                smallListFont = new Font(FormMain.tabControlMain.Font.FontFamily, 9f);

                if (system32Path == "") system32Path = Marshal.PtrToStringUni(MAppWorkCall4(94, Nullptr, Nullptr));
                if (systemRootPath == "") systemRootPath = Marshal.PtrToStringUni(MAppWorkCall4(95, Nullptr, Nullptr));
                if (csrssPath == "") csrssPath = Marshal.PtrToStringUni(MAppWorkCall4(96, Nullptr, Nullptr));
                if (ntoskrnlPath == "") ntoskrnlPath = Marshal.PtrToStringUni(MAppWorkCall4(97, Nullptr, Nullptr));
                if (svchostPath == "")
                {
                    svchostPath = (systemRootPath + @"\System32\svchost.exe").ToLower();
                    svchostPathwow = (systemRootPath + @"\syswow64\svchost.exe").ToLower();
                }

                windowsProcess.Add(@"C:\Program Files\Windows Defender\NisSrv.exe".ToLower());
                windowsProcess.Add(@"C:\Program Files\Windows Defender\MsMpEng.exe".ToLower());
                windowsProcess.Add(svchostPath);
                windowsProcess.Add((systemRootPath + @"\System32\csrss.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\conhost.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"‪\System32\sihost.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\winlogon.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\wininit.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\smss.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\services.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\dwm.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\lsass.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\explorer.exe").ToLower());

                veryimporantProcess.Add((systemRootPath + @"\System32\wininit.exe").ToLower());
                veryimporantProcess.Add((systemRootPath + @"\System32\csrss.exe").ToLower());
                veryimporantProcess.Add((systemRootPath + @"\System32\lsass.exe").ToLower());
                veryimporantProcess.Add((systemRootPath + @"\System32\smss.exe").ToLower());

                //计算单个程序最大理想内存mb，项目颜色需要用
                ulong allMem = MPERF_GetRamAll();
                if (allMem > 34359738368) maxMem = 16000;
                else if (allMem > 17179869184) maxMem = 8200;
                else if (allMem > 8589934592) maxMem = 4100;
                else if (allMem > 4294967296) maxMem = 2100;
                this.allMem = allMem;

                Inited = true;

                if (MIsRunasAdmin()) FormMain.MainPageScMgr.ScMgrInit();
                else LogWarn2("Not admin , start sc manager failed.");
                if (SysVer.IsWin8Upper()) FormMain.MainPageUwpMgr.UWPListInit();
                else LogWarn2("Not uwp available in this system");

                ProcessListRefesh();
                ProcessListSimpleInit();
                ProcessListInitPerfs();

                if (isFirstLoad)
                {
                    if (sortitem < listProcess.Header.Items.Count && sortitem >= 0)
                    {
                        lvwColumnSorter.Order = sorta ? SortOrder.Ascending : SortOrder.Descending;
                        lvwColumnSorter.SortColumn = sortitem;
                        listProcess.Header.Items[sortitem].ArrowType = sorta ? TaskMgrListHeaderSortArrow.Ascending : TaskMgrListHeaderSortArrow.Descending;
                        listProcess.Header.Invalidate();
                        listProcess.ListViewItemSorter = lvwColumnSorter;
                        if (sortitem == 0) listProcess.ShowGroup = true;
                        else listProcess.ShowGroup = false;
                    }

                    ProcessListLoadFinished();
                    isFirstLoad = false;
                }

                FormMain.baseProcessRefeshTimer.Start();
                FormMain.baseProcessRefeshTimerLowUWP.Start();
                FormMain.baseProcessRefeshTimerLow.Start();
                FormMain.baseProcessRefeshTimerLowSc.Start();

                FormMain.StartingProgressShowHide(false);

                startLoadDealyTimer.Start();
            }

        }
        private void ProcessListLoadFinished()
        {
            //firstLoad
            listProcess.Show();
            FormMain.Cursor = Cursors.Arrow;
        }

        public void ProcessListDayUpdate(double cpu, double ram, double disk, double net, bool perfsimpleGeted)
        {
            listProcess.Locked = true;

            if (perfMainInited)
            {
                //Refesh perfs             
                if (!perfsimpleGeted && (cpuindex != -1 || ramindex != -1 || diskindex != -1 || netindex != -1))
                    MPERF_GlobalUpdatePerformanceCounters();
                if (cpuindex != -1)
                {
                    if (!perfsimpleGeted) cpu = MPERF_GetCpuUseAge();
                    listProcess.Colunms[cpuindex].TextBig = (int)cpu + "%";
                    if (cpu >= 95)
                        listProcess.Colunms[cpuindex].IsHot = true;
                    else listProcess.Colunms[cpuindex].IsHot = false;
                }
                if (ramindex != -1)
                {
                    if (!perfsimpleGeted) ram = MPERF_GetRamUseAge2() * 100;
                    listProcess.Colunms[ramindex].TextBig = (int)ram + "%";
                    if (ram >= 95)
                        listProcess.Colunms[ramindex].IsHot = true;
                    else listProcess.Colunms[ramindex].IsHot = false;
                }
                if (diskindex != -1)
                {
                    if (!perfsimpleGeted) disk = MPERF_GetDiskUseage() * 100;
                    listProcess.Colunms[diskindex].TextBig = (int)disk + "%";
                    if (disk >= 95)
                        listProcess.Colunms[diskindex].IsHot = true;
                    else listProcess.Colunms[diskindex].IsHot = false;
                }
                if (netindex != -1)
                {
                    if (!perfsimpleGeted) net = MPERF_GetNetWorkUseage() * 100;
                    listProcess.Colunms[netindex].TextBig = (int)net + "%";
                    if (net >= 95)
                        listProcess.Colunms[netindex].IsHot = true;
                    else listProcess.Colunms[netindex].IsHot = false;
                }
            }

            //Refesh Process List
            ProcessListRefesh2();

            listProcess.Locked = false;
            listProcess.Header.Invalidate();
        }
        public void ProcessListRefesh()
        {
            //清空整个列表并加载
            isLoadFull = true;

            uwps.Clear();
            uwpHostPid.Clear();
            uwpwins.Clear();

            if (SysVer.IsWin8Upper()) MAppVProcessAllWindowsUWP();

            listProcess.Locked = true;
            listProcess.Items.Clear();

            loadedPs.Clear();

            MProcessMonitor.EnumAllProcess(processMonitor);

            ProcessListRefeshPidTree();

            bool refeshAColumData = lvwColumnSorter.SortColumn == cpuindex
              || lvwColumnSorter.SortColumn == ramindex
              || lvwColumnSorter.SortColumn == diskindex
              || lvwColumnSorter.SortColumn == netindex
              || lvwColumnSorter.SortColumn == stateindex;

            lbProcessCount.Text = LanuageFBuffers.Str_ProcessCount + " : " + listProcess.Items.Count;
            Log("Full load for " + listProcess.Items.Count + " items.");

            refeshLowLock = true;
            ProcessListForceRefeshAll();
            refeshLowLock = false;

            listProcess.Locked = false;
            if (refeshAColumData)
                listProcess.Sort(false);//排序
            listProcess.Locked = false;
            //刷新列表
            listProcess.SyncItems(true);

            isLoadFull = false;
        }
        public void ProcessListRefesh2()
        {
            isGlobalRefeshing = true;
            
            //更新进程 tcp 数据
            if (netindex != -1) MPERF_NET_UpdateAllProcessNetInfo();

            uwpwins.Clear();

            //刷新所有数据
            listProcess.Locked = true;

            //刷新窗口
            MAppWorkCall3(222);

            MProcessMonitor.RefeshAllProcess(processMonitor);

            //枚举一些UWP应用
            if (SysVer.IsWin8Upper()) MAppVProcessAllWindowsUWP();

            //刷新性能数据
            bool refeshAColumData = lvwColumnSorter.SortColumn == cpuindex
                || lvwColumnSorter.SortColumn == ramindex
                || lvwColumnSorter.SortColumn == diskindex
                || lvwColumnSorter.SortColumn == netindex
                || lvwColumnSorter.SortColumn == stateindex;
            ProcessListUpdateValues(refeshAColumData ? lvwColumnSorter.SortColumn : -1);
            ProcessListRefeshPidTree();

            if (!FormMain.IsSimpleView)
            {
                listProcess.Sort(false);//排序
                listProcess.Locked = false;
                //刷新列表
                listProcess.SyncItems(true);

                FormMain.lbProcessCount.Text = LanuageFBuffers.Str_ProcessCount + " : " + listProcess.Items.Count;
            }
            else
            {
                listProcess.Locked = false;
                ProcessListSimpleRefesh();
            }

            isGlobalRefeshing = false;
            if (isGlobalRefeshingAll)
            {
                isGlobalRefeshingAll = false;
                ProcessListForceRefeshAll();
            }

            ProcessListKillLastEndItem();
        }

        public void ProcessListRemoveEprocessCol()
        {
            if (eprocessindex != -1)
            {
                listProcess.Colunms.Remove(listProcess.Colunms[eprocessindex]);
                eprocessindex = -1;
            }
        }
        private void ProcessListRefeshPidTree()
        {
            //Refesh Pid tree
            foreach (PsItem p in loadedPs)
            {
                p.parent = null;
                p.childs.Clear();
            }
            foreach (PsItem p in loadedPs)
            {
                PsItem parent = ProcessListFindPsItem(p.ppid);
                if (parent != null)
                {
                    p.parent = parent;
                    parent.childs.Add(p);
                }
                else if (p.parent != null)
                {
                    if (p.parent.childs.Contains(p))
                        p.parent.childs.Remove(p);
                    p.parent = null;
                }
            }
        }
        public void ProcessListForceRefeshAll(bool refeshStaticValues = false)
        {
            if (isGlobalRefeshing)
            {
                isGlobalRefeshingAll = true;
                return;
            }
            //强制刷新所有的条目
            foreach (PsItem p in loadedPs)
            {
                TaskMgrListItem li = p.item;
                if (li.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    ProcessListUpdate(p.pid, false, li, -1);
                    if (refeshStaticValues)
                    {
                        foreach (TaskMgrListItem ix in li.Childs)
                            if (ix.Type == TaskMgrListItemType.ItemProcess)
                                ProcessListLoadStaticValues(ix.PID, ix, ((PsItem)ix.Tag));
                    }
                }
                else
                {
                    if (refeshStaticValues && li.Type == TaskMgrListItemType.ItemProcess)
                        ProcessListLoadStaticValues(p.pid, li, p);
                    else if (refeshStaticValues && li.Type == TaskMgrListItemType.ItemProcessHost)
                    {
                        foreach (TaskMgrListItem ix in li.Childs)
                            if (ix.Type == TaskMgrListItemType.ItemProcess)
                                ProcessListLoadStaticValues(ix.PID, ix, ((PsItem)ix.Tag));
                    }
                    ProcessListUpdate(p.pid, false, li, -1);
                }
            }
        }
        private void ProcessListForceRefeshAllUWP()
        {
            for (int i = 0; i < listProcess.Items.Count; i++)
            {
                if (listProcess.Items[i].Type == TaskMgrListItemType.ItemUWPHost)
                    ProcessListUpdate(listProcess.Items[i].PID, false, listProcess.Items[i], -1);
            }
        }

        private void ProcessListLoad(uint pid, uint ppid, string exename, string exefullpath, IntPtr hprocess, IntPtr processItem)
        {
            //PsItem oldps = ProcessListFindPsItem(pid);
            //if (oldps != null)
            //    Log("ProcessListLoad for a alreday contains item : " + oldps);

            bool need_add_tolist = true;
            //base
            PsItem p = new PsItem();
            p.processItem = processItem;
            p.pid = pid;
            p.ppid = ppid;
            loadedPs.Add(p);

            PsItem parentpsItem = null;
            if (ProcessListIsProcessLoaded(p.ppid, out parentpsItem))
            {
                p.parent = parentpsItem;
                parentpsItem.childs.Add(p);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(exefullpath);

            bool isw = false;
            PEOCESSKINFO infoStruct = new PEOCESSKINFO();
            if (FormMain.IsKernelLoaded)
            {
                if (MGetProcessEprocess(pid, ref infoStruct))
                {
                    if (string.IsNullOrEmpty(exefullpath))
                    {
                        exefullpath = infoStruct.ImageFullName;
                        stringBuilder.Append(exefullpath);
                    }
                }
            }

            TaskMgrListItem taskMgrListItem;
            if (pid == 0)
            {
                isw = true;
                taskMgrListItem = new TaskMgrListItem(LanuageFBuffers.Str_IdleProcess);
                stringBuilder.Append(LanuageFBuffers.Str_IdleProcessDsb);
            }
            else if (pid == 2)
            {
                isw = true;
                exename = LanuageFBuffers.Str_SystemInterrupts;
                taskMgrListItem = new TaskMgrListItem(LanuageFBuffers.Str_SystemInterrupts);
                stringBuilder.Append(LanuageFBuffers.Str_InterruptsProcessDsb);
            }
            else if (pid == 4)
            {
                isw = true;
                taskMgrListItem = new TaskMgrListItem("System");
                stringBuilder.Append(ntoskrnlPath);
            }
            else if (exename == "Registry")
            {
                isw = true;
                taskMgrListItem = new TaskMgrListItem(exename);
                stringBuilder.Append(ntoskrnlPath);
            }
            else if (exename == "Memory Compression")
            {
                isw = true;
                taskMgrListItem = new TaskMgrListItem(exename);
                stringBuilder.Append(ntoskrnlPath);
            }
            else if (IsUnAccessableWindowsProcess(exename, hprocess, ref stringBuilder))
            {
                isw = true;
                StringBuilder exeDescribe = new StringBuilder(256);
                if (MGetExeDescribe(stringBuilder.ToString(), exeDescribe, 256))
                {
                    string exeDescribeStr = exeDescribe.ToString();
                    exeDescribeStr = exeDescribeStr.Trim();
                    if (exeDescribeStr != "")
                        taskMgrListItem = new TaskMgrListItem(exeDescribeStr);
                    else taskMgrListItem = new TaskMgrListItem(exename);
                }
                else taskMgrListItem = new TaskMgrListItem(exename);
            }
            else if (stringBuilder.ToString() != "")
            {
                StringBuilder exeDescribe = new StringBuilder(256);
                if (MGetExeDescribe(stringBuilder.ToString(), exeDescribe, 256))
                {
                    string exeDescribeStr = exeDescribe.ToString();
                    exeDescribeStr = exeDescribeStr.Trim();
                    if (exeDescribeStr != "")
                        taskMgrListItem = new TaskMgrListItem(exeDescribeStr);
                    else taskMgrListItem = new TaskMgrListItem(exename);
                }
                else taskMgrListItem = new TaskMgrListItem(exename);
            }
            else taskMgrListItem = new TaskMgrListItem(exename);

            //test is 32 bit app in 64os
            if (FormMain.Is64OS)
            {
                if (hprocess != IntPtr.Zero)
                {
                    if (MGetProcessIs32Bit(hprocess))
                        taskMgrListItem.Text = taskMgrListItem.Text + " (" + LanuageFBuffers.Str_Process32Bit + ")";
                }
            }

            p.item = taskMgrListItem;
            p.handle = hprocess;
            p.exename = exename;
            p.pid = pid;
            p.exepath = stringBuilder.ToString();
            p.isWindowsProcess = isw || IsWindowsProcess(exefullpath);

            StringBuilder stringBuilderUserName = new StringBuilder(260);
            if (MGetProcessUserName(p.handle, stringBuilderUserName, 260))
                p.username = stringBuilderUserName.ToString();

            taskMgrListItem.Type = TaskMgrListItemType.ItemProcess;
            taskMgrListItem.IsFullData = true;
            taskMgrListItem.IsDanger = IsVeryImporant(p);
            taskMgrListItem.IsUnEndable = !IsEndable(p);

            //Test service
            bool isSvcHoct = false;
            if (exefullpath != null && (exefullpath.ToLower() == svchostPath || exefullpath.ToLower() == svchostPathwow) || exename == "svchost.exe")
            {
                //svchost.exe add a icon
                taskMgrListItem.Icon = PCMgr.Properties.Resources.icoServiceHost;
                isSvcHoct = true;
            }
            else
            {
                //get pe icon
                IntPtr intPtr = MGetExeIcon(stringBuilder.ToString());
                if (intPtr != IntPtr.Zero) taskMgrListItem.Icon = Icon.FromHandle(intPtr);
            }

            //try get service info
            if (mainPageScMgr.ScCanUse && mainPageScMgr.ScValidPid.Contains(pid))
            {
                //find sc item
                if (mainPageScMgr.ScMgrFindRunSc(p))
                {
                    if (isSvcHoct)
                    {
                        if (p.svcs.Count == 1)
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = LanuageFBuffers.Str_ServiceHost + " : " + p.svcs[0].scName + " (" + mainPageScMgr.ScGroupNameToFriendlyName(p.svcs[0].groupName) + ")";
                            else taskMgrListItem.Text = LanuageFBuffers.Str_ServiceHost + " : " + p.svcs[0].scName;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = LanuageFBuffers.Str_ServiceHost + " : " + mainPageScMgr.ScGroupNameToFriendlyName(p.svcs[0].groupName) + "(" + p.svcs.Count + ")";
                            else taskMgrListItem.Text = LanuageFBuffers.Str_ServiceHost + " (" + p.svcs.Count + ")";
                        }
                    }
                    TaskMgrListItemChild tx = null;
                    for (int i = 0; i < p.svcs.Count; i++)
                    {
                        tx = new TaskMgrListItemChild(p.svcs[i].scDsb, mainPageScMgr.IcoSc);
                        tx.Tag = p.svcs[i].scName;
                        tx.Type = TaskMgrListItemType.ItemService;
                        taskMgrListItem.Childs.Add(tx);
                    }
                    p.isSvchost = true;
                }
            }

            // if (pid == 6064)
            //    ;
            //if ((exefullpath != null && exefullpath.ToLower() == @"‪c:\windows\explorer.exe") 
            //    || (exename != null && exename.ToLower() == @"‪explorer.exe"))
            //   explorerPid = pid;

            //ps data item
            if (SysVer.IsWin8Upper())
                p.isUWP = hprocess == IntPtr.Zero ? false : MGetProcessIsUWP(hprocess);

            taskMgrListItem.Tag = p;

            //13 empty item
            for (int i = 0; i < 13; i++) taskMgrListItem.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listProcess.Font } );

            //Add default data col
            {
                if (cpuindex != -1)
                {
                    taskMgrListItem.SubItems[cpuindex].Text = "- %";
                    taskMgrListItem.SubItems[cpuindex].BackColor = dataGridZeroColor;
                    taskMgrListItem.SubItems[cpuindex].CustomData = 0;
                }
                if (ramindex != -1)
                {
                    taskMgrListItem.SubItems[ramindex].Text = "- M";
                    taskMgrListItem.SubItems[ramindex].BackColor = dataGridZeroColor;
                    taskMgrListItem.SubItems[ramindex].CustomData = 0;
                }
                if (diskindex != -1)
                {

                    taskMgrListItem.SubItems[diskindex].Text = "0 MB/" + LanuageFBuffers.Str_Second;
                    taskMgrListItem.SubItems[diskindex].BackColor = dataGridZeroColor;
                    taskMgrListItem.SubItems[diskindex].CustomData = 0;
                }
                if (netindex != -1)
                {
                    taskMgrListItem.SubItems[netindex].Text = "- Mbps";
                    taskMgrListItem.SubItems[netindex].CustomData = 0;
                    taskMgrListItem.SubItems[netindex].BackColor = dataGridZeroColor;

                    if (!FormMain.IsAdmin)
                        taskMgrListItem.SubItems[netindex].ForeColor = Color.Gray;
                }
            }

            //UWP app

            UwpHostItem hostitem = null;
            if (p.isUWP)
            {
                taskMgrListItem.IsUWP = true;
                if (stateindex != -1)
                {
                    taskMgrListItem.DrawUWPPausedGray = true;
                    taskMgrListItem.SubItems[stateindex].DrawUWPPausedIcon = true;
                }
                if (mainPageUwpMgr.Inited)
                {
                    //get fullname
                    int len = 0;
                    if (!MGetUWPPackageFullName(hprocess, ref len, null))
                        goto OUT;
                    StringBuilder b = new StringBuilder(len);
                    if (!MGetUWPPackageFullName(hprocess, ref len, b))
                        goto OUT;
                    p.uwpFullName = b.ToString();
                    if (p.uwpFullName == "")
                        goto OUT;
                    StringBuilder cb = new StringBuilder(260);
                    if (!MGetUWPPackageIdName(hprocess, cb, 260))
                        goto OUT;
                    p.uwpPackageIdName = cb.ToString();
                    TaskMgrListItem uapp = mainPageUwpMgr.UWPListFindItem(p.uwpFullName);
                    if (uapp == null)
                    {
                        //Try force load uwp info from exe file
                        uapp = mainPageUwpMgr.UWPListTryForceLoadUWPInfo(p.uwpFullName, p.uwpPackageIdName, p.exepath);
                        if (uapp == null) goto OUT;
                    }
                    //copy data form uwp app list
                    if (companyindex != -1)
                        taskMgrListItem.SubItems[companyindex].Text = taskMgrListItem.SubItems[1].Text;
                    taskMgrListItem.IsUWPICO = true;
                    taskMgrListItem.IsFullData = true;
                    taskMgrListItem.Type = TaskMgrListItemType.ItemUWPProcess;
                    taskMgrListItem.IsChildItem = true;

                    UwpItem parentItem = ProcessListFindUWPItem(p.uwpFullName);
                    if (parentItem != null)
                    {
                        //Fill this item to parent item
                        TaskMgrListItemGroup g = parentItem.uwpItem;
                        g.Icon = uapp.Icon;
                        g.Image = uapp.Image;
                        g.Type = TaskMgrListItemType.ItemUWPHost;
                        g.Childs.Add(taskMgrListItem);
                        g.Text = uapp.Text;
                        g.DisplayChildCount = g.Childs.Count > 1;
                        p.uwpItem = parentItem;
                        if (uapp.UWPIcoCusColor)
                            g.UWPIcoColor = uapp.UWPIcoColor;

                        if (ProcessListFindUWPItemWithHostId(p.pid) == null)
                            uwpHostPid.Add(new UwpHostItem(parentItem, p.pid));

                        need_add_tolist = false;
                    }
                    else
                    {
                        //create new uwp item and add this to parent item
                        parentItem = new UwpItem();

                        TaskMgrListItemGroup g = new TaskMgrListItemGroup(uapp.Text);
                        UWP_PACKAGE_INFO pkg = (UWP_PACKAGE_INFO)uapp.Tag;

                        g.Icon = uapp.Icon;
                        g.Image = uapp.Image;
                        g.Childs.Add(taskMgrListItem);
                        g.Type = TaskMgrListItemType.ItemUWPHost;
                        g.Group = listProcess.Groups[1];
                        g.IsUWPICO = true;
                        if (uapp.UWPIcoCusColor)
                            g.UWPIcoColor = uapp.UWPIcoColor;

                        g.PID = (uint)1;
                        //10 empty item
                        for (int i = 0; i < 13; i++) g.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listProcess.Font });
                        if (stateindex != -1)
                        {
                            g.SubItems[stateindex].DrawUWPPausedIcon = true;
                        }
                        if (nameindex != -1) g.SubItems[nameindex].Text = p.uwpFullName;
                        if (pathindex != -1) g.SubItems[pathindex].Text = uapp.SubItems[3].Text;
                        if (netindex != -1)
                        {
                            g.SubItems[netindex].Text = "- Mbps";
                            g.SubItems[netindex].CustomData = 0;
                            g.SubItems[netindex].BackColor = dataGridZeroColor;
                            if (!FormMain.IsAdmin)
                                g.SubItems[netindex].ForeColor = Color.Gray;      
                        }

                        g.Tag = parentItem;

                        parentItem.uwpMainAppDebText = pkg.DisplayName;
                        parentItem.uwpInstallDir = pkg.InstallPath;
                        parentItem.uwpFullName = p.uwpFullName;
                        parentItem.uwpItem = g;
                        p.uwpItem = parentItem;

                        if (ProcessListFindUWPItemWithHostId(p.pid) == null) uwpHostPid.Add(new UwpHostItem(parentItem, p.pid));

                        uwps.Add(parentItem);
                        listProcess.Items.Add(g);
                        need_add_tolist = false;
                    }

                    p.uwpRealApp = p.exepath.Contains(parentItem.uwpInstallDir);

                    //For Icon
                    if (p.exepath != "" && p.uwpRealApp)
                    {
                        taskMgrListItem.Icon = uapp.Icon;
                        if (uapp.UWPIcoCusColor)
                            taskMgrListItem.UWPIcoColor = uapp.UWPIcoColor;
                    }
                    else taskMgrListItem.IsUWPICO = false;

                    if (p.uwpRealApp)
                        taskMgrListItem.Text = uapp.Text;
                }
            }
        OUT:
            if (need_add_tolist)
            {
                hostitem = ProcessListFindUWPItemWithHostId(ppid);
                //UWP app childs
                if (hostitem != null)
                {
                    hostitem.item.uwpItem.Childs.Add(taskMgrListItem);
                    need_add_tolist = false;
                }
            }

            //data items
            ProcessListLoadStaticValues(pid, taskMgrListItem, p);

            //Init performance

            for (int i = 1; i < taskMgrListItem.SubItems.Count; i++)
                taskMgrListItem.SubItems[i].Font = smallListFont;

            thisLoadItem = taskMgrListItem;
            MAppVProcessAllWindowsGetProcessWindow(pid);
            thisLoadItem = null;

            if (taskMgrListItem.Childs.Count > 0)
                taskMgrListItem.Group = listProcess.Groups[0];
            else if (p.isWindowsProcess)
                taskMgrListItem.Group = listProcess.Groups[2];
            else taskMgrListItem.Group = listProcess.Groups[1];

            taskMgrListItem.PID = pid;
            if (need_add_tolist) listProcess.Items.Add(taskMgrListItem);

            ProcessListUpdate(pid, true, taskMgrListItem);
        }
        private void ProcessListLoadStaticValues(uint pid, TaskMgrListItem it, PsItem p)
        {
            if (nameindex != -1)
            {
                if (pid == 0) it.SubItems[nameindex].Text = LanuageFBuffers.Str_IdleProcess;
                else if (pid == 4) it.SubItems[nameindex].Text = "ntoskrnl.exe";
                else it.SubItems[nameindex].Text = p.exename;
            }
            if (pidindex != -1)
            {
                if (pid == 2)
                    it.SubItems[pidindex].Text = "-";
                else it.SubItems[pidindex].Text = pid.ToString();
            }
            if (pathindex != -1) if (p.exepath != "") it.SubItems[pathindex].Text = p.exepath;
            if (cmdindex != -1 && p.handle != Nullptr)
            {
                StringBuilder s = new StringBuilder(2048);
                if (MGetProcessCommandLine(p.handle, s, 2048, pid))
                    it.SubItems[cmdindex].Text = s.ToString();
            }
            if (companyindex != -1)
            {
                if (p.exepath != "")
                {
                    StringBuilder exeCompany = new StringBuilder(256);
                    if (MGetExeCompany(p.exepath, exeCompany, 256))
                        it.SubItems[companyindex].Text = exeCompany.ToString();
                }
            }
        }
        private void ProcessListUpdate(uint pid, bool isload, TaskMgrListItem it, int ipdateOneDataCloum = -1, bool forceProcessHost = false)
        {
            if (!forceProcessHost && (it.Type == TaskMgrListItemType.ItemUWPHost || it.Type == TaskMgrListItemType.ItemProcessHost))
            {
                //Group uppdate
                ProcessListUpdate_GroupChilds(isload, it, ipdateOneDataCloum);

                if (it.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    bool ispause = false;
                    bool running = ProcessListGetUwpIsRunning(it, out ((UwpItem)it.Tag).firstHwnd);
                    if (it.Childs.Count > 0)
                    {
                        foreach (TaskMgrListItem ix in it.Childs)
                            if (ix.Type == TaskMgrListItemType.ItemProcess)
                            {
                                if (((PsItem)ix.Tag).isPaused)
                                {
                                    ispause = true;
                                    break;
                                }
                            }
                        if (stateindex != -1 && stateindex != ipdateOneDataCloum)
                            it.SubItems[stateindex].Text = ispause ? LanuageFBuffers.Str_StatusPaused : "";
                        it.DrawUWPPaused = ispause;
                        if (ispause && running)
                            running = MAppWorkCall3(161, ((UwpItem)it.Tag).firstHwnd) == 1;
                    }
                    it.Group = running ? listProcess.Groups[0] : listProcess.Groups[1];
                }
                else if (it.Type == TaskMgrListItemType.ItemProcessHost)
                {
                    ProcessListUpdate_State(pid, it, (PsItem)it.Tag);
                    ProcessListUpdate_WindowsAndGroup(pid, it, ((PsItem)it.Tag), isload);
                }

                //Performance 
                if (isGlobalBadDataLock) return;

                if (cpuindex != -1 && ipdateOneDataCloum != cpuindex)
                {
                    double d = 0; int datacount = 0;
                    foreach (TaskMgrListItem ix in it.Childs)
                    {
                        if (ix.Type == TaskMgrListItemType.ItemProcess)
                        {
                            d += ix.SubItems[cpuindex].CustomData;
                            datacount++;
                        }
                    }
                    double ii2 = d;
                    it.SubItems[cpuindex].Text = ii2.ToString("0.0") + "%";
                    it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii2, 100);
                    it.SubItems[cpuindex].CustomData = ii2;
                }
                if (ramindex != -1 && ipdateOneDataCloum != ramindex)
                {
                    double d = 0;
                    foreach (TaskMgrListItem ix in it.Childs)
                        if (ix.Type == TaskMgrListItemType.ItemProcess)
                            d += ix.SubItems[ramindex].CustomData;
                    it.SubItems[ramindex].Text = FormatFileSizeMen(Convert.ToInt64(d * 1024));
                    it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(d / 1024, maxMem);
                    it.SubItems[ramindex].CustomData = d;
                }
                if (diskindex != -1 && ipdateOneDataCloum != diskindex)
                {
                    double d = 0;
                    foreach (TaskMgrListItem ix in it.Childs)
                        if (ix.Type == TaskMgrListItemType.ItemProcess)
                            d += ix.SubItems[diskindex].CustomData;
                    if (d < 0.1 && d >= PERF_LIMIT_MIN_DATA_DISK) d = 0.1;
                    else if (d < PERF_LIMIT_MIN_DATA_DISK) d = 0;
                    if (d != 0)
                    {
                        it.SubItems[diskindex].Text = d.ToString("0.0") + " MB/" + LanuageFBuffers.Str_Second;
                        it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(d, 1024);
                        it.SubItems[diskindex].CustomData = d;
                    }
                    else
                    {
                        it.SubItems[diskindex].Text = "0 MB/" + LanuageFBuffers.Str_Second;
                        it.SubItems[diskindex].BackColor = dataGridZeroColor;
                        it.SubItems[diskindex].CustomData = 0;
                    }
                }
                if (netindex != -1 && FormMain.IsAdmin && ipdateOneDataCloum != netindex)
                {
                    double d = 0;
                    foreach (TaskMgrListItem ix in it.Childs)
                        if (ix.Type == TaskMgrListItemType.ItemProcess)
                            d += ix.SubItems[netindex].CustomData;
                    if (d < 0.1 && d >= PERF_LIMIT_MIN_DATA_NETWORK) d = 0.1; else if (d < PERF_LIMIT_MIN_DATA_NETWORK) d = 0;
                    if (d != 0)
                    {
                        it.SubItems[netindex].Text = d.ToString("0.0") + " Mbps";
                        it.SubItems[netindex].CustomData = d;
                        it.SubItems[netindex].BackColor = ProcessListGetColorFormValue(d, 16);
                    }
                    else
                    {
                        it.SubItems[netindex].Text = "0 Mbps";
                        it.SubItems[netindex].CustomData = 0;
                        it.SubItems[netindex].BackColor = dataGridZeroColor;
                    }
                }
            }
            else
            {
                PsItem p = ((PsItem)it.Tag);

                ProcessListUpdate_WindowsAndGroup(pid, it, p, isload);

                if (stateindex != -1)
                {
                    if (ipdateOneDataCloum != stateindex)
                        ProcessListUpdate_State(pid, it, p);
                }
                else ProcessListUpdate_State(pid, it, p);

                if (isGlobalBadDataLock) return;

                if (cpuindex != -1 && ipdateOneDataCloum != cpuindex) ProcessListUpdatePerf_Cpu(pid, it, p);
                if (ramindex != -1 && ipdateOneDataCloum != ramindex) ProcessListUpdatePerf_Ram(pid, it, p);
                if (diskindex != -1 && ipdateOneDataCloum != diskindex) ProcessListUpdatePerf_Disk(pid, it, p);
                if (netindex != -1 && ipdateOneDataCloum != netindex) ProcessListUpdatePerf_Net(pid, it, p);
            }
        }
        private void ProcessListUpdateOnePerfCloum(uint pid, TaskMgrListItem it, int ipdateOneDataCloum, bool forceProcessHost = false)
        {
            if (!forceProcessHost && (it.Type == TaskMgrListItemType.ItemUWPHost || it.Type == TaskMgrListItemType.ItemProcessHost))
            {
                TaskMgrListItem ii = it as TaskMgrListItem;
                if (stateindex != -1 && ipdateOneDataCloum == stateindex)
                {
                    bool ispause = false;

                    if (stateindex != -1 && ipdateOneDataCloum == stateindex && it.Childs.Count > 0)
                    {
                        foreach (TaskMgrListItem ix in it.Childs)
                            if (ix.Type == TaskMgrListItemType.ItemProcess)
                            {
                                PsItem p = ((PsItem)ix.Tag);
                                ProcessListUpdate_State(ix.PID, ix, p);
                                if (ix.SubItems[stateindex].Text == LanuageFBuffers.Str_StatusPaused)
                                {
                                    ispause = true;
                                    break;
                                }
                            }
                        it.SubItems[stateindex].Text = ispause ? LanuageFBuffers.Str_StatusPaused : "";
                    }
                    if (it.Type == TaskMgrListItemType.ItemUWPHost)
                    {
                        bool running = ProcessListGetUwpIsRunning(it, out ((UwpItem)it.Tag).firstHwnd);
                        if (ispause && running)
                            running = MAppWorkCall3(161, ((UwpItem)it.Tag).firstHwnd) == 1;
                        it.Group = running ? listProcess.Groups[0] : listProcess.Groups[1];
                    }
                }
                if (ipdateOneDataCloum > -1)
                {
                    double d = 0; int datacount = 0;
                    foreach (TaskMgrListItem ix in ii.Childs)
                    {
                        if (ix.Type == TaskMgrListItemType.ItemProcess)
                        {
                            ProcessListUpdateOnePerfCloum(ix.PID, ix, ipdateOneDataCloum);
                            d += ix.SubItems[ipdateOneDataCloum].CustomData;
                            datacount++;
                        }
                    }

                    //Performance 
                    if (isGlobalBadDataLock) return;

                    if (cpuindex != -1 && ipdateOneDataCloum == cpuindex)
                    {
                        double ii2 = d;// (d / datacount);
                        it.SubItems[cpuindex].Text = ii2.ToString("0.0") + "%";
                        it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii2, 100);
                        it.SubItems[cpuindex].CustomData = ii2;
                    }
                    else if (ramindex != -1 && ipdateOneDataCloum == ramindex)
                    {
                        it.SubItems[ramindex].Text = FormatFileSizeMen(Convert.ToInt64(d * 1024));
                        it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(d / 1024, maxMem);
                        it.SubItems[ramindex].CustomData = d;
                    }
                    else if (diskindex != -1 && ipdateOneDataCloum == diskindex)
                    {
                        if (d < 0.1 && d >= PERF_LIMIT_MIN_DATA_DISK) d = 0.1;
                        else if (d < PERF_LIMIT_MIN_DATA_DISK) d = 0;
                        if (d != 0)
                        {
                            it.SubItems[diskindex].Text = d.ToString("0.0") + " MB/" + LanuageFBuffers.Str_Second;
                            it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(d, 1024);
                            it.SubItems[diskindex].CustomData = d;
                            return;
                        }
                        it.SubItems[netindex].Text = "0 MB/" + LanuageFBuffers.Str_Second;
                        it.SubItems[netindex].CustomData = 0;
                        it.SubItems[netindex].BackColor = dataGridZeroColor;
                    }
                    else if (netindex != -1 && FormMain.IsAdmin && ipdateOneDataCloum == netindex)
                    {
                        if (d < 0.1 && d >= PERF_LIMIT_MIN_DATA_NETWORK) d = 0.1; else if (d < PERF_LIMIT_MIN_DATA_NETWORK) d = 0;
                        if (d != 0)
                        {
                            it.SubItems[netindex].Text = d.ToString("0.0") + " Mbps";
                            it.SubItems[netindex].CustomData = d;
                            it.SubItems[netindex].BackColor = ProcessListGetColorFormValue(d, 16);
                            return;
                        }
                        it.SubItems[netindex].Text = "0 Mbps";
                        it.SubItems[netindex].CustomData = 0;
                        it.SubItems[netindex].BackColor = dataGridZeroColor;
                    }
                }
            }
            else
            {
                PsItem p = ((PsItem)it.Tag);
                if (stateindex != -1 && ipdateOneDataCloum == stateindex) ProcessListUpdate_State(pid, it, p);

                if (isGlobalBadDataLock) return;

                if (cpuindex != -1 && ipdateOneDataCloum == cpuindex) ProcessListUpdatePerf_Cpu(pid, it, p);
                if (ramindex != -1 && ipdateOneDataCloum == ramindex) ProcessListUpdatePerf_Ram(pid, it, p);
                if (diskindex != -1 && ipdateOneDataCloum == diskindex) ProcessListUpdatePerf_Disk(pid, it, p);
                if (netindex != -1 && ipdateOneDataCloum == netindex) ProcessListUpdatePerf_Net(pid, it, p);
            }
        }

        private void ProcessListUpdateValues(int refeshAllDataColum)
        {
            //update process perf data

            if (!FormMain.IsSimpleView)
            {
                if (refeshAllDataColum != -1)
                    foreach (PsItem p in loadedPs)
                        ProcessListUpdateOnePerfCloum(p.pid, p.item, refeshAllDataColum);

                for (int i = listProcess.ShowedItems.Count - 1; i < listProcess.ShowedItems.Count && i >= 0; i--)
                {
                    if (listProcess.ShowedItems[i].Parent != null) continue;
                    //只刷新显示的条目
                    if (listProcess.ShowedItems[i].Type == TaskMgrListItemType.ItemUWPHost)
                        ProcessListUpdate(listProcess.ShowedItems[i].PID, false, listProcess.ShowedItems[i], refeshAllDataColum);
                    else ProcessListUpdate(listProcess.ShowedItems[i].PID, false, listProcess.ShowedItems[i], refeshAllDataColum);
                }
            }
        }

        //Child & Group
        private int ProcessListUpdate_ChildItemsAdd(TaskMgrListItem it, PsItem p)
        {
            int allCount = 0;
            //递归添加所有子进程
            foreach (PsItem child in p.childs)
            {
                if (!child.isWindowShow)
                {
                    allCount++;

                    if (listProcess.Items.Contains(child.item))
                        listProcess.Items.Remove(child.item);
                    else if (child.item.Parent != null)
                        child.item.Parent.Childs.Remove(child.item);

                    if (!it.Childs.Contains(child.item))
                        it.Childs.Add(child.item);

                    if (child.childs.Count > 0)
                        allCount += ProcessListUpdate_ChildItemsAdd(it, child);
                }
                else
                {
                    if (it.Childs.Contains(child.item))
                        it.Childs.Remove(child.item);
                    else if (child.item.Parent != null)
                        child.item.Parent.Childs.Remove(child.item);

                    if (!listProcess.Items.Contains(child.item))
                        listProcess.Items.Add(child.item);
                }
            }
            return allCount;
        }
        private void ProcessListUpdate_ChildItems(uint pid, TaskMgrListItem it, PsItem p)
        {
            if (p.isWindowShow && p.childs.Count > 0 && !IsExplorer(p) && !it.IsCloneItem)
            {
                if (it.Type != TaskMgrListItemType.ItemProcessHost)
                {
                    it.Type = TaskMgrListItemType.ItemProcessHost;

                    //Clone a child item
                    TaskMgrListItem cloneItem = new TaskMgrListItem();
                    cloneItem.Text = it.Text;
                    cloneItem.PID = it.PID;
                    cloneItem.Type = TaskMgrListItemType.ItemProcess;
                    cloneItem.Tag = p;
                    cloneItem.DisplayChildCount = false;
                    cloneItem.DisplayChildValue = 0;
                    cloneItem.IsCloneItem = true; cloneItem.IsFullData = true;
                    cloneItem.Icon = it.Icon; cloneItem.Image = it.Image;
                    //Copy 13 empty item
                    for (int i = 0; i < 13; i++)
                    {
                        cloneItem.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        cloneItem.SubItems[i].Text = it.SubItems[i].Text;
                        cloneItem.SubItems[i].Font = it.SubItems[i].Font;
                    }
                    //Make it hight light
                    cloneItem.SubItems[0].ForeColor = Color.FromArgb(0x11, 0x66, 0x00);
                    if (pidindex != -1)
                        it.SubItems[pidindex].Text = "";
                    it.Childs.Add(cloneItem);
                }

                it.DisplayChildCount = true;
                it.DisplayChildValue = ProcessListUpdate_ChildItemsAdd(it, p) + 1;

                if (it.DisplayChildValue == 1)
                {
                    if (!ProcessListUpdate_GroupChildsIsValid(it))
                    {
                        it.Type = TaskMgrListItemType.ItemProcess;
                        it.DisplayChildCount = false;
                        ProcessListUpdate_BreakProcHost(it, true);
                    }
                }
            }
            else
            {
                if (it.Type != TaskMgrListItemType.ItemProcess)
                    it.Type = TaskMgrListItemType.ItemProcess;

                it.DisplayChildCount = false;

                if (it.Childs.Count > 0)
                    ProcessListUpdate_BreakProcHost(it, true);
            }
        }
        private void ProcessListUpdate_GroupChilds(bool isload, TaskMgrListItem ii, int ipdateOneDataCloum = -1)
        {
            for (int i = ii.Childs.Count - 1; i >= 0; i--)
            {
                TaskMgrListItem li = ii.Childs[i];
                if (li.Type == TaskMgrListItemType.ItemProcess)
                    ProcessListUpdate(li.PID, isload, li, ipdateOneDataCloum);
            }
        }
        private bool ProcessListUpdate_GroupChildsIsValid(TaskMgrListItem it)
        {
            bool findCloneItem = false, findRealItem = false;
            if (it.Childs.Count > 0)
            {
                for (int i = it.Childs.Count - 1; i >= 0; i--)
                {
                    TaskMgrListItem lics = it.Childs[i];
                    if (lics.Type == TaskMgrListItemType.ItemProcess)
                    {
                        if (lics.IsCloneItem) findCloneItem = true;
                        else findRealItem = true;
                        if (findCloneItem && findRealItem)
                            return true;
                    }
                }
            }
            return findCloneItem && findRealItem;
        }

        //All runtime data
        private void ProcessListUpdate_WindowsAndGroup(uint pid, TaskMgrListItem it, PsItem p, bool isload)
        {
            if (pid > 4)
            {
                //Child and group
                if (!p.isSvchost)
                {
                    //remove invalid windows
                    for (int i = it.Childs.Count - 1; i >= 0; i--)
                    {
                        if (it.Childs[i].Type == TaskMgrListItemType.ItemWindow)
                        {
                            IntPtr h = (IntPtr)it.Childs[i].Tag;
                            if (!IsWindow(h) || !IsWindowVisible(h))
                                it.Childs.Remove(it.Childs[i]);
                        }
                    }
                    if (!isload)
                    {
                        //update window
                        thisLoadItem = it;
                        MAppVProcessAllWindowsGetProcessWindow(pid);
                        thisLoadItem = null;

                        IntPtr firstWindow = IntPtr.Zero;
                        int windowCount = 0;
                        for (int i = it.Childs.Count - 1; i >= 0; i--)
                        {
                            if (it.Childs[i].Type == TaskMgrListItemType.ItemWindow)
                            {
                                if (firstWindow == IntPtr.Zero) firstWindow = (IntPtr)it.Childs[i].Tag;
                                windowCount++;
                            }
                        }
                        //group
                        if (windowCount > 0)
                        {
                            p.isWindowShow = true;
                            if (it.Group != listProcess.Groups[0])
                                it.Group = listProcess.Groups[0];
                            ProcessListUpdate_ChildItems(pid, it, p);
                            if (windowCount == 1)
                                p.firstHwnd = firstWindow;
                            else p.firstHwnd = IntPtr.Zero;
                        }
                        else
                        {
                            p.firstHwnd = IntPtr.Zero;

                            bool needBreak = false;

                            if (p.isWindowsProcess)
                            {
                                if (it.Group != listProcess.Groups[2])
                                {
                                    if (typeindex != -1)
                                        it.SubItems[typeindex].Text = listProcess.Groups[2].Header;
                                    it.Group = listProcess.Groups[2];
                                    needBreak = true;
                                }
                                p.isWindowShow = false;
                            }
                            else
                            {
                                if (it.Group != listProcess.Groups[1])
                                {
                                    if (typeindex != -1)
                                        it.SubItems[typeindex].Text = listProcess.Groups[1].Header;
                                    it.Group = listProcess.Groups[1];
                                    needBreak = true;
                                }
                                p.isWindowShow = false;
                            }

                            if (needBreak && it.Childs.Count > 0)
                                ProcessListUpdate_BreakProcHost(it);
                        }
                    }
                }
                else
                {
                    if (isload)
                    {
                        p.isWindowShow = false;
                        it.Group = listProcess.Groups[p.isWindowsProcess ? 2 : 1];
                        if (typeindex != -1)
                            it.SubItems[typeindex].Text = it.Group.Header;
                    }
                }
            }
        }
        private void ProcessListUpdate_State(uint pid, TaskMgrListItem it, PsItem p)
        {
            int i = MGetProcessState(p.processItem, IntPtr.Zero);
            if (i == 1)
            {
                p.isPaused = false;
                if (stateindex != -1) it.SubItems[stateindex].Text = "";
                if (p.isSvchost == false && it.Childs.Count > 0)
                {
                    bool hung = false;
                    foreach (TaskMgrListItem c in it.Childs)
                        if (c.Type == TaskMgrListItemType.ItemWindow)
                            if (IsHungAppWindow((IntPtr)c.Tag))
                            {
                                hung = true;
                                break;
                            }
                    p.isHung = hung;
                    if (hung)
                    {
                        if (stateindex != -1)
                        {
                            it.SubItems[stateindex].Text = LanuageFBuffers.Str_StatusHung;
                            it.SubItems[stateindex].ForeColor = Color.FromArgb(219, 107, 58);
                        }
                    }
                }
                else p.isHung = false;
            }
            else if (i == 2)
            {
                p.isPaused = true;
                if (stateindex != -1)
                {
                    it.SubItems[stateindex].Text = LanuageFBuffers.Str_StatusPaused;
                    it.SubItems[stateindex].ForeColor = Color.FromArgb(22, 158, 250);
                }
            }
            else if (i == -1)//A bug
            {
                if (loadedPs.Contains(p)) loadedPs.Remove(p);
                if (listProcess.Items.Contains(it)) listProcess.Items.Remove(it);
                return;
            }
            else p.isPaused = false;

            if (p.isUWP && it.Parent != null)
                it.DrawUWPPaused = p.isPaused;
            else it.DrawUWPPaused = false;
        }
        private void ProcessListUpdatePerf_Cpu(uint pid, TaskMgrListItem it, PsItem p)
        {
            if (pid != 0 && p.processItem != IntPtr.Zero)
            {
                double ii = MProcessPerformanctMonitor.GetProcessCpuUseAge(p.processItem);
                it.SubItems[cpuindex].Text = ii.ToString("0.0") + "%";
                it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii, 100);
                it.SubItems[cpuindex].CustomData = ii;
            }
            else
            {
                it.SubItems[cpuindex].Text = "0.0%";
                it.SubItems[cpuindex].BackColor = dataGridZeroColor;
                it.SubItems[cpuindex].CustomData = 0;
            }
        }
        private void ProcessListUpdatePerf_Ram(uint pid, TaskMgrListItem it, PsItem p)
        {
            if (p.isUWP && p.isPaused)
            {
                if (isRamPercentage) it.SubItems[ramindex].Text = "0.1 %";
                else it.SubItems[ramindex].Text = "0.1 MB";
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(0.1, 1024);
                it.SubItems[ramindex].CustomData = 1;
            }
            else if (pid == 2)
            {
                if (isRamPercentage) it.SubItems[ramindex].Text = "0.0 %";
                else it.SubItems[ramindex].Text = "0.0 MB";
                it.SubItems[ramindex].BackColor = dataGridZeroColor;
                it.SubItems[ramindex].CustomData = 1;
            }
            else if (pid == 4 || pid == 0)
            {
                if (isRamPercentage) it.SubItems[ramindex].Text = "0.1 %";
                else it.SubItems[ramindex].Text = "0.1 MB";
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(0.1, 1024);
                it.SubItems[ramindex].CustomData = 1;
            }
            else if (p.processItem != IntPtr.Zero)
            {
                uint ii = MProcessPerformanctMonitor.GetProcessPrivateWoringSet(p.processItem);
                if (isRamPercentage)
                    it.SubItems[ramindex].Text = ProcessListGetPrecentValue(ii, allMem);
                else
                    it.SubItems[ramindex].Text = FormatFileSizeMen(Convert.ToInt64(ii));
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(ii / 1048576, maxMem);
                it.SubItems[ramindex].CustomData = ii / 1024d;
            }
        }
        private void ProcessListUpdatePerf_Disk(uint pid, TaskMgrListItem it, PsItem p)
        {
            if (p.processItem != IntPtr.Zero && p.pid != 2)
            {
                ulong disk = MProcessPerformanctMonitor.GetProcessIOSpeed(p.processItem);
                double val = (disk / 1048576d);
                if (val < 0.1 && val >= PERF_LIMIT_MIN_DATA_DISK)
                {
                    if (isDiskPercentage) it.SubItems[diskindex].Text = "0.1 %";
                    else it.SubItems[diskindex].Text = "0.1 MB/" + LanuageFBuffers.Str_Second;

                    it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(val, 128);
                    it.SubItems[diskindex].CustomData = val;
                }
                else if (val < PERF_LIMIT_MIN_DATA_DISK) val = 0;
                else if (val != 0)
                {
                    if (isDiskPercentage) it.SubItems[diskindex].Text = ProcessListGetPrecentValue(val, maxDiskRate); 
                    else it.SubItems[diskindex].Text = val.ToString("0.0") + " MB/" + LanuageFBuffers.Str_Second;

                    it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(val, 128);
                    it.SubItems[diskindex].CustomData = val;
                    return;
                }
            }

            if (isDiskPercentage) it.SubItems[diskindex].Text = "0.0 %";
            else it.SubItems[diskindex].Text = "0 MB/" + LanuageFBuffers.Str_Second;

            it.SubItems[diskindex].BackColor = dataGridZeroColor;
            it.SubItems[diskindex].CustomData = 0;
        }
        private void ProcessListUpdatePerf_Net(uint pid, TaskMgrListItem it, PsItem p)
        {
            if (!FormMain.IsAdmin) return;
            //if (p.updateLock) { p.updateLock = false; return; }
            if (pid > 4 && MPERF_NET_IsProcessInNet(pid))
            {
                double allMBytesPerSec = MProcessPerformanctMonitor.GetProcessNetworkSpeed(p.processItem) / 1048576d;

                if (allMBytesPerSec < 0.1 && allMBytesPerSec >= PERF_LIMIT_MIN_DATA_NETWORK) allMBytesPerSec = 0.1;
                else if (allMBytesPerSec < PERF_LIMIT_MIN_DATA_NETWORK)
                {
                    if (isNetPercentage) it.SubItems[netindex].Text = "0.1 %";
                    else it.SubItems[netindex].Text = "0.1 Mbps";
                    it.SubItems[netindex].CustomData = allMBytesPerSec;
                    it.SubItems[netindex].BackColor = ProcessListGetColorFormValue(allMBytesPerSec, 16);
                }
                else if (allMBytesPerSec != 0)
                {

                    if (isNetPercentage) it.SubItems[netindex].Text = ProcessListGetPrecentValue(allMBytesPerSec, maxNetRate);
                    else it.SubItems[netindex].Text = allMBytesPerSec.ToString("0.0") + " Mbps";
                    it.SubItems[netindex].CustomData = allMBytesPerSec;
                    it.SubItems[netindex].BackColor = ProcessListGetColorFormValue(allMBytesPerSec, 16);
                    return;
                }
            }

            if (isNetPercentage) it.SubItems[netindex].Text = "0.0 %";
            else it.SubItems[netindex].Text = "0 Mbps";
            it.SubItems[netindex].CustomData = 0;
            it.SubItems[netindex].BackColor = dataGridZeroColor;
        }
        private void ProcessListUpdate_BreakProcHost(TaskMgrListItem it, bool doNotClearall = false)
        {
            if (it.Childs.Count > 0)
            {
                for (int i = it.Childs.Count - 1; i >= 0; i--)
                {
                    TaskMgrListItem lics = it.Childs[i];
                    if (lics.Type == TaskMgrListItemType.ItemProcess)
                    {
                        if (lics.IsCloneItem == false) listProcess.Items.Add(lics);
                        if (doNotClearall) it.Childs.Remove(lics);
                    }
                }
                if (!doNotClearall) it.Childs.Clear();
            }
            if (pidindex != -1) it.SubItems[pidindex].Text = it.PID.ToString();
        }

        //Delete
        private void ProcessListFree(PsItem it)
        {
            //remove invalid item         
            //MAppWorkCall3(174, IntPtr.Zero, new IntPtr(it.pid));

            UwpHostItem hostitem = ProcessListFindUWPItemWithHostId(it.pid);
            if (hostitem != null) uwpHostPid.Remove(hostitem);
            if (it.uwpItem != null)
                it.uwpItem = null;

            it.svcs.Clear();

            foreach (PsItem pchid in it.childs)
                pchid.parent = null;
            it.childs.Clear();

            if (it.parent != null)
            {
                it.parent.childs.Remove(it);
                it.parent = null;
            }

            TaskMgrListItem li = it.item;
            if (li == null) li = ProcessListFindItem(it.pid);
            if (li != null)
                ProcessListRemoveItem(li);
            else Log("ProcessListFree for a no host item : " + it);

            loadedPs.Remove(it);
        }
        public void ProcessListFreeAll()
        {
            if (Inited)
            {
                //the exit clear
                uwps.Clear();
                uwpHostPid.Clear();
                for (int i = 0; i < loadedPs.Count; i++)
                    ProcessListFree(loadedPs[i]);
                loadedPs.Clear();
                listProcess.Items.Clear();

                MProcessMonitor.DestroyProcessMonitor(processMonitor);

                ProcessListSaveCols();
                ProcessListSaveSettings();
                ProcessListUnInitPerfs();
                ProcessListSimpleExit();
            }
        }
        private void ProcessListRemoveItem(TaskMgrListItem li)
        {
            //is a group item
            if (li.Type == TaskMgrListItemType.ItemUWPHost || li.Type == TaskMgrListItemType.ItemProcessHost)
            {
                li.Group = listProcess.Groups[1];
                ProcessListUpdate_BreakProcHost(li);
                listProcess.Items.Remove(li);
            }
            else
            {
                if (li.Parent != null)//is a child item
                {
                    TaskMgrListItem iii = li.Parent;
                    iii.Childs.Remove(li);
                    listProcess.Items.Remove(li);
                    //uwp host item
                    if (iii.Type == TaskMgrListItemType.ItemUWPHost)
                    {
                        if (iii.Childs.Count == 0)//o to remove
                        {
                            listProcess.Items.Remove(iii);
                            UwpItem parentItem = ProcessListFindUWPItem(iii.Tag.ToString());
                            if (parentItem != null)
                                uwps.Remove(parentItem);
                        }
                    }
                }
                else
                {
                    listProcess.Items.Remove(li);
                }
            }
        }

        //CallBacks
        private void ProcessListRemoveItemCallBack(uint pid)
        {
            PsItem oldps = ProcessListFindPsItem(pid);
            if (oldps != null)
            {
                ProcessListFree(oldps);
                Log("Process remove : pid " + pid);
            }
            else
            {
                TaskMgrListItem li = ProcessListFindItem(pid);
                if (li != null) { ProcessListRemoveItem(li); Log("Process remove : pid " + pid); }
                else Log("ProcessListRemoveItemCallBack for a not found item : pid " + pid);
            }
        }
        private void ProcessListNewItemCallBack(uint pid, uint parentid, string exename, string exefullpath, IntPtr hProcess, IntPtr processItem)
        {
            ProcessListLoad(pid, parentid, exename, exefullpath, hProcess, processItem);
            if (!isLoadFull) Log("New process item : " + exename + " (" + pid + ")");
        }

        //Operation
        public void ProcessListEndTask(uint pid)
        {
            ProcessListEndTask(pid, null);
        }
        public void ProcessListSetTo(uint pid)
        {
            TaskMgrListItem i = ProcessListFindItem(pid);
            if (i != null) ProcessListSetTo(i);
        }
        public void ProcessListKillProcTree(uint pid)
        {
            PsItem p = ProcessListFindPsItem(pid);
            if (p != null) ProcessListKillProcTree(p, true);
        }
        public void ProcessListKillCurrentUWP()
        {
            TaskMgrListItem li = listProcess.SelectedItem;
            if (li != null) ProcessListEndTask(0, li);
        }
        public void ProcessListEndCurrentApp()
        {
            TaskMgrListItem li = listApps.SelectedItem;
            if (li == null) return;
            ProcessListEndTask(0, li);
        }
        public void ProcessListSetToCurrentApp()
        {
            TaskMgrListItem li = listApps.SelectedItem;
            if (li == null) return;
            ProcessListSetTo(li);
        }

        private void ProcessListEndTask(uint pid, TaskMgrListItem taskMgrListItem)
        {
            //结束任务
            if (taskMgrListItem == null) taskMgrListItem = ProcessListFindItem(pid);
            if (taskMgrListItem != null)
            {
                if (taskMgrListItem.Type == TaskMgrListItemType.ItemProcessHost)
                {
                    bool ananyrs = false;
                    PsItem p = taskMgrListItem.Tag as PsItem;
                    if (p.isWindowShow && !p.isSvchost)
                    {
                        if (taskMgrListItem.Childs.Count > 0)
                        {
                            IntPtr target = IntPtr.Zero;
                            for (int i = taskMgrListItem.Childs.Count - 1; i >= 0; i--)
                                if (taskMgrListItem.Childs[i].Type == TaskMgrListItemType.ItemWindow)
                                {
                                    target = (IntPtr)taskMgrListItem.Childs[i].Tag;
                                    if (target != IntPtr.Zero)
                                        if (MAppWorkCall3(192, IntPtr.Zero, target) == 1)
                                            ananyrs = true;
                                }
                        }
                        else ananyrs = true;
                    }

                    if (!ananyrs) nextKillItem = taskMgrListItem;
                    else
                    {
                        foreach (TaskMgrListItem lichild in taskMgrListItem.Childs)
                        {
                            if (lichild.Type == TaskMgrListItemType.ItemProcess)
                                if (!MKillProcessUser2(lichild.PID, false, true))
                                    break;
                        }
                        MKillProcessUser2(taskMgrListItem.PID, true, true);
                    }
                }
                else if (taskMgrListItem.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    bool ananyrs = false;
                    IntPtr target = ((UwpItem)taskMgrListItem.Tag).firstHwnd;
                    if (target != Nullptr)
                    {
                        if (MAppWorkCall3(192, IntPtr.Zero, target) == 1)
                            ananyrs = true;
                    }
                    if (ananyrs)
                    {
                        foreach (TaskMgrListItem lichild in taskMgrListItem.Childs)
                        {
                            if (lichild.Type == TaskMgrListItemType.ItemProcess)
                                MKillProcessUser2(lichild.PID, true, true);
                        }
                    }
                    else nextKillItem = taskMgrListItem;
                }
                else if (taskMgrListItem.Type == TaskMgrListItemType.ItemProcess)
                {
                    bool ananyrs = false;
                    PsItem p = taskMgrListItem.Tag as PsItem;
                    if (p.isWindowShow && !p.isSvchost)
                    {
                        if (taskMgrListItem.Childs.Count > 0)
                        {
                            IntPtr target = IntPtr.Zero;
                            for (int i = taskMgrListItem.Childs.Count - 1; i >= 0; i--)
                                if (taskMgrListItem.Childs[i].Tag != null)
                                {
                                    target = (IntPtr)taskMgrListItem.Childs[i].Tag;
                                    if (target != IntPtr.Zero)
                                        if (MAppWorkCall3(192, IntPtr.Zero, target) == 1)
                                            ananyrs = true;
                                }
                        }
                        else ananyrs = true;
                    }
                    if (ananyrs) MKillProcessUser2(taskMgrListItem.PID, true, false);
                }
            }
            else Log2("Not found process item " + pid + " in list.");
        }
        private void ProcessListSetTo(TaskMgrListItem taskMgrListItem)
        {
            //设置到
            if (taskMgrListItem != null)
            {
                if (taskMgrListItem.Type == TaskMgrListItemType.ItemProcessHost || taskMgrListItem.Type == TaskMgrListItemType.ItemProcess)
                {
                    PsItem p = taskMgrListItem.Tag as PsItem;
                    if (p.firstHwnd != IntPtr.Zero) MAppWorkCall3(213, p.firstHwnd, p.firstHwnd);
                }
                else if (taskMgrListItem.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    UwpItem p = taskMgrListItem.Tag as UwpItem;
                    if (p.firstHwnd != IntPtr.Zero) MAppWorkCall3(213, p.firstHwnd, p.firstHwnd);
                }
            }
        }
        private void ProcessListKillProcTree(PsItem p, bool showerr)
        {
            for (int i = p.childs.Count - 1; i >= 0; i--)
            {
                PsItem child = ProcessListFindPsItem(p.childs[i].pid);
                if (child == null)
                {
                    Log2("Not found child " + p.childs[i].pid + " for " + p.pid + ".");
                    continue;
                }
                if (child.childs.Count > 0) ProcessListKillProcTree(child, false);
                MKillProcessUser2(child.pid, showerr, true);
            }
        }
        private void ProcessListKillLastEndItem()
        {
            if (nextKillItem != null)
            {
                if (listProcess.Items.Contains(nextKillItem))
                {
                    if (nextKillItem.Type == TaskMgrListItemType.ItemProcessHost)
                    {
                        foreach (TaskMgrListItem lichild in nextKillItem.Childs)
                        {
                            if (lichild.Type == TaskMgrListItemType.ItemProcess)
                                if (!MKillProcessUser2(lichild.PID, false, true))
                                    break;
                        }
                        MKillProcessUser2(nextKillItem.PID, true, true);
                    }
                    else if (nextKillItem.Type == TaskMgrListItemType.ItemUWPHost)
                    {
                        if (!M_UWP_KillUWPApplication(((UwpItem)nextKillItem.Tag).uwpFullName))
                        {
                            foreach (TaskMgrListItem lichild in nextKillItem.Childs)
                            {
                                if (lichild.Type == TaskMgrListItemType.ItemProcess)
                                    if (!MKillProcessUser2(lichild.PID, false, true))
                                        break;
                            }
                            MKillProcessUser2(nextKillItem.PID, true, true);
                        }
                    }
                }
                else Log("The process exited befor ProcessListKillLastEndItem called.");
                nextKillItem = null;
            }
        }

        //Simple List
        private void ProcessListSimpleInit()
        {
            listApps.NoHeader = true;
            FormMain.expandFewerDetals.Show();
            FormMain.expandFewerDetals.Expanded = true;
        }
        private void ProcessListSimpleExit()
        {
            if (FormMain.IsSimpleView) lastSimpleSize = FormMain.Size;
            SetConfig("OldSizeSimple", "AppSetting", lastSimpleSize.Width + "-" + lastSimpleSize.Height);
            SetConfigBool("SimpleView", "AppSetting", FormMain.IsSimpleView);
        }
        private void ProcessListSimpleRefesh()
        {
            listApps.Locked = true;
            listApps.Items.Clear();
            foreach (TaskMgrListItem li in listProcess.Items)
            {
                if (li.Group == listProcess.Groups[0])
                {
                    if (li.Type == TaskMgrListItemType.ItemProcess)
                        if (IsExplorer((PsItem)li.Tag))
                            continue;
                    if (li.PID != currentProcessPid)
                        listApps.Items.Add(li);
                }
            }
            listApps.Locked = false;
            listApps.SyncItems(true);
            listApps_SelectItemChanged(null, null);
        }

        //Expand & Collapse
        public void ProcessListExpandAll()
        {
            listProcess.Locked = true;
            foreach (TaskMgrListItem li in listProcess.Items)
            {
                if (li.Childs.Count > 0 && !li.ChildsOpened)
                    li.ChildsOpened = true;
            }
            listProcess.Locked = false;
            listProcess.SyncItems(true);
        }
        public void ProcessListCollapseAll()
        {
            listProcess.Locked = true;
            foreach (TaskMgrListItem li in listProcess.Items)
            {
                if (li.Childs.Count > 0 && li.ChildsOpened)
                    li.ChildsOpened = false;
            }
            listProcess.Locked = false;
            listProcess.SyncItems(true);
        }

        //Events
        public void check_showAllProcess_CheckedChanged(object sender, EventArgs e)
        {
            //switch to admin
            //显示所有进程（切换到管理员模式）
            if (!MIsRunasAdmin())
            {
                if (FormMain.check_showAllProcess.Checked)
                {
                    MAppRebotAdmin();
                    FormMain.check_showAllProcess.Checked = false;
                }
                else FormMain.check_showAllProcess.Checked = false;
            }
            else FormMain.check_showAllProcess.Hide();
        }
        public void expandFewerDetals_Click(object sender, EventArgs e)
        {
            if (!FormMain.IsSimpleView)
            {
                lastSize = FormMain.Size;
                FormMain.IsSimpleView = true;
                if (FormMain.Size.Width > lastSimpleSize.Width || FormMain.Size.Height > lastSimpleSize.Height)
                    FormMain.Size = lastSimpleSize;
            }
        }
        public void expandMoreDetals_Click(object sender, EventArgs e)
        {
            if (FormMain.IsSimpleView)
            {
                lastSimpleSize = FormMain.Size;
                FormMain.IsSimpleView = false;
                if (FormMain.Size.Width < lastSize.Width || FormMain.Size.Height < lastSize.Height)
                    FormMain.Size = lastSize;
            }
        }

        //Buttons
        public void btnEndTaskSimple_Click(object sender, EventArgs e)
        {
            TaskMgrListItem taskMgrListItem = listApps.SelectedItem;
            if (taskMgrListItem != null)
                ProcessListEndTask(0, taskMgrListItem);
        }
        public void btnEndProcess_Click(object sender, EventArgs e)
        {
            TaskMgrListItem taskMgrListItem = listProcess.SelectedItem;
            if (taskMgrListItem != null)
            {
                if (taskMgrListItem.Group == listProcess.Groups[0])
                    ProcessListEndTask(0, taskMgrListItem);
                else MAppWorkCall3(178, FormMain.Handle, IntPtr.Zero);
            }
        }

        //Timers
        private void BaseProcessRefeshTimerLowSc_Tick(object sender, EventArgs e)
        {
            if (FormMain.tabControlMain.SelectedTab == FormMain.tabPageProcCtl)
            {
                mainPageScMgr.ScMgrRefeshList();
            }
        }
        private void BaseProcessRefeshTimerLow_Tick(object sender, EventArgs e)
        {
            refeshLowLock = true;
            if (FormMain.tabControlMain.SelectedTab == FormMain.tabPageProcCtl)
            {
                ProcessListForceRefeshAll();
            }
            refeshLowLock = false;
        }
        private void BaseProcessRefeshTimerLowUWP_Tick(object sender, EventArgs e)
        {
            if (FormMain.tabControlMain.SelectedTab == FormMain.tabPageProcCtl)
            {
                ProcessListForceRefeshAllUWP();
            }
        }
        private void StartLoadDealyTimer_Tick(object sender, EventArgs e)
        {
            isGlobalBadDataLock = false;
            refeshLowLock = true;
            ProcessListForceRefeshAllUWP();
            ProcessListForceRefeshAll();
            refeshLowLock = false;
            startLoadDealyTimer.Stop();
        }

        #region ListEvents

        private void listApps_SelectItemChanged(object sender, EventArgs e)
        {
            btnEndTaskSimple.Enabled = listApps.SelectedItem != null;
        }
        private void listApps_KeyDown(object sender, KeyEventArgs e)
        {
            TaskMgrListItem li = listApps.SelectedItem;
            if (li == null) return;
            if (e.KeyCode == Keys.Delete)
                ProcessListEndTask(0, li);
            else if (e.KeyCode == Keys.Apps)
            {
                Point p = listApps.GetItemPoint(li);
                p = listApps.PointToScreen(p);
                MAppWorkCall3(212, new IntPtr(p.X), new IntPtr(p.Y));
                MAppWorkCall3(214, FormMain.Handle, IntPtr.Zero);
            }
        }
        private void listApps_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listApps.SelectedItem != null)
                {
                    MAppWorkCall3(212, new IntPtr(FormMain.MousePosition.X), new IntPtr(FormMain.MousePosition.Y));
                    MAppWorkCall3(214, FormMain.Handle, IntPtr.Zero);
                }
            }
        }
        private void listApps_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TaskMgrListItem li = listApps.SelectedItem;
                if (li == null) return;
                ProcessListSetTo(li);
            }
        }

        private void listProcess_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TaskMgrListItem li = listProcess.SelectedItem;
            if (li == null) return;
            if (e.Button == MouseButtons.Left)
            {
                if (li.OldSelectedItem != null)
                {
                    if (li.OldSelectedItem.Type == TaskMgrListItemType.ItemWindow && li.OldSelectedItem.Tag != null)
                    {
                        IntPtr data = (IntPtr)li.OldSelectedItem.Tag;
                        MAppWorkCall3(213, data, IntPtr.Zero);
                        FormMain.WindowState = FormWindowState.Minimized;
                    }
                }
                else if (li.Type == TaskMgrListItemType.ItemWindow)
                {
                    if (li.Tag != null)
                    {
                        IntPtr data = (IntPtr)li.Tag;
                        MAppWorkCall3(213, data, IntPtr.Zero);
                        FormMain.WindowState = FormWindowState.Minimized;
                    }
                }
                else if (li.Childs.Count > 0)
                {
                    li.ChildsOpened = !li.ChildsOpened;
                    listProcess.SyncItems(true);
                }
            }
        }
        private void listProcess_ShowMenuSelectItem(Point pos = default(Point))
        {
            if (listProcess.SelectedItem != null)
            {
                TaskMgrListItem selectedItem = listProcess.SelectedItem.OldSelectedItem == null ?
                 listProcess.SelectedItem : listProcess.SelectedItem.OldSelectedItem;
                if (selectedItem.Type == TaskMgrListItemType.ItemProcess
                    || selectedItem.Type == TaskMgrListItemType.ItemUWPProcess
                    || selectedItem.Type == TaskMgrListItemType.ItemProcessHost)
                {
                    PsItem t = (PsItem)selectedItem.Tag;
                    int rs = MAppWorkShowMenuProcess(t.exepath, selectedItem.Text, t.pid, FormMain.Handle, t.firstHwnd != FormMain.Handle ? t.firstHwnd : IntPtr.Zero, isSelectExplorer ? 1 : 0, nextSecType, pos.X, pos.Y);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    UwpItem t = (UwpItem)selectedItem.Tag;
                    MAppWorkShowMenuProcess(t.uwpInstallDir, t.uwpFullName, 1, FormMain.Handle, t.firstHwnd, 0, nextSecType, pos.X, pos.Y);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemWindow)
                {
                    MAppWorkCall3(212, new IntPtr(pos.X), new IntPtr(pos.Y));
                    MAppWorkCall3(189, FormMain.Handle, (IntPtr)selectedItem.Tag);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemService)
                {
                    IntPtr scname = Marshal.StringToHGlobalUni((string)selectedItem.Tag);
                    MAppWorkCall3(212, new IntPtr(pos.X), new IntPtr(pos.Y));
                    MAppWorkCall3(184, FormMain.Handle, scname);
                    Marshal.FreeHGlobal(scname);
                }
            }
        }
        private void listProcess_PrepareShowMenuSelectItem()
        {
            if (listProcess.SelectedItem != null)
            {
                TaskMgrListItem selectedItem = listProcess.SelectedItem.OldSelectedItem == null ?
                    listProcess.SelectedItem : listProcess.SelectedItem.OldSelectedItem;
                if (selectedItem.Type == TaskMgrListItemType.ItemProcess
                    || selectedItem.Type == TaskMgrListItemType.ItemUWPProcess
                    || selectedItem.Type == TaskMgrListItemType.ItemProcessHost)
                {
                    PsItem t = (PsItem)selectedItem.Tag;
                    if (IsEndable(t))
                    {
                        btnEndProcess.Enabled = true;
                        MAppWorkShowMenuProcessPrepare(t.exepath, t.exename, t.pid, IsImporant(t), IsVeryImporant(t));

                        if (IsExplorer(t))
                        {
                            nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_REBOOT;
                            btnEndProcess.Text = LanuageFBuffers.Str_Resrat;
                            isSelectExplorer = true;
                        }
                        else
                        {
                            if (t.isWindowShow)
                            {
                                if (stateindex != -1)
                                {
                                    string s = listProcess.SelectedItem.SubItems[stateindex].Text;
                                    if (s == LanuageFBuffers.Str_StatusPaused || s == LanuageFBuffers.Str_StatusHung)
                                    {
                                        btnEndProcess.Text = LanuageFBuffers.Str_Endproc;
                                        nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                                        goto OUT;
                                    }
                                }

                                btnEndProcess.Text = LanuageFBuffers.Str_Endtask;
                                nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_RESENT_BACK;

                            }
                            else
                            {
                                btnEndProcess.Text = LanuageFBuffers.Str_Endproc;
                                nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                            }
                            OUT:
                            isSelectExplorer = false;
                        }
                    }
                    else
                    {
                        nextSecType = 4;
                        btnEndProcess.Enabled = false;
                    }
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_UWP_RESENT_BACK;
                    string exepath = selectedItem.Tag.ToString();
                    MAppWorkShowMenuProcessPrepare(exepath, null, 0, false, false);
                    btnEndProcess.Text = LanuageFBuffers.Str_Endtask;
                    btnEndProcess.Enabled = true;
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemWindow)
                {
                    MAppWorkCall3(198, IntPtr.Zero, (IntPtr)selectedItem.Tag);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemService)
                {
                    IntPtr scname = Marshal.StringToHGlobalUni((string)selectedItem.Tag);
                    MAppWorkCall3(197, IntPtr.Zero, scname);
                    Marshal.FreeHGlobal(scname);
                }
            }
        }
        private void listProcess_MouseClick(object sender, MouseEventArgs e)
        {
            if (listProcess.SelectedItem == null) return;
            if (e.Button == MouseButtons.Right)
            {
                listProcess_PrepareShowMenuSelectItem();
                listProcess_ShowMenuSelectItem();
            }
        }
        private void listProcess_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                listProcess_PrepareShowMenuSelectItem();
        }
        private void listProcess_SelectItemChanged(object sender, EventArgs e)
        {
            if (listProcess.SelectedItem == null) btnEndProcess.Enabled = false;
        }
        private void listProcess_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnEndProcess_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Apps)
            {
                if (listProcess.SelectedItem != null)
                {
                    Point p = listProcess.GetItemPoint(listProcess.SelectedItem);

                    listProcess_PrepareShowMenuSelectItem();
                    listProcess_ShowMenuSelectItem(listProcess.PointToScreen(p));
                }
            }
        }

        private void listProcess_Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left)
            {
                listProcess.Locked = true;
                if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.None)
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                    sortitem = e.Index;
                    sorta = true;
                }
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                    sortitem = e.Index;
                    sorta = true;
                }
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                    sortitem = e.Index;
                    sorta = false;
                }
                lvwColumnSorter.SortColumn = e.Index;
                listProcess.ListViewItemSorter = lvwColumnSorter;
                if (0 == lvwColumnSorter.SortColumn)
                    listProcess.ShowGroup = true;
                else listProcess.ShowGroup = false;
                listProcess.Sort();
                listProcess.Locked = false;
                listProcess.Invalidate();
            }
            else if (e.MouseEventArgs.Button == MouseButtons.Right)
            {
                FormMain.contextMenuStripMainHeader.Show(FormMain.MousePosition);
            }
        }

        private class TaskListViewColumnSorter : ListViewColumnSorter
        {
            private MainPageProcess m;

            public TaskListViewColumnSorter(MainPageProcess m)
            {
                this.m = m;
            }
            public override int Compare(TaskMgrListItem x, TaskMgrListItem y)
            {
                int compareResult = 0;

                if (SortColumn == 0) compareResult = string.Compare(x.Text, y.Text);
                else if (SortColumn == m.cpuindex
                    || SortColumn == m.ramindex
                    || SortColumn == m.diskindex
                    || SortColumn == m.netindex)
                    compareResult = ObjectCompare.Compare(x.SubItems[SortColumn].CustomData, y.SubItems[SortColumn].CustomData);
                else if (SortColumn == m.pidindex)
                    compareResult = ObjectCompare.Compare(x.PID, y.PID);
                else compareResult = string.Compare(x.SubItems[SortColumn].Text, y.SubItems[SortColumn].Text);

                if (compareResult == 0)
                    compareResult = ObjectCompare.Compare(x.PID, y.PID);
                if (Order == SortOrder.Ascending)
                    return compareResult;
                else if (Order == SortOrder.Descending)
                    return (-compareResult);
                return compareResult;
            }
        }

        private void 百分比ToolStripMenuItemRam_Click(object sender, EventArgs e)
        {
            if (!isRamPercentage)
            {
                isRamPercentage = true;
                FormMain.百分比ToolStripMenuItemRam.Checked = true;
                FormMain.值ToolStripMenuItemRam.Checked = false;
                ProcessListRefesh();
            }
        }
        private void 值ToolStripMenuItemRam_Click(object sender, EventArgs e)
        {
            if (isRamPercentage)
            {
                isRamPercentage = false;
                FormMain.百分比ToolStripMenuItemRam.Checked = false;
                FormMain.值ToolStripMenuItemRam.Checked = true;
                ProcessListRefesh();
            }
        }
        private void 百分比ToolStripMenuItemDisk_Click(object sender, EventArgs e)
        {
            if (!isDiskPercentage)
            {
                isDiskPercentage = true;
                FormMain.百分比ToolStripMenuItemDisk.Checked = true;
                FormMain.值ToolStripMenuItemDisk.Checked = false;
                ProcessListRefesh();
            }
        }
        private void 值ToolStripMenuItemDisk_Click(object sender, EventArgs e)
        {
            if (isDiskPercentage)
            {
                isDiskPercentage = false;
                FormMain.百分比ToolStripMenuItemDisk.Checked = false;
                FormMain.值ToolStripMenuItemDisk.Checked = true;
                ProcessListRefesh();
            }
        }
        private void 百分比ToolStripMenuItemNet_Click(object sender, EventArgs e)
        {
            if (!isNetPercentage)
            {
                isNetPercentage = true;
                FormMain.百分比ToolStripMenuItemNet.Checked = true;
                FormMain.值ToolStripMenuItemNet.Checked = false;
                ProcessListRefesh();
            }
        }
        private void 值ToolStripMenuItemNet_Click(object sender, EventArgs e)
        {
            if (isNetPercentage)
            {
                isNetPercentage = false;
                FormMain.百分比ToolStripMenuItemNet.Checked = false;
                FormMain.值ToolStripMenuItemNet.Checked = true;
                ProcessListRefesh();
            }
        }

        #endregion

        #region Headers

        public bool saveheader = true;
        private List<itemheader> headers = new List<itemheader>();
        private itemheaderTip[] headerTips = new itemheaderTip[]{
            new itemheaderTip("TitleCPU", "TipCPU"),
            new itemheaderTip("TitleRam", "TipRam"),
            new itemheaderTip("TitleDisk", "TipDisk"),
            new itemheaderTip("TitleNet", "TipNet"),
        };
        private itemheaderDef[] headerDefs = new itemheaderDef[]{
            new itemheaderDef("TitleProcName", 170),
            new itemheaderDef("TitleType", 100),
            new itemheaderDef("TitlePublisher", 100),
            new itemheaderDef("TitleStatus", 80),
            new itemheaderDef("TitlePID", 55),
            new itemheaderDef("TitleCPU", 75),
            new itemheaderDef("TitleRam", 75),
            new itemheaderDef("TitleDisk", 75),
            new itemheaderDef("TitleNet", 75),
            new itemheaderDef("TitleProcPath", 240),
            new itemheaderDef("TitleCmdLine", 200),
            new itemheaderDef("TitleEProcess", 100),

        };
        private int currHeaderI = 0;
        private int listProcessTryGetHeaderDefaultWidth(string name)
        {
            foreach (itemheaderDef d in headerDefs)
            {
                if (d.herdername == name)
                    return d.width;
            }
            return 100;
        }
        private string listProcessTryGetHeaderTip(string name)
        {
            foreach (itemheaderTip t in headerTips)
            {
                if (t.herdername == name)
                    return LanuageMgr.GetStr(t.name);
            }
            return null;
        }
        private void listProcessAddHeader(string name, int width)
        {
            if (listProcessGetListHeaderItem(name) != null) return;
            headers.Add(new itemheader(currHeaderI, name, width));
            currHeaderI++;
            TaskMgrListHeaderItem li = new TaskMgrListHeaderItem();
            string tip = listProcessTryGetHeaderTip(name);
            if (name != null)
                li.ToolTip = tip;
            li.TextSmall = LanuageMgr.GetStr(name);
            li.Identifier = name;
            li.Width = width;
            listProcess.Colunms.Add(li);
        }
        private int listProcessGetListIndex(string name)
        {
            int rs = -1;
            foreach (TaskMgrListHeaderItem li in listProcess.Colunms)
            {
                if (li.Identifier == name)
                {
                    rs = li.Index;
                    break;
                }
            }
            return rs;
        }
        public itemheader listProcessGetListHeaderItem(string name)
        {
            itemheader rs = null;
            for (int i = 0; i < headers.Count; i++)
            {
                if (headers[i].name == name)
                {
                    rs = headers[i];
                    break;
                }
            }
            return rs;
        }
        private void listProcessAddHeaderMenu(string name)
        {
            ToolStripItem item = new ToolStripMenuItem(LanuageMgr.GetStr(name, false));

            item.Name = name;
            item.Click += MainHeadeMenuItem_Click;
            item.ImageScaling = ToolStripItemImageScaling.None;

            FormMain.contextMenuStripMainHeader.Items.Insert(FormMain.contextMenuStripMainHeader.Items.Count - 2, item);
        }
        private void listProcessCheckHeaderMenu(string name, bool show)
        {
            foreach (ToolStripItem item in FormMain.contextMenuStripMainHeader.Items)
            {
                if ((item is ToolStripMenuItem) && item.Name == name)
                {
                    ((ToolStripMenuItem)item).Checked = show;
                    break;
                }
            }
        }
        private int listProcessGetHeaderMenuDefIndex(string name)
        {
            int index = 1;
            for (int i = 1; i < FormMain.contextMenuStripMainHeader.Items.Count; i++)
            {
                ToolStripItem item = FormMain.contextMenuStripMainHeader.Items[i];
                if ((item is ToolStripMenuItem))
                {
                    if (((ToolStripMenuItem)item).Checked)
                        index++;
                }
                if (item.Name == name)
                    break;
            }
            return index;
        }
        private void listProcessGetAllHeaderIndexs(string name = "")
        {
            if (name == "")
            {
                nameindex = listProcessGetListIndex("TitleProcName");
                companyindex = listProcessGetListIndex("TitlePublisher");
                stateindex = listProcessGetListIndex("TitleStatus");
                pidindex = listProcessGetListIndex("TitlePID");
                cpuindex = listProcessGetListIndex("TitleCPU");
                ramindex = listProcessGetListIndex("TitleRam");
                diskindex = listProcessGetListIndex("TitleDisk");
                netindex = listProcessGetListIndex("TitleNet");
                pathindex = listProcessGetListIndex("TitleProcPath");
                cmdindex = listProcessGetListIndex("TitleCmdLine");
                eprocessindex = listProcessGetListIndex("TitleEProcess");
                typeindex = listProcessGetListIndex("TitleType");
            }
            else
            {
                if (name == "TitleProcName") nameindex = listProcessGetListIndex("TitleProcName");
                else if (name == "TitlePublisher") companyindex = listProcessGetListIndex("TitlePublisher");
                else if (name == "TitleStatus") stateindex = listProcessGetListIndex("TitleStatus");
                else if (name == "TitlePID") pidindex = listProcessGetListIndex("TitlePID");
                else if (name == "TitleCPU") cpuindex = listProcessGetListIndex("TitleCPU");
                else if (name == "TitleRam") ramindex = listProcessGetListIndex("TitleRam");
                else if (name == "TitleDisk") diskindex = listProcessGetListIndex("TitleDisk");
                else if (name == "TitleNet") netindex = listProcessGetListIndex("TitleNet");
                else if (name == "TitleProcPath") pathindex = listProcessGetListIndex("TitleProcPath");
                else if (name == "TitleCmdLine") cmdindex = listProcessGetListIndex("TitleCmdLine");
                else if (name == "TitleEProcess") eprocessindex = listProcessGetListIndex("TitleEProcess");
                else if (name == "TitleType") typeindex = listProcessGetListIndex("TitleType");
            }
        }
        private void listProcessInsertHeader(string name, int width, int index)
        {
            if (listProcessGetListHeaderItem(name) != null) return;
            headers.Add(new itemheader(index, name, width));
            currHeaderI++;
            TaskMgrListHeaderItem li = new TaskMgrListHeaderItem();
            string tip = listProcessTryGetHeaderTip(name);
            if (name != null)
                li.ToolTip = tip;
            li.TextSmall = LanuageMgr.GetStr(name);
            li.Identifier = name;
            li.Width = width;
            listProcess.Colunms.Add(li);

            li.DisplayIndex = index;

            listProcessGetAllHeaderIndexs("");
            listProcess.Header.Invalidate();
            ProcessListForceRefeshAll(true);

        }
        private void listProcessRemoveHeader(string name)
        {
            itemheader h = listProcessGetListHeaderItem(name);
            if (h != null)
            {
                headers.Remove(h);

                foreach (TaskMgrListHeaderItem li in listProcess.Colunms)
                {
                    if (li.Identifier == name)
                    {
                        listProcess.Colunms.Remove(li);
                        break;
                    }
                }

                listProcessGetAllHeaderIndexs("");
                listProcess.Header.Invalidate();
                ProcessListForceRefeshAll();
            }
        }

        private void MainHeadeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item.Checked)
            {
                item.Checked = false;
                listProcessRemoveHeader(item.Name);
            }
            else
            {
                item.Checked = true;
                int index = listProcessGetHeaderMenuDefIndex(item.Name);
                if (index < 1) index = 1;
                listProcessInsertHeader(item.Name, listProcessTryGetHeaderDefaultWidth(item.Name), index);
            }
            listProcessGetAllHeaderIndexs(item.Name);
        }

        int nameindex = 0;
        int companyindex = 0;
        public int stateindex = 0;
        int pidindex = 0;
        int cpuindex = 0;
        int ramindex = 0;
        int diskindex = 0;
        int netindex = 0;
        int pathindex = 0;
        int cmdindex = 0;
        int eprocessindex = 0;
        int typeindex = 0;

        #endregion

        #region Debug

        public void DebugCmd(string[] cmd, uint size)
        {
            if (size >= 2)
            {
                string cmd1 = cmd[1];
                switch (cmd1)
                {
                    case "?":
                    case "help":
                        {
                            LogText("app lg commands help: \n" +
                                "\nfrefuwp mainPageScMgr.ScMgrRefeshList();" +
                                "\nfref ProcessListForceRefeshAll(all)" +
                                "\nfrefsc ProcessListForceRefeshAllUWP" +
                                "\nfrefpidtree ProcessListRefeshPidTree" +
                                "\nrefesh ProcessListRefesh2 (Common refesh function)" +
                                "\nrefeshall ProcessListRefesh" +
                                "\ngetsel get select item info" +
                                "\nsetsel [int:pid] set current select item" +
                                "\nendcur ProcessListEndCurrentApp " +
                                "\nsettocur ProcessListSetToCurrentApp " +
                                "\nenditem [int:pid] ProcessListEndTask" +
                                "\nfindps [int:pid] find psitem" +
                                "\nfinduwp [string:fullname] find uwp item" +
                                "\nvuwpwins view all uwp window");
                            break;
                        }
                    case "frefuwp": mainPageScMgr.ScMgrRefeshList(); break;
                    case "fref": ProcessListForceRefeshAll((size >= 3 && cmd[2] == "all")); break;
                    case "frefsc": ProcessListForceRefeshAllUWP(); break;
                    case "frefpidtree": ProcessListRefeshPidTree(); break;
                    case "refesh": ProcessListRefesh2(); break;
                    case "refeshall":ProcessListRefesh(); break;
                    case "getsel":
                        {
                            if ((size >= 3 && cmd[2] == "app") || FormMain.IsSimpleView)
                                LogText("listApps.SelectedItem (Index " + listApps.SelectedIindex + ") : " + listApps.SelectedItem.ToString());
                            else if ((size >= 3 && cmd[2] == "proc") || !FormMain.IsSimpleView)
                                LogText("listProcess.SelectedItem (Index " + listProcess.SelectedIindex + ") : " + listProcess.SelectedItem.ToString());
                            else LogErr("Nothing to show.");
                            break;
                        }
                    case "setsel":
                        {
                            int targetIndex = 0;
                            if(size>=3&&int.TryParse(cmd[2], out targetIndex))
                            {
                                if (FormMain.IsSimpleView)
                                {
                                    listApps.SelectedItem = listApps.Items[targetIndex];
                                    LogText("listApps.SelectedIndex = " + targetIndex);
                                }
                                else
                                {
                                    listProcess.SelectedItem = listProcess.Items[targetIndex];
                                    LogText("listProcess.SelectedIndex = " + targetIndex);
                                }
                            }
                            else LogErr("Bad pararm 0 [int].");
                            break;
                        }
                    case "endcur": ProcessListEndCurrentApp(); break;
                    case "settocur": ProcessListSetToCurrentApp(); break;
                    case "enditem":
                        {
                            int pid = 0;
                            if (size >= 3 && int.TryParse(cmd[2], out pid))
                            {
                                TaskMgrListItem li = ProcessListFindItem((uint)pid);
                                if (li == null) LogWarn("Not found item " + pid);
                                else
                                {
                                    ProcessListEndTask(0, li);
                                    LogText("ProcessListEndTask " + li.Text);
                                }

                            }
                            else LogErr("Bad pararm 0 [int].");
                            break;
                        }
                    case "findps":
                        {
                            int pid = 0;
                            if (size >= 3 && int.TryParse(cmd[2], out pid))
                            {
                                PsItem p = ProcessListFindPsItem((uint)pid);
                                if (p == null) LogWarn("Not found psitem " + pid);
                                else LogText(p.Print());
                            }
                            else LogErr("Bad pararm 0 [int].");

                            break;
                        }
                    case "finduwp":
                        {
                            if (size >= 3)
                            {
                                UwpItem u = ProcessListFindUWPItem(cmd[2]);
                                if (u == null) LogWarn("Not found UwpItem " + cmd[2]);
                                else LogText(u.Print());
                            }
                            else LogErr("Bad pararm 0 [string].");
                            break;
                        }
                    case "vuwpwins":
                        {
                            string ss = "All uwpwins (" + uwpwins.Count + ") :";
                            foreach (UwpWinItem s in uwpwins)
                                ss += s.ToString() + "\n";
                            LogText(ss);
                            break;
                        }
                }
            }
        }

        #endregion
    }
}
