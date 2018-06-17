namespace TaskMgr
{
    partial class FormMain
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.spl1 = new System.Windows.Forms.PictureBox();
            this.lbShowDetals = new System.Windows.Forms.LinkLabel();
            this.btnEndProcess = new System.Windows.Forms.Button();
            this.lbProcessCount = new System.Windows.Forms.Label();
            this.listProcess = new TaskMgr.Ctls.TaskMgrList();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lbDriversCount = new System.Windows.Forms.Label();
            this.listDrivers = new TaskMgr.Ctls.TaskMgrList();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.spBottom = new System.Windows.Forms.PictureBox();
            this.tabControlMain.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spl1)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spBottom)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPage1);
            this.tabControlMain.Controls.Add(this.tabPage2);
            this.tabControlMain.Controls.Add(this.tabPage3);
            this.tabControlMain.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(909, 594);
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.spl1);
            this.tabPage1.Controls.Add(this.lbShowDetals);
            this.tabPage1.Controls.Add(this.btnEndProcess);
            this.tabPage1.Controls.Add(this.lbProcessCount);
            this.tabPage1.Controls.Add(this.listProcess);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(901, 564);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "进程管理";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // spl1
            // 
            this.spl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(207)))), ((int)(((byte)(207)))));
            this.spl1.Location = new System.Drawing.Point(163, 537);
            this.spl1.Name = "spl1";
            this.spl1.Size = new System.Drawing.Size(1, 14);
            this.spl1.TabIndex = 7;
            this.spl1.TabStop = false;
            // 
            // lbShowDetals
            // 
            this.lbShowDetals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbShowDetals.DisabledLinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.lbShowDetals.Image = global::TaskMgr.Properties.Resources.application_view_list;
            this.lbShowDetals.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbShowDetals.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lbShowDetals.LinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.lbShowDetals.Location = new System.Drawing.Point(8, 535);
            this.lbShowDetals.Name = "lbShowDetals";
            this.lbShowDetals.Size = new System.Drawing.Size(150, 17);
            this.lbShowDetals.TabIndex = 6;
            this.lbShowDetals.TabStop = true;
            this.lbShowDetals.Text = "打开进程详细信息窗口";
            this.lbShowDetals.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbShowDetals.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbShowDetals_LinkClicked);
            // 
            // btnEndProcess
            // 
            this.btnEndProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEndProcess.Enabled = false;
            this.btnEndProcess.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnEndProcess.Location = new System.Drawing.Point(806, 531);
            this.btnEndProcess.Name = "btnEndProcess";
            this.btnEndProcess.Size = new System.Drawing.Size(85, 24);
            this.btnEndProcess.TabIndex = 2;
            this.btnEndProcess.Text = "结束进程(E)";
            this.btnEndProcess.UseVisualStyleBackColor = true;
            this.btnEndProcess.Click += new System.EventHandler(this.btnEndProcess_Click);
            // 
            // lbProcessCount
            // 
            this.lbProcessCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbProcessCount.AutoSize = true;
            this.lbProcessCount.Location = new System.Drawing.Point(171, 535);
            this.lbProcessCount.Name = "lbProcessCount";
            this.lbProcessCount.Size = new System.Drawing.Size(66, 17);
            this.lbProcessCount.TabIndex = 5;
            this.lbProcessCount.Text = "进程数：--";
            // 
            // listProcess
            // 
            this.listProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listProcess.BackColor = System.Drawing.SystemColors.Window;
            this.listProcess.FocusedType = false;
            this.listProcess.Icons = null;
            this.listProcess.ListViewItemSorter = null;
            this.listProcess.Location = new System.Drawing.Point(0, 0);
            this.listProcess.Name = "listProcess";
            this.listProcess.ShowGroup = false;
            this.listProcess.Size = new System.Drawing.Size(901, 520);
            this.listProcess.TabIndex = 4;
            this.listProcess.Text = "taskMgrList1";
            this.listProcess.Value = 0D;
            this.listProcess.XOffest = 0;
            this.listProcess.SelectItemChanged += new System.EventHandler(this.listProcess_SelectItemChanged);
            this.listProcess.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listProcess_MouseDown);
            this.listProcess.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listProcess_MouseUp);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lbDriversCount);
            this.tabPage2.Controls.Add(this.listDrivers);
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(901, 564);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "内核管理";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lbDriversCount
            // 
            this.lbDriversCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbDriversCount.AutoSize = true;
            this.lbDriversCount.Location = new System.Drawing.Point(8, 535);
            this.lbDriversCount.Name = "lbDriversCount";
            this.lbDriversCount.Size = new System.Drawing.Size(66, 17);
            this.lbDriversCount.TabIndex = 6;
            this.lbDriversCount.Text = "驱动数：--";
            // 
            // listDrivers
            // 
            this.listDrivers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listDrivers.BackColor = System.Drawing.SystemColors.Window;
            this.listDrivers.FocusedType = false;
            this.listDrivers.ListViewItemSorter = null;
            this.listDrivers.Location = new System.Drawing.Point(1, 1);
            this.listDrivers.Name = "listDrivers";
            this.listDrivers.ShowGroup = false;
            this.listDrivers.Size = new System.Drawing.Size(898, 519);
            this.listDrivers.TabIndex = 0;
            this.listDrivers.Text = "taskMgrList1";
            this.listDrivers.Value = 0D;
            this.listDrivers.XOffest = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(901, 564);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "系统管理";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // spBottom
            // 
            this.spBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spBottom.BackColor = System.Drawing.Color.DarkGray;
            this.spBottom.Location = new System.Drawing.Point(0, 545);
            this.spBottom.Name = "spBottom";
            this.spBottom.Size = new System.Drawing.Size(907, 1);
            this.spBottom.TabIndex = 1;
            this.spBottom.TabStop = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(907, 593);
            this.Controls.Add(this.spBottom);
            this.Controls.Add(this.tabControlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "任务管理器";
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.Deactivate += new System.EventHandler(this.FormMain_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spl1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spBottom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.PictureBox spBottom;
        private System.Windows.Forms.Button btnEndProcess;
        private Ctls.TaskMgrList listProcess;
        private System.Windows.Forms.Label lbProcessCount;
        private Ctls.TaskMgrList listDrivers;
        private System.Windows.Forms.PictureBox spl1;
        private System.Windows.Forms.LinkLabel lbShowDetals;
        private System.Windows.Forms.Label lbDriversCount;
    }
}

