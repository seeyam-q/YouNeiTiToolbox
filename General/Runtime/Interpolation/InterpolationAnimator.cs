using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace FortySevenE
{
    public class InterpolationAnimator : MonoBehaviour
    {
        public enum AutoHideType
        {
            NoAutoHide,
            ReverseAnim,
            SnapToZero
        }
        
        [Serializable]
        public class AnimSettings
        {
            public AnimationCurve normalizeAnimCurve = AnimationCurve.Linear(0,0,1,1);
            public ColorInterpolation[] colorTargets = new ColorInterpolation[] {};
            public TransformInterpolation[] transformTargets = new TransformInterpolation[] {};
            public ShaderInterpolation[] shaderAnims = new ShaderInterpolation[]{};
        }
        
        public float revealDuration = 0.3f;
        public AnimSettings settings = new AnimSettings();

        [Header("Auto Hide")] public AutoHideType autoHide = AutoHideType.NoAutoHide;
        [Tooltip("0 or positive numbers will hide in x seconds after fully revealed")]
        public float autoHideDelay = 0f;

        public bool IsAnimating { get; private set; }
        public bool IsAnimatingToFullVisibility => IsAnimating && !_currentAnimReverse;
        public bool IsFullyRevealed => CurrentVisibilityAlpha >= 1;

        public event Action<float> alphaChanged;
        public UnityEvent onVisible;
        public UnityEvent onHidden;
        public UnityEvent onFullyRevealed;

        [field: SerializeField] public float CurrentVisibilityAlpha { get; private set; }

        [FormerlySerializedAs("activeInEditrMode")] [SerializeField]
        private bool activeInEditorMode = false;
        
        private CancellationTokenSource _animCancellationTokenSource;
        private float _currentAnimTimer;
        private bool _currentAnimReverse;

        private CancellationTokenSource _autoHideCancellationTokenSource;

        private void OnValidate()
        {
            if (activeInEditorMode && !Application.isPlaying)
            {
                CurrentVisibilityAlpha = Mathf.Clamp01(CurrentVisibilityAlpha);
                SetAnimationAlpha(CurrentVisibilityAlpha);
            }
        }

        private void Awake()
        {
            RefreshVisibility(true);
        }

        private void OnEnable()
        {
            RefreshVisibility(false);
        }

        private void RefreshVisibility(bool invokeEvents)
        {
            SetAnimationAlpha(CurrentVisibilityAlpha);
            if (CurrentVisibilityAlpha > 0)
            {
                onVisible?.Invoke();
            }
            else
            {
                onHidden?.Invoke();
            }
        }

        public void Reveal()
        {
            SetReveal(true);
        }

        public void Hide()
        {
            SetReveal(false);
        }
        
        async void AnimTick(float startTime, bool reverse, CancellationToken cancellationToken)
        {
            _currentAnimTimer = startTime;
            _currentAnimReverse = reverse;
            float animRatio()=> reverse ? (1f - (_currentAnimTimer / revealDuration)) : (_currentAnimTimer / revealDuration);
            bool shouldStop() => animRatio() >= 1 || animRatio() <= 0;
            while (!shouldStop())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                IsAnimating = true;
                await Task.Yield();

                if (reverse)
                {
                    _currentAnimTimer += Time.deltaTime;
                }
                else
                {
                    _currentAnimTimer -= Time.deltaTime;
                }
                SetAnimationAlpha(settings.normalizeAnimCurve.Evaluate(Mathf.Clamp01(animRatio())));
            }

            IsAnimating = false;
        }

        public void StopAnimation()
        {
            if (IsAnimating)
            {
                _animCancellationTokenSource?.Cancel();
            }
        }

        public void SetReveal(bool reveal)
        {
            if (!Application.isPlaying) return;
            float animStartTime = reveal ? 0 : 1;
            if (IsAnimating)
            {
                if (reveal == IsAnimatingToFullVisibility) return;
                animStartTime = _currentAnimTimer;
                StopAnimation();
            }
            else
            {
                float targetAlpha = reveal ? 1 : 0;
                if (CurrentVisibilityAlpha == targetAlpha) return;
            }
            
            _animCancellationTokenSource ??= new CancellationTokenSource();
            
            AnimTick(animStartTime, !reveal, _animCancellationTokenSource.Token);
        }

        public void ScrubAnimation(float ratio)
        {
            StopAnimation();
            SetAnimationAlpha(settings.normalizeAnimCurve.Evaluate(ratio));
        }

        protected void SetAnimationAlpha(float alpha)
        {
            foreach (var target in settings.colorTargets)
            {
                target.SetAnimRatio(alpha);
            }

            foreach (var target in settings.shaderAnims)
            {
                target.SetAnimRatio(alpha);
            }

            foreach (var transformAnim in settings.transformTargets)
            {
                transformAnim.SetAnimRatio(alpha);
            }

            // foreach (var scriptableAnim in scriptableAnims) {
            //   if (scriptableAnim.target == null) continue;
            //   var scriptableAnimAlpha = (scriptableAnim.animCurve != null && scriptableAnim.animCurve.keys.Length > 1) ? scriptableAnim.animCurve.Evaluate(alpha) : alpha;
            //   scriptableAnim.target.SetAnimAlpha(scriptableAnimAlpha);
            // }

            if (alpha > 0 && CurrentVisibilityAlpha == 0)
            {
                onVisible?.Invoke();
            }
            else if (alpha == 0 && CurrentVisibilityAlpha > 0)
            {
                onHidden?.Invoke();
            }
            else if (alpha >= 1 && CurrentVisibilityAlpha < 1)
            {
                onFullyRevealed?.Invoke();
                if (Application.isPlaying)
                {
                    _autoHideCancellationTokenSource?.Cancel();
                    if (autoHide > AutoHideType.NoAutoHide)
                    {
                        StartAutoHide();
                    }
                }
            }

            CurrentVisibilityAlpha = alpha;
            alphaChanged?.Invoke(CurrentVisibilityAlpha);
        }

        private async void StartAutoHide()
        {
            _autoHideCancellationTokenSource ??= new CancellationTokenSource();
            await Task.Delay(TimeSpan.FromSeconds(autoHideDelay), _autoHideCancellationTokenSource.Token);
            switch (autoHide)
            {
                case AutoHideType.ReverseAnim:
                    SetReveal(false);
                    break;
                case AutoHideType.SnapToZero:
                    SetAnimationAlpha(0);
                    break;
            }
        }
    }
}