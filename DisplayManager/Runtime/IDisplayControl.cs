namespace FortySevenE.DisplayManager
{
    public interface IDisplayControl
    {
        void SetPositionAndSize(int index, DisplayWindow displayWindow);
        void SetPosition(int index, DisplayWindow displayWindow);
        void SetSize(int index, int width, int height);
        void SetWindowStyle(int index, WindowStyle displayStyle);
        void RefreshDisplayPointersAfterNewDisplayAdded();
        bool GetMonitorResolution(DisplayWindow displayWindow, out int width, out int height);
    }
}
