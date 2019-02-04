using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PCMgr.Lanuages;
using static PCMgr.NativeMethods;

namespace PCMgr.WorkWindow
{
    public partial class FormSettings : Form
    {
        public FormSettings(FormMain m)
        {
            InitializeComponent();
            main = m;
        }

        public static Font LoadFontSettingForUI(Control c)
        {
            try
            {
                string font = GetConfig("Font", "AppSetting", "");
                if (font != "")
                    c.Font = (Font)new FontConverter().ConvertFromString(font);
                return c.Font;
            }
            catch
            {
                return null;
            }
        }

        private bool fontchanged = false;
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
            string setting_lg = GetConfig("Lanuage", "AppSetting");
            switch (setting_lg)
            {
                case "zh":
                    comboBox_lg.SelectedIndex = 0;
                    break;
                case "en":
                    comboBox_lg.SelectedIndex = 1;
                    break;

            }
           

            checkBoxAbortShutdown.Checked = GetConfigBool("AbortShutdown", "AppSetting", false);
            checkBoxTop.Checked = GetConfigBool("TopMost", "AppSetting", false);
            checkBoxCloseHide.Checked = GetConfigBool("CloseHideToNotfication", "AppSetting", false);
            checkBoxSelfProtect.Checked = GetConfigBool("SelfProtect", "AppSetting", false);

            labelDriverLoadStatus.Text = MCanUseKernel() ? "Driver Loaded" : "Driver not load";

            checkBoxAutoLoadDriver.Checked = GetConfigBool("LoadKernelDriver", "Configure", false);
            checkBoxHighLightNoSystetm.Checked = GetConfigBool("HighLightNoSystetm", "Configure", false);
            checkBoxShowDebugWindow.Checked = GetConfigBool("ShowDebugWindow", "Configure", false);
            checkBoxNTOSPDB.Checked = GetConfigBool("UseKrnlPDB", "Configure", true);

            textBoxTitle.Text = GetConfig("Title", "AppSetting", "");

            checkBoxShowHotKey.Checked = GetConfigBool("HotKey", "AppSetting", true);
            comboBoxShowHotKey1.SelectedItem = GetConfig("HotKey1", "AppSetting", "(None)");
            comboBoxShowHotKey2.SelectedItem = GetConfig("HotKey2", "AppSetting", "T");

            checkBoxUseMyDbgView.Checked = GetConfigBool("LogDbgPrint", "Configure", true);

            string terproc = GetConfig("TerProcFun", "Configure", "PspTerProc");
            radioButtonPspTerProc.Checked = terproc == "PspTerProc";
            radioButtonApcPspTerProc.Checked = terproc == "ApcPspTerProc";
        }
        private void save_settings()
        {
            switch (comboBox_lg.SelectedIndex)
            {
                case 0:
                    SetConfig("Lanuage", "AppSetting", "zh");
                    break;
                case 1:
                    SetConfig("Lanuage", "AppSetting", "en");
                    break;

            }
            SetConfigBool("AbortShutdown", "AppSetting", checkBoxAbortShutdown.Checked);
            if (radioButtonPspTerProc.Checked)
                SetConfig("TerProcFun", "Configure", "PspTerProc");
            else if (radioButtonPspTerProc.Checked)
                SetConfig("TerProcFun", "Configure", "ApcPspTerProc");

            SetConfigBool("LogDbgPrint", "Configure", checkBoxUseMyDbgView.Checked);
            SetConfigBool("UseKrnlPDB", "Configure", checkBoxNTOSPDB.Checked);

            SetConfigBool("LoadKernelDriver", "Configure", checkBoxAutoLoadDriver.Checked);
            SetConfigBool("HighLightNoSystetm", "Configure", checkBoxHighLightNoSystetm.Checked);
            SetConfigBool("ShowDebugWindow", "Configure", checkBoxShowDebugWindow.Checked);
            SetConfigBool("HotKey", "AppSetting", checkBoxShowHotKey.Checked);
            SetConfigBool("SelfProtect", "AppSetting", checkBoxSelfProtect.Checked);
            SetConfig("Title", "AppSetting", textBoxTitle.Text);
            SetConfig("HotKey1", "AppSetting", comboBoxShowHotKey1.SelectedItem.ToString());
            SetConfig("HotKey2", "AppSetting", comboBoxShowHotKey2.SelectedItem.ToString());
            if(fontchanged) SetConfig("Font", "AppSetting", new FontConverter().ConvertToString(tabControl1.Font));

            Close();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkBoxTop_CheckedChanged(object sender, EventArgs e)
        {
            FormMain.AppHWNDSendMessage(Win32.WM_COMMAND, new IntPtr(40117), IntPtr.Zero);
            //SetConfigBool("TopMost", "AppSetting", checkBoxTop.Checked);
        }
        private void checkBoxCloseHide_CheckedChanged(object sender, EventArgs e)
        {
            FormMain.AppHWNDSendMessage(Win32.WM_COMMAND, new IntPtr(40120), IntPtr.Zero);
            //SetConfigBool("CloseHideToNotfication", "AppSetting", checkBoxCloseHide.Checked);
        }
        private void checkBoxCannotCreateProc_CheckedChanged(object sender, EventArgs e)
        {
            if (!M_SU_SetKrlMonSet_CreateProcess(checkBoxCannotCreateProc.Checked))
                MessageBox.Show(LanuageFBuffers.Str_Failed);
        }
        private void checkBoxCannotCreateThread_CheckedChanged(object sender, EventArgs e)
        {
            if (!M_SU_SetKrlMonSet_CreateThread(checkBoxCannotCreateThread.Checked))
                MessageBox.Show(LanuageFBuffers.Str_Failed);
        }
        private void checkBoxCannotLoadDriver_CheckedChanged(object sender, EventArgs e)
        {
            if (!M_SU_SetKrlMonSet_LoadImage(checkBoxCannotLoadDriver.Checked))
                MessageBox.Show(LanuageFBuffers.Str_Failed);
        }
        private void checkBoxAutoLoadDriver_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoLoadDriver.Checked)
            {
                if (MAppWorkCall3(155) != 1)
                    checkBoxAutoLoadDriver.Checked = false;
            }
        }

        private void btnChooseFont_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                fontchanged = true;
                Font = fontDialog1.Font;
            }
        }
    }
}
