using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class TaskMgrList : Control
    {
        internal const int itemHeight = 28, groupHeaderHeight = 38, smallItemHeight = 22;

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
            header.HearderWidthChanged += Header_HearderWidthChanged;
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
            ChildStringFormat = new StringFormat();
            ChildStringFormat.LineAlignment = StringAlignment.Center;
            ChildStringFormat.Trimming = StringTrimming.EllipsisCharacter;
            ChildStringFormat.Alignment = StringAlignment.Near;
            Controls.Add(scrol);
            scrol.Hide();
            t = new Timer();
            t.Tick += T_Tick;
            t.Interval = 40;
            defLineColorPen = new Pen(Color.FromArgb(234, 213, 160));
            hotLineColorPen = new Pen(Color.FromArgb(248, 106, 42));
            defBgSolidBrush = new SolidBrush(Color.FromArgb(255, 249, 228));
            defTagSolidBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
            errTagSolidBrush = new SolidBrush(Color.Orange);
            defChildColorPen = new Pen(Color.FromArgb(0, 120, 215), 3);
            DrawIcon = true;
        }

        private void Header_HearderWidthChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        public void ReposVscroll()
        {
            scrol.Height = Height - header.Height - 16;
            scrol.Location = new Point(Width - 16, header.Height);
        }

        private bool b1 = false;
        private void Scrol_ValueChanged(object sender, EventArgs e)
        {
            if (!b1)
            {
                if (!m)
                {
                    m = true;
                    t.Start();
                    {
                        yOffest = ((VScrollBar)sender).Value - ((VScrollBar)sender).Minimum;
                        SyncItems(true);
                    }
                }
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
        private SolidBrush errTagSolidBrush = null;
        private SolidBrush defTagSolidBrush = null;
        private SolidBrush defBgSolidBrush = null;
        private Pen defChildColorPen = null;
        private Pen defLineColorPen = null;
        private Pen hotLineColorPen = null;
        private VScrollBar scrol = null;
        private TaskMgrListGroupCollection groups = null;
        private TaskMgrListItemCollection items = null;
        private TaskMgrListItemCollection showedItems = null;
        private TaskMgrListHeader header;
        private bool showGroup = false, m = false;
        private double value = 0;
        private int xOffest = 0;
        private int yOffest = 0;
        private ImageList imageList;
        private int allItemHeight = 0;
        private Font fnormalText = new Font("微软雅黑", 13f);
        private Font fgroupText = new Font("微软雅黑", 13f);
        private Font fnormalText2 = new Font("微软雅黑", 9f);
        private TaskMgrListItem selectedItem = null;
        private TaskMgrListItem mouseenteredItem_ = null;
        private TaskMgrListItem mouseenteredItem
        {
            get { return mouseenteredItem_; }
            set
            {
                if (mouseenteredItem_ != value)
                {
                    TaskMgrListItem li = mouseenteredItem_;
                    mouseenteredItem_ = value;
                    InvalidAItem(li);
                    InvalidAItem(mouseenteredItem_);
                }
            }
        }
        private TaskMgrListItemChild selectedChildItem = null;
        private int ougtHeight = 0;
        private bool focused = false;
        private bool locked = false;
        private ListViewColumnSorter sorter = null;
        private bool sorted = false;
        private StringFormat ChildStringFormat = null;

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
            set { focused = value; if(Visible) Invalidate(); }
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
        public bool DrawIcon
        {
            get;set;
        }
        public void Sort(bool sync=true)
        {
            if (sorter != null)
            {
                if (sorter.Order == SortOrder.Ascending || sorter.Order == SortOrder.Descending)
                {
                    sorted = true;
                    items.Sort(sorter);
                    if(sync) SyncItems(true);
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
                            if ((items[i1].IsGroup || items[i1].IsAppHost) && items[i1].ChildsOpened)
                            {
                                allItemHeight += itemHeight;
                                if (items[i1].IsAppHost) allItemHeight += items[i1].ChildItemsHeight;
                               TaskMgrListItem t = items[i1];
                                for (int i2 = 0; i2 < t.Items.Count; i2++)
                                {
                                    t.Items[i2].YPos = allItemHeight;
                                    allItemHeight += t.Items[i2].ItemHeight;
                                }
                            }
                            else allItemHeight += items[i1].ItemHeight;
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
                    if ((items[i1].IsGroup || items[i1].IsAppHost) && items[i1].ChildsOpened)
                    {
                        allItemHeight += itemHeight;
                        if (items[i1].IsAppHost) allItemHeight += items[i1].ChildItemsHeight;
                        TaskMgrListItem t = items[i1];
                        for (int i2 = 0; i2 < t.Items.Count; i2++)
                        {
                            t.Items[i2].YPos = allItemHeight;
                            allItemHeight += t.Items[i2].ItemHeight;
                        }
                    }
                    else allItemHeight += items[i1].ItemHeight;
                }
            }

            if (allItemHeight > h)
            {
                isvs = true;
                ougtHeight = allItemHeight - h - header.Height;

                if (yOffest > ougtHeight && ougtHeight >= 0)
                    yOffest = ougtHeight + 16;

                scrol.Maximum = allItemHeight - header.Height + 16;
                scrol.LargeChange = h < 0 ? 0 : h;
                scrol.SmallChange = allItemHeight / 50;
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
        private void DrawItem(Graphics g, TaskMgrListItem item, int index, Rectangle rect)
        {
            showedItems.Add(item);

            if (selectedItem == item)
            {
                int height = item.ChildsOpened ? itemHeight + item.Childs.Count * smallItemHeight : itemHeight - 2;
                if (FocusedType) MListDrawItem(Handle, g.GetHdc(), 1 - xOffest, selectedItem.YPos + 1 - yOffest, header.AllWidth, height, 1);
                else MListDrawItem(Handle, g.GetHdc(), 1 - xOffest, selectedItem.YPos + 1 - yOffest, header.AllWidth, height, 2);
                g.ReleaseHdc();
            }
            else if (mouseenteredItem == item)
            {
                MListDrawItem(Handle, g.GetHdc(), 1 - xOffest, mouseenteredItem.YPos + 1 - yOffest, header.AllWidth, item.ChildsOpened ? itemHeight + item.Childs.Count * smallItemHeight : itemHeight - 2, 3);
                g.ReleaseHdc();
            }

            if (item.IsUWP)
            {
                if (item.IsUWPButErrInfo) g.FillRectangle(errTagSolidBrush, new Rectangle(53 - xOffest, item.YPos - yOffest + 11, 6, 6));
                else g.FillRectangle(defTagSolidBrush, new Rectangle(53 - xOffest, item.YPos - yOffest + 11, 6, 6));
            }
            if (item.IsUWPICO) g.FillRectangle(defTagSolidBrush, new Rectangle(7 - xOffest + 25, item.YPos - YOffest + itemHeight / 2 - 8, 16, 16));

            #region SubItems
            for (int i2 = 0; i2 < item.SubItems.Count && i2 < header.Items.Count; i2++)
            {
                int x = header.Items[i2].X - xOffest;
                if (x <= rect.Right && x + header.Items[i2].Width > rect.Left)
                {
                    Color bgcolor = item.SubItems[i2].BackColor;
                    StringFormat f = header.Items[i2].AlignmentStringFormat;
                    if (bgcolor.A != 0 && !(bgcolor.R == 255 && bgcolor.G == 255 && bgcolor.B == 255))
                    {
                        using (SolidBrush s = (selectedItem == item || mouseenteredItem == item) ?
                           new SolidBrush(Color.FromArgb(150, bgcolor)) : new SolidBrush(bgcolor))
                            g.FillRectangle(s, x + 1, item.YPos - yOffest, header.Items[i2].Width - 1, itemHeight);
                    }

                    if (i2 > 0 && item.SubItems[i2].Text != "") g.DrawString(item.SubItems[i2].Text, item.SubItems[i2].Font, item.SubItems[i2].ForeColorSolidBrush, new Rectangle(x + 6, item.YPos - yOffest, header.Items[i2].Width - 10, itemHeight), f);
                    else if (i2 == 0) g.DrawString(item.Text, fnormalText2, item.SubItems[0].ForeColorSolidBrush, new Rectangle(x + (DrawIcon ? 63 : 25), item.YPos - yOffest, header.Items[0].Width - (DrawIcon ? 60 : 25), itemHeight), f);
                }
                else if (x > rect.Right) break;
            }
            #endregion

            #region Childs
            if (item.Childs.Count > 0 || item.IsGroup || item.IsAppHost)
            {
                if (item.ChildsOpened)
                {
                    if (item.GlyphHoted && item == mouseenteredItem) MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 8);
                    else MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 6);
                    g.ReleaseHdc();
                    for (int i2 = 0; i2 < item.Childs.Count; i2++)
                    {
                        if (item.YPos - yOffest + i2 * smallItemHeight + itemHeight < rect.Bottom)
                        {
                            if (item.Childs[i2] == item.OldSelectedItem)
                            {
                                if (FocusedType) MListDrawItem(Handle, g.GetHdc(), 37 - xOffest, item.YPos - yOffest + i2 * smallItemHeight + itemHeight, header.AllWidth - 37, smallItemHeight, 4);
                                else MListDrawItem(Handle, g.GetHdc(), 37 - xOffest, item.YPos - yOffest + i2 * smallItemHeight + itemHeight, header.AllWidth - 37, smallItemHeight, 2);
                                g.ReleaseHdc();
                            }
                            g.DrawString(item.Childs[i2].Text, fnormalText2, Brushes.Black, new Rectangle(65 - xOffest, item.YPos - yOffest + smallItemHeight * i2 + itemHeight, Width - 2, itemHeight), ChildStringFormat);
                            if (item.Childs[i2].Icon != null) g.DrawIcon(item.Childs[i2].Icon, new Rectangle(40 - xOffest, item.YPos - yOffest + i2 * smallItemHeight + itemHeight + 4, 16, 16));
                        }
                        else break;
                    }
                }
                else
                {
                    if (item.GlyphHoted && item == mouseenteredItem) MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 7);
                    else MListDrawItem(Handle, g.GetHdc(), 4 - xOffest, item.YPos - yOffest + 5, 16, 16, 5);
                    g.ReleaseHdc();
                }
            }
            #endregion

            #region Icons
            if (item.Icon != null) g.DrawIcon(item.Icon, new Rectangle(7 - xOffest + 25, item.YPos - YOffest + itemHeight / 2 - 8, 16, 16));
            else if (item.Image != null) g.DrawImage(item.Image, new Rectangle(7 - xOffest + 25, item.YPos - YOffest + itemHeight / 2 - 8, 16, 16));
            #endregion
        }
        private void DrawItemGroup(Graphics g, TaskMgrListItem item, int index, Rectangle rect)
        {
            showedItems.Add(item);
            DrawItem(g, item, index, rect);
            if (item.ChildsOpened)
            {
                for (int i = 0; i < item.Items.Count; i++)
                    DrawItem(g, item.Items[i], index, rect);
                if (item.IsAppHost)
                {
                    int y = item.YPos - yOffest + itemHeight;
                    g.DrawLine(defChildColorPen, 12 - xOffest, y, 12 - xOffest, y + item.ItemHeight);
                }
            }
        }
        private void PaintItems(Graphics g, Rectangle r)
        {
            showedItems.Clear();

            //draw lines
            bool isFirstLine = true;
            for (int i = 0; i < header.Items.Count; i++)
            {
                int x = header.Items[i].X - xOffest;
                int xw = x + header.Items[i].Width;
                if (x < r.Right && xw > 0)
                {
                    if (header.Items[i].IsNum)
                    {
                        if(isFirstLine)
                        {
                            if (header.Items[i].IsHot) g.DrawLine(hotLineColorPen, x, r.Top, x, r.Bottom);
                            else g.DrawLine(defLineColorPen, x, r.Top, x, r.Bottom);
                            isFirstLine = false;
                        }
                        if (header.Items[i].IsHot)
                        {
                            g.DrawLine(hotLineColorPen, x, r.Top, x, r.Bottom);
                            g.DrawLine(hotLineColorPen, xw, r.Top, xw, r.Bottom);
                        }
                        else g.DrawLine(defLineColorPen, xw, r.Top, xw, r.Bottom);
                        g.FillRectangle(defBgSolidBrush, x + 2, r.Top, header.Items[i].Width - 2, r.Height);
                    }
                }
                else if (x >= r.Right) break;
            }

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
                                if (items[i1].YPos  - yOffest >= r.Top || items[i1].YPos - yOffest <= r.Bottom)
                                {
                                    if (items[i1].IsGroup || items[i1].IsAppHost) DrawItemGroup(g, items[i1], i1, r);
                                    else DrawItem(g, items[i1], i1, r);
                                    lastdrawitem = items[i1];
                                }
                                else if (items[i1].YPos - yOffest > r.Bottom)
                                    break;
                            }
                        }
                        if (lastdrawitem != null) paintgroupHeadery = lastdrawitem.YPos - yOffest + lastdrawitem.ItemHeight;
                    }
                }
            }
            else
            {
                for (int i1 = 0; i1 < items.Count; i1++)
                {
                    if (items[i1].YPos - yOffest >= r.Top || items[i1].YPos - yOffest <= r.Bottom)
                    {
                        if (items[i1].IsGroup || items[i1].IsAppHost)
                            DrawItemGroup(g, items[i1], i1, r);
                        else
                            DrawItem(g, items[i1], i1, r);
                    }
                    else if (items[i1].YPos  - yOffest > r.Bottom)
                        break;
                }
            }
        }

        private void InvalidAItem(TaskMgrListItem it)
        {
            if (it != null)
            {
                int y = it.YPos - yOffest;
                int height = (it.ChildsOpened ? itemHeight + it.Childs.Count * smallItemHeight : itemHeight);
                Invalidate(new Rectangle(0, y, Width, height));
            }
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
            if (mouseenteredItem != null)
            {
                bool fullinved = false;
                if (selectedItem != mouseenteredItem)
                {
                    SelectItemChanged?.Invoke(this, EventArgs.Empty);
                    TaskMgrListItem it = selectedItem;
                    selectedItem = mouseenteredItem;
                    if (it != null) InvalidAItem(it);
                }
                if (selectedItem.GlyphHoted)
                {
                    if (selectedItem.ChildsOpened)
                    {
                        selectedItem.ChildsOpened = false;
                        fullinved = true;
                    }
                    else
                    {
                        fullinved = true;
                        selectedItem.ChildsOpened = true;
                    }
                    SyncItems(false);
                }
                if (selectedItem.ChildsOpened)
                    selectedChildItem = selectedItem.OldSelectedItem;
                else selectedChildItem = null;

                if (fullinved == false && selectedItem != null)
                    InvalidAItem(selectedItem);
                else if (fullinved) Invalidate();
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!m)
            {
                bool oldgy = false;
                m = true;
                t.Start();
                {
                    if (e.X > 0 && e.X < XOffest + header.AllWidth)
                    {
                        for (int i = showedItems.Count - 1; i >= 0; i--)
                        {
                            int y = showedItems[i].YPos - yOffest;
                            int yheight = (showedItems[i].ChildsOpened ? itemHeight + showedItems[i].Childs.Count * smallItemHeight : itemHeight);
                            if (e.Y > y && e.Y < y + yheight)
                            {
                                oldgy = showedItems[i].GlyphHoted;
                                if (e.Y < y + 30 && e.X >= 0 && e.X <= 22 - xOffest) showedItems[i].GlyphHoted = true;
                                else if (showedItems[i].Childs.Count > 0 && showedItems[i].ChildsOpened)
                                {
                                    showedItems[i].GlyphHoted = false;
                                    if (e.Y - y > itemHeight)
                                    {
                                        int iii = ((e.Y - y - itemHeight)) / smallItemHeight;
                                        if (iii >= 0 && iii < showedItems[i].Childs.Count)
                                            showedItems[i].OldSelectedItem = showedItems[i].Childs[iii];
                                        else showedItems[i].OldSelectedItem = null;
                                    }
                                    else
                                    {
                                        showedItems[i].OldSelectedItem = null;
                                        selectedChildItem = null;
                                    }

                                    InvalidAItem(showedItems[i]);
                                }
                                else showedItems[i].GlyphHoted = false;
                                if (oldgy != showedItems[i].GlyphHoted)
                                    Invalidate(new Rectangle(0, y, 22, 22));
 
                                mouseenteredItem = showedItems[i];
                                return;
                            }
                            else
                            {
                                if (showedItems[i].OldSelectedItem != null)
                                {
                                    showedItems[i].OldSelectedItem = null;
                                    InvalidAItem(showedItems[i]);
                                }
                            }
                        }
                        if (mouseenteredItem != null)
                        {
                            mouseenteredItem = null;
                            InvalidAItem(mouseenteredItem);
                        }
                    }
                    else if(mouseenteredItem!=null)
                    {
                        mouseenteredItem = null;
                        InvalidAItem(mouseenteredItem);
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

    public enum TaskMgrListItemTextDirection
    {
        Left,
        Center,
        Right,
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
            subItem = new List<TaskMgrListViewSubItem>();
            items = new TaskMgrListItemCollection();
            items.ItemAdd += Items_ItemAdd;
            items.ItemRemoved += Items_ItemRemoved;
        }
        public TaskMgrListItem(string a) : base(a)
        {
            childs = new List<TaskMgrListItemChild>();
            subItem = new List<TaskMgrListViewSubItem>();
            items = new TaskMgrListItemCollection();
            items.ItemAdd += Items_ItemAdd;
            items.ItemRemoved += Items_ItemRemoved;
        }

        private List<TaskMgrListItemChild> childs;
        private int yPos;
        private bool childsOpened = false;
        private bool glyphHoted = false;
        private bool isGroup = false;
        private Icon icon;
        private uint pid;
        private TaskMgrListGroup group;
        private List<TaskMgrListViewSubItem> subItem = null;

        public class TaskMgrListViewSubItem : ListViewSubItem
        {
            private SolidBrush _ForeColorSolidBrush = null;

            public double CustomData { get; set; }
            public new Color ForeColor
            {
                get { return base.ForeColor; }
                set
                {
                    base.ForeColor = value;
                    if (_ForeColorSolidBrush != null)
                        _ForeColorSolidBrush.Dispose();
                    _ForeColorSolidBrush = new SolidBrush(value);
                }
            }
            public SolidBrush ForeColorSolidBrush
            {
                get
                {
                    if (_ForeColorSolidBrush == null)
                        _ForeColorSolidBrush = new SolidBrush(ForeColor);
                    return _ForeColorSolidBrush;
                }
            }
        }

        public virtual int ChildItemsHeight
        {
            get
            {
                return childs.Count * TaskMgrList.smallItemHeight;
            }
        }
        public virtual int ItemHeight
        {
            get
            {
                if (IsGroup) return (TaskMgrList.itemHeight * ((ChildsOpened ? Items.Count : 0) + 1));
                else if (IsAppHost) return (TaskMgrList.itemHeight + TaskMgrList.itemHeight * (ChildsOpened ? Items.Count : 0) + (childsOpened ? (childs.Count * TaskMgrList.smallItemHeight) : 0));
                else return TaskMgrList.itemHeight + (childsOpened ? (childs.Count * TaskMgrList.smallItemHeight) : 0);
            }
        }
        public new List<TaskMgrListViewSubItem> SubItems { get { return subItem; } }
        public Rectangle GlyphRect { get; set; }
        public new TaskMgrListGroup Group
        {
            get { return group; }
            set
            {
                group = value;
            }
        }
        public uint PID
        {
            get { return pid; }
            set { pid = value; }
        }
        public Icon Icon
        {
            get { return icon; }
            set { icon = value; }
        }
        public Image Image { get; set; }
        public bool IsGroup
        {
            get { return isGroup; }
            set
            {
                isGroup = value;
            }
        }
        public bool IsAppHost { get; set; }
        public bool IsUWPICO { get; set; }
        public TaskMgrListItem Parent { get; set; }
        public bool IsUWP { get; set; }
        public bool IsUWPButErrInfo { get; set; }
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
        public bool HasChild(IntPtr tag)
        {
            bool rs = false;
            foreach (TaskMgrListItemChild c in childs)
                if ((IntPtr)c.Tag== tag) return true;
            return rs;
        }

        private void Items_ItemRemoved(TaskMgrListItem obj)
        {
            obj.Parent = null;
        }
        private void Items_ItemAdd(TaskMgrListItem obj)
        {
            obj.Parent = this;
        }

        private TaskMgrListItemCollection items = null;

        public TaskMgrListItemCollection Items
        {
            get { return items; }
        }
    }
    public class TaskMgrListItemGroup : TaskMgrListItem
    {
        public TaskMgrListItemGroup(string a) : base(a)
        {
            IsGroup = true;
        }
        public TaskMgrListItemGroup()
        {
            IsGroup = true;
        }

        public override int ItemHeight { get { return (TaskMgrList.itemHeight * ((ChildsOpened ? Items.Count : 0) + 1)); } }
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

        public void Sort(ListViewColumnSorter order)
        {
            if (order.Order == SortOrder.Ascending)
            {
                order.OrderOfSort = SortOrder.Ascending;
                order.ColumnToSort = order.SortColumn;
                base.Sort(order);
            }
            else if (order.Order == SortOrder.Descending)
            {
                order.OrderOfSort = SortOrder.Descending;
                order.ColumnToSort = order.SortColumn;
                base.Sort(order);
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
            if (string.IsNullOrEmpty(value)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[+-]?\d*$");
        }

        public virtual int Compare(TaskMgrListItem x, TaskMgrListItem y)
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
    public class ListViewColumnSorter : ListViewComparer
    {
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        protected CaseInsensitiveComparer ObjectCompare;

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

        public override int Compare(TaskMgrListItem x, TaskMgrListItem y)
        {
            int compareResult = 0;
            if (int.TryParse(x.SubItems[ColumnToSort].Text, out comaretInt) &&
                 int.TryParse(x.SubItems[ColumnToSort].Text, out comaretInt))
                compareResult = int.Parse(x.SubItems[ColumnToSort].Text).CompareTo(int.Parse(y.SubItems[ColumnToSort].Text));
            else
            {
                compareResult = ObjectCompare.Compare(x.SubItems[ColumnToSort].Text, y.SubItems[ColumnToSort].Text);
                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                    return compareResult;
                else if (OrderOfSort == SortOrder.Descending)
                    return (-compareResult);
            }
            return compareResult;
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
