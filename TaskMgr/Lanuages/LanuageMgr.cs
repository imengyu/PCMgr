using System.Resources;

namespace PCMgr.Lanuages
{
    class LanuageMgr
    {
        private static ResourceManager resLG;

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
        public static string GetStr(string name)
        {
            return resLG.GetString(name);
        }
        public static string GetStr2(string name, out int size)
        {
            string s = resLG.GetString(name);
            size = s.Length + 1;
            return s;
        }
    }
}
