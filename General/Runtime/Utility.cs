using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    public static class Utility
    {
        public static float Remap(float value, float from1, float to1, float from2, float to2, bool clamp = false)
        {
            var remappedValue = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            if (clamp)
            {
                remappedValue = Mathf.Clamp(remappedValue, from2, to2);
            }

            return remappedValue;
        }

        public static float Remap(float value, Vector4 remapRatio, bool clamp = false)
        {
            return Remap(value, remapRatio.x, remapRatio.y, remapRatio.z, remapRatio.w, clamp);
        }

        public static float Ratio(float value, float min, float max, bool clamp)
        {
            if (clamp) value = Mathf.Clamp(value, min, max);
            float delta = value - min;
            return delta / (max - min);
        }

        public static float Ratio(float value, Vector2 range, bool clamp)
        {
            if (clamp) value = Mathf.Clamp(value, range.x, range.y);
            float delta = value - range.x;
            return delta / (range.y - range.x);
        }
    }
}