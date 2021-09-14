using System;
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
        FullMenuBar
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
        // resize multi displays breaks Unity's UI event system (I love Unity!). Making it optional incase UI interaction is needed on multi-display
        public bool resizeMultiDisplays;
        public DisplayWindow[] displayList;
    }
}