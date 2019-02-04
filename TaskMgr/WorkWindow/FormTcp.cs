using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static PCMgr.NativeMethods;

namespace PCMgr.WorkWindow
{
    public partial class FormTcp : Form
    {
        public FormTcp(FormMain m)
        {
            InitializeComponent();
            formMain = m;
            _CALLBACK = CALLBACK;
            _CALLBACKPtr = Marshal.GetFunctionPointerForDelegate(_CALLBACK);
        }

        private MCONNECTION_ENUM_CALLBACK _CALLBACK;
        private IntPtr _CALLBACKPtr = IntPtr.Zero;
        private FormMain formMain;

        private bool CALLBACK(uint ProcessId, uint Protcol, string LocalAddr, uint LocalPort, string RemoteAddr, uint RemotePort, uint state)
        {
            ListViewItem li = new ListViewItem(FindProcessName(ProcessId));
            li.SubItems.Add(ProtcolToString(Protcol));
            li.SubItems.Add(LocalAddr);
            li.SubItems.Add(LocalPort.ToString());
            li.SubItems.Add(RemoteAddr);
            li.SubItems.Add(RemotePort.ToString());
            li.SubItems.Add(MPERF_NET_TcpConnectionStateToString(state));
            li.Tag = ProcessId;
            listTcp.Items.Add(li);

            return true;
        }

        private string ProtcolToString(uint Protcol)
        {
            switch(Protcol)
            {
                case M_CONNECTION_TYPE_TCP: return "Tcp";
                case M_CONNECTION_TYPE_TCP6: return "Tcp6";
                case M_CONNECTION_TYPE_UDP: return "Udp";
                case M_CONNECTION_TYPE_UDP6: return "Udp6";
                default: return "";
            }
        }
        private string FindProcessName(uint ProcessId)
        {
            string rs = "";
            if (formMain != null)
            {
               if(formMain.MainPageProcess.Inited)
                {
                    rs = formMain.MainPageProcess.ProcessListFindProcessName(ProcessId) + " (" + ProcessId + ")";
                    if(rs == null) rs = " (" + ProcessId + ")";
                }
            }
            return rs;
        }
        private void FormTcp_Load(object sender, EventArgs e)
        {
            contextMenuStrip1.Renderer = new Helpers.ClassicalMenuRender(Handle);
            LoadList();
        }
        private void buttonRefesh_Click(object sender, EventArgs e)
        {
            LoadList();
        }

        private void LoadList()
        {
            listTcp.Items.Clear();
            MPERF_NET_UpdateAllProcessNetInfo();
            MPERF_NET_EnumTcpConnections(_CALLBACKPtr);
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listTcp.SelectedItems.Count > 0)
            {
                string s = "";
                for (int i = 0; i < listTcp.Columns.Count; i++)
                {
                    s += " " + listTcp.Columns[i].Text + " : ";
                    s += listTcp.SelectedItems[0].SubItems[i].Text;
                }
                MCopyToClipboard2(s);
            }
        }
        private void 转到进程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listTcp.SelectedItems.Count > 0)
            {
                uint pid = (uint)listTcp.SelectedItems[0].Tag;
                formMain.MainPageProcess.ProcessListSelectProcess(pid);
            }
        }
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadList();
        }

        private void listTcp_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(MousePosition);
        }
        private void listTcp_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (listTcp.SelectedItems.Count > 0)
                {
                    ListViewItem item = listTcp.SelectedItems[0];
                    System.Drawing.Point p = item.Position; p.X = 0;
                    p = listTcp.PointToScreen(p);
                    contextMenuStrip1.Show(p);
                }
            }
        }
    }
}
