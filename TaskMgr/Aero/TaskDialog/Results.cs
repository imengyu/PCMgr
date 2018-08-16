/*****************************************************
 *            Vista Controls for .NET 2.0
 * 
 * http://www.codeplex.com/vistacontrols
 * 
 * @author: Lorenz Cuno Klopfenstein
 * Licensed under Microsoft Community License (Ms-CL)
 * 
 *****************************************************/


namespace PCMgr.Aero.TaskDialog
{

    /// <summary>Class that aggregates the results of an "indirect" Task Dialog.</summary>
    public class Results {
        public Results(int buttonId, int radioId, bool selVerification) {
            ButtonID = buttonId;
            RadioID = radioId;
            IsVerificationChecked = selVerification;
        }

        public int ButtonID { get; set; }
        public int RadioID { get; set; }
        public bool IsVerificationChecked { get; set; }

        public Result CommonButton {
            get {
                if (ButtonID > 0 && ButtonID <= 8)
                    return (Result)ButtonID;
                else
                    return Result.None;
            }
        }
    }

    /// <summary>Results returned by Task Dialogs when closed by the user.</summary>
    public enum Result : int {
		None = 0,
		OK = 1,
		Cancel = 2,
		Abort = 3,
		Retry = 4,
		Ignore = 5,
		Yes = 6,
		No = 7,
		Close = 8
    }

}
