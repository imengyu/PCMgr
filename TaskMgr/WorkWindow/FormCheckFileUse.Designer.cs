namespace PCMgr.WorkWindow
{
    partial class FormCheckFileUse
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCheckFileUse));
            this.listViewUsing = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnClose = new System.Windows.Forms.Button();
            this.btnReleaseAll = new System.Windows.Forms.Button();
            this.labelFileInfo = new System.Windows.Forms.Label();
            this.btnRelease = new System.Windows.Forms.Button();
            this.btnRefesh = new System.Windows.Forms.Button();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewUsing
            // 
            resources.ApplyResources(this.listViewUsing, "listViewUsing");
            this.listViewUsing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listViewUsing.MultiSelect = false;
            this.listViewUsing.Name = "listViewUsing";
            this.listViewUsing.UseCompatibleStateImageBehavior = false;
            this.listViewUsing.View = System.Windows.Forms.View.Details;
            this.listViewUsing.SelectedIndexChanged += new System.EventHandler(this.listViewUsing_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnReleaseAll
            // 
            resources.ApplyResources(this.btnReleaseAll, "btnReleaseAll");
            this.btnReleaseAll.Name = "btnReleaseAll";
            this.btnReleaseAll.UseVisualStyleBackColor = true;
            this.btnReleaseAll.Click += new System.EventHandler(this.btnReleaseAll_Click);
            // 
            // labelFileInfo
            // 
            resources.ApplyResources(this.labelFileInfo, "labelFileInfo");
            this.labelFileInfo.Name = "labelFileInfo";
            // 
            // btnRelease
            // 
            resources.ApplyResources(this.btnRelease, "btnRelease");
            this.btnRelease.Name = "btnRelease";
            this.btnRelease.UseVisualStyleBackColor = true;
            this.btnRelease.Click += new System.EventHandler(this.btnRelease_Click);
            // 
            // btnRefesh
            // 
            resources.ApplyResources(this.btnRefesh, "btnRefesh");
            this.btnRefesh.Name = "btnRefesh";
            this.btnRefesh.UseVisualStyleBackColor = true;
            this.btnRefesh.Click += new System.EventHandler(this.btnRefesh_Click);
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // FormCheckFileUse
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRefesh);
            this.Controls.Add(this.btnRelease);
            this.Controls.Add(this.labelFileInfo);
            this.Controls.Add(this.btnReleaseAll);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.listViewUsing);
            this.Name = "FormCheckFileUse";
            this.Load += new System.EventHandler(this.FormCheckFileUse_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewUsing;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnReleaseAll;
        private System.Windows.Forms.Label labelFileInfo;
        private System.Windows.Forms.Button btnRelease;
        private System.Windows.Forms.Button btnRefesh;
        private System.Windows.Forms.ColumnHeader columnHeader5;
    }
}