using System;
using System.Windows.Forms;
using static PCMgr.NativeMethods;

namespace PCMgr.WorkWindow
{
    public partial class FormSetAffinity : Form
    {
        public FormSetAffinity(uint pid, IntPtr hProcess)
        {
            InitializeComponent();
            currentPid = pid;
            currentHProcess = hProcess;
        }

        private IntPtr currentHProcess = IntPtr.Zero;
        private uint currentPid = 0;

        private static UInt32 systemAffinityMask = 0;

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            UInt32 affinityMask = 0;

            for (UInt16 i = 0; i < sizeof(UInt32) * 8; i++)
            {
                if (IsCpuItemChecked(i))
                    affinityMask |= (UInt32)1 << i;
            }

            MAppWorkCall3(171, currentHProcess, new IntPtr(affinityMask));
        }

        private void AddCpuItem(int index)
        {
            index++;
            if (index >= 1)
            {
                if (index >= listItems.Items.Count)
                {
                    ListViewItem li = new ListViewItem("CPU " + index);
                    li.Tag = "cpu";
                    listItems.Items.Add(li);
                }
            }
        }
        private void CheckCpuItem(int index)
        {
            if (index >= 0 && index < listItems.Items.Count - 1)
            {
                listItems.Items[index + 1].Checked = true;
            }
        }
        private bool IsCpuItemChecked(int index)
        {
            if (index >= 0 && index < listItems.Items.Count - 1)
                return listItems.Items[index + 1].Checked;
            return false;
        }
        private void FormSetAffinity_Load(object sender, EventArgs e)
        {
            FormSettings.LoadFontSettingForUI(this);
            if (currentPid > 4 && currentHProcess != IntPtr.Zero)
            {
                if (systemAffinityMask == 0)
                    MGetSystemAffinityMask(ref systemAffinityMask);
                if (systemAffinityMask == 0)
                    goto Error;

                UInt32 affinityMask = 0;
                int ntstatus = MGetProcessAffinityMask(currentHProcess, ref affinityMask);
                if (ntstatus == 0 && affinityMask != 0)
                {
                    for (int i = 0; i < 8 * 8; i++)
                    {
                        if ((i < sizeof(UInt32) * 8) && ((systemAffinityMask >> i) & 0x1) == 0x1)
                        {
                            if (((affinityMask >> i) & 0x1) == 0x1)
                            {
                                AddCpuItem(i);
                                CheckCpuItem(i);
                            }
                        }
                    }
                }
                else
                {
                    lbError.Text += "\nNTSTATUS : " + MNtStatusToStr(ntstatus);
                    goto Error;
                }
            }

            lbTip.Text = Lanuages.LanuageMgr.GetStr("AllowWhatCpusToRun") + "\"" + MAppGetCurSelectName() + "\"?";

            return;
            Error:
            {
                lbError.Show();
                lbTip.Hide();
                listItems.Hide();
                btnOk.Enabled = false;
            }
        }

        private void listItems_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void listItems_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag !=null && e.Item.Tag.ToString() == "all")
            {
                foreach (ListViewItem li in listItems.Items)
                    li.Checked = e.Item.Checked;
            }
        }
    }
}
