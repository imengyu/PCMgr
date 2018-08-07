using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace PCMgrUWP
{

    public class UWPPackage
    {
        public string Name = "";
        public string FullName = "";
        public string Publisher = "";
        public string Description = "";
        public string InstalledLocation = "";
        public string[] Apps;
        public string IconPath = null;

        public override string ToString()
        {
            return FullName;
        }

    }
    public class UWPManager
    {
#if _X64_
        public const string COREDLLNAME = "PCMgr64.dll";
#else
        public const string COREDLLNAME = "PCMgr32.dll";
#endif

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_FileExist([MarshalAs(UnmanagedType.LPWStr)]string path);

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);
        private static string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
        {
            string sWin8ManifestString = string.Format("@{{{0}? {1}}}", pathToPRI, resourceKey);
            var outBuff = new StringBuilder(1024);
            int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
            return outBuff.ToString();
        }

        private PackageManager packageManager = new PackageManager();
        private bool enumlated = false;
        private int packageCount = 0;
        private List<UWPPackage> packageDatas = new List<UWPPackage>();

        public int PackageCount { get { return packageCount; } }
        public List<UWPPackage> Packages { get { return packageDatas; } }

        public static string DisplayNameTozhCN(string dsb)
        {
            switch(dsb)
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
            }
            return dsb;
        }

        private static string ExtractDisplayIcon(string dir, string logoPath)
        {
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
            return Path.ChangeExtension(imageFile, "scale-16.png");
        }
        private static string ExtractDisplayName(string dir, Package package, string displayName)
        {
            bool k1 = false;
            var priPath = Path.Combine(dir, "\\pris\\resources.zh-CN.pri");
            if (!MFM_FileExist(priPath)) { priPath = Path.Combine(dir, "resources.pri"); k1 = true; }

            if (!Uri.TryCreate(displayName, UriKind.Absolute, out Uri uri))
                Uri.TryCreate("ms-resource:ApplicationDisplayName", UriKind.Absolute, out uri);

            string resource = "";
            string name = "";

            resource = displayName;
            name = ExtractStringFromPRIFile(priPath, resource);
            if (!string.IsNullOrWhiteSpace(name)) return name;

            resource = string.Format("ms-resource://{0}/resources/{1}", package.Id.Name, uri.Segments.Last());
            name = ExtractStringFromPRIFile(priPath, resource);
            if (!string.IsNullOrWhiteSpace(name)) return name;


            if (!k1)
            {
                priPath = Path.Combine(dir, "resources.pri");
                name = ExtractStringFromPRIFile(priPath, resource);
                if (!string.IsNullOrWhiteSpace(name)) return name;
            }

            var res = string.Concat(uri.Segments.Skip(1));
            resource = string.Format("ms-resource://{0}/{1}", package.Id.Name, res);
            name = ExtractStringFromPRIFile(priPath, resource);
            if (!string.IsNullOrWhiteSpace(name)) return name;

            name = ExtractStringFromPRIFile(priPath, "ms-resource:ApplicationDisplayName");
            if (!string.IsNullOrWhiteSpace(name)) return name;

            return displayName;
        }

        public void StartApp(string packageName)
        {
            
        }
        public void UnInstallAppAsync(string packageName)
        {
            packageManager.RemovePackageAsync(packageName);
        }

        public bool EnumlateAll()
        {
            if (!enumlated)
            {
                var userSecurityId = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;
                var packages = packageManager.FindPackagesForUser(userSecurityId);
                foreach (var package in packages)
                {
                    UWPPackage p = new UWPPackage();
                    string logoPath = "";
                    string dsbText = "";
                    string dsbName = "";
                    string dsbPublisher = "";
                    XmlDocument xml = new XmlDocument();

                    string dir = package.InstalledLocation.Path;
                    string file = Path.Combine(dir, "AppxManifest.xml");

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
                            else if (propItem.Name == "Description")
                                dsbText = propItem.InnerText;
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
                            }
                        }
                        p.Apps = apps.ToArray();
                    }

                    p.InstalledLocation = dir;
                    p.FullName = package.Id.FullName;

                    //ms-resource:AppxManifest_DisplayName

                    if (dsbText != "") p.Description = dsbText.StartsWith("ms-resource:") ? ExtractDisplayName(p.InstalledLocation, package, dsbText) : dsbText;
                    if (dsbName != "") p.Name = dsbName.StartsWith("ms-resource:") ? ExtractDisplayName(p.InstalledLocation, package, dsbName) : dsbName;
                    if (dsbPublisher != "") p.Publisher = dsbPublisher.StartsWith("ms-resource:") ? ExtractDisplayName(p.InstalledLocation, package, dsbPublisher) : dsbPublisher;
                    if (logoPath != "") p.IconPath = ExtractDisplayIcon(p.InstalledLocation, logoPath);

                    xml.Clone();

                    if (p.Name != "")
                        packageDatas.Add(p);
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
