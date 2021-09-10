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
}
