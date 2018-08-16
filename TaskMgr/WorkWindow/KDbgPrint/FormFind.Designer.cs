namespace PCMgr.WorkWindow.KDbgPrint
{
    partial class FormFind
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFind));
            this.textBoxEnter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonFind = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxFullSearch = new System.Windows.Forms.CheckBox();
            this.checkBoxDevideLorH = new System.Windows.Forms.CheckBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.radioButtonFindUp = new System.Windows.Forms.RadioButton();
            this.radioButtonFindLow = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // textBoxEnter
            // 
            this.textBoxEnter.Location = new System.Drawing.Point(59, 12);
            this.textBoxEnter.Name = "textBoxEnter";
            this.textBoxEnter.Size = new System.Drawing.Size(305, 21);
            this.textBoxEnter.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "字符串";
            // 
            // buttonFind
            // 
            this.buttonFind.Image = ((System.Drawing.Image)(resources.GetObject("buttonFind.Image")));
            this.buttonFind.Location = new System.Drawing.Point(370, 10);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(24, 23);
            this.buttonFind.TabIndex = 2;
            this.toolTip1.SetToolTip(this.buttonFind, "查找");
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // checkBoxFullSearch
            // 
            this.checkBoxFullSearch.AutoSize = true;
            this.checkBoxFullSearch.Location = new System.Drawing.Point(14, 51);
            this.checkBoxFullSearch.Name = "checkBoxFullSearch";
            this.checkBoxFullSearch.Size = new System.Drawing.Size(72, 16);
            this.checkBoxFullSearch.TabIndex = 3;
            this.checkBoxFullSearch.Text = "全字匹配";
            this.checkBoxFullSearch.UseVisualStyleBackColor = true;
            // 
            // checkBoxDevideLorH
            // 
            this.checkBoxDevideLorH.AutoSize = true;
            this.checkBoxDevideLorH.Location = new System.Drawing.Point(14, 73);
            this.checkBoxDevideLorH.Name = "checkBoxDevideLorH";
            this.checkBoxDevideLorH.Size = new System.Drawing.Size(84, 16);
            this.checkBoxDevideLorH.TabIndex = 4;
            this.checkBoxDevideLorH.Text = "区分大小写";
            this.checkBoxDevideLorH.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(320, 70);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 5;
            this.buttonClose.Text = "关闭";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // radioButtonFindUp
            // 
            this.radioButtonFindUp.AutoSize = true;
            this.radioButtonFindUp.Checked = true;
            this.radioButtonFindUp.Location = new System.Drawing.Point(137, 50);
            this.radioButtonFindUp.Name = "radioButtonFindUp";
            this.radioButtonFindUp.Size = new System.Drawing.Size(71, 16);
            this.radioButtonFindUp.TabIndex = 6;
            this.radioButtonFindUp.TabStop = true;
            this.radioButtonFindUp.Text = "向上查找";
            this.radioButtonFindUp.UseVisualStyleBackColor = true;
            // 
            // radioButtonFindLow
            // 
            this.radioButtonFindLow.AutoSize = true;
            this.radioButtonFindLow.Location = new System.Drawing.Point(137, 73);
            this.radioButtonFindLow.Name = "radioButtonFindLow";
            this.radioButtonFindLow.Size = new System.Drawing.Size(71, 16);
            this.radioButtonFindLow.TabIndex = 7;
            this.radioButtonFindLow.Text = "向下查找";
            this.radioButtonFindLow.UseVisualStyleBackColor = true;
            // 
            // FormFind
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 108);
            this.Controls.Add(this.radioButtonFindLow);
            this.Controls.Add(this.radioButtonFindUp);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.checkBoxDevideLorH);
            this.Controls.Add(this.checkBoxFullSearch);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxEnter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFind";
            this.ShowInTaskbar = false;
            this.Text = "查找";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormFind_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxEnter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBoxFullSearch;
        private System.Windows.Forms.CheckBox checkBoxDevideLorH;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.RadioButton radioButtonFindUp;
        private System.Windows.Forms.RadioButton radioButtonFindLow;
    }
}