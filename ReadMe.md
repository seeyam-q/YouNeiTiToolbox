# YouNeTi Toolbox

A collection of handy tools for creative coding in Unity.

##### Dependencies

https://openupm.com/packages/com.yasirkula.runtimeinspector/

https://openupm.com/packages/jillejr.newtonsoft.json-for-unity.converters/

---

## [Bootstrapper](./Bootstrapper)

Bind fields and properties of `Monobehaviours` objects to a JSON file. Save, load, make changes to them at runtime.

##### Usage

1. Drag `Bootstrap` prefab into the main scene.
2. In your `MonoBehaviour` code, add [System.Runtime.Serialization namespace](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization?view=net-5.0), then add [[DataMember(Name = "")\]](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datamemberattribute?view=net-5.0) to any serialized properties that you want to be adjusted at runtime.
3. Add the `GameObject` with the above `MonoBehaviour` component to the `SettingPopulateList` of `BootstrapDictionary` on the `Bootstrap` prefab.
4. Enter the play mode and hit `Generate Bootstrap File`. A `bootstrapSettings.json` will be created at either `StreamingAsset` or [Persistent Data Path](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) based on your choice. Stop the play mode.
5. You are all set. Enter the play mode again. Hit `G` key to toggle the runtime inspector to change and save settings on the fly. You can use `runtimeEditorKeyStroke` property to change the key binding for the bootstrap inspector.

##### Tips

1. Bootstrap works better with reference types (class) than value types (struct). You can always create a `[Serializable]` class container to group similar settings together (regardless of reference types or value types) for better organizations.
2. [DisplayManager](./DisplayManager) and [ExternalAssetLoader](./ExternalAssetLoader) implement Bootstrap settings already. If you are also using them in your project, remember to drag their `GameObject` to the `SettingPopulateList`.

---

## [ExternalAssetLoader](./ExternalAssetLoader)

Provides a unifying way to download and load external media assets (audio, image, video) to your Unity project.

##### Usage

1. Create an empty `GameObject` in the main scene and add `ExternalAssetLoader` component to the object.
2. Change `LocalAssetsFolderPath` to suit the need of the project.
3. On startup, all media files under `LocalAssetsFolderPath` will be loaded and `AssetController.LocalAssetLoadingFinished` will be called when it's done. Audio files will be loaded as `AudioClip` and image files will be loaded as `Texture2D`. Video files will only be loaded as their absolute file path. The filename without the extension will be used as the `AssetKey` to retrieve the asset.
4. You can access the loaded asset via the functions below:

```plaintext
ExternalAssetLoader.Instance.AllAvailableAssets[assetkey]
ExternalAssetLoader.Instance.PreLoadedImagesAssets[assetKey]
ExternalAssetLoader.Instance.PreLoadedAudioClips[assetKey]
ExternalAssetLoader.Instance.DownloadedVideoPaths[assetKey]
```

---

## [BetterLogging](./General/Runtime/BetterLogging.cs)

Better logging ;)  

Add timestamp to each Unity log message, and save the log for each app session to the [persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html). Also provides verbosity filtering.

##### Usage

```
BetterLogging.Log(string log, LogLevel logLevel)
```

