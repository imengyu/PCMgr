using System;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormDelFileProgress : Form
    {
        public FormDelFileProgress()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | 0x200;
                return myCp;
            }
        }

        private void FormDelFileProgress_Load(object sender, EventArgs e)
        {

        }
    }
}
