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
        Audio
    }

    [Serializable]
    public class ExternalAssetInfo
    {
        [JsonRequired] public string key;
        [HideInInspector] [JsonIgnore] public ExternalAssetType type;
        [JsonRequired] public string absPath;
    }

    public class GetAssetsRequest
    {
        public string folderPath;
        public bool showLoadingGUI;
        public Action<ExternalLoadedAssetCollection> onCompleted;

        public GetAssetsRequest()
        {

        }

        public GetAssetsRequest(string folderPath, bool showLoadingGUI, Action<ExternalLoadedAssetCollection> onCompleted)
        {
            this.folderPath = folderPath;
            this.showLoadingGUI = showLoadingGUI;
            this.onCompleted = onCompleted;
        }
    }

    public class ExternalLoadedAssetCollection
    {
        public Dictionary<string, ExternalAssetInfo> allAvailableAssets = new Dictionary<string, ExternalAssetInfo>();
        public Dictionary<string, Texture2D> preLoadedTextures = new Dictionary<string, Texture2D>();
        public Dictionary<string, AudioClip> preLoadedAudioClips = new Dictionary<string, AudioClip>();
        public Dictionary<string, string> keyToAbsVideoPaths = new Dictionary<string, string>();
    }
}