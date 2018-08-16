namespace PCMgr.WorkWindow
{
    partial class FormFileTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFileTool));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.btnChooseFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnCheckUsing = new System.Windows.Forms.Button();
            this.btnFroceDelete = new System.Windows.Forms.Button();
            this.btnFillWithData = new System.Windows.Forms.Button();
            this.labelFileInformation = new System.Windows.Forms.Label();
            this.btnDisplayFileInfo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBoxFilePath
            // 
            resources.ApplyResources(this.textBoxFilePath, "textBoxFilePath");
            this.textBoxFilePath.Name = "textBoxFilePath";
            // 
            // btnChooseFile
            // 
            resources.ApplyResources(this.btnChooseFile, "btnChooseFile");
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.UseVisualStyleBackColor = true;
            this.btnChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // btnCheckUsing
            // 
            resources.ApplyResources(this.btnCheckUsing, "btnCheckUsing");
            this.btnCheckUsing.Name = "btnCheckUsing";
            this.btnCheckUsing.UseVisualStyleBackColor = true;
            this.btnCheckUsing.Click += new System.EventHandler(this.btnCheckUsing_Click);
            // 
            // btnFroceDelete
            // 
            resources.ApplyResources(this.btnFroceDelete, "btnFroceDelete");
            this.btnFroceDelete.Name = "btnFroceDelete";
            this.btnFroceDelete.UseVisualStyleBackColor = true;
            this.btnFroceDelete.Click += new System.EventHandler(this.btnFroceDelete_Click);
            // 
            // btnFillWithData
            // 
            resources.ApplyResources(this.btnFillWithData, "btnFillWithData");
            this.btnFillWithData.Name = "btnFillWithData";
            this.btnFillWithData.UseVisualStyleBackColor = true;
            this.btnFillWithData.Click += new System.EventHandler(this.btnFillWithData_Click);
            // 
            // labelFileInformation
            // 
            resources.ApplyResources(this.labelFileInformation, "labelFileInformation");
            this.labelFileInformation.Name = "labelFileInformation";
            // 
            // btnDisplayFileInfo
            // 
            resources.ApplyResources(this.btnDisplayFileInfo, "btnDisplayFileInfo");
            this.btnDisplayFileInfo.Name = "btnDisplayFileInfo";
            this.btnDisplayFileInfo.UseVisualStyleBackColor = true;
            this.btnDisplayFileInfo.Click += new System.EventHandler(this.btnDisplayFileInfo_Click);
            // 
            // FormFileTool
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnDisplayFileInfo);
            this.Controls.Add(this.btnFillWithData);
            this.Controls.Add(this.btnFroceDelete);
            this.Controls.Add(this.btnCheckUsing);
            this.Controls.Add(this.btnChooseFile);
            this.Controls.Add(this.textBoxFilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelFileInformation);
            this.Name = "FormFileTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Button btnChooseFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnCheckUsing;
        private System.Windows.Forms.Button btnFroceDelete;
        private System.Windows.Forms.Button btnFillWithData;
        private System.Windows.Forms.Label labelFileInformation;
        private System.Windows.Forms.Button btnDisplayFileInfo;
    }
}