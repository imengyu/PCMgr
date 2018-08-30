namespace PCMgr.Ctls
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformancePageRam));
            this.performanceRamPoolGrid = new PCMgr.Ctls.PerformanceRamPoolGrid();
            this.performanceInfos = new PCMgr.Ctls.PerformanceInfos();
            this.performanceGridGlobal = new PCMgr.Ctls.PerformanceGrid();
            this.performanceTitle = new PCMgr.Ctls.PerformanceTitle();
            this.SuspendLayout();
            // 
            // performanceRamPoolGrid
            // 
            resources.ApplyResources(this.performanceRamPoolGrid, "performanceRamPoolGrid");
            this.performanceRamPoolGrid.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(237)))), ((int)(((byte)(254)))));
            this.performanceRamPoolGrid.CausesValidation = false;
            this.performanceRamPoolGrid.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(18)))), ((int)(((byte)(174)))));
            this.performanceRamPoolGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(176)))), ((int)(((byte)(215)))));
            this.performanceRamPoolGrid.Name = "performanceRamPoolGrid";
            this.performanceRamPoolGrid.StrVauleFree = null;
            this.performanceRamPoolGrid.StrVauleModified = null;
            this.performanceRamPoolGrid.StrVauleStandby = null;
            this.performanceRamPoolGrid.StrVauleUsing = null;
            this.performanceRamPoolGrid.TextColor = System.Drawing.Color.Gray;
            this.performanceRamPoolGrid.TipVauleFree = null;
            this.performanceRamPoolGrid.TipVauleModified = null;
            this.performanceRamPoolGrid.TipVauleStandby = null;
            this.performanceRamPoolGrid.TipVauleUsing = null;
            this.performanceRamPoolGrid.TopTextHeight = 20;
            this.performanceRamPoolGrid.VauleFree = 0D;
            this.performanceRamPoolGrid.VauleModified = 0D;
            this.performanceRamPoolGrid.VauleStandby = 0D;
            this.performanceRamPoolGrid.VauleUsing = 0D;
            // 
            // performanceInfos
            // 
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            resources.ApplyResources(this.performanceInfos, "performanceInfos");
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.ItemMargan = 0;
            this.performanceInfos.LineOffest = 0;
            this.performanceInfos.MaxSpeicalItemsWidth = 300;
            this.performanceInfos.MaxSpeicalItemsWidthLimit = 500;
            this.performanceInfos.Name = "performanceInfos";
            // 
            // performanceGridGlobal
            // 
            resources.ApplyResources(this.performanceGridGlobal, "performanceGridGlobal");
            this.performanceGridGlobal.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(220)))), ((int)(((byte)(98)))), ((int)(((byte)(244)))));
            this.performanceGridGlobal.BottomTextHeight = 20;
            this.performanceGridGlobal.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(18)))), ((int)(((byte)(174)))));
            this.performanceGridGlobal.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridGlobal.DrawData2 = false;
            this.performanceGridGlobal.DrawData2Bg = false;
            this.performanceGridGlobal.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(222)))), ((int)(((byte)(240)))));
            this.performanceGridGlobal.MaxScaleText = "";
            this.performanceGridGlobal.MaxScaleValue = 0;
            this.performanceGridGlobal.MaxValue = 100;
            this.performanceGridGlobal.Name = "performanceGridGlobal";
            this.performanceGridGlobal.TopTextHeight = 20;
            // 
            // performanceTitle
            // 
            resources.ApplyResources(this.performanceTitle, "performanceTitle");
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // PerformancePageRam
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceRamPoolGrid);
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.performanceGridGlobal);
            this.Controls.Add(this.performanceTitle);
            this.Name = "PerformancePageRam";
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
