using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TaskMgr.Ctls
{
    public class TaskMgrList : Control
    {
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MListDrawItem(IntPtr hWnd, IntPtr hdc, int x, int y, int w, int h, int state);
        [DllImport(FormMain.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MAppWorkCall3(int id, IntPtr hWnd, IntPtr data);

        public TaskMgrList()
        {
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(16, 16);
            header = new TaskMgrListHeader(this);
            header.Dock = DockStyle.Top;
            header.Height = 58;
            Controls.Add(header);
            header.XOffestChanged += Header_XOffestChanged;
            groups = new TaskMgrListGroupCollection();
            items = new TaskMgrListItemCollection();
            items.ItemAdd += Items_ItemAdd;
            items.ItemRemoved += Items_ItemRemoved;
            showedItems = new TaskMgrListItemCollection();
            scrol = new VScrollBar();
            scrol.Name = "VScrollBarBase";
            scrol.LargeChange = 2;
            scrol.SmallChange = 1;
            scrol.Height = Height - header.Height - 16;
            scrol.Location = new Point(Width - 16, header.Height);
            scrol.Width = 16;
            scrol.ValueChanged += Scrol_ValueChanged;

            Controls.Add(scrol);
            scrol.Hide();
            t = new Timer();
            t.Tick += T_Tick;
            t.Interval = 50;
        }

        private bool b1 = false;
        private void Scrol_ValueChanged(object sender, EventArgs e)
        {
            if (!b1)
            {
                yOffest = ((VScrollBar)sender).Value - ((VScrollBar)sender).Minimum;
                SyncItems(true);
            }
        }
        private void Header_XOffestChanged(object sender, EventArgs e)
        {
            XOffest = header.XOffiest;
        }
        private void T_Tick(object sender, EventArgs e)
        {
            if (m)
                m = false;
            t.Stop();
        }
        private void Items_ItemRemoved(TaskMgrListItem obj)
        {
            SyncItems(true);
        }
        private void Items_ItemAdd(TaskMgrListItem obj)
        {
            if (sorted) Sort();
            else SyncItems(true);
        }

        private Timer t;
        private VScrollBar scrol = null;
        private TaskMgrListGroupCollection groups = null;
        private TaskMgrListItemCollection items = null;
        private TaskMgrListItemCollection showedItems = null;
        private TaskMgrListHeader header;
        private bool showGroup = false, m = false;
        private double value = 0;
        private int itemHeight = 28, groupHeaderHeight = 38, smallItemHeight = 22;
        private int xOffest = 0;
        private int yOffest = 0;
        private ImageList imageList;
        private int allItemHeight = 0;
        private Font fnormalText = new Font("微软雅黑", 13f);
        private Font fgroupText = new Font("微软雅黑", 13f);
        private Font fnormalText2 = new Font("微软雅黑", 9f);
        private TaskMgrListItem selectedItem = null;
        private TaskMgrListItem mouseenteredItem = null;
        private TaskMgrListItemChild selectedChildItem = null;
        private List<int> dwawLine = new List<int>();
        private int ougtHeight = 0;
        private bool focused = false;
        private bool locked = false;
        private ListViewColumnSorter sorter = null;
        private bool sorted = false;

        public bool isvs = false;
        public bool ishs = false;

        public TaskMgrListItemChild SelectedChildItem
        {
            get { return selectedChildItem; }
        }
        public ListViewColumnSorter ListViewItemSorter
        {
            get { return sorter; }
            set { sorter = value; }
        }
        public TaskMgrListItem SelectedItem
        {
            get { return selectedItem; }
        }
        public TaskMgrListHeader Header
        {
            get { return header; }
        }
        public bool FocusedType
        {
            get { return focused; }
            set { focused = value; Invalidate(); }
        }
        public TaskMgrListItemCollection ShowedItems
        {
            get { return showedItems; }
        }
        public TaskMgrListGroupCollection Groups
        {
            get { return groups; }
        }
        public TaskMgrListItemCollection Items
        {
            get { return items; }
        }
        public bool ShowGroup
        {
            get { return showGroup; }
            set { showGroup = value; }
        }
        public double Value
        {
            get { return value; }
            set
            {
                this.value = value;
                yOffest = (int)(allItemHeight * value);
                SyncItems(true);
            }
        }
        public ImageList Icons
        {
            get { return imageList; }
            set { imageList = value; }
        }
        public TaskMgrListHeaderItemCollection Colunms
        {
            get { return header.Items; }
        }
        public int XOffest
        {
            get { return xOffest; }
            set { xOffest = value; SyncItems(true); }
        }
        public int YOffest
        {
            get { return yOffest; }
        }
        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }
        public void Sort()
        {
            if (sorter != null)
            {
                if (sorter.Order == SortOrder.Ascending || sorter.Order == SortOrder.Descending)
                {
                    sorted = true;
                    items.Sort(sorter);
                    SyncItems(true);
                }
                else sorted = false;
            }
            else sorted = false;
        }
        public void SyncItems(bool paint)
        {
            b1 = true;
            allItemHeight = header.Height;
            int h = Height - header.Height;

            if (showGroup)
            {
                for (int i = 0; i < groups.Count; i++)
                    groups[i].Tag = 0;

                for (int i = 0; i < groups.Count; i++)
                {
                    int icount = 0;
                    for (int i1 = 0; i1 < items.Count; i1++)
                    {
                        if (items[i1].Group == groups[i])
                        {
                            if ((int)groups[i].Tag == 0)
                            {
                                groups[i].Y = allItemHeight;
                                allItemHeight += groupHeaderHeight;
                                groups[i].Tag = 1;
                            }

                            items[i1].YPos = allItemHeight;
                            if (items[i1].ChildsOpened)
                            {
                                for (int i2 = 0; i2 < items[i1].Childs.Count; i2++)
                                    allItemHeight += smallItemHeight;
                            }
                            allItemHeight += itemHeight;
                            icount++;
                        }
                    }
                    groups[i].ChildsCount = icount;
                }

            }
            else
            {
                for (int i1 = 0; i1 < items.Count; i1++)
                {
                    items[i1].YPos = allItemHeight;
                    if (items[i1].ChildsOpened)
                    {
                        for (int i2 = 0; i2 < items[i1].Childs.Count; i2++)
                            allItemHeight += smallItemHeight;
                    }
                    allItemHeight += itemHeight;
                }
            }

            if (allItemHeight > h)
            {
                isvs = true;
                ougtHeight = allItemHeight - h - header.Height;

                if (yOffest > ougtHeight && ougtHeight >= 0)
                    yOffest = ougtHeight + 16;

                scrol.Maximum = allItemHeight - header.Height + 16;
                scrol.LargeChange = h;
                scrol.SmallChange = allItemHeight / 20;
                scrol.Left = Width - 16;
                scrol.Value = yOffest + 16;

                if (ishs)
                    scrol.Height = h - 16;
                else scrol.Height = h;
                if (!scrol.Visible) scrol.Show();

                header.Vsitem(true);
            }
            else
            {
                isvs = false;
                ougtHeight = 0;
                value = 0;
                yOffest = 0;
                scrol.Hide();
            }
            if (paint) Invalidate();
            b1 = false;
        }
        private void DrawItem(Graphics g, TaskMgrListItem item)
        {
            showedItems.Add(item);

            /*for (int i2 = 0; i2 < item.SubItems.Count && i2 < header.Items.Count; i2++)
            {
                int x = header.Items[i2].X - xOffest;
                if (x < Width && x + header.Items[i2].Width > 0)
                {
                    Color bgcolor = item.SubItems[i2].BackColor;
                    if (bgcolor.A != 0 && !(bgcolor.R == 255 && bgcolor.G == 255 && bgcolor.B == 255))
                    {
                        if (!dwawLine.Contains(x))
                            dwawLine.Add(x);
                        g.FillRectangle(new SolidBrush(bgcolor), x, item.YPos - yOffest, header.Items[i2].Width, itemHeight);
                        if (!dwawLine.Contains(x + header.Items[i2].Width))
                            dwawLine.Add(x + header.Items[i2].Width);
                    }
                }
            }*/

            if (selectedItem == item)
            {
                if (FocusedType)
                    MListDrawItem(Handle, g.GetHdc(), 1 - xOffest, selectedItem.YPos + 1 - yOffest, header.AllWidth, itemHeight - 2, 1);
                else
                    MListDrawItem(Handle, g.GetHdc(), 1 - xOffest, selectedItem.YPos + 1 - yOffest, header.AllWidth, itemHeight - 2, 2);
                g.ReleaseHdc();
            }
            if (mouseenteredItem == item && item != selectedItem)
            {
                MListDrawItem(Handle, g.GetHdc(), 1 - xOffest, mouseenteredItem.YPos + 1 - yOffest, header.AllWidth, itemHeight - 2, 3);
                g.ReleaseHdc();
            }

            #region SubItems
            for (int i2 = 0; i2 < item.SubItems.Count && i2 < header.Items.Count; i2++)
            {
                int x = header.Items[i2].X - xOffest;
                if (x < Width && x + header.Items[i2].Width > 0)
                {
                    StringFormat f = new StringFormat();
                    f.LineAlignment = StringAlignment.Far;
                    f.Trimming = StringTrimming.EllipsisCharacter;
                    f.Alignment = header.Items[i2].Alignment;

                    if (item.SubItems[i2].Text != "")
                        if (i2 > 0)
                            g.DrawString(item.SubItems[i2].Text, item.SubItems[i2].Font, new SolidBrush(item.SubItems[i2].ForeColor), new Rectangle(x + 2, item.YPos - yOffest + itemHeight / 2 - item.SubItems[i2].Font.Height / 2, header.Items[i2].Width - 2, item.SubItems[i2].Font.Height), f);
                        else g.DrawString(item.SubItems[0].Text, fnormalText2, new SolidBrush(item.SubItems[0].ForeColor), new Rectangle(x + 63, item.YPos - yOffest + itemHeight / 2 - fnormalText2.Height / 2, header.Items[0].Width - 60, fnormalText2.Height), f);
                    f.Dispose();
                }
            }
            #endregion

            #region Childs
            if (item.Childs.Count > 0)
            {
                if (item.ChildsOpened)
                {
                    if (item.GlyphHoted && item == mouseenteredItem)
                        MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 8);
                    else
                        MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 6);
                    g.ReleaseHdc();
                    StringFormat f = new StringFormat();
                    f.LineAlignment = StringAlignment.Far;
                    f.Trimming = StringTrimming.EllipsisCharacter;
                    f.Alignment = StringAlignment.Near;
                    for (int i2 = 0; i2 < item.Childs.Count; i2++)
                    {
                        if (item.Childs[i2] == item.OldSelectedItem)
                        {
                            if (FocusedType)
                                MListDrawItem(Handle, g.GetHdc(), 37 - xOffest, item.YPos - yOffest + i2 * smallItemHeight + itemHeight, header.AllWidth - 22, smallItemHeight, 4);
                            else
                                MListDrawItem(Handle, g.GetHdc(), 37 - xOffest, item.YPos - yOffest + i2 * smallItemHeight + itemHeight, header.AllWidth - 22, smallItemHeight, 2);
                            g.ReleaseHdc();
                        }
                        g.DrawString(item.Childs[i2].Text, fnormalText2, Brushes.Black, new Rectangle(65 - xOffest, item.YPos - yOffest + smallItemHeight * i2 + itemHeight + itemHeight / 2 - fnormalText2.Height / 2, Width - 2, fnormalText2.Height), f);
                        if (item.Childs[i2].Icon != null)
                            g.DrawIcon(item.Childs[i2].Icon, new Rectangle(40 - xOffest, item.YPos - yOffest + i2 * smallItemHeight + itemHeight + 4, 16, 16));
                    }
                    f.Dispose();
                }
                else
                {
                    if (item.GlyphHoted && item == mouseenteredItem)
                        MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 7);
                    else
                        MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 5);
                    g.ReleaseHdc();
                }
            }
            #endregion

            #region Icons
            if (item.Icon != null)
                g.DrawIcon(item.Icon, new Rectangle(7 - xOffest + 25, item.YPos - YOffest + itemHeight / 2 - 8, 16, 16));
            #endregion
        }
        private void PaintItems(Graphics g, Rectangle r)
        {
            dwawLine.Clear();
            showedItems.Clear();
            if (showGroup)
            {
                int paintgroupHeadery = header.Height - yOffest;
                for (int i = 0; i < groups.Count; i++)
                {
                    if (groups[i].Tag == null || (int)groups[i].Tag == 1)
                    {
                        TaskMgrListGroup gurrs = groups[i];
                        Rectangle rgtext = new Rectangle(3 - xOffest, gurrs.Y - yOffest, 3 - xOffest + Width - 6, 34);
                        if (rgtext.IntersectsWith(r))
                        {
                            StringFormat f = new StringFormat();
                            f.LineAlignment = StringAlignment.Far;
                            f.Trimming = StringTrimming.EllipsisCharacter;
                            if (gurrs.HeaderAlignment == HorizontalAlignment.Center)
                                f.Alignment = StringAlignment.Center;
                            else if (gurrs.HeaderAlignment == HorizontalAlignment.Left)
                                f.Alignment = StringAlignment.Near;
                            else if (gurrs.HeaderAlignment == HorizontalAlignment.Right)
                                f.Alignment = StringAlignment.Far;

                            if (gurrs.Header != "")
                                g.DrawString(gurrs.Header + "(" + gurrs.ChildsCount + ")", fgroupText, new SolidBrush(Color.FromArgb(31, 89, 195)), rgtext, f);
                            f.Dispose();
                        }

                        TaskMgrListItem lastdrawitem = null;
                        for (int i1 = 0; i1 < items.Count; i1++)
                        {
                            if (items[i1].Group == gurrs)
                            {
                                if (items[i1].YPos - yOffest >= r.Top && items[i1].YPos - yOffest + itemHeight <= r.Bottom + header.Height)
                                {
                                    DrawItem(g, items[i1]);
                                    lastdrawitem = items[i1];
                                }
                            }
                        }
                        if (lastdrawitem != null) paintgroupHeadery = lastdrawitem.YPos - yOffest + itemHeight;
                    }
                }
            }
            else
            {
                for (int i1 = 0; i1 < items.Count; i1++)
                {
                    if (items[i1].YPos - yOffest >= r.Top && items[i1].YPos - yOffest + itemHeight <= r.Bottom + header.Height)
                        DrawItem(g, items[i1]);
                }
            }

            for (int i = 0; i < dwawLine.Count; i++)
                if (i != 0 && dwawLine[i] < Width)
                    g.DrawLine(new Pen(Color.FromArgb(234, 213, 160)), dwawLine[i], r.Top, dwawLine[i], r.Bottom);
        }

        private void InvalidAItem(TaskMgrListItem it)
        {
            int y = it.YPos - yOffest;
            int height = (it.ChildsOpened ? itemHeight + it.Childs.Count * smallItemHeight : itemHeight);
            Invalidate(new Rectangle(0, y, Width, height));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode) MAppWorkCall3(182, Handle, IntPtr.Zero);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (scrol.Visible)
            {
                if (e.Delta < 0)
                {
                    if (yOffest <= ougtHeight + 16 - scrol.SmallChange)
                        yOffest += scrol.SmallChange;
                    else
                        yOffest = ougtHeight + 16;
                    SyncItems(true);
                }
                else
                {
                    if (yOffest > scrol.SmallChange)
                        yOffest -= scrol.SmallChange;
                    else yOffest = 0;
                    SyncItems(true);
                }
            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Size != new Size(0, 0))
            {
                SyncItems(false);
                ((HScrollBar)Controls["HScrollBarBase"]).Top = Height - 16;
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (mouseenteredItem != null && selectedItem != null)
            {
                if (selectedItem != mouseenteredItem)
                {
                    SelectItemChanged?.Invoke(this, EventArgs.Empty);
                    TaskMgrListItem it = selectedItem;
                    selectedItem = mouseenteredItem;
                    InvalidAItem(it);
                }
                if (selectedItem.GlyphHoted)
                {
                    if (selectedItem.ChildsOpened)
                        selectedItem.ChildsOpened = false;
                    else selectedItem.ChildsOpened = true;

                    SyncItems(false);
                }
                if (selectedItem.ChildsOpened)
                    selectedChildItem = selectedItem.OldSelectedItem;
                else selectedChildItem = null;

                if (selectedItem != null)
                    InvalidAItem(selectedItem);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(!m)
            {
                m = true;
                t.Start();
                {
                    for (int i = 0; i < showedItems.Count; i++)
                    {
                        int y = showedItems[i].YPos - yOffest;
                        int yheight = (showedItems[i].ChildsOpened ? itemHeight + showedItems[i].Childs.Count * smallItemHeight : itemHeight);
                        if (e.Y > y && e.Y < y + yheight)
                        {
                            if (mouseenteredItem == showedItems[i])
                                return;
                            if (mouseenteredItem == null)
                                mouseenteredItem = showedItems[i];
                            else
                            {
                                TaskMgrListItem it = mouseenteredItem;
                                mouseenteredItem = showedItems[i];
                                InvalidAItem(it);
                            }

                            if (e.X >= 0 && e.X <= 22 - xOffest)
                                showedItems[i].GlyphHoted = true;
                            else if (showedItems[i].Childs.Count > 0 && showedItems[i].ChildsOpened)
                            {
                                showedItems[i].GlyphHoted = false;
                                int iii = ((e.Y - y - itemHeight)) / smallItemHeight;
                                if (iii >= 0 && iii < showedItems[i].Childs.Count)
                                    showedItems[i].OldSelectedItem = showedItems[i].Childs[iii];
                                if (e.Y - y - itemHeight < itemHeight)
                                    selectedChildItem = null;
                            }
                            else showedItems[i].GlyphHoted = false;

                            Invalidate(new Rectangle(0, y, Width, yheight));
                            //return;
                        }
                    }
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!locked)
            {
                base.OnPaint(e);
                PaintItems(e.Graphics, e.ClipRectangle);
                if (isvs && ishs) e.Graphics.FillRectangle(Brushes.White, new Rectangle(Width - 16, Height - 16, 16, 16));
            }
        }

        public event EventHandler SelectItemChanged;
    }

    public class TaskMgrListItemChild
    {
        public TaskMgrListItemChild(string text,Icon ico)
        {
            this.text = text;
            this.ico = ico;
        }

        private string text;
        private Icon ico;
        private bool selected = false;
        private object tag = null;

        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
        public string Text
        {
            set { text = value; }
            get { return text; }
        }
        public Icon Icon
        {
            get { return ico; }
            set { ico = value; }
        }
    }
    public class TaskMgrListItem : ListViewItem
    {
        public TaskMgrListItem()
        {
            childs = new List<TaskMgrListItemChild>();
        }
        public TaskMgrListItem(string a) : base(a)
        {
            childs = new List<TaskMgrListItemChild>();
        }

        private List<TaskMgrListItemChild> childs;
        private int yPos;
        private bool childsOpened = false;
        private bool glyphHoted = false;
        private Icon icon;
        private int pid;
        private TaskMgrListGroup group;

        public new TaskMgrListGroup Group
        {
            get { return group; }
            set { group = value; }
        }
        public int PID
        {
            get { return pid; }
            set { pid = value; }
        }
        public Icon Icon
        {
            get { return icon; }
            set { icon = value; }
        }
        public bool GlyphHoted
        {
            get { return glyphHoted; }
            set { glyphHoted = value; }
        }
        public TaskMgrListItemChild OldSelectedItem = null;
        public bool ChildsOpened
        {
            get { return childsOpened; }
            set { childsOpened = value; }
        }
        public int YPos
        {
            get { return yPos; }
            set { yPos = value; }
        }
        public List<TaskMgrListItemChild> Childs
        {
            get { return childs; }
        }
    }
    public class TaskMgrListGroup 
    {
        public TaskMgrListGroup(string text)
        {
            header = text;
        }
        private string header = "";
        private HorizontalAlignment headerAlignment = HorizontalAlignment.Left;
        private string name = "";
        private object tag = null;
        private int count = 0;
        private int y = 0;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        public int ChildsCount
        {
            get { return count; }
            set { count = value; }
        }
        public string Header { get { return header; } set { header = value; } }
        public HorizontalAlignment HeaderAlignment { get { return headerAlignment; } set { headerAlignment = value; } }             
        public string Name { get { return name; } set { name = value; } }      
        public object Tag { get { return tag; } set { tag = value; } }
    }
    public class TaskMgrListGroupCollection : System.Collections.CollectionBase
    {
        public TaskMgrListGroupCollection()
        {
        }
        public void Add(TaskMgrListGroup newcontrol)
        {
            List.Add(newcontrol);
            HearderAdd?.Invoke(newcontrol);
        }
        public void Remove(TaskMgrListGroup control)
        {
            List.Remove(control);
            HearderRemoved?.Invoke(control);
        }
        public void Insert(int index, TaskMgrListGroup control)
        {
            List.Insert(index, control);
        }
        public bool Contains(TaskMgrListGroup control)
        {
            return List.Contains(control);
        }
        public new void Clear()
        {
            List.Clear();
        }
        public TaskMgrListGroup this[int index]
        {
            get
            {
                return (TaskMgrListGroup)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        public TaskMgrListGroup this[string key]
        {
            get
            {
                TaskMgrListGroup result = null;
                foreach (TaskMgrListGroup ix in List)
                {
                    if (ix.Name == key)
                        return ix;
                }
                return result;
            }
        }

        public delegate void TaskMgrListEventrHandler(TaskMgrListGroup obj);

        public event TaskMgrListEventrHandler HearderAdd;
        public event TaskMgrListEventrHandler HearderRemoved;
    }
    public class TaskMgrListItemCollection : List<TaskMgrListItem>
    {
        public TaskMgrListItemCollection()
        {
        }
        public new void Add(TaskMgrListItem newcontrol)
        {
            base.Add(newcontrol);
            ItemAdd?.Invoke(newcontrol);
        }
        public new void Remove(TaskMgrListItem control)
        {
            base.Remove(control);
            ItemRemoved?.Invoke(control);
        }
        public new void Insert(int index, TaskMgrListItem control)
        {
            base.Insert(index, control);
        }
        public new bool Contains(TaskMgrListItem control)
        {
            return base.Contains(control);
        }
        public new void Clear()
        {
            base.Clear();
        }
        public TaskMgrListItem this[string key]
        {
            get
            {
                nextFoundKey = key;
                TaskMgrListItem result = null;
                Predicate<TaskMgrListItem> f = new Predicate<TaskMgrListItem>(FindItem);
                base.Find(f);
                nextFoundKey = "";
                return result;
            }
        }

        private bool FindItem(TaskMgrListItem p)
        {
            if (p.Text == nextFoundKey)
                return true;
            return false;
        }
        private string nextFoundKey = "";
        private ListViewComparer comparer = new ListViewComparer();

        public void Sort(ListViewColumnSorter order)
        {
            if (order.Order == SortOrder.Ascending)
            {
                comparer.OrderOfSort = SortOrder.Ascending;
                comparer.ColumnToSort = order.SortColumn;
                Sort(comparer);
            }
            else if (order.Order == SortOrder.Descending)
            {
                comparer.OrderOfSort = SortOrder.Descending;
                comparer.ColumnToSort = order.SortColumn;
                Sort(comparer);
            }
        }

        public delegate void TaskMgrListEventrHandler(TaskMgrListItem obj);

        public event TaskMgrListEventrHandler ItemAdd;
        public event TaskMgrListEventrHandler ItemRemoved;
    }

    public class ListViewComparer : IComparer<TaskMgrListItem>
    {
        public int ColumnToSort = 0;
        public SortOrder OrderOfSort = SortOrder.None;

        private static bool IsInt(string value)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[+-]?\d*$");
        }

        public int Compare(TaskMgrListItem x, TaskMgrListItem y)
        {
            int compareResult;

            if (IsInt(x.SubItems[ColumnToSort].Text) && IsInt(x.SubItems[ColumnToSort].Text))
                compareResult = int.Parse(x.SubItems[ColumnToSort].Text).CompareTo(int.Parse(y.SubItems[ColumnToSort].Text));
            else
                compareResult = string.Compare(x.SubItems[ColumnToSort].Text, y.SubItems[ColumnToSort].Text);
            if (OrderOfSort == SortOrder.Ascending)
                return compareResult;
            else if (OrderOfSort == SortOrder.Descending)
                return (-compareResult);
            else return 0;
        }
    }

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        private int comaretInt = 0;
        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            if (int.TryParse(listviewX.SubItems[ColumnToSort].Text, out comaretInt) &&
               int.TryParse(listviewY.SubItems[ColumnToSort].Text, out comaretInt))
            {
                compareResult = int.Parse(listviewX.SubItems[ColumnToSort].Text).CompareTo(int.Parse(listviewY.SubItems[ColumnToSort].Text));
            }
            else
            {
                // Compare the two items
                compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
            }
            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }

    }
}
