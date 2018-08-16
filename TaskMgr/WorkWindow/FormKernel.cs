using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormKernel : Form
    {
        public FormKernel()
        {
            InitializeComponent();
        }

        private void treeViewPages_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Name == "内核钩子")
            {
                FormMain.Instance.ShowFormHooks();
            }
        }
    }
}
