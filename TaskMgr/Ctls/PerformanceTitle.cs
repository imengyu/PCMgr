using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskMgr.Ctls
{
    public class PerformanceTitle : Control
    {
        public PerformanceTitle()
        {
            TitleFont = new Font("微软雅黑", 18);
            SmallTitleFont = new Font("微软雅黑", 12);
            stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
            stringFormatRight.LineAlignment = StringAlignment.Far;
        }

        public Font TitleFont { get; set; }
        public Font SmallTitleFont { get; set; }
        public string Title { get; set; }
        public string SmallTitle { get; set; }

        private StringFormat stringFormatRight = null;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            using (SolidBrush s = new SolidBrush(ForeColor))
            {
                if (Title != "") g.DrawString(Title, TitleFont, s, 0, 0);
                if (SmallTitle != "") g.DrawString(SmallTitle, SmallTitleFont, s, new Rectangle(0, 0, Width, Height), stringFormatRight);
            }
        }
    }
}
