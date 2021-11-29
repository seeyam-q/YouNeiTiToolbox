using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FortySevenE.ExternalAssetLoading
{
    public enum ExternalAssetType
    {
        Unknown,
        Image,
        Video,
        Audio,
        Text
    }

    [Serializable]
    public class ExternalAssetInfo
    {
        [JsonRequired] public string key;
        [HideInInspector] [JsonIgnore] public ExternalAssetType type;
        [JsonRequired] public string absPath;
    }

    public class LoadAssetsRequest
    {
        public string folderPath;
        public bool showLoadingGUI = false;
        public Action<ExternalLoadedAssetCollection> onCompleted;

        public LoadAssetsRequest()
        {

        }

        public LoadAssetsRequest(string folderPath, bool showLoadingGUI, Action<ExternalLoadedAssetCollection> onCompleted)
        {
            this.folderPath = folderPath;
            this.showLoadingGUI = showLoadingGUI;
            this.onCompleted = onCompleted;
        }

        public override string ToString()
        {
            return folderPath;
        }
    }

    public class ExternalLoadedAssetCollection
    {
        public Dictionary<string, ExternalAssetInfo> allAvailableAssets = new Dictionary<string, ExternalAssetInfo>();
        public Dictionary<string, Texture2D> preLoadedTextures = new Dictionary<string, Texture2D>();
        public Dictionary<string, AudioClip> preLoadedAudioClips = new Dictionary<string, AudioClip>();
        public Dictionary<string, string> preLoadedTexts = new Dictionary<string, string>();
        public Dictionary<string, string> keyToAbsVideoPaths = new Dictionary<string, string>();
    }
}