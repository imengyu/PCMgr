using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class PerformanceList : Control
    {
        public const int max_small_data_count = 30;

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
            Controls.Add(scrol);
            LoadAllFonts();

            t = new Timer();
            t.Tick += T_Tick;
            t.Interval = 40;
        }

        private void Scrol_ValueChanged(object sender, EventArgs e)
        {
            yOffest = ((VScrollBar)sender).Value - ((VScrollBar)sender).Minimum;
            Invalidate();
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

        private void LoadAllFonts()
        {
            hugeTextFont = new Font(Font.FontFamily, 10.5f);
            smallTextFont = new Font(Font.FontFamily, 9f);
        }

        private IntPtr hThemeListView = IntPtr.Zero;
        private bool m = false;
        private Timer t;
        private Font hugeTextFont = null;
        private Font smallTextFont = null;
        private PerformanceListItemCollection items = null;
        private int yOffest = 0;
        private int allItemHeight = 0;
        private int outHeight = 0;
        private VScrollBar scrol = null;
        private PerformanceListItem mouseEnterItem = null;
        private PerformanceListItem selectedItem = null;

        public event EventHandler SelectedtndexChanged;

        public PerformanceListItem Selectedtem { get { return selectedItem; } set { selectedItem = value;Invalidate(); } }
        public PerformanceListItemCollection Items { get { return items; } }

        private void OnSelectedtndexChanged(EventArgs e)
        {
            SelectedtndexChanged?.Invoke(this, e);
        }

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
                outHeight = allItemHeight - Height;

                if (yOffest > outHeight && outHeight >= 0)
                    yOffest = outHeight + 16;

                scrol.Maximum = allItemHeight;
                scrol.LargeChange = Height < 0 ? 0 : Height;
                scrol.SmallChange = allItemHeight / 50;
                scrol.Left = Width - 16;
                scrol.Value = yOffest + 16;
                scrol.Height = Height;
                if (!scrol.Visible)
                    scrol.Show();
            }
            else
            {
                outHeight = 0;
                yOffest = 0;
                if (scrol.Visible)
                    scrol.Hide();
            }
            if (paint) Invalidate();
        }

        private void DrawItem(Graphics g, PerformanceListItem it, int y)
        {
            if (it == mouseEnterItem)
            {
                TaskMgrListApis.MListDrawItem(hThemeListView, g.GetHdc(), 2, mouseEnterItem.ItemY + 1 - yOffest, Width - 6, mouseEnterItem.ItemHeight - 2, TaskMgrListApis.M_DRAW_LISTVIEW_HOT);
                g.ReleaseHdc();
            }
            else if (it == selectedItem)
            {
                TaskMgrListApis.MListDrawItem(hThemeListView, g.GetHdc(), 2, selectedItem.ItemY + 1 - yOffest, Width - 6, selectedItem.ItemHeight - 2, TaskMgrListApis.M_DRAW_LISTVIEW_SELECT);
                g.ReleaseHdc();
            }

            g.DrawString(it.Name, hugeTextFont, Brushes.Black, 80, y + 10);
            g.DrawString(it.SmallText, smallTextFont, Brushes.Black, 80, y + 30);

            DrawItemDataGrid(g, it, y);
        }
        private void DrawItemDataGrid(Graphics g, PerformanceListItem it, int y)
        {
            Rectangle rect = new Rectangle(10, y + 10, 60, 40);
            g.FillRectangle(Brushes.White, rect);
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
                        float ypos = rect.Top + rect.Height - it.Data[i] * division;
                        if (ypos < rect.Top) ypos = rect.Top;
                        ps.Add(new PointF(rect.Left + offset + i * single, ypos));
                        g.DrawLine(it.BasePen, ps[ps.Count - 2], ps[ps.Count - 1]);
                    }

                    ps.Add(new PointF(rect.Left + rect.Width, rect.Top + rect.Height));
                    g.FillClosedCurve(it.BgBrush, ps.ToArray(), System.Drawing.Drawing2D.FillMode.Alternate, 0f);              
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

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                if (selectedItem != null)
                {
                    PerformanceListItem last_selectedItem = selectedItem;
                    int index = items.IndexOf(last_selectedItem);

                    CON:
                    if (index >= 1)
                    {
                        if (!items[index - 1].Gray)
                            selectedItem = items[index - 1];
                        else
                        {
                            index--;
                            goto CON;
                        }

                        InvalidAItem(selectedItem);
                        InvalidAItem(last_selectedItem);
                        OnSelectedtndexChanged(EventArgs.Empty);
                        return false;
                    }
                }
            }
            else if (keyData == Keys.Down)
            {
                if (selectedItem == null)
                {
                    if (items.Count > 0)
                    {
                        Selectedtem = items[0];
                        OnSelectedtndexChanged(EventArgs.Empty);
                    }
                    return false; 
                }
                else
                {
                    PerformanceListItem last_selectedItem = selectedItem;
                    int index = items.IndexOf(last_selectedItem);

                    CON:
                    if (index < items.Count - 1)
                    {
                        if (!items[index + 1].Gray) 
                            selectedItem = items[index + 1];
                        else
                        {
                            index++;
                            goto CON;
                        }
                        
                        InvalidAItem(selectedItem);
                        InvalidAItem(last_selectedItem);
                        OnSelectedtndexChanged(EventArgs.Empty);
                        return false;
                    }
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            LoadAllFonts();
        }
        protected override void CreateHandle()
        {
            base.CreateHandle();
            if (!DesignMode)
            {
                TaskMgrListApis.MSetAsExplorerTheme(Handle);
                hThemeListView = TaskMgrListApis.MOpenThemeData(Handle, "LISTVIEW");
            }
        }
        protected override void DestroyHandle()
        {
            if (!DesignMode)
            {
                TaskMgrListApis.MCloseThemeData(hThemeListView);
                hThemeListView = IntPtr.Zero;
            }
            base.DestroyHandle();
        }
        protected override void OnGotFocus(EventArgs e)
        {
            if (selectedItem == null)
            {
                if (items.Count > 0)
                    Selectedtem = items[0];
            }
            base.OnGotFocus(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                if (mouseEnterItem != null)
                {
                    if (selectedItem != null) InvalidAItem(selectedItem);
                    selectedItem = mouseEnterItem;
                    InvalidAItem(selectedItem);
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
                        if (!items[i].Gray)
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
                        else mouseEnterItem = null;
                    }
                }
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (scrol.Visible)
            {
                if (e.Delta < 0)
                {
                    if (yOffest <= outHeight + 16 - scrol.SmallChange)
                    {
                        scrol.Value += scrol.SmallChange;
                    }
                    else
                    {
                        scrol.Value = outHeight + 16;
                    }
                }
                else
                {
                    if (yOffest > scrol.SmallChange)
                    {
                        scrol.Value -= scrol.SmallChange;
                    }
                    else
                    {
                        scrol.Value = 0;
                    }
                }
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (mouseEnterItem != null)
            {
                PerformanceListItem o = mouseEnterItem;
                mouseEnterItem = null;
                InvalidAItem(o);
            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SyncItems(true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle refrect = e.ClipRectangle;
            int drawedHeight = -yOffest;
            for (int i = 0; i < items.Count; i++)
            {
                PerformanceListItem it = items[i];
                if (drawedHeight >= refrect.Top - it.ItemHeight)
                    DrawItem(g, it, it.ItemY - yOffest);
                drawedHeight += it.ItemHeight;
                if (drawedHeight > refrect.Bottom) break;
            }
        }

        /// <summary>
        /// 获取一个值，用以指示 System.ComponentModel.Component 当前是否处于设计模式。
        /// </summary>
        protected new bool DesignMode
        {
            get
            {

#if DEBUG
                bool returnFlag = false;
                if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                {
                    returnFlag = true;
                }
                else if (System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpper().Equals("DEVENV"))
                {
                    returnFlag = true;
                }
                return returnFlag;
#else
                return base.DesignMode;
#endif
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
        public Brush BgBrush2 { get; set; }
        public Pen BasePen2
        {
            get { return basePen2; }
            set
            {
                basePen2 = value;
            }
        }
        public string Name { get; set; }
        public List<int> Data { get { return dataIem; } }

        public object Tag { get; set; }

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
        public void AddData2(int d)
        {
            if (b2)
            {
                dataIem2.RemoveAt(0);
                dataIem2.Add(d);
            }
            else
            {
                int index = dataIem2.Count - 1;
                dataIem2[index] = (dataIem2[index] + d) / 2;
            }
            b2 = !b2;
        }

        public int PageIndex { get; set; }
        public bool Gray { get; set; }
        public int ItemY { get; set; }
        public int ItemHeight { get { return 60; } }
        public string SmallText { get; set; }

        public override string ToString()
        {
            return Name;
        }
        private bool b2 = false;
        private bool b = false;
        private Pen basePen = null;
        private Pen basePen2 = null;
        private List<int> dataIem = null;
        private List<int> dataIem2 = null;
    }
    public class PerformanceListItemCollection : System.Collections.CollectionBase
    {
        public PerformanceListItemCollection()
        {
        }

        public int IndexOf(PerformanceListItem newcontrol)
        {
            return List.IndexOf(newcontrol);
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
