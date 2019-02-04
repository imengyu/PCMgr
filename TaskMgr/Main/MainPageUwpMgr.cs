using PCMgr.Aero.TaskDialog;
using PCMgr.Ctls;
using PCMgr.Helpers;
using PCMgr.Lanuages;
using PCMgr.WorkWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using static PCMgr.Main.MainUtils;
using static PCMgr.NativeMethods;

namespace PCMgr.Main
{
    class MainPageUwpMgr : MainPage
    {
        private TaskMgrList listUwpApps;
        private Panel pl_UWPEnumFailTip;
        private Label lbUWPEnumFailText;
        private ContextMenuStrip contextMenuStripUWP;

        public MainPageUwpMgr(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageUWPCtl)
        {
            listUwpApps = formMain.listUwpApps;
            pl_UWPEnumFailTip = formMain.pl_UWPEnumFailTip;
            lbUWPEnumFailText = formMain.lbUWPEnumFailText;
            contextMenuStripUWP = formMain.contextMenuStripUWP;
        }

        protected override void OnLoadControlEvents()
        {
            listUwpApps.Header.CloumClick += listUwpApps_Header_CloumClick;
            listUwpApps.KeyDown += listUwpApps_KeyDown;
            listUwpApps.MouseClick += listUwpApps_MouseClick;

            FormMain.打开应用ToolStripMenuItem.Click += 打开应用ToolStripMenuItem_Click;
            FormMain.卸载应用ToolStripMenuItem.Click += 卸载应用ToolStripMenuItem_Click;
            FormMain.打开安装位置ToolStripMenuItem.Click += 打开安装位置ToolStripMenuItem_Click;
            FormMain.复制名称ToolStripMenuItem.Click += 复制名称ToolStripMenuItem_Click;
            FormMain.复制完整名称ToolStripMenuItem.Click += 复制完整名称ToolStripMenuItem_Click;
            FormMain.复制发布者ToolStripMenuItem.Click += 复制ToolStripMenuItem_Click;

            base.OnLoadControlEvents();
        }

        //枚举通用应用

        public class TaskListViewUWPColumnSorter : ListViewColumnSorter
        {
            public TaskListViewUWPColumnSorter()
            {
            }
            public override int Compare(TaskMgrListItem x, TaskMgrListItem y)
            {
                int compareResult = 0;
                compareResult = string.Compare(x.SubItems[SortColumn].Text, y.SubItems[SortColumn].Text);
                if (compareResult == 0)
                    compareResult = ObjectCompare.Compare(x.PID, y.PID);
                if (Order == SortOrder.Ascending)
                    return compareResult;
                else if (Order == SortOrder.Descending)
                    return (-compareResult);
                return compareResult;
            }
        }

        private TaskListViewUWPColumnSorter uWPColumnSorter = new TaskListViewUWPColumnSorter();
        public void UWPListRefesh()
        {
            if (Inited)
            {
                listUwpApps.Show();
                pl_UWPEnumFailTip.Hide();
                listUwpApps.Items.Clear();

                if (!M_UWP_EnumUWPApplications())
                {
                    listUwpApps.Hide();
                    pl_UWPEnumFailTip.Show();
                    lbUWPEnumFailText.Text = LanuageMgr.GetStr("UWPEnumFail", false);
                    return;
                }


                int count = M_UWP_GetUWPApplicationsCount();
                for (int i = 0; i < count; i++)
                {
                    UWP_PACKAGE_INFO info = M_UWP_GetUWPApplicationAt((uint)i);

                    TaskMgrListItem li = new TaskMgrListItem(info.DisplayName);
                    li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                    li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                    li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                    li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                    li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                    li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
                    li.SubItems[0].Font = listUwpApps.Font;
                    li.SubItems[1].Font = listUwpApps.Font;
                    li.SubItems[2].Font = listUwpApps.Font;
                    li.SubItems[0].Text = info.DisplayName;
                    li.SubItems[1].Text = info.AppPackageFullName;
                    li.SubItems[2].Text = info.InstallPath;
                    li.SubItems[3].Text = info.AppUserModelId;
                    li.Tag = info;
                    li.IsUWPICO = true;
                    if (info.IconBackgroundColor != 0 && info.IconBackgroundColor != 65535 && info.IconBackgroundColor != 30720)
                        li.UWPIcoColor = Uint32StrToColor((uint)info.IconBackgroundColor);

                    string iconpath = UWPSearchIcon(info.InstallPath, info.IconPath);
                    if (iconpath != "" && MFM_FileExist(iconpath))
                    {
                        using (Image img = Image.FromFile(iconpath))
                            li.Icon = IconUtils.ConvertToIcon(img);
                        // li.Image = IconUtils.GetThumbnail(new Bitmap(iconpath), 16, 16);
                    }
                    listUwpApps.Items.Add(li);
                }
            }
        }
        public void UWPListInit()
        {
            if (!Inited)
            {
                listUwpApps.Header.Height = 36;
                listUwpApps.ReposVscroll();
                listUwpApps.ListViewItemSorter = uWPColumnSorter;

                Inited = true;

                UWPListLoadCols();
                UWPListRefesh();
            }
        }
        public void UWPListUnInit()
        {
            listUwpApps.Items.Clear();
        }
        private string UWPSearchIcon(string dir, string logoPath)
        {
            //Force search
            var imageFile = Path.Combine(dir, logoPath);
            var name = Path.GetFileName(imageFile);

            if (MFM_FileExist(imageFile)) return imageFile;
            var
            scaleImage = Path.ChangeExtension(imageFile, "scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "targetsize-16.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "targetsize-24.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "targetsize-44.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "contrast-black.targetsize-44.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "scale-200_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "scale-100_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            imageFile = dir + "\\" + logoPath.Replace(name, "") + "contrast-black\\" + name;
            scaleImage = Path.ChangeExtension(imageFile, "scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "contrast-black_scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "scale-200_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "scale-100_contrast-black.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-200.png"); if (MFM_FileExist(scaleImage)) return scaleImage;
            scaleImage = Path.ChangeExtension(imageFile, "Theme-Dark_Scale-100.png"); if (MFM_FileExist(scaleImage)) return scaleImage;

            imageFile = Path.Combine(dir, "en-us", logoPath);
            if (MFM_FileExist(imageFile)) return imageFile;
            return "";
        }
        private void UWPListLoadCols()
        {
            TaskMgrListHeaderItem li8 = new TaskMgrListHeaderItem();
            li8.TextSmall = LanuageMgr.GetStr("TitleName", false);
            li8.Width = 300;
            listUwpApps.Colunms.Add(li8);
            TaskMgrListHeaderItem li10 = new TaskMgrListHeaderItem();
            li10.TextSmall = LanuageMgr.GetStr("TitleFullName", false);
            li10.Width = 260;
            listUwpApps.Colunms.Add(li10);
            TaskMgrListHeaderItem li11 = new TaskMgrListHeaderItem();
            li11.TextSmall = LanuageMgr.GetStr("TitleInstallDir", false);
            li11.Width = 260;
            listUwpApps.Colunms.Add(li11);
        }

        /// <summary>
        /// 导出 ms-resource: 的字符串资源
        /// </summary>
        /// <param name="dir">UWP 安装目录</param>
        /// <param name="package">包</param>
        /// <param name="resource">resource key</param>
        /// <returns></returns>
        private static string ExtractMSResourceString(string dir, string packageIdName, string resource)
        {
            if (resource.StartsWith("ms-resource:"))
            {
                var priPath = dir + "\\resources.pri";
                if (resource.Contains("/"))
                {
                    //检查reskey是否合法
                    string resourceRevStart = resource.Replace("ms-resource:", "");//去掉msresource
                    string[] resourceSps = resourceRevStart.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (resourceSps.Length > 0 && resourceSps[0] == packageIdName)
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
                        string resourceKeyReal = "ms-resource://" + packageIdName;
                        foreach (string s in resourceSps)
                            if (s.Contains("ms-resource:"))
                                resourceKeyReal += "/" + s.Replace("ms-resource:", "");
                            else resourceKeyReal += "/" + s;
                        string name = ExtractStringFromPRIFile(priPath, resourceKeyReal);
                        if (!string.IsNullOrWhiteSpace(name))
                            return name;//成功返回
                        else
                        {
                            resourceKeyReal = "ms-resource://" + packageIdName + "/resources"; 
                            foreach (string s in resourceSps)
                                if (s.Contains("ms-resource:"))
                                    resourceKeyReal += "/" + s.Replace("ms-resource:", "");
                                else resourceKeyReal += "/" + s;
                            name = ExtractStringFromPRIFile(priPath, resourceKeyReal);
                            if (!string.IsNullOrWhiteSpace(name))
                                return name;//成功返回
                        }
                    }
                }
                else
                {
                    string name = "";

                    string reskeyold = resource.StartsWith("ms-resource:") ? resource.Replace("ms-resource:", "") : resource;
                    //if (reskeyold == "AppxManifest_DisplayName")
                    //    resource = string.Format("ms-resource://{0}/resources/DisplayName", package.Id.Name);
                    //else
                    resource = string.Format("ms-resource://{0}/resources/{1}", packageIdName, reskeyold);
                    name = ExtractStringFromPRIFile(priPath, resource);
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            return resource;
        }

        public TaskMgrListItem UWPListFindItem(string fullName)
        {
            TaskMgrListItem rs = null;
            foreach (TaskMgrListItem r in listUwpApps.Items)
                if (((UWP_PACKAGE_INFO)r.Tag).AppPackageFullName == fullName)
                {
                    rs = r;
                    break;
                }
            return rs;
        }
        public TaskMgrListItem UWPListTryForceLoadUWPInfo(string fullName, string packageIdName, string exeFullPath)
        {
            string appmpath, appfimalyid, dir = Path.GetDirectoryName(exeFullPath);
            if (UWPIsSure(exeFullPath, out appmpath, out appfimalyid))
            {
                string dsbName, logoPath, bgColor;
                if (UWPReadAppMainfanst(dir, packageIdName, appmpath, out dsbName, out logoPath, out bgColor)) 
                    return UWPForceReadAddItem(fullName, appfimalyid, dir, dsbName, logoPath, bgColor);
            }
            return null;
        }

        private bool UWPIsSure(string exeFullPath, out string appmpath, out string appfimalyid)
        {
            string dir = Path.GetDirectoryName(exeFullPath).ToLower();
            if (dir.Contains(@"c:\windows\systemapps")) {
                appfimalyid = dir.Replace(@"c:\windows\systemapps\", "");
                appmpath = dir + "\\AppxManifest.xml";
                return MFM_FileExist(appmpath);
            }
            if (dir.Contains(@"c:\program files\windowsapps")) {
                appfimalyid = dir.Replace(@"c:\program files\windowsapps\", "");
                appmpath = dir + "\\AppxManifest.xml";
                return MFM_FileExist(appmpath);
            }
            appmpath = null;
            appfimalyid = null;
            return false;
        }
        private bool UWPReadAppMainfanst(string dir, string packageIdName, string appmpath, out string dsbName1, out string logoPath1, out string bgColor1)
        {
            string dsbName = "", logoPath = "", bgColor = "";
            //Load xml to read AppxManifest
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(appmpath);
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
                    for (int i = 0; i < nApps.ChildNodes.Count && i < 1; i++)
                    {
                        XmlNode apptem = nApps.ChildNodes[i];
                        if (apptem.Name == "Application")
                        {
                            for (int i1 = 0; i1 < apptem.ChildNodes.Count; i1++)
                            {
                                XmlNode apppropItem = apptem.ChildNodes[i1];
                                if (apppropItem.Name == "uap:VisualElements" || apppropItem.Name == "VisualElements")
                                {
                                    if (apppropItem.Attributes["DisplayName"] != null)
                                        dsbName = apppropItem.Attributes["DisplayName"].InnerText;
                                    if (apppropItem.Attributes["Square44x44Logo"] != null)
                                        logoPath = apppropItem.Attributes["Square44x44Logo"].InnerText;
                                    else if (apppropItem.Attributes["Square150x150Logo"] != null)
                                        logoPath = apppropItem.Attributes["Square150x150Logo"].InnerText;
                                    else if (apppropItem.Attributes["Logo"] != null)
                                        logoPath = apppropItem.Attributes["Logo"].InnerText;
                                    if (apppropItem.Attributes["BackgroundColor"] != null)
                                        bgColor = apppropItem.Attributes["BackgroundColor"].InnerText;
                                }
                            }
                        }
                    }
                    bgColor1 = bgColor;
                    dsbName1 = ExtractMSResourceString(dir, packageIdName, dsbName);
                    logoPath1 = ExtractMSResourceString(dir, packageIdName, logoPath); 
                    return true;
                }
            }
            catch { }
            bgColor1 = "";
            dsbName1 = "";
            logoPath1 = "";
            return false;
        }
        private TaskMgrListItem UWPForceReadAddItem(string fullName, string appfimalyid, string installDir, string dsbName, string logoPath, string bgColor)
        {
            UWP_PACKAGE_INFO info = new UWP_PACKAGE_INFO();
            info.AppPackageFullName = fullName;
            info.AppPackageFamilyName = appfimalyid;
            info.DisplayName = dsbName;
            info.IconPath = logoPath;
            info.InstallPath = installDir;

            TaskMgrListItem li = new TaskMgrListItem(info.DisplayName);
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems.Add(new TaskMgrListItem.TaskMgrListViewSubItem());
            li.SubItems[0].Font = listUwpApps.Font;
            li.SubItems[1].Font = listUwpApps.Font;
            li.SubItems[2].Font = listUwpApps.Font;
            li.SubItems[0].Text = info.DisplayName;
            li.SubItems[1].Text = info.AppPackageFullName;
            li.SubItems[2].Text = info.InstallPath;
            li.SubItems[3].Text = info.AppUserModelId;
            li.Tag = info;
            if (bgColor != "" && bgColor != "transparent") li.UWPIcoColor = MainUtils.HexStrToColor(bgColor);
            li.IsUWPICO = true;

            string iconpath = UWPSearchIcon(info.InstallPath, info.IconPath);
            if (iconpath != "" && MFM_FileExist(iconpath))
            {
                using (Image img = Image.FromFile(iconpath))
                    li.Icon = IconUtils.ConvertToIcon(img);
            }
            listUwpApps.Items.Add(li);
            return li;
        }

        private void 打开应用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWP_PACKAGE_INFO pkg = ((UWP_PACKAGE_INFO)listUwpApps.SelectedItem.Tag);
                if (pkg.AppUserModelId != "")
                {
                    uint processid = 0;
                    M_UWP_RunUWPApp(pkg.AppUserModelId, ref processid);
                }
            }
        }
        private void 卸载应用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MRunExe("ms-settings:appsfeatures", null);
        }
        private void 打开安装位置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWP_PACKAGE_INFO pkg = ((UWP_PACKAGE_INFO)listUwpApps.SelectedItem.Tag);
                MFM_OpenFile(pkg.InstallPath, Handle);
            }
        }
        private void 复制名称ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWP_PACKAGE_INFO pkg = ((UWP_PACKAGE_INFO)listUwpApps.SelectedItem.Tag);
                MCopyToClipboard2(pkg.DisplayName);
            }
        }
        private void 复制完整名称ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                UWP_PACKAGE_INFO pkg = ((UWP_PACKAGE_INFO)listUwpApps.SelectedItem.Tag);
                MCopyToClipboard2(pkg.AppPackageFullName);
            }
        }
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUwpApps.SelectedItem != null)
            {
                string s = "";
                for (int i = 0; i < listUwpApps.Colunms.Count; i++)
                {
                    s += " " + listUwpApps.Colunms[i].TextSmall + " : ";
                    s += listUwpApps.SelectedItem.SubItems[i].Text;
                }
                MCopyToClipboard2(s);
            }
        }

        private void listUwpApps_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listUwpApps.SelectedItem != null)
                {
                    Point p = listUwpApps.GetItemPoint(listUwpApps.SelectedItem);
                    contextMenuStripUWP.Show(listUwpApps.PointToScreen(p));
                }
            }
        }
        private void listUwpApps_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listUwpApps.SelectedItem != null)
                contextMenuStripUWP.Show(MousePosition);
        }
        private void listUwpApps_Header_CloumClick(object sender, TaskMgrListHeader.TaskMgrListHeaderEventArgs e)
        {
            if (e.MouseEventArgs.Button == MouseButtons.Left)
            {
                listUwpApps.Locked = true;
                if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.None)
                    uWPColumnSorter.Order = SortOrder.Ascending;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                    uWPColumnSorter.Order = SortOrder.Ascending;
                else if (e.Item.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    uWPColumnSorter.Order = SortOrder.Descending;

                uWPColumnSorter.SortColumn = e.Index;
                listUwpApps.Locked = false;
                listUwpApps.Sort();
            }
        }
    }
}
