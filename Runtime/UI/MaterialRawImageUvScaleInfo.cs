using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FortySevenE
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RawImage))]
    public class MaterialRawImageUvScaleInfo : MonoBehaviour
    {
        [field: SerializeField] public RawImage TargetRawImage { get; private set; }
        [SerializeField] private string _shaderUvScaleKeyword = "_GraphicUvST";

        private Vector4 _cachedUvRect;
        
        private void Awake()
        {
            if (TargetRawImage == null)
            {
                TargetRawImage = GetComponent<RawImage>();
            }
        }

        private void OnEnable()
        {
            SyncUvRectToMaterial();
        }

        private void Update()
        {
            if (_cachedUvRect != UvRectToVector(TargetRawImage.uvRect))
            {
                SyncUvRectToMaterial();
            }
        }

        public void SyncUvRectToMaterial()
        {
            _cachedUvRect = UvRectToVector(TargetRawImage.uvRect);
            TargetRawImage.material.SetVector(GlobalHashMap.GetShaderHash(_shaderUvScaleKeyword), _cachedUvRect);
            TargetRawImage.enabled = false;
            TargetRawImage.enabled = true;
        }

        private Vector4 UvRectToVector(Rect rect)
        {
            return new Vector4(rect.width, rect.height, rect.x, rect.y);
        }
    }
   
}