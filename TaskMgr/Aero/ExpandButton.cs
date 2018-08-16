using System;
using System.Drawing;
using System.Windows.Forms;

namespace PCMgr.Aero
{
    [ToolboxBitmap(typeof(Button))]
    public class ExpandButton : Button
    {
        public ExpandButton()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
            stringFormat.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;
        }

        private enum State
        {
            Normal,
            Hover,
            Pressed
        }

        public bool Expanded {
            get { return expanded; }
            set
            {
                if (expanded != value)
                {
                    expanded = value;
                    ExpandedChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public event EventHandler ExpandedChanged;

        private bool expanded = false;
        private StringFormat stringFormat = null;
        private IntPtr hTheme = IntPtr.Zero;
        private State state = State.Normal;
        private const int WM_UPDATEUISTATE = 0x0128;

        protected override void OnGotFocus(EventArgs e)
        {
            state = State.Hover;
            Invalidate();
            base.OnGotFocus(e);
        }
        protected override void OnLostFocus(EventArgs e)
        {
            if (state != State.Normal)
            {
                state = State.Normal;
                Invalidate();
            }
            base.OnLostFocus(e);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_UPDATEUISTATE)
            {
                if (PCMgr.NativeMethods.LOWORD((uint)m.WParam.ToInt32()) == 1)
                {
                    if (stringFormat.HotkeyPrefix == System.Drawing.Text.HotkeyPrefix.Show)
                        stringFormat.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Hide;
                    else stringFormat.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;
                }
            }
            base.WndProc(ref m);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            state = State.Hover;
        }
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            state = State.Normal;
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            state = State.Pressed;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            state = State.Normal;
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            if (DesignMode == false)
                hTheme = Ctls.TaskMgrListApis.MOpenThemeData(Handle, "TASKDIALOG");
            base.OnHandleCreated(e);
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (!DesignMode) Ctls.TaskMgrListApis.MCloseThemeData(hTheme);
                hTheme = IntPtr.Zero;
            }
            if (stringFormat != null)
            {
                stringFormat.Dispose();
                stringFormat = null;
            }
            base.Dispose(disposing);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (!DesignMode)
            {
                IntPtr hdc = e.Graphics.GetHdc();
                switch (state)
                {
                    case State.Normal:
                        NativeMethods.MExpandDrawButton(hTheme, hdc, 0, Height > 19 ? Height / 2 - 9 : 0, NativeMethods.M_DRAW_EXPAND_NORMAL, expanded);
                        break;
                    case State.Hover:
                        NativeMethods.MExpandDrawButton(hTheme, hdc, 0, Height > 19 ? Height / 2 - 9 : 0, NativeMethods.M_DRAW_EXPAND_HOVER, expanded);
                        break;
                    case State.Pressed:
                        NativeMethods.MExpandDrawButton(hTheme, hdc, 0, Height > 19 ? Height / 2 - 9 : 0, NativeMethods.M_DRAW_EXPAND_PRESSED, expanded);
                        break;
                }
                e.Graphics.ReleaseHdc();
            }
            using (SolidBrush s = new SolidBrush(ForeColor))
                e.Graphics.DrawString(Text, Font, s, new Rectangle(20, 0, Width - 20, Height), stringFormat);
            if (Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
            //base.OnPaint(e);
        }
    }
}
