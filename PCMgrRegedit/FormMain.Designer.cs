namespace PCMgrRegedit
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.contextMenuStripTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新建ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.项ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.字符串值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dWORD32位值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.qWORD64位值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.多字符串值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.可扩充字符串值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.重命名ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.权限ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.复制项名称ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderData = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.编辑ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查找ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查找下一个ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.二进制值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.修改ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.修改二进制数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.重命名ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.新建ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.contextMenuStripTree.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStripView.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(0, 48);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.listView);
            this.splitContainer.Size = new System.Drawing.Size(919, 489);
            this.splitContainer.SplitterDistance = 232;
            this.splitContainer.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.ContextMenuStrip = this.contextMenuStripTree;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 3;
            this.treeView.ImageList = this.imageList;
            this.treeView.LabelEdit = true;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(232, 489);
            this.treeView.TabIndex = 0;
            this.treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
            this.treeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterExpand);
            this.treeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.treeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyUp);
            // 
            // contextMenuStripTree
            // 
            this.contextMenuStripTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建ToolStripMenuItem,
            this.toolStripSeparator2,
            this.删除ToolStripMenuItem,
            this.重命名ToolStripMenuItem,
            this.权限ToolStripMenuItem,
            this.toolStripSeparator1,
            this.复制项名称ToolStripMenuItem});
            this.contextMenuStripTree.Name = "contextMenuStripTree";
            this.contextMenuStripTree.Size = new System.Drawing.Size(153, 126);
            // 
            // 新建ToolStripMenuItem
            // 
            this.新建ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.项ToolStripMenuItem,
            this.二进制值ToolStripMenuItem,
            this.字符串值ToolStripMenuItem,
            this.dWORD32位值ToolStripMenuItem,
            this.qWORD64位值ToolStripMenuItem,
            this.多字符串值ToolStripMenuItem,
            this.可扩充字符串值ToolStripMenuItem});
            this.新建ToolStripMenuItem.Name = "新建ToolStripMenuItem";
            this.新建ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.新建ToolStripMenuItem.Text = "新建(&N)";
            // 
            // 项ToolStripMenuItem
            // 
            this.项ToolStripMenuItem.Name = "项ToolStripMenuItem";
            this.项ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.项ToolStripMenuItem.Text = "项(&K)";
            this.项ToolStripMenuItem.Click += new System.EventHandler(this.项ToolStripMenuItem_Click);
            // 
            // 字符串值ToolStripMenuItem
            // 
            this.字符串值ToolStripMenuItem.Name = "字符串值ToolStripMenuItem";
            this.字符串值ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.字符串值ToolStripMenuItem.Text = "字符串值(&S)";
            this.字符串值ToolStripMenuItem.Click += new System.EventHandler(this.字符串值ToolStripMenuItem_Click);
            // 
            // dWORD32位值ToolStripMenuItem
            // 
            this.dWORD32位值ToolStripMenuItem.Name = "dWORD32位值ToolStripMenuItem";
            this.dWORD32位值ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.dWORD32位值ToolStripMenuItem.Text = "DWORD (32位)值(&D)";
            this.dWORD32位值ToolStripMenuItem.Click += new System.EventHandler(this.dWORD32位值ToolStripMenuItem_Click);
            // 
            // qWORD64位值ToolStripMenuItem
            // 
            this.qWORD64位值ToolStripMenuItem.Name = "qWORD64位值ToolStripMenuItem";
            this.qWORD64位值ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.qWORD64位值ToolStripMenuItem.Text = "QWORD (64位)值(&Q)";
            this.qWORD64位值ToolStripMenuItem.Click += new System.EventHandler(this.qWORD64位值ToolStripMenuItem_Click);
            // 
            // 多字符串值ToolStripMenuItem
            // 
            this.多字符串值ToolStripMenuItem.Name = "多字符串值ToolStripMenuItem";
            this.多字符串值ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.多字符串值ToolStripMenuItem.Text = "多字符串值(&M)";
            this.多字符串值ToolStripMenuItem.Click += new System.EventHandler(this.多字符串值ToolStripMenuItem_Click);
            // 
            // 可扩充字符串值ToolStripMenuItem
            // 
            this.可扩充字符串值ToolStripMenuItem.Name = "可扩充字符串值ToolStripMenuItem";
            this.可扩充字符串值ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.可扩充字符串值ToolStripMenuItem.Text = "可扩充字符串值(&E)";
            this.可扩充字符串值ToolStripMenuItem.Click += new System.EventHandler(this.可扩充字符串值ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.删除ToolStripMenuItem.Text = "删除(&D)";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // 重命名ToolStripMenuItem
            // 
            this.重命名ToolStripMenuItem.Name = "重命名ToolStripMenuItem";
            this.重命名ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.重命名ToolStripMenuItem.Text = "重命名(&R)";
            this.重命名ToolStripMenuItem.Click += new System.EventHandler(this.重命名ToolStripMenuItem_Click);
            // 
            // 权限ToolStripMenuItem
            // 
            this.权限ToolStripMenuItem.Name = "权限ToolStripMenuItem";
            this.权限ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.权限ToolStripMenuItem.Text = "权限(&P)";
            this.权限ToolStripMenuItem.Click += new System.EventHandler(this.权限ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // 复制项名称ToolStripMenuItem
            // 
            this.复制项名称ToolStripMenuItem.Name = "复制项名称ToolStripMenuItem";
            this.复制项名称ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.复制项名称ToolStripMenuItem.Text = "复制项名称(&C)";
            this.复制项名称ToolStripMenuItem.Click += new System.EventHandler(this.复制项名称ToolStripMenuItem_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ItemText");
            this.imageList.Images.SetKeyName(1, "ItemBinary");
            this.imageList.Images.SetKeyName(2, "ItemError");
            this.imageList.Images.SetKeyName(3, "ItemLoading");
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderType,
            this.columnHeaderData});
            this.listView.ContextMenuStrip = this.contextMenuStripView;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView.HideSelection = false;
            this.listView.LabelEdit = true;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(683, 489);
            this.listView.SmallImageList = this.imageList;
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView_AfterLabelEdit);
            this.listView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listView_KeyUp);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "名称";
            this.columnHeaderName.Width = 188;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "类型";
            this.columnHeaderType.Width = 141;
            // 
            // columnHeaderData
            // 
            this.columnHeaderData.Text = "数据";
            this.columnHeaderData.Width = 330;
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAddress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxAddress.Location = new System.Drawing.Point(9, 28);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(898, 14);
            this.textBoxAddress.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.查看ToolStripMenuItem,
            this.编辑ToolStripMenuItem,
            this.关于ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(919, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.退出ToolStripMenuItem});
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(58, 21);
            this.文件ToolStripMenuItem.Text = "文件(&F)";
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.退出ToolStripMenuItem.Text = "退出(&E)";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.退出ToolStripMenuItem_Click);
            // 
            // 查看ToolStripMenuItem
            // 
            this.查看ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新ToolStripMenuItem});
            this.查看ToolStripMenuItem.Name = "查看ToolStripMenuItem";
            this.查看ToolStripMenuItem.Size = new System.Drawing.Size(60, 21);
            this.查看ToolStripMenuItem.Text = "查看(&V)";
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.刷新ToolStripMenuItem.Text = "刷新(&R)";
            // 
            // 编辑ToolStripMenuItem
            // 
            this.编辑ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.查找ToolStripMenuItem,
            this.查找下一个ToolStripMenuItem});
            this.编辑ToolStripMenuItem.Name = "编辑ToolStripMenuItem";
            this.编辑ToolStripMenuItem.Size = new System.Drawing.Size(59, 21);
            this.编辑ToolStripMenuItem.Text = "编辑(&E)";
            // 
            // 查找ToolStripMenuItem
            // 
            this.查找ToolStripMenuItem.Name = "查找ToolStripMenuItem";
            this.查找ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.查找ToolStripMenuItem.Text = "查找(&F)";
            // 
            // 查找下一个ToolStripMenuItem
            // 
            this.查找下一个ToolStripMenuItem.Name = "查找下一个ToolStripMenuItem";
            this.查找下一个ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.查找下一个ToolStripMenuItem.Text = "查找下一个(&X)";
            // 
            // 关于ToolStripMenuItem
            // 
            this.关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            this.关于ToolStripMenuItem.Size = new System.Drawing.Size(60, 21);
            this.关于ToolStripMenuItem.Text = "关于(&A)";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.statusProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(919, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(32, 17);
            this.statusLabel.Text = "就绪";
            // 
            // statusProgressBar
            // 
            this.statusProgressBar.Name = "statusProgressBar";
            this.statusProgressBar.Size = new System.Drawing.Size(100, 16);
            this.statusProgressBar.Visible = false;
            // 
            // 二进制值ToolStripMenuItem
            // 
            this.二进制值ToolStripMenuItem.Name = "二进制值ToolStripMenuItem";
            this.二进制值ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.二进制值ToolStripMenuItem.Text = "二进制值(&B)";
            this.二进制值ToolStripMenuItem.Click += new System.EventHandler(this.二进制值ToolStripMenuItem_Click);
            // 
            // contextMenuStripView
            // 
            this.contextMenuStripView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.修改ToolStripMenuItem,
            this.修改二进制数据ToolStripMenuItem,
            this.toolStripSeparator3,
            this.新建ToolStripMenuItem1,
            this.toolStripSeparator4,
            this.删除ToolStripMenuItem1,
            this.重命名ToolStripMenuItem1});
            this.contextMenuStripView.Name = "contextMenuStripView";
            this.contextMenuStripView.Size = new System.Drawing.Size(177, 126);
            // 
            // 修改ToolStripMenuItem
            // 
            this.修改ToolStripMenuItem.Name = "修改ToolStripMenuItem";
            this.修改ToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.修改ToolStripMenuItem.Text = "修改(&M)";
            this.修改ToolStripMenuItem.Click += new System.EventHandler(this.修改ToolStripMenuItem_Click);
            // 
            // 修改二进制数据ToolStripMenuItem
            // 
            this.修改二进制数据ToolStripMenuItem.Name = "修改二进制数据ToolStripMenuItem";
            this.修改二进制数据ToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.修改二进制数据ToolStripMenuItem.Text = "修改二进制数据(&B)";
            this.修改二进制数据ToolStripMenuItem.Click += new System.EventHandler(this.修改二进制数据ToolStripMenuItem_Click);
            // 
            // 删除ToolStripMenuItem1
            // 
            this.删除ToolStripMenuItem1.Name = "删除ToolStripMenuItem1";
            this.删除ToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.删除ToolStripMenuItem1.Text = "删除(&D)";
            this.删除ToolStripMenuItem1.Click += new System.EventHandler(this.删除ToolStripMenuItem1_Click);
            // 
            // 重命名ToolStripMenuItem1
            // 
            this.重命名ToolStripMenuItem1.Name = "重命名ToolStripMenuItem1";
            this.重命名ToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.重命名ToolStripMenuItem1.Text = "重命名(&R)";
            this.重命名ToolStripMenuItem1.Click += new System.EventHandler(this.重命名ToolStripMenuItem1_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(173, 6);
            // 
            // 新建ToolStripMenuItem1
            // 
            this.新建ToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem6,
            this.toolStripMenuItem7,
            this.toolStripMenuItem5,
            this.toolStripMenuItem4,
            this.toolStripMenuItem3,
            this.toolStripMenuItem1});
            this.新建ToolStripMenuItem1.Name = "新建ToolStripMenuItem1";
            this.新建ToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.新建ToolStripMenuItem1.Text = "新建(&N)";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(173, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem1.Text = "可扩充字符串值(&E)";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.可扩充字符串值ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem3.Text = "多字符串值(&M)";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.多字符串值ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem4.Text = "QWORD (64位)值(&Q)";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.qWORD64位值ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem5.Text = "DWORD (32位)值(&D)";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.dWORD32位值ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem6.Text = "字符串值(&S)";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.字符串值ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem7.Text = "二进制值(&B)";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.二进制值ToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(919, 562);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.textBoxAddress);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PCMgr 注册表编辑器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.contextMenuStripTree.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStripView.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 编辑ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查找ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查找下一个ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关于ToolStripMenuItem;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderData;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar statusProgressBar;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTree;
        private System.Windows.Forms.ToolStripMenuItem 新建ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 项ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 字符串值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dWORD32位值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem qWORD64位值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 多字符串值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 可扩充字符串值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 重命名ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 权限ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 复制项名称ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 二进制值ToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripView;
        private System.Windows.Forms.ToolStripMenuItem 修改ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 修改二进制数据ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem 新建ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 重命名ToolStripMenuItem1;
    }
}

