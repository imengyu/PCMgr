using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class PerformanceTitle : Control
    {
        public PerformanceTitle()
        {
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            LoadAllFonts();
            stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
            stringFormatRight.LineAlignment = StringAlignment.Far;
            stringFormatRight.Trimming = StringTrimming.EllipsisCharacter;
            stringFormatRight.FormatFlags = StringFormatFlags.LineLimit;
        }

        private void LoadAllFonts()
        {
            TitleFont = new Font(Font.FontFamily, 18);
            SmallTitleFont = new Font(Font.FontFamily, 12);
        }

        public Font TitleFont { get; set; }
        public Font SmallTitleFont { get; set; }
        [Localizable(true)]
        public string Title { get; set; }
        [Localizable(true)]
        public string SmallTitle { get; set; }

        private StringFormat stringFormatRight = null;

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            LoadAllFonts();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            using (SolidBrush s = new SolidBrush(ForeColor))
            {
                if (Title != "") g.DrawString(Title, TitleFont, s, 0, 0);
                int w = (int)g.MeasureString(Title, TitleFont).Width;
                if (SmallTitle != "") g.DrawString(SmallTitle, SmallTitleFont, s, new Rectangle(w + 2, 0, Width - w - 2, Height), stringFormatRight);
            }
        }
    }
}
