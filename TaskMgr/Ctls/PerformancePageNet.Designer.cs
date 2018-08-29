namespace PCMgr.Ctls
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformancePageNet));
            this.performanceInfos = new PCMgr.Ctls.PerformanceInfos();
            this.performanceTitle = new PCMgr.Ctls.PerformanceTitle();
            this.performanceGrid = new PCMgr.Ctls.PerformanceGrid();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // performanceInfos
            // 
            resources.ApplyResources(this.performanceInfos, "performanceInfos");
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 10.5F);
            this.performanceInfos.ItemMargan = 10;
            this.performanceInfos.LineOffest = 5;
            this.performanceInfos.MaxSpeicalItemsWidth = 170;
            this.performanceInfos.MaxSpeicalItemsWidthLimit = 250;
            this.performanceInfos.Name = "performanceInfos";
            // 
            // performanceTitle
            // 
            resources.ApplyResources(this.performanceTitle, "performanceTitle");
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            // 
            // performanceGrid
            // 
            resources.ApplyResources(this.performanceGrid, "performanceGrid");
            this.performanceGrid.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(255)))), ((int)(((byte)(157)))), ((int)(((byte)(89)))));
            this.performanceGrid.BottomTextHeight = 20;
            this.performanceGrid.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(79)))), ((int)(((byte)(1)))));
            this.performanceGrid.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(79)))), ((int)(((byte)(1)))));
            this.performanceGrid.DrawData2 = true;
            this.performanceGrid.DrawData2Bg = false;
            this.performanceGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(222)))), ((int)(((byte)(207)))));
            this.performanceGrid.MaxScaleText = "";
            this.performanceGrid.MaxScaleValue = 0;
            this.performanceGrid.MaxValue = 100;
            this.performanceGrid.Name = "performanceGrid";
            this.toolTip1.SetToolTip(this.performanceGrid, resources.GetString("performanceGrid.ToolTip"));
            this.performanceGrid.TopTextHeight = 20;
            // 
            // PerformancePageNet
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceTitle);
            this.Controls.Add(this.performanceGrid);
            this.Name = "PerformancePageNet";
            this.Load += new System.EventHandler(this.PerformancePageNet_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PerformanceGrid performanceGrid;
        private PerformanceTitle performanceTitle;
        private PerformanceInfos performanceInfos;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
