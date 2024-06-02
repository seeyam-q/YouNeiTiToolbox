using System;
using MS.Events;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace FortySevenE.TimelineCallback
{
    public class TimelineCallbackAsset : PlayableAsset, ITimelineClipAsset
    {
        public TimelineCallbackBehaviour callbackControl = default;
    
        public TimelineClip TargetTimelineClip { get; set; }
    
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            callbackControl.TargetTimelineClip = TargetTimelineClip;
            var playable = ScriptPlayable<TimelineCallbackBehaviour>.Create(graph, callbackControl);
            return playable;
        }

        public string GetDisplayName()
        {
            return callbackControl.callback.funcName;
        }
        public ClipCaps clipCaps => ClipCaps.Blending;
    }

    [Serializable]
    public class TimelineCallbackBehaviour : PlayableBehaviour
    {
        public bool retroactive;
        public bool onlyTriggerOnce;
        public TimelineSerializableCallback callback = new TimelineSerializableCallback();
        public Component TargetBinding { get; set; }
        public TimelineClip TargetTimelineClip { get; set; }
        public double ClipStart => TargetTimelineClip.start;
        public double ClipEnd => TargetTimelineClip.end;
        public int EventInvokeCounter { get; private set; }
        
        public bool Enabled => callback.CanInvoke() && !(onlyTriggerOnce && EventInvokeCounter > 0);


        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
            callback.target = TargetBinding;
        }
        
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            //Debug.Log($"FrameID{info.frameId}, Time:{playable.GetTime():0.000}, DeltaTime{info.deltaTime:0.000}");
            // Only invoke if time has passed to avoid invoking
            // repeatedly after resume
            if ((info.frameId == 0) || (info.deltaTime > 0) || playable.GetTime()<=0)
            {
                TriggerCallback();
            }
        }

        public void TriggerCallback()
        {
            callback.Invoke(SerializableEvent.EMPTY_ARGS);
            EventInvokeCounter++;
        }
        
        public void ResetInvokeCounter()
        {
            EventInvokeCounter = 0;
        }
    }
   
}