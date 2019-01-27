using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PCMgrUpdate
{
    public partial class FormMain : Form
    {
        public FormMain(string[] agrs)
        {
            InitializeComponent();
            CurrentUpdateWorker = new UpdateWorker();
            CurrentUpdateWorker.ReadAgrs(agrs);
        }
   
        public string CurrentVersion { get; private set; }
        public void SetMainStatus(string s)
        {
            mainStatus.Invoke(new Action(delegate { mainStatus.Text = s; }));
        }
        public void SetMainPrecentText(string s)
        {
            mainPrecent.Invoke(new Action(delegate { mainPrecent.Text = s; }));
        }
        public void SetMainProgress(int progress, ProgressBarStyle style = ProgressBarStyle.Blocks)
        {
            mainProgress.Invoke(new Action(delegate
            {
                mainProgress.Value = progress;
                if (mainProgress.Style != style) mainProgress.Style = style;
            }));
        }
        public void SetMainCancelBtnEnable(bool enable)
        {
            mainCancelBtn.Invoke(new Action(delegate { mainCancelBtn.Enabled = enable; }));
        }

        private UpdateWorker CurrentUpdateWorker = null;

        private void FormMain_Load(object sender, EventArgs e)
        {
#if _X64_
            if (File.Exists(Application.StartupPath + "\\PCMgr64.dll"))
#else
            if (File.Exists(Application.StartupPath + "\\PCMgr32.dll"))
#endif
            {
                try
                {
                    CurrentVersion = Marshal.PtrToStringUni(NativeMethods.MAppGetVersion());
                    Text = Text + "  -  版本 : " + CurrentVersion + " / " + Marshal.PtrToStringUni(NativeMethods.MAppGetBulidDate());
                    //NativeMethods.FreeLibrary(NativeMethods.MAppGetCoreModulHandle());
                }
                catch
                {
                    Text = Text + "  -  修复 没有找到软件安装目录 ";
                    CurrentVersion = "0.0.0.0";
                    CurrentUpdateWorker.IsFix = true;
                }
            }
            else
            {
                Text = Text + "  -  在线安装 ";
                CurrentVersion = "0.0.0.0";
                CurrentUpdateWorker.IsInstall = true;
            }
        }
        private void FormMain_Shown(object sender, EventArgs e)
        {
            CurrentUpdateWorker.RunUpdate(this);
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CurrentUpdateWorker.Updateing && !CurrentUpdateWorker.CanCancel)
                if (MessageBox.Show("是否取消更新？", "提示 - PC Manager 更新工具") == DialogResult.Yes)
                    e.Cancel = true;
        }

        private void mainCancelBtn_Click(object sender, EventArgs e)
        {
            if (CurrentUpdateWorker.Updateing)
            {
                if (!CurrentUpdateWorker.CanCancel) mainCancelBtn.Enabled = false;
                else
                {
                    if (MessageBox.Show("是否取消更新？", "提示 - PC Manager 更新工具") == DialogResult.Yes)
                    {
                        CurrentUpdateWorker.CancelUpdate();
                        Close();
                    }
                }
            }
            else Close();
        }
    }
}
