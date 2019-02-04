using System;
using System.Collections.Generic;
using System.Resources;
using System.Windows.Forms;
using static PCMgr.NativeMethods.LogApi;
using static PCMgr.NativeMethods;

namespace PCMgr.Lanuages
{
    class LanuageMgr
    {
        private static ResourceManager resLG;
        private static Dictionary<string, string> lanuageBuffers = new Dictionary<string, string>();
        private static List<string> lanuageBadRes = new List<string>();

        private static int cacheUseage = 0;
        private static int badResource = 0;

        private static void AddBadRes(string s)
        {
            badResource++;
            lanuageBadRes.Add(s);
        }

        public static string CurrentLanuage { get; private set; }
        public static bool IsChinese { get; private set; }

        public static bool LoadLanuageResource(string lg)
        {
            try
            {
                CurrentLanuage = lg;
                switch (lg)
                {
                    case "zh":
                    case "zh-CN":
                        resLG = new ResourceManager(typeof(LanuageResource_zh));
                        IsChinese = true;
                        return true;
                    case "en":
                    case "en-US":
                        resLG = new ResourceManager(typeof(LanuageResource_en));
                        return true;
                    default:
                        resLG = new ResourceManager("PCMgrLanuage.LanuageResource_" + lg, System.Reflection.Assembly.GetExecutingAssembly());
                        IsChinese = true;
                        return true;
                }
            }
            catch
            {
                try
                {
                    resLG = new ResourceManager(typeof(LanuageResource_zh));
                    return true;
                }
                catch
                {

                }
                return false;
            }
        }

        public static string GetStr(string name, bool buffer = true)
        {
            if (!buffer)
            {
                string s1 = resLG.GetString(name);
                if (s1 == null) { s1 = "[" + name + "::ResoureceNotFounnd]"; AddBadRes(s1); }
                return s1;
            }
            if (lanuageBuffers.ContainsKey(name))
            {
                cacheUseage++;
                return lanuageBuffers[name];
            }
            string s = resLG.GetString(name);
            if (s != null) lanuageBuffers.Add(name, s);
            else { s = "[" + name + "::ResoureceNotFounnd]"; AddBadRes(s); }
            return s;
        }
        public static string GetStr2(string name, out int size)
        {
            string s = GetStr(name);
            size = s.Length + 1;
            return s;
        }

        public static void InitLanuage()
        {
            string lanuage = GetConfig("Lanuage", "AppSetting");
            if (lanuage != "")
            {
                try
                {
                    Log("Loading Lanuage Resource : " + lanuage);
                    LoadLanuageResource(lanuage);
                }
                catch (Exception e)
                {
                    LogErr2("Lanuage resource load failed !\n" + e.ToString());
                    MessageBox.Show(e.ToString(), "ERROR !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                LoadLanuageResource("zh");
                SetConfig("Lanuage", "AppSetting", "zh");
                LogWarn("Not found Lanuage settings , use default zh-CN .");
            }

            InitLanuageItems();
            if (lanuage != "" && lanuage != "zh" && lanuage != "zh-CN")
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lanuage);
        }
        private static void InitLanuageItems()
        {
            try
            {
                LanuageFBuffers.LoadFBuffers();

                MLG_SetLanuageItems_CanRealloc();

                MAppSetLanuageItems(0, 0, GetStr("KillAskStart"), 0);
                MAppSetLanuageItems(0, 1, GetStr("KillAskEnd"), 0);
                MAppSetLanuageItems(0, 2, GetStr("KillAskContent"), 0);
                MAppSetLanuageItems(0, 3, GetStr("KillFailed"), 0);
                MAppSetLanuageItems(0, 4, GetStr("AccessDenied"), 0);
                MAppSetLanuageItems(0, 5, GetStr("OpFailed"), 0);
                MAppSetLanuageItems(0, 6, GetStr("InvalidProcess"), 0);
                MAppSetLanuageItems(0, 7, GetStr("CantCopyFile"), 0);
                MAppSetLanuageItems(0, 8, GetStr("CantMoveFile"), 0);
                MAppSetLanuageItems(0, 9, GetStr("ChooseTargetDir"), 0);

                int size = 0;
                MAppSetLanuageItems(1, 0, GetStr2("Moveing", out size), size);
                MAppSetLanuageItems(1, 1, GetStr2("Copying", out size), size);
                MAppSetLanuageItems(1, 2, GetStr2("FileExist", out size), size);
                MAppSetLanuageItems(1, 3, GetStr2("FileExist2", out size), size);
                MAppSetLanuageItems(1, 4, GetStr2("TitleQuestion", out size), size);
                MAppSetLanuageItems(1, 5, GetStr2("TipTitle", out size), size);
                MAppSetLanuageItems(1, 6, GetStr2("DelSure", out size), size);
                MAppSetLanuageItems(1, 7, GetStr2("DelAsk1", out size), size);
                MAppSetLanuageItems(1, 8, GetStr2("DelAsk2", out size), size);
                MAppSetLanuageItems(1, 9, GetStr2("DelAsk3", out size), size);
                MAppSetLanuageItems(1, 10, GetStr2("DeleteIng", out size), size);
                MAppSetLanuageItems(1, 11, GetStr2("NoAdminTipText", out size), size);
                MAppSetLanuageItems(1, 12, GetStr2("NoAdminTipTitle", out size), size);
                MAppSetLanuageItems(1, 13, GetStr2("DelFailed", out size), size);
                MAppSetLanuageItems(1, 14, LanuageFBuffers.Str_IdleProcess, LanuageFBuffers.Str_IdleProcess.Length + 1);
                MAppSetLanuageItems(1, 15, GetStr2("EndProcFailed", out size), size);
                MAppSetLanuageItems(1, 16, GetStr2("OpenProcFailed", out size), size);
                MAppSetLanuageItems(1, 17, GetStr2("SusProcFailed", out size), size);
                MAppSetLanuageItems(1, 18, GetStr2("ResProcFailed", out size), size);
                MAppSetLanuageItems(1, 19, GetStr2("MenuRebootAsAdmin", out size), size);
                MAppSetLanuageItems(1, 20, GetStr2("Visible", out size), size);
                MAppSetLanuageItems(1, 21, GetStr2("CantGetPath", out size), size);
                MAppSetLanuageItems(1, 22, GetStr2("FreeLibSuccess", out size), size);
                MAppSetLanuageItems(1, 23, GetStr2("Priority", out size), size);
                MAppSetLanuageItems(1, 24, GetStr2("EntryPoint", out size), size);
                MAppSetLanuageItems(1, 25, GetStr2("ModuleName", out size), size);
                MAppSetLanuageItems(1, 26, GetStr2("State", out size), size);
                MAppSetLanuageItems(1, 27, GetStr2("ContextSwitch", out size), size);
                MAppSetLanuageItems(1, 28, GetStr2("ModulePath", out size), size);
                MAppSetLanuageItems(1, 29, GetStr2("Address", out size), size);
                MAppSetLanuageItems(1, 30, GetStr2("Size", out size), size);
                MAppSetLanuageItems(1, 31, GetStr2("TitlePublisher", out size), size);
                MAppSetLanuageItems(1, 32, GetStr2("WindowText", out size), size);
                MAppSetLanuageItems(1, 33, GetStr2("WindowHandle", out size), size);
                MAppSetLanuageItems(1, 34, GetStr2("WindowClassName", out size), size);
                MAppSetLanuageItems(1, 35, GetStr2("BelongThread", out size), size);
                MAppSetLanuageItems(1, 36, GetStr2("VWinTitle", out size), size);
                MAppSetLanuageItems(1, 37, GetStr2("VModulTitle", out size), size);
                MAppSetLanuageItems(1, 38, GetStr2("VThreadTitle", out size), size);
                MAppSetLanuageItems(1, 39, GetStr2("EnumModuleFailed", out size), size);
                MAppSetLanuageItems(1, 40, GetStr2("EnumThreadFailed", out size), size);
                MAppSetLanuageItems(1, 41, GetStr2("FreeInvalidProc", out size), size);
                MAppSetLanuageItems(1, 42, GetStr2("FreeFailed", out size), size);
                MAppSetLanuageItems(1, 43, GetStr2("KillThreadError", out size), size);
                MAppSetLanuageItems(1, 44, GetStr2("KillThreadInvThread", out size), size);
                MAppSetLanuageItems(1, 45, GetStr2("OpenThreadFailed", out size), size);
                MAppSetLanuageItems(1, 46, GetStr2("SuThreadErr", out size), size);
                MAppSetLanuageItems(1, 47, GetStr2("ReThreadErr", out size), size);
                MAppSetLanuageItems(1, 48, GetStr2("InvThread", out size), size);
                MAppSetLanuageItems(1, 49, GetStr2("SuThreadWarn", out size), size);
                MAppSetLanuageItems(1, 50, GetStr2("KernelNotLoad", out size), size);

                MAppSetLanuageItems(2, 0, GetStr2("DelStartupItemAsk", out size), size);
                MAppSetLanuageItems(2, 1, GetStr2("DelStartupItemAsk2", out size), size);
                MAppSetLanuageItems(2, 2, LanuageFBuffers.Str_Endtask, LanuageFBuffers.Str_Endtask.Length + 1);
                MAppSetLanuageItems(2, 3, LanuageFBuffers.Str_Resrat, LanuageFBuffers.Str_Resrat.Length + 1);
                MAppSetLanuageItems(2, 4, GetStr2("LoadDriver", out size), size);
                MAppSetLanuageItems(2, 5, GetStr2("UnLoadDriver", out size), size);
                MAppSetLanuageItems(2, 6, LanuageFBuffers.Str_FileNotExist, LanuageFBuffers.Str_FileNotExist.Length + 1);
                MAppSetLanuageItems(2, 7, GetStr2("FileTrust", out size), size);
                MAppSetLanuageItems(2, 8, GetStr2("FileNotTrust", out size), size);
                MAppSetLanuageItems(2, 9, GetStr2("OpenServiceError", out size), size);
                MAppSetLanuageItems(2, 10, GetStr2("DelScError", out size), size);
                MAppSetLanuageItems(2, 11, GetStr2("ChangeScStartTypeFailed", out size), size);
                MAppSetLanuageItems(2, 12, GetStr2("SetTo", out size), size);
                MAppSetLanuageItems(2, 13, GetStr2("KillTreeAskEnd", out size), size);
                MAppSetLanuageItems(2, 14, GetStr2("KillTreeContent", out size), size);
                MAppSetLanuageItems(2, 15, GetStr2("WantDisconnectUser", out size), size);
                MAppSetLanuageItems(2, 16, GetStr2("WantLogooffUser", out size), size);
                MAppSetLanuageItems(2, 17, GetStr2("PleaseEnterPassword", out size), size);
                MAppSetLanuageItems(2, 18, GetStr2("ConnectSessionFailed", out size), size);
                MAppSetLanuageItems(2, 19, GetStr2("ConnectSession", out size), size);
                MAppSetLanuageItems(2, 20, GetStr2("DisConnectSession", out size), size);
                MAppSetLanuageItems(2, 21, GetStr2("DisConnectSessionFailed", out size), size);
                MAppSetLanuageItems(2, 22, GetStr2("LogoffSession", out size), size);
                MAppSetLanuageItems(2, 23, GetStr2("DisConnectSessionFailed1", out size), size);
                MAppSetLanuageItems(2, 24, GetStr2("SetProcPriorityClassFailed", out size), size);
                MAppSetLanuageItems(2, 25, GetStr2("SetProcAffinityFailed", out size), size);
                MAppSetLanuageItems(2, 26, GetStr2("WarnTitle", out size), size);
                MAppSetLanuageItems(2, 27, GetStr2("LoadDriverWarn", out size), size);
                MAppSetLanuageItems(2, 28, GetStr2("LoadDriverWarnTitle", out size), size);
                MAppSetLanuageItems(2, 29, GetStr2("DetachDebuggerTitle", out size), size);
                MAppSetLanuageItems(2, 30, GetStr2("DetachDebuggerError", out size), size);
                MAppSetLanuageItems(2, 31, GetStr2("DetachDebuggerNotDebugger", out size), size);
                MAppSetLanuageItems(2, 32, GetStr2("ChangePriorityAsk", out size), size);
                MAppSetLanuageItems(2, 33, GetStr2("ChangePriorityContent", out size), size);
                MAppSetLanuageItems(2, 34, GetStr2("OpenFileError", out size), size);
                MAppSetLanuageItems(2, 35, GetStr2("CreateDumpFailed", out size), size);
                MAppSetLanuageItems(2, 36, GetStr2("CreateDumpSuccess", out size), size);
                MAppSetLanuageItems(2, 37, GetStr2("PleaseEnumIn64", out size), size);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ERROR !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void DebugCmd(string[] cmd, uint size)
        {
            if (size >= 2)
            {
                string cmd1 = cmd[1];
                switch (cmd1)
                {
                    case "viewbadres":
                        {
                            string ss = "All bad resource key (" + badResource + "):";
                            foreach (string s in lanuageBadRes)
                                ss += s + "\n";
                            LogText(ss);
                            break;
                        }
                    case "get":
                        if (size >= 3) LogText("Lanuage resource for " + cmd[2] + " : " + GetStr(cmd[2], false));
                        else LogErr("Invalid input resource name.");
                        break;
                    case "clearbuf":
                        lanuageBuffers.Clear();
                        Log("LanuageMgr buffer has been emptied.");
                        break;
                    case "buffer":
                        LogText("LanuageMgr buffer stats" +
                            "\nBuffer count : " + lanuageBuffers.Count + "" +
                            "\nCache Useage : " + cacheUseage + "" +
                            "\nBad Resource : " + badResource + "" + "");
                        break;
                    case "vbuffer":
                        {
                            string ss = "All lanuage buffer (" + lanuageBuffers.Count + ") :";
                            foreach (string s in lanuageBuffers.Keys)
                                ss += s + "\n";
                            LogText(ss);
                            break;
                        }
                    case "?":
                    case "help":
                        LogText("app lg commands help: \n" +
                            "\nviewbadres view bad res" +
                            "\nget [string:reskey] get lanuage string resoure" +
                            "\nclearbuf clear all resoure caches" +
                            "\nbuffer LanuageMgr cache status" +
                            "\nvbuffer list LanuageMgr cache");
                        break;
                }
            }
            else LogText("LanuageMgr.CurrentLanuage : " + CurrentLanuage);
        }
    }
}
