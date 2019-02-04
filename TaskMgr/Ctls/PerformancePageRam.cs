using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PCMgr.Lanuages;
using static PCMgr.NativeMethods;

namespace PCMgr.Ctls
{
    public partial class PerformancePageRam : UserControl, IPerformancePage
    {
        public PerformancePageRam()
        {
            InitializeComponent();
        }

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
            ulong all = MSystemMemoryPerformanctMonitor.GetAllMemory();
            ulong used = MSystemMemoryPerformanctMonitor.GetMemoryUsed();

            ulong divor = 0;
            string unit = GetBestFilesizeUnit(all, out divor);

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

            if (MSystemPerformanctMonitor.UpdatePerformance())
            {
                MSystemMemoryPerformanctMonitor.UpdateMemoryListInfo();

                ulong pagesize = MSystemPerformanctMonitor.GetPageSize();
                ulong availableSize = MSystemMemoryPerformanctMonitor.GetMemoryAvail();
                ulong usedSize = all_ram - availableSize;
                ulong compressedSize = 0;
                ulong compressedEstimateSize = 0;
                ulong compressedSavedSize = 0;
                ulong modifedSize= MSystemMemoryPerformanctMonitor.GetModifiedSize();
                ulong standbySize = MSystemMemoryPerformanctMonitor.GetStandBySize();
                ulong freeSize = availableSize - modifedSize - standbySize;
                ulong divier = all_ram / 1048576;

                if(!compressInfoFailed)
                {
                    MSystemMemoryPerformanctMonitor.SYSTEM_COMPRESSION_INFO compressionInfo = new MSystemMemoryPerformanctMonitor.SYSTEM_COMPRESSION_INFO();
                    if (MSystemMemoryPerformanctMonitor.GetMemoryCompressionInfo(ref compressionInfo))
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
                performanceRamPoolGrid.StrVauleUsing = FormatFileSize(usedSize);
                performanceRamPoolGrid.StrVauleModified = FormatFileSize(modifedSize);
                performanceRamPoolGrid.StrVauleStandby = FormatFileSize(standbySize);
                performanceRamPoolGrid.StrVauleFree = FormatFileSize(freeSize);
                performanceRamPoolGrid.TipVauleFree = string.Format(fTipVauleFree, performanceRamPoolGrid.StrVauleFree);
                performanceRamPoolGrid.TipVauleModified = string.Format(fTipVauleModified, performanceRamPoolGrid.StrVauleModified);
                performanceRamPoolGrid.TipVauleStandby = string.Format(fTipVauleStandby, performanceRamPoolGrid.StrVauleStandby);

                if (compressInfoFailed) performanceRamPoolGrid.TipVauleUsing = string.Format(fTipVauleUsing, performanceRamPoolGrid.StrVauleUsing);
                else performanceRamPoolGrid.TipVauleUsing = string.Format(fTipVauleUsing, performanceRamPoolGrid.StrVauleUsing, FormatFileSize(compressedSize), FormatFileSize(compressedEstimateSize), NativeMethods.FormatFileSize(compressedSavedSize));

                performanceRamPoolGrid.Invalidate();

                if (compressInfoFailed) item_ramuseage.Value = FormatFileSize(usedSize);
                else item_ramuseage.Value = FormatFileSize(usedSize) + " (" + FormatFileSize(compressedSize) + ")";

                item_ramcanuse.Value = FormatFileSize(availableSize);

                item_sended.Value = FormatFileSize(pagesize * MSystemMemoryPerformanctMonitor.GetCommitTotal()) + "/" + FormatFileSize(pagesize * MSystemMemoryPerformanctMonitor.GetCommitLimit());
                item_cached.Value = FormatFileSize(pagesize * MSystemMemoryPerformanctMonitor.GetSystemCacheSize());
                item_nopagepool.Value = FormatFileSize(pagesize * MSystemMemoryPerformanctMonitor.GetKernelNonpaged());
                item_pagepool.Value = FormatFileSize(pagesize * MSystemMemoryPerformanctMonitor.GetKernelPaged());
                performanceInfos.Invalidate();
            }
        }
        public void PageSetGridUnit(string s)
        {
            performanceGridGlobal.LeftBottomText = s;
        }
        public void PageInit()
        {
            contextMenuStrip.Renderer = new Helpers.ClassicalMenuRender(Handle);

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
            all_ram = MSystemMemoryPerformanctMonitor.GetAllMemory(); 
            performanceGridGlobal.RightText = FormatFileSize(all_ram);

            DeviceApi.MDEVICE_GetMemoryDeviceInfo();
            performanceTitle.SmallTitle = Marshal.PtrToStringUni(DeviceApi.MDEVICE_GetMemoryDeviceName());
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("Speed"), DeviceApi.MDEVICE_GetMemoryDeviceSpeed().ToString() + " MHz"));
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("FormFactor"),
              DeviceApi.MDEVICE_MemoryFormFactorToString(DeviceApi.MDEVICE_GetMemoryDeviceFormFactor())
                ));
            UInt16 used = 0, all = 0;
            if (DeviceApi.MDEVICE_GetMemoryDeviceUsed(ref all, ref used))
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("DeviceLocator"), used + "/" + all));

            fTipVauleFree = LanuageMgr.GetStr("MemTipFree");
            fTipVauleModified = LanuageMgr.GetStr("MemTipModifed");
            fTipVauleStandby = LanuageMgr.GetStr("MemTipStandby");
            fTipVauleUsing = LanuageMgr.GetStr("MemTipUsing");
        }
        public void PageShowRightMenu()
        {
            contextMenuStrip.Show(this, System.Drawing.Point.Empty);
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
        public event EventHandler AppKeyDown;

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
        private void PerformancePageRam_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) AppKeyDown?.Invoke(this, e);
        }
    }
}
