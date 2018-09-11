using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using PCMgr.Aero.TaskDialog;
using PCMgr.Ctls;
using PCMgr.Helpers;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using PCMgrUWP;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.Win32;
using static PCMgr.NativeMethods.DeviceApi;
using static PCMgr.NativeMethods.CSCall;

namespace PCMgr
{
    public partial class FormMain : Form
    {
        #region ToZz
        public static class 想反编译这个程序吗
        {
            public const string Copyright = "Copyright (C) 2018 DreamFish";
            public const string 版权所有 = "版权所有 Copyright (C) 2018 DreamFish";
            public const string 不用反编译了 = "为什么没有混淆？因为大部分核心功能都在C++模块里（PCMgr32.dll），" +
                "C++的程序多难反编译啊！";
            public const string QQ = "1501076885";
        }
        #endregion

        public FormMain(string[] agrs)
        {
            Instance = this;
            InitializeComponent();
            InitializeCtlText();

            baseProcessRefeshTimer.Interval = 1000;
            baseProcessRefeshTimer.Tick += BaseProcessRefeshTimer_Tick;
            listProcess.Header.CloumClick += Header_CloumClick;
            baseProcessRefeshTimerLow.Interval = 10000;
            baseProcessRefeshTimerLow.Tick += BaseProcessRefeshTimerLow_Tick;
            baseProcessRefeshTimerLowSc.Interval = 120000;
            baseProcessRefeshTimerLowSc.Tick += BaseProcessRefeshTimerLowSc_Tick;
            baseProcessRefeshTimerLowUWP.Interval = 5000;
            baseProcessRefeshTimerLowUWP.Tick += BaseProcessRefeshTimerLowUWP_Tick;
            this.agrs = agrs;

            LoadAllFonts();


        }

        public static string cfgFilePath = "";
        private string[] agrs = null;

        //private bool showSystemProcess = false;
        private bool showHiddenFiles = false;

        private bool processListInited = false;
        private bool driverListInited = false;
        private bool scListInited = false;
        private bool fileListInited = false;
        private bool startListInited = false;
        private bool uwpListInited = false;
        private bool perfInited = false;
        private bool perfMainInited = false;
        private bool perfTrayInited = false;
        private bool perfMainInitFailed = false;
        private bool processListDetailsInited = false;
        private bool usersListInited = false;

        public static FormMain Instance { private set; get; }
        public const string MICROSOFT = "Microsoft Corporation";

        #region ProcessListWork

        private const double PERF_LIMIT_MIN_DATA_DISK = 0.005;
        private const double PERF_LIMIT_MIN_DATA_NETWORK = 0.001;

        private bool forceRefeshLowLock = false;
        private bool refeshLowLock = false;
        private Size lastSimpleSize = new Size();
        private Size lastSize = new Size();
        private int nextSecType = -1;
        private int sortitem = -1;
        private bool sorta = false;
        private bool isFirstLoad = true;
        private bool mergeApps = true;
        private Timer baseProcessRefeshTimer = new Timer();
        private Timer baseProcessRefeshTimerLow = new Timer();
        private Timer baseProcessRefeshTimerLowUWP = new Timer();
        private Timer baseProcessRefeshTimerLowSc = new Timer();
        private TaskListViewColumnSorter lvwColumnSorter = null;

        private bool isGlobalRefeshing = false;
        private bool isGlobalRefeshingAll = false;

        private bool isRamPercentage = false;
        private bool isDiskPercentage = false;
        private bool isNetPercentage = false;

        private class PsItem
        {
            public IntPtr processItem = IntPtr.Zero;
            public IntPtr handle;
            public uint pid;
            public uint ppid;
            public string exename;
            public string exepath;
            public TaskMgrListItem item = null;
            public bool isSvchost = false;
            public bool isUWP = false;
            public bool isWindowShow = false;
            public bool isWindowsProcess = false;
            public bool isPaused = false;
            public bool isHung = false;

            public IntPtr firstHwnd;

            public UwpItem uwpItem = null;
            public string uwpFullName;
            public bool uwpRealApp = false;

            public bool updateLock = false;

            public override string ToString()
            {
                return "(" + pid + ")  " + exename + " " + exepath;
            }

            public PsItem parent = null;
            public List<PsItem> childs = new List<PsItem>();
            public List<ScItem> svcs = new List<ScItem>();
        }
        private class UwpItem
        {
            public string uwpInstallDir = "";
            public TaskMgrListItemGroup uwpItem = null;
            public string uwpFullName = "";
            public string uwpMainAppDebText = "";
            public IntPtr firstHwnd;

            public override string ToString()
            {
                return uwpMainAppDebText + " (" + uwpFullName + ")";
            }
        }
        private class UwpWinItem
        {
            public IntPtr hWnd = IntPtr.Zero;
            public uint ownerPid = 0;
        }
        private class UwpHostItem
        {
            public UwpHostItem(UwpItem item, uint pid)
            {
                this.pid = pid;
                this.item = item;
            }

            public UwpItem item;
            public uint pid;

            public override string ToString()
            {
                return "(" + pid + ")" + item.ToString();
            }
        }

        private bool isSimpleView
        {
            get { return _isSimpleView; }
            set
            {
                _isSimpleView = value;
                if (_isSimpleView)
                {
                    MAppWorkCall3(215, Handle, IntPtr.Zero);
                    pl_simple.Show();
                    tabControlMain.Hide();
                    baseProcessRefeshTimer.Interval = 2000;
                    baseProcessRefeshTimer.Start();
                    BaseProcessRefeshTimer_Tick(this, null);
                    baseProcessRefeshTimerLow.Interval = 10000;
                    baseProcessRefeshTimerLowUWP.Start();
                    baseProcessRefeshTimerLow.Start();
                    listProcess.Locked = true;
                }
                else
                {
                    MAppWorkCall3(216, Handle, IntPtr.Zero);
                    listApps.Items.Clear();
                    pl_simple.Hide();
                    tabControlMain.Show();
                    MAppWorkCall3(163, IntPtr.Zero, IntPtr.Zero);
                    listProcess.Locked = false;
                    ProcessListForceRefeshAll();
                }
            }
        }

        private bool _isSimpleView = false;
        private bool is64OS = false;
        private bool isSelectExplorer = false;
        private uint currentProcessPid = 0;

        private List<UwpHostItem> uwpHostPid = new List<UwpHostItem>();
        private List<PsItem> loadedPs = new List<PsItem>();
        private List<UwpItem> uwps = new List<UwpItem>();
        private List<UwpWinItem> uwpwins = new List<UwpWinItem>();
        private List<string> windowsProcess = new List<string>();
        private List<string> veryimporantProcess = new List<string>();
        private Color dataGridZeroColor = Color.FromArgb(255, 244, 196);

        private string csrssPath = "";
        private string ntoskrnlPath = "";
        private string systemRootPath = "";
        private string svchostPath = "";
        private string svchostPathwow = "";

        private TaskMgrListItem nextKillItem = null;
        private bool isRunAsAdmin = false;
        private Font smallListFont = null;
        private TaskMgrListItem thisLoadItem = null;

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
            Control fp = FromHandle(hWndParent);
            f.ShowDialog(fp);
        }

        private bool IsVeryImporant(ProcessDetalItem p)
        {
            if (p.exepath != null)
            {
                string str = p.exepath.ToLower();
                foreach (string s in veryimporantProcess)
                    if (s == str) return true;
            }
            return false;
        }
        private bool IsImporant(ProcessDetalItem p)
        {
            if (p.exepath != null)
            {
                if (p.exepath.ToLower() == @"c:\windows\system32\svchost.exe") return true;
                return IsWindowsProcess(p.exepath);
            }
            return false;
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

        private IntPtr processMonitor = IntPtr.Zero;

        private void ProcessListInitPerfs()
        {
            if (!perfMainInitFailed)
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
        private void ProcessListInit()
        {
            //初始化
            if (!processListInited)
            {
                currentProcessPid = (uint)MAppWorkCall3(180, IntPtr.Zero, IntPtr.Zero);

                ProcessNewItemCallBack = ProcessListNewItemCallBack;
                ProcessRemoveItemCallBack = ProcessListRemoveItemCallBack;

                ptrProcessNewItemCallBack = Marshal.GetFunctionPointerForDelegate(ProcessNewItemCallBack);
                ptrProcessRemoveItemCallBack = Marshal.GetFunctionPointerForDelegate(ProcessRemoveItemCallBack);

                processMonitor = MProcessMonitor.CreateProcessMonitor(ptrProcessRemoveItemCallBack, ptrProcessNewItemCallBack, Nullptr);
                isRunAsAdmin = MIsRunasAdmin();

                if (!isRunAsAdmin)
                {
                    spl1.Visible = true;
                    check_showAllProcess.Visible = true;
                }

                smallListFont = new Font(tabControlMain.Font.FontFamily, 9f);

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

                processListInited = true;

                if (MIsRunasAdmin())
                    ScMgrInit();
                if (SysVer.IsWin8Upper())
                    UWPListInit();

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

                baseProcessRefeshTimer.Start();
                baseProcessRefeshTimerLowUWP.Start();
                baseProcessRefeshTimerLow.Start();
                baseProcessRefeshTimerLowSc.Start();

                StartingProgressShowHide(false);
            }

        }
        private void ProcessListLoadFinished()
        {
            //firstLoad
            listProcess.Show();
            Cursor = Cursors.Arrow;
        }

        private void ProcessListRefesh()
        {
            //清空整个列表并加载

            uwps.Clear();
            uwpHostPid.Clear();
            uwpwins.Clear();

            if (SysVer.IsWin8Upper()) MAppVProcessAllWindowsUWP();

            listProcess.Locked = true;

            MProcessMonitor.EnumAllProcess(processMonitor);

            ProcessListRefeshPidTree();

            bool refeshAColumData = lvwColumnSorter.SortColumn == cpuindex
              || lvwColumnSorter.SortColumn == ramindex
              || lvwColumnSorter.SortColumn == diskindex
              || lvwColumnSorter.SortColumn == netindex
              || lvwColumnSorter.SortColumn == stateindex;

            lbProcessCount.Text = str_proc_count + " : " + listProcess.Items.Count;

            refeshLowLock = true;
            ProcessListForceRefeshAll();
            refeshLowLock = false;

            listProcess.Locked = false;
            if (refeshAColumData)
                listProcess.Sort(false);//排序
            listProcess.Locked = false;
            //刷新列表
            listProcess.SyncItems(true);
        }
        private void ProcessListRefesh2()
        {
            isGlobalRefeshing = true;

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

            if (!isSimpleView)
            {
                listProcess.Sort(false);//排序
                listProcess.Locked = false;
                //刷新列表
                listProcess.SyncItems(true);

                lbProcessCount.Text = str_proc_count + " : " + listProcess.Items.Count;
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
        private void ProcessListForceRefeshAll(bool refeshStaticValues = false)
        {               
            if(isGlobalRefeshing)
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
                    if(refeshStaticValues)
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
                            if(ix.Type == TaskMgrListItemType.ItemProcess)
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
            if (canUseKernel)
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
                taskMgrListItem = new TaskMgrListItem(str_idle_process);
                stringBuilder.Append(str_IdleProcessDsb);
            }
            else if (pid == 2)
            {
                isw = true;
                exename = str_system_interrupts;
                taskMgrListItem = new TaskMgrListItem(str_system_interrupts);
                stringBuilder.Append(str_InterruptsProcessDsb);
            }
            else if (pid == 4)
            {
                isw = true;
                taskMgrListItem = new TaskMgrListItem("System");
                stringBuilder.Append(ntoskrnlPath);
            }
            else if (pid == 88 && exename == "Registry") { isw = true; taskMgrListItem = new TaskMgrListItem("Registry"); stringBuilder.Append(ntoskrnlPath); }
            else if (pid < 1024 && hprocess == Nullptr && exename == "csrss.exe")
            {
                isw = true;
                taskMgrListItem = new TaskMgrListItem("Client Server Runtime Process");
                stringBuilder.Append(csrssPath);
            }
            else if (exename == "Memory Compression") { isw = true; taskMgrListItem = new TaskMgrListItem("Memory Compression"); stringBuilder.Append(ntoskrnlPath); }
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
            if (is64OS)
            {
                if (hprocess != IntPtr.Zero)
                {
                    if (MGetProcessIs32Bit(hprocess))
                        taskMgrListItem.Text = taskMgrListItem.Text + " (" + str_proc_32 + ")";
                }
            }

            p.item = taskMgrListItem;
            p.handle = hprocess;
            p.exename = exename;
            p.pid = pid;
            p.exepath = stringBuilder.ToString();
            p.isWindowsProcess = isw||  IsWindowsProcess(exefullpath);

            taskMgrListItem.Type = TaskMgrListItemType.ItemProcess;
            taskMgrListItem.IsFullData = true;

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
            if (scCanUse && scValidPid.Contains(pid))
            {
                //find sc item
                if (ScMgrFindRunSc(p))
                {
                    if (isSvcHoct)
                    {
                        if (p.svcs.Count == 1)
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = str_service_host + " : " + p.svcs[0].scName + " (" + ScGroupNameToFriendlyName(p.svcs[0].groupName) + ")";
                            else taskMgrListItem.Text = str_service_host + " : " + p.svcs[0].scName;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = str_service_host + " : " + ScGroupNameToFriendlyName(p.svcs[0].groupName) + "(" + p.svcs.Count + ")";
                            else taskMgrListItem.Text = str_service_host + " (" + p.svcs.Count + ")";
                        }
                    }
                    TaskMgrListItemChild tx = null;
                    for (int i = 0; i < p.svcs.Count; i++)
                    {
                        tx = new TaskMgrListItemChild(p.svcs[0].scDsb, icoSc);
                        tx.Tag = p.svcs[0].scName;
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
            for (int i = 0; i < 13; i++) taskMgrListItem.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());

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
                if (uwpListInited)
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
                    TaskMgrListItem uapp = UWPListFindItem(p.uwpFullName);
                    if (uapp == null)
                        goto OUT;
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

                        if (ProcessListFindUWPItemWithHostId(p.pid) == null)
                            uwpHostPid.Add(new UwpHostItem(parentItem, p.pid));

                        need_add_tolist = false;
                    }
                    else
                    {
                        //create new uwp item and add this to parent item
                        parentItem = new UwpItem();

                        TaskMgrListItemGroup g = new TaskMgrListItemGroup(uapp.Text);
                        UWPPackage pkg = uapp.Tag as UWPPackage;

                        g.Icon = uapp.Icon;
                        g.Image = uapp.Image;
                        g.Childs.Add(taskMgrListItem);
                        g.Type = TaskMgrListItemType.ItemUWPHost;
                        g.Group = listProcess.Groups[1];
                        g.IsUWPICO = true;

                        g.PID = (uint)1;
                        //10 empty item
                        for (int i = 0; i < 13; i++) g.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listProcess.Font });
                        if (stateindex != -1)
                        {
                            g.SubItems[stateindex].DrawUWPPausedIcon = true;
                        }
                        if (nameindex != -1) g.SubItems[nameindex].Text = p.uwpFullName;
                        if (pathindex != -1) g.SubItems[pathindex].Text = uapp.SubItems[4].Text;

                        g.Tag = parentItem;

                        parentItem.uwpMainAppDebText = pkg.MainAppDisplayName;
                        parentItem.uwpInstallDir = pkg.InstalledLocation;
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
                        taskMgrListItem.Icon = uapp.Icon;
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
                if (pid == 0) it.SubItems[nameindex].Text = str_idle_process;
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
                StringBuilder s = new StringBuilder(1024);
                if (MGetProcessCommandLine(p.handle, s, 1024, pid))
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
                            it.SubItems[stateindex].Text = ispause ? str_status_paused : "";
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
                    it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(d / 1024, 1024);
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
                        it.SubItems[diskindex].Text = d.ToString("0.0") + " MB/" + str_sec;
                        it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(d, 1024);
                        it.SubItems[diskindex].CustomData = d;
                    }
                    else
                    {
                        it.SubItems[diskindex].Text = "0 MB/" + str_sec;
                        it.SubItems[diskindex].BackColor = dataGridZeroColor;
                        it.SubItems[diskindex].CustomData = 0;
                    }
                }
                if (netindex != -1 && ipdateOneDataCloum != netindex)
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
                                if (ix.SubItems[stateindex].Text == str_status_paused)
                                {
                                    ispause = true;
                                    break;
                                }
                            }
                        it.SubItems[stateindex].Text = ispause ? str_status_paused : "";
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
                        it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(d / 1024, 1024);
                        it.SubItems[ramindex].CustomData = d;
                    }
                    else if (diskindex != -1 && ipdateOneDataCloum == diskindex)
                    {
                        if (d < 0.1 && d >= PERF_LIMIT_MIN_DATA_DISK) d = 0.1;
                        else if (d < PERF_LIMIT_MIN_DATA_DISK) d = 0;
                        if (d != 0)
                        {
                            it.SubItems[diskindex].Text = d.ToString("0.0") + " MB/" + str_sec;
                            it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(d, 1024);
                            it.SubItems[diskindex].CustomData = d;
                            return;
                        }
                        it.SubItems[netindex].Text = "0 MB/" + str_sec;
                        it.SubItems[netindex].CustomData = 0;
                        it.SubItems[netindex].BackColor = dataGridZeroColor;
                    }
                    else if (netindex != -1 && ipdateOneDataCloum == netindex)
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

                if (cpuindex != -1 && ipdateOneDataCloum == cpuindex) ProcessListUpdatePerf_Cpu(pid, it, p);
                if (ramindex != -1 && ipdateOneDataCloum == ramindex) ProcessListUpdatePerf_Ram(pid, it, p);
                if (diskindex != -1 && ipdateOneDataCloum == diskindex) ProcessListUpdatePerf_Disk(pid, it, p);
                if (netindex != -1 && ipdateOneDataCloum == netindex) ProcessListUpdatePerf_Net(pid, it, p);
            }
        }

        private void ProcessListUpdateValues(int refeshAllDataColum)
        {
            //update process perf data

            if (!isSimpleView)
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
                        if(findCloneItem && findRealItem)
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
                            it.SubItems[stateindex].Text = str_status_hung;
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
                    it.SubItems[stateindex].Text = str_status_paused;
                    it.SubItems[stateindex].ForeColor = Color.FromArgb(22, 158, 250);
                }
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
                it.SubItems[ramindex].Text = "0.1 MB";
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(0.1, 1024);
                it.SubItems[ramindex].CustomData = 1;
            }
            else if (pid == 2)
            {
                it.SubItems[ramindex].Text = "0.0 MB";
                it.SubItems[ramindex].BackColor = dataGridZeroColor;
                it.SubItems[ramindex].CustomData = 1;
            }
            else if (pid == 4 || pid == 0)
            {
                it.SubItems[ramindex].Text = "0.1 MB";
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(0.1, 1024);
                it.SubItems[ramindex].CustomData = 1;
            }
            else if (p.processItem != IntPtr.Zero)
            {
                uint ii = MProcessPerformanctMonitor.GetProcessPrivateWoringSet(p.processItem);
                it.SubItems[ramindex].Text = FormatFileSizeMen(Convert.ToInt64(ii));
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(ii / 1048576, 1024);
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
                    it.SubItems[diskindex].Text = "0.1 MB/" + str_sec;
                    it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(val, 128);
                    it.SubItems[diskindex].CustomData = val;
                }
                else if (val < PERF_LIMIT_MIN_DATA_DISK) val = 0;
                else if (val != 0)
                {
                    it.SubItems[diskindex].Text = val.ToString("0.0") + " MB/" + str_sec;
                    it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(val, 128);
                    it.SubItems[diskindex].CustomData = val;
                    return;
                }
            }

            it.SubItems[diskindex].Text = "0 MB/" + str_sec;
            it.SubItems[diskindex].BackColor = dataGridZeroColor;
            it.SubItems[diskindex].CustomData = 0;
        }
        private void ProcessListUpdatePerf_Net(uint pid, TaskMgrListItem it, PsItem p)
        {
            //if (p.updateLock) { p.updateLock = false; return; }
            if (pid > 4 && MPERF_NET_IsProcessInNet(pid))
            {
                double allMBytesPerSec = MProcessPerformanctMonitor.GetProcessNetworkSpeed(p.processItem) / 1048576d;

                if (allMBytesPerSec < 0.1 && allMBytesPerSec >= PERF_LIMIT_MIN_DATA_NETWORK) allMBytesPerSec = 0.1;
                else if (allMBytesPerSec < PERF_LIMIT_MIN_DATA_NETWORK)
                {
                    it.SubItems[netindex].Text = "0.1 Mbps";
                    it.SubItems[netindex].CustomData = allMBytesPerSec;
                    it.SubItems[netindex].BackColor = ProcessListGetColorFormValue(allMBytesPerSec, 16);
                }
                else if (allMBytesPerSec != 0)
                {
                    it.SubItems[netindex].Text = allMBytesPerSec.ToString("0.0") + " Mbps";
                    it.SubItems[netindex].CustomData = allMBytesPerSec;
                    it.SubItems[netindex].BackColor = ProcessListGetColorFormValue(allMBytesPerSec, 16);
                    return;
                }
            }

            it.SubItems[netindex].Text = "0 Mbps";
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
                if(!doNotClearall) it.Childs.Clear();
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
        private void ProcessListFreeAll()
        {
            //the exit clear
            uwps.Clear();
            uwpHostPid.Clear();
            for (int i = 0; i < loadedPs.Count; i++)
                ProcessListFree(loadedPs[i]);
            loadedPs.Clear();
            listProcess.Items.Clear();

            MProcessMonitor.DestroyProcessMonitor(processMonitor);
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

                    if (listProcess.Items.Contains(li))
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
                else listProcess.Items.Remove(li);
            }
        }

        //CallBacks
        private void ProcessListRemoveItemCallBack(uint pid)
        {
            PsItem oldps = ProcessListFindPsItem(pid);
            if (oldps != null)
            {
                ProcessListFree(oldps);
            }
            else
            {
                TaskMgrListItem li = ProcessListFindItem(pid);
                if (li != null) ProcessListRemoveItem(li);
                else Log("ProcessListRemoveItemCallBack for a not found item : pid " + pid);
            }
        }
        private void ProcessListNewItemCallBack(uint pid, uint parentid, string exename, string exefullpath, IntPtr hProcess, IntPtr processItem)
        {
            if (!isRunAsAdmin && string.IsNullOrEmpty(exefullpath) && pid != 0 && pid != 2 && pid != 4 && pid != 88)
                return;

            ProcessListLoad(pid, parentid, exename, exefullpath, hProcess, processItem);
        }

        //Operation
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

                    if (!ananyrs)
                        nextKillItem = taskMgrListItem;
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
                if (child.childs.Count > 0)
                    ProcessListKillProcTree(child, false);
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
                        if(!M_UWP_KillUWPApplication(((UwpItem)nextKillItem.Tag).uwpFullName))
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
                nextKillItem = null;
            }
        }

        //Simple List
        private void ProcessListSimpleInit()
        {
            listApps.NoHeader = true;
            expandFewerDetals.Show();
            expandFewerDetals.Expanded = true;

            isSimpleView = GetConfigBool("SimpleView", "AppSetting", true);
        }
        private void ProcessListSimpleExit()
        {
            if (isSimpleView)
            {
                lastSimpleSize = Size;
            }
            SetConfig("OldSizeSimple", "AppSetting", lastSimpleSize.Width + "-" + lastSimpleSize.Height);
            SetConfigBool("SimpleView", "AppSetting", isSimpleView);
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
        private void ProcessListExpandAll()
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
        private void ProcessListCollapseAll()
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
        private void check_showAllProcess_CheckedChanged(object sender, EventArgs e)
        {
            //switch to admin
            //显示所有进程（切换到管理员模式）
            if (!MIsRunasAdmin())
            {
                if (check_showAllProcess.Checked)
                {
                    MAppRebotAdmin();
                    check_showAllProcess.Checked = false;
                }
                else check_showAllProcess.Checked = false;
            }
            else check_showAllProcess.Hide();
        }
        private void lbShowDetals_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //if (!MAppVProcess(Handle)) TaskDialog.Show("无法打开详细信息窗口", str_AppTitle, "未知错误。", TaskDialogButton.OK, TaskDialogIcon.Stop);
        }
        private void expandFewerDetals_Click(object sender, EventArgs e)
        {
            if (!isSimpleView)
            {
                lastSize = Size;
                isSimpleView = true;
                if (Size.Width > lastSimpleSize.Width || Size.Height > lastSimpleSize.Height)
                    Size = lastSimpleSize;
            }
        }
        private void expandMoreDetals_Click(object sender, EventArgs e)
        {
            if (isSimpleView)
            {
                lastSimpleSize = Size;
                isSimpleView = false;
                if (Size.Width < lastSize.Width || Size.Height < lastSize.Height)
                    Size = lastSize;
            }
        }

        //Buttons
        private void btnEndTaskSimple_Click(object sender, EventArgs e)
        {
            TaskMgrListItem taskMgrListItem = listApps.SelectedItem;
            if (taskMgrListItem != null)
                ProcessListEndTask(0, taskMgrListItem);
        }
        private void btnEndProcess_Click(object sender, EventArgs e)
        {
            TaskMgrListItem taskMgrListItem = listProcess.SelectedItem;
            if (taskMgrListItem != null)
            {
                if (taskMgrListItem.Group == listProcess.Groups[0])
                    ProcessListEndTask(0, taskMgrListItem);
                else MAppWorkCall3(178, Handle, IntPtr.Zero);
            }
        }

        //Timers
        private void BaseProcessRefeshTimerLowSc_Tick(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageProcCtl)
                ScMgrRefeshList();
        }
        private void BaseProcessRefeshTimerLow_Tick(object sender, EventArgs e)
        {
            refeshLowLock = true;
            if (tabControlMain.SelectedTab == tabPageProcCtl)
                ProcessListForceRefeshAll();
            refeshLowLock = false;
        }
        private void BaseProcessRefeshTimerLowUWP_Tick(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageProcCtl)
                ProcessListForceRefeshAllUWP();
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
                Point p = listApps.GetiItemPoint(li);
                p = listApps.PointToScreen(p);
                MAppWorkCall3(212, new IntPtr(p.X), new IntPtr(p.Y));
                MAppWorkCall3(214, Handle, IntPtr.Zero);
            }
        }
        private void listApps_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listApps.SelectedItem != null)
                {
                    MAppWorkCall3(212, new IntPtr(MousePosition.X), new IntPtr(MousePosition.Y));
                    MAppWorkCall3(214, Handle, IntPtr.Zero);
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
                        WindowState = FormWindowState.Minimized;
                    }
                }
                else if (li.Type == TaskMgrListItemType.ItemWindow)
                {
                    if (li.Tag != null)
                    {
                        IntPtr data = (IntPtr)li.Tag;
                        MAppWorkCall3(213, data, IntPtr.Zero);
                        WindowState = FormWindowState.Minimized;
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
                    int rs = MAppWorkShowMenuProcess(t.exepath, selectedItem.Text, t.pid, Handle, t.firstHwnd != Handle ? t.firstHwnd : IntPtr.Zero, isSelectExplorer ? 1 : 0, nextSecType, pos.X, pos.Y);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    UwpItem t = (UwpItem)selectedItem.Tag;
                    MAppWorkShowMenuProcess(t.uwpInstallDir, t.uwpFullName, 1, Handle, t.firstHwnd, 0, nextSecType, pos.X, pos.Y);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemWindow)
                {
                    MAppWorkCall3(212, new IntPtr(pos.X), new IntPtr(pos.Y));
                    MAppWorkCall3(189, Handle, (IntPtr)selectedItem.Tag);
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemService)
                {
                    IntPtr scname = Marshal.StringToHGlobalUni((string)selectedItem.Tag);
                    MAppWorkCall3(212, new IntPtr(pos.X), new IntPtr(pos.Y));
                    MAppWorkCall3(184, Handle, scname);
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
                    if (t.pid > 4)
                    {
                        btnEndProcess.Enabled = true;
                        MAppWorkShowMenuProcessPrepare(t.exepath, t.exename, t.pid, IsImporant(t), IsVeryImporant(t));

                        if (IsExplorer(t))
                        {
                            nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_REBOOT;
                            btnEndProcess.Text = str_resrat;
                            isSelectExplorer = true;
                        }
                        else
                        {
                            if (t.isWindowShow)
                            {
                                if (stateindex != -1)
                                {
                                    string s = listProcess.SelectedItem.SubItems[stateindex].Text;
                                    if (s == str_status_paused || s == str_status_hung)
                                    {
                                        btnEndProcess.Text = str_endproc;
                                        nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                                        goto OUT;
                                    }
                                }

                                btnEndProcess.Text = str_endtask;
                                nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_RESENT_BACK;

                            }
                            else
                            {
                                btnEndProcess.Text = str_endproc;
                                nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                            }
                            OUT:
                            isSelectExplorer = false;
                        }
                    }
                    else btnEndProcess.Enabled = false;
                }
                else if (selectedItem.Type == TaskMgrListItemType.ItemUWPHost)
                {
                    nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_UWP_RESENT_BACK;
                    string exepath = selectedItem.Tag.ToString();
                    MAppWorkShowMenuProcessPrepare(exepath, null, 0, false, false);
                    btnEndProcess.Text = str_endtask;
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
            if (listProcess.SelectedItem == null)
                btnEndProcess.Enabled = false;
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
                    Point p = listProcess.GetiItemPoint(listProcess.SelectedItem);

                    listProcess_PrepareShowMenuSelectItem();
                    listProcess_ShowMenuSelectItem(listProcess.PointToScreen(p));
                }
            }
        }

        private void Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
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
                contextMenuStripMainHeader.Show(MousePosition);
            }
        }

        private class TaskListViewColumnSorter : ListViewColumnSorter
        {
            private FormMain m;

            public TaskListViewColumnSorter(FormMain m)
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
            if(!isRamPercentage)
            {
                isRamPercentage = true;
                百分比ToolStripMenuItemRam.Checked = true;
                值ToolStripMenuItemRam.Checked = false;
            }
        }
        private void 值ToolStripMenuItemRam_Click(object sender, EventArgs e)
        {
            if (isRamPercentage)
            {
                isRamPercentage = false;
                百分比ToolStripMenuItemRam.Checked = false;
                值ToolStripMenuItemRam.Checked = true;
            }
        }
        private void 百分比ToolStripMenuItemDisk_Click(object sender, EventArgs e)
        {
            if (!isDiskPercentage)
            {
                isDiskPercentage = true;
                百分比ToolStripMenuItemDisk.Checked = true;
                值ToolStripMenuItemDisk.Checked = false;
            }
        }
        private void 值ToolStripMenuItemDisk_Click(object sender, EventArgs e)
        {
            if (isDiskPercentage)
            {
                isDiskPercentage = false;
                百分比ToolStripMenuItemDisk.Checked = false;
                值ToolStripMenuItemDisk.Checked = true;
            }
        }
        private void 百分比ToolStripMenuItemNet_Click(object sender, EventArgs e)
        {
            if (!isNetPercentage)
            {
                isNetPercentage = true;
                百分比ToolStripMenuItemNet.Checked = true;
                值ToolStripMenuItemNet.Checked = false;
            }
        }
        private void 值ToolStripMenuItemNet_Click(object sender, EventArgs e)
        {
            if (isNetPercentage)
            {
                isNetPercentage = false;
                百分比ToolStripMenuItemNet.Checked = false;
                值ToolStripMenuItemNet.Checked = true;
            }
        }

        #endregion

        #region Headers

        public class itemheader
        {
            public itemheader(int index, string name, int wi)
            {
                this.index = index;
                this.name = name;
                width = wi;
                show = true;
            }

            public int width = 0;
            public bool show = false;
            public int index = 0;
            public string name = "";
        }
        public struct itemheaderTip
        {
            public itemheaderTip(string hn, string n)
            {
                herdername = hn;
                name = n;
            }
            public string herdername;
            public string name;
        }
        public struct itemheaderDef
        {
            public itemheaderDef(string hn, int width)
            {
                herdername = hn;
                this.width = width;
            }
            public string herdername;
            public int width;
        }
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
            ToolStripItem item = new ToolStripMenuItem(LanuageMgr.GetStr(name));

            item.Name = name;
            item.Click += MainHeadeMenuItem_Click;

            contextMenuStripMainHeader.Items.Insert(contextMenuStripMainHeader.Items.Count - 2, item);
        }
        private void listProcessCheckHeaderMenu(string name, bool show)
        {
            foreach (ToolStripItem item in contextMenuStripMainHeader.Items)
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
            for (int i = 1; i < contextMenuStripMainHeader.Items.Count; i++)
            {
                ToolStripItem item = contextMenuStripMainHeader.Items[i];
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

        #endregion

        #region ProcessDetalsListWork

        private IntPtr processMonitorDetals = IntPtr.Zero;

        private List<ProcessDetalItem> loadedDetalProcess = new List<ProcessDetalItem>();
        private class ProcessDetalItem
        {
            public ProcessDetalItem()
            {

            }
            public IntPtr handle;
            public uint pid;
            public uint ppid;
            public string exename;
            public string eprocess;
            public string exepath;
            public IntPtr processItem = IntPtr.Zero;
            public ProcessDetalItem parent = null;
            public ListViewItem item = null;
            public List<ProcessDetalItem> childs = new List<ProcessDetalItem>();
        }

        public bool nextUpdateStaticVals = false;

        //Find iten
        private bool ProcessListDetailsIsProcessLoaded(uint pid, out ProcessDetalItem item)
        {
            bool rs = false;
            foreach (ProcessDetalItem f in loadedDetalProcess)
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
        private ProcessDetalItem ProcessListDetailsFindPsItem(uint pid)
        {
            ProcessDetalItem rs = null;
            foreach (ProcessDetalItem i in loadedDetalProcess)
            {
                if (i.pid == pid)
                {
                    rs = i;
                    return rs;
                }
            }
            return rs;
        }

        private void ProcessListDetailsInit()
        {
            if (!processListDetailsInited)
            {
                //if (!processListInited) ProcessListInit();

                listViewItemComparerProcDetals = new ListViewItemComparerProcDetals();

                ProcessNewItemCallBackDetails = ProcessListDetailsNewItemCallBack;
                ProcessRemoveItemCallBackDetails = ProcessListDetailsRemoveItemCallBack;

                ptrProcessNewItemCallBackDetails = Marshal.GetFunctionPointerForDelegate(ProcessNewItemCallBackDetails);
                ptrProcessRemoveItemCallBackDetails = Marshal.GetFunctionPointerForDelegate(ProcessRemoveItemCallBackDetails);

                processMonitorDetals = MProcessMonitor.CreateProcessMonitor(ptrProcessRemoveItemCallBackDetails, ptrProcessNewItemCallBackDetails, Nullptr);

                MAppWorkCall3(160, listProcessDetals.Handle);
                MAppWorkCall3(182, listProcessDetals.Handle);
                listProcessDetals.ListViewItemSorter = listViewItemComparerProcDetals;
                ComCtlApi.MListViewProcListWndProc(listProcessDetals.Handle);

                if (systemRootPath == "") systemRootPath = Marshal.PtrToStringUni(MAppWorkCall4(95, Nullptr, Nullptr));
                if (csrssPath == "") csrssPath = Marshal.PtrToStringUni(MAppWorkCall4(96, Nullptr, Nullptr));
                if (ntoskrnlPath == "") ntoskrnlPath = Marshal.PtrToStringUni(MAppWorkCall4(97, Nullptr, Nullptr));

                ProcessListDetailsLoadColumns();
                ProcessListDetailsILoadAllItem();

                processListDetailsInited = true;
            }
        }
        private void ProcessListDetailsUnInit()
        {
            if (processListDetailsInited)
            {
                ProcessListDetailsSaveColumns();
                ProcessDetalsListFreeAll();

                MProcessMonitor.DestroyProcessMonitor(processMonitorDetals);
                processListDetailsInited = false;
            }
        }

        //CallBacks
        private void ProcessListDetailsRemoveItemCallBack(uint pid)
        {
            ProcessDetalItem oldps = ProcessListDetailsFindPsItem(pid);
            if (oldps != null) ProcessListDetailsFree(oldps);
            else Log("ProcessListDetailsRemoveItemCallBack for a not found item : pid " + pid);
        }
        private void ProcessListDetailsNewItemCallBack(uint pid, uint parentid, string exename, string exefullpath, IntPtr hProcess, IntPtr processItem)
        {
            if (!isRunAsAdmin && string.IsNullOrEmpty(exefullpath) && pid != 0 && pid != 2 && pid != 4 && pid != 88)
                return;
            ProcessListDetailsLoad(pid, parentid, exename, exefullpath, hProcess, processItem);
        }


        //Add item
        private void ProcessListDetailsLoad(uint pid, uint ppid, string exename, string exefullpath, IntPtr hprocess, IntPtr processItem)
        {
            //base
            ProcessDetalItem p = new ProcessDetalItem();

            p.pid = pid;
            p.ppid = ppid;
            loadedDetalProcess.Add(p);

            ProcessDetalItem parentpsItem = null;
            if (ProcessListDetailsIsProcessLoaded(p.ppid, out parentpsItem))
            {
                p.parent = parentpsItem;
                parentpsItem.childs.Add(p);
            }

            if (pid == 0)
                exename = str_idle_process;
            else if (pid == 2)
                exename = str_system_interrupts;
            else if (pid == 4 || exename == "Registry" || exename == "Memory Compression")
                exefullpath = ntoskrnlPath;
            else if (pid < 800 && ppid < 500 && exename == "csrss.exe")
                exefullpath = csrssPath;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(exefullpath);

            PEOCESSKINFO infoStruct = new PEOCESSKINFO();
            if (canUseKernel)
            {
                if (MGetProcessEprocess(pid, ref infoStruct))
                {
                    p.eprocess = infoStruct.Eprocess;

                    if (string.IsNullOrEmpty(exefullpath))
                    {
                        exefullpath = infoStruct.ImageFullName;
                        stringBuilder.Append(exefullpath);
                    }
                }
            }

            ListViewItem li = new ListViewItem();

            p.item = li;
            p.processItem = processItem;
            p.handle = hprocess;
            p.exename = exename;
            p.exepath = stringBuilder.ToString();

            //icon
            li.ImageKey = ProcessListDetailsGetIcon(p.exepath);
            li.Tag = p;

            //16 empty item
            for (int i = 0; i < 16; i++) li.SubItems.Add(new ListViewItem.ListViewSubItem());



            //static items
            ProcessListDetailsUpdateStaticItems(pid, li, p);

            ProcessListDetailsUpdate(pid, true, li);

            listProcessDetals.Items.Add(li);
        }

        //Update dyamic data
        private void ProcessListDetailsUpdateStaticItems(uint pid, ListViewItem li, ProcessDetalItem p)
        {
            //static items
            if (colNameIndex != -1 && string.IsNullOrEmpty(li.SubItems[colNameIndex].Text))  li.SubItems[colNameIndex].Text = p.exename;
            if (colPathIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPathIndex].Text)) li.SubItems[colPathIndex].Text = p.exepath;
            if (colPIDIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPIDIndex].Text))
            {
                if (pid == 2)
                    li.SubItems[colPIDIndex].Text = "-";
                else li.SubItems[colPIDIndex].Text = pid.ToString();
            }
            if (colPPIDIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPPIDIndex].Text)) li.SubItems[colPPIDIndex].Text = p.ppid.ToString();
            if (colDescriptionIndex != -1 && string.IsNullOrEmpty(li.SubItems[colDescriptionIndex].Text))
            {
                if (pid == 0)
                    li.SubItems[colDescriptionIndex].Text = str_IdleProcessDsb;
                else if (pid == 2)
                    li.SubItems[colDescriptionIndex].Text = str_InterruptsProcessDsb;
                else if (p.exepath != "")
                {
                    StringBuilder stringBuilderDescription = new StringBuilder(260);
                    if (MGetExeDescribe(p.exepath, stringBuilderDescription, 260))
                        li.SubItems[colDescriptionIndex].Text = stringBuilderDescription.ToString();
                }
            }

            if (pid == 2)
                goto JUMPADD;

            if (colEprocessIndex != -1 && string.IsNullOrEmpty(li.SubItems[colEprocessIndex].Text))
                li.SubItems[colEprocessIndex].Text = p.eprocess;
            if (colCommandLineIndex != -1 && string.IsNullOrEmpty(li.SubItems[colCommandLineIndex].Text))
            {
                StringBuilder stringBuilderCommandLine = new StringBuilder(512);
                if (p.handle != IntPtr.Zero && MGetProcessCommandLine(p.handle, stringBuilderCommandLine, 512, pid))
                    li.SubItems[colCommandLineIndex].Text = stringBuilderCommandLine.ToString();
            }
            if (is64OS && colPlatformIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPlatformIndex].Text))
            {
                if (MGetProcessIs32Bit(p.handle))
                    li.SubItems[colCommandLineIndex].Text = str_proc_32;
                else li.SubItems[colCommandLineIndex].Text = str_proc_64;
            }

            if (colUserNameIndex != -1 && p.handle != IntPtr.Zero && string.IsNullOrEmpty(li.SubItems[colUserNameIndex].Text))
            {
                StringBuilder stringBuilderUserName = new StringBuilder(260);
                if (MGetProcessUserName(p.handle, stringBuilderUserName, 260))
                    li.SubItems[colUserNameIndex].Text = stringBuilderUserName.ToString();
            }
            if (colSessionIDIndex != -1 && string.IsNullOrEmpty(li.SubItems[colSessionIDIndex].Text))
                li.SubItems[colSessionIDIndex].Text = MGetProcessSessionID(p.processItem).ToString();
            JUMPADD:
            return;
        }
        private void ProcessListDetailsUpdate(uint pid, bool isload, ListViewItem it, int ipdateOneDataCloum = -1, bool forceProcessHost = false)
        {
            ProcessDetalItem p = it.Tag as ProcessDetalItem;
            if (colCPUIndex != -1 && colCPUIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPU(it, p);
            if (colCycleIndex != -1 && colCycleIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_Cycle(it, p);
            if (p.pid == 2) return;
            if (colWorkingSetPrivateIndex != -1 && colWorkingSetPrivateIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetPrivate(it, p);
            if (colWorkingSetIndex != -1 && colWorkingSetIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSet(it, p);
            if (colWorkingSetShareIndex != -1 && colWorkingSetShareIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetShare(it, p);
            if (colPeakWorkingSetIndex != -1 && colPeakWorkingSetIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PeakWorkingSet(it, p);
            if (colNonPagedPoolIndex != -1 && colNonPagedPoolIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_NonPagedPool(it, p);
            if (colPagedPoolIndex != -1 && colPagedPoolIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PagedPool(it, p);
            if (colCommitedSizeIndex != -1 && colCommitedSizeIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CommitedSize(it, p);
            if (colPageErrorIndex != -1 && colPageErrorIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFault(it, p);
            if (colHandleCountIndex != -1 && colHandleCountIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_HandleCount(it, p);
            if (colThreadCountIndex != -1 && colThreadCountIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_ThreadsCount(it, p);
            if (colIOReadIndex != -1 && colIOReadIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IORead(it, p);
            if (colIOWriteIndex != -1 && colIOWriteIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWrite(it, p);
            if (colIOOtherIndex != -1 && colIOOtherIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOther(it, p);
            if (colIOReadBytesIndex != -1 && colIOReadBytesIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOReadBytes(it, p);
            if (colIOWriteBytesIndex != -1 && colIOWriteBytesIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWriteBytes(it, p);
            if (colIOOtherBytesIndex != -1 && colIOOtherBytesIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOtherBytes(it, p);
            if (colCPUTimeIndex != -1 && colCPUTimeIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPUTime(it, p);
            if (colStateIndex != -1 && colStateIndex != ipdateOneDataCloum)
                ProcessListDetails_Update_State(it, p);
            if (colGDIObjectIndex != -1 && colGDIObjectIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_GdiHandleCount(it, p);
            if (colUserObjectIndex != -1 && colUserObjectIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_UserHandleCount(it, p);
            if (colWorkingSetIncreasementIndex != -1 && colWorkingSetIncreasementIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetIncreasement(it, p);
            if (colPageErrorIncreasementIndex != -1 && colPageErrorIncreasementIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFaultIncreasement(it, p);
        }
        //update a column be use to sort
        private void ProcessListDetailsUpdateOnePerfCloum(uint pid, ListViewItem it, int ipdateOneDataCloum, bool forceProcessHost = false)
        {
            ProcessDetalItem p = it.Tag as ProcessDetalItem;
            if (p.pid == 2)
            {
                if (colCPUIndex != -1 && colCPUIndex == ipdateOneDataCloum)
                    ProcessListDetails_Perf_Update_CPU(it, p);
                else if (colCycleIndex != -1 && colCycleIndex == ipdateOneDataCloum)
                    ProcessListDetails_Perf_Update_Cycle(it, p);
                return;
            }
            if (colCPUIndex != -1 && colCPUIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPU(it, p);
            else if (colWorkingSetPrivateIndex != -1 && colWorkingSetPrivateIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetPrivate(it, p);
            else if (colWorkingSetIndex != -1 && colWorkingSetIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSet(it, p);
            else if (colWorkingSetShareIndex != -1 && colWorkingSetShareIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetShare(it, p);
            else if (colPeakWorkingSetIndex != -1 && colPeakWorkingSetIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PeakWorkingSet(it, p);
            else if (colNonPagedPoolIndex != -1 && colNonPagedPoolIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_NonPagedPool(it, p);
            else if (colPagedPoolIndex != -1 && colPagedPoolIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PagedPool(it, p);
            else if (colCommitedSizeIndex != -1 && colCommitedSizeIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CommitedSize(it, p);
            else if (colPageErrorIndex != -1 && colPageErrorIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFault(it, p);
            else if (colHandleCountIndex != -1 && colHandleCountIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_HandleCount(it, p);
            else if (colThreadCountIndex != -1 && colThreadCountIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_ThreadsCount(it, p);
            else if (colIOReadIndex != -1 && colIOReadIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IORead(it, p);
            else if (colIOWriteIndex != -1 && colIOWriteIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWrite(it, p);
            else if (colIOOtherIndex != -1 && colIOOtherIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOther(it, p);
            else if (colIOReadBytesIndex != -1 && colIOReadBytesIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOReadBytes(it, p);
            else if (colIOWriteBytesIndex != -1 && colIOWriteBytesIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWriteBytes(it, p);
            else if (colIOOtherBytesIndex != -1 && colIOOtherBytesIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOtherBytes(it, p);
            else if (colCPUTimeIndex != -1 && colCPUTimeIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPUTime(it, p);
            else if (colCycleIndex != -1 && colCycleIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_Cycle(it, p);
            else if (colStateIndex != -1 && colStateIndex == ipdateOneDataCloum)
                ProcessListDetails_Update_State(it, p);
            else if (colGDIObjectIndex != -1 && colGDIObjectIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_GdiHandleCount(it, p);
            else if (colUserObjectIndex != -1 && colUserObjectIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_UserHandleCount(it, p);
            else if (colWorkingSetIncreasementIndex != -1 && colWorkingSetIncreasementIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetIncreasement(it, p);
            else if (colPageErrorIncreasementIndex != -1 && colPageErrorIncreasementIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFaultIncreasement(it, p);
        }
        private void ProcessListDetailsUpdateValues(int refeshAllDataColum, bool updateStaticItems=false)
        {
            //update process perf data
            foreach (ListViewItem it in listProcessDetals.Items)
            {
                if (updateStaticItems)
                    ProcessListDetailsUpdateStaticItems(((ProcessDetalItem)it.Tag).pid, it, (ProcessDetalItem)it.Tag);
                if (refeshAllDataColum != -1)
                    ProcessListDetailsUpdateOnePerfCloum(((ProcessDetalItem)it.Tag).pid, it, refeshAllDataColum);
            }
            if (updateStaticItems)
            {

                foreach (ColumnHeader c in listProcessDetals.Columns)
                {
                    itemheaderTag t = ((itemheaderTag)c.Tag);
                    if (t.needAutoSize)
                    {
                        listProcessDetals.AutoResizeColumn(c.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                        t.needAutoSize = false;
                    }
                }
            }
            if (listProcessDetals.Items.Count == 0) return;
            int start = listProcessDetals.Items.IndexOf(listProcessDetals.TopItem), end = listProcessDetals.Items.Count;
            ListViewItem liThis = null;
            for (int i = start; i < end; i++)
            {
                liThis = listProcessDetals.Items[i];
                if (liThis.Position.Y < listProcessDetals.Height)
                    ProcessListDetailsUpdate(((ProcessDetalItem)liThis.Tag).pid, false, liThis, refeshAllDataColum);
                else break;
            }
        }

        //All perf data
        private void ProcessListDetails_Perf_Update_CPU(ListViewItem it, ProcessDetalItem p)
        {
            double data = MProcessPerformanctMonitor.GetProcessCpuUseAge(p.processItem);
            it.SubItems[colCPUIndex].Text = data.ToString("00.0");
            it.SubItems[colCPUIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_CPUTime(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessCpuTime(p.processItem);
            TimeSpan time = TimeSpan.FromMilliseconds(Convert.ToDouble(data));
            it.SubItems[colCPUTimeIndex].Text = time.Hours + ":" + time.Minutes + ":" + time.Seconds;
            it.SubItems[colCPUTimeIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_Cycle(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessCycle(p.processItem);
            it.SubItems[colCycleIndex].Text = data.ToString();
            it.SubItems[colCycleIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSetPrivate(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSETPRIVATE);
            it.SubItems[colWorkingSetPrivateIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetPrivateIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSetShare(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSETSHARE);
            it.SubItems[colWorkingSetShareIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetShareIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSet(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSET);
            it.SubItems[colWorkingSetIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSetIncreasement(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSET_INC);
            it.SubItems[colWorkingSetIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PeakWorkingSet(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PEAKWORKINGSET);
            it.SubItems[colPeakWorkingSetIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colPeakWorkingSetIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_NonPagedPool(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_NONPAGEDPOOL);
            it.SubItems[colNonPagedPoolIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colNonPagedPoolIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PagedPool(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PAGEDPOOL);
            it.SubItems[colPagedPoolIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colPagedPoolIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_CommitedSize(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_COMMITEDSIZE);
            it.SubItems[colCommitedSizeIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colCommitedSizeIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PageFault(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PAGEDFAULT);
            it.SubItems[colPageErrorIndex].Text = data.ToString();
            it.SubItems[colPageErrorIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PageFaultIncreasement(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PAGEDFAULT_INC);
            it.SubItems[colPageErrorIndex].Text = data.ToString();
            it.SubItems[colPageErrorIndex].Tag = data;
        }

        private void ProcessListDetails_Perf_Update_HandleCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessHandlesCount(p.processItem);
            it.SubItems[colHandleCountIndex].Text = data.ToString();
            it.SubItems[colHandleCountIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_ThreadsCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessThreadsCount(p.processItem);
            it.SubItems[colThreadCountIndex].Text = data.ToString();
            it.SubItems[colThreadCountIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_GdiHandleCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessGdiHandleCount(p.handle);
            it.SubItems[colGDIObjectIndex].Text = data.ToString();
            it.SubItems[colGDIObjectIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_UserHandleCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessUserHandleCount(p.handle);
            it.SubItems[colUserObjectIndex].Text = data.ToString();
            it.SubItems[colUserObjectIndex].Tag = data;
        }

        private void ProcessListDetails_Perf_Update_IORead(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_READ);
            it.SubItems[colIOReadIndex].Text = data.ToString();
            it.SubItems[colIOReadIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOWrite(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_WRITE);
            it.SubItems[colIOWriteIndex].Text = data.ToString();
            it.SubItems[colIOWriteIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOOther(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_OTHER);
            it.SubItems[colIOOtherIndex].Text = data.ToString();
            it.SubItems[colIOOtherIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOReadBytes(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_READ_BYTES);
            it.SubItems[colIOReadBytesIndex].Text = data.ToString();
            it.SubItems[colIOReadBytesIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOWriteBytes(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_WRITE_BYTES);
            it.SubItems[colIOWriteBytesIndex].Text = data.ToString();
            it.SubItems[colIOWriteBytesIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOOtherBytes(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_OTHER_BYTES);
            it.SubItems[colIOOtherBytesIndex].Text = data.ToString();
            it.SubItems[colIOOtherBytesIndex].Tag = data;
        }

        private void ProcessListDetails_Update_State(ListViewItem it, ProcessDetalItem p)
        {
            int i = MGetProcessState(p.processItem, IntPtr.Zero);
            if (i == 1)
            {
                it.SubItems[colStateIndex].Text = "";
                /*if (p.isSvchost == false && it.Childs.Count > 0)
                {
                    bool hung = false;
                    foreach (TaskMgrListItem c in it.Childs)
                        if (c.Type == TaskMgrListItemType.ItemWindow)
                            if (IsHungAppWindow((IntPtr)c.Tag))
                            {
                                hung = true;
                                break;
                            }
                    if (hung)
                    {
                        it.SubItems[colStateIndex].Text = str_status_hung;
                        it.SubItems[colStateIndex].ForeColor = Color.FromArgb(219, 107, 58);
                    }
                }*/
            }
            else if (i == 2)
            {
                it.SubItems[colStateIndex].Text = str_status_paused;
                it.SubItems[colStateIndex].ForeColor = Color.FromArgb(22, 158, 250);
            }
        }


        //Full load
        private void ProcessListDetailsILoadAllItem()
        {
            ComCtlApi.MListViewProcListLock(true);
            MProcessMonitor.EnumAllProcess(processMonitorDetals);
            ComCtlApi.MListViewProcListLock(false);
            listProcessDetals.Invalidate();
        }

        //Refesh
        private void ProcessListDetailsRefesh()
        {
            ComCtlApi.MListViewProcListLock(true);

            //刷新所有数据
            MProcessMonitor.RefeshAllProcess(processMonitorDetals);
            //刷新性能数据
            bool refeshAColumData = ProcessListDetailsIsDyamicDataColumn(listViewItemComparerProcDetals.SortColumn);
            ProcessListDetailsUpdateValues(refeshAColumData ? listViewItemComparerProcDetals.SortColumn : -1, nextUpdateStaticVals);
            ProcessListRefeshPidTree();

            nextUpdateStaticVals = false;

            listProcessDetals.Sort();
            ComCtlApi.MListViewProcListLock(false);
            listProcessDetals.Invalidate();
        }
        private void ProcessListDetailsFree(ProcessDetalItem it, bool delitem = true)
        {
            //remove invalid item
            //MAppWorkCall3(174, IntPtr.Zero, new IntPtr(it.pid));

            it.childs.Clear();
            if (it.parent != null && it.parent.childs.Contains(it))
                it.parent.childs.Remove(it);
            it.parent = null;
            loadedDetalProcess.Remove(it);
            if (delitem) listProcessDetals.Items.Remove(it.item);
        }
        private void ProcessDetalsListFreeAll()
        {
            listProcessDetals.Items.Clear();
            //the exit clear
            for (int i = 0; i < loadedDetalProcess.Count; i++)
                ProcessListDetailsFree(loadedDetalProcess[i], false);
            loadedDetalProcess.Clear();
        }

        //Ico
        private string ProcessListDetailsGetIcon(string exepath)
        {
            if (exepath == "") exepath = "Default";
            if (!imageListProcessDetalsIcons.Images.ContainsKey(exepath))
            {
                IntPtr intPtr = MGetExeIcon(exepath == "Default" ? null : exepath);
                if (intPtr != IntPtr.Zero)
                    imageListProcessDetalsIcons.Images.Add(exepath, Icon.FromHandle(intPtr));
            }
            return exepath;
        }

        //Events
        private void listProcessDetals_MouseClick(object sender, MouseEventArgs e)
        {
            if (listProcessDetals.SelectedItems.Count == 0) return;
            ProcessDetalItem ps = listProcessDetals.SelectedItems[0].Tag as ProcessDetalItem;
            if (e.Button == MouseButtons.Left)
            {
                if (ps.pid > 4)
                {
                    btnEndProcess.Enabled = true;
                    MAppWorkShowMenuProcessPrepare(ps.exepath, ps.exename, ps.pid, IsImporant(ps), IsVeryImporant(ps));
                    nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                }
                else btnEndProcessDetals.Enabled = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                MAppWorkShowMenuProcessPrepare(ps.exepath, ps.exename, ps.pid, IsImporant(ps), IsVeryImporant(ps));
                MAppWorkShowMenuProcess(ps.exepath, ps.exename, ps.pid, Handle, IntPtr.Zero, 0, nextSecType, MousePosition.X, MousePosition.Y);
            }
        }
        private void listProcessDetals_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEndProcessDetals.Enabled = listProcessDetals.SelectedItems.Count != 0;
        }
        private void listProcessDetals_KeyDown(object sender, KeyEventArgs e)
        {
            if (listProcessDetals.SelectedItems.Count > 0)
            {
                if (e.KeyCode == Keys.Apps)
                {
                    ListViewItem item = listProcessDetals.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listProcessDetals.PointToScreen(p);
                    ProcessDetalItem ps = item.Tag as ProcessDetalItem;
                    MAppWorkShowMenuProcess(ps.exepath, ps.exename, ps.pid, Handle, IntPtr.Zero, 0, nextSecType, p.X, p.Y);
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    ListViewItem item = listProcessDetals.SelectedItems[0];
                    ProcessDetalItem ps = item.Tag as ProcessDetalItem;
                    MAppWorkShowMenuProcessPrepare(ps.exepath, ps.exename, ps.pid, IsImporant(ps), IsVeryImporant(ps));
                    MAppWorkCall3(178, Handle, IntPtr.Zero);
                }
            }
        }

        private void btnEndProcessDetals_Click(object sender, EventArgs e)
        {
            if (listProcessDetals.SelectedItems.Count == 0) return;
            MAppWorkCall3(178, Handle, IntPtr.Zero);
        }

        #region Columns

        private int colLastDown = -1;

        private int colEprocessIndex = -1;//ok
        private int colUserNameIndex = -1;//ok
        private int colNameIndex = -1;//ok
        private int colPackageNameIndex = -1;//n
        private int colPIDIndex = -1;//ok
        private int colPPIDIndex = -1;//ok
        private int colStateIndex = -1;//ok
        private int colSessionIDIndex = -1;//ok
        private int colJobIDIndex = -1;
        private int colCPUIndex = -1;//ok
        private int colCPUTimeIndex = -1;//ok
        private int colCycleIndex = -1;//ok
        private int colPeakWorkingSetIndex = -1;//ok
        private int colWorkingSetIncreasementIndex = -1;//ok
        private int colWorkingSetIndex = -1;//ok
        private int colWorkingSetPrivateIndex = -1;//ok
        private int colWorkingSetShareIndex = -1;//ok
        private int colCommitedSizeIndex = -1;//ok
        private int colPagedPoolIndex = -1;//ok
        private int colNonPagedPoolIndex = -1;//ok
        private int colPageErrorIndex = -1;//ok
        private int colPageErrorIncreasementIndex = -1;//ok
        private int colHandleCountIndex = -1;//ok
        private int colThreadCountIndex = -1;//ok
        private int colUserObjectIndex = -1;//ok
        private int colGDIObjectIndex = -1;//ok
        private int colIOReadIndex = -1;//ok
        private int colIOWriteIndex = -1;//ok
        private int colIOOtherIndex = -1;//ok
        private int colIOReadBytesIndex = -1;//ok
        private int colIOWriteBytesIndex = -1;//ok
        private int colIOOtherBytesIndex = -1;//ok
        private int colPathIndex = -1;//ok
        private int colCommandLineIndex = -1;//ok
        private int colPlatformIndex = -1;//ok
        private int colOSContextIndex = -1;
        private int colDescriptionIndex = -1;//ok
        private int colDepIndex = -1;
        private int colUACVIndex = -1;

        private IntPtr hListHeader = IntPtr.Zero;
        private ToolTip colsTip = new ToolTip();

        public string[] allCols = new string[]
        {
            "TitlePID","TitlePackageName","TitleStatus","TitleSessionID","TitleJobID","TitleParentPID","TitleCycle",
            "TitleCPU","TitleCPUTime","TitlePeakWorkingSet","TitleWorkingSetCrease",
            "TitleWorkingSet","TitleWorkingSetPrivate","TitleWorkingSetShare","TitleCommited",
            "TitlePagedPool","TitleNonPagedPool","TitlePagedError","TitlePagedErrorCrease","TitleHandleCount",
            "TitleThreadCount","TitleUserObject","TitleGdiObject","TitleIORead","TitleIOWrite","TitleIOOther",
            "TitleIOReadBytes","TitleIOWriteBytes","TitleIOOtherBytes","TitleProcPath","TitleCmdLine","TitleEProcess",
            "TitlePlatform","TitleOperationSystemContext","TitleDescription","TitleDEP","TitleUACVirtualization","TitleProcName"
        };
        public string[] numberCols = new string[]
        {
            "TitlePID","TitleSessionID","TitleJobID","TitleParentPID","TitleCycle",
            "TitleCPU","TitlePeakWorkingSet","TitleWorkingSetCrease",
            "TitleWorkingSet","TitleWorkingSetPrivate","TitleWorkingSetShare","TitleCommited",
            "TitlePagedPool","TitleNonPagedPool","TitlePagedError","TitlePagedErrorCrease","TitleHandleCount",
            "TitleThreadCount","TitleUserObject","TitleGdiObject","TitleIORead","TitleIOWrite","TitleIOOther",
            "TitleIOReadBytes","TitleIOWriteBytes","TitleIOOtherBytes"
        };

        private itemheaderTip[] detailsHeaderTips = new itemheaderTip[]{
            new itemheaderTip("TitleCPU", "TipCPU"),
            new itemheaderTip("TitlePID", "TipPID"),
            new itemheaderTip("TitleStatus", "TipStatus"),
            new itemheaderTip("TitleJobID", "TipJobID"),
            new itemheaderTip("TitleWorkingSetPrivate", "TipPrivateWorkingSet"),
            new itemheaderTip("TitleCPUTime", "TipCPUTime"),
            new itemheaderTip("TitleCycle", "TipCycle"),
            new itemheaderTip("TitleCommited", "TipCommitedSize"),
            new itemheaderTip("TitlePagedPool", "TipPagedSize"),
            new itemheaderTip("TitleNonPagedPool", "TipNonPagedSize"),
            new itemheaderTip("TitlePagedError", "TipPageErr"),
            new itemheaderTip("TitleHandleCount", "TipHandleCount"),
            new itemheaderTip("TitleThreadCount", "TipThredCount"),
            new itemheaderTip("TitleCmdLine", "TipCmdLine"),
            new itemheaderTip("TitleUserObject", "TipUserObject"),
            new itemheaderTip("TitleGdiObject", "TipGDIObject"),
            new itemheaderTip("TitlePlatform", "TipPlatform"),
            new itemheaderTip("TitleWorkingSet", "TipWorkingSet"),
            new itemheaderTip("TitleWorkingSetShare", "TipShareWorkingSet"),
            new itemheaderTip("TitleIOOther", "TipIOOther"),
            new itemheaderTip("TitleIOOtherBytes", "TipIOOtherBytes"),
            new itemheaderTip("TitleIORead", "TipIORead"),
            new itemheaderTip("TitleIOReadBytes", "TipIOReadBytes"),
            new itemheaderTip("TitleIOWrite", "TipIOWrite"),
            new itemheaderTip("TitleIOWriteBytes", "TipIOWriteBytes"),
        };
        private class itemheaderTag
        {
            public string tip;
            public bool needAutoSize;
        }

        public string ProcessListDetailsGetHeaderTip(string name)
        {
            foreach (itemheaderTip t in detailsHeaderTips)
            {
                if (t.herdername == name)
                    return LanuageMgr.GetStr(t.name);
            }
            return null;
        }
        public void ProcessListDetailsAddHeader(string name, int width = -1)
        {
            itemheaderTag t = new itemheaderTag();
            ColumnHeader li = new ColumnHeader();
            li.Name = name;
            li.Text = LanuageMgr.GetStr(name);
            if (width == -1)
            {
                t.needAutoSize = true;
                width = 100;
            }
            else t.needAutoSize = false;
            li.Width = width;
            string tip = ProcessListDetailsGetHeaderTip(name);
            if (ProcessListDetailsIsMumberColumn(name))
                li.TextAlign = HorizontalAlignment.Right;
            listProcessDetals.Columns.Add(li);

            if (tip != null) t.tip = tip;

            li.Tag = t;
        }
        public int ProcessListDetailsGetListIndex(string name)
        {
            int rs = -1;
            ColumnHeader c = ProcessListDetailsFindHeader(name);
            if (c != null)
                rs = listProcessDetals.Columns.IndexOf(c);
            return rs;
        }
        public ColumnHeader ProcessListDetailsFindHeader(string name)
        {
            ColumnHeader rs = null;
            foreach (ColumnHeader c in listProcessDetals.Columns)
                if (c.Name == name)
                {
                    rs = c;
                    break;
                }
            return rs;
        }
        public void ProcessListDetailsRemoveHeader(string name)
        {
            ColumnHeader li = ProcessListDetailsFindHeader(name);
            if (li != null) listProcessDetals.Columns.Remove(li);
        }

        private bool ProcessListDetailsIsDyamicDataColumn(int index)
        {
            if (index != -1)
            {
                if (index == colCPUIndex || index == colCPUTimeIndex || index == colCycleIndex
                    || index == colPeakWorkingSetIndex || index == colWorkingSetIncreasementIndex || index == colWorkingSetIndex
                    || index == colWorkingSetPrivateIndex || index == colWorkingSetShareIndex || index == colCommitedSizeIndex
                    || index == colPagedPoolIndex || index == colHandleCountIndex
                    || index == colNonPagedPoolIndex || index == colPageErrorIndex || index == colPageErrorIncreasementIndex
                    || index == colThreadCountIndex || index == colGDIObjectIndex || index == colUserObjectIndex
                    || index == colIOOtherBytesIndex || index == colIOWriteBytesIndex || index == colIOReadBytesIndex
                    || index == colIOOtherIndex || index == colIOWriteIndex || index == colIOReadIndex)
                    return true;
            }
            return false;
        }
        private bool ProcessListDetailsIsMumberColumn(string name)
        {
            if (name != "")
            {
                foreach (string i in numberCols)
                    if (i == name)
                        return true;
            }
            return false;
        }
        public bool ProcessListDetailsIsStringColumn(int index)
        {
            if (index != -1)
            {
                if (index == colNameIndex || index == colPackageNameIndex || index == colStateIndex
                    || index == colCPUTimeIndex || index == colPathIndex || index == colCommandLineIndex
                    || index == colPlatformIndex || index == colOSContextIndex || index == colDescriptionIndex
                    || index == colDepIndex || index == colUACVIndex || index == colUserNameIndex)
                    return true;
            }
            return false;
        }
        public void ProcessListDetailsGetColumnsIndex()
        {
            //加载所有列表头的序号
            colPPIDIndex = ProcessListDetailsGetListIndex("TitleParentPID");
            colPIDIndex = ProcessListDetailsGetListIndex("TitlePID");
            colPackageNameIndex = ProcessListDetailsGetListIndex("TitlePackageName");
            colStateIndex = ProcessListDetailsGetListIndex("TitleStatus");
            colSessionIDIndex = ProcessListDetailsGetListIndex("TitleSessionID");
            colJobIDIndex = ProcessListDetailsGetListIndex("TitleJobID");
            colCPUIndex = ProcessListDetailsGetListIndex("TitleCPU");
            colCPUTimeIndex = ProcessListDetailsGetListIndex("TitleCPUTime");
            colCycleIndex = ProcessListDetailsGetListIndex("TitleCycle");
            colPeakWorkingSetIndex = ProcessListDetailsGetListIndex("TitlePeakWorkingSet");
            colWorkingSetIncreasementIndex = ProcessListDetailsGetListIndex("TitleWorkingSetCrease");
            colWorkingSetIndex = ProcessListDetailsGetListIndex("TitleWorkingSet");
            colWorkingSetPrivateIndex = ProcessListDetailsGetListIndex("TitleWorkingSetPrivate");
            colWorkingSetShareIndex = ProcessListDetailsGetListIndex("TitleWorkingSetShare");
            colCommitedSizeIndex = ProcessListDetailsGetListIndex("TitleCommited");
            colPagedPoolIndex = ProcessListDetailsGetListIndex("TitlePagedPool");
            colNonPagedPoolIndex = ProcessListDetailsGetListIndex("TitleNonPagedPool");
            colPageErrorIndex = ProcessListDetailsGetListIndex("TitlePagedError");
            colPageErrorIncreasementIndex = ProcessListDetailsGetListIndex("TitlePagedErrorCrease");
            colHandleCountIndex = ProcessListDetailsGetListIndex("TitleHandleCount");
            colThreadCountIndex = ProcessListDetailsGetListIndex("TitleThreadCount");
            colUserObjectIndex = ProcessListDetailsGetListIndex("TitleUserObject");
            colGDIObjectIndex = ProcessListDetailsGetListIndex("TitleGdiObject");
            colIOReadIndex = ProcessListDetailsGetListIndex("TitleIORead");
            colIOWriteIndex = ProcessListDetailsGetListIndex("TitleIOWrite");
            colIOOtherIndex = ProcessListDetailsGetListIndex("TitleIOOther");
            colIOReadBytesIndex = ProcessListDetailsGetListIndex("TitleIOReadBytes");
            colIOWriteBytesIndex = ProcessListDetailsGetListIndex("TitleIOWriteBytes");
            colIOOtherBytesIndex = ProcessListDetailsGetListIndex("TitleIOOtherBytes");
            colPathIndex = ProcessListDetailsGetListIndex("TitleProcPath");
            colCommandLineIndex = ProcessListDetailsGetListIndex("TitleCmdLine");
            colPlatformIndex = ProcessListDetailsGetListIndex("TitlePlatform");
            colOSContextIndex = ProcessListDetailsGetListIndex("TitleOperationSystemContext");
            colDescriptionIndex = ProcessListDetailsGetListIndex("TitleDescription");
            colDepIndex = ProcessListDetailsGetListIndex("TitleDEP");
            colUACVIndex = ProcessListDetailsGetListIndex("TitleUACVirtualization");
            colNameIndex = ProcessListDetailsGetListIndex("TitleProcName");
            colUserNameIndex = ProcessListDetailsGetListIndex("TitleUserName");
            colEprocessIndex = ProcessListDetailsGetListIndex("TitleEProcess");

        }
        private void ProcessListDetailsSaveColumns()
        {
            if (listProcessDetals.Columns.Count > 0)
            {
                string finalString = "";
                ColumnHeader currentColumn = null;
                for (int i = listProcessDetals.Columns.Count - 1; i >= 0; i--)
                {
                    currentColumn = null;
                    foreach (ColumnHeader li in listProcessDetals.Columns)
                        if (li.DisplayIndex == i)
                        {
                            currentColumn = li;
                            break;
                        }
                    if (currentColumn != null)
                        finalString = currentColumn.Name + "-" + currentColumn.Width + "#" + finalString;
                }
                SetConfig("DetalHeaders", "AppSetting", finalString);
                SetConfig("DetalSort", "AppSetting", listViewItemComparerProcDetals.SortColumn + "#" + (listViewItemComparerProcDetals.Asdening ? "Asdening" : "Descending"));
            }
        }
        private void ProcessListDetailsLoadColumns()
        {
            hListHeader = ComCtlApi.MListViewGetHeaderControl(listProcessDetals.Handle);
            //加载列表头
            if (listProcessDetals.Columns.Count > 0) listProcessDetals.Columns.Clear();
            string headersStr = GetConfig("DetalHeaders", "AppSetting");
            if (headersStr == "") headersStr = "TitleProcName-190#TitlePID-55#TitleStatus-55#TitleUserName-70#TitleCPU-60#TitleWorkingSetPrivate-70#TitleDescription-400#";
            string[] headers = headersStr.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < headers.Length && i < 16; i++)
                ProcessListDetailsAddColumns(headers[i]);

            ProcessListDetailsGetColumnsIndex();
            if (colNameIndex == -1)
            {
                ProcessListDetailsAddColumns("TitleProcName-130");
                colNameIndex = ProcessListDetailsGetListIndex("TitleProcName");
            }

            string sortInfo = GetConfig("DetalSort", "AppSetting");
            if (sortInfo.Contains("#"))
            {
                string[] sortInfo2 = sortInfo.Split('#');
                if (sortInfo.Length >= 2)
                {
                    int col = 0;
                    int.TryParse(sortInfo2[0], out col);
                    if (col >= 0 && col < listProcessDetals.Columns.Count)
                        listViewItemComparerProcDetals.SortColumn = col;
                    if (sortInfo2[1] == "Asdening") listViewItemComparerProcDetals.Asdening = true;
                    else if (sortInfo2[1] == "Descending") listViewItemComparerProcDetals.Asdening = false;
                    ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn, listViewItemComparerProcDetals.Asdening, false);
                }
            }
        }
        private void ProcessListDetailsAddColumns(string s)
        {
            string sname = s; int width = 70;
            if (s.Contains("-"))
            {
                string[] ss = s.Split('-');
                sname = ss[0];
                if (ss.Length >= 2)
                    int.TryParse(ss[1], out width);
            }
            if (width > 1024 || width <= 0) width = 70;
            if (s.Trim() != "") ProcessListDetailsAddHeader(sname, width);
        }

        private void 隐藏列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colLastDown != -1)
            {
                listProcessDetals.Columns.Remove(listProcessDetals.Columns[colLastDown]);
                colLastDown = -1;
                ProcessListDetailsGetColumnsIndex();
            }
        }
        private void 选择列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDetalsistHeaders().ShowDialog();
        }
        private void 将此列调整为合适大小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colLastDown != -1)
                listProcessDetals.AutoResizeColumn(colLastDown, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private int listProcessDetals_lastEnterColumn = -1;
        private void listProcessDetals_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            colLastDown = e.Column;
            if (listViewItemComparerProcDetals.SortColumn != e.Column)
            {
                ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn, false, true);
                listViewItemComparerProcDetals.SortColumn = e.Column;
                listViewItemComparerProcDetals.Asdening = true;
                ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn,
                    listViewItemComparerProcDetals.Asdening, false);
            }
            else
            {
                listViewItemComparerProcDetals.Asdening = !listViewItemComparerProcDetals.Asdening;
                ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn,
                      listViewItemComparerProcDetals.Asdening, false);
            }
            listProcessDetals.Sort();
        }
        private void listProcessDetals_ColumnMouseMove(object sender, int index, Point p)
        {
            if (index > 0 && index < listProcessDetals.Columns.Count)
            {
                if (index != listProcessDetals_lastEnterColumn)
                {
                    listProcessDetals_lastEnterColumn = index;
                    ColumnHeader col = listProcessDetals.Columns[index];
                    if (col.Tag != null)
                    {
                        colsTip.Show(((itemheaderTag)col.Tag).tip, listProcessDetals, p.X, p.Y + 3, 5000);
                    }
                    else
                    {
                        listProcessDetals_lastEnterColumn = -1;
                        colsTip.Hide(listProcessDetals);
                    }
                }
            }
            else
            {
                listProcessDetals_lastEnterColumn = -1;
                colsTip.Hide(listProcessDetals);
            }
        }
        private void listProcessDetals_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            nextUpdateStaticVals = true;
        }

        private ListViewItemComparerProcDetals listViewItemComparerProcDetals = null;
        private class ListViewItemComparerProcDetals : IComparer
        {
            public ListViewItemComparerProcDetals()
            {
                formMain = FormMain.Instance;
            }

            private FormMain formMain;
            private int col;
            private bool asdening = false;

            public int SortColumn { get { return col; } set { col = value; } }
            public bool Asdening { get { return asdening; } set { asdening = value; } }

            public int Compare(object o1, object o2)
            {
                ListViewItem x = o1 as ListViewItem, y = o2 as ListViewItem;
                int returnVal = -1;
                if (x.SubItems[col].Text == y.SubItems[col].Text) return -1;
                if (formMain.ProcessListDetailsIsStringColumn(col))
                {
                    returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
                }
                else
                {
                    UInt64 xi, yi;
                    if (UInt64.TryParse(x.SubItems[col].Text, out xi) && UInt64.TryParse(y.SubItems[col].Text, out yi))
                    {
                        if (x.SubItems[col].Text == y.SubItems[col].Text) returnVal = 0;
                        else if (xi > yi) returnVal = 1;
                        else if (xi < yi) returnVal = -1;
                    }
                }
                if (asdening) returnVal = -returnVal;
                return returnVal;
            }
        }


        #endregion

        #endregion

        #region FileMgrWork

        private Dictionary<string, string> fileTypeNames = new Dictionary<string, string>();
        private TreeNode lastClickTreeNode = null;
        private string lastShowDir = "";
        private bool lastRightClicked = false;

        private void FileMgrInit()
        {
            if (!fileListInited)
            {
                fileListInited = true;

                DelingDialogInit();

                fileMgrCallBack = FileMgrCallBack;
                MFM_SetCallBack(Marshal.GetFunctionPointerForDelegate(fileMgrCallBack));

                imageListFileMgrLeft.Images.Add("folder", Icon.FromHandle(MFM_GetFolderIcon()));
                imageListFileMgrLeft.Images.Add("mycp", Icon.FromHandle(MFM_GetMyComputerIcon()));

                imageListFileTypeList.Images.Add("folder", Icon.FromHandle(MFM_GetFolderIcon()));

                MAppWorkCall3(182, treeFmLeft.Handle, IntPtr.Zero);
                MAppWorkCall3(182, listFm.Handle, IntPtr.Zero);

                string smycp = Marshal.PtrToStringAuto(MFM_GetMyComputerName());
                treeFmLeft.Nodes.Add("mycp", smycp, "mycp", "mycp").Tag = "mycp";
                MFM_GetRoots();
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
                        n.Nodes.Add("loading", str_loading, "loading", "loading");
                        break;
                    }
                case 3:
                    {
                        if (wParam.ToInt32() == -1)
                        {
                            lastClickTreeNode.Nodes[0].Text = str_VisitFolderFailed;
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
                            n.Nodes.Add("loading", str_loading, "loading", "loading");
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
                            ListViewItem lvi = listFm.Items.Add(str_VisitFolderFailed, "err");
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
                        case 0: lbFileMgrStatus.Text = str_Ready; break;
                        case 1:
                            {
                                if (listFm.SelectedItems.Count > 0)
                                    lbFileMgrStatus.Text = str_ReadyStatus + listFm.Items.Count + str_ReadyStatusEnd2 + listFm.SelectedItems.Count + str_ReadyStatusEnd;
                                else lbFileMgrStatus.Text = str_ReadyStatus + listFm.Items.Count + str_ReadyStatusEnd;
                                break;
                            }
                        case 2: lbFileMgrStatus.Text = ""; break;
                        case 3: lbFileMgrStatus.Text = ""; break;
                        case 4: lbFileMgrStatus.Text = ""; break;
                        case 5: lbFileMgrStatus.Text = str_FileCuted; break;
                        case 6: lbFileMgrStatus.Text = str_FileCopyed; break;
                        case 7: lbFileMgrStatus.Text = str_NewFolderFailed; break;
                        case 8: lbFileMgrStatus.Text = str_NewFolderSuccess; break;
                        case 9: lbFileMgrStatus.Text = str_PathCopyed; break;
                        case 10: lbFileMgrStatus.Text = str_FolderCuted; break;
                        case 11: lbFileMgrStatus.Text = str_FolderCopyed; break;
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
                    return showHiddenFiles ? new IntPtr(1) : new IntPtr(0);
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
        private void FileMgrShowFiles(string path)
        {
            if (path == null)
            {
                path = lastShowDir;
                lastShowDir = null;
            }
            if (lastShowDir != path)
            {
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
                if (path == "mycp") fileSystemWatcher.Path = "";
                else fileSystemWatcher.Path = path;
                fileSystemWatcher.EnableRaisingEvents = true;


                FileMgrUpdateStatus(1);
            }
        }
        private void FileMgrUpdateStatus(int i)
        {
            FileMgrCallBack(15, new IntPtr(i), IntPtr.Zero);
        }
        private void FileMgrTreeOpenItem(TreeNode n)
        {
            if (n.Nodes.Count == 0 || n.Nodes[0].Text == str_loading && n.Tag != null)
            {
                lastClickTreeNode = n;
                string s = n.Tag.ToString();
                if (MFM_GetFolders(s))
                    lastClickTreeNode.Nodes.Remove(lastClickTreeNode.Nodes[0]);
            }
        }

        private void textBoxFmCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnFmAddGoto_Click(sender, e);
        }
        private void btnFmAddGoto_Click(object sender, EventArgs e)
        {

            if (textBoxFmCurrent.Text == "")
                TaskDialog.Show(str_PleaseEnterPath, str_TipTitle);
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
                else TaskDialog.Show(str_PathUnExists, str_TipTitle);
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
                            TaskDialog.Show(str_FolderHasExist);
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
                        TaskDialog.Show(str_InvalidFileName);
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
                                if (TaskDialog.Show(str_OpenAsk, str_AskTitle, str_PathStart + path, TaskDialogButton.Yes | TaskDialogButton.No) == Result.Yes)
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

        #endregion

        #region ScMgrWork

        private class ListViewItemComparer : IComparer
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

        private ListViewItemComparer listViewItemComparerSc = new ListViewItemComparer();
        private List<uint> scValidPid = new List<uint>();
        private List<ScItem> runningSc = new List<ScItem>();
        private Icon icoSc = null;
        private Dictionary<string, string> scGroupFriendlyName = new Dictionary<string, string>();
        private bool scCanUse = false;
        private IntPtr hListHeaderSc = IntPtr.Zero;

        private class ScTag
        {
            public uint startType = 0;
            public uint runningState = 0;
            public string name = "";
            public string binaryPathName = "";
        }
        private class ScItem
        {
            public ScItem(int pid, string groupName, string scName, string scDsb)
            {
                this.scDsb = scDsb;
                this.scName = scName;
                this.groupName = groupName;
                this.pid = pid;
            }
            public string groupName = "";
            public string scName = "";
            public string scDsb = "";
            public int pid;
        }

        private string ScGroupNameToFriendlyName(string s)
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
        private bool ScMgrFindRunSc(PsItem p)
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
        private string ScMgrFindDriverSc(string driverOrgPath)
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
        private void ScMgrInit()
        {
            if (!scListInited)
            {
                if (!MIsRunasAdmin())
                {
                    listService.Hide();
                    pl_ScNeedAdminTip.Show();
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
                        TaskDialog.Show(LanuageMgr.GetStr("StartSCMFailed"), str_ErrTitle, "", TaskDialogButton.OK, TaskDialogIcon.Stop);

                    scMgrEnumServicesCallBack = ScMgrIEnumServicesCallBack;
                    scMgrEnumServicesCallBackPtr = Marshal.GetFunctionPointerForDelegate(scMgrEnumServicesCallBack);

                    scCanUse = true;
                    ScMgrRefeshList();

                }

                icoSc = new Icon(Properties.Resources.icoService, 16, 16);

                listService.ListViewItemSorter = listViewItemComparerSc;
                hListHeaderSc = ComCtlApi.MListViewGetHeaderControl(listService.Handle, false);

                scListInited = true;
            }
        }
        private void ScMgrRefeshList()
        {
            if (scCanUse)
            {
                scValidPid.Clear();
                runningSc.Clear();
                listService.Items.Clear();
                MEnumServices(scMgrEnumServicesCallBackPtr);
                lbServicesCount.Text = LanuageMgr.GetStr("ServiceCount") + " : " + (listService.Items.Count == 0 ? "--" : listService.Items.Count.ToString());
            }
        }
        private void ScMgrIEnumServicesCallBack(IntPtr dspName, IntPtr scName, uint scType, uint currentState, uint dwProcessId, bool syssc,
            uint dwStartType, IntPtr lpBinaryPathName, IntPtr lpLoadOrderGroup)
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
                case 0x0003: li.SubItems.Add(str_status_stopped); break;
                case 0x0002:
                case 0x0004: li.SubItems.Add(str_status_running); break;
                case 0x0006:
                case 0x0007: li.SubItems.Add(str_status_paused); break;
                default: li.SubItems.Add(""); break;
            }
            li.SubItems.Add(Marshal.PtrToStringUni(lpLoadOrderGroup));
            switch (dwStartType)
            {
                case 0x0000: li.SubItems.Add(str_DriverLoad); break;
                case 0x0001: li.SubItems.Add(str_DriverLoad); break;
                case 0x0002: li.SubItems.Add(str_AutoStart); break;
                case 0x0003: li.SubItems.Add(str_DemandStart); break;
                case 0x0004: li.SubItems.Add(str_Disabled); break;
                case 0x0080: li.SubItems.Add(""); break;
                default: li.SubItems.Add(""); break;
            }
            switch (scType)
            {
                case 0x0002: li.SubItems.Add(str_FileSystem); break;
                case 0x0001: li.SubItems.Add(str_KernelDriver); break;
                case 0x0010: li.SubItems.Add(str_UserService); break;
                case 0x0020: li.SubItems.Add(str_SystemService); break;
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
                    if (highlight_nosystem && exeCompany.ToString() != MICROSOFT)
                        hightlight = true;
                }
                else if (highlight_nosystem) hightlight = true;
            }
            else
            {
                li.SubItems.Add(path);
                li.SubItems.Add(str_FileNotExist);
                if (highlight_nosystem) hightlight = true;
            }
            if (hightlight)
            {
                li.ForeColor = Color.Blue;
                foreach (ListViewItem.ListViewSubItem s in li.SubItems)
                    s.ForeColor = Color.Blue;
            }
            listService.Items.Add(li);
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

        #endregion

        #region UWPMWork

        private class TaskListViewUWPColumnSorter : ListViewColumnSorter
        {
            public TaskListViewUWPColumnSorter()
            {
            }
            public override int Compare(TaskMgrListItem x, TaskMgrListItem y)
            {
                int compareResult = 0;
                compareResult = string.Compare(x.SubItems[SortColumn].Text, y.SubItems[SortColumn].Text);
                if (compareResult == 0)
                    compareResult = ObjectCompare.Compare(x.PID, y.PID);
                if (Order == SortOrder.Ascending)
                    return compareResult;
                else if (Order == SortOrder.Descending)
                    return (-compareResult);
                return compareResult;
            }
        }
        private object uWPManager = null;

        private TaskListViewUWPColumnSorter uWPColumnSorter = new TaskListViewUWPColumnSorter();
        private void UWPListRefesh()
        {
            if (uwpListInited)
            {
                listUwpApps.Show();
                pl_UWPEnumFailTip.Hide();
                listUwpApps.Items.Clear();
                if (uWPManager != null) ((UWPManager)uWPManager).Clear();
                try
                {
                    ((UWPManager)uWPManager).EnumlateAll();
                    for (int i = 0; i < ((UWPManager)uWPManager).Packages.Count; i++)
                    {
                        UWPPackage pkg = ((UWPManager)uWPManager).Packages[i];

                        TaskMgrListItem li = new TaskMgrListItem(LanuageMgr.IsChinese ? UWPManager.DisplayNameTozhCN(pkg.Name) : pkg.Name);
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems[0].Font = listUwpApps.Font;
                        li.SubItems[1].Font = listUwpApps.Font;
                        li.SubItems[2].Font = listUwpApps.Font;
                        li.SubItems[3].Font = listUwpApps.Font;
                        li.SubItems[0].Text = LanuageMgr.IsChinese ? UWPManager.DisplayNameTozhCN(pkg.Name) : pkg.Name;
                        li.SubItems[1].Text = pkg.Publisher;
                        li.SubItems[2].Text = pkg.FullName;
                        li.SubItems[3].Text = pkg.InstalledLocation;
                        li.SubItems[4].Text = pkg.MainAppDisplayName;
                        li.Tag = pkg;
                        li.IsUWPICO = true;

                        string iconpath = pkg.IconPath;
                        if (iconpath != "" && MFM_FileExist(iconpath))
                        {
                            using (Image img = Image.FromFile(iconpath))
                                li.Icon = IconUtils.ConvertToIcon(img);
                            //
                            //     li.Image = IconUtils.GetThumbnail(new Bitmap(iconpath), 16, 16);
                        }
                        listUwpApps.Items.Add(li);
                    }
                }
                catch (Exception e)
                {
                    listUwpApps.Hide();
                    pl_UWPEnumFailTip.Show();
                    lbUWPEnumFailText.Text = LanuageMgr.GetStr("UWPEnumFail") + "\n\n" + e.ToString();
                }
            }
        }
        private void UWPListInit()
        {
            if (!uwpListInited)
            {
                uWPManager = new UWPManager();
                uwpListInited = true;
                UWPListRefesh();

                listUwpApps.ListViewItemSorter = uWPColumnSorter;
                listUwpApps.Header.CloumClick += UWPList_Header_CloumClick;
            }
        }
        private void UWPListUnInit()
        {
            listUwpApps.Items.Clear();
            ((UWPManager)uWPManager).Clear();
            uWPManager = null;
        }

        private void UWPList_Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left)
            {
                listUwpApps.Locked = true;
                if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.None)
                    uWPColumnSorter.Order = SortOrder.Ascending;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                    uWPColumnSorter.Order = SortOrder.Ascending;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    uWPColumnSorter.Order = SortOrder.Descending;

                uWPColumnSorter.SortColumn = e.Index;
                listUwpApps.Locked = false;
                listUwpApps.Sort();
            }
        }

        private TaskMgrListItem UWPListFindItem(string fullName)
        {
            TaskMgrListItem rs = null;
            foreach (TaskMgrListItem r in listUwpApps.Items)
                if (r.Tag.ToString() == fullName)
                {
                    rs = r;
                    break;
                }
            return rs;
        }

        private void 打开应用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWPPackage pkg = ((UWPPackage)listUwpApps.SelectedItem.Tag);
                if (pkg.Apps.Length > 0)
                {
                    int first_ = pkg.FullName.IndexOf('_');
                    int last_ = pkg.FullName.LastIndexOf('_');
                    string indxxstr = pkg.FullName;
                    if (first_ < last_ && last_ > 0 && last_ < indxxstr.Length)
                        indxxstr = indxxstr.Replace(indxxstr.Substring(first_, last_ - first_), "");
                    uint processid = 0;
                    M_UWP_RunUWPApp(indxxstr + "!" + pkg.Apps[0], ref processid);
                }
            }
        }
        private void 卸载应用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MRunExe("ms-settings:appsfeatures", null);
        }
        private void 打开安装位置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWPPackage pkg = ((UWPPackage)listUwpApps.SelectedItem.Tag);
                MFM_OpenFile(pkg.InstalledLocation, Handle);
            }
        }
        private void 复制名称ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWPPackage pkg = ((UWPPackage)listUwpApps.SelectedItem.Tag);
                MCopyToClipboard2(pkg.Name);
            }
        }
        private void 复制完整名称ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWPPackage pkg = ((UWPPackage)listUwpApps.SelectedItem.Tag);
                MCopyToClipboard2(pkg.FullName);
            }
        }
        private void 复制发布者ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                string s = "";
                for (int i = 0; i < listUwpApps.Colunms.Count; i++)
                {
                    s += " " + listUwpApps.Colunms[i].TextSmall + " : ";
                    s += listUwpApps.SelectedItem.SubItems[i].Text;
                }
                NativeMethods.MCopyToClipboard2(s);
            }
        }

        private void listUwpApps_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listUwpApps.SelectedItem != null)
                {
                    Point p = listUwpApps.GetiItemPoint(listUwpApps.SelectedItem);
                    contextMenuStripUWP.Show(listUwpApps.PointToScreen(p));
                }
            }
        }
        private void listUwpApps_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listUwpApps.SelectedItem != null)
                contextMenuStripUWP.Show(MousePosition);
        }

        #endregion

        #region PerfWork

        public static Color CpuDrawColor = Color.FromArgb(17, 125, 187);
        public static Color CpuBgColor = Color.FromArgb(241, 246, 250);
        public static Color RamDrawColor = Color.FromArgb(139, 18, 174);
        public static Color RamBgColor = Color.FromArgb(244, 242, 244);
        public static Color DiskDrawColor = Color.FromArgb(77, 166, 12);
        public static Color DiskBgColor = Color.FromArgb(239, 247, 223);
        public static Color NetDrawColor = Color.FromArgb(167, 79, 1);
        public static Color NetBgColor = Color.FromArgb(252, 243, 235);

        public static Color CpuDrawColor2 = Color.FromArgb(100, 17, 125, 187);
        public static Color RamDrawColor2 = Color.FromArgb(100, 139, 18, 174);
        public static Color DiskDrawColor2 = Color.FromArgb(100, 77, 166, 12);
        public static Color NetDrawColor2 = Color.FromArgb(100, 167, 79, 1);

        public static Color CpuBgColor2 = Color.FromArgb(100, 85, 193, 255);
        public static Color RamBgColor2 = Color.FromArgb(180, 220, 98, 244);
        public static Color DiskBgColor2 = Color.FromArgb(100, 239, 247, 223);
        public static Color NetBgColor2 = Color.FromArgb(100, 255, 157, 89);

        PerfItemHeader perfItemHeaderCpu;
        PerfItemHeader perfItemHeaderRam;

        PerformanceListItem perf_cpu = new PerformanceListItem();
        PerformanceListItem perf_ram = new PerformanceListItem();

        private class PerfItemHeader
        {
            public IntPtr performanceCounterNative = IntPtr.Zero;
            public PerformanceListItem item = null;
            public IPerformancePage performancePage = null;

            public bool Inited { get; set; }

            public override string ToString()
            {
                if (item != null)
                    return item.ToString();
                return base.ToString();
            }
        }

        private List<PerfItemHeader> perfItems = new List<PerfItemHeader>();
        private List<IPerformancePage> perfPages = new List<IPerformancePage>();

        private IPerformancePage currSelectedPerformancePage = null;

        private FormSpeedBall.SpeedItem itemCpu;
        private FormSpeedBall.SpeedItem itemRam;
        private FormSpeedBall.SpeedItem itemDisk;
        private FormSpeedBall.SpeedItem itemNet;
        private IntPtr netCounterMain = IntPtr.Zero;

        private Size lastGraphicSize = new Size();
        private Control lastGridParent = null;
        private Size lastGridSize = new Size();

        private void splitContainerPerfCtls_Panel2_SizeChanged(object sender, EventArgs e)
        {
            PerfPagesResize(new Size(splitContainerPerfCtls.Panel2.Width -
             (splitContainerPerfCtls.Panel2.VerticalScroll.Visible ? 40 : 30), splitContainerPerfCtls.Panel2.Height - 30));
        }
        private void performanceLeftList_SelectedtndexChanged(object sender, EventArgs e)
        {
            if (performanceLeftList.Selectedtem == perf_cpu)
                PerfPagesTo(0, perfItemHeaderCpu);
            else if (performanceLeftList.Selectedtem == perf_ram)
                PerfPagesTo(1, perfItemHeaderRam);
            else if (performanceLeftList.Selectedtem.PageIndex != 0)
                PerfPagesTo(performanceLeftList.Selectedtem.PageIndex, (PerfItemHeader)performanceLeftList.Selectedtem.Tag);
            else PerfPagesToNull();
        }
        private void OpeningPageMenuEventHandler(IPerformancePage sender, ToolStripMenuItem menuItemView)
        {
            if (menuItemView.DropDownItems.Count == 0)
            {
                /*ToolStripItem tcpu = menuItemView.DropDownItems.Add("CPU");
                tcpu.Tag = perfItemHeaderCpu.performancePage;
                tcpu.Click += Tcpu_Click;
                ToolStripItem tram = menuItemView.DropDownItems.Add(perfItemHeaderRam.item.Name);
                tram.Tag = perfItemHeaderRam.performancePage;
                tram.Click += Tram_Click;*/
                foreach (PerfItemHeader h in perfItems)
                {
                    ToolStripItem tx = menuItemView.DropDownItems.Add(h.item.Name);
                    tx.Tag = h;
                    tx.Click += Tx_Click;
                }
            }

            foreach (ToolStripMenuItem i in menuItemView.DropDownItems)
            {
                if (i.Tag != null && ((PerfItemHeader)i.Tag).performancePage == sender)
                    i.Checked = true;
                else if (i.Checked) i.Checked = false;
            }
        }

        private void Tx_Click(object sender, EventArgs e)
        {
            PerfItemHeader tag = null;
            ToolStripItem item = sender as ToolStripItem;
            if (item.Tag != null)
            {
                tag = (PerfItemHeader)item.Tag;
                PerfPagesTo(tag.performancePage, tag);
            }
        }
        private void Tcpu_Click(object sender, EventArgs e)
        {
            PerfPagesTo(0, perfItemHeaderCpu);
        }
        private void Tram_Click(object sender, EventArgs e)
        {
            PerfPagesTo(1, perfItemHeaderRam);
        }

        private void SwithGraphicViewEventHandler(IPerformancePage sender)
        {
            Panel gridPanel = sender.GridPanel;
            if (!sender.PageIsGraphicMode)
            {
                spBottom.Visible = false;
                tabControlMain.Visible = false;
                lastSize = Size;

                lastGridSize = gridPanel.Size;

                lastGridParent = gridPanel.Parent;
                lastGridParent.Controls.Remove(gridPanel);
                pl_perfGridHost.Controls.Add(gridPanel);

                gridPanel.Size = new Size(pl_perfGridHost.Width - 30, pl_perfGridHost.Height - 30);
                gridPanel.Location = new Point(15, 15);

                sender.PageIsGraphicMode = true;

                pl_perfGridHost.BringToFront();

                MAppWorkCall3(167, Handle, IntPtr.Zero);

                Size = lastGraphicSize;
            }
            else
            {
                pl_perfGridHost.SendToBack();

                pl_perfGridHost.Controls.Remove(gridPanel);
                lastGridParent.Controls.Add(gridPanel);

                MAppWorkCall3(173, Handle, IntPtr.Zero);

                gridPanel.Size = lastGridSize;
                gridPanel.Location = Point.Empty;

                sender.PageIsGraphicMode = false;

                spBottom.Visible = true;
                tabControlMain.Visible = true;
                lastGraphicSize = Size;
                Size = lastSize;
            }
        }

        private void PerfPagesToNull()
        {
            if (currSelectedPerformancePage != null)
            {
                currSelectedPerformancePage.PageHide();
                currSelectedPerformancePage.PageIsActive = false;
            }
            currSelectedPerformancePage = null;
        }
        private void PerfPagesTo(int index, PerfItemHeader header)
        {
            PerfPagesTo(perfPages[index], header);
        }
        private void PerfPagesTo(IPerformancePage page, PerfItemHeader header)
        {
            bool isGrMode = false;

            if (currSelectedPerformancePage != null)
            {
                if (currSelectedPerformancePage.PageIsGraphicMode)
                {
                    isGrMode = true;

                    pl_perfGridHost.SendToBack();

                    pl_perfGridHost.Controls.Remove(currSelectedPerformancePage.GridPanel);
                    lastGridParent.Controls.Add(currSelectedPerformancePage.GridPanel);

                    currSelectedPerformancePage.GridPanel.Size = lastGridSize;
                    currSelectedPerformancePage.GridPanel.Location = Point.Empty;

                    currSelectedPerformancePage.PageIsGraphicMode = false;
                }

                currSelectedPerformancePage.PageHide();
                currSelectedPerformancePage.PageIsActive = false;
            }

            currSelectedPerformancePage = null;
            currSelectedPerformancePage = page;

            if (!header.Inited)
            {
                currSelectedPerformancePage.PageInit();
                header.Inited = true;
            }

            currSelectedPerformancePage.PageShow();
            currSelectedPerformancePage.PageIsActive = true;

            performanceLeftList.Selectedtem = performanceLeftList.Items[perfPages.IndexOf(currSelectedPerformancePage)];

            if (isGrMode)
            {
                lastGridSize = currSelectedPerformancePage.GridPanel.Size;

                lastGridParent = currSelectedPerformancePage.GridPanel.Parent;
                lastGridParent.Controls.Remove(currSelectedPerformancePage.GridPanel);
                pl_perfGridHost.Controls.Add(currSelectedPerformancePage.GridPanel);

                currSelectedPerformancePage.GridPanel.Size = new Size(pl_perfGridHost.Width - 30, pl_perfGridHost.Height - 30);
                currSelectedPerformancePage.GridPanel.Location = new Point(15, 15);

                currSelectedPerformancePage.PageIsGraphicMode = true;

                pl_perfGridHost.BringToFront();
            }
        }
        private void PerfPagesAddToCtl(Control c, string name)
        {
            splitContainerPerfCtls.Panel2.Controls.Add(c);

            c.Visible = false;
            c.Anchor = AnchorStyles.Left | AnchorStyles.Top;//| AnchorStyles.Right | AnchorStyles.Bottom;
            //c.Size = new Size(splitContainerPerfCtls.Panel2.Width - 30, splitContainerPerfCtls.Panel2.Height - 30);
            c.Location = new Point(15, 15);
            c.Text = "资源信息页 " + name;
            c.Font = tabControlMain.Font;
        }
        private void PerfPagesResize(Size targetSize)
        {
            foreach (PerfItemHeader h in perfItems)
                if (h.performancePage != null)
                    if (!h.performancePage.PageIsGraphicMode)
                        h.performancePage.Size = targetSize;
        }
        private void PerfPagesInit()
        {
            PerformancePageCpu performanceCpu = new PerformancePageCpu();
            performanceCpu.OpeningPageMenu += OpeningPageMenuEventHandler;
            performanceCpu.SwithGraphicView += SwithGraphicViewEventHandler;
            PerfPagesAddToCtl(performanceCpu, perf_cpu.Name);
            perfPages.Add(performanceCpu);

            perfItemHeaderCpu = new PerfItemHeader();
            perfItemHeaderCpu.item = perf_cpu;
            perfItemHeaderCpu.performancePage = performanceCpu;
            perfItems.Add(perfItemHeaderCpu);

            PerformancePageRam performanceRam = new PerformancePageRam();
            performanceRam.OpeningPageMenu += OpeningPageMenuEventHandler;
            performanceRam.SwithGraphicView += SwithGraphicViewEventHandler;
            PerfPagesAddToCtl(performanceRam, perf_ram.Name);
            perfPages.Add(performanceRam);

            perfItemHeaderRam = new PerfItemHeader();
            perfItemHeaderRam.item = perf_ram;
            perfItemHeaderRam.performancePage = performanceRam;
            perfItems.Add(perfItemHeaderRam);
        }
        private void PerfInit()
        {
            if (!perfInited)
            {
                MDEVICE_Init();

                perf_cpu.Name = "CPU";
                perf_cpu.SmallText = "-- %";
                perf_cpu.BasePen = new Pen(CpuDrawColor, 2);
                perf_cpu.BgBrush = new SolidBrush(CpuBgColor);
                performanceLeftList.Items.Add(perf_cpu);

                perf_ram.Name = LanuageMgr.GetStr("TitleRam");
                perf_ram.SmallText = "-- %";
                perf_ram.BasePen = new Pen(RamDrawColor, 2);
                perf_ram.BgBrush = new SolidBrush(RamBgColor);
                performanceLeftList.Items.Add(perf_ram);


                PerfPagesInit();

                MDEVICE_GetLogicalDiskInfo();
                uint count = MPERF_InitDisksPerformanceCounters();
                for (int i = 0; i < count; i++)
                {
                    PerfItemHeader perfItemHeader = new PerfItemHeader();
                    perfItemHeader.performanceCounterNative = MPERF_GetDisksPerformanceCounters(i);
                    perfItemHeader.item = new PerformanceListItem();

                    StringBuilder sb = new StringBuilder(32);
                    MPERF_GetDisksPerformanceCountersInstanceName(perfItemHeader.performanceCounterNative, sb, 32);
                    uint diskIndex = (uint)(count - i - 1);// MDEVICE_GetPhysicalDriveFromPartitionLetter(sb.ToString()[2]);

                    perfItemHeader.item.Name = LanuageMgr.GetStr("TitleDisk") + sb.ToString();
                    perfItemHeader.item.BasePen = new Pen(DiskDrawColor);
                    perfItemHeader.item.BgBrush = new SolidBrush(DiskBgColor);
                    perfItemHeader.item.Tag = perfItemHeader;
                    perfItems.Add(perfItemHeader);

                    PerformancePageDisk performancedisk = new PerformancePageDisk(perfItemHeader.performanceCounterNative, diskIndex);
                    performancedisk.OpeningPageMenu += OpeningPageMenuEventHandler;
                    performancedisk.SwithGraphicView += SwithGraphicViewEventHandler;
                    PerfPagesAddToCtl(performancedisk, perfItemHeader.item.Name);
                    perfPages.Add(performancedisk);

                    perfItemHeader.performancePage = performancedisk;

                    perfItemHeader.item.PageIndex = perfPages.Count - 1;
                    performanceLeftList.Items.Add(perfItemHeader.item);
                }

                count = MDEVICE_GetNetworkAdaptersInfo();
                for (int i = 0; i < count; i++)
                {
                    StringBuilder sbName = new StringBuilder(128);
                    if (MDEVICE_GetNetworkAdapterInfoItem(i, sbName, 128))
                    {
                        PerfItemHeader perfItemHeader = new PerfItemHeader();
                        perfItemHeader.performanceCounterNative = MPERF_GetNetworksPerformanceCounterWithName(sbName.ToString());
                        perfItemHeader.item = new PerformanceListItem();

                        bool isWifi = MDEVICE_GetNetworkAdapterIsWIFI(sbName.ToString());

                        perfItemHeader.item.Name = isWifi ? "Wi-Fi" : LanuageMgr.GetStr("Ethernet");
                        perfItemHeader.item.BasePen = new Pen(NetDrawColor);
                        perfItemHeader.item.BgBrush = new SolidBrush(NetBgColor);
                        perfItemHeader.item.Tag = perfItemHeader;
                        perfItems.Add(perfItemHeader);

                        StringBuilder sbIPV4 = new StringBuilder(32);
                        StringBuilder sbIPV6 = new StringBuilder(64);
                        bool enabled = MDEVICE_GetNetworkAdapterInfoFormName(sbName.ToString(),
                            sbIPV4, 32, sbIPV6, 64);
                        perfItemHeader.item.Gray = !enabled;
                        if (!enabled)
                            perfItemHeader.item.SmallText = LanuageMgr.GetStr("NotConnect");

                        PerformancePageNet performancenet = new PerformancePageNet(perfItemHeader.performanceCounterNative, isWifi, sbName.ToString());
                        performancenet.OpeningPageMenu += OpeningPageMenuEventHandler;
                        performancenet.SwithGraphicView += SwithGraphicViewEventHandler;
                        performancenet.v4 = sbIPV4.ToString();
                        performancenet.v6 = sbIPV6.ToString();

                        PerfPagesAddToCtl(performancenet, perfItemHeader.item.Name);
                        perfPages.Add(performancenet);

                        perfItemHeader.performancePage = performancenet;

                        perfItemHeader.item.PageIndex = perfPages.Count - 1;
                        performanceLeftList.Items.Add(perfItemHeader.item);
                    }
                }

                performanceLeftList.UpdateAll();
                performanceLeftList.Invalidate();

                PerfPagesTo(0, perfItemHeaderCpu);
                PerfPagesResize(new Size(splitContainerPerfCtls.Panel2.Width - (splitContainerPerfCtls.Panel2.VerticalScroll.Visible ? 40 : 30), splitContainerPerfCtls.Panel2.Height - 30));

                perfInited = true;
            }
        }
        private void PerfInitTray()
        {
            if (!perfTrayInited)
            {
                if (MPERF_InitNetworksPerformanceCounters2() > 0)
                    netCounterMain = MPERF_GetNetworksPerformanceCounters(0);

                formSpeedBall = new FormSpeedBall();
                formSpeedBall.Show();
                ShowWindow(formSpeedBall.Handle, 0);

                Font itemHugeFont = new Font(Font.FontFamily, 10.5f);
                Font itemValueFont = new Font(Font.FontFamily, 10.5f);

                itemCpu = new FormSpeedBall.SpeedItem("CPU", CpuBgColor2, CpuDrawColor);
                itemRam = new FormSpeedBall.SpeedItem(LanuageMgr.GetStr("TitleRam"), RamBgColor2, RamDrawColor2);
                itemDisk = new FormSpeedBall.SpeedItem(LanuageMgr.GetStr("TitleDisk"), DiskBgColor2, DiskDrawColor2);
                itemNet = new FormSpeedBall.SpeedItem(LanuageMgr.GetStr("TitleNet"), NetBgColor2, NetDrawColor2);

                itemCpu.TextFont = itemHugeFont;
                itemCpu.ValueFont = itemValueFont;
                itemRam.TextFont = itemHugeFont;
                itemRam.ValueFont = itemValueFont;
                itemDisk.TextFont = itemHugeFont;
                itemDisk.ValueFont = itemValueFont;
                itemNet.TextFont = itemHugeFont;
                itemNet.ValueFont = itemValueFont;

                itemCpu.GridType = FormSpeedBall.SpeedItemGridType.OneGrid;
                itemRam.GridType = FormSpeedBall.SpeedItemGridType.NoGrid;
                itemNet.GridType = FormSpeedBall.SpeedItemGridType.NoValue;
                itemDisk.GridType = FormSpeedBall.SpeedItemGridType.OneGrid;

                formSpeedBall.Items.Add(itemCpu);
                formSpeedBall.Items.Add(itemRam);
                formSpeedBall.Items.Add(itemDisk);
                formSpeedBall.Items.Add(itemNet);

                perfTrayInited = true;
            }
        }
        private void PerfUpdate()
        {
            foreach (PerfItemHeader h in perfItems)
            {
                if (h.item.Gray) continue;

                string outCustomStr = null;
                int outdata1 = -1;
                int outdata2 = -1;
                if (h.performancePage.PageUpdateSimple(out outCustomStr, out outdata1, out outdata2))
                {
                    if (outCustomStr != null)
                        h.item.SmallText = outCustomStr;
                }
                else
                {
                    if (outCustomStr == null)
                        h.item.SmallText = outdata1.ToString("0.0") + "%";
                }

                if (outdata2 != -1 && outdata2 != -1)
                    h.item.AddData((outdata1 + outdata2) / 2);
                else if (outdata1 != -1)
                    h.item.AddData(outdata1);
            }

            if (currSelectedPerformancePage != null)
                currSelectedPerformancePage.PageUpdate();
        }
        private void PerfClear()
        {
            foreach (IPerformancePage h in perfPages)
                h.PageDelete();
            perfPages.Clear();

            MPERF_DestroyNetworksPerformanceCounters();
            MPERF_DestroyDisksPerformanceCounters();
            perfItems.Clear();

            MDEVICE_DestroyLogicalDiskInfo();
            MDEVICE_DestroyNetworkAdaptersInfo();
            MDEVICE_UnInit();

            SetConfig("OldSizeGraphic", "AppSetting", lastGraphicSize.Width + "-" + lastGraphicSize.Height);

            formSpeedBall.Close();
        }
        private void PerfUpdateGridUnit()
        {
            string unistr = "";
            if (baseProcessRefeshTimer.Enabled)
                unistr = (baseProcessRefeshTimer.Interval / 1000 * 60).ToString() + str_sec;
            else unistr = str_status_paused;
            foreach (IPerformancePage p in perfPages)
                p.PageSetGridUnit(unistr);
        }
        private void PerfSetTrayPos()
        {
            Point p = MousePosition;
            p.X -= 15; p.Y -= 15;
            Point t = new Point();
            if (p.Y < 50)
                t.Y = 45;
            else if (p.Y > Screen.PrimaryScreen.Bounds.Height - 50)
                t.Y = Screen.PrimaryScreen.Bounds.Height - formSpeedBall.Height - 45;
            else t.Y = p.Y - formSpeedBall.Height;

            if (p.X < 50)
                t.X = 45;
            else if (p.X > Screen.PrimaryScreen.Bounds.Width - 50)
                t.X = Screen.PrimaryScreen.Bounds.Width - formSpeedBall.Width - 45;
            else t.X = p.X - formSpeedBall.Width;

            if (t.X < 0) t.X = 2;
            if (t.X > Screen.PrimaryScreen.Bounds.Width - formSpeedBall.Width) t.X = Screen.PrimaryScreen.Bounds.Width - formSpeedBall.Width - 2;
            if (t.Y < 0) t.Y = 2;
            if (t.Y > Screen.PrimaryScreen.Bounds.Height - formSpeedBall.Height) t.X = Screen.PrimaryScreen.Bounds.Height - formSpeedBall.Height - 2;

            formSpeedBall.Location = t;
        }

        private FormSpeedBall formSpeedBall = null;

        private void linkLabelOpenPerfMon_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MRunExe("perfmon.exe", "/res");
        }

        #endregion

        #region LanuageWork

        public static string str_system_interrupts = "";
        public static string str_idle_process = "";
        public static string str_service_host = "";
        public static string str_status_paused = "";
        public static string str_status_hung = "";
        public static string str_status_running = "";
        public static string str_status_stopped = "";

        public static string str_proc_count = "";
        public static string str_proc_32 = "";
        public static string str_proc_64 = "";
        public static string str_get_failed = "";
        public static string str_sec = "";
        public static string str_loading = "";
        public static string str_frocedelsuccess = "";
        public static string str_filldatasuccess = "";
        public static string str_filldatafailed = "";
        public static string str_getfileinfofailed = "";
        public static string str_filenotexist = "";
        public static string str_failed = "";

        public static string str_endproc = "";
        public static string str_endtask = "";
        public static string str_resrat = "";

        public static string str_VisitFolderFailed = "";
        public static string str_TipTitle = "";
        public static string str_ErrTitle = "";
        public static string str_AskTitle = "";
        public static string str_PathUnExists = "";
        public static string str_PleaseEnterPath = "";
        public static string str_Ready = "";
        public static string str_ReadyStatus = "";
        public static string str_ReadyStatusEnd = "";
        public static string str_ReadyStatusEnd2 = "";
        public static string str_FileCuted = "";
        public static string str_FileCopyed = "";
        public static string str_NewFolderFailed = "";
        public static string str_NewFolderSuccess = "";
        public static string str_PathCopyed = "";
        public static string str_FolderCuted = "";
        public static string str_FolderCopyed = "";
        public static string str_FolderHasExist = "";
        public static string str_OpenAsk = "";
        public static string str_PathStart = "";
        public static string str_DriverLoad = "";
        public static string str_AutoStart = "";
        public static string str_DemandStart = "";
        public static string str_Disabled = "";
        public static string str_FileSystem = "";
        public static string str_KernelDriver = "";
        public static string str_UserService = "";
        public static string str_SystemService = "";
        public static string str_InvalidFileName = "";
        public static string str_RefeshSuccess = "";
        public static string str_InvalidHwnd = "";
        public static string str_ChangeWindowTextAsk = "";
        public static string str_UnlockFileSuccess = "";
        public static string str_UnlockFileFailed = "";
        public static string str_CollectingFiles = "";
        public static string str_DeleteFiles = "";
        public static string str_PleaseChooseDriver = "";
        public static string str_DriverLoadSuccessFull = "";
        public static string str_DriverLoadFailed = "";
        public static string str_DriverUnLoadSuccessFull = "";
        public static string str_DriverUnLoadFailed = "";
        public static string str_PleaseEnterDriverServiceName = "";
        public static string str_DriverCount = "";
        public static string str_FileNotExist = "";
        public static string str_DriverCountLoaded = "";
        public static string str_AppTitle = "";
        public static string str_FileTrust = "";
        public static string str_FileTrustViewCrt = "";
        public static string str_FunCreateing = "";
        public static string str_PleaseEnterTargetAddress = "";
        public static string str_PleaseEnterDaSize = "";
        public static string str_DblClickToDa = "";
        public static string str_KillAskStart = "";
        public static string str_KillAskEnd = "";
        public static string str_KillAskImporantGiveup = "";
        public static string str_KillAskContentImporant = "";
        public static string str_Close = "";
        public static string str_Cancel = "";
        public static string str_KillAskContentVeryImporant = "";
        public static string str_TitleVeryWarn = "";
        public static string str_SuspendStart = "";
        public static string str_SuspendEnd = "";
        public static string str_SuspendWarnContent = "";
        public static string str_SuspendVeryImporantWarnContent = "";
        public static string str_DblCklShow_EPROCESS = "";
        public static string str_DblCklShow_KPROCESS = "";
        public static string str_DblCklShow_PEB = "";
        public static string str_DblCklShow_RTL_USER_PROCESS_PARAMETERS = "";
        public static string str_CantFind = "";
        public static string str_No = "";
        public static string str_Yes = "";
        public static string str_SetTo = "";
        public static string str_KillTreeAskEnd = "";
        public static string str_KillTreeContent = "";
        public static string str_Sent = "";
        public static string str_Receive = "";
        public static string str_MemFree = "Free";
        public static string str_MemModifed = "Modifed";
        public static string str_MemStandby = "Standby";
        public static string str_MemUsingS = "Using";
        public static string str_IdleProcessDsb = "IdleProcessDsb";
        public static string str_InterruptsProcessDsb = "";
        public static string str_WarnTitle = "";
        public static string str_DriverNotLoad = "";
        public static string str_SuspendThisTitle = "";
        public static string str_SuspendThisText = "";
        public static string str_SuspendCheckText = "";
        public static string str_ShowMain = "";
        public static string str_HideMain = "";

        /*
        
         * DblCklShow_EPROCESS	Double Click this item to show _EPROCESS of process	
DblCklShow_KPROCESS	Double Click this item to show _KPROCESS of process	
DblCklShow_PEB	Double Click this item to show _PEB of process	
DblCklShow_RTL_USER_PROCESS_PARAMETERS	Double Click this item to show RTL_USER_PROCESS_PARAMETERS  of process	

         */

        public static void InitLanuage()
        {
            string lanuage = GetConfig("Lanuage", "AppSetting");
            if (lanuage != "")
            {
                try
                {
                    Log("Load Lanuage Resource : " + lanuage);
                    LanuageMgr.LoadLanuageResource(lanuage);
                }
                catch (Exception e)
                {
                    LogErr("Lanuage resource load failed !\n" + e.ToString());
                    MessageBox.Show(e.ToString(), "ERROR !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                LanuageMgr.LoadLanuageResource("zh");
                SetConfig("Lanuage", "AppSetting", "zh");
                LogWarn("Not found Lanuage settings , use default zh-CN .");
            }

            InitLanuageItems();
            if (lanuage != "" && lanuage != "zh" && lanuage != "zh-CN")
            {
                UWPManager.StringLocate = lanuage;
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lanuage);
            }
        }
        private static void InitLanuageItems()
        {
            try
            {
                MLG_SetLanuageItems_CanRealloc();

                str_InterruptsProcessDsb = LanuageMgr.GetStr("InterruptsProcessDsb");
                str_system_interrupts = LanuageMgr.GetStr("SystemInterrupts"); 
                str_DriverLoadFailed = LanuageMgr.GetStr("DriverLoadFailed");
                str_ShowMain = LanuageMgr.GetStr("ShowMain");
                str_HideMain = LanuageMgr.GetStr("HideMain");
                str_WarnTitle = LanuageMgr.GetStr("WarnTitle");
                str_IdleProcessDsb = LanuageMgr.GetStr("IdleProcessDsb");
                str_MemFree = LanuageMgr.GetStr("MemFree");
                str_MemModifed = LanuageMgr.GetStr("MemModifed");
                str_MemStandby = LanuageMgr.GetStr("MemStandby");
                str_MemUsingS = LanuageMgr.GetStr("MemUsingS");
                str_Receive = LanuageMgr.GetStr("Receive");
                str_Sent = LanuageMgr.GetStr("Send");
                str_KillTreeAskEnd = LanuageMgr.GetStr("KillTreeAskEnd");
                str_KillTreeContent = LanuageMgr.GetStr("KillTreeContent");
                str_No = LanuageMgr.GetStr("No");
                str_Yes = LanuageMgr.GetStr("Yes");
                str_DblClickToDa = LanuageMgr.GetStr("DblClickToDa");
                str_FunCreateing = LanuageMgr.GetStr("FunCreateing");
                str_FileTrustViewCrt = LanuageMgr.GetStr("FileTrustViewCrt");
                str_AppTitle = LanuageMgr.GetStr("AppTitle");
                str_DriverCountLoaded = LanuageMgr.GetStr("DriverCountLoaded");
                str_FileNotExist = LanuageMgr.GetStr("FileNotExist");
                str_PleaseEnterDriverServiceName = LanuageMgr.GetStr("PleaseEnterDriverServiceName");
                str_DriverUnLoadFailed = LanuageMgr.GetStr("DriverUnLoadFailed");
                str_DriverUnLoadSuccessFull = LanuageMgr.GetStr("DriverUnLoadSuccessFull");
                str_DriverLoadSuccessFull = LanuageMgr.GetStr("DriverLoadSuccessFull");
                str_DriverLoadSuccessFull = LanuageMgr.GetStr("DriverLoadSuccessFull");
                str_DeleteFiles = LanuageMgr.GetStr("DeleteFiles");
                str_CollectingFiles = LanuageMgr.GetStr("CollectingFiles");
                str_UnlockFileSuccess = LanuageMgr.GetStr("UnlockFileSuccess");
                str_UnlockFileFailed = LanuageMgr.GetStr("UnlockFileFailed");
                str_filenotexist = LanuageMgr.GetStr("PathUnExists");
                str_failed = LanuageMgr.GetStr("OpFailed");
                str_getfileinfofailed = LanuageMgr.GetStr("GetFileInfoFailed");
                str_filldatasuccess = LanuageMgr.GetStr("FillFileSuccess");
                str_filldatafailed = LanuageMgr.GetStr("FillFileFailed");
                str_frocedelsuccess = LanuageMgr.GetStr("FroceDelSuccess");
                str_idle_process = LanuageMgr.GetStr("SystemIdleProcess");
                str_service_host = LanuageMgr.GetStr("ServiceHost");
                str_status_paused = LanuageMgr.GetStr("StatusPaused");
                str_status_hung = LanuageMgr.GetStr("StatusHang");
                str_proc_count = LanuageMgr.GetStr("ProcessCount");
                str_proc_32 = LanuageMgr.GetStr("Process32Bit");
                str_proc_64 = LanuageMgr.GetStr("Process64Bit");
                str_get_failed = LanuageMgr.GetStr("GetFailed");
                str_sec = LanuageMgr.GetStr("Second");
                str_status_running = LanuageMgr.GetStr("StatusRunning");
                str_status_stopped = LanuageMgr.GetStr("StatusStopped");
                str_endproc = LanuageMgr.GetStr("BtnEndProcess");
                str_endtask = LanuageMgr.GetStr("BtnEndTask");
                str_resrat = LanuageMgr.GetStr("BtnRestartText");
                str_loading = LanuageMgr.GetStr("Loading");
                str_VisitFolderFailed = LanuageMgr.GetStr("VisitFolderFailed");
                str_TipTitle = LanuageMgr.GetStr("TipTitle");
                str_ErrTitle = LanuageMgr.GetStr("ErrTitle");
                str_AskTitle = LanuageMgr.GetStr("AskTitle ");
                str_PathUnExists = LanuageMgr.GetStr("PathUnExists");
                str_PleaseEnterPath = LanuageMgr.GetStr("PleaseEnterPath");
                str_Ready = LanuageMgr.GetStr("Ready");
                str_ReadyStatus = LanuageMgr.GetStr("ReadyStatus");
                str_ReadyStatusEnd = LanuageMgr.GetStr("ReadyStatusEnd");
                str_ReadyStatusEnd2 = LanuageMgr.GetStr("ReadyStatusEnd2");
                str_FileCuted = LanuageMgr.GetStr("FileCuted");
                str_FileCopyed = LanuageMgr.GetStr("FileCopyed");
                str_NewFolderFailed = LanuageMgr.GetStr("NewFolderFailed");
                str_NewFolderSuccess = LanuageMgr.GetStr("NewFolderSuccess ");
                str_PathCopyed = LanuageMgr.GetStr("PathCopyed");
                str_FolderCuted = LanuageMgr.GetStr("FolderCuted");
                str_FolderCopyed = LanuageMgr.GetStr("FolderCopyed");
                str_FolderHasExist = LanuageMgr.GetStr("FolderHasExist");
                str_OpenAsk = LanuageMgr.GetStr("OpenAsk");
                str_PathStart = LanuageMgr.GetStr("PathStart");
                str_DriverLoad = LanuageMgr.GetStr("DriverLoad");
                str_AutoStart = LanuageMgr.GetStr("AutoStart");
                str_DemandStart = LanuageMgr.GetStr("DemandStart");
                str_Disabled = LanuageMgr.GetStr("Disabled");
                str_FileSystem = LanuageMgr.GetStr("FileSystem");
                str_KernelDriver = LanuageMgr.GetStr("KernelDriver");
                str_UserService = LanuageMgr.GetStr("UserService");
                str_SystemService = LanuageMgr.GetStr("SystemService");
                str_InvalidFileName = LanuageMgr.GetStr("InvalidFileName");
                str_InvalidHwnd = LanuageMgr.GetStr("InvalidHwnd");
                str_RefeshSuccess = LanuageMgr.GetStr("RefeshSuccess");
                str_PleaseChooseDriver = LanuageMgr.GetStr("PleaseChooseDriver");
                str_DriverCount = LanuageMgr.GetStr("DriverCount");
                str_FileTrust = LanuageMgr.GetStr("FileTrust");
                str_PleaseEnterTargetAddress = LanuageMgr.GetStr("PleaseEnterTargetAddress");
                str_PleaseEnterDaSize = LanuageMgr.GetStr("PleaseEnterDaSize");
                str_KillAskStart = LanuageMgr.GetStr("KillAskStart");
                str_KillAskEnd = LanuageMgr.GetStr("KillAskEnd");
                str_KillAskImporantGiveup = LanuageMgr.GetStr("KillAskImporantGiveup");
                str_KillAskContentImporant = LanuageMgr.GetStr("KillAskContentImporant");
                str_Close = LanuageMgr.GetStr("Close");
                str_Cancel = LanuageMgr.GetStr("Cancel");
                str_KillAskContentVeryImporant = LanuageMgr.GetStr("KillAskContentVeryImporant");
                str_TitleVeryWarn = LanuageMgr.GetStr("TitleVeryWarn");
                str_SuspendStart = LanuageMgr.GetStr("SuspendStart");
                str_SuspendEnd = LanuageMgr.GetStr("SuspendEnd");
                str_SuspendWarnContent = LanuageMgr.GetStr("SuspendWarnContent");
                str_SuspendVeryImporantWarnContent = LanuageMgr.GetStr("SuspendVeryImporantWarnContent");
                str_DblCklShow_EPROCESS = LanuageMgr.GetStr("DblCklShow_EPROCESS");
                str_DblCklShow_KPROCESS = LanuageMgr.GetStr("DblCklShow_KPROCESS");
                str_DblCklShow_PEB = LanuageMgr.GetStr("DblCklShow_PEB");
                str_DblCklShow_RTL_USER_PROCESS_PARAMETERS = LanuageMgr.GetStr("DblCklShow_RTL_USER_PROCESS_PARAMETERS");
                str_CantFind = LanuageMgr.GetStr("CantFind");
                str_SetTo = LanuageMgr.GetStr("SetTo");
                str_DriverNotLoad = LanuageMgr.GetStr("DriverNotLoad");
                str_SuspendThisTitle = LanuageMgr.GetStr("SuspendThisTitle");
                str_SuspendThisText = LanuageMgr.GetStr("SuspendThisText");
                str_SuspendCheckText = LanuageMgr.GetStr("SuspendCheckText");

                MAppSetLanuageItems(0, 0, str_KillAskStart, 0);
                MAppSetLanuageItems(0, 1, str_KillAskEnd, 0);
                MAppSetLanuageItems(0, 2, LanuageMgr.GetStr("KillAskContent"), 0);
                MAppSetLanuageItems(0, 3, LanuageMgr.GetStr("KillFailed"), 0);
                MAppSetLanuageItems(0, 4, LanuageMgr.GetStr("AccessDenied"), 0);
                MAppSetLanuageItems(0, 5, LanuageMgr.GetStr("OpFailed"), 0);
                MAppSetLanuageItems(0, 6, LanuageMgr.GetStr("InvalidProcess"), 0);
                MAppSetLanuageItems(0, 7, LanuageMgr.GetStr("CantCopyFile"), 0);
                MAppSetLanuageItems(0, 8, LanuageMgr.GetStr("CantMoveFile"), 0);
                MAppSetLanuageItems(0, 9, LanuageMgr.GetStr("ChooseTargetDir"), 0);

                int size = 0;
                MAppSetLanuageItems(1, 0, LanuageMgr.GetStr2("Moveing", out size), size);
                MAppSetLanuageItems(1, 1, LanuageMgr.GetStr2("Copying", out size), size);
                MAppSetLanuageItems(1, 2, LanuageMgr.GetStr2("FileExist", out size), size);
                MAppSetLanuageItems(1, 3, LanuageMgr.GetStr2("FileExist2", out size), size);
                MAppSetLanuageItems(1, 4, LanuageMgr.GetStr2("TitleQuestion", out size), size);
                MAppSetLanuageItems(1, 5, LanuageMgr.GetStr2("TipTitle", out size), size);
                MAppSetLanuageItems(1, 6, LanuageMgr.GetStr2("DelSure", out size), size);
                MAppSetLanuageItems(1, 7, LanuageMgr.GetStr2("DelAsk1", out size), size);
                MAppSetLanuageItems(1, 8, LanuageMgr.GetStr2("DelAsk2", out size), size);
                MAppSetLanuageItems(1, 9, LanuageMgr.GetStr2("DelAsk3", out size), size);
                MAppSetLanuageItems(1, 10, LanuageMgr.GetStr2("DeleteIng", out size), size);
                MAppSetLanuageItems(1, 11, LanuageMgr.GetStr2("NoAdminTipText", out size), size);
                MAppSetLanuageItems(1, 12, LanuageMgr.GetStr2("NoAdminTipTitle", out size), size);
                MAppSetLanuageItems(1, 13, LanuageMgr.GetStr2("DelFailed", out size), size);
                MAppSetLanuageItems(1, 14, str_idle_process, str_idle_process.Length + 1);
                MAppSetLanuageItems(1, 15, LanuageMgr.GetStr2("EndProcFailed", out size), size);
                MAppSetLanuageItems(1, 16, LanuageMgr.GetStr2("OpenProcFailed", out size), size);
                MAppSetLanuageItems(1, 17, LanuageMgr.GetStr2("SusProcFailed", out size), size);
                MAppSetLanuageItems(1, 18, LanuageMgr.GetStr2("ResProcFailed", out size), size);
                MAppSetLanuageItems(1, 19, LanuageMgr.GetStr2("MenuRebootAsAdmin", out size), size);
                MAppSetLanuageItems(1, 20, LanuageMgr.GetStr2("Visible", out size), size);
                MAppSetLanuageItems(1, 21, LanuageMgr.GetStr2("CantGetPath", out size), size);
                MAppSetLanuageItems(1, 22, LanuageMgr.GetStr2("FreeLibSuccess", out size), size);
                MAppSetLanuageItems(1, 23, LanuageMgr.GetStr2("Priority", out size), size);
                MAppSetLanuageItems(1, 24, LanuageMgr.GetStr2("EntryPoint", out size), size);
                MAppSetLanuageItems(1, 25, LanuageMgr.GetStr2("ModuleName", out size), size);
                MAppSetLanuageItems(1, 26, LanuageMgr.GetStr2("State", out size), size);
                MAppSetLanuageItems(1, 27, LanuageMgr.GetStr2("ContextSwitch", out size), size);
                MAppSetLanuageItems(1, 28, LanuageMgr.GetStr2("ModulePath", out size), size);
                MAppSetLanuageItems(1, 29, LanuageMgr.GetStr2("Address", out size), size);
                MAppSetLanuageItems(1, 30, LanuageMgr.GetStr2("Size", out size), size);
                MAppSetLanuageItems(1, 31, LanuageMgr.GetStr2("TitlePublisher", out size), size);
                MAppSetLanuageItems(1, 32, LanuageMgr.GetStr2("WindowText", out size), size);
                MAppSetLanuageItems(1, 33, LanuageMgr.GetStr2("WindowHandle", out size), size);
                MAppSetLanuageItems(1, 34, LanuageMgr.GetStr2("WindowClassName", out size), size);
                MAppSetLanuageItems(1, 35, LanuageMgr.GetStr2("BelongThread", out size), size);
                MAppSetLanuageItems(1, 36, LanuageMgr.GetStr2("VWinTitle", out size), size);
                MAppSetLanuageItems(1, 37, LanuageMgr.GetStr2("VModulTitle", out size), size);
                MAppSetLanuageItems(1, 38, LanuageMgr.GetStr2("VThreadTitle", out size), size);
                MAppSetLanuageItems(1, 39, LanuageMgr.GetStr2("EnumModuleFailed", out size), size);
                MAppSetLanuageItems(1, 40, LanuageMgr.GetStr2("EnumThreadFailed", out size), size);
                MAppSetLanuageItems(1, 41, LanuageMgr.GetStr2("FreeInvalidProc", out size), size);
                MAppSetLanuageItems(1, 42, LanuageMgr.GetStr2("FreeFailed", out size), size);
                MAppSetLanuageItems(1, 43, LanuageMgr.GetStr2("KillThreadError", out size), size);
                MAppSetLanuageItems(1, 44, LanuageMgr.GetStr2("KillThreadInvThread", out size), size);
                MAppSetLanuageItems(1, 45, LanuageMgr.GetStr2("OpenThreadFailed", out size), size);
                MAppSetLanuageItems(1, 46, LanuageMgr.GetStr2("SuThreadErr", out size), size);
                MAppSetLanuageItems(1, 47, LanuageMgr.GetStr2("ReThreadErr", out size), size);
                MAppSetLanuageItems(1, 48, LanuageMgr.GetStr2("InvThread", out size), size);
                MAppSetLanuageItems(1, 49, LanuageMgr.GetStr2("SuThreadWarn", out size), size);
                MAppSetLanuageItems(1, 50, LanuageMgr.GetStr2("KernelNotLoad", out size), size);

                MAppSetLanuageItems(2, 0, LanuageMgr.GetStr2("DelStartupItemAsk", out size), size);
                MAppSetLanuageItems(2, 1, LanuageMgr.GetStr2("DelStartupItemAsk2", out size), size);
                MAppSetLanuageItems(2, 2, str_endtask, str_endtask.Length + 1);
                MAppSetLanuageItems(2, 3, str_resrat, str_resrat.Length + 1);
                MAppSetLanuageItems(2, 4, LanuageMgr.GetStr2("LoadDriver", out size), size);
                MAppSetLanuageItems(2, 5, LanuageMgr.GetStr2("UnLoadDriver", out size), size);
                MAppSetLanuageItems(2, 6, str_FileNotExist, str_FileNotExist.Length + 1);
                MAppSetLanuageItems(2, 7, LanuageMgr.GetStr2("FileTrust", out size), size);
                MAppSetLanuageItems(2, 8, LanuageMgr.GetStr2("FileNotTrust", out size), size);
                MAppSetLanuageItems(2, 9, LanuageMgr.GetStr2("OpenServiceError", out size), size);
                MAppSetLanuageItems(2, 10, LanuageMgr.GetStr2("DelScError", out size), size);
                MAppSetLanuageItems(2, 11, LanuageMgr.GetStr2("ChangeScStartTypeFailed", out size), size);
                MAppSetLanuageItems(2, 12, str_SetTo, str_SetTo.Length + 1);
                MAppSetLanuageItems(2, 13, str_KillTreeAskEnd, str_KillTreeAskEnd.Length + 1);
                MAppSetLanuageItems(2, 14, str_KillTreeContent, str_KillTreeContent.Length + 1);
                MAppSetLanuageItems(2, 15, LanuageMgr.GetStr2("WantDisconnectUser", out size), size);
                MAppSetLanuageItems(2, 16, LanuageMgr.GetStr2("WantLogooffUser", out size), size);
                MAppSetLanuageItems(2, 17, LanuageMgr.GetStr2("PleaseEnterPassword", out size), size);
                MAppSetLanuageItems(2, 18, LanuageMgr.GetStr2("ConnectSessionFailed", out size), size);
                MAppSetLanuageItems(2, 19, LanuageMgr.GetStr2("ConnectSession", out size), size);
                MAppSetLanuageItems(2, 20, LanuageMgr.GetStr2("DisConnectSession", out size), size);
                MAppSetLanuageItems(2, 21, LanuageMgr.GetStr2("DisConnectSessionFailed", out size), size);
                MAppSetLanuageItems(2, 22, LanuageMgr.GetStr2("LogoffSession", out size), size);
                MAppSetLanuageItems(2, 23, LanuageMgr.GetStr2("DisConnectSessionFailed1", out size), size);
                MAppSetLanuageItems(2, 24, LanuageMgr.GetStr2("SetProcPriorityClassFailed", out size), size);
                MAppSetLanuageItems(2, 25, LanuageMgr.GetStr2("SetProcAffinityFailed", out size), size);
                MAppSetLanuageItems(2, 26, str_WarnTitle, str_WarnTitle.Length + 1);
                MAppSetLanuageItems(2, 27, LanuageMgr.GetStr2("LoadDriverWarn", out size), size);
                MAppSetLanuageItems(2, 28, LanuageMgr.GetStr2("LoadDriverWarnTitle", out size), size);
                MAppSetLanuageItems(2, 29, LanuageMgr.GetStr2("DetachDebuggerTitle", out size), size);
                MAppSetLanuageItems(2, 30, LanuageMgr.GetStr2("DetachDebuggerError", out size), size);
                MAppSetLanuageItems(2, 31, LanuageMgr.GetStr2("DetachDebuggerNotDebugger", out size), size);
                MAppSetLanuageItems(2, 32, LanuageMgr.GetStr2("ChangePriorityAsk", out size), size);
                MAppSetLanuageItems(2, 33, LanuageMgr.GetStr2("ChangePriorityContent", out size), size);
                MAppSetLanuageItems(2, 34, LanuageMgr.GetStr2("OpenFileError", out size), size);
                MAppSetLanuageItems(2, 35, LanuageMgr.GetStr2("CreateDumpFailed", out size), size);
                MAppSetLanuageItems(2, 36, LanuageMgr.GetStr2("CreateDumpSuccess", out size), size);
                MAppSetLanuageItems(2, 37, LanuageMgr.GetStr2("PleaseEnumIn64", out size), size);
                

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ERROR !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string Native_LanuageItems_CallBack(string s)
        {
            return LanuageMgr.GetStr(s);
        }

        #endregion

        #region StartMWork

        private TaskMgrListItemGroup knowDlls = null;
        private TaskMgrListItemGroup rightMenu1 = null;
        private TaskMgrListItemGroup rightMenu2 = null;
        private TaskMgrListItemGroup rightMenu3 = null;
        private TaskMgrListItemGroup printMonitors = null;
        private TaskMgrListItemGroup printProviders = null;

        private TaskListViewUWPColumnSorter startColumnSorter = new TaskListViewUWPColumnSorter();
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
        private void StartMListInit()
        {
            if (!startListInited)
            {
                listStartup.ListViewItemSorter = startColumnSorter;
                listStartup.Header.CloumClick += StartList_Header_CloumClick;

                enumStartupsCallBack = StartMList_CallBack;
                enumStartupsCallBackPtr = Marshal.GetFunctionPointerForDelegate(enumStartupsCallBack);
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

                StartMListRefesh();
                startListInited = true;
            }
        }
        private void StartMListRefesh()
        {
            knowDlls.Childs.Clear();
            rightMenu2.Childs.Clear();
            rightMenu1.Childs.Clear();
            listStartup.Items.Clear();
            startId = 0;

            MEnumStartups(enumStartupsCallBackPtr);

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
                            if (highlight_nosystem && li.SubItems[3].Text != MICROSOFT)
                                settoblue = true;
                        }
                        else if (highlight_nosystem)
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
                            if (highlight_nosystem && li.SubItems[3].Text != MICROSOFT)
                                settoblue = true;
                        }
                        else if (highlight_nosystem)
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
                            if (highlight_nosystem && li.SubItems[3].Text != MICROSOFT)
                                settoblue = true;
                        }
                        else if (highlight_nosystem)
                            settoblue = true;
                    }
                    else if (pathstr.StartsWith("wow64") && pathstr.EndsWith(".dll"))
                    {
#if !_X64_
                        if (!MIs64BitOS())
                        {
                            if (highlight_nosystem)
                                settoblue = true;
                            li.SubItems[3].Text = str_FileNotExist;
                        }
#endif
                        if (pathstr != "wow64.dll" && pathstr != "wow64cpu.dll" && pathstr != "wow64win.dll")
                        {
                            if (highlight_nosystem)
                                settoblue = true;
                            li.SubItems[3].Text = str_FileNotExist;
                        }
                    }
                    else
                    {
                        if (highlight_nosystem)
                            settoblue = true;
                        li.SubItems[3].Text = str_FileNotExist;
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
                li.Image = imageListFileTypeList.Images[".dll"];
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
        private void StartMListRemoveItem(uint id)
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

        private void StartMListExpandAll()
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
        private void StartMListCollapseAll()
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

        private void StartList_Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
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

        #endregion

        #region KernelMWork

        private class ListViewItemComparerKr : IComparer
        {
            private int col;
            private bool asdening = false;

            public int SortColum { get { return col; } set { col = value; } }
            public bool Asdening { get { return asdening; } set { asdening = value; } }

            public int Compare(object o1, object o2)
            {
                ListViewItem x = o1 as ListViewItem, y = o2 as ListViewItem;
                int returnVal = -1;
                if (x.SubItems[col].Text == y.SubItems[col].Text) return -1;
                if (col == 6)
                {
                    int xi, yi;
                    if (int.TryParse(x.SubItems[col].Text, out xi) && int.TryParse(y.SubItems[col].Text, out yi))
                    {
                        if (x.SubItems[col].Text == y.SubItems[col].Text) returnVal = 0;
                        else if (xi > yi) returnVal = 1;
                        else if (xi < yi) returnVal = -1;
                    }
                }
                else returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
                if (asdening) returnVal = -returnVal;
                return returnVal;
            }
        }

        private IntPtr hListHeaderDrv = IntPtr.Zero;

        private ListViewItemComparerKr listViewItemComparerKr = new ListViewItemComparerKr();
        private bool showAllDriver = false;
        private bool canUseKernel = false;

        private void KernelListInit()
        {
            if (!driverListInited)
            {
                if (canUseKernel)
                {
                    enumKernelModulsCallBack = KernelEnumCallBack;
                    enumKernelModulsCallBackPtr = Marshal.GetFunctionPointerForDelegate(enumKernelModulsCallBack);

                    listViewItemComparerKr.SortColum = 6;
                    listDrivers.ListViewItemSorter = listViewItemComparerKr;
                    MAppWorkCall3(182, listDrivers.Handle, IntPtr.Zero);
                    hListHeaderDrv = ComCtlApi.MListViewGetHeaderControl(listDrivers.Handle, false);

                    KernelLisRefesh();
                }
                else
                {
                    listDrivers.Hide();
                    pl_driverNotLoadTip.Show();
                    linkRestartAsAdminDriver.Visible = !MIsRunasAdmin();
                }
                driverListInited = true;
            }
        }
        private void KernelEnumCallBack(IntPtr kmi, IntPtr BaseDllName, IntPtr FullDllPath, IntPtr FullDllPathOrginal, IntPtr szEntryPoint, IntPtr SizeOfImage, IntPtr szDriverObject, IntPtr szBase, IntPtr szServiceName, uint Order)
        {
            if (Order == 9999)
            {
                if (showAllDriver) lbDriversCount.Text = str_DriverCountLoaded + kmi.ToInt32() + "  " + str_DriverCount + BaseDllName.ToInt32();
                else
#if _X64_
                    lbDriversCount.Text = str_DriverCount + kmi.ToInt64();
#else
                    lbDriversCount.Text = str_DriverCount + kmi.ToInt32();
#endif

                return;
            }

            string baseDllName = Marshal.PtrToStringUni(BaseDllName);
            string fullDllPath = Marshal.PtrToStringUni(FullDllPath);

            ListViewItem li = new ListViewItem(baseDllName);
            li.Tag = kmi;
            //7 emepty items
            for (int i = 0; i < 8; i++) li.SubItems.Add(new ListViewItem.ListViewSubItem() { Font = listDrivers.Font });

            if (Order != 10000)
            {
                li.SubItems[0].Text = baseDllName;
                li.SubItems[1].Text = Marshal.PtrToStringUni(szBase);
                li.SubItems[2].Text = Marshal.PtrToStringUni(SizeOfImage);
                li.SubItems[3].Text = Marshal.PtrToStringUni(szDriverObject);
                li.SubItems[4].Text = fullDllPath;
                li.SubItems[5].Text = Marshal.PtrToStringUni(szServiceName);
                li.SubItems[6].Text = Order.ToString();
            }
            else
            {
                li.SubItems[0].Text = baseDllName;
                li.SubItems[1].Text = "-";
                li.SubItems[2].Text = "-";
                li.SubItems[3].Text = "-";
                li.SubItems[4].Text = fullDllPath;
                li.SubItems[5].Text = Marshal.PtrToStringUni(szServiceName);
                li.SubItems[6].Text = "-";
            }

            bool hightlight = false;
            if (MFM_FileExist(fullDllPath))
            {
                StringBuilder exeCompany = new StringBuilder(256);
                if (MGetExeCompany(fullDllPath, exeCompany, 256))
                {
                    li.SubItems[7].Text = exeCompany.ToString();
                    if (highlight_nosystem && exeCompany.ToString() != MICROSOFT)
                        hightlight = true;
                }
                else if (highlight_nosystem) hightlight = true;
                if (hightlight)
                {
                    li.ForeColor = Color.Blue;
                    foreach (ListViewItem.ListViewSubItem s in li.SubItems)
                        s.ForeColor = Color.Blue;
                }
            }
            else
            {
                li.SubItems[7].Text = str_FileNotExist;
                if (highlight_nosystem) hightlight = true;
            }
            if (hightlight)
            {
                li.ForeColor = Color.Blue;
                foreach (ListViewItem.ListViewSubItem s in li.SubItems)
                    s.ForeColor = Color.Blue;
            }

            listDrivers.Items.Add(li);
        }
        private void KernelLisRefesh()
        {
            if (canUseKernel)
            {
                foreach (ListViewItem li in listDrivers.Items)
                {
                    IntPtr kmi = (IntPtr)li.Tag;
                    if (kmi != IntPtr.Zero)
                        M_SU_EnumKernelModulsItemDestroy(kmi);
                }
                listDrivers.Items.Clear();
                M_SU_EnumKernelModuls(enumKernelModulsCallBackPtr, showAllDriver);
            }
        }
        private void KernelListUnInit()
        {
            foreach (ListViewItem li in listDrivers.Items)
            {
                IntPtr kmi = (IntPtr)li.Tag;
                if (kmi != IntPtr.Zero)
                    M_SU_EnumKernelModulsItemDestroy(kmi);
            }
            listDrivers.Items.Clear();
        }

        private void linkRestartAsAdminDriver_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetConfig("LoadKernelDriver", "Configure", "TRUE");
            MAppRebotAdmin2("select kernel");
        }
        private void linkLabelShowKernelTools_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("The function aren't complete. ");
        }

        private void listDrivers_MouseUp(object sender, MouseEventArgs e)
        {
            if (listDrivers.SelectedItems.Count > 0)
            {
                if (e.Button == MouseButtons.Right) M_SU_EnumKernelModuls_ShowMenu((IntPtr)listDrivers.SelectedItems[0].Tag, showAllDriver, 0, 0);
            }
        }
        private void listDrivers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparerKr)listDrivers.ListViewItemSorter).Asdening = !((ListViewItemComparerKr)listDrivers.ListViewItemSorter).Asdening;
            ((ListViewItemComparerKr)listDrivers.ListViewItemSorter).SortColum = e.Column;
            ComCtlApi.MListViewSetColumnSortArrow(hListHeaderSc, ((ListViewItemComparer)listService.ListViewItemSorter).SortColum,
             ((ListViewItemComparer)listService.ListViewItemSorter).Asdening, false);
            listDrivers.Sort();
        }
        private void listDrivers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listDrivers.SelectedItems.Count > 0)
                {
                    ListViewItem item = listDrivers.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listService.PointToScreen(p);
                    M_SU_EnumKernelModuls_ShowMenu((IntPtr)item.Tag, showAllDriver, p.X, p.Y);
                }
            }
        }

        public void ShowFormHooks()
        {
            MessageBox.Show("The function aren't complete. ");
        }

        #endregion

        #region UsersWork

        private void UsersListInit()
        {
            if (!usersListInited)
            {
                enumUsersCallBack = UsersListEnumUsersCallBack;
                enumUsersCallBackCallBack_ptr = Marshal.GetFunctionPointerForDelegate(enumUsersCallBack);

                UsersListLoad();
                usersListInited = true;
            }
        }
        private void UsersListUnInit()
        {
            if (usersListInited)
            {
                listUsers.Items.Clear();
            }
        }
        private void UsersListLoad()
        {
            listUsers.Items.Clear();
            MEnumUsers(enumUsersCallBackCallBack_ptr, IntPtr.Zero);
        }

        private void listUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listUsers.SelectedItem != null)
                {
                    Point p = listStartup.GetiItemPoint(listUsers.SelectedItem);
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
            if (listUsers.SelectedItem != null)
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

        private bool UsersListEnumUsersCallBack(IntPtr userName, uint sessionId, uint userId, IntPtr domain, IntPtr customData)
        {
            string name = Marshal.PtrToStringUni(userName);
            string domainStr = Marshal.PtrToStringUni(domain);
            TaskMgrListItem li = new TaskMgrListItem(name);
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems[0].Text = name;
            li.SubItems[1].Text = userId.ToString();
            li.SubItems[2].Text = sessionId.ToString();
            li.SubItems[3].Text = domainStr;
            li.SubItems[0].Font = listUsers.Font;
            li.SubItems[1].Font = listUsers.Font;
            li.SubItems[2].Font = listUsers.Font;
            li.SubItems[3].Font = listUsers.Font;
            li.Tag = sessionId;
            listUsers.Items.Add(li);
            return true;
        }

        #endregion

        #region NotifyWork

        private FormDelFileProgress delingdialog = null;

        private void DelingDialogInitHide()
        {
            MAppWorkCall3(200, delingdialog.Handle, IntPtr.Zero);
        }
        private void DelingDialogInit()
        {
            delingdialog = new FormDelFileProgress();
            delingdialog.Show(this);
            MAppWorkCall3(200, delingdialog.Handle, IntPtr.Zero);
        }
        private void DelingDialogClose()
        {
            if (delingdialog != null)
            {
                delingdialog.Close();
                delingdialog = null;
            }
        }
        private void ShowHideDelingDialog(bool show)
        {
            delingdialog.Invoke(new Action(delegate
            {
                delingdialog.Visible = show;
                if (show)
                {
                    delingdialog.Location = new Point(Left + Width / 2 - delingdialog.Width / 2, Top + Height / 2 - delingdialog.Height / 2);
                    delingdialog.Text = str_DeleteFiles;
                }
            }));
        }
        private void DelingDialogUpdate(string path, int value)
        {
            delingdialog.label.Invoke(new Action(delegate { delingdialog.label.Text = path; }));
            if (value == -1)
            {
                delingdialog.progressBar.Invoke(new Action(delegate { delingdialog.progressBar.Style = ProgressBarStyle.Marquee; }));
                delingdialog.Invoke(new Action(delegate
                {
                    delingdialog.Text = str_CollectingFiles;
                }));
            }
            else
            {
                delingdialog.progressBar.Invoke(new Action(delegate
                {
                    delingdialog.progressBar.Style = ProgressBarStyle.Blocks;
                    if (value >= 0 && value <= 100) delingdialog.progressBar.Value = value;
                }));
            }
        }

        private string lastVeryExe = "";
        private void FileTrustedLink_HyperlinkClick(object sender, HyperlinkEventArgs e)
        {
            if (!string.IsNullOrEmpty(lastVeryExe))
                MShowExeFileSignatureInfo(lastVeryExe);
        }

        private void StartingProgressShowHide(bool show)
        {
            lbStartingStatus.Invoke(new Action(delegate { lbStartingStatus.Visible = show; }));
            listProcess.Invoke(new Action(delegate { listProcess.Visible = !show; }));
        }
        private void StartingProgressUpdate(string text)
        {
            lbStartingStatus.Invoke(new Action(delegate { lbStartingStatus.Text = text; }));
        }

        private void ShowNoPdbWarn(string moduleName)
        {
            Invoke(new Action(delegate
            {
                TaskDialog noPdbWarnDialog = new TaskDialog(string.Format(LanuageMgr.GetStr("NoPDBWarn"), moduleName), str_TipTitle, string.Format(LanuageMgr.GetStr("NoPDBWarnText"), moduleName, moduleName));
                noPdbWarnDialog.EnableHyperlinks = true;
                noPdbWarnDialog.Show(this);
            }));
        }

        private bool TermintateImporantProcess(IntPtr name, int id)
        {
            TaskDialog taskDialog = null;
            if (id == 1)//强制结束警告
            {
                taskDialog = new TaskDialog(str_KillAskStart + " \"" + Marshal.PtrToStringUni(name) + "\" " + str_KillAskEnd, str_AppTitle, str_KillAskContentImporant);
                taskDialog.VerificationText = str_KillAskImporantGiveup;
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, str_Close),
                new CustomButton(2, str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 2)//强制暂停警告
            {
                taskDialog = new TaskDialog(str_SuspendStart + " \"" + Marshal.PtrToStringUni(name) + "\" " + str_SuspendEnd,
                    str_AppTitle, str_SuspendWarnContent);
                taskDialog.VerificationText = str_KillAskImporantGiveup;
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, str_Close),
                new CustomButton(2, str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 3)//强制结束重要警告
            {
                taskDialog = new TaskDialog(str_KillAskStart + " \"" + Marshal.PtrToStringUni(name) + "\" " + str_KillAskEnd,
                    str_TitleVeryWarn, str_KillAskContentVeryImporant);
                taskDialog.VerificationText = str_KillAskImporantGiveup;
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, str_Close),
                new CustomButton(2, str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 4)//强制暂停重要重要警告
            {
                taskDialog = new TaskDialog(str_SuspendStart + " \"" + Marshal.PtrToStringUni(name) + "\" " + str_SuspendEnd,
                    str_TitleVeryWarn, str_SuspendVeryImporantWarnContent);
                taskDialog.VerificationText = str_KillAskImporantGiveup;
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, str_Close),
                new CustomButton(2, str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 5)//暂停当前进程警告
            {
                taskDialog = new TaskDialog(str_SuspendThisTitle, str_AppTitle, str_SuspendThisText);
                taskDialog.VerificationText = str_SuspendCheckText;
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, str_Yes),
                new CustomButton(2, str_No),
                };
                taskDialog.EnableButton(1, false);
            }

            Results rs = taskDialog.Show(this);
            return rs.ButtonID == 1;
        }
        private void TermintateImporantProcess_TaskDialog_VerificationClick(object sender, CheckEventArgs e)
        {
            TaskDialog taskDialog = sender as TaskDialog;
            taskDialog.EnableButton(1, e.IsChecked);
        }

        #endregion

        #region Callbacks

        private static LanuageItems_CallBack lanuageItems_CallBack;

        private EnumUsersCallBack enumUsersCallBack;

        private MProcessMonitor.ProcessMonitorNewItemCallBack ProcessNewItemCallBackDetails;
        private MProcessMonitor.ProcessMonitorRemoveItemCallBack ProcessRemoveItemCallBackDetails;

        private MProcessMonitor.ProcessMonitorNewItemCallBack ProcessNewItemCallBack;
        private MProcessMonitor.ProcessMonitorRemoveItemCallBack ProcessRemoveItemCallBack;

        private EnumWinsCallBack enumWinsCallBack;
        private GetWinsCallBack getWinsCallBack;

        private IntPtr enumUsersCallBackCallBack_ptr;
        private IntPtr ptrProcessNewItemCallBack;
        private IntPtr ptrProcessRemoveItemCallBack;
        private IntPtr ptrProcessNewItemCallBackDetails;
        private IntPtr ptrProcessRemoveItemCallBackDetails;
        private WNDPROC coreWndProc = null;
        private EXITCALLBACK exitCallBack;
        private WORKERCALLBACK workerCallBack;
        private TerminateImporantWarnCallBack terminateImporantWarnCallBack;
        private MFCALLBACK fileMgrCallBack;

        private EnumServicesCallBack scMgrEnumServicesCallBack;
        private IntPtr scMgrEnumServicesCallBackPtr = IntPtr.Zero;

        private EnumStartupsCallBack enumStartupsCallBack;
        private IntPtr enumStartupsCallBackPtr = IntPtr.Zero;

        private EnumKernelModulsCallBack enumKernelModulsCallBack;
        private IntPtr enumKernelModulsCallBackPtr = IntPtr.Zero;

        #endregion

        private void BaseProcessRefeshTimer_Tick(object sender, EventArgs e)
        {
            //整体刷新定时器

            double cpu = 0;
            double ram = 0;
            double disk = 0;
            double net = 0;

            bool perfsimpleGeted = false;

            if (perfMainInited && IsWindowVisible(formSpeedBall.Handle))
            {
                MPERF_GlobalUpdatePerformanceCounters();

                cpu = MPERF_GetCpuUseAge();
                ram = MPERF_GetRamUseAge2() * 100;
                disk = MPERF_GetDiskUseage() * 100;
                net = MPERF_GetNetWorkUseage() * 100;

                perfsimpleGeted = true;

                itemCpu.Value = cpu.ToString("0.0") + " %";
                itemCpu.NumValue = cpu / 100;
                itemCpu.AddData1((int)cpu);

                ulong all = MSystemMemoryPerformanctMonitor.GetAllMemory();
                ulong used = MSystemMemoryPerformanctMonitor.GetMemoryUsed();

                ulong divor = 0;
                string unit = GetBestFilesizeUnit(all, out divor);

                itemRam.Value = (used / (double)divor).ToString("0.0") + " " + unit + "/" + (all / (double)divor).ToString("0.0") + " " + unit + "  (" + ram.ToString("0.0") + "%)";
                itemRam.NumValue = ram / 100;

                itemDisk.Value = disk.ToString("0.0") + " %";
                itemDisk.NumValue = disk / 100;
                itemDisk.AddData1((int)disk);

                double netsent = 0, netreceive = 0;
                if (MPERF_GetNetworksPerformanceCountersValues(netCounterMain, ref netsent, ref netreceive))
                    itemNet.Value = str_Sent + " : " + (netsent / 1024 * 8).ToString("0.0") + " Kbps  "
                        + str_Receive + " : " + (netreceive / 1024 * 8).ToString("0.0") + " Kbps";
                else itemNet.Value = net.ToString("0.0") + " %";


                formSpeedBall.Invalidate();
            }

            if (!Visible) return;
            if (forceRefeshLowLock) return;
            //base RefeshTimer
            if (tabControlMain.SelectedTab == tabPageProcCtl && processListInited)
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
            else if (tabControlMain.SelectedTab == tabPagePerfCtl)
            {
                if (!perfsimpleGeted)
                {
                    MPERF_GlobalUpdatePerformanceCounters();
                    MPERF_GlobalUpdateCpu();
                }

                PerfUpdate();

                performanceLeftList.Invalidate();
            }
            else if (tabControlMain.SelectedTab == tabPageDetals && processListDetailsInited)
            {
                ProcessListDetailsRefesh();
            }
        }

        //Worker Callback
        private void AppWorkerCallBack(int msg, IntPtr wParam, IntPtr lParam)
        {
            //这是从 c++ 调用回来的函数
            switch (msg)
            {
                case M_CALLBACK_SWITCH_MAINGROUP_SET:
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                        {
                            listProcess.ShowGroup = wParam.ToInt32() == 1;
                            listProcess.SyncItems(true);
                        }
                        break;
                    }
                case M_CALLBACK_SWITCH_REFESHRATE_SET:
                    {
                        int c = wParam.ToInt32();
                        if (c == 2)
                        {
                            baseProcessRefeshTimer.Enabled = false;
                            baseProcessRefeshTimer.Stop();
                            baseProcessRefeshTimerLowUWP.Stop();
                            SetConfig("RefeshTime", "AppSetting", "Stop");
                            baseProcessRefeshTimerLow.Stop();
                            baseProcessRefeshTimerLowSc.Stop();
                            PerfUpdateGridUnit();
                        }
                        else
                        {
                            baseProcessRefeshTimer.Enabled = true;
                            if (c == 1) {
                                baseProcessRefeshTimer.Interval = 2000;
                                baseProcessRefeshTimerLow.Interval = 10000;
                                SetConfig("RefeshTime", "AppSetting", "Slow");
                            }
                            else if (c == 0) {
                                baseProcessRefeshTimer.Interval = 1000;
                                baseProcessRefeshTimerLow.Interval = 5000;
                                SetConfig("RefeshTime", "AppSetting", "Fast");
                            }
                            baseProcessRefeshTimer.Start();
                            baseProcessRefeshTimerLowUWP.Start();
                            baseProcessRefeshTimerLow.Start();
                            baseProcessRefeshTimerLowSc.Start();
                            PerfUpdateGridUnit();
                        }
                        break;
                    }
                case M_CALLBACK_SWITCH_TOPMOST_SET:
                    {
                        TopMost = wParam.ToInt32() == 1;
                        break;
                    }
                case M_CALLBACK_SWITCH_CLOSEHIDE_SET:
                    {
                        close_hide = wParam.ToInt32() == 1;
                        break;
                    }
                case M_CALLBACK_SWITCH_MINHIDE_SET:
                    {
                        min_hide = wParam.ToInt32() == 1;
                        break;
                    }
                case M_CALLBACK_GOTO_SERVICE:
                    {
                        string scname = Marshal.PtrToStringUni(wParam);
                        tabControlMain.SelectedTab = tabPageScCtl;
                        foreach (ListViewItem it in listService.Items)
                        {
                            if (it.Text == scname)
                            {
                                int i = listService.Items.IndexOf(it);
                                listService.EnsureVisible(i);
                                it.Selected = true;
                            }
                            else it.Selected = false;
                        }
                        break;
                    }
                case M_CALLBACK_REFESH_SCLIST:
                    {
                        ScMgrRefeshList();
                        break;
                    }
                case M_CALLBACK_KILLPROCTREE:
                    {
                        PsItem p = ProcessListFindPsItem((uint)wParam.ToInt32());
                        if (p != null) ProcessListKillProcTree(p, true);
                        break;
                    }
                case M_CALLBACK_SPY_TOOL:
                    {
                        new FormSpyWindow(wParam).ShowDialog();
                        break;
                    }
                case M_CALLBACK_FILE_TOOL:
                    {
                        new FormFileTool().ShowDialog();
                        break;
                    }
                case M_CALLBACK_ABOUT:
                    {
                        Caller.ShowAboutDlg();
                        break;
                    }
                case M_CALLBACK_ENDTASK:
                    {
                        uint pid = Convert.ToUInt32(wParam.ToInt32());
                        ProcessListEndTask(pid, null);
                        break;
                    }
                case M_CALLBACK_LOADDRIVER_TOOL:
                    {
                        new FormLoadDriver().Show();
                        break;
                    }
                case M_CALLBACK_SCITEM_REMOVED:
                    {
                        if (scListInited)
                        {
                            string targetName = Marshal.PtrToStringUni(wParam);
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
                        break;
                    }
                case M_CALLBACK_SHOW_PROGRESS_DLG:
                    {
                        ShowHideDelingDialog(true);
                        break;
                    }
                case M_CALLBACK_UPDATE_PROGRESS_DLG_TO_DELETEING:
                    {
                        ShowHideDelingDialog(false);
                        DelingDialogUpdate(str_DeleteFiles, 0);
                        break;
                    }
                case M_CALLBACK_UPDATE_PROGRESS_DLG_ALL:
                    {
                        DelingDialogUpdate(Marshal.PtrToStringUni(wParam), lParam.ToInt32());
                        break;
                    }
                case M_CALLBACK_UPDATE_PROGRESS_DLG_TO_COLLECTING:
                    {
                        DelingDialogUpdate(str_CollectingFiles, -1);
                        break;
                    }
                case M_CALLBACK_KERNEL_INIT:
                    {
                        AppWorkerCallBack(41, IntPtr.Zero, IntPtr.Zero);
                        if (MInitKernel(null))
                            if (GetConfigBool("SelfProtect", "AppSetting"))
                                MAppWorkCall3(203, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case M_CALLBACK_VIEW_HANDLES:
                    {
                        new FormVHandles(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog();
                        break;
                    }
                case M_CALLBACK_KERNEL_INIT_LIST:
                    {
                        KernelListInit();
                        break;
                    }
                case M_CALLBACK_KERNEL_SWITCH_SHOWALLDRV:
                    {
                        showAllDriver = !showAllDriver;
                        KernelLisRefesh();
                        break;
                    }
                case M_CALLBACK_START_ITEM_REMVED:
                    {
                        StartMListRemoveItem(Convert.ToUInt32(wParam.ToInt32()));
                        break;
                    }
                case M_CALLBACK_VIEW_KSTRUCTS:
                    {
                        new FormVKrnInfo(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog();
                        break;
                    }
                case M_CALLBACK_VIEW_TIMER:
                    {
                        //timer
                        if (MCanUseKernel())
                            new FormVTimers(Convert.ToUInt32(wParam.ToInt32())).ShowDialog();
                        else MessageBox.Show(str_DriverNotLoad);
                        break;
                    }
                case M_CALLBACK_VIEW_HOTKEY:
                    {
                        //hotkey
                        if (MCanUseKernel())
                            new FormVHotKeys(Convert.ToUInt32(wParam.ToInt32())).ShowDialog();
                        else MessageBox.Show(str_DriverNotLoad);
                        break;
                    }
                case M_CALLBACK_SHOW_TRUSTED_DLG:
                    {
                        string path = Marshal.PtrToStringUni(wParam);
                        lastVeryExe = path;
                        TaskDialog d = new TaskDialog(str_FileTrust, str_TipTitle, (path == null ? "" : path) + "\n\n" + str_FileTrustViewCrt);
                        d.EnableHyperlinks = true;
                        d.HyperlinkClick += FileTrustedLink_HyperlinkClick;
                        d.Show(this);
                        break;
                    }
                case M_CALLBACK_MDETALS_LIST_HEADER_RIGHTCLICK:
                    {
                        colLastDown = wParam.ToInt32();
                        隐藏列ToolStripMenuItem.Enabled = colLastDown != colNameIndex;
                        contextMenuStripProcDetalsCol.Show(MousePosition);
                        break;
                    }
                case M_CALLBACK_KDA:
                    {
                        new FormKDA().ShowDialog(this);
                        break;
                    }
                case M_CALLBACK_SETAFFINITY:
                    {
                        new FormSetAffinity((uint)wParam.ToInt32(), lParam).ShowDialog();
                        break;
                    }
                case M_CALLBACK_UPDATE_LOAD_STATUS:
                    {
                        StartingProgressUpdate(Marshal.PtrToStringUni(wParam));
                        break;
                    }
                case M_CALLBACK_SHOW_NOPDB_WARN:
                    {
                        ShowNoPdbWarn(Marshal.PtrToStringAnsi(wParam));
                        break;
                    }
                case M_CALLBACK_INVOKE_LASTLOAD_STEP:
                    {
                        Invoke(new Action(AppLastLoadStep));
                        break;
                    }
                case M_CALLBACK_DBGPRINT_SHOW:
                    {
                        if (kDbgPrint == null)
                        {
                            kDbgPrint = new FormKDbgPrint();
                            kDbgPrint.FormClosed += KDbgPrint_FormClosed;
                        }
                        kDbgPrint.Show();
                        break;
                    }
                case M_CALLBACK_DBGPRINT_CLOSE:
                    {
                        if (kDbgPrint != null && !exitkDbgPrintCalled)
                        {
                            exitkDbgPrintCalled = true;
                            kDbgPrint.Close();
                            kDbgPrint = null;
                            exitkDbgPrintCalled = false;
                        }
                        break;
                    }
                case M_CALLBACK_DBGPRINT_DATA:
                    {
                        if (kDbgPrint != null)
                            kDbgPrint.Add(Marshal.PtrToStringUni(wParam));
                        break;
                    }
                case M_CALLBACK_DBGPRINT_EMEPTY:
                    {
                        if (kDbgPrint != null)
                            kDbgPrint.Add("");
                        break;
                    }
                case M_CALLBACK_SHOW_LOAD_STATUS:
                    {
                        if (listProcess.Visible) listProcess.Invoke(new Action(listProcess.Hide));
                        StartingProgressShowHide(true);
                        break;
                    }
                case M_CALLBACK_HLDE_LOAD_STATUS:
                    {
                        if (!listProcess.Visible) listProcess.Invoke(new Action(listProcess.Show));
                        StartingProgressShowHide(false);
                        break;
                    }
                case M_CALLBACK_MDETALS_LIST_HEADER_MOUSEMOVE:
                    {
                        listProcessDetals_ColumnMouseMove(listProcessDetals, wParam.ToInt32(),
                            new Point(LOWORD(lParam), HIWORD(lParam)));
                        break;
                    }
                case M_CALLBACK_KERNEL_VIELL_PRGV:
                    {
                        new FormVPrivilege(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog();
                        break;
                    }
                case M_CALLBACK_KERNEL_TOOL:
                    {
                        linkLabelShowKernelTools_LinkClicked(this, null);
                        break;
                    }
                case M_CALLBACK_HOOKS:
                    {
                        ShowFormHooks();
                        break;
                    }
                case M_CALLBACK_NETMON:
                    {
                        //netmon
                        break;
                    }
                case M_CALLBACK_REGEDIT:
                    {
                        //regedit
                        break;
                    }
                case M_CALLBACK_FILEMGR:
                    {
                        tabControlMain.SelectedTab = tabPageFileCtl;
                        break;
                    }
                case M_CALLBACK_COLLAPSE_ALL:
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                            ProcessListCollapseAll();
                        else if (tabControlMain.SelectedTab == tabPageStartCtl)
                            StartMListCollapseAll();
                        break;
                    }
                case M_CALLBACK_SIMPLEVIEW_ACT:
                    {
                        if (wParam.ToInt32() == 1)
                        {
                            TaskMgrListItem li = listApps.SelectedItem;
                            if (li == null) return;
                            ProcessListEndTask(0, li);
                        }
                        else if (wParam.ToInt32() == 0)
                        {
                            TaskMgrListItem li = listApps.SelectedItem;
                            if (li == null) return;
                            ProcessListSetTo(li);
                        }
                        break;
                    }
                case M_CALLBACK_UWPKILL:
                    {
                        TaskMgrListItem li = listProcess.SelectedItem;
                        if (li != null) ProcessListEndTask(0, li);
                        break;
                    }
                case M_CALLBACK_EXPAND_ALL:
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                            ProcessListExpandAll();
                        else if (tabControlMain.SelectedTab == tabPageStartCtl)
                            StartMListExpandAll();
                        break;
                    }
            }
        }

        #region FormEvent

        public const string QQ = "1501076885";

        private void AppLastLoadStep()
        {
            int id = AppRunAgrs();
            if (id != 0 && GetConfigBool("SimpleView", "AppSetting", true)) id = 0;
            switch (id)
            {
                case 1:
                    tabControlMain.SelectedTab = tabPageKernelCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageKernelCtl, 0, TabControlAction.Selected));
                    break;
                case 3:
                    tabControlMain.SelectedTab = tabPagePerfCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPagePerfCtl, 0, TabControlAction.Selected));
                    break;
                case 4:
                    tabControlMain.SelectedTab = tabPageUWPCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageUWPCtl, 0, TabControlAction.Selected));
                    break;
                case 5:
                    tabControlMain.SelectedTab = tabPageScCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageScCtl, 0, TabControlAction.Selected));
                    break;
                case 6:
                    tabControlMain.SelectedTab = tabPageStartCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageStartCtl, 0, TabControlAction.Selected));
                    break;
                case 7:
                    tabControlMain.SelectedTab = tabPageFileCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageFileCtl, 0, TabControlAction.Selected));
                    break;
                case 8:
                    tabControlMain.SelectedTab = tabPageDetals;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageDetals, 0, TabControlAction.Selected));
                    return;
                case 9:
                    tabControlMain.SelectedTab = tabPageUsers;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageUsers, 0, TabControlAction.Selected));
                    return;
                case 0:
                default:
                    ProcessListInit();
                    break;
            }
            MAppWorkCall3(188, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(177, IntPtr.Zero, IntPtr.Zero);

        }
        private void AppLoadKernel()
        {
            if (GetConfigBool("LoadKernelDriver", "Configure"))
            {
                Log("Loading Kernel...");
                if (!MInitKernel(null))
                {
                    if (eprocessindex != -1)
                    {
                        listProcess.Colunms.Remove(listProcess.Colunms[eprocessindex]);
                        eprocessindex = -1;
                    }

                    if (MIsKernelNeed64())
                        TaskDialog.Show(LanuageMgr.GetStr("LoadDriverErrNeed64"), LanuageMgr.GetStr("ErrTitle"), LanuageMgr.GetStr("LoadDriverErrNeed64Text"), TaskDialogButton.OK, TaskDialogIcon.None);
                    else TaskDialog.Show(LanuageMgr.GetStr("LoadDriverErr"), LanuageMgr.GetStr("ErrTitle"), "", TaskDialogButton.OK, TaskDialogIcon.None);
                    AppLastLoadStep();
                }
                else
                {
                    if (GetConfigBool("SelfProtect", "AppSetting"))
                        MAppWorkCall3(203, IntPtr.Zero, IntPtr.Zero);
                }
                canUseKernel = MCanUseKernel();
            }
            else
            {
                AppLastLoadStep();
                if (eprocessindex != -1)
                {
                    listProcess.Colunms.Remove(listProcess.Colunms[eprocessindex]);
                    eprocessindex = -1;
                }
            }
        }
        //Load and exit
        private void AppLoad()
        {
            AppOnLoad();
        }
        private void AppExit()
        {
            //退出函数
            Log("App exit...");
            AppOnExit();
            Application.Exit();
        }

        private void AppOnLoad()
        {
            //初始化函数
 
            Log("Loading callbacks...");

            exitCallBack = AppExit;
            terminateImporantWarnCallBack = TermintateImporantProcess;
            enumWinsCallBack = MainEnumWinsCallBack;
            getWinsCallBack = MainGetWinsCallBack;
            workerCallBack = AppWorkerCallBack;
            lanuageItems_CallBack = Native_LanuageItems_CallBack;

            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(exitCallBack), 1);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(terminateImporantWarnCallBack), 2);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(enumWinsCallBack), 3);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(getWinsCallBack), 4);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(workerCallBack), 5);
            MLG_SetLanuageItems_CallBack(Marshal.GetFunctionPointerForDelegate(lanuageItems_CallBack));

            MAppWorkCall3(181, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(183, Handle, IntPtr.Zero);
            coreWndProc = (WNDPROC)Marshal.GetDelegateForFunctionPointer(MAppSetCallBack(IntPtr.Zero, 0), typeof(WNDPROC));

            SetConfig("LastWindowTitle", "AppSetting", Text);

            Log("Loading Settings...");

            LoadSettings();

            SysVer.Get();
            if (!SysVer.IsWin8Upper())
                tabControlMain.TabPages.Remove(tabPageUWPCtl);
            else M_UWP_Init();

            if (!MGetPrivileges()) TaskDialog.Show(LanuageMgr.GetStr("FailedGetPrivileges"), str_AppTitle, "", TaskDialogButton.OK, TaskDialogIcon.Warning);
#if _X64_
            Log("64 Bit OS ");
            is64OS = true;
#else
            is64OS = MIs64BitOS();
            Log(is64OS ? "64 Bit OS but 32 bit app " : "32 Bit OS");
#endif

            Log("Loading...");

            LoadList();

            if (MIsRunasAdmin())
                AppLoadKernel();
            else AppLastLoadStep();
        }
        private void AppOnExit()
        {
            if (!exitCalled)
            {
                baseProcessRefeshTimer.Stop();

                fileSystemWatcher.EnableRaisingEvents = false;
                SaveListColumnsWidth();
                ProcessListSimpleExit();
                AppWorkerCallBack(38, IntPtr.Zero, IntPtr.Zero);
                DelingDialogClose();
                MPERF_NET_FreeAllProcessNetInfo();
                UsersListUnInit();
                PerfClear();
                ProcessListUnInitPerfs();
                ProcessListFreeAll();
                ProcessListDetailsUnInit();
                MSCM_Exit();
                KernelListUnInit();
                M_LOG_Close();
                if (SysVer.IsWin8Upper())
                {
                    UWPListUnInit();
                    M_UWP_UnInit();
                }
                MAppWorkCall3(204, IntPtr.Zero, IntPtr.Zero);
                MAppWorkCall3(207, Handle, IntPtr.Zero);
                exitCalled = true;
            }
        }

        private int AppRunAgrs()
        {
            if (agrs.Length > 0)
            {
                Log("App Agrs 0 : " + agrs[0]);
                if (agrs[0] == "select" && agrs.Length > 1)
                {
                    Log("App Agrs 1 : " + agrs[1]);
                    if (agrs[1] == "kernel")
                        return 1;
                    if (agrs[1] == "perf")
                        return 3;
                    if (agrs[1] == "uwpapps")
                        return 4;
                    if (agrs[1] == "services")
                        return 5;
                    if (agrs[1] == "startmgr")
                        return 6;
                    if (agrs[1] == "filemgr")
                        return 7;
                    if (agrs[1] == "details")
                        return 8;
                    if (agrs[1] == "users")
                        return 9;
                }
                else if (agrs[0] == "spy")
                    new FormSpyWindow(GetDesktopWindow()).ShowDialog();
                else if (agrs[0] == "filetool")
                    new FormFileTool().ShowDialog();
                else if (agrs[0] == "loaddriver")
                    new FormLoadDriver().ShowDialog();
            }
            return 0;
        }

        private FormKDbgPrint kDbgPrint = null;
        private bool exitkDbgPrintCalled = false;
        private void KDbgPrint_FormClosed(object sender, FormClosedEventArgs e)
        {
            kDbgPrint = null;
        }

        private bool exitCalled = false;
        private int showHideHotKetId = 0;

        private bool close_hide = false;
        private bool min_hide = false;
        private bool highlight_nosystem = false;

        private void LoadAllFonts()
        {
            FormSettings.LoadFontSettingForUI(tabControlMain);
            pl_simple.Font = tabControlMain.Font;
            pl_perfGridHost.Font = tabControlMain.Font;
            lbStartingStatus.Font = tabControlMain.Font;
        }
        private void LoadList()
        {
            lvwColumnSorter = new TaskListViewColumnSorter(this);

            TaskMgrListGroup lg = new TaskMgrListGroup(LanuageMgr.GetStr("TitleApp"));
            listProcess.Groups.Add(lg);
            TaskMgrListGroup lg2 = new TaskMgrListGroup(LanuageMgr.GetStr("TitleBackGround"));
            listProcess.Groups.Add(lg2);
            TaskMgrListGroup lg3 = new TaskMgrListGroup(LanuageMgr.GetStr("TitleWinApp"));
            listProcess.Groups.Add(lg3);
            listProcess.Header.CanMoveCloum = true;

            listUwpApps.Header.Height = 36;
            listUwpApps.ReposVscroll();
            listStartup.Header.Height = 36;
            listStartup.ReposVscroll();
            //listStartup.DrawIcon = false;
            TaskMgrListHeaderItem li = new TaskMgrListHeaderItem();
            li.TextSmall = LanuageMgr.GetStr("TitleName");
            li.Width = 200;
            listStartup.Colunms.Add(li);
            TaskMgrListHeaderItem li2 = new TaskMgrListHeaderItem();
            li2.TextSmall = LanuageMgr.GetStr("TitleCmdLine");
            li2.Width = 200;
            listStartup.Colunms.Add(li2);
            TaskMgrListHeaderItem li3 = new TaskMgrListHeaderItem();
            li3.TextSmall = LanuageMgr.GetStr("TitleFilePath");
            li3.Width = 200;
            listStartup.Colunms.Add(li3);
            TaskMgrListHeaderItem li4 = new TaskMgrListHeaderItem();
            li4.TextSmall = LanuageMgr.GetStr("TitlePublisher");
            li4.Width = 100;
            listStartup.Colunms.Add(li4);
            TaskMgrListHeaderItem li5 = new TaskMgrListHeaderItem();
            li5.TextSmall = LanuageMgr.GetStr("TitleRegPath");
            li5.Width = 200;
            listStartup.Colunms.Add(li5);

            TaskMgrListHeaderItem li8 = new TaskMgrListHeaderItem();
            li8.TextSmall = LanuageMgr.GetStr("TitleName");
            li8.Width = 400;
            listUwpApps.Colunms.Add(li8);
            TaskMgrListHeaderItem li9 = new TaskMgrListHeaderItem();
            li9.TextSmall = LanuageMgr.GetStr("TitlePublisher");
            li9.Width = 100;
            listUwpApps.Colunms.Add(li9);
            TaskMgrListHeaderItem li10 = new TaskMgrListHeaderItem();
            li10.TextSmall = LanuageMgr.GetStr("TitleFullName");
            li10.Width = 130;
            listUwpApps.Colunms.Add(li10);
            TaskMgrListHeaderItem li11 = new TaskMgrListHeaderItem();
            li11.TextSmall = LanuageMgr.GetStr("TitleInstallDir");
            li11.Width = 130;
            listUwpApps.Colunms.Add(li11);

            listUsers.Header.Height = 36;
            listUsers.ReposVscroll();
            listUsers.DrawIcon = false;
            TaskMgrListHeaderItem li13 = new TaskMgrListHeaderItem();
            li13.TextSmall = LanuageMgr.GetStr("TitleName");
            li13.Width = 450;
            listUsers.Colunms.Add(li13);
            TaskMgrListHeaderItem li14 = new TaskMgrListHeaderItem();
            li14.TextSmall = "ID";
            li14.Width = 50;
            listUsers.Colunms.Add(li14);
            TaskMgrListHeaderItem li15 = new TaskMgrListHeaderItem();
            li15.TextSmall = LanuageMgr.GetStr("TitleSessionID");
            li15.Width = 50;
            listUsers.Colunms.Add(li15);
            TaskMgrListHeaderItem li16 = new TaskMgrListHeaderItem();
            li16.TextSmall = LanuageMgr.GetStr("TitleDomainName");
            li16.Width = 60;
            listUsers.Colunms.Add(li16);

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

            LoadListColumnsWidth();

            sorta = GetConfigBool("ListSortDk", "AppSetting", true);
            string sortitemxx = GetConfig("ListSortIndex", "AppSetting", "0");
            if (sortitemxx != "" && sortitemxx != "-1")
                int.TryParse(sortitemxx, out sortitem);
            showHiddenFiles = GetConfigBool("ShowHiddenFiles", "AppSetting");
            MFM_SetShowHiddenFiles(showHiddenFiles);
        }
        private void LoadSettings()
        {
            MAppWorkCall3(206, IntPtr.Zero, new IntPtr(GetConfig("TerProcFun", "Configure", "PspTerProc") == "ApcPspTerProc" ? 1 : 0));
            highlight_nosystem = GetConfigBool("HighLightNoSystetm", "Configure", false);
            mergeApps = GetConfigBool("MergeApps", "Configure", true);

            int iSplitterDistanceperf = 0;
            if (int.TryParse(GetConfig("SplitterDistancePerf", "AppSetting", "0"), out iSplitterDistanceperf) && iSplitterDistanceperf > 0)
                splitContainerPerfCtls.SplitterDistance = iSplitterDistanceperf;

            isRamPercentage = GetConfigBool("RamPercentage", "Configure", false);
            isDiskPercentage = GetConfigBool("DiskPercentage", "Configure", false);
            isNetPercentage = GetConfigBool("NetPercentage", "Configure", false);

            if (isRamPercentage) 百分比ToolStripMenuItemRam.Checked = true;
            else 值ToolStripMenuItemRam.Checked = true;

            if (isDiskPercentage) 百分比ToolStripMenuItemDisk.Checked = true;
            else 值ToolStripMenuItemDisk.Checked = true;

            if (isNetPercentage) 百分比ToolStripMenuItemNet.Checked = true;
            else 值ToolStripMenuItemNet.Checked = true;

            MAppWorkCall3(194, IntPtr.Zero, GetConfigBool("TopMost", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(195, IntPtr.Zero, GetConfigBool("CloseHideToNotfication", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(196, IntPtr.Zero, GetConfigBool("MinHide", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(162, IntPtr.Zero, GetConfigBool("MainGrouping", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(164, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(163, IntPtr.Zero, IntPtr.Zero);
        }
        private void LoadLastPos()
        {
            if (GetConfigBool("OldIsMax", "AppSetting"))
                WindowState = FormWindowState.Maximized;
            else
            {
                bool s_isSimpleView = GetConfigBool("SimpleView", "AppSetting", true);

                string p = GetConfig("OldPos", "AppSetting");
                if (p.Contains("-"))
                {
                    string[] pp = p.Split('-');
                    try
                    {
                        Left = int.Parse(pp[0]);
                        Top = int.Parse(pp[1]);
                        if (Left > Screen.PrimaryScreen.Bounds.Width)
                            Left = 100;
                        if (Top > Screen.PrimaryScreen.Bounds.Height)
                            Top = 200;
                    }
                    catch { }
                }

                string sg = GetConfig("OldSizeGraphic", "AppSetting", "640-320");
                if (sg.Contains("-"))
                {
                    string[] ss = sg.Split('-');
                    try
                    {
                        int w = int.Parse(ss[0]); if (w + Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - Left;
                        int h = int.Parse(ss[1]); if (h + Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - Top;
                        lastGraphicSize = new Size(w, h);

                        if (s_isSimpleView)
                        {
                            Width = w;
                            Height = h;
                        }
                    }
                    catch { }
                }
                string sl = GetConfig("OldSizeSimple", "AppSetting", "380-334");
                if (sl.Contains("-"))
                {
                    string[] ss = sl.Split('-');
                    try
                    {
                        int w = int.Parse(ss[0]); if (w + Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - Left;
                        int h = int.Parse(ss[1]); if (h + Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - Top;
                        lastSimpleSize = new Size(w, h);

                        if (s_isSimpleView)
                        {
                            Width = w;
                            Height = h;
                        }
                    }
                    catch { }
                }
                string s = GetConfig("OldSize", "AppSetting", "780-500");
                if (s.Contains("-"))
                {
                    string[] ss = s.Split('-');
                    try
                    {
                        int w = int.Parse(ss[0]); if (w + Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - Left;
                        int h = int.Parse(ss[1]); if (h + Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - Top;
                        lastSize = new Size(w, h);
                        if (!s_isSimpleView)
                        {
                            Width = w;
                            Height = h;
                        }
                    }
                    catch { }
                }
            }
        }
        private void LoadHotKey()
        {
            if (GetConfigBool("HotKey", "AppSetting", true))
            {
                string k1 = GetConfig("HotKey1", "AppSetting", "(None)");
                string k2 = GetConfig("HotKey2", "AppSetting", "T");
                if (k1 == "(None)") k1 = "None";
                Keys kv1, kv2;
                try
                {
                    if (k1 != "(None)") kv1 = (Keys)Enum.Parse(typeof(Keys), k1);
                    else kv1 = Keys.None;
                    kv2 = (Keys)Enum.Parse(typeof(Keys), k2);
                }
                catch (Exception e)
                {
                    LogErr("Invalid hotkey settings : " + e.Message);
                    kv2 = Keys.T;
                    kv1 = Keys.Shift;
                }

                showHideHotKetId = MAppRegShowHotKey(Handle, (uint)(int)kv1, (uint)(int)kv2);
                MAppWorkCall3(209, Handle, IntPtr.Zero);
            }
        }

        private void LoadListColumnsWidth()
        {
            string s = GetConfig("ListStartsWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < listStartup.Colunms.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        listStartup.Colunms[i].Width = width;
                }
            }
            s = GetConfig("ListUWPsWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < listUwpApps.Colunms.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        listUwpApps.Colunms[i].Width = width;
                }
            }
            s = GetConfig("ListUsersWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < listUsers.Colunms.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        listUsers.Colunms[i].Width = width;
                }
            }
            s = GetConfig("ListDriversWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < listDrivers.Columns.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        listDrivers.Columns[i].Width = width;
                }
            }
            s = GetConfig("ListServiceWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < listService.Columns.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        listService.Columns[i].Width = width;
                }
            }
        }
        private void SaveListColumnsWidth()
        {
            string s = "";
            foreach (TaskMgrListHeaderItem he in listStartup.Colunms)
                s += "#" + he.Width;
            SetConfig("ListStartsWidths", "AppSetting", s);
            s = "";
            foreach (TaskMgrListHeaderItem he in listUwpApps.Colunms)
                s += "#" + he.Width;
            SetConfig("ListUWPsWidths", "AppSetting", s);
            s = "";
            foreach (TaskMgrListHeaderItem he in listUsers.Colunms)
                s += "#" + he.Width;
            SetConfig("ListUsersWidths", "AppSetting", s);
            s = "";
            foreach (ColumnHeader he in listDrivers.Columns)
                s += "#" + he.Width;
            SetConfig("ListDriversWidths", "AppSetting", s);
            s = "";
            foreach (ColumnHeader he in listService.Columns)
                s += "#" + he.Width;
            SetConfig("ListServiceWidths", "AppSetting", s);
        }

        private void InitializeCtlText()
        {
            spBottom.Text = "底部分隔符";
            spl1.Text = "分隔符";
            sp5.Text = "分隔符";
            sp2.Text = "分隔符";
            sp3.Text = "分隔符";
            sp4.Text = "分隔符";
            pl_simple.Text = "应用简略信息视图";
            pl_perfGridHost.Text = "图形摘要视图控制项";
            listProcessDetals.Text = "进程详细信息列表";
            listService.Text = "服务列表视图";
            listDrivers.Text = "驱动列表视图";
            listFm.Text = "文件列表视图";
            treeFmLeft.Text = "文件夹树视图";
            splitContainerFm.Text = "文件管理视图";
            splitContainerFm.Panel1.Text = "文件夹树列表控制项";
            splitContainerFm.Panel2.Text = "文件列表控制项";
            splitContainerPerfCtls.Text = "资源监视控制项";
            splitContainerPerfCtls.Panel1.Text = "资源监视列表控制项";
            splitContainerPerfCtls.Panel2.Text = "资源监视页控制项";
            tabControlMain.Text = "主页面选项卡控制";


        }

        //notifyIcon
        private bool notifyIcon_mouseEntered = false;

        private void notifyIcon_MouseEnter(object sender, EventArgs e)
        {
            PerfSetTrayPos();
            ShowWindow(formSpeedBall.Handle, 5);
        }
        public void notifyIcon_MouseLeave(object sender, EventArgs e)
        {
            notifyIcon_mouseEntered = false;
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MAppWorkCall3(208, Handle, Handle);
        }
        private void notifyIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (!notifyIcon_mouseEntered)
            {
                notifyIcon_mouseEntered = true;
                notifyIcon_MouseEnter(sender, e);
            }
        }

        //notifyIcon menu
        private void 退出程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppExit();
        }
        private void 显示隐藏主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsWindowVisible(Handle))
                ShowWindow(Handle, 0);
            else
                ShowWindow(Handle, 5);
        }
        private void contextMenuStripTray_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsWindowVisible(Handle))
                显示隐藏主界面ToolStripMenuItem.Text = str_HideMain;
            else
                显示隐藏主界面ToolStripMenuItem.Text = str_ShowMain;
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            AppLoad();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {

            Text = GetConfig("Title", "AppSetting", "任务管理器");

            if (Text == "") Text = str_AppTitle;

            LoadHotKey();
            LoadLastPos();

            PerfInitTray();
        }
        private void FormMain_Activated(object sender, EventArgs e)
        {
            listUwpApps.FocusedType = true;
            listStartup.FocusedType = true;
            listProcess.FocusedType = true;
        }
        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            listUwpApps.FocusedType = false;
            listStartup.FocusedType = false;
            listProcess.FocusedType = false;
        }
        private void FormMain_OnWmCommand(int id)
        {
            switch (id)
            {
                //def in resource.h
                case 40005://Settings
                    {
                        new FormSettings(this).ShowDialog();
                        break;
                    }
                case 40034://Choose column
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                        {
                            WorkWindow.FormMainListHeaders f = new WorkWindow.FormMainListHeaders(this);
                            if (f.ShowDialog() == DialogResult.OK)
                                MAppWorkCall3(191, IntPtr.Zero, IntPtr.Zero);
                        }
                        else if (tabControlMain.SelectedTab == tabPageDetals)
                        {
                            new FormDetalsistHeaders().ShowDialog();
                        }
                        break;
                    }
                case 40017: //Sleep system
                    {
                        Application.SetSuspendState(PowerState.Suspend, true, true);
                        break;
                    }
                case 41174:
                    {
                        //Hibernate system
                        Application.SetSuspendState(PowerState.Hibernate, true, true);
                        break;
                    }
                case 41130:
                case 41012://Refesh
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                            ProcessListRefesh();
                        else if (tabControlMain.SelectedTab == tabPageKernelCtl)
                            KernelLisRefesh();
                        else if (tabControlMain.SelectedTab == tabPageStartCtl)
                            StartMListRefesh();
                        else if (tabControlMain.SelectedTab == tabPageScCtl)
                            ScMgrRefeshList();
                        else if (tabControlMain.SelectedTab == tabPageFileCtl)
                            FileMgrShowFiles(null);
                        else if (tabControlMain.SelectedTab == tabPageUWPCtl)
                            UWPListRefesh();
                        else if (tabControlMain.SelectedTab == tabPagePerfCtl)
                            BaseProcessRefeshTimer_Tick(null, null);
                        else if (tabControlMain.SelectedTab == tabPageDetals)
                            BaseProcessRefeshTimer_Tick(null, null);
                        else if (tabControlMain.SelectedTab == tabPageUsers)
                            UsersListLoad();
                        break;
                    }
                case 40019://Reboot
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleReboot"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41020://Logoff
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleLogoOff"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(186, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 40018://Shutdown
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleShutdown"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41151://FShutdown
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleFShutdown"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(201, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41152://FRebbot
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleFRebbot"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(202, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41153://Test2
                    {
                        //MCller
                        break;
                    }

            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (close_hide)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            GetConfig("SplitterDistancePerf", "AppSetting", splitContainerPerfCtls.SplitterDistance.ToString());
            SetConfigBool("MainGrouping", "AppSetting", listProcess.ShowGroup);
            SetConfig("ListSortIndex", "AppSetting", sortitem.ToString());
            if (sorta) SetConfig("ListSortDk", "AppSetting", "TRUE");
            else SetConfig("ListSortDk", "AppSetting", "FALSE");
            if (!isSimpleView)
                SetConfig("OldSize", "AppSetting", Width.ToString() + "-" + Height.ToString());
            else SetConfig("OldSize", "AppSetting", lastSize.Width.ToString() + "-" + lastSize.Height.ToString());
            SetConfig("OldPos", "AppSetting", Left.ToString() + "-" + Top.ToString());
            SetConfigBool("OldIsMax", "AppSetting", WindowState == FormWindowState.Maximized);
            SetConfigBool("RamPercentage", "Configure", isRamPercentage);
            SetConfigBool("DiskPercentage", "Configure", isDiskPercentage);
            SetConfigBool("NetPercentage", "Configure", isNetPercentage);

            if (saveheader)
            {
                string headers = "";
                for (int i = 1; i < listProcess.Header.SortedItems.Count; i++)
                    headers = headers + "#" + listProcess.Header.SortedItems[i].Identifier + "-" + listProcess.Header.SortedItems[i].Width;
                SetConfig("MainHeaders", "AppSetting", headers);
            }
            SetConfig("MainHeaders1", "AppSetting", listProcess.Colunms[0].Width.ToString());

            notifyIcon.Visible = false;

            AppOnExit();
        }
        private void FormMain_OnWmHotKey(int id)
        {
            if (id == showHideHotKetId)
            {
                if (!IsWindowVisible(Handle))
                    MAppWorkCall3(208, Handle, IntPtr.Zero);
            }
        }
        private void FormMain_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                listProcess.Locked = false;
                if (processListInited)
                    BaseProcessRefeshTimer_Tick(sender, e);
            }
            else
            {
                listProcess.Locked = true;
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_COMMAND)
                FormMain_OnWmCommand(m.WParam.ToInt32());
            else if (m.Msg == WM_HOTKEY)
                FormMain_OnWmHotKey(m.WParam.ToInt32());
            else if (m.Msg == WM_SYSCOMMAND)
            {
                if (min_hide && m.WParam.ToInt32() == 0xF20)//SC_MINIMIZE
                    Hide();
            }
            coreWndProc?.Invoke(m.HWnd, Convert.ToUInt32(m.Msg), m.WParam, m.LParam);
        }

        public static void AppHWNDSendMessage(uint message, IntPtr wParam, IntPtr lParam)
        {
            MAppWorkCall2(message, wParam, lParam);
        }

        #endregion

        private void tabControlMain_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPageProcCtl)
            {
                ProcessListInit();
            }
            else if (e.TabPage == tabPageScCtl)
            {
                ScMgrInit();
            }
            else if (e.TabPage == tabPageFileCtl)
            {
                FileMgrInit();
            }
            else if (e.TabPage == tabPageUWPCtl)
            {
                UWPListInit();
            }
            else if (e.TabPage == tabPagePerfCtl)
            {
                PerfInit();
            }
            else if (e.TabPage == tabPageStartCtl)
            {
                StartMListInit();
            }
            else if (e.TabPage == tabPageKernelCtl)
            {
                KernelListInit();
            }
            else if (e.TabPage == tabPageDetals)
            {
                ProcessListDetailsInit();
            }
            else if (e.TabPage == tabPageUsers)
            {
                UsersListInit();
            }
        }


    }
}
