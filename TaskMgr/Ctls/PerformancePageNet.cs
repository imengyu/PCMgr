using System;
using System.Windows.Forms;
using System.Diagnostics;
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
        public PerformancePageNet(IntPtr netadapter)
        {
            InitializeComponent(); currNet = netadapter;
        }

        private IntPtr currNet = IntPtr.Zero;

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

            item_readSpeed.Value= NativeMethods.FormatFileSize(Convert.ToInt64(sent)) + "/" + FormMain.str_sec;
            item_writeSpeed.Value = NativeMethods.FormatFileSize(Convert.ToInt64(receive)) + "/" + FormMain.str_sec;

            performanceGrid.AddData2((int)(sent * 0.0001));
            performanceGrid.AddData((int)(receive * 0.0001));
            performanceGrid.Invalidate();

            performanceInfos.UpdateSpeicalItems();
        }
        public double PageUpdateSimple()
        {
            return NativeMethods.MPERF_GetNetworksPerformanceCountersSimpleValues(currNet);
        }

        private PerformanceInfos.PerformanceInfoSpeicalItem item_readSpeed = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_writeSpeed = null;

        private void PerformancePageNet_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(64);
            NativeMethods.MPERF_GetNetworksPerformanceCountersInstanceName(currNet, sb, 64);
            performanceTitle.SmallTitle = sb.ToString();

            item_readSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed.LineSp = true;
            item_readSpeed.Name = LanuageMgr.GetStr("Send");
            item_writeSpeed.Name = LanuageMgr.GetStr("Receive");
            item_readSpeed.DrawFrontLine = true;
            item_readSpeed.FrontLineColor = FormMain.NetDrawColor;
            item_readSpeed.FrontLineIsDotted = true;
            item_writeSpeed.DrawFrontLine = true;
            item_writeSpeed.FrontLineColor = FormMain.NetDrawColor;


            performanceInfos.FontTitle = new Font("微软雅黑", 9);
            performanceInfos.SpeicalItems.Add(item_readSpeed);
            performanceInfos.SpeicalItems.Add(item_writeSpeed);
        }


    }
}
