using PCMgr.Lanuages;
using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.NativeMethods;
using PCMgr.Helpers;

namespace PCMgr.Main
{
    class MainPageKernelDrvMgr : MainPage
    {
        private ListView listDrivers;

        private Label lbDriversCount;

        public MainPageKernelDrvMgr(FormMain formMain) : base(formMain, (TabPage)formMain.tabPageKernelCtl)
        {
            listDrivers = formMain.listDrivers;
            lbDriversCount = formMain.lbDriversCount;
        }

        protected override void OnLoad()
        {
            mainSettings = FormMain.MainSettings;
            base.OnLoad();
        }
        protected override void OnLoadControlEvents()
        {
            FormMain.linkRestartAsAdminDriver.LinkClicked += linkRestartAsAdminDriver_LinkClicked;
            FormMain.linkLabelShowKernelTools.LinkClicked += linkLabelShowKernelTools_LinkClicked;

            this.listDrivers.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listDrivers_ColumnClick);
            this.listDrivers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listDrivers_KeyDown);
            this.listDrivers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listDrivers_MouseUp);

            base.OnLoadControlEvents();
        }

        private MainSettings mainSettings = null;

        //内核驱动枚举代码

        private class ListViewItemComparerKr : IComparer
        {
            public int SortColum { get; set; }
            public bool Asdening { get; set; } = false;

            public int Compare(object o1, object o2)
            {
                ListViewItem x = o1 as ListViewItem, y = o2 as ListViewItem;
                int returnVal = -1;
                if (x.SubItems[SortColum].Text == y.SubItems[SortColum].Text) return -1;
                if (SortColum == 6)
                {
                    int xi, yi;
                    if (int.TryParse(x.SubItems[SortColum].Text, out xi) && int.TryParse(y.SubItems[SortColum].Text, out yi))
                    {
                        if (x.SubItems[SortColum].Text == y.SubItems[SortColum].Text) returnVal = 0;
                        else if (xi > yi) returnVal = 1;
                        else if (xi < yi) returnVal = -1;
                    }
                }
                else returnVal = String.Compare(((ListViewItem)x).SubItems[SortColum].Text, ((ListViewItem)y).SubItems[SortColum].Text);
                if (Asdening) returnVal = -returnVal;
                return returnVal;
            }
        }

        private IntPtr hListHeaderDrv = IntPtr.Zero;

        private ListViewItemComparerKr listViewItemComparerKr = new ListViewItemComparerKr();

        private bool showAllDriver = false;

        public void KernelListInit()
        {
            if (!Inited)
            {
#if !_X64_
                if (MIs64BitOS())
                {
                    FormMain.lbRestartAsAdminDriver.Text = LanuageMgr.GetStr("X64EnumDriver", false);
                    listDrivers.Hide();
                    FormMain.pl_driverNotLoadTip.Show();
                    FormMain.linkRestartAsAdminDriver.Visible = false;
                    Inited = true;
                    return;
                }
#endif
                if (FormMain.IsKernelLoaded)
                {
                    NativeBridge.enumKernelModulsCallBack = KernelEnumCallBack;
                    NativeBridge.enumKernelModulsCallBackPtr = Marshal.GetFunctionPointerForDelegate(NativeBridge.enumKernelModulsCallBack);

                    listViewItemComparerKr.SortColum = 6;
                    listDrivers.ListViewItemSorter = listViewItemComparerKr;
                    MAppWorkCall3(182, listDrivers.Handle, IntPtr.Zero);
                    hListHeaderDrv = ComCtlApi.MListViewGetHeaderControl(listDrivers.Handle, false);

                    Inited = true;

                    KernelLisRefesh();
                }
                else
                {
                    Inited = true;

                    listDrivers.Hide();
                    FormMain.pl_driverNotLoadTip.Show();
                    FormMain.linkRestartAsAdminDriver.Visible = !FormMain.IsAdmin;
                }         
            }
        }
        private void KernelEnumCallBack(IntPtr kmi, IntPtr BaseDllName, IntPtr FullDllPath, IntPtr FullDllPathOrginal, IntPtr szEntryPoint, IntPtr SizeOfImage, IntPtr szDriverObject, IntPtr szBase, IntPtr szServiceName, uint Order)
        {
            if (Order == 9999)
            {
                if (showAllDriver) lbDriversCount.Text = LanuageFBuffers.Str_DriverCountLoaded + kmi.ToInt32() + "  " + LanuageFBuffers.Str_DriverCount + BaseDllName.ToInt32();
                else
#if _X64_
                    lbDriversCount.Text = LanuageFBuffers.Str_DriverCount + kmi.ToInt64();
#else
                    lbDriversCount.Text = LanuageFBuffers.Str_DriverCount + kmi.ToInt32();
#endif

                return;
            }

            string baseDllName = Marshal.PtrToStringUni(BaseDllName);
            string fullDllPath = Marshal.PtrToStringUni(FullDllPath);

            ListViewItem li = new ListViewItem(baseDllName);
            li.Tag = kmi;
            //7 emepty items
            for (int i = 0; i < 8; i++) li.SubItems.Add(new ListViewItem.ListViewSubItem() { Font = listDrivers.Font });

            if (Order != 10000)
            {
                li.SubItems[0].Text = baseDllName;
                li.SubItems[1].Text = Marshal.PtrToStringUni(szBase);
                li.SubItems[2].Text = Marshal.PtrToStringUni(SizeOfImage);
                li.SubItems[3].Text = Marshal.PtrToStringUni(szDriverObject);
                li.SubItems[4].Text = fullDllPath;
                li.SubItems[5].Text = Marshal.PtrToStringUni(szServiceName);
                li.SubItems[6].Text = Order.ToString();
            }
            else
            {
                li.SubItems[0].Text = baseDllName;
                li.SubItems[1].Text = "-";
                li.SubItems[2].Text = "-";
                li.SubItems[3].Text = "-";
                li.SubItems[4].Text = fullDllPath;
                li.SubItems[5].Text = Marshal.PtrToStringUni(szServiceName);
                li.SubItems[6].Text = "-";
            }

            bool hightlight = false;
            if (MFM_FileExist(fullDllPath))
            {
                StringBuilder exeCompany = new StringBuilder(256);
                if (MGetExeCompany(fullDllPath, exeCompany, 256))
                {
                    li.SubItems[7].Text = exeCompany.ToString();
                    if (mainSettings.HighlightNoSystem && exeCompany.ToString() != ConstVals.MICROSOFT)
                        hightlight = true;
                }
                else if (mainSettings.HighlightNoSystem) hightlight = true;
                if (hightlight)
                {
                    li.ForeColor = Color.Blue;
                    foreach (ListViewItem.ListViewSubItem s in li.SubItems)
                        s.ForeColor = Color.Blue;
                }
            }
            else
            {
                li.SubItems[7].Text = LanuageFBuffers.Str_FileNotExist;
                if (mainSettings.HighlightNoSystem) hightlight = true;
            }
            if (hightlight)
            {
                li.ForeColor = Color.Blue;
                foreach (ListViewItem.ListViewSubItem s in li.SubItems)
                    s.ForeColor = Color.Blue;
            }

            listDrivers.Items.Add(li);
        }
        public void KernelLisRefesh()
        {
            if (FormMain.IsKernelLoaded)
            {
                foreach (ListViewItem li in listDrivers.Items)
                {
                    IntPtr kmi = (IntPtr)li.Tag;
                    if (kmi != IntPtr.Zero)
                        M_SU_EnumKernelModulsItemDestroy(kmi);
                }
                listDrivers.Items.Clear();
                M_SU_EnumKernelModuls(NativeBridge.enumKernelModulsCallBackPtr, showAllDriver);
            }
        }
        public void KernelListUnInit()
        {
            foreach (ListViewItem li in listDrivers.Items)
            {
                IntPtr kmi = (IntPtr)li.Tag;
                if (kmi != IntPtr.Zero)
                    M_SU_EnumKernelModulsItemDestroy(kmi);
            }
            listDrivers.Items.Clear();
        }
        public void KernelListToggleShowAllDrv()
        {
            showAllDriver = !showAllDriver;
            KernelLisRefesh();
        }

        private void linkRestartAsAdminDriver_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MAppWorkCall3(155) == 1)
            {
                SetConfig("LoadKernelDriver", "Configure", "TRUE");
                MAppRebotAdmin2("select kernel");
            }
        }
        private void linkLabelShowKernelTools_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormMain.ShowKernelTools();
        }

        private void listDrivers_MouseUp(object sender, MouseEventArgs e)
        {
            if (listDrivers.SelectedItems.Count > 0)
            {
                if (e.Button == MouseButtons.Right) M_SU_EnumKernelModuls_ShowMenu((IntPtr)listDrivers.SelectedItems[0].Tag, showAllDriver, 0, 0);
            }
        }
        private void listDrivers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListViewItemComparerKr)listDrivers.ListViewItemSorter).Asdening = !((ListViewItemComparerKr)listDrivers.ListViewItemSorter).Asdening;
            ((ListViewItemComparerKr)listDrivers.ListViewItemSorter).SortColum = e.Column;
            ComCtlApi.MListViewSetColumnSortArrow(hListHeaderDrv, ((ListViewItemComparerKr)listDrivers.ListViewItemSorter).SortColum,
             ((ListViewItemComparerKr)listDrivers.ListViewItemSorter).Asdening, false);
            listDrivers.Sort();
        }
        private void listDrivers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listDrivers.SelectedItems.Count > 0)
                {
                    ListViewItem item = listDrivers.SelectedItems[0];
                    Point p = item.Position; p.X = 0;
                    p = listDrivers.PointToScreen(p);
                    M_SU_EnumKernelModuls_ShowMenu((IntPtr)item.Tag, showAllDriver, p.X, p.Y);
                }
            }
        }


    }
}
