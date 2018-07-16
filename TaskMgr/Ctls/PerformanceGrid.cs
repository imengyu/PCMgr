using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class PerformanceGrid : Control
    {
        public PerformanceGrid()
        {
            DrawData2 = false;
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            BgBrush = new SolidBrush(Color.FromArgb(241, 246, 250));
            DrawPen = new Pen(Color.FromArgb(17, 125, 187));
            DrawPen2 = new Pen(Color.FromArgb(17, 125, 187));
            DrawPen2.DashStyle = DashStyle.Dash;
            GridPen = new Pen(Color.FromArgb(206, 226, 240));
            TextBrush = Brushes.Gray;
            dataIem = new List<int>();
            for (int i = 0; i < 60; i++)
                dataIem.Add(0);
            dataIem2 = new List<int>();
            for (int i = 0; i < 60; i++)
                dataIem2.Add(0);
            stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
            TopTextHeight = 20;
            BottomTextHeight = 20;
        }

        private List<int> dataIem2 = null;
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
        public Color DrawColor2
        {
            get { return DrawPen2.Color; }
            set
            {
                if (DrawPen2 != null)
                    DrawPen2.Color = value;
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
        public Pen DrawPen2 { get; set; }
        public Pen GridPen { get; set; }
        public string LeftText { get; set; }
        public string RightText { get; set; }
        public string LeftBottomText { get; set; }
        public string RightBottomText { get; set; }
        public int TopTextHeight { get; set; }
        public int BottomTextHeight { get; set; }

        public bool DrawData2 { get; set; }
        public bool DrawData2Bg { get; set; }

        public List<int> Data { get { return dataIem; } }
        public List<int> Data2 { get { return dataIem2; } }

        private PointF[] pts = new PointF[62];

        public void AddData(int d)
        {
            dataIem.RemoveAt(0);
            dataIem.Add(d);
        }
        public void AddData2(int d)
        {
            dataIem2.RemoveAt(0);
            dataIem2.Add(d);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            //g.SmoothingMode = SmoothingMode.AntiAlias;     

            if (LeftText != "") g.DrawString(LeftText, Font, TextBrush, 0, 0);
            if (RightText != "") g.DrawString(RightText, Font, TextBrush, new Rectangle(0, 0, Width, TopTextHeight), stringFormatRight);

            float single = 1.0F * Width / (60 - 1);
            float division = 1.0F * (Height - TopTextHeight - BottomTextHeight) / 100;
            float offset = 1.0F * (60 - dataIem.Count) * single;

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

            if (dataIem.Count > 0)
            {
                pts[0].X = 0;
                pts[0].Y = Height - BottomTextHeight;//左下

                for (int i = 0; i < dataIem.Count; i++)
                {
                    pts[i + 1].X = offset + i * single;
                    pts[i + 1].Y = Height - BottomTextHeight - dataIem[i] * division - 1;
                }

                pts[61].X = Width;
                pts[61].Y = Height - BottomTextHeight;//右下

                if (BgBrush != Brushes.White) g.FillClosedCurve(BgBrush, pts);
                for (int i = 1; i < 61; i++) g.DrawLine(DrawPen, pts[i - 1], pts[i]);
            }
            if (DrawData2 && dataIem2.Count > 0)
            {
                pts[0].X = 0;
                pts[0].Y = Height - BottomTextHeight;//左下

                for (int i = 0; i < dataIem.Count; i++)
                {
                    pts[i + 1].X = offset + i * single;
                    pts[i + 1].Y = Height - BottomTextHeight - dataIem2[i] * division - 1;
                }

                pts[61].X = Width;
                pts[61].Y = Height - BottomTextHeight;//右下

                if (DrawData2Bg && BgBrush != Brushes.White) g.FillClosedCurve(BgBrush, pts);
                for (int i = 1; i < 61; i++) g.DrawLine(DrawPen2, pts[i - 1], pts[i]);
            }

            if (LeftBottomText != "") g.DrawString(LeftBottomText, Font, TextBrush, 0, Height - BottomTextHeight + 2);
            if (RightBottomText != "") g.DrawString(RightBottomText, Font, TextBrush, new Rectangle(20, Height - BottomTextHeight + 2, Width - 20, BottomTextHeight), stringFormatRight);

            g.DrawRectangle(DrawPen, new Rectangle(0, TopTextHeight, Width - 1, Height - 1 - TopTextHeight - BottomTextHeight));
        }
    }
}
