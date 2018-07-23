namespace PCMgr.WorkWindow
{
    partial class FormFillWithData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFillWithData));
            this.checkBoxForce = new System.Windows.Forms.CheckBox();
            this.radioButtonEmepty = new System.Windows.Forms.RadioButton();
            this.radioButtonZeroData = new System.Windows.Forms.RadioButton();
            this.numericUpDownFileSize = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.labelFilePath = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFileSize)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxForce
            // 
            resources.ApplyResources(this.checkBoxForce, "checkBoxForce");
            this.checkBoxForce.Name = "checkBoxForce";
            this.checkBoxForce.UseVisualStyleBackColor = true;
            // 
            // radioButtonEmepty
            // 
            resources.ApplyResources(this.radioButtonEmepty, "radioButtonEmepty");
            this.radioButtonEmepty.Checked = true;
            this.radioButtonEmepty.Name = "radioButtonEmepty";
            this.radioButtonEmepty.TabStop = true;
            this.radioButtonEmepty.UseVisualStyleBackColor = true;
            // 
            // radioButtonZeroData
            // 
            resources.ApplyResources(this.radioButtonZeroData, "radioButtonZeroData");
            this.radioButtonZeroData.Name = "radioButtonZeroData";
            this.radioButtonZeroData.TabStop = true;
            this.radioButtonZeroData.UseVisualStyleBackColor = true;
            // 
            // numericUpDownFileSize
            // 
            resources.ApplyResources(this.numericUpDownFileSize, "numericUpDownFileSize");
            this.numericUpDownFileSize.DecimalPlaces = 2;
            this.numericUpDownFileSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numericUpDownFileSize.Name = "numericUpDownFileSize";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonStart
            // 
            resources.ApplyResources(this.buttonStart, "buttonStart");
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // labelFilePath
            // 
            resources.ApplyResources(this.labelFilePath, "labelFilePath");
            this.labelFilePath.AutoEllipsis = true;
            this.labelFilePath.Name = "labelFilePath";
            // 
            // FormFillWithData
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelFilePath);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.numericUpDownFileSize);
            this.Controls.Add(this.radioButtonZeroData);
            this.Controls.Add(this.radioButtonEmepty);
            this.Controls.Add(this.checkBoxForce);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFillWithData";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFileSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxForce;
        private System.Windows.Forms.RadioButton radioButtonEmepty;
        private System.Windows.Forms.RadioButton radioButtonZeroData;
        private System.Windows.Forms.NumericUpDown numericUpDownFileSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelFilePath;
    }
}