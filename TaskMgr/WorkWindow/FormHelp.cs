using PCMgr.Aero.TaskDialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormHelp : Form
    {
        public FormHelp()
        {
            InitializeComponent();
        }

        private void FormHelp_Load(object sender, EventArgs e)
        {
            if (!NativeMethods.MREG_IsCurrentIEVersionOK(11000, NativeMethods.MAppGetName()))
                if(!NativeMethods.MREG_SetCurrentIEVersion(11000, NativeMethods.MAppGetName()))
                {
                    TaskDialog t = new TaskDialog("此页面无法打开", "错误");
                    t.Content = "您的 IE 版本过低，无法打开此页面。或者，您可以使用其他浏览器访问我们的在线帮助：<A HREF=\"http://127.0.0.1/softs/pcmgr/help/\">在线帮助文档</A>";
                    t.EnableHyperlinks = true;
                    t.Show(this);
                }

            webBrowser1.ObjectForScripting = this;
            webBrowser1.Navigate("http://127.0.0.1/softs/pcmgr/help/");
        }
    }
}
