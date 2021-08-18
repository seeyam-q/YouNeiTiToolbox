# External AssetLoader

Provides a unifying way to download and load external media assets (audio, image, video) to your Unity project.
Unity may have an official package [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@1.19/manual/index.html) that does the similar thing. But I personally haven't tested it yet.

### Install

1. Install dependency: https://github.com/seeyam-q/YouNeiTiToolbox.git#upm-youneiti-toolbox
2. In the Unity editor, `Window->Package Manager` add package from git url: https://github.com/seeyam-q/YouNeiTiToolbox.git#upm-external-asset-loader

### Usage

1. Create an empty `GameObject` in the main scene and add `ExternalAssetLoader` component to the object.
2. Change `LocalAssetsFolderPath` to suit the need of the project.
3. On startup, all media files under `LocalAssetsFolderPath` will be loaded and `AssetController.LocalAssetLoadingFinished` will be called when it's done. Audio files will be loaded as `AudioClip` and image files will be loaded as `Texture2D`. Video files will only be loaded as their absolute file path. The filename without the extension will be used as the `AssetKey` to retrieve the asset.
4. You can access the loaded asset via the functions below:

```c#
ExternalAssetLoader.Instance.AllAvailableAssets[assetkey]
ExternalAssetLoader.Instance.PreLoadedImagesAssets[assetKey]
ExternalAssetLoader.Instance.PreLoadedAudioClips[assetKey]
ExternalAssetLoader.Instance.DownloadedVideoPaths[assetKey]
```