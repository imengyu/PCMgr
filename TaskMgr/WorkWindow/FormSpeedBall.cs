using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PCMgr.WorkWindow
{
    public partial class FormSpeedBall : Form
    {
        public FormSpeedBall(FormMain m)
        {
            formMain = m;
            Items = new List<SpeedItem>();
            InitializeComponent();

            leftCenterFormat = new StringFormat();
            rightCenterFormat = new StringFormat();
            leftCenterFormat.LineAlignment = StringAlignment.Center;
            rightCenterFormat.LineAlignment = StringAlignment.Center;
            rightCenterFormat.Alignment = StringAlignment.Far;
        }

        private FormMain formMain = null;

        private StringFormat leftCenterFormat;
        private StringFormat rightCenterFormat;

        public enum SpeedItemGridType
        {
            NoValue,
            NoGrid,
            OneGrid,
            TwoGrid,
        }
        public class SpeedItem
        {
            public SpeedItem()
            {
                bg = new SolidBrush(Color.White);
                bgf = new SolidBrush(Color.White);
                fo = new SolidBrush(Color.Blue);
                pfo = new Pen(Color.Blue);
                pfod = new Pen(Color.Blue);
                pfod.DashStyle = DashStyle.Dash;
                Height = 50;
                for (int i = 0; i < 60; i++)
                {
                    Data2.Add(0);
                    Data.Add(0);
                }
            }
            public SpeedItem(string text, Color cbg, Color cforground)
            {
                bg = new SolidBrush(cbg);
                bgf = new SolidBrush(Color.FromArgb(180, cbg));
                fo = new SolidBrush(cforground);
                pfo = new Pen(cforground);
                pfod = new Pen(cforground);
                pfod.DashStyle = DashStyle.Dash;
                Text = text;
                Height = 50;
                for (int i = 0; i < 60; i++)
                {
                    Data2.Add(0);
                    Data.Add(0);
                }
            }

            public List<int> Data = new List<int>();
            public List<int> Data2 = new List<int>();

            public void AddData1(int i)
            {
                Data.RemoveAt(0);
                Data.Add(i);
            }
            public void AddData2(int i)
            {
                Data2.RemoveAt(0);
                Data2.Add(i);
            }

            public SpeedItemGridType GridType { get; set; }

            public int Height { get; set; }
            public string Text { get; set; }
            public string Value { get; set; }
            public double NumValue { get; set; }

            public SolidBrush bg;
            public SolidBrush fo;
            public Pen pfo;
            public Pen pfod;
            public SolidBrush bgf;

            public Font TextFont { get; set; }
            public Font ValueFont { get; set; }

            public Color BgColor
            {
                get
                {
                    return bg.Color;
                }
                set
                {
                    bg.Color = value;
                    bgf.Color = value;
                }
            }
            public Color Color
            {
                get
                {
                    return fo.Color;
                }
                set
                {
                    fo.Color = value;
                    pfo.Color = value;
                    pfod.Color = value;
                }
            }
        }

        private PointF[] pts = new PointF[62];

        public List<SpeedItem> Items { get; private set; }

        private void FormSpeedBall_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
            int drawHeight = 1;
            SpeedItem thisitem = null;
            for (int i = 0; i < Items.Count; i++)
            {
                thisitem = Items[i];

                Rectangle rectangle = new Rectangle(1, drawHeight, Width - 2, thisitem.Height);
                Rectangle rectangleText = new Rectangle(8, drawHeight, Width - 10, thisitem.Height);

                g.FillRectangle(thisitem.bg, rectangle);

                if (thisitem.GridType == SpeedItemGridType.NoGrid)
                    g.FillRectangle(thisitem.fo, new Rectangle(1, drawHeight, (int)(Width * thisitem.NumValue) - 2, thisitem.Height));
                else if (thisitem.GridType == SpeedItemGridType.OneGrid)
                {
                    pts[0].X = 1;
                    pts[0].Y = rectangle.Bottom;//左下

                    for (int j = 0; j < thisitem.Data.Count; j++)
                    {
                        pts[j + 1].Y = rectangle.Bottom - thisitem.Data[j] / 100f * rectangle.Height;
                        pts[j + 1].X = rectangle.X + (j / 60f) * (Width - 2);

                        if (pts[j + 1].Y < rectangle.Y)
                            pts[j + 1].Y = rectangle.Y + 1;
                    }

                    pts[61].X = Width - 1;
                    pts[61].Y = rectangle.Bottom;//右下

                    if (thisitem.bgf != Brushes.White) g.FillClosedCurve(thisitem.bgf, pts, FillMode.Alternate, 0f);
                    for (int j = 1; j < 61; j++)
                        g.DrawLine(thisitem.pfo, pts[j - 1], pts[j]);
                }
                else if (thisitem.GridType == SpeedItemGridType.TwoGrid)
                {
                    pts[0].X = 1;
                    pts[0].Y = rectangle.Bottom;//左下

                    for (int j = 0; j < thisitem.Data2.Count; j++)
                    {
                        pts[j + 1].Y = rectangle.Bottom - thisitem.Data2[j] / 100f * rectangle.Height;
                        pts[j + 1].X = rectangle.X + (j / 60f) * (Width - 2);

                        if (pts[j + 1].Y < rectangle.Y)
                            pts[j + 1].Y = rectangle.Y + 1;
                    }

                    pts[61].X = Width - 1;
                    pts[61].Y = rectangle.Bottom;//右下

                    for (int j = 1; j < 61; j++)
                        g.DrawLine(thisitem.pfo, pts[j - 1], pts[j]);
                }

                if (thisitem.TextFont == null) thisitem.TextFont = Font;
                if (thisitem.ValueFont == null) thisitem.ValueFont = Font;

                g.DrawString(thisitem.Text, thisitem.TextFont, Brushes.Black, rectangleText, leftCenterFormat);
                g.DrawString(thisitem.Value, thisitem.ValueFont, Brushes.Black, rectangleText, rightCenterFormat);

                drawHeight += thisitem.Height;
                if (drawHeight > Height)
                    break;
            }
        }

        private void FormSpeedBall_Deactivate(object sender, System.EventArgs e)
        {
            Visible = false;
            formMain.notifyIcon_MouseLeave(sender, e);
        }

        private void FormSpeedBall_MouseLeave(object sender, System.EventArgs e)
        {
            Visible = false;
            formMain.notifyIcon_MouseLeave(sender, e);
        }
    }
}
