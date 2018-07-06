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


        //private bool showSystemProcess = false;
        private bool showHiddenFiles = false;

        #endregion

        #region API S

        #region MainApi

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
        EnumProcessCallBack2 enumProcessCallBack2;
        taskdialogcallback taskDialogCallBack;
        EnumWinsCallBack enumWinsCallBack;
        EnumWinsCallBack getWinsCallBack;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumProcessCallBack2(int pid);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long WNDPROC(IntPtr hWnd, uint msg, IntPtr lParam, IntPtr wParam);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EXITCALLBACK();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumProcessCallBack(int pid, int ppid, IntPtr name, IntPtr exefullpath, int tp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int taskdialogcallback(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)]string text, [MarshalAs(UnmanagedType.LPWStr)] string title, [MarshalAs(UnmanagedType.LPWStr)]string apptl, int ico, int button);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumWinsCallBack(IntPtr hWnd, IntPtr hWndParent);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MEnumProcessFree();
        //[DllImport(COREDLLNAME, EntryPoint = "MGetExeProfenceInfo")]
        //private static extern EXEPROFENCE MGetExeProfenceInfo(long dwPId, int intervalTime, ulong lastcputime);
        [DllImport(COREDLLNAME, EntryPoint = "MAppVProcessAllWindowsGetProcessWindow", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MAppVProcessAllWindowsGetProcessWindow(long pid);
        [DllImport(COREDLLNAME, EntryPoint = "MGetPrivileges2", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetPrivileges2();
        [DllImport(COREDLLNAME, EntryPoint = "MGetPrivileges", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetPrivileges();
        [DllImport(COREDLLNAME, EntryPoint = "MEnumProcess", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MEnumProcess(IntPtr callback);
        [DllImport(COREDLLNAME, EntryPoint = "MEnumProcess2", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MEnumProces2(IntPtr callback);
        [DllImport(COREDLLNAME, EntryPoint = "MAppSetCallBack", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppSetCallBack(IntPtr ptr, int id);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MAppWorkCall3(int id, IntPtr hWnd, IntPtr data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MAppExit")]
        private static extern void MAppExit();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MAppRebot")]
        private static extern void MAppRebot();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MIs64BitOS")]
        public static extern bool MIs64BitOS();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetExeState")]
        private static extern int MGetExeState(long dwPID, IntPtr hwnd);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetProcessFullPathEx", CharSet = CharSet.Unicode)]
        private static extern string MGetProcessFullPathEx(long dwPID);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetExeInfo", CharSet = CharSet.Unicode)]
        private static extern bool MGetExeInfo(string strFilePath, string InfoItem, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetExeDescribe", CharSet = CharSet.Unicode)]
        private static extern bool MGetExeDescribe(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetExeCompany", CharSet = CharSet.Unicode)]
        private static extern bool MGetExeCompany(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetExeIcon", CharSet = CharSet.Unicode)]
        private static extern IntPtr MGetExeIcon(string pszFullPath);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetCpuUseAge")]
        private static extern double MGetCpuUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetRamUseAge")]
        private static extern double MGetRamUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetDiskUseAge")]
        private static extern double MGetDiskUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MAppWorkShowMenuProcess")]
        private static extern int MAppWorkShowMenuProcess([MarshalAs(UnmanagedType.LPWStr)]string strFilePath, [MarshalAs(UnmanagedType.LPWStr)]string strFileName, long pid, IntPtr hWnd, int data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetExeRam")]
        private static extern ulong MGetExeRam(long pid);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MGetAllRam")]
        private static extern ulong MGetAllRam();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetProcessCommandLine(long pid, StringBuilder b, int m);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MAppVProcess(IntPtr hWnd);
        #endregion

        #region FM API

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            uint dwReserved0;
            uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MFCALLBACK(int msg, IntPtr lParam, IntPtr wParam);

        private MFCALLBACK fileMgrCallBack;

        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFileAttr", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MFM_GetFileAttr(uint attr, StringBuilder sb, int maxcount, ref bool bout);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFileTime", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MFM_GetFileTime(ref System.Runtime.InteropServices.ComTypes.FILETIME fILETIME, StringBuilder sb, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFileIcon", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr MFM_GetFileIcon([MarshalAs(UnmanagedType.LPWStr)] string fileExt, StringBuilder sb, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFolderIcon", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MFM_GetFolderIcon();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetMyComputerIcon", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MFM_GetMyComputerIcon();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_SetCallBack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MFM_SetCallBack(IntPtr cp);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetRoots", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MFM_GetRoots();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFolders", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_GetFolders([MarshalAs(UnmanagedType.LPWStr)] string filePath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFiles", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_GetFiles([MarshalAs(UnmanagedType.LPWStr)] string filePath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetMyComputerName", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MFM_GetMyComputerName();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_OpenFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_OpenFile([MarshalAs(UnmanagedType.LPWStr)] string filePath, IntPtr hWnd);
        [DllImport(COREDLLNAME, EntryPoint = "MAppWorkShowMenuFM", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppWorkShowMenuFM([MarshalAs(UnmanagedType.LPWStr)] string filePath, bool mutilSelect, int selectCount);
        [DllImport(COREDLLNAME, EntryPoint = "MAppWorkShowMenuFMF", CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppWorkShowMenuFMF([MarshalAs(UnmanagedType.LPWStr)] string filePath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_IsValidateFolderFileName", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_IsValidateFolderFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_CreateDir", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_CreateDir([MarshalAs(UnmanagedType.LPWStr)] string path);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_UpdateFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_UpdateFile([MarshalAs(UnmanagedType.LPWStr)] string fullPath, [MarshalAs(UnmanagedType.LPWStr)] string dirPath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_ReUpdateFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_ReUpdateFile([MarshalAs(UnmanagedType.LPWStr)] string fullPath, [MarshalAs(UnmanagedType.LPWStr)] string dirPath);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MFM_SetShowHiddenFiles(bool b);

        public static String FormatFileSize(Int64 fileSize)
        {
            if (fileSize < 0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                return string.Format("{0:########0.00} G", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1024 * 1024)
            {
                return string.Format("{0:####0.00} M", ((Double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.00} K", ((Double)fileSize) / 1024);
            }
            else
            {
                return string.Format("{0} b", fileSize);
            }
        }

        #endregion



        #endregion

        private bool processListInited = false;
        private bool driverListInited = false;
        private bool scListInited = false;
        private bool fileListInited = false;
        private bool startListInited = false;

        #region ProcessListWork
        private int sortitem = -1;
        private bool sorta = false;
        private bool isFirstLoad = true;
        private Timer baseProcessRefeshTimer = new Timer();
        public static string currentProcessName = "";
        private ListViewColumnSorter lvwColumnSorter = new ListViewColumnSorter();

        private class PsItem
        {
            public Ctls.TaskMgrListItem item = null;
            public int pid;
            public ulong lastcoutime = 0;
        }
        private struct PsTag
        {
            public long pid;
            public string exename;
            public string exepath;
        }
        private bool isSelectExplorer = false;
        private List<int> validPid = new List<int>();
        private List<PsItem> loadedPs = new List<PsItem>();
        private List<string> windowsProcess = new List<string>();
        private long selectedpid = 0;
        private Font smallListFont = new Font("微软雅黑", 9f);
        private TaskMgrListItem thisLoadItem = null;

        private void MainGetWinsCallBack(IntPtr hWnd, IntPtr data)
        {
            if (thisLoadItem != null)
            {
                if (WorkWindow.FormSpyWindow.IsWindow(hWnd))
                {
                    if (WorkWindow.FormSpyWindow.IsWindowVisible(hWnd))
                    {
                        IntPtr icon = WorkWindow.FormSpyWindow.MGetWindowIcon(hWnd);
                        TaskMgrListItemChild c = new TaskMgrListItemChild(Marshal.PtrToStringAuto(data), icon != IntPtr.Zero ? Icon.FromHandle(icon) : Properties.Resources.icoShowedWindow);
                        c.Tag = hWnd;
                        thisLoadItem.Childs.Add(c);
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

        private bool IsWindowsProcess(string str)
        {
            bool rs = false;
            foreach (string s in windowsProcess)
            {
                if (s == str)
                {
                    rs = true;
                    break;
                }
            }
            return rs;
        }
        private TaskMgrListItem ProcessListFindItem(int pid)
        {
            TaskMgrListItem rs = null;
            foreach (TaskMgrListItem i in listProcess.Items)
            {
                if (i.PID == pid)
                {
                    rs = i;
                    break;
                }
            }
            return rs;
        }
        private bool ProcessListIsProcessLoaded(int pid, out PsItem item)
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
        private void ProcessListInit()
        {
            if (!processListInited)
            {
                baseProcessRefeshTimer.Start();

                processListInited = true;
                ProcessListRefesh();
            }
        }
        private void ProcessListRefesh()
        {
            ProcessListPrepareClear();
            listProcess.Locked = true;
            MEnumProcess(Marshal.GetFunctionPointerForDelegate(enumProcessCallBack));
        }
        private void ProcessListRefesh2()
        {
            ProcessListPrepareClear();
            listProcess.Locked = true;
            MEnumProces2(Marshal.GetFunctionPointerForDelegate(enumProcessCallBack2));
            ProcessListClear();
            ProcessListUpdateValues();
            listProcess.Locked = false;
            listProcess.Invalidate();
        }
        private void ProcessListLoad(int pid, int ppid, string exename, string exefullpath)
        {
            PsItem p = new PsItem();
            p.pid = pid;
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
            p.item = taskMgrListItem;
            if (exefullpath == @"C:\Windows\System32\svchost.exe" || exename == "svchost.exe")
                taskMgrListItem.Icon = Properties.Resources.icoServiceHost;
            else
            {
                IntPtr intPtr = MGetExeIcon(stringBuilder.ToString());
                if (intPtr != IntPtr.Zero) taskMgrListItem.Icon = Icon.FromHandle(intPtr);
            }

            PsTag t = new PsTag
            {
                exename = exename,
                pid = pid,
                exepath = stringBuilder.ToString()
            };
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
            else if (pid == 0 || pid == 4 || IsWindowsProcess(exefullpath))
                taskMgrListItem.Group = listProcess.Groups[2];
            else taskMgrListItem.Group = listProcess.Groups[1];

            taskMgrListItem.PID = pid;
            listProcess.Items.Add(taskMgrListItem);
            ProcessListUpdate(pid, true, taskMgrListItem);
        }
        private void ProcessListUpdate(int pid, bool isload = false, TaskMgrListItem it=null)
        {
            //Child and group
            if (it.Childs.Count > 0)
            {
                for (int i = it.Childs.Count - 1; i >= 0; i--)
                {
                    IntPtr h = (IntPtr)it.Childs[i].Tag;
                    if (!WorkWindow.FormSpyWindow.IsWindow(h) || !WorkWindow.FormSpyWindow.IsWindowVisible(h))
                        it.Childs.Remove(it.Childs[i]);
                }
                if (it.Childs.Count == 0) it.Group = listProcess.Groups[1];
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
                    else if (pid == 0 || pid == 4 || IsWindowsProcess(((PsTag)it.Tag).exepath))
                        it.Group = listProcess.Groups[2];
                    else it.Group = listProcess.Groups[1];
                }
            }

            if (stateindex != -1)
            {
                int i = MGetExeState(pid, IntPtr.Zero);
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
            validPid.Clear();
        }
        private void ProcessListClear()
        {
            for (int i = loadedPs.Count - 1; i >= 0; i--)
            {
                if (!validPid.Contains(loadedPs[i].pid))
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
                validPid.Add(pid);
                PsItem item;
                if (ProcessListIsProcessLoaded(pid, out item))
                    ProcessListUpdate(pid, false, item.item);
                else ProcessListLoad(pid, ppid, Marshal.PtrToStringAuto(name), Marshal.PtrToStringAuto(exefullpath));
            }
            else if (tp == 0)
            {
                ProcessListClear();
                lbProcessCount.Text = "进程数：" + pid;
                if (isFirstLoad)
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
        private void ProcessListHandle2(int pid)
        {          
            validPid.Add(pid);
        }
        private void ProcessListUpdateValues()
        {
            for (int i = 0; i < listProcess.ShowedItems.Count; i++)
            {
                ProcessListUpdate(listProcess.ShowedItems[i].PID, false, listProcess.ShowedItems[i]);
            }
        }

        private void BaseProcessRefeshTimer_Tick(object sender, EventArgs e)
        {
            listProcess.Locked = true;
            if (cpuindex != -1)
                listProcess.Colunms[cpuindex].TextBig = ((int)(MGetCpuUseAge())) + "%";
            if (ramindex != -1)
                listProcess.Colunms[ramindex].TextBig = ((int)(MGetRamUseAge() * 100)) + "%";
            if (diskindex != -1)
                listProcess.Colunms[diskindex].TextBig = (MGetDiskUseAge() * 100).ToString("00") + "%";
            ProcessListRefesh2();
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
                    PsTag t = (PsTag)listProcess.SelectedItem.Tag;
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
                PsTag t = (PsTag)listProcess.SelectedItem.Tag;
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
        private void lbShowDetals_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!MAppVProcess(Handle)) TaskDialog.Show("无法打开详细信息窗口", DEFAPPTITLE, "未知错误。", TaskDialogButton.OK, TaskDialogIcon.Stop);
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

        #region FileMgrWork

        private Dictionary<string, string> fileTypeNames = new Dictionary<string, string>();
        private TreeNode lastClickTreeNode = null;
        private string lastShowDir = "";

        private void FileMgrInit()
        {
            if (!fileListInited)
            {
                fileListInited = true;

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
                        n.Nodes.Add("loading", "正在加载...", "loading", "loading");
                        break;
                    }
                case 3:
                    {
                        if (wParam.ToInt32() == -1)
                        {
                            lastClickTreeNode.Nodes[0].Text = "无法访问此文件夹";
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
                            n.Nodes.Add("loading", "正在加载...", "loading", "loading");
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
                            ListViewItem lvi = listFm.Items.Add("无法读取文件夹", "err");
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
                                if (!imageListFileTypeList.Images.ContainsKey(fpath) && File.Exists(fpath))
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
                        ListViewItem listViewItem = listFm.Items.Add("新建文件夹", "folder");
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
                        case 0: lbFileMgrStatus.Text = "就绪"; break;
                        case 1: {
                                if (listFm.SelectedItems.Count > 0)
                                    lbFileMgrStatus.Text = "就绪  共 " + listFm.Items.Count + " 个项目  选择了 " + listFm.SelectedItems.Count + " 个项目";
                                else lbFileMgrStatus.Text = "就绪  共 " + listFm.Items.Count + " 个项目";
                                break;
                            }
                        case 2: lbFileMgrStatus.Text = ""; break;
                        case 3: lbFileMgrStatus.Text = ""; break;
                        case 4: lbFileMgrStatus.Text = ""; break;
                        case 5: lbFileMgrStatus.Text = "文件已经剪切到剪贴板"; break;
                        case 6: lbFileMgrStatus.Text = "文件已经复制到剪贴板"; break;
                        case 7: lbFileMgrStatus.Text = "新建文件夹失败"; break;
                        case 8: lbFileMgrStatus.Text = "新建文件夹成功"; break;
                        case 9: lbFileMgrStatus.Text = "路径已经复制到剪贴板"; break;
                        case 10: lbFileMgrStatus.Text = "文件夹已经剪切到剪贴板"; break;
                        case 11: lbFileMgrStatus.Text = "文件夹已经复制到剪贴板"; break;
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

        private void textBoxFmCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnFmAddGoto_Click(sender, e);
        }
        private void btnFmAddGoto_Click(object sender, EventArgs e)
        {
            if (textBoxFmCurrent.Text == "")
                TaskDialog.Show("请输入要跳转的路径！", "提示");
            else if (Directory.Exists(textBoxFmCurrent.Text))
                FileMgrShowFiles(textBoxFmCurrent.Text);
            else if (File.Exists(textBoxFmCurrent.Text)) {
                string d = Path.GetDirectoryName(textBoxFmCurrent.Text);
                string f = Path.GetFileName(textBoxFmCurrent.Text);
                FileMgrShowFiles(d);
                ListViewItem[] lis = listFm.Items.Find(f, false);
                if (lis.Length > 0) lis[0].Selected = true;
            }
            else TaskDialog.Show("路径不存在！", "提示");
        }
        private void treeFmLeft_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 0 || e.Node.Nodes[0].Text == "正在加载..." && e.Node.Tag != null)
            {
                lastClickTreeNode = e.Node;
                string s = e.Node.Tag.ToString();
                if (MFM_GetFolders(s)) lastClickTreeNode.Nodes.Remove(lastClickTreeNode.Nodes[0]);
            }
        }
        private void treeFmLeft_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode n = treeFmLeft.SelectedNode;
            if (n != null && n.Tag != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    lastClickTreeNode = n;
                    FileMgrShowFiles(lastClickTreeNode.Tag.ToString());
                }
                else if (e.Button == MouseButtons.Right)
                {
                    MAppWorkShowMenuFMF(n.Tag.ToString());
                }
            }
        }

        private ListViewItem currEditingItem = null;
        private void listFm_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (currEditingItem != null && e.Item == 0)
            {
                string path = currEditingItem.Tag.ToString();
                string targetName = e.Label;
                //Folder
                if(path == "newfolder")
                {
                    if (targetName == "")
                    {
                        targetName = "新建文件夹";
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
                            TaskDialog.Show("指定的文件夹已经存在");
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
                        TaskDialog.Show("文件名无效");
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
                        else if (File.Exists(path))
                        {
                            if (TaskDialog.Show("您想打开此文件吗？", "疑问", "文件路径：" + path, TaskDialogButton.Yes | TaskDialogButton.No) == Result.Yes)
                                MFM_OpenFile(path, Handle);
                        }
                    }
                }
            }
        }
        private void listFm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listFm.SelectedItems.Count > 0)
                {
                    ListViewItem listViewItem = listFm.SelectedItems[0];
                    string path = listViewItem.Tag.ToString();
                    if (e.Button == MouseButtons.Right)
                        MAppWorkShowMenuFM(path, listFm.SelectedItems.Count > 1, listFm.SelectedItems.Count);
                }
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

        private void ScMgrInit()
        {
            if(!scListInited)
            {
                scListInited = true;
            }
        }

        private void linkOpenScMsc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MFM_OpenFile("services.msc", Handle);
        }



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
            showHiddenFiles = GetConfig("ShowHiddenFiles", "AppSetting")=="True";
            MFM_SetShowHiddenFiles(showHiddenFiles);
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

            ProcessListInit();
        }
        private void AppExit()
        {
            MEnumProcessFree();
            fileSystemWatcher.EnableRaisingEvents = false;

            Application.Exit();
        }

        #region FormEvent

        private void FormMain_Shown(object sender, EventArgs e)
        {
            AppLoad();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            exitCallBack = AppExit;
            taskDialogCallBack = TaskDialogCallback;
            enumProcessCallBack = ProcessListHandle;
            enumWinsCallBack = MainEnumWinsCallBack;
            getWinsCallBack = MainGetWinsCallBack;
            enumProcessCallBack2 = ProcessListHandle2;
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(exitCallBack), 1);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(taskDialogCallBack), 2);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(enumWinsCallBack), 3);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(getWinsCallBack), 4);

            MAppWorkCall3(183, Handle, IntPtr.Zero);
            coreWndProc = (WNDPROC)Marshal.GetDelegateForFunctionPointer(MAppSetCallBack(IntPtr.Zero, 0), typeof(WNDPROC));
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
                case 41130:
                case 41012:
                    ProcessListRefesh();
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
        }
    }
}
