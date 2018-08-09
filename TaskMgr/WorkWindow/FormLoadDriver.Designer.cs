namespace PCMgr.WorkWindow
{
    partial class FormLoadDriver
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLoadDriver));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDriverPath = new System.Windows.Forms.TextBox();
            this.buttonChoose = new System.Windows.Forms.Button();
            this.buttonUnLoad = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxServName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxDrvServDsb = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "sys";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // buttonLoad
            // 
            resources.ApplyResources(this.buttonLoad, "buttonLoad");
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBoxDriverPath
            // 
            resources.ApplyResources(this.textBoxDriverPath, "textBoxDriverPath");
            this.textBoxDriverPath.Name = "textBoxDriverPath";
            this.textBoxDriverPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBoxDriverPath_DragDrop);
            this.textBoxDriverPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBoxDriverPath_DragEnter);
            // 
            // buttonChoose
            // 
            resources.ApplyResources(this.buttonChoose, "buttonChoose");
            this.buttonChoose.Name = "buttonChoose";
            this.buttonChoose.UseVisualStyleBackColor = true;
            this.buttonChoose.Click += new System.EventHandler(this.buttonChoose_Click);
            // 
            // buttonUnLoad
            // 
            resources.ApplyResources(this.buttonUnLoad, "buttonUnLoad");
            this.buttonUnLoad.Name = "buttonUnLoad";
            this.buttonUnLoad.UseVisualStyleBackColor = true;
            this.buttonUnLoad.Click += new System.EventHandler(this.buttonUnLoad_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // textBoxServName
            // 
            resources.ApplyResources(this.textBoxServName, "textBoxServName");
            this.textBoxServName.Name = "textBoxServName";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // textBoxDrvServDsb
            // 
            resources.ApplyResources(this.textBoxDrvServDsb, "textBoxDrvServDsb");
            this.textBoxDrvServDsb.Name = "textBoxDrvServDsb";
            // 
            // FormLoadDriver
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxDrvServDsb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxServName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonUnLoad);
            this.Controls.Add(this.buttonChoose);
            this.Controls.Add(this.textBoxDriverPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonLoad);
            this.MaximizeBox = false;
            this.Name = "FormLoadDriver";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDriverPath;
        private System.Windows.Forms.Button buttonChoose;
        private System.Windows.Forms.Button buttonUnLoad;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxServName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxDrvServDsb;
    }
}