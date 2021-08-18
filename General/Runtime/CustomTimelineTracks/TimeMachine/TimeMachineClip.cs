#if TIMELINE_PRESENT
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace FortySevenE
{
	[Serializable]
	public class TimeMachineClip : PlayableAsset, ITimelineClipAsset
	{
		[HideInInspector] public TimeMachineBehaviour template = new TimeMachineBehaviour();

		public TimeMachineBehaviour.TimeMachineAction action;
		public TimeMachineBehaviour.Condition condition;
		public string markerToJumpTo = "", markerLabel = "";
		public float timeToJumpTo = 0f;
		public string customComponentName = "";
		public string customBooleanFieldName = "";
		public ExposedReference<GameObject> target;

		public ClipCaps clipCaps
		{
			get { return ClipCaps.None; }
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<TimeMachineBehaviour>.Create(graph, template);
			TimeMachineBehaviour clone = playable.GetBehaviour();
			var targetObject = target.Resolve(graph.GetResolver());
			clone.markerToJumpTo = markerToJumpTo;
			clone.action = action;
			clone.condition = condition;
			clone.markerLabel = markerLabel;
			clone.timeToJumpTo = timeToJumpTo;
			var targetComponent = targetObject?.GetComponent(customComponentName);
			FieldInfo targetFieldInfo = null;
			if (targetComponent != null)
			{
				foreach (FieldInfo fieldInfo in targetComponent.GetType()
					         .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
				{
					if (fieldInfo.Name == customBooleanFieldName)
					{
						targetFieldInfo = fieldInfo;
						break;
					}
				}

				if (targetFieldInfo != null)
				{
					clone.targetComponent = targetComponent;
					clone.customBooleanFieldInfo = targetFieldInfo;
				}
			}

			return playable;
		}
	}
}
#endif