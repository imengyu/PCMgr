namespace TaskMgr.Ctls
{
    partial class PerformancePageNet
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
            this.performanceInfos = new TaskMgr.Ctls.PerformanceInfos();
            this.performanceTitle = new TaskMgr.Ctls.PerformanceTitle();
            this.performanceGrid = new TaskMgr.Ctls.PerformanceGrid();
            this.SuspendLayout();
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
            this.performanceInfos.LineOffest = 5;
            this.performanceInfos.Location = new System.Drawing.Point(0, 307);
            this.performanceInfos.MaxSpeicalItemsWidth = 350;
            this.performanceInfos.Name = "performanceInfos";
            this.performanceInfos.Size = new System.Drawing.Size(506, 114);
            this.performanceInfos.TabIndex = 2;
            this.performanceInfos.Text = "performanceInfos1";
            // 
            // performanceTitle
            // 
            this.performanceTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.performanceTitle.Location = new System.Drawing.Point(0, 0);
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.Size = new System.Drawing.Size(506, 38);
            this.performanceTitle.SmallTitle = "网卡名称";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle.TabIndex = 1;
            this.performanceTitle.Title = "网络";
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            // 
            // performanceGrid
            // 
            this.performanceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.performanceGrid.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(243)))), ((int)(((byte)(235)))));
            this.performanceGrid.BottomTextHeight = 20;
            this.performanceGrid.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(79)))), ((int)(((byte)(1)))));
            this.performanceGrid.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(79)))), ((int)(((byte)(1)))));
            this.performanceGrid.DrawData2 = true;
            this.performanceGrid.DrawData2Bg = false;
            this.performanceGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(222)))), ((int)(((byte)(207)))));
            this.performanceGrid.LeftBottomText = "60 秒";
            this.performanceGrid.LeftText = "吞吐量";
            this.performanceGrid.Location = new System.Drawing.Point(3, 44);
            this.performanceGrid.Name = "performanceGrid";
            this.performanceGrid.RightBottomText = "0";
            this.performanceGrid.RightText = "100Kbps";
            this.performanceGrid.Size = new System.Drawing.Size(503, 257);
            this.performanceGrid.TabIndex = 0;
            this.performanceGrid.TopTextHeight = 20;
            // 
            // PerformancePageNet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceTitle);
            this.Controls.Add(this.performanceGrid);
            this.Name = "PerformancePageNet";
            this.Size = new System.Drawing.Size(506, 421);
            this.Load += new System.EventHandler(this.PerformancePageNet_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PerformanceGrid performanceGrid;
        private PerformanceTitle performanceTitle;
        private PerformanceInfos performanceInfos;
    }
}
