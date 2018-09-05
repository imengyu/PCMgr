using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PCMgr.Lanuages;

namespace PCMgr.Ctls
{
    public partial class PerformancePageRam : UserControl, IPerformancePage
    {
        public PerformancePageRam()
        {
            InitializeComponent();
        }

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

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MPERF_GetRamUseAge2();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_UpdatePerformance();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_UpdateMemoryListInfo();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_GetMemoryCompressionInfo(ref SYSTEM_COMPRESSION_INFO _compression_info);

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetStandBySize();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetModifiedSize();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetPageSize();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetKernelPaged();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetKernelNonpaged();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetSystemCacheSize();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetCommitTotal();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetCommitLimit();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamAvail();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamAvailPageFile();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetAllRam();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamUsed();

        private string fTipVauleFree;
        private string fTipVauleModified;
        private string fTipVauleStandby;
        private string fTipVauleUsing;
        private ulong all_ram = 0;
        private bool compressInfoFailed = false;

        public Panel GridPanel => panelGrid;
        public bool PageIsGraphicMode { get; set; }
        public bool PageIsActive { get; set; }
        public bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2)
        {
            int ramuse = (int)(MPERF_GetRamUseAge2() * 100);
            if (!PageIsActive)
                performanceGridGlobal.AddData(ramuse);
            ulong all = MPERF_GetAllRam();
            ulong used = MPERF_GetRamUsed();

            ulong divor = 0;
            string unit = NativeMethods.GetBestFilesizeUnit(all, out divor);

            customString =
               (used / (double)divor).ToString("0.0") + " " + unit + "/" + (all / (double)divor).ToString("0.0") + " " + unit + "  (" + ramuse + "%)";

            outdata1 = ramuse;
            outdata2 = -1;
            return true;
        }
        public void PageDelete()
        {
        }
        public void PageHide()
        {
            Hide();
        }
        public void PageShow()
        {
            Show();
        }
        public void PageUpdate()
        {
            int ramuse = (int)(MPERF_GetRamUseAge2() * 100);
            performanceGridGlobal.AddData(ramuse);
            performanceGridGlobal.Invalidate();

            if (MPERF_UpdatePerformance())
            {
                MPERF_UpdateMemoryListInfo();

                ulong pagesize = MPERF_GetPageSize();
                ulong availableSize = MPERF_GetRamAvail();
                ulong usedSize = all_ram - availableSize;
                ulong compressedSize = 0;
                ulong compressedEstimateSize = 0;
                ulong compressedSavedSize = 0;
                ulong modifedSize= MPERF_GetModifiedSize();
                ulong standbySize = MPERF_GetStandBySize();
                ulong freeSize = availableSize - modifedSize - standbySize;
                ulong divier = all_ram / 1048576;

                if(!compressInfoFailed)
                {
                    SYSTEM_COMPRESSION_INFO compressionInfo = new SYSTEM_COMPRESSION_INFO();
                    if (MPERF_GetMemoryCompressionInfo(ref compressionInfo))
                    {
                        compressedSize = compressionInfo.CompressionWorkingSetSize;
                        compressedEstimateSize = compressionInfo.CompressedSize;
                        if (compressedEstimateSize > compressedSize)
                            compressedSavedSize = compressedEstimateSize - compressedSize;
                    }
                    else
                    {
                        fTipVauleUsing = LanuageMgr.GetStr("MemTipUsingS");
                        compressInfoFailed = true;
                    }
                }

                performanceRamPoolGrid.VauleUsing = (usedSize / 1048576) / (double)(divier);
                performanceRamPoolGrid.VauleModified = (modifedSize / 1048576) / (double)(divier);
                performanceRamPoolGrid.VauleStandby = (standbySize / 1048576) / (double)(divier);
                performanceRamPoolGrid.VauleFree = (freeSize / 1048576) / (double)(divier);
                performanceRamPoolGrid.StrVauleUsing = NativeMethods.FormatFileSize(usedSize);
                performanceRamPoolGrid.StrVauleModified = NativeMethods.FormatFileSize(modifedSize);
                performanceRamPoolGrid.StrVauleStandby = NativeMethods.FormatFileSize(standbySize);
                performanceRamPoolGrid.StrVauleFree = NativeMethods.FormatFileSize(freeSize);
                performanceRamPoolGrid.TipVauleFree = string.Format(fTipVauleFree, performanceRamPoolGrid.StrVauleFree);
                performanceRamPoolGrid.TipVauleModified = string.Format(fTipVauleModified, performanceRamPoolGrid.StrVauleModified);
                performanceRamPoolGrid.TipVauleStandby = string.Format(fTipVauleStandby, performanceRamPoolGrid.StrVauleStandby);

                if (compressInfoFailed) performanceRamPoolGrid.TipVauleUsing = string.Format(fTipVauleUsing, performanceRamPoolGrid.StrVauleUsing);
                else performanceRamPoolGrid.TipVauleUsing = string.Format(fTipVauleUsing, performanceRamPoolGrid.StrVauleUsing, NativeMethods.FormatFileSize(compressedSize), NativeMethods.FormatFileSize(compressedEstimateSize), NativeMethods.FormatFileSize(compressedSavedSize));

                performanceRamPoolGrid.Invalidate();

                if (compressInfoFailed) item_ramuseage.Value = NativeMethods.FormatFileSize(usedSize);
                else item_ramuseage.Value = NativeMethods.FormatFileSize(usedSize) + " (" + NativeMethods.FormatFileSize(compressedSize) + ")";

                item_ramcanuse.Value = NativeMethods.FormatFileSize(availableSize);

                item_sended.Value = NativeMethods.FormatFileSize(pagesize * MPERF_GetCommitTotal()) + "/" + NativeMethods.FormatFileSize(pagesize * MPERF_GetCommitLimit());
                item_cached.Value = NativeMethods.FormatFileSize(pagesize * MPERF_GetSystemCacheSize());
                item_nopagepool.Value = NativeMethods.FormatFileSize(pagesize * MPERF_GetKernelNonpaged());
                item_pagepool.Value = NativeMethods.FormatFileSize(pagesize * MPERF_GetKernelPaged());
                performanceInfos.Invalidate();
            }
        }
        public void PageSetGridUnit(string s)
        {
            performanceGridGlobal.LeftBottomText = s;
        }
        public void PageInit()
        {
            item_ramuseage = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_ramcanuse = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_sended = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_cached = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_pagepool = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_nopagepool = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_ramuseage.Name = LanuageMgr.GetStr("MemUsing");
            item_ramcanuse.Name = LanuageMgr.GetStr("MenCanUse");
            item_sended.LineSp = true;
            item_sended.Name = LanuageMgr.GetStr("Submited");
            item_cached.Name = LanuageMgr.GetStr("Cached");
            item_pagepool.LineSp = true;
            item_pagepool.Name = LanuageMgr.GetStr("PagedPool");
            item_nopagepool.Name = LanuageMgr.GetStr("NonPagedPool");
            performanceInfos.SpeicalItems.Add(item_ramuseage);
            performanceInfos.SpeicalItems.Add(item_ramcanuse);
            performanceInfos.SpeicalItems.Add(item_sended);
            performanceInfos.SpeicalItems.Add(item_cached);
            performanceInfos.SpeicalItems.Add(item_pagepool);
            performanceInfos.SpeicalItems.Add(item_nopagepool);
            all_ram = MPERF_GetAllRam();
            performanceGridGlobal.RightText = NativeMethods.FormatFileSize(all_ram);

            NativeMethods.DeviceApi.MDEVICE_GetMemoryDeviceInfo();
            performanceTitle.SmallTitle = Marshal.PtrToStringUni(NativeMethods.DeviceApi.MDEVICE_GetMemoryDeviceName());
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("Speed"), NativeMethods.DeviceApi.MDEVICE_GetMemoryDeviceSpeed().ToString() + " MHz"));
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("FormFactor"),
              NativeMethods.DeviceApi.MDEVICE_MemoryFormFactorToString(NativeMethods.DeviceApi.MDEVICE_GetMemoryDeviceFormFactor())
                ));
            UInt16 used = 0, all = 0;
            if (NativeMethods.DeviceApi.MDEVICE_GetMemoryDeviceUsed(ref all, ref used))
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("DeviceLocator"), used + "/" + all));

            fTipVauleFree = LanuageMgr.GetStr("MemTipFree");
            fTipVauleModified = LanuageMgr.GetStr("MemTipModifed");
            fTipVauleStandby = LanuageMgr.GetStr("MemTipStandby");
            fTipVauleUsing = LanuageMgr.GetStr("MemTipUsing");
        }

        PerformanceInfos.PerformanceInfoSpeicalItem item_ramuseage = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_ramcanuse = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_sended = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_cached = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_pagepool = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_nopagepool = null;


        public void PageFroceSetData(int s)
        {
            performanceGridGlobal.AddData(s);
        }

        public event SwithGraphicViewEventHandler SwithGraphicView;
        public event OpeningPageMenuEventHandler OpeningPageMenu;

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = performanceTitle.Title + "\n    " + performanceTitle.SmallTitle;
            s += performanceInfos.GetCopyString();
            Clipboard.SetText(s);
        }
        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            图形摘要视图ToolStripMenuItem.Checked = PageIsGraphicMode;
            OpeningPageMenu?.Invoke(this, 查看ToolStripMenuItem);
        }
        private void 图形摘要视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwithGraphicView?.Invoke(this);
        }

        private void PerformancePageRam_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip.Show(MousePosition);
        }
        private void PerformancePageRam_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                SwithGraphicView?.Invoke(this);
        }
        private void PerformancePageRam_MouseDown(object sender, MouseEventArgs e)
        {
            if (PageIsGraphicMode)
                if (e.Button == MouseButtons.Left && e.Clicks == 1)
                    NativeMethods.MAppWorkCall3(165, IntPtr.Zero, IntPtr.Zero);
        }


    }
}
