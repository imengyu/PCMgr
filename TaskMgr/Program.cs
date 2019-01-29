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
            LanuageMgr.InitLanuage();

            NativeMethods.MGetPrivileges2();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool run = true;
            //Agrs
            if (agrs.Length > 0)
                run = MainRunAgrs(agrs);

            //Start Warns
            if (run && !NativeMethods.MIsRunasAdmin())
                run = ShowNOADMINWarn();
#if !_X64_
            if (run && NativeMethods.MIs64BitOS())
                run = Show64Warn();
#endif
            //if (run) Application.Run(new FormSL(agrs));
            if (run) Application.Run(new FormMain(agrs));
        }

        private static bool MainRunAgrs(string[] agrs)
        {
            if (agrs.Length > 0)
            {
                NativeMethods.Log("MainRunAgrs 0 : " + agrs[0]);
                if (agrs[0].StartsWith("-")) agrs[0] = agrs[0].Remove(0, 1);
                switch (agrs[0])
                {
                    case "reboot":
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleReboot"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show().CommonButton == Result.Yes)
                            {
                                NativeMethods.MGetPrivileges();
                                NativeMethods.MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                            }
                            return false;
                        }
                    case "shutdown":
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleShutdown"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
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
                    case "vexp":
                        if (agrs.Length > 1)
                        {
                            if (NativeMethods.MFM_FileExist(agrs[1]))
                            {
                                IntPtr file = Marshal.StringToHGlobalUni(agrs[1]);
                                NativeMethods.MAppWorkCall3(168, IntPtr.Zero, file);
                                Marshal.FreeHGlobal(file);
                                return false;
                            }
                            else NativeMethods.Log("Invalid args[1] : " + agrs[1] + " File not exists");
                        }
                        break;
                    case "vimp":
                        if (agrs.Length > 1)
                        {
                            if (NativeMethods.MFM_FileExist(agrs[1]))
                            {
                                IntPtr file = Marshal.StringToHGlobalUni(agrs[1]);
                                NativeMethods.MAppWorkCall3(169, IntPtr.Zero, file);
                                Marshal.FreeHGlobal(file);
                                return false;
                            }
                            else NativeMethods.Log("Invalid args[1] : " + agrs[1] + " File not exists");
                        }
                        break;
                    case "test":
                        {
                            Application.Run(new WorkWindow.FormTest());
                            return false;
                        }
                    case "test2":
                        {
                            LanuageMgr.InitLanuage();
                            return false;
                        }
                    case "about":
                        {
                            Application.Run(new WorkWindow.FormAbout());
                            return false;
                        }
                }
            }
            return true;
        }

        #region Start Warn

        private static bool Show64Warn()
        {
            if (NativeMethods.GetConfigBool("X32Warning", "AppSetting", true))
            {
                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("X64WarnTitle"), LanuageFBuffers.Str_AppTitle);
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
                TaskDialog t = new TaskDialog(LanuageMgr.GetStr("NeedAdmin"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("RequestAdminText"));
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

        #endregion

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                NativeMethods.FLogErr(ex.ToString());
                DialogResult d = MessageBox.Show("An exception occurs, click \"Abort\" to forec exit process immediately. \n" + ex.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                if (d == DialogResult.Abort) Environment.Exit(0);
            }
            else
            {
                DialogResult d = MessageBox.Show("An exception occurs, causing the program to stop running. Do you want to generate an error report?", "Exception ! ", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (d == DialogResult.No)
                    Environment.Exit(0);
                else
                {

                }
            }
        }
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            NativeMethods.FLogErr(e.Exception.ToString());

            DialogResult d = MessageBox.Show("An exception occurs, click \"Abort\" to forec exit process immediately. \n" + e.Exception.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (d == DialogResult.Abort) Environment.Exit(0);
        }

        #region Program Entry

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppMainGetArgs([MarshalAs(UnmanagedType.LPWStr)]string cmdline);
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MAppMainGetArgsStr(int index);
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MAppMainGetArgsFreeAll();

        [STAThread]
        public static int ProgramEntry(string cmdline)
        {
            //Set STAThread
            System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
            //Handle Exception
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //开始的暂停调试功能
            if (NativeMethods.GetConfigBool("BreakInStart", "AppSetting"))
                MessageBox.Show("Program Entry break , you can attatch to debugger now.");

            int argscount = MAppMainGetArgs(cmdline);
            List<string> agrs = new List<string>();
            for (int i = 0; i < argscount; i++)
                agrs.Add(Marshal.PtrToStringUni(MAppMainGetArgsStr(i)));
            MAppMainGetArgsFreeAll();
            agrs.RemoveAt(0);
            Main(agrs.ToArray());
            return 0;
        }

        #endregion
    }
}
