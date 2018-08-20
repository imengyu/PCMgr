using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class TaskMgrListHeader : Control
    {
        public TaskMgrListHeader(TaskMgrList par)
        {
            parent = par;
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            items = new TaskMgrListHeaderItemCollection();
            items.HearderAdd += Items_HearderAdd;
            items.HearderRemoved += Items_HearderRemoved;
            t.Tick += T_Tick;
            t.Interval = 40;
            if (par != null)
            {
                vs = new HScrollBar();
                vs.Name = "HScrollBarBase";
                vs.Location = new Point(0, par.Height - 16);
                if (par.Width == 0)
                    vs.Width = Width;
                else vs.Width = par.Width - 16;
                vs.Height = 16;
                vs.Visible = false;
                vs.ValueChanged += Vs_ValueChanged;
                vs.TabIndex = 3;
                vs.TabStop = true;
                par.Controls.Add(vs);
            }
            lineBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.Transparent, Color.FromArgb(187, 187, 187));
            hotBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(254, 216, 200), Color.FromArgb(254, 192, 166));
            hotBrushHover = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(254, 242, 237), Color.FromArgb(254, 212, 194));
            tipToolTip = new ToolTip();

            CanSizeCloum = true;
        }

        private void Vs_ValueChanged(object sender, EventArgs e)
        {
            if (!m)
            {
                m = true;
                t.Start();
                {
                    XOffest = ((HScrollBar)sender).Value - ((HScrollBar)sender).Minimum;
                    XOffestChanged?.Invoke(this, EventArgs.Empty);
                }
            }
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
                    arredItem = obj;
            vsed = false;
            Vsitem();
        }

        private LinearGradientBrush hotBrushHover = null;
        private LinearGradientBrush hotBrush = null;
        private LinearGradientBrush lineBrush = null;
        private IntPtr hTheme = IntPtr.Zero;
        private HScrollBar vs = null;
        private int xOffiest = 0;
        private TaskMgrList parent = null;
        private Timer t = new Timer();
        private bool mouseDowned = false, m = false;
        private TaskMgrListHeaderItemCollection items;
        private TaskMgrListHeaderItem lastTooltipItem = null;
        private TaskMgrListHeaderItem enteredItem = null;
        private TaskMgrListHeaderItem arredItem = null;
        private TaskMgrListHeaderItem resizeItem = null;
        private TaskMgrListHeaderItem moveItem = null;
        private ToolTip tipToolTip = null;
        private int mouseDownXInBlock = 0;
        private int firstBlockW = 0;
        private int mouseDownXPos = 0;

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
        public int XOffest
        {
            get { return xOffiest; }
            set { xOffiest = value;vsed = false; Vsitem(); Invalidate(); XOffestChanged?.Invoke(this, null); }
        }
        public bool CanMoveCloum
        {
            get;set;
        }
        public bool CanSizeCloum
        {
            get; set;
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

        public bool ScrollToItem(TaskMgrListHeaderItem item)
        {
            //item.X -Width + item.Width =  +xOffiest 
            int xrpos = item.X - xOffiest;
            if (xrpos < 0)
            {
                XOffest = item.X;
                return true;
            }
            else if (xrpos + item.Width > Width)
            {
                XOffest = item.X - Width + item.Width;
                return true;
            }
            return false;
        }
        
        private void ReSetEnterItemArrow()
        {
            foreach (TaskMgrListHeaderItem ii in items)
                if (ii != enteredItem) ii.ArrowType = TaskMgrListHeaderSortArrow.None;
            if (enteredItem.ArrowType == TaskMgrListHeaderSortArrow.None)
                enteredItem.ArrowType = TaskMgrListHeaderSortArrow.Ascending;
            else if (enteredItem.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                enteredItem.ArrowType = TaskMgrListHeaderSortArrow.Descending;
            else if (enteredItem.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                enteredItem.ArrowType = TaskMgrListHeaderSortArrow.Ascending;
            arredItem = enteredItem;
            Invalidate();
        }
        private void ExchangeTwoCloseItem(int index1, bool isNext)
        {
            if (isNext)
            {
                int index2 = index1 + 1;
                TaskMgrListHeaderItem li = items[index1];
                items[index1] = items[index2];
                items[index2] = li;

                items[index2].X = items[index1].X - items[index2].Width;
            }
            else
            {
                int index2 = index1 - 1;
                TaskMgrListHeaderItem li = items[index1];
                items[index1] = items[index2];
                items[index2] = li;

                items[index2].X = items[index1].X + items[index1].Width;
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Right)
            {
                if (enteredItem == null)
                {
                    if (items.Count > 0)
                    {
                        enteredItem = items[0];
                        if (!ScrollToItem(enteredItem))
                            Invalidate();
                        return false;
                    }

                }
                else
                {
                    TaskMgrListHeaderItem last_selectedItem = enteredItem;
                    int index = items.IndexOf(last_selectedItem);
                    if (index < items.Count - 1)
                        enteredItem = items[index + 1];
                    if (!ScrollToItem(enteredItem))
                        Invalidate();
                    return false;
                }
            }
            else if (keyData == Keys.Left)
            {
                if (enteredItem != null)
                {
                    TaskMgrListHeaderItem last_selectedItem = enteredItem;
                    int index = items.IndexOf(last_selectedItem);
                    if (index >= 1)
                        enteredItem = items[index - 1];
                    if (!ScrollToItem(enteredItem))
                        Invalidate();
                    return false;
                }
            }
            else if (keyData == Keys.Down)
            {
                if (Parent != null)
                    Parent.Focus();
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (DesignMode == false)
            {
                hTheme = TaskMgrListApis.MOpenThemeData(Handle, "HEADER");
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!DesignMode) TaskMgrListApis.MCloseThemeData(hTheme);
                hTheme = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }
        protected override void OnLostFocus(EventArgs e)
        {
            if (enteredItem != null)
            {
                enteredItem = null;
                Invalidate();
            }
            base.OnLostFocus(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            if (enteredItem == null)
            {
                if (items.Count > 0)
                {
                    enteredItem = items[0];
                    Invalidate();
                }
            }
            base.OnGotFocus(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (enteredItem != null)
                {
                    ReSetEnterItemArrow();
                    MouseEventArgs e1 = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
                    CloumClick?.Invoke(this, new TaskMgrListHeaderEventArgs(enteredItem, items.IndexOf(enteredItem), e1));
                }
            }
            base.OnKeyDown(e);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            vsed = false;
            Vsitem();
            base.OnSizeChanged(e);

        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (moveItem != null)
            {
                mouseDownXPos = e.X;
                if (items.Count > 0)
                    firstBlockW = items[0].Width;
                mouseDownXInBlock = e.X - (moveItem.X - XOffest);
            }
            mouseDowned = true;
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (moveItem != null)
            {
                CloumIndexChanged?.Invoke(this, null);
                moveItem = null;
            }
            else if (resizeItem != null)
            {
                resizeItem = null;
            }
            else if (enteredItem != null)
            {
                if (e.Button == MouseButtons.Left)
                    ReSetEnterItemArrow();
                CloumClick?.Invoke(this, new TaskMgrListHeaderEventArgs(enteredItem, items.IndexOf(enteredItem), e));
            }
            mouseDowned = false;
            base.OnMouseUp(e);
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
                    if (x - (resizeItem.X - XOffest) > 35)
                    {
                        resizeItem.Width = x - (resizeItem.X - XOffest);
                        vsed = false;
                        Vsitem();
                        Invalidate();
                        CloumWidthChanged?.Invoke(this, null);
                    }
                }
                else if (mouseDowned && moveItem != null)
                {
                    int mxr = e.X + XOffest;
                    int mx = mxr + XOffest;
                    if (mx > firstBlockW)
                    {
                        moveItem.X = mx;

                        //向左移动
                        if (e.X < mouseDownXPos)
                        {
                            int indexthis = items.IndexOf(moveItem);
                            if (items.IndexOf(moveItem) >= 2)
                            {
                                int indexlast = indexthis - 1;
                                //超过上一个块的位置
                                if (mxr < items[indexlast].X)
                                {
                                    //交换
                                    ExchangeTwoCloseItem(indexthis, false);
                                }
                            }
                        }
                        //向右移动
                        else if (e.X > mouseDownXPos)
                        {
                            int indexthis = items.IndexOf(moveItem);
                            if (indexthis <= items.Count - 2)
                            {
                                int indexlast = indexthis + 1;
                                //超过上一个块的位置
                                if (mxr < items[indexlast].X)
                                {
                                    //交换
                                    ExchangeTwoCloseItem(indexthis, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        int xx = items[i].X - xOffiest;
                        int ii = xx + items[i].Width;
                        if (CanSizeCloum && x > ii - 3 && x < ii + 3)
                        {
                            Cursor = Cursors.SizeWE;
                            if (enteredItem != null)
                                enteredItem.MouseEntered = false;
                            enteredItem = null;
                            resizeItem = items[i];
                            moveItem = null;
                            break;
                        }
                        else if (CanMoveCloum && x > xx + 3 && x < ii - 3)
                        {
                            if (enteredItem != null)
                            {
                                enteredItem.MouseEntered = false;
                                enteredItem = null;
                            }
                            resizeItem = null;
                            moveItem = items[i];
                            break;
                        }
                        else
                        {
                            moveItem = null;
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
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            if (enteredItem != null && lastTooltipItem == null)
            {
                if (!string.IsNullOrEmpty(enteredItem.ToolTip))
                {
                    tipToolTip.Show(enteredItem.ToolTip, this, MousePosition);
                    lastTooltipItem = enteredItem;
                }
            }
            else if (enteredItem == null)
            {
                if(lastTooltipItem != null)
                {
                    tipToolTip.Hide(this);
                    lastTooltipItem = null;
                }
            }
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
            using (Pen p = new Pen(lineBrush))
            {
                int h = Height - 1;
                bool arrdrawed = false;
                for (int i = 0; i < Items.Count; i++)
                {
                    int x = items[i].X - XOffest;

                    g.DrawLine(p, new Point(x, 0), new Point(x, h));

                    if (items[i] == enteredItem)
                    {
                        if (items[i].IsHot)
                            g.FillRectangle(hotBrushHover, new Rectangle((thisWidth + 1), 0, items[i].Width - 1, h));
                        else {
                            TaskMgrListApis.MHeaderDrawItem(hTheme, g.GetHdc(), (thisWidth + 1), 0, items[i].Width - 1, h, TaskMgrListApis.M_DRAW_HEADER_HOT);
                            g.ReleaseHdc();
                        }
                    }
                    else if (items[i].IsHot) g.FillRectangle(hotBrush, new Rectangle((thisWidth + 1), 0, items[i].Width - 1, h));

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
                            // g.DrawImage(PCMgr.Properties.Resources.listHeaderArrowAscending, posx, 0, 9, 5);
                            TaskMgrListApis.MHeaderDrawItem(hTheme, g.GetHdc(), posx, 0, 9, 6, TaskMgrListApis.M_DRAW_HEADER_SORTUP);
                            g.ReleaseHdc();
                            arrdrawed = true;
                        }
                        else if (items[i].ArrowType == TaskMgrListHeaderSortArrow.Descending)
                        {
                            int posx = thisWidth + Items[i].Width / 2 - 4;
                            TaskMgrListApis.MHeaderDrawItem(hTheme, g.GetHdc(), posx, 0, 9, 6, TaskMgrListApis.M_DRAW_HEADER_SORTDOWN);
                            g.ReleaseHdc();
                            arrdrawed = true;
                        }
                    thisWidth += items[i].Width;
                }
                g.DrawLine(new Pen(Color.FromArgb(160, 160, 160)), new Point(0, Height - 1), new Point(Width, Height - 1));
            }

        }

        public bool Vsitem(bool b = false)
        {
            if (moveItem != null) return false;
            if (b && parent.ishs)
            {
                HScrollBar vs = (HScrollBar)(parent.Controls["HScrollBarBase"]);
                if (parent.Width != 0)
                {
                    if (parent.isvs) vs.Width = parent.Width - 16;
                    else vs.Width = parent.Width;
                }
                else vs.Width = Width;
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
                if (thisWidth == 0) return false;
                if (parent != null)
                {
                    HScrollBar vs = (HScrollBar)(parent.Controls["HScrollBarBase"]);
                    if (thisWidth > Width)
                    {
                        parent.ishs = true;
                        if (!vs.Visible)
                            vs.Show();
                        if (vs.Width == 0) vs.Width = Width;
                        vs.Maximum = thisWidth;
                        vs.LargeChange = parent.Width;
                        vs.SmallChange = 5;
                    }
                    else
                    {
                        parent.ishs = false;
                        if (vs.Visible)
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

        public event EventHandler XOffestChanged;
        public event EventHandler CloumWidthChanged;
        public event EventHandler CloumIndexChanged;
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

        public string ToolTip { get; set; }
        public string Identifier { get; set; }
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
            set
            {
                a = value;
            }
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
