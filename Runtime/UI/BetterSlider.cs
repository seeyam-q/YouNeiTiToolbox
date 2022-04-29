using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace FortySevenE
{
    [RequireComponent(typeof(Slider))]
    public class BetterSlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        public Slider Slider { get; private set; }
#if TMP_PRESENT
        [SerializeField] TMP_Text _handleValueLabel = default;
        [SerializeField] string _handleValueToStringFormat = "";
#endif


        public bool IsBeingPressed { get; private set; }

        public event Action<float> ValueChanged;

        private float _cacheSliderValue = -1;

        private void Awake()
        {
            Slider = GetComponent<Slider>();
        }

        private void Update()
        {
            if (Slider.value == _cacheSliderValue)
            {
                return;
            }

            ValueChanged?.Invoke(Slider.value);
            _cacheSliderValue = Slider.value;
#if TMP_PRESENT
            if (_handleValueLabel != null)
            {
                _handleValueLabel.text = _cacheSliderValue.ToString(_handleValueToStringFormat);
            }
#endif
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsBeingPressed = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            IsBeingPressed = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsBeingPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsBeingPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsBeingPressed = false;
        }
    }
}