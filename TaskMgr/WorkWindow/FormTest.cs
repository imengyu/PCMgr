using PCMgr.Ctls;
using PCMgr.Helpers;
using PCMgr.Lanuages;
using System;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormTest : Form
    {
        public FormTest()
        {
            InitializeComponent();
        }

        private void FormTest_Load(object sender, EventArgs e)
        {
            contextMenuStripUWP.Renderer = new ClassicalMenuRender(Handle);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {

        }

        private void FormTest_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button== MouseButtons.Right)
            {
                contextMenuStripUWP.Show(MousePosition);
            }
        }
    }
}
