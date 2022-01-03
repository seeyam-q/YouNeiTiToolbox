using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    public static class DictionaryExtensions
    {
		public static void CreateNewOrUpdateExisting<TKey, TValue>( this IDictionary<TKey, TValue> map, TKey key, TValue value)
		{
			map[key] = value;
		}
	}

    public static class RectTransformExtensions
    {
        // Bottom Left is (0, 0) , and Top Right is (1, 1)
        public static Vector2 ScreenSpaceToUv(this RectTransform rectTransform, Vector3 screenSpace, Camera referenceCamera = null)
        {
            Vector3[] worldSpaceRectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldSpaceRectCorners);

            var uiCamera = referenceCamera == null ? Camera.main : referenceCamera;

            var screenBottomLeft = uiCamera.WorldToScreenPoint(worldSpaceRectCorners[0]);
            var screenTopRight = uiCamera.WorldToScreenPoint(worldSpaceRectCorners[2]);

            return new Vector2(
                (screenSpace.x - screenBottomLeft.x) / (screenTopRight.x - screenBottomLeft.x), 
                (screenSpace.y - screenBottomLeft.y) / (screenTopRight.y - screenBottomLeft.y));
        }
    }

    public static class VectorExtensions
    {
        public static float GetHorizontalSqrDistance(this Vector3 vectorA, Vector3 vectorB)
        {
            var delta = (vectorB - vectorA);
            delta.y = 0;
            return delta.sqrMagnitude;
        }
    }

    //https://forum.unity.com/threads/get-the-layernumber-from-a-layermask.114553/#post-5890667
    public static class LayerMaskExtensions
    {
        public static int FirstSetLayer(this LayerMask mask)
        {
            int value = mask.value;
            if (value == 0) return 0;  // Early out
            for (int l = 1; l < 32; l++)
                if ((value & (1 << l)) != 0) return l;  // Bitwise
            return -1;  // This line won't ever be reached but the compiler needs it
        }
    }
}
