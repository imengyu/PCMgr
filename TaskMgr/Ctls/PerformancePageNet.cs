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

namespace TaskMgr.Ctls
{
    public partial class PerformancePageNet : UserControl, IPerformancePage
    {
        public PerformancePageNet()
        {
            InitializeComponent();
        }
        public PerformancePageNet(string netadapter)
        {
            InitializeComponent(); currNet = netadapter;
        }

        private string currNet = "";

        public void PageDelete()
        {
            if (performanceCounter_sent != null) performanceCounter_sent.Close();
            if (performanceCounter_receive != null) performanceCounter_receive.Close();
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
            float sent = performanceCounter_sent.NextValue();
            float receive = performanceCounter_receive.NextValue();
            item_readSpeed.Value= FormMain.FormatFileSize(Convert.ToInt64(sent)) + "/秒";
            item_writeSpeed.Value = FormMain.FormatFileSize(Convert.ToInt64(receive)) + "/秒";

            performanceGrid.AddData((int)(sent * 0.0001));
            performanceGrid.AddData2((int)(receive * 0.0001));
            performanceGrid.Invalidate();

            performanceInfos.UpdateSpeicalItems();
        }

        private PerformanceInfos.PerformanceInfoSpeicalItem item_readSpeed = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_writeSpeed = null;
        private PerformanceCounter performanceCounter_sent;
        private PerformanceCounter performanceCounter_receive;

        private void PerformancePageNet_Load(object sender, EventArgs e)
        {
            performanceTitle.SmallTitle = currNet;

            item_readSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed.LineSp = true;
            item_readSpeed.Name = "发送";
            item_writeSpeed.Name = "接收";
            item_readSpeed.DrawFrontLine = true;
            item_readSpeed.FrontLineColor = FormMain.NetDrawColor;
            item_writeSpeed.DrawFrontLine = true;
            item_writeSpeed.FrontLineColor = FormMain.NetDrawColor;
            item_writeSpeed.FrontLineIsDotted = true;

            performanceCounter_sent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", currNet, true);
            performanceCounter_receive = new PerformanceCounter("Network Interface", "Bytes Received/sec", currNet, true);

            performanceInfos.SpeicalItems.Add(item_readSpeed);
            performanceInfos.SpeicalItems.Add(item_writeSpeed);
        }
    }
}
