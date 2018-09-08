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
            this.performanceCpusAll = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.显示内核时间ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.图形摘要视图ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelGrid = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCpus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCpusAll)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.panelGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // performanceCpus
            // 
            resources.ApplyResources(this.performanceCpus, "performanceCpus");
            this.performanceCpus.Name = "performanceCpus";
            this.performanceCpus.TabStop = false;
            this.performanceCpus.Paint += new System.Windows.Forms.PaintEventHandler(this.performanceCpus_Paint);
            this.performanceCpus.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseClick);
            this.performanceCpus.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDoubleClick);
            this.performanceCpus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDown);
            this.performanceCpus.MouseLeave += new System.EventHandler(this.performanceCpus_MouseLeave);
            this.performanceCpus.MouseMove += new System.Windows.Forms.MouseEventHandler(this.performanceCpus_MouseMove);
            // 
            // performanceInfos
            // 
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            resources.ApplyResources(this.performanceInfos, "performanceInfos");
            this.performanceInfos.FontText = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("Microsoft YaHei UI", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.performanceInfos.ItemMargan = 0;
            this.performanceInfos.LineOffest = 0;
            this.performanceInfos.MaxSpeicalItemsWidth = 250;
            this.performanceInfos.MaxSpeicalItemsWidthLimit = 250;
            this.performanceInfos.Name = "performanceInfos";
            this.performanceInfos.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseClick);
            this.performanceInfos.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDoubleClick);
            // 
            // performanceGridGlobal
            // 
            resources.ApplyResources(this.performanceGridGlobal, "performanceGridGlobal");
            this.performanceGridGlobal.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(85)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
            this.performanceGridGlobal.BgColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(231)))), ((int)(((byte)(241)))));
            this.performanceGridGlobal.BottomTextHeight = 20;
            this.performanceGridGlobal.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridGlobal.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridGlobal.DrawData2 = false;
            this.performanceGridGlobal.DrawData2Bg = true;
            this.performanceGridGlobal.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(226)))), ((int)(((byte)(240)))));
            this.performanceGridGlobal.MaxScaleText = "";
            this.performanceGridGlobal.MaxScaleValue = 0;
            this.performanceGridGlobal.MaxValue = 100;
            this.performanceGridGlobal.Name = "performanceGridGlobal";
            this.toolTip1.SetToolTip(this.performanceGridGlobal, resources.GetString("performanceGridGlobal.ToolTip"));
            this.performanceGridGlobal.TopTextHeight = 20;
            this.performanceGridGlobal.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseClick);
            this.performanceGridGlobal.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDoubleClick);
            this.performanceGridGlobal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDown);
            // 
            // performanceTitle
            // 
            resources.ApplyResources(this.performanceTitle, "performanceTitle");
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.performanceTitle.TitleFont = new System.Drawing.Font("Microsoft YaHei UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.performanceTitle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseClick);
            this.performanceTitle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDoubleClick);
            this.performanceTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDown);
            // 
            // performanceCpusAll
            // 
            resources.ApplyResources(this.performanceCpusAll, "performanceCpusAll");
            this.performanceCpusAll.Name = "performanceCpusAll";
            this.performanceCpusAll.TabStop = false;
            this.performanceCpusAll.Paint += new System.Windows.Forms.PaintEventHandler(this.performanceCpusAll_Paint);
            this.performanceCpusAll.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseClick);
            this.performanceCpusAll.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDoubleClick);
            this.performanceCpusAll.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.显示内核时间ToolStripMenuItem,
            this.图形摘要视图ToolStripMenuItem,
            this.查看ToolStripMenuItem,
            this.复制ToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // 显示内核时间ToolStripMenuItem
            // 
            this.显示内核时间ToolStripMenuItem.CheckOnClick = true;
            this.显示内核时间ToolStripMenuItem.Name = "显示内核时间ToolStripMenuItem";
            resources.ApplyResources(this.显示内核时间ToolStripMenuItem, "显示内核时间ToolStripMenuItem");
            this.显示内核时间ToolStripMenuItem.CheckedChanged += new System.EventHandler(this.显示内核时间ToolStripMenuItem_CheckedChanged);
            // 
            // 图形摘要视图ToolStripMenuItem
            // 
            this.图形摘要视图ToolStripMenuItem.CheckOnClick = true;
            this.图形摘要视图ToolStripMenuItem.Name = "图形摘要视图ToolStripMenuItem";
            resources.ApplyResources(this.图形摘要视图ToolStripMenuItem, "图形摘要视图ToolStripMenuItem");
            this.图形摘要视图ToolStripMenuItem.Click += new System.EventHandler(this.图形摘要视图ToolStripMenuItem_Click);
            // 
            // 查看ToolStripMenuItem
            // 
            this.查看ToolStripMenuItem.Name = "查看ToolStripMenuItem";
            resources.ApplyResources(this.查看ToolStripMenuItem, "查看ToolStripMenuItem");
            // 
            // 复制ToolStripMenuItem
            // 
            this.复制ToolStripMenuItem.Name = "复制ToolStripMenuItem";
            resources.ApplyResources(this.复制ToolStripMenuItem, "复制ToolStripMenuItem");
            this.复制ToolStripMenuItem.Click += new System.EventHandler(this.复制ToolStripMenuItem_Click);
            // 
            // panelGrid
            // 
            resources.ApplyResources(this.panelGrid, "panelGrid");
            this.panelGrid.Controls.Add(this.performanceTitle);
            this.panelGrid.Controls.Add(this.performanceCpus);
            this.panelGrid.Controls.Add(this.performanceGridGlobal);
            this.panelGrid.Controls.Add(this.performanceCpusAll);
            this.panelGrid.Name = "panelGrid";
            // 
            // PerformancePageCpu
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.panelGrid);
            this.Name = "PerformancePageCpu";
            this.Load += new System.EventHandler(this.PerformanceCpu_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageCpu_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.performanceCpus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.performanceCpusAll)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public PerformanceGrid performanceGridGlobal;
        public PerformanceTitle performanceTitle;
        public PerformanceInfos performanceInfos;
        private System.Windows.Forms.PictureBox performanceCpus;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox performanceCpusAll;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem 显示内核时间ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 图形摘要视图ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.Panel panelGrid;
    }
}
