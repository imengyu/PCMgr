using PCMgr.Ctls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace PCMgr.Main
{
    static class MainUtils
    {
        public static Color HexStrToColor(string s)
        {
            if (string.IsNullOrEmpty(s) || s == "transparent")
                    return Color.FromArgb(0, 120, 215);
            if (s[0] == '#')
            {
                s = s.Remove(0, 1);
                int r, g, b;
                int.TryParse(s.Substring(0, 2), out r);
                int.TryParse(s.Substring(2, 2), out g);
                int.TryParse(s.Substring(3, 2), out b);
                return Color.FromArgb(r, g, b);
            }
            else return Color.FromName(s);
        }
        public static Color Uint32StrToColor(uint u)
        {
            if (u == 0) return Color.FromArgb(0, 120, 215);

            uint r, g, b;
            b = (u & 0xff0000) >> 16;
            g = (u & 0x00ff00) >> 8;
            r = (u & 0x0000ff);

            return Color.FromArgb((int)r, (int)g, (int)b);
        }
        public static Bitmap IconToBitmap(Icon ico, int w, int h)
        {
            Bitmap b = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);
            g.DrawIcon(ico, new Rectangle(0, 0, w, h));
            g.Dispose();
            return b;
        }

        public class PsItem
        {
            public IntPtr processItem = IntPtr.Zero;
            public IntPtr handle;
            public uint pid;
            public uint ppid;
            public string exename;
            public string exepath;
            public string username;
            public TaskMgrListItem item = null;
            public bool isSvchost = false;
            public bool isUWP = false;
            public bool isWindowShow = false;
            public bool isWindowsProcess = false;
            public bool isPaused = false;
            public bool isHung = false;

            public IntPtr firstHwnd;

            public UwpItem uwpItem = null;
            public string uwpFullName;
            public bool uwpRealApp = false;
            public string uwpPackageIdName;

            public bool updateLock = false;

            public override string ToString()
            {
                return "(" + pid + ")  " + exename + " " + exepath;
            }
            public string Print()
            {
                string s = "(" + pid + ")  " + exename + " " + exepath;

                Type t = typeof(PsItem);
                FieldInfo[] fields = t.GetFields();
                foreach (FieldInfo field in fields)
                    s += "\n" + field.FieldType + " " + field.Name + " = " + field.GetValue(this);

                return s;
            }

            public PsItem parent = null;
            public List<PsItem> childs = new List<PsItem>();
            public List<ScItem> svcs = new List<ScItem>();
        }
        public class UwpItem
        {
            public string uwpInstallDir = "";
            public TaskMgrListItemGroup uwpItem = null;
            public string uwpFullName = "";
            public string uwpMainAppDebText = "";
            public IntPtr firstHwnd;

            public string Print()
            {
                string s = ToString();

                Type t = typeof(PsItem);
                FieldInfo[] fields = t.GetFields();
                foreach (FieldInfo field in fields)
                    s += "\n" + field.FieldType + " " + field.Name + " = " + field.GetValue(this);

                return s;
            }
            public override string ToString()
            {
                return uwpMainAppDebText + " (" + uwpFullName + ")";
            }
        }
        public class UwpWinItem
        {
            public IntPtr hWnd = IntPtr.Zero;
            public uint ownerPid = 0;

            public override string ToString()
            {
                return "HWND " + hWnd + " ( pid : " + ownerPid + ")";
            }
        }
        public class UwpHostItem
        {
            public UwpHostItem(UwpItem item, uint pid)
            {
                this.pid = pid;
                this.item = item;
            }

            public UwpItem item;
            public uint pid;

            public override string ToString()
            {
                return "(" + pid + ")" + item.ToString();
            }
        }
        public class ScTag
        {
            public uint startType = 0;
            public uint runningState = 0;
            public string name = "";
            public string binaryPathName = "";
        }
        public class ScItem
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
        public struct itemheaderTip
        {
            public itemheaderTip(string hn, string n)
            {
                herdername = hn;
                name = n;
            }
            public string herdername;
            public string name;
        }
        public struct itemheaderDef
        {
            public itemheaderDef(string hn, int width)
            {
                herdername = hn;
                this.width = width;
            }
            public string herdername;
            public int width;
        }
    }
}
