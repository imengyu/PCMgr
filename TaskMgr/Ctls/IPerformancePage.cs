namespace PCMgr.Ctls
{
    interface IPerformancePage
    {
        void PageDelete();
        void PageShow();
        void PageHide();
        void PageUpdate();
        void PageSetGridUnit(string s);
        void PageFroceSetData(int s);
    }
}
