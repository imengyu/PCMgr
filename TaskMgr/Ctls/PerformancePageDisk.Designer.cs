namespace TaskMgr.Ctls
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
            this.performanceGridDiskTime = new TaskMgr.Ctls.PerformanceGrid();
            this.performanceTitle1 = new TaskMgr.Ctls.PerformanceTitle();
            this.performanceGridSpeed = new TaskMgr.Ctls.PerformanceGrid();
            this.performanceInfos = new TaskMgr.Ctls.PerformanceInfos();
            this.SuspendLayout();
            // 
            // performanceGridDiskTime
            // 
            this.performanceGridDiskTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.performanceGridDiskTime.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(223)))));
            this.performanceGridDiskTime.BottomTextHeight = 20;
            this.performanceGridDiskTime.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            this.performanceGridDiskTime.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridDiskTime.DrawData2 = false;
            this.performanceGridDiskTime.DrawData2Bg = false;
            this.performanceGridDiskTime.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            this.performanceGridDiskTime.LeftBottomText = "60秒";
            this.performanceGridDiskTime.LeftText = "活动时间";
            this.performanceGridDiskTime.Location = new System.Drawing.Point(3, 41);
            this.performanceGridDiskTime.Name = "performanceGridDiskTime";
            this.performanceGridDiskTime.RightBottomText = "0";
            this.performanceGridDiskTime.RightText = "100%";
            this.performanceGridDiskTime.Size = new System.Drawing.Size(585, 136);
            this.performanceGridDiskTime.TabIndex = 0;
            this.performanceGridDiskTime.TopTextHeight = 20;
            // 
            // performanceTitle1
            // 
            this.performanceTitle1.Dock = System.Windows.Forms.DockStyle.Top;
            this.performanceTitle1.Location = new System.Drawing.Point(0, 0);
            this.performanceTitle1.Name = "performanceTitle1";
            this.performanceTitle1.Size = new System.Drawing.Size(588, 35);
            this.performanceTitle1.SmallTitle = "磁盘名称";
            this.performanceTitle1.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle1.TabIndex = 1;
            this.performanceTitle1.Title = "磁盘序号";
            this.performanceTitle1.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            // 
            // performanceGridSpeed
            // 
            this.performanceGridSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.performanceGridSpeed.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(223)))));
            this.performanceGridSpeed.BottomTextHeight = 20;
            this.performanceGridSpeed.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            this.performanceGridSpeed.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(180)))), ((int)(((byte)(90)))));
            this.performanceGridSpeed.DrawData2 = false;
            this.performanceGridSpeed.DrawData2Bg = false;
            this.performanceGridSpeed.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            this.performanceGridSpeed.LeftBottomText = "60秒";
            this.performanceGridSpeed.LeftText = "磁盘传输速率";
            this.performanceGridSpeed.Location = new System.Drawing.Point(3, 183);
            this.performanceGridSpeed.Name = "performanceGridSpeed";
            this.performanceGridSpeed.RightBottomText = "0";
            this.performanceGridSpeed.RightText = "100%";
            this.performanceGridSpeed.Size = new System.Drawing.Size(585, 96);
            this.performanceGridSpeed.TabIndex = 2;
            this.performanceGridSpeed.TopTextHeight = 20;
            // 
            // performanceInfos
            // 
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            this.performanceInfos.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 10.5F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 10.5F);
            this.performanceInfos.ItemMargan = 10;
            this.performanceInfos.LineOffest = 3;
            this.performanceInfos.Location = new System.Drawing.Point(0, 285);
            this.performanceInfos.MaxSpeicalItemsWidth = 500;
            this.performanceInfos.Name = "performanceInfos";
            this.performanceInfos.Size = new System.Drawing.Size(588, 81);
            this.performanceInfos.TabIndex = 3;
            this.performanceInfos.Text = "performanceInfos1";
            // 
            // PerformancePageDisk
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceGridSpeed);
            this.Controls.Add(this.performanceTitle1);
            this.Controls.Add(this.performanceGridDiskTime);
            this.Name = "PerformancePageDisk";
            this.Size = new System.Drawing.Size(588, 366);
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
