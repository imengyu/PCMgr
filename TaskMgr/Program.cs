using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PCMgr.Aero.TaskDialog;
using PCMgr.Lanuages;

namespace PCMgr
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main(string[]agrs)
        {
            FormMain.currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            FormMain.InitLanuage();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;

            bool run = true;

            if (!FormMain.MIsRunasAdmin())
                run = ShowNOADMINWarn();
            if (run && FormMain.MIs64BitOS())
                run = Show64Warn();

            if (run) Application.Run(new FormMain(agrs));
        }

        private static bool Show64Warn()
        {
            if (FormMain.GetConfigBool("X32Warning", "AppSetting", true))
            {
                bool has64 = FormMain.MIsFinded64();
                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("X64WarnTitle"), FormMain.DEFAPPTITLE);
                t.Content = LanuageMgr.GetStr("X64WarnText");
                t.ExpandedInformation = LanuageMgr.GetStr("X64WarnMoreText") + (has64 ? LanuageMgr.GetStr("X64WarnFinded64Text") : "");
                t.CommonIcon = TaskDialogIcon.Warning;
                t.VerificationText = LanuageMgr.GetStr("DoNotRemidMeLater");
                t.VerificationClick += T_TaskDialog_64Warn_VerificationClick;
                if (has64)
                {
                    CustomButton[] btns = new CustomButton[3];
                    btns[0] = new CustomButton(Result.Yes, LanuageMgr.GetStr("ContinueRun"));
                    btns[1] = new CustomButton(Result.No, LanuageMgr.GetStr("CancelRun"));
                    btns[2] = new CustomButton(Result.Abort, LanuageMgr.GetStr("Run64"));
                    t.CustomButtons = btns;
                }
                else
                {
                    CustomButton[] btns = new CustomButton[2];
                    btns[0] = new CustomButton(Result.Yes, LanuageMgr.GetStr("ContinueRun"));
                    btns[1] = new CustomButton(Result.No, LanuageMgr.GetStr("CancelRun"));
                    t.CustomButtons = btns;
                }
                t.CanBeMinimized = false;
                t.EnableHyperlinks = true;
                Results rs = t.Show();
                if (rs.CommonButton == Result.No) return false;
                else if (rs.CommonButton == Result.Abort)
                {
                    FormMain.MRun64();
                    return false;
                }
            }
            return true;
        }
        private static bool ShowNOADMINWarn()
        {
            if (FormMain.GetConfigBool("NOAdminWarning", "AppSetting", false))
            {
                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("NeedAdmin"), FormMain.DEFAPPTITLE, LanuageMgr.GetStr("RequestAdminText"));
                t.CommonIcon = TaskDialogIcon.Warning;
                t.VerificationText = LanuageMgr.GetStr("DoNotRemidMeLater");
                t.VerificationClick += T_TaskDialog_NOADMINWarn_VerificationClick;
                CustomButton[] btns = new CustomButton[3];
                btns[0] = new CustomButton(Result.Yes, LanuageMgr.GetStr("ContinueRun"));
                btns[1] = new CustomButton(Result.No, LanuageMgr.GetStr("CancelRun"));
                btns[2] = new CustomButton(Result.Abort, LanuageMgr.GetStr("RunAsAdmin"));
                t.CustomButtons = btns;
                t.CanBeMinimized = false;
                t.EnableHyperlinks = true;
                Results rs = t.Show();
                if (rs.CommonButton == Result.No) return false;
                else if (rs.CommonButton == Result.Abort)
                {
                    FormMain.MAppRebotAdmin();
                    return false;
                }
            }
            return true;
        }

        private static void T_TaskDialog_NOADMINWarn_VerificationClick(object sender, CheckEventArgs e)
        {
            if (e.IsChecked)
                FormMain.SetConfig("NOAdminWarning", "AppSetting", "FALSE");
            else FormMain.SetConfig("NOAdminWarning", "AppSetting", "TRUE");
        }
        private static void T_TaskDialog_64Warn_VerificationClick(object sender, CheckEventArgs e)
        {
            if (e.IsChecked)
                FormMain.SetConfig("X32Warning", "AppSetting", "FALSE");
            else FormMain.SetConfig("X32Warning", "AppSetting", "TRUE");
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            DialogResult d = MessageBox.Show(e.Exception.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (d == DialogResult.Abort)
                Environment.Exit(0);
        }

        public static int EntryPoint(string args)
        {
            Main(args.Split(' '));
            return 0;
        }
    }
}
