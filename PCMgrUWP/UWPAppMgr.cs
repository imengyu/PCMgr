using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static PCMgrUWP.NativeMethods;

namespace PCMgrUWP
{
    public class UWPPackage
    {
        public string Name = "";
        public string FullName = "";
        public string Publisher = "";
        public string InstalledLocation = "";
        public string IconPath = null;
        public string MainAppDisplayName = "";

        public string[] Apps = new string[0];

        public override string ToString()
        {
            return FullName;
        }
    }
    public class UWPManager
    {
        private PackageManager packageManager = new PackageManager();
        private bool enumlated = false;

        private int packageCount = 0;
        private static string stringLocate = "zh-CN";
        private List<UWPPackage> packageDatas = new List<UWPPackage>();

        public int PackageCount { get { return packageCount; } }
        public List<UWPPackage> Packages { get { return packageDatas; } }

        /// <summary>
        /// 获取或设置查找字符串的地区语言
        /// </summary>
        public static string StringLocate
        {
            get { return stringLocate; }
            set { stringLocate = value; }
        }
        public static string DisplayNameTozhCN(string dsb)
        {
            /*switch(dsb)
            {
                case "Get Help":
                    return "获取帮助";
                case "My Office":
                    return "我的 Office";
                case "Windows Alarms & Clock":
                    return "闹钟和时钟";
                case "Windows Voice Recorder":
                    return "录音机";
                case "Mobile Plans":
                    return "移动套餐";
                case "Micrsoft Pepole":
                    return "人脉";
                case "Windows Calculator":
                    return "计算器";
                case "Micrsoft Photos":
                    return "Micrsoft 照片";
                case "Windows Maps":
                    return "地图";
                case "Groove Music":
                    return "Groove 音乐";
                case "Movies & TV":
                    return "电影和电视";
                case "Micrsoft Tips":
                    return "使用技巧";
                case "Mail and Calendar":
                    return "邮件和日历";
                case "Windows Camera":
                    return "相机";
            }*/
            return dsb;
        }

        /// <summary>
        /// 导出图标
        /// </summary>
        /// <param name="dir">UWP 安装目录</param>
        /// <param name="package"></param>
        /// <param name="logoPath">图标位置</param>
        /// <returns></returns>
        private static string ExtractDisplayIcon(string dir, Package package, string logoPath)
        {
            /*string path = "";
            logoPath = logoPath.Replace("\\", "/");
            if (logoPath.StartsWith("ms-resource:"))
            {
                path = ExtractStringFromPRIFile(package.Id.FullName, logoPath);
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
            else
            {
                path = ExtractStringFromPRIFile(package.Id.FullName, "ms-resource://" + logoPath);
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
            return path;*/
            //if (string.IsNullOrWhiteSpace(path)) 
            {
                //Force search
                var imageFile = Path.Combine(dir, logoPath);
                var dir2 = Path.GetDirectoryName(imageFile);
                var name = Path.GetFileName(imageFile);
                var
                scaleImage = Path.ChangeExtension(imageFile, "scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-200_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-100_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                imageFile = Path.Combine(dir + "\\contrast-black", name);
                scaleImage = Path.ChangeExtension(imageFile, "scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-200_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-100_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                imageFile = Path.Combine(dir2 + "\\contrast-black", name);
                scaleImage = Path.ChangeExtension(imageFile, "scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-200_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "scale-100_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
                imageFile = Path.Combine(dir, logoPath);

                if (MFM_FileExist(imageFile)) return imageFile;

                imageFile = Path.Combine(dir, "en-us", logoPath);
                if (MFM_FileExist(imageFile)) return imageFile;

                return Path.ChangeExtension(imageFile, "Square44x44Logo.png");
            }
        }
        /// <summary>
        /// 导出 ms-resource: 的字符串资源
        /// </summary>
        /// <param name="dir">UWP 安装目录</param>
        /// <param name="package">包</param>
        /// <param name="resource">resource key</param>
        /// <returns></returns>
        private static string ExtractMSResourceString(string dir, Package package, string resource)
        {
            if (resource.StartsWith("ms-resource:"))
            {
                var priPath = dir + "\\pris\\resources." + stringLocate + ".pri";
                if (!MFM_FileExist(priPath)) priPath = dir + "\\resources.pri";

                if (resource.Contains("/"))
                {
                    //检查reskey是否合法
                    string resourceRevStart = resource.Replace("ms-resource:", "");//去掉msresource
                    string[] resourceSps = resourceRevStart.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (resourceSps.Length > 0 && resourceSps[0] == package.Id.Name)
                    {
                        //说明开头是package.Id.Name
                        string resourceKeyReal = "ms-resource:";
                        if (resourceRevStart.Contains("ms-resource:"))
                            resourceRevStart = resourceRevStart.Replace("ms-resource:", "");
                        resourceKeyReal += resourceRevStart;
                        string name = ExtractStringFromPRIFile(priPath, resourceKeyReal);
                        if (!string.IsNullOrWhiteSpace(name))
                            return name;//成功返回
                    }
                    else
                    {
                        //说明开头不是是package.Id.Name，需要添加
                        string resourceKeyReal = "ms-resource://" + package.Id.Name;
                        foreach (string s in resourceSps)
                            if (s.Contains("ms-resource:"))
                                resourceKeyReal += "/" + s.Replace("ms-resource:", "");
                            else resourceKeyReal += "/" + s;
                        string name = ExtractStringFromPRIFile(priPath, resourceKeyReal);
                        if (!string.IsNullOrWhiteSpace(name))
                            return name;//成功返回
                    }
                }
                else
                {
                    string name = "";

                    string reskeyold = resource.StartsWith("ms-resource:") ? resource.Replace("ms-resource:", "") : resource;
                    //if (reskeyold == "AppxManifest_DisplayName")
                    //    resource = string.Format("ms-resource://{0}/resources/DisplayName", package.Id.Name);
                    //else
                        resource = string.Format("ms-resource://{0}/resources/{1}", package.Id.Name, reskeyold);
                    name = ExtractStringFromPRIFile(priPath, resource);
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            return resource;
        }

        /// <summary>
        /// 清空上一次的枚举结果
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            if (enumlated)
            {
                enumlated = false;
                packageDatas.Clear();
            }
            return !enumlated;
        }
        /// <summary>
        /// 枚举所有 UWP 应用
        /// </summary>
        /// <returns>返回是否成功</returns>
        public bool EnumlateAll()
        {
            if (!enumlated)
            {
                var userSecurityId = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;
                var packages = packageManager.FindPackagesForUserWithPackageTypes(userSecurityId, PackageTypes.Main | PackageTypes.Bundle);
                foreach (var package in packages)
                {
                    UWPPackage p = new UWPPackage();
                    string logoPath = "";
                    string dsbName = "";
                    string dsbPublisher = "";
                    string dsbMainApp = "";

                    string dir = package.InstalledLocation.Path;
                    string file = Path.Combine(dir, "AppxManifest.xml");

                    if (MFM_FileExist(file))
                    {
                        //Load xml to read AppxManifest
                        XmlDocument xml = new XmlDocument();
                        xml.Load(file);
                        XmlNode nRoot = xml.ChildNodes[1].Name == "Package" ? xml.ChildNodes[1] : xml.ChildNodes[0];
                        XmlNode nProp = null;
                        foreach (XmlNode n in nRoot)
                            if (n.Name == "Properties")
                            {
                                nProp = n;
                                break;
                            }
                        if (nProp != null)
                            for (int i = 0; i < nProp.ChildNodes.Count; i++)
                            {
                                XmlNode propItem = nProp.ChildNodes[i];
                                if (propItem.Name == "DisplayName")
                                    dsbName = propItem.InnerText;
                                else if (propItem.Name == "PublisherDisplayName")
                                    dsbPublisher = propItem.InnerText;
                                else if (propItem.Name == "Logo")
                                    logoPath = propItem.InnerText;
                            }
                        XmlNode nApps = null;
                        foreach (XmlNode n in nRoot)
                            if (n.Name == "Applications")
                            {
                                nApps = n;
                                break;
                            }
                        if (nApps != null)
                        {
                            List<string> apps = new List<string>();
                            for (int i = 0; i < nApps.ChildNodes.Count; i++)
                            {
                                XmlNode apptem = nApps.ChildNodes[i];
                                if (apptem.Name == "Application")
                                {
                                    if (apptem.Attributes["Id"] != null)
                                        apps.Add(apptem.Attributes["Id"].InnerText);
                                    for (int i1 = 0; i1 < apptem.ChildNodes.Count; i1++)
                                    {
                                        XmlNode apppropItem = apptem.ChildNodes[i1];
                                        if (apppropItem.Name == "uap:VisualElements" || apppropItem.Name == "VisualElements")
                                        {
                                            if (apppropItem.Attributes["DisplayName"] != null)
                                                dsbMainApp = apppropItem.Attributes["DisplayName"].InnerText;
                                        }
                                    }
                                }
                            }
                            p.Apps = apps.ToArray();
                        }
                    }

                    if (p.Apps.Length <= 0) continue;

                    p.InstalledLocation = dir;
                    p.FullName = package.Id.FullName;


                    //ms-resource:AppxManifest_DisplayName
                    //1527c705-839a-4832-9118-54d4Bd6a0c89_10.0.17134.1_neutral_neutral_cw5n1h2txyewy
                    //

                    // if (p.FullName == "1527c705-839a-4832-9118-54d4Bd6a0c89_10.0.17134.1_neutral_neutral_cw5n1h2txyewy")
                    //     ;

                    if (dsbName != "") p.Name = ExtractMSResourceString(p.InstalledLocation, package, dsbName);
                    if (dsbPublisher != "") p.Publisher = ExtractMSResourceString(p.InstalledLocation, package, dsbPublisher);
                    if (logoPath != "") p.IconPath = ExtractDisplayIcon(p.InstalledLocation, package, logoPath);
                    if (dsbMainApp != "") dsbMainApp = ExtractMSResourceString(p.InstalledLocation, package, dsbMainApp);
                    else dsbMainApp = p.Name;
                    p.MainAppDisplayName = dsbMainApp;

                    if (p.Name != "") packageDatas.Add(p);
                    packageCount += 1;
                }
                if (packageCount < 1)
                    packageCount = 0;
                enumlated = true;
            }
            return enumlated;
        }
    }
}
