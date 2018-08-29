using System;
using System.Windows.Forms;
using PCMgr.Lanuages;
using System.Text;
using System.Drawing;

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
        private string currNetName = "";
        private IntPtr currNet = IntPtr.Zero;
        private int lastMaxSpeed = 100;

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
            double sent = 0;
            double receive = 0;

            NativeMethods.MPERF_GetNetworksPerformanceCountersValues(currNet, ref sent, ref receive);

            item_readSpeed.Value = NativeMethods.FormatNetSpeed(Convert.ToInt64(sent));
            item_writeSpeed.Value = NativeMethods.FormatNetSpeed(Convert.ToInt64(receive));

            performanceGrid.AddData2((int)(sent / 1024 * 8));
            performanceGrid.AddData((int)(receive / 1024 * 8));

            //刷新速度标尺
            lastMaxSpeed = (int)(performanceGrid.DataAverage * 0.7 + performanceGrid.MaxData * 0.3) / 2;
            //刷新最大单位
            if (lastMaxSpeed > performanceGrid.MaxValue)
            {
                performanceGrid.MaxValue = GetSpeedMaxUnit();
                performanceGrid.RightText = NativeMethods.FormatNetSpeedUnit(performanceGrid.MaxValue * 1024 / 8);
            }
            else if (lastMaxSpeed < performanceGrid.MaxValue)
            {
                int maxValue = GetSpeedMaxUnit();
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
                performanceGrid.MaxScaleText = NativeMethods.FormatNetSpeedUnit(lastMaxSpeed * 1024 / 8);
            }
            else performanceGrid.MaxScaleValue = 0;

            performanceGrid.Invalidate();

            performanceInfos.UpdateSpeicalItems();
        }
        public bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2)
        {
            double sent = 0;
            double receive = 0;

            NativeMethods.MPERF_GetNetworksPerformanceCountersValues(currNet, ref sent, ref receive);

            int data1 = (int)(sent * 0.0001);
            int data2 = (int)(receive * 0.0001);

            if (!PageIsActive)
            {
                performanceGrid.AddData2(data1);
                performanceGrid.AddData(data2);
            }

            customString = FormMain.str_Sent + " : " + (sent / 1024 * 8).ToString("0.0") + " " 
                + FormMain.str_Receive + " : " + (receive / 1024 * 8).ToString("0.0") + " Kbps";
            outdata1 = data1;
            outdata2 = data2;
            return true;
        }

        private int GetSpeedMaxUnit()
        {
            if (lastMaxSpeed > 100)
            {
                if (lastMaxSpeed < 600)
                    return 500;//500k
                if (lastMaxSpeed < 1030)
                    return 1024;//1m
                if (lastMaxSpeed < 2050)
                    return 2048;//2m
                if (lastMaxSpeed < 5130)
                    return 5120;//5m
                if (lastMaxSpeed < 10300)
                    return 10240;//10m
                if (lastMaxSpeed < 103000)
                    return 102400;//100m
                if (lastMaxSpeed < 513000)
                    return 512000;//500m
                else return 1048576;//1g
            }
            return 100;
        }

        private PerformanceInfos.PerformanceInfoSpeicalItem item_readSpeed = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_writeSpeed = null;

        private void PerformancePageNet_Load(object sender, EventArgs e)
        {
            performanceTitle.Title = currNetIsWifi ? "Wi-Fi" : LanuageMgr.GetStr("Ethernet");
            performanceTitle.SmallTitle = currNetName;

            item_readSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed.LineSp = true;
            item_readSpeed.Name = FormMain.str_Sent;
            item_writeSpeed.Name = FormMain.str_Receive;
            item_readSpeed.DrawFrontLine = true;
            item_readSpeed.FrontLineColor = FormMain.NetDrawColor;
            item_readSpeed.FrontLineIsDotted = true;
            item_writeSpeed.DrawFrontLine = true;
            item_writeSpeed.FrontLineColor = FormMain.NetDrawColor;

            performanceGrid.MaxValue = 100;
            performanceGrid.DrawData2 = true;
            performanceGrid.MaxUnitPen = new Pen(FormMain.NetDrawColor);

            performanceInfos.FontTitle = new Font("微软雅黑", 9);
            performanceInfos.SpeicalItems.Add(item_readSpeed);
            performanceInfos.SpeicalItems.Add(item_writeSpeed);

            if (currNetIsWifi)
            {
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("AdapterName"), "WALN"));

            }
            else
            {
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("AdapterName"), currNetName));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("ConnectType"), performanceTitle.Title));
            }

            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("IPV4"), v4));
            performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("IPV6"), v6));
        }


    }
}
