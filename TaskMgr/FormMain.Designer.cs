namespace PCMgr
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.splitContainerFm = new System.Windows.Forms.SplitContainer();
            this.treeFmLeft = new System.Windows.Forms.TreeView();
            this.imageListFileMgrLeft = new System.Windows.Forms.ImageList(this.components);
            this.listFm = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListFileTypeList = new System.Windows.Forms.ImageList(this.components);
            this.splitContainerPerfCtls = new System.Windows.Forms.SplitContainer();
            this.sp3 = new System.Windows.Forms.Panel();
            this.performanceLeftList = new PCMgr.Ctls.PerformanceList();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageProcCtl = new System.Windows.Forms.TabPage();
            this.check_showAllProcess = new System.Windows.Forms.CheckBox();
            this.spl1 = new System.Windows.Forms.PictureBox();
            this.lbShowDetals = new System.Windows.Forms.LinkLabel();
            this.btnEndProcess = new System.Windows.Forms.Button();
            this.listProcess = new PCMgr.Ctls.TaskMgrList();
            this.lbProcessCount = new System.Windows.Forms.Label();
            this.tabPageKernelCtl = new System.Windows.Forms.TabPage();
            this.lbDriversCount = new System.Windows.Forms.Label();
            this.listDrivers = new PCMgr.Ctls.TaskMgrList();
            this.tabPageSysCtl = new System.Windows.Forms.TabPage();
            this.tabPagePerfCtl = new System.Windows.Forms.TabPage();
            this.tabPageUWPCtl = new System.Windows.Forms.TabPage();
            this.pl_UWPEnumFailTip = new System.Windows.Forms.Panel();
            this.lbUWPEnumFailText = new System.Windows.Forms.Label();
            this.listUwpApps = new PCMgr.Ctls.TaskMgrList();
            this.tabPageScCtl = new System.Windows.Forms.TabPage();
            this.pl_ScNeedAdminTip = new System.Windows.Forms.Panel();
            this.linkRebootAsAdmin = new System.Windows.Forms.LinkLabel();
            this.lbScNeedAdminTip = new System.Windows.Forms.Label();
            this.sp2 = new System.Windows.Forms.PictureBox();
            this.linkOpenScMsc = new System.Windows.Forms.LinkLabel();
            this.listService = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lbServicesCount = new System.Windows.Forms.Label();
            this.tabPageStartCtl = new System.Windows.Forms.TabPage();
            this.listStartup = new System.Windows.Forms.ListView();
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageFileCtl = new System.Windows.Forms.TabPage();
            this.lbFileMgrStatus = new System.Windows.Forms.Label();
            this.btnFmAddGoto = new System.Windows.Forms.Button();
            this.textBoxFmCurrent = new System.Windows.Forms.TextBox();
            this.spBottom = new System.Windows.Forms.PictureBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.fileSystemWatcher = new System.IO.FileSystemWatcher();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerFm)).BeginInit();
            this.splitContainerFm.Panel1.SuspendLayout();
            this.splitContainerFm.Panel2.SuspendLayout();
            this.splitContainerFm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPerfCtls)).BeginInit();
            this.splitContainerPerfCtls.Panel1.SuspendLayout();
            this.splitContainerPerfCtls.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageProcCtl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spl1)).BeginInit();
            this.tabPageKernelCtl.SuspendLayout();
            this.tabPagePerfCtl.SuspendLayout();
            this.tabPageUWPCtl.SuspendLayout();
            this.pl_UWPEnumFailTip.SuspendLayout();
            this.tabPageScCtl.SuspendLayout();
            this.pl_ScNeedAdminTip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp2)).BeginInit();
            this.tabPageStartCtl.SuspendLayout();
            this.tabPageFileCtl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerFm
            // 
            resources.ApplyResources(this.splitContainerFm, "splitContainerFm");
            this.splitContainerFm.Name = "splitContainerFm";
            // 
            // splitContainerFm.Panel1
            // 
            resources.ApplyResources(this.splitContainerFm.Panel1, "splitContainerFm.Panel1");
            this.splitContainerFm.Panel1.Controls.Add(this.treeFmLeft);
            this.toolTip.SetToolTip(this.splitContainerFm.Panel1, resources.GetString("splitContainerFm.Panel1.ToolTip"));
            // 
            // splitContainerFm.Panel2
            // 
            resources.ApplyResources(this.splitContainerFm.Panel2, "splitContainerFm.Panel2");
            this.splitContainerFm.Panel2.Controls.Add(this.listFm);
            this.toolTip.SetToolTip(this.splitContainerFm.Panel2, resources.GetString("splitContainerFm.Panel2.ToolTip"));
            this.toolTip.SetToolTip(this.splitContainerFm, resources.GetString("splitContainerFm.ToolTip"));
            // 
            // treeFmLeft
            // 
            resources.ApplyResources(this.treeFmLeft, "treeFmLeft");
            this.treeFmLeft.FullRowSelect = true;
            this.treeFmLeft.ImageList = this.imageListFileMgrLeft;
            this.treeFmLeft.Name = "treeFmLeft";
            this.toolTip.SetToolTip(this.treeFmLeft, resources.GetString("treeFmLeft.ToolTip"));
            this.treeFmLeft.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeFmLeft_BeforeExpand);
            this.treeFmLeft.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeFmLeft_AfterSelect);
            this.treeFmLeft.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFmLeft_NodeMouseClick);
            this.treeFmLeft.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeFmLeft_MouseClick);
            // 
            // imageListFileMgrLeft
            // 
            this.imageListFileMgrLeft.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListFileMgrLeft.ImageStream")));
            this.imageListFileMgrLeft.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListFileMgrLeft.Images.SetKeyName(0, "sec");
            this.imageListFileMgrLeft.Images.SetKeyName(1, "loading");
            this.imageListFileMgrLeft.Images.SetKeyName(2, "err");
            // 
            // listFm
            // 
            resources.ApplyResources(this.listFm, "listFm");
            this.listFm.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.listFm.FullRowSelect = true;
            this.listFm.Name = "listFm";
            this.listFm.ShowItemToolTips = true;
            this.listFm.SmallImageList = this.imageListFileTypeList;
            this.toolTip.SetToolTip(this.listFm, resources.GetString("listFm.ToolTip"));
            this.listFm.UseCompatibleStateImageBehavior = false;
            this.listFm.View = System.Windows.Forms.View.Details;
            this.listFm.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listFm_AfterLabelEdit);
            this.listFm.SelectedIndexChanged += new System.EventHandler(this.listFm_SelectedIndexChanged);
            this.listFm.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listFm_MouseClick);
            this.listFm.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listFm_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // columnHeader6
            // 
            resources.ApplyResources(this.columnHeader6, "columnHeader6");
            // 
            // imageListFileTypeList
            // 
            this.imageListFileTypeList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListFileTypeList.ImageStream")));
            this.imageListFileTypeList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListFileTypeList.Images.SetKeyName(0, "err");
            // 
            // splitContainerPerfCtls
            // 
            resources.ApplyResources(this.splitContainerPerfCtls, "splitContainerPerfCtls");
            this.splitContainerPerfCtls.Name = "splitContainerPerfCtls";
            // 
            // splitContainerPerfCtls.Panel1
            // 
            resources.ApplyResources(this.splitContainerPerfCtls.Panel1, "splitContainerPerfCtls.Panel1");
            this.splitContainerPerfCtls.Panel1.Controls.Add(this.sp3);
            this.splitContainerPerfCtls.Panel1.Controls.Add(this.performanceLeftList);
            this.toolTip.SetToolTip(this.splitContainerPerfCtls.Panel1, resources.GetString("splitContainerPerfCtls.Panel1.ToolTip"));
            // 
            // splitContainerPerfCtls.Panel2
            // 
            resources.ApplyResources(this.splitContainerPerfCtls.Panel2, "splitContainerPerfCtls.Panel2");
            this.toolTip.SetToolTip(this.splitContainerPerfCtls.Panel2, resources.GetString("splitContainerPerfCtls.Panel2.ToolTip"));
            this.toolTip.SetToolTip(this.splitContainerPerfCtls, resources.GetString("splitContainerPerfCtls.ToolTip"));
            // 
            // sp3
            // 
            resources.ApplyResources(this.sp3, "sp3");
            this.sp3.BackColor = System.Drawing.Color.Silver;
            this.sp3.Name = "sp3";
            this.toolTip.SetToolTip(this.sp3, resources.GetString("sp3.ToolTip"));
            // 
            // performanceLeftList
            // 
            resources.ApplyResources(this.performanceLeftList, "performanceLeftList");
            this.performanceLeftList.BackColor = System.Drawing.Color.White;
            this.performanceLeftList.Name = "performanceLeftList";
            this.performanceLeftList.Selectedtem = null;
            this.toolTip.SetToolTip(this.performanceLeftList, resources.GetString("performanceLeftList.ToolTip"));
            this.performanceLeftList.SelectedtndexChanged += new System.EventHandler(this.performanceLeftList_SelectedtndexChanged);
            // 
            // tabControlMain
            // 
            resources.ApplyResources(this.tabControlMain, "tabControlMain");
            this.tabControlMain.Controls.Add(this.tabPageProcCtl);
            this.tabControlMain.Controls.Add(this.tabPageKernelCtl);
            this.tabControlMain.Controls.Add(this.tabPageSysCtl);
            this.tabControlMain.Controls.Add(this.tabPagePerfCtl);
            this.tabControlMain.Controls.Add(this.tabPageUWPCtl);
            this.tabControlMain.Controls.Add(this.tabPageScCtl);
            this.tabControlMain.Controls.Add(this.tabPageStartCtl);
            this.tabControlMain.Controls.Add(this.tabPageFileCtl);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.toolTip.SetToolTip(this.tabControlMain, resources.GetString("tabControlMain.ToolTip"));
            this.tabControlMain.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControlMain_Selected);
            // 
            // tabPageProcCtl
            // 
            resources.ApplyResources(this.tabPageProcCtl, "tabPageProcCtl");
            this.tabPageProcCtl.Controls.Add(this.check_showAllProcess);
            this.tabPageProcCtl.Controls.Add(this.spl1);
            this.tabPageProcCtl.Controls.Add(this.lbShowDetals);
            this.tabPageProcCtl.Controls.Add(this.btnEndProcess);
            this.tabPageProcCtl.Controls.Add(this.listProcess);
            this.tabPageProcCtl.Controls.Add(this.lbProcessCount);
            this.tabPageProcCtl.Name = "tabPageProcCtl";
            this.toolTip.SetToolTip(this.tabPageProcCtl, resources.GetString("tabPageProcCtl.ToolTip"));
            this.tabPageProcCtl.UseVisualStyleBackColor = true;
            // 
            // check_showAllProcess
            // 
            resources.ApplyResources(this.check_showAllProcess, "check_showAllProcess");
            this.check_showAllProcess.Name = "check_showAllProcess";
            this.toolTip.SetToolTip(this.check_showAllProcess, resources.GetString("check_showAllProcess.ToolTip"));
            this.check_showAllProcess.UseVisualStyleBackColor = true;
            this.check_showAllProcess.CheckedChanged += new System.EventHandler(this.check_showAllProcess_CheckedChanged);
            // 
            // spl1
            // 
            resources.ApplyResources(this.spl1, "spl1");
            this.spl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(207)))), ((int)(((byte)(207)))));
            this.spl1.Name = "spl1";
            this.spl1.TabStop = false;
            this.toolTip.SetToolTip(this.spl1, resources.GetString("spl1.ToolTip"));
            // 
            // lbShowDetals
            // 
            resources.ApplyResources(this.lbShowDetals, "lbShowDetals");
            this.lbShowDetals.DisabledLinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.lbShowDetals.Image = global::PCMgr.Properties.Resources.application_view_list;
            this.lbShowDetals.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lbShowDetals.LinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.lbShowDetals.Name = "lbShowDetals";
            this.lbShowDetals.TabStop = true;
            this.toolTip.SetToolTip(this.lbShowDetals, resources.GetString("lbShowDetals.ToolTip"));
            this.lbShowDetals.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbShowDetals_LinkClicked);
            // 
            // btnEndProcess
            // 
            resources.ApplyResources(this.btnEndProcess, "btnEndProcess");
            this.btnEndProcess.Name = "btnEndProcess";
            this.toolTip.SetToolTip(this.btnEndProcess, resources.GetString("btnEndProcess.ToolTip"));
            this.btnEndProcess.UseVisualStyleBackColor = true;
            this.btnEndProcess.Click += new System.EventHandler(this.btnEndProcess_Click);
            // 
            // listProcess
            // 
            resources.ApplyResources(this.listProcess, "listProcess");
            this.listProcess.BackColor = System.Drawing.SystemColors.Window;
            this.listProcess.FocusedType = false;
            this.listProcess.Icons = null;
            this.listProcess.ListViewItemSorter = null;
            this.listProcess.Name = "listProcess";
            this.listProcess.ShowGroup = false;
            this.toolTip.SetToolTip(this.listProcess, resources.GetString("listProcess.ToolTip"));
            this.listProcess.Value = 0D;
            this.listProcess.XOffest = 0;
            this.listProcess.SelectItemChanged += new System.EventHandler(this.listProcess_SelectItemChanged);
            this.listProcess.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listProcess_KeyDown);
            this.listProcess.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listProcess_MouseUp);
            // 
            // lbProcessCount
            // 
            resources.ApplyResources(this.lbProcessCount, "lbProcessCount");
            this.lbProcessCount.Name = "lbProcessCount";
            this.toolTip.SetToolTip(this.lbProcessCount, resources.GetString("lbProcessCount.ToolTip"));
            // 
            // tabPageKernelCtl
            // 
            resources.ApplyResources(this.tabPageKernelCtl, "tabPageKernelCtl");
            this.tabPageKernelCtl.Controls.Add(this.lbDriversCount);
            this.tabPageKernelCtl.Controls.Add(this.listDrivers);
            this.tabPageKernelCtl.Name = "tabPageKernelCtl";
            this.toolTip.SetToolTip(this.tabPageKernelCtl, resources.GetString("tabPageKernelCtl.ToolTip"));
            this.tabPageKernelCtl.UseVisualStyleBackColor = true;
            // 
            // lbDriversCount
            // 
            resources.ApplyResources(this.lbDriversCount, "lbDriversCount");
            this.lbDriversCount.Name = "lbDriversCount";
            this.toolTip.SetToolTip(this.lbDriversCount, resources.GetString("lbDriversCount.ToolTip"));
            // 
            // listDrivers
            // 
            resources.ApplyResources(this.listDrivers, "listDrivers");
            this.listDrivers.BackColor = System.Drawing.SystemColors.Window;
            this.listDrivers.FocusedType = false;
            this.listDrivers.ListViewItemSorter = null;
            this.listDrivers.Name = "listDrivers";
            this.listDrivers.ShowGroup = false;
            this.toolTip.SetToolTip(this.listDrivers, resources.GetString("listDrivers.ToolTip"));
            this.listDrivers.Value = 0D;
            this.listDrivers.XOffest = 0;
            // 
            // tabPageSysCtl
            // 
            resources.ApplyResources(this.tabPageSysCtl, "tabPageSysCtl");
            this.tabPageSysCtl.Name = "tabPageSysCtl";
            this.toolTip.SetToolTip(this.tabPageSysCtl, resources.GetString("tabPageSysCtl.ToolTip"));
            this.tabPageSysCtl.UseVisualStyleBackColor = true;
            // 
            // tabPagePerfCtl
            // 
            resources.ApplyResources(this.tabPagePerfCtl, "tabPagePerfCtl");
            this.tabPagePerfCtl.Controls.Add(this.splitContainerPerfCtls);
            this.tabPagePerfCtl.Name = "tabPagePerfCtl";
            this.toolTip.SetToolTip(this.tabPagePerfCtl, resources.GetString("tabPagePerfCtl.ToolTip"));
            this.tabPagePerfCtl.UseVisualStyleBackColor = true;
            // 
            // tabPageUWPCtl
            // 
            resources.ApplyResources(this.tabPageUWPCtl, "tabPageUWPCtl");
            this.tabPageUWPCtl.Controls.Add(this.pl_UWPEnumFailTip);
            this.tabPageUWPCtl.Controls.Add(this.listUwpApps);
            this.tabPageUWPCtl.Name = "tabPageUWPCtl";
            this.toolTip.SetToolTip(this.tabPageUWPCtl, resources.GetString("tabPageUWPCtl.ToolTip"));
            this.tabPageUWPCtl.UseVisualStyleBackColor = true;
            // 
            // pl_UWPEnumFailTip
            // 
            resources.ApplyResources(this.pl_UWPEnumFailTip, "pl_UWPEnumFailTip");
            this.pl_UWPEnumFailTip.Controls.Add(this.lbUWPEnumFailText);
            this.pl_UWPEnumFailTip.Name = "pl_UWPEnumFailTip";
            this.toolTip.SetToolTip(this.pl_UWPEnumFailTip, resources.GetString("pl_UWPEnumFailTip.ToolTip"));
            // 
            // lbUWPEnumFailText
            // 
            resources.ApplyResources(this.lbUWPEnumFailText, "lbUWPEnumFailText");
            this.lbUWPEnumFailText.Name = "lbUWPEnumFailText";
            this.toolTip.SetToolTip(this.lbUWPEnumFailText, resources.GetString("lbUWPEnumFailText.ToolTip"));
            // 
            // listUwpApps
            // 
            resources.ApplyResources(this.listUwpApps, "listUwpApps");
            this.listUwpApps.BackColor = System.Drawing.SystemColors.Window;
            this.listUwpApps.FocusedType = false;
            this.listUwpApps.ListViewItemSorter = null;
            this.listUwpApps.Name = "listUwpApps";
            this.listUwpApps.ShowGroup = false;
            this.toolTip.SetToolTip(this.listUwpApps, resources.GetString("listUwpApps.ToolTip"));
            this.listUwpApps.Value = 0D;
            this.listUwpApps.XOffest = 0;
            // 
            // tabPageScCtl
            // 
            resources.ApplyResources(this.tabPageScCtl, "tabPageScCtl");
            this.tabPageScCtl.Controls.Add(this.pl_ScNeedAdminTip);
            this.tabPageScCtl.Controls.Add(this.sp2);
            this.tabPageScCtl.Controls.Add(this.linkOpenScMsc);
            this.tabPageScCtl.Controls.Add(this.listService);
            this.tabPageScCtl.Controls.Add(this.lbServicesCount);
            this.tabPageScCtl.Name = "tabPageScCtl";
            this.toolTip.SetToolTip(this.tabPageScCtl, resources.GetString("tabPageScCtl.ToolTip"));
            this.tabPageScCtl.UseVisualStyleBackColor = true;
            // 
            // pl_ScNeedAdminTip
            // 
            resources.ApplyResources(this.pl_ScNeedAdminTip, "pl_ScNeedAdminTip");
            this.pl_ScNeedAdminTip.Controls.Add(this.linkRebootAsAdmin);
            this.pl_ScNeedAdminTip.Controls.Add(this.lbScNeedAdminTip);
            this.pl_ScNeedAdminTip.Name = "pl_ScNeedAdminTip";
            this.toolTip.SetToolTip(this.pl_ScNeedAdminTip, resources.GetString("pl_ScNeedAdminTip.ToolTip"));
            // 
            // linkRebootAsAdmin
            // 
            resources.ApplyResources(this.linkRebootAsAdmin, "linkRebootAsAdmin");
            this.linkRebootAsAdmin.DisabledLinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.linkRebootAsAdmin.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkRebootAsAdmin.LinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.linkRebootAsAdmin.Name = "linkRebootAsAdmin";
            this.linkRebootAsAdmin.TabStop = true;
            this.toolTip.SetToolTip(this.linkRebootAsAdmin, resources.GetString("linkRebootAsAdmin.ToolTip"));
            this.linkRebootAsAdmin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRebootAsAdmin_LinkClicked);
            // 
            // lbScNeedAdminTip
            // 
            resources.ApplyResources(this.lbScNeedAdminTip, "lbScNeedAdminTip");
            this.lbScNeedAdminTip.Name = "lbScNeedAdminTip";
            this.toolTip.SetToolTip(this.lbScNeedAdminTip, resources.GetString("lbScNeedAdminTip.ToolTip"));
            // 
            // sp2
            // 
            resources.ApplyResources(this.sp2, "sp2");
            this.sp2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(207)))), ((int)(((byte)(207)))));
            this.sp2.Name = "sp2";
            this.sp2.TabStop = false;
            this.toolTip.SetToolTip(this.sp2, resources.GetString("sp2.ToolTip"));
            // 
            // linkOpenScMsc
            // 
            resources.ApplyResources(this.linkOpenScMsc, "linkOpenScMsc");
            this.linkOpenScMsc.DisabledLinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.linkOpenScMsc.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkOpenScMsc.LinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.linkOpenScMsc.Name = "linkOpenScMsc";
            this.linkOpenScMsc.TabStop = true;
            this.toolTip.SetToolTip(this.linkOpenScMsc, resources.GetString("linkOpenScMsc.ToolTip"));
            this.linkOpenScMsc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkOpenScMsc_LinkClicked);
            // 
            // listService
            // 
            resources.ApplyResources(this.listService, "listService");
            this.listService.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listService.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14});
            this.listService.FullRowSelect = true;
            this.listService.MultiSelect = false;
            this.listService.Name = "listService";
            this.toolTip.SetToolTip(this.listService, resources.GetString("listService.ToolTip"));
            this.listService.UseCompatibleStateImageBehavior = false;
            this.listService.View = System.Windows.Forms.View.Details;
            this.listService.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listService_MouseClick);
            // 
            // columnHeader7
            // 
            resources.ApplyResources(this.columnHeader7, "columnHeader7");
            // 
            // columnHeader8
            // 
            resources.ApplyResources(this.columnHeader8, "columnHeader8");
            // 
            // columnHeader9
            // 
            resources.ApplyResources(this.columnHeader9, "columnHeader9");
            // 
            // columnHeader10
            // 
            resources.ApplyResources(this.columnHeader10, "columnHeader10");
            // 
            // columnHeader11
            // 
            resources.ApplyResources(this.columnHeader11, "columnHeader11");
            // 
            // columnHeader12
            // 
            resources.ApplyResources(this.columnHeader12, "columnHeader12");
            // 
            // columnHeader13
            // 
            resources.ApplyResources(this.columnHeader13, "columnHeader13");
            // 
            // columnHeader14
            // 
            resources.ApplyResources(this.columnHeader14, "columnHeader14");
            // 
            // lbServicesCount
            // 
            resources.ApplyResources(this.lbServicesCount, "lbServicesCount");
            this.lbServicesCount.Name = "lbServicesCount";
            this.toolTip.SetToolTip(this.lbServicesCount, resources.GetString("lbServicesCount.ToolTip"));
            // 
            // tabPageStartCtl
            // 
            resources.ApplyResources(this.tabPageStartCtl, "tabPageStartCtl");
            this.tabPageStartCtl.Controls.Add(this.listStartup);
            this.tabPageStartCtl.Name = "tabPageStartCtl";
            this.toolTip.SetToolTip(this.tabPageStartCtl, resources.GetString("tabPageStartCtl.ToolTip"));
            this.tabPageStartCtl.UseVisualStyleBackColor = true;
            // 
            // listStartup
            // 
            resources.ApplyResources(this.listStartup, "listStartup");
            this.listStartup.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listStartup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17,
            this.columnHeader19,
            this.columnHeader18,
            this.columnHeader20});
            this.listStartup.FullRowSelect = true;
            this.listStartup.MultiSelect = false;
            this.listStartup.Name = "listStartup";
            this.listStartup.ShowItemToolTips = true;
            this.listStartup.SmallImageList = this.imageListFileTypeList;
            this.toolTip.SetToolTip(this.listStartup, resources.GetString("listStartup.ToolTip"));
            this.listStartup.UseCompatibleStateImageBehavior = false;
            this.listStartup.View = System.Windows.Forms.View.Details;
            this.listStartup.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listStartup_MouseClick);
            // 
            // columnHeader15
            // 
            resources.ApplyResources(this.columnHeader15, "columnHeader15");
            // 
            // columnHeader16
            // 
            resources.ApplyResources(this.columnHeader16, "columnHeader16");
            // 
            // columnHeader17
            // 
            resources.ApplyResources(this.columnHeader17, "columnHeader17");
            // 
            // columnHeader19
            // 
            resources.ApplyResources(this.columnHeader19, "columnHeader19");
            // 
            // columnHeader18
            // 
            resources.ApplyResources(this.columnHeader18, "columnHeader18");
            // 
            // columnHeader20
            // 
            resources.ApplyResources(this.columnHeader20, "columnHeader20");
            // 
            // tabPageFileCtl
            // 
            resources.ApplyResources(this.tabPageFileCtl, "tabPageFileCtl");
            this.tabPageFileCtl.Controls.Add(this.lbFileMgrStatus);
            this.tabPageFileCtl.Controls.Add(this.btnFmAddGoto);
            this.tabPageFileCtl.Controls.Add(this.textBoxFmCurrent);
            this.tabPageFileCtl.Controls.Add(this.splitContainerFm);
            this.tabPageFileCtl.Name = "tabPageFileCtl";
            this.toolTip.SetToolTip(this.tabPageFileCtl, resources.GetString("tabPageFileCtl.ToolTip"));
            this.tabPageFileCtl.UseVisualStyleBackColor = true;
            // 
            // lbFileMgrStatus
            // 
            resources.ApplyResources(this.lbFileMgrStatus, "lbFileMgrStatus");
            this.lbFileMgrStatus.Name = "lbFileMgrStatus";
            this.toolTip.SetToolTip(this.lbFileMgrStatus, resources.GetString("lbFileMgrStatus.ToolTip"));
            // 
            // btnFmAddGoto
            // 
            resources.ApplyResources(this.btnFmAddGoto, "btnFmAddGoto");
            this.btnFmAddGoto.Name = "btnFmAddGoto";
            this.toolTip.SetToolTip(this.btnFmAddGoto, resources.GetString("btnFmAddGoto.ToolTip"));
            this.btnFmAddGoto.UseVisualStyleBackColor = true;
            this.btnFmAddGoto.Click += new System.EventHandler(this.btnFmAddGoto_Click);
            // 
            // textBoxFmCurrent
            // 
            resources.ApplyResources(this.textBoxFmCurrent, "textBoxFmCurrent");
            this.textBoxFmCurrent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxFmCurrent.Name = "textBoxFmCurrent";
            this.toolTip.SetToolTip(this.textBoxFmCurrent, resources.GetString("textBoxFmCurrent.ToolTip"));
            this.textBoxFmCurrent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxFmCurrent_KeyDown);
            // 
            // spBottom
            // 
            resources.ApplyResources(this.spBottom, "spBottom");
            this.spBottom.BackColor = System.Drawing.Color.DarkGray;
            this.spBottom.Name = "spBottom";
            this.spBottom.TabStop = false;
            this.toolTip.SetToolTip(this.spBottom, resources.GetString("spBottom.ToolTip"));
            // 
            // fileSystemWatcher
            // 
            this.fileSystemWatcher.EnableRaisingEvents = true;
            this.fileSystemWatcher.SynchronizingObject = this;
            this.fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Changed);
            this.fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Created);
            this.fileSystemWatcher.Deleted += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Deleted);
            this.fileSystemWatcher.Renamed += new System.IO.RenamedEventHandler(this.fileSystemWatcher_Renamed);
            // 
            // notifyIcon
            // 
            resources.ApplyResources(this.notifyIcon, "notifyIcon");
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.spBottom);
            this.Controls.Add(this.tabControlMain);
            this.Cursor = System.Windows.Forms.Cursors.AppStarting;
            this.Name = "FormMain";
            this.toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.Deactivate += new System.EventHandler(this.FormMain_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.splitContainerFm.Panel1.ResumeLayout(false);
            this.splitContainerFm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerFm)).EndInit();
            this.splitContainerFm.ResumeLayout(false);
            this.splitContainerPerfCtls.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPerfCtls)).EndInit();
            this.splitContainerPerfCtls.ResumeLayout(false);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageProcCtl.ResumeLayout(false);
            this.tabPageProcCtl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spl1)).EndInit();
            this.tabPageKernelCtl.ResumeLayout(false);
            this.tabPageKernelCtl.PerformLayout();
            this.tabPagePerfCtl.ResumeLayout(false);
            this.tabPageUWPCtl.ResumeLayout(false);
            this.pl_UWPEnumFailTip.ResumeLayout(false);
            this.tabPageScCtl.ResumeLayout(false);
            this.tabPageScCtl.PerformLayout();
            this.pl_ScNeedAdminTip.ResumeLayout(false);
            this.pl_ScNeedAdminTip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp2)).EndInit();
            this.tabPageStartCtl.ResumeLayout(false);
            this.tabPageFileCtl.ResumeLayout(false);
            this.tabPageFileCtl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageProcCtl;
        private System.Windows.Forms.TabPage tabPageKernelCtl;
        private System.Windows.Forms.TabPage tabPageSysCtl;
        private System.Windows.Forms.PictureBox spBottom;
        private System.Windows.Forms.Button btnEndProcess;
        private Ctls.TaskMgrList listProcess;
        private System.Windows.Forms.Label lbProcessCount;
        private Ctls.TaskMgrList listDrivers;
        private System.Windows.Forms.PictureBox spl1;
        private System.Windows.Forms.LinkLabel lbShowDetals;
        private System.Windows.Forms.Label lbDriversCount;
        private System.Windows.Forms.TabPage tabPageScCtl;
        private System.Windows.Forms.TabPage tabPageStartCtl;
        private System.Windows.Forms.TabPage tabPageFileCtl;
        private System.Windows.Forms.SplitContainer splitContainerFm;
        private System.Windows.Forms.TreeView treeFmLeft;
        private System.Windows.Forms.ListView listFm;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ImageList imageListFileMgrLeft;
        private System.Windows.Forms.TextBox textBoxFmCurrent;
        private System.Windows.Forms.ImageList imageListFileTypeList;
        private System.Windows.Forms.Button btnFmAddGoto;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label lbFileMgrStatus;
        private System.IO.FileSystemWatcher fileSystemWatcher;
        private System.Windows.Forms.ListView listService;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.PictureBox sp2;
        private System.Windows.Forms.LinkLabel linkOpenScMsc;
        private System.Windows.Forms.Label lbServicesCount;
        private System.Windows.Forms.Panel pl_ScNeedAdminTip;
        private System.Windows.Forms.LinkLabel linkRebootAsAdmin;
        private System.Windows.Forms.Label lbScNeedAdminTip;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.CheckBox check_showAllProcess;
        private System.Windows.Forms.TabPage tabPageUWPCtl;
        private Ctls.TaskMgrList listUwpApps;
        private System.Windows.Forms.Panel pl_UWPEnumFailTip;
        private System.Windows.Forms.Label lbUWPEnumFailText;
        private System.Windows.Forms.TabPage tabPagePerfCtl;
        private System.Windows.Forms.SplitContainer splitContainerPerfCtls;
        private Ctls.PerformanceList performanceLeftList;
        private System.Windows.Forms.Panel sp3;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ListView listStartup;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
    }
}

