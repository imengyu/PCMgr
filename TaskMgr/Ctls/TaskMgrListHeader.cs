using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class TaskMgrListHeader : Control
    {
        public TaskMgrListHeader()
        {
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            items = new TaskMgrListHeaderItemCollection();
            items.HearderAdd += Items_HearderAdd;
            items.HearderRemoved += Items_HearderRemoved;
            items.HearderInserted += Items_HearderInserted;
            t.Tick += T_Tick;
            t.Interval = 40;
            lineBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.Transparent, Color.FromArgb(187, 187, 187));
            hotBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(254, 216, 200), Color.FromArgb(254, 192, 166));
            hotBrushHover = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(254, 242, 237), Color.FromArgb(254, 212, 194));
            tipToolTip = new ToolTip();

            CanSizeCloum = true;
        }
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
            items.HearderInserted += Items_HearderInserted;
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
                vs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                vs.Text = "列表水平滚动条";
                par.Controls.Add(vs);
            }
            lineBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.Transparent, Color.FromArgb(187, 187, 187));
            hotBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(254, 216, 200), Color.FromArgb(254, 192, 166));
            hotBrushHover = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(254, 242, 237), Color.FromArgb(254, 212, 194));
            tipToolTip = new ToolTip();


            LoadAllFonts();
            CanSizeCloum = true;
        }

        private void LoadAllFonts()
        {
            fb = new Font(Font.FontFamily, 12f);
            fs = new Font(Font.FontFamily, 8.5f);
        }

        private void Vs_ValueChanged(object sender, EventArgs e)
        {
            XOffest = ((HScrollBar)sender).Value - ((HScrollBar)sender).Minimum;
            if (!m)
            {
                m = true;
                t.Start();
                XOffestChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private void T_Tick(object sender, EventArgs e)
        {
            if (m) m = false; t.Stop();
        }

        private void Items_HearderRemoved(TaskMgrListHeaderItem obj)
        {
            vsed = false;
            obj.taskMgrListHeaderInternal = null;
            if (sortedItems.Contains(obj))
            {
                sortedItems.Remove(obj);
                RebulidColumnsIndexForDelete(obj.DisplayIndex);
            }
            Vsitem();
        }
        private void Items_HearderAdd(TaskMgrListHeaderItem obj)
        {
            if (arredItem == null)
                if (obj.ArrowType == TaskMgrListHeaderSortArrow.Ascending || obj.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    arredItem = obj;
            vsed = false;

            obj.taskMgrListHeaderInternal = this;

            if (obj.DisplayIndex == -1)
                obj.SetDisplayIndex(items.IndexOf(obj));

            sortedItems.Add(obj);
            Vsitem();
        }
        private void Items_HearderInserted(TaskMgrListHeaderItem obj, int index)
        {
            if (arredItem == null)
                if (obj.ArrowType == TaskMgrListHeaderSortArrow.Ascending || obj.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                    arredItem = obj;
            vsed = false;
            obj.taskMgrListHeaderInternal = this;
            RebulidColumnsIndexForInsert(index, obj);
            Vsitem();
        }

        private Pen blueLinePen = new Pen(Color.FromArgb(17, 125, 187), 2);
        private LinearGradientBrush hotBrushHover = null;
        private LinearGradientBrush hotBrush = null;
        private LinearGradientBrush lineBrush = null;
        private IntPtr hTheme = IntPtr.Zero;
        private HScrollBar vs = null;
        private int xOffiest = 0;
        private TaskMgrList parent = null;
        private Timer t = new Timer();
        private bool mouseDowned = false, m = false,drawBlueLineLeft=false, drawBlueLineRight = false;
        private TaskMgrListHeaderItemCollection items;
        private TaskMgrListHeaderItem lastTooltipItem = null;
        private TaskMgrListHeaderItem enteredItem = null;
        private TaskMgrListHeaderItem arredItem = null;
        private TaskMgrListHeaderItem resizeItem = null;
        private TaskMgrListHeaderItem moveItem = null;
        private ToolTip tipToolTip = null;
        private int firstBlockW = 0;
        private int mouseDownXPos = 0;
        private int mouseDownXPosInBlock = 0;
        private int movingDownIndex = 0;

        private Font fb = null;
        private Font fs = null;
        private int allWidth = 0;
        private bool vsed = false;
        private TaskMgrListHeaderItemCollection sortedItems = new TaskMgrListHeaderItemCollection();

        public TaskMgrListHeaderItemCollection SortedItems
        {
            get { return sortedItems; }
        }
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
        public int LastMoveCloumIndex { get { return movingDownIndex; } }
        public int LastMoveCloumNowIndex { get; private set; }

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

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Right)
            {
                if (enteredItem == null)
                {
                    if (sortedItems.Count > 0)
                    {
                        enteredItem = sortedItems[0];
                        if (!ScrollToItem(enteredItem))
                            Invalidate();
                        return false;
                    }
                }
                else
                {
                    TaskMgrListHeaderItem last_selectedItem = enteredItem;
                    int index = sortedItems.IndexOf(last_selectedItem);
                    if (index < sortedItems.Count - 1)
                        enteredItem = sortedItems[index + 1];
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
                    int index = sortedItems.IndexOf(last_selectedItem);
                    if (index >= 1)
                        enteredItem = sortedItems[index - 1];
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

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            LoadAllFonts();
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
            mouseDownXPos = e.X;
            mouseDowned = true;
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (moveItem != null)
            {
                if (moveItem.TargetIndex != 0 && moveItem.TargetIndex != sortedItems.IndexOf(moveItem))
                {
                    moveItem.DisplayIndex = moveItem.TargetIndex;

                    LastMoveCloumNowIndex = moveItem.TargetIndex;
                    moveItem.TargetIndex = 0;
                }

                moveItem.MovingDrawX = moveItem.X;
                moveItem.IsMoveing = false;

                drawBlueLineLeft = false;
                drawBlueLineRight = false;

                vsed = false;
                Invalidate();
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
                CloumClick?.Invoke(this, new TaskMgrListHeaderEventArgs(enteredItem, Items.IndexOf(enteredItem), e));
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
                    int mxl = mxr - mouseDownXPosInBlock;
                    if (mxr >= firstBlockW && mxr <= allWidth)
                    {
                        if (mxl >= firstBlockW && mxl <= allWidth - moveItem.Width)
                            moveItem.MovingDrawX = mxl;

                        //向左移动
                        if (e.X < mouseDownXPos)
                        {
                            drawBlueLineLeft = true;
                            drawBlueLineRight = false;
                            for (int i = movingDownIndex; i > 0; i--)
                            {
                                int xx = sortedItems[i].X - xOffiest;
                                int w = sortedItems[i].Width;
                                int fw = 0;
                                if (i > 1) fw = sortedItems[i - 1].Width;
                                int ii = xx + w;
                                sortedItems[i].DrawBlueLine = (x > xx - fw / 2 && x <= ii - w / 2);
                                if (sortedItems[i].DrawBlueLine)
                                    moveItem.TargetIndex = i;
                            }
                        }
                        //向右移动
                        else if (e.X > mouseDownXPos)
                        {
                            drawBlueLineLeft = false;
                            drawBlueLineRight = true;
                            for (int i = movingDownIndex; i < sortedItems.Count; i++)
                            {
                                int xx = sortedItems[i].X - xOffiest;
                                int w = sortedItems[i].Width;
                                int nw = 0;
                                if(i < sortedItems.Count-1) nw= sortedItems[i+1].Width;
                                int ii = xx + w;
                                sortedItems[i].DrawBlueLine = (x > ii - w / 2 && x <= ii + nw / 2);
                                if (sortedItems[i].DrawBlueLine)
                                    moveItem.TargetIndex = i;
                            }
                        }

                        Invalidate();
                    }
                }
                else
                {
                    for (int i = 0; i < sortedItems.Count; i++)
                    {
                        int xx = sortedItems[i].X - xOffiest;
                        int ii = xx + sortedItems[i].Width;
                        if (CanSizeCloum && x > ii - 3 && x < ii + 3 && resizeItem == null)
                        {
                            Cursor = Cursors.SizeWE;
                            if (enteredItem != null)
                                enteredItem.MouseEntered = false;
                            enteredItem = null;

                            if (e.Button == MouseButtons.Left)
                                resizeItem = sortedItems[i];
                            moveItem = null;
                            break;
                        }
                        else if (CanMoveCloum && x > xx + 3 && x < ii - 3 && moveItem == null && e.Button == MouseButtons.Left)
                        {
                            if (enteredItem != null)
                            {
                                enteredItem.MouseEntered = false;
                                enteredItem = null;
                            }
                            resizeItem = null;

                            moveItem = sortedItems[i];
                            moveItem.IsMoveing = true;

                            moveItem.MovingDrawX = moveItem.X;
                            if (items.Count > 0)
                                firstBlockW = sortedItems[0].Width;
                            movingDownIndex = sortedItems.IndexOf(moveItem);
                            mouseDownXPosInBlock = x - xx;

                            break;
                        }
                        else
                        {
                            moveItem = null;
                            resizeItem = null;
                            Cursor = Cursors.Default;
                            if (x > xx && x < (xx + sortedItems[i].Width))
                            {
                                if (enteredItem != null)
                                    enteredItem.MouseEntered = false;
                                sortedItems[i].MouseEntered = true;
                                enteredItem = sortedItems[i];
                                Invalidate();
                                ShowToolTip();
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
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (enteredItem != null)
            {
                enteredItem.MouseEntered = false;
                enteredItem = null;
                Invalidate();
            }
            tipToolTip.Hide(this);
            base.OnMouseLeave(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            ShowToolTip();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Vsitem())
                return;

            Graphics g = e.Graphics;
            using (Pen p = new Pen(lineBrush))
            {
                TaskMgrListHeaderItem thisItem = null;
                int h = Height - 1;
                bool arrdrawed = false;
                for (int i = 0; i < sortedItems.Count; i++)
                {
                    thisItem = sortedItems[i];

                    int x = (thisItem.IsMoveing ? (thisItem.MovingDrawX - XOffest) : (thisItem.X - XOffest));

                    if (thisItem == enteredItem)
                    {
                        if (thisItem.IsHot)
                            g.FillRectangle(hotBrushHover, new Rectangle((x + 1), 0, thisItem.Width - 1, h));
                        else
                        {
                            TaskMgrListApis.MHeaderDrawItem(hTheme, g.GetHdc(), (x + 1), 0, thisItem.Width - 1, h, TaskMgrListApis.M_DRAW_HEADER_HOT);
                            g.ReleaseHdc();
                        }
                    }
                    else if (thisItem.IsHot) g.FillRectangle(hotBrush, new Rectangle((x + 1), 0, thisItem.Width - 1, h));

                    string tb = thisItem.TextBig;
                    string ts = thisItem.TextSmall;
                    if (tb != "" || ts != "")
                    {
                        StringFormat f = thisItem.AlignmentStringFormat;
                        if (tb != "")
                            g.DrawString(tb, fb, Brushes.Black, new Rectangle(x + 3, 10, thisItem.Width - 6, 24), f);
                        if (ts != "")
                            g.DrawString(ts, fs, new SolidBrush(Color.FromArgb(76, 96, 122)), new Rectangle(x + 3, Height - 22, thisItem.Width - 6, 18), f);
                    }
                    if (!arrdrawed)
                    {
                        if (thisItem.ArrowType == TaskMgrListHeaderSortArrow.Ascending)
                        {
                            int posx = x + thisItem.Width / 2 - 4;
                            TaskMgrListApis.MHeaderDrawItem(hTheme, g.GetHdc(), posx, 0, 9, 6, TaskMgrListApis.M_DRAW_HEADER_SORTUP);
                            g.ReleaseHdc();
                            arrdrawed = true;
                        }
                        else if (thisItem.ArrowType == TaskMgrListHeaderSortArrow.Descending)
                        {
                            int posx = x + thisItem.Width / 2 - 4;
                            TaskMgrListApis.MHeaderDrawItem(hTheme, g.GetHdc(), posx, 0, 9, 6, TaskMgrListApis.M_DRAW_HEADER_SORTDOWN);
                            g.ReleaseHdc();
                            arrdrawed = true;
                        }
                    }
                    g.DrawLine(p, new Point(x + thisItem.Width, 0), new Point(x + thisItem.Width, h));
                    if (thisItem.DrawBlueLine)
                    {
                        if (drawBlueLineRight) g.DrawLine(blueLinePen, new Point(thisItem.X + thisItem.Width, 0), new Point(thisItem.X + thisItem.Width, h));
                        else if (drawBlueLineLeft) g.DrawLine(blueLinePen, new Point(thisItem.X, 0), new Point(thisItem.X, h));
                    }
                }
                g.DrawLine(new Pen(Color.FromArgb(160, 160, 160)), new Point(0, Height - 1), new Point(Width, Height - 1));
            }

        }


        internal void RebulidColumnsIndexForInsert(int insertIndex, TaskMgrListHeaderItem insertItem)
        {
            insertItem.SetDisplayIndex(insertIndex);
            sortedItems.Insert(insertIndex, insertItem);
            for (int i = insertIndex + 1; i < sortedItems.Count; i++)
                sortedItems[i].SetDisplayIndex(i);
            ColumnsIndexChanged();
        }
        private void RebulidColumnsIndexForDelete(int deleteDisplayIndex)
        {
            int i = deleteDisplayIndex;
            for (; i < items.Count; i++)
            {
                foreach (TaskMgrListHeaderItem h in Items)
                    if (h.DisplayIndex == i + 1)
                    {
                        h.SetDisplayIndex(i);
                        break;
                    }
            }
            ColumnsIndexChanged();
        }
        internal void RebulidColumnsIndex()
        {
            items[0].DisplayIndex = 0;
        }
        internal void ColumnsIndexChanged()
        {
            sortedItems.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                foreach (TaskMgrListHeaderItem h in Items)
                    if(h.DisplayIndex == i)
                    {
                        sortedItems.Add(h);
                        break;
                    }
            }
            vsed = false;
            Vsitem();
            CloumIndexChanged?.Invoke(this, null);
        }

        private void ShowToolTip()
        {
            if (enteredItem != null && lastTooltipItem == null)
            {
                if (!string.IsNullOrEmpty(enteredItem.ToolTip))
                {
                    tipToolTip.Show(enteredItem.ToolTip, this, enteredItem.X - xOffiest, Height + 5);
                    lastTooltipItem = enteredItem;
                }
            }
            else if (enteredItem == null && lastTooltipItem != null)
            {
                tipToolTip.Hide(this);
                lastTooltipItem = null;
            }
            else if (enteredItem != lastTooltipItem)
            {
                if (enteredItem != null && !string.IsNullOrEmpty(enteredItem.ToolTip))
                {
                    tipToolTip.Show(enteredItem.ToolTip, this, enteredItem.X - xOffiest, Height + 5);
                    lastTooltipItem = enteredItem;
                }
                else
                {
                    tipToolTip.Hide(this);
                    lastTooltipItem = null;
                }
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
                for (int i = 0; i < sortedItems.Count; i++)
                {
                    sortedItems[i].X = thisWidth;
                    thisWidth += sortedItems[i].Width;
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
                        int targetVal= vs.Value - vs.Minimum;
                        if (xOffiest != targetVal)
                        {
                            xOffiest = targetVal;
                            XOffestChanged?.Invoke(this, null);
                        }
                    }
                    else
                    {
                        parent.ishs = false;
                        if (vs.Visible)
                            vs.Hide();
                        if (xOffiest != 0)
                        {
                            xOffiest = 0;
                            XOffestChanged?.Invoke(this, null);
                        }
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
            HearderInserted?.Invoke(control, index);
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
        public delegate void TaskMgrListHeaderInsetEventrHandler(TaskMgrListHeaderItem obj, int index);

        public event TaskMgrListHeaderEventrHandler HearderAdd;
        public event TaskMgrListHeaderEventrHandler HearderRemoved;
        public event TaskMgrListHeaderInsetEventrHandler HearderInserted;
    }
    public class TaskMgrListHeaderItem
    {
        string tbig = "", tsml = "", name = "";
        int w = 100, x = 0, displayIndex = -1;
        TaskMgrListHeaderSortArrow a = TaskMgrListHeaderSortArrow.None;
        bool m = false;
        StringAlignment agr = StringAlignment.Near;
        StringFormat format = null;
        bool m1 = false;
        internal TaskMgrListHeader taskMgrListHeaderInternal = null;
        internal void SetDisplayIndex(int d) { displayIndex = d; }

        public int Index { get { if (taskMgrListHeaderInternal != null) return taskMgrListHeaderInternal.Items.IndexOf(this); else return -1; }  }
        public int TargetIndex { get; set; }
        public int DisplayIndex
        {
            get { return displayIndex; }
            set
            {
                if (taskMgrListHeaderInternal == null)
                    displayIndex = value;
                else
                {
                    int num = Math.Min(displayIndex, value);
                    int num2 = Math.Max(displayIndex, value);
                    int[] array = new int[taskMgrListHeaderInternal.Items.Count];
                    bool flag = value > displayIndex;
                    TaskMgrListHeaderItem columnHeader = null;
                    for (int i = 0; i < taskMgrListHeaderInternal.Items.Count; i++)
                    {
                        TaskMgrListHeaderItem columnHeader2 = taskMgrListHeaderInternal.Items[i];
                        if (columnHeader2.DisplayIndex == displayIndex)
                        {
                            columnHeader = columnHeader2;
                        }
                        else if (columnHeader2.DisplayIndex >= num && columnHeader2.DisplayIndex <= num2)
                        {
                            columnHeader2.displayIndex -= (flag ? 1 : -1);
                        }
                        if (i != Index)
                        {
                            array[columnHeader2.displayIndex] = i;
                        }
                    }
                    columnHeader.displayIndex = value;
                    array[columnHeader.displayIndex] = Index;
                    taskMgrListHeaderInternal.ColumnsIndexChanged();
                }
            }
        }
        public string ToolTip { get; set; }
        public string Identifier { get; set; }
        public bool IsNum { get; set; }
        public bool DrawBlueLine { get; set; }
        public bool IsMoveing { get; set; }
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
                    format.FormatFlags = StringFormatFlags.LineLimit;
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
        public int MovingDrawX { get; set; }
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
        public TaskMgrListHeader TaskMgrListHeader
        {
            get { return taskMgrListHeaderInternal; }
        }

        public override string ToString()
        {
            return tsml + " (" + displayIndex + ")";
        }
    }
}
