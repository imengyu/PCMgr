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

namespace PCMgr.Aero.TaskDialog {

    public class ClickEventArgs : EventArgs {
        public ClickEventArgs(int buttonID) {
            ButtonID = buttonID;
            PreventClosing = false;
        }

        public int ButtonID { get; set; }
        public bool PreventClosing { get; set; }
    }

    public class CheckEventArgs : EventArgs {
        public CheckEventArgs(bool state) {
            IsChecked = state;
        }

        public bool IsChecked { get; set; }
    }

    public class ExpandEventArgs : EventArgs {
        public ExpandEventArgs(bool state) {
            IsExpanded = state;
        }

        public bool IsExpanded { get; set; }
    }

    public class TimerEventArgs : EventArgs {
        public TimerEventArgs(long ticks) {
            Ticks = ticks;
            ResetCount = false;
        }

        public long Ticks { get; set; }
        public bool ResetCount { get; set; }
    }

    public class HyperlinkEventArgs : EventArgs {
        public HyperlinkEventArgs(string url) {
            Url = url;
        }

        public string Url { get; set; }
    }

}
