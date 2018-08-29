namespace PCMgr.Ctls
{
    partial class PerformancePageCpu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformancePageCpu));
            this.performanceCpus = new System.Windows.Forms.PictureBox();
            this.performanceInfos = new PCMgr.Ctls.PerformanceInfos();
            this.performanceGridGlobal = new PCMgr.Ctls.PerformanceGrid();
            this.performanceTitle = new PCMgr.Ctls.PerformanceTitle();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.performanceCpus)).BeginInit();
            this.SuspendLayout();
            // 
            // performanceCpus
            // 
            resources.ApplyResources(this.performanceCpus, "performanceCpus");
            this.performanceCpus.Name = "performanceCpus";
            this.performanceCpus.TabStop = false;
            this.toolTip1.SetToolTip(this.performanceCpus, resources.GetString("performanceCpus.ToolTip"));
            this.performanceCpus.Paint += new System.Windows.Forms.PaintEventHandler(this.performanceCpus_Paint);
            this.performanceCpus.MouseLeave += new System.EventHandler(this.performanceCpus_MouseLeave);
            this.performanceCpus.MouseMove += new System.Windows.Forms.MouseEventHandler(this.performanceCpus_MouseMove);
            // 
            // performanceInfos
            // 
            resources.ApplyResources(this.performanceInfos, "performanceInfos");
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.ItemMargan = 0;
            this.performanceInfos.LineOffest = 0;
            this.performanceInfos.MaxSpeicalItemsWidth = 250;
            this.performanceInfos.MaxSpeicalItemsWidthLimit = 250;
            this.performanceInfos.Name = "performanceInfos";
            this.toolTip1.SetToolTip(this.performanceInfos, resources.GetString("performanceInfos.ToolTip"));
            // 
            // performanceGridGlobal
            // 
            resources.ApplyResources(this.performanceGridGlobal, "performanceGridGlobal");
            this.performanceGridGlobal.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(85)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
            this.performanceGridGlobal.BottomTextHeight = 20;
            this.performanceGridGlobal.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridGlobal.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridGlobal.DrawData2 = false;
            this.performanceGridGlobal.DrawData2Bg = false;
            this.performanceGridGlobal.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(226)))), ((int)(((byte)(240)))));
            this.performanceGridGlobal.MaxScaleText = "";
            this.performanceGridGlobal.MaxScaleValue = 0;
            this.performanceGridGlobal.MaxValue = 100;
            this.performanceGridGlobal.Name = "performanceGridGlobal";
            this.toolTip1.SetToolTip(this.performanceGridGlobal, resources.GetString("performanceGridGlobal.ToolTip"));
            this.performanceGridGlobal.TopTextHeight = 20;
            // 
            // performanceTitle
            // 
            resources.ApplyResources(this.performanceTitle, "performanceTitle");
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolTip1.SetToolTip(this.performanceTitle, resources.GetString("performanceTitle.ToolTip"));
            // 
            // PerformancePageCpu
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceCpus);
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceGridGlobal);
            this.Controls.Add(this.performanceTitle);
            this.Name = "PerformancePageCpu";
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.PerformanceCpu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.performanceCpus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public PerformanceGrid performanceGridGlobal;
        public PerformanceTitle performanceTitle;
        public PerformanceInfos performanceInfos;
        private System.Windows.Forms.PictureBox performanceCpus;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
