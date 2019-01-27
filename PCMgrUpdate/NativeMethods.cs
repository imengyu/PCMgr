using System;
using System.Runtime.InteropServices;

namespace PCMgrUpdate
{
    class NativeMethods
    {
        /// <summary>
        /// Native dll名称
        /// </summary>
#if _X64_
        public const string COREDLLNAME = "PCMgr64.dll";
#else
        public const string COREDLLNAME = "PCMgr32.dll";
#endif

        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppGetCoreModulHandle();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppGetVersion();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MAppTest(int id, IntPtr p);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAppGetBulidDate();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIsRunasAdmin();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MAppRebotAdmin3([MarshalAsAttribute(UnmanagedType.LPWStr)]string args, ref bool userCanceled);
    }
}
