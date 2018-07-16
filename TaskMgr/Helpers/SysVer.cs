using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCMgr.Helpers
{
    static class SysVer
    {
        private static string osName = "";
        private static bool _isWin8Upper = false;

        public static string OsName { get { return osName; } }

        public static bool IsWin8Upper()
        {
            return _isWin8Upper;
        }
        public static void Get()
        {
            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 6:
                            switch (os.Version.Minor)
                            {
                                case 0:
                                    osName = "Windows  Vista ";
                                    break;
                                case 1:
                                    osName = "Windows   7 ";
                                    break;
                                case 2:
                                    osName = "Windows   8 ";
                                    _isWin8Upper = true;
                                    break;
                                case 3:
                                    osName = "Windows   8.1 ";
                                    _isWin8Upper = true;
                                    break;
                            }
                            break;
                        case 10:
                            osName = "Windows   10 ";
                            _isWin8Upper = true;
                            break;
                    }
                    break;
            }
        }
    }
}
