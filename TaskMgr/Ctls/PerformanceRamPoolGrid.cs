using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class PerformanceRamPoolGrid : Control
    {
        public PerformanceRamPoolGrid()
        {
            BgBrush = new SolidBrush(FormMain.RamBgColor);
            DrawPen = new Pen(FormMain.RamDrawColor);
            GridPen = new Pen(Color.FromArgb(206, 176, 215));
            TextBrush = new SolidBrush(Color.Gray);
            stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
        }

        public Color BgColor
        {
            get { return (BgBrush as SolidBrush).Color; }
            set
            {
                if (BgBrush != null)
                    (BgBrush as SolidBrush).Color = value;
            }
        }
        public Color DrawColor
        {
            get { return DrawPen.Color; }
            set
            {
                if (DrawPen != null)
                    DrawPen.Color = value;
            }
        }
        public Color GridColor
        {
            get { return GridPen.Color; }
            set
            {
                if (GridPen != null)
                    GridPen.Color = value;
            }
        }
        public Brush BgBrush { get; set; }
        public Pen DrawPen { get; set; }
        public Pen GridPen { get; set; }
        [Bindable(true), Localizable(true)]
        public string LeftText { get; set; }
        [Bindable(true), Localizable(true)]
        public string RightText { get; set; }
        public Color TextColor
        {
            get { return (TextBrush as SolidBrush).Color; }
            set
            {
                if (TextBrush != null)
                    (TextBrush as SolidBrush).Color = value;
            }
        }

        private Brush TextBrush { get; set; }
        private StringFormat stringFormatRight = null;

        public double VauleUsing { get; set; }
        public double VauleCompressed { get; set; }

        public int TopTextHeight { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle r = new Rectangle(0, TopTextHeight, Width-1, Height - TopTextHeight-1);

            if (LeftText != "") g.DrawString(LeftText, Font, TextBrush, 0, 0);
            if (RightText != "") g.DrawString(RightText, Font, TextBrush, new Rectangle(0, 0, Width, TopTextHeight), stringFormatRight);

            int w1 = 0;
            if (VauleUsing > 0 && VauleUsing < 1)
            {
                w1 = (int)(VauleUsing * Width);
                if (w1 > 1)
                {
                    g.FillRectangle(BgBrush, new Rectangle(0, r.Top, w1, r.Height));
                    g.DrawLine(DrawPen, w1, r.Top, w1, r.Bottom);
                }
            }
            if (VauleCompressed > 0 && VauleCompressed < 1)
            {
                int w2 = (int)(VauleCompressed * Width);
                if (w2 > 1) g.DrawLine(DrawPen, w1 + w2, r.Top, w1 + w2, r.Bottom);
            }

            g.DrawRectangle(DrawPen, r);
        }
    }
}
