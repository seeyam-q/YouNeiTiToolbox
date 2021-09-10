using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    public static class Utility
    {
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static float Ratio(float value, float min, float max, bool clamp)
        {
            float delta = value - min;
            if (clamp)
            {
                delta = Mathf.Max(delta, 0);
            }
            return delta / (max - min);
        }
    }
}