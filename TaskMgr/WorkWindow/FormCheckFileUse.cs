using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormCheckFileUse : Form
    {
        public FormCheckFileUse()
        {
            InitializeComponent();
        }
        public FormCheckFileUse(string filepath)
        {
            InitializeComponent();
            filePath = filepath;
        }

        private string filePath = "";
        private void FormCheckFileUse_Load(object sender, EventArgs e)
        {
            FormMain.MAppWorkCall3(182, listViewUsing.Handle, IntPtr.Zero);

            Callback = _MFUSEINGCALLBACK;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);
            labelFileInfo.Text = filePath;
            btnRefesh_Click(sender, e);
        }


        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_FileExist([MarshalAs(UnmanagedType.LPWStr)]string filepath);
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_EnumFileHandles([MarshalAs(UnmanagedType.LPWStr)]string filepath, IntPtr callback);

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_CloseHandleWithProcess(IntPtr handle);


        private IntPtr CallbackPtr = IntPtr.Zero;
        private MFUSEINGCALLBACK Callback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MFUSEINGCALLBACK(IntPtr handle, uint dwpid, [MarshalAs(UnmanagedType.LPWStr)]string value, int fileType, [MarshalAs(UnmanagedType.LPWStr)]string exepath);

        private void _MFUSEINGCALLBACK(IntPtr handle, uint dwpid, [MarshalAs(UnmanagedType.LPWStr)]string value, int fileType, [MarshalAs(UnmanagedType.LPWStr)]string exepath)
        {
            string exename = exepath == "" ? "" : System.IO.Path.GetFileName(exepath);
            ListViewItem li = new ListViewItem(exename);
            li.SubItems.Add(value);
            li.SubItems.Add(dwpid.ToString());
            li.SubItems.Add(exepath);
            li.SubItems.Add(fileType.ToString());
            li.Tag = handle;
            listViewUsing.Items.Add(li);
        }

        private void btnRefesh_Click(object sender, EventArgs e)
        {
            listViewUsing.Items.Clear();
            bool succeed = false;
            if (MFM_FileExist(filePath))
            {
                if (MFM_EnumFileHandles(filePath, CallbackPtr))
                {
                    btnRelease.Enabled = true;
                    btnReleaseAll.Enabled = true;
                    succeed = true;
                }
            }
            else MessageBox.Show(FormMain.str_filenotexist);

            if (!succeed)
            {
                btnRelease.Enabled = false;
                btnReleaseAll.Enabled = false;
                MessageBox.Show(FormMain.str_failed);
            }
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (listViewUsing.SelectedItems.Count > 0)
            {
                IntPtr handle = (IntPtr)listViewUsing.SelectedItems[0].Tag;
                if (M_SU_CloseHandleWithProcess(handle))
                    MessageBox.Show(FormMain.str_UnlockFileSuccess);
                else MessageBox.Show(FormMain.str_UnlockFileFailed);
            }

        }

        private void btnReleaseAll_Click(object sender, EventArgs e)
        {
            if (listViewUsing.Items.Count > 0)
            {
                for (int i = listViewUsing.Items.Count; i >= 0; i--)
                {
                    IntPtr handle = (IntPtr)listViewUsing.Items[i].Tag;
                    if (!M_SU_CloseHandleWithProcess(handle))
                        MessageBox.Show(FormMain.str_UnlockFileFailed + " : " + listViewUsing.Items[i].SubItems[1].Text);
                }
            }
        }

        private void listViewUsing_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRelease.Enabled = listViewUsing.SelectedItems.Count > 0;
        }
    }
}
