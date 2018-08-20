using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            MaxUnitPen = new Pen(Color.FromArgb(164, 222, 154));
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
            MaxScaleText = "";
            MaxValue = 100;
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
        public Pen MaxUnitPen { get; set; }

        [Bindable(true), Localizable(true)]
        public string LeftText { get; set; }
        [Bindable(true), Localizable(true)]
        public string RightText { get; set; }
        [Bindable(true), Localizable(true)]
        public string LeftBottomText { get; set; }
        [Bindable(true), Localizable(true)]
        public string RightBottomText { get; set; }

        public int TopTextHeight { get; set; }
        public int BottomTextHeight { get; set; }

        public bool DrawData2 { get; set; }
        public bool DrawData2Bg { get; set; }

        /// <summary>
        /// 数据1
        /// </summary>
        public List<int> Data { get { return dataIem; } }
        /// <summary>
        /// 数据2
        /// </summary>
        public List<int> Data2 { get { return dataIem2; } }

        private PointF[] pts = new PointF[62];
        private int lastMaxData = 0;
        private int lastDataAverage1 = 0;
        private int lastDataAverage2 = 0;
        private int lastDataAverage1_zeroCount = 0;
        private int lastDataAverage2_zeroCount = 0;

        /// <summary>
        /// .数据12平均值
        /// </summary>
        public int DataAverage { get { return (lastDataAverage1 + lastDataAverage2) / 2; } }
        /// <summary>
        /// 数据1平均值
        /// </summary>
        public int DataAverage1 { get { return lastDataAverage1; } }
        /// <summary>
        ///  数据2平均值
        /// </summary>
        public int DataAverage2 { get { return lastDataAverage2; } }
        /// <summary>
        /// 最大数据
        /// </summary>
        public int MaxData { get { return lastMaxData; } }

        /// <summary>
        /// 标尺虚线文字
        /// </summary>
        public string MaxScaleText { get; set; }
        /// <summary>
        /// 标尺虚线位置
        /// </summary>
        public int MaxScaleValue { get; set; }

        /// <summary>
        /// 最大单位
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// 添加实线数据
        /// </summary>
        public void AddData(int d)
        {
            dataIem.RemoveAt(0);
            dataIem.Add(d);
        }
        ///
        /// <summary>
        /// 添加虚线数据
        /// </summary>
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
            float division = 1.0F * (Height - TopTextHeight - BottomTextHeight) / MaxValue;
            float divisionGrid = 1.0F * (Height - TopTextHeight - BottomTextHeight) / 100;
            float offset = 1.0F * (60 - dataIem.Count) * single;

            for (int i = 1; i < 10; i++)
            {
                int y = (int)(i * 10 * divisionGrid) + TopTextHeight;
                g.DrawLine(GridPen, 1, y, Width - 2, y);
            }
            for (int i = 1; i <= 6; i++)
            {
                int x = (int)((i * 10) * single);
                g.DrawLine(GridPen, x, TopTextHeight, x, Height - 2 - BottomTextHeight);
            }

            lastMaxData = 0;
            lastDataAverage1 = 0;
            lastDataAverage2 = 0;
            lastDataAverage1_zeroCount = 0;
            lastDataAverage2_zeroCount = 0;

            if (dataIem.Count > 0)
            {
                pts[0].X = 0;
                pts[0].Y = Height - BottomTextHeight;//左下

                for (int i = 0; i < dataIem.Count; i++)
                {
                    if (dataIem[i] > lastMaxData) lastMaxData = dataIem[i];
                    if (dataIem[i] > 0) lastDataAverage1 += dataIem[i];
                    else lastDataAverage1_zeroCount++;

                    pts[i + 1].X = offset + i * single;
                    pts[i + 1].Y = Height - BottomTextHeight - dataIem[i] * division - 1;
                    if (pts[i + 1].Y < TopTextHeight) pts[i + 1].Y = TopTextHeight;
                }
                if (lastDataAverage1_zeroCount != dataIem.Count) lastDataAverage1 /= dataIem.Count - lastDataAverage1_zeroCount;

                pts[61].X = Width;
                pts[61].Y = Height - BottomTextHeight;//右下

                if (BgBrush != Brushes.White) g.FillClosedCurve(BgBrush, pts);
                for (int i = 1; i < 61; i++) g.DrawLine(DrawPen, pts[i - 1], pts[i]);
            }
            if (DrawData2 && dataIem2.Count > 0)
            {
                pts[0].X = 0;
                pts[0].Y = Height - BottomTextHeight;//左下

                for (int i = 0; i < dataIem2.Count; i++)
                {
                    if (dataIem2[i] > lastMaxData) lastMaxData = dataIem2[i];
                    if (dataIem2[i] > 0) lastDataAverage2 += dataIem2[i];
                    else lastDataAverage2_zeroCount++;

                    pts[i + 1].X = offset + i * single;
                    pts[i + 1].Y = Height - BottomTextHeight - dataIem2[i] * division - 1;
                    if (pts[i + 1].Y < TopTextHeight) pts[i + 1].Y = TopTextHeight;
                }
                if(dataIem2.Count != lastDataAverage2_zeroCount) lastDataAverage2 /= dataIem2.Count - lastDataAverage2_zeroCount;

                pts[61].X = Width;
                pts[61].Y = Height - BottomTextHeight;//右下

                if (DrawData2Bg && BgBrush != Brushes.White) g.FillClosedCurve(BgBrush, pts);
                for (int i = 1; i < 61; i++) g.DrawLine(DrawPen2, pts[i - 1], pts[i]);
            }

            if (MaxScaleValue > 0)
            {
                int y = (int)(TopTextHeight + (Height - TopTextHeight - BottomTextHeight) * (1 - MaxScaleValue / (double)MaxValue));
                g.DrawLine(MaxUnitPen, 1, y, Width - 2, y);
                if (MaxScaleText != "")
                    g.DrawString(MaxScaleText, Font, TextBrush, new Rectangle(Width - 90, y + 2, 90, 20), stringFormatRight);
            }

            if (LeftBottomText != "") g.DrawString(LeftBottomText, Font, TextBrush, 0, Height - BottomTextHeight + 2);
            if (RightBottomText != "") g.DrawString(RightBottomText, Font, TextBrush, new Rectangle(20, Height - BottomTextHeight + 2, Width - 20, BottomTextHeight), stringFormatRight);


            g.DrawRectangle(DrawPen, new Rectangle(0, TopTextHeight, Width - 1, Height - 1 - TopTextHeight - BottomTextHeight));
        }
    }
}
