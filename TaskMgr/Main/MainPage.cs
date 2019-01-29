using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCMgr.Main
{
    class MainPage
    {
        public MainPage(FormMain formMain, TabPage page)
        {
            Page = page;
            FormMain = formMain;
            NativeBridge = formMain.MainNativeBridge;
            Handle = formMain.Handle;
        }

        protected MainNativeBridge NativeBridge { get; private set; }
        protected TabPage Page { get; private set; }
        protected FormMain FormMain { get; private set; }

        public Size Size { get => FormMain.Size; set { FormMain.Size = value; } }
        public Font Font { get => FormMain.Font; }
        public Point MousePosition { get => Control.MousePosition; }
        public IntPtr Handle { get; } = IntPtr.Zero;
        public bool Inited { get; set; } = false;
        public virtual int GetUpdateDatum()
        {
            return 1;
        }
        public void UnLoad()
        {
            OnUnLoad();
        }
        public void Load()
        {
            OnLoad();
            OnLoadControlEvents();
        }

        protected virtual void OnUnLoad() { }
        protected virtual void OnLoad() { }
        protected virtual void OnLoadControlEvents() { }
    }
}
