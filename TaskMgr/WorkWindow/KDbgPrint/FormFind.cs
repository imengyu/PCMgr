using System;
using System.Windows.Forms;

namespace PCMgr.WorkWindow.KDbgPrint
{
    public partial class FormFind : Form
    {
        public FormFind()
        {
            InitializeComponent();
        }

        private void FormFind_Load(object sender, EventArgs e)
        {

        }
        public bool Down { get { return radioButtonFindLow.Checked; } set { radioButtonFindLow.Checked = value; } }
        public bool Up { get { return radioButtonFindUp.Checked; } set { radioButtonFindUp.Checked = value; } }
        public string KeyWord { get { return textBoxEnter.Text; } }
        public bool FullSearch { get { return checkBoxFullSearch.Checked; } set { checkBoxFullSearch.Checked = value; } }
        public bool CaseSensitive { get { return checkBoxDevideLorH.Checked; } set { checkBoxDevideLorH.Checked = value; } }

        public event EventHandler Find;

        private void buttonFind_Click(object sender, EventArgs e)
        {
            Find?.Invoke(this, e);
        }
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
