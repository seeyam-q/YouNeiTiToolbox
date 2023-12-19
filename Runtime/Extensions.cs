using System;
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

    public static class LayerMaskExtensions
    {
        //https://forum.unity.com/threads/get-the-layernumber-from-a-layermask.114553/#post-5890667
        public static int FirstSetLayer(this LayerMask mask)
        {
            int value = mask.value;
            if (value == 0) return 0;  // Early out
            for (int l = 1; l < 32; l++)
                if ((value & (1 << l)) != 0) return l;  // Bitwise
            return -1;  // This line won't ever be reached but the compiler needs it
        }
    }

    public static class CollectionExtensions
    {
        public static Dictionary<object, List<int>> _fullCycleRandomRecord;
        public static T FullCycleRandom<T>(this List<T> list)
        {
            List<int> fillValues(int count) 
            {
                List<int> filledValues = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    filledValues.Add(i);
                }
                return filledValues;
            }

            if (list == null)
            {
                throw new NullReferenceException();
            }

            if (list.Count == 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (_fullCycleRandomRecord == null)
            {
                _fullCycleRandomRecord = new Dictionary<object, List<int>>();
            }

            List<int> values;
            if (!_fullCycleRandomRecord.ContainsKey(list))
            {
                values = fillValues(list.Count);
                _fullCycleRandomRecord.Add(list, values);
            }

            _fullCycleRandomRecord.TryGetValue(list, out values);
            if (values.Count == 0)
            {
                values = fillValues(list.Count);
                _fullCycleRandomRecord[list] = values;
            }
            int randomIndex = UnityEngine.Random.Range(0, values.Count);
            int randVal = values[randomIndex];
            values.RemoveAt(randomIndex);
            return list[randVal];
        }
        public static List<T> Shuffle<T>(this IList<T> list)  
        {  
            var rng = new System.Random();
            var shuffledList = new List<T>();
            foreach (var item in list)
            {
                shuffledList.Add(item);
            }
            int n = shuffledList.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = shuffledList[k];  
                shuffledList[k] = shuffledList[n];  
                shuffledList[n] = value;  
            }

            return shuffledList;
        }
    }

    public static class RendererExtensions
    {
        private static bool UseMaterialPropertyBlock
        {
            get
            {
                bool useMaterialPropertyBlock = false;
#if URP_PRESENT || HDRP_PRESENT
                useMaterialPropertyBlock = false;
#endif
                if (!Application.isPlaying) useMaterialPropertyBlock = true;
                return useMaterialPropertyBlock;
            }
        }
        
        public static void SetTexture(this Renderer targetRenderer, string keyword, Texture texture, int materialIndex = 0)
        {
            if (UseMaterialPropertyBlock)
            {
                var properties = new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(properties);
                properties.SetTexture(GlobalHashMap.GetShaderHash(keyword), texture);
                targetRenderer.SetPropertyBlock(properties);
            }
            else
            {
                targetRenderer.materials[materialIndex].SetTexture(GlobalHashMap.GetShaderHash(keyword), texture);
            }
        }
        
        public static void SetFloat(this Renderer targetRenderer, string keyword, float floatValue, int materialIndex = 0)
        {
            if (UseMaterialPropertyBlock)
            {
                var properties = new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(properties);
                properties.SetFloat(GlobalHashMap.GetShaderHash(keyword), floatValue);
                targetRenderer.SetPropertyBlock(properties);
            }
            else
            {
                targetRenderer.materials[materialIndex].SetFloat(GlobalHashMap.GetShaderHash(keyword), floatValue);
            }
        }
        
        public static void SetVector(this Renderer targetRenderer, string keyword, Vector4 vectorValue, int materialIndex = 0)
        {
            if (UseMaterialPropertyBlock)
            {
                var properties = new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(properties);
                properties.SetVector(GlobalHashMap.GetShaderHash(keyword), vectorValue);
                targetRenderer.SetPropertyBlock(properties);
            }
            else
            {
                targetRenderer.materials[materialIndex].SetVector(GlobalHashMap.GetShaderHash(keyword), vectorValue);
            }
        }
        
        public static void SetColor(this Renderer targetRenderer, string keyword, Color colorValue, int materialIndex = 0)
        {
            if (UseMaterialPropertyBlock)
            {
                var properties = new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(properties);
                properties.SetColor(GlobalHashMap.GetShaderHash(keyword), colorValue);
                targetRenderer.SetPropertyBlock(properties);
            }
            else
            {
                targetRenderer.materials[materialIndex].SetColor(GlobalHashMap.GetShaderHash(keyword), colorValue);
            }
        }

        public static void SetMatrix(this Renderer targetRenderer, string keyword, Matrix4x4 matrixValue, int materialIndex = 0)
        {
            if (UseMaterialPropertyBlock)
            {
                var properties = new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(properties);
                properties.SetMatrix(GlobalHashMap.GetShaderHash(keyword), matrixValue);
                targetRenderer.SetPropertyBlock(properties);
            }
            else
            {
                targetRenderer.materials[materialIndex].SetMatrix(GlobalHashMap.GetShaderHash(keyword), matrixValue);
            }
        }
    }
}
