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
using PCMgr.Lanuages;

namespace PCMgr.Ctls
{
    public partial class PerformancePageDisk : UserControl, IPerformancePage
    {
        public PerformancePageDisk()
        {
            InitializeComponent();
        }
        public PerformancePageDisk(string diskname)
        {
            InitializeComponent();
            currDisk = diskname;
        }

        private int lastDiskTime = 0;

        public void PageDelete()
        {
            if (performanceCounter_read != null) performanceCounter_read.Close();
            if (performanceCounter_write != null) performanceCounter_write.Close();
            if (performanceCounter_readSpeed != null) performanceCounter_readSpeed.Close();
            if (performanceCounter_writeSpeed != null) performanceCounter_writeSpeed.Close();
            if (performanceCounter_avgQue != null) performanceCounter_avgQue.Close();
        }
        public void PageFroceSetData(int s)
        {
            lastDiskTime = s;
            performanceGridDiskTime.AddData(s);
        }
        public void PageHide()
        {
            Hide();
        }
        public void PageSetGridUnit(string s)
        {
            performanceGridDiskTime.RightBottomText = s;
            performanceGridSpeed.RightBottomText = s;
        }
        public void PageShow()
        {
            Show();
        }
        public void PageUpdate()
        {
            performanceGridDiskTime.Invalidate();
            item_diskTime.Value = lastDiskTime + "%";

            float readbytes = performanceCounter_readSpeed.NextValue();
            float writebytes = performanceCounter_writeSpeed.NextValue();

            item_readSpeed.Value = FormMain.FormatFileSize(Convert.ToInt64(readbytes)) + "/" + FormMain.str_sec;
            item_writeSpeed.Value = FormMain.FormatFileSize(Convert.ToInt64(writebytes)) + "/" + FormMain.str_sec;

            item_responseTime.Value = performanceCounter_avgQue.NextValue().ToString("0.0") + "%";

            int read = (int)performanceCounter_read.NextValue(), write = (int)performanceCounter_write.NextValue();

            performanceGridSpeed.AddData(read);
            performanceGridSpeed.AddData2(write);
            performanceGridSpeed.Invalidate();

            performanceInfos.UpdateSpeicalItems();
        }

        private PerformanceCounter performanceCounter_read;
        private PerformanceCounter performanceCounter_write;
        private PerformanceCounter performanceCounter_readSpeed;
        private PerformanceCounter performanceCounter_writeSpeed;
        private PerformanceCounter performanceCounter_avgQue;

        private PerformanceInfos.PerformanceInfoSpeicalItem item_diskTime = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_responseTime = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_readSpeed = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_writeSpeed = null;
        private string currDisk = "";

        private void InitValues()
        {
            item_diskTime = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_responseTime = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_readSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();

            performanceTitle1.Title = LanuageMgr.GetStr("TitleDisk") + currDisk;
            performanceTitle1.SmallTitle = "";

            performanceCounter_read = new PerformanceCounter("PhysicalDisk", "Disk Reads/sec", currDisk, true);
            performanceCounter_write = new PerformanceCounter("PhysicalDisk", "Disk Writes/sec", currDisk, true);
            performanceCounter_readSpeed = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", currDisk, true);
            performanceCounter_writeSpeed = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", currDisk, true);
            performanceCounter_avgQue = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", currDisk, true);

            item_diskTime.Name = LanuageMgr.GetStr("ActiveTime");
            item_responseTime.Name = LanuageMgr.GetStr("QueueTime");
            item_readSpeed.LineSp = true;
            item_readSpeed.Name = LanuageMgr.GetStr("ReadSpeed");
            item_writeSpeed.Name = LanuageMgr.GetStr("WriteSpeed");
            item_readSpeed.DrawFrontLine = true;
            item_readSpeed.FrontLineColor = FormMain.DiskDrawColor;
            item_writeSpeed.DrawFrontLine = true;
            item_writeSpeed.FrontLineColor = FormMain.DiskDrawColor;
            item_writeSpeed.FrontLineIsDotted = true;
            performanceInfos.SpeicalItems.Add(item_diskTime);
            performanceInfos.SpeicalItems.Add(item_responseTime);
            performanceInfos.SpeicalItems.Add(item_readSpeed);
            performanceInfos.SpeicalItems.Add(item_writeSpeed);

            performanceGridSpeed.DrawData2 = true;
        }
        private void InitStaticValues()
        {

        }

        private void PerformancePageDisk_Load(object sender, EventArgs e)
        {
            InitValues();
            InitStaticValues();
        }
    }
}
