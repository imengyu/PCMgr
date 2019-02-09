using PCMgr.Aero.TaskDialog;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.Main.MainUtils;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.CSCall;
using static PCMgr.NativeMethods.Win32;

namespace PCMgr.Main
{
    internal class MainNativeBridge
    {
        public void InitCallbacks()
        {
            workerCallBack = AppWorkerCallBack;
            lanuageItems_CallBack = Native_LanuageItems_CallBack;

            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(exitCallBack), 1);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(terminateImporantWarnCallBack), 2);
            MAppSetCallBack(Marshal.GetFunctionPointerForDelegate(workerCallBack), 5);
            MLG_SetLanuageItems_CallBack(Marshal.GetFunctionPointerForDelegate(lanuageItems_CallBack));
        }

        public MainNativeBridge(FormMain m)
        {
            formMain = m;
        }

        public void WndProc(ref System.Windows.Forms.Message m)
        {
            //WndProc部分交予 Native 控制
            coreWndProc?.Invoke(m.HWnd, Convert.ToUInt32(m.Msg), m.WParam, m.LParam);
        }

        private string Native_LanuageItems_CallBack(string s)
        {
            return LanuageMgr.GetStr(s, false);
        }

        private FormMain formMain = null;
        private FormTcp formTcp = null;

        //所有 Native 回调

        public static LanuageItems_CallBack lanuageItems_CallBack;
        public EnumUsersCallBack enumUsersCallBack;

        public MProcessMonitor.ProcessMonitorNewItemCallBack ProcessNewItemCallBackDetails;
        public MProcessMonitor.ProcessMonitorRemoveItemCallBack ProcessRemoveItemCallBackDetails;

        public MProcessMonitor.ProcessMonitorNewItemCallBack ProcessNewItemCallBack;
        public MProcessMonitor.ProcessMonitorRemoveItemCallBack ProcessRemoveItemCallBack;

        public EnumWinsCallBack enumWinsCallBack;
        public GetWinsCallBack getWinsCallBack;

        public IntPtr enumUsersCallBackCallBack_ptr;
        public IntPtr ptrProcessNewItemCallBack;
        public IntPtr ptrProcessRemoveItemCallBack;
        public IntPtr ptrProcessNewItemCallBackDetails;
        public IntPtr ptrProcessRemoveItemCallBackDetails;
        public IntPtr enumStartupsCallBackPtr = IntPtr.Zero;
        public IntPtr scMgrEnumServicesCallBackPtr = IntPtr.Zero;
        public IntPtr enumKernelModulsCallBackPtr = IntPtr.Zero;

        public WNDPROC coreWndProc = null;
        public EXITCALLBACK exitCallBack;
        public WORKERCALLBACK workerCallBack;
        public TerminateImporantWarnCallBack terminateImporantWarnCallBack;
        public MFCALLBACK fileMgrCallBack;

        public EnumServicesCallBack scMgrEnumServicesCallBack;
        public EnumStartupsCallBack enumStartupsCallBack;
        public EnumKernelModulsCallBack enumKernelModulsCallBack;


        //Worker Callback
        public void AppWorkerCallBack(int msg, IntPtr wParam, IntPtr lParam)
        {
            //这是从 c++ native 调用回来的函数
            switch (msg)
            {
                case M_CALLBACK_SW_AOP_WND: break;
                case M_CALLBACK_CLEAR_ILLEGAL_TOP_WND:
                    {
                        StringBuilder sb = new StringBuilder(260);
                        GetWindowText(wParam, sb, 260);
                        FormWindowKillAsk fa = new FormWindowKillAsk("窗口名称 ：" + sb.ToString(), wParam);
                        fa.Show();
                        MAppWorkCall3(213, fa.Handle);
                        break;
                    }
                case M_CALLBACK_SWITCH_IDM_ALWAYSTOP_SET: break;
                case M_CALLBACK_SWITCH_MAINGROUP_SET: formMain.GroupSwitch(wParam.ToInt32() == 1); break;
                case M_CALLBACK_SWITCH_REFESHRATE_SET: formMain.SetMainRefeshTimerInterval(wParam.ToInt32()); break;
                case M_CALLBACK_SWITCH_TOPMOST_SET: formMain.TopMost = wParam.ToInt32() == 1; break;
                case M_CALLBACK_SWITCH_CLOSEHIDE_SET: formMain.MainSettings.CloseHide = wParam.ToInt32() == 1; break;
                case M_CALLBACK_SWITCH_MINHIDE_SET: formMain.MainSettings.MinHide = wParam.ToInt32() == 1; break;
                case M_CALLBACK_GOTO_SERVICE: formMain.MainPageScMgr.ScMgrGoToService(Marshal.PtrToStringUni(wParam)); break;
                case M_CALLBACK_REFESH_SCLIST: formMain.MainPageScMgr.ScMgrRefeshList(); break;
                case M_CALLBACK_KILLPROCTREE: formMain.MainPageProcess.ProcessListKillProcTree((uint)wParam.ToInt32()); break;
                case M_CALLBACK_SPY_TOOL:
                    {
                        new FormSpyWindow(wParam).ShowDialog();
                        break;
                    }
                case M_CALLBACK_FILE_TOOL:
                    {
                        new FormFileTool().ShowDialog();
                        break;
                    }
                case M_CALLBACK_ABOUT:
                    {
                        new FormAbout().ShowDialog();
                        break;
                    }
                case M_CALLBACK_ENDTASK: formMain.MainPageProcess.ProcessListEndTask(Convert.ToUInt32(wParam.ToInt32())); break;
                case M_CALLBACK_LOADDRIVER_TOOL: new FormLoadDriver().Show(); break;
                case M_CALLBACK_SCITEM_REMOVED: formMain.MainPageScMgr.ScMgrRemoveInvalidItem(Marshal.PtrToStringUni(wParam)); break;
                case M_CALLBACK_SHOW_PROGRESS_DLG: formMain.DelingDialogShowHide(true); break;
                case M_CALLBACK_UPDATE_PROGRESS_DLG_TO_DELETEING:
                    {
                        formMain.DelingDialogShowHide(false);
                        formMain.DelingDialogUpdate(LanuageMgr.GetStr("DeleteFiles"), 0);
                        break;
                    }
                case M_CALLBACK_UPDATE_PROGRESS_DLG_ALL: formMain.DelingDialogUpdate(Marshal.PtrToStringUni(wParam), lParam.ToInt32()); break;
                case M_CALLBACK_UPDATE_PROGRESS_DLG_TO_COLLECTING: formMain.DelingDialogUpdate(LanuageMgr.GetStr("CollectingFiles"), -1); break;
                case M_CALLBACK_KERNEL_INIT:
                    {
                        AppWorkerCallBack(41, IntPtr.Zero, IntPtr.Zero);
                        if (MInitKernel(null))
                            if (GetConfigBool("SelfProtect", "AppSetting"))
                                MAppWorkCall3(203, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                case M_CALLBACK_VIEW_HANDLES: new FormVHandles(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog(); break;
                case M_CALLBACK_KERNEL_INIT_LIST: formMain.MainPageKernelDrvMgr.KernelListInit(); break;
                case M_CALLBACK_KERNEL_SWITCH_SHOWALLDRV: formMain.MainPageKernelDrvMgr.KernelListToggleShowAllDrv(); break;
                case M_CALLBACK_START_ITEM_REMVED: formMain.MainPageStartMgr.StartMListRemoveItem(Convert.ToUInt32(wParam.ToInt32())); break;
                case M_CALLBACK_VIEW_KSTRUCTS:
                    {
                        if (MCanUseKernel()) new FormVKrnInfo(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog();
                        else MessageBox.Show(LanuageFBuffers.Str_DriverNotLoad);
                        break;
                    }
                case M_CALLBACK_VIEW_TIMER:
                    {
                        //timer
                        if (MCanUseKernel()) new FormVTimers(Convert.ToUInt32(wParam.ToInt32())).ShowDialog();
                        else MessageBox.Show(LanuageFBuffers.Str_DriverNotLoad);
                        break;
                    }
                case M_CALLBACK_VIEW_HOTKEY:
                    {
                        //hotkey
                        if (MCanUseKernel()) new FormVHotKeys(Convert.ToUInt32(wParam.ToInt32())).ShowDialog();
                        else MessageBox.Show(LanuageFBuffers.Str_DriverNotLoad);
                        break;
                    }
                case M_CALLBACK_SHOW_TRUSTED_DLG: formMain.VeryTrust(Marshal.PtrToStringUni(wParam)); break;
                case M_CALLBACK_MDETALS_LIST_HEADER_RIGHTCLICK: formMain.MainPageProcessDetails.ProcessListDetailsHeaderRightClick(wParam.ToInt32()); break;
                case M_CALLBACK_KDA: new FormKDA().ShowDialog(formMain); break;
                case M_CALLBACK_SETAFFINITY: new FormSetAffinity((uint)wParam.ToInt32(), lParam).ShowDialog(); break;
                case M_CALLBACK_UPDATE_LOAD_STATUS: formMain.StartingProgressUpdate(Marshal.PtrToStringUni(wParam)); break;
                case M_CALLBACK_SHOW_NOPDB_WARN: formMain.ShowNoPdbWarn(Marshal.PtrToStringAnsi(wParam)); break;
                case M_CALLBACK_INVOKE_LASTLOAD_STEP: formMain.Invoke(new Action(formMain.AppLastLoadStep)); break;
                case M_CALLBACK_DBGPRINT_SHOW: formMain.KDbgPrintShow(); break;
                case M_CALLBACK_DBGPRINT_CLOSE: formMain.KDbgPrintClose(); break;
                case M_CALLBACK_DBGPRINT_DATA: formMain.KDbgPrintData(Marshal.PtrToStringUni(wParam)); break;
                case M_CALLBACK_DBGPRINT_EMEPTY: formMain.KDbgPrintData(); break;
                case M_CALLBACK_SHOW_LOAD_STATUS: formMain.StartingProgressShowHide(true); break;
                case M_CALLBACK_HLDE_LOAD_STATUS: formMain.StartingProgressShowHide(false); break;
                case M_CALLBACK_MDETALS_LIST_HEADER_MOUSEMOVE: formMain.MainPageProcessDetails.ProcessListDetailsHeaderMouseMove(wParam.ToInt32(), new System.Drawing.Point(LOWORD(lParam), HIWORD(lParam))); break;
                case M_CALLBACK_KERNEL_VIELL_PRGV: new FormVPrivilege(Convert.ToUInt32(wParam.ToInt32()), Marshal.PtrToStringUni(lParam)).ShowDialog(); break;
                case M_CALLBACK_KERNEL_TOOL: formMain.ShowKernelTools(); break;
                case M_CALLBACK_HOOKS: formMain.ShowFormHooks(); break;
                case M_CALLBACK_NETMON: /*netmon*/ break;
                case M_CALLBACK_REGEDIT: /*regedit*/ break;
                case M_CALLBACK_FILEMGR: formMain.SetToFileMgr(); break;
                case M_CALLBACK_COLLAPSE_ALL: formMain.CollapseAll(); break;
                case M_CALLBACK_SIMPLEVIEW_ACT:
                    {
                        if (wParam.ToInt32() == 1) formMain.MainPageProcess.ProcessListEndCurrentApp();
                        else if (wParam.ToInt32() == 0) formMain.MainPageProcess.ProcessListSetToCurrentApp();
                        break;
                    }
                case M_CALLBACK_UWPKILL: formMain.MainPageProcess.ProcessListKillCurrentUWP(); break;
                case M_CALLBACK_EXPAND_ALL: formMain.ExpandAll(); break;
                case M_CALLBACK_SHOW_HELP: new FormHelp().ShowDialog(); break;
                case M_CALLBACK_RUN_APP_CMD:
                    {
                        List<string> cmds = new List<string>();

                        StringBuilder sb = new StringBuilder(260);
                        uint count = MAppCmdGetCount();
                        for (uint i = 0; i < count; i++)
                        {
                            sb.Clear();
                            if (MAppCmdGetAt(i, sb, 260))
                                cmds.Add(sb.ToString());
                        }

                        formMain.AppDebugCmd(cmds.ToArray(), count);

                        cmds.Clear();
                        cmds = null;

                        break;
                    }
                case M_CALLBACK_VIEW_TCP:
                    {
                        if (formTcp == null)
                        {
                            formTcp = new FormTcp(formMain);
                            formTcp.FormClosed += FormTcp_FormClosed;
                        }
                        formTcp.Show();
                        break;
                    }
                case M_CALLBACK_RELOAD_PERF_DEVICE_LIST:
                    {
                        if (formMain.MainPagePerf.Inited)
                            formMain.MainPagePerf.PerfReloadMoveableDevices();
                        break;
                    }
            }
        }

        private void FormTcp_FormClosed(object sender, FormClosedEventArgs e)
        {
            formTcp = null;
        }
    }
}
