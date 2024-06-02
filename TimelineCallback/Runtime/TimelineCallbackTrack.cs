using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace FortySevenE.TimelineCallback
{
    [TrackClipType(typeof(TimelineCallbackAsset))]
    [TrackBindingType(typeof(Component))]
    public class TimelineCallbackTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var bindingTarget= go.GetComponent<PlayableDirector>().GetGenericBinding(this) as Component;
            var trackPlayable = ScriptPlayable<TimelineCallbackMixerBehaviour>.Create(graph, inputCount);
            foreach (var clip in GetClips())
            {
                if (clip.asset is TimelineCallbackAsset playableAsset)
                {
                    playableAsset.callbackControl.TargetBinding = bindingTarget;
                    playableAsset.TargetTimelineClip = clip;
                }
            }

            return trackPlayable;
        }
    }

    public class TimelineCallbackMixerBehaviour : PlayableBehaviour
    {
       public bool DebugLogging { get; set; }
        public bool Initialized { get; private set; }
        private Dictionary<string, List<TimelineCallbackBehaviour>> _clipControlsByFunctionName;

        void Init(Playable playable)
        {
            _clipControlsByFunctionName = new Dictionary<string, List<TimelineCallbackBehaviour>>();
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                ScriptPlayable<TimelineCallbackBehaviour> inputPlayable =
                    (ScriptPlayable<TimelineCallbackBehaviour>)playable.GetInput(i);
                TimelineCallbackBehaviour input = inputPlayable.GetBehaviour();
                input.ResetInvokeCounter();
                var functionName = input.callback.funcName;
                if (!_clipControlsByFunctionName.ContainsKey(functionName))
                {
                    _clipControlsByFunctionName.Add(functionName, new List<TimelineCallbackBehaviour>());
                }

                _clipControlsByFunctionName[functionName].Add(input);
            }

            foreach (var clipControlsPerType in _clipControlsByFunctionName)
            {
                clipControlsPerType.Value.Sort((x, y) => x.ClipEnd.CompareTo(y.ClipEnd));
            }

            Initialized = true;
        }

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
            if(!Initialized) Init(playable);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            
            double time = playable.GetTime();

            foreach (var clipControlsPerType in _clipControlsByFunctionName)
            {
                bool inTheMiddleOfNearestClipToLeft =
                    GetNearestInputToTheLeft(clipControlsPerType.Value, time, out var nearestLeftInput);
                if (nearestLeftInput is { Enabled: true, retroactive: true, EventInvokeCounter: 0 } && !inTheMiddleOfNearestClipToLeft)
                {
                    if (DebugLogging)
                    {
                        Debug.Log(
                            $"Retroactively trigger event: end time {nearestLeftInput}" +
                            $", current trigger count: {nearestLeftInput.EventInvokeCounter}");   
                    }
                    nearestLeftInput.TriggerCallback();
                }
            }
        }

        private bool GetNearestInputToTheLeft(List<TimelineCallbackBehaviour> leftToRightList,
            double targetTime, out TimelineCallbackBehaviour nearestInput)
        {
            int? nearestIndex = null;
            for (int i = leftToRightList.Count - 1; i >= 0; i--)
            {
                if (leftToRightList[i].ClipStart < targetTime)
                {
                    nearestIndex = i;
                    break;
                }
            }

            if (!nearestIndex.HasValue)
            {
                nearestInput = null;
                return false;
            }

            nearestInput = leftToRightList[nearestIndex.Value];
            return leftToRightList[nearestIndex.Value].ClipEnd > targetTime;
        }
    }   
}
