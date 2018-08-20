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

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MPERF_GetRamUseAge2();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_UpdatePerformance();

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
        private static extern ulong MPERF_GetCommitPeak();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamAvail();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamAvailPageFile();
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetAllRam();
        

        private ulong all_ram = 0;

        public double PageUpdateSimple()
        {
            return 0;
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
                ulong pagesize = MPERF_GetPageSize();
                ulong av = MPERF_GetRamAvail();
                ulong used = all_ram - av;
                ulong avpaged = MPERF_GetRamAvailPageFile();

                performanceRamPoolGrid.VauleUsing = (double)(used / 1048576) / (double)(all_ram / 1048576);
                performanceRamPoolGrid.VauleCompressed = (double)(avpaged / 1048576) / (double)(all_ram / 1048576);
                performanceRamPoolGrid.Invalidate();

                item_ramuseage.Value = NativeMethods.FormatFileSize(used) + " (" + NativeMethods.FormatFileSize(avpaged) + ")";
                item_ramcanuse.Value = NativeMethods.FormatFileSize(av);

                item_sended.Value = NativeMethods.FormatFileSize(pagesize * MPERF_GetCommitTotal()) + "/" + NativeMethods.FormatFileSize(pagesize * MPERF_GetCommitPeak());
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


        PerformanceInfos.PerformanceInfoSpeicalItem item_ramuseage = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_ramcanuse = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_sended = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_cached = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_pagepool = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_nopagepool = null;

        private void PerformanceRam_Load(object sender, EventArgs e)
        {
            item_ramuseage = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_ramcanuse = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_sended = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_cached = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_pagepool = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_nopagepool = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_ramuseage.Name = LanuageMgr.GetStr("MemUsing");
            item_ramcanuse.Name = LanuageMgr.GetStr("MenCanUse");
            item_sended.Name = LanuageMgr.GetStr("Submited");
            item_cached.Name = LanuageMgr.GetStr("Cached");
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


        }

        public void PageFroceSetData(int s)
        {
            performanceGridGlobal.AddData(s);
        }
    }
}
