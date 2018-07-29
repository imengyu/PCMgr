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
            if (!System.IO.File.Exists(Application.StartupPath + "\\" + FormMain.COREDLLNAME))
            {
                MessageBox.Show("Failed to find " + FormMain.COREDLLNAME + " !", "PCMgr - ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FormMain.currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            FormMain.M_LOG_Init(FormMain.GetConfigBool("ShowDebugWindow", "Configure", false));
            FormMain.InitLanuage();
            FormMain.Log("CS App starting : " + FormMain.currentProcessName + ".exe");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;

            bool run = true;

            if (agrs.Length > 0)
                run = MainRunAgrs(agrs);

            if (run && FormMain.MAppStartTest())
                run = ShowRun2Warn();

            if (run && !FormMain.MIsRunasAdmin())
                run = ShowNOADMINWarn();
#if !_X64_
            if (run && FormMain.MIs64BitOS())
                run = Show64Warn();
#endif

            if (run) Application.Run(new FormMain(agrs));
            else FormMain.Log("Cancel run.");
        }
        public static bool MainRunAgrs(string[] agrs)
        {
            if (agrs.Length > 0)
            {
                FormMain.Log("MainRunAgrs 0 : " + agrs[0]);
                switch(agrs[0])
                {
                    case "reboot":
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleReboot"), FormMain.str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show().CommonButton == Result.Yes)
                            {
                                FormMain.MGetPrivileges();
                                FormMain.MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                            }
                            return false;
                        }
                    case "shutdown":
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleShutdown"), FormMain.str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show().CommonButton == Result.Yes)
                            {
                                FormMain.MGetPrivileges();
                                FormMain.MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                            }
                            return false;
                        }
                    case "vmodul":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if(uint.TryParse(agrs[1], out pid))
                            {
                                FormMain.MAppVProcessModuls(pid, IntPtr.Zero, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                        }
                        break;
                    case "vthread":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if (uint.TryParse(agrs[1], out pid))
                            {
                                FormMain.MAppVProcessThreads(pid, IntPtr.Zero, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                        }
                        break;
                    case "vwin":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if (uint.TryParse(agrs[1], out pid))
                            {
                                FormMain.MAppVProcessWindows(pid, IntPtr.Zero, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                        }
                        break;
                    case "vhandle":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if (uint.TryParse(agrs[1], out pid))
                            {
                                new WorkWindow.FormVHandles(pid, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                        }
                        break;
                    case "spy":
                        FormMain.Log("MainRunAgrs run spy ");
                        new WorkWindow.FormSpyWindow(FormMain.GetDesktopWindow());
                        return false;
                }
            }
            return true;
        }

        private static bool ShowRun2Warn()
        {
            bool has64 = FormMain.MIsFinded64();
            TaskDialog t = new TaskDialog("", FormMain.str_AppTitle);
            t.Content = LanuageMgr.GetStr("Run2WarnText");
            t.CommonIcon = TaskDialogIcon.None;
            CustomButton[] btns = new CustomButton[3];
            btns[0] = new CustomButton(Result.Yes, LanuageMgr.GetStr("ContinueRun"));
            btns[1] = new CustomButton(Result.No, LanuageMgr.GetStr("CancelRun"));
            btns[2] = new CustomButton(Result.Ignore, LanuageMgr.GetStr("Run2KillOld"));
            t.CustomButtons = btns;
            t.UseCommandLinks = true;
            t.CanBeMinimized = false;
            t.EnableHyperlinks = true;
            Results rs = t.Show();
            if (rs.CommonButton == Result.No) return false;
            else if (rs.CommonButton == Result.Yes) return true;
            else if (rs.CommonButton == Result.Ignore)
            {
                if (FormMain.MAppKillOld(FormMain.currentProcessName + ".exe"))
                    TaskDialog.Show(LanuageMgr.GetStr("KillOldSuccess"), FormMain.str_AppTitle);
                else TaskDialog.Show(LanuageMgr.GetStr("KillOldFailed"), FormMain.str_AppTitle);
                return true;
            }
            return true;
        }
        private static bool Show64Warn()
        {
            if (FormMain.GetConfigBool("X32Warning", "AppSetting", true))
            {
                bool has64 = FormMain.MIsFinded64();
                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("X64WarnTitle"), FormMain.str_AppTitle);
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
                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("NeedAdmin"), FormMain.str_AppTitle, LanuageMgr.GetStr("RequestAdminText"));
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
            FormMain.FLogErr(e.Exception.ToString());

            DialogResult d = MessageBox.Show(e.Exception.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (d == DialogResult.Abort)
                Environment.Exit(0);
        }
    }
}
