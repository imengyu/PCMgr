using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PCMgr.Ctls
{
    public class PerformanceRamPoolGrid : Control
    {
        public PerformanceRamPoolGrid()
        {
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            BgBrush = new SolidBrush(Main.MainPagePerf.RamBgColor);
            DrawPen = new Pen(Main.MainPagePerf.RamDrawColor);
            GridPen = new Pen(Color.FromArgb(206, 176, 215));
            TextBrush = new SolidBrush(Color.Gray);
            stringFormatRight = new StringFormat();
            stringFormatRight.Alignment = StringAlignment.Far;
            stringFormatCenter = new StringFormat();
            stringFormatCenter.Alignment = StringAlignment.Center;
            stringFormatCenter.LineAlignment = StringAlignment.Center;
            stringFormatRightBottom = new StringFormat();
            stringFormatRightBottom.Alignment = StringAlignment.Far;
            stringFormatRightBottom.LineAlignment = StringAlignment.Far;
        }

        private Brush brushModified = new SolidBrush(Color.FromArgb(206, 180, 215));
        private Brush brushBackuped = new SolidBrush(Color.FromArgb(231, 207, 238));
        private ToolTip toolTip = new ToolTip();

        private Rectangle rectInUse = default(Rectangle);
        private Rectangle rectModified = default(Rectangle);
        private Rectangle rectStandby = default(Rectangle);
        private Rectangle rectFree = default(Rectangle);

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
        public Color GridColor
        {
            get { return GridPen.Color; }
            set
            {
                if (GridPen != null)
                    GridPen.Color = value;
            }
        }
        public Brush BgBrush { get; set; }
        public Pen DrawPen { get; set; }
        public Pen GridPen { get; set; }
        [Localizable(true)]
        public string LeftText { get; set; }
        [Localizable(true)]
        public string RightText { get; set; }
        public Color TextColor
        {
            get { return (TextBrush as SolidBrush).Color; }
            set
            {
                if (TextBrush != null)
                    (TextBrush as SolidBrush).Color = value;
            }
        }

        private Brush TextBrush { get; set; }
        private StringFormat stringFormatRight = null;
        private StringFormat stringFormatCenter = null;
        private StringFormat stringFormatRightBottom = null;
        private const int BottomTextOffestY = 2;

        public int TopTextHeight { get; set; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ShowTooltip(e.Location);
            base.OnMouseMove(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            lastShowTooltip = 0;
            toolTip.Hide(this);
            base.OnMouseLeave(e);
        }

        private int lastShowTooltip = 0;
        private void ShowTooltip(Point p)
        {
            if (rectInUse.Contains(p) && lastShowTooltip != 1)
            {
                lastShowTooltip = 1;
                toolTip.Show(TipVauleUsing, this, 0, Height + 2, 5000);
            }
            else if (rectModified.Contains(p) && lastShowTooltip != 2)
            {
                lastShowTooltip = 2;
                toolTip.Show(TipVauleModified, this, rectModified.X, Height + 2, 3000);
            }
            else if (rectStandby.Contains(p) && lastShowTooltip != 3)
            {
                lastShowTooltip = 3;
                toolTip.Show(TipVauleStandby, this, rectStandby.X, Height + 2, 4000);
            }
            else if (rectFree.Contains(p) && lastShowTooltip != 4)
            {
                lastShowTooltip = 4;
                toolTip.Show(TipVauleFree, this, rectFree.X, Height + 2, 3000);
            }
        }

        public string TipVauleUsing { get; set; }
        public string TipVauleModified { get; set; }
        public string TipVauleStandby { get; set; }
        public string TipVauleFree { get; set; }
        public string StrVauleUsing { get; set; }
        public string StrVauleModified { get; set; }
        public string StrVauleStandby { get; set; }
        public string StrVauleFree { get; set; }
        public double VauleUsing { get; set; }
        public double VauleModified { get; set; }
        public double VauleStandby { get; set; }
        public double VauleFree { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle r = new Rectangle(0, TopTextHeight, Width - 1, Height - TopTextHeight - 1 - TopTextHeight);

            if (LeftText != "") g.DrawString(LeftText, Font, TextBrush, 0, 0);
            if (RightText != "") g.DrawString(RightText, Font, TextBrush, new Rectangle(0, 0, Width, TopTextHeight), stringFormatRight);

            int w = 0;
            if (VauleUsing > 0 && VauleUsing < 1)
            {
                w = (int)(VauleUsing * Width);
                if (w > 1)
                {
                    rectInUse = new Rectangle(0, r.Top, w, r.Height);
                    g.FillRectangle(BgBrush, rectInUse);
                    g.DrawLine(DrawPen, w, r.Top, w, r.Bottom);
                    if (w > 100)
                    {
                        g.DrawString(Lanuages.LanuageMgr.GetStr("MemUsingS"), Font, TextBrush, 0, Height - TopTextHeight + BottomTextOffestY);
                        g.DrawString(StrVauleUsing, Font, TextBrush, rectInUse, stringFormatCenter);
                    }
                }
                else rectInUse = default(Rectangle);
            }
            if (VauleModified > 0 && VauleModified < 1)
            {
                int oldw = w;
                w += (int)(VauleModified * Width);
                if (w - oldw > 1)
                {
                    rectModified = new Rectangle(oldw, r.Top, w - oldw, r.Height);
                    g.FillRectangle(brushModified, rectModified);
                    g.DrawLine(DrawPen, w, r.Top, w, r.Bottom);

                    if (rectInUse.Width > 100)//右
                        g.DrawString(Lanuages.LanuageMgr.GetStr("MemModifed"), Font, TextBrush, new Rectangle(rectInUse.X, Height - TopTextHeight + BottomTextOffestY, rectInUse.Width + rectModified.Width, TopTextHeight), stringFormatRight);
                    if (w - oldw > 50)
                        g.DrawString(StrVauleModified, Font, TextBrush, rectModified, stringFormatCenter);
                }
                else rectModified = default(Rectangle);
            }
            if (VauleStandby > 0 && VauleStandby < 1)
            {
                int oldw = w;
                w += (int)(VauleStandby * Width);
                if (w - oldw > 1)
                {
                    rectStandby = new Rectangle(oldw, r.Top, w - oldw, r.Height);
                    g.FillRectangle(brushBackuped, rectStandby);
                    g.DrawLine(DrawPen, w, r.Top, w, r.Bottom);

                    if (w - oldw > 50)           
                        g.DrawString(Lanuages.LanuageMgr.GetStr("MemStandby"), Font, TextBrush, oldw, Height - TopTextHeight + BottomTextOffestY);
                    if (w - oldw > 50)
                        g.DrawString(StrVauleStandby, Font, TextBrush, rectStandby, stringFormatCenter);
                 
                }
                else rectStandby = default(Rectangle);
            }
            if (VauleFree > 0 && VauleFree < 1)
            {
                int oldw = w;
                w += (int)(VauleFree * Width);
                rectFree = new Rectangle(oldw, r.Top, w - oldw, r.Height);
                if (rectStandby.Width > 50)//右
                    g.DrawString(Lanuages.LanuageMgr.GetStr("MemFree"), Font, TextBrush, new Rectangle(rectStandby.X, Height - TopTextHeight + BottomTextOffestY, rectStandby.Width + rectFree.Width, TopTextHeight), stringFormatRight);
                if (w - oldw > 50)
                    g.DrawString(StrVauleFree, Font, TextBrush, rectFree, stringFormatCenter);
                
            }

            g.DrawRectangle(DrawPen, r);
        }
    }
}
