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

namespace TaskMgr.Aero.TaskDialog {
	public partial class TaskDialog {

		#region Static display methods

		/// <summary>Displays a task dialog that has a message.</summary>
		/// <param name="text">The text to display.</param>
		public static Result Show(string instruction) {
			return InternalShow(IntPtr.Zero, defaultTitle, instruction, defaultContent, TaskDialogButton.OK, TaskDialogIcon.None);
		}

		/// <summary>Displays a task dialog that has a message and a title.</summary>
		/// <param name="text">The text to display.</param>
		/// <param name="title">The title bar caption of the dialog.</param>
		public static Result Show(string instruction, string title) {
			return InternalShow(IntPtr.Zero, title, instruction, defaultContent, TaskDialogButton.OK, TaskDialogIcon.None);
		}

		/// <summary>Displays a task dialog that has a message, a title and an instruction.</summary>
		/// <param name="text">The text to display.</param>
		/// <param name="title">The title bar caption of the dialog.</param>
		/// <param name="instruction">The instruction shown below the main text.</param>
		public static Result Show(string instruction, string title, string content) {
			return InternalShow(IntPtr.Zero, title, instruction, content, TaskDialogButton.OK, TaskDialogIcon.None);
		}

		/// <summary>Displays a task dialog that has a message, a title, an instruction and one or more buttons.</summary>
		/// <param name="text">The text to display.</param>
		/// <param name="title">The title bar caption of the dialog.</param>
		/// <param name="instruction">The instruction shown below the main text.</param>
		/// <param name="buttons">Value that specifies which button or buttons to display.</param>
		public static Result Show(string instruction, string title, string content, TaskDialogButton buttons) {
			return InternalShow(IntPtr.Zero, title, instruction, content, buttons, TaskDialogIcon.None);
		}

		/// <summary>Displays a task dialog that has a message, a title, an instruction, one or more buttons and an icon.</summary>
		/// <param name="text">The text to display.</param>
		/// <param name="title">The title bar caption of the dialog.</param>
		/// <param name="instruction">The instruction shown below the main text.</param>
		/// <param name="buttons">Value that specifies which button or buttons to display.</param>
		/// <param name="icon">The icon to display.</param>
		public static Result Show(string instruction, string title, string content, TaskDialogButton buttons, TaskDialogIcon icon) {
			return InternalShow(IntPtr.Zero, title, instruction, content, buttons, icon);
		}

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        private static Result InternalShow(IntPtr parent, string title, string instruction, string content, TaskDialogButton commonButtons, TaskDialogIcon icon) {
			int dlgValue;

			try {
                //Get handle for parent window if none specified (behave like MessageBox)
                if (parent == IntPtr.Zero)
                    parent = GetActiveWindow();

                if (NativeMethods.TaskDialog(parent, IntPtr.Zero, title, instruction, content, (int)commonButtons, new IntPtr((long)icon), out dlgValue) != 0)
					throw new Exception(String.Format("Native call to {0} failed.", "TaskDialog"));
			}
			catch (EntryPointNotFoundException ex) {
				throw new Exception("Common Controls library version 6.0 not loaded. Must run on Vista and must provide a manifest.", ex);
			}
			catch (Exception ex) {
				throw new Exception("Failed to create TaskDialog.", ex);
			}

			//Convert int value to common dialog result
			if (dlgValue > 0 && dlgValue <= 8)
				return (Result)dlgValue;
			else
				return Result.None;
		}

		#endregion

	}
}
