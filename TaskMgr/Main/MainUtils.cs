using PCMgr.Ctls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCMgr.Main
{
    static class MainUtils
    {
        public class PsItem
        {
            public IntPtr processItem = IntPtr.Zero;
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
            public bool isPaused = false;
            public bool isHung = false;

            public string username;

            public IntPtr firstHwnd;

            public UwpItem uwpItem = null;
            public string uwpFullName;
            public bool uwpRealApp = false;

            public bool updateLock = false;

            public override string ToString()
            {
                return "(" + pid + ")  " + exename + " " + exepath;
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

            public override string ToString()
            {
                return uwpMainAppDebText + " (" + uwpFullName + ")";
            }
        }
        public class UwpWinItem
        {
            public IntPtr hWnd = IntPtr.Zero;
            public uint ownerPid = 0;
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
