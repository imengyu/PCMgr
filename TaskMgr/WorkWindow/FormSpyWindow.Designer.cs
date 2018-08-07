namespace PCMgr.WorkWindow
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
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.启用窗口ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.禁用窗口ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.显示逻辑区域ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelMain.SuspendLayout();
            this.contextMenuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewMain
            // 
            resources.ApplyResources(this.treeViewMain, "treeViewMain");
            this.treeViewMain.FullRowSelect = true;
            this.treeViewMain.HotTracking = true;
            this.treeViewMain.Name = "treeViewMain";
            this.treeViewMain.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMain_AfterExpand);
            this.treeViewMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMain_AfterSelect);
            this.treeViewMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeViewMain_MouseUp);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBoxClassName
            // 
            resources.ApplyResources(this.textBoxClassName, "textBoxClassName");
            this.textBoxClassName.Name = "textBoxClassName";
            this.textBoxClassName.ReadOnly = true;
            // 
            // textBoxRect
            // 
            resources.ApplyResources(this.textBoxRect, "textBoxRect");
            this.textBoxRect.Name = "textBoxRect";
            this.textBoxRect.ReadOnly = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // textBoxClientRect
            // 
            resources.ApplyResources(this.textBoxClientRect, "textBoxClientRect");
            this.textBoxClientRect.Name = "textBoxClientRect";
            this.textBoxClientRect.ReadOnly = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // textBoxCtlId
            // 
            resources.ApplyResources(this.textBoxCtlId, "textBoxCtlId");
            this.textBoxCtlId.Name = "textBoxCtlId";
            this.textBoxCtlId.ReadOnly = true;
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // textBoxHandle
            // 
            resources.ApplyResources(this.textBoxHandle, "textBoxHandle");
            this.textBoxHandle.Name = "textBoxHandle";
            this.textBoxHandle.ReadOnly = true;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // textBoxText
            // 
            resources.ApplyResources(this.textBoxText, "textBoxText");
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyDown);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // textBoxStytle
            // 
            resources.ApplyResources(this.textBoxStytle, "textBoxStytle");
            this.textBoxStytle.Name = "textBoxStytle";
            this.textBoxStytle.ReadOnly = true;
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // textBoxExStytle
            // 
            resources.ApplyResources(this.textBoxExStytle, "textBoxExStytle");
            this.textBoxExStytle.Name = "textBoxExStytle";
            this.textBoxExStytle.ReadOnly = true;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // buttonRefesh
            // 
            resources.ApplyResources(this.buttonRefesh, "buttonRefesh");
            this.buttonRefesh.Name = "buttonRefesh";
            this.buttonRefesh.UseVisualStyleBackColor = true;
            this.buttonRefesh.Click += new System.EventHandler(this.buttonRefesh_Click);
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // checkBoxShowVisible
            // 
            resources.ApplyResources(this.checkBoxShowVisible, "checkBoxShowVisible");
            this.checkBoxShowVisible.Name = "checkBoxShowVisible";
            this.checkBoxShowVisible.UseVisualStyleBackColor = true;
            // 
            // panelMain
            // 
            resources.ApplyResources(this.panelMain, "panelMain");
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
            this.panelMain.Name = "panelMain";
            // 
            // labelState
            // 
            resources.ApplyResources(this.labelState, "labelState");
            this.labelState.Name = "labelState";
            // 
            // contextMenuStripMain
            // 
            resources.ApplyResources(this.contextMenuStripMain, "contextMenuStripMain");
            this.contextMenuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新ToolStripMenuItem,
            this.toolStripSeparator1,
            this.显示ToolStripMenuItem,
            this.隐藏ToolStripMenuItem,
            this.toolStripSeparator2,
            this.启用窗口ToolStripMenuItem,
            this.禁用窗口ToolStripMenuItem,
            this.toolStripSeparator3,
            this.删除ToolStripMenuItem,
            this.显示逻辑区域ToolStripMenuItem});
            this.contextMenuStripMain.Name = "contextMenuStripMain";
            // 
            // 刷新ToolStripMenuItem
            // 
            resources.ApplyResources(this.刷新ToolStripMenuItem, "刷新ToolStripMenuItem");
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.刷新ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // 显示ToolStripMenuItem
            // 
            resources.ApplyResources(this.显示ToolStripMenuItem, "显示ToolStripMenuItem");
            this.显示ToolStripMenuItem.Name = "显示ToolStripMenuItem";
            this.显示ToolStripMenuItem.Click += new System.EventHandler(this.显示ToolStripMenuItem_Click);
            // 
            // 隐藏ToolStripMenuItem
            // 
            resources.ApplyResources(this.隐藏ToolStripMenuItem, "隐藏ToolStripMenuItem");
            this.隐藏ToolStripMenuItem.Name = "隐藏ToolStripMenuItem";
            this.隐藏ToolStripMenuItem.Click += new System.EventHandler(this.隐藏ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // 启用窗口ToolStripMenuItem
            // 
            resources.ApplyResources(this.启用窗口ToolStripMenuItem, "启用窗口ToolStripMenuItem");
            this.启用窗口ToolStripMenuItem.Name = "启用窗口ToolStripMenuItem";
            this.启用窗口ToolStripMenuItem.Click += new System.EventHandler(this.启用窗口ToolStripMenuItem_Click);
            // 
            // 禁用窗口ToolStripMenuItem
            // 
            resources.ApplyResources(this.禁用窗口ToolStripMenuItem, "禁用窗口ToolStripMenuItem");
            this.禁用窗口ToolStripMenuItem.Name = "禁用窗口ToolStripMenuItem";
            this.禁用窗口ToolStripMenuItem.Click += new System.EventHandler(this.禁用窗口ToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // 删除ToolStripMenuItem
            // 
            resources.ApplyResources(this.删除ToolStripMenuItem, "删除ToolStripMenuItem");
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // 显示逻辑区域ToolStripMenuItem
            // 
            resources.ApplyResources(this.显示逻辑区域ToolStripMenuItem, "显示逻辑区域ToolStripMenuItem");
            this.显示逻辑区域ToolStripMenuItem.Name = "显示逻辑区域ToolStripMenuItem";
            this.显示逻辑区域ToolStripMenuItem.Click += new System.EventHandler(this.显示逻辑区域ToolStripMenuItem_Click);
            // 
            // FormSpyWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.labelState);
            this.Name = "FormSpyWindow";
            this.ShowInTaskbar = false;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 启用窗口ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 禁用窗口ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem 显示逻辑区域ToolStripMenuItem;
    }
}