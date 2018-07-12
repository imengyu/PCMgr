using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TaskMgr.Ctls
{
    public partial class PerformancePageRam : UserControl, IPerformancePage
    {
        public PerformancePageRam()
        {
            InitializeComponent();
        }

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern double MPERF_GetRamUseAge2();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MGetAllRam();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_UpdatePerformance();

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetPageSize();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetKernelPaged();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetKernelNonpaged();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetSystemCacheSize();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetCommitTotal();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetCommitPeak();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamAvail();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetRamAvailPageFile();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong MPERF_GetAllRam();
        

        private ulong all_ram = 0;

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

                item_ramuseage.Value = FormMain.FormatFileSize(used) + " (" + FormMain.FormatFileSize(avpaged) + ")";
                item_ramcanuse.Value = FormMain.FormatFileSize(av);

                item_sended.Value = FormMain.FormatFileSize(pagesize * MPERF_GetCommitTotal()) + "/" + FormMain.FormatFileSize(pagesize * MPERF_GetCommitPeak());
                item_cached.Value = FormMain.FormatFileSize(pagesize * MPERF_GetSystemCacheSize());
                item_nopagepool.Value = FormMain.FormatFileSize(pagesize * MPERF_GetKernelNonpaged());
                item_pagepool.Value = FormMain.FormatFileSize(pagesize * MPERF_GetKernelPaged());
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
            item_ramuseage.Name = "使用中 (已缓存)";
            item_ramcanuse.Name = "可用内存";
            item_sended.Name = "已提交";
            item_cached.Name = "已缓存";
            item_pagepool.Name = "分页缓冲池";
            item_nopagepool.Name = "非分页缓冲池";
            performanceInfos.SpeicalItems.Add(item_ramuseage);
            performanceInfos.SpeicalItems.Add(item_ramcanuse);
            performanceInfos.SpeicalItems.Add(item_sended);
            performanceInfos.SpeicalItems.Add(item_cached);
            performanceInfos.SpeicalItems.Add(item_pagepool);
            performanceInfos.SpeicalItems.Add(item_nopagepool);
            all_ram = MPERF_GetAllRam();
            performanceGridGlobal.RightText = FormMain.FormatFileSize(all_ram);
        }

        public void PageFroceSetData(int s)
        {
            performanceGridGlobal.AddData(s);
        }
    }
}
