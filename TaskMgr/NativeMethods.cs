using System;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;

using System.Text;
using System.Diagnostics;

namespace PCMgr
{
    /// <summary>
    /// Native 函数
    /// </summary>
    class NativeMethods
    {
        private static string _cfgFilePath = "";

        public static string cfgFilePath
        {
            get
            {
                if(_cfgFilePath == "") _cfgFilePath = Marshal.PtrToStringUni(M_CFG_GetCfgFilePath());
                return _cfgFilePath;
            }
        }
        public static IntPtr Nullptr = new IntPtr(0);

        public static IntPtr True = new IntPtr(1);
        public static IntPtr False = Nullptr;

        public static IntPtr BoolToIntPtr(bool b)
        {
            return b ? True : False;
        }
        public static bool IntPtrToBool(IntPtr ptr)
        {
            return ptr != Nullptr;
        }

        /// <summary>
        /// Win32 API
        /// </summary>
        public static class Win32
        {
            //Win32 api
            [DllImport("Kernel32", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
            public static extern string GetCommandLineW();
            [DllImport("Kernel32.dll")]
            public static extern IntPtr LoadLibrary(string moduleName);
            [DllImport("Kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            public static Delegate LoadFunction<T>(string dllPath, string functionName)
            {
                var hModule = LoadLibrary(dllPath);
                var functionAddress = GetProcAddress(hModule, functionName);
                return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
            }

            public static ushort LOWORD(IntPtr value)
            {
                return LOWORD((uint)value.ToInt32());
            }
            public static ushort HIWORD(IntPtr value)
            {
                return HIWORD((uint)value.ToInt32());
            }
            public static ushort LOWORD(uint value)
            {
                return (ushort)(value & 0xFFFF);
            }
            public static ushort HIWORD(uint value)
            {
                return (ushort)(value >> 16);
            }
            public static byte LOWBYTE(ushort value)
            {
                return (byte)(value & 0xFF);
            }
            public static byte HIGHBYTE(ushort value)
            {
                return (byte)(value >> 8);
            }
            public static uint MAKELPARAM(ushort a, ushort b)
            {
                return (uint)((uint)a & 0xffff) | ((uint)(b & 0xffff) << 16);
            }


            public const int MB_OK = 0x00000000;
            public const int MB_OKCANCEL = 0x00000001;
            public const int MB_ABORTRETRYIGNORE = 0x00000002;
            public const int MB_YESNOCANCEL = 0x00000003;
            public const int MB_YESNO = 0x00000004;
            public const int MB_RETRYCANCEL = 0x00000005;
            public const int MB_ICONHAND = 0x00000010;
            public const int MB_ICONQUESTION = 0x00000020;
            public const int MB_ICONEXCLAMATION = 0x00000030;
            public const int MB_ICONASTERISK = 0x00000040;
            public const int MB_ICONWARNING = MB_ICONEXCLAMATION;
            public const int MB_ICONERROR = MB_ICONHAND;

            public const int WM_SYSCOMMAND = 0x0112;
            public const int WM_COMMAND = 0x0111;
            public const int WM_HOTKEY = 0x0312;

            [DllImport("kernel32")]
            public static extern uint GetLastError();
            [DllImport("kernel32")]
            public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]
            public static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
            [DllImport("user32")]
            public static extern bool IsHungAppWindow(IntPtr hWnd);
            [DllImport("user32")]
            public static extern bool IsWindowVisible(IntPtr hWnd);
            [DllImport("user32")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder stringbulider, int nMaxCount);
            [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MGetWindowIcon(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, byte[] byBuffer, int nMaxCount);
            [DllImport("user32.dll")]
            public static extern int SetWindowText(IntPtr hWnd, string text);
            [DllImport("user32.dll")]
            public static extern int GetClassName(IntPtr hWnd, byte[] byBuffer, int nMaxCount);
            [DllImport("user32.dll")]
            public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hChildAfter, string lpszClass, string lpszWindowText);
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(POINT pt);
            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int cmd);
            [DllImport("user32.dll")]
            public static extern bool CloseWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool IsWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern int GetDlgCtrlID(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool DestroyWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool DestroyIcon(IntPtr hIcon);
            [DllImport("user32.dll")]
            public static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO lpWindowInfo);
            [DllImport("user32.dll")]
            public static extern long GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll")]
            public static extern int GetSystemMetrics(int nIndex);
            [DllImport("user32.dll")]
            public static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int lpDwProcessId);
            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32", EntryPoint = "RegisterWindowMessage")]
            public static extern int RegisterWindowMessage(string lpString);
            [DllImport("OLEACC.DLL", EntryPoint = "ObjectFromLresult")]
            public static extern int ObjectFromLresult(
                int lResult,
                ref System.Guid riid,
                int wParam,
                [MarshalAs(UnmanagedType.Interface), System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out]ref System.Object ppvObject
            );
            [DllImport("user32.dll")]
            public static extern bool InvalidateRect(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStruct)] RECT lpRect, bool bErase);
            [DllImport("user32.dll")]
            public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

            public const int GWL_STYLE = -16;
            public const int GWL_EXSTYLE = -20;
            public const long WS_VISIBLE = 0x10000000;
            public const long WS_EX_TRANSPARENT = 0x20;
            public const long GWL_HWNDPARENT = -8;

            public const uint WM_GETICON = 0x7F;
            public const int ICON_SMALL = 0;

            public const int SM_XVIRTUALSCREEN = 76;
            public const int SM_YVIRTUALSCREEN = 77;
            public const int SM_CXVIRTUALSCREEN = 78;
            public const int SM_CYVIRTUALSCREEN = 79;

            public struct POINT
            {
                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }

                public int X;
                public int Y;
            }
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
                public System.Drawing.Rectangle ToRectangle()
                {
                    return new System.Drawing.Rectangle(Left, Top, Right - Left, Bottom - Top);
                }
            }
            public struct WINDOWINFO
            {
                public uint cbSize;
                public RECT rcWindow;
                public RECT rcClient;
                public uint dwStyle;
                public uint dwExStyle;
                public uint dwWindowStatus;
                public uint cxWindowBorders;
                public uint cyWindowBorders;
                public short atomWindowType;
                public short wCreatorVersion;
            }
        }

        //C++ 模块api

        //所有API说明及参数说明其参照 PCMgrCore项目 的头文件中的注释

        /// <summary>
        /// Native dll名称
        /// </summary>
#if _X64_
        public const string COREDLLNAME = "PCMgr64.dll";
#else
        public const string COREDLLNAME = "PCMgr32.dll";
#endif

        #region Main Api

        /// <summary>
        /// Native 回调类型，其对应 PCMgrCore项目 的 cscall.h
        /// </summary>
        public static class CSCall
        {
            public const int M_CALLBACK_SW_AOP_WND = 1;
            public const int M_CALLBACK_CLEAR_ILLEGAL_TOP_WND = 2;
            public const int M_CALLBACK_SWITCH_IDM_ALWAYSTOP_SET = 3;
            public const int M_CALLBACK_SWITCH_MAINGROUP_SET = 4;
            public const int M_CALLBACK_SWITCH_REFESHRATE_SET = 5;
            public const int M_CALLBACK_SWITCH_TOPMOST_SET = 6;
            public const int M_CALLBACK_SWITCH_CLOSEHIDE_SET = 7;
            public const int M_CALLBACK_SWITCH_MINHIDE_SET = 8;
            public const int M_CALLBACK_GOTO_SERVICE = 9;
            public const int M_CALLBACK_REFESH_SCLIST = 10;
            public const int M_CALLBACK_KILLPROCTREE = 11;
            public const int M_CALLBACK_SPY_TOOL = 12;
            public const int M_CALLBACK_FILE_TOOL = 13;
            public const int M_CALLBACK_ABOUT = 14;
            public const int M_CALLBACK_ENDTASK = 15;
            public const int M_CALLBACK_LOADDRIVER_TOOL = 16;
            public const int M_CALLBACK_SCITEM_REMOVED = 17;
            public const int M_CALLBACK_SHOW_PROGRESS_DLG = 18;
            public const int M_CALLBACK_UPDATE_PROGRESS_DLG_TO_DELETEING = 19;
            public const int M_CALLBACK_UPDATE_PROGRESS_DLG_ALL = 20;
            public const int M_CALLBACK_UPDATE_PROGRESS_DLG_TO_COLLECTING = 21;
            public const int M_CALLBACK_KERNEL_INIT = 22;
            public const int M_CALLBACK_VIEW_HANDLES = 23;
            public const int M_CALLBACK_KERNEL_INIT_LIST = 24;
            public const int M_CALLBACK_KERNEL_SWITCH_SHOWALLDRV = 25;
            public const int M_CALLBACK_START_ITEM_REMVED = 26;
            public const int M_CALLBACK_VIEW_KSTRUCTS = 27;
            public const int M_CALLBACK_VIEW_TIMER = 28;
            public const int M_CALLBACK_VIEW_HOTKEY = 29;
            public const int M_CALLBACK_SHOW_TRUSTED_DLG = 30;
            public const int M_CALLBACK_MDETALS_LIST_HEADER_RIGHTCLICK = 31;
            public const int M_CALLBACK_KDA = 32;
            public const int M_CALLBACK_SETAFFINITY = 33;
            public const int M_CALLBACK_UPDATE_LOAD_STATUS = 34;
            public const int M_CALLBACK_SHOW_NOPDB_WARN = 35;
            public const int M_CALLBACK_INVOKE_LASTLOAD_STEP = 36;
            public const int M_CALLBACK_DBGPRINT_SHOW = 37;
            public const int M_CALLBACK_DBGPRINT_CLOSE = 38;
            public const int M_CALLBACK_DBGPRINT_DATA = 39;
            public const int M_CALLBACK_DBGPRINT_EMEPTY = 40;
            public const int M_CALLBACK_SHOW_LOAD_STATUS = 41;
            public const int M_CALLBACK_HLDE_LOAD_STATUS = 42;
            public const int M_CALLBACK_MDETALS_LIST_HEADER_MOUSEMOVE = 43;

            public const int M_CALLBACK_KERNEL_VIELL_PRGV = 51;
            public const int M_CALLBACK_KERNEL_TOOL = 52;
            public const int M_CALLBACK_HOOKS = 53;
            public const int M_CALLBACK_NETMON = 54;
            public const int M_CALLBACK_REGEDIT = 55;
            public const int M_CALLBACK_FILEMGR = 56;
            public const int M_CALLBACK_COLLAPSE_ALL = 57;
            public const int M_CALLBACK_SIMPLEVIEW_ACT = 58;
            public const int M_CALLBACK_UWPKILL = 59;
            public const int M_CALLBACK_EXPAND_ALL = 60;
            public const int M_CALLBACK_SHOW_HELP = 61;
            public const int M_CALLBACK_RUN_APP_CMD = 62;
            public const int M_CALLBACK_VIEW_TCP = 63;

            
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WORKERCALLBACK(int msg, IntPtr lParam, IntPtr wParam);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate long WNDPROC(IntPtr hWnd, uint msg, IntPtr lParam, IntPtr wParam);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EXITCALLBACK();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool TerminateImporantWarnCallBack(IntPtr name, int id);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_CFG_GetConfigBOOL([MarshalAs(UnmanagedType.LPWStr)]string configkey, [MarshalAs(UnmanagedType.LPWStr)]string configSection, bool defaultValue);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_CFG_SetConfigBOOL([MarshalAs(UnmanagedType.LPWStr)]string configkey, [MarshalAs(UnmanagedType.LPWStr)] string configSection, bool defaultValue);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr M_CFG_GetCfgFilePath();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MRunExe([MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string args, bool runAsadmin = false, IntPtr hWnd = default(IntPtr));
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MCopyToClipboard2([MarshalAs(UnmanagedType.LPWStr)]string path);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIsKernelNeed64();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_SU_Test([MarshalAs(UnmanagedType.LPStr)]string instr);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_LOG_Init(bool forecConsole);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_LOG_Close();

        [return:MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string MAppGetName();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string MAppGetVersion();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string MAppGetBulidDate();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MGetShieldIcon2();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MGetPrivileges();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_SU_SetSysver(uint ver);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MGetPrivileges2();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppSetCallBack(IntPtr ptr, int id);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppWorkCall2(uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MAppWorkCall3(int id, IntPtr hWnd, IntPtr data);
        public static int MAppWorkCall3(int id)
        {
            return MAppWorkCall3(id, Nullptr, Nullptr);
        }
        public static int MAppWorkCall3(int id, IntPtr hWnd)
        {
            return MAppWorkCall3(id, hWnd, Nullptr);
        }
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppWorkCall4(int id, IntPtr hWnd, IntPtr data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppWorkCall5(int id, IntPtr hWnd, IntPtr data1, IntPtr data2, IntPtr data3);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppExit();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppRebot();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIs64BitOS();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIsRunasAdmin();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppRebotAdmin();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppRebotAdmin2([MarshalAs(UnmanagedType.LPWStr)]string agrs);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MAppSetLanuageItems(int i, int id, [MarshalAs(UnmanagedType.LPWStr)]string s, int size);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MLG_SetLanuageRes([MarshalAs(UnmanagedType.LPWStr)]string s0, [MarshalAs(UnmanagedType.LPWStr)]string s);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MLG_SetLanuageItems_CanRealloc();

        public static int MAppRegShowHotKey(IntPtr hWnd, uint vkkey, uint key)
        {
            return MAppWorkCall5(55, hWnd, new IntPtr(vkkey), new IntPtr(key), Nullptr).ToInt32();
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_FileExist([MarshalAs(UnmanagedType.LPWStr)]string path);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppTest(int id, IntPtr ptr);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MLG_SetLanuageItems_CallBack(IntPtr callback);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public delegate string LanuageItems_CallBack([MarshalAs(UnmanagedType.LPWStr)]string s);

        public static string MAppGetCurSelectName()
        {
            IntPtr strptr = MAppWorkCall4(101, IntPtr.Zero, IntPtr.Zero);
            return Marshal.PtrToStringUni(strptr);
        }

        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern string MNtStatusToStr(int ntstatus);

        public static bool SetConfigBool(string configkey, string configSection, bool configData)
        {
            return M_CFG_SetConfigBOOL(configkey, configSection, configData);
        }
        public static bool GetConfigBool(string configkey, string configSection, bool defaultValue = false)
        {
            return M_CFG_GetConfigBOOL(configkey, configSection, defaultValue);
        }
        public static bool SetConfig(string configkey, string configSection, string configData)
        {
            long OpStation = Win32.WritePrivateProfileString(configSection, configkey, configData, cfgFilePath);
            return (OpStation != 0);
        }
        public static string GetConfig(string configkey, string configSection, string configDefData = "")
        {
            StringBuilder temp = new StringBuilder(1024);
            Win32.GetPrivateProfileString(configSection, configkey, configDefData, temp, 1024, cfgFilePath);
            return temp.ToString();
        }
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MREG_SetCurrentIEVersion(uint ver, [MarshalAs(UnmanagedType.LPWStr)]string currentProcessName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MREG_IsCurrentIEVersionOK(uint ver, [MarshalAs(UnmanagedType.LPWStr)]string currentProcessName);

#if _X64_
        public const string CMDDLLNAME = "PCMgrCmd64.dll";
#else
        public const string CMDDLLNAME = "PCMgrCmd32.dll";
#endif

        [DllImport(CMDDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MAppCmdGetCount();
        [DllImport(CMDDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool MAppCmdGetAt(uint i, StringBuilder buf , uint maxbuf);

        #endregion

        #region PROC API

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EnumProcessCallBack2(uint pid, IntPtr system_process);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EnumProcessCallBack(uint pid, uint ppid, IntPtr name, IntPtr exefullpath, int tp, IntPtr hprocess, IntPtr system_process, IntPtr customData);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int taskdialogcallback(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)]string text, [MarshalAs(UnmanagedType.LPWStr)] string title, [MarshalAs(UnmanagedType.LPWStr)]string apptl, int ico, int button);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EnumWinsCallBack(IntPtr hWnd, IntPtr hWndParent);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetWinsCallBack(IntPtr hWnd, IntPtr hWndParent, int i);

        public const int MENU_SELECTED_PROCESS_KILL_ACT_KILL = 0;
        public const int MENU_SELECTED_PROCESS_KILL_ACT_REBOOT = 1;
        public const int MENU_SELECTED_PROCESS_KILL_ACT_RESENT_BACK = 2;
        public const int MENU_SELECTED_PROCESS_KILL_ACT_UWP_RESENT_BACK = 3;

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_SU_SetKrlMonSet_CreateProcess(bool allow);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_SU_SetKrlMonSet_CreateThread(bool allow);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_SU_SetKrlMonSet_LoadImage(bool allow);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PEOCESSKINFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Eprocess;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string PebAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string JobAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ImageFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ImageFullName;
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MCommandLineToFilePath(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MGetProcessState(IntPtr processItem, IntPtr hwnd);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetProcessEprocess(uint pid, ref PEOCESSKINFO infoStruct);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetExeInfo(string strFilePath, string InfoItem, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetExeDescribe(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetExeCompany(string pszFullPath, StringBuilder b, int maxcount);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr MGetExeIcon(string pszFullPath);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHOWMENUPROC
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FilePath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FileName;
            public uint Pid;
            public IntPtr hWnd;
            public IntPtr selectedhWnd;
            public int data;
            public int type;
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHOWMENUPROCPPER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FilePath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FileName;
            public uint Pid;
            public bool IsImporant;
            public bool IsVeryImporant;
        }

        public static int MAppWorkShowMenuProcess(string strFilePath, string strFileName, uint pid, IntPtr hWnd, IntPtr selectedhWnd, int data, int type, int x, int y)
        {
            SHOWMENUPROC sHOWMENUPROC = new SHOWMENUPROC();
            sHOWMENUPROC.FilePath = strFilePath;
            sHOWMENUPROC.FileName = strFileName;
            sHOWMENUPROC.Pid = pid;
            sHOWMENUPROC.hWnd = hWnd;
            sHOWMENUPROC.selectedhWnd = selectedhWnd;
            sHOWMENUPROC.type = type;
            sHOWMENUPROC.data = data;
            sHOWMENUPROC.x = x;
            sHOWMENUPROC.y = y;

            IntPtr structBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(sHOWMENUPROC));
            Marshal.StructureToPtr(sHOWMENUPROC, structBuffer, false);
            int rs = MAppWorkCall3(221, structBuffer);
            Marshal.FreeHGlobal(structBuffer);

            return rs;
        }
        public static int MAppWorkShowMenuProcessPrepare(string strFilePath, string strFileName, uint pid, bool isimporant, bool isveryimporant)
        {
            SHOWMENUPROCPPER sHOWMENUPROCPPER = new SHOWMENUPROCPPER();
            sHOWMENUPROCPPER.FileName = strFileName;
            sHOWMENUPROCPPER.FilePath = strFilePath;
            sHOWMENUPROCPPER.Pid = pid;
            sHOWMENUPROCPPER.IsImporant = isimporant;
            sHOWMENUPROCPPER.IsVeryImporant = isveryimporant;

            IntPtr structBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(sHOWMENUPROCPPER));
            Marshal.StructureToPtr(sHOWMENUPROCPPER, structBuffer, false);
            int rs = MAppWorkCall3(220, structBuffer);
            Marshal.FreeHGlobal(structBuffer);

            return rs;
        }

        public static bool MKillProcessUser2(uint pid, bool showErr, bool ignoreTerminateing)
        {
            return IntPtrToBool(MAppWorkCall5(50, Nullptr, new IntPtr(pid), BoolToIntPtr(showErr), BoolToIntPtr(ignoreTerminateing)));
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MAppVProcessModuls(uint dwPID, IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)]string procName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MAppVProcessThreads(uint dwPID, IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)]string procName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MAppVProcessWindows(uint dwPID, IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)]string procName);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetProcessCommandLine(IntPtr handle, StringBuilder b, int m, uint pid);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MAppVProcess(IntPtr hWnd);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MEnumProcess(IntPtr callback, IntPtr customData);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MEnumProcess2(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MEnumProcessFree();

        public static bool MAppVProcessAllWindowsGetProcessWindow(uint pid)
        {
            return MAppWorkCall3(218, new IntPtr(pid)) == 1;
        }
        public static bool MAppVProcessAllWindowsGetProcessWindow2(uint pid)
        {
            return MAppWorkCall3(219, new IntPtr(pid)) == 1;
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MCloseHandle(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MUpdateProcess(uint pid, IntPtr callback, IntPtr customData);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MRunUWPApp([MarshalAs(UnmanagedType.LPWStr)]string packageName, [MarshalAs(UnmanagedType.LPWStr)]string appname);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_UnInstallUWPApplication([MarshalAs(UnmanagedType.LPWStr)]string appname);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MGetProcessThreadsCount(IntPtr p);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MGetProcessHandlesCount(IntPtr p);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MGetProcessIsUWP(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MGetProcessIs32Bit(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetUWPPackageFullName(IntPtr handle, ref int len, StringBuilder buf);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetUWPPackageIdName(IntPtr handle, StringBuilder buf, int len);

        public static void MAppVProcessAllWindowsUWP()
        {
            MAppWorkCall3(217);
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MGetProcessSessionID(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MGetProcessUserName(IntPtr handle, StringBuilder buf, int len);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MShowExeFileSignatureInfo([MarshalAs(UnmanagedType.LPWStr)]string filePath);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MCanUseKernel();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MUninitKernel();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MInitKernel(string currDir);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MGetSystemAffinityMask(ref UInt32 SystemAffinityMask);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MGetProcessAffinityMask(IntPtr handle, ref UInt32 AffinityMask);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MGetProcessUserHandleCount(IntPtr handle);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MGetProcessGdiHandleCount(IntPtr handle);

        #endregion

        #region PERF API

        //3 get value
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetCpuUseAge();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetCpuUseAge2();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetCpuUseAgeKernel();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetCpuUseAgeUser();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetCpuUseAgeUser2();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetRamUseAge2();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetDiskUseage();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetNetWorkUseage();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 MPERF_GetRamAll();

        //3
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_Destroy3PerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_Init3PerformanceCounters();

        //update
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_GlobalUpdatePerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_GlobalUpdateCpu();
        

        //Network     
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_NET_UpdateAllProcessNetInfo();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_NET_IsProcessInNet(uint pid);

        //Cpus
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_InitCpuDetalsPerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_DestroyCpuDetalsPerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MPERF_GetCpuDetalsPerformanceCountersCount();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetCpuDetalsCpuUsage(int index);

        //disk perf
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MPERF_InitDisksPerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_DestroyDisksPerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MPERF_GetDisksPerformanceCounters(int index);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetDisksPerformanceCountersValues(IntPtr data, ref double out_readSpeed, ref double out_writeSpeed, ref double out_read, ref double out_write, ref double out_readavgQue);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MPERF_GetDisksPerformanceCountersInstanceName(IntPtr data, StringBuilder buf, int size);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetDisksPerformanceCountersSimpleValues(IntPtr data);

        //Network perf
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MPERF_InitNetworksPerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MPERF_InitNetworksPerformanceCounters2();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_DestroyNetworksPerformanceCounters();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MPERF_GetNetworksPerformanceCounters(int index);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MPERF_GetNetworksPerformanceCountersValues(IntPtr data, ref double out_sent, ref double out_receive);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MPERF_GetNetworksPerformanceCountersInstanceName(IntPtr data, StringBuilder buf, int size);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern double MPERF_GetNetworksPerformanceCountersSimpleValues(IntPtr data);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr MPERF_GetNetworksPerformanceCounterWithName([MarshalAs(UnmanagedType.LPWStr)]string name);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern UInt64 MPERF_GetProcessNetworkSpeed(IntPtr processItem);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool MCONNECTION_ENUM_CALLBACK(uint ProcessId, uint Protcol, [MarshalAs(UnmanagedType.LPWStr)]string LocalAddr, uint LocalPort, [MarshalAs(UnmanagedType.LPWStr)]string RemoteAddr, uint RemotePort, uint state);

        public const int M_CONNECTION_TYPE_TCP = 4;
        public const int M_CONNECTION_TYPE_TCP6 = 6;
        public const int M_CONNECTION_TYPE_UDP = 2;
        public const int M_CONNECTION_TYPE_UDP6 = 5;

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MPERF_NET_EnumTcpConnections(IntPtr cp);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern string MPERF_NET_TcpConnectionStateToString(uint state);

        #endregion

        #region FM API

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
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
        public delegate IntPtr MFCALLBACK(int msg, IntPtr lParam, IntPtr wParam);

        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFileAttr", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MFM_GetFileAttr(uint attr, StringBuilder sb, int maxcount, ref bool bout);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFileTime", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MFM_GetFileTime(ref System.Runtime.InteropServices.ComTypes.FILETIME fILETIME, StringBuilder sb, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFileIcon", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr MFM_GetFileIcon([MarshalAs(UnmanagedType.LPWStr)] string fileExt, StringBuilder sb, int maxcount);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFolderIcon", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MFM_GetFolderIcon();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetMyComputerIcon", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MFM_GetMyComputerIcon();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_SetCallBack", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MFM_SetCallBack(IntPtr cp);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetRoots", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MFM_GetRoots();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFolders", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_GetFolders([MarshalAs(UnmanagedType.LPWStr)] string filePath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetFiles", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_GetFiles([MarshalAs(UnmanagedType.LPWStr)] string filePath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_GetMyComputerName", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MFM_GetMyComputerName();
        [DllImport(COREDLLNAME, EntryPoint = "MFM_OpenFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_OpenFile([MarshalAs(UnmanagedType.LPWStr)] string filePath, IntPtr hWnd);

        public static int MAppWorkShowMenuFM([MarshalAs(UnmanagedType.LPWStr)] string filePath, bool mutilSelect, int selectCount)
        {
            IntPtr strPtrfilePath = Marshal.StringToHGlobalUni(filePath);
            int rs = MAppWorkCall5(53, Nullptr, strPtrfilePath, BoolToIntPtr(mutilSelect), new IntPtr(selectCount)).ToInt32();
            Marshal.FreeHGlobal(strPtrfilePath);
            return rs;
        }
        public static int MAppWorkShowMenuFMF([MarshalAs(UnmanagedType.LPWStr)] string filePath)
        {
            IntPtr strPtrfilePath = Marshal.StringToHGlobalUni(filePath);
            int rs = MAppWorkCall5(54, Nullptr, strPtrfilePath, Nullptr, Nullptr).ToInt32();
            Marshal.FreeHGlobal(strPtrfilePath);
            return rs;
        }

        [DllImport(COREDLLNAME, EntryPoint = "MFM_IsValidateFolderFileName", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_IsValidateFolderFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_CreateDir", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_CreateDir([MarshalAs(UnmanagedType.LPWStr)] string path);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_UpdateFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_UpdateFile([MarshalAs(UnmanagedType.LPWStr)] string fullPath, [MarshalAs(UnmanagedType.LPWStr)] string dirPath);
        [DllImport(COREDLLNAME, EntryPoint = "MFM_ReUpdateFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_ReUpdateFile([MarshalAs(UnmanagedType.LPWStr)] string fullPath, [MarshalAs(UnmanagedType.LPWStr)] string dirPath);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MFM_SetShowHiddenFiles(bool b);

        public static String GetBestFilesizeUnit(Int64 fileSize, out Int64 divisor)
        {
            if (fileSize >= 1073741824)
            {
                divisor = 1073741824;
                return "GB";
            }
            else if (fileSize >= 1048576)
            {
                divisor = 1048576;
                return "MB";
            }
            else
            {
                divisor = 1024;
                return "KB";
            }
        }
        public static String GetBestFilesizeUnit(UInt64 fileSize, out UInt64 divisor)
        {
            if (fileSize >= 1073741824)
            {
                divisor = 1073741824;
                return "GB";
            }
            else if (fileSize >= 1048576)
            {
                divisor = 1048576;
                return "MB";
            }
            else
            {
                divisor = 1024;
                return "KB";
            }
        }

        public static String FormatFileSizeKBUnit(Int64 fileSize)
        {
            fileSize = fileSize * 1024;

            if (fileSize < 0) { throw new ArgumentOutOfRangeException("fileSize"); }
            else if (fileSize >= 1073741824)
            {
                return string.Format("{0:########0} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1048576)
            {
                return string.Format("{0:####0} MB", ((Double)fileSize) / (1024 * 1024));
            }
            else
            {
                return string.Format("{0:####0} KB", ((Double)fileSize) / 1024);
            }
        }
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
        public static String FormatFileSizeMenSingal(Int64 fileSize)
        {
            if (fileSize < 0)
                throw new ArgumentOutOfRangeException("fileSize");
            return (fileSize / 1024).ToString("0") + " K";
        }
        public static String FormatFileSizeMenSingal(UInt64 fileSize)
        {
            if (fileSize < 0)
                throw new ArgumentOutOfRangeException("fileSize");
            return (fileSize / 1024).ToString("0") + " K";
        }
        public static String FormatNetSpeed(Int64 speedBytes)
        {
            if (speedBytes < 0)
                throw new ArgumentOutOfRangeException("fileSize");
            speedBytes *= 8;
            if (speedBytes >= 1073741824)
                return string.Format("{0:########0.00} Gbps", ((Double)speedBytes) / (1024 * 1024 * 1024));
            else if (speedBytes >= 1048576)
                return string.Format("{0:####0.00} Mbps", ((Double)speedBytes) / (1024 * 1024));
            else
                return string.Format("{0:####0.00} Kbps", ((Double)speedBytes) / 1024);
        }
        public static String FormatNetSpeedUnit(Int64 speedBytes)
        {
            if (speedBytes < 0)
                throw new ArgumentOutOfRangeException("fileSize");
            speedBytes *= 8;
            if (speedBytes >= 1073741824)
                return string.Format("{0:########0} Gbps", ((Double)speedBytes) / (1024 * 1024 * 1024));
            else if (speedBytes >= 1048576)
                return string.Format("{0:####0} Mbps", ((Double)speedBytes) / (1024 * 1024));
            else
                return string.Format("{0:####0} Kbps", ((Double)speedBytes) / 1024);
        }

        #endregion

        #region SC API

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EnumServicesCallBack(IntPtr dspName, IntPtr scName, uint scType, uint currentState, uint dwProcessId, bool syssc,
                uint dwStartType, IntPtr lpBinaryPathName, IntPtr lpLoadOrderGroup, bool add);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MSCM_Init();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MSCM_Exit();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MEnumServices(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MSCM_ShowMenu(IntPtr hDlg, [MarshalAs(UnmanagedType.LPWStr)] string serviceName, uint running, uint startType, [MarshalAs(UnmanagedType.LPWStr)] string path, int x, int y);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MSCM_UpdateServiceStatus([MarshalAs(UnmanagedType.LPWStr)] string serviceName, IntPtr callback);

        #endregion

        #region SM API


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EnumStartupsCallBack(IntPtr name, IntPtr type, IntPtr path, IntPtr rootregpath, IntPtr regpath, IntPtr regvalue);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MEnumStartups(IntPtr callback);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void MStartupsMgr_ShowMenu(IntPtr rootkey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)] string filepath, [MarshalAs(UnmanagedType.LPWStr)]string regvalue, uint id, int x, int y);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr MREG_ROOTKEYToStr(IntPtr krootkey);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool MREG_GetBIOSTime(StringBuilder stringBuilder);

        #endregion

        #region KRNL API

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EnumKernelModulsCallBack(IntPtr kmi, IntPtr BaseDllName, IntPtr FullDllPath, IntPtr FullDllPathOrginal, IntPtr szEntryPoint, IntPtr SizeOfImage, IntPtr szDriverObject, IntPtr szBase, IntPtr szServiceName, uint Order);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool M_SU_EnumKernelModuls(IntPtr callback, bool showall);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_SU_EnumKernelModulsItemDestroy(IntPtr kmi);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_SU_EnumKernelModuls_ShowMenu(IntPtr kmi, bool showall, int x, int y);

        #endregion

        #region USER API

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public delegate bool EnumUsersCallBack(IntPtr userName, uint sessionId, uint userId, IntPtr domain, IntPtr customData);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_User_EnumUsers(IntPtr callback, IntPtr customData);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool M_User_GetUserInfo([MarshalAs(UnmanagedType.LPWStr)]string userName, IntPtr userIcoPath, IntPtr userFullName, int max2);

        #endregion

        #region UWP API

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_Init();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_UnInit();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_RunUWPApp([MarshalAs(UnmanagedType.LPWStr)]string strAppUserModelId, ref uint pdwProcessId);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_KillUWPApplication([MarshalAs(UnmanagedType.LPWStr)]string packageName);


        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_EnumUWPApplications();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int M_UWP_GetUWPApplicationsCount();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_GetUWPApplicationAt(uint i, ref UWP_PACKAGE_INFO infoStruct);
        public static UWP_PACKAGE_INFO M_UWP_GetUWPApplicationAt(uint i)
        {
            UWP_PACKAGE_INFO uWP_PACKAGE_INFO = new UWP_PACKAGE_INFO();
            M_UWP_GetUWPApplicationAt(i, ref uWP_PACKAGE_INFO);
            return uWP_PACKAGE_INFO;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct UWP_PACKAGE_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string AppUserModelId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string AppPackageFamilyName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string AppPackageFullName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string InstallPath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string IconPath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DisplayName;

            public int IconBackgroundColor;
        }

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);
        public static string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
        {
            string sWin8ManifestString = string.Format("@{{{0}?{1}}}", pathToPRI, resourceKey);
            var outBuff = new StringBuilder(1024);
            int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
            return outBuff.ToString();
        }

        #endregion

        public static class DeviceApi
        {
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool MDEVICE_Init();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void MDEVICE_UnInit();

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool MDEVICE_GetLogicalDiskInfo();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool MDEVICE_DestroyLogicalDiskInfo();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint MDEVICE_GetLogicalDiskInfoSize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint MDEVICE_GetPhysicalDriveFromPartitionLetter(char letter);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern bool MDEVICE_GetLogicalDiskInfoItem(int index, StringBuilder nameBuffer, StringBuilder modelBuffer, ref UInt32 outIndex, ref UInt64 outSize, StringBuilder sizeBuffer);

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern bool MDEVICE_GetIsSystemDisk([MarshalAs(UnmanagedType.LPStr)]string perfstr);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern bool MDEVICE_GetIsPageFileDisk([MarshalAs(UnmanagedType.LPStr)]string perfstr);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint MDEVICE_GetPhysicalDriveIndexInWMI([MarshalAs(UnmanagedType.LPWStr)]string perfstr);

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint MDEVICE_GetNetworkAdaptersInfo();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool MDEVICE_DestroyNetworkAdaptersInfo();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern bool MDEVICE_GetNetworkAdapterInfoFormName(
                [MarshalAs(UnmanagedType.LPWStr)]string name, StringBuilder sbV4, int bufferSizeV4,
                StringBuilder sbV6, int bufferSizeV6);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern bool MDEVICE_GetNetworkAdapterInfoItem(int index, StringBuilder name, int bufferSize);

            public static bool MDEVICE_GetNetworkAdapterIsWIFI(string name)
            {
                if (name != null && (name.ToLower().Contains("wifi") || name.ToLower().Contains("wireless lan") || name.ToLower().Contains("wireless") || name.Contains("wi-fi")))
                    return true;
                return false;
            }
            public static string MDEVICE_MemoryFormFactorToString(UInt16 _formFactor)
            {
                string formFactor = string.Empty;

                switch (_formFactor)
                {
                    case 1:
                        formFactor = "Other";
                        break;
                    case 2:
                        formFactor = "SIP";
                        break;
                    case 3:
                        formFactor = "DIP";
                        break;
                    case 4:
                        formFactor = "ZIP";
                        break;
                    case 5:
                        formFactor = "SOJ";
                        break;
                    case 6:
                        formFactor = "Proprietary";
                        break;
                    case 7:
                        formFactor = "SIMM";
                        break;
                    case 8:
                        formFactor = "DIMM";
                        break;
                    case 9:
                        formFactor = "TSOP";
                        break;
                    case 10:
                        formFactor = "PGA";
                        break;
                    case 11:
                        formFactor = "RIMM";
                        break;
                    case 12:
                        formFactor = "SODIMM";
                        break;
                    case 13:
                        formFactor = "SRIMM";
                        break;
                    case 14:
                        formFactor = "SMD";
                        break;
                    case 15:
                        formFactor = "SSMP";
                        break;
                    case 16:
                        formFactor = "QFP";
                        break;
                    case 17:
                        formFactor = "TQFP";
                        break;
                    case 18:
                        formFactor = "SOIC";
                        break;
                    case 19:
                        formFactor = "LCC";
                        break;
                    case 20:
                        formFactor = "PLCC";
                        break;
                    case 21:
                        formFactor = "BGA";
                        break;
                    case 22:
                        formFactor = "FPBGA";
                        break;
                    case 23:
                        formFactor = "LGA";
                        break;
                    default:
                        formFactor = "Unknown";
                        break;
                }

                return formFactor;
            }

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool MDEVICE_GetMemoryDeviceUsed(ref UInt16 outAll, ref UInt16 outUsed);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool MDEVICE_GetMemoryDeviceInfo();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern IntPtr MDEVICE_GetMemoryDeviceName();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt32 MDEVICE_GetMemoryDeviceSpeed();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern IntPtr MDEVICE_GetMemoryDeviceLocator();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt16 MDEVICE_GetMemoryDeviceFormFactor();
        }
        public static class ComCtlApi
        {
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MListViewGetHeaderControl(IntPtr hList, bool ismain = true);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void MListViewSetColumnSortArrow(IntPtr hListHeader, int index, bool isUp, bool no);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void MListViewProcListWndProc(IntPtr hList);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void MListViewProcListLock(bool locked);
        }
        public static class LogApi
        {
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Error_ForceFileW")]
            public static extern void FLogErr([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Warning_ForceFileW")]
            public static extern void FLogWarn([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Info_ForceFileW")]
            public static extern void FLogInfo([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_Str_ForceFileW")]
            public static extern void FLog([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_LogErrW")]
            public static extern void LogErr([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_LogWarnW")]
            public static extern void LogWarn([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_LogInfoW")]
            public static extern void LogInfo([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_LogW")]
            public static extern void Log([MarshalAs(UnmanagedType.LPWStr)]string format);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "M_LOG_LogTextW")]
            public static extern void LogText([MarshalAs(UnmanagedType.LPWStr)]string format);

            private static void LogX2WithStack(string format, string type)
            {
                var st = new StackTrace(new StackFrame(2, true));
                var stf = st.GetFrame(0);
                string stack = "\n  [At] " + stf.GetFileName() + "  (" + stf.GetFileLineNumber() + ")  " + stf.GetMethod();
                switch (type)
                {
                    case "Err": LogErr(format + stack);break;
                    case "Warn": LogWarn(format + stack); break;
                    case "Info": LogInfo(format + stack); break;
                    default: Log(format + stack); break;
                }
               
            }

            public static void Log2(string format)
            {
                LogX2WithStack(format, "");
            }
            public static void LogErr2(string format)
            {
                LogX2WithStack(format, "Err");
            }
            public static void LogWarn2(string format)
            {
                LogX2WithStack(format, "Warn");
            }
            public static void LogInfo2(string format)
            {
                LogX2WithStack(format, "Info");
            }

        }

        public static class MCpuInfoMonitor
        {
#if _X64_
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuL1Cache@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuL1Cache();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuL2Cache@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuL2Cache();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuL3Cache@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuL3Cache();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuPackage@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuPackage();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuNodeCount@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuNodeCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuInfos@MCpuInfoMonitor@@SAHXZ")]
            public static extern bool GetCpuInfos();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuName@MCpuInfoMonitor@@SAHPEA_WH@Z", CharSet = CharSet.Unicode)]
            public static extern bool GetCpuName(StringBuilder buf, int size);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuFrequency@MCpuInfoMonitor@@SAHXZ")]
            public static extern int GetCpuFrequency();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessorNumber@MCpuInfoMonitor@@SAHXZ")]
            public static extern int GetProcessorNumber();
#else
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuL1Cache@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuL1Cache();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuL2Cache@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuL2Cache();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuL3Cache@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuL3Cache();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuPackage@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuPackage();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuNodeCount@MCpuInfoMonitor@@SAKXZ")]
            public static extern uint GetCpuNodeCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuInfos@MCpuInfoMonitor@@SAHXZ")]
            public static extern bool GetCpuInfos();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuName@MCpuInfoMonitor@@SAHPA_WH@Z", CharSet = CharSet.Unicode)]
            public static extern bool GetCpuName(StringBuilder buf, int size);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuFrequency@MCpuInfoMonitor@@SAHXZ")]
            public static extern int GetCpuFrequency();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessorNumber@MCpuInfoMonitor@@SAHXZ")]
            public static extern int GetProcessorNumber();
#endif
        }
        public static class MSystemMemoryPerformanctMonitor
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SYSTEM_COMPRESSION_INFO
            {
                public UInt32 Version;
                public UInt32 CompressionPid;
                public UInt64 CompressionWorkingSetSize;
                public UInt64 CompressSize;
                public UInt64 CompressedSize;
                public UInt64 NonCompressedSize;
            }

#if _X64_
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?UpdateMemoryListInfo@MSystemMemoryPerformanctMonitor@@SAHXZ")]
            public static extern bool UpdateMemoryListInfo();

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetAllMemory@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetAllMemory();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetKernelPaged@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetKernelPaged();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetKernelNonpaged@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetKernelNonpaged();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetSystemCacheSize@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetSystemCacheSize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCommitTotal@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetCommitTotal();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCommitLimit@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetCommitLimit();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryAvail@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetMemoryAvail();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryUsed@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetMemoryUsed();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryAvailPageFile@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetMemoryAvailPageFile();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryUsage@MSystemMemoryPerformanctMonitor@@SANXZ")]
            public static extern double GetMemoryUsage();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetStandBySize@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetStandBySize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetModifiedSize@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetModifiedSize();

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryCompressionInfo@MSystemMemoryPerformanctMonitor@@SAHPEAU_PROCESS_COMPRESSION_INFO@@@Z")]
            public static extern bool GetMemoryCompressionInfo(ref SYSTEM_COMPRESSION_INFO outInfo);
#else
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryCompressionInfo@MSystemMemoryPerformanctMonitor@@SAHPAU_PROCESS_COMPRESSION_INFO@@@Z")]
            public static extern bool GetMemoryCompressionInfo(ref SYSTEM_COMPRESSION_INFO outInfo);

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?UpdateMemoryListInfo@MSystemMemoryPerformanctMonitor@@SAHXZ")]
            public static extern bool UpdateMemoryListInfo();

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetAllMemory@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetAllMemory();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetKernelPaged@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetKernelPaged();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetKernelNonpaged@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetKernelNonpaged();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetSystemCacheSize@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetSystemCacheSize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCommitTotal@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetCommitTotal();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCommitLimit@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetCommitLimit();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryAvail@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetMemoryAvail();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryUsed@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetMemoryUsed();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryAvailPageFile@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetMemoryAvailPageFile();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetMemoryUsage@MSystemMemoryPerformanctMonitor@@SANXZ")]
            public static extern double GetMemoryUsage();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetStandBySize@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetStandBySize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetModifiedSize@MSystemMemoryPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetModifiedSize();
#endif
        }
        public static class MSystemPerformanctMonitor
        {
#if _X64_
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetThreadCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetThreadCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetHandleCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetHandleCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetProcessCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetPageSize@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetPageSize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetCpuCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?UpdatePerformance@MSystemPerformanctMonitor@@SAHXZ")]
            public static extern bool UpdatePerformance();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetSystemRunTime@MSystemPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetSystemRunTime();
#else
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetThreadCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetThreadCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetHandleCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetHandleCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetProcessCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetPageSize@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetPageSize();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetCpuCount@MSystemPerformanctMonitor@@SAKXZ")]
            public static extern uint GetCpuCount();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?UpdatePerformance@MSystemPerformanctMonitor@@SAHXZ")]
            public static extern bool UpdatePerformance();
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetSystemRunTime@MSystemPerformanctMonitor@@SA_KXZ")]
            public static extern UInt64 GetSystemRunTime();
#endif
        }
        public static class MProcessMonitor
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool ProcessMonitorUpdateNotIncludeItemCallBack(uint pid);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ProcessMonitorRemoveItemCallBack(uint pid);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void ProcessMonitorNewItemCallBack(uint pid, uint parentid, [MarshalAs(UnmanagedType.LPWStr)]string exename, [MarshalAs(UnmanagedType.LPWStr)]string exefullpath, IntPtr hProcess, IntPtr processItem);

#if _X64_
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?CreateProcessMonitor@MProcessMonitor@@SAPEAV1@P6AXK@ZP6AXKKPEA_W1PEAXPEAUtag_MPROCESS_ITEM@@@ZP6AHK@Z@Z")]
            public static extern IntPtr CreateProcessMonitor(IntPtr removeItemCallBack, IntPtr newItemCallBack, IntPtr updateNotIncludeItemCallBack);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?DestroyProcessMonitor@MProcessMonitor@@SAXPEAV1@@Z")]
            public static extern void DestroyProcessMonitor(IntPtr monitor);

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?EnumAllProcess@MProcessMonitor@@SAHPEAV1@@Z")]
            public static extern bool EnumAllProcess(IntPtr monitor);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?RefeshAllProcess@MProcessMonitor@@SAHPEAV1@@Z")]
            public static extern bool RefeshAllProcess(IntPtr monitor);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?RefeshAllProcessNotInclude@MProcessMonitor@@SAHPEAV1@@Z")]
            public static extern bool RefeshAllProcessNotInclude(IntPtr monitor);
#else
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?CreateProcessMonitor@MProcessMonitor@@SAPAV1@P6AXK@ZP6AXKKPA_W1PAXPAUtag_MPROCESS_ITEM@@@ZP6AHK@Z@Z")]
            public static extern IntPtr CreateProcessMonitor(IntPtr removeItemCallBack, IntPtr newItemCallBack, IntPtr updateNotIncludeItemCallBack);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?DestroyProcessMonitor@MProcessMonitor@@SAXPAV1@@Z")]
            public static extern void DestroyProcessMonitor(IntPtr monitor);

            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?EnumAllProcess@MProcessMonitor@@SAHPAV1@@Z")]
            public static extern bool EnumAllProcess(IntPtr monitor);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?RefeshAllProcess@MProcessMonitor@@SAHPAV1@@Z")]
            public static extern bool RefeshAllProcess(IntPtr monitor);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?RefeshAllProcessNotInclude@MProcessMonitor@@SAHPAV1@@Z")]
            public static extern bool RefeshAllProcessNotInclude(IntPtr monitor);
#endif
        }
        public static class MProcessPerformanctMonitor
        {
            public static int M_GET_PROCMEM_WORKINGSET = 0;
            public static int M_GET_PROCMEM_WORKINGSETPRIVATE = 1;
            public static int M_GET_PROCMEM_WORKINGSETSHARE = 2;
            public static int M_GET_PROCMEM_PEAKWORKINGSET = 3;
            public static int M_GET_PROCMEM_COMMITEDSIZE = 4;
            public static int M_GET_PROCMEM_NONPAGEDPOOL = 5;
            public static int M_GET_PROCMEM_PAGEDPOOL = 6;
            public static int M_GET_PROCMEM_PAGEDFAULT = 7;
            public static int M_GET_PROCMEM_WORKINGSET_INC = 8;
            public static int M_GET_PROCMEM_PAGEDFAULT_INC = 9;

            public static int M_GET_PROCIO_READ = 0;
            public static int M_GET_PROCIO_WRITE = 1;
            public static int M_GET_PROCIO_OTHER = 2;
            public static int M_GET_PROCIO_READ_BYTES = 3;
            public static int M_GET_PROCIO_WRITE_BYTES = 4;
            public static int M_GET_PROCIO_OTHER_BYTES = 5;

            public static UInt64 GetProcessNetworkSpeed(IntPtr processItem)
            {
                return MPERF_GetProcessNetworkSpeed(processItem);
            }

#if _X64_
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessPrivateWoringSet@MProcessPerformanctMonitor@@CA_KPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt32 GetProcessPrivateWoringSet(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessIOSpeed@MProcessPerformanctMonitor@@CA_KPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt32 GetProcessIOSpeed(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuUseAgeKernel@MProcessPerformanctMonitor@@CANPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern double GetProcessCpuUseAgeKernel(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuUseAgeUser@MProcessPerformanctMonitor@@CANPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern double GetProcessCpuUseAgeUser(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuUseAge@MProcessPerformanctMonitor@@CANPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern double GetProcessCpuUseAge(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuTime@MProcessPerformanctMonitor@@CA_KPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt64 GetProcessCpuTime(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCycle@MProcessPerformanctMonitor@@CA_KPEAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt64 GetProcessCycle(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessMemoryInfo@MProcessPerformanctMonitor@@CA_KPEAUtag_MPROCESS_ITEM@@H@Z")]
            public static extern UInt32 GetProcessMemoryInfo(IntPtr processItem, int col);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessIOInfo@MProcessPerformanctMonitor@@CA_KPEAUtag_MPROCESS_ITEM@@H@Z")]
            public static extern UInt64 GetProcessIOInfo(IntPtr processItem, int col);
#else
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessPrivateWoringSet@MProcessPerformanctMonitor@@CAKPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt32 GetProcessPrivateWoringSet(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessIOSpeed@MProcessPerformanctMonitor@@CAKPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt32 GetProcessIOSpeed(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuUseAgeKernel@MProcessPerformanctMonitor@@CANPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern double GetProcessCpuUseAgeKernel(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuUseAgeUser@MProcessPerformanctMonitor@@CANPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern double GetProcessCpuUseAgeUser(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuUseAge@MProcessPerformanctMonitor@@CANPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern double GetProcessCpuUseAge(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCpuTime@MProcessPerformanctMonitor@@CA_KPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt64 GetProcessCpuTime(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessCycle@MProcessPerformanctMonitor@@CA_KPAUtag_MPROCESS_ITEM@@@Z")]
            public static extern UInt64 GetProcessCycle(IntPtr processItem);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessMemoryInfo@MProcessPerformanctMonitor@@CAKPAUtag_MPROCESS_ITEM@@H@Z")]
            public static extern UInt32 GetProcessMemoryInfo(IntPtr processItem, int col);
            [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetProcessIOInfo@MProcessPerformanctMonitor@@CA_KPAUtag_MPROCESS_ITEM@@H@Z")]
            public static extern UInt64 GetProcessIOInfo(IntPtr processItem, int col);
#endif
        }
    }

}
