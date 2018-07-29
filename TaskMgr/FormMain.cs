using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PCMgr
{
    public partial class FormMain : Form
    {
        public const string MICOSOFT = "Microsoft Corporation";
#if _X64_
        public const string COREDLLNAME = "PCMgr64.dll";
#else
        public const string COREDLLNAME = "PCMgr32.dll";
#endif
        public static FormMain Instance { private set; get; }

        public FormMain(string[] agrs)
        {
            Instance = this;
            InitializeComponent();
            baseProcessRefeshTimer.Interval = 1000;
            baseProcessRefeshTimer.Tick += BaseProcessRefeshTimer_Tick;
            listProcess.Header.CloumClick += Header_CloumClick;
            this.agrs = agrs;
        }

        #region Config
        public static bool SetConfigBool(string configkey, string configSection, bool configData)
        {
            return SetConfig(configkey, configSection, configData ? "TRUE" : "FALSE");
        }
        public static bool SetConfig(string configkey, string configSection, string configData)
        {
            long OpStation = WritePrivateProfileString(configSection, configkey, configData, Application.StartupPath + "\\" + currentProcessName + ".ini");
            return (OpStation != 0);
        }
        public static string GetConfig(string configkey, string configSection, string configDefData = "")
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(configSection, configkey, configDefData, temp, 1024, Application.StartupPath + "\\" + currentProcessName + ".ini");
            return temp.ToString();
        }
        public static bool GetConfigBool(string configkey, string configSection, bool defaultValue = false)
        {
            string s = GetConfig(configkey, configSection, defaultValue.ToString());
            if (s == "TRUE" || s == "True" || s == "true" || s == "1" || s.ToLower() == "true")
                return true;
            return false;
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

        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_COMMAND = 0x0111;

        [DllImport("kernel32")]
        public static extern uint GetLastError();
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("user32")]
        private static extern bool IsHungAppWindow(IntPtr hWnd);
        [DllImport("user32")]
        public static extern IntPtr GetDesktopWindow();

        private int TaskDialogCallback(IntPtr hwnd, string text, string title, string instruction, int ico, int button)
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

            TaskDialog t = new TaskDialog(instruction, title, text, tbtn, tico);
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

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_SU_Test([MarshalAs(UnmanagedType.LPStr)]string instr);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MAppStartEnd();       
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_LOG_Init(bool forecConsole);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void M_LOG_Close();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Error_ForceFile")]
        public static extern void FLogErr([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Warning_ForceFile")]
        public static extern void FLogWarn([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Info_ForceFile")]
        public static extern void FLogInfo([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Str_ForceFile")]
        public static extern void FLog([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Error")]
        public static extern void LogErr([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Warning")]
        public static extern void LogWarn([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Info")]
        public static extern void LogInfo([MarshalAs(UnmanagedType.LPWStr)]string format);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Str")]
        public static extern void Log([MarshalAs(UnmanagedType.LPWStr)]string format);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MGetPrivileges();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_SU_SetSysver(uint ver);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetPrivileges2();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MAppStartTest();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MAppKillOld([MarshalAs(UnmanagedType.LPWStr)]string procname);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppSetCallBack(IntPtr ptr, int id);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppWorkCall2(uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MAppWorkCall3(int id, IntPtr hWnd, IntPtr data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MAppExit();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MAppRebot();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIs64BitOS();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIsRunasAdmin();
        public static bool MIsFinded64()
        {
            return File.Exists(Application.StartupPath + "\\PCMgr64.exe");
        }
        public static bool MRun64()
        {
            return MFM_OpenFile(Application.StartupPath + "\\PCMgr64.exe", IntPtr.Zero);
        }
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppRebotAdmin();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppRebotAdmin2([MarshalAs(UnmanagedType.LPWStr)]string agrs);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MAppSetLanuageItems(int i, int id, [MarshalAs(UnmanagedType.LPWStr)]string s, int size);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MLG_SetLanuageRes([MarshalAs(UnmanagedType.LPWStr)]string s0, [MarshalAs(UnmanagedType.LPWStr)]string s);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MAppRegShowHotKey(IntPtr hWnd, uint vkkey, uint key);

        private static LanuageItems_CallBack lanuageItems_CallBack;

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_FileExist([MarshalAs(UnmanagedType.LPWStr)]string path);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MLG_SetLanuageItems_CallBack(IntPtr callback);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate string LanuageItems_CallBack([MarshalAs(UnmanagedType.LPWStr)]string s);

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

        private const int MENU_SELECTED_PROCESS_KILL_ACT_KILL = 0;
        private const int MENU_SELECTED_PROCESS_KILL_ACT_REBOOT = 1;
        private const int MENU_SELECTED_PROCESS_KILL_ACT_RESENT_BACK = 2;

        private EnumProcessCallBack enumProcessCallBack;
        private EnumProcessCallBack2 enumProcessCallBack2;
        private EnumWinsCallBack enumWinsCallBack;
        private GetWinsCallBack getWinsCallBack;

        private IntPtr enumProcessCallBack_ptr;
        private IntPtr enumProcessCallBack2_ptr;

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_SU_SetKrlMonSet_CreateProcess(bool allow);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_SU_SetKrlMonSet_CreateThread(bool allow);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_SU_SetKrlMonSet_LoadImage(bool allow);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MCommandLineToFilePath(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MGetProcessState(uint dwPID, IntPtr hwnd);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetExeInfo(string strFilePath, string InfoItem, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetExeDescribe(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetExeCompany(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MGetProcessEprocess(uint pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr MGetExeIcon(string pszFullPath);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MGetCpuUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MGetRamUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MGetDiskUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppWorkShowMenuProcess([MarshalAs(UnmanagedType.LPWStr)]string strFilePath, [MarshalAs(UnmanagedType.LPWStr)]string strFileName, long pid, IntPtr hWnd, int data, int type);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppWorkShowMenuProcessPrepare([MarshalAs(UnmanagedType.LPWStr)]string strFilePath, [MarshalAs(UnmanagedType.LPWStr)]string strFileName, long pid);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MAppVProcessModuls(uint dwPID, IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)]string procName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MAppVProcessThreads(uint dwPID, IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)]string procName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MAppVProcessWindows(uint dwPID, IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)]string procName);      

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

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MCanUseKernel();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MUninitKernel();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MInitKernel(string currDir);

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

        public static String FormatFileSize(UInt64 fileSize)
        {
            if (fileSize < 0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }
            else if (fileSize >= 1073741824)
            {
                return string.Format("{0:########0.00} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1048576)
            {
                return string.Format("{0:####0.00} MB", ((Double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.00} KB", ((Double)fileSize) / 1024);
            }
            else return string.Format("{0} B", fileSize);
        }
        public static String FormatFileSize(Int64 fileSize)
        {
            if (fileSize < 0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }
            else if (fileSize >= 1073741824)
            {
                return string.Format("{0:########0.00} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1048576)
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
            else if (fileSize >= 1073741824)
                return string.Format("{0:########0.0} GB", ((Double)fileSize) / (1024 * 1024 * 1024));         
            else if (fileSize >= 1048576)         
                return string.Format("{0:####0.0} MB", ((Double)fileSize) / (1024 * 1024));         
            else if (fileSize >= 1024)
                return string.Format("{0:####0.0} KB", ((Double)fileSize) / 1024);
            else return string.Format("{0} B", fileSize);
        }
        public static String FormatFileSizeMen(Int64 fileSize)
        {
            if (fileSize < 0)
                throw new ArgumentOutOfRangeException("fileSize");
            else if (fileSize >= 1073741824)
                return string.Format("{0:########0.0} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            else
            {
                if (fileSize >= 1048576) return string.Format("{0:####0.0} MB", ((Double)fileSize) / (1024 * 1024));
                else return "0.1 MB";
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
        private static extern void MSCM_ShowMenu(IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)] string serviceName, uint running, uint startType, [MarshalAs(UnmanagedType.LPWStr)] string path);


        #endregion

        #region SM API

        private EnumStartupsCallBack enumStartupsCallBack;
        private IntPtr enumStartupsCallBackPtr = IntPtr.Zero;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumStartupsCallBack(IntPtr name, IntPtr type, IntPtr path, IntPtr rootregpath, IntPtr regpath, IntPtr regvalue);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MEnumStartups(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern void MStartupsMgr_ShowMenu(IntPtr rootkey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)] string filepath, [MarshalAs(UnmanagedType.LPWStr)]string regvalue, uint id);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr MREG_ROOTKEYToStr(IntPtr krootkey);

        #endregion

        #region KRNL API

        private EnumKernelModulsCallBack enumKernelModulsCallBack;
        private IntPtr enumKernelModulsCallBackPtr = IntPtr.Zero;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnumKernelModulsCallBack(IntPtr kmi, IntPtr BaseDllName, IntPtr FullDllPath, IntPtr FullDllPathOrginal, IntPtr szEntryPoint, IntPtr SizeOfImage, IntPtr szDriverObject, IntPtr szBase, IntPtr szServiceName, uint Order);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool M_SU_EnumKernelModuls(IntPtr callback, bool showall);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void M_SU_EnumKernelModulsItemDestroy(IntPtr kmi);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void M_SU_EnumKernelModuls_ShowMenu(IntPtr kmi, bool showall);

        #endregion

        #endregion

        private string[] agrs = null;

        private bool processListInited = false;
        private bool driverListInited = false;
        private bool scListInited = false;
        private bool fileListInited = false;
        private bool startListInited = false;
        private bool uwpListInited = false;
        private bool perfInited = false;
        private bool perfMainInited = false;

        #region ProcessListWork

        private int nextSecType = -1;
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

        public PerformanceCounter performanceCounter_disk_system = null;
        public PerformanceCounter performanceCounter_cpu_system = null;

        private class PsItem
        {
            public IntPtr perfData;
            public IntPtr handle;
            public uint pid;
            public uint ppid;
            public string exename;
            public string exepath;
            public TaskMgrListItem item = null;
            public TaskMgrListItem hostitem = null;
            public bool isSvchost = false;
            public bool isUWP = false;
            public bool isWindowShow = false;
            public bool isWindowsProcess = false;

            public uwpitem uwpItem = null;
            public string uwpFullName;

            public PsItem parent = null;
            public List<PsItem> childs = new List<PsItem>();
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
        private class uwphostitem
        {
            public uwphostitem(uwpitem item, uint pid)
            {
                this.pid = pid;
                this.item = item;
            }

            public uwpitem item;
            public uint pid;
        }

        private uint explorerPid = 0;
        private bool is64OS = false;
        private bool isSelectExplorer = false;
        private List<uint> validPid = new List<uint>();
        private List<uwphostitem> uwpHostPid = new List<uwphostitem>();
        private List<PsItem> loadedPs = new List<PsItem>();
        private List<uwpitem> uwps = new List<uwpitem>();
        private List<uwpwinitem> uwpwins = new List<uwpwinitem>();
        private List<string> windowsProcess = new List<string>();

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
                                TaskMgrListItemChild c = new TaskMgrListItemChild(Marshal.PtrToStringAuto(data), icon != IntPtr.Zero ? Icon.FromHandle(icon) : PCMgr.Properties.Resources.icoShowedWindow);
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
            foreach (uwpwinitem u in uwpwins)
                if (u.title.Contains(dsbText))
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

        private uwphostitem ProcessListFindUWPItemWithHostId(uint pid)
        {
            uwphostitem rs = null;
            foreach (uwphostitem i in uwpHostPid)
            {
                if (i.pid == pid)
                {
                    rs = i;
                    break;
                }
            }
            return rs;
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
                if (i.PID == pid)
                {
                    rs = i;
                    return rs;
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

        private void ProcessListInitIn1Slater()
        {
            Timer t = new Timer();
            t.Interval = 50;
            t.Tick += ProcessListInitIn1T_Tick;
            t.Start();
        }

        private void ProcessListInitIn1T_Tick(object sender, EventArgs e)
        {
            (sender as Timer).Stop();
            ProcessListInit();
        }

        private void ProcessListInitLater()
        {
            if (!perfMainInited)
            {
                performanceCounter_cpu_total = new PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);
                performanceCounter_ram_total = new PerformanceCounter("Memory", "% Committed Bytes In Use", "", true);
                performanceCounter_disk_total = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total", true);
                performanceCounter_net_total = new PerformanceCounter("Network Interface", "Bytes Total/sec", "", true);
                string[] instanceNames = new PerformanceCounterCategory(performanceCounter_net_total.CategoryName).GetInstanceNames();
                performanceCounter_net_total.InstanceName = instanceNames[0];

                performanceCounter_cpu_system = new PerformanceCounter("Process", "% Processor Time", "System", true);
                performanceCounter_disk_system = new PerformanceCounter("Process", "IO Data Bytes/sec", "System", true);

                perfMainInited = true;
            }
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
                    ScMgrInit();
                if (SysVer.IsWin8Upper())
                    UWPListInit();

                ProcessListRefesh();

                DelingDialogInitHide();
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
            ProcessListRefeshEndGroupWork();
            lbProcessCount.Text = str_proc_count + " : " + listProcess.Items.Count;
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

            if (SysVer.IsWin8Upper()) MAppVProcessAllWindowsUWP();

            bool refeshAColumData = lvwColumnSorter.SortColumn == cpuindex
                || lvwColumnSorter.SortColumn == ramindex
                || lvwColumnSorter.SortColumn == diskindex
                || lvwColumnSorter.SortColumn == netindex
                || lvwColumnSorter.SortColumn == stateindex;
            ProcessListUpdateValues(refeshAColumData ? lvwColumnSorter.SortColumn : -1);

            ProcessListRefeshEndGroupWork();

            if (refeshAColumData)
                listProcess.Sort(false);
            listProcess.Locked = false;
            listProcess.SyncItems(true);

            lbProcessCount.Text = str_proc_count + " : " + listProcess.Items.Count;
        }
        private void ProcessListRefeshEndGroupWork()
        {
            /*foreach (PsItem p in loadedPs)
            { 
                if(p.isWindowShow)
                {
                    if (p.hostitem == null)
                    {

                    }
                }
            }*/
        }

        private void ProcessListLoad(uint pid, uint ppid, string exename, string exefullpath, IntPtr hprocess)
        {
            bool need_add_tolist = true;
            //base
            PsItem p = new PsItem();
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
            TaskMgrListItem taskMgrListItem;
            if (pid == 0) taskMgrListItem = new TaskMgrListItem(str_idle_process);
            else if (pid == 4) taskMgrListItem = new TaskMgrListItem("System");
            else if (pid == 88 && exename == "Registry") taskMgrListItem = new TaskMgrListItem("Registry");
            else if (exename == "Memory Compression") taskMgrListItem = new TaskMgrListItem("Memory Compression");
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
                        taskMgrListItem.Text = taskMgrListItem.Text + " (" + str_proc_32 + ")";
                }
            }

            p.item = taskMgrListItem;
            p.perfData = MPERF_PerfDataCreate();
            p.handle = hprocess;
            p.exename = exename;
            p.pid = pid;
            p.exepath = stringBuilder.ToString();
            p.isWindowsProcess = (pid == 0 || pid == 4
                            || (pid == 88 && exename == "Registry")
                            || (pid < 1024 && exename == "csrss.exe")
                            || exename == "Memory Compression"
                            || IsWindowsProcess(exefullpath));

            //Test service
            bool isSvcHoct = false;
            if (exefullpath != null && (exefullpath.ToLower() == @"c:\windows\system32\svchost.exe" || exefullpath.ToLower() == @"c:\windows\syswow64\svchost.exe") || exename == "svchost.exe")
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
                    if (p.svcs.Count == 1)
                    {
                        if (isSvcHoct)
                        {
                            if (!string.IsNullOrEmpty(p.svcs[0].groupName))
                                taskMgrListItem.Text = str_service_host + " : " + p.svcs[0].scName + " (" + ScGroupNameToFriendlyName(p.svcs[0].groupName) + ")";
                            else taskMgrListItem.Text = str_service_host + " : " + p.svcs[0].scName;
                        }
                    }
                    else
                    {
                        if (isSvcHoct)
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
                        taskMgrListItem.Childs.Add(tx);
                    }
                    p.isSvchost = true;
                }
            }

            if (exefullpath != null && (exefullpath.ToLower() == @"‪c:\windows\explorer.exe" || (exename != null && exename.ToLower() == @"‪explorer.exe")))
                explorerPid = pid;

            //ps data item
            if (SysVer.IsWin8Upper())
                p.isUWP = hprocess == IntPtr.Zero ? false : MGetProcessIsUWP(hprocess);

            taskMgrListItem.Tag = p;

            //10 empty item
            for (int i = 0; i < 10; i++) taskMgrListItem.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());

            //UWP app

            uwphostitem hostitem = null;
            if (p.isUWP)
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

                                        if (ProcessListFindUWPItemWithHostId(p.pid) == null) uwpHostPid.Add(new uwphostitem(parentItem, p.pid));

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
                                        g.PID = (uint)1;
                                        //10 empty item
                                        for (int i = 0; i < 10; i++) g.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listProcess.Font });

                                        if (nameindex != -1) g.SubItems[nameindex].Text = p.uwpFullName;
                                        if (pathindex != -1) g.SubItems[pathindex].Text = uapp.SubItems[4].Text;

                                        g.Tag = uapp.SubItems[4].Text;

                                        parentItem.uwpFullName = p.uwpFullName;
                                        parentItem.uwpItem = g;
                                        p.uwpItem = parentItem;

                                        if (ProcessListFindUWPItemWithHostId(p.pid) == null) uwpHostPid.Add(new uwphostitem(parentItem, p.pid));

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
            if (need_add_tolist)
            {
                hostitem = ProcessListFindUWPItemWithHostId(ppid);
                //UWP app childs
                if (hostitem != null)
                {
                    hostitem.item.uwpItem.Items.Add(taskMgrListItem);
                    need_add_tolist = false;
                }
            }

            //data items

            if (nameindex != -1)
            {
                if (pid == 0) taskMgrListItem.SubItems[nameindex].Text = str_idle_process;
                else if (pid == 4) taskMgrListItem.SubItems[nameindex].Text = "ntoskrnl.exe";
                else taskMgrListItem.SubItems[nameindex].Text = exename;
            }
            if (pidindex != -1) {
                taskMgrListItem.SubItems[pidindex].Text = pid.ToString();
            }
            if (pathindex != -1) if (stringBuilder.ToString() != "") taskMgrListItem.SubItems[pathindex].Text = stringBuilder.ToString();
            if (cmdindex != -1)
            {
                if (hprocess != IntPtr.Zero)
                {
                    StringBuilder s = new StringBuilder(2048);
                    if (MGetProcessCommandLine(hprocess, s, 2048))
                        taskMgrListItem.SubItems[cmdindex].Text = s.ToString();
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
            if (eprocessindex != -1)
            {
                StringBuilder eprocess = new StringBuilder(32);
                if (MGetProcessEprocess(pid, eprocess, 32))
                    taskMgrListItem.SubItems[eprocessindex].Text = eprocess.ToString();
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
            if (need_add_tolist) listProcess.Items.Add(taskMgrListItem);
            ProcessListUpdate(pid, true, taskMgrListItem);
        }
        private void ProcessListUpdate(uint pid, bool isload, TaskMgrListItem it, int ipdateOneDataCloum = -1)
        {
            if (it.IsGroup || it.IsAppHost)
            {
                //Group uppdate
                ProcessListUpdate_GroupChilds(isload, it, ipdateOneDataCloum);

                if (stateindex != -1 && ipdateOneDataCloum != stateindex && it.Items.Count > 0) it.SubItems[stateindex].Text = it.Items[0].SubItems[stateindex].Text;

                if (!it.IsAppHost)
                {
                    bool running = ProcessListGetUwpIsRunning(it.Text);
                    if (running && stateindex != -1) if (it.SubItems[stateindex].Text == str_status_paused) running = false;
                    it.Group = running ? listProcess.Groups[0] : listProcess.Groups[1];
                }

                //Performance 

                if (cpuindex != -1 && ipdateOneDataCloum != cpuindex)
                {
                    double d = 0; int datacount = 0;
                    foreach (TaskMgrListItem ix in it.Items)
                    {
                        d += ix.SubItems[cpuindex].CustomData;
                        datacount++;
                    }
                    double ii2 = (d / datacount);
                    it.SubItems[cpuindex].Text = ii2.ToString("0.0") + "%";
                    it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii2, 100);
                    it.SubItems[cpuindex].CustomData = ii2;
                }
                if (ramindex != -1 && ipdateOneDataCloum != ramindex)
                {
                    double d = 0;
                    foreach (TaskMgrListItem ix in it.Items)
                        d += ix.SubItems[ramindex].CustomData;
                    it.SubItems[ramindex].Text = FormatFileSizeMen(Convert.ToInt64(d * 1024));
                    it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(d / 1024, 1024);
                    it.SubItems[ramindex].CustomData = d;
                }
                if (diskindex != -1 && ipdateOneDataCloum != diskindex)
                {
                    double d = 0;
                    foreach (TaskMgrListItem ix in it.Items)
                        d += ix.SubItems[diskindex].CustomData;
                    it.SubItems[diskindex].Text = d.ToString("0.0") + " MB/" + str_sec;
                    it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(d, 1024);
                    it.SubItems[diskindex].CustomData = d;
                }
                if (netindex != -1 && ipdateOneDataCloum != netindex)
                {

                }
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
                    if (it.Childs.Count == 0)
                        it.Group = listProcess.Groups[1];
                    else
                    {
                        p.isWindowShow = true;
                        it.Group = listProcess.Groups[0];
                    }
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
                        else if (p.isWindowsProcess)
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

                if (stateindex != -1 && ipdateOneDataCloum != stateindex) ProcessListUpdate_State(pid, it, p.handle, p);
                if (cpuindex != -1 && ipdateOneDataCloum != cpuindex) ProcessListUpdatePerf_Cpu(pid, it, p.handle, p);
                if (ramindex != -1 && ipdateOneDataCloum != ramindex) ProcessListUpdatePerf_Ram(pid, it, p.handle);
                if (diskindex != -1 && ipdateOneDataCloum != diskindex) ProcessListUpdatePerf_Disk(pid, it, p.handle);
                if (netindex != -1 && ipdateOneDataCloum != netindex) ProcessListUpdatePerf_Net(pid, it, p.handle);
            }
        }
        private void ProcessListUpdateOnePerfCloum(uint pid, TaskMgrListItem it, int ipdateOneDataCloum)
        {
            if (it.IsGroup || it.IsAppHost)
            {
                TaskMgrListItem ii = it as TaskMgrListItem;
                if (stateindex != -1 && ipdateOneDataCloum == stateindex)
                {
                    if (!it.IsAppHost)
                    {
                        bool running = ProcessListGetUwpIsRunning(it.Text);
                        if (running && stateindex != -1) if (it.SubItems[stateindex].Text == str_status_paused) running = false;
                        it.Group = running ? listProcess.Groups[0] : listProcess.Groups[1];
                    }
                    else
                    {
                        if (stateindex != -1 && ipdateOneDataCloum == stateindex && it.Items.Count > 0)
                        {
                            if (it.Items.Count > 0)
                            {
                                PsItem p = ((PsItem)it.Items[0].Tag);
                                ProcessListUpdate_State(p.pid, it.Items[0], p.handle, p);
                                it.SubItems[stateindex].Text = it.Items[0].SubItems[stateindex].Text;
                            }
                        }
                    }
                }
                if (ipdateOneDataCloum > -1)
                {
                    double d = 0; int datacount = 0;
                    foreach (TaskMgrListItem ix in ii.Items)
                    {
                        ProcessListUpdateOnePerfCloum(ix.PID, ix, ipdateOneDataCloum);
                        d += ix.SubItems[ipdateOneDataCloum].CustomData;
                        datacount++;
                    }

                    //Performance 
                    if (cpuindex != -1 && ipdateOneDataCloum == cpuindex)
                    {
                        double ii2 = (d / datacount);
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
                        it.SubItems[diskindex].Text = d.ToString("0.0") + " MB/" + str_sec;
                        it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(d, 1024);
                        it.SubItems[diskindex].CustomData = d;
                    }
                    else if (netindex != -1 && ipdateOneDataCloum == netindex)
                    {

                    }
                }
            }
            else
            {
                PsItem p = ((PsItem)it.Tag);
                if (stateindex != -1 && ipdateOneDataCloum == stateindex) ProcessListUpdate_State(pid, it, p.handle, p);
                if (cpuindex != -1 && ipdateOneDataCloum == cpuindex) ProcessListUpdatePerf_Cpu(pid, it, p.handle, p);
                if (ramindex != -1 && ipdateOneDataCloum == ramindex) ProcessListUpdatePerf_Ram(pid, it, p.handle);
                if (diskindex != -1 && ipdateOneDataCloum == diskindex) ProcessListUpdatePerf_Disk(pid, it, p.handle);
                if (netindex != -1 && ipdateOneDataCloum == netindex) ProcessListUpdatePerf_Net(pid, it, p.handle);
            }
        }
        private void ProcessListUpdate_GroupChilds(bool isload, TaskMgrListItem ii, int ipdateOneDataCloum = -1)
        {
            foreach (TaskMgrListItem ix in ii.Items) ProcessListUpdate(ix.PID, isload, ix, ipdateOneDataCloum);
            if (ii.Items.Count > 0) ProcessListUpdate(ii.Items[0].PID, isload, ii.Items[0], ipdateOneDataCloum);
        }
        private void ProcessListUpdate_State(uint pid, TaskMgrListItem it, IntPtr handle, PsItem p)
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
                        it.SubItems[stateindex].Text = str_status_hung;
                        it.SubItems[stateindex].ForeColor = Color.FromArgb(219, 107, 58);
                    }
                }
            }
            else if (i == 2)
            {
                it.SubItems[stateindex].Text = str_status_paused;
                it.SubItems[stateindex].ForeColor = Color.FromArgb(22, 158, 250);
            }
        }
        private void ProcessListUpdatePerf_Cpu(uint pid, TaskMgrListItem it, IntPtr handle, PsItem p)
        {
            if (handle != IntPtr.Zero)
            {
                double ii = MPERF_GetProcessCpuUseAge(handle, p.perfData);
                it.SubItems[cpuindex].Text = ii.ToString("0.0") + "%";
                it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii, 100);
                it.SubItems[cpuindex].CustomData = ii;
            }
            else if (pid == 4)
            {
                if (performanceCounter_cpu_system != null)
                {
                    double ii = performanceCounter_cpu_system.NextValue();
                    it.SubItems[cpuindex].Text = ii.ToString("0.0") + "%";
                    it.SubItems[cpuindex].BackColor = ProcessListGetColorFormValue(ii, 100);
                    it.SubItems[cpuindex].CustomData = ii;
                }
            }
        }
        private void ProcessListUpdatePerf_Ram(uint pid, TaskMgrListItem it, IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                uint ii = MPERF_GetProcessRam(handle);
                it.SubItems[ramindex].Text = FormatFileSizeMen(Convert.ToInt64(ii));
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(ii / 1048576, 1024);
                it.SubItems[ramindex].CustomData = ii / 1024d;
            }
            else if (pid == 4)
            {
                it.SubItems[ramindex].Text = "0.1 MB";
                it.SubItems[ramindex].BackColor = ProcessListGetColorFormValue(0.1, 1024);
                it.SubItems[ramindex].CustomData = 1;
            }
        }
        private void ProcessListUpdatePerf_Disk(uint pid, TaskMgrListItem it, IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                PsItem p = ((PsItem)it.Tag);
                ulong disk = MPERF_GetProcessDiskRate(handle, p.perfData);
                it.SubItems[diskindex].Text = (disk / 1024d).ToString("0.0") + " MB/" + str_sec;
                it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(disk, 1048576);
                it.SubItems[diskindex].CustomData = (disk / 1024d);
            }
            else if (pid == 4)
            {
                if (performanceCounter_disk_system != null)
                {
                    double disk = performanceCounter_disk_system.NextValue();
                    it.SubItems[diskindex].Text = ((disk / 1024) / 1024d).ToString("0.0") + " MB/" + str_sec;
                    it.SubItems[diskindex].BackColor = ProcessListGetColorFormValue(disk / 1024, 1048576);
                    it.SubItems[diskindex].CustomData = (disk / 1024d);
                }
            }
        }
        private void ProcessListUpdatePerf_Net(uint pid, TaskMgrListItem it, IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {

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
                if (i is TaskMgrListItemGroup)
                {
                    TaskMgrListItemGroup ii = i as TaskMgrListItemGroup;
                    foreach (TaskMgrListItem ix in ii.Items)
                        validPid.Remove(ix.PID);
                    if (ii.PID != 1) validPid.Remove(i.PID);
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

            uwphostitem hostitem = ProcessListFindUWPItemWithHostId(it.pid);
            if (hostitem != null) uwpHostPid.Remove(hostitem);

            MPERF_PerfDataDestroy(it.perfData);
            it.svcs.Clear();
            loadedPs.Remove(it);
            if (it.parent != null)
                it.parent.childs.Remove(it);
            if (it.uwpItem != null)
                it.uwpItem = null;
            if (li != null)
            {
                //is a group item
                if (li.Parent != null)
                {
                    TaskMgrListItem iii = li.Parent;
                    iii.Items.Remove(li);
                    if (iii.Items.Count == 0)//o to remove
                    {
                        listProcess.Items.Remove(iii);
                        uwpitem parentItem = ProcessListFindUWPItem(iii.Tag.ToString());
                        if (parentItem != null) uwps.Remove(parentItem);
                    }
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
        private void ProcessListEndTask(uint pid, TaskMgrListItem taskMgrListItem)
        {
            if (taskMgrListItem == null) taskMgrListItem = ProcessListFindItem(pid);
            if (taskMgrListItem != null)
            {
                PsItem p = taskMgrListItem.Tag as PsItem;
                if (p.isWindowShow && !p.isSvchost)
                {
                    if (taskMgrListItem.Childs.Count > 0)
                    {
                        IntPtr target = IntPtr.Zero;
                        foreach (TaskMgrListItemChild c in taskMgrListItem.Childs)
                            if (c.Tag != null)
                            {
                                target = (IntPtr)c.Tag;
                                break;
                            }
                        if (target != IntPtr.Zero) MAppWorkCall3(192, IntPtr.Zero, target);
                        return;
                    }
                }
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
            if (listProcess.SelectedItem == null) return;

            if (listProcess.SelectedItem.OldSelectedItem == null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (listProcess.SelectedItem.IsGroup)
                    {
                        string exepath = listProcess.SelectedItem.Tag.ToString();
                        MAppWorkShowMenuProcessPrepare(exepath, null, 0);
                        btnEndProcess.Enabled = false;
                    }
                    else
                    {
                        PsItem t = (PsItem)listProcess.SelectedItem.Tag;
                        if (t.pid > 4)
                        {
                            btnEndProcess.Enabled = true;
                            MAppWorkShowMenuProcessPrepare(t.exepath, t.exename, t.pid);

                            if (t.exename.ToLower() == "explorer.exe")
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
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (listProcess.SelectedItem.IsGroup)
                        MAppWorkShowMenuProcess(listProcess.SelectedItem.Tag.ToString(), null, 0, Handle, 0, nextSecType);
                    else
                    {
                        PsItem t = (PsItem)listProcess.SelectedItem.Tag;
                        int rs = MAppWorkShowMenuProcess(t.exepath, t.exename, t.pid, Handle, isSelectExplorer ? 1 : 0, nextSecType);
                    }
                }
            }
            else if (listProcess.SelectedItem.OldSelectedItem != null)
            {
                if (e.Button == MouseButtons.Right)
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
                else if (e.Button == MouseButtons.Left)
                {
                    btnEndProcess.Enabled = false;
                    PsItem t = (PsItem)listProcess.SelectedItem.Tag;
                    if (t.isSvchost)
                    {
                        IntPtr scname = Marshal.StringToHGlobalUni((string)listProcess.SelectedItem.OldSelectedItem.Tag);
                        MAppWorkCall3(197, IntPtr.Zero, scname);
                        Marshal.FreeHGlobal(scname);
                    }
                    else MAppWorkCall3(198, IntPtr.Zero, (IntPtr)listProcess.SelectedItem.OldSelectedItem.Tag);
                }
            }
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
        }

        private void btnEndProcess_Click(object sender, EventArgs e)
        {
            TaskMgrListItem taskMgrListItem = listProcess.SelectedItem;
            if (taskMgrListItem != null)
                ProcessListEndTask(0, taskMgrListItem);
            MAppWorkCall3(190, Handle, IntPtr.Zero);
        }
        private void lbShowDetals_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //if (!MAppVProcess(Handle)) TaskDialog.Show("无法打开详细信息窗口", str_AppTitle, "未知错误。", TaskDialogButton.OK, TaskDialogIcon.Stop);
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
            li.TextSmall = LanuageMgr.GetStr(name);
            li.FuckYou = name;
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
        int eprocessindex = 0;
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
                        case 1: {
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
                else if (File.Exists(textBoxFmCurrent.Text))
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
            if (e.Node.Nodes.Count == 0 || e.Node.Nodes[0].Text == str_loading && e.Node.Tag != null)
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
                        else if (File.Exists(path))
                        {
                            if (TaskDialog.Show(str_OpenAsk, str_AskTitle, str_PathStart + path, TaskDialogButton.Yes | TaskDialogButton.No) == Result.Yes)
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

        public class ListViewItemComparer : IComparer
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
            foreach(ListViewItem li in listService.Items)
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

                    ScMgrRefeshList();
                    scCanUse = true;
                }

                icoSc = new Icon(PCMgr.Properties.Resources.icoService, 16, 16);

                listService.ListViewItemSorter = listViewItemComparerSc;

                scListInited = true;
            }
        }
        private void ScMgrRefeshList()
        {
            scValidPid.Clear();
            runningSc.Clear();
            listService.Items.Clear();
            MEnumServices(scMgrEnumServicesCallBackPtr);
            lbServicesCount.Text = LanuageMgr.GetStr("ServiceCount") + " : " + (listService.Items.Count == 0 ? "--" : listService.Items.Count.ToString());
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
                    if (highlight_nosystem && exeCompany.ToString() != MICOSOFT)
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
        private void listService_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparer)listService.ListViewItemSorter).Asdening = !((ListViewItemComparer)listService.ListViewItemSorter).Asdening;
            ((ListViewItemComparer)listService.ListViewItemSorter).SortColum = e.Column;
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
            if (!uwpListInited)
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
                catch (Exception e)
                {
                    listUwpApps.Hide();
                    pl_UWPEnumFailTip.Show();
                    lbUWPEnumFailText.Text = LanuageMgr.GetStr("UWPEnumFail") + "\n\n" + e.ToString();
                }
                uwpListInited = true;
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

        PerformanceListItem perf_cpu = new PerformanceListItem();
        PerformanceListItem perf_ram = new PerformanceListItem();

        private class PerfItemHeader
        {
            public PerformanceListItem item = null;
            public PerformanceCounter performanceCounter = null;
            public float x = 1;
        }

        private List<PerfItemHeader> perfItems = new List<PerfItemHeader>();
        private List<IPerformancePage> perfPages = new List<IPerformancePage>();

        private IPerformancePage currSelectedPerformancePage = null;

        private void performanceLeftList_SelectedtndexChanged(object sender, EventArgs e)
        {
            if (performanceLeftList.Selectedtem == perf_cpu)
                PerfPagesTo(0);
            else if (performanceLeftList.Selectedtem == perf_ram)
                PerfPagesTo(1);
            else if (performanceLeftList.Selectedtem.PageIndex != 0)
                PerfPagesTo(performanceLeftList.Selectedtem.PageIndex);
            else
                PerfPagesToNull();
        }
        private void PerfPagesToNull()
        {
            if (currSelectedPerformancePage != null)
                currSelectedPerformancePage.PageHide();
            currSelectedPerformancePage = null;
        }
        private void PerfPagesTo(int index)
        {
            if (currSelectedPerformancePage != null)
                currSelectedPerformancePage.PageHide();
            currSelectedPerformancePage = null;
            currSelectedPerformancePage = perfPages[index];
            currSelectedPerformancePage.PageShow();
        }
        private void PerfPagesAddToCtl(Control c)
        {
            c.Visible = false;
            splitContainerPerfCtls.Panel2.Controls.Add(c);
            c.Size = new Size(splitContainerPerfCtls.Panel2.Width - 30, splitContainerPerfCtls.Panel2.Height - 30);
            c.Location = new Point(15, 15);
            c.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top;
        }
        private void PerfPagesInit()
        {
            PerformancePageCpu performanceCpu = new PerformancePageCpu(performanceCounter_cpu_total);
            PerfPagesAddToCtl(performanceCpu);
            perfPages.Add(performanceCpu);

            PerformancePageRam performanceRam = new PerformancePageRam();
            PerfPagesAddToCtl(performanceRam);
            perfPages.Add(performanceRam);
        }
        private void PerfInit()
        {
            if (!perfInited)
            {
                perf_cpu.Name = "CPU";
                perf_cpu.SmallText = "0 %";
                perf_cpu.BasePen = new Pen(CpuDrawColor, 2);
                perf_cpu.BgBrush = new SolidBrush(CpuBgColor);
                performanceLeftList.Items.Add(perf_cpu);

                perf_ram.Name = LanuageMgr.GetStr("TitleRam");
                perf_ram.SmallText = "0 %";
                perf_ram.BasePen = new Pen(RamDrawColor, 2);
                perf_ram.BgBrush = new SolidBrush(RamBgColor);
                performanceLeftList.Items.Add(perf_ram);

                PerfPagesInit();

                string[] disk_instanceNames = new PerformanceCounterCategory("PhysicalDisk").GetInstanceNames();
                foreach (string s in disk_instanceNames)
                {
                    if (s != "_Total")
                    {
                        PerfItemHeader perfItemHeader = new PerfItemHeader();
                        perfItemHeader.performanceCounter = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "", true);
                        perfItemHeader.performanceCounter.InstanceName = s;
                        perfItemHeader.item = new PerformanceListItem();
                        perfItemHeader.item.Name = LanuageMgr.GetStr("TitleDisk") + s;
                        perfItemHeader.item.BasePen = new Pen(DiskDrawColor);
                        perfItemHeader.item.BgBrush = new SolidBrush(DiskBgColor);
                        perfItems.Add(perfItemHeader);

                        PerformancePageDisk performancedisk = new PerformancePageDisk(s);
                        PerfPagesAddToCtl(performancedisk);
                        perfPages.Add(performancedisk);

                        perfItemHeader.item.PageIndex = perfPages.Count - 1;


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
                    perfItemHeader.item.Name = LanuageMgr.GetStr("TitleNet");
                    perfItemHeader.item.BasePen = new Pen(NetDrawColor);
                    perfItemHeader.item.BgBrush = new SolidBrush(NetBgColor);
                    perfItemHeader.x = 0.0001f;
                    perfItems.Add(perfItemHeader);

                    PerformancePageNet performancedisk = new PerformancePageNet(s);
                    PerfPagesAddToCtl(performancedisk);
                    perfPages.Add(performancedisk);

                    perfItemHeader.item.PageIndex = perfPages.Count - 1;

                    performanceLeftList.Items.Add(perfItemHeader.item);
                }

                performanceLeftList.UpdateAll();
                performanceLeftList.Invalidate();

                PerfPagesTo(0);

                perfInited = true;
            }
        }
        private void PerfUpdate()
        {
            foreach (PerfItemHeader h in perfItems)
            {
                float data = (h.performanceCounter.NextValue() * h.x);
                h.item.SmallText = data.ToString("0.0") + "%";
                h.item.AddData((int)data);
            }

            if (currSelectedPerformancePage != null)
                currSelectedPerformancePage.PageUpdate();
        }
        private void PerfClear()
        {
            foreach (IPerformancePage h in perfPages)
                h.PageDelete();
            perfPages.Clear();
            foreach (PerfItemHeader h in perfItems)
            {
                h.performanceCounter.Close();
                h.item = null;
            }
            perfItems.Clear();
        }

        #endregion

        #region LanuageWork

        private static string str_idle_process = "";
        private static string str_service_host = "";
        private static string str_status_paused = "";
        private static string str_status_hung = "";
        private static string str_status_running = "";
        private static string str_status_stopped = "";

        public static string str_proc_count = "";
        private static string str_proc_32 = "";
        private static string str_get_failed = "";
        public static string str_sec = "";
        public static string str_loading = "";
        public static string str_frocedelsuccess = "";
        public static string str_filldatasuccess = "";
        public static string str_filldatafailed = "";
        public static string str_getfileinfofailed = "";
        public static string str_filenotexist = "";
        public static string str_failed = "";

        private static string str_endproc = "";
        private static string str_endtask = "";
        private static string str_resrat = "";

        private static string str_VisitFolderFailed = "";
        public static string str_TipTitle = "";
        public static string str_ErrTitle = "";
        public static string str_AskTitle = "";
        public static string str_PathUnExists = "";
        private static string str_PleaseEnterPath = "";
        private static string str_Ready = "";
        private static string str_ReadyStatus = "";
        private static string str_ReadyStatusEnd = "";
        private static string str_ReadyStatusEnd2 = "";
        private static string str_FileCuted = "";
        private static string str_FileCopyed = "";
        private static string str_NewFolderFailed = "";
        private static string str_NewFolderSuccess = "";
        private static string str_PathCopyed = "";
        private static string str_FolderCuted = "";
        private static string str_FolderCopyed = "";
        private static string str_FolderHasExist = "";
        private static string str_OpenAsk = "";
        private static string str_PathStart = "";
        private static string str_DriverLoad = "";
        private static string str_AutoStart = "";
        private static string str_DemandStart = "";
        private static string str_Disabled = "";
        private static string str_FileSystem = "";
        private static string str_KernelDriver = "";
        private static string str_UserService = "";
        private static string str_SystemService = "";
        private static string str_InvalidFileName = "";
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
        private static string str_DriverCount = "";
        public static string str_FileNotExist = "";
        private static string str_DriverCountLoaded = "";
        public static string str_AppTitle = "";
        public static string str_FileTrust = "";

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
            if (lanuage != "" && lanuage != "zh" && lanuage != "zh-CN") System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lanuage);

            MLG_SetLanuageRes(Application.StartupPath, lanuage);
        }
        public static void InitLanuageItems()
        {
            try
            {
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
                str_AutoStart = LanuageMgr.GetStr("AutoStart ");
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

                MAppSetLanuageItems(0, 0, LanuageMgr.GetStr("KillAskStart"), 0);
                MAppSetLanuageItems(0, 1, LanuageMgr.GetStr("KillAskEnd"), 0);
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
            if(!startListInited)
            {
                enumStartupsCallBack = StartMList_CallBack;
                enumStartupsCallBackPtr = Marshal.GetFunctionPointerForDelegate(enumStartupsCallBack);
                StartMListRefesh();
                startListInited = true;
            }
        }
        private void StartMListRefesh()
        {
            listStartup.Items.Clear();
            startId = 0;

            knowDlls = new TaskMgrListItemGroup("Know Dlls");
            knowDlls.IsGroup = true;
            knowDlls.Text = "Know Dlls";
            knowDlls.Image = imageListFileTypeList.Images[".dll"];
            knowDlls.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());

            listStartup.Items.Add(knowDlls);
            MEnumStartups(enumStartupsCallBackPtr);
        }
        private void StartMList_CallBack(IntPtr name, IntPtr type, IntPtr path, IntPtr rootregpath, IntPtr regpath, IntPtr regvalue)
        {
            bool settoblue = false;
            TaskMgrListItem li = new TaskMgrListItem(Marshal.PtrToStringUni(name));
            for (int i = 0; i < 5; i++) li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem() { Font = listStartup.Font });

            li.SubItems[0].Text = li.Text;
            // li.SubItems[1].Text = Marshal.PtrToStringUni(type);
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
                    if (File.Exists(pathstr))
                    {
                        li.Icon = Icon.FromHandle(MGetExeIcon(pathstr));
                        StringBuilder exeCompany = new StringBuilder(256);
                        if (MGetExeCompany(pathstr, exeCompany, 256))
                        {
                            li.SubItems[3].Text = exeCompany.ToString();
                            if (highlight_nosystem && li.SubItems[3].Text != MICOSOFT)
                                settoblue = true;
                        }
                        else if (highlight_nosystem)
                            settoblue = true;
                    }
                    else if (File.Exists(@"C:\Windows\System32\" + pathstr))
                    {
                        li.Icon = Icon.FromHandle(MGetExeIcon(@"C:\Windows\System32\" + pathstr));
                        StringBuilder exeCompany = new StringBuilder(256);
                        if (MGetExeCompany(@"C:\Windows\System32\" + pathstr, exeCompany, 256))
                        {
                            li.SubItems[3].Text = exeCompany.ToString();
                            if (highlight_nosystem && li.SubItems[3].Text != MICOSOFT)
                                settoblue = true;
                        }
                        else if (highlight_nosystem)
                            settoblue = true;
                    }
                    else if (File.Exists(@"C:\Windows\SysWOW64\" + pathstr))
                    {
                        li.Icon = Icon.FromHandle(MGetExeIcon(@"C:\Windows\SysWOW64\" + pathstr));
                        StringBuilder exeCompany = new StringBuilder(256);
                        if (MGetExeCompany(@"C:\Windows\SysWOW64\" + pathstr, exeCompany, 256))
                        {
                            li.SubItems[3].Text = exeCompany.ToString();
                            if (highlight_nosystem && li.SubItems[3].Text != MICOSOFT)
                                settoblue = true;
                        }
                        else if (highlight_nosystem)
                            settoblue = true;
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
                li.Image =  imageListFileTypeList.Images[".dll"];
                knowDlls.Items.Add(li);
            }
            else
            {
                if (settoblue)
                    for (int i = 0; i < 5; i++)
                        li.SubItems[i].ForeColor = Color.Blue;
                listStartup.Items.Add(li);
            }
        }
        private void StartMListRemoveItem(uint id)
        {
            TaskMgrListItem target = null;
            foreach (TaskMgrListItem li in listStartup.Items)
            {
                startitem item = (startitem)li.Tag;
                if (item.id == id)
                {
                    target = li;
                    break;
                }
            }
            if (target != null)
                listStartup.Items.Remove(target);
        }

        private void listStartup_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listStartup.SelectedItem != null)
                {
                    startitem item = (startitem)listStartup.SelectedItem.Tag;
                    MStartupsMgr_ShowMenu(item.rootregpath, item.path, item.filepath, item.valuename, item.id);
                }
            }
        }

        #endregion

        #region KernelMWork

        private ListViewItemComparer listViewItemComparerKr = new ListViewItemComparer();
        private bool showAllDriver = false;

        private void KernelListInit()
        {
            if(!driverListInited)
            {
                if (MCanUseKernel())
                {
                    enumKernelModulsCallBack = KernelEnumCallBack;
                    enumKernelModulsCallBackPtr = Marshal.GetFunctionPointerForDelegate(enumKernelModulsCallBack);

                    listDrivers.ListViewItemSorter = listViewItemComparerKr;

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
            if(Order==9999)
            {
                if (showAllDriver) lbDriversCount.Text = str_DriverCountLoaded + kmi.ToInt32() + "  " + str_DriverCount + BaseDllName.ToInt32();
                else lbDriversCount.Text = str_DriverCount + kmi.ToInt32();
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
                    if (highlight_nosystem && exeCompany.ToString() != MICOSOFT)
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
            if (MCanUseKernel())
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
        private void listDrivers_MouseUp(object sender, MouseEventArgs e)
        {
            if (listDrivers.SelectedItems.Count > 0)
            {
                if (e.Button == MouseButtons.Right) M_SU_EnumKernelModuls_ShowMenu((IntPtr)listDrivers.SelectedItems[0].Tag, showAllDriver);
            }
        }
        private void listDrivers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparer)listDrivers.ListViewItemSorter).Asdening = !((ListViewItemComparer)listDrivers.ListViewItemSorter).Asdening;
            ((ListViewItemComparer)listDrivers.ListViewItemSorter).SortColum = e.Column;
            listDrivers.Sort();
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
        }
        private void DelingDialogClose()
        {
            delingdialog.Close();
            delingdialog = null;
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

        private string _FileTrustedLinkLastFile = "";

        private void FileTrustedLink_HyperlinkClick(object sender, HyperlinkEventArgs e)
        {
            if (!string.IsNullOrEmpty(_FileTrustedLinkLastFile))
            {
                
            }
        }

        #endregion

        private void BaseProcessRefeshTimer_Tick(object sender, EventArgs e)
        {
            if (!perfMainInited) ProcessListInitLater();
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
                    listProcess.Colunms[diskindex].TextBig = ((int)(performanceCounter_disk_total.NextValue() / 10)) + "%";
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
                int cpu = (int)(performanceCounter_cpu_total.NextValue());
                perf_cpu.SmallText = cpu + " %";
                perf_cpu.AddData(cpu);

                perfPages[0].PageFroceSetData(cpu);

                int ram = (int)(MGetRamUseAge() * 100);
                perf_ram.SmallText = ram + " %";
                perf_ram.AddData(ram);

                if (perfPages[1] != currSelectedPerformancePage)
                    perfPages[1].PageFroceSetData(ram);

                PerfUpdate();

                performanceLeftList.Invalidate();
            }
        }

        private bool exitCalled = false;

        private void AppWorkerCallBack(int msg, IntPtr wParam, IntPtr lParam)
        {
            switch(msg)
            {
                case 5:
                    {
                        int c = wParam.ToInt32();
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
                        int c = wParam.ToInt32();
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
                        int c = wParam.ToInt32();
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
                        int c = wParam.ToInt32();
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
                case 10:
                    {
                        ScMgrRefeshList();
                        break;
                    }
                case 11:
                    {

                        break;
                    }
                case 12:
                    {
                        new FormSpyWindow(wParam).ShowDialog();
                        break;
                    }
                case 13:
                    {
                        new FormFileTool().ShowDialog();
                        break;
                    }
                case 14:
                    {
                        new FormAbout().ShowDialog();
                        break;
                    }
                case 15:
                    {
                        uint pid = Convert.ToUInt32(wParam.ToInt32());
                        ProcessListEndTask(pid, null);
                        break;
                    }
                case 16:
                    {
                        new FormLoadDriver().Show();
                        break;
                    }
                case 17:
                    {

                        break;
                    }
                case 18:
                    {
                        ShowHideDelingDialog(true);
                        break;
                    }
                case 19:
                    {
                        ShowHideDelingDialog(false);
                        DelingDialogUpdate(str_DeleteFiles, 0);
                        break;
                    }
                case 20:
                    {
                        DelingDialogUpdate(Marshal.PtrToStringUni(wParam), lParam.ToInt32());
                        break;
                    }
                case 21:
                    {
                        DelingDialogUpdate(str_CollectingFiles, -1);
                        break;
                    }
                case 22:
                    {
                        if (MInitKernel(Application.StartupPath))
                            if (GetConfigBool("SelfProtect", "AppSetting"))
                                MAppWorkCall3(203, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 23:
                    {
                        new FormVHandles(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog();
                        break;
                    }
                case 24:
                    {
                        KernelListInit();
                        break;
                    }
                case 25:
                    {
                        showAllDriver = !showAllDriver;
                        KernelLisRefesh();
                        break;
                    }
                case 26:
                    {
                        StartMListRemoveItem(Convert.ToUInt32(wParam.ToInt32()));
                        break;
                    }
                case 27:
                    {
                        new FormKrnInfo(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog();
                        break;
                    }
                case 28:
                    {
                        //timer
                        break;
                    }
                case 29:
                    {
                        //hotkey
                        break;
                    }
                case 30:
                    {
                        string path = Marshal.PtrToStringUni(wParam);
                        _FileTrustedLinkLastFile = path;
                        TaskDialog d = new TaskDialog("", str_TipTitle, (path == null ? "" : (path + "\n")) + str_FileTrust);
                        d.EnableHyperlinks = true;
                        d.HyperlinkClick += FileTrustedLink_HyperlinkClick;
                        d.Show(this);
                        break;
                    }
                case 31:
                    {
                        break;
                    }
                case 32:
                    {
                        break;
                    }
                case 33:
                    {

                        break;
                    }
            }
        }
        private void AppLoad()
        {
            Log("Loading callbacks...");

            exitCallBack = AppExit;
            taskDialogCallBack = TaskDialogCallback;
            enumProcessCallBack = ProcessListHandle;
            enumWinsCallBack = MainEnumWinsCallBack;
            getWinsCallBack = MainGetWinsCallBack;
            enumProcessCallBack2 = ProcessListHandle2;
            workerCallBack = AppWorkerCallBack;
            lanuageItems_CallBack = Native_LanuageItems_CallBack;

            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(exitCallBack), 1);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(taskDialogCallBack), 2);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(enumWinsCallBack), 3);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(getWinsCallBack), 4);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(workerCallBack), 5);
            MLG_SetLanuageItems_CallBack(Marshal.GetFunctionPointerForDelegate(lanuageItems_CallBack));

            MAppWorkCall3(181, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(183, Handle, IntPtr.Zero);
            coreWndProc = (WNDPROC)Marshal.GetDelegateForFunctionPointer(MAppSetCallBack(IntPtr.Zero, 0), typeof(WNDPROC));

            IntPtr strAppDir = Marshal.StringToHGlobalUni(Application.StartupPath);
            MAppWorkCall3(199, IntPtr.Zero, strAppDir);
            Marshal.FreeHGlobal(strAppDir);

            Log("Loading Settings...");

            LoadSettings();

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

            if (!MGetPrivileges()) TaskDialog.Show(LanuageMgr.GetStr("FailedGetPrivileges"), str_AppTitle, "", TaskDialogButton.OK, TaskDialogIcon.Warning);
            is64OS = MIs64BitOS();

            Log("is64OS " + is64OS);
            Log("Loading setings...");

            LoadList();

            if (MIsRunasAdmin())
                AppLoadKernel();

            DelingDialogInit();

            Log("AppRunAgrs...");
            bool notmain = true;
            int id = AppRunAgrs();
            switch (id)
            {
                case 1:
                    tabControlMain.SelectedTab = tabPageKernelCtl;
                    break;
                case 2:
                    tabControlMain.SelectedTab = tabPageSysCtl;
                    break;
                case 3:
                    tabControlMain.SelectedTab = tabPagePerfCtl;
                    break;
                case 4:
                    tabControlMain.SelectedTab = tabPageUWPCtl;
                    break;
                case 5:
                    tabControlMain.SelectedTab = tabPageScCtl;
                    break;
                case 6:
                    tabControlMain.SelectedTab = tabPageStartCtl;
                    break;
                case 7:
                    tabControlMain.SelectedTab = tabPageFileCtl;
                    break;
                case 8:
                    return;
                case 0:
                default:
                    notmain = false;
                    ProcessListInitIn1Slater();
                    break;
            }
            if(notmain) DelingDialogInitHide();
        }
        private void AppOnExit()
        {
            if (!exitCalled)
            {
                baseProcessRefeshTimer.Stop();

                DelingDialogClose();
                PerfClear();
                ProcessListFreeAll();
                MSCM_Exit();
                MEnumProcessFree();
                KernelListUnInit();
                fileSystemWatcher.EnableRaisingEvents = false;
                M_LOG_Close();
                MAppStartEnd();
                MAppWorkCall3(204, IntPtr.Zero, IntPtr.Zero);
                MAppWorkCall3(207, Handle, IntPtr.Zero);
                exitCalled = true;
            }
        }
        private void AppExit()
        {
            Log("App exit...");
            AppOnExit();
            Application.Exit();
        }
        private void AppLoadKernel()
        {
            if (GetConfigBool("LoadKernelDriver", "Configure"))
            {
                Log("Loading Kernel...");
                if (!MInitKernel(Application.StartupPath))
                {
                    if (eprocessindex != -1)
                    {
                        listProcess.Colunms.Remove(listProcess.Colunms[eprocessindex]);
                        eprocessindex = -1;
                    }

                    TaskDialog.Show(LanuageMgr.GetStr("LoadDriverErr"), LanuageMgr.GetStr("ErrTitle"), "", TaskDialogButton.OK, TaskDialogIcon.None);
                }
                else
                {
                    if (GetConfigBool("SelfProtect", "AppSetting"))
                        MAppWorkCall3(203, IntPtr.Zero, IntPtr.Zero);
                }
            }
            else
            {
                if (eprocessindex != -1)
                {
                    listProcess.Colunms.Remove(listProcess.Colunms[eprocessindex]);
                    eprocessindex = -1;
                }
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
                    if (agrs[1] == "system")
                        return 2;
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

        public static void AppHWNDSendMessage(uint message, IntPtr wParam, IntPtr lParam)
        {
            MAppWorkCall2(message, wParam, lParam);
        }

        #region FormEvent

        private bool close_hide = false;
        private bool min_hide = false;
        private bool highlight_nosystem = false;

        private void LoadList()
        {
            lvwColumnSorter = new TaskListViewColumnSorter(this);

            TaskMgrListGroup lg = new TaskMgrListGroup(LanuageMgr.GetStr("TitleApp"));
            listProcess.Groups.Add(lg);
            TaskMgrListGroup lg2 = new TaskMgrListGroup(LanuageMgr.GetStr("TitleBackGround"));
            listProcess.Groups.Add(lg2);
            TaskMgrListGroup lg3 = new TaskMgrListGroup(LanuageMgr.GetStr("TitleWinApp"));
            listProcess.Groups.Add(lg3);

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
            TaskMgrListHeaderItem li12 = new TaskMgrListHeaderItem();
            li12.TextSmall = LanuageMgr.GetStr("TitleDescription");
            li12.Width = 100;
            listUwpApps.Colunms.Add(li12);
            TaskMgrListHeaderItem li10 = new TaskMgrListHeaderItem();
            li10.TextSmall = LanuageMgr.GetStr("TitleFullName");
            li10.Width = 130;
            listUwpApps.Colunms.Add(li10);
            TaskMgrListHeaderItem li11 = new TaskMgrListHeaderItem();
            li11.TextSmall = LanuageMgr.GetStr("TitleInstallDir");
            li11.Width = 130;
            listUwpApps.Colunms.Add(li11);

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
                        string[] headersvx = headersv[i].Split('-');
                        listProcessAddHeader(headersvx[0], int.Parse(headersvx[1]));
                    }
                }
            }
            else if (headers == "")
            {
                listProcessAddHeader("TitleStatus", 70);
                listProcessAddHeader("TitlePID", 55);
                listProcessAddHeader("TitleProcName", 130);
                listProcessAddHeader("TitleCPU", 75);
                listProcessAddHeader("TitleRam", 75);
                listProcessAddHeader("TitleDisk", 75);
                listProcessAddHeader("TitleNet", 75);
            }

            string s2 = GetConfig("RefeshTime", "AppSetting");
            switch (s2)
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
        private void LoadSettings()
        {
            MAppWorkCall3(206, IntPtr.Zero, new IntPtr(GetConfig("TerProcFun", "Configure", "PspTerProc") == "ApcPspTerProc" ? 1 : 0));
            highlight_nosystem = GetConfigBool("HighLightNoSystetm", "Configure", false);
        }
        private void LoadLastPos()
        {
            if (GetConfigBool("OldIsMax", "AppSetting"))
                WindowState = FormWindowState.Maximized;
            else
            {
                string s = GetConfig("OldSize", "AppSetting");
                string p = GetConfig("OldPos", "AppSetting");
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
                if (s.Contains("-"))
                {
                    string[] ss = s.Split('-');
                    try
                    {
                        int w = int.Parse(ss[0]); if (w + Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - Left;
                        int h = int.Parse(ss[1]); if (h + Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - Top;
                        Width = w;
                        Height = h;
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
                    kv2 = (Keys)Enum.Parse(typeof(Keys), k1);
                }
                catch(Exception e)
                {
                    LogErr("Invalid hotkey settings : " + e.Message);
                    kv2 = Keys.T;
                    kv1 = Keys.None;
                }

                MAppRegShowHotKey(Handle, (uint)(int)kv1, (uint)(int)kv2);
            }
        }

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
            Text = GetConfig("Title", "AppSetting", "任务管理器");
            if (Text == "") Text = str_AppTitle;

            LoadHotKey();
            LoadLastPos();
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
                case 40005:
                    {
                        new FormSettings(this).ShowDialog();
                        break;
                    }
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
                        break;
                    }
                case 40019:
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleReboot"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41020:
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleLogoOff"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(186, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 40018:
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleShutdown"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41151:
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleFShutdown"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(201, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41152:
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleFRebbot"), str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(202, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41153:
                    {
                        M_SU_Test("M_SU_Test and test string!");
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
                return;
            }

            SetConfig("ListSortIndex", "AppSetting", sortitem.ToString());
            if (sorta) SetConfig("ListSortDk", "AppSetting", "TRUE");
            else SetConfig("ListSortDk", "AppSetting", "FALSE");
            SetConfig("OldSize", "AppSetting", Width.ToString() + "-" + Height.ToString());
            SetConfig("OldPos", "AppSetting", Left.ToString() + "-" + Top.ToString());
            SetConfigBool("OldIsMax", "AppSetting", WindowState == FormWindowState.Maximized);

            if (saveheader)
            {
                string headers = "";
                for (int i = 1; i < listProcess.Colunms.Count; i++)
                    headers = headers + "#" + listProcess.Colunms[i].FuckYou + "-" + listProcess.Colunms[i].Width;
                SetConfig("MainHeaders", "AppSetting", headers);
            }
            SetConfig("MainHeaders1", "AppSetting", listProcess.Colunms[0].Width.ToString());

            AppOnExit();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_COMMAND)
                FormMain_OnWmCommand(m.WParam.ToInt32());
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (min_hide && m.WParam.ToInt32() == 0xF20)//SC_MINIMIZE
                    Hide();
            }
            coreWndProc?.Invoke(m.HWnd, Convert.ToUInt32(m.Msg), m.WParam, m.LParam);
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
        }


    }
}
