using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  FortySevenE
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class AnchoredToRectTransform : MonoBehaviour
	{
		public RectTransform targetRectTransform;
		public Vector2 posRatio;
		public Vector2 posOffset = Vector2.zero;
		public bool syncSize = false;
		public Vector2 syncSizeRatio = Vector2.one;

		public RectTransform RectTransform { get; private set; }

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
		}

		private void OnValidate()
		{
			Awake();
		}

		// Update is called once per frame
		void Update()
		{
			if (targetRectTransform != null)
			{
				if (syncSize)
				{
					var rect = RectTransform.rect;
					rect.width = targetRectTransform.rect.width * syncSizeRatio.x;
					rect.height = targetRectTransform.rect.height * syncSizeRatio.y;
					RectTransform.sizeDelta = rect.size;
				}

				RectTransform.anchorMin = targetRectTransform.anchorMin;
				RectTransform.anchorMax = targetRectTransform.anchorMax;
				RectTransform.anchoredPosition = targetRectTransform.anchoredPosition + posOffset +
				                                 new Vector2(targetRectTransform.rect.width * (posRatio.x - targetRectTransform.pivot.x) * targetRectTransform.localScale.x,
					                                 targetRectTransform.rect.height * (posRatio.y - targetRectTransform.pivot.y) * targetRectTransform.localScale.y);
			}
		}
	}
	
}
