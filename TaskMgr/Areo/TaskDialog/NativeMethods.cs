/*****************************************************
 *            Vista Controls for .NET 2.0
 * 
 * http://www.codeplex.com/vistacontrols
 * 
 * @author: Lorenz Cuno Klopfenstein
 * Licensed under Microsoft Community License (Ms-CL)
 * 
 *****************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PCMgr.Aero.TaskDialog {

    internal class NativeMethods {

        /// <summary>Direct Task Dialog call.</summary>
        [DllImport(FormMain.COREDLLNAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MTaskDialog(IntPtr hWndParent, IntPtr hInstance,
            string pszWindowTitle, string pszMainInstruction, string pszContent,
            int dwCommonButtons, IntPtr pszIcon, out int pnButton);

        /// <summary>Indirect Task Dialog call. Allows complex dialogs with interaction logic (via callback).</summary>
        [DllImport(FormMain.COREDLLNAME, CharSet = CharSet.Unicode, PreserveSig = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MTaskDialogIndirect(ref TaskDialogConfig pTaskConfig,
            out int pnButton, out int pnRadioButton, out bool pfVerificationFlagChecked);

        public static int TaskDialog(IntPtr hWndParent, IntPtr hInstance, string pszWindowTitle, string pszMainInstruction, string pszContent,
            int dwCommonButtons, IntPtr pszIcon, out int pnButton)
        {
            return MTaskDialog(hWndParent, hInstance, pszWindowTitle, pszMainInstruction, pszContent,
                dwCommonButtons, pszIcon, out pnButton);
        }
        public static IntPtr TaskDialogIndirect(ref TaskDialogConfig pTaskConfig,
            out int pnButton, out int pnRadioButton, out bool pfVerificationFlagChecked)
        {
            return MTaskDialogIndirect(ref pTaskConfig, out pnButton, out pnRadioButton, out pfVerificationFlagChecked);
        }

        internal delegate IntPtr TaskDialogCallback(IntPtr hwnd, uint msg, UIntPtr wParam, IntPtr lParam, IntPtr refData);


        /// <summary>The Task Dialog config structure.</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        internal struct TaskDialogConfig {
            public uint cbSize;
            public IntPtr hwndParent;
            public IntPtr hInstance;
            public TaskDialogFlags dwFlags;
            public TaskDialogButton dwCommonButtons;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszWindowTitle;
            public IntPtr hMainIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszMainInstruction;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszContent;
            public uint cButtons;
            public IntPtr pButtons;
            public int nDefaultButton;
            public uint cRadioButtons;
            public IntPtr pRadioButtons;
            public int nDefaultRadioButton;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszVerificationText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExpandedInformation;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExpandedControlText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCollapsedControlText;
            public IntPtr hFooterIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszFooter;
            public TaskDialogCallback pfCallback;
            public IntPtr lpCallbackData;
            public uint cxWidth;
        }

        /// <summary>Flags used in TaskDialogConfig struct.</summary>
        /// <remarks>From CommCtrl.h.</remarks>
        [Flags]
        internal enum TaskDialogFlags {
            TDF_ENABLE_HYPERLINKS = 0x0001,
            TDF_USE_HICON_MAIN = 0x0002,
            TDF_USE_HICON_FOOTER = 0x0004,
            TDF_ALLOW_DIALOG_CANCELLATION = 0x0008,
            TDF_USE_COMMAND_LINKS = 0x0010,
            TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020,
            TDF_EXPAND_FOOTER_AREA = 0x0040,
            TDF_EXPANDED_BY_DEFAULT = 0x0080,
            TDF_VERIFICATION_FLAG_CHECKED = 0x0100,
            TDF_SHOW_PROGRESS_BAR = 0x0200,
            TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400,
            TDF_CALLBACK_TIMER = 0x0800,
            TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000,
            TDF_RTL_LAYOUT = 0x2000,
            TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000,
            TDF_CAN_BE_MINIMIZED = 0x8000
        }

        /// <summary>Notifications returned by Task Dialogs to the callback.</summary>
        /// <remarks>From CommCtrl.h.</remarks>
        public enum TaskDialogNotification : uint {
            TDN_CREATED = 0,
            TDN_NAVIGATED = 1,
            TDN_BUTTON_CLICKED = 2,            // wParam = Button ID
            TDN_HYPERLINK_CLICKED = 3,            // lParam = (LPCWSTR)pszHREF
            TDN_TIMER = 4,            // wParam = Milliseconds since dialog created or timer reset
            TDN_DESTROYED = 5,
            TDN_RADIO_BUTTON_CLICKED = 6,            // wParam = Radio Button ID
            TDN_DIALOG_CONSTRUCTED = 7,
            TDN_VERIFICATION_CLICKED = 8,             // wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0
            TDN_HELP = 9,
            TDN_EXPANDO_BUTTON_CLICKED = 10            // wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
        }

        /// <summary>Messages that can be sent to Task Dialogs.</summary>
        /// <remarks>From CommCtrl.h.</remarks>
        public enum TaskDialogMessages : uint {
            TDM_NAVIGATE_PAGE = 0x0400 + 101,
            TDM_CLICK_BUTTON = 0x0400 + 102, // wParam = Button ID
            TDM_SET_MARQUEE_PROGRESS_BAR = 0x0400 + 103, // wParam = 0 (nonMarque) wParam != 0 (Marquee)
            TDM_SET_PROGRESS_BAR_STATE = 0x0400 + 104, // wParam = new progress state
            TDM_SET_PROGRESS_BAR_RANGE = 0x0400 + 105, // lParam = MAKELPARAM(nMinRange, nMaxRange)
            TDM_SET_PROGRESS_BAR_POS = 0x0400 + 106, // wParam = new position
            TDM_SET_PROGRESS_BAR_MARQUEE = 0x0400 + 107, // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
            TDM_SET_ELEMENT_TEXT = 0x0400 + 108, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
            TDM_CLICK_RADIO_BUTTON = 0x0400 + 110, // wParam = Radio Button ID
            TDM_ENABLE_BUTTON = 0x0400 + 111, // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
            TDM_ENABLE_RADIO_BUTTON = 0x0400 + 112, // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
            TDM_CLICK_VERIFICATION = 0x0400 + 113, // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
            TDM_UPDATE_ELEMENT_TEXT = 0x0400 + 114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
            TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = 0x0400 + 115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
            TDM_UPDATE_ICON = 0x0400 + 116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
        }
    }

}
