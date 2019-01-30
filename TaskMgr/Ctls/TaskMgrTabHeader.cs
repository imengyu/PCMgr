using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PCMgr.Ctls
{
    public class TaskMgrTabHeader : Control
    {
        public TaskMgrTabHeader(TabControl t)
        {
            targetTab = t;
            targetTab.SelectedIndexChanged += TargetTab_SelectedIndexChanged;
            targetTab.VisibleChanged += TargetTab_VisibleChanged;

            BackColor = Color.White;
            Cursor = Cursors.Arrow;

            stringFormatCenter = new StringFormat();
            stringFormatCenter.Alignment = StringAlignment.Center;
            stringFormatCenter.LineAlignment = StringAlignment.Center;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            CopyTabs();
        }

        private void TargetTab_VisibleChanged(object sender, EventArgs e)
        {
            Visible = targetTab.Visible;
        }

        private void TargetTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private StringFormat stringFormatCenter = null;
        private TabControl targetTab = null;
        private class TabItem
        {
            public TabItem(TabPage t) {
                Text = t.Text;
                TabPage = t;
            }
            public int Width { get; set; } = -1;
            public int X { get; set; } = -1;
            public TabPage TabPage { get; set; }
            public string Text { get; set; }
            public bool Hover { get; set; }
        }
        private List<TabItem> allTabs = new List<TabItem>();
        private TabItem enteredTab = null;

        private void CopyTabs()
        {
            for (int i = 0; i < targetTab.TabCount; i++)
            {
                allTabs.Add(new TabItem(targetTab.TabPages[i]));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int currentX = 2, y = Height - 23;
            bool seled = false;
            Graphics g = e.Graphics;
            TabRenderer.DrawTabPage(g, new Rectangle(0, Height - 1, Width, 2));
            for (int i = 0; i < allTabs.Count; i++)
            {
                seled = false;
                TabItem it = allTabs[i];

                if (it.Width == -1) it.Width = (int)g.MeasureString(it.Text, Font).Width + 7;

                Rectangle rect = new Rectangle(currentX, y, it.Width, 22);
                if (it.TabPage == targetTab.SelectedTab) { rect.X -= 1; rect.Height = 25; rect.Y = Height - 25; seled = true; rect.Width += 7; }
                if (e.ClipRectangle.IntersectsWith(rect))
                {
                    TabItemState state = TabItemState.Normal;
                    if (it.TabPage == targetTab.SelectedTab) state = TabItemState.Selected; 
                    else if (it.Hover) state = TabItemState.Hot;

                    TabRenderer.DrawTabItem(g, rect, it.Text, Font, state);
                    //g.DrawString(it.Text, Font, Brushes.Black, rect, stringFormatCenter);
                }
                it.X = currentX;
                currentX += seled ? it.Width + 6 : it.Width;
            }
            base.OnPaint(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            TabItem old = null;
            bool finded = false;
            for (int i = 0; i < allTabs.Count; i++)
            {
                TabItem it = allTabs[i];
                if (!finded)
                {
                    if (e.X > it.X && e.X < it.X + it.Width) { it.Hover = true; finded = true; old = enteredTab; enteredTab = it; }
                    else it.Hover = false;
                }
                else it.Hover = false;
            }
            if (!finded)
            {
                if (enteredTab != null) Invalidate(new Rectangle(enteredTab.X, 0, enteredTab.Width, Height));
            }
            else
            {
                if (old != null) Invalidate(new Rectangle(old.X, 0, old.Width, Height));
                Invalidate(new Rectangle(enteredTab.X, 0, enteredTab.Width, Height));
            }

            base.OnMouseMove(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (enteredTab != null)
            {
                enteredTab.Hover = false;
                enteredTab = null;
                Invalidate();
            }
            base.OnMouseLeave(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (enteredTab != null)
            {
                if (targetTab.SelectedTab != enteredTab.TabPage)
                    targetTab.SelectedTab = enteredTab.TabPage;
            }
            base.OnMouseDown(e);
        }
    }
}
