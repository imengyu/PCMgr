using PCMgr.Aero.TaskDialog;
using PCMgr.Lanuages;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static PCMgr.NativeMethods;

namespace PCMgr.WorkWindow
{
    public partial class FormLoadDriver : Form
    {
        public FormLoadDriver()
        {
            InitializeComponent(); 
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MUnLoadKernelDriver([MarshalAs(UnmanagedType.LPWStr)]string szSvrName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MLoadKernelDriver([MarshalAs(UnmanagedType.LPWStr)]string lpszDriverName, [MarshalAs(UnmanagedType.LPWStr)]string driverPath, [MarshalAs(UnmanagedType.LPWStr)]string lpszDisplayName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_FileExist([MarshalAs(UnmanagedType.LPWStr)]string path);

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBoxDriverPath.Text = openFileDialog1.FileName;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            string path = textBoxDriverPath.Text;
            if (path == "")
            {
                TaskDialog.Show(LanuageMgr.GetStr("PleaseChooseDriver"), LanuageFBuffers.Str_TipTitle);
                return;
            }
            if(!MFM_FileExist(path))
            {
                TaskDialog.Show(LanuageMgr.GetStr("PathUnExists"), LanuageFBuffers.Str_TipTitle);
                return;
            }
            if (textBoxServName.Text == "") textBoxServName.Text = System.IO.Path.GetFileNameWithoutExtension(path);
            if (MLoadKernelDriver(textBoxServName.Text, path, textBoxDrvServDsb.Text)) TaskDialog.Show(LanuageMgr.GetStr("DriverLoadSuccessFull"), LanuageFBuffers.Str_TipTitle);
            else TaskDialog.Show(string.Format(LanuageMgr.GetStr("DriverLoadFailed"), Win32.GetLastError()), LanuageFBuffers.Str_TipTitle);
        }
        private void buttonUnLoad_Click(object sender, EventArgs e)
        {
            if (textBoxServName.Text == "") TaskDialog.Show(LanuageMgr.GetStr("PleaseEnterDriverServiceName"), LanuageFBuffers.Str_TipTitle);
            else
            {
                if (MUnLoadKernelDriver(textBoxServName.Text)) TaskDialog.Show(LanuageMgr.GetStr("DriverUnLoadSuccessFull"), LanuageFBuffers.Str_TipTitle);
                else TaskDialog.Show(string.Format(LanuageMgr.GetStr("DriverUnLoadFailed"), Win32.GetLastError()), LanuageFBuffers.Str_TipTitle);
            }
        }

        private void textBoxDriverPath_DragDrop(object sender, DragEventArgs e)
        {
            textBoxDriverPath.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }
        private void textBoxDriverPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FormLoadDriver_Load(object sender, EventArgs e)
        {
            FormSettings.LoadFontSettingForUI(this);
        }
    }
}
