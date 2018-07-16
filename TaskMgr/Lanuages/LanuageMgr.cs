using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace PCMgr.Lanuages
{
    class LanuageMgr
    {
        private static ResourceManager resLG;

        public static bool LoadLanuageResource(string lg)
        {
            try
            {
                switch(lg)
                {
                    case "zh":
                    case "zh-CN":
                        resLG = new ResourceManager(typeof(LanuageResource_zh));
                        return true;
                    case "en":
                    case "en-US":
                        resLG = new ResourceManager(typeof(LanuageResource_en));
                        return true;
                    default:
                        resLG = new ResourceManager("PCMgrLanuage.LanuageResource_" + lg, System.Reflection.Assembly.GetExecutingAssembly());
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
