namespace PCMgr.WorkWindow
{
    partial class FormTcp
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
            this.listTcp = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderProtocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLocalAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLocalPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRemoteAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRemotePort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRefesh = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.转到进程ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listTcp
            // 
            this.listTcp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listTcp.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderProtocol,
            this.columnHeaderLocalAddress,
            this.columnHeaderLocalPort,
            this.columnHeaderRemoteAddress,
            this.columnHeaderRemotePort,
            this.columnHeaderState});
            this.listTcp.Location = new System.Drawing.Point(12, 44);
            this.listTcp.Name = "listTcp";
            this.listTcp.Size = new System.Drawing.Size(768, 392);
            this.listTcp.TabIndex = 1;
            this.listTcp.UseCompatibleStateImageBehavior = false;
            this.listTcp.View = System.Windows.Forms.View.Details;
            this.listTcp.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listTcp_KeyUp);
            this.listTcp.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listTcp_MouseClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "名称";
            this.columnHeaderName.Width = 194;
            // 
            // columnHeaderProtocol
            // 
            this.columnHeaderProtocol.Text = "协议版本";
            // 
            // columnHeaderLocalAddress
            // 
            this.columnHeaderLocalAddress.Text = "本地地址";
            this.columnHeaderLocalAddress.Width = 135;
            // 
            // columnHeaderLocalPort
            // 
            this.columnHeaderLocalPort.Text = "本地端口";
            // 
            // columnHeaderRemoteAddress
            // 
            this.columnHeaderRemoteAddress.Text = "远程地址";
            this.columnHeaderRemoteAddress.Width = 150;
            // 
            // columnHeaderRemotePort
            // 
            this.columnHeaderRemotePort.Text = "远程端口";
            // 
            // columnHeaderState
            // 
            this.columnHeaderState.Text = "状态";
            this.columnHeaderState.Width = 98;
            // 
            // buttonRefesh
            // 
            this.buttonRefesh.Location = new System.Drawing.Point(12, 12);
            this.buttonRefesh.Name = "buttonRefesh";
            this.buttonRefesh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefesh.TabIndex = 2;
            this.buttonRefesh.Text = "刷新";
            this.buttonRefesh.UseVisualStyleBackColor = true;
            this.buttonRefesh.Click += new System.EventHandler(this.buttonRefesh_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.复制ToolStripMenuItem,
            this.转到进程ToolStripMenuItem,
            this.刷新ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStrip1.Size = new System.Drawing.Size(142, 70);
            // 
            // 复制ToolStripMenuItem
            // 
            this.复制ToolStripMenuItem.Name = "复制ToolStripMenuItem";
            this.复制ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.复制ToolStripMenuItem.Text = "复制(&C)";
            this.复制ToolStripMenuItem.Click += new System.EventHandler(this.复制ToolStripMenuItem_Click);
            // 
            // 转到进程ToolStripMenuItem
            // 
            this.转到进程ToolStripMenuItem.Name = "转到进程ToolStripMenuItem";
            this.转到进程ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.转到进程ToolStripMenuItem.Text = "转到进程(&G)";
            this.转到进程ToolStripMenuItem.Click += new System.EventHandler(this.转到进程ToolStripMenuItem_Click);
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.刷新ToolStripMenuItem.Text = "刷新(&R)";
            this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.刷新ToolStripMenuItem_Click);
            // 
            // FormTcp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 448);
            this.Controls.Add(this.buttonRefesh);
            this.Controls.Add(this.listTcp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormTcp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TCP 连接";
            this.Load += new System.EventHandler(this.FormTcp_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListView listTcp;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderLocalAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderLocalPort;
        private System.Windows.Forms.ColumnHeader columnHeaderRemoteAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderRemotePort;
        private System.Windows.Forms.ColumnHeader columnHeaderProtocol;
        private System.Windows.Forms.ColumnHeader columnHeaderState;
        private System.Windows.Forms.Button buttonRefesh;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 转到进程ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
    }
}