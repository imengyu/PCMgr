using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PCMgr.NativeMethods;
using static PCMgr.NativeMethods.Win32;

namespace PCMgr.WorkWindow
{
    public partial class FormSpyWindow : Form
    {
        public FormSpyWindow(IntPtr hWnd)
        {
            InitializeComponent();
            this.hWnd = hWnd;
        }
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        public class WindowInfo
        {
            private IntPtr _Hwnd;

            public IntPtr Hwnd
            {
                get { return _Hwnd; }
                set { _Hwnd = value; }
            }

            private string _WindowText;

            public string WindowText
            {
                get { return _WindowText; }
                set { _WindowText = value; }
            }

            private string _ClassName;

            public string ClassName
            {
                get { return _ClassName; }
                set { _ClassName = value; }
            }

            private WINDOWINFO _WndInfo;
            internal WINDOWINFO WndInfo
            {
                get { return _WndInfo; }
                set { _WndInfo = value; }
            }
            private System.Diagnostics.Process _Process;
            public System.Diagnostics.Process Process
            {
                get { return _Process; }
                set { _Process = value; }
            }
            public WindowInfo(IntPtr hWnd)
            {
                this._Hwnd = hWnd;
            }
        }

        private struct TreeViewWinData
        {
            public IntPtr hwnd;
            public bool loaded;
            public WindowInfo info;
        }

        private IntPtr hWnd = IntPtr.Zero;
        private byte[] m_byTextBuffer;
        private ImageList mainImageList = new ImageList();

        private void FormSpyWindow_Load(object sender, EventArgs e)
        {
            FormSettings.LoadFontSettingForUI(this);

            #region Datas
            m_dic_style = new Dictionary<uint, string>();
            m_dic_style.Add(0x00000000, "WS_OVERLAPPED");
            m_dic_style.Add(0x80000000, "WS_POPUP");
            m_dic_style.Add(0x40000000, "WS_CHILD");
            m_dic_style.Add(0x20000000, "WS_MINIMIZE");
            m_dic_style.Add(0x10000000, "WS_VISIBLE");
            m_dic_style.Add(0x08000000, "WS_DISABLED");
            m_dic_style.Add(0x04000000, "WS_CLIPSIBLINGS");
            m_dic_style.Add(0x02000000, "WS_CLIPCHILDREN");
            m_dic_style.Add(0x01000000, "WS_MAXIMIZE");
            m_dic_style.Add(0x00800000, "WS_BORDER");
            m_dic_style.Add(0x00400000, "WS_DLGFRAME");
            m_dic_style.Add(0x00200000, "WS_VSCROLL");
            m_dic_style.Add(0x00100000, "WS_HSCROLL");
            m_dic_style.Add(0x00080000, "WS_SYSMENU");
            m_dic_style.Add(0x00040000, "WS_THICKFRAME");
            m_dic_style.Add(0x00020000, "WS_GROUP|WS_MINIMIZEBOX");
            m_dic_style.Add(0x00010000, "WS_TABSTOP|WS_MAXIMIZEBOX");
            m_dic_exstyle = new Dictionary<uint, string>();
            m_dic_exstyle.Add(0x00000001, "WS_EX_DLGMODALFRAME");
            m_dic_exstyle.Add(0x00000004, "WS_EX_NOPARENTNOTIFY");
            m_dic_exstyle.Add(0x00000008, "WS_EX_TOPMOST");
            m_dic_exstyle.Add(0x00000010, "WS_EX_ACCEPTFILES");
            m_dic_exstyle.Add(0x00000020, "WS_EX_TRANSPARENT");
            m_dic_exstyle.Add(0x00000040, "WS_EX_MDICHILD");
            m_dic_exstyle.Add(0x00000080, "WS_EX_TOOLWINDOW");
            m_dic_exstyle.Add(0x00000100, "WS_EX_WINDOWEDGE");
            m_dic_exstyle.Add(0x00000200, "WS_EX_CLIENTEDGE|WS_EX_RTLREADING");
            m_dic_exstyle.Add(0x00000400, "WS_EX_CONTEXTHELP");
            m_dic_exstyle.Add(0x00001000, "WS_EX_RIGHT");
            m_dic_exstyle.Add(0x00000000, "WS_EX_LEFT|WS_EX_LTRREADING|WS_EX_RIGHTSCROLLBAR");
            m_dic_exstyle.Add(0x00004000, "WS_EX_LEFTSCROLLBAR");
            m_dic_exstyle.Add(0x00010000, "WS_EX_CONTROLPARENT");
            m_dic_exstyle.Add(0x00020000, "WS_EX_STATICEDGE");
            m_dic_exstyle.Add(0x00040000, "WS_EX_APPWINDOW");
            m_dic_exstyle.Add(0x00080000, "WS_EX_LAYERED");
            m_dic_exstyle.Add(0x00100000, "WS_EX_NOINHERITLAYOUT");
            m_dic_exstyle.Add(0x00200000, "WS_EX_NOREDIRECTIONBITMAP");
            m_dic_exstyle.Add(0x00400000, "WS_EX_LAYOUTRTL");
            m_dic_exstyle.Add(0x02000000, "WS_EX_COMPOSITED");
            m_dic_exstyle.Add(0x08000000, "WS_EX_NOACTIVATE");
            #endregion

            mainImageList.ImageSize = new Size(16, 16);
            mainImageList.ColorDepth = ColorDepth.Depth32Bit;
            mainImageList.TransparentColor = Color.FromArgb(255, 0, 255);

            #region LoadIcon
            int i = 0;
            mainImageList.Images.Add(PCMgr.Properties.Resources.icoShowedWindow);
            mainImageList.Images.Add(PCMgr.Properties.Resources.icoHidedWindow);
            i++;//1
            mainImageList.Images.Add(PCMgr.Properties.Resources.icoWins);
            i++;
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_CheckBox);
            i++;
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_RadioButton);
            i++;
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_PictureBox);
            i++;
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_Button);
            i++;
            mainImageList.Images.SetKeyName(i, "button");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_ComboBox);
            i++;
            mainImageList.Images.SetKeyName(i, "combobox");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_TextBox);
            i++;
            mainImageList.Images.SetKeyName(i, "edit");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_Form);
            i++;
            mainImageList.Images.SetKeyName(i, "none");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_ProgressBar);
            i++;
            mainImageList.Images.SetKeyName(i, "progress");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_RichTextBox);
            i++;
            mainImageList.Images.SetKeyName(i, "richedit");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_VScrollBar);
            i++;
            mainImageList.Images.SetKeyName(i, "scrollbar");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_Label);
            i++;
            mainImageList.Images.SetKeyName(i, "static");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_StatusStrip);
            i++;
            mainImageList.Images.SetKeyName(i, "statusbar");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_DateTimePicker);
            i++;
            mainImageList.Images.SetKeyName(i, "sysdatetimepick32");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_LinkLabel);
            i++;
            mainImageList.Images.SetKeyName(i, "syslink");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_ListView);
            i++;//15
            mainImageList.Images.SetKeyName(i, "listview");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_TabControl);
            i++;
            mainImageList.Images.SetKeyName(i, "systabcontrol32");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_TreeView);
            i++;
            mainImageList.Images.SetKeyName(i, "systreeview32");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_ToolBar);
            i++;
            mainImageList.Images.SetKeyName(i, "toolbar");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_TrackBar);
            i++;
            mainImageList.Images.SetKeyName(i, "trackbar");
            mainImageList.Images.Add(PCMgr.Properties.Resources.System_Windows_Forms_Form);
            i++;
            mainImageList.Images.SetKeyName(i, "window");
            #endregion

            treeViewMain.ImageList = mainImageList;
            m_byTextBuffer = new byte[256];
            LoadChildWindows();
        }
        private bool LoadChildWindows(bool re=false)
        {
            if (hWnd != IntPtr.Zero)
            {
                if (IsWindow(hWnd))
                {
                    treeViewMain.Nodes.Clear();

                    WindowInfo wi = this.GetWindowInfo(hWnd);
                    Text = "[" + wi.WindowText + "] TaskManager Spy";

                    TreeNode nd = new TreeNode(wi.Hwnd.ToString("X").PadLeft(8, '0') + " | " + wi.WindowText + " | " + wi.ClassName);
                    IntPtr hIcon = MGetWindowIcon(hWnd);
                    if (hIcon != IntPtr.Zero)
                    {
                        mainImageList.Images.Add(Icon.FromHandle(hIcon));
                        nd.ImageIndex = mainImageList.Images.Count - 1;
                    }
                    else
                    {
                        if (IsWindowVisible(wi.Hwnd))
                            nd.ImageIndex = 1;
                        else
                            nd.ImageIndex = 0;
                    }
                    TreeViewWinData d = new TreeViewWinData();
                    d.hwnd = hWnd;
                    d.loaded = false;
                    d.info = wi;
                    nd.Tag = d;
                    TreeNode ndnull = new TreeNode(FormMain.str_loading);
                    ndnull.Name = "ndnull";
                    nd.Nodes.Add(ndnull);
                    treeViewMain.Nodes.Add(nd);

                    labelState.Hide();
                    panelMain.Show();

                    if (re) MessageBox.Show(FormMain.str_RefeshSuccess);
                }
                else labelState.Text = FormMain.str_InvalidHwnd;
            }
            else labelState.Text = FormMain.str_InvalidHwnd;
            return false;
        }
        private void LoadChilds(IntPtr hWnd, TreeNode treeNode)
        {
            LoadTreeWnd(hWnd,IntPtr.Zero, treeNode);
        }
        public void LoadTreeWnd(IntPtr hParent, IntPtr hAfter, TreeNode treeNode)
        {
            while ((hAfter = FindWindowEx(hParent, hAfter, null, null)) != IntPtr.Zero)
            {
                WindowInfo wi = this.GetWindowInfo(hAfter);
                if (checkBoxShowVisible.Checked && (wi.WndInfo.dwStyle & WS_VISIBLE) == 0) continue;
                TreeNode node = this.GetNodeFromWindowInfo(wi);
                if (wi.Process == null) wi.Process = ((TreeViewWinData)treeNode.Tag).info.Process;
                if (!IsWindowVisible(wi.Hwnd))
                    node.ForeColor = Color.Gray;
                else if ((wi.WndInfo.dwExStyle & WS_EX_TRANSPARENT) != 0)
                    node.ForeColor = Color.Red;

                TreeViewWinData d = new WorkWindow.FormSpyWindow.TreeViewWinData();
                d.loaded = false;
                d.hwnd = wi.Hwnd;
                d.info = wi;
                node.Tag = d;
                TreeNode ndnull = new TreeNode("正在加载……");
                ndnull.Name = "ndnull";
                node.Nodes.Add(ndnull);
                treeNode.Nodes.Add(node);
            }
        }
        private TreeNode GetNodeFromWindowInfo(WindowInfo w)
        {
            TreeNode node = new TreeNode(w.Hwnd.ToString("X").PadLeft(8, '0') + " | " + w.WindowText + " | " + w.ClassName);
            bool se = false;
            for(int i=0;i< mainImageList.Images.Keys.Count;i++)
            {
                if (mainImageList.Images.Keys[i] != "")
                {
                    if (w.ClassName.ToLower().Contains(mainImageList.Images.Keys[i]))
                    {
                        se = true;                      
                        if (mainImageList.Images.Keys[i] == "button")
                        {
                            if ((w.WndInfo.dwStyle & 2) == 2 || (w.WndInfo.dwStyle & 3) == 3)
                            {
                                node.ImageIndex = 3;
                                node.SelectedImageIndex = 3;
                            }
                            else if ((w.WndInfo.dwStyle & 4) == 4 || (w.WndInfo.dwStyle & 9) == 9)
                            {
                                node.ImageIndex = 4;
                                node.SelectedImageIndex = 4;
                            }
                            else
                            {
                                node.ImageIndex = i;
                                node.SelectedImageIndex = i;
                            }
                        }
                        else if (mainImageList.Images.Keys[i] == "static")
                        {
                            if ((w.WndInfo.dwStyle & 0x3) == 3 || (w.WndInfo.dwStyle & 0xE) == 0xE)
                            {
                                node.ImageIndex = 5;
                                node.SelectedImageIndex = 5;
                            }
                            else
                            {
                                node.ImageIndex = i;
                                node.SelectedImageIndex = i;
                            }
                        }
                        else
                        {
                            node.ImageIndex = i;
                            node.SelectedImageIndex = i;
                        }
                        break;
                    }
                }
            }
            if (!se)
            {
                if (!IsWindowVisible(w.Hwnd))
                {
                    node.ForeColor = Color.Gray;
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                }
                else if ((w.WndInfo.dwExStyle & WS_EX_TRANSPARENT) != 0)
                    node.ForeColor = Color.Red;
                else { node.ImageIndex = 0; node.SelectedImageIndex = 0; }
            }
            return node;
        }
        private WindowInfo GetWindowInfo(IntPtr hWnd)
        {
            WindowInfo windowInfo = new WindowInfo(hWnd);
            WINDOWINFO wi = new WINDOWINFO();
            NativeMethods.Win32.GetWindowInfo(hWnd, ref wi);
            //GetWindowInfo返回的ExStyle貌似有些问题
            wi.dwExStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);
            int len = GetClassName(hWnd, m_byTextBuffer, m_byTextBuffer.Length);
            windowInfo.ClassName = Encoding.Default.GetString(m_byTextBuffer, 0, len > m_byTextBuffer.Length ? m_byTextBuffer.Length : len);
            len = GetWindowText(hWnd, m_byTextBuffer, m_byTextBuffer.Length);
            windowInfo.WindowText = Encoding.Default.GetString(m_byTextBuffer, 0, len > m_byTextBuffer.Length ? m_byTextBuffer.Length : len);
            windowInfo.WndInfo = wi;
            return windowInfo;
        }
        private void treeViewMain_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Expand)
            {
                TreeViewWinData d = (TreeViewWinData)e.Node.Tag;
                if (!d.loaded)
                {
                    LoadChilds(d.hwnd, e.Node);
                    d.loaded = true;
                    e.Node.Tag = d;
                    TreeNode n = e.Node.Nodes["ndnull"];
                    if(n!=null)e.Node.Nodes.Remove(n);
                }
            }
        }
        private IntPtr selectHwnd = IntPtr.Zero;
        private Dictionary<uint, string> m_dic_style;
        private Dictionary<uint, string> m_dic_exstyle;
        private void treeViewMain_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewMain.SelectedNode != null)
            {
                TreeViewWinData d = (TreeViewWinData)treeViewMain.SelectedNode.Tag;
                selectHwnd = d.hwnd;
                textBoxClassName.Text = d.info.ClassName;
                textBoxClientRect.Text = d.info.WndInfo.rcClient.ToRectangle().ToString();
                textBoxRect.Text = d.info.WndInfo.rcWindow.ToRectangle().ToString();
                textBoxHandle.Text = d.hwnd.ToInt32().ToString();
                textBoxStytle.Text = "0x" + d.info.WndInfo.dwStyle.ToString("X").PadLeft(8, '0') + "\r\n--------\r\n";
                textBoxCtlId.Text = GetDlgCtrlID(d.hwnd).ToString();
                foreach (var v in m_dic_style)
                {
                    if ((d.info.WndInfo.dwStyle & v.Key) == v.Key)
                    {
                        textBoxStytle.Text += "0x" + v.Key.ToString("X").PadLeft(8, '0') + " " + v.Value + "\r\n";
                    }
                }
                textBoxExStytle.Text = "0x" + d.info.WndInfo.dwExStyle.ToString("X").PadLeft(8, '0') + "\r\n--------\r\n";
                foreach (var v in m_dic_exstyle)
                {
                    if ((d.info.WndInfo.dwExStyle & v.Key) == v.Key)
                    {
                        textBoxExStytle.Text += "0x" + v.Key.ToString("X").PadLeft(8, '0') + " " + v.Value + "\r\n";
                    }
                }
                textBoxText.Text = d.info.WindowText;
            }
        }
        private void textBoxText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (MessageBox.Show(FormMain.str_ChangeWindowTextAsk, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    textBoxText.Text.Remove(textBoxText.Text.Length - 1);
                    SetWindowText(selectHwnd, textBoxText.Text);
                    InvalidateRect(selectHwnd, IntPtr.Zero, true);
                    refeshitem();
                }
                else
                {
                    byte[] text = new byte[512];
                    GetWindowText(hWnd, text, 512);
                    textBoxText.Text = Encoding.Default.GetString(text);
                }
            }
        }
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadChildWindows(true);
        }
        private void treeViewMain_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button== MouseButtons.Right)
            {
                if(treeViewMain.SelectedNode!=null)
                {
                    if (IsWindowVisible(selectHwnd))
                    {
                        显示ToolStripMenuItem.Enabled = false;
                        隐藏ToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        显示ToolStripMenuItem.Enabled = true;
                        隐藏ToolStripMenuItem.Enabled = false;
                    }
                    contextMenuStripMain.Show(MousePosition);
                }
            }
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewMain.SelectedNode != null)
            {
                MAppWorkCall3(205, selectHwnd, IntPtr.Zero);
                refeshitem();
            }
        }
        private void 隐藏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewMain.SelectedNode != null)
            {
                MAppWorkCall3(200, selectHwnd, IntPtr.Zero);
                refeshitem();
            }
        }
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewMain.SelectedNode != null)
            {
                CloseWindow(selectHwnd);
                DestroyWindow(selectHwnd);
                refeshitem();
            }
        }

        private void buttonRefesh_Click(object sender, EventArgs e)
        {
            刷新ToolStripMenuItem_Click(null, null);
        }
        private void refeshitem()
        {
            if (treeViewMain.SelectedNode != null)
            {
                TreeViewWinData d = (TreeViewWinData)treeViewMain.SelectedNode.Tag;
                WindowInfo w = d.info;
                d.info.WindowText = textBoxText.Text;
                treeViewMain.SelectedNode.Text = w.Hwnd.ToString("X").PadLeft(8, '0') + " | " + w.WindowText + " | " + w.ClassName;
                treeViewMain.SelectedNode.Tag = d;
                if (!IsWindowVisible(w.Hwnd))
                    treeViewMain.SelectedNode.ForeColor = Color.Gray;
                else if ((w.WndInfo.dwExStyle & WS_EX_TRANSPARENT) != 0)
                    treeViewMain.SelectedNode.ForeColor = Color.Red;
                else treeViewMain.SelectedNode.ForeColor = Color.Black;
            }
        }

        private void 启用窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewMain.SelectedNode != null)
                MAppWorkCall3(211, selectHwnd, IntPtr.Zero);
        }
        private void 禁用窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewMain.SelectedNode != null)
                MAppWorkCall3(210, selectHwnd, IntPtr.Zero);
        }

        private void 显示逻辑区域ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
