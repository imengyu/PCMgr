using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TaskMgr.Aero.TaskDialog;
using TaskMgr.Ctls;

namespace TaskMgr
{
    public partial class FormMain : Form
    {
        public const string COREDLLNAME = "PCMgr32.dll";
        public const string DEFAPPTITLE = "PC Manager";

        public FormMain()
        {
            InitializeComponent();
            baseProcessRefeshTimer.Interval = 2000;
            baseProcessRefeshTimer.Tick += BaseProcessRefeshTimer_Tick;
            listProcess.Header.CloumClick += Header_CloumClick;
        }

        #region Config
        public static bool SetConfig(string configkey, string configSection, string configData)
        {
            long OpStation = WritePrivateProfileString(configSection, configkey, configData, Application.StartupPath + "\\" + currentProcessName + ".ini");
            return (OpStation != 0);
        }
        public static string GetConfig(string configkey, string configSection)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(configSection, configkey, "", temp, 1024, Application.StartupPath + "\\" + currentProcessName + ".ini");
            return temp.ToString();
        }
        #endregion

        #region API S

        /*struct EXEPROFENCE
        {
            int state;
            double cpu;
            ulong ram;
            int disk;
            int internet;
            long cputime;
        }*/

        const int MB_OK = 0x00000000;
        const int MB_OKCANCEL = 0x00000001;
        const int MB_ABORTRETRYIGNORE = 0x00000002;
        const int MB_YESNOCANCEL = 0x00000003;
        const int MB_YESNO = 0x00000004;
        const int MB_RETRYCANCEL = 0x00000005;
        const int MB_ICONHAND = 0x00000010;
        const int MB_ICONQUESTION = 0x00000020;
        const int MB_ICONEXCLAMATION = 0x00000030;
        const int MB_ICONASTERISK = 0x00000040;
        const int MB_ICONWARNING = MB_ICONEXCLAMATION;
        const int MB_ICONERROR = MB_ICONHAND;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);
        [DllImport("user32")]
        private static extern bool IsHungAppWindow(IntPtr hWnd);

        private int TaskDialogCallback(IntPtr hwnd, string text, string title, string apptl, int ico, int button)
        {
            TaskDialogIcon tico = TaskDialogIcon.None;
            if (ico == MB_ICONERROR)
                tico = TaskDialogIcon.Stop;
            else if (ico == MB_ICONWARNING)
                tico = TaskDialogIcon.Warning;
            else if (ico == MB_ICONASTERISK)
                tico = TaskDialogIcon.Information;

            TaskDialogButton tbtn = TaskDialogButton.OK;
            if (button == MB_OK)
                tbtn = TaskDialogButton.OK;
            else if (button == MB_OKCANCEL)
                tbtn = TaskDialogButton.OK | TaskDialogButton.Cancel;
            else if (button == MB_YESNO)
                tbtn = TaskDialogButton.Yes | TaskDialogButton.No;
            else if (button == MB_YESNOCANCEL)
                tbtn = TaskDialogButton.Yes | TaskDialogButton.No | TaskDialogButton.Cancel;
            else if (button == MB_ABORTRETRYIGNORE)
                tbtn = TaskDialogButton.Retry | TaskDialogButton.Cancel;

            TaskDialog t = new TaskDialog(apptl, title, text, tbtn, tico);
            return t.Show(hwnd).ButtonID;
        }

        WNDPROC coreWndProc = null;
        EXITCALLBACK exitCallBack;
        EnumProcessCallBack enumProcessCallBack;
        taskdialogcallback taskDialogCallBack;
        EnumWinsCallBack enumWinsCallBack;
        EnumWinsCallBack getWinsCallBack;

        private delegate long WNDPROC(IntPtr hWnd, uint msg, IntPtr lParam, IntPtr wParam);
        private delegate void EXITCALLBACK();
        private delegate void EnumProcessCallBack(int pid, int ppid, IntPtr name, IntPtr exefullpath, int tp);
        private delegate int taskdialogcallback(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)]string text, [MarshalAs(UnmanagedType.LPWStr)] string title, [MarshalAs(UnmanagedType.LPWStr)]string apptl, int ico, int button);
        private delegate void EnumWinsCallBack(IntPtr hWnd, IntPtr hWndParent);

        //[DllImport(COREDLLNAME, EntryPoint = "MGetExeProfenceInfo")]
        //private static extern EXEPROFENCE MGetExeProfenceInfo(long dwPId, int intervalTime, ulong lastcputime);
        [DllImport(COREDLLNAME, EntryPoint = "MAppVProcessAllWindowsGetProcessWindow")]
        private static extern bool MAppVProcessAllWindowsGetProcessWindow(long pid);
        [DllImport(COREDLLNAME, EntryPoint = "MGetPrivileges2")]
        private static extern bool MGetPrivileges2();
        [DllImport(COREDLLNAME, EntryPoint = "MGetPrivileges")]
        private static extern bool MGetPrivileges();
        [DllImport(COREDLLNAME, EntryPoint = "MEnumProcess")]
        private static extern void MEnumProcess(IntPtr callback);
        [DllImport(COREDLLNAME, EntryPoint = "MAppSetCallBack")]
        public static extern IntPtr MAppSetCallBack(IntPtr ptr, int id);
        [DllImport(COREDLLNAME)]
        public static extern int MAppWorkCall3(int id, IntPtr hWnd, IntPtr data);
        [DllImport(COREDLLNAME, EntryPoint = "MAppExit")]
        private static extern void MAppExit();
        [DllImport(COREDLLNAME, EntryPoint = "MAppRebot")]
        private static extern void MAppRebot();
        [DllImport(COREDLLNAME, EntryPoint = "MIs64BitOS")]
        public static extern bool MIs64BitOS();
        [DllImport(COREDLLNAME, EntryPoint = "MGetExeState")]
        private static extern int MGetExeState(long dwPID, IntPtr hwnd);
        [DllImport(COREDLLNAME, EntryPoint = "MGetProcessFullPathEx", CharSet = CharSet.Unicode)]
        private static extern string MGetProcessFullPathEx(long dwPID);
        [DllImport(COREDLLNAME, EntryPoint = "MGetExeInfo", CharSet = CharSet.Unicode)]
        private static extern bool MGetExeInfo(string strFilePath, string InfoItem, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MGetExeDescribe", CharSet = CharSet.Unicode)]
        private static extern bool MGetExeDescribe(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MGetExeCompany", CharSet = CharSet.Unicode)]
        private static extern bool MGetExeCompany(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MGetExeIcon", CharSet = CharSet.Unicode)]
        private static extern IntPtr MGetExeIcon(string pszFullPath);
        [DllImport(COREDLLNAME, EntryPoint = "MGetCpuUseAge")]
        private static extern double MGetCpuUseAge();
        [DllImport(COREDLLNAME, EntryPoint = "MGetRamUseAge")]
        private static extern double MGetRamUseAge();
        [DllImport(COREDLLNAME, EntryPoint = "MGetDiskUseAge")]
        private static extern double MGetDiskUseAge();
        [DllImport(COREDLLNAME, EntryPoint = "MAppWorkShowMenuProcess")]
        private static extern int MAppWorkShowMenuProcess([MarshalAs(UnmanagedType.LPWStr)]string strFilePath, [MarshalAs(UnmanagedType.LPWStr)]string strFileName, long pid, IntPtr hWnd, int data);
        [DllImport(COREDLLNAME, EntryPoint = "MGetExeRam")]
        private static extern ulong MGetExeRam(long pid);
        [DllImport(COREDLLNAME, EntryPoint = "MGetAllRam")]
        private static extern ulong MGetAllRam();
        [DllImport(COREDLLNAME, CharSet = CharSet.Unicode)]
        private static extern bool MGetProcessCommandLine(long pid, StringBuilder b, int m);
        [DllImport(COREDLLNAME)]
        private static extern bool MAppVProcess(IntPtr hWnd);
        #endregion

        #region ProcessListWork
        private int sortitem = -1;
        private bool sorta = false;
        private bool isFirstLoad = true;
        private Timer baseProcessRefeshTimer = new Timer();
        public static string currentProcessName = "";
        private ListViewColumnSorter lvwColumnSorter = new ListViewColumnSorter();
        private class psitem
        {
            public int pid;
            public bool isvalidid = false;
            public ulong lastcoutime = 0;
        }
        private struct pstag
        {
            public long pid;
            public string exename;
            public string exepath;
        }
        private bool isSelectExplorer = false;
        private List<psitem> loadedPs = new List<psitem>();
        private List<string> windowsProcess = new List<string>();
        private long selectedpid = 0;
        private Font smallListFont = new Font("微软雅黑", 9f);
        private TaskMgrListItem thisLoadItem = null;

        private bool IsWindowsrocess(string str)
        {
            bool rs = false;
            foreach(string s in windowsProcess)
            {
                if (s == str)
                {
                    rs = true;
                    break;
                }
            }
            return rs;
        }
        private void MainGetWinsCallBack(IntPtr hWnd, IntPtr data)
        {
            if (thisLoadItem != null)
            {
                if (WorkWindow.FormSpyWindow.IsWindow(hWnd))
                {
                    //if (WorkWindow.FormSpyWindow.IsWindowVisible(hWnd))
                    //{


                    IntPtr icon = WorkWindow.FormSpyWindow.MGetWindowIcon(hWnd);
                    TaskMgrListItemChild c = new TaskMgrListItemChild(Marshal.PtrToStringAuto(data), icon != IntPtr.Zero ? Icon.FromHandle(icon) : Properties.Resources.icoShowedWindow);
                    c.Tag = hWnd;
                    thisLoadItem.Childs.Add(c);

                    //}
                }
            }
        }
        private void MainEnumWinsCallBack(IntPtr hWnd, IntPtr hWndParent)
        {
            WorkWindow.FormSpyWindow f = new WorkWindow.FormSpyWindow(hWnd);
            Control fp = FromHandle(hWndParent);
            f.ShowDialog(fp);
        }
        private TaskMgrListItem ProcessListFindItem(int pid)
        {
            TaskMgrListItem rs = null;
            foreach (TaskMgrListItem r in listProcess.Items)
            {
                if (r.PID == pid)
                {
                    rs = r;
                    break;
                }
            }
            return rs;
        }
        private bool ProcessListIsProcessLoaded(int pid)
        {
            bool rs = false;
            foreach (psitem f in loadedPs)
            {
                if (f.pid == pid)
                {
                    rs = true;
                    break;
                }
            }
            return rs;
        }
        private void ProcessListInit()
        {
            ProcessListPrepareClear();
            listProcess.Locked = true;
            MEnumProcess(Marshal.GetFunctionPointerForDelegate(enumProcessCallBack));
        }
        private void ProcessListLoad(int pid, int ppid, string exename, string exefullpath)
        {
            psitem p = new psitem();
            p.pid = pid;
            p.isvalidid = true;
            loadedPs.Add(p);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(exefullpath);
            TaskMgrListItem taskMgrListItem;
            if (pid == 0) taskMgrListItem = new TaskMgrListItem("系统空闲进程");
            else if (pid == 4) taskMgrListItem = new TaskMgrListItem("System");
            else if (stringBuilder.ToString() != "")
            {
                StringBuilder exeDescribe = new StringBuilder(256);
                if (MGetExeDescribe(stringBuilder.ToString(), exeDescribe, 256))
                    taskMgrListItem = new TaskMgrListItem(exeDescribe.ToString());
                else taskMgrListItem = new TaskMgrListItem(exename);
            }
            else taskMgrListItem = new TaskMgrListItem(exename);

            if (exefullpath == @"C:\Windows\System32\svchost.exe" || exename == "svchost.exe")
                taskMgrListItem.Icon = Properties.Resources.icoServiceHost;
            else
            {
                IntPtr intPtr = MGetExeIcon(stringBuilder.ToString());
                if (intPtr != IntPtr.Zero) taskMgrListItem.Icon = Icon.FromHandle(intPtr);
            }

            pstag t = new TaskMgr.FormMain.pstag();
            t.exename = exename;
            t.pid = pid;
            t.exepath = stringBuilder.ToString();
            taskMgrListItem.Tag = t;
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");
            taskMgrListItem.SubItems.Add("");

            if (nameindex != -1)
            {
                if (pid == 0) taskMgrListItem.SubItems[nameindex].Text = "系统空闲进程";
                else if (pid == 4) taskMgrListItem.SubItems[nameindex].Text = "ntoskrnl.exe";
                else taskMgrListItem.SubItems[nameindex].Text = exename;
            }
            if (pidindex != -1)
                taskMgrListItem.SubItems[pidindex].Text = pid.ToString();
            if (pathindex != -1)
                if (stringBuilder.ToString() != "")
                    taskMgrListItem.SubItems[pathindex].Text = stringBuilder.ToString();
            if (cmdindex != -1)
            {
                StringBuilder s = new StringBuilder(260);
                if (MGetProcessCommandLine(pid, s, 260))
                    taskMgrListItem.SubItems[cmdindex].Text = s.ToString();
            }
            if (companyindex != -1)
            {
                if (stringBuilder.ToString() != "")
                {
                    StringBuilder exeCompany = new StringBuilder(256);
                    if (MGetExeCompany(stringBuilder.ToString(), exeCompany, 256)) taskMgrListItem.SubItems[companyindex].Text = exeCompany.ToString();
                }
            }

            for (int i = 1; i < taskMgrListItem.SubItems.Count; i++)
                taskMgrListItem.SubItems[i].Font = smallListFont;

            thisLoadItem = taskMgrListItem;
            MAppVProcessAllWindowsGetProcessWindow(pid);
            thisLoadItem = null;

            if (taskMgrListItem.Childs.Count > 0)
                taskMgrListItem.Group = listProcess.Groups[0];
            else if (pid == 0 || pid == 4 || IsWindowsrocess(exefullpath))
                taskMgrListItem.Group = listProcess.Groups[2];
            else taskMgrListItem.Group = listProcess.Groups[1];

            taskMgrListItem.PID = pid;
            listProcess.Items.Add(taskMgrListItem);
            ProcessListUpdate(pid, ppid, exename, exefullpath, true, p);
        }
        private void ProcessListUpdate(int pid, int ppid, string name, string exefullpath, bool isload = false, psitem item=null)
        {
            psitem thisitem = null;
            if (!isload)
            {
                foreach (psitem f in loadedPs)
                {
                    if (f.pid == pid)
                    {
                        thisitem = f;
                        break;
                    }
                }
                if (thisitem == null)
                    return;
                thisitem.isvalidid = true;
            }
            else thisitem = item;

            TaskMgrListItem it = ProcessListFindItem(pid);
            if (it.Childs.Count > 0)
            {
                for (int i = it.Childs.Count - 1; i >= 0; i--)
                {
                    IntPtr h = (IntPtr)it.Childs[i].Tag;
                    if (!WorkWindow.FormSpyWindow.IsWindow(h))
                        it.Childs.Remove(it.Childs[i]);
                }
                if (it.Childs.Count == 0)
                    it.Group = listProcess.Groups[1];
            }
            else
            {
                if (!isload)
                {
                    thisLoadItem = it;
                    MAppVProcessAllWindowsGetProcessWindow(pid);
                    thisLoadItem = null;

                    if (it.Childs.Count > 0)
                        it.Group = listProcess.Groups[0];
                    else if (pid == 0 || pid == 4 || IsWindowsrocess(exefullpath))
                        it.Group = listProcess.Groups[2];
                    else it.Group = listProcess.Groups[1];
                }
            }


            if(stateindex!=-1)
            {
                int i = MGetExeState(pid,IntPtr.Zero);
                if (i == 1)
                {
                    it.SubItems[stateindex].Text = "";
                    if (it.Childs.Count > 0)
                        if (IsHungAppWindow((IntPtr)it.Childs[0].Tag))
                        {
                            it.SubItems[stateindex].Text = "无响应";
                            //.SubItems[stateindex].ForeColor = Color.FromArgb(219, 107, 58);
                        }
                }
                else if (i == 2)
                {
                    it.SubItems[stateindex].Text = "已暂停";
                    it.SubItems[stateindex].ForeColor = Color.FromArgb(22, 158, 250);
                }
            }
            /*if (cpuindex != -1)
            {
                it.SubItems[cpuindex].BackColor = Color.FromArgb(255, 249, 228);

            }*/
            if (ramindex != -1)
            {
                ulong l = MGetExeRam(pid);
                if (l == 0) it.SubItems[ramindex].Text = "-";
                else
                {
                    double i = l / (double)1024;
                    if (i < 0.1) i = 0.1;
                    it.SubItems[ramindex].Text = i.ToString("0.0") + "MB";
                }
                //it.SubItems[ramindex].Text = l + "K";
            }

            //EXEPROFENCE exeinfo =
            //MGetExeProfenceInfo(pid, baseProcessRefeshTimer.Interval, thisitem.lastcoutime);


        }
        private Color ProcessListGetColorFormValue(double v, double maxv)
        {
            double d = v / maxv;
            if (d <= 0)
                return Color.FromArgb(255, 244, 196);
            else if (d > 0 && d <= 0.1)
                return Color.FromArgb(255, 228, 135); 
            else if (d > 0.1 && d <= 0.3)
                return Color.FromArgb(249, 236, 168);
            else if (d > 0.3 && d <= 0.6)
                return Color.FromArgb(255, 198, 61);
            else if (d > 0.6 && d <= 0.8)
                return Color.FromArgb(252, 184, 22);
            else if (d > 0.8 && d <= 0.9)
                return Color.FromArgb(255, 167, 29);
            else if (d > 0.9)
                return Color.FromArgb(255, 160, 19);
            return Color.FromArgb(255, 249, 228);
        }
        private void ProcessListPrepareClear()
        {
            for (int i = 0; i < loadedPs.Count; i++)
            {
                loadedPs[i].isvalidid = false;
            }
        }
        private void ProcessListClear()
        {
            for (int i = loadedPs.Count - 1; i >= 0; i--)
            {
                if (!loadedPs[i].isvalidid)
                {
                    TaskMgrListItem li = ProcessListFindItem(loadedPs[i].pid);
                    loadedPs.Remove(loadedPs[i]);
                    if (li != null) listProcess.Items.Remove(li);
                }
            }
        }
        private void ProcessListHandle(int pid, int ppid, IntPtr name, IntPtr exefullpath, int tp)
        {
            if (tp == 1)
            {
                if (ProcessListIsProcessLoaded(pid))
                    ProcessListUpdate(pid, ppid, Marshal.PtrToStringAuto(name), Marshal.PtrToStringAuto(exefullpath));
                else ProcessListLoad(pid, ppid, Marshal.PtrToStringAuto(name), Marshal.PtrToStringAuto(exefullpath));
            }
            else if (tp == 0)
            {
                ProcessListClear();
                lbProcessCount.Text = "进程数：" + pid;                          
                //if (isClickRefesh) { TaskDialog.Show("刷新成功。", DEFAPPTITLE, ""); isClickRefesh = false; }
                if(isFirstLoad)
                {
                    if (sortitem < listProcess.Header.Items.Count && sortitem >= 0)
                    {
                        lvwColumnSorter.Order = sorta ? SortOrder.Ascending : SortOrder.Descending;
                        lvwColumnSorter.SortColumn = sortitem;
                        listProcess.Header.Items[sortitem].ArrowType = sorta ? TaskMgrListHeaderSortArrow.Ascending : TaskMgrListHeaderSortArrow.Descending;
                        listProcess.Header.Invalidate();
                        listProcess.ListViewItemSorter = lvwColumnSorter;
                        listProcess.Sort();
                        if (sortitem == 0)
                            listProcess.ShowGroup = true;
                        else listProcess.ShowGroup = false;
                    }
                    isFirstLoad = false;
                }
                listProcess.Locked = false;
                listProcess.SyncItems(true);
            }
        }

        private void BaseProcessRefeshTimer_Tick(object sender, EventArgs e)
        {
            listProcess.Locked = true;
            if (cpuindex != -1)
                listProcess.Colunms[cpuindex].TextBig = ((int)(MGetCpuUseAge())) + "%";
            if (ramindex != -1)
                listProcess.Colunms[ramindex].TextBig = ((int)(MGetRamUseAge() * 100)) + "%";
            //if (diskindex != -1)
            //listProcess.Colunms[diskindex].TextBig = (MGetDiskUseAge() * 100).ToString("00") + "%";
            //ProcessListInit();
            listProcess.Locked = false;
            listProcess.Header.Invalidate();
            //listProcess.Invalidate();
        }

        #region ListEvents

        private void listProcess_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listProcess.SelectedItem != null && listProcess.SelectedChildItem == null)
                {
                    pstag t = (pstag)listProcess.SelectedItem.Tag;
                    int rs = MAppWorkShowMenuProcess(t.exepath,
                        t.exename,
                        selectedpid, Handle, isSelectExplorer ? 1 : 0);
                }
                else if (listProcess.SelectedChildItem != null)
                {
                    int rs = MAppWorkCall3(189, Handle, (IntPtr)listProcess.SelectedChildItem.Tag);
                }
            }
        }
        private void listProcess_MouseDown(object sender, MouseEventArgs e)
        {
            if (listProcess.SelectedItem != null)
            {
                pstag t = (pstag)listProcess.SelectedItem.Tag;
                selectedpid = t.pid;
                if (selectedpid > 4)
                {
                    btnEndProcess.Enabled = true;
                    if (nameindex != -1)
                        if (t.exename == "explorer.exe")
                        { btnEndProcess.Text = "重新启动(E)"; isSelectExplorer = true; }
                        else { btnEndProcess.Text = "结束进程(E)"; isSelectExplorer = false; }
                }
                else btnEndProcess.Enabled = false;
            }
            else btnEndProcess.Enabled = false;
        }
        private void listProcess_SelectItemChanged(object sender, EventArgs e)
        {

        }
        private void Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left && e.MouseEventArgs.Clicks == 1)
            {
                if (e.Index != 2)
                {
                    listProcess.Locked = true;
                    if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.None)
                    {
                        lvwColumnSorter.Order = SortOrder.None;
                        sortitem = -1;
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
            }
        }
        private void btnEndProcess_Click(object sender, EventArgs e)
        {
            MAppWorkCall3(190, Handle, IntPtr.Zero);
        }
        #endregion

        #region Headers
        public class itemheader
        {
            public itemheader(int index, string name,int wi)
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
        public bool saveheader = true;
        List<itemheader> headers = new List<itemheader>();
        int currHeaderI = 0;
        private void listProcessAddHeader(string name, int width)
        {
            headers.Add(new itemheader(currHeaderI, name, width));
            currHeaderI++;
            TaskMgrListHeaderItem li = new TaskMgrListHeaderItem();
            li.TextSmall = name;
            li.Width = width;
            listProcess.Colunms.Add(li);
        }
        private int listProcessGetListIndex(string name)
        {
            int rs = -1;
            for (int i = 0; i < headers.Count; i++)
            {
                if (headers[i].name == name)
                {
                    if (headers[i].show)
                    {
                        rs = headers[i].index;
                    }
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

        ulong allRam = 0;
        ulong ramtop = 0;
        int nameindex = 0;
        int companyindex = 0;
        int stateindex = 0;
        int pidindex = 0;
        int cpuindex = 0;
        int ramindex = 0;
        int diskindex = 0;
        int netindex = 0;
        int pathindex = 0;
        int cmdindex = 0;
        #endregion

        #endregion

        private void AppLoad()
        {
            if (!MGetPrivileges()) TaskDialog.Show("提权失败！", DEFAPPTITLE, "", TaskDialogButton.OK, TaskDialogIcon.Warning);

            #region GetSettings
            string sortamxx = GetConfig("ListSortDk", "AppSetting");
            if (sortamxx != "")
                if (sortamxx == "TRUE")
                    sorta = true;
            string sortitemxx = GetConfig("ListSortIndex", "AppSetting");
            if (sortitemxx != "" && sortitemxx != "-1")
            {
                try
                {
                    sortitem = int.Parse(sortitemxx);
                }
                catch { }
            }

            #endregion

            #region LoadList
            TaskMgrListGroup lg = new TaskMgrListGroup("应用");
            listProcess.Groups.Add(lg);
            TaskMgrListGroup lg2 = new TaskMgrListGroup("后台进程");
            listProcess.Groups.Add(lg2);
            TaskMgrListGroup lg3 = new TaskMgrListGroup("Windows进程");
            listProcess.Groups.Add(lg3);
            TaskMgrListGroup lg4 = new TaskMgrListGroup("危险进程");
            listProcess.Groups.Add(lg4);

            listDrivers.Header.Height = 36;
            TaskMgrListHeaderItem li = new TaskMgrListHeaderItem();
            li.TextSmall = "驱动名称";
            li.Width = 200;
            listDrivers.Colunms.Add(li);
            TaskMgrListHeaderItem li1 = new TaskMgrListHeaderItem();
            li1.TextSmall = "基地址";
            li1.Width = 70;
            listDrivers.Colunms.Add(li1);
            TaskMgrListHeaderItem li2 = new TaskMgrListHeaderItem();
            li2.TextSmall = "大小";
            li2.Width = 80;
            listDrivers.Colunms.Add(li2);
            TaskMgrListHeaderItem li3 = new TaskMgrListHeaderItem();
            li3.TextSmall = "驱动对象";
            li3.Width = 80;
            listDrivers.Colunms.Add(li3);
            TaskMgrListHeaderItem li4 = new TaskMgrListHeaderItem();
            li4.TextSmall = "驱动路径";
            li4.Width = 250;
            listDrivers.Colunms.Add(li4);
            TaskMgrListHeaderItem li5 = new TaskMgrListHeaderItem();
            li5.TextSmall = "服务名";
            li5.Width = 70;
            listDrivers.Colunms.Add(li5);
            TaskMgrListHeaderItem li6 = new TaskMgrListHeaderItem();
            li6.TextSmall = "加载顺序";
            li6.Width = 50;
            listDrivers.Colunms.Add(li6);
            TaskMgrListHeaderItem li7 = new TaskMgrListHeaderItem();
            li7.TextSmall = "发布者";
            li7.Width = 150;
            listDrivers.Colunms.Add(li7);

            string s1 = GetConfig("MainHeaders1", "AppSetting");
            if (s1 != "")
                listProcessAddHeader("名称", int.Parse(s1));
            else listProcessAddHeader("名称", 200);
            string headers = GetConfig("MainHeaders", "AppSetting");
            if (headers.Contains("#"))
            {
                string[] headersv = headers.Split('#');
                for (int i = 0; i < headersv.Length; i++)
                {
                    if (headersv[i].Contains("-"))
                    {
                        string[] headersvx = headersv[i].Split('-');
                        listProcessAddHeader(headersvx[0], int.Parse(headersvx[1]));
                    }
                }
            }

            nameindex = listProcessGetListIndex("进程名称");
            companyindex = listProcessGetListIndex("发布者");
            stateindex = listProcessGetListIndex("状态");
            pidindex = listProcessGetListIndex("PID");
            cpuindex = listProcessGetListIndex("CPU");
            ramindex = listProcessGetListIndex("内存");
            diskindex = listProcessGetListIndex("磁盘");
            netindex = listProcessGetListIndex("网络");
            pathindex = listProcessGetListIndex("进程路径");
            cmdindex = listProcessGetListIndex("命令行");

            windowsProcess.Add(@"C:\Program Files\Windows Defender\NisSrv.exe");
            windowsProcess.Add(@"C:\Program Files\Windows Defender\MsMpEng.exe");
            windowsProcess.Add(@"C:\Windows\System32\svchost.exe");
            windowsProcess.Add(@"C:\Windows\System32\csrss.exe");
            windowsProcess.Add(@"C:\Windows\System32\conhost.exe");
            windowsProcess.Add(@"C:\Windows\System32\lsass.exe");
            windowsProcess.Add(@"‪C:\Windows\System32\sihost.exe");
            windowsProcess.Add(@"C:\Windows\System32\winlogon.exe");
            windowsProcess.Add(@"C:\Windows\System32\wininit.exe");
            windowsProcess.Add(@"C:\Windows\System32\smss.exe");
            windowsProcess.Add(@"C:\Windows\System32\services.exe");
            windowsProcess.Add(@"C:\Windows\System32\dwm.exe");
            /*windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");
            windowsProcess.Add(@"");*/
            #endregion

            allRam = MGetAllRam();
            ramtop = (ulong)((double)allRam * 0.2);

            ProcessListInit();

            #region Pos
            string s = GetConfig("OldSize", "AppSetting");
            string p = GetConfig("OldPos", "AppSetting");
            if (s.Contains("-"))
            {
                string[] ss = s.Split('-');
                Width = int.Parse(ss[0]);
                Height = int.Parse(ss[1]);
            }
            if (p.Contains("-"))
            {
                string[] pp = p.Split('-');
                Left = int.Parse(pp[0]);
                Top = int.Parse(pp[1]);
            }
            #endregion

            baseProcessRefeshTimer.Start();
        }
        private void AppExit()
        {
            Application.Exit();
        }

        #region FormEvent
        private void FormMain_Load(object sender, EventArgs e)
        {
            exitCallBack = AppExit;
            taskDialogCallBack = TaskDialogCallback;
            enumProcessCallBack = ProcessListHandle;
            enumWinsCallBack = MainEnumWinsCallBack;
            getWinsCallBack = MainGetWinsCallBack;
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(exitCallBack), 1);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(taskDialogCallBack), 2);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(enumWinsCallBack), 3);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(getWinsCallBack), 4);

            MAppWorkCall3(183, Handle, IntPtr.Zero);
            coreWndProc = (WNDPROC)Marshal.GetDelegateForFunctionPointer(MAppSetCallBack(IntPtr.Zero, 0), typeof(WNDPROC));
            AppLoad();
        }
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0111)
                FormMain_OnWmCommand(m.WParam.ToInt32());
            coreWndProc?.Invoke(m.HWnd, Convert.ToUInt32(m.Msg), m.WParam, m.LParam);
        }
        private void FormMain_Activated(object sender, EventArgs e)
        {
            listProcess.FocusedType = true;
        }
        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            listProcess.FocusedType = false;
        }
        private void FormMain_OnWmCommand(int id)
        {
            switch(id)
            {
                case 40034:
                    WorkWindow.FormMainListHeaders f = new WorkWindow.FormMainListHeaders(this);
                    if (f.ShowDialog() == DialogResult.OK)
                        MAppWorkCall3(191, IntPtr.Zero, IntPtr.Zero);
                    break;
                case 41012:
                    ProcessListInit();
                    break;
                case 40019:
                    {
                        TaskDialog t = new TaskDialog("您即将重启。", DEFAPPTITLE, "确定继续吗？", TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).ButtonID == (int)Result.Yes)
                            MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41020:
                    {
                        TaskDialog t = new TaskDialog("您即将注销。", DEFAPPTITLE, "确定继续吗？", TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).ButtonID == (int)Result.Yes)
                            MAppWorkCall3(186, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 40018:
                    {
                        TaskDialog t = new TaskDialog("您即将关机。", DEFAPPTITLE, "确定继续吗？", TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).ButtonID == (int)Result.Yes)
                            MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetConfig("ListSortIndex", "AppSetting", sortitem.ToString());
            if (sorta) SetConfig("ListSortDk", "AppSetting", "TRUE");
            else SetConfig("ListSortDk", "AppSetting", "FALSE");
            SetConfig("OldSize", "AppSetting", Width.ToString() + "-" + Height.ToString());
            SetConfig("OldPos", "AppSetting", Left.ToString() + "-" + Top.ToString());

            if (saveheader)
            {
                string headers = "";
                for (int i = 1; i < listProcess.Colunms.Count; i++)
                    headers = headers + "#" + listProcess.Colunms[i].TextSmall + "-" + listProcess.Colunms[i].Width;
                SetConfig("MainHeaders", "AppSetting", headers);
            }
            SetConfig("MainHeaders1", "AppSetting", listProcess.Colunms[0].Width.ToString());
        }


        #endregion

        private void lbShowDetals_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!MAppVProcess(Handle)) TaskDialog.Show("无法打开详细信息窗口", DEFAPPTITLE, "未知错误。", TaskDialogButton.OK, TaskDialogIcon.Stop);
        }
    }
}
