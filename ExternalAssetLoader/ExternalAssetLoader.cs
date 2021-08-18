using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using Newtonsoft.Json;

namespace FortySevenE.ExternalAssetLoading
{
    public class ExternalAssetLoader : MonoBehaviour
    {
        private readonly string[] _videoExtension = { ".mp4", ".mov", ".mkv", ".webm" };
        private readonly string[] _audioExtension = { ".wav", ".mp3", ".ogg", ".aac" };
        private readonly string[] _imageExtension = { ".jpg", ".png", ".tiff", ".jpeg", ".psd", ".tga" };

        public static ExternalAssetLoader Instance;
        public static event Action LocalAssetLoadingFinished;
        public static readonly string AssetKeyInstancePrefix = "[{0}]";

        public ExternalLoadedAssetCollection LocalLoadedAssetCollection { get; private set; }

        [Header("Local Assets: Will add all the files in _localAssetsFolder to the asset list with the key of their file name without ext")]
        [JsonProperty("LocalAssetsFolder")] [SerializeField] private string _localAssetsFolderPath = "[StreamingAssetsPath]/AssetLoaderLocalAssets";
        [JsonProperty("DownloadAssetsFolder")] [SerializeField] private string _downloadAssetFolderPath = "[PersistentDataPath]/AssetLoaderDownloads";

        [Header("GUI Settings")]
        [SerializeField] private AssetLoadingGUISettings _assetLoadingGUISettings;

        [Header("Assets Status (RuntimeDebug)")]
        [SerializeField] private List<ExternalAssetInfo> _localAssetList;

        [Header("Callbacks")]
        [SerializeField] private UnityEvent LocalAssetLoadingFinished_UnityEvent;


        public bool IsBusyLoading { get; private set; }

        private ExternalLoadedAssetCollection _currentLoadingAssetCollection;
        private Queue<GetAssetsRequest> _pendingLoadingRequests = new Queue<GetAssetsRequest>();
        private Coroutine _assetLoadingCoroutine;
        private UnityWebRequest _currentAssetLoadingWebRequest;
        private bool _showLoadingGUI;
        private GUIStyle _loadingStatusGUIStyle;
        private string _assetLoadingStatusString;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(Instance);
            }
        }

        private void Start()
        {
            //Setup GUI
            _loadingStatusGUIStyle = new GUIStyle();
            _loadingStatusGUIStyle.alignment = TextAnchor.LowerLeft;
            _loadingStatusGUIStyle.normal.textColor = _assetLoadingGUISettings.textColor;
            _loadingStatusGUIStyle.fontSize = 12 * Screen.width / 1080;

            //Load local assets
            LoadAssets(new GetAssetsRequest(
                _localAssetsFolderPath,
                showLoadingGUI: true,
                onCompleted: (ExternalLoadedAssetCollection loadedAssetCollection) =>
                {
                    LocalLoadedAssetCollection = loadedAssetCollection;
                    _localAssetList = new List<ExternalAssetInfo>(loadedAssetCollection.allAvailableAssets.Values);
                    LocalAssetLoadingFinished_UnityEvent?.Invoke();
                    LocalAssetLoadingFinished?.Invoke();
                }
            ));
        }

        private void Update()
        {
            if (_pendingLoadingRequests.Count > 0 && !IsBusyLoading)
            {
                ProcessLoadAssetsRequest(_pendingLoadingRequests.Dequeue());
            }
        }

        public void LoadAssets(GetAssetsRequest request)
        {
            _pendingLoadingRequests.Enqueue(request);
        }

        private void ProcessLoadAssetsRequest(GetAssetsRequest request)
        {
            IsBusyLoading = true;

            var foundAssetInfos = new List<ExternalAssetInfo>();
            #region Looking for all available assets
            string absLocalAssetsFolder = UnityPathHelper.DecodeUnityPlatformSpecificFolder(request.folderPath);
            if (Directory.Exists(absLocalAssetsFolder))
            {
                string[] assetFiles = Directory.GetFiles(absLocalAssetsFolder);
                if (assetFiles != null && assetFiles.Length > 0)
                {
                    string fileExtension = "";
                    string fileNameWithoutExtension = "";

                    foreach (string assetFileFullPath in assetFiles)
                    {
                        fileExtension = Path.GetExtension(assetFileFullPath).ToLower();

                        //Ignore meta files
                        if (fileExtension == ".meta")
                        {
                            continue;
                        }

                        fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetFileFullPath);

                        ExternalAssetInfo localAssetInfo = new ExternalAssetInfo();
                        localAssetInfo.key = fileNameWithoutExtension;
                        localAssetInfo.absPath = assetFileFullPath;
                        localAssetInfo.type = GetAssetType(assetFileFullPath);

                        foundAssetInfos.Add(localAssetInfo);
                    }
                }
            }
            #endregion

            #region Load assets
            _showLoadingGUI = request.showLoadingGUI;

            if (_assetLoadingCoroutine != null)
            {
                StopCoroutine(_assetLoadingCoroutine);
                _assetLoadingCoroutine = null;
            }

            if (_currentAssetLoadingWebRequest != null)
            {
                _currentAssetLoadingWebRequest.Abort();
                _currentAssetLoadingWebRequest = null;
            }

            _assetLoadingCoroutine = StartCoroutine(
                LoadAssetsCoroutine(
                    foundAssetInfos,
                    (ExternalLoadedAssetCollection loadedAssetCollection) =>
                    {
                        try
                        {
                            request.onCompleted?.Invoke(loadedAssetCollection);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[{GetType().Name}]Exception happened on AssetLoading callbacks: {e}");
                        }

                        CleanUpAfterAssetLoadingFinished();
                    })
                );
            #endregion
        }

        private IEnumerator LoadAssetsCoroutine(IEnumerable<ExternalAssetInfo> assetInfos, Action<ExternalLoadedAssetCollection> onCompleteCallback)
        {
            _currentLoadingAssetCollection = new ExternalLoadedAssetCollection();

            foreach (ExternalAssetInfo assetInfo in assetInfos)
            {
                if (assetInfo.type == ExternalAssetType.Unknown)
                {
                    assetInfo.type = GetAssetType(assetInfo.absPath);
                }

                if (assetInfo.type == ExternalAssetType.Unknown)
                {
                    Debug.LogWarningFormat("[{0}] Cannot identify the assset type of {1}. Skip", GetType().Name, assetInfo.key);
                    continue;
                }

                //Load assets via UnityWebRequest
                switch(assetInfo.type)
                {
                    case ExternalAssetType.Image:
                        _currentAssetLoadingWebRequest = UnityWebRequestTexture.GetTexture(assetInfo.absPath);
                        break;

                    case ExternalAssetType.Audio:
                        _currentAssetLoadingWebRequest = UnityWebRequestMultimedia.GetAudioClip(assetInfo.absPath, AudioType.UNKNOWN);
                        break;
                    case ExternalAssetType.Video:
                        _currentAssetLoadingWebRequest = UnityWebRequest.Get(assetInfo.absPath);
                        break;
                }

                yield return _currentAssetLoadingWebRequest.SendWebRequest();

                if (!string.IsNullOrEmpty(_currentAssetLoadingWebRequest.error))
                {
                    Debug.LogWarningFormat("[{0}] {1}: Failed to load <color=blue>{2}</color>", GetType().Name, _currentAssetLoadingWebRequest.error, _currentAssetLoadingWebRequest.url);
                    continue;
                }

                //If it is a network path, save the download locally
                if (!_currentAssetLoadingWebRequest.uri.IsFile)
                {
                    string downloadFolderPath = UnityPathHelper.DecodeUnityPlatformSpecificFolder(_downloadAssetFolderPath);
                    if (!Directory.Exists(downloadFolderPath))
                    {
                        Directory.CreateDirectory(downloadFolderPath);
                    }

                    string downloadedFilePath = Path.Combine(downloadFolderPath, Path.GetFileName(assetInfo.absPath));

                    File.WriteAllBytes(downloadedFilePath, _currentAssetLoadingWebRequest.downloadHandler.data);
                    assetInfo.absPath = downloadedFilePath;
                }

                switch (assetInfo.type)
                {
                    case ExternalAssetType.Image:
                        //Generate mipmap for loaded textures
                        Texture2D loadedTexture = DownloadHandlerTexture.GetContent(_currentAssetLoadingWebRequest);
                        Texture2D mipmappedTexture = new Texture2D(loadedTexture.width, loadedTexture.height, loadedTexture.format, true, true);
                        mipmappedTexture.SetPixels(loadedTexture.GetPixels());
                        mipmappedTexture.Apply();

                        if (_currentLoadingAssetCollection.preLoadedTextures.ContainsKey(assetInfo.key))
                        {
                            _currentLoadingAssetCollection.preLoadedTextures[assetInfo.key] = mipmappedTexture;
                        }
                        else
                        {
                            _currentLoadingAssetCollection.preLoadedTextures.Add(assetInfo.key, mipmappedTexture);
                        }
                        break;
                    case ExternalAssetType.Audio:
                        if (_currentLoadingAssetCollection.preLoadedAudioClips.ContainsKey(assetInfo.key))
                        {
                            _currentLoadingAssetCollection.preLoadedAudioClips[assetInfo.key] = DownloadHandlerAudioClip.GetContent(_currentAssetLoadingWebRequest);
                        }
                        else
                        {
                            _currentLoadingAssetCollection.preLoadedAudioClips.Add(assetInfo.key, DownloadHandlerAudioClip.GetContent(_currentAssetLoadingWebRequest));
                        }
                        break;
                    case ExternalAssetType.Video:
                        if (_currentLoadingAssetCollection.keyToAbsVideoPaths.ContainsKey(assetInfo.key))
                        {
                            _currentLoadingAssetCollection.keyToAbsVideoPaths[assetInfo.key] = assetInfo.absPath;
                        }
                        else
                        {
                            _currentLoadingAssetCollection.keyToAbsVideoPaths.Add(assetInfo.key, assetInfo.absPath);
                        }
                        break;
                }
                _currentLoadingAssetCollection.allAvailableAssets.Add(assetInfo.key, assetInfo);
            }

            onCompleteCallback?.Invoke(_currentLoadingAssetCollection);
            _currentLoadingAssetCollection = null;
        }

        private ExternalAssetType GetAssetType (string assetPath)
        {
            string assetExtension = Path.GetExtension(assetPath).ToLower();

            if (_videoExtension.Contains(assetExtension))
            {
                return ExternalAssetType.Video;
            }
            else if (_audioExtension.Contains(assetExtension))
            {
                return ExternalAssetType.Audio;
            }
            else if (_imageExtension.Contains(assetExtension))
            {
                return ExternalAssetType.Image;
            }

            return ExternalAssetType.Unknown;
        }

        private void CleanUpAfterAssetLoadingFinished()
        {
            IsBusyLoading = false;
            _showLoadingGUI = false;
            _assetLoadingCoroutine = null;
            _currentAssetLoadingWebRequest = null;
        }

        private void OnGUI()
        {
            if (_showLoadingGUI)
            {
                #region Loading GUI Background
                if (_assetLoadingGUISettings.background != null)
                {
                    GUI.DrawTexture(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)), _assetLoadingGUISettings.background, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: _assetLoadingGUISettings.backgroundColor, borderWidth: 0, borderRadius: 0);
                }
                else
                {
                    GUI.DrawTexture(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: _assetLoadingGUISettings.backgroundColor, borderWidth: 0, borderRadius: 0);
                }
                #endregion

                #region Loading Animation
                if (_assetLoadingGUISettings.spriteAnimationArray != null && _assetLoadingGUISettings.spriteAnimationArray.Length > 0)
                {
                    int currentFrameIndex = Mathf.FloorToInt(Time.time * 30f) % _assetLoadingGUISettings.spriteAnimationArray.Length;
                    Sprite currentFrameSprite = _assetLoadingGUISettings.spriteAnimationArray[currentFrameIndex];
                    Rect loadingAnimationPosRect = new Rect(Screen.width / 2 - currentFrameSprite.rect.width / 2, Screen.height / 2 - currentFrameSprite.rect.height / 2, currentFrameSprite.rect.width, currentFrameSprite.rect.height);
                    Rect textureUVRect = new Rect(currentFrameSprite.rect.x / currentFrameSprite.texture.width, currentFrameSprite.rect.y / currentFrameSprite.texture.height, currentFrameSprite.rect.width / currentFrameSprite.texture.width, currentFrameSprite.rect.height / currentFrameSprite.texture.height);
                    GUI.DrawTextureWithTexCoords(loadingAnimationPosRect, currentFrameSprite.texture, textureUVRect);
                }
                #endregion

                #region Loading Status

                _assetLoadingStatusString = "";

                if (_assetLoadingCoroutine != null && _currentLoadingAssetCollection != null)
                {
                    _assetLoadingStatusString += string.Format("<b>Asset Loading</b>" + Environment.NewLine + "Images: {0}" + Environment.NewLine + "Audios: {1}" + Environment.NewLine + "Videos: {2}",
                        _currentLoadingAssetCollection.preLoadedTextures.Count,
                        _currentLoadingAssetCollection.preLoadedAudioClips.Count,
                        _currentLoadingAssetCollection.keyToAbsVideoPaths.Count);

                    _assetLoadingStatusString += Environment.NewLine;

                    if (_currentAssetLoadingWebRequest != null)
                    {
                        _assetLoadingStatusString += string.Format("<color=blue>{0}</color> --- {1}", _currentAssetLoadingWebRequest.url, _currentAssetLoadingWebRequest.downloadProgress);
                    }
                }

                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), _assetLoadingStatusString, _loadingStatusGUIStyle);
                #endregion
            }
        }
    }
}
