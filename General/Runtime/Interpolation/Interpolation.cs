using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace FortySevenE
{
    [Serializable]
    public abstract class BaseInterpolation
    {
        public AnimationCurve animCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public abstract void SetAnimRatio(float ratio);

        public float CurrentAnimRatio { get; protected set; }
        public float CurrentCurvedAlpha { get; protected set; }

        protected float SampleCurve(float ratio)
        {
            CurrentAnimRatio = ratio;
            CurrentCurvedAlpha = (animCurve != null && animCurve.keys.Length > 1)
                ? animCurve.Evaluate(ratio)
                : ratio;
            return CurrentCurvedAlpha;
        }
    }

    [Serializable]
    public class ColorInterpolation : BaseInterpolation
    {
        [ColorUsage(true, true)] public Color hideColor;
        [ColorUsage(true, true)] public Color showColor;
        public CanvasGroup targetCanvasGroup;
        public Graphic targetUiGraphic;

        public override void SetAnimRatio(float ratio)
        {
            var alpha = SampleCurve(ratio);
            var currentColor = Color.Lerp(hideColor, showColor, alpha);
            if (targetUiGraphic)
            {
                targetUiGraphic.color = currentColor;
            }

            if (targetCanvasGroup)
            {
                targetCanvasGroup.alpha = currentColor.a;
            }
        }
    }

    [Serializable]
    public class ShaderInterpolation : BaseInterpolation
    {
        public bool setGlobal;
        public Renderer targetRenderer;
        public Material targetMaterial;
        public int rendererMaterialIndex;

        [Header("Color")] public string rendererColorKeyword = "";
        [ColorUsage(true, true)] public Color hideColor;
        [ColorUsage(true, true)] public Color showColor;

        [Header("Float")] public string rendererFloatKeyword = "";
        public float hideFloat;
        public float showFloat;

        [Header("Tex Sequence")] public string textureKeyword;
        public Texture[] textureSequence;

        public int CurrentTexSequenceIndex
        {
            get
            {
                if (textureSequence != null && textureSequence.Length > 0)
                {
                    return Mathf.FloorToInt((textureSequence.Length - 1) * CurrentCurvedAlpha);
                }

                return -1;
            }
        }

        public void ApplyToMaterial(Material m, float alpha)
        {
            if (!string.IsNullOrEmpty(rendererColorKeyword))
            {
                m.SetColor(
                    GlobalHashMap.GetShaderHash(rendererColorKeyword), Color.Lerp(hideColor, showColor, alpha));
            }

            if (!string.IsNullOrEmpty(rendererFloatKeyword))
            {
                var currentFloat = Mathf.Lerp(hideFloat, showFloat, alpha);
                m.SetFloat(GlobalHashMap.GetShaderHash(rendererFloatKeyword), currentFloat);
            }

            if (!string.IsNullOrEmpty(textureKeyword))
            {
                if (textureSequence != null && textureSequence.Length > 0 && textureSequence[CurrentTexSequenceIndex])
                {
                    m.SetTexture(GlobalHashMap.GetShaderHash(textureKeyword), textureSequence[CurrentTexSequenceIndex]);
                }
            }
        }

        public override void SetAnimRatio(float ratio)
        {
            var shaderAnimAlpha = SampleCurve(ratio);

            if (setGlobal)
            {
                if (!string.IsNullOrEmpty(rendererColorKeyword))
                {
                    Shader.SetGlobalColor(
                        GlobalHashMap.GetShaderHash(rendererColorKeyword),
                        Color.Lerp(hideColor, showColor, shaderAnimAlpha));
                }

                if (!string.IsNullOrEmpty(rendererFloatKeyword))
                {
                    var currentFloat = Mathf.Lerp(hideFloat, showFloat, shaderAnimAlpha);
                    Shader.SetGlobalFloat(GlobalHashMap.GetShaderHash(rendererFloatKeyword), currentFloat);
                }
            }
            else
            {
                if (targetRenderer)
                {
                    bool useMaterialPropertyBlock = false;
#if URP_PRESENT || HDRP_PRESENT
                    useMaterialPropertyBlock = false;
#endif
                    if (!Application.isPlaying) useMaterialPropertyBlock = true;

                    if (useMaterialPropertyBlock)
                    {
                        var properties = new MaterialPropertyBlock();
                        targetRenderer.GetPropertyBlock(properties);
                        if (!string.IsNullOrEmpty(rendererColorKeyword))
                        {
                            properties.SetColor(GlobalHashMap.GetShaderHash(rendererColorKeyword),
                                Color.Lerp(hideColor, showColor, shaderAnimAlpha));
                        }

                        if (!string.IsNullOrEmpty(rendererFloatKeyword))
                        {
                            var currentFloat = Mathf.Lerp(hideFloat, showFloat, shaderAnimAlpha);
                            properties.SetFloat(GlobalHashMap.GetShaderHash(rendererFloatKeyword), currentFloat);
                        }

                        if (!string.IsNullOrEmpty(textureKeyword))
                        {
                            if (textureSequence != null && textureSequence.Length > 0 &&
                                textureSequence[CurrentTexSequenceIndex])
                            {
                                properties.SetTexture(GlobalHashMap.GetShaderHash(textureKeyword),
                                    textureSequence[CurrentTexSequenceIndex]);
                            }
                        }

                        targetRenderer.SetPropertyBlock(properties);
                    }
                    else
                    {
                        var materials = targetRenderer.materials;
                        ApplyToMaterial(materials[rendererMaterialIndex % materials.Length], shaderAnimAlpha);
                    }
                }

                if (targetMaterial)
                {
                    ApplyToMaterial(targetMaterial, shaderAnimAlpha);
                }
            }
        }
    }

    [Serializable]
    public class TransformInterpolation : BaseInterpolation
    {
        public Transform target;
        public Axis posUpdateAxis = Axis.None;
        public Vector3 hideLocalPos = default;
        public Vector3 showLocalPos = default;

        public bool updateRotation = false;
        public Quaternion hideLocalRot = default;
        public Quaternion showLocalRot = default;

        public Axis scaleUpdateAxis = Axis.None;
        public Vector3 hideLocalScale = default;
        public Vector3 showLocalScale = default;

        private Vector3 UpdateVector3(Vector3 sourceValue, Vector3 targetValue, Axis updateFlags)
        {
            if (updateFlags.HasFlag(Axis.X))
            {
                sourceValue.x = targetValue.x;
            }

            if (updateFlags.HasFlag(Axis.Y))
            {
                sourceValue.y = targetValue.y;
            }

            if (updateFlags.HasFlag(Axis.Z))
            {
                sourceValue.z = targetValue.z;
            }

            return sourceValue;
        }

        public override void SetAnimRatio(float ratio)
        {
            var transformAlpha = SampleCurve(ratio);
            if (posUpdateAxis != Axis.None)
            {
                target.localPosition =
                    UpdateVector3(
                        target.localPosition,
                        Vector3.Lerp(hideLocalPos, showLocalPos, transformAlpha),
                        posUpdateAxis
                    );
            }

            if (updateRotation)
            {
                target.localRotation = Quaternion.Slerp(hideLocalRot, showLocalRot, transformAlpha);
            }

            if (scaleUpdateAxis != Axis.None)
            {
                target.localScale =
                    UpdateVector3(
                        target.localScale,
                        Vector3.Lerp(hideLocalScale, showLocalScale, transformAlpha),
                        scaleUpdateAxis
                    );
            }
        }
    }
}