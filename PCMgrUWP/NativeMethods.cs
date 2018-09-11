using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PCMgrUWP
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct UWP_PACKAGE_APP_INFO
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string AppUserModelId;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct UWP_PACKAGE_INFO
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string PublisherDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Logo;
        public uint ApplicationsCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.Struct)]
        public UWP_PACKAGE_APP_INFO[] Applications;
    };

    internal static class NativeMethods
    {
        //Apis
#if _X64_
        private const string COREDLLNAME = "PCMgr64.dll";
#else
        private const string COREDLLNAME = "PCMgr32.dll";
#endif
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MIs64BitOS();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MFM_FileExist([MarshalAs(UnmanagedType.LPWStr)]string path);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool M_UWP_ReadUWPPackage([MarshalAs(UnmanagedType.LPWStr)]string installDir, ref UWP_PACKAGE_INFO info);

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);
        public static string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
        {
            string sWin8ManifestString = string.Format("@{{{0}?{1}}}", pathToPRI, resourceKey);
            var outBuff = new StringBuilder(1024);
            int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
            return outBuff.ToString();
        }
    }
}
