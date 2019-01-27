using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgrUpdate
{
    public partial class FormA : Form
    {
        public FormA()
        {
            InitializeComponent();
        }
        public FormA(string s, string n)
        {
            InitializeComponent();
            textBox1.Text = s;
            Text = "MD5 for file : " + n;
        }
    }
}
