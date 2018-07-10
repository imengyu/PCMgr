using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TaskMgr.Aero.TaskDialog;

namespace TaskMgr
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;

            FormMain.currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            bool run = true;

            if (!FormMain.MIsRunasAdmin())
                run = ShowNOADMINWarn();
            if (FormMain.MIs64BitOS())
                run = Show64Warn();
            if (run) Application.Run(new FormMain());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            DialogResult d = MessageBox.Show(e.Exception.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (d == DialogResult.Abort)
                Environment.Exit(0);
        }

        private static bool Show64Warn()
        {
            if (FormMain.GetConfig("X32Warning", "AppSetting") == "TRUE")
            {
                bool has64 = FormMain.MIsFinded64();
                TaskDialog t = new TaskDialog("检测到软件版本与系统版本不符", FormMain.DEFAPPTITLE);
                t.Content = "检测到您在64位系统中运行32位的 " + FormMain.DEFAPPTITLE + " ，这将导致某些功能不可用。您还要继续运行软件吗？";
                t.ExpandedInformation = "当前版本的 " + FormMain.DEFAPPTITLE + " 是 32位的，而您的系统版本是64位的，一些高级功能（比如内核驱动查看）必须使用64位的驱动，所以此版本无法使用这些高级功能。" + (has64 ? "\n已经在您的软件目录下找到64位的 " + FormMain.DEFAPPTITLE + " ，点击”启动64位版本“可以直接启动软件。" : "");
                t.CommonIcon = TaskDialogIcon.Warning;
                t.VerificationText = "不要再提醒我了";
                t.VerificationClick += T_TaskDialog_64Warn_VerificationClick;
                if (has64)
                {
                    CustomButton[] btns = new CustomButton[3];
                    btns[0] = new CustomButton(Result.Yes, "继续运行");
                    btns[1] = new CustomButton(Result.No, "取消运行");
                    btns[2] = new CustomButton(Result.Abort, "运行64位版本");
                    t.CustomButtons = btns;
                }
                else
                {
                    CustomButton[] btns = new CustomButton[2];
                    btns[0] = new CustomButton(Result.Yes, "继续运行");
                    btns[1] = new CustomButton(Result.No, "取消运行");
                    t.CustomButtons = btns;
                }
                t.CanBeMinimized = true;
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
            if (FormMain.GetConfig("NOAdminWarning", "AppSetting") == "TRUE")
            {
                TaskDialog t = new TaskDialog("我需要更多的权限...", FormMain.DEFAPPTITLE, "检测到您没有赋予 " + FormMain.DEFAPPTITLE + " 管理员权限，这会导致一些高级功能不可用。\n" + FormMain.DEFAPPTITLE + " 需要管理员权限 才能对系统进行控制和管理。\n\n了解什么是<A HREF=\"#\">管理员权限</A>。\n了解本软件使用<A HREF=\"#\">管理员权限控制和管理的详情</A>。");
                t.CommonIcon = TaskDialogIcon.Warning;
                t.VerificationText = "不要再提醒我了";
                t.VerificationClick += T_TaskDialog_NOADMINWarn_VerificationClick;
                CustomButton[] btns = new CustomButton[3];
                btns[0] = new CustomButton(Result.Yes, "继续运行");
                btns[1] = new CustomButton(Result.No, "取消运行");
                btns[2] = new CustomButton(Result.Abort, "以管理员身份重启软件");
                t.CustomButtons = btns;
                t.CanBeMinimized = true;
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
            if(e.IsChecked)
                FormMain.SetConfig("NOAdminWarning", "AppSetting", "FALSE");
            else FormMain.SetConfig("NOAdminWarning", "AppSetting", "TRUE");
        }
        private static void T_TaskDialog_64Warn_VerificationClick(object sender, CheckEventArgs e)
        {
            if (e.IsChecked)
                FormMain.SetConfig("X32Warning", "AppSetting", "FALSE");
            else FormMain.SetConfig("X32Warning", "AppSetting", "TRUE");
        }

        public static int EntryPoint(string args)
        {
            Main();
            return 0;
        }
    }
}
