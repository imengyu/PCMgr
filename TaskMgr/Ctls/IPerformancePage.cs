using System.Drawing;

namespace PCMgr.Ctls
{
    public delegate void OpeningPageMenuEventHandler(IPerformancePage sender, System.Windows.Forms.ToolStripMenuItem menuItemView);
    public delegate void SwithGraphicViewEventHandler(IPerformancePage sender);

    public interface IPerformancePage
    {
        bool PageIsActive { get; set; }
        bool PageIsGraphicMode { get; set; }

        void PageInit();
        void PageDelete();
        void PageShow();
        void PageHide();
        void PageUpdate();
        bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2);
        void PageSetGridUnit(string s);
        void PageFroceSetData(int s);

        event SwithGraphicViewEventHandler SwithGraphicView;
        event OpeningPageMenuEventHandler OpeningPageMenu;

        System.Windows.Forms.Panel GridPanel { get; }
        Size Size { get; set; }
    }
}
