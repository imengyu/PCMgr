using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PCMgr.Lanuages;
using static PCMgr.NativeMethods.DeviceApi;

namespace PCMgr.Ctls
{
    public partial class PerformancePageDisk : UserControl, IPerformancePage
    {
        public PerformancePageDisk()
        {
            InitializeComponent();
        }
        public PerformancePageDisk(IntPtr diskname, uint index)
        {
            InitializeComponent();
            currDisk = diskname;
            currDiskIndex = index;
        }

        private int lastDiskTime = 0;
        private int lastMaxSpeed = 100;

        public bool PageIsActive { get; set; }
        public void PageDelete()
        {

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
            performanceGridDiskTime.LeftBottomText = s;
            performanceGridSpeed.LeftBottomText = s;
        }
        public void PageShow()
        {
            Show();
        }
        public void PageUpdate()
        {
            performanceGridDiskTime.Invalidate();
            item_diskTime.Value = lastDiskTime + "%";

            double readbytes = 0;
            double writebytes = 0;
            double read = 0;
            double write = 0;
            double avgque = 0;

            //获取数据
            NativeMethods.MPERF_GetDisksPerformanceCountersValues(currDisk,
                ref readbytes, ref writebytes,
                ref read, ref write, ref avgque);

            //下面的条目
            item_readSpeed.Value = NativeMethods.FormatFileSize(Convert.ToInt64(readbytes)) + "/" + FormMain.str_sec;
            item_writeSpeed.Value = NativeMethods.FormatFileSize(Convert.ToInt64(writebytes)) + "/" + FormMain.str_sec;

            item_responseTime.Value = avgque.ToString("0.0") + "%";

            int readKB = (int)(readbytes / 1024);
            int writeKB = (int)(writebytes / 1024);

            //速率图表数据
            performanceGridSpeed.AddData2(readKB);
            performanceGridSpeed.AddData(writeKB);

            //刷新速度标尺
            lastMaxSpeed = (int)(performanceGridSpeed.DataAverage * 0.7 + performanceGridSpeed.MaxData * 0.3) / 2;

            //刷新最大单位
            if (lastMaxSpeed > performanceGridSpeed.MaxValue)
            {
                performanceGridSpeed.MaxValue = GetSpeedMaxUnit();
                performanceGridSpeed.RightText = NativeMethods.FormatFileSizeKBUnit(performanceGridSpeed.MaxValue) + "/" + FormMain.str_sec;
            }
            else if (lastMaxSpeed < performanceGridSpeed.MaxValue)
            {
                int maxValue = GetSpeedMaxUnit();
                if (performanceGridSpeed.MaxValue > maxValue)
                {
                    performanceGridSpeed.MaxValue = maxValue;
                    performanceGridSpeed.RightText = NativeMethods.FormatFileSizeKBUnit(maxValue) + "/" + FormMain.str_sec;
                }
            }
            //刷新最大速度标尺
            if (performanceGridSpeed.MaxValue != 100 && lastMaxSpeed != performanceGridSpeed.MaxData
                && lastMaxSpeed >= performanceGridSpeed.MaxValue * 0.10
                && lastMaxSpeed <= performanceGridSpeed.MaxValue * 0.95)
            {
                performanceGridSpeed.MaxScaleValue = lastMaxSpeed;
                performanceGridSpeed.MaxScaleText = NativeMethods.FormatFileSize(lastMaxSpeed * 1024) + "/" + FormMain.str_sec;
            }
            else performanceGridSpeed.MaxScaleValue = 0;

            //重绘
            performanceGridSpeed.Invalidate();
            performanceInfos.UpdateSpeicalItems();
        }
        public bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2)
        {
            customString = null;
            int all = (int)(NativeMethods.MPERF_GetDisksPerformanceCountersSimpleValues(currDisk));
            if (all > 100) all = 100;
            lastDiskTime = all;
            performanceGridDiskTime.AddData(lastDiskTime);
            outdata1 = all;
            outdata2 = -1;
            return false;
        }

        private PerformanceInfos.PerformanceInfoSpeicalItem item_diskTime = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_responseTime = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_readSpeed = null;
        private PerformanceInfos.PerformanceInfoSpeicalItem item_writeSpeed = null;
        private IntPtr currDisk = IntPtr.Zero;
        private uint currDiskIndex = 0;
        private string currDiskName = "";

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
        private void InitValues()
        {
            item_diskTime = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_responseTime = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_readSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();
            item_writeSpeed = new PerformanceInfos.PerformanceInfoSpeicalItem();

            StringBuilder sb = new StringBuilder(32);
            NativeMethods.MPERF_GetDisksPerformanceCountersInstanceName(currDisk, sb, 32);
            currDiskName = sb.ToString();
            performanceTitle1.Title = LanuageMgr.GetStr("TitleDisk") + currDiskName;
            performanceTitle1.SmallTitle = "";

            item_diskTime.Name = LanuageMgr.GetStr("ActiveTime");
            item_responseTime.Name = LanuageMgr.GetStr("QueueTime");
            item_readSpeed.LineSp = true;
            item_readSpeed.Name = LanuageMgr.GetStr("ReadSpeed");
            item_writeSpeed.Name = LanuageMgr.GetStr("WriteSpeed");
            item_readSpeed.DrawFrontLine = true;
            item_readSpeed.FrontLineIsDotted = true;
            item_readSpeed.FrontLineColor = FormMain.DiskDrawColor;
            item_writeSpeed.DrawFrontLine = true;
            item_writeSpeed.FrontLineColor = FormMain.DiskDrawColor;

            performanceInfos.FontTitle = new Font("微软雅黑", 9);
            performanceInfos.SpeicalItems.Add(item_diskTime);
            performanceInfos.SpeicalItems.Add(item_responseTime);
            performanceInfos.SpeicalItems.Add(item_readSpeed);
            performanceInfos.SpeicalItems.Add(item_writeSpeed);

            performanceGridSpeed.RightText = NativeMethods.FormatFileSizeKBUnit(lastMaxSpeed) + "/" + FormMain.str_sec;
            performanceGridSpeed.MaxValue = 100;
            performanceGridSpeed.DrawData2 = true;
        }
        private void InitStaticValues()
        {
            UInt64 size = 0;
            UInt32 index = 0;
            StringBuilder stringBuilderModel = new StringBuilder(64);
            StringBuilder stringBuilderName = new StringBuilder(64);
            StringBuilder stringBuilderSize = new StringBuilder(64);

            if (MDEVICE_GetLogicalDiskInfoItem((int)currDiskIndex, stringBuilderName, stringBuilderName, ref index, ref size, stringBuilderSize))
            {
                if (stringBuilderSize.Length > 0)
                    size = UInt64.Parse(stringBuilderSize.ToString());
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("Capacity"), NativeMethods.FormatFileSizeKBUnit(Convert.ToInt64(size / 1024))));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("Formatted"), NativeMethods.FormatFileSizeKBUnit(Convert.ToInt64(size / 1024))));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("IsSystemDir"), MDEVICE_GetIsSystemDisk(currDiskName) ? FormMain.str_Yes : FormMain.str_No));
                performanceInfos.StaticItems.Add(new PerformanceInfos.PerformanceInfoStaticItem(LanuageMgr.GetStr("PageFile"), MDEVICE_GetIsPageFileDisk(currDiskName) ? FormMain.str_Yes : FormMain.str_No));

                performanceTitle1.SmallTitle = stringBuilderName.ToString();
            }
        }

        private void PerformancePageDisk_Load(object sender, EventArgs e)
        {
            InitValues();
            InitStaticValues();
        }


    }
}
