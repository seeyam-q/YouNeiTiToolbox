#if TIMELINE_PRESENT
using System;
using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;

namespace FortySevenE
{
    [Serializable]
    public class TimeMachineBehaviour : PlayableBehaviour
    {
        public TimeMachineAction action;
        public Condition condition;
        public string markerToJumpTo, markerLabel;
        public float timeToJumpTo;
        public Component targetComponent;
        public FieldInfo customBooleanFieldInfo;

        [HideInInspector] public bool clipExecuted = false; //the user shouldn't author this, the Mixer does

        public bool ConditionMet()
        {
            switch (condition)
            {
                case Condition.Always:
                    return true;

                case Condition.CustomBooleanField:
                    return GetCustomBooleanValue();

                case Condition.Never:
                default:
                    return false;
            }
        }

        public bool GetCustomBooleanValue()
        {
            if (targetComponent != null && customBooleanFieldInfo != null)
            {
                var fieldValue = customBooleanFieldInfo.GetValue(targetComponent);
                if (fieldValue is bool booleanValue)
                {
                    return booleanValue;
                }
            }

            Debug.Log($"target: {targetComponent} - customBooleanField: {customBooleanFieldInfo}");

            return false;
        }

        public enum TimeMachineAction
        {
            Marker,
            JumpToTime,
            JumpToMarker,
            Pause,
        }

        public enum Condition
        {
            Always,
            Never,
            CustomBooleanField,
        }
    }
}
#endif