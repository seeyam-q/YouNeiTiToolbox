using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FortySevenE
{
    public interface ITexturePropertyProvider
    {
        Texture CurrentTexture { get; }
    }

    public interface IFloatPropertyProvider
    {
        float FloatMaterialProperty { get; }
    }

    public interface IVectorPropertyProvider
    {
        Vector4 VectorMaterialProperty { get; }
    }

    public class MaterialPropertyApplier : MonoBehaviour
    {
        [Serializable]
        internal class RendererPropertyPair
        {
            public Renderer renderer;
            public string property;
        }

        [Serializable]
        internal class TexturePropertyApplier
        {
            [SerializeField] internal MonoBehaviour _iTextureProvider;
            public RendererPropertyPair[] targetRenderers;

            public ITexturePropertyProvider TextureProvider => _textureProvider != null ? _textureProvider : _iTextureProvider.GetComponent<ITexturePropertyProvider>();
            internal ITexturePropertyProvider _textureProvider;
        }

        [Serializable]
        internal class FloatPropertyApplier
        {
            [SerializeField] internal MonoBehaviour _iFloatProvider;
            public RendererPropertyPair[] targetRenderers;


            public IFloatPropertyProvider FloatProvider => _floatProvider != null ? _floatProvider : _iFloatProvider.GetComponent<IFloatPropertyProvider>();
            internal IFloatPropertyProvider _floatProvider;
        }

        [Serializable]
        internal class VectorPropertyApplier
        {
            [SerializeField] internal MonoBehaviour _iVectorProvider;
            public RendererPropertyPair[] targetRenderers;

            public IVectorPropertyProvider VectorProvider => _vectorProvider != null ? _vectorProvider : _iVectorProvider.GetComponent<IVectorPropertyProvider>();
            internal IVectorPropertyProvider _vectorProvider;
        }

        [SerializeField] TexturePropertyApplier[] _textureAppliers;
        [SerializeField] FloatPropertyApplier[] _floatApplier;
        [SerializeField] VectorPropertyApplier[] _vectorApplier;

        private MaterialPropertyBlock _tempPropertyBlock;

        void Update()
        {
            if (_tempPropertyBlock == null) { _tempPropertyBlock = new MaterialPropertyBlock(); }

            foreach (var textureApplier in _textureAppliers)
            {
                var texture = textureApplier.TextureProvider?.CurrentTexture;
                foreach (var targetRenderer in textureApplier.targetRenderers)
                {
                    if (texture != null) 
                    {
                        _tempPropertyBlock.Clear();
                        targetRenderer.renderer.GetPropertyBlock(_tempPropertyBlock);
                        _tempPropertyBlock.SetTexture(GlobalHashMap.GetShaderHash(targetRenderer.property), texture);
                        targetRenderer.renderer.SetPropertyBlock(_tempPropertyBlock);
                    }
                }
            }

            foreach (var floatApplier in _floatApplier)
            {
                var floatValue = floatApplier.FloatProvider?.FloatMaterialProperty;
                foreach (var targetRenderer in floatApplier.targetRenderers)
                {
                    if (floatValue != null) 
                    {
                        _tempPropertyBlock.Clear();
                        targetRenderer.renderer.GetPropertyBlock(_tempPropertyBlock);
                        _tempPropertyBlock.SetFloat(GlobalHashMap.GetShaderHash(targetRenderer.property), floatValue.Value);
                        targetRenderer.renderer.SetPropertyBlock(_tempPropertyBlock);
                    }
                }
            }

            foreach (var vectorApplier in _vectorApplier)
            {
                var vectorValue = vectorApplier.VectorProvider?.VectorMaterialProperty;
                foreach (var targetRenderer in vectorApplier.targetRenderers)
                {
                    if (vectorValue != null) 
                    {
                        _tempPropertyBlock.Clear();
                        targetRenderer.renderer.GetPropertyBlock(_tempPropertyBlock);
                        _tempPropertyBlock.SetVector(GlobalHashMap.GetShaderHash(targetRenderer.property), vectorValue.Value);
                        targetRenderer.renderer.SetPropertyBlock(_tempPropertyBlock);
                    }
                }
            }
        }

        public bool ValidateInterfaceReferences()
        {
            bool valid = true;
            foreach (var applier in _textureAppliers)
            {
                if (applier._iTextureProvider!= null && applier._iTextureProvider.GetComponent<ITexturePropertyProvider>() == null)
                {
                    Debug.LogWarning($"{applier._iTextureProvider} does not implement ITextureProvider");
                    applier._textureProvider = null;
                    applier._iTextureProvider = null;
                    valid = false;
                }
            }

            foreach (var applier in _floatApplier)
            {
                if (applier._iFloatProvider != null && applier._iFloatProvider.GetComponent<ITexturePropertyProvider>() == null)
                {
                    Debug.LogWarning($"{applier._iFloatProvider} does not implement IFloatProvider");
                    applier._floatProvider = null;
                    applier._iFloatProvider = null;
                    valid = false;
                }
            }

            foreach (var applier in _vectorApplier)
            {
                if (applier._iVectorProvider != null && applier._iVectorProvider.GetComponent<ITexturePropertyProvider>() == null)
                {
                    Debug.LogWarning($"{applier._iVectorProvider} does not implement IVectorProvider");
                    applier._vectorProvider = null;
                    applier._iVectorProvider = null;
                    valid = false;
                }
            }

            return valid;
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(MaterialPropertyApplier))]
    public class MaterialPropertyApplierInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as MaterialPropertyApplier;

            base.OnInspectorGUI();
            if (!script.ValidateInterfaceReferences())
            {
                base.OnInspectorGUI();
            }
        }
    }
    #endif
}
