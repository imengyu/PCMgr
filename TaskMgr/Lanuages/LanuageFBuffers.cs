using System;
using System.Reflection;

namespace PCMgr.Lanuages
{
    static class LanuageFBuffers
    {
        private class LanuageStrAttribute : Attribute
        {
            public string LanuageResName { get; private set; }

            public LanuageStrAttribute(string lanuageResName)
            {
                LanuageResName = lanuageResName;
            }
        }

        public static void LoadFBuffers()
        {
            Type t = typeof(LanuageFBuffers);
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                LanuageStrAttribute attr = (LanuageStrAttribute)field.GetCustomAttribute(typeof(LanuageStrAttribute));
                if (attr != null && attr.LanuageResName != "") field.SetValue(null, LanuageMgr.GetStr(attr.LanuageResName, false));
                else field.SetValue(null, LanuageMgr.GetStr(field.Name.Remove(0, 4), false));
            }
        }

        public static string Str_AppTitle = "";
        public static string Str_Yes = "";
        public static string Str_No = "";
        public static string Str_Cancel = "";
        public static string Str_Close = "";
        [LanuageStr("OpFailed")]
        public static string Str_Failed = "";
        public static string Str_FileNotExist = "";
        public static string Str_DriverLoad = "";
        public static string Str_DriverNotLoad = "";
        public static string Str_DriverCountLoaded = "";
        public static string Str_DriverCount = "";
        public static string Str_AutoStart = "";
        public static string Str_DemandStart = "";
        public static string Str_Disabled = "";
        public static string Str_FileSystem = "";
        public static string Str_KernelDriver = "";
        public static string Str_UserService = "";
        public static string Str_SystemService = "";
        public static string Str_ErrTitle = "";
        public static string Str_AskTitle = "";
        public static string Str_TipTitle = "";
        public static string Str_Ready = "";
        public static string Str_ReadyStatus = "";
        public static string Str_ReadyStatusEnd2 = "";
        public static string Str_ReadyStatusEnd = "";
        public static string Str_VisitFolderFailed = "";
        public static string Str_Loading = "";
        public static string Str_ProcessCount = "";
        public static string Str_InterruptsProcessDsb = "";
        public static string Str_SystemInterrupts = "";
        public static string Str_IdleProcessDsb = "";

        [LanuageStr("SystemIdleProcess")]
        public static string Str_IdleProcess = "";
        public static string Str_StatusRunning = "";
        public static string Str_StatusStopped = "";
        public static string Str_StatusPaused = "";
        [LanuageStr("StatusHang")]
        public static string Str_StatusHung = "";

        [LanuageStr("BtnEndTask")]
        public static string Str_Endtask = "";
        [LanuageStr("BtnEndProcess")]
        public static string Str_Endproc = "";
        [LanuageStr("BtnRestart")]
        public static string Str_Resrat = "";

        public static string Str_Second = "";
        public static string Str_ServiceHost = "";
        public static string Str_Process32Bit = "";
        public static string Str_Process64Bit = "";
    }
}
