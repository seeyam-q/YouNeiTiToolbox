using UnityEngine;

namespace FortySevenE.DisplayManager
{
    public class DisplayControl_UnityNative : MonoBehaviour, IDisplayControl
    {
        public void RefreshDisplayPointersAfterNewDisplayAdded()
        {

        }

        public void SetPosition(int index, DisplayWindow displayWindow)
        {
            SetPositionAndSize(index, displayWindow);
        }

        public void SetPositionAndSize(int index, DisplayWindow displayWindow)
        {
            int width = displayWindow.width;
            int height = displayWindow.height;
            if (width == 0)
            {
                width = Display.displays[index].systemWidth;
            }

            if (height == 0)
            {
                height = Display.displays[index].systemHeight;
            }

            Display.displays[index].SetParams(width, height, displayWindow.left, displayWindow.top);
        }

        public void SetSize(int index, int width, int height)
        {
            //SetPositionAndSize(index, 0, 0, true, width, height);
        }

        public void SetWindowStyle(int index, WindowStyle displayStyle)
        {
            switch(displayStyle)
            {
                case WindowStyle.Borderless:
                    Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                    break;
                case WindowStyle.FullMenuBar:
                    Screen.fullScreenMode = FullScreenMode.Windowed;     
                    break;
                case WindowStyle.MenuBarNoResize:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                case WindowStyle.FullMenuBarMinimized:
                    Debug.LogWarning(("Minimized window only works on Windows."));
                    break;
            }
        }

        public bool GetMonitorResolution(DisplayWindow displayWindow, out int width, out int height)
        {
            int index = displayWindow.relativeMonitorIndex;
            if (index >= 0 && index < Display.displays.Length)
            {
                width = Display.displays[index].systemWidth;
                height = Display.displays[index].systemHeight;
                return true;
            }
            width = 0; height = 0;
            return false;
        }
    }
}
