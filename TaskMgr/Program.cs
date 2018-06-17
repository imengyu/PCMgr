using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

            FormMain.currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            bool run = true;
            if (FormMain.MIs64BitOS())
            {
                string w = FormMain.GetConfig("X32Warning", "AppSetting");
                if (w == "TRUE")
                {
                    DialogResult rs = MessageBox.Show("检测到您在64位系统中运行32位的 " + FormMain.DEFAPPTITLE + " ，这将导致某些功能不可用。\n您还要继续运行软件吗？\n点击“是”继续运行，点击“取消”不再提示。", FormMain.DEFAPPTITLE, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                    if (rs == DialogResult.No)
                        run = false;
                    else if (rs == DialogResult.Cancel)
                        FormMain.SetConfig("X32Warning", "AppSetting", "FALSE");
                }
            }

            if (run)
            {
                Application.Run(new FormMain());
            }
        }

        public static int EntryPoint(string args)
        {
            Main();
            return 0;
        }
    }
}
