/*
* VISTA CONTROLS FOR .NET 2.0
* 
* Written by Marco Minerva, mailto:marco.minerva@gmail.com
* 
* This code is released under the Microsoft Community License (Ms-CL).
* A copy of this license is available at
* http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedcommunitylicense.mspx
*/

using System;
using System.Runtime.InteropServices;

namespace PCMgr.Aero
{
    /// <summary>
    /// Control style and notification constants
    /// </summary>
    internal class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern void SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

		/*[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
		public static extern bool StopMouseCapture();

		[DllImport("user32.dll", EntryPoint = "SetCapture")]
		public static extern IntPtr StartMouseCapture(IntPtr hWnd);*/

		//Window styles
		public const int WS_EX_CONTROLPARENT = 0x00010000;
		public const int WS_EX_CLIENTEDGE = 0x00000200;
		public const int WS_BORDER = 0x00800000;

		//Window NCHITTEST values
		public const int HTTRANSPARENT = -1;
        
        //Button styles
        public const int BS_COMMANDLINK = 0x0000000E;
        public const int BS_SPLITBUTTON = 0x0000000C;
		public const int BS_DEFSPLITBUTTON = 0x0000000D;
        //Button messages
        public const int BCM_SETNOTE = 0x00001609;
        public const int BCM_SETSHIELD = 0x0000160C;
        public const int BM_SETIMAGE = 0x00F7;
		public const int BCM_SETSPLITINFO = 0x1600 + 0x0007;

		public const int BCN_SETDROPDOWNSTATE = 0x1606;

        public const int ECM_FIRST = 0x1500;
        public const int EM_SETCUEBANNER = ECM_FIRST + 1;

        //ComboBox messages
        public const int CB_SETCUEBANNER = 0x1703;

        //Static messages
        public const int STM_SETICON = 0x0170;

        //Treeview universal constants
        public const int TV_FIRST = 0x1100;
        //Treeview messages
        public const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
        public const int TVM_GETEXTENDEDSTYLE = TV_FIRST + 45;
        public const int TVM_SETAUTOSCROLLINFO = TV_FIRST + 59;
        //Treeview styles
        public const int TVS_NOHSCROLL = 0x8000;
        //Treeview extended styles
        public const int TVS_EX_AUTOHSCROLL = 0x0020;
        public const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
        public const int TVS_EX_DOUBLEBUFFER = 0x0004;
        public const int GWL_STYLE = -16;

        //Listview universal constants
        public const int LVM_FIRST = 0x1000;
        //Listview messages
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        public const int LVS_EX_FULLROWSELECT = 0x00000020;
        //Listview extended styles
        public const int LVS_EX_DOUBLEBUFFER = 0x00010000;

        //Progressbar styles
        public const int PBS_SMOOTHREVERSE = 0x10;
        public const int PBST_NORMAL = 0x0001; //used with PBM_SETSTATE
        public const int PBST_ERROR = 0x0002; //used with PBM_SETSTATE
        public const int PBST_PAUSED = 0x0003; //used with PBM_SETSTATE
        //Progressbar messages
        public const int PBM_SETSTATE = WM_USER + 16;

        //Windows notifications
        public const int WM_NULL = 0x00;
        public const int WM_CREATE = 0x01;
        public const int WM_DESTROY = 0x02;
        public const int WM_MOVE = 0x03;
        public const int WM_SIZE = 0x05;
        public const int WM_ACTIVATE = 0x06;
        public const int WM_SETFOCUS = 0x07;
        public const int WM_KILLFOCUS = 0x08;
        public const int WM_ENABLE = 0x0A;
        public const int WM_SETREDRAW = 0x0B;
        public const int WM_SETTEXT = 0x0C;
        public const int WM_GETTEXT = 0x0D;
        public const int WM_GETTEXTLENGTH = 0x0E;
        public const int WM_PAINT = 0x0F;
        public const int WM_CLOSE = 0x10;
        public const int WM_QUERYENDSESSION = 0x11;
        public const int WM_QUIT = 0x12;
        public const int WM_QUERYOPEN = 0x13;
        public const int WM_ERASEBKGND = 0x14;
        public const int WM_SYSCOLORCHANGE = 0x15;
        public const int WM_ENDSESSION = 0x16;
        public const int WM_SYSTEMERROR = 0x17;
        public const int WM_SHOWWINDOW = 0x18;
        public const int WM_CTLCOLOR = 0x19;
        public const int WM_WININICHANGE = 0x1A;
        public const int WM_SETTINGCHANGE = 0x1A;
        public const int WM_DEVMODECHANGE = 0x1B;
        public const int WM_ACTIVATEAPP = 0x1C;
        public const int WM_FONTCHANGE = 0x1D;
        public const int WM_TIMECHANGE = 0x1E;
        public const int WM_CANCELMODE = 0x1F;
        public const int WM_SETCURSOR = 0x20;
        public const int WM_MOUSEACTIVATE = 0x21;
        public const int WM_CHILDACTIVATE = 0x22;
        public const int WM_QUEUESYNC = 0x23;
        public const int WM_GETMINMAXINFO = 0x24;
        public const int WM_PAINTICON = 0x26;
        public const int WM_ICONERASEBKGND = 0x27;
        public const int WM_NEXTDLGCTL = 0x28;
        public const int WM_SPOOLERSTATUS = 0x2A;
        public const int WM_DRAWITEM = 0x2B;
        public const int WM_MEASUREITEM = 0x2C;
        public const int WM_DELETEITEM = 0x2D;
        public const int WM_VKEYTOITEM = 0x2E;
        public const int WM_CHARTOITEM = 0x2F;

        public const int WM_SETFONT = 0x30;
        public const int WM_GETFONT = 0x31;
        public const int WM_SETHOTKEY = 0x32;
        public const int WM_GETHOTKEY = 0x33;
        public const int WM_QUERYDRAGICON = 0x37;
        public const int WM_COMPAREITEM = 0x39;
        public const int WM_COMPACTING = 0x41;
        public const int WM_WINDOWPOSCHANGING = 0x46;
        public const int WM_WINDOWPOSCHANGED = 0x47;
        public const int WM_POWER = 0x48;
        public const int WM_COPYDATA = 0x4A;
        public const int WM_CANCELJOURNAL = 0x4B;
        public const int WM_NOTIFY = 0x4E;
        public const int WM_INPUTLANGCHANGEREQUEST = 0x50;
        public const int WM_INPUTLANGCHANGE = 0x51;
        public const int WM_TCARD = 0x52;
        public const int WM_HELP = 0x53;
        public const int WM_USERCHANGED = 0x54;
        public const int WM_NOTIFYFORMAT = 0x55;
        public const int WM_CONTEXTMENU = 0x7B;
        public const int WM_STYLECHANGING = 0x7C;
        public const int WM_STYLECHANGED = 0x7D;
        public const int WM_DISPLAYCHANGE = 0x7E;
        public const int WM_GETICON = 0x7F;
        public const int WM_SETICON = 0x80;

        public const int WM_NCCREATE = 0x81;
        public const int WM_NCDESTROY = 0x82;
        public const int WM_NCCALCSIZE = 0x83;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_NCPAINT = 0x85;
        public const int WM_NCACTIVATE = 0x86;
        public const int WM_GETDLGCODE = 0x87;
        public const int WM_NCMOUSEMOVE = 0xA0;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_NCLBUTTONUP = 0xA2;
        public const int WM_NCLBUTTONDBLCLK = 0xA3;
        public const int WM_NCRBUTTONDOWN = 0xA4;
        public const int WM_NCRBUTTONUP = 0xA5;
        public const int WM_NCRBUTTONDBLCLK = 0xA6;
        public const int WM_NCMBUTTONDOWN = 0xA7;
        public const int WM_NCMBUTTONUP = 0xA8;
        public const int WM_NCMBUTTONDBLCLK = 0xA9;

        public const int WM_KEYFIRST = 0x100;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_CHAR = 0x102;
        public const int WM_DEADCHAR = 0x103;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_SYSCHAR = 0x106;
        public const int WM_SYSDEADCHAR = 0x107;
        public const int WM_KEYLAST = 0x108;

        public const int WM_IME_STARTCOMPOSITION = 0x10D;
        public const int WM_IME_ENDCOMPOSITION = 0x10E;
        public const int WM_IME_COMPOSITION = 0x10F;
        public const int WM_IME_KEYLAST = 0x10F;

        public const int WM_INITDIALOG = 0x110;
        public const int WM_COMMAND = 0x111;
        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_TIMER = 0x113;
        public const int WM_HSCROLL = 0x114;
        public const int WM_VSCROLL = 0x115;
        public const int WM_INITMENU = 0x116;
        public const int WM_INITMENUPOPUP = 0x117;
        public const int WM_MENUSELECT = 0x11F;
        public const int WM_MENUCHAR = 0x120;
        public const int WM_ENTERIDLE = 0x121;

        public const int WM_CTLCOLORMSGBOX = 0x132;
        public const int WM_CTLCOLOREDIT = 0x133;
        public const int WM_CTLCOLORLISTBOX = 0x134;
        public const int WM_CTLCOLORBTN = 0x135;
        public const int WM_CTLCOLORDLG = 0x136;
        public const int WM_CTLCOLORSCROLLBAR = 0x137;
        public const int WM_CTLCOLORSTATIC = 0x138;

        public const int WM_MOUSEFIRST = 0x200;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MBUTTONDBLCLK = 0x209;
        public const int WM_MOUSELAST = 0x20A;
        public const int WM_MOUSEWHEEL = 0x20A;

        public const int WM_PARENTNOTIFY = 0x210;
        public const int WM_ENTERMENULOOP = 0x211;
        public const int WM_EXITMENULOOP = 0x212;
        public const int WM_NEXTMENU = 0x213;
        public const int WM_SIZING = 0x214;
        public const int WM_CAPTURECHANGED = 0x215;
        public const int WM_MOVING = 0x216;
        public const int WM_POWERBROADCAST = 0x218;
        public const int WM_DEVICECHANGE = 0x219;

        public const int WM_MDICREATE = 0x220;
        public const int WM_MDIDESTROY = 0x221;
        public const int WM_MDIACTIVATE = 0x222;
        public const int WM_MDIRESTORE = 0x223;
        public const int WM_MDINEXT = 0x224;
        public const int WM_MDIMAXIMIZE = 0x225;
        public const int WM_MDITILE = 0x226;
        public const int WM_MDICASCADE = 0x227;
        public const int WM_MDIICONARRANGE = 0x228;
        public const int WM_MDIGETACTIVE = 0x229;
        public const int WM_MDISETMENU = 0x230;
        public const int WM_ENTERSIZEMOVE = 0x231;
        public const int WM_EXITSIZEMOVE = 0x232;
        public const int WM_DROPFILES = 0x233;
        public const int WM_MDIREFRESHMENU = 0x234;

        public const int WM_IME_SETCONTEXT = 0x281;
        public const int WM_IME_NOTIFY = 0x282;
        public const int WM_IME_CONTROL = 0x283;
        public const int WM_IME_COMPOSITIONFULL = 0x284;
        public const int WM_IME_SELECT = 0x285;
        public const int WM_IME_CHAR = 0x286;
        public const int WM_IME_KEYDOWN = 0x290;
        public const int WM_IME_KEYUP = 0x291;

        public const int WM_MOUSEHOVER = 0x2A1;
        public const int WM_NCMOUSELEAVE = 0x2A2;
        public const int WM_MOUSELEAVE = 0x2A3;

        public const int WM_CUT = 0x300;
        public const int WM_COPY = 0x301;
        public const int WM_PASTE = 0x302;
        public const int WM_CLEAR = 0x303;
        public const int WM_UNDO = 0x304;

        public const int WM_RENDERFORMAT = 0x305;
        public const int WM_RENDERALLFORMATS = 0x306;
        public const int WM_DESTROYCLIPBOARD = 0x307;
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_PAINTCLIPBOARD = 0x309;
        public const int WM_VSCROLLCLIPBOARD = 0x30A;
        public const int WM_SIZECLIPBOARD = 0x30B;
        public const int WM_ASKCBFORMATNAME = 0x30C;
        public const int WM_CHANGECBCHAIN = 0x30D;
        public const int WM_HSCROLLCLIPBOARD = 0x30E;
        public const int WM_QUERYNEWPALETTE = 0x30F;
        public const int WM_PALETTEISCHANGING = 0x310;
        public const int WM_PALETTECHANGED = 0x311;

        public const int WM_HOTKEY = 0x312;
        public const int WM_PRINT = 0x317;
        public const int WM_PRINTCLIENT = 0x318;

        public const int WM_HANDHELDFIRST = 0x358;
        public const int WM_HANDHELDLAST = 0x35F;
        public const int WM_PENWINFIRST = 0x380;
        public const int WM_PENWINLAST = 0x38F;
        public const int WM_COALESCE_FIRST = 0x390;
        public const int WM_COALESCE_LAST = 0x39F;
        public const int WM_DDE_FIRST = 0x3E0;
        public const int WM_DDE_INITIATE = 0x3E0;
        public const int WM_DDE_TERMINATE = 0x3E1;
        public const int WM_DDE_ADVISE = 0x3E2;
        public const int WM_DDE_UNADVISE = 0x3E3;
        public const int WM_DDE_ACK = 0x3E4;
        public const int WM_DDE_DATA = 0x3E5;
        public const int WM_DDE_REQUEST = 0x3E6;
        public const int WM_DDE_POKE = 0x3E7;
        public const int WM_DDE_EXECUTE = 0x3E8;
        public const int WM_DDE_LAST = 0x3E8;

        public const int WM_USER = 0x400;
        public const int WM_APP = 0x8000;
    }
}
