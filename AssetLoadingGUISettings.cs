using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE.ExternalAssetLoading
{
    [Serializable]
    public class AssetLoadingGUISettings
    {
        public Sprite[] spriteAnimationArray;
        public Color textColor = Color.black;
        public Texture2D background;
        public Color backgroundColor = Color.black;
    }
}