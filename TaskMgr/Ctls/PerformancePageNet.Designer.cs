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
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.图形摘要视图ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.contextMenuStrip.SuspendLayout();
            this.panelGrid.SuspendLayout();
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
            this.performanceInfos.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseClick);
            this.performanceInfos.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDoubleClick);
            // 
            // performanceTitle
            // 
            resources.ApplyResources(this.performanceTitle, "performanceTitle");
            this.performanceTitle.Name = "performanceTitle";
            this.performanceTitle.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            this.performanceTitle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseClick);
            this.performanceTitle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDoubleClick);
            this.performanceTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDown);
            // 
            // performanceGrid
            // 
            resources.ApplyResources(this.performanceGrid, "performanceGrid");
            this.performanceGrid.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(255)))), ((int)(((byte)(157)))), ((int)(((byte)(89)))));
            this.performanceGrid.BgColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
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
            this.performanceGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseClick);
            this.performanceGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDoubleClick);
            this.performanceGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.图形摘要视图ToolStripMenuItem,
            this.查看ToolStripMenuItem,
            this.复制ToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
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
            this.panelGrid.Controls.Add(this.performanceGrid);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseClick);
            this.panelGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDoubleClick);
            this.panelGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDown);
            // 
            // PerformancePageNet
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.panelGrid);
            this.Name = "PerformancePageNet";
            this.Load += new System.EventHandler(this.PerformancePageNet_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageNet_MouseDown);
            this.contextMenuStrip.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PerformanceGrid performanceGrid;
        private PerformanceTitle performanceTitle;
        private PerformanceInfos performanceInfos;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem 图形摘要视图ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.Panel panelGrid;
    }
}
