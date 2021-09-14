using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FortySevenE.DisplayManager
{
    public class DisplayControl_UnityNative : MonoBehaviour, IDisplayControl
    {
        Dictionary<int, DisplayWindow> _cacheDisplaySettingsAfterReload;

        public void RefreshDisplayPointersAfterNewDisplayAdded()
        {

        }

        public void SetPosition(int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex)
        {
            SetPositionAndSize(index, left, top, true, relativeMonitorIndex, 0, 0);
        }

        public void SetPositionAndSize(int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex, int width, int height)
        {
            UnityEngine.Display.displays[index].SetParams(width, height, left, top);
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
            }
        }
    }
}
