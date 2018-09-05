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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformancePageDisk));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.performanceGridDiskTime = new PCMgr.Ctls.PerformanceGrid();
            this.performanceGridSpeed = new PCMgr.Ctls.PerformanceGrid();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.图形摘要视图ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.performanceTitle1 = new PCMgr.Ctls.PerformanceTitle();
            this.performanceInfos = new PCMgr.Ctls.PerformanceInfos();
            this.contextMenuStrip.SuspendLayout();
            this.panelGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // performanceGridDiskTime
            // 
            resources.ApplyResources(this.performanceGridDiskTime, "performanceGridDiskTime");
            this.performanceGridDiskTime.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(223)))));
            this.performanceGridDiskTime.BgColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.performanceGridDiskTime.BottomTextHeight = 20;
            this.performanceGridDiskTime.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            this.performanceGridDiskTime.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            this.performanceGridDiskTime.DrawData2 = false;
            this.performanceGridDiskTime.DrawData2Bg = false;
            this.performanceGridDiskTime.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            this.performanceGridDiskTime.MaxScaleText = "";
            this.performanceGridDiskTime.MaxScaleValue = 0;
            this.performanceGridDiskTime.MaxValue = 100;
            this.performanceGridDiskTime.Name = "performanceGridDiskTime";
            this.toolTip1.SetToolTip(this.performanceGridDiskTime, resources.GetString("performanceGridDiskTime.ToolTip"));
            this.performanceGridDiskTime.TopTextHeight = 20;
            this.performanceGridDiskTime.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseClick);
            this.performanceGridDiskTime.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDoubleClick);
            this.performanceGridDiskTime.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDown);
            // 
            // performanceGridSpeed
            // 
            resources.ApplyResources(this.performanceGridSpeed, "performanceGridSpeed");
            this.performanceGridSpeed.BgColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(223)))));
            this.performanceGridSpeed.BgColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.performanceGridSpeed.BottomTextHeight = 20;
            this.performanceGridSpeed.DrawColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            this.performanceGridSpeed.DrawColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(180)))), ((int)(((byte)(90)))));
            this.performanceGridSpeed.DrawData2 = false;
            this.performanceGridSpeed.DrawData2Bg = false;
            this.performanceGridSpeed.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            this.performanceGridSpeed.MaxScaleText = "";
            this.performanceGridSpeed.MaxScaleValue = 0;
            this.performanceGridSpeed.MaxValue = 100;
            this.performanceGridSpeed.Name = "performanceGridSpeed";
            this.toolTip1.SetToolTip(this.performanceGridSpeed, resources.GetString("performanceGridSpeed.ToolTip"));
            this.performanceGridSpeed.TopTextHeight = 20;
            this.performanceGridSpeed.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseClick);
            this.performanceGridSpeed.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDoubleClick);
            this.performanceGridSpeed.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDown);
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
            this.panelGrid.Controls.Add(this.performanceTitle1);
            this.panelGrid.Controls.Add(this.performanceGridDiskTime);
            this.panelGrid.Controls.Add(this.performanceGridSpeed);
            this.panelGrid.Name = "panelGrid";
            // 
            // performanceTitle1
            // 
            resources.ApplyResources(this.performanceTitle1, "performanceTitle1");
            this.performanceTitle1.Name = "performanceTitle1";
            this.performanceTitle1.SmallTitleFont = new System.Drawing.Font("微软雅黑", 12F);
            this.performanceTitle1.TitleFont = new System.Drawing.Font("微软雅黑", 18F);
            this.performanceTitle1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseClick);
            this.performanceTitle1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDoubleClick);
            this.performanceTitle1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDown);
            // 
            // performanceInfos
            // 
            this.performanceInfos.ColorText = System.Drawing.Color.Black;
            this.performanceInfos.ColorTitle = System.Drawing.Color.Gray;
            resources.ApplyResources(this.performanceInfos, "performanceInfos");
            this.performanceInfos.FontText = new System.Drawing.Font("微软雅黑", 9F);
            this.performanceInfos.FontTextSpeical = new System.Drawing.Font("微软雅黑", 15F);
            this.performanceInfos.FontTitle = new System.Drawing.Font("微软雅黑", 10.5F);
            this.performanceInfos.ItemMargan = 10;
            this.performanceInfos.LineOffest = 6;
            this.performanceInfos.MaxSpeicalItemsWidth = 300;
            this.performanceInfos.MaxSpeicalItemsWidthLimit = 300;
            this.performanceInfos.Name = "performanceInfos";
            this.performanceInfos.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseClick);
            this.performanceInfos.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDoubleClick);
            // 
            // PerformancePageDisk
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.performanceInfos);
            this.Controls.Add(this.panelGrid);
            this.Name = "PerformancePageDisk";
            this.Load += new System.EventHandler(this.PerformancePageDisk_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformancePageDisk_MouseDown);
            this.contextMenuStrip.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PerformanceGrid performanceGridDiskTime;
        private PerformanceTitle performanceTitle1;
        private PerformanceGrid performanceGridSpeed;
        private PerformanceInfos performanceInfos;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem 图形摘要视图ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.Panel panelGrid;
    }
}
