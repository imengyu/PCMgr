using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static PCMgr.Ctls.TaskMgrListApis;

namespace PCMgr.Helpers
{
    class ClassicalMenuRender : ToolStripRenderer, IDisposable
    {
        private const int M_DRAW_MENU_HOT = 1;
        private const int M_DRAW_MENU_CHECK = 3;
        private const int M_DRAW_MENU_CHECK_BACKGROUND = 4;
        private const int M_DRAW_MENU_RADIO = 5;
        private const int M_DRAW_MENU_SUB = 6;

        [DllImport(NativeMethods.COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void MMenuDrawItem(IntPtr hTheme, IntPtr hdc, int x, int y, int w, int h, int state, bool enabled);

        public ClassicalMenuRender(IntPtr hWnd)
        {
            menuThemeData = MOpenThemeData(hWnd, "MENU");
        }
        public void Dispose()
        {
            MCloseThemeData(menuThemeData);
        }

        private IntPtr menuThemeData = IntPtr.Zero;

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (e.Item is ToolStripSeparator)
            {
                ToolStripSeparator sp = (ToolStripSeparator)e.Item;
                e.Graphics.DrawLine(Pens.LightGray, sp.ContentRectangle.Left + 30, sp.ContentRectangle.Top, sp.ContentRectangle.Right, sp.ContentRectangle.Top);
            }
            else base.OnRenderSeparator(e);
        }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item is ToolStripMenuItem)
            {
                MMenuDrawItem(menuThemeData, e.Graphics.GetHdc(), 
                    e.ArrowRectangle.X, e.ArrowRectangle.Y, 
                    e.ArrowRectangle.Width, e.ArrowRectangle.Height, M_DRAW_MENU_SUB, e.Item.Enabled);
                e.Graphics.ReleaseHdc();
            }
            else base.OnRenderArrow(e);
        }
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //if (e.ToolStrip is ContextMenuStrip || e.ToolStrip is Contex)
                e.Graphics.DrawRectangle(Pens.LightGray, new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
            //else base.OnRenderToolStripBorder(e);
        }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if(e.Item.Enabled) TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, Color.Black, e.TextFormat);
            else TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, Color.Gray, e.TextFormat);
        }
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is ToolStripMenuItem)
            {
                ToolStripMenuItem i = e.Item as ToolStripMenuItem;
                if (i.Selected)
                {
                    MMenuDrawItem(menuThemeData, e.Graphics.GetHdc(), e.Item.ContentRectangle.X + 1, e.Item.ContentRectangle.Y, e.Item.Size.Width - 4, e.Item.Size.Height - 1, M_DRAW_MENU_HOT, e.Item.Enabled);
                    e.Graphics.ReleaseHdc();
                }
            }
            else base.OnRenderMenuItemBackground(e);
        }
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Item is ToolStripMenuItem)
            {
                ToolStripMenuItem i = e.Item as ToolStripMenuItem;
                if (i.Checked)
                    if (i.CheckState == CheckState.Checked)
                    {
                        int x = 3, y = 1, w = e.Item.Height - 1, h = w;
                        IntPtr hdc = e.Graphics.GetHdc();
                        MMenuDrawItem(menuThemeData, hdc, x, y, w, h, M_DRAW_MENU_CHECK, e.Item.Enabled);
                        MMenuDrawItem(menuThemeData, hdc, x, y, w, h, M_DRAW_MENU_CHECK_BACKGROUND, e.Item.Enabled);

                        e.Graphics.ReleaseHdc(hdc);
                    }
                    else if (i.CheckState == CheckState.Indeterminate)
                    {
                        IntPtr hdc = e.Graphics.GetHdc();
                        int x = 3, y = 1, w = e.Item.Height - 1, h = w;
                        MMenuDrawItem(menuThemeData, hdc, x, y, w, h, M_DRAW_MENU_RADIO, e.Item.Enabled);
                        MMenuDrawItem(menuThemeData, hdc, x, y, w, h, M_DRAW_MENU_CHECK_BACKGROUND, e.Item.Enabled);

                        e.Graphics.ReleaseHdc(hdc);
                    }
            }
            else base.OnRenderItemCheck(e);
        }

    }
}
