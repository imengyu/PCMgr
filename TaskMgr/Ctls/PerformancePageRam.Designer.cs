namespace TaskMgr.Ctls
{
    partial class PerformancePageRam
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
            this.performanceGridGlobal = new TaskMgr.Ctls.PerformanceGrid();
            this.performanceTitle = new TaskMgr.Ctls.PerformanceTitle();
            this.performanceRamPoolGrid = new TaskMgr.Ctls.PerformanceRamPoolGrid();
            this.SuspendLayout();
            // 
            // performanceInfos
            // 
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            this.performanceInfos.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.Location = new System.Drawing.Point(0, 308);
            this.performanceInfos.MaxSpeicalItemsWidth = 300;
            this.performanceInfos.Name = "performanceInfos";
            this.performanceInfos.Size = new System.Drawing.Size(588, 113);
            this.performanceInfos.TabIndex = 5;
            // 
            // performanceGridGlobal
            // 
            this.performanceGridGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.performanceGridGlobal.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(242)))), ((int)(((byte)(244)))));
            this.performanceGridGlobal.BottomTextHeight = 20;
            this.performanceGridGlobal.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(18)))), ((int)(((byte)(174)))));
            this.performanceGridGlobal.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(222)))), ((int)(((byte)(240)))));
            this.performanceGridGlobal.LeftBottomText = "60 秒";
            this.performanceGridGlobal.LeftText = "内存使用量";
            this.performanceGridGlobal.Location = new System.Drawing.Point(0, 45);
            this.performanceGridGlobal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.performanceGridGlobal.Name = "performanceGridGlobal";
            this.performanceGridGlobal.RightBottomText = "0";
            this.performanceGridGlobal.RightText = "MAXRAMSIZE";
            this.performanceGridGlobal.Size = new System.Drawing.Size(588, 192);
            this.performanceGridGlobal.TabIndex = 3;
            this.performanceGridGlobal.TopTextHeight = 20;
            // 
            // performanceTitle
            // 
            this.performanceTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.performanceTitle.Location = new System.Drawing.Point(0, 0);
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.Size = new System.Drawing.Size(588, 38);
            this.performanceTitle.SmallTitle = "";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.performanceTitle.TabIndex = 4;
            this.performanceTitle.Title = "内存";
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // performanceRamPoolGrid
            // 
            this.performanceRamPoolGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.performanceRamPoolGrid.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(242)))), ((int)(((byte)(244)))));
            this.performanceRamPoolGrid.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(18)))), ((int)(((byte)(174)))));
            this.performanceRamPoolGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(176)))), ((int)(((byte)(215)))));
            this.performanceRamPoolGrid.LeftText = "内存组合";
            this.performanceRamPoolGrid.Location = new System.Drawing.Point(0, 244);
            this.performanceRamPoolGrid.Name = "performanceRamPoolGrid";
            this.performanceRamPoolGrid.RightText = null;
            this.performanceRamPoolGrid.Size = new System.Drawing.Size(588, 46);
            this.performanceRamPoolGrid.TabIndex = 6;
            this.performanceRamPoolGrid.Text = "performanceRamPoolGrid1";
            this.performanceRamPoolGrid.TextColor = System.Drawing.Color.Gray;
            this.performanceRamPoolGrid.TopTextHeight = 20;
            this.performanceRamPoolGrid.VauleCompressed = 0D;
            this.performanceRamPoolGrid.VauleUsing = 0D;
            // 
            // PerformancePageRam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceRamPoolGrid);
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceGridGlobal);
            this.Controls.Add(this.performanceTitle);
            this.Margin = new System.Windows.Forms.Padding(20);
            this.Name = "PerformancePageRam";
            this.Size = new System.Drawing.Size(588, 421);
            this.Load += new System.EventHandler(this.PerformanceRam_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public PerformanceInfos performanceInfos;
        public PerformanceGrid performanceGridGlobal;
        public PerformanceTitle performanceTitle;
        private PerformanceRamPoolGrid performanceRamPoolGrid;
    }
}
