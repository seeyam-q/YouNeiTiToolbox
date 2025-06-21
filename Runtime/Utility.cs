using System;
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
        
        public static T Lerp<T>(T start, T end, float t) where T : struct
        {
            t = Mathf.Clamp01(t);

            if (typeof(T) == typeof(float))
            {
                // We must cast to object first, then to the specific type.
                // This is how you work around C#'s strict generic type safety.
                float s = (float)(object)start;
                float e = (float)(object)end;
                return (T)(object)Mathf.Lerp(s, e, t);
            }

            if (typeof(T) == typeof(Vector2))
            {
                Vector2 s = (Vector2)(object)start;
                Vector2 e = (Vector2)(object)end;
                return (T)(object)Vector2.Lerp(s, e, t);
            }

            if (typeof(T) == typeof(Vector3))
            {
                Vector3 s = (Vector3)(object)start;
                Vector3 e = (Vector3)(object)end;
                return (T)(object)Vector3.Lerp(s, e, t);
            }

            if (typeof(T) == typeof(Vector4))
            {
                Vector4 s = (Vector4)(object)start;
                Vector4 e = (Vector4)(object)end;
                return (T)(object)Vector4.Lerp(s, e, t);
            }

            if (typeof(T) == typeof(Color))
            {
                Color s = (Color)(object)start;
                Color e = (Color)(object)end;
                return (T)(object)Color.Lerp(s, e, t);
            }
            
            if (typeof(ILerpable<T>).IsAssignableFrom(typeof(T)))
            {
                // Cast the 'start' value to the interface.
                // The 'start' instance itself will perform the lerp.
                ILerpable<T> s = (ILerpable<T>)start;
            
                // Call the interface method, passing 'end' as the target.
                return s.Lerp(start, end, t);
            }

            // It's better to throw an exception for unsupported types than to fail silently.
            throw new NotSupportedException($"The type '{typeof(T).Name}' is not supported by GenericLerp.");
        }
    }
    
    public interface ILerpable<T>
    {
        T Lerp(T start, T end, float amount);
    }
}