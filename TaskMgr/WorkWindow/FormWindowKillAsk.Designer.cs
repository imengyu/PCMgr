namespace PCMgr.WorkWindow
{
    partial class FormWindowKillAsk
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
            this.lb_wndinfo = new System.Windows.Forms.Label();
            this.btnKill = new System.Windows.Forms.Button();
            this.btnWndize = new System.Windows.Forms.Button();
            this.btnNoTop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lb_wndinfo
            // 
            this.lb_wndinfo.AutoSize = true;
            this.lb_wndinfo.Location = new System.Drawing.Point(46, 42);
            this.lb_wndinfo.Name = "lb_wndinfo";
            this.lb_wndinfo.Size = new System.Drawing.Size(82, 24);
            this.lb_wndinfo.TabIndex = 0;
            this.lb_wndinfo.Text = "label1";
            // 
            // btnKill
            // 
            this.btnKill.Location = new System.Drawing.Point(50, 79);
            this.btnKill.Name = "btnKill";
            this.btnKill.Size = new System.Drawing.Size(78, 44);
            this.btnKill.TabIndex = 1;
            this.btnKill.Text = "杀死";
            this.btnKill.UseVisualStyleBackColor = true;
            this.btnKill.Click += new System.EventHandler(this.btnKill_Click);
            // 
            // btnWndize
            // 
            this.btnWndize.Location = new System.Drawing.Point(134, 79);
            this.btnWndize.Name = "btnWndize";
            this.btnWndize.Size = new System.Drawing.Size(99, 44);
            this.btnWndize.TabIndex = 2;
            this.btnWndize.Text = "窗口化";
            this.btnWndize.UseVisualStyleBackColor = true;
            this.btnWndize.Click += new System.EventHandler(this.btnWndize_Click);
            // 
            // btnNoTop
            // 
            this.btnNoTop.Location = new System.Drawing.Point(239, 79);
            this.btnNoTop.Name = "btnNoTop";
            this.btnNoTop.Size = new System.Drawing.Size(117, 44);
            this.btnNoTop.TabIndex = 3;
            this.btnNoTop.Text = "取消置顶";
            this.btnNoTop.UseVisualStyleBackColor = true;
            this.btnNoTop.Click += new System.EventHandler(this.btnNoTop_Click);
            // 
            // FormWindowKillAsk
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 146);
            this.Controls.Add(this.btnNoTop);
            this.Controls.Add(this.btnWndize);
            this.Controls.Add(this.btnKill);
            this.Controls.Add(this.lb_wndinfo);
            this.Name = "FormWindowKillAsk";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "清除窗口？";
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.FormWindowKillAsk_Deactivate);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_wndinfo;
        private System.Windows.Forms.Button btnKill;
        private System.Windows.Forms.Button btnWndize;
        private System.Windows.Forms.Button btnNoTop;
    }
}