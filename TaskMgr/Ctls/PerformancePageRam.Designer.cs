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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformancePageRam));
            this.performanceRamPoolGrid = new PCMgr.Ctls.PerformanceRamPoolGrid();
            this.performanceInfos = new PCMgr.Ctls.PerformanceInfos();
            this.performanceGridGlobal = new PCMgr.Ctls.PerformanceGrid();
            this.performanceTitle = new PCMgr.Ctls.PerformanceTitle();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.图形摘要视图ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.contextMenuStrip.SuspendLayout();
            this.panelGrid.SuspendLayout();
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
            this.performanceRamPoolGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseClick);
            this.performanceRamPoolGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDoubleClick);
            this.performanceRamPoolGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDown);
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
            this.performanceInfos.MaxSpeicalItemsWidth = 300;
            this.performanceInfos.MaxSpeicalItemsWidthLimit = 500;
            this.performanceInfos.Name = "performanceInfos";
            this.performanceInfos.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseClick);
            this.performanceInfos.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDoubleClick);
            // 
            // performanceGridGlobal
            // 
            resources.ApplyResources(this.performanceGridGlobal, "performanceGridGlobal");
            this.performanceGridGlobal.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(220)))), ((int)(((byte)(98)))), ((int)(((byte)(244)))));
            this.performanceGridGlobal.BgColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
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
            this.performanceGridGlobal.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseClick);
            this.performanceGridGlobal.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDoubleClick);
            this.performanceGridGlobal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDown);
            // 
            // performanceTitle
            // 
            resources.ApplyResources(this.performanceTitle, "performanceTitle");
            this.performanceTitle.Cursor = System.Windows.Forms.Cursors.Default;
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            this.performanceTitle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseClick);
            this.performanceTitle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDoubleClick);
            this.performanceTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDown);
            // 
            // contextMenuStrip
            // 
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.图形摘要视图ToolStripMenuItem,
            this.查看ToolStripMenuItem,
            this.复制ToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // 图形摘要视图ToolStripMenuItem
            // 
            resources.ApplyResources(this.图形摘要视图ToolStripMenuItem, "图形摘要视图ToolStripMenuItem");
            this.图形摘要视图ToolStripMenuItem.Name = "图形摘要视图ToolStripMenuItem";
            this.图形摘要视图ToolStripMenuItem.Click += new System.EventHandler(this.图形摘要视图ToolStripMenuItem_Click);
            // 
            // 查看ToolStripMenuItem
            // 
            resources.ApplyResources(this.查看ToolStripMenuItem, "查看ToolStripMenuItem");
            this.查看ToolStripMenuItem.Name = "查看ToolStripMenuItem";
            // 
            // 复制ToolStripMenuItem
            // 
            resources.ApplyResources(this.复制ToolStripMenuItem, "复制ToolStripMenuItem");
            this.复制ToolStripMenuItem.Name = "复制ToolStripMenuItem";
            this.复制ToolStripMenuItem.Click += new System.EventHandler(this.复制ToolStripMenuItem_Click);
            // 
            // panelGrid
            // 
            resources.ApplyResources(this.panelGrid, "panelGrid");
            this.panelGrid.Controls.Add(this.performanceTitle);
            this.panelGrid.Controls.Add(this.performanceRamPoolGrid);
            this.panelGrid.Controls.Add(this.performanceGridGlobal);
            this.panelGrid.Name = "panelGrid";
            // 
            // PerformancePageRam
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.panelGrid);
            this.Name = "PerformancePageRam";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PerformancePageRam_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageRam_MouseDown);
            this.contextMenuStrip.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public PerformanceInfos performanceInfos;
        public PerformanceGrid performanceGridGlobal;
        public PerformanceTitle performanceTitle;
        private PerformanceRamPoolGrid performanceRamPoolGrid;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem 图形摘要视图ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.Panel panelGrid;
    }
}
