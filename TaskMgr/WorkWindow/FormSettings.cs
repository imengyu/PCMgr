using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PCMgr.Lanuages;
using PCMgr;

namespace PCMgr.WorkWindow
{
    public partial class FormSettings : Form
    {
        public FormSettings(FormMain m)
        {
            InitializeComponent();
            main = m;
        }

        private bool load = true;
        private FormMain main;

        private void ApplyResource()
        {
            ComponentResourceManager res = new ComponentResourceManager(typeof(FormMain));
            foreach (Control ctl in Controls)
            {
                res.ApplyResources(ctl, ctl.Name);
            }
            ResumeLayout(false);
            PerformLayout();
            res.ApplyResources(this, "$this");
        }
        private void comboBox_lg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!load)
                MessageBox.Show(LanuageMgr.GetStr("LanuageChangedTip"), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            load_settings();
            load = false;
        }
        private void load_settings()
        {
            string setting_lg = FormMain.GetConfig("Lanuage", "AppSetting");
            switch(setting_lg)
            {
                case "zh":
                    comboBox_lg.SelectedIndex = 0;
                    break;
                case "zh-Hant":
                    comboBox_lg.SelectedIndex = 1;
                    break;
                case "en":
                    comboBox_lg.SelectedIndex = 2;
                    break;

            }

        }
        private void save_settings()
        {
            switch (comboBox_lg.SelectedIndex)
            {
                case 0:
                    FormMain.SetConfig("Lanuage", "AppSetting", "zh");
                    break;
                case 1:
                    FormMain.SetConfig("Lanuage", "AppSetting", "zh-Hant");
                    break;
                case 2:
                    FormMain.SetConfig("Lanuage", "AppSetting", "en");
                    break;

            }
            Close();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            save_settings();
        }
    }
}
