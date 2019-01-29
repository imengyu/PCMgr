using System;
using static PCMgr.NativeMethods;
using System.Windows.Forms;
using System.Drawing;
using PCMgr.Ctls;

namespace PCMgr.Main
{
    class MainSettings 
    {
        private FormMain formMain = null;

        public MainSettings(FormMain formMain)
        {
            this.formMain = formMain;
        }

        private int showHideHotKetId = -1;

        public int GetShowHideHotKetId() { return showHideHotKetId; }
        public bool MinHide { get; set; } = false;
        public bool CloseHide { get; set; } = false;
        public bool HighlightNoSystem { get; private set; } = false;
        public bool ShowHiddenFiles { get; private set; } = false;

        public void LoadSettings()
        {
            ShowHiddenFiles = GetConfigBool("ShowHiddenFiles", "AppSetting");
            HighlightNoSystem = GetConfigBool("HighLightNoSystetm", "Configure", false);

            int iSplitterDistanceperf = 0;
            if (int.TryParse(GetConfig("SplitterDistancePerf", "AppSetting", "0"), out iSplitterDistanceperf) && iSplitterDistanceperf > 0)
                formMain.splitContainerPerfCtls.SplitterDistance = iSplitterDistanceperf;

            LoadNativeSettings();
            LoadLastPos();
            LoadHotKey();
        }
        public void SaveSettings()
        {
            GetConfig("SplitterDistancePerf", "AppSetting", formMain.splitContainerPerfCtls.SplitterDistance.ToString());
            SetConfigBool("MainGrouping", "AppSetting", formMain.listProcess.ShowGroup);

            if (!formMain.IsSimpleView)
                SetConfig("OldSize", "AppSetting", formMain.Width.ToString() + "-" + formMain.Height.ToString());
            else SetConfig("OldSize", "AppSetting", formMain.LastSize.Width.ToString() + "-" + formMain.LastSize.Height.ToString());
            SetConfig("OldPos", "AppSetting", formMain.Left.ToString() + "-" + formMain.Top.ToString());
            SetConfigBool("OldIsMax", "AppSetting", formMain.WindowState == FormWindowState.Maximized);
        }

        private void LoadNativeSettings()
        {
            MFM_SetShowHiddenFiles(ShowHiddenFiles);
            MAppWorkCall3(194, IntPtr.Zero, GetConfigBool("TopMost", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(195, IntPtr.Zero, GetConfigBool("CloseHideToNotfication", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(196, IntPtr.Zero, GetConfigBool("MinHide", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(162, IntPtr.Zero, GetConfigBool("MainGrouping", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(156, IntPtr.Zero, GetConfigBool("AlwaysOnTop", "AppSetting", false) ? new IntPtr(1) : IntPtr.Zero);
            MAppWorkCall3(164, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(163, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(206, IntPtr.Zero, new IntPtr(GetConfig("TerProcFun", "Configure", "PspTerProc") == "ApcPspTerProc" ? 1 : 0));
        }
        private void LoadLastPos()
        {
            if (GetConfigBool("OldIsMax", "AppSetting"))
                formMain.WindowState = FormWindowState.Maximized;
            else
            {
                string p = GetConfig("OldPos", "AppSetting");
                if (p.Contains("-"))
                {
                    string[] pp = p.Split('-');
                    try
                    {
                        formMain.Left = int.Parse(pp[0]);
                        formMain.Top = int.Parse(pp[1]);
                        if (formMain.Left > Screen.PrimaryScreen.Bounds.Width)
                            formMain.Left = 100;
                        if (formMain.Top > Screen.PrimaryScreen.Bounds.Height)
                            formMain.Top = 200;
                    }
                    catch { }
                }
                string sl = GetConfig("OldSizeSimple", "AppSetting", "380-334");
                if (sl.Contains("-"))
                {
                    string[] ss = sl.Split('-');
                    try
                    {
                        int w = int.Parse(ss[0]); if (w + formMain.Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - formMain.Left;
                        int h = int.Parse(ss[1]); if (h + formMain.Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - formMain.Top;
                        formMain.LastSimpleSize = new Size(w, h);

                        if (GetConfigBool("SimpleView", "AppSetting", true))
                        {
                            formMain.Width = w;
                            formMain.Height = h;
                        }
                    }
                    catch { }
                }
                string s = GetConfig("OldSize", "AppSetting", "780-500");
                if (s.Contains("-"))
                {
                    string[] ss = s.Split('-');
                    try
                    {
                        int w = int.Parse(ss[0]); if (w + formMain.Left > Screen.PrimaryScreen.WorkingArea.Width) w = Screen.PrimaryScreen.WorkingArea.Width - formMain.Left;
                        int h = int.Parse(ss[1]); if (h + formMain.Top > Screen.PrimaryScreen.WorkingArea.Height) h = Screen.PrimaryScreen.WorkingArea.Height - formMain.Top;
                        formMain.LastSize = new Size(w, h);
                    }
                    catch { }
                }
            }
        }
        private void LoadHotKey()
        {
            if (GetConfigBool("HotKey", "AppSetting", true))
            {
                string k1 = GetConfig("HotKey1", "AppSetting", "(None)");
                string k2 = GetConfig("HotKey2", "AppSetting", "T");
                if (k1 == "(None)") k1 = "None";
                Keys kv1, kv2;
                try
                {
                    if (k1 != "(None)") kv1 = (Keys)Enum.Parse(typeof(Keys), k1);
                    else kv1 = Keys.None;
                    kv2 = (Keys)Enum.Parse(typeof(Keys), k2);
                }
                catch (Exception e)
                {
                    LogErr("Invalid hotkey settings : " + e.Message);
                    kv2 = Keys.T;
                    kv1 = Keys.Shift;
                }

                showHideHotKetId = MAppRegShowHotKey(formMain.Handle, (uint)(int)kv1, (uint)(int)kv2);
                MAppWorkCall3(209, formMain.Handle, IntPtr.Zero);
            }
        }

        //保存和读取视图列的宽度
        public void LoadListColumnsWidth()
        {
            string s = GetConfig("ListStartsWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < formMain.listStartup.Colunms.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        formMain.listStartup.Colunms[i].Width = width;
                }
            }
            s = GetConfig("ListUWPsWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < formMain.listUwpApps.Colunms.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        formMain.listUwpApps.Colunms[i].Width = width;
                }
            }
            s = GetConfig("ListUsersWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < formMain.listUsers.Colunms.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        formMain.listUsers.Colunms[i].Width = width;
                }
            }
            s = GetConfig("ListDriversWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < formMain.listDrivers.Columns.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        formMain.listDrivers.Columns[i].Width = width;
                }
            }
            s = GetConfig("ListServiceWidths", "AppSetting", "");
            if (s.Contains("#"))
            {
                string[] ss = s.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ss.Length && i < formMain.listService.Columns.Count; i++)
                {
                    int width = 0;
                    if (int.TryParse(ss[i], out width) && width > 0 && width < 1000)
                        formMain.listService.Columns[i].Width = width;
                }
            }
        }
        public void SaveListColumnsWidth()
        {
            string s = "";
            foreach (TaskMgrListHeaderItem he in formMain.listStartup.Colunms)
                s += "#" + he.Width;
            SetConfig("ListStartsWidths", "AppSetting", s);
            s = "";
            foreach (TaskMgrListHeaderItem he in formMain.listUwpApps.Colunms)
                s += "#" + he.Width;
            SetConfig("ListUWPsWidths", "AppSetting", s);
            s = "";
            foreach (TaskMgrListHeaderItem he in formMain.listUsers.Colunms)
                s += "#" + he.Width;
            SetConfig("ListUsersWidths", "AppSetting", s);
            s = "";
            foreach (ColumnHeader he in formMain.listDrivers.Columns)
                s += "#" + he.Width;
            SetConfig("ListDriversWidths", "AppSetting", s);
            s = "";
            foreach (ColumnHeader he in formMain.listService.Columns)
                s += "#" + he.Width;
            SetConfig("ListServiceWidths", "AppSetting", s);
        }
    }
}
