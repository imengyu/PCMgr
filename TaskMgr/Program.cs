using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            FormMain.cfgFilePath = Marshal.PtrToStringUni(NativeMethods.M_CFG_GetCfgFilePath());
            NativeMethods.Log("cfgFilePath : " + FormMain.cfgFilePath);
            FormMain.InitLanuage();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;

            bool run = true;

            if (agrs.Length > 0)
                run = MainRunAgrs(agrs);

            if (run && NativeMethods.MAppStartTest() && !TryCloseLastApp())
                run = ShowRun2Warn();

            if (run && !NativeMethods.MIsRunasAdmin())
                run = ShowNOADMINWarn();
#if !_X64_
            if (run && NativeMethods.MIs64BitOS())
                run = Show64Warn();
#endif
            
            //Application.Run(new WorkWindow.FormAbout());
            if (run) Application.Run(new FormMain(agrs));
            else NativeMethods.Log("Cancel run.");
        }

        private static bool MainRunAgrs(string[] agrs)
        {
            if (agrs.Length > 0)
            {
                NativeMethods.Log("MainRunAgrs 0 : " + agrs[0]);
                switch(agrs[0])
                {
                    case "reboot":
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleReboot"), FormMain.str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show().CommonButton == Result.Yes)
                            {
                                NativeMethods.MGetPrivileges();
                                NativeMethods.MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                            }
                            return false;
                        }
                    case "shutdown":
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleShutdown"), FormMain.str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show().CommonButton == Result.Yes)
                            {
                                NativeMethods.MGetPrivileges();
                                NativeMethods.MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                            }
                            return false;
                        }
                    case "vmodul":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if(uint.TryParse(agrs[1], out pid))
                            {
                                NativeMethods.MAppVProcessModuls(pid, IntPtr.Zero, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                            else NativeMethods.Log("Invalid args[1] : " + agrs[1]);
                        }
                        break;
                    case "vthread":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if (uint.TryParse(agrs[1], out pid))
                            {
                                NativeMethods.MAppVProcessThreads(pid, IntPtr.Zero, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                            else NativeMethods.Log("Invalid args[1] : " + agrs[1]);
                        }
                        break;
                    case "vwin":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if (uint.TryParse(agrs[1], out pid))
                            {
                                NativeMethods.MAppVProcessWindows(pid, IntPtr.Zero, agrs.Length > 2 ? agrs[2] : "");
                                return false;
                            }
                            else NativeMethods.Log("Invalid args[1] : " + agrs[1]);
                        }
                        break;
                    case "vhandle":
                        if (agrs.Length > 1)
                        {
                            uint pid = 0;
                            if (uint.TryParse(agrs[1], out pid))
                            {
                                Application.Run(new WorkWindow.FormVHandles(pid, agrs.Length > 2 ? agrs[2] : ""));
                                return false;
                            }
                            else NativeMethods.Log("Invalid args[1] : " + agrs[1]);
                        }
                        break;
                    case "kda":
                        NativeMethods.Log("MainRunAgrs run kda ");
                        Application.Run(new WorkWindow.FormKDA());
                        return false;
                }
            }
            return true;
        }

        private static bool TryCloseLastApp()
        {
            string lastTitle = NativeMethods.GetConfig("LastWindowTitle", "AppSetting");
            if (lastTitle != "")
                return NativeMethods.MAppStartTryCloseLastApp(lastTitle);
            return false;
        }
        private static bool ShowRun2Warn()
        {
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
                if (NativeMethods.MAppKillOld(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe"))
                {
                    TaskDialog.Show(LanuageMgr.GetStr("KillOldSuccess"), FormMain.str_AppTitle);
                    NativeMethods.MAppStartTest();
                }
                else TaskDialog.Show(LanuageMgr.GetStr("KillOldFailed"), FormMain.str_AppTitle);
                return true;
            }
            return true;
        }
        private static bool Show64Warn()
        {
            if (NativeMethods.GetConfigBool("X32Warning", "AppSetting", true))
            {

                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("X64WarnTitle"), FormMain.str_AppTitle);
                t.Content = LanuageMgr.GetStr("X64WarnText");
                t.CommonIcon = TaskDialogIcon.Warning;
                t.VerificationText = LanuageMgr.GetStr("DoNotRemidMeLater");
                t.VerificationClick += T_TaskDialog_64Warn_VerificationClick;

                CustomButton[] btns = new CustomButton[2];
                btns[0] = new CustomButton(Result.Yes, LanuageMgr.GetStr("ContinueRun"));
                btns[1] = new CustomButton(Result.No, LanuageMgr.GetStr("CancelRun"));
                t.CustomButtons = btns;
                t.CanBeMinimized = false;
                t.EnableHyperlinks = true;
                Results rs = t.Show();
                if (rs.CommonButton == Result.No) return false;
            }
            return true;
        }
        private static bool ShowNOADMINWarn()
        {
            if (NativeMethods.GetConfigBool("NOAdminWarning", "AppSetting", false))
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
                    NativeMethods.MAppRebotAdmin();
                    return false;
                }
            }
            return true;
        }

        private static void T_TaskDialog_NOADMINWarn_VerificationClick(object sender, CheckEventArgs e)
        {
            NativeMethods.SetConfigBool("NOAdminWarning", "AppSetting", !e.IsChecked);
        }
        private static void T_TaskDialog_64Warn_VerificationClick(object sender, CheckEventArgs e)
        {
            NativeMethods.SetConfigBool("X32Warning", "AppSetting", !e.IsChecked);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            NativeMethods.FLogErr(e.Exception.ToString());

            DialogResult d = MessageBox.Show(e.Exception.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (d == DialogResult.Abort)
                Environment.Exit(0);
        }
    
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppMainGetArgs([MarshalAs(UnmanagedType.LPWStr)]string cmdline);
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MAppMainGetArgsStr(int index);
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MAppMainGetArgsFreeAll();

        public static int ProgramEntry2()
        {
            return ProgramEntry(NativeMethods.Win32.GetCommandLineW());
        }
        public static int ProgramEntry(string cmdline)
        {
            int argscount = MAppMainGetArgs(cmdline);
            List<string> agrs = new List<string>();
            for (int i = 0; i < argscount; i++)
                agrs.Add(Marshal.PtrToStringUni(MAppMainGetArgsStr(i)));
            MAppMainGetArgsFreeAll();
            agrs.RemoveAt(0);
            Main(agrs.ToArray());
            return 0;
        }
    }
}
