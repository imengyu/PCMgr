namespace PCMgr.WorkWindow
{
    partial class FormHooks
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("SSDT");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Shadow SSDT");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Object 钩子");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormHooks));
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControlUser = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainerKernel = new System.Windows.Forms.SplitContainer();
            this.treeViewKernelHooks = new System.Windows.Forms.TreeView();
            this.tabControlMain.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControlUser.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerKernel)).BeginInit();
            this.splitContainerKernel.Panel1.SuspendLayout();
            this.splitContainerKernel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControlMain.Controls.Add(this.tabPage1);
            this.tabControlMain.Controls.Add(this.tabPage2);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(947, 570);
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tabControlUser);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(939, 541);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "应用层钩子";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabControlUser
            // 
            this.tabControlUser.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControlUser.Controls.Add(this.tabPage3);
            this.tabControlUser.Controls.Add(this.tabPage4);
            this.tabControlUser.Location = new System.Drawing.Point(3, 3);
            this.tabControlUser.Multiline = true;
            this.tabControlUser.Name = "tabControlUser";
            this.tabControlUser.SelectedIndex = 0;
            this.tabControlUser.Size = new System.Drawing.Size(940, 538);
            this.tabControlUser.TabIndex = 1;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(932, 509);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "消息钩子";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(932, 509);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "键盘钩子";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainerKernel);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(939, 541);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "内核钩子";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainerKernel
            // 
            this.splitContainerKernel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerKernel.Location = new System.Drawing.Point(3, 3);
            this.splitContainerKernel.Name = "splitContainerKernel";
            // 
            // splitContainerKernel.Panel1
            // 
            this.splitContainerKernel.Panel1.Controls.Add(this.treeViewKernelHooks);
            this.splitContainerKernel.Size = new System.Drawing.Size(933, 535);
            this.splitContainerKernel.SplitterDistance = 176;
            this.splitContainerKernel.TabIndex = 0;
            // 
            // treeViewKernelHooks
            // 
            this.treeViewKernelHooks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewKernelHooks.Location = new System.Drawing.Point(0, 0);
            this.treeViewKernelHooks.Name = "treeViewKernelHooks";
            treeNode1.Name = "SSDT";
            treeNode1.Text = "SSDT";
            treeNode2.Name = "Shadow SSDT";
            treeNode2.Text = "Shadow SSDT";
            treeNode3.Name = "Object 钩子";
            treeNode3.Text = "Object 钩子";
            this.treeViewKernelHooks.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            this.treeViewKernelHooks.Size = new System.Drawing.Size(176, 535);
            this.treeViewKernelHooks.TabIndex = 1;
            // 
            // FormHooks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(947, 570);
            this.Controls.Add(this.tabControlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormHooks";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "钩子";
            this.tabControlMain.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControlUser.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainerKernel.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerKernel)).EndInit();
            this.splitContainerKernel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabControl tabControlUser;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainerKernel;
        private System.Windows.Forms.TreeView treeViewKernelHooks;
    }
}