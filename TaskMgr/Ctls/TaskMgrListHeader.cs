using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace TaskMgr.Ctls
{
    public class TaskMgrListHeader : Control
    {
        public TaskMgrListHeader(TaskMgrList par)
        {
            parent = par;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            items = new TaskMgrListHeaderItemCollection();
            items.HearderAdd += Items_HearderAdd;
            items.HearderRemoved += Items_HearderRemoved;
            t.Tick += T_Tick;
            t.Interval = 100;
            if (par != null)
            {
                HScrollBar vs = new HScrollBar();
                vs.Name = "HScrollBarBase";
                vs.Location = new Point(0, par.Height - 16);
                vs.Width = par.Width - 16;
                vs.Height = 16;
                vs.Visible = false;
                vs.ValueChanged += Vs_ValueChanged;
                par.Controls.Add(vs);
            }
        }

        private void Vs_ValueChanged(object sender, EventArgs e)
        {
            XOffiest = ((HScrollBar)sender).Value - ((HScrollBar)sender).Minimum;
            XOffestChanged?.Invoke(this, EventArgs.Empty);
        }
        private void Items_HearderRemoved(TaskMgrListHeaderItem obj)
        {
            vsed = false;
            Vsitem();
        }
        private void T_Tick(object sender, EventArgs e)
        {
            if (m) m = false; t.Stop();
        }
        private void Items_HearderAdd(TaskMgrListHeaderItem obj)
        {
            if (arredItem == null)
                if (obj.ArrowType == TaskMgrListHeaderSortArrow.Ascending || obj.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                {

                    arredItem = obj;
                }
            vsed = false;
            Vsitem();
        }

        private int xOffiest = 0;
        private TaskMgrList parent = null;
        private Timer t = new Timer();
        private bool mouseDowned = false, m = false;
        private TaskMgrListHeaderItemCollection items;
        private TaskMgrListHeaderItem enteredItem = null;
        private TaskMgrListHeaderItem arredItem = null;
        private TaskMgrListHeaderItem resizeItem = null;
        private Font fb = new Font("微软雅黑", 12f);
        private Font fs = new Font("微软雅黑", 8.5f);
        private int allWidth = 0;
        private bool vsed = false;

        public int AllWidth
        {
            get { return allWidth; }
        }
        public TaskMgrListHeaderItemCollection Items
        {
            get { return items; }
        }
        public int XOffiest
        {
            get { return xOffiest; }
            set { xOffiest = value;vsed = false; Vsitem(); Invalidate(); }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            vsed = false;
            Vsitem();
            base.OnSizeChanged(e);

        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDowned = true;
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseDowned = false;
            base.OnMouseUp(e);
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (enteredItem != null)
            {
                if (e.Button == MouseButtons.Left && e.Clicks == 1)
                {
                    if (arredItem != null)
                        if (arredItem != enteredItem)
                            arredItem.ArrowType = TaskMgrListHeaderSortArrow.None;
                    if (enteredItem.ArrowType == TaskMgrListHeaderSortArrow.None)
                        enteredItem.ArrowType = TaskMgrListHeaderSortArrow.Ascending;
                    else if (enteredItem.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                        enteredItem.ArrowType = TaskMgrListHeaderSortArrow.Descending;
                    else if (enteredItem.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                        enteredItem.ArrowType = TaskMgrListHeaderSortArrow.Ascending;
                    arredItem = enteredItem;
                    Invalidate();
                }
                CloumClick?.Invoke(this, new TaskMgrListHeaderEventArgs(enteredItem, items.IndexOf(enteredItem), e));
            }
            base.OnMouseClick(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!m)
            {
                int x = e.X;
                m = true;
                t.Start();
                if (mouseDowned && resizeItem != null)
                {
                    if (x - resizeItem.X > 35)
                    {
                        resizeItem.Width = x - resizeItem.X;
                        vsed = false;
                        Vsitem();
                        Invalidate();
                        HearderWidthChanged?.Invoke(this, null);
                    }
                }
                else
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        int xx = items[i].X - xOffiest;
                        int ii = xx + items[i].Width;
                        if (x > ii - 3 && x < ii + 3)
                        {
                            Cursor = Cursors.SizeWE;
                            if (enteredItem != null)
                                enteredItem.MouseEntered = false;
                            enteredItem = null;
                            resizeItem = items[i];
                            break;
                        }
                        else
                        {
                            resizeItem = null;
                            Cursor = Cursors.Default;
                            if (x > xx && x < (xx + items[i].Width))
                            {
                                if (enteredItem != null)
                                    enteredItem.MouseEntered = false;
                                items[i].MouseEntered = true;
                                enteredItem = items[i];
                                Invalidate();
                                break;
                            }

                        }
                    }
                }
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (enteredItem != null)
            {
                enteredItem.MouseEntered = false;
                enteredItem = null;
                Invalidate();
            }
            base.OnMouseLeave(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Vsitem())
                return;

            Graphics g = e.Graphics;
            int thisWidth = -xOffiest;
            LinearGradientBrush b = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.Transparent, Color.FromArgb(187, 187, 187));
            Pen p = new Pen(b);
            int h = Height - 1;
            bool arrdrawed = false;
            for (int i = 0; i < Items.Count; i++)
            {
                int x = thisWidth + items[i].Width;

                g.DrawLine(p, new Point(x, 0), new Point(x, h));
                if (items[i].MouseEntered)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(229, 243, 255)), new Rectangle(thisWidth + 1, 0, items[i].Width - 1, h));
                if (items[i].IsHot)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(248, 106, 42)), new Rectangle(thisWidth + 1, 0, items[i].Width - 1, h));
                string tb = items[i].TextBig;
                string ts = items[i].TextSmall;
                if (tb != "" || ts != "")
                {
                    StringFormat f = items[i].AlignmentStringFormat;
                    if (tb != "")
                        g.DrawString(tb, fb, Brushes.Black, new Rectangle(thisWidth + 3, 10, items[i].Width - 6, 24), f);
                    if (ts != "")
                        g.DrawString(ts, fs, new SolidBrush(Color.FromArgb(76, 96, 122)), new Rectangle(thisWidth + 3, Height - 22, items[i].Width - 6, 18), f);
                }
                if (!arrdrawed)
                    if (items[i].ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                    {
                        int posx = thisWidth + Items[i].Width / 2 - 4;
                        g.DrawImage(Properties.Resources.listHeaderArrowAscending, posx, 0, 9, 5);
                        arrdrawed = true;
                    }
                    else if (items[i].ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    {
                        int posx = thisWidth + Items[i].Width / 2 - 4;
                        g.DrawImage(Properties.Resources.listHeaderArrowDisascending, posx, 0, 9, 5);
                        arrdrawed = true;
                    }
                thisWidth += items[i].Width;
            }
            g.DrawLine(new Pen(Color.FromArgb(160, 160, 160)), new Point(0, Height - 1), new Point(Width, Height - 1));
        }
        public bool Vsitem(bool b=false)
        {
            if (b && parent.ishs)
            {
                HScrollBar vs = (HScrollBar)(parent.Controls["HScrollBarBase"]);
                if (parent.isvs)
                    vs.Width = parent.Width - 16;
                else vs.Width = parent.Width;
            }
            if (!vsed)
            {
                int thisWidth = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    items[i].X = thisWidth;
                    thisWidth += items[i].Width;
                }
                vsed = true;
                if (parent != null)
                {
                    HScrollBar vs = (HScrollBar)(parent.Controls["HScrollBarBase"]);
                    if (thisWidth > Width)
                    {
                        parent.ishs = true;
                        vs.Show();
                        vs.Maximum = thisWidth;
                        vs.LargeChange = parent.Width;
                        vs.SmallChange = 5;
                    }
                    else
                    {
                        parent.ishs = false;
                        vs.Hide();
                    }
                    allWidth = thisWidth;
                    return true;
                }
                allWidth = thisWidth;
                return true;
            }
            return false;
        }

        public event EventHandler HearderWidthChanged;
        public event EventHandler XOffestChanged;
        public event TaskMgrListHeaderEventHandler CloumClick;

        public class TaskMgrListHeaderEventArgs : EventArgs
        {
            public TaskMgrListHeaderEventArgs(TaskMgrListHeaderItem i, int index, MouseEventArgs e)
            {
                item = i;
                this.index = index;
                eM = e;
            }

            private MouseEventArgs eM;
            private TaskMgrListHeaderItem item;
            private int index = 0;

            public MouseEventArgs MouseEventArgs
            {
                get { return eM; }
            }
            public int Index
            {
                get { return index; }
            }
            public TaskMgrListHeaderItem Item
            {
                get { return item; }
            }
        }

        public delegate void TaskMgrListHeaderEventHandler(object sender, TaskMgrListHeaderEventArgs e);
    }



    public enum TaskMgrListHeaderSortArrow
    {
        None,
        Ascending,
        Descending
    }
    public class TaskMgrListHeaderItemCollection : System.Collections.CollectionBase
    {
        public TaskMgrListHeaderItemCollection()
        {
        }
        public void Add(TaskMgrListHeaderItem newcontrol)
        {
            List.Add(newcontrol);
            HearderAdd?.Invoke(newcontrol);
        }
        public void Remove(TaskMgrListHeaderItem control)
        {
            List.Remove(control);
            HearderRemoved?.Invoke(control);
        }
        public void Insert(int index, TaskMgrListHeaderItem control)
        {
            List.Insert(index, control);
        }
        public bool Contains(TaskMgrListHeaderItem control)
        {
            return List.Contains(control);
        }
        public new void Clear()
        {
            List.Clear();
        }
        public int IndexOf(TaskMgrListHeaderItem control)
        {
            return List.IndexOf(control);
        }
        public TaskMgrListHeaderItem this[int index]
        {
            get
            {
                return (TaskMgrListHeaderItem)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        public TaskMgrListHeaderItem this[string key]
        {
            get
            {
                TaskMgrListHeaderItem result = null;
                foreach (TaskMgrListHeaderItem ix in List)
                {
                    if (ix.Name == key)
                        return ix;
                }
                return result;
            }
        }

        public delegate void TaskMgrListHeaderEventrHandler(TaskMgrListHeaderItem obj);

        public event TaskMgrListHeaderEventrHandler HearderAdd;
        public event TaskMgrListHeaderEventrHandler HearderRemoved;
    }
    public class TaskMgrListHeaderItem
    {
        string tbig = "", tsml = "", name = "";
        int w = 100, x = 0;
        TaskMgrListHeaderSortArrow a = TaskMgrListHeaderSortArrow.None;
        bool m = false;
        StringAlignment agr = StringAlignment.Near;
        StringFormat format = null;
        bool m1 = false;

        public bool IsNum { get; set; }
        public bool IsHot
        {
            get { return m1; }
            set { m1 = value; }
        }
        public StringFormat AlignmentStringFormat
        {
            get
            {
                if (format == null)
                {
                    format = new StringFormat();
                    format.LineAlignment = StringAlignment.Center;
                    format.FormatFlags |= StringFormatFlags.LineLimit;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    format.Alignment = agr;
                }
                return format;
            }
        }
        public StringAlignment Alignment
        {
            get { return agr; }
            set { agr = value; }
        }
        public string TextBig
        {
            get { return tbig; }
            set { tbig = value; }
        }
        public string TextSmall
        {
            get { return tsml; }
            set { tsml = value; }
        }
        public int Width
        {
            get { return w; }
            set { w = value; }
        }
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public TaskMgrListHeaderSortArrow ArrowType
        {
            get { return a; }
            set { a = value; }
        }
        public bool MouseEntered
        {
            get { return m; }
            set { m = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

    }
}
