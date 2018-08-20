using System.Runtime.InteropServices;

namespace PCMgr.Helpers
{
    static class SysVer
    {
        private static bool _isWin8Upper = false;

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetWindowsWin8Upper();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetWindowsBulidVersion();

        public static bool IsWin8Upper()
        {
            return _isWin8Upper;
        }
        public static void Get()
        {
            MGetWindowsBulidVersion();
            _isWin8Upper = MGetWindowsWin8Upper(); 
        }
    }
}
