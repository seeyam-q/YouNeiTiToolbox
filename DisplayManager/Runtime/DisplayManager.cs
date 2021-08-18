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
#else
                Debug.LogError("No Implementation for the current OS, DisplayController will be destroyed");
                Destroy(this);
#endif
            }

#if !UNITY_EDITOR
            if (UnityDisplayList.Length > 0)
            {
                _displayControl.RefreshDisplayPointersAfterNewDisplayAdded();

                SetDisplayWindowStyle(0, UnityDisplayList[0].windowStyle);
                SetPositionAndSize(0, UnityDisplayList[0].left, UnityDisplayList[0].top, true, relativeMonitorIndex: UnityDisplayList[0].relativeMonitorIndex,  UnityDisplayList[0].width, UnityDisplayList[0].height);

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
                            Display.displays[i].Activate(displayWidth, displayHeight, 0);

                            if (_displaySettings.resizeMultiDisplays)
                            {
                                yield return null;
                                _displayControl.RefreshDisplayPointersAfterNewDisplayAdded();
                                yield return null;
                                SetDisplayWindowStyle(i, UnityDisplayList[i].windowStyle);
                                yield return null;
                                SetPositionAndSize(i, UnityDisplayList[i].left, UnityDisplayList[i].top, true, relativeMonitorIndex: UnityDisplayList[i].relativeMonitorIndex, displayWidth, displayHeight);

                            }
                        }
                    }

                    yield return null;
#if UNITY_STANDALONE_WIN
                    if (_displaySettings.apiType == DisplayControlApiType.OSSpecific)
                    {
                        // Activating more displays will mess up the main display for some reasons, so let's set up the main display again
                        SetDisplayWindowStyle(0, UnityDisplayList[0].windowStyle);
                        SetPositionAndSize(0, UnityDisplayList[0].left, UnityDisplayList[0].top, true,
                            relativeMonitorIndex: UnityDisplayList[0].relativeMonitorIndex, UnityDisplayList[0].width,
                            UnityDisplayList[0].height);
                    }
#endif 
                }
            }
#endif
        }

        public void SetDisplayPos (int index, int left, int top, bool relativeToMonitor = true, int relativeMonitorIndex = 0)
        {
            _displayControl.SetPosition(index, left, top, relativeToMonitor, relativeMonitorIndex);
        }

        public void SetDisplaySize (int index, int width, int height)
        {
            _displayControl.SetSize(index, width, height);
        }

        public void SetPositionAndSize (int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex,  int width, int height)
        {
            _displayControl.SetPositionAndSize(index, left, top, relativeToMonitor, relativeMonitorIndex, width, height);
        }

        public void SetDisplayWindowStyle (int index, WindowStyle displayStyle)
        {
            _displayControl.SetWindowStyle(index, displayStyle);
        }
    }
}
