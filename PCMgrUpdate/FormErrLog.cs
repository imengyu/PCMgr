using System;
using System.Windows.Forms;

namespace PCMgrUpdate
{
    public partial class FormErrLog : Form
    {
        public FormErrLog()
        {
            InitializeComponent();
        }
        public FormErrLog(string s)
        {
            InitializeComponent();
            textBox1.Text = s;
        }
        private void FormErrLog_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
