namespace PCMgr.Ctls
{
    partial class PerformancePageDisk
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformancePageDisk));
            this.performanceGridDiskTime = new PCMgr.Ctls.PerformanceGrid();
            this.performanceTitle1 = new PCMgr.Ctls.PerformanceTitle();
            this.performanceGridSpeed = new PCMgr.Ctls.PerformanceGrid();
            this.performanceInfos = new PCMgr.Ctls.PerformanceInfos();
            this.SuspendLayout();
            // 
            // performanceGridDiskTime
            // 
            resources.ApplyResources(this.performanceGridDiskTime, "performanceGridDiskTime");
            this.performanceGridDiskTime.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(223)))));
            this.performanceGridDiskTime.BottomTextHeight = 20;
            this.performanceGridDiskTime.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            this.performanceGridDiskTime.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridDiskTime.DrawData2 = false;
            this.performanceGridDiskTime.DrawData2Bg = false;
            this.performanceGridDiskTime.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            this.performanceGridDiskTime.LeftBottomText = "60秒";
            this.performanceGridDiskTime.LeftText = "活动时间";
            this.performanceGridDiskTime.Name = "performanceGridDiskTime";
            this.performanceGridDiskTime.RightBottomText = "0";
            this.performanceGridDiskTime.RightText = "100%";
            this.performanceGridDiskTime.TopTextHeight = 20;
            // 
            // performanceTitle1
            // 
            resources.ApplyResources(this.performanceTitle1, "performanceTitle1");
            this.performanceTitle1.Name = "performanceTitle1";
            this.performanceTitle1.SmallTitle = "磁盘名称";
            this.performanceTitle1.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle1.Title = "磁盘序号";
            this.performanceTitle1.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            // 
            // performanceGridSpeed
            // 
            resources.ApplyResources(this.performanceGridSpeed, "performanceGridSpeed");
            this.performanceGridSpeed.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(223)))));
            this.performanceGridSpeed.BottomTextHeight = 20;
            this.performanceGridSpeed.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            this.performanceGridSpeed.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(180)))), ((int)(((byte)(90)))));
            this.performanceGridSpeed.DrawData2 = false;
            this.performanceGridSpeed.DrawData2Bg = false;
            this.performanceGridSpeed.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            this.performanceGridSpeed.LeftBottomText = "60s";
            this.performanceGridSpeed.LeftText = "磁盘传输速率";
            this.performanceGridSpeed.Name = "performanceGridSpeed";
            this.performanceGridSpeed.RightBottomText = "0";
            this.performanceGridSpeed.RightText = "100%";
            this.performanceGridSpeed.TopTextHeight = 20;
            // 
            // performanceInfos
            // 
            resources.ApplyResources(this.performanceInfos, "performanceInfos");
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 10.5F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 10.5F);
            this.performanceInfos.ItemMargan = 10;
            this.performanceInfos.LineOffest = 3;
            this.performanceInfos.MaxSpeicalItemsWidth = 500;
            this.performanceInfos.Name = "performanceInfos";
            // 
            // PerformancePageDisk
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceGridSpeed);
            this.Controls.Add(this.performanceTitle1);
            this.Controls.Add(this.performanceGridDiskTime);
            this.Name = "PerformancePageDisk";
            this.Load += new System.EventHandler(this.PerformancePageDisk_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PerformanceGrid performanceGridDiskTime;
        private PerformanceTitle performanceTitle1;
        private PerformanceGrid performanceGridSpeed;
        private PerformanceInfos performanceInfos;
    }
}
