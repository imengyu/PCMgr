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
        public static void Main()
        {
            FormMain.currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            FormMain.InitLanuage();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.Run(new FormMain());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            DialogResult d = MessageBox.Show(e.Exception.ToString(), "Exception ! ", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (d == DialogResult.Abort)
                Environment.Exit(0);
        }

        public static int EntryPoint(string args)
        {
            Main();
            return 0;
        }
    }
}
