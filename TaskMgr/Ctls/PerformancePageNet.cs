using System;
using System.Windows.Forms;
using PCMgr.Lanuages;
using System.Drawing;
using static PCMgr.NativeMethods;
using System.Text;

namespace PCMgr.Ctls
{
    public partial class PerformancePageNet : UserControl, IPerformancePage
    {
        public PerformancePageNet()
        {
            InitializeComponent();
        }
        public PerformancePageNet(IntPtr netadapter, bool isWifi, string name)
        {
            InitializeComponent();
            currNet = netadapter;
            currNetName = name;
            currNetIsWifi = isWifi;
        }
        private bool currNetIsWifi = false;
        public string v4 = "", v6 = "";
        public UInt64 physicalMaxSpeed = 100000000;
        private string currNetName = "";
        private IntPtr currNet = IntPtr.Zero;
        private int lastMaxSpeed = 100;
        private uint maxSpeedOverflow = 100;
        private uint maxpySpeed = 100;
        private int outLinkQuality = 0;

        private double lastReceive = 0;
        private double lastSent = 0;

        public string GUID { get; set; } = "";

        public Panel GridPanel => panelGrid;
        public bool PageIsGraphicMode { get; set; }
        public bool PageIsActive { get; set; }
        public void PageDelete()
        {
        }
        public void PageFroceSetData(int s)
        {
            
        }
        public void PageHide()
        {
            Hide();
        }
        public void PageSetGridUnit(string s)
        {
            performanceGrid.LeftBottomText = s;
        }
        public void PageShow()
        {
            Show();
        }
        public void PageUpdate()
        {
            NativeMethods.MPERF_GetNetworksPerformanceCountersValues(currNet, ref lastSent, ref lastReceive);

            double sent = lastSent;
            double receive = lastReceive;

            item_readSpeed.Value = NativeMethods.FormatNetSpeed(Convert.ToInt64(sent));
            item_writeSpeed.Value = NativeMethods.FormatNetSpeed(Convert.ToInt64(receive));

            performanceGrid.AddData2((int)(sent / 1024 * 8));
            performanceGrid.AddData((int)(receive / 1024 * 8));

            //刷新速度标尺
            lastMaxSpeed = (int)(performanceGrid.DataAverage * 0.7 + performanceGrid.MaxData * 0.3) / 2;
            //刷新最大单位
            if (lastMaxSpeed > performanceGrid.MaxValue)
            {
                performanceGrid.MaxValue = (int)GetSpeedMaxUnit();
                performanceGrid.RightText = NativeMethods.FormatNetSpeedUnit(performanceGrid.MaxValue * 1024 / 8);
            }
            else if (lastMaxSpeed < performanceGrid.MaxValue)
            {
                int maxValue = (int)GetSpeedMaxUnit();
                if (performanceGrid.MaxValue > maxValue)
                {
                    performanceGrid.MaxValue = maxValue;
                    performanceGrid.RightText = NativeMethods.FormatNetSpeedUnit(maxValue * 1024 / 8);
                }
            }
            //刷新最大速度标尺
            if (performanceGrid.MaxValue != 100 && lastMaxSpeed != performanceGrid.MaxData
                && lastMaxSpeed >= performanceGrid.MaxValue * 0.10
                && lastMaxSpeed <= performanceGrid.MaxValue * 0.95)
            {
                performanceGrid.MaxScaleValue = lastMaxSpeed;
                performanceGrid.MaxScaleText = NativeMethods.FormatNetSpeed(lastMaxSpeed * 1024 / 8);
            }
            else performanceGrid.MaxScaleValue = 0;

            performanceGrid.Invalidate();

            performanceInfos.UpdateSpeicalItems();
        }
        public bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2)
        {        
            if (!PageIsActive)
            {
                NativeMethods.MPERF_GetNetworksPerformanceCountersValues(currNet, ref lastSent, ref lastReceive);

                performanceGrid.AddData2((int)(lastSent / 1024 * 8));
                performanceGrid.AddData((int)(lastReceive / 1024 * 8));
            }

            customString = LanuageMgr.GetStr("Send") + " " + (lastSent / 1024 * 8).ToString("0.0") + " "
                + LanuageMgr.GetStr("Receive") + " " + (lastReceive / 1024 * 8).ToString("0.0") + " Kbps";
            outdata2 = (int)(lastSent / 1024 * 8);
            outdata1 = (int)(lastReceive / 1024 * 8);
            return true;
        }
        public void PageInit()
        {
            maxpySpeed = (uint)(physicalMaxSpeed / 1024);
            maxSpeedOverflow = (uint)(physicalMaxSpeed * 0.76 / 1024);

            performanceTitle.Title = currNetIsWifi ? "Wi-Fi" : LanuageMgr.GetStr("Ethernet");
            performanceTitle.SmallTitle = currNetName;

            item_readSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed.LineSp = true;
            item_readSpeed.Name = LanuageMgr.GetStr("Send");
            item_writeSpeed.Name = LanuageMgr.GetStr("Receive");
            item_readSpeed.DrawFrontLine = true;
            item_readSpeed.FrontLineColor = Main.MainPagePerf.NetDrawColor;
            item_readSpeed.FrontLineIsDotted = true;
            item_writeSpeed.DrawFrontLine = true;
            item_writeSpeed.FrontLineColor = Main.MainPagePerf.NetDrawColor;

            performanceGrid.MaxValue = 100;
            performanceGrid.DrawData2 = true;
            performanceGrid.MaxUnitPen = new Pen(Main.MainPagePerf.NetDrawColor);

            performanceInfos.FontTitle = new Font("Microsoft YaHei UI", 9);
            performanceInfos.SpeicalItems.Add(item_readSpeed);
            performanceInfos.SpeicalItems.Add(item_writeSpeed);

            if (currNetIsWifi)
            {
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("AdapterName"), "WALN"));
                if (MWLAN_CanUse() && GUID != "")
                {
                    int bssPhyType = 0;
                    StringBuilder outSsidName = new StringBuilder(64);
                    if (MWLAN_GetAdapterWLANInformation(GUID, ref outLinkQuality, outSsidName, 64, ref bssPhyType)) {
                        performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("ConnectionType"), GetWlanConType(bssPhyType)));
                        performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem("SSID", outSsidName.ToString()));
                    }
                }
            }
            else
            {
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("AdapterName"), currNetName));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("ConnectType"), performanceTitle.Title));
            }

            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("IPV4"), v4));
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("IPV6"), v6));

            if (currNetIsWifi) performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("LinkQuality"), GetWlanLinkQualityIcon(outLinkQuality)));
        }
        public int PageGetSpeedMaxUnit(int speed)
        {
            if (speed > 100)
            {
                if(speed > maxSpeedOverflow)
                    return (int)maxpySpeed;//physicalMaxSpeed
                if (speed < 600)
                    return 500;//500k
                if (speed < 1030)
                    return 1024;//1m
                if (speed < 2050)
                    return 2048;//2m
                if (speed < 5130)
                    return 5120;//5m
                if (speed < 10300)
                    return 10240;//10m
                if (speed < 20500)
                    return 20480;//20m
                if (speed < 51000)
                    return 51200;//50m
                if (speed < 103400)
                    return 102400;//100m
                if (speed < 520000)
                    return 512000;//500m
                else return 1048576;//1g
            }
            return 100;
        }
        public void PageShowRightMenu()
        {
            contextMenuStrip.Show(this, Point.Empty);
        }

        private string GetWlanConType(int c)
        {
            switch (c)
            {
                case 1:
                    return "FHSS";
                case 2:
                case 5:
                    return "802.11b";
                case 4:
                    return "802.11a";
                case 6:
                    return "802.11g";
                case 7:
                    return "802.11n";
                case 8:
                    return "802.11ac";
                case 9:
                    return "802.11ad";
                case 0:
                default:
                case 3:
                    return "";
            }
        }
        private Icon GetWlanLinkQualityIcon(int quality)
        {
            if (quality < 5) return Properties.Resources.icoWifiLevel0;
            else if (quality < 20) return Properties.Resources.icoWifiLevel1;
            else if (quality < 40) return Properties.Resources.icoWifiLevel2;
            else if (quality < 60) return Properties.Resources.icoWifiLevel3;
            else if (quality < 80) return Properties.Resources.icoWifiLevel4;
            else if (quality >= 80) return Properties.Resources.icoWifiLevel5;
            else return Properties.Resources.icoWifiLevel0;
        }
        private int GetSpeedMaxUnit()
        {
            return PageGetSpeedMaxUnit(lastMaxSpeed);
        }

        private PerformanceInfos.PerformanceInfoSpeicalItem item_readSpeed = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_writeSpeed = null;

        private void PerformancePageNet_Load(object sender, EventArgs e)
        {
            contextMenuStrip.Renderer = new Helpers.ClassicalMenuRender(Handle);
        }

        public event OpeningPageMenuEventHandler OpeningPageMenu;
        public event SwithGraphicViewEventHandler SwithGraphicView;
        public event EventHandler AppKeyDown;
        
        private void PerformancePageNet_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip.Show(MousePosition);
        }
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = performanceTitle.Title + "\n    " + performanceTitle.SmallTitle;
            s += performanceInfos.GetCopyString();
            Clipboard.SetText(s);
        }
        private void 图形摘要视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwithGraphicView?.Invoke(this);
        }

        private void PerformancePageNet_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                SwithGraphicView?.Invoke(this);
        }
        private void PerformancePageNet_MouseDown(object sender, MouseEventArgs e)
        {
            if (PageIsGraphicMode)
                if (e.Button == MouseButtons.Left && e.Clicks == 1)
                    NativeMethods.MAppWorkCall3(165, IntPtr.Zero, IntPtr.Zero);
        }
        private void PerformancePageNet_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) AppKeyDown?.Invoke(this, e);
        }

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            图形摘要视图ToolStripMenuItem.Checked = PageIsGraphicMode;
            OpeningPageMenu?.Invoke(this, 查看ToolStripMenuItem);

        }
    }
}
