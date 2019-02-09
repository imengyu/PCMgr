using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using PCMgr.Aero.TaskDialog;
using PCMgr.Ctls;
using PCMgr.Helpers;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.Win32;
using static PCMgr.NativeMethods.LogApi;

using PCMgr.Main;

//
// 说明 ：这里是主窗口 上层的逻辑代码
//
// C++ Native 层的代码 请到项目 PCMgrCore 查看
//
// 在看代码之前请先按 Ctrl+M,Ctrl+O 折叠大纲，这样看起来会清爽一点
// 个人代码水平有限，写不出优质的代码，各位看官就当一个笑话看看好了
// 此软件并非仅我一人开发，因此代码风格会很乱，真的抱歉
//
// 这是一个垃圾的软件
// 2019
//屎山

namespace PCMgr
{
    /// <summary>
    /// 主窗口
    /// </summary>
    public partial class FormMain : Form
    {
        public FormMain(string[] agrs)
        {
            this.agrs = agrs;
            InitializeComponent();

            baseProcessRefeshTimer.Interval = 1000;
            baseProcessRefeshTimer.Tick += BaseProcessRefeshTimer_Tick;

            lbStartingStatus.BringToFront();
        }

        private string[] agrs = null;
        private bool _isSimpleView = false;
        private bool _isNativeBridgeLoaded = false;

        //Timers
        internal Timer baseProcessRefeshTimer = new Timer();
        internal Timer baseProcessRefeshTimerLow = new Timer();
        internal Timer baseProcessRefeshTimerLowUWP = new Timer();
        internal Timer baseProcessRefeshTimerLowSc = new Timer();

        internal void SetMainRefeshTimerInterval(int c)
        {
            if (c == 2)
            {
                baseProcessRefeshTimer.Enabled = false;
                baseProcessRefeshTimer.Stop();
                baseProcessRefeshTimerLowUWP.Stop();
                SetConfig("RefeshTime", "AppSetting", "Stop");
                baseProcessRefeshTimerLow.Stop();
                baseProcessRefeshTimerLowSc.Stop();
                mainPagePerf.PerfUpdateGridUnit();
            }
            else
            {
                baseProcessRefeshTimer.Enabled = true;
                if (c == 1)
                {
                    baseProcessRefeshTimer.Interval = 2000;
                    baseProcessRefeshTimerLow.Interval = 10000;
                    SetConfig("RefeshTime", "AppSetting", "Slow");
                }
                else if (c == 0)
                {
                    baseProcessRefeshTimer.Interval = 1000;
                    baseProcessRefeshTimerLow.Interval = 5000;
                    SetConfig("RefeshTime", "AppSetting", "Fast");
                }
                baseProcessRefeshTimer.Start();
                baseProcessRefeshTimerLowUWP.Start();
                baseProcessRefeshTimerLow.Start();
                baseProcessRefeshTimerLowSc.Start();
                mainPagePerf.PerfUpdateGridUnit();
            }
        }

        internal bool IsSimpleView
        {
            get { return _isSimpleView; }
            set
            {
                _isSimpleView = value;
                if (_isSimpleView)
                {
                    MAppWorkCall3(215, Handle, IntPtr.Zero);
                    pl_simple.Show();
                    tabControlMain.Hide();
                    baseProcessRefeshTimer.Interval = 2000;
                    baseProcessRefeshTimer.Start();
                    BaseProcessRefeshTimer_Tick(this, null);
                    baseProcessRefeshTimerLow.Interval = 10000;
                    baseProcessRefeshTimerLowUWP.Start();
                    baseProcessRefeshTimerLow.Start();
                    listProcess.Locked = true;
                }
                else
                {
                    MAppWorkCall3(216, Handle, IntPtr.Zero);
                    listApps.Items.Clear();
                    pl_simple.Hide();
                    tabControlMain.Show();
                    MAppWorkCall3(163, IntPtr.Zero, IntPtr.Zero);
                    listProcess.Locked = false;
                    mainPageProcess.ProcessListForceRefeshAll();
                }
            }
        }
        internal bool Is64OS { get; private set; }
        internal bool IsAdmin { get; private set; }
        internal bool IsKernelLoaded { get; private set; }

        internal Size LastSimpleSize { get; set; }
        internal Size LastSize { get; set; }
        internal MainNativeBridge MainNativeBridge { get { return mainNativeBridge; } }
        internal MainPageScMgr MainPageScMgr { get { return mainPageScMgr; } }
        internal MainPageUwpMgr MainPageUwpMgr { get { return mainPageUwpMgr; } }
        internal MainPageProcess MainPageProcess { get { return mainPageProcess; } }
        internal MainPageProcessDetails MainPageProcessDetails { get { return mainPageProcessDetails; } }
        internal MainPageKernelDrvMgr MainPageKernelDrvMgr { get { return mainPageKernelDrvMgr; } }
        internal MainPageStartMgr MainPageStartMgr { get { return mainPageStartMgr; } }
        internal MainSettings MainSettings { get; private set; }
        internal MainPagePerf MainPagePerf { get { return mainPagePerf; } }

        private MainNativeBridge mainNativeBridge = null;
        private MainPageProcess mainPageProcess = null;
        private MainPageProcessDetails mainPageProcessDetails = null;
        private MainPageKernelDrvMgr mainPageKernelDrvMgr = null;
        private MainPageFileMgr mainPageFileMgr = null;
        private MainPagePerf mainPagePerf = null;
        private MainPageScMgr mainPageScMgr = null;
        private MainPageStartMgr mainPageStartMgr = null;
        private MainPageUserMgr mainPageUserMgr = null;
        private MainPageUwpMgr mainPageUwpMgr = null;

        #region NotifyWork

        //一些对话框

        private FormDelFileProgress delingdialog = null;

        public void DelingDialogInitHide()
        {
            MAppWorkCall3(200, delingdialog.Handle, IntPtr.Zero);
        }
        public void DelingDialogInit()
        {
            delingdialog = new FormDelFileProgress();
            delingdialog.Show(this);
            MAppWorkCall3(200, delingdialog.Handle, IntPtr.Zero);
        }
        public void DelingDialogClose()
        {
            if (delingdialog != null)
            {
                delingdialog.Close();
                delingdialog = null;
            }
        }
        public void DelingDialogShowHide(bool show)
        {
            delingdialog.Invoke(new Action(delegate
            {
                delingdialog.Visible = show;
                if (show)
                {
                    delingdialog.Location = new Point(Left + Width / 2 - delingdialog.Width / 2, Top + Height / 2 - delingdialog.Height / 2);
                    delingdialog.Text = LanuageMgr.GetStr("DeleteFiles");
                }
            }));
        }
        public void DelingDialogUpdate(string path, int value)
        {
            delingdialog.label.Invoke(new Action(delegate { delingdialog.label.Text = path; }));
            if (value == -1)
            {
                delingdialog.progressBar.Invoke(new Action(delegate { delingdialog.progressBar.Style = ProgressBarStyle.Marquee; }));
                delingdialog.Invoke(new Action(delegate
                {
                    delingdialog.Text = LanuageMgr.GetStr("CollectingFiles");
                }));
            }
            else
            {
                delingdialog.progressBar.Invoke(new Action(delegate
                {
                    delingdialog.progressBar.Style = ProgressBarStyle.Blocks;
                    if (value >= 0 && value <= 100) delingdialog.progressBar.Value = value;
                }));
            }
        }

        private string lastVeryExe = "";
        private void FileTrustedLink_HyperlinkClick(object sender, HyperlinkEventArgs e)
        {
            if (!string.IsNullOrEmpty(lastVeryExe))
                MShowExeFileSignatureInfo(lastVeryExe);
        }
        public void VeryTrust(string path)
        {
            lastVeryExe = path;
            TaskDialog d = new TaskDialog(LanuageMgr.GetStr("FileTrust"), LanuageFBuffers.Str_TipTitle, (path == null ? "" : path) + "\n\n" + LanuageMgr.GetStr("FileTrustViewCrt"));
            d.EnableHyperlinks = true;
            d.HyperlinkClick += FileTrustedLink_HyperlinkClick;
            d.Show(this);
        }

        public void StartingProgressShowHide(bool show)
        {
            lbStartingStatus.Invoke(new Action(delegate { lbStartingStatus.Visible = show; }));
            listProcess.Invoke(new Action(delegate { listProcess.Visible = !show; }));
        }
        public void StartingProgressUpdate(string text)
        {
            lbStartingStatus.Invoke(new Action(delegate { lbStartingStatus.Text = text; }));
        }

        public void ShowNoPdbWarn(string moduleName)
        {
            Invoke(new Action(delegate
            {
                TaskDialog noPdbWarnDialog = new TaskDialog(string.Format(LanuageMgr.GetStr("NoPDBWarn", false), moduleName), LanuageFBuffers.Str_TipTitle, string.Format(LanuageMgr.GetStr("NoPDBWarnText", false), moduleName, moduleName));
                noPdbWarnDialog.EnableHyperlinks = true;
                noPdbWarnDialog.Show(this);
            }));
        }

        public bool TermintateImporantProcess(IntPtr name, int id)
        {
            TaskDialog taskDialog = null;
            if (id == 1)//强制结束警告
            {
                taskDialog = new TaskDialog(LanuageMgr.GetStr("KillAskStart") + " \"" + Marshal.PtrToStringUni(name) + "\" " + LanuageMgr.GetStr("KillAskEnd"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("KillAskContentImporant"));
                taskDialog.VerificationText = LanuageMgr.GetStr("KillAskImporantGiveup");
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, LanuageFBuffers.Str_Close),
                new CustomButton(2, LanuageFBuffers.Str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 2)//强制暂停警告
            {
                taskDialog = new TaskDialog(LanuageMgr.GetStr("SuspendStart") + " \"" + Marshal.PtrToStringUni(name) + "\" " + LanuageMgr.GetStr("SuspendEnd"),
                    LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("SuspendWarnContent"));
                taskDialog.VerificationText = LanuageMgr.GetStr("KillAskImporantGiveup");
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, LanuageFBuffers.Str_Close),
                new CustomButton(2, LanuageFBuffers.Str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 3)//强制结束重要警告
            {
                taskDialog = new TaskDialog(LanuageMgr.GetStr("KillAskStart") + " \"" + Marshal.PtrToStringUni(name) + "\" " + LanuageMgr.GetStr("KillAskEnd"),
                    LanuageMgr.GetStr("TitleVeryWarn"), LanuageMgr.GetStr("KillAskContentVeryImporant"));
                taskDialog.VerificationText = LanuageMgr.GetStr("KillAskImporantGiveup");
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, LanuageFBuffers.Str_Close),
                new CustomButton(2, LanuageFBuffers.Str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 4)//强制暂停重要重要警告
            {
                taskDialog = new TaskDialog(LanuageMgr.GetStr("SuspendStart") + " \"" + Marshal.PtrToStringUni(name) + "\" " + LanuageMgr.GetStr("SuspendEnd"),
                   LanuageMgr.GetStr("TitleVeryWarn"), LanuageMgr.GetStr("SuspendVeryImporantWarnContent"));
                taskDialog.VerificationText = LanuageMgr.GetStr("KillAskImporantGiveup");
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, LanuageFBuffers.Str_Close),
                new CustomButton(2, LanuageFBuffers.Str_Cancel),
                };
                taskDialog.EnableButton(1, false);
            }
            if (id == 5)//暂停当前进程警告
            {
                taskDialog = new TaskDialog(LanuageMgr.GetStr("SuspendThisTitle"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("SuspendThisText"));
                taskDialog.VerificationText = LanuageMgr.GetStr("SuspendCheckText");
                taskDialog.VerificationClick += TermintateImporantProcess_TaskDialog_VerificationClick;
                taskDialog.CustomButtons = new CustomButton[]
                {
                new CustomButton(1, LanuageFBuffers.Str_Yes),
                new CustomButton(2, LanuageFBuffers.Str_No),
                };
                taskDialog.EnableButton(1, false);
            }

            Results rs = taskDialog.Show(this);
            return rs.ButtonID == 1;
        }
        public void TermintateImporantProcess_TaskDialog_VerificationClick(object sender, CheckEventArgs e)
        {
            TaskDialog taskDialog = sender as TaskDialog;
            taskDialog.EnableButton(1, e.IsChecked);
        }

        #endregion

        private void BaseProcessRefeshTimer_Tick(object sender, EventArgs e)
        {
            //整体刷新定时器

            double cpu = 0;
            double ram = 0;
            double disk = 0;
            double net = 0;

            bool perfsimpleGeted = false;

            if (mainPagePerf.Inited) mainPagePerf.PerfDayUpdate(out cpu, out ram, out disk, out net, out perfsimpleGeted);
            if (!Visible || (WindowState == FormWindowState.Minimized)) return;
            //base RefeshTimer
            if (tabControlMain.SelectedTab == tabPageProcCtl && mainPageProcess.Inited) mainPageProcess.ProcessListDayUpdate(cpu, ram, disk, net, perfsimpleGeted);
            else if (tabControlMain.SelectedTab == tabPagePerfCtl)
            {
                if (!perfsimpleGeted)
                {
                    MPERF_GlobalUpdatePerformanceCounters();
                    MPERF_GlobalUpdateCpu();
                }

                mainPagePerf.PerfUpdate();

                performanceLeftList.Invalidate();
            }
            else if (tabControlMain.SelectedTab == tabPageDetals && mainPageProcessDetails.Inited) mainPageProcessDetails.ProcessListDetailsRefesh();

        }

        //Load and exit
        private void AppLoad()
        {
            AppOnLoad();
        }
        private void AppExit()
        {
            //退出函数
            Log("App exit...");
            AppOnExit();
            Application.Exit();
        }

        private int AppRunAgrs()
        {
            if (agrs.Length > 0)
            {
                Log("App Agrs 0 : " + agrs[0]);
                if (agrs[0] == "select" && agrs.Length > 1)
                {
                    Log("App Agrs 1 : " + agrs[1]);
                    if (agrs[1] == "kernel")
                        return 1;
                    if (agrs[1] == "perf")
                        return 3;
                    if (agrs[1] == "uwpapps")
                        return 4;
                    if (agrs[1] == "services")
                        return 5;
                    if (agrs[1] == "startmgr")
                        return 6;
                    if (agrs[1] == "filemgr")
                        return 7;
                    if (agrs[1] == "details")
                        return 8;
                    if (agrs[1] == "users")
                        return 9;
                }
                else if (agrs[0] == "spy")
                    new FormSpyWindow(GetDesktopWindow()).ShowDialog();
                else if (agrs[0] == "filetool")
                    new FormFileTool().ShowDialog();
                else if (agrs[0] == "loaddriver")
                    new FormLoadDriver().ShowDialog();
            }
            return 0;
        }

        public void AppLastLoadStep()
        {
            int id = AppRunAgrs();
            if (id != 0 && GetConfigBool("SimpleView", "AppSetting", true)) id = 0;
            switch (id)
            {
                case 1:
                    tabControlMain.SelectedTab = tabPageKernelCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageKernelCtl, 0, TabControlAction.Selected));
                    break;
                case 3:
                    tabControlMain.SelectedTab = tabPagePerfCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPagePerfCtl, 0, TabControlAction.Selected));
                    break;
                case 4:
                    tabControlMain.SelectedTab = tabPageUWPCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageUWPCtl, 0, TabControlAction.Selected));
                    break;
                case 5:
                    tabControlMain.SelectedTab = tabPageScCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageScCtl, 0, TabControlAction.Selected));
                    break;
                case 6:
                    tabControlMain.SelectedTab = tabPageStartCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageStartCtl, 0, TabControlAction.Selected));
                    break;
                case 7:
                    tabControlMain.SelectedTab = tabPageFileCtl;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageFileCtl, 0, TabControlAction.Selected));
                    break;
                case 8:
                    tabControlMain.SelectedTab = tabPageDetals;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageDetals, 0, TabControlAction.Selected));
                    return;
                case 9:
                    tabControlMain.SelectedTab = tabPageUsers;
                    lbStartingStatus.Hide();
                    tabControlMain.Show();
                    tabControlMain_Selected(this, new TabControlEventArgs(tabPageUsers, 0, TabControlAction.Selected));
                    return;
                case 0:
                default:
                    mainPageProcess.ProcessListInit();
                    break;
            }
            MAppWorkCall3(188, IntPtr.Zero, IntPtr.Zero);
            MAppWorkCall3(177, IntPtr.Zero, IntPtr.Zero);

        }
        private void AppLoadKernel()
        {
            if (GetConfigBool("LoadKernelDriver", "Configure"))
            {
                Log("Loading Kernel...");
                if (!MInitKernel(null))
                {
                    mainPageProcess.ProcessListRemoveEprocessCol();

                    if (MIsKernelNeed64())
                        TaskDialog.Show(LanuageMgr.GetStr("LoadDriverErrNeed64"), LanuageMgr.GetStr("ErrTitle"), LanuageMgr.GetStr("LoadDriverErrNeed64Text"), TaskDialogButton.OK, TaskDialogIcon.None);
                    else TaskDialog.Show(LanuageMgr.GetStr("LoadDriverErr"), LanuageMgr.GetStr("ErrTitle"), "", TaskDialogButton.OK, TaskDialogIcon.None);
                    AppLastLoadStep();
                }
                else
                {
                    if (GetConfigBool("SelfProtect", "AppSetting"))
                        MAppWorkCall3(203, IntPtr.Zero, IntPtr.Zero);
                }
                IsKernelLoaded = MCanUseKernel();
            }
            else
            {
                AppLastLoadStep();
                mainPageProcess.ProcessListRemoveEprocessCol();
            }
        }
        private void AppLoadPages()
        {
            mainPageProcess = new MainPageProcess(this);
            mainPageProcessDetails = new MainPageProcessDetails(this);
            mainPageKernelDrvMgr = new MainPageKernelDrvMgr(this);
            mainPageFileMgr = new MainPageFileMgr(this);
            mainPagePerf = new MainPagePerf(this);
            mainPageScMgr = new MainPageScMgr(this);
            mainPageStartMgr = new MainPageStartMgr(this);
            mainPageUserMgr = new MainPageUserMgr(this);
            mainPageUwpMgr = new MainPageUwpMgr(this);

            mainPageProcess.Load();
            mainPageProcessDetails.Load();
            mainPageKernelDrvMgr.Load();
            mainPageFileMgr.Load();
            mainPagePerf.Load();
            mainPageScMgr.Load();
            mainPageStartMgr.Load();
            mainPageUserMgr.Load();
            mainPageUwpMgr.Load();
        }
        private void AppUnLoadPages()
        {
            //mainNativeBridge = new MainNativeBridge();
            mainPageProcess.UnLoad();
            mainPageProcessDetails.UnLoad();
            mainPageKernelDrvMgr.UnLoad();
            mainPageFileMgr.UnLoad();
            mainPagePerf.UnLoad();
            mainPageScMgr.UnLoad();
            mainPageStartMgr.UnLoad();
            mainPageUserMgr.UnLoad();
            mainPageUwpMgr.UnLoad();
        }

        private void AppOnPreLoad()
        {
            //初始化函数

            //初始化桥
            mainNativeBridge = new MainNativeBridge(this);
            mainNativeBridge.exitCallBack = AppExit;
            mainNativeBridge.terminateImporantWarnCallBack = TermintateImporantProcess;
            mainNativeBridge.coreWndProc = (WNDPROC)Marshal.GetDelegateForFunctionPointer(MAppSetCallBack(IntPtr.Zero, 0), typeof(WNDPROC));
            mainNativeBridge.InitCallbacks();

            _isNativeBridgeLoaded = true;

            MainSettings = new MainSettings(this);

            //加载设置和所有页的代码
            AppLoadPages();
            AppLoadSettings();
            AppLoadAllMenuStyles();

            //标题
            SetConfig("LastWindowTitle", "AppSetting", Text);
            Text = GetConfig("Title", "AppSetting", "任务管理器");
            if (Text == "") Text = LanuageFBuffers.Str_AppTitle;

            new System.Threading.Thread(mainPagePerf.PerfInitTray).Start();

            //系统位数
#if _X64_
            Log("64 Bit OS ");
            Is64OS = true;
#else
            Is64OS = MIs64BitOS();
            Log(Is64OS ? "64 Bit OS but 32 bit app " : "32 Bit OS");
#endif
            IsAdmin = MIsRunasAdmin();

            //判断是否支持通用应用（win8 以上）
            SysVer.Get();
            if (!SysVer.IsWin8Upper()) tabControlMain.TabPages.Remove(tabPageUWPCtl);
            else M_UWP_Init();

            //提升权限
            if (!MGetPrivileges()) TaskDialog.Show(LanuageMgr.GetStr("FailedGetPrivileges"), LanuageFBuffers.Str_AppTitle, "", TaskDialogButton.OK, TaskDialogIcon.Warning);

            //tab 头 的自定义
            TaskMgrTabHeader tabHeader = new TaskMgrTabHeader(tabControlMain);
            Controls.Add(tabHeader);
            tabHeader.Dock = DockStyle.Top;
            tabHeader.Height = 27;
            tabHeader.BringToFront();
            tabHeader.Font = tabControlMain.Font;
            if (IsSimpleView) tabHeader.Visible = false;

            //Shell Icon
            IntPtr shellIconPtr = MGetShieldIcon2();
            Icon shellIcon = Icon.FromHandle(shellIconPtr);
            Bitmap shellIcoBtimap = MainUtils.IconToBitmap(shellIcon, 16, 16);

            check_showAllProcess.Image = shellIcoBtimap;
            linkRebootAsAdmin.Image = shellIcoBtimap;
            linkRestartAsAdminDriver.Image = shellIcoBtimap;

            DestroyIcon(shellIconPtr);
        }
        private void AppOnLoad()
        { 
            if (MIsRunasAdmin()) AppLoadKernel();
            else AppLastLoadStep();
        }
        private void AppOnExit()
        {
            if (!exitCalled)
            {
                baseProcessRefeshTimer.Stop();

                AppSaveSettings();
                AppUnLoadPages();

                fileSystemWatcher.EnableRaisingEvents = false;
                
                mainNativeBridge.AppWorkerCallBack(38, IntPtr.Zero, IntPtr.Zero);
                DelingDialogClose();

                mainPageUserMgr.UsersListUnInit();
                mainPagePerf.PerfClear();

                mainPageProcess.ProcessListFreeAll();
                mainPageProcessDetails.ProcessListDetailsUnInit();
                MSCM_Exit();
                mainPageKernelDrvMgr.KernelListUnInit();
                M_LOG_Close();
                if (SysVer.IsWin8Upper())
                {
                    mainPageUwpMgr.UWPListUnInit();
                    M_UWP_UnInit();
                }
                MAppWorkCall3(204, IntPtr.Zero, IntPtr.Zero);
                MAppWorkCall3(207, Handle, IntPtr.Zero);
                exitCalled = true;
            }
        }

        private void AppLoadAllMenuStyles()
        {
            contextMenuStripMainHeader.Renderer = new ClassicalMenuRender(Handle);
            contextMenuStripPerfListLeft.Renderer = contextMenuStripMainHeader.Renderer;
            contextMenuStripProcDetalsCol.Renderer = contextMenuStripMainHeader.Renderer;
            contextMenuStripTray.Renderer = contextMenuStripMainHeader.Renderer;
            contextMenuStripUWP.Renderer = contextMenuStripMainHeader.Renderer;
        }

        private void AppLoadSettings()
        {
            MainSettings.LoadSettings();
            MainSettings.LoadListColumnsWidth();
        }
        private void AppSaveSettings()
        {
            MainSettings.SaveListColumnsWidth();
            MainSettings.SaveSettings();
        }

        #region App debug cmd

        public void AppDebugCmd(string[] cmd, uint size)
        {
            string cmd0 = cmd[0];
            switch (cmd0)
            {
                case "test": Log("app test success!"); break;
                case "gc": GC.Collect(); break;
                case "?":
                case "help":
                    LogText("app debug commands help: \n" +
                        "exit Save exit application" +
                        "reboot Reboot application" +
                        "stat Show app run stats" +
                        "procs MainPageProcess debug (?)" +
                        "lg LanuageMgr debug (?)" +
                        "settings SettingsMgr debug (?)");
                    break;
                case "sets": MainSettings.DebugCmd(cmd, size); break;
                case "procs": mainPageProcess.DebugCmd(cmd, size); break;
                case "lg": LanuageMgr.DebugCmd(cmd, size); break;
                default: LogWarn("Not found command : " + cmd0); break;
            }
        }

        #endregion

        #region KDbgPrint

        private FormKDbgPrint kDbgPrint = null;
        private bool exitkDbgPrintCalled = false;
        private void KDbgPrint_FormClosed(object sender, FormClosedEventArgs e)
        {
            kDbgPrint = null;
        }

        public void KDbgPrintShow()
        {
            if (kDbgPrint == null)
            {
                kDbgPrint = new FormKDbgPrint();
                kDbgPrint.FormClosed += KDbgPrint_FormClosed;
            }
            kDbgPrint.Show();
        }
        public void KDbgPrintClose()
        {
            if (kDbgPrint != null && !exitkDbgPrintCalled)
            {
                exitkDbgPrintCalled = true;
                kDbgPrint.Close();
                kDbgPrint = null;
                exitkDbgPrintCalled = false;
            }
        }
        public void KDbgPrintData(string data)
        {
            if(kDbgPrint != null) kDbgPrint.Add(data);
        }
        public void KDbgPrintData()
        {
            if (kDbgPrint != null) kDbgPrint.Add("");
        }

        #endregion

        #region FormEvent
        //窗口的一些事件

        private bool exitCalled = false;

        //notifyIcon 托盘图标事件
        private bool notifyIcon_mouseEntered = false;
        private void notifyIcon_MouseEnter(object sender, EventArgs e)
        {
            mainPagePerf.PerfShowSpeedBall();
        }
        public void notifyIcon_MouseLeave(object sender, EventArgs e)
        {
            notifyIcon_mouseEntered = false;
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MAppWorkCall3(208, Handle, Handle);
        }
        private void notifyIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (!notifyIcon_mouseEntered)
            {
                notifyIcon_mouseEntered = true;
                notifyIcon_MouseEnter(sender, e);
            }
        }

        //notifyIcon menu 托盘图标菜单事件
        private void 退出程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppExit();
        }
        private void 显示隐藏主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsWindowVisible(Handle)) ShowWindow(Handle, 0);
            else ShowWindow(Handle, 5);
        }
        private void contextMenuStripTray_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsWindowVisible(Handle)) 显示隐藏主界面ToolStripMenuItem.Text = LanuageMgr.GetStr("HideMain");
            else 显示隐藏主界面ToolStripMenuItem.Text = LanuageMgr.GetStr("ShowMain");
        }

        //窗口事件
        private void FormMain_Shown(object sender, EventArgs e)
        {
            AppLoad();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            AppOnPreLoad();
        }
        private void FormMain_Activated(object sender, EventArgs e)
        {
            listUwpApps.FocusedType = true;
            listStartup.FocusedType = true;
            listProcess.FocusedType = true;
        }
        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            listUwpApps.FocusedType = false;
            listStartup.FocusedType = false;
            listProcess.FocusedType = false;
        }
        private void FormMain_OnWmCommand(int id)
        {
            switch (id)
            {
                //def in resource.h
                case 40005://Settings
                    {
                        new FormSettings(this).ShowDialog();
                        break;
                    }
                case 40034://Choose column
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                        {
                            FormMainListHeaders f = new FormMainListHeaders(this);
                            if (f.ShowDialog() == DialogResult.OK) MAppWorkCall3(191, IntPtr.Zero, IntPtr.Zero);
                        }
                        else if (tabControlMain.SelectedTab == tabPageDetals)
                            new FormDetalsistHeaders(this).ShowDialog();
                        break;
                    }
                case 40017: //Sleep system
                    {
                        Application.SetSuspendState(PowerState.Suspend, true, true);
                        break;
                    }
                case 41174:
                    {
                        //Hibernate system
                        Application.SetSuspendState(PowerState.Hibernate, true, true);
                        break;
                    }
                case 41130:
                case 41012://Refesh
                    {
                        if (tabControlMain.SelectedTab == tabPageProcCtl)
                            mainPageProcess.ProcessListRefesh();
                        else if (tabControlMain.SelectedTab == tabPageKernelCtl)
                            mainPageKernelDrvMgr.KernelLisRefesh();
                        else if (tabControlMain.SelectedTab == tabPageStartCtl)
                            mainPageStartMgr.StartMListRefesh();
                        else if (tabControlMain.SelectedTab == tabPageScCtl)
                            mainPageScMgr.ScMgrRefeshList();
                        else if (tabControlMain.SelectedTab == tabPageFileCtl)
                            mainPageFileMgr.FileMgrShowFiles(null);
                        else if (tabControlMain.SelectedTab == tabPageUWPCtl)
                            mainPageUwpMgr.UWPListRefesh();
                        else if (tabControlMain.SelectedTab == tabPagePerfCtl)
                            BaseProcessRefeshTimer_Tick(null, null);
                        else if (tabControlMain.SelectedTab == tabPageDetals)
                            BaseProcessRefeshTimer_Tick(null, null);
                        else if (tabControlMain.SelectedTab == tabPageUsers)
                            mainPageUserMgr.UsersListLoad();
                        break;
                    }
                case 40019://Reboot
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleReboot"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(185, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41020://Logoff
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleLogoOff"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(186, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 40018://Shutdown
                    {
                        TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleShutdown"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                        if (t.Show(this).CommonButton == Result.Yes)
                            MAppWorkCall3(187, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case 41151://FShutdown
                    {
                        if (IsKernelLoaded)
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleFShutdown"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show(this).CommonButton == Result.Yes)
                                MAppWorkCall3(201, IntPtr.Zero, IntPtr.Zero);
                        }
                        else TaskDialog.Show(LanuageFBuffers.Str_DriverNotLoad, LanuageFBuffers.Str_AppTitle);
                        break;
                    }
                case 41152://FRebbot
                    {
                        if (IsKernelLoaded)
                        {
                            TaskDialog t = new TaskDialog(LanuageMgr.GetStr("TitleFRebbot"), LanuageFBuffers.Str_AppTitle, LanuageMgr.GetStr("TitleContinue"), TaskDialogButton.Yes | TaskDialogButton.No, TaskDialogIcon.Warning);
                            if (t.Show(this).CommonButton == Result.Yes)
                                MAppWorkCall3(202, IntPtr.Zero, IntPtr.Zero);
                        }
                        else TaskDialog.Show(LanuageFBuffers.Str_DriverNotLoad, LanuageFBuffers.Str_AppTitle);
                        break;
                    }
                case 41153://Test2
                    {
                        //MCller
                        break;
                    }

            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MainSettings.CloseHide)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            notifyIcon.Visible = false;
            AppOnExit();
        }
        private void FormMain_OnWmHotKey(int id)
        {
            if (id == MainSettings.GetShowHideHotKetId())
            {
                if (!IsWindowVisible(Handle))
                    MAppWorkCall3(208, Handle, IntPtr.Zero);
            }
        }
        private void FormMain_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                listProcess.Locked = false;
                if (mainPageProcess.Inited) BaseProcessRefeshTimer_Tick(sender, e);
            }
            else listProcess.Locked = true;

        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_COMMAND)
                FormMain_OnWmCommand(m.WParam.ToInt32());
            else if (m.Msg == WM_HOTKEY)
                FormMain_OnWmHotKey(m.WParam.ToInt32());
            else if (m.Msg == WM_SYSCOMMAND)
            {
                if (MainSettings.MinHide && m.WParam.ToInt32() == 0xF20)//SC_MINIMIZE
                    Hide();
            }
            //WndProc部分交予 Native 控制
            if(_isNativeBridgeLoaded) MainNativeBridge.WndProc(ref m);
        }

        public static void AppHWNDSendMessage(uint message, IntPtr wParam, IntPtr lParam)
        {
            MAppWorkCall2(message, wParam, lParam);
        }

        public void GroupSwitch(bool b)
        {
            if (tabControlMain.SelectedTab == tabPageProcCtl)
            {
                listProcess.ShowGroup = b;
                listProcess.SyncItems(true);
            }
        }
        public void ShowKernelTools()
        {
            MessageBox.Show("The function aren't complete. ");
        }
        public void ShowFormHooks()
        {
            MessageBox.Show("The function aren't complete. ");
        }
        public void SetToFileMgr()
        {
            tabControlMain.SelectedTab = tabPageFileCtl;
        }
        public void CollapseAll()
        {
            if (tabControlMain.SelectedTab == tabPageProcCtl)
                mainPageProcess.ProcessListCollapseAll();
            else if (tabControlMain.SelectedTab == tabPageStartCtl)
                mainPageStartMgr.StartMListCollapseAll();
        }
        public void ExpandAll()
        {
            if (tabControlMain.SelectedTab == tabPageProcCtl)
                mainPageProcess.ProcessListExpandAll();
            else if (tabControlMain.SelectedTab == tabPageStartCtl)
                mainPageStartMgr.StartMListExpandAll();
        }


        #endregion

        //标签点击事件
        private void tabControlMain_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPageProcCtl)
            {
                mainPageProcess.ProcessListInit();
            }
            else if (e.TabPage == tabPageScCtl)
            {
                mainPageScMgr.ScMgrInit();
            }
            else if (e.TabPage == tabPageFileCtl)
            {
                mainPageFileMgr.FileMgrInit();
            }
            else if (e.TabPage == tabPageUWPCtl)
            {
                mainPageUwpMgr.UWPListInit();
            }
            else if (e.TabPage == tabPagePerfCtl)
            {
                mainPagePerf.PerfInit();
            }
            else if (e.TabPage == tabPageStartCtl)
            {
                mainPageStartMgr.StartMListInit();
            }
            else if (e.TabPage == tabPageKernelCtl)
            {
                mainPageKernelDrvMgr.KernelListInit();
            }
            else if (e.TabPage == tabPageDetals)
            {
                mainPageProcessDetails.ProcessListDetailsInit();
            }
            else if (e.TabPage == tabPageUsers)
            {
                mainPageUserMgr.UsersListInit();
            }
        }
    }
}
