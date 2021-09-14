namespace FortySevenE.DisplayManager
{
    public interface IDisplayControl
    {
        void SetPositionAndSize(int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex, int width, int height);
        void SetPosition(int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex);
        void SetSize(int index, int width, int height);
        void SetWindowStyle(int index, WindowStyle displayStyle);
        void RefreshDisplayPointersAfterNewDisplayAdded();
    }
}
