namespace PCMgr.Ctls
{
    interface IPerformancePage
    {
        void PageDelete();
        void PageShow();
        void PageHide();
        void PageUpdate();
        double PageUpdateSimple();
        void PageSetGridUnit(string s);
        void PageFroceSetData(int s);
    }
}
