using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using PCMgr.Lanuages;
using static PCMgr.NativeMethods;

namespace PCMgr.Ctls
{
    public partial class PerformancePageCpu : UserControl, IPerformancePage
    {
        public PerformancePageCpu()
        {
            InitializeComponent();

            _showKernelTime = GetConfigBool("CpuShowKernelTime", "AppSetting");
        }

        private TimeSpan times;
        private int cpuCount = 0;
        private int cpuUseage = 0;
        private string maxSpeed = "";
        private bool notDrawGrid = false;
        private bool _showKernelTime = false;
        private bool showKernelTime {
            get { return _showKernelTime; }
            set
            {
                if (value != _showKernelTime)
                {
                    _showKernelTime = value;
                    SetConfigBool("CpuShowKernelTime", "AppSetting", value);
                    performanceGridGlobal.DrawData2 = _showKernelTime;
                }
            }
        }

        public bool PageIsGraphicMode { get; set; }
        public bool PageIsActive { get; set; }

        public Panel GridPanel => panelGrid;
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
            //cpuUseage = (int)(MPERF_GetCpuUseAge2());

            if (!notDrawGrid)
            {
                if (showKernelTime)
                {
                    int cpuKernelTime = (int)(cpuUseage - MPERF_GetCpuUseAgeUser2());
                    performanceGridGlobal.AddData2(cpuKernelTime);
                }
                performanceGridGlobal.AddData(cpuUseage);
                performanceGridGlobal.Invalidate();
            }

            item_cpuuseage.Value = cpuUseage.ToString() + "%";
            if (MSystemPerformanctMonitor.UpdatePerformance())
            {
                item_process_count.Value = MSystemPerformanctMonitor.GetProcessCount().ToString();
                item_thread_count.Value = MSystemPerformanctMonitor.GetThreadCount().ToString();
                item_handle_count.Value = MSystemPerformanctMonitor.GetHandleCount().ToString();
                times = TimeSpan.FromMilliseconds(Convert.ToDouble(MSystemPerformanctMonitor.GetSystemRunTime()));
                item_run_time.Value = times.Days + ":" + times.Hours.ToString("00") + ":" + times.Minutes.ToString("00") + ":" + times.Seconds.ToString("00");
                if (cpuCount > 1)
                {
                    if(notDrawGrid) performanceCpusAll.Invalidate();
                    else performanceCpus.Invalidate();
                }
                performanceInfos.UpdateSpeicalItems();
            }
        }
        public void PageDelete()
        {
            MPERF_DestroyCpuDetalsPerformanceCounters();
        }
        public void PageSetGridUnit(string s)
        {
            performanceGridGlobal.LeftBottomText = s;
        }
        public bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2)
        {
            cpuUseage = (int)(MPERF_GetCpuUseAge2());

            customString = cpuUseage.ToString() + "%  " + maxSpeed;
            outdata1 = cpuUseage;
            outdata2 = -1;

            if (!PageIsActive)
                performanceGridGlobal.AddData(outdata1);

            return true;
        }
        public void PageInit()
        {


            GetStaticInfos();
            InitRuntimeInfo();
            InitCpusInfo();
        }

        PerformanceInfos.PerformanceInfoSpeicalItem item_cpuuseage = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_cpuuseage_freq = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_process_count = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_thread_count = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_handle_count = null;
        PerformanceInfos.PerformanceInfoSpeicalItem item_run_time = null;

        private StringFormat performanceCpusText = null;
        private Font performanceCpusTextFont = null;
        private Brush performanceCpusTextBrush = null;

        private int lineCount = 1;

        private void GetStaticInfos()
        {
            StringBuilder stringBuilder = new StringBuilder(64);
            if (MCpuInfoMonitor.GetCpuName(stringBuilder, 64))
                performanceTitle.SmallTitle = stringBuilder.ToString();
            else performanceTitle.SmallTitle = "";

            maxSpeed = (MCpuInfoMonitor.GetCpuFrequency() / 1024d).ToString("0.0") + " GHz";

            cpuCount = MCpuInfoMonitor.GetProcessorNumber();

            if (cpuCount >= 16)
            {
                if (cpuCount % 10 == 0) lineCount = cpuCount / 10;
                else if (cpuCount % 8 == 0) lineCount = cpuCount / 8;
                else if (cpuCount % 4 == 0) lineCount = cpuCount / 4;
                else if (cpuCount % 2 == 0) lineCount = cpuCount / 2;

                performanceCpus.Hide();
                performanceGridGlobal.Hide();

                performanceCpusAll.Show();

                notDrawGrid = true;
            }

            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("MaxSpeed"), MCpuInfoMonitor.GetCpuFrequency().ToString() + " MHz"));
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("CpuCpunt"), cpuCount.ToString()));

            if (MCpuInfoMonitor.GetCpuInfos())
            {
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("CpuPackageCount"), MCpuInfoMonitor.GetCpuPackage().ToString()));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("L" + 1 + LanuageMgr.GetStr("Cache"), FormatFileSize1(Convert.ToInt32(MCpuInfoMonitor.GetCpuL1Cache()))));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("L" + 2 + LanuageMgr.GetStr("Cache"), FormatFileSize1(Convert.ToInt32(MCpuInfoMonitor.GetCpuL2Cache()))));
                if (MCpuInfoMonitor.GetCpuL3Cache() != 0) performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("L" + 3 + LanuageMgr.GetStr("Cache"), FormatFileSize1(Convert.ToInt32(MCpuInfoMonitor.GetCpuL3Cache()))));
            }
        }
        private void InitRuntimeInfo()
        {
            item_cpuuseage = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_cpuuseage.Name = LanuageMgr.GetStr("Useage");
            item_cpuuseage_freq = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_cpuuseage_freq.Name = "";
            item_cpuuseage_freq.Value = "              ";
            item_process_count = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_process_count.Name = FormMain.str_proc_count;
            item_thread_count = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_thread_count.Name = LanuageMgr.GetStr("ThreadCount");
            item_handle_count = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_handle_count.Name = LanuageMgr.GetStr("HandleCount");
            item_run_time = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_run_time.Name = LanuageMgr.GetStr("RunTime");
            performanceInfos.SpeicalItems.Add(item_cpuuseage);
            performanceInfos.SpeicalItems.Add(item_cpuuseage_freq);
            performanceInfos.SpeicalItems.Add(item_process_count);
            performanceInfos.SpeicalItems.Add(item_thread_count);
            performanceInfos.SpeicalItems.Add(item_handle_count);
            performanceInfos.SpeicalItems.Add(item_run_time);
        }
        private void InitCpusInfo()
        {
            performanceCpusTextFont = new Font("Microsoft YaHei UI", 9);
            performanceCpusTextBrush = Brushes.Black;
            performanceCpusText = new StringFormat();
            performanceCpusText.Alignment = StringAlignment.Center;
            performanceCpusText.LineAlignment = StringAlignment.Center;

            performanceGridGlobal.DrawData2 = _showKernelTime;

            MPERF_InitCpuDetalsPerformanceCounters();
        }

        private void PerformanceCpu_Load(object sender, EventArgs e)
        {
            performanceCpus.Text = "逻辑处理器利用率视图";
            performanceCpusAll.Text = "所有逻辑处理器利用率视图";
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
                int performanceCounterCpusListCount = MPERF_GetCpuDetalsPerformanceCountersCount();
                for (int i = 0; i < performanceCounterCpusListCount; i++)
                {
                    double useage = MPERF_GetCpuDetalsCpuUsage(i);

                    using (SolidBrush s = new SolidBrush(performanceCpus_GetColorFormValue((int)(useage))))
                    {
                        Rectangle r = new Rectangle(x, 1, width, performanceCpus.Height - 2);
                        g.FillRectangle(s, r);
                        g.DrawString(useage.ToString("0.0") + "%", performanceCpusTextFont, performanceCpusTextBrush, r, performanceCpusText);
                    }

                    x += width;
                }
            }
            g.DrawRectangle(performanceGridGlobal.DrawPen, 0, 0, performanceCpus.Width - 1, performanceCpus.Height - 1);
        }
        private void performanceCpusAll_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (cpuCount > 1)
            {
                int height = performanceCpusAll.Height / lineCount;
                int width = performanceCpusAll.Width / (cpuCount / lineCount);

                int x = 0, y = 0;

                int performanceCounterCpusListCount = MPERF_GetCpuDetalsPerformanceCountersCount();
                for (int i = 0; i < performanceCounterCpusListCount; i++)
                {
                    double useage = MPERF_GetCpuDetalsCpuUsage(i);

                    using (SolidBrush s = new SolidBrush(performanceCpus_GetColorFormValue((int)(useage))))
                    {
                        Rectangle r = new Rectangle(x, y + 1, width, height - 2);
                        g.FillRectangle(s, r);
                        g.DrawString(useage.ToString("0.0") + "%", performanceCpusTextFont, performanceCpusTextBrush, r, performanceCpusText);
                    }

                    x += width;

                    if (x + width >= performanceCpusAll.Width)
                    {
                        x = 0;
                        y += height;
                    }
                }
            }
            g.DrawRectangle(performanceGridGlobal.DrawPen, 0, 0, performanceCpusAll.Width - 1, performanceCpusAll.Height - 1);
        }
        private void performanceCpus_MouseMove(object sender, MouseEventArgs e)
        {
            ShowTooltip(e.Location);
        }
        private void performanceCpus_MouseLeave(object sender, EventArgs e)
        {
            lastShowTooltip = -1;
            toolTip1.Hide(this);
        }

        private int lastShowTooltip = -1;
        private void ShowTooltip(Point p)
        {
            int curri = (p.X / (performanceCpus.Width / cpuCount));
            if (curri != lastShowTooltip)
            {
                lastShowTooltip = curri;
                toolTip1.Show("CPU " + curri, performanceCpus, curri * (performanceCpus.Width / cpuCount), performanceCpus.Height + 3, 5000);
            }
        }

        public event SwithGraphicViewEventHandler SwithGraphicView;
        public event OpeningPageMenuEventHandler OpeningPageMenu;

        private void 显示内核时间ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            showKernelTime = 显示内核时间ToolStripMenuItem.Checked;
        }
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = performanceTitle.Title + "\n    " + performanceTitle.SmallTitle;
            s += performanceInfos.GetCopyString();
            Clipboard.SetText(s);
        }
        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (显示内核时间ToolStripMenuItem.Checked != showKernelTime)
                显示内核时间ToolStripMenuItem.Checked = showKernelTime;
            图形摘要视图ToolStripMenuItem.Checked = PageIsGraphicMode;
            OpeningPageMenu?.Invoke(this, 查看ToolStripMenuItem);
        }
        private void 图形摘要视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwithGraphicView?.Invoke(this);
        }

        private void PerformancePageCpu_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip.Show(MousePosition);
        }
        private void PerformancePageCpu_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                SwithGraphicView?.Invoke(this);
        }
        private void PerformancePageCpu_MouseDown(object sender, MouseEventArgs e)
        {
            if (PageIsGraphicMode)
                if (e.Button == MouseButtons.Left && e.Clicks == 1)
                    MAppWorkCall3(165, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
