using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PCMgr.Helpers
{
    static class SysVer
    {
        private static bool _isWin8Upper = false;

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MGetWindowsWin8Upper();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
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
