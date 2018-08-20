using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormFileTool : Form
    {
        public FormFileTool()
        {
            InitializeComponent();
        }

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MFM_DeleteFileForce(string path);
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern bool MFM_GetFileInformationString(string szFile, StringBuilder strbuf, uint bufsize);

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBoxFilePath.Text = openFileDialog1.FileName;
        }

        private void btnCheckUsing_Click(object sender, EventArgs e)
        {
            new FormCheckFileUse(textBoxFilePath.Text).ShowDialog();
        }

        private void btnFroceDelete_Click(object sender, EventArgs e)
        {
            if (textBoxFilePath.Text != "")
            {
                if (MFM_DeleteFileForce(textBoxFilePath.Text))
                    MessageBox.Show(FormMain.str_frocedelsuccess);

            }
        }

        private void btnFillWithData_Click(object sender, EventArgs e)
        {
            if (textBoxFilePath.Text != "")
                new FormFillWithData(textBoxFilePath.Text).ShowDialog();
        }

        private void btnDisplayFileInfo_Click(object sender, EventArgs e)
        {
            if (textBoxFilePath.Text != "")
            {
                StringBuilder buf = new StringBuilder(256);
                if (MFM_GetFileInformationString(textBoxFilePath.Text, buf, 256))
                    labelFileInformation.Text = buf.ToString();
                else labelFileInformation.Text = FormMain.str_getfileinfofailed;
            }
        }
    }
}
