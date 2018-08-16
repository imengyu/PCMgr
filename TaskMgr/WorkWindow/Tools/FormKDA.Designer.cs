namespace PCMgr.WorkWindow
{
    partial class FormKDA
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormKDA));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.listViewDA = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBoxBariny = new System.Windows.Forms.TextBox();
            this.textBoxTargetAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDesize = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.复制地址ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制二进制码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制OpCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制汇编代码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelErrStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            resources.ApplyResources(this.splitContainer, "splitContainer");
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.labelErrStatus);
            this.splitContainer.Panel1.Controls.Add(this.listViewDA);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.textBoxBariny);
            // 
            // listViewDA
            // 
            this.listViewDA.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader3});
            resources.ApplyResources(this.listViewDA, "listViewDA");
            this.listViewDA.FullRowSelect = true;
            this.listViewDA.MultiSelect = false;
            this.listViewDA.Name = "listViewDA";
            this.listViewDA.UseCompatibleStateImageBehavior = false;
            this.listViewDA.View = System.Windows.Forms.View.Details;
            this.listViewDA.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewDA_MouseClick);
            this.listViewDA.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewDA_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // textBoxBariny
            // 
            resources.ApplyResources(this.textBoxBariny, "textBoxBariny");
            this.textBoxBariny.Name = "textBoxBariny";
            // 
            // textBoxTargetAddress
            // 
            resources.ApplyResources(this.textBoxTargetAddress, "textBoxTargetAddress");
            this.textBoxTargetAddress.Name = "textBoxTargetAddress";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // textBoxDesize
            // 
            resources.ApplyResources(this.textBoxDesize, "textBoxDesize");
            this.textBoxDesize.Name = "textBoxDesize";
            // 
            // buttonStart
            // 
            resources.ApplyResources(this.buttonStart, "buttonStart");
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.复制地址ToolStripMenuItem,
            this.复制二进制码ToolStripMenuItem,
            this.复制OpCodeToolStripMenuItem,
            this.复制汇编代码ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // 复制地址ToolStripMenuItem
            // 
            this.复制地址ToolStripMenuItem.Name = "复制地址ToolStripMenuItem";
            resources.ApplyResources(this.复制地址ToolStripMenuItem, "复制地址ToolStripMenuItem");
            this.复制地址ToolStripMenuItem.Click += new System.EventHandler(this.复制地址ToolStripMenuItem_Click);
            // 
            // 复制二进制码ToolStripMenuItem
            // 
            this.复制二进制码ToolStripMenuItem.Name = "复制二进制码ToolStripMenuItem";
            resources.ApplyResources(this.复制二进制码ToolStripMenuItem, "复制二进制码ToolStripMenuItem");
            this.复制二进制码ToolStripMenuItem.Click += new System.EventHandler(this.复制二进制码ToolStripMenuItem_Click);
            // 
            // 复制OpCodeToolStripMenuItem
            // 
            this.复制OpCodeToolStripMenuItem.Name = "复制OpCodeToolStripMenuItem";
            resources.ApplyResources(this.复制OpCodeToolStripMenuItem, "复制OpCodeToolStripMenuItem");
            this.复制OpCodeToolStripMenuItem.Click += new System.EventHandler(this.复制OpCodeToolStripMenuItem_Click);
            // 
            // 复制汇编代码ToolStripMenuItem
            // 
            this.复制汇编代码ToolStripMenuItem.Name = "复制汇编代码ToolStripMenuItem";
            resources.ApplyResources(this.复制汇编代码ToolStripMenuItem, "复制汇编代码ToolStripMenuItem");
            this.复制汇编代码ToolStripMenuItem.Click += new System.EventHandler(this.复制汇编代码ToolStripMenuItem_Click);
            // 
            // labelErrStatus
            // 
            resources.ApplyResources(this.labelErrStatus, "labelErrStatus");
            this.labelErrStatus.AutoEllipsis = true;
            this.labelErrStatus.BackColor = System.Drawing.Color.White;
            this.labelErrStatus.Name = "labelErrStatus";
            // 
            // FormDA
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxDesize);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxTargetAddress);
            this.Controls.Add(this.splitContainer);
            this.Name = "FormDA";
            this.Load += new System.EventHandler(this.FormDA_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTargetAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDesize;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ListView listViewDA;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TextBox textBoxBariny;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 复制地址ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制二进制码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制OpCodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制汇编代码ToolStripMenuItem;
        private System.Windows.Forms.Label labelErrStatus;
    }
}