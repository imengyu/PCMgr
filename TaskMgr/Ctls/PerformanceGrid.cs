using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskMgr.Ctls
{
    public class PerformanceGrid : Control
    {
        public PerformanceGrid()
        {
            BgBrush = new SolidBrush(Color.FromArgb(241, 246, 250));
            DrawPen = new Pen(Color.FromArgb(17,125,187));
            GridPen = new Pen(Color.FromArgb(206, 226, 240));
            TextBrush = Brushes.Gray;
            dataIem = new List<int>();
            stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
            TopTextHeight = 20;
            BottomTextHeight = 20;
        }

        private List<int> dataIem = null;
        private StringFormat stringFormatRight = null;

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
        public Brush TextBrush { get; set; }
        public Brush BgBrush { get; set; }
        public Pen DrawPen { get; set; }
        public Pen GridPen { get; set; }
        public string LeftText { get; set; }
        public string RightText { get; set; }
        public string LeftBottomText { get; set; }
        public string RightBottomText { get; set; }
        public int TopTextHeight { get; set; }
        public int BottomTextHeight { get; set; }

        public List<int> Data { get { return dataIem; } }

        public void AddData(int d)
        {
            Data.RemoveAt(0);
            Data.Add(d);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (LeftText != "") g.DrawString(LeftText, Font, TextBrush, 0, 0);
            if (RightText != "") g.DrawString(RightText, Font, TextBrush, new Rectangle(0, 0, Width, TopTextHeight), stringFormatRight);

            float single = 1.0F * Width / (60 - 1);
            float division = 1.0F * (Height - TopTextHeight - BottomTextHeight) / 100;
            float offset = 1.0F * (60 - Data.Count) * single;

            for (int i = 1; i < 10; i++)
            {
                int y = (int)(i * 10 * division) + TopTextHeight;
                g.DrawLine(GridPen, 1, y, Width - 2, y);
            }

            for (int i = 1; i <= 6; i++)
            {
                int x = (int)((i * 10) * single);
                g.DrawLine(GridPen, x, TopTextHeight, x, Height - 2 - BottomTextHeight);
            }

            if (Data.Count > 0)
            {
                List<PointF> ps = new List<PointF>();
                ps.Add(new PointF(0, Height - BottomTextHeight));
                ps.Add(new PointF(offset + 0 * single, Height + TopTextHeight - Data[0] * division));

                for (int i = 1; i < Data.Count; i++)
                {
                    ps.Add(new PointF(offset + i * single, Height - BottomTextHeight - Data[i] * division));
                    g.DrawLine(DrawPen, ps[ps.Count - 2], ps[ps.Count - 1]);
                }

                ps.Add(new PointF(Width, Height));
                if (BgBrush != Brushes.White)
                    g.FillClosedCurve(BgBrush, ps.ToArray());
            }

            if (LeftBottomText != "") g.DrawString(LeftBottomText, Font, TextBrush, 0, Height - BottomTextHeight);
            if (RightBottomText != "") g.DrawString(RightBottomText, Font, TextBrush, new Rectangle(20, Height - BottomTextHeight, Width - 20, BottomTextHeight), stringFormatRight);

            g.DrawRectangle(DrawPen, new Rectangle(0, TopTextHeight, Width - 1, Height - 1 - TopTextHeight - BottomTextHeight));
        }
    }
}
