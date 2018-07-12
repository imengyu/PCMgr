using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TaskMgr.Ctls
{
    public class PerformanceList : Control
    {
        public const int max_small_data_count = 30;

        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MListDrawItem(IntPtr hWnd, IntPtr hdc, int x, int y, int w, int h, int state);

        public PerformanceList()
        {
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            items = new PerformanceListItemCollection();
            items.ItemAdd += Items_ItemAdd;
            items.ItemRemoved += Items_ItemRemoved;

            scrol = new VScrollBar();
            scrol.Name = "VScrollBarBase";
            scrol.LargeChange = 2;
            scrol.SmallChange = 1;
            scrol.Height = Height;
            scrol.Location = new Point(Width - 16, 0);
            scrol.Width = 16;
            scrol.ValueChanged += Scrol_ValueChanged; ;
            scrol.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            t = new Timer();
            t.Tick += T_Tick;
            t.Interval = 40;
        }

        private void Scrol_ValueChanged(object sender, EventArgs e)
        {
            yOffest = ((VScrollBar)sender).Value - ((VScrollBar)sender).Minimum;
            SyncItems(true);
        }
        private void T_Tick(object sender, EventArgs e)
        {
            if (m) m = false;
            t.Stop();
        }

        private void Items_ItemRemoved(PerformanceListItem obj)
        {
        }
        private void Items_ItemAdd(PerformanceListItem obj)
        { 
        }

        private bool m = false;
        private Timer t;
        private Font hugeTextFont = new Font("微软雅黑", 10.5f);
        private Font smallTextFont = new Font("微软雅黑", 9f);
        private PerformanceListItemCollection items = null;
        private int yOffest = 0;
        private int allItemHeight = 0;
        private int outtHeight = 0;
        private VScrollBar scrol = null;
        private PerformanceListItem mouseEnterItem = null;
        private PerformanceListItem selectedtem = null;

        public event EventHandler SelectedtndexChanged;

        public PerformanceListItem Selectedtem { get { return selectedtem; } set { selectedtem = value;Invalidate(); } }
        public PerformanceListItemCollection Items { get { return items; } }

        public void UpdateAll()
        {
            SyncItems(true);
        }
        public void SyncItems(bool paint)
        {
            allItemHeight = 0;
           for(int i=0;i<items.Count;i++)
            {
                items[i].ItemY = allItemHeight;
                allItemHeight += items[i].ItemHeight;
            }
            if (allItemHeight > Height)
            {
                outtHeight = allItemHeight - Height;

                if (yOffest > outtHeight && outtHeight >= 0)
                    yOffest = outtHeight + 16;

                scrol.Maximum = allItemHeight;
                scrol.LargeChange = Height < 0 ? 0 : Height;
                scrol.SmallChange = allItemHeight / 50;
                scrol.Left = Width - 16;
                scrol.Value = yOffest + 16;
                scrol.Height = Height;
                if (!scrol.Visible) scrol.Show();
            }
            else
            {
                outtHeight = 0;
                yOffest = 0;
                scrol.Hide();
            }
            if (paint) Invalidate();
        }

        private void DrawItem(Graphics g, PerformanceListItem it, int y)
        {
            if (it == mouseEnterItem)
            {
                MListDrawItem(Handle, g.GetHdc(), 2, mouseEnterItem.ItemY + 1 - yOffest, Width - 6, mouseEnterItem.ItemHeight - 2, 3);
                g.ReleaseHdc();
            }
            else if (it == selectedtem)
            {
                MListDrawItem(Handle, g.GetHdc(), 2, selectedtem.ItemY + 1 - yOffest, Width - 6, selectedtem.ItemHeight - 2, 1);
                g.ReleaseHdc();
            }

            g.DrawString(it.Name, hugeTextFont, Brushes.Black, 80, y + 10);
            g.DrawString(it.SmallText, smallTextFont, Brushes.Black, 80, y + 30);

            DrawItemDataGrid(g, it, y);
        }
        private void DrawItemDataGrid(Graphics g, PerformanceListItem it, int y)
        {
            Rectangle rect = new Rectangle(10, y + 10, 60, 40);
            if (it.Gray)
                g.DrawRectangle(Pens.Gray, rect);
            else
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                float single = 1.0F * rect.Width / (max_small_data_count - 1);
                float division = 1.0F * rect.Height / 100;
                float offset = 1.0F * (max_small_data_count - it.Data.Count) * single;

                List<PointF> ps = new List<PointF>();
                ps.Add(new PointF(rect.Left, rect.Top + rect.Height));
                ps.Add(new PointF(rect.Left + offset + 0 * single, rect.Top + rect.Height - it.Data[0] * division));

                if (it.BgBrush != Brushes.White)
                {
                    for (int i = 1; i < it.Data.Count; i++)
                    {
                        ps.Add(new PointF(rect.Left + offset + i * single, rect.Top + rect.Height - it.Data[i] * division));
                        g.DrawLine(it.BasePen, ps[ps.Count - 2], ps[ps.Count - 1]);
                    }
                    ps.Add(new PointF(rect.Left + rect.Width, rect.Top + rect.Height));
                    g.FillClosedCurve(it.BgBrush, ps.ToArray());
                    
                }
                else
                {
                    for (int i = 1; i < it.Data.Count; i++)
                        g.DrawLine(it.BasePen, ps[ps.Count - 2], ps[ps.Count - 1]);
                }

                ps.Clear();
                g.DrawRectangle(it.BorderPen, rect);
            }
        }
        private void InvalidAItem(PerformanceListItem it)
        {
            Invalidate(new Rectangle(0, it.ItemY - yOffest, Width, it.ItemHeight));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                if (mouseEnterItem != null)
                {
                    if (selectedtem != null) InvalidAItem(selectedtem);
                    selectedtem = mouseEnterItem;
                    InvalidAItem(selectedtem);
                    SelectedtndexChanged?.Invoke(this, null);
                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!m)
            {
                m = true;
                t.Start();
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        int y = items[i].ItemY - yOffest;
                        if (e.Y > y && e.Y < y + items[i].ItemHeight)
                        {
                            if (mouseEnterItem != null)
                                InvalidAItem(mouseEnterItem);
                            mouseEnterItem = items[i];
                            InvalidAItem(mouseEnterItem);
                            break;
                        }
                        else if (y + items[i].ItemHeight > e.Y) break;
                    }
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle refrect = e.ClipRectangle;
            int drawedHeight = 0;
            for (int i = 0; i < items.Count; i++)
            {
                PerformanceListItem it = items[i];
                if (drawedHeight >= refrect.Top)
                    DrawItem(g, it, it.ItemY - yOffest);
                drawedHeight += it.ItemHeight;
                if (drawedHeight > refrect.Bottom) break;
            }
        }
    }
    public class PerformanceListItem
    {
        public PerformanceListItem()
        {
            dataIem = new List<int>();
            for (int i = 0; i < 30; i++)
                Data.Add(0);
            Gray = false;
        }

        public Pen BorderPen { get; set; }
        public Brush BgBrush { get; set; }
        public Pen BasePen
        {
            get { return basePen; }
            set
            {
                basePen = value;
                if (BorderPen == null)
                    BorderPen = new Pen(basePen.Color, 1);
            }
        }
        public string Name { get; set; }
        public List<int> Data { get { return dataIem; } }


        public void AddData(int d)
        {
            if (b)
            {
                dataIem.RemoveAt(0);
                dataIem.Add(d);
            }
            else
            {
                int index = dataIem.Count - 1;
                dataIem[index] = (dataIem[index] + d) / 2;
            }
            b = !b;
        }

        public int PageIndex { get; set; }
        public bool Gray { get; set; }
        public int ItemY { get; set; }
        public int ItemHeight { get { return 60; } }
        public string SmallText { get; set; }

        private bool b = false;
        private Pen basePen = null;
        private List<int> dataIem = null;
    }
    public class PerformanceListItemCollection : System.Collections.CollectionBase
    {
        public PerformanceListItemCollection()
        {
        }

        public void Add(PerformanceListItem newcontrol)
        {
            List.Add(newcontrol);
            ItemAdd?.Invoke(newcontrol);
        }
        public void Remove(PerformanceListItem control)
        {
            List.Remove(control);
            ItemRemoved?.Invoke(control);
        }
        public void Insert(int index, PerformanceListItem control)
        {
            List.Insert(index, control);
        }
        public bool Contains(PerformanceListItem control)
        {
            return List.Contains(control);
        }
        public new void Clear()
        {
            List.Clear();
        }
        public PerformanceListItem this[int index]
        {
            get
            {
                return (PerformanceListItem)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        public PerformanceListItem this[string key]
        {
            get
            {
                PerformanceListItem result = null;
                foreach (PerformanceListItem ix in List)
                {
                    if (ix.Name == key)
                        return ix;
                }
                return result;
            }
        }

        public delegate void PerformanceListItemEventrHandler(PerformanceListItem obj);

        public event PerformanceListItemEventrHandler ItemAdd;
        public event PerformanceListItemEventrHandler ItemRemoved;
    }; 
}
