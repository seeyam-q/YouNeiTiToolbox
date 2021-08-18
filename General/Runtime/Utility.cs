using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    public static class Utility
    {
        public static float Remap(float value, float from1, float to1, float from2, float to2, bool clamp = false)
        {
            if (to1 - from1 == 0)
            {
                return value;
            }

            var ratio = Ratio(value, from1, to1, clamp);
            var remappedValue = Mathf.Lerp(from2, to2, ratio);
            return remappedValue;
        }

        public static float Remap(float value, Vector4 remapRatio, bool clamp = false)
        {
            return Remap(value, remapRatio.x, remapRatio.y, remapRatio.z, remapRatio.w, clamp);
        }

        public static float Ratio(float value, float min, float max, bool clamp)
        {
            if (max - min == 0)
            {
                return 0;
            }
            
            if (clamp) value = Mathf.Clamp(value, min, max);
            float delta = value - min;
            return delta / (max - min);
        }

        public static float Ratio(float value, Vector2 range, bool clamp)
        {
            if (range.y - range.x == 0)
            {
                return 0;
            }
            
            if (clamp) value = Mathf.Clamp(value, range.x, range.y);
            float delta = value - range.x;
            return delta / (range.y - range.x);
        }
    }
}