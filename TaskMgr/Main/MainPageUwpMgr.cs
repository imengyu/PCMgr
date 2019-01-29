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
            FormMain.复制发布者ToolStripMenuItem.Click += 复制发布者ToolStripMenuItem_Click;

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
        private void 复制发布者ToolStripMenuItem_Click(object sender, EventArgs e)
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
                    Point p = listUwpApps.GetiItemPoint(listUwpApps.SelectedItem);
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
