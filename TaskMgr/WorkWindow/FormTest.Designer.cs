namespace PCMgr.WorkWindow
{
    partial class FormTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripUWP = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打开应用ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.卸载应用ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开安装位置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制名称ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制完整名称ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制发布者ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.项目1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.进行ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenuStripUWP.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.AllowColumnReorder = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(54, 45);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(767, 270);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "2222";
            this.columnHeader1.Width = 138;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "1111";
            this.columnHeader2.Width = 43;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "3333";
            this.columnHeader3.Width = 54;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "44444";
            this.columnHeader4.Width = 49;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "55";
            this.columnHeader5.Width = 34;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Width = 85;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "ColumnHeader6";
            this.columnHeader7.Width = 100;
            // 
            // contextMenuStripUWP
            // 
            this.contextMenuStripUWP.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStripUWP.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开应用ToolStripMenuItem,
            this.卸载应用ToolStripMenuItem,
            this.toolStripSeparator2,
            this.打开安装位置ToolStripMenuItem,
            this.toolStripSeparator1,
            this.复制名称ToolStripMenuItem,
            this.复制完整名称ToolStripMenuItem,
            this.复制发布者ToolStripMenuItem});
            this.contextMenuStripUWP.Name = "contextMenuStripUWP";
            this.contextMenuStripUWP.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStripUWP.Size = new System.Drawing.Size(181, 170);
            // 
            // 打开应用ToolStripMenuItem
            // 
            this.打开应用ToolStripMenuItem.Checked = true;
            this.打开应用ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.打开应用ToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.打开应用ToolStripMenuItem.Name = "打开应用ToolStripMenuItem";
            this.打开应用ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.打开应用ToolStripMenuItem.Text = "打开应用(&O)";
            // 
            // 卸载应用ToolStripMenuItem
            // 
            this.卸载应用ToolStripMenuItem.Name = "卸载应用ToolStripMenuItem";
            this.卸载应用ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.卸载应用ToolStripMenuItem.Text = "卸载应用(&U)";
            // 
            // 打开安装位置ToolStripMenuItem
            // 
            this.打开安装位置ToolStripMenuItem.Name = "打开安装位置ToolStripMenuItem";
            this.打开安装位置ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.打开安装位置ToolStripMenuItem.Text = "打开安装位置(&D)";
            // 
            // 复制名称ToolStripMenuItem
            // 
            this.复制名称ToolStripMenuItem.Checked = true;
            this.复制名称ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.复制名称ToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.复制名称ToolStripMenuItem.Name = "复制名称ToolStripMenuItem";
            this.复制名称ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.复制名称ToolStripMenuItem.Text = "复制名称(&N)";
            // 
            // 复制完整名称ToolStripMenuItem
            // 
            this.复制完整名称ToolStripMenuItem.Enabled = false;
            this.复制完整名称ToolStripMenuItem.Name = "复制完整名称ToolStripMenuItem";
            this.复制完整名称ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.复制完整名称ToolStripMenuItem.Text = "复制完整名称(&N)";
            // 
            // 复制发布者ToolStripMenuItem
            // 
            this.复制发布者ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.项目1ToolStripMenuItem,
            this.进行ToolStripMenuItem});
            this.复制发布者ToolStripMenuItem.Name = "复制发布者ToolStripMenuItem";
            this.复制发布者ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.复制发布者ToolStripMenuItem.Text = "复制(&C)";
            // 
            // 项目1ToolStripMenuItem
            // 
            this.项目1ToolStripMenuItem.Checked = true;
            this.项目1ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.项目1ToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.项目1ToolStripMenuItem.Name = "项目1ToolStripMenuItem";
            this.项目1ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.项目1ToolStripMenuItem.Text = "项目1";
            // 
            // 进行ToolStripMenuItem
            // 
            this.进行ToolStripMenuItem.Name = "进行ToolStripMenuItem";
            this.进行ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.进行ToolStripMenuItem.Text = "进行";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // FormTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 346);
            this.Controls.Add(this.listView1);
            this.Name = "FormTest";
            this.Text = "FormTest";
            this.Load += new System.EventHandler(this.FormTest_Load);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormTest_MouseUp);
            this.contextMenuStripUWP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        public System.Windows.Forms.ContextMenuStrip contextMenuStripUWP;
        public System.Windows.Forms.ToolStripMenuItem 打开应用ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem 卸载应用ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem 打开安装位置ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem 复制名称ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem 复制完整名称ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem 复制发布者ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 项目1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 进行ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}