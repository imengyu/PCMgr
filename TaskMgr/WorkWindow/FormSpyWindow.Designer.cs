namespace TaskMgr.WorkWindow
{
    partial class FormSpyWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSpyWindow));
            this.treeViewMain = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxClassName = new System.Windows.Forms.TextBox();
            this.textBoxRect = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxClientRect = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxCtlId = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxHandle = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxStytle = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxExStytle = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonRefesh = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.checkBoxShowVisible = new System.Windows.Forms.CheckBox();
            this.panelMain = new System.Windows.Forms.Panel();
            this.labelState = new System.Windows.Forms.Label();
            this.contextMenuStripMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.显示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.隐藏ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelMain.SuspendLayout();
            this.contextMenuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewMain
            // 
            this.treeViewMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewMain.FullRowSelect = true;
            this.treeViewMain.HotTracking = true;
            this.treeViewMain.Location = new System.Drawing.Point(12, 12);
            this.treeViewMain.Name = "treeViewMain";
            this.treeViewMain.Size = new System.Drawing.Size(436, 464);
            this.treeViewMain.TabIndex = 0;
            this.treeViewMain.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMain_AfterExpand);
            this.treeViewMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMain_AfterSelect);
            this.treeViewMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeViewMain_MouseUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(494, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "类";
            // 
            // textBoxClassName
            // 
            this.textBoxClassName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxClassName.Location = new System.Drawing.Point(517, 17);
            this.textBoxClassName.Name = "textBoxClassName";
            this.textBoxClassName.ReadOnly = true;
            this.textBoxClassName.Size = new System.Drawing.Size(224, 21);
            this.textBoxClassName.TabIndex = 2;
            // 
            // textBoxRect
            // 
            this.textBoxRect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRect.Location = new System.Drawing.Point(517, 44);
            this.textBoxRect.Multiline = true;
            this.textBoxRect.Name = "textBoxRect";
            this.textBoxRect.ReadOnly = true;
            this.textBoxRect.Size = new System.Drawing.Size(224, 44);
            this.textBoxRect.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(458, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "窗口矩形";
            // 
            // textBoxClientRect
            // 
            this.textBoxClientRect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxClientRect.Location = new System.Drawing.Point(517, 94);
            this.textBoxClientRect.Multiline = true;
            this.textBoxClientRect.Name = "textBoxClientRect";
            this.textBoxClientRect.ReadOnly = true;
            this.textBoxClientRect.Size = new System.Drawing.Size(224, 44);
            this.textBoxClientRect.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(458, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "客户矩形";
            // 
            // textBoxCtlId
            // 
            this.textBoxCtlId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCtlId.Location = new System.Drawing.Point(517, 144);
            this.textBoxCtlId.Name = "textBoxCtlId";
            this.textBoxCtlId.ReadOnly = true;
            this.textBoxCtlId.Size = new System.Drawing.Size(224, 21);
            this.textBoxCtlId.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(470, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "控件ID";
            // 
            // textBoxHandle
            // 
            this.textBoxHandle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHandle.Location = new System.Drawing.Point(517, 171);
            this.textBoxHandle.Name = "textBoxHandle";
            this.textBoxHandle.ReadOnly = true;
            this.textBoxHandle.Size = new System.Drawing.Size(224, 21);
            this.textBoxHandle.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(458, 174);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "窗口句柄";
            // 
            // textBoxText
            // 
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.Location = new System.Drawing.Point(517, 198);
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxText.Size = new System.Drawing.Size(224, 86);
            this.textBoxText.TabIndex = 12;
            this.textBoxText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyDown);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(458, 201);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "窗口文字";
            // 
            // textBoxStytle
            // 
            this.textBoxStytle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStytle.Location = new System.Drawing.Point(517, 290);
            this.textBoxStytle.Multiline = true;
            this.textBoxStytle.Name = "textBoxStytle";
            this.textBoxStytle.ReadOnly = true;
            this.textBoxStytle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStytle.Size = new System.Drawing.Size(224, 80);
            this.textBoxStytle.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(458, 293);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "窗口样式";
            // 
            // textBoxExStytle
            // 
            this.textBoxExStytle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExStytle.Location = new System.Drawing.Point(517, 376);
            this.textBoxExStytle.Multiline = true;
            this.textBoxExStytle.Name = "textBoxExStytle";
            this.textBoxExStytle.ReadOnly = true;
            this.textBoxExStytle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxExStytle.Size = new System.Drawing.Size(224, 100);
            this.textBoxExStytle.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(458, 379);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 24);
            this.label8.TabIndex = 15;
            this.label8.Text = "窗口扩展\r\n样式";
            // 
            // buttonRefesh
            // 
            this.buttonRefesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefesh.Location = new System.Drawing.Point(12, 482);
            this.buttonRefesh.Name = "buttonRefesh";
            this.buttonRefesh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefesh.TabIndex = 17;
            this.buttonRefesh.Text = "刷新";
            this.buttonRefesh.UseVisualStyleBackColor = true;
            this.buttonRefesh.Click += new System.EventHandler(this.buttonRefesh_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(668, 484);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 18;
            this.buttonClose.Text = "关闭";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // checkBoxShowVisible
            // 
            this.checkBoxShowVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShowVisible.AutoSize = true;
            this.checkBoxShowVisible.Location = new System.Drawing.Point(93, 486);
            this.checkBoxShowVisible.Name = "checkBoxShowVisible";
            this.checkBoxShowVisible.Size = new System.Drawing.Size(132, 16);
            this.checkBoxShowVisible.TabIndex = 19;
            this.checkBoxShowVisible.Text = "不显示隐藏的子窗口";
            this.checkBoxShowVisible.UseVisualStyleBackColor = true;
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.checkBoxShowVisible);
            this.panelMain.Controls.Add(this.buttonRefesh);
            this.panelMain.Controls.Add(this.textBoxExStytle);
            this.panelMain.Controls.Add(this.label8);
            this.panelMain.Controls.Add(this.textBoxStytle);
            this.panelMain.Controls.Add(this.label7);
            this.panelMain.Controls.Add(this.textBoxText);
            this.panelMain.Controls.Add(this.label6);
            this.panelMain.Controls.Add(this.textBoxHandle);
            this.panelMain.Controls.Add(this.label5);
            this.panelMain.Controls.Add(this.textBoxCtlId);
            this.panelMain.Controls.Add(this.label4);
            this.panelMain.Controls.Add(this.textBoxClientRect);
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.textBoxRect);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Controls.Add(this.textBoxClassName);
            this.panelMain.Controls.Add(this.label1);
            this.panelMain.Controls.Add(this.treeViewMain);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(755, 519);
            this.panelMain.TabIndex = 20;
            this.panelMain.Visible = false;
            // 
            // labelState
            // 
            this.labelState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelState.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelState.Location = new System.Drawing.Point(0, 0);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(755, 519);
            this.labelState.TabIndex = 21;
            this.labelState.Text = "正在加载……";
            this.labelState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contextMenuStripMain
            // 
            this.contextMenuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新ToolStripMenuItem,
            this.toolStripSeparator1,
            this.显示ToolStripMenuItem,
            this.隐藏ToolStripMenuItem,
            this.删除ToolStripMenuItem});
            this.contextMenuStripMain.Name = "contextMenuStripMain";
            this.contextMenuStripMain.Size = new System.Drawing.Size(101, 98);
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.刷新ToolStripMenuItem.Text = "刷新";
            this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.刷新ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(97, 6);
            // 
            // 显示ToolStripMenuItem
            // 
            this.显示ToolStripMenuItem.Name = "显示ToolStripMenuItem";
            this.显示ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.显示ToolStripMenuItem.Text = "显示";
            this.显示ToolStripMenuItem.Click += new System.EventHandler(this.显示ToolStripMenuItem_Click);
            // 
            // 隐藏ToolStripMenuItem
            // 
            this.隐藏ToolStripMenuItem.Name = "隐藏ToolStripMenuItem";
            this.隐藏ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.隐藏ToolStripMenuItem.Text = "隐藏";
            this.隐藏ToolStripMenuItem.Click += new System.EventHandler(this.隐藏ToolStripMenuItem_Click);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.删除ToolStripMenuItem.Text = "删除";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // FormSpyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(755, 519);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.labelState);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSpyWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormSpyWindow";
            this.Load += new System.EventHandler(this.FormSpyWindow_Load);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.contextMenuStripMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewMain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxClassName;
        private System.Windows.Forms.TextBox textBoxRect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxClientRect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxCtlId;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxHandle;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxStytle;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxExStytle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonRefesh;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkBoxShowVisible;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMain;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 显示ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 隐藏ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
    }
}