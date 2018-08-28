namespace PCMgr.Ctls
{
    interface IPerformancePage
    {
        bool PageIsActive { get; set; }
        void PageDelete();
        void PageShow();
        void PageHide();
        void PageUpdate();
        bool PageUpdateSimple(out string customString, out int outdata1, out int outdata2);
        void PageSetGridUnit(string s);
        void PageFroceSetData(int s);
    }
}
