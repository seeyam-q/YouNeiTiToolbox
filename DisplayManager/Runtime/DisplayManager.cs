using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

namespace FortySevenE.DisplayManager
{
    public class DisplayManager : MonoBehaviour
    {
        public static DisplayManager Instance;

        [SerializeField] [DataMember(Name = "DisplaySettings")] DisplaySettings _displaySettings;

        public DisplayWindow[] UnityDisplayList { get { return _displaySettings.displayList; } set{ _displaySettings.displayList = value; } }

        private IDisplayControl _displayControl;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator Start()
        {
            //Unity will set the display style based on PlayerSettings. Let's wait after that so our display settings will not be overwritten
            yield return null;

            if (_displaySettings.apiType == DisplayControlApiType.UnityNative)
            {
                _displayControl = gameObject.AddComponent<DisplayControl_UnityNative>();
            }
            else
            {
#if UNITY_STANDALONE_WIN
                _displayControl = gameObject.AddComponent<DisplayControl_WindowsOS>();
                
                var monitors = DisplayControl_WindowsOS.FetchAllDisplays();
                if (monitors != null && monitors.Count > 0)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"[DisplayManager] Found {monitors.Count} connected displays via Windows API:");
                    for (int i = 0; i < monitors.Count; i++)
                    {
                        var m = monitors[i];
                        sb.AppendLine($"[{i}] Index: {m.DisplayNumber} | Name: {m.MonitorName} | Res: {m.Width}x{m.Height} at X:{m.X}, Y:{m.Y} | Primary: {m.IsPrimaryDisplay} | GPU: {m.DeviceName}");
                    }
                    Debug.Log(sb.ToString());
                }
#else
                Debug.LogError("No Implementation for the current OS, DisplayController will be destroyed");
                Destroy(this);
#endif
            }

#if !UNITY_EDITOR
            if (UnityDisplayList.Length > 0)
            {
                _displayControl.RefreshDisplayPointersAfterNewDisplayAdded();

                if (UnityDisplayList[0].autoSizeToMonitor)
                {
                    if (_displayControl.GetMonitorResolution(UnityDisplayList[0], out int mWidth, out int mHeight))
                    {
                        UnityDisplayList[0].width = mWidth;
                        UnityDisplayList[0].height = mHeight;
                    }
                }

                Screen.SetResolution(UnityDisplayList[0].width, UnityDisplayList[0].height, FullScreenMode.Windowed);
                yield return new WaitForEndOfFrame(); // Wait for Unity's internal swapchain to resize

                SetDisplayWindowStyle(0, UnityDisplayList[0].windowStyle);
                _displayControl.SetPositionAndSize(0, UnityDisplayList[0]);

                //Wait for the main display to be set up
                yield return null;

                // Activate more displays if available
                if (UnityDisplayList.Length > 1)
                {
                    for (int i = 1; i < UnityDisplayList.Length; i++)
                    {
                        if (i < Display.displays.Length)
                        {
                            Debug.Log ($"Display[{i}] - ({Display.displays[i].systemWidth}, {Display.displays[i].systemHeight})");
                            
                            // Ask Unity to activate more displays at the desired size
                            var displayWidth = UnityDisplayList[i].width;
                            var displayHeight = UnityDisplayList[i].height;
                            
                            if (UnityDisplayList[i].autoSizeToMonitor) 
                            {
                                if (_displayControl.GetMonitorResolution(UnityDisplayList[i], out int mWidth, out int mHeight))
                                {
                                    displayWidth = mWidth;
                                    displayHeight = mHeight;
                                    UnityDisplayList[i].width = mWidth;
                                    UnityDisplayList[i].height = mHeight;
                                }
                            }
                            Display.displays[i].Activate(displayWidth, displayHeight, 0);

                            if (_displaySettings.resizeMultiDisplays)
                            {
                                yield return null;
                                _displayControl.RefreshDisplayPointersAfterNewDisplayAdded();
                                yield return null;
                                SetDisplayWindowStyle(i, UnityDisplayList[i].windowStyle);
                                yield return null;
                                _displayControl.SetPositionAndSize(i, UnityDisplayList[i]);

                            }
                        }
                    }

                    yield return null;
#if UNITY_STANDALONE_WIN
                    if (_displaySettings.apiType == DisplayControlApiType.OSSpecific)
                    {
                        // Activating more displays will mess up the main display for some reasons, so let's set up the main display again
                        yield return new WaitForSeconds(0.1f); // Ensures Windows DPICHANGED events settle before final lock
                        SetDisplayWindowStyle(0, UnityDisplayList[0].windowStyle);
                        _displayControl.SetPositionAndSize(0, UnityDisplayList[0]);
                    }
#endif 
                }
            }
#endif
        }

        public void SetDisplayPos (int index, int left, int top, bool relativeToMonitor = true, int relativeMonitorIndex = 0)
        {
            DisplayWindow win = new DisplayWindow(left, top, 0, 0, WindowStyle.Borderless);
            win.relativeMonitorIndex = relativeMonitorIndex;
            _displayControl.SetPosition(index, win);
        }

        public void SetDisplaySize (int index, int width, int height)
        {
            _displayControl.SetSize(index, width, height);
        }

        public void SetPositionAndSize (int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex,  int width, int height)
        {
            DisplayWindow win = new DisplayWindow(left, top, width, height, WindowStyle.Borderless);
            win.relativeMonitorIndex = relativeMonitorIndex;
            _displayControl.SetPositionAndSize(index, win);
        }
        
        public void SetPositionAndSize (int index, DisplayWindow displayWindow)
        {
            _displayControl.SetPositionAndSize(index, displayWindow);
        }

        public void SetDisplayWindowStyle (int index, WindowStyle displayStyle)
        {
            _displayControl.SetWindowStyle(index, displayStyle);
        }
    }
}
