using System;
using System.Collections;
using System.Collections.Generic;
using MS.Events;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace FortySevenE.TimelineCallback
{
    [CustomTimelineEditor(typeof(TimelineCallbackAsset))]
    public class TimelineCallbackPlayableAssetEditor : ClipEditor
    {
        private GUIStyle _extrapolateSignGuiStyle = new GUIStyle();
        readonly float _extrapolateSignWidth = 16f;
        private readonly float _extrapolateSignHeight = 4f;

        public override void OnClipChanged(TimelineClip clip)
        {
            base.OnClipChanged(clip);
            var timelineEventClip = clip.asset as TimelineCallbackAsset;
            if (timelineEventClip != null)
            {
                clip.displayName = timelineEventClip.GetDisplayName();   
            }
        }
        
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            base.DrawBackground(clip, region);
        
            _extrapolateSignGuiStyle.fontStyle = FontStyle.Bold;
            _extrapolateSignGuiStyle.alignment = TextAnchor.UpperCenter;
            _extrapolateSignGuiStyle.normal.textColor = Color.white;
            _extrapolateSignGuiStyle.fontSize = 14;

            if (clip.asset is TimelineCallbackAsset normTimeControlClip)
            {
                if (normTimeControlClip.callbackControl.retroactive)
                {
                    var extrapolateSignRect = new Rect(region.position.max.x + 1 - _extrapolateSignWidth,
                        region.position.min.y - _extrapolateSignHeight,
                        _extrapolateSignWidth, _extrapolateSignHeight);
                    EditorGUI.LabelField(extrapolateSignRect, "∞", _extrapolateSignGuiStyle);
                } 
            }
        }

        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var drawOptions = base.GetClipOptions(clip);
            var timelineEventClip = clip.asset as TimelineCallbackAsset;
            if (timelineEventClip != null)
            {
                switch (timelineEventClip.callbackControl.callback.callState)
                {
                    case CallState.Off:
                        drawOptions.highlightColor = new Color(0.5f, 0, 0);
                        break;
                    case CallState.EditorAndRuntime:
                        drawOptions.highlightColor = new Color(0,0.5f,0f) ;
                        break;
                    case CallState.RuntimeOnly:
                        drawOptions.highlightColor = new Color(0.4448276f, 0f, 1f);
                        break;
                }
            }

            return drawOptions;
        }
    }   
}
