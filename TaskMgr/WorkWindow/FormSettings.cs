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

#if _X64_
            checkBoxEnableLoadDrvCallback.Enabled = false;
#endif
        }
        private void load_settings()
        {
            string setting_lg = FormMain.GetConfig("Lanuage", "AppSetting");
            switch (setting_lg)
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

            checkBoxTop.Checked = FormMain.GetConfigBool("TopMost", "AppSetting", false);
            checkBoxCloseHide.Checked = FormMain.GetConfigBool("CloseHideToNotfication", "AppSetting", false);
            checkBoxSelfProtect.Checked = FormMain.GetConfigBool("SelfProtect", "AppSetting", false);

            labelDriverLoadStatus.Text = FormMain.MCanUseKernel() ? "Driver Loaded" : "Driver not load";

            checkBoxAutoLoadDriver.Checked = FormMain.GetConfigBool("LoadKernelDriver", "Configure", false);
            checkBoxHighLightNoSystetm.Checked = FormMain.GetConfigBool("HighLightNoSystetm", "Configure", false);
            checkBoxShowDebugWindow.Checked = FormMain.GetConfigBool("ShowDebugWindow", "Configure", true);
            checkBoxNTOSPDB.Checked = FormMain.GetConfigBool("UseKrnlPDB", "Configure", true);

            textBoxTitle.Text = FormMain.GetConfig("Title", "AppSetting", "");

            checkBoxShowHotKey.Checked = FormMain.GetConfigBool("HotKey", "AppSetting", true);
            comboBoxShowHotKey1.SelectedItem = FormMain.GetConfig("HotKey1", "AppSetting", "(None)");
            comboBoxShowHotKey2.SelectedItem = FormMain.GetConfig("HotKey2", "AppSetting", "T");

            checkBoxUseMyDbgView.Checked = FormMain.GetConfigBool("LogDbgPrint", "Configure", true);

            string terproc = FormMain.GetConfig("TerProcFun", "Configure", "PspTerProc");
            radioButtonPspTerProc.Checked = terproc == "PspTerProc";
            radioButtonApcPspTerProc.Checked = terproc == "ApcPspTerProc";
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

            if (radioButtonPspTerProc.Checked)
                FormMain.SetConfig("TerProcFun", "Configure", "PspTerProc");
            else if (radioButtonPspTerProc.Checked)
                FormMain.SetConfig("TerProcFun", "Configure", "ApcPspTerProc");

            FormMain.SetConfigBool("LogDbgPrint", "Configure", checkBoxUseMyDbgView.Checked);
            FormMain.SetConfigBool("UseKrnlPDB", "Configure", checkBoxNTOSPDB.Checked);

            FormMain.SetConfigBool("LoadKernelDriver", "Configure", checkBoxAutoLoadDriver.Checked);
            FormMain.SetConfigBool("HighLightNoSystetm", "Configure", checkBoxHighLightNoSystetm.Checked);
            FormMain.SetConfigBool("ShowDebugWindow", "Configure", checkBoxShowDebugWindow.Checked);
            FormMain.SetConfigBool("HotKey", "AppSetting", checkBoxShowHotKey.Checked);
            FormMain.SetConfigBool("SelfProtect", "AppSetting", checkBoxSelfProtect.Checked);
            FormMain.SetConfig("Title", "AppSetting", textBoxTitle.Text);
            if (textBoxTitle.Text != "")
                FormMain.Instance.Text = textBoxTitle.Text;
            else FormMain.Instance.Text = FormMain.str_AppTitle;
            FormMain.SetConfig("HotKey1", "AppSetting", comboBoxShowHotKey1.SelectedItem.ToString());
            FormMain.SetConfig("HotKey2", "AppSetting", comboBoxShowHotKey2.SelectedItem.ToString());

            Close();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkBoxTop_CheckedChanged(object sender, EventArgs e)
        {
            FormMain.AppHWNDSendMessage(FormMain.WM_COMMAND, new IntPtr(40117), IntPtr.Zero);
            //FormMain.SetConfigBool("TopMost", "AppSetting", checkBoxTop.Checked);
        }
        private void checkBoxCloseHide_CheckedChanged(object sender, EventArgs e)
        {
            FormMain.AppHWNDSendMessage(FormMain.WM_COMMAND, new IntPtr(40120), IntPtr.Zero);
            //FormMain.SetConfigBool("CloseHideToNotfication", "AppSetting", checkBoxCloseHide.Checked);
        }

        private void checkBoxCannotCreateProc_CheckedChanged(object sender, EventArgs e)
        {
            if (!FormMain.M_SU_SetKrlMonSet_CreateProcess(checkBoxCannotCreateProc.Checked))
                MessageBox.Show(FormMain.str_failed);
        }
        private void checkBoxCannotCreateThread_CheckedChanged(object sender, EventArgs e)
        {
            if (!FormMain.M_SU_SetKrlMonSet_CreateThread(checkBoxCannotCreateThread.Checked))
                MessageBox.Show(FormMain.str_failed);
        }
        private void checkBoxCannotLoadDriver_CheckedChanged(object sender, EventArgs e)
        {
            if (!FormMain.M_SU_SetKrlMonSet_LoadImage(checkBoxCannotLoadDriver.Checked))
                MessageBox.Show(FormMain.str_failed);
        }
    }
}
