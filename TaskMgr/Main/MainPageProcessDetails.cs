using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PCMgr.Main.MainUtils;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.LogApi;

namespace PCMgr.Main
{
    class MainPageProcessDetails : MainPage
    {
        private ListView listProcessDetals;
        private Button btnEndProcessDetals;

        public MainPageProcessDetails(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageDetals)
        {
            listProcessDetals = formMain.listProcessDetals;
            btnEndProcessDetals = formMain.btnEndProcessDetals;
        }

        protected override void OnLoadControlEvents()
        {
            btnEndProcessDetals.Click += btnEndProcessDetals_Click;

            listProcessDetals.ColumnClick += listProcessDetals_ColumnClick;
            listProcessDetals.ColumnReordered += listProcessDetals_ColumnReordered;
            listProcessDetals.SelectedIndexChanged += listProcessDetals_SelectedIndexChanged;
            listProcessDetals.KeyDown += listProcessDetals_KeyDown;
            listProcessDetals.MouseClick += listProcessDetals_MouseClick;

            FormMain.隐藏列ToolStripMenuItem.Click += 隐藏列ToolStripMenuItem_Click;
            FormMain.选择列ToolStripMenuItem.Click += 选择列ToolStripMenuItem_Click;
            FormMain.将此列调整为合适大小ToolStripMenuItem.Click += 将此列调整为合适大小ToolStripMenuItem_Click;

            base.OnLoadControlEvents();
        }
       

        //详细信息 页面代码

        private IntPtr processMonitorDetals = IntPtr.Zero;

        private List<ProcessDetalItem> loadedDetalProcess = new List<ProcessDetalItem>();
        private class ProcessDetalItem
        {
            public ProcessDetalItem()
            {

            }
            public IntPtr handle;
            public uint pid;
            public uint ppid;
            public string exename;
            public string eprocess;
            public string exepath;
            public IntPtr processItem = IntPtr.Zero;
            public ProcessDetalItem parent = null;
            public ListViewItem item = null;
            public List<ProcessDetalItem> childs = new List<ProcessDetalItem>();
        }

        private int nextSecType = -1;
        public bool nextUpdateStaticVals = false;

        private List<string> windowsProcess = new List<string>();
        private List<string> veryimporantProcess = new List<string>();

        private string csrssPath = "";
        private string ntoskrnlPath = "";
        private string systemRootPath = "";
        private string svchostPath = "";

        private bool IsWindowsProcess(string str)
        {
            //检测是不是Windows进程
            if (str != null)
            {
                str = str.ToLower();
                foreach (string s in windowsProcess)
                    if (s == str) return true;
            }
            return false;
        }
        private bool IsVeryImporant(ProcessDetalItem p)
        {
            if (p.exepath != null)
            {
                string str = p.exepath.ToLower();
                foreach (string s in veryimporantProcess)
                    if (s == str) return true;
            }
            return false;
        }
        private bool IsImporant(ProcessDetalItem p)
        {
            if (p.exepath != null)
            {
                if (p.exepath.ToLower() == @"c:\windows\system32\svchost.exe") return true;
                return IsWindowsProcess(p.exepath);
            }
            return false;
        }

        //Find iten
        private bool ProcessListDetailsIsProcessLoaded(uint pid, out ProcessDetalItem item)
        {
            bool rs = false;
            foreach (ProcessDetalItem f in loadedDetalProcess)
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
        private ProcessDetalItem ProcessListDetailsFindPsItem(uint pid)
        {
            ProcessDetalItem rs = null;
            foreach (ProcessDetalItem i in loadedDetalProcess)
            {
                if (i.pid == pid)
                {
                    rs = i;
                    return rs;
                }
            }
            return rs;
        }

        public void ProcessListDetailsInit()
        {
            if (!Inited)
            {
                //if (!processListInited) ProcessListInit();

                listViewItemComparerProcDetals = new ListViewItemComparerProcDetals(this);

                NativeBridge.ProcessNewItemCallBackDetails = ProcessListDetailsNewItemCallBack;
                NativeBridge.ProcessRemoveItemCallBackDetails = ProcessListDetailsRemoveItemCallBack;

                NativeBridge.ptrProcessNewItemCallBackDetails = Marshal.GetFunctionPointerForDelegate(NativeBridge.ProcessNewItemCallBackDetails);
                NativeBridge.ptrProcessRemoveItemCallBackDetails = Marshal.GetFunctionPointerForDelegate(NativeBridge.ProcessRemoveItemCallBackDetails);

                processMonitorDetals = MProcessMonitor.CreateProcessMonitor(NativeBridge.ptrProcessRemoveItemCallBackDetails, NativeBridge.ptrProcessNewItemCallBackDetails, Nullptr);

                MAppWorkCall3(160, listProcessDetals.Handle);
                MAppWorkCall3(182, listProcessDetals.Handle);
                listProcessDetals.ListViewItemSorter = listViewItemComparerProcDetals;
                ComCtlApi.MListViewProcListWndProc(listProcessDetals.Handle);

                if (systemRootPath == "") systemRootPath = Marshal.PtrToStringUni(MAppWorkCall4(95, Nullptr, Nullptr));
                if (csrssPath == "") csrssPath = Marshal.PtrToStringUni(MAppWorkCall4(96, Nullptr, Nullptr));
                if (ntoskrnlPath == "") ntoskrnlPath = Marshal.PtrToStringUni(MAppWorkCall4(97, Nullptr, Nullptr));

                windowsProcess.Add(@"C:\Program Files\Windows Defender\NisSrv.exe".ToLower());
                windowsProcess.Add(@"C:\Program Files\Windows Defender\MsMpEng.exe".ToLower());
                windowsProcess.Add(svchostPath);
                windowsProcess.Add((systemRootPath + @"\System32\csrss.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\conhost.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"‪\System32\sihost.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\winlogon.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\wininit.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\smss.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\services.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\dwm.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\System32\lsass.exe").ToLower());
                windowsProcess.Add((systemRootPath + @"\explorer.exe").ToLower());

                veryimporantProcess.Add((systemRootPath + @"\System32\wininit.exe").ToLower());
                veryimporantProcess.Add((systemRootPath + @"\System32\csrss.exe").ToLower());
                veryimporantProcess.Add((systemRootPath + @"\System32\lsass.exe").ToLower());
                veryimporantProcess.Add((systemRootPath + @"\System32\smss.exe").ToLower());

                Inited = true;

                ProcessListDetailsLoadColumns();
                ProcessListDetailsILoadAllItem();

            }
        }
        public void ProcessListDetailsUnInit()
        {
            if (Inited)
            {
                ProcessListDetailsSaveColumns();
                ProcessDetalsListFreeAll();

                MProcessMonitor.DestroyProcessMonitor(processMonitorDetals);
                Inited = false;
            }
        }

        //CallBacks
        private void ProcessListDetailsRemoveItemCallBack(uint pid)
        {
            ProcessDetalItem oldps = ProcessListDetailsFindPsItem(pid);
            if (oldps != null) ProcessListDetailsFree(oldps);
            else Log("ProcessListDetailsRemoveItemCallBack for a not found item : pid " + pid);
        }
        private void ProcessListDetailsNewItemCallBack(uint pid, uint parentid, string exename, string exefullpath, IntPtr hProcess, IntPtr processItem)
        {
            if (!FormMain.IsAdmin && string.IsNullOrEmpty(exefullpath) && pid != 0 && pid != 2 && pid != 4 && pid != 88)
                return;
            ProcessListDetailsLoad(pid, parentid, exename, exefullpath, hProcess, processItem);
        }


        //Add item
        private void ProcessListDetailsLoad(uint pid, uint ppid, string exename, string exefullpath, IntPtr hprocess, IntPtr processItem)
        {
            //base
            ProcessDetalItem p = new ProcessDetalItem();

            p.pid = pid;
            p.ppid = ppid;
            loadedDetalProcess.Add(p);

            ProcessDetalItem parentpsItem = null;
            if (ProcessListDetailsIsProcessLoaded(p.ppid, out parentpsItem))
            {
                p.parent = parentpsItem;
                parentpsItem.childs.Add(p);
            }

            if (pid == 0)
                exename = LanuageFBuffers.Str_IdleProcess;
            else if (pid == 2)
                exename = LanuageFBuffers.Str_SystemInterrupts;
            else if (pid == 4 || exename == "Registry" || exename == "Memory Compression")
                exefullpath = ntoskrnlPath;
            else if (pid < 800 && ppid < 500 && exename == "csrss.exe")
                exefullpath = csrssPath;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(exefullpath);

            PEOCESSKINFO infoStruct = new PEOCESSKINFO();
            if (FormMain.IsKernelLoaded)
            {
                if (MGetProcessEprocess(pid, ref infoStruct))
                {
                    p.eprocess = infoStruct.Eprocess;

                    if (string.IsNullOrEmpty(exefullpath))
                    {
                        exefullpath = infoStruct.ImageFullName;
                        stringBuilder.Append(exefullpath);
                    }
                }
            }

            ListViewItem li = new ListViewItem();

            p.item = li;
            p.processItem = processItem;
            p.handle = hprocess;
            p.exename = exename;
            p.exepath = stringBuilder.ToString();

            //icon
            li.ImageKey = ProcessListDetailsGetIcon(p.exepath);
            li.Tag = p;

            //16 empty item
            for (int i = 0; i < 16; i++) li.SubItems.Add(new ListViewItem.ListViewSubItem());



            //static items
            ProcessListDetailsUpdateStaticItems(pid, li, p);

            ProcessListDetailsUpdate(pid, true, li);

            listProcessDetals.Items.Add(li);
        }

        //Update dyamic data
        private void ProcessListDetailsUpdateStaticItems(uint pid, ListViewItem li, ProcessDetalItem p)
        {
            //static items
            if (colNameIndex != -1 && string.IsNullOrEmpty(li.SubItems[colNameIndex].Text)) li.SubItems[colNameIndex].Text = p.exename;
            if (colPathIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPathIndex].Text)) li.SubItems[colPathIndex].Text = p.exepath;
            if (colPIDIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPIDIndex].Text))
            {
                if (pid == 2)
                    li.SubItems[colPIDIndex].Text = "-";
                else li.SubItems[colPIDIndex].Text = pid.ToString();
            }
            if (colPPIDIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPPIDIndex].Text)) li.SubItems[colPPIDIndex].Text = p.ppid.ToString();
            if (colDescriptionIndex != -1 && string.IsNullOrEmpty(li.SubItems[colDescriptionIndex].Text))
            {
                if (pid == 0)
                    li.SubItems[colDescriptionIndex].Text = LanuageFBuffers.Str_IdleProcessDsb;
                else if (pid == 2)
                    li.SubItems[colDescriptionIndex].Text = LanuageFBuffers.Str_InterruptsProcessDsb;
                else if (p.exepath != "")
                {
                    StringBuilder stringBuilderDescription = new StringBuilder(260);
                    if (MGetExeDescribe(p.exepath, stringBuilderDescription, 260))
                        li.SubItems[colDescriptionIndex].Text = stringBuilderDescription.ToString();
                }
            }

            if (pid == 2)
                goto JUMPADD;

            if (colEprocessIndex != -1 && string.IsNullOrEmpty(li.SubItems[colEprocessIndex].Text))
                li.SubItems[colEprocessIndex].Text = p.eprocess;
            if (colCommandLineIndex != -1 && string.IsNullOrEmpty(li.SubItems[colCommandLineIndex].Text))
            {
                StringBuilder stringBuilderCommandLine = new StringBuilder(512);
                if (p.handle != IntPtr.Zero && MGetProcessCommandLine(p.handle, stringBuilderCommandLine, 512, pid))
                    li.SubItems[colCommandLineIndex].Text = stringBuilderCommandLine.ToString();
            }
            if (FormMain.Is64OS && colPlatformIndex != -1 && string.IsNullOrEmpty(li.SubItems[colPlatformIndex].Text))
            {
                if (MGetProcessIs32Bit(p.handle))
                    li.SubItems[colCommandLineIndex].Text = LanuageFBuffers.Str_Process32Bit;
                else li.SubItems[colCommandLineIndex].Text = LanuageFBuffers.Str_Process64Bit;
            }

            if (colUserNameIndex != -1 && p.handle != IntPtr.Zero && string.IsNullOrEmpty(li.SubItems[colUserNameIndex].Text))
            {
                StringBuilder stringBuilderUserName = new StringBuilder(260);
                if (MGetProcessUserName(p.handle, stringBuilderUserName, 260))
                    li.SubItems[colUserNameIndex].Text = stringBuilderUserName.ToString();
            }
            if (colSessionIDIndex != -1 && string.IsNullOrEmpty(li.SubItems[colSessionIDIndex].Text))
                li.SubItems[colSessionIDIndex].Text = MGetProcessSessionID(p.processItem).ToString();
            JUMPADD:
            return;
        }
        private void ProcessListDetailsUpdate(uint pid, bool isload, ListViewItem it, int ipdateOneDataCloum = -1, bool forceProcessHost = false)
        {
            ProcessDetalItem p = it.Tag as ProcessDetalItem;
            if (colCPUIndex != -1 && colCPUIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPU(it, p);
            if (colCycleIndex != -1 && colCycleIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_Cycle(it, p);
            if (p.pid == 2) return;
            if (colWorkingSetPrivateIndex != -1 && colWorkingSetPrivateIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetPrivate(it, p);
            if (colWorkingSetIndex != -1 && colWorkingSetIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSet(it, p);
            if (colWorkingSetShareIndex != -1 && colWorkingSetShareIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetShare(it, p);
            if (colPeakWorkingSetIndex != -1 && colPeakWorkingSetIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PeakWorkingSet(it, p);
            if (colNonPagedPoolIndex != -1 && colNonPagedPoolIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_NonPagedPool(it, p);
            if (colPagedPoolIndex != -1 && colPagedPoolIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PagedPool(it, p);
            if (colCommitedSizeIndex != -1 && colCommitedSizeIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CommitedSize(it, p);
            if (colPageErrorIndex != -1 && colPageErrorIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFault(it, p);
            if (colHandleCountIndex != -1 && colHandleCountIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_HandleCount(it, p);
            if (colThreadCountIndex != -1 && colThreadCountIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_ThreadsCount(it, p);
            if (colIOReadIndex != -1 && colIOReadIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IORead(it, p);
            if (colIOWriteIndex != -1 && colIOWriteIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWrite(it, p);
            if (colIOOtherIndex != -1 && colIOOtherIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOther(it, p);
            if (colIOReadBytesIndex != -1 && colIOReadBytesIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOReadBytes(it, p);
            if (colIOWriteBytesIndex != -1 && colIOWriteBytesIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWriteBytes(it, p);
            if (colIOOtherBytesIndex != -1 && colIOOtherBytesIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOtherBytes(it, p);
            if (colCPUTimeIndex != -1 && colCPUTimeIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPUTime(it, p);
            if (colStateIndex != -1 && colStateIndex != ipdateOneDataCloum)
                ProcessListDetails_Update_State(it, p);
            if (colGDIObjectIndex != -1 && colGDIObjectIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_GdiHandleCount(it, p);
            if (colUserObjectIndex != -1 && colUserObjectIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_UserHandleCount(it, p);
            if (colWorkingSetIncreasementIndex != -1 && colWorkingSetIncreasementIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetIncreasement(it, p);
            if (colPageErrorIncreasementIndex != -1 && colPageErrorIncreasementIndex != ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFaultIncreasement(it, p);
        }
        //update a column be use to sort
        private void ProcessListDetailsUpdateOnePerfCloum(uint pid, ListViewItem it, int ipdateOneDataCloum, bool forceProcessHost = false)
        {
            ProcessDetalItem p = it.Tag as ProcessDetalItem;
            if (p.pid == 2)
            {
                if (colCPUIndex != -1 && colCPUIndex == ipdateOneDataCloum)
                    ProcessListDetails_Perf_Update_CPU(it, p);
                else if (colCycleIndex != -1 && colCycleIndex == ipdateOneDataCloum)
                    ProcessListDetails_Perf_Update_Cycle(it, p);
                return;
            }
            if (colCPUIndex != -1 && colCPUIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPU(it, p);
            else if (colWorkingSetPrivateIndex != -1 && colWorkingSetPrivateIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetPrivate(it, p);
            else if (colWorkingSetIndex != -1 && colWorkingSetIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSet(it, p);
            else if (colWorkingSetShareIndex != -1 && colWorkingSetShareIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetShare(it, p);
            else if (colPeakWorkingSetIndex != -1 && colPeakWorkingSetIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PeakWorkingSet(it, p);
            else if (colNonPagedPoolIndex != -1 && colNonPagedPoolIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_NonPagedPool(it, p);
            else if (colPagedPoolIndex != -1 && colPagedPoolIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PagedPool(it, p);
            else if (colCommitedSizeIndex != -1 && colCommitedSizeIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CommitedSize(it, p);
            else if (colPageErrorIndex != -1 && colPageErrorIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFault(it, p);
            else if (colHandleCountIndex != -1 && colHandleCountIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_HandleCount(it, p);
            else if (colThreadCountIndex != -1 && colThreadCountIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_ThreadsCount(it, p);
            else if (colIOReadIndex != -1 && colIOReadIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IORead(it, p);
            else if (colIOWriteIndex != -1 && colIOWriteIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWrite(it, p);
            else if (colIOOtherIndex != -1 && colIOOtherIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOther(it, p);
            else if (colIOReadBytesIndex != -1 && colIOReadBytesIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOReadBytes(it, p);
            else if (colIOWriteBytesIndex != -1 && colIOWriteBytesIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOWriteBytes(it, p);
            else if (colIOOtherBytesIndex != -1 && colIOOtherBytesIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_IOOtherBytes(it, p);
            else if (colCPUTimeIndex != -1 && colCPUTimeIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_CPUTime(it, p);
            else if (colCycleIndex != -1 && colCycleIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_Cycle(it, p);
            else if (colStateIndex != -1 && colStateIndex == ipdateOneDataCloum)
                ProcessListDetails_Update_State(it, p);
            else if (colGDIObjectIndex != -1 && colGDIObjectIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_GdiHandleCount(it, p);
            else if (colUserObjectIndex != -1 && colUserObjectIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_UserHandleCount(it, p);
            else if (colWorkingSetIncreasementIndex != -1 && colWorkingSetIncreasementIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_WorkingSetIncreasement(it, p);
            else if (colPageErrorIncreasementIndex != -1 && colPageErrorIncreasementIndex == ipdateOneDataCloum)
                ProcessListDetails_Perf_Update_PageFaultIncreasement(it, p);
        }
        private void ProcessListDetailsUpdateValues(int refeshAllDataColum, bool updateStaticItems = false)
        {
            //update process perf data
            foreach (ListViewItem it in listProcessDetals.Items)
            {
                if (updateStaticItems)
                    ProcessListDetailsUpdateStaticItems(((ProcessDetalItem)it.Tag).pid, it, (ProcessDetalItem)it.Tag);
                if (refeshAllDataColum != -1)
                    ProcessListDetailsUpdateOnePerfCloum(((ProcessDetalItem)it.Tag).pid, it, refeshAllDataColum);
            }
            if (updateStaticItems)
            {

                foreach (ColumnHeader c in listProcessDetals.Columns)
                {
                    itemheaderTag t = ((itemheaderTag)c.Tag);
                    if (t.needAutoSize)
                    {
                        listProcessDetals.AutoResizeColumn(c.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                        t.needAutoSize = false;
                    }
                }
            }
            if (listProcessDetals.Items.Count == 0) return;
            int start = listProcessDetals.Items.IndexOf(listProcessDetals.TopItem), end = listProcessDetals.Items.Count;
            ListViewItem liThis = null;
            for (int i = start; i < end; i++)
            {
                liThis = listProcessDetals.Items[i];
                if (liThis.Position.Y < listProcessDetals.Height)
                    ProcessListDetailsUpdate(((ProcessDetalItem)liThis.Tag).pid, false, liThis, refeshAllDataColum);
                else break;
            }
        }

        //All perf data
        private void ProcessListDetails_Perf_Update_CPU(ListViewItem it, ProcessDetalItem p)
        {
            double data = MProcessPerformanctMonitor.GetProcessCpuUseAge(p.processItem);
            it.SubItems[colCPUIndex].Text = data.ToString("00.0");
            it.SubItems[colCPUIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_CPUTime(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessCpuTime(p.processItem);
            TimeSpan time = TimeSpan.FromMilliseconds(Convert.ToDouble(data));
            it.SubItems[colCPUTimeIndex].Text = time.Hours + ":" + time.Minutes + ":" + time.Seconds;
            it.SubItems[colCPUTimeIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_Cycle(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessCycle(p.processItem);
            it.SubItems[colCycleIndex].Text = data.ToString();
            it.SubItems[colCycleIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSetPrivate(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSETPRIVATE);
            it.SubItems[colWorkingSetPrivateIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetPrivateIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSetShare(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSETSHARE);
            it.SubItems[colWorkingSetShareIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetShareIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSet(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSET);
            it.SubItems[colWorkingSetIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_WorkingSetIncreasement(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_WORKINGSET_INC);
            it.SubItems[colWorkingSetIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colWorkingSetIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PeakWorkingSet(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PEAKWORKINGSET);
            it.SubItems[colPeakWorkingSetIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colPeakWorkingSetIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_NonPagedPool(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_NONPAGEDPOOL);
            it.SubItems[colNonPagedPoolIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colNonPagedPoolIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PagedPool(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PAGEDPOOL);
            it.SubItems[colPagedPoolIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colPagedPoolIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_CommitedSize(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_COMMITEDSIZE);
            it.SubItems[colCommitedSizeIndex].Text = FormatFileSizeMenSingal(data);
            it.SubItems[colCommitedSizeIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PageFault(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PAGEDFAULT);
            it.SubItems[colPageErrorIndex].Text = data.ToString();
            it.SubItems[colPageErrorIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_PageFaultIncreasement(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MProcessPerformanctMonitor.GetProcessMemoryInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCMEM_PAGEDFAULT_INC);
            it.SubItems[colPageErrorIndex].Text = data.ToString();
            it.SubItems[colPageErrorIndex].Tag = data;
        }

        private void ProcessListDetails_Perf_Update_HandleCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessHandlesCount(p.processItem);
            it.SubItems[colHandleCountIndex].Text = data.ToString();
            it.SubItems[colHandleCountIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_ThreadsCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessThreadsCount(p.processItem);
            it.SubItems[colThreadCountIndex].Text = data.ToString();
            it.SubItems[colThreadCountIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_GdiHandleCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessGdiHandleCount(p.handle);
            it.SubItems[colGDIObjectIndex].Text = data.ToString();
            it.SubItems[colGDIObjectIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_UserHandleCount(ListViewItem it, ProcessDetalItem p)
        {
            uint data = MGetProcessUserHandleCount(p.handle);
            it.SubItems[colUserObjectIndex].Text = data.ToString();
            it.SubItems[colUserObjectIndex].Tag = data;
        }

        private void ProcessListDetails_Perf_Update_IORead(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_READ);
            it.SubItems[colIOReadIndex].Text = data.ToString();
            it.SubItems[colIOReadIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOWrite(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_WRITE);
            it.SubItems[colIOWriteIndex].Text = data.ToString();
            it.SubItems[colIOWriteIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOOther(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_OTHER);
            it.SubItems[colIOOtherIndex].Text = data.ToString();
            it.SubItems[colIOOtherIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOReadBytes(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_READ_BYTES);
            it.SubItems[colIOReadBytesIndex].Text = data.ToString();
            it.SubItems[colIOReadBytesIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOWriteBytes(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_WRITE_BYTES);
            it.SubItems[colIOWriteBytesIndex].Text = data.ToString();
            it.SubItems[colIOWriteBytesIndex].Tag = data;
        }
        private void ProcessListDetails_Perf_Update_IOOtherBytes(ListViewItem it, ProcessDetalItem p)
        {
            UInt64 data = MProcessPerformanctMonitor.GetProcessIOInfo(p.processItem, MProcessPerformanctMonitor.M_GET_PROCIO_OTHER_BYTES);
            it.SubItems[colIOOtherBytesIndex].Text = data.ToString();
            it.SubItems[colIOOtherBytesIndex].Tag = data;
        }

        private void ProcessListDetails_Update_State(ListViewItem it, ProcessDetalItem p)
        {
            int i = MGetProcessState(p.processItem, IntPtr.Zero);
            if (i == 1)
            {
                it.SubItems[colStateIndex].Text = "";
                /*if (p.isSvchost == false && it.Childs.Count > 0)
                {
                    bool hung = false;
                    foreach (TaskMgrListItem c in it.Childs)
                        if (c.Type == TaskMgrListItemType.ItemWindow)
                            if (IsHungAppWindow((IntPtr)c.Tag))
                            {
                                hung = true;
                                break;
                            }
                    if (hung)
                    {
                        it.SubItems[colStateIndex].Text = str_status_hung;
                        it.SubItems[colStateIndex].ForeColor = Color.FromArgb(219, 107, 58);
                    }
                }*/
            }
            else if (i == 2)
            {
                it.SubItems[colStateIndex].Text = LanuageFBuffers.Str_StatusPaused;
                it.SubItems[colStateIndex].ForeColor = System.Drawing.Color.FromArgb(22, 158, 250);
            }
        }

        public void ProcessListDetailsHeaderRightClick(int colLastDown)
        {
            this.colLastDown = colLastDown;
            FormMain.隐藏列ToolStripMenuItem.Enabled = colLastDown != colNameIndex;
            FormMain.contextMenuStripProcDetalsCol.Show(MousePosition);
        }
        public void ProcessListDetailsHeaderMouseMove(int clicks, Point pos)
        {
            listProcessDetals_ColumnMouseMove(listProcessDetals, clicks, pos);
        }

        //Full load
        public void ProcessListDetailsILoadAllItem()
        {
            ComCtlApi.MListViewProcListLock(true);
            MProcessMonitor.EnumAllProcess(processMonitorDetals);
            ComCtlApi.MListViewProcListLock(false);
            listProcessDetals.Invalidate();
        }

        //Refesh
        public void ProcessListDetailsRefesh()
        {
            ComCtlApi.MListViewProcListLock(true);

            //刷新所有数据
            MProcessMonitor.RefeshAllProcess(processMonitorDetals);
            //刷新性能数据
            bool refeshAColumData = ProcessListDetailsIsDyamicDataColumn(listViewItemComparerProcDetals.SortColumn);
            ProcessListDetailsUpdateValues(refeshAColumData ? listViewItemComparerProcDetals.SortColumn : -1, nextUpdateStaticVals);

            nextUpdateStaticVals = false;

            listProcessDetals.Sort();
            ComCtlApi.MListViewProcListLock(false);
            listProcessDetals.Invalidate();
        }
        private void ProcessListDetailsFree(ProcessDetalItem it, bool delitem = true)
        {
            //remove invalid item
            //MAppWorkCall3(174, IntPtr.Zero, new IntPtr(it.pid));

            it.childs.Clear();
            if (it.parent != null && it.parent.childs.Contains(it))
                it.parent.childs.Remove(it);
            it.parent = null;
            loadedDetalProcess.Remove(it);
            if (delitem) listProcessDetals.Items.Remove(it.item);
        }
        private void ProcessDetalsListFreeAll()
        {
            listProcessDetals.Items.Clear();
            //the exit clear
            for (int i = 0; i < loadedDetalProcess.Count; i++)
                ProcessListDetailsFree(loadedDetalProcess[i], false);
            loadedDetalProcess.Clear();
        }

        //Ico
        private string ProcessListDetailsGetIcon(string exepath)
        {
            if (exepath == "") exepath = "Default";
            if (!FormMain.imageListProcessDetalsIcons.Images.ContainsKey(exepath))
            {
                IntPtr intPtr = MGetExeIcon(exepath == "Default" ? null : exepath);
                if (intPtr != IntPtr.Zero)
                    FormMain.imageListProcessDetalsIcons.Images.Add(exepath, Icon.FromHandle(intPtr));
            }
            return exepath;
        }

        //Events
        private void listProcessDetals_MouseClick(object sender, MouseEventArgs e)
        {
            if (listProcessDetals.SelectedItems.Count == 0) return;
            ProcessDetalItem ps = listProcessDetals.SelectedItems[0].Tag as ProcessDetalItem;
            if (e.Button == MouseButtons.Left)
            {
                if (ps.pid > 4)
                {
                    btnEndProcessDetals.Enabled = true;
                    MAppWorkShowMenuProcessPrepare(ps.exepath, ps.exename, ps.pid, IsImporant(ps), IsVeryImporant(ps));
                    nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                }
                else btnEndProcessDetals.Enabled = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                nextSecType = MENU_SELECTED_PROCESS_KILL_ACT_KILL;
                MAppWorkShowMenuProcessPrepare(ps.exepath, ps.exename, ps.pid, IsImporant(ps), IsVeryImporant(ps));
                MAppWorkShowMenuProcess(ps.exepath, ps.exename, ps.pid, FormMain.Handle, IntPtr.Zero, 0, nextSecType, FormMain.MousePosition.X, FormMain.MousePosition.Y);
            }
        }
        private void listProcessDetals_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEndProcessDetals.Enabled = listProcessDetals.SelectedItems.Count != 0;
        }
        private void listProcessDetals_KeyDown(object sender, KeyEventArgs e)
        {
            if (listProcessDetals.SelectedItems.Count > 0)
            {
                if (e.KeyCode == Keys.Apps)
                {
                    ListViewItem item = listProcessDetals.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listProcessDetals.PointToScreen(p);
                    ProcessDetalItem ps = item.Tag as ProcessDetalItem;
                    MAppWorkShowMenuProcess(ps.exepath, ps.exename, ps.pid, FormMain.Handle, IntPtr.Zero, 0, nextSecType, p.X, p.Y);
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    ListViewItem item = listProcessDetals.SelectedItems[0];
                    ProcessDetalItem ps = item.Tag as ProcessDetalItem;
                    MAppWorkShowMenuProcessPrepare(ps.exepath, ps.exename, ps.pid, IsImporant(ps), IsVeryImporant(ps));
                    MAppWorkCall3(178, FormMain.Handle, IntPtr.Zero);
                }
            }
        }

        private void btnEndProcessDetals_Click(object sender, EventArgs e)
        {
            if (listProcessDetals.SelectedItems.Count == 0) return;
            MAppWorkCall3(178, FormMain.Handle, IntPtr.Zero);
        }

        #region Columns

        private int colLastDown = -1;

        private int colEprocessIndex = -1;//ok
        private int colUserNameIndex = -1;//ok
        private int colNameIndex = -1;//ok
        private int colPackageNameIndex = -1;//n
        private int colPIDIndex = -1;//ok
        private int colPPIDIndex = -1;//ok
        private int colStateIndex = -1;//ok
        private int colSessionIDIndex = -1;//ok
        private int colJobIDIndex = -1;
        private int colCPUIndex = -1;//ok
        private int colCPUTimeIndex = -1;//ok
        private int colCycleIndex = -1;//ok
        private int colPeakWorkingSetIndex = -1;//ok
        private int colWorkingSetIncreasementIndex = -1;//ok
        private int colWorkingSetIndex = -1;//ok
        private int colWorkingSetPrivateIndex = -1;//ok
        private int colWorkingSetShareIndex = -1;//ok
        private int colCommitedSizeIndex = -1;//ok
        private int colPagedPoolIndex = -1;//ok
        private int colNonPagedPoolIndex = -1;//ok
        private int colPageErrorIndex = -1;//ok
        private int colPageErrorIncreasementIndex = -1;//ok
        private int colHandleCountIndex = -1;//ok
        private int colThreadCountIndex = -1;//ok
        private int colUserObjectIndex = -1;//ok
        private int colGDIObjectIndex = -1;//ok
        private int colIOReadIndex = -1;//ok
        private int colIOWriteIndex = -1;//ok
        private int colIOOtherIndex = -1;//ok
        private int colIOReadBytesIndex = -1;//ok
        private int colIOWriteBytesIndex = -1;//ok
        private int colIOOtherBytesIndex = -1;//ok
        private int colPathIndex = -1;//ok
        private int colCommandLineIndex = -1;//ok
        private int colPlatformIndex = -1;//ok
        private int colOSContextIndex = -1;
        private int colDescriptionIndex = -1;//ok
        private int colDepIndex = -1;
        private int colUACVIndex = -1;

        private IntPtr hListHeader = IntPtr.Zero;
        private ToolTip colsTip = new ToolTip();

        public string[] allCols = new string[]
        {
            "TitlePID","TitlePackageName","TitleStatus","TitleSessionID","TitleJobID","TitleParentPID","TitleCycle",
            "TitleCPU","TitleCPUTime","TitlePeakWorkingSet","TitleWorkingSetCrease",
            "TitleWorkingSet","TitleWorkingSetPrivate","TitleWorkingSetShare","TitleCommited",
            "TitlePagedPool","TitleNonPagedPool","TitlePagedError","TitlePagedErrorCrease","TitleHandleCount",
            "TitleThreadCount","TitleUserObject","TitleGdiObject","TitleIORead","TitleIOWrite","TitleIOOther",
            "TitleIOReadBytes","TitleIOWriteBytes","TitleIOOtherBytes","TitleProcPath","TitleCmdLine","TitleEProcess",
            "TitlePlatform","TitleOperationSystemContext","TitleDescription","TitleDEP","TitleUACVirtualization","TitleProcName"
        };
        public string[] numberCols = new string[]
        {
            "TitlePID","TitleSessionID","TitleJobID","TitleParentPID","TitleCycle",
            "TitleCPU","TitlePeakWorkingSet","TitleWorkingSetCrease",
            "TitleWorkingSet","TitleWorkingSetPrivate","TitleWorkingSetShare","TitleCommited",
            "TitlePagedPool","TitleNonPagedPool","TitlePagedError","TitlePagedErrorCrease","TitleHandleCount",
            "TitleThreadCount","TitleUserObject","TitleGdiObject","TitleIORead","TitleIOWrite","TitleIOOther",
            "TitleIOReadBytes","TitleIOWriteBytes","TitleIOOtherBytes"
        };

        private itemheaderTip[] detailsHeaderTips = new itemheaderTip[]{
            new itemheaderTip("TitleCPU", "TipCPU"),
            new itemheaderTip("TitlePID", "TipPID"),
            new itemheaderTip("TitleStatus", "TipStatus"),
            new itemheaderTip("TitleJobID", "TipJobID"),
            new itemheaderTip("TitleWorkingSetPrivate", "TipPrivateWorkingSet"),
            new itemheaderTip("TitleCPUTime", "TipCPUTime"),
            new itemheaderTip("TitleCycle", "TipCycle"),
            new itemheaderTip("TitleCommited", "TipCommitedSize"),
            new itemheaderTip("TitlePagedPool", "TipPagedSize"),
            new itemheaderTip("TitleNonPagedPool", "TipNonPagedSize"),
            new itemheaderTip("TitlePagedError", "TipPageErr"),
            new itemheaderTip("TitleHandleCount", "TipHandleCount"),
            new itemheaderTip("TitleThreadCount", "TipThredCount"),
            new itemheaderTip("TitleCmdLine", "TipCmdLine"),
            new itemheaderTip("TitleUserObject", "TipUserObject"),
            new itemheaderTip("TitleGdiObject", "TipGDIObject"),
            new itemheaderTip("TitlePlatform", "TipPlatform"),
            new itemheaderTip("TitleWorkingSet", "TipWorkingSet"),
            new itemheaderTip("TitleWorkingSetShare", "TipShareWorkingSet"),
            new itemheaderTip("TitleIOOther", "TipIOOther"),
            new itemheaderTip("TitleIOOtherBytes", "TipIOOtherBytes"),
            new itemheaderTip("TitleIORead", "TipIORead"),
            new itemheaderTip("TitleIOReadBytes", "TipIOReadBytes"),
            new itemheaderTip("TitleIOWrite", "TipIOWrite"),
            new itemheaderTip("TitleIOWriteBytes", "TipIOWriteBytes"),
        };
        private class itemheaderTag
        {
            public string tip;
            public bool needAutoSize;
        }

        public string ProcessListDetailsGetHeaderTip(string name)
        {
            foreach (itemheaderTip t in detailsHeaderTips)
            {
                if (t.herdername == name)
                    return LanuageMgr.GetStr(t.name);
            }
            return null;
        }
        public void ProcessListDetailsAddHeader(string name, int width = -1)
        {
            itemheaderTag t = new itemheaderTag();
            ColumnHeader li = new ColumnHeader();
            li.Name = name;
            li.Text = LanuageMgr.GetStr(name, false);
            if (width == -1)
            {
                t.needAutoSize = true;
                width = 100;
            }
            else t.needAutoSize = false;
            li.Width = width;
            string tip = ProcessListDetailsGetHeaderTip(name);
            if (ProcessListDetailsIsMumberColumn(name))
                li.TextAlign = HorizontalAlignment.Right;
            listProcessDetals.Columns.Add(li);

            if (tip != null) t.tip = tip;

            li.Tag = t;
        }
        public int ProcessListDetailsGetListIndex(string name)
        {
            int rs = -1;
            ColumnHeader c = ProcessListDetailsFindHeader(name);
            if (c != null)
                rs = listProcessDetals.Columns.IndexOf(c);
            return rs;
        }
        public ColumnHeader ProcessListDetailsFindHeader(string name)
        {
            ColumnHeader rs = null;
            foreach (ColumnHeader c in listProcessDetals.Columns)
                if (c.Name == name)
                {
                    rs = c;
                    break;
                }
            return rs;
        }
        public void ProcessListDetailsRemoveHeader(string name)
        {
            ColumnHeader li = ProcessListDetailsFindHeader(name);
            if (li != null) listProcessDetals.Columns.Remove(li);
        }

        private bool ProcessListDetailsIsDyamicDataColumn(int index)
        {
            if (index != -1)
            {
                if (index == colCPUIndex || index == colCPUTimeIndex || index == colCycleIndex
                    || index == colPeakWorkingSetIndex || index == colWorkingSetIncreasementIndex || index == colWorkingSetIndex
                    || index == colWorkingSetPrivateIndex || index == colWorkingSetShareIndex || index == colCommitedSizeIndex
                    || index == colPagedPoolIndex || index == colHandleCountIndex
                    || index == colNonPagedPoolIndex || index == colPageErrorIndex || index == colPageErrorIncreasementIndex
                    || index == colThreadCountIndex || index == colGDIObjectIndex || index == colUserObjectIndex
                    || index == colIOOtherBytesIndex || index == colIOWriteBytesIndex || index == colIOReadBytesIndex
                    || index == colIOOtherIndex || index == colIOWriteIndex || index == colIOReadIndex)
                    return true;
            }
            return false;
        }
        private bool ProcessListDetailsIsMumberColumn(string name)
        {
            if (name != "")
            {
                foreach (string i in numberCols)
                    if (i == name)
                        return true;
            }
            return false;
        }
        public bool ProcessListDetailsIsStringColumn(int index)
        {
            if (index != -1)
            {
                if (index == colNameIndex || index == colPackageNameIndex || index == colStateIndex
                    || index == colCPUTimeIndex || index == colPathIndex || index == colCommandLineIndex
                    || index == colPlatformIndex || index == colOSContextIndex || index == colDescriptionIndex
                    || index == colDepIndex || index == colUACVIndex || index == colUserNameIndex)
                    return true;
            }
            return false;
        }
        public void ProcessListDetailsGetColumnsIndex()
        {
            //加载所有列表头的序号
            colPPIDIndex = ProcessListDetailsGetListIndex("TitleParentPID");
            colPIDIndex = ProcessListDetailsGetListIndex("TitlePID");
            colPackageNameIndex = ProcessListDetailsGetListIndex("TitlePackageName");
            colStateIndex = ProcessListDetailsGetListIndex("TitleStatus");
            colSessionIDIndex = ProcessListDetailsGetListIndex("TitleSessionID");
            colJobIDIndex = ProcessListDetailsGetListIndex("TitleJobID");
            colCPUIndex = ProcessListDetailsGetListIndex("TitleCPU");
            colCPUTimeIndex = ProcessListDetailsGetListIndex("TitleCPUTime");
            colCycleIndex = ProcessListDetailsGetListIndex("TitleCycle");
            colPeakWorkingSetIndex = ProcessListDetailsGetListIndex("TitlePeakWorkingSet");
            colWorkingSetIncreasementIndex = ProcessListDetailsGetListIndex("TitleWorkingSetCrease");
            colWorkingSetIndex = ProcessListDetailsGetListIndex("TitleWorkingSet");
            colWorkingSetPrivateIndex = ProcessListDetailsGetListIndex("TitleWorkingSetPrivate");
            colWorkingSetShareIndex = ProcessListDetailsGetListIndex("TitleWorkingSetShare");
            colCommitedSizeIndex = ProcessListDetailsGetListIndex("TitleCommited");
            colPagedPoolIndex = ProcessListDetailsGetListIndex("TitlePagedPool");
            colNonPagedPoolIndex = ProcessListDetailsGetListIndex("TitleNonPagedPool");
            colPageErrorIndex = ProcessListDetailsGetListIndex("TitlePagedError");
            colPageErrorIncreasementIndex = ProcessListDetailsGetListIndex("TitlePagedErrorCrease");
            colHandleCountIndex = ProcessListDetailsGetListIndex("TitleHandleCount");
            colThreadCountIndex = ProcessListDetailsGetListIndex("TitleThreadCount");
            colUserObjectIndex = ProcessListDetailsGetListIndex("TitleUserObject");
            colGDIObjectIndex = ProcessListDetailsGetListIndex("TitleGdiObject");
            colIOReadIndex = ProcessListDetailsGetListIndex("TitleIORead");
            colIOWriteIndex = ProcessListDetailsGetListIndex("TitleIOWrite");
            colIOOtherIndex = ProcessListDetailsGetListIndex("TitleIOOther");
            colIOReadBytesIndex = ProcessListDetailsGetListIndex("TitleIOReadBytes");
            colIOWriteBytesIndex = ProcessListDetailsGetListIndex("TitleIOWriteBytes");
            colIOOtherBytesIndex = ProcessListDetailsGetListIndex("TitleIOOtherBytes");
            colPathIndex = ProcessListDetailsGetListIndex("TitleProcPath");
            colCommandLineIndex = ProcessListDetailsGetListIndex("TitleCmdLine");
            colPlatformIndex = ProcessListDetailsGetListIndex("TitlePlatform");
            colOSContextIndex = ProcessListDetailsGetListIndex("TitleOperationSystemContext");
            colDescriptionIndex = ProcessListDetailsGetListIndex("TitleDescription");
            colDepIndex = ProcessListDetailsGetListIndex("TitleDEP");
            colUACVIndex = ProcessListDetailsGetListIndex("TitleUACVirtualization");
            colNameIndex = ProcessListDetailsGetListIndex("TitleProcName");
            colUserNameIndex = ProcessListDetailsGetListIndex("TitleUserName");
            colEprocessIndex = ProcessListDetailsGetListIndex("TitleEProcess");

        }
        private void ProcessListDetailsSaveColumns()
        {
            if (listProcessDetals.Columns.Count > 0)
            {
                string finalString = "";
                ColumnHeader currentColumn = null;
                for (int i = listProcessDetals.Columns.Count - 1; i >= 0; i--)
                {
                    currentColumn = null;
                    foreach (ColumnHeader li in listProcessDetals.Columns)
                        if (li.DisplayIndex == i)
                        {
                            currentColumn = li;
                            break;
                        }
                    if (currentColumn != null)
                        finalString = currentColumn.Name + "-" + currentColumn.Width + "#" + finalString;
                }
                SetConfig("DetalHeaders", "AppSetting", finalString);
                SetConfig("DetalSort", "AppSetting", listViewItemComparerProcDetals.SortColumn + "#" + (listViewItemComparerProcDetals.Asdening ? "Asdening" : "Descending"));
            }
        }
        private void ProcessListDetailsLoadColumns()
        {
            hListHeader = ComCtlApi.MListViewGetHeaderControl(listProcessDetals.Handle);
            //加载列表头
            if (listProcessDetals.Columns.Count > 0) listProcessDetals.Columns.Clear();
            string headersStr = GetConfig("DetalHeaders", "AppSetting");
            if (headersStr == "") headersStr = "TitleProcName-190#TitlePID-55#TitleStatus-55#TitleUserName-70#TitleCPU-60#TitleWorkingSetPrivate-70#TitleDescription-400#";
            string[] headers = headersStr.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < headers.Length && i < 16; i++)
                ProcessListDetailsAddColumns(headers[i]);

            ProcessListDetailsGetColumnsIndex();
            if (colNameIndex == -1)
            {
                ProcessListDetailsAddColumns("TitleProcName-130");
                colNameIndex = ProcessListDetailsGetListIndex("TitleProcName");
            }

            string sortInfo = GetConfig("DetalSort", "AppSetting");
            if (sortInfo.Contains("#"))
            {
                string[] sortInfo2 = sortInfo.Split('#');
                if (sortInfo.Length >= 2)
                {
                    int col = 0;
                    int.TryParse(sortInfo2[0], out col);
                    if (col >= 0 && col < listProcessDetals.Columns.Count)
                        listViewItemComparerProcDetals.SortColumn = col;
                    if (sortInfo2[1] == "Asdening") listViewItemComparerProcDetals.Asdening = true;
                    else if (sortInfo2[1] == "Descending") listViewItemComparerProcDetals.Asdening = false;
                    ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn, listViewItemComparerProcDetals.Asdening, false);
                }
            }
        }
        private void ProcessListDetailsAddColumns(string s)
        {
            string sname = s; int width = 70;
            if (s.Contains("-"))
            {
                string[] ss = s.Split('-');
                sname = ss[0];
                if (ss.Length >= 2)
                    int.TryParse(ss[1], out width);
            }
            if (width > 1024 || width <= 0) width = 70;
            if (s.Trim() != "") ProcessListDetailsAddHeader(sname, width);
        }

        private void 隐藏列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colLastDown != -1)
            {
                listProcessDetals.Columns.Remove(listProcessDetals.Columns[colLastDown]);
                colLastDown = -1;
                ProcessListDetailsGetColumnsIndex();
            }
        }
        private void 选择列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDetalsistHeaders(FormMain).ShowDialog();
        }
        private void 将此列调整为合适大小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colLastDown != -1)
                listProcessDetals.AutoResizeColumn(colLastDown, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private int listProcessDetals_lastEnterColumn = -1;

        private void listProcessDetals_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            colLastDown = e.Column;
            if (listViewItemComparerProcDetals.SortColumn != e.Column)
            {
                ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn, false, true);
                listViewItemComparerProcDetals.SortColumn = e.Column;
                listViewItemComparerProcDetals.Asdening = true;
                ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn,
                    listViewItemComparerProcDetals.Asdening, false);
            }
            else
            {
                listViewItemComparerProcDetals.Asdening = !listViewItemComparerProcDetals.Asdening;
                ComCtlApi.MListViewSetColumnSortArrow(hListHeader, listViewItemComparerProcDetals.SortColumn,
                      listViewItemComparerProcDetals.Asdening, false);
            }
            listProcessDetals.Sort();
        }
        private void listProcessDetals_ColumnMouseMove(object sender, int index, Point p)
        {
            if (index > 0 && index < listProcessDetals.Columns.Count)
            {
                if (index != listProcessDetals_lastEnterColumn)
                {
                    listProcessDetals_lastEnterColumn = index;
                    ColumnHeader col = listProcessDetals.Columns[index];
                    if (col.Tag != null)
                    {
                        colsTip.Show(((itemheaderTag)col.Tag).tip, listProcessDetals, p.X, p.Y + 3, 5000);
                    }
                    else
                    {
                        listProcessDetals_lastEnterColumn = -1;
                        colsTip.Hide(listProcessDetals);
                    }
                }
            }
            else
            {
                listProcessDetals_lastEnterColumn = -1;
                colsTip.Hide(listProcessDetals);
            }
        }
        private void listProcessDetals_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            nextUpdateStaticVals = true;
        }

        private ListViewItemComparerProcDetals listViewItemComparerProcDetals = null;
        private class ListViewItemComparerProcDetals : IComparer
        {
            public ListViewItemComparerProcDetals(MainPageProcessDetails m)
            {
                formMain = m;
            }

            private MainPageProcessDetails formMain;

            public int SortColumn { get; set; }
            public bool Asdening { get; set; } = false;

            public int Compare(object o1, object o2)
            {
                ListViewItem x = o1 as ListViewItem, y = o2 as ListViewItem;
                int returnVal = -1;
                if (x.SubItems[SortColumn].Text == y.SubItems[SortColumn].Text) return -1;
                if (formMain.ProcessListDetailsIsStringColumn(SortColumn))
                {
                    returnVal = String.Compare(((ListViewItem)x).SubItems[SortColumn].Text, ((ListViewItem)y).SubItems[SortColumn].Text);
                }
                else
                {
                    UInt64 xi, yi;
                    if (UInt64.TryParse(x.SubItems[SortColumn].Text, out xi) && UInt64.TryParse(y.SubItems[SortColumn].Text, out yi))
                    {
                        if (x.SubItems[SortColumn].Text == y.SubItems[SortColumn].Text) returnVal = 0;
                        else if (xi > yi) returnVal = 1;
                        else if (xi < yi) returnVal = -1;
                    }
                }
                if (Asdening) returnVal = -returnVal;
                return returnVal;
            }
        }


        #endregion
    }
}
