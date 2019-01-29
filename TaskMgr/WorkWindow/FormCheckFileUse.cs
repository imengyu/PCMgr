using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static PCMgr.NativeMethods;

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
            MAppWorkCall3(182, listViewUsing.Handle, IntPtr.Zero);

            Callback = _MFUSEINGCALLBACK;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);
            labelFileInfo.Text = filePath;
            btnRefesh_Click(sender, e);
        }


        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool MFM_EnumFileHandles([MarshalAs(UnmanagedType.LPWStr)]string filepath, IntPtr callback);

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
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
            else MessageBox.Show(Lanuages.LanuageFBuffers.Str_FileNotExist);

            if (!succeed)
            {
                btnRelease.Enabled = false;
                btnReleaseAll.Enabled = false;
                MessageBox.Show(Lanuages.LanuageFBuffers.Str_Failed);
            }
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (listViewUsing.SelectedItems.Count > 0)
            {
                IntPtr handle = (IntPtr)listViewUsing.SelectedItems[0].Tag;
                if (M_SU_CloseHandleWithProcess(handle))
                    MessageBox.Show(Lanuages.LanuageMgr.GetStr("UnlockFileSuccess"));
                else MessageBox.Show(Lanuages.LanuageMgr.GetStr("UnlockFileFailed"));
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
                        MessageBox.Show(Lanuages.LanuageMgr.GetStr("UnlockFileFailed") + " : " + listViewUsing.Items[i].SubItems[1].Text);
                }
            }
        }

        private void listViewUsing_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRelease.Enabled = listViewUsing.SelectedItems.Count > 0;
        }
    }
}
