namespace PCMgrUpdate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.mainProgress = new System.Windows.Forms.ProgressBar();
            this.mainCancelBtn = new System.Windows.Forms.Button();
            this.mainStatus = new System.Windows.Forms.Label();
            this.mainPrecent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainProgress
            // 
            this.mainProgress.Location = new System.Drawing.Point(25, 85);
            this.mainProgress.Name = "mainProgress";
            this.mainProgress.Size = new System.Drawing.Size(1035, 46);
            this.mainProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.mainProgress.TabIndex = 0;
            // 
            // mainCancelBtn
            // 
            this.mainCancelBtn.Location = new System.Drawing.Point(914, 154);
            this.mainCancelBtn.Name = "mainCancelBtn";
            this.mainCancelBtn.Size = new System.Drawing.Size(146, 46);
            this.mainCancelBtn.TabIndex = 1;
            this.mainCancelBtn.Text = "取消";
            this.mainCancelBtn.UseVisualStyleBackColor = true;
            this.mainCancelBtn.Click += new System.EventHandler(this.mainCancelBtn_Click);
            // 
            // mainStatus
            // 
            this.mainStatus.AutoEllipsis = true;
            this.mainStatus.Location = new System.Drawing.Point(33, 34);
            this.mainStatus.Name = "mainStatus";
            this.mainStatus.Size = new System.Drawing.Size(1027, 29);
            this.mainStatus.TabIndex = 2;
            this.mainStatus.Text = "正在初始化......";
            this.mainStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // mainPrecent
            // 
            this.mainPrecent.AutoEllipsis = true;
            this.mainPrecent.Location = new System.Drawing.Point(33, 165);
            this.mainPrecent.Name = "mainPrecent";
            this.mainPrecent.Size = new System.Drawing.Size(860, 24);
            this.mainPrecent.TabIndex = 3;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 241);
            this.Controls.Add(this.mainPrecent);
            this.Controls.Add(this.mainStatus);
            this.Controls.Add(this.mainCancelBtn);
            this.Controls.Add(this.mainProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PC Manager 更新程序";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar mainProgress;
        private System.Windows.Forms.Label mainStatus;
        private System.Windows.Forms.Label mainPrecent;
        public System.Windows.Forms.Button mainCancelBtn;
    }
}

