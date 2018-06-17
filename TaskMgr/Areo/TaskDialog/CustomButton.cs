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

    /// <summary>Represents a custom button shown on a Task Dialog.</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct CustomButton {
        private int _id;

        [MarshalAs(UnmanagedType.LPWStr)]
        private string _txt;

        /// <summary>Instantiates a new custom button with an ID and a text.</summary>
        /// <param name="id">Unique ID that will be returned by the Task Dialog if the button is clicked.
        /// Use values greater than 8 to prevent conflicts with common buttons.</param>
        /// <param name="text">Text label shown on the button. If you enable Command Links, a newline here
        /// separates the upper from the lower string on the button.</param>
        public CustomButton(int id, string text) {
            _id = id;
            _txt = text;
        }

        /// <summary>Instantiates a new custom button with an ID and a text.</summary>
        /// <param name="id">Common ID that will be returned by the Task Dialog if the button is clicked.</param>
        /// <param name="text">Text label shown on the button. If you enable Command Links, a newline here
        /// separates the upper from the lower string on the button.</param>
        public CustomButton(Result commonResult, string text) {
            _id = (int)commonResult;
            _txt = text;
        }

        /// <summary>Unique ID that will be returned by the Task Dialog if the button is clicked.</summary>
        public int Id {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>Text label shown on the button. If you enable Command Links, a newline here
        /// separates the upper from the lower string on the button.</summary>
        public string Text {
            get { return _txt; }
            set { _txt = value; }
        }

        public static int SizeOf() {
            return Marshal.SizeOf(typeof(CustomButton));
        }
    }

}
