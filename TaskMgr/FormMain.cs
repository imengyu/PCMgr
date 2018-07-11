using PCMgrUWP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TaskMgr.Aero.TaskDialog;
using TaskMgr.Ctls;
using TaskMgr.Helpers;

namespace TaskMgr
{
    public partial class FormMain : Form
    {
        public const string COREDLLNAME = "PCMgr32.dll";
        public const string DEFAPPTITLE = "任务管理器";

        public FormMain()
        {
            InitializeComponent();
            baseProcessRefeshTimer.Interval = 1000;
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

        #region Apis

        #region MainApi

        private const int MB_OK = 0x00000000;
        private const int MB_OKCANCEL = 0x00000001;
        private const int MB_ABORTRETRYIGNORE = 0x00000002;
        private const int MB_YESNOCANCEL = 0x00000003;
        private const int MB_YESNO = 0x00000004;
        private const int MB_RETRYCANCEL = 0x00000005;
        private const int MB_ICONHAND = 0x00000010;
        private const int MB_ICONQUESTION = 0x00000020;
        private const int MB_ICONEXCLAMATION = 0x00000030;
        private const int MB_ICONASTERISK = 0x00000040;
        private const int MB_ICONWARNING = MB_ICONEXCLAMATION;
        private const int MB_ICONERROR = MB_ICONHAND;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
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

        private WNDPROC coreWndProc = null;
        private EXITCALLBACK exitCallBack;
        private WORKERCALLBACK workerCallBack;
        private taskdialogcallback taskDialogCallBack;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void WORKERCALLBACK(int msg, IntPtr lParam, IntPtr wParam);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long WNDPROC(IntPtr hWnd, uint msg, IntPtr lParam, IntPtr wParam);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EXITCALLBACK();

        [DllImport(COREDLLNAME, EntryPoint = "MGetPrivileges2", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetPrivileges2();
        [DllImport(COREDLLNAME, EntryPoint = "MGetPrivileges", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetPrivileges();
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
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MIsRunasAdmin")]
        public static extern bool MIsRunasAdmin();
        public static bool MIsFinded64()
        {
            return File.Exists(Application.StartupPath + "\\PCMgr64.exe");
        }
        public static bool MRun64()
        {
            return MFM_OpenFile(Application.StartupPath + "\\PCMgr64.exe", IntPtr.Zero);
        }
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MAppRebotAdmin")]
        public static extern void MAppRebotAdmin();
        #endregion

        #region PROC API

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumProcessCallBack2(uint pid);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumProcessCallBack(uint pid, uint ppid, IntPtr name, IntPtr exefullpath, int tp, IntPtr hprocess);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int taskdialogcallback(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)]string text, [MarshalAs(UnmanagedType.LPWStr)] string title, [MarshalAs(UnmanagedType.LPWStr)]string apptl, int ico, int button);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumWinsCallBack(IntPtr hWnd, IntPtr hWndParent);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetWinsCallBack(IntPtr hWnd, IntPtr hWndParent, int i);

        private EnumProcessCallBack enumProcessCallBack;
        private EnumProcessCallBack2 enumProcessCallBack2;
        private EnumWinsCallBack enumWinsCallBack;
        private GetWinsCallBack getWinsCallBack;

        private IntPtr enumProcessCallBack_ptr;
        private IntPtr enumProcessCallBack2_ptr;

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MGetProcessState(uint dwPID, IntPtr hwnd);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetExeInfo(string strFilePath, string InfoItem, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetExeDescribe(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetExeCompany(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr MGetExeIcon(string pszFullPath);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MGetCpuUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MGetRamUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MGetDiskUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppWorkShowMenuProcess([MarshalAs(UnmanagedType.LPWStr)]string strFilePath, [MarshalAs(UnmanagedType.LPWStr)]string strFileName, long pid, IntPtr hWnd, int data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetProcessCommandLine(IntPtr handle, StringBuilder b, int m);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MAppVProcess(IntPtr hWnd);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MEnumProcess(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MEnumProcess2Refesh(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MEnumProcessFree();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MAppVProcessAllWindowsGetProcessWindow(long pid);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MAppVProcessAllWindowsGetProcessWindow2(long pid);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MCloseHandle(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MReUpdateProcess(uint pid, IntPtr callback);


        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MPERF_PerfDataCreate();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MPERF_PerfDataDestroy(IntPtr data);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MPERF_CpuTimeUpdate();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetProcessRam(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MPERF_GetProcessCpuUseAge(IntPtr handle, IntPtr perfdata);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetProcessDiskRate(IntPtr handle, IntPtr perfdata);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetProcessIsUWP(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetProcessIs32Bit(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetUWPPackageId(IntPtr handle, IntPtr data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetUWPPackageFullName(IntPtr handle, ref int len, StringBuilder buf);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void MAppVProcessAllWindowsUWP();


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
                return string.Format("{0:########0.00} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1024 * 1024)
            {
                return string.Format("{0:####0.00} MB", ((Double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.00} KB", ((Double)fileSize) / 1024);
            }
            else
            {
                return string.Format("{0} B", fileSize);
            }
        }
        public static String FormatFileSize1(Int64 fileSize)
        {
            if (fileSize < 0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                return string.Format("{0:########0.0} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1024 * 1024)
            {
                return string.Format("{0:####0.0} MB", ((Double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.0} KB", ((Double)fileSize) / 1024);
            }
            else
            {
                return string.Format("{0} B", fileSize);
            }
        }

        #endregion

        #region SC API

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumServicesCallBack(IntPtr dspName, IntPtr scName, uint scType, uint currentState, uint dwProcessId, bool syssc,
            uint dwStartType, IntPtr lpBinaryPathName, IntPtr lpLoadOrderGroup);

        private EnumServicesCallBack scMgrEnumServicesCallBack;
        private IntPtr scMgrEnumServicesCallBackPtr = IntPtr.Zero;

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MSCM_Init();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void MSCM_Exit();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MEnumServices(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void MSCM_ShowMenu(IntPtr hDlg,[MarshalAs(UnmanagedType.LPWStr)] string serviceName, uint running, uint startType, [MarshalAs(UnmanagedType.LPWStr)] string path);
        

        #endregion


        #endregion

        private bool processListInited = false;
        private bool driverListInited = false;
        private bool scListInited = false;
        private bool fileListInited = false;
        private bool startListInited = false;
        private bool uwpListInited = false;
        private bool perfInited = false;

        #region ProcessListWork
        private int sortitem = -1;
        private bool sorta = false;
        private bool isFirstLoad = true;
        private Timer baseProcessRefeshTimer = new Timer();
        public static string currentProcessName = "";
        private TaskListViewColumnSorter lvwColumnSorter = null;

        public PerformanceCounter performanceCounter_cpu_total = null;
        public PerformanceCounter performanceCounter_ram_total = null;
        public PerformanceCounter performanceCounter_disk_total = null;
        public PerformanceCounter performanceCounter_net_total = null;

        private class PsItem
        {
            public IntPtr perfData;
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

            public uwpitem uwpItem = null;
            public string uwpFullName;

            public List<ScItem> svcs = new List<ScItem>();
        }
        private class uwpitem
        {
            public TaskMgrListItemGroup uwpItem = null;
            public string uwpFullName = "";
        }
        private class uwpwinitem
        {
            public IntPtr hWnd = IntPtr.Zero;
            public string title = "";
        }

        private bool is64OS = false;
        private bool isSelectExplorer = false;
        private List<uint> validPid = new List<uint>();
        private List<uint> uwpHostPid = new List<uint>();
        private List<PsItem> loadedPs = new List<PsItem>();
        private List<uwpitem> uwps = new List<uwpitem>();
        private List<uwpwinitem> uwpwins = new List<uwpwinitem>();
        private List<string> windowsProcess = new List<string>();
        private long selectedpid = 0;
        private bool isRunAsAdmin = false;
        private bool firstLoad = true;
        private Font smallListFont = new Font("微软雅黑", 9f);
        private TaskMgrListItem thisLoadItem = null;

        private void MainGetWinsCallBack(IntPtr hWnd, IntPtr data, int i)
        {
            if (i == 1)
            {
                if (WorkWindow.FormSpyWindow.IsWindowVisible(hWnd))
                {
                    uwpwinitem item = new uwpwinitem();
                    item.hWnd = hWnd;
                    item.title = Marshal.PtrToStringAuto(data);
                    uwpwins.Add(item);
                }
            }
            else
            {
                if (thisLoadItem != null)
                {
                    if (((PsItem)thisLoadItem.Tag).exepath.ToLower() != @"c:\windows\system32\dwm.exe")
                    {
                        if (!thisLoadItem.HasChild(hWnd))
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
            if (str != null)
                return windowsProcess.Contains(str.ToLower());
            else return false;
        }

        private bool ProcessListGetUwpIsRunning(string dsbText)
        {
            bool rs = false;
            foreach(uwpwinitem u in uwpwins)
                if(u.title.Contains(dsbText))
                {
                    rs = true;
                    break;
                }
            return rs;
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

        private uwpitem ProcessListFindUWPItem(string fullName)
        {
            uwpitem rs = null;
            foreach (uwpitem i in uwps)
            {
                if (i.uwpFullName == fullName)
                {
                    rs = i;
                    break;
                }
            }
            return rs;
        }
        private TaskMgrListItem ProcessListFindItem(uint pid)
        {
            TaskMgrListItem rs = null;
            foreach (TaskMgrListItem i in listProcess.Items)
            {
                if (i is TaskMgrListItemGroup)
                {
                    TaskMgrListItemGroup ii = i as TaskMgrListItemGroup;
                    foreach (TaskMgrListItem ix in ii.Items)
                    {
                        if (ix.PID == pid)
                        {
                            rs = ix;
                            return rs;
                        }
                    }
                }
                else
                {
                    if (i.PID == pid)
                    {
                        rs = i;
                        return rs;
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

        private void ProcessListInit()
        {
            if (!processListInited)
            {
                enumProcessCallBack = ProcessListHandle;
                enumProcessCallBack2 = ProcessListHandle2;

                enumProcessCallBack_ptr = Marshal.GetFunctionPointerForDelegate(enumProcessCallBack);
                enumProcessCallBack2_ptr = Marshal.GetFunctionPointerForDelegate(enumProcessCallBack2);

                baseProcessRefeshTimer.Start();
                isRunAsAdmin = MIsRunasAdmin();

                performanceCounter_cpu_total = new PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);
                performanceCounter_ram_total = new PerformanceCounter("Memory", "% Committed Bytes In Use", "", true);
                performanceCounter_disk_total = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total", true);
                performanceCounter_net_total = new PerformanceCounter("Network Interface", "Bytes Total/sec", "", true);
                string[] instanceNames = new PerformanceCounterCategory(performanceCounter_net_total.CategoryName).GetInstanceNames();
                performanceCounter_net_total.InstanceName = instanceNames[0];

                if (!isRunAsAdmin)
                {
                    spl1.Visible = true;
                    check_showAllProcess.Visible = true;
                }

                windowsProcess.Add(@"C:\Program Files\Windows Defender\NisSrv.exe".ToLower());
                windowsProcess.Add(@"C:\Program Files\Windows Defender\MsMpEng.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\svchost.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\csrss.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\conhost.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\lsass.exe".ToLower());
                windowsProcess.Add(@"‪C:\Windows\System32\sihost.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\winlogon.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\wininit.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\smss.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\services.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\dwm.exe".ToLower());
                windowsProcess.Add(@"C:\Windows\System32\sihost.exe".ToLower());
                windowsProcess.Add(@"‪C:\Windows\explorer.exe".ToLower());
                windowsProcess.Add(@"‪explorer.exe".ToLower());
                /*
                windowsProcess.Add(@"".ToLower());
                windowsProcess.Add(@"".ToLower());       
                windowsProcess.Add(@"".ToLower());        
                windowsProcess.Add(@"".ToLower());   
                windowsProcess.Add(@"".ToLower());       
                windowsProcess.Add(@"".ToLower());      
                windowsProcess.Add(@"".ToLower());          
                windowsProcess.Add(@"".ToLower());         
                windowsProcess.Add(@"".ToLower());      
                windowsProcess.Add(@"".ToLower());      
                windowsProcess.Add(@"".ToLower());
                windowsProcess.Add(@"".ToLower());
                windowsProcess.Add(@"".ToLower());
                windowsProcess.Add(@"".ToLower());
                windowsProcess.Add(@"".ToLower());
                */

                processListInited = true;
                if (MIsRunasAdmin())
                {
                    ScMgrInit();
                }
                if (SysVer.IsWin8Upper())
                    UWPListInit();
                ProcessListRefesh();
            }
        }

        private void ProcessListRefesh()
        {
            uwps.Clear();
            uwpHostPid.Clear();
            uwpwins.Clear();

            if (SysVer.IsWin8Upper()) MAppVProcessAllWindowsUWP();

            ProcessListPrepareClear();
            listProcess.Locked = true;
            MEnumProcess(enumProcessCallBack_ptr);
        }
        private void ProcessListRefesh1Finished()
        {
            if (firstLoad) ProcessListLoadFinished();
            ProcessListClear();
            lbProcessCount.Text = "进程数：" + listProcess.Items.Count;
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
                    listProcess.SyncItems(false);
                    listProcess.Sort();
                }
                isFirstLoad = false;
            }
            listProcess.Locked = false;
            listProcess.SyncItems(true);
        }
        private void ProcessListRefesh2()
        {
            if (cpuindex != -1) MPERF_CpuTimeUpdate();
            uwpwins.Clear();

            ProcessListPrepareClear();
            listProcess.Locked = true;
            MEnumProcess2Refesh(enumProcessCallBack2_ptr);
            ProcessListClear();


            if(SysVer.IsWin8Upper()) MAppVProcessAllWindowsUWP();

            bool refeshAColumData = lvwColumnSorter.SortColumn == cpuindex
                || lvwColumnSorter.SortColumn == ramindex
                || lvwColumnSorter.SortColumn == diskindex
                || lvwColumnSorter.SortColumn == netindex
                || lvwColumnSorter.SortColumn == stateindex;
            ProcessListUpdateValues(refeshAColumData ? lvwColumnSorter.SortColumn : -1);

            if (refeshAColumData) listProcess.Sort();
            listProcess.Locked = false;
            listProcess.SyncItems(true);

            lbProcessCount.Text = "进程数：" + listProcess.Items.Count;
        }

        private void ProcessListLoad(uint pid, uint ppid, string exename, string exefullpath, IntPtr hprocess)
        {
            bool need_add_tolist = true;
            //base
            PsItem p = new PsItem();
            p.pid = pid;
            p.ppid = ppid;
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
            //test is 32 bit app in 64os
            if (is64OS)
            {
                if (hprocess != IntPtr.Zero)
                {
                    if (MGetProcessIs32Bit(hprocess))
                        taskMgrListItem.Text = taskMgrListItem.Text + " (32 位)";
                }
            }

            p.item = taskMgrListItem;

            //Test service
            bool isSvcHoct = false;
            if (exefullpath != null && (exefullpath.ToLower() == @"c:\windows\system32\svchost.exe" || exefullpath.ToLower() == @"c:\windows\syswow64\svchost.exe") || exename == "svchost.exe")
            {
                //svchost.exe add a icon
                taskMgrListItem.Icon = Properties.Resources.icoServiceHost;
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
                    if (p.svcs.Count == 1)
                    {
                        if (isSvcHoct)
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = "服务主机：" + p.svcs[0].scName + " (" + ScGroupNameToFriendlyName(p.svcs[0].groupName) + ")";
                            else taskMgrListItem.Text = "服务主机：" + p.svcs[0].scName;
                        }
                    }
                    else
                    {
                        if (isSvcHoct)
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = "服务主机：" + ScGroupNameToFriendlyName(p.svcs[0].groupName) + "(" + p.svcs.Count + ")";
                            else taskMgrListItem.Text = "服务主机 (" + p.svcs.Count + ")";
                        }
                    }
                    TaskMgrListItemChild tx = null;
                    for (int i = 0; i < p.svcs.Count; i++)
                    {
                        tx = new TaskMgrListItemChild(p.svcs[0].scDsb, icoSc);
                        tx.Tag = p.svcs[0].scName;
                        taskMgrListItem.Childs.Add(tx);
                    }
                    p.isSvchost = true;
                }
            }

            //ps data item
            if (SysVer.IsWin8Upper())
                p.isUWP = hprocess == IntPtr.Zero ? false : MGetProcessIsUWP(hprocess);
            p.perfData = MPERF_PerfDataCreate();
            p.handle = hprocess;
            p.exename = exename;
            p.pid = pid;
            p.exepath = stringBuilder.ToString();
            p.isWindowsProcess = (pid == 0
                            || pid == 4
                            || (pid == 88 && exename == "Registry")
                            || (pid < 1024 && exename == "csrss.exe")
                            || exename == "Memory Compression"
                            || IsWindowsProcess(exefullpath));

            taskMgrListItem.Tag = p;

            //10 empty item
            for (int i = 0; i < 10; i++) taskMgrListItem.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());

            //UWP app

            if(p.isUWP)
            {
                taskMgrListItem.IsUWP = true;
                if (uwpListInited)
                {                   
                    //get fullname
                    int len = 0;
                    if (MGetUWPPackageFullName(hprocess, ref len, null))
                    {
                        StringBuilder b = new StringBuilder(len);
                        if (MGetUWPPackageFullName(hprocess, ref len, b))
                        {
                            p.uwpFullName = b.ToString();
                            if (p.uwpFullName != "")
                            {
                                TaskMgrListItem uapp = UWPListFindItem(p.uwpFullName);
                                if (uapp != null)
                                {
                                    //copy data form uwp app list
                                    taskMgrListItem.Text = uapp.Text;
                                    taskMgrListItem.Icon = uapp.Icon;
                                    if (companyindex != -1)
                                        taskMgrListItem.SubItems[companyindex].Text = taskMgrListItem.SubItems[1].Text;
                                    taskMgrListItem.IsUWPICO = true;

                                    uwpitem parentItem = ProcessListFindUWPItem(p.uwpFullName);
                                    if (parentItem != null)
                                    {
                                        TaskMgrListItemGroup g = parentItem.uwpItem;
                                        g.Icon = uapp.Icon;
                                        g.Items.Add(taskMgrListItem);
                                        g.Text = uapp.Text + " (" + g.Items.Count + ")";
                                        p.uwpItem = parentItem;

                                        need_add_tolist = false;
                                    }
                                    else
                                    {
                                        parentItem = new uwpitem();

                                        TaskMgrListItemGroup g = new TaskMgrListItemGroup(uapp.Text);
                                        g.Icon = uapp.Icon;
                                        g.Items.Add(taskMgrListItem);
                                        g.Group = listProcess.Groups[1];
                                        g.IsUWPICO = true;
                                        //10 empty item
                                        for (int i = 0; i < 10; i++) g.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listProcess.Font });

                                        if (nameindex != -1) g.SubItems[nameindex].Text = p.uwpFullName;
                                        if (pathindex != -1) g.SubItems[pathindex].Text = uapp.SubItems[4].Text;

                                        g.Tag = uapp.SubItems[4].Text;

                                        parentItem.uwpFullName = p.uwpFullName;
                                        parentItem.uwpItem = g;
                                        p.uwpItem = parentItem;

                                        uwps.Add(parentItem);
                                        listProcess.Items.Add(g);
                                        need_add_tolist = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //data items

            if (nameindex != -1)
            {
                if (pid == 0) taskMgrListItem.SubItems[nameindex].Text = "系统空闲进程";
                else if (pid == 4) taskMgrListItem.SubItems[nameindex].Text = "ntoskrnl.exe";
                else taskMgrListItem.SubItems[nameindex].Text = exename;
            }
            if (pidindex != -1) {
                taskMgrListItem.SubItems[pidindex].Text = pid.ToString();
            }
            if (pathindex != -1) if (stringBuilder.ToString() != "") taskMgrListItem.SubItems[pathindex].Text = stringBuilder.ToString();
            if (cmdindex != -1)
            {
                StringBuilder s = new StringBuilder(2048);
                if (MGetProcessCommandLine(hprocess, s, 2048))
                    taskMgrListItem.SubItems[cmdindex].Text = s.ToString();
                else
                {
                    taskMgrListItem.SubItems[cmdindex].Text = "获取失败";
                    taskMgrListItem.SubItems[cmdindex].ForeColor = Color.Orange;
                }
            }
            if (companyindex != -1)
            {
                if (stringBuilder.ToString() != "")
                {
                    StringBuilder exeCompany = new StringBuilder(256);
                    if (MGetExeCompany(stringBuilder.ToString(), exeCompany, 256)) taskMgrListItem.SubItems[companyindex].Text = exeCompany.ToString();
                }
            }

            //Init performance

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
            if(need_add_tolist) listProcess.Items.Add(taskMgrListItem);
            ProcessListUpdate(pid, true, taskMgrListItem);
        }
        private void ProcessListUpdate(uint pid, bool isload = false, TaskMgrListItem it = null, int ipdateOneDataCloum = -1)
        {
            if (it is TaskMgrListItemGroup)
            {
                //uwp uppdate
                TaskMgrListItemGroup ii = it as TaskMgrListItemGroup;
                if (ii.ChildsOpened)
                {
                    foreach (TaskMgrListItem ix in ii.Items)
                        ProcessListUpdate(ix.PID, isload, ix, ipdateOneDataCloum);
                }
                else if (ii.Items.Count > 0)
                    ProcessListUpdate(ii.Items[0].PID, isload, ii.Items[0], ipdateOneDataCloum);

                if (stateindex != -1 && ipdateOneDataCloum != stateindex && ii.Items.Count > 0)
                    it.SubItems[stateindex].Text = ii.Items[0].SubItems[stateindex].Text;
                bool running = ProcessListGetUwpIsRunning(it.Text);
                if (running && stateindex != -1) if (it.SubItems[stateindex].Text == "已暂停") running = false;
                it.Group = running ? listProcess.Groups[0] : listProcess.Groups[1];
            }
            else
            {
                //Child and group
                PsItem p = ((PsItem)it.Tag);
                bool issvchost = p.isSvchost;
                if (!issvchost && it.Childs.Count > 0)
                { 
                    //remove invalid windows
                    for (int i = it.Childs.Count - 1; i >= 0; i--)
                    {
                        IntPtr h = (IntPtr)it.Childs[i].Tag;
                        if (!WorkWindow.FormSpyWindow.IsWindow(h) || !WorkWindow.FormSpyWindow.IsWindowVisible(h))
                            it.Childs.Remove(it.Childs[i]);
                    }
                    //update window
                    thisLoadItem = it;
                    MAppVProcessAllWindowsGetProcessWindow(pid);
                    thisLoadItem = null;

                    //group
                    if (it.Childs.Count == 0) it.Group = listProcess.Groups[1];
                    else p.isWindowShow = true;
                }
                else
                {
                    if (!isload)
                    {
                        //update window
                        thisLoadItem = it;
                        MAppVProcessAllWindowsGetProcessWindow(pid);
                        thisLoadItem = null;

                        //group
                        if (!issvchost && it.Childs.Count > 0)
                        {
                            it.Group = listProcess.Groups[0];
                            p.isWindowShow = true;
                        }
                        else if(p.isWindowsProcess)
                        {
                            it.Group = listProcess.Groups[2];
                            p.isWindowShow = false;
                        }
                        else
                        {
                            it.Group = listProcess.Groups[1];
                            p.isWindowShow = false;
                        }
                    }
                }
                //State
                if (stateindex != -1 && ipdateOneDataCloum != stateindex)
                {
                    int i = MGetProcessState(pid, IntPtr.Zero);
                    if (i == 1)
                    {
                        it.SubItems[stateindex].Text = "";
                        if (p.isSvchost == false && it.Childs.Count > 0 && p.exepath.ToLower() != @"c:\windows\system32\dwm.exe")
                        {
                            bool hung = false;
                            foreach (TaskMgrListItemChild c in it.Childs)
                                if (IsHungAppWindow((IntPtr)c.Tag))
                                {
                                    hung = true;
                                    break;
                                }
                            if (hung)
                            {
                                it.SubItems[stateindex].Text = "无响应";
                                it.SubItems[stateindex].ForeColor = Color.FromArgb(219, 107, 58);
                            }
                        }
                    }
                    else if (i == 2)
                    {
                        it.SubItems[stateindex].Text = "已暂停";
                        it.SubItems[stateindex].ForeColor = Color.FromArgb(22, 158, 250);
                    }
                }
                IntPtr handle = p.handle;
                if (handle != IntPtr.Zero)
                {
                    //Performance 
                    if (cpuindex != -1 && ipdateOneDataCloum != cpuindex)
                    {
                        double ii = MPERF_GetProcessCpuUseAge(handle, p.perfData);
                        it.SubItems[cpuindex].Text = ii.ToString("0.0") + "%";
                        it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii, 100);
                    }
                    if (ramindex != -1 && ipdateOneDataCloum != ramindex)
                    {
                        uint ii = MPERF_GetProcessRam(handle);
                        it.SubItems[ramindex].Text = FormatFileSize1(Convert.ToInt64(ii));
                        it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(ii / 1048576, 1024);
                    }
                    if (diskindex != -1 && ipdateOneDataCloum != diskindex)
                    {
                        ulong disk = MPERF_GetProcessDiskRate(handle, p.perfData);
                        it.SubItems[diskindex].Text = (disk / 1024d).ToString("0.0") + " MB/秒";
                        it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(disk, 1048576);
                    }
                    if (netindex != -1 && ipdateOneDataCloum != netindex)
                    {

                    }
                }
            }
        }
        private void ProcessListUpdateOnePerfCloum(uint pid, TaskMgrListItem it, int ipdateOneDataCloum)
        {
            if (it is TaskMgrListItemGroup)
            {
                TaskMgrListItemGroup ii = it as TaskMgrListItemGroup;
                if (stateindex != -1 && ipdateOneDataCloum == stateindex)
                {
                    bool running = ProcessListGetUwpIsRunning(it.Text);
                    if (running && stateindex != -1) if (it.SubItems[stateindex].Text == "已暂停") running = false;
                    it.Group = running ? listProcess.Groups[0] : listProcess.Groups[1];
                }
                else
                {
                    foreach (TaskMgrListItem ix in ii.Items)
                        ProcessListUpdateOnePerfCloum(ix.PID, ix, ipdateOneDataCloum);
                    /*
                     * /Performance 
                    if (cpuindex != -1 && ipdateOneDataCloum == cpuindex)
                    {
                        it.SubItems[cpuindex].Text = ii.Items[0].SubItems[cpuindex].Text;
                        it.SubItems[cpuindex].BackColor = ii.Items[0].SubItems[cpuindex].BackColor
                    }
                    else if (ramindex != -1 && ipdateOneDataCloum == ramindex)
                    {
                        it.SubItems[ramindex].Text = ii.Items[0].SubItems[ramindex].Text
                        it.SubItems[ramindex].BackColor = ii.Items[0].SubItems[ramindex].BackColor
                    }
                    else if (diskindex != -1 && ipdateOneDataCloum == diskindex)
                    {
                        it.SubItems[diskindex].Text = ii.Items[0].SubItems[diskindex].Text
                        it.SubItems[diskindex].BackColor = ii.Items[0].SubItems[diskindex].BackColor
                    }
                    else if (netindex != -1 && ipdateOneDataCloum == netindex)
                    {

                    }*/
                }
            }
            else
            {
                PsItem p = ((PsItem)it.Tag);
                if (stateindex != -1 && ipdateOneDataCloum == stateindex)
                {
                    int i = MGetProcessState(pid, IntPtr.Zero);
                    if (i == 1)
                    {
                        it.SubItems[stateindex].Text = "";
                        if (p.isSvchost == false && it.Childs.Count > 0)
                        {
                            bool hung = false;
                            foreach (TaskMgrListItemChild c in it.Childs)
                                if (IsHungAppWindow((IntPtr)c.Tag))
                                {
                                    hung = true;
                                    break;
                                }
                            if (hung)
                            {
                                it.SubItems[stateindex].Text = "无响应";
                                it.SubItems[stateindex].ForeColor = Color.FromArgb(219, 107, 58);
                            }
                        }
                    }
                    else if (i == 2)
                    {
                        it.SubItems[stateindex].Text = "已暂停";
                        it.SubItems[stateindex].ForeColor = Color.FromArgb(22, 158, 250);
                    }
                }
                else
                {
                    IntPtr handle = p.handle;
                    if (handle != IntPtr.Zero)
                    {
                        //Performance 
                        if (cpuindex != -1 && ipdateOneDataCloum == cpuindex)
                        {
                            double ii = MPERF_GetProcessCpuUseAge(handle, p.perfData);
                            it.SubItems[cpuindex].Text = ii.ToString("0.0") + "%";
                            it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii, 100);
                        }
                        else if (ramindex != -1 && ipdateOneDataCloum == ramindex)
                        {
                            uint ii = MPERF_GetProcessRam(handle);
                            it.SubItems[ramindex].Text = FormatFileSize1(Convert.ToInt64(ii));
                            it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(ii / 1048576, 1024);
                        }
                        else if (diskindex != -1 && ipdateOneDataCloum == diskindex)
                        {
                            ulong disk = MPERF_GetProcessDiskRate(handle, p.perfData);
                            it.SubItems[diskindex].Text = (disk / 1024d).ToString("0.0") + " MB/秒";
                            it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(disk, 1048576);
                        }
                        else if (netindex != -1 && ipdateOneDataCloum == netindex)
                        {

                        }
                    }
                }
            }
        }

        private void ProcessListPrepareClear()
        {
            //clear valid Pids
            validPid.Clear();
        }
        private void ProcessListClear()
        {
            //clear invalid items
            uint pid = 0;
            for (int i = loadedPs.Count - 1; i >= 0; i--)
            {
                pid = loadedPs[i].pid;
                if (!validPid.Contains(pid))
                    ProcessListFree(loadedPs[i]);
            }

            foreach (TaskMgrListItem i in listProcess.Items)
                if(i is TaskMgrListItemGroup)
                {
                    TaskMgrListItemGroup ii = i as TaskMgrListItemGroup;
                    foreach(TaskMgrListItem ix in ii.Items)
                        validPid.Remove(ix.PID);
                }
                else validPid.Remove(i.PID);
            if (validPid.Count > 0)
            {
                for (int i = validPid.Count - 1; i >= 0; i--)
                    MReUpdateProcess(validPid[i], enumProcessCallBack_ptr);
            }
        }
        private void ProcessListFree(PsItem it)
        {
            //remove invalid item
            TaskMgrListItem li = ProcessListFindItem(it.pid);
            MCloseHandle(it.handle);
            if (uwpHostPid.Contains(it.pid)) uwpHostPid.Remove(it.pid);
            MPERF_PerfDataDestroy(it.perfData);
            it.svcs.Clear();
            loadedPs.Remove(it);
            if (it.uwpItem != null)
                it.uwpItem = null;
            if (li != null)
            {
                //is a group item
                if (li.Parent != null)
                {
                    TaskMgrListItemGroup iii = li.Parent;
                    iii.Items.Remove(li);
                    if (iii.Items.Count == 0)//o to remove
                        listProcess.Items.Remove(iii);
                    else if (iii.Items.Count > 1)
                    {
                        //update (x) child item count
                        string text = iii.Text;
                        if (text.Contains("(") && text.EndsWith(")"))
                        {
                            text = text.Remove(text.Length - 3);
                            iii.Text = text + " (" + iii.Items.Count + ")";
                        }
                    }
                }
                else listProcess.Items.Remove(li);
            }
        }
        private void ProcessListFreeAll()
        {
            //the exit clear
            uwps.Clear();
            uwpHostPid.Clear();
            if (performanceCounter_cpu_total != null) performanceCounter_cpu_total.Close();
            if (performanceCounter_ram_total != null) performanceCounter_ram_total.Close();
            if (performanceCounter_disk_total != null) performanceCounter_disk_total.Close();
            if (performanceCounter_net_total != null) performanceCounter_net_total.Close();
            for (int i = 0; i < loadedPs.Count; i++)
                ProcessListFree(loadedPs[i]);
            loadedPs.Clear();
            listProcess.Items.Clear();
        }

        private void ProcessListHandle(uint pid, uint ppid, IntPtr name, IntPtr exefullpath, int tp, IntPtr hprocess)
        {
            //enum proc callback
            if (tp == 1)
            {
                if (!isRunAsAdmin && exefullpath == IntPtr.Zero && pid != 0 && pid != 4 && pid != 88)
                    return;
                validPid.Add(pid);
                PsItem item;
                if (ProcessListIsProcessLoaded(pid, out item))
                    ProcessListUpdate(pid, false, item.item);
                else ProcessListLoad(pid, ppid, Marshal.PtrToStringAuto(name), Marshal.PtrToStringAuto(exefullpath), hprocess);
            }
            else if (tp == 0) ProcessListRefesh1Finished();
        }
        private void ProcessListHandle2(uint pid)
        {           
            //enum proc callback2 (refesh)
            validPid.Add(pid);
        }

        private void ProcessListUpdateValues(int refeshAllDataColum)
        {
            //update process perf data
            if (refeshAllDataColum != -1)
            {
                foreach (TaskMgrListItem it in listProcess.Items)
                    ProcessListUpdateOnePerfCloum(it.PID, it, refeshAllDataColum);
            }
            for (int i = 0; i < listProcess.ShowedItems.Count; i++)
                ProcessListUpdate(listProcess.ShowedItems[i].PID, false, listProcess.ShowedItems[i], refeshAllDataColum);
        }
        private void ProcessListLoadFinished()
        {
            //firstLoad
            firstLoad = false;
            tabControlMain.Show();
            Cursor = Cursors.Arrow;
        }

        private void BaseProcessRefeshTimer_Tick(object sender, EventArgs e)
        {
            //base RefeshTimer
            if (tabControlMain.SelectedTab == tabPageProcCtl)
            {
                //Refesh perfs
                listProcess.Locked = true;
                if (cpuindex != -1)
                {
                    int cpu = (int)(performanceCounter_cpu_total.NextValue());
                    listProcess.Colunms[cpuindex].TextBig = cpu + "%";
                    if (cpu >= 95)
                        listProcess.Colunms[cpuindex].IsHot = true;
                    else listProcess.Colunms[cpuindex].IsHot = false;
                }
                if (ramindex != -1)
                {
                    int ram = (int)(MGetRamUseAge() * 100);
                    listProcess.Colunms[ramindex].TextBig = ram + "%";
                    if (ram >= 95)
                        listProcess.Colunms[ramindex].IsHot = true;
                    else listProcess.Colunms[ramindex].IsHot = false;
                }
                if (diskindex != -1)
                {
                    listProcess.Colunms[diskindex].TextBig = ((int)(performanceCounter_disk_total.NextValue())) + "%";
                }
                if (netindex != -1)
                {
                    listProcess.Colunms[netindex].TextBig = ((int)(performanceCounter_net_total.NextValue() * 0.00001)) + "%";
                }
                //Refesh Process List
                ProcessListRefesh2();
                listProcess.Locked = false;
                listProcess.Header.Invalidate();
            }
            else if (tabControlMain.SelectedTab == tabPagePerfCtl)
            {
                int cpu= (int)(performanceCounter_cpu_total.NextValue());
                perf_cpu.SmallText = cpu + " %";
                perf_cpu.AddData(cpu);

                int ram = (int)(MGetRamUseAge() * 100);
                perf_ram.SmallText = ram + " %";
                perf_ram.AddData(ram);

                PerfUpdate();

                performanceLeftList.Invalidate();
            }
        }

        private void check_showAllProcess_CheckedChanged(object sender, EventArgs e)
        {
            //switch to admin
            if (!MIsRunasAdmin())
                if (check_showAllProcess.Checked)
                {
                    MAppRebotAdmin();
                    check_showAllProcess.Checked = false;
                }
                else check_showAllProcess.Checked = false;
        }

        #region ListEvents

        private void listProcess_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listProcess.SelectedItem != null)
                {
                    if (listProcess.SelectedItem.OldSelectedItem == null)
                    {
                        if (listProcess.SelectedItem.IsGroup)
                        {
                            string exepath = listProcess.SelectedItem.Tag.ToString();
                            MAppWorkShowMenuProcess(exepath, null, 0, Handle, 0);
                        }
                        else
                        {
                            PsItem t = (PsItem)listProcess.SelectedItem.Tag;
                            int rs = MAppWorkShowMenuProcess(t.exepath,
                                t.exename,
                                selectedpid, Handle, isSelectExplorer ? 1 : 0);
                        }
                    }
                    else if (listProcess.SelectedItem.OldSelectedItem != null)
                    {
                        PsItem t = (PsItem)listProcess.SelectedItem.Tag;
                        if (t.isSvchost)
                        {
                            IntPtr scname = Marshal.StringToHGlobalUni((string)listProcess.SelectedItem.OldSelectedItem.Tag);
                            MAppWorkCall3(184, Handle, scname);
                            Marshal.FreeHGlobal(scname);
                        }
                        else
                        {
                            MAppWorkCall3(189, Handle, (IntPtr)listProcess.SelectedItem.OldSelectedItem.Tag);
                        }
                    }
                }
            }
        }
        private void listProcess_MouseDown(object sender, MouseEventArgs e)
        {
            if (listProcess.SelectedItem != null)
            {
                if (!listProcess.SelectedItem.IsGroup)
                {
                    PsItem t = (PsItem)listProcess.SelectedItem.Tag;
                    selectedpid = t.pid;
                    if (selectedpid > 4)
                    {
                        btnEndProcess.Enabled = true;
                        if (nameindex != -1)
                            if (t.exename == "explorer.exe")
                            { btnEndProcess.Text = "重新启动(E)"; isSelectExplorer = true; }
                            else { btnEndProcess.Text = "结束进程(E)"; isSelectExplorer = false; }
                        if (t.isWindowShow)
                            btnEndProcess.Text = "结束任务(E)";
                        else btnEndProcess.Text = "结束进程(E)";
                    }
                    else btnEndProcess.Enabled = false;
                }
                else btnEndProcess.Enabled = false;
            }
            else btnEndProcess.Enabled = false;
        }
        private void listProcess_SelectItemChanged(object sender, EventArgs e)
        {

        }
        private void listProcess_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnEndProcess_Click(sender, e);
            }
        }

        private void Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left)
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

        private void btnEndProcess_Click(object sender, EventArgs e)
        {
            TaskMgrListItem taskMgrListItem = listProcess.SelectedItem;
            if (taskMgrListItem != null)
            {
                PsItem p = taskMgrListItem.Tag as PsItem;
                if (p.isWindowShow&&!p.isSvchost)
                {
                    if (taskMgrListItem.Childs.Count > 0)
                    {
                        foreach (TaskMgrListItemChild c in taskMgrListItem.Childs)
                            if (c.Tag != null)
                            {
                                IntPtr handle = (IntPtr)c.Tag;
                                MAppWorkCall3(192, IntPtr.Zero, handle);
                            }
                        return;
                    }
                }
            }
            MAppWorkCall3(190, Handle, IntPtr.Zero);
        }
        private void lbShowDetals_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!MAppVProcess(Handle)) TaskDialog.Show("无法打开详细信息窗口", DEFAPPTITLE, "未知错误。", TaskDialogButton.OK, TaskDialogIcon.Stop);
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
                else if(SortColumn == m.cpuindex
                    || SortColumn == m.cpuindex
                    || SortColumn == m.cpuindex
                    || SortColumn == m.cpuindex)
                    compareResult = ObjectCompare.Compare(x.SubItems[SortColumn].CustomData, y.SubItems[SortColumn].CustomData);              
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
        private bool lastRightClicked = false;

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

        private List<uint> scValidPid = new List<uint>();
        private List<ScItem> runningSc = new List<ScItem>();
        private Icon icoSc = null;
        private Dictionary<string, string> scGroupFriendlyName = new Dictionary<string, string>();
        private bool scCanUse = false;

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
            string rs = "";
            if (s != null)
                if (!scGroupFriendlyName.TryGetValue(s, out rs))
                    rs = s;
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
        private void ScMgrInit()
        {
            if(!scListInited)
            {         
                if (!MIsRunasAdmin())
                {
                    listService.Hide();
                    pl_ScNeedAdminTip.Show();
                }
                else
                {
                    scGroupFriendlyName.Add("localService", "本地服务");
                    scGroupFriendlyName.Add("LocalService", "本地服务");
                    scGroupFriendlyName.Add("LocalSystem", "本地系统");
                    scGroupFriendlyName.Add("LocalSystemNetworkRestricted", "本地系统（网络受限）");
                    scGroupFriendlyName.Add("LocalServiceNetworkRestricted", "本地服务（网络受限）");
                    scGroupFriendlyName.Add("LocalServiceNoNetwork", "本地服务（无网络）");
                    scGroupFriendlyName.Add("LocalServiceAndNoImpersonation", "本地服务（模拟）");
                    scGroupFriendlyName.Add("NetworkServiceAndNoImpersonation", "网络服务（模拟）");
                    scGroupFriendlyName.Add("NetworkService", "网络服务");
                    scGroupFriendlyName.Add("NetworkServiceNetworkRestricted", "网络服务（网络受限）");
                    scGroupFriendlyName.Add("UnistackSvcGroup", "Unistack 服务组");
                    scGroupFriendlyName.Add("NetSvcs", "网络服务");
                    scGroupFriendlyName.Add("netsvcs", "网络服务");

                    MAppWorkCall3(182, listService.Handle, IntPtr.Zero);

                    if (!MSCM_Init())
                        TaskDialog.Show("无法启动服务管理器", "错误", "", TaskDialogButton.OK, TaskDialogIcon.Stop);

                    scMgrEnumServicesCallBack = ScMgrIEnumServicesCallBack;
                    scMgrEnumServicesCallBackPtr = Marshal.GetFunctionPointerForDelegate(scMgrEnumServicesCallBack);

                    ScMgrRefeshList();
                    scCanUse = true;
                }

                icoSc = new Icon(Properties.Resources.icoService, 16, 16);
                scListInited = true;
            }
        }
        private void ScMgrRefeshList()
        {
            scValidPid.Clear();
            runningSc.Clear();
            listService.Items.Clear();
            MEnumServices(scMgrEnumServicesCallBackPtr);
            lbServicesCount.Text = "服务数：" + (listService.Items.Count == 0 ? "--" : listService.Items.Count.ToString());
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
            switch(currentState)
            {                
                case 0x0001:
                case 0x0003: li.SubItems.Add("已停止");  break;
                case 0x0002:
                case 0x0004: li.SubItems.Add("正在运行"); break;
                case 0x0006:
                case 0x0007: li.SubItems.Add("已暂停"); break;
                default: li.SubItems.Add(""); break;
            }
            li.SubItems.Add(Marshal.PtrToStringUni(lpLoadOrderGroup));
            switch (dwStartType)
            {
                case 0x0000: li.SubItems.Add("驱动加载"); break;
                case 0x0001: li.SubItems.Add("驱动"); break;
                case 0x0002: li.SubItems.Add("自动"); break;
                case 0x0003: li.SubItems.Add("手动"); break;
                case 0x0004: li.SubItems.Add("禁用"); break;
                case 0x0080: li.SubItems.Add(""); break;
                default: li.SubItems.Add(""); break;
            }
            switch (scType)
            {
                case 0x0002: li.SubItems.Add("文件系统"); break;
                case 0x0001: li.SubItems.Add("内核服务"); break;
                case 0x0010: li.SubItems.Add("用户服务"); break;
                case 0x0020: li.SubItems.Add("系统服务"); break;
                default: li.SubItems.Add(""); break;
            }
            li.SubItems.Add(Marshal.PtrToStringUni(lpBinaryPathName));
            listService.Items.Add(li);
        }

        private void listService_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listService.SelectedItems.Count > 0)
                {
                    ListViewItem item = listService.SelectedItems[0];
                    ScTag t = item.Tag as ScTag;
                    MSCM_ShowMenu(Handle, t.name, t.runningState, t.startType, t.binaryPathName);
                }
            }
        }

        private void linkRebootAsAdmin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MAppRebotAdmin();
        }
        private void linkOpenScMsc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MFM_OpenFile("services.msc", Handle);
        }

        #endregion

        #region UWPMWork

        private void UWPListRefesh()
        {
            listUwpApps.Show();
            pl_UWPEnumFailTip.Hide();
            listUwpApps.Items.Clear();
            uwpListInited = false;
            UWPListInit();
        }
        private void UWPListInit()
        {
            if(!uwpListInited)
            {
                UWPManager uWPManager = new UWPManager();
                try
                {
                    uWPManager.EnumlateAll();
                    for (int i = 0; i < uWPManager.Packages.Count; i++)
                    {
                        TaskMgrListItem li = new TaskMgrListItem(UWPManager.DisplayNameTozhCN(uWPManager.Packages[i].Name));
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                        li.SubItems[0].Font = listUwpApps.Font;
                        li.SubItems[1].Font = listUwpApps.Font;
                        li.SubItems[2].Font = listUwpApps.Font;
                        li.SubItems[3].Font = listUwpApps.Font;
                        li.SubItems[4].Font = listUwpApps.Font;
                        li.SubItems[0].Text = UWPManager.DisplayNameTozhCN(uWPManager.Packages[i].Name);
                        li.SubItems[1].Text = uWPManager.Packages[i].Publisher;
                        li.SubItems[2].Text = uWPManager.Packages[i].Description;
                        li.SubItems[3].Text = uWPManager.Packages[i].FullName;
                        li.SubItems[4].Text = uWPManager.Packages[i].InstalledLocation;
                        li.Tag = uWPManager.Packages[i].FullName;
                        li.IsUWPICO = true;

                        string iconpath = uWPManager.Packages[i].IconPath;
                        if (iconpath != "" && File.Exists(iconpath))
                        {
                            using (Image icofile = Image.FromFile(iconpath))
                            {
                                li.Icon = IconUtils.ConvertToIcon(icofile);
                            }
                        }
                        listUwpApps.Items.Add(li);
                    }
                }
                catch(Exception e)
                {
                    listUwpApps.Hide();
                    pl_UWPEnumFailTip.Show();
                    lbUWPEnumFailText.Text = "无法枚举通用应用\n\n" + e.ToString();
                }
                uwpListInited = true;
            }
        }
        private TaskMgrListItem UWPListFindItem(string fullName)
        {
            TaskMgrListItem rs = null;
            foreach(TaskMgrListItem r in listUwpApps.Items)
                if(r.Tag.ToString()== fullName)
                {
                    rs = r;
                    break;
                }
            return rs;
        }

        #endregion

        #region PerfWork

        PerformanceListItem perf_cpu = new PerformanceListItem();
        PerformanceListItem perf_ram = new PerformanceListItem();

        private class PerfItemHeader
        {
            public PerformanceListItem item = null;
            public PerformanceCounter performanceCounter = null;
            public float x = 1;
        }

        private List<PerfItemHeader> perfItems = new List<PerfItemHeader>();

        private void PerfInit()
        {
            if (!perfInited)
            {
                perf_cpu.Name = "CPU";
                perf_cpu.SmallText = "0 %";
                perf_cpu.BasePen = new Pen(Color.FromArgb(17, 125, 187), 2);
                perf_cpu.BgBrush = new SolidBrush(Color.FromArgb(241, 246, 250));

                performanceLeftList.Items.Add(perf_cpu);

                perf_ram.Name = "内存";
                perf_ram.SmallText = "0 %";
                perf_ram.BasePen = new Pen(Color.FromArgb(139, 18, 174), 2);
                perf_ram.BgBrush = new SolidBrush(Color.FromArgb(244, 242, 244));

                string[] disk_instanceNames = new PerformanceCounterCategory("PhysicalDisk").GetInstanceNames();
                foreach (string s in disk_instanceNames)
                {
                    if (s != "_Total")
                    {
                        PerfItemHeader perfItemHeader = new PerfItemHeader();
                        perfItemHeader.performanceCounter = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "", true);
                        perfItemHeader.performanceCounter.InstanceName = s;
                        perfItemHeader.item = new PerformanceListItem();
                        perfItemHeader.item.Name = "磁盘 " + s;
                        perfItemHeader.item.BasePen = new Pen(Color.FromArgb(77, 166, 12));
                        perfItemHeader.item.BgBrush = Brushes.White;
                        perfItems.Add(perfItemHeader);

                        performanceLeftList.Items.Add(perfItemHeader.item);
                    }
                }

                string[] network_instanceNames = new PerformanceCounterCategory("Network Interface").GetInstanceNames();
                foreach (string s in network_instanceNames)
                {
                    PerfItemHeader perfItemHeader = new PerfItemHeader();
                    perfItemHeader.performanceCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", "", true);
                    perfItemHeader.performanceCounter.InstanceName = s;
                    perfItemHeader.item = new PerformanceListItem();
                    perfItemHeader.item.Name = "网络 ";
                    perfItemHeader.item.BasePen = new Pen(Color.FromArgb(167, 79, 1));
                    perfItemHeader.item.BgBrush = Brushes.White;
                    perfItemHeader.x = 0.00001f;
                    perfItems.Add(perfItemHeader);

                    performanceLeftList.Items.Add(perfItemHeader.item);
                }

                performanceLeftList.Items.Add(perf_ram);
                performanceLeftList.UpdateAll();
                performanceLeftList.Invalidate();

                perfInited = true;
            }
        }
        private void PerfUpdate()
        {
            foreach(PerfItemHeader h in perfItems)
            {
                float data = (h.performanceCounter.NextValue() * h.x);
                h.item.SmallText = data.ToString("0.0") + "%";
                h.item.AddData((int)data);
            }
        }
        private void PerfClear()
        {
            foreach (PerfItemHeader h in perfItems)
            {
                h.performanceCounter.Close();
                h.item = null;
            }
            perfItems.Clear();
        }

        #endregion

        private void AppWorkerCallBack(int msg, IntPtr lParam, IntPtr wParam)
        {
            switch(msg)
            {
                case 5:
                    {
                        int c = lParam.ToInt32();
                        if (c == 0)
                        {
                            baseProcessRefeshTimer.Stop();
                            SetConfig("RefeshTime", "AppSetting", "Stop");
                        }
                        else
                        {
                            if (c == 1) { baseProcessRefeshTimer.Interval = 2000; SetConfig("RefeshTime", "AppSetting", "Slow"); }
                            else if (c == 2) { baseProcessRefeshTimer.Interval = 1000; SetConfig("RefeshTime", "AppSetting", "Fast"); }
                            baseProcessRefeshTimer.Start();
                        }
                        break;
                    }
                case 6:
                    {
                        int c = lParam.ToInt32();
                        if(c==0)
                        {
                            SetConfig("TopMost", "AppSetting", "FALSE");
                            TopMost = false;
                        }
                        else if(c==1)
                        {
                            SetConfig("TopMost", "AppSetting", "TRUE");
                            TopMost = true;
                        }
                        break;
                    }
                case 7:
                    {
                        int c = lParam.ToInt32();
                        if (c == 0)
                        {
                            SetConfig("CloseHideToNotfication", "AppSetting", "FALSE");
                            close_hide = false;
                        }
                        else if (c == 1)
                        {
                            SetConfig("CloseHideToNotfication", "AppSetting", "TRUE");
                            close_hide = true;
                        }
                        break;
                    }
                case 8:
                    {
                        int c = lParam.ToInt32();
                        if (c == 0)
                        {
                            SetConfig("MinHide", "AppSetting", "FALSE");
                            min_hide = false;
                        }
                        else if (c == 1)
                        {
                            SetConfig("MinHide", "AppSetting", "TRUE");
                            min_hide = true;
                        }
                        break;
                    }
                case 9:
                    {
                        string scname = Marshal.PtrToStringUni(lParam);
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
                case 10:
                    {
                        ScMgrRefeshList();
                        break;
                    }
            }
        }
        private void AppLoad()
        {
            exitCallBack = AppExit;
            taskDialogCallBack = TaskDialogCallback;
            enumProcessCallBack = ProcessListHandle;
            enumWinsCallBack = MainEnumWinsCallBack;
            getWinsCallBack = MainGetWinsCallBack;
            enumProcessCallBack2 = ProcessListHandle2;
            workerCallBack = AppWorkerCallBack;

            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(exitCallBack), 1);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(taskDialogCallBack), 2);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(enumWinsCallBack), 3);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(getWinsCallBack), 4);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(workerCallBack), 5);

            MAppWorkCall3(183, Handle, IntPtr.Zero);
            coreWndProc = (WNDPROC)Marshal.GetDelegateForFunctionPointer(MAppSetCallBack(IntPtr.Zero, 0), typeof(WNDPROC));

            if (!MGetPrivileges()) TaskDialog.Show("提权失败！", DEFAPPTITLE, "", TaskDialogButton.OK, TaskDialogIcon.Warning);
            is64OS = MIs64BitOS();

            SysVer.Get();
            if (!SysVer.IsWin8Upper()) tabControlMain.TabPages.Remove(tabPageUWPCtl);

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
            showHiddenFiles = GetConfig("ShowHiddenFiles", "AppSetting") == "TRUE";
            MFM_SetShowHiddenFiles(showHiddenFiles);
            #endregion

            #region LoadList

            lvwColumnSorter = new TaskListViewColumnSorter(this);

            TaskMgrListGroup lg = new TaskMgrListGroup("应用");
            listProcess.Groups.Add(lg);
            TaskMgrListGroup lg2 = new TaskMgrListGroup("后台进程");
            listProcess.Groups.Add(lg2);
            TaskMgrListGroup lg3 = new TaskMgrListGroup("Windows进程");
            listProcess.Groups.Add(lg3);
            TaskMgrListGroup lg4 = new TaskMgrListGroup("危险进程");
            listProcess.Groups.Add(lg4);

            listUwpApps.Header.Height = 36;
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

            TaskMgrListHeaderItem li8 = new TaskMgrListHeaderItem();
            li8.TextSmall = "名称";
            li8.Width = 400;
            listUwpApps.Colunms.Add(li8);
            TaskMgrListHeaderItem li9 = new TaskMgrListHeaderItem();
            li9.TextSmall = "发布者";
            li9.Width = 100;
            listUwpApps.Colunms.Add(li9);
            TaskMgrListHeaderItem li12 = new TaskMgrListHeaderItem();
            li12.TextSmall = "说明";
            li12.Width = 100;
            listUwpApps.Colunms.Add(li12);
            TaskMgrListHeaderItem li10 = new TaskMgrListHeaderItem();
            li10.TextSmall = "完整名称";
            li10.Width = 130;
            listUwpApps.Colunms.Add(li10);
            TaskMgrListHeaderItem li11 = new TaskMgrListHeaderItem();
            li11.TextSmall = "安装路径";
            li11.Width = 130;
            listUwpApps.Colunms.Add(li11);

            string s1 = GetConfig("MainHeaders1", "AppSetting");
            if (s1 != "") listProcessAddHeader("名称", int.Parse(s1));
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
            else if (headers == "")
            {
                listProcessAddHeader("状态", 70);
                listProcessAddHeader("PID", 55);
                listProcessAddHeader("进程名称", 130);
                listProcessAddHeader("CPU", 75);
                listProcessAddHeader("内存", 75);
                listProcessAddHeader("磁盘", 75);
                listProcessAddHeader("网络", 75);
            }

            string s2 = GetConfig("RefeshTime", "AppSetting");
            switch(s2)
            {
                case "Stop":
                    MAppWorkCall3(193, IntPtr.Zero, new IntPtr(0));
                    break;
                case "Slow":
                    MAppWorkCall3(193, IntPtr.Zero, new IntPtr(1));
                    break;
                case "Fast":
                    MAppWorkCall3(193, IntPtr.Zero, new IntPtr(2));
                    break;
            }

            MAppWorkCall3(194, IntPtr.Zero, GetConfig("TopMost", "AppSetting") == "TRUE" ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(195, IntPtr.Zero, GetConfig("CloseHideToNotfication", "AppSetting") == "TRUE" ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(196, IntPtr.Zero, GetConfig("MinHide", "AppSetting") == "TRUE" ? new IntPtr(1) : IntPtr.Zero);

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

            #endregion

            ProcessListInit();

            /*TaskMgrListItem it = new TaskMgrListItem();
            it.Text = "w4643634";
            it.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            TaskMgrListItem it2 = new TaskMgrListItem();
            it2.Text = "设置大小官员";
            it2.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            listProcess.Items.Add(it);
            listProcess.Items.Add(it2);
            baseProcessRefeshTimer.Stop();
            tabControlMain.Show();*/
        }
        private void AppExit()
        {
            PerfClear();
            ProcessListFreeAll();
            MSCM_Exit();
            MEnumProcessFree();
            fileSystemWatcher.EnableRaisingEvents = false;

            Application.Exit();
        }

        #region FormEvent

        private bool close_hide = false;
        private bool min_hide = false;

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }
        private void FormMain_Shown(object sender, EventArgs e)
        {
            AppLoad();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            #region Pos
            string s = GetConfig("OldSize", "AppSetting");
            string p = GetConfig("OldPos", "AppSetting");
            if (s.Contains("-"))
            {
                string[] ss = s.Split('-');
                try
                {
                    Width = int.Parse(ss[0]);
                    Height = int.Parse(ss[1]);
                }
                catch { }
            }
            if (p.Contains("-"))
            {
                string[] pp = p.Split('-');
                try
                {
                    Left = int.Parse(pp[0]);
                    Top = int.Parse(pp[1]);
                }
                catch { }
            }
            #endregion         
        }
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0111)
                FormMain_OnWmCommand(m.WParam.ToInt32());
            if (m.Msg == 0x0112)
            {
                if (min_hide && m.WParam.ToInt32() == 0xF20)
                    Hide();
            }
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
            switch (id)
            {
                case 40034:
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                        {
                            WorkWindow.FormMainListHeaders f = new WorkWindow.FormMainListHeaders(this);
                            if (f.ShowDialog() == DialogResult.OK)
                                MAppWorkCall3(191, IntPtr.Zero, IntPtr.Zero);
                        }
                        break;
                    }
                case 41130:
                case 41012:
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                            ProcessListRefesh();
                        else if (tabControlMain.SelectedTab == tabPageKernelCtl)
                            ;
                        else if (tabControlMain.SelectedTab == tabPageStartCtl)
                            ;
                        else if (tabControlMain.SelectedTab == tabPageScCtl)
                            ScMgrRefeshList();
                        else if (tabControlMain.SelectedTab == tabPageFileCtl)
                            FileMgrShowFiles(null);
                        else if (tabControlMain.SelectedTab == tabPageUWPCtl)
                            UWPListRefesh();
                        else if (tabControlMain.SelectedTab == tabPagePerfCtl)
                            BaseProcessRefeshTimer_Tick(null, null);
                        break;
                    }
                case 40019:
                    {
                        TaskDialog t = new TaskDialog("您即将重启。", DEFAPPTITLE, "确定继续吗？", TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41020:
                    {
                        TaskDialog t = new TaskDialog("您即将注销。", DEFAPPTITLE, "确定继续吗？", TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(186, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 40018:
                    {
                        TaskDialog t = new TaskDialog("您即将关机。", DEFAPPTITLE, "确定继续吗？", TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(close_hide)
            {
                e.Cancel = true;
                Hide();
            }

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
            else if (e.TabPage == tabPageUWPCtl)
            {
                UWPListInit();
            }
            else if (e.TabPage == tabPagePerfCtl)
            {
                PerfInit();
            }
        }


    }
}
