using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static PCMgr.NativeMethods;

namespace PCMgr.WorkWindow
{
    public partial class FormKDA : Form
    {
        public FormKDA()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            das();
        }

#if _X64_
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_KDA(IntPtr callback, UInt64 startaddress, UInt64 size);
#else
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_KDA(IntPtr callback, uint startaddress, uint size);
#endif
        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool M_SU_KDA_Test(IntPtr callback);

        private IntPtr CallbackPtr = IntPtr.Zero;
        private DACALLBACK Callback;

#if _X64_
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DACALLBACK(UInt64 curaddress, IntPtr address, IntPtr shell, IntPtr bariny, IntPtr asm);
        private void _DACALLBACK(UInt64 curaddress, IntPtr addressstr, IntPtr shell, IntPtr bariny, IntPtr asm)
        {
            this.curaddress = curaddress;
            showedsize = curaddress - address;
            string barinystr = Marshal.PtrToStringUni(bariny);
            textBoxBariny.Text += barinystr;
            add_Item(Marshal.PtrToStringUni(addressstr), Marshal.PtrToStringUni(shell), barinystr, Marshal.PtrToStringUni(asm));
            if (showedsize == oncemaxdsize)
                add_Item("", "", FormMain.str_DblClickToDa, "conload");
        }
#else
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DACALLBACK(UInt32 curaddress, IntPtr address, IntPtr shell, IntPtr bariny, IntPtr asm);
        private void _DACALLBACK(UInt32 curaddress, IntPtr addressstr, IntPtr shell, IntPtr bariny, IntPtr asm)
        {
            this.curaddress = curaddress;
            showedsize = curaddress - address;
            string barinystr = Marshal.PtrToStringUni(bariny);
            textBoxBariny.Text += barinystr;
            add_Item(Marshal.PtrToStringUni(addressstr), Marshal.PtrToStringUni(shell), barinystr, Marshal.PtrToStringUni(asm));
            if (showedsize == oncemaxdsize)
                add_Item("", "", FormMain.str_DblClickToDa, "conload");
        }
#endif
        private void FormDA_Load(object sender, EventArgs e)
        {
            FormSettings.LoadFontSettingForUI(this);
            Callback = _DACALLBACK;
            CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);
            textBoxDesize.Text = "000000FF";
        }



#if _X64_
        private UInt64 curaddress = 0;
        private UInt64 address = 0;
        private UInt64 size = 0;
        private UInt64 showedsize = 0;
        private const UInt64 oncemaxdsize = 0xFF;
#else
        private UInt32 curaddress = 0;
        private UInt32 address = 0;
        private UInt32 size = 0;
        private UInt32 showedsize = 0;
        private const UInt32 oncemaxdsize = 0xFF;
#endif

        private void dastest()
        {
            labelErrStatus.Hide();
            listViewDA.Show();

            listViewDA.Items.Clear();
            textBoxBariny.Text = "";
            M_SU_KDA_Test(CallbackPtr);
        }
        private void das()
        {
            labelErrStatus.Hide();
            listViewDA.Show();

            address = 0;
            size = 0;
            showedsize = 0;
            listViewDA.Items.Clear();
            textBoxBariny.Text = "";
            if (!MCanUseKernel())
            {
                show_err(FormMain.str_DriverLoadFailed);
                return;
            }
            if (textBoxTargetAddress.Text == "")
            {
                show_err(FormMain.str_PleaseEnterTargetAddress);
                return;
            }
            if (textBoxDesize.Text == "")
            {
                show_err(FormMain.str_PleaseEnterDaSize);
                return;
            }

#if _X64_
            address = Convert.ToUInt64(textBoxTargetAddress.Text, 16);
            size = Convert.ToUInt64(textBoxDesize.Text, 16);
#else
            address = Convert.ToUInt32(textBoxTargetAddress.Text, 16);
            size = Convert.ToUInt32(textBoxDesize.Text, 16);
#endif
            bool rs = false;
            if (address <= 0)
            {
                show_err("Enter address not valid : " + address.ToString());
                return;
            }
            if (size <= 0)
            {
                show_err("Enter size not valid : " + size.ToString());
                return;
            }
            if (size > oncemaxdsize)
                rs = M_SU_KDA(CallbackPtr, address, oncemaxdsize);
            else rs = M_SU_KDA(CallbackPtr, address, size);
            if (!rs) NativeMethods.LogErr("KDA Failed!");
        }
        private void show_err(string s)
        {
            listViewDA.Hide();
            labelErrStatus.Text = s;
            labelErrStatus.Show();
        }
        private void add_Item(string address, string shell, string binary, string asm, string tag = null)
        {
            ListViewItem li = new ListViewItem(address);
            li.SubItems.Add(binary);
            li.SubItems.Add(shell);
            li.SubItems.Add(asm);
            li.Tag = tag;
            listViewDA.Items.Add(li);
        }

        private void listViewDA_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewDA.SelectedItems.Count > 0)
            {
                if (listViewDA.SelectedItems[0].Tag != null && listViewDA.SelectedItems[0].Tag.ToString() == "conload")
                {
#if _X64_
                    UInt64 thissize = size - showedsize;
#else
                    UInt32 thissize = size - showedsize;
#endif
                    if (thissize > oncemaxdsize)
                        M_SU_KDA(CallbackPtr, curaddress, oncemaxdsize);
                    else M_SU_KDA(CallbackPtr, curaddress, thissize);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            dastest();
        }

        private void 复制地址ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MCopyToClipboard2(listViewDA.SelectedItems[0].SubItems[0].Text);
        }
        private void 复制二进制码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MCopyToClipboard2(listViewDA.SelectedItems[0].SubItems[1].Text);
        }
        private void 复制OpCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MCopyToClipboard2(listViewDA.SelectedItems[0].SubItems[2].Text);
        }
        private void 复制汇编代码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MCopyToClipboard2(listViewDA.SelectedItems[0].SubItems[3].Text);
        }

        private void listViewDA_MouseClick(object sender, MouseEventArgs e)
        {
            if (listViewDA.SelectedItems.Count > 0)
            {
                if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Show(MousePosition);
                }
            }
        }
    }
}
