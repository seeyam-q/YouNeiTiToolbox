using System;
using UnityEngine;
using System.Runtime.Serialization;

namespace FortySevenE.DisplayManager
{
    public enum DisplayControlApiType
    {
        OSSpecific,
        UnityNative
    }

    public enum WindowStyle
    {
        Borderless,
        MenuBarNoResize,
        FullMenuBar,
        FullMenuBarMinimized
    }

	[Serializable]
	public class DisplayWindow
	{
        public int relativeMonitorIndex;
        public WindowStyle windowStyle;

		public int left;
		public int top;
		public int width;
		public int height;

        public DisplayWindow(int left, int top, int width, int height, WindowStyle windowStyle)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
            this.windowStyle = windowStyle;
        }
    }

    [Serializable]
    public class DisplaySettings 
    {
        public DisplayControlApiType apiType;
        [Tooltip("Multi-display runs in full screen by default. Resizing them breaks Unity's UI event system in some versions of Unity.")]
        public bool resizeMultiDisplays;
        public DisplayWindow[] displayList;
    }
}