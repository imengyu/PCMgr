using System;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        public void BtnClose()
        {
            Close();
        }
        public void BtnRunUpdate()
        {
            NativeMethods.MRunExe("PCMgrUpdate.exe","");
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            if(!NativeMethods.MREG_IsCurrentIEVersionOK(11000, NativeMethods.MAppGetName()))
                NativeMethods.MREG_SetCurrentIEVersion(11000, NativeMethods.MAppGetName());

            webBrowser1.ObjectForScripting = this;
            webBrowser1.Navigate("about:blank");
            webBrowser1.Document.Write(Lanuages.LanuageMgr.IsChinese ? Properties.Resources.PageAbout : Properties.Resources.PageAboutEn);
            webBrowser1.Document.GetElementById("txt_title").SetAttribute("src", Properties.Resources.ImgAboutTitle);
            webBrowser1.Document.GetElementById("txt_show_ver").InnerText = NativeMethods.MAppGetVersion() + " / " + NativeMethods.MAppGetBulidDate();
            webBrowser1.Document.GetElementById("txt_current_ver").InnerText = NativeMethods.MAppGetVersion();

            if (NativeMethods.MIs64BitOS()) {
#if _X64_
                webBrowser1.Document.GetElementById("img_platform").SetAttribute("src", Properties.Resources.ImgX64);
#elif _X86_
                webBrowser1.Document.GetElementById("img_platform").SetAttribute("src", Properties.Resources.ImgX32OnX64);
#endif
            }
            else webBrowser1.Document.GetElementById("img_platform").SetAttribute("src", Properties.Resources.ImgX32);
        }
    }
}
