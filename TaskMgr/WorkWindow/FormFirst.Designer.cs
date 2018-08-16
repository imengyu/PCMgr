namespace PCMgr.WorkWindow
{
    partial class FormFirst
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFirst));
            this.wizardControl1 = new AeroWizard.WizardControl();
            this.wizardPageStart = new AeroWizard.WizardPage();
            this.commandLink1 = new PCMgr.Aero.CommandLink();
            this.commandLink2 = new PCMgr.Aero.CommandLink();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).BeginInit();
            this.wizardPageStart.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.wizardControl1, "wizardControl1");
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.Pages.Add(this.wizardPageStart);
            // 
            // wizardPageStart
            // 
            this.wizardPageStart.AllowBack = false;
            this.wizardPageStart.Controls.Add(this.commandLink2);
            this.wizardPageStart.Controls.Add(this.commandLink1);
            this.wizardPageStart.Name = "wizardPageStart";
            resources.ApplyResources(this.wizardPageStart, "wizardPageStart");
            // 
            // commandLink1
            // 
            resources.ApplyResources(this.commandLink1, "commandLink1");
            this.commandLink1.Name = "commandLink1";
            this.commandLink1.UseVisualStyleBackColor = true;
            // 
            // commandLink2
            // 
            resources.ApplyResources(this.commandLink2, "commandLink2");
            this.commandLink2.Name = "commandLink2";
            this.commandLink2.UseVisualStyleBackColor = true;
            // 
            // FormFirst
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wizardControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormFirst";
            this.Load += new System.EventHandler(this.FormFirst_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).EndInit();
            this.wizardPageStart.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizardControl1;
        private AeroWizard.WizardPage wizardPageStart;
        private Aero.CommandLink commandLink2;
        private Aero.CommandLink commandLink1;
    }
}