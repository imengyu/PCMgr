using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TaskMgr.Ctls
{
    public partial class PerformancePageCpu : UserControl, IPerformancePage
    {
        public PerformancePageCpu()
        {
            InitializeComponent();
        }
        public PerformancePageCpu(PerformanceCounter performanceCounterCpu)
        {
            InitializeComponent();
            this.performanceCounterCpu = performanceCounterCpu;
        }

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetCpuL1Cache();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetCpuL2Cache();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetCpuL3Cache();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetCpuPackage();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetCpuNodeCount();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_GetCpuInfos();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MPERF_UpdatePerformance();     
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MPERF_GetCpuName(StringBuilder buf, int size);
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MPERF_GetCpuFrequency();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MPERF_GetProcessNumber();

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetThreadCount();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetHandleCount();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint MPERF_GetProcessCount();
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 MPERF_GetRunTime();

        private PerformanceCounter performanceCounterCpu = null;
        private TimeSpan times;
        private int cpuCount = 0;
        private int cpuUseage = 0;

        public void PageFroceSetData(int s)
        {
            cpuUseage = s;
            performanceGridGlobal.AddData(s);
        }
        public void PageHide()
        {
            Visible = false;
        }
        public void PageShow()
        {
            Visible = true;
        }
        public void PageUpdate()
        {
            int cpuuse = cpuUseage;
            item_cpuuseage.Value = cpuuse.ToString() + "%";
            performanceGridGlobal.Invalidate();
            if (MPERF_UpdatePerformance())
            {
                item_process_count.Value = MPERF_GetProcessCount().ToString();
                item_thread_count.Value = MPERF_GetThreadCount().ToString();
                item_handle_count.Value = MPERF_GetHandleCount().ToString();
                times = TimeSpan.FromMilliseconds(Convert.ToDouble(MPERF_GetRunTime()));
                item_run_time.Value = times.Days + ":" + times.Hours.ToString("00") + ":" + times.Minutes.ToString("00") + ":" + times.Seconds.ToString("00");
                if (cpuCount > 1) performanceCpus.Invalidate();
                performanceInfos.UpdateSpeicalItems();
            }
        }
        public void PageDelete()
        {
            foreach (PerformanceCounter p in performanceCounterCpusList)
            {
                p.Close();
            }
        }
        public void PageSetGridUnit(string s)
        {
            performanceGridGlobal.LeftBottomText = s;
        }

        PerformanceInfos.PerformanceInfoSpeicalItem item_cpuuseage = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_cpuuseage_freq = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_process_count = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_thread_count = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_handle_count = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_run_time = null;

        private List<PerformanceCounter> performanceCounterCpusList = new List<PerformanceCounter>();
        private StringFormat performanceCpusText = null;
        private Font performanceCpusTextFont = null;
        private Brush performanceCpusTextBrush = null;

        private void GetStaticInfos()
        {
            StringBuilder stringBuilder = new StringBuilder(64);
            if (MPERF_GetCpuName(stringBuilder, 64))
                performanceTitle.SmallTitle = stringBuilder.ToString();
            else performanceTitle.SmallTitle = "";

            cpuCount = MPERF_GetProcessNumber();

            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("最大速度：", MPERF_GetCpuFrequency().ToString() + " MHz"));
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("逻辑处理器：", cpuCount.ToString()));

            if (MPERF_GetCpuInfos())
            {
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("内核：", MPERF_GetCpuPackage().ToString()));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("L1 缓存：", FormMain.FormatFileSize1(Convert.ToInt32(MPERF_GetCpuL1Cache()))));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("L2 缓存：", FormMain.FormatFileSize1(Convert.ToInt32(MPERF_GetCpuL2Cache()))));
                if (MPERF_GetCpuL3Cache() != 0) performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("L3 缓存：", FormMain.FormatFileSize1(Convert.ToInt32(MPERF_GetCpuL3Cache()))));
            }
        }
        private void InitRuntimeInfo()
        {
            item_cpuuseage = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_cpuuseage.Name = "利用率";
            item_cpuuseage_freq = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_cpuuseage_freq.Name = "";
            item_cpuuseage_freq.Value = "              ";
            item_process_count = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_process_count.Name = "进程数";
            item_thread_count = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_thread_count.Name = "线程数";
            item_handle_count = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_handle_count.Name = "句柄数";
            item_run_time = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_run_time.Name = "正常运行时间";
            performanceInfos.SpeicalItems.Add(item_cpuuseage);
            performanceInfos.SpeicalItems.Add(item_cpuuseage_freq);
            performanceInfos.SpeicalItems.Add(item_process_count);
            performanceInfos.SpeicalItems.Add(item_thread_count);
            performanceInfos.SpeicalItems.Add(item_handle_count);
            performanceInfos.SpeicalItems.Add(item_run_time);
        }
        private void InitCpusInfo()
        {
            performanceCpusTextFont = new Font("微软雅黑", 9);
            performanceCpusTextBrush = Brushes.Black;
            performanceCpusText = new StringFormat();
            performanceCpusText.Alignment = StringAlignment.Center;
            performanceCpusText.LineAlignment = StringAlignment.Center;
            string[] cpus = new PerformanceCounterCategory("Processor Information").GetInstanceNames();
            foreach(string cpu in cpus)
            {
                if(!cpu.Contains("_Total"))
                    performanceCounterCpusList.Add(new PerformanceCounter("Processor Information", "% Processor Time", cpu, true));
            }
        }

        private void PerformanceCpu_Load(object sender, EventArgs e)
        {
            GetStaticInfos();
            InitRuntimeInfo();
            InitCpusInfo();
        }

        private Color performanceCpus_GetColorFormValue(int i)
        {
            if (i == 0)
                return Color.White;
            else if (i > 0 && i <= 20)
                return Color.FromArgb(241, 246, 250);
            else if (i > 20 && i <= 30)
                return Color.FromArgb(180, 200, 240);
            else if (i > 30 && i <= 60)
                return Color.FromArgb(100, 180, 239);
            else if (i > 60 && i <= 80)
                return Color.FromArgb(80, 164, 236);
            else if (i > 80 && i <= 85)
                return Color.FromArgb(20, 146, 220);
            else if (i > 85 && i <= 90)
                return Color.SandyBrown;
            else if (i > 90 && i <= 95)
                return Color.DarkOrange;
            else if (i > 95 && i < 100)
                return Color.Tomato;
            else if (i >= 100)
                return Color.FromArgb(243, 90, 52);
            return Color.White;
        }
        private void performanceCpus_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (cpuCount > 1)
            {
                int width = performanceCpus.Width / cpuCount;
                int x = 0;
                for (int i = 0; i < performanceCounterCpusList.Count; i++)
                {
                    int useage = (int)performanceCounterCpusList[i].NextValue();

                    using (SolidBrush s = new SolidBrush(performanceCpus_GetColorFormValue(useage)))
                    {
                        Rectangle r = new Rectangle(x, 1, width, performanceCpus.Height - 2);
                        g.FillRectangle(s, r);
                        g.DrawString(useage + "%", performanceCpusTextFont, performanceCpusTextBrush, r, performanceCpusText);
                    }

                    x += width;
                }
            }
            g.DrawRectangle(performanceGridGlobal.DrawPen, 0, 0, performanceCpus.Width-1, performanceCpus.Height-1);
        }

    }
}
