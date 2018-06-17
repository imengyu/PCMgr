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
using System.Text;
using System.Runtime.InteropServices;

namespace TaskMgr.Aero.TaskDialog {

    /// <summary>Stores a Task Dialog message that will be sent to a dialog in order to update its state.</summary>
    internal class Message : IDisposable {
        
        /// <summary>Text values that can be updated.</summary>
        public enum DialogElements : int {
            TDE_CONTENT,
            TDE_EXPANDED_INFORMATION,
            TDE_FOOTER,
            TDE_MAIN_INSTRUCTION
        }

        /// <summary>Simple int, int message.</summary>
        public Message(NativeMethods.TaskDialogMessages msg, int w, int l) {
            MessageType = msg;
            wParam = w;
            lParam = l;
        }

        /// <summary>Simple int, bool message.</summary>
        public Message(NativeMethods.TaskDialogMessages msg, int w, bool l) {
            MessageType = msg;
            wParam = w;
            lParam = (l) ? 1 : 0;
        }

        /// <summary>Simple bool, bool message.</summary>
        public Message(NativeMethods.TaskDialogMessages msg, bool w, bool l) {
            MessageType = msg;
            wParam = (w) ? 1 : 0;
            lParam = (l) ? 1 : 0;
        }

		/// <summary>Simple bool, int message.</summary>
		public Message(NativeMethods.TaskDialogMessages msg, bool w, int l) {
			MessageType = msg;
			wParam = (w) ? 1 : 0;
			lParam = l;
		}

        /// <summary>Simple int, long (hi word and lo word) message.</summary>
        public Message(NativeMethods.TaskDialogMessages msg, int w, int l_hi, int l_lo) {
            MessageType = msg;
            wParam = w;
            lParam = (l_lo << 16) + l_hi;
        }

        /// <summary>Text updating message.</summary>
		/// <remarks>The string will be marshaled: the Message must be correctly disposed after use.</remarks>
        public Message(NativeMethods.TaskDialogMessages msg, DialogElements element, string s) {
            MessageType = msg;
            wParam = (int)element;

            _unsafeHandle = Marshal.StringToHGlobalUni(s);
            lParam = (int)_unsafeHandle;
        }

		/// <summary>Navigation message.</summary>
		/// <remarks>The config structure will be marshaled: must be correctly disposed after use.</remarks>
        public Message(NativeMethods.TaskDialogMessages msg, int w, NativeMethods.TaskDialogConfig config) {
            MessageType = msg;
            wParam = w;

            _unsafeHandle = Marshal.AllocHGlobal(Marshal.SizeOf(config));
            Marshal.StructureToPtr(config, _unsafeHandle, false);
            lParam = (int)_unsafeHandle;
        }

        IntPtr _unsafeHandle = IntPtr.Zero;

        public NativeMethods.TaskDialogMessages MessageType { get; set; }

        public int wParam { get; set; }

        public int lParam { get; set; }


        #region IDisposable Members

        public void Dispose() {
            if (_unsafeHandle != IntPtr.Zero)
                Marshal.FreeHGlobal(_unsafeHandle);
        }

        #endregion
    }
}
