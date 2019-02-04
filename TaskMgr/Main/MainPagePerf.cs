using PCMgr.Aero.TaskDialog;
using PCMgr.Ctls;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.Main.MainUtils;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.Win32;
using static PCMgr.NativeMethods.DeviceApi;

namespace PCMgr.Main
{
    class MainPagePerf : MainPage
    {
        private SplitContainer splitContainerPerfCtls;
        private PerformanceList performanceLeftList;
        private Panel pl_perfGridHost;
        private PictureBox spBottom;
        private TabControl tabControlMain;

        public MainPagePerf(FormMain formMain) : base(formMain, (TabPage)formMain.tabPagePerfCtl)
        {
            splitContainerPerfCtls = formMain.splitContainerPerfCtls;
            performanceLeftList = formMain.performanceLeftList;
            pl_perfGridHost = formMain.pl_perfGridHost;
            spBottom = formMain.spBottom;
            tabControlMain = formMain.tabControlMain;
        }

        protected override void OnLoadControlEvents()
        {
            splitContainerPerfCtls.Panel2.SizeChanged += splitContainerPerfCtls_Panel2_SizeChanged;
            performanceLeftList.SelectedtndexChanged += performanceLeftList_SelectedtndexChanged;
            performanceLeftList.MouseClick += PerformanceLeftList_MouseClick;
            performanceLeftList.KeyUp += PerformanceLeftList_KeyUp;
            FormMain.linkLabelOpenPerfMon.LinkClicked += linkLabelOpenPerfMon_LinkClicked;
            FormMain.隐藏图形ToolStripMenuItem.Click += 隐藏图形ToolStripMenuItem_Click;

            base.OnLoadControlEvents();
        }


        //性能页面代码

        private bool perfTrayInited = false;

        public static Color CpuDrawColor = Color.FromArgb(17, 125, 187);
        public static Color CpuBgColor = Color.FromArgb(241, 246, 250);
        public static Color RamDrawColor = Color.FromArgb(139, 18, 174);
        public static Color RamBgColor = Color.FromArgb(244, 242, 244);
        public static Color DiskDrawColor = Color.FromArgb(77, 166, 12);
        public static Color DiskBgColor = Color.FromArgb(239, 247, 223);
        public static Color NetDrawColor = Color.FromArgb(167, 79, 1);
        public static Color NetBgColor = Color.FromArgb(252, 243, 235);

        public static Color CpuDrawColor2 = Color.FromArgb(100, 17, 125, 187);
        public static Color RamDrawColor2 = Color.FromArgb(100, 139, 18, 174);
        public static Color DiskDrawColor2 = Color.FromArgb(100, 77, 166, 12);
        public static Color NetDrawColor2 = Color.FromArgb(100, 167, 79, 1);

        public static Color CpuBgColor2 = Color.FromArgb(100, 85, 193, 255);
        public static Color RamBgColor2 = Color.FromArgb(180, 220, 98, 244);
        public static Color DiskBgColor2 = Color.FromArgb(100, 239, 247, 223);
        public static Color NetBgColor2 = Color.FromArgb(100, 255, 157, 89);

        PerfItemHeader perfItemHeaderCpu;
        PerfItemHeader perfItemHeaderRam;

        PerformanceListItem perf_cpu = new PerformanceListItem();
        PerformanceListItem perf_ram = new PerformanceListItem();

        private class PerfItemHeader
        {
            public IntPtr performanceCounterNative = IntPtr.Zero;
            public PerformanceListItem item = null;
            public IPerformancePage performancePage = null;

            public bool Inited { get; set; }

            public override string ToString()
            {
                if (item != null)
                    return item.ToString();
                return base.ToString();
            }
        }

        private List<PerfItemHeader> perfItems = new List<PerfItemHeader>();
        private List<IPerformancePage> perfPages = new List<IPerformancePage>();

        private IPerformancePage currSelectedPerformancePage = null;

        private FormSpeedBall.SpeedItem itemCpu;
        private FormSpeedBall.SpeedItem itemRam;
        private FormSpeedBall.SpeedItem itemDisk;
        private FormSpeedBall.SpeedItem itemNet;
        private IntPtr netCounterMain = IntPtr.Zero;

        private Size lastGraphicSize = new Size();
        private Control lastGridParent = null;
        private Size lastGridSize = new Size();
        private bool perfTrayShowed = false;
        private Size lastSize { get => FormMain.LastSize; set { FormMain.LastSize = value; } }

        private void splitContainerPerfCtls_Panel2_SizeChanged(object sender, EventArgs e)
        {
            PerfPagesResize(new Size(splitContainerPerfCtls.Panel2.Width -
             (splitContainerPerfCtls.Panel2.VerticalScroll.Visible ? 40 : 30), splitContainerPerfCtls.Panel2.Height - 30));
        }
        private void performanceLeftList_SelectedtndexChanged(object sender, EventArgs e)
        {
            if (performanceLeftList.Selectedtem == perf_cpu)
                PerfPagesTo(0, perfItemHeaderCpu);
            else if (performanceLeftList.Selectedtem == perf_ram)
                PerfPagesTo(1, perfItemHeaderRam);
            else if (performanceLeftList.Selectedtem.PageIndex != 0)
                PerfPagesTo(performanceLeftList.Selectedtem.PageIndex, (PerfItemHeader)performanceLeftList.Selectedtem.Tag);
            else PerfPagesToNull();
        }
        private void OpeningPageMenuEventHandler(IPerformancePage sender, ToolStripMenuItem menuItemView)
        {
            if (menuItemView.DropDownItems.Count == 0)
            {
                /*ToolStripItem tcpu = menuItemView.DropDownItems.Add("CPU");
                tcpu.Tag = perfItemHeaderCpu.performancePage;
                tcpu.Click += Tcpu_Click;
                ToolStripItem tram = menuItemView.DropDownItems.Add(perfItemHeaderRam.item.Name);
                tram.Tag = perfItemHeaderRam.performancePage;
                tram.Click += Tram_Click;*/
                foreach (PerfItemHeader h in perfItems)
                {
                    ToolStripItem tx = menuItemView.DropDownItems.Add(h.item.Name);
                    tx.Tag = h;
                    tx.Click += Tx_Click;
                }
            }

            foreach (ToolStripMenuItem i in menuItemView.DropDownItems)
            {
                if (i.Tag != null && ((PerfItemHeader)i.Tag).performancePage == sender)
                    i.Checked = true;
                else if (i.Checked) i.Checked = false;
            }
        }
        private void SwithGraphicViewEventHandler(IPerformancePage sender)
        {
            Panel gridPanel = sender.GridPanel;
            if (!sender.PageIsGraphicMode)
            {
                spBottom.Visible = false;
                tabControlMain.Visible = false;
                lastSize = Size;

                lastGridSize = gridPanel.Size;

                lastGridParent = gridPanel.Parent;
                lastGridParent.Controls.Remove(gridPanel);
                pl_perfGridHost.Controls.Add(gridPanel);

                gridPanel.Size = new Size(pl_perfGridHost.Width - 30, pl_perfGridHost.Height - 30);
                gridPanel.Location = new Point(15, 15);

                sender.PageIsGraphicMode = true;

                pl_perfGridHost.BringToFront();

                MAppWorkCall3(167, Handle, IntPtr.Zero);

                Size = lastGraphicSize;
            }
            else
            {
                pl_perfGridHost.SendToBack();

                pl_perfGridHost.Controls.Remove(gridPanel);
                lastGridParent.Controls.Add(gridPanel);

                MAppWorkCall3(173, Handle, IntPtr.Zero);

                gridPanel.Size = lastGridSize;
                gridPanel.Location = Point.Empty;

                sender.PageIsGraphicMode = false;

                spBottom.Visible = true;
                tabControlMain.Visible = true;
                lastGraphicSize = Size;
                Size = lastSize;
            }
        }
        private void AppKeyDown(object sender, EventArgs e)
        {
            ((IPerformancePage)sender).PageShowRightMenu();
        }

        private void Tx_Click(object sender, EventArgs e)
        {
            PerfItemHeader tag = null;
            ToolStripItem item = sender as ToolStripItem;
            if (item.Tag != null)
            {
                tag = (PerfItemHeader)item.Tag;
                PerfPagesTo(tag.performancePage, tag);
            }
        }
        private void Tcpu_Click(object sender, EventArgs e)
        {
            PerfPagesTo(0, perfItemHeaderCpu);
        }
        private void Tram_Click(object sender, EventArgs e)
        {
            PerfPagesTo(1, perfItemHeaderRam);
        }

        private void PerfPagesToNull()
        {
            if (currSelectedPerformancePage != null)
            {
                currSelectedPerformancePage.PageHide();
                currSelectedPerformancePage.PageIsActive = false;
            }
            currSelectedPerformancePage = null;
        }
        private void PerfPagesTo(int index, PerfItemHeader header)
        {
            PerfPagesTo(perfPages[index], header);
        }
        private void PerfPagesTo(IPerformancePage page, PerfItemHeader header)
        {
            bool isGrMode = false;

            if (currSelectedPerformancePage != null)
            {
                if (currSelectedPerformancePage.PageIsGraphicMode)
                {
                    isGrMode = true;

                    pl_perfGridHost.SendToBack();

                    pl_perfGridHost.Controls.Remove(currSelectedPerformancePage.GridPanel);
                    lastGridParent.Controls.Add(currSelectedPerformancePage.GridPanel);

                    currSelectedPerformancePage.GridPanel.Size = lastGridSize;
                    currSelectedPerformancePage.GridPanel.Location = Point.Empty;

                    currSelectedPerformancePage.PageIsGraphicMode = false;
                }

                currSelectedPerformancePage.PageHide();
                currSelectedPerformancePage.PageIsActive = false;
            }

            currSelectedPerformancePage = null;
            currSelectedPerformancePage = page;

            if (!header.Inited)
            {
                currSelectedPerformancePage.PageInit();
                header.Inited = true;
            }

            currSelectedPerformancePage.PageShow();
            currSelectedPerformancePage.PageIsActive = true;

            performanceLeftList.Selectedtem = performanceLeftList.Items[perfPages.IndexOf(currSelectedPerformancePage)];

            if (isGrMode)
            {
                lastGridSize = currSelectedPerformancePage.GridPanel.Size;

                lastGridParent = currSelectedPerformancePage.GridPanel.Parent;
                lastGridParent.Controls.Remove(currSelectedPerformancePage.GridPanel);
                pl_perfGridHost.Controls.Add(currSelectedPerformancePage.GridPanel);

                currSelectedPerformancePage.GridPanel.Size = new Size(pl_perfGridHost.Width - 30, pl_perfGridHost.Height - 30);
                currSelectedPerformancePage.GridPanel.Location = new Point(15, 15);

                currSelectedPerformancePage.PageIsGraphicMode = true;

                pl_perfGridHost.BringToFront();
            }
        }
        private void PerfPagesAddToCtl(Control c, string name)
        {
            splitContainerPerfCtls.Panel2.Controls.Add(c);

            c.Visible = false;
            c.Anchor = AnchorStyles.Left | AnchorStyles.Top;//| AnchorStyles.Right | AnchorStyles.Bottom;
            //c.Size = new Size(splitContainerPerfCtls.Panel2.Width - 30, splitContainerPerfCtls.Panel2.Height - 30);
            c.Location = new Point(15, 15);
            c.Text = "资源信息页 " + name;
            c.Font = tabControlMain.Font;
        }
        private void PerfPagesResize(Size targetSize)
        {
            foreach (PerfItemHeader h in perfItems)
                if (h.performancePage != null)
                    if (!h.performancePage.PageIsGraphicMode)
                        h.performancePage.Size = targetSize;
        }
        private void PerfPagesInit()
        {
            PerformancePageCpu performanceCpu = new PerformancePageCpu();
            performanceCpu.OpeningPageMenu += OpeningPageMenuEventHandler;
            performanceCpu.SwithGraphicView += SwithGraphicViewEventHandler;
            performanceCpu.AppKeyDown += AppKeyDown;
            PerfPagesAddToCtl(performanceCpu, perf_cpu.Name);
            perfPages.Add(performanceCpu);

            perfItemHeaderCpu = new PerfItemHeader();
            perfItemHeaderCpu.item = perf_cpu;
            perfItemHeaderCpu.performancePage = performanceCpu;
            perfItems.Add(perfItemHeaderCpu);

            PerformancePageRam performanceRam = new PerformancePageRam();
            performanceRam.OpeningPageMenu += OpeningPageMenuEventHandler;
            performanceRam.SwithGraphicView += SwithGraphicViewEventHandler;
            performanceRam.AppKeyDown += AppKeyDown;
            PerfPagesAddToCtl(performanceRam, perf_ram.Name);
            perfPages.Add(performanceRam);

            perfItemHeaderRam = new PerfItemHeader();
            perfItemHeaderRam.item = perf_ram;
            perfItemHeaderRam.performancePage = performanceRam;
            perfItems.Add(perfItemHeaderRam);
        }
        public void PerfInit()
        {
            //初始化perf页面
            if (!Inited)
            {
                PerfLoadSettings();

                MDEVICE_Init();

                perf_cpu.Name = "CPU";
                perf_cpu.SmallText = "-- %";
                perf_cpu.BasePen = new Pen(CpuDrawColor, 2);
                perf_cpu.BgBrush = new SolidBrush(CpuBgColor);
                perf_cpu.NoGridImage = Properties.Resources.pointCpu;
                performanceLeftList.Items.Add(perf_cpu);

                perf_ram.Name = LanuageMgr.GetStr("TitleRam");
                perf_ram.SmallText = "-- %";
                perf_ram.BasePen = new Pen(RamDrawColor, 2);
                perf_ram.BgBrush = new SolidBrush(RamBgColor);
                perf_ram.NoGridImage = Properties.Resources.pointRam;
                performanceLeftList.Items.Add(perf_ram);

                PerfPagesInit();

                //磁盘页面
                MDEVICE_GetLogicalDiskInfo();
                uint count = MPERF_InitDisksPerformanceCounters();
                for (int i = 0; i < count; i++)
                {
                    PerfItemHeader perfItemHeader = new PerfItemHeader();
                    perfItemHeader.performanceCounterNative = MPERF_GetDisksPerformanceCounters(i);
                    perfItemHeader.item = new PerformanceListItem();
                    perfItemHeader.item.NoGridImage = Properties.Resources.pointDisk;

                    StringBuilder sb = new StringBuilder(32);
                    MPERF_GetDisksPerformanceCountersInstanceName(perfItemHeader.performanceCounterNative, sb, 32);
                    string index = sb.ToString().Split(' ')[0];
                    uint diskIndex = MDEVICE_GetPhysicalDriveIndexInWMI(index);

                    perfItemHeader.item.Name = LanuageMgr.GetStr("TitleDisk") + sb.ToString();
                    perfItemHeader.item.BasePen = new Pen(DiskDrawColor);
                    perfItemHeader.item.BgBrush = new SolidBrush(DiskBgColor);
                    perfItemHeader.item.Tag = perfItemHeader;
                    perfItems.Add(perfItemHeader);

                    PerformancePageDisk performancedisk = new PerformancePageDisk(perfItemHeader.performanceCounterNative, diskIndex);
                    performancedisk.OpeningPageMenu += OpeningPageMenuEventHandler;
                    performancedisk.SwithGraphicView += SwithGraphicViewEventHandler;
                    performancedisk.AppKeyDown += AppKeyDown;
                    PerfPagesAddToCtl(performancedisk, perfItemHeader.item.Name);
                    perfPages.Add(performancedisk);

                    perfItemHeader.performancePage = performancedisk;

                    perfItemHeader.item.PageIndex = perfPages.Count - 1;
                    performanceLeftList.Items.Add(perfItemHeader.item);
                }

                //网卡页面
                count = MDEVICE_GetNetworkAdaptersInfo();
                for (int i = 0; i < count; i++)
                {
                    StringBuilder sbName = new StringBuilder(128);
                    if (MDEVICE_GetNetworkAdapterInfoItem(i, sbName, 128))
                    {
                        PerfItemHeader perfItemHeader = new PerfItemHeader();
                        perfItemHeader.performanceCounterNative = MPERF_GetNetworksPerformanceCounterWithName(sbName.ToString());
                        perfItemHeader.item = new PerformanceListItem();

                        bool isWifi = MDEVICE_GetNetworkAdapterIsWIFI(sbName.ToString());

                        perfItemHeader.item.Name = isWifi ? "Wi-Fi" : LanuageMgr.GetStr("Ethernet");
                        perfItemHeader.item.BasePen = new Pen(NetDrawColor);
                        perfItemHeader.item.BgBrush = new SolidBrush(NetBgColor);
                        perfItemHeader.item.BasePen2 = new Pen(NetDrawColor2);
                        perfItemHeader.item.BasePen2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        perfItemHeader.item.BgBrush2 = Brushes.White;
                        perfItemHeader.item.Tag = perfItemHeader;
                        perfItemHeader.item.NoGridImage = Properties.Resources.pointNet;
                        perfItemHeader.item.EnableAutoMax = true;
                        perfItemHeader.item.EnableData2 = true;
                        perfItems.Add(perfItemHeader);

                        StringBuilder sbIPV4 = new StringBuilder(32);
                        StringBuilder sbIPV6 = new StringBuilder(64);
                        bool enabled = MDEVICE_GetNetworkAdapterInfoFormName(sbName.ToString(),
                            sbIPV4, 32, sbIPV6, 64);
                        perfItemHeader.item.Gray = !enabled;
                        if (!enabled)
                            perfItemHeader.item.SmallText = LanuageMgr.GetStr("NotConnect");

                        PerformancePageNet performancenet = new PerformancePageNet(perfItemHeader.performanceCounterNative, isWifi, sbName.ToString());
                        performancenet.OpeningPageMenu += OpeningPageMenuEventHandler;
                        performancenet.SwithGraphicView += SwithGraphicViewEventHandler;
                        performancenet.v4 = sbIPV4.ToString();
                        performancenet.v6 = sbIPV6.ToString();
                        performancenet.AppKeyDown += AppKeyDown;

                        PerfPagesAddToCtl(performancenet, perfItemHeader.item.Name);
                        perfPages.Add(performancenet);

                        perfItemHeader.performancePage = performancenet;

                        perfItemHeader.item.AutoMaxCallback = performancenet.PageGetSpeedMaxUnit;
                        perfItemHeader.item.PageIndex = perfPages.Count - 1;
                        performanceLeftList.Items.Add(perfItemHeader.item);
                    }
                }

                performanceLeftList.UpdateAll();
                performanceLeftList.Invalidate();

                PerfPagesTo(0, perfItemHeaderCpu);
                PerfPagesResize(new Size(splitContainerPerfCtls.Panel2.Width - (splitContainerPerfCtls.Panel2.VerticalScroll.Visible ? 40 : 30), splitContainerPerfCtls.Panel2.Height - 30));

                Inited = true;
            }
        }

        private void PerfLoadSettings()
        {
            string sg = GetConfig("OldSizeGraphic", "AppSetting", "640-320");
            if (sg.Contains("-"))
            {
                string[] ss = sg.Split('-');
                try
                {
                    int w = int.Parse(ss[0]); if (w + FormMain.Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - FormMain.Left;
                    int h = int.Parse(ss[1]); if (h + FormMain.Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - FormMain.Top;
                    lastGraphicSize = new Size(w, h);
                }
                catch { }
            }
            performanceLeftList.DrawDataGrid = GetConfigBool("PerfShowGraphic", "AppSetting", true);
            FormMain.隐藏图形ToolStripMenuItem.Checked = !performanceLeftList.DrawDataGrid;
        }
        private void PerfSaveSettings()
        {
            SetConfig("OldSizeGraphic", "AppSetting", lastGraphicSize.Width + "-" + lastGraphicSize.Height);
            SetConfigBool("PerfShowGraphic", "AppSetting", performanceLeftList.DrawDataGrid);
        }
        public void PerfInitTray()
        {
            if (!perfTrayInited)
            {
                if (MPERF_InitNetworksPerformanceCounters2() > 0)
                    netCounterMain = MPERF_GetNetworksPerformanceCounters(0);

                formSpeedBall = new FormSpeedBall(FormMain);
                formSpeedBall.VisibleChanged += FormSpeedBall_VisibleChanged;
                formSpeedBall.Show();
                ShowWindow(formSpeedBall.Handle, 0);

                Font itemHugeFont = new Font(Font.FontFamily, 10.5f);
                Font itemValueFont = new Font(Font.FontFamily, 10.5f);

                itemCpu = new FormSpeedBall.SpeedItem("CPU", CpuBgColor2, CpuDrawColor);
                itemRam = new FormSpeedBall.SpeedItem(LanuageMgr.GetStr("TitleRam"), RamBgColor2, RamDrawColor2);
                itemDisk = new FormSpeedBall.SpeedItem(LanuageMgr.GetStr("TitleDisk"), DiskBgColor2, DiskDrawColor2);
                itemNet = new FormSpeedBall.SpeedItem(LanuageMgr.GetStr("TitleNet"), NetBgColor2, NetDrawColor2);

                itemCpu.TextFont = itemHugeFont;
                itemCpu.ValueFont = itemValueFont;
                itemRam.TextFont = itemHugeFont;
                itemRam.ValueFont = itemValueFont;
                itemDisk.TextFont = itemHugeFont;
                itemDisk.ValueFont = itemValueFont;
                itemNet.TextFont = itemHugeFont;
                itemNet.ValueFont = itemValueFont;

                itemCpu.GridType = FormSpeedBall.SpeedItemGridType.OneGrid;
                itemRam.GridType = FormSpeedBall.SpeedItemGridType.NoGrid;
                itemNet.GridType = FormSpeedBall.SpeedItemGridType.NoValue;
                itemDisk.GridType = FormSpeedBall.SpeedItemGridType.OneGrid;

                formSpeedBall.Items.Add(itemCpu);
                formSpeedBall.Items.Add(itemRam);
                formSpeedBall.Items.Add(itemDisk);
                formSpeedBall.Items.Add(itemNet);

                perfTrayInited = true;
            }
        }

        private void FormSpeedBall_VisibleChanged(object sender, EventArgs e)
        {
            perfTrayShowed = formSpeedBall.Visible;
        }

        public void PerfUpdate()
        {
            foreach (PerfItemHeader h in perfItems)
            {
                if (h.item.Gray) continue;

                string outCustomStr = null;
                int outdata1 = -1;
                int outdata2 = -1;
                if (h.performancePage.PageUpdateSimple(out outCustomStr, out outdata1, out outdata2))
                {
                    if (outCustomStr != null)
                        h.item.SmallText = outCustomStr;
                }
                else
                {
                    if (outCustomStr == null)
                        h.item.SmallText = outdata1.ToString("0.0") + "%";
                }

                if (outdata2 != -1 && outdata2 != -1)
                {
                    h.item.AddData(outdata1);
                    h.item.AddData2(outdata2);
                }
                else if (outdata1 != -1)
                    h.item.AddData(outdata1);
            }

            if (currSelectedPerformancePage != null)
                currSelectedPerformancePage.PageUpdate();
        }
        public void PerfClear()
        {
            foreach (IPerformancePage h in perfPages)
                h.PageDelete();
            perfPages.Clear();

            MPERF_DestroyNetworksPerformanceCounters();
            MPERF_DestroyDisksPerformanceCounters();
            perfItems.Clear();

            MDEVICE_DestroyLogicalDiskInfo();
            MDEVICE_DestroyNetworkAdaptersInfo();
            MDEVICE_UnInit();

            PerfSaveSettings();

            formSpeedBall.Invoke(new Action(formSpeedBall.Close));
        }
        public void PerfUpdateGridUnit()
        {
            string unistr = "";
            if (FormMain.baseProcessRefeshTimer.Enabled)
                unistr = (FormMain.baseProcessRefeshTimer.Interval / 1000 * 60).ToString() + LanuageFBuffers.Str_Second;
            else unistr = LanuageFBuffers.Str_StatusPaused;
            foreach (IPerformancePage p in perfPages)
                p.PageSetGridUnit(unistr);
        }
        private void PerfSetTrayPos()
        {
            Point p = MousePosition;
            p.X -= 15; p.Y -= 15;
            Point t = new Point();
            if (p.Y < 50)
                t.Y = 45;
            else if (p.Y > Screen.PrimaryScreen.Bounds.Height - 50)
                t.Y = Screen.PrimaryScreen.Bounds.Height - formSpeedBall.Height - 45;
            else t.Y = p.Y - formSpeedBall.Height;

            if (p.X < 50)
                t.X = 45;
            else if (p.X > Screen.PrimaryScreen.Bounds.Width - 50)
                t.X = Screen.PrimaryScreen.Bounds.Width - formSpeedBall.Width - 45;
            else t.X = p.X - formSpeedBall.Width;

            if (t.X < 0) t.X = 2;
            if (t.X > Screen.PrimaryScreen.Bounds.Width - formSpeedBall.Width) t.X = Screen.PrimaryScreen.Bounds.Width - formSpeedBall.Width - 2;
            if (t.Y < 0) t.Y = 2;
            if (t.Y > Screen.PrimaryScreen.Bounds.Height - formSpeedBall.Height) t.X = Screen.PrimaryScreen.Bounds.Height - formSpeedBall.Height - 2;

            formSpeedBall.Invoke(new Action(delegate { formSpeedBall.Location = t; }));
        }
        public void PerfShowSpeedBall()
        {
            PerfSetTrayPos();
            formSpeedBall.Invoke(new Action(delegate
           {
               ShowWindow(formSpeedBall.Handle, 5);
           }));

        }
        public void PerfDayUpdate(out double cpu, out double ram, out double disk, out double net, out bool perfsimpleGeted)
        {
            if (perfTrayShowed)
            {
                MPERF_GlobalUpdatePerformanceCounters();

                cpu = MPERF_GetCpuUseAge2();
                ram = MPERF_GetRamUseAge2() * 100;
                disk = MPERF_GetDiskUseage() * 100;
                net = MPERF_GetNetWorkUseage() * 100;

                perfsimpleGeted = true;

                itemCpu.Value = cpu.ToString("0.0") + " %";
                itemCpu.NumValue = cpu / 100;
                itemCpu.AddData1((int)cpu);

                ulong all = MSystemMemoryPerformanctMonitor.GetAllMemory();
                ulong used = MSystemMemoryPerformanctMonitor.GetMemoryUsed();

                ulong divor = 0;
                string unit = GetBestFilesizeUnit(all, out divor);

                itemRam.Value = (used / (double)divor).ToString("0.0") + " " + unit + "/" + (all / (double)divor).ToString("0.0") + " " + unit + "  (" + ram.ToString("0.0") + "%)";
                itemRam.NumValue = ram / 100;

                itemDisk.Value = disk.ToString("0.0") + " %";
                itemDisk.NumValue = disk / 100;
                itemDisk.AddData1((int)disk);

                double netsent = 0, netreceive = 0;
                if (MPERF_GetNetworksPerformanceCountersValues(netCounterMain, ref netsent, ref netreceive))
                    itemNet.Value = LanuageMgr.GetStr("Send") + " " + (netsent / 1024 * 8).ToString("0.0") + " Kbps  "
                        + LanuageMgr.GetStr("Receive") + " " + (netreceive / 1024 * 8).ToString("0.0") + " Kbps";
                else itemNet.Value = net.ToString("0.0") + " %";

                formSpeedBall.Invoke(new Action(delegate
                {
                    formSpeedBall.Invalidate();
                }));
            }
            else
            {
                cpu = 0;
                ram = 0;
                disk = 0;
                net = 0;
                perfsimpleGeted = false;
            }
        }

        private FormSpeedBall formSpeedBall = null;

        private void PerformanceLeftList_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right) FormMain.contextMenuStripPerfListLeft.Show(MousePosition);
        }
        private void linkLabelOpenPerfMon_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MRunExe("perfmon.exe", "/res");
        }
        private void 隐藏图形ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (performanceLeftList.DrawDataGrid)
            {
                performanceLeftList.DrawDataGrid = false;
                FormMain.隐藏图形ToolStripMenuItem.Checked = true;
            }
            else
            {
                performanceLeftList.DrawDataGrid = true;
                FormMain.隐藏图形ToolStripMenuItem.Checked = false;
            }
        }
        private void PerformanceLeftList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) FormMain.contextMenuStripPerfListLeft.Show(MousePosition);
        }
    }
}
