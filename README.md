# Bootstrapper 

Bind fields and properties of `Monobehaviours` objects to a JSON file. Save, load, make changes to them at runtime.

### Install
1. Install dependencies: [com.yasirkula.runtimeinspector/](https://openupm.com/packages/com.yasirkula.runtimeinspector/) and [jillejr.newtonsoft.json-for-unity.converters/](https://openupm.com/packages/jillejr.newtonsoft.json-for-unity.converters/)
1. In the Unity editor, `Window->Package Manager` add package from git url: https://github.com/seeyam-q/YouNeiTiToolbox.git#upm-bootstrapper

### Usage

1. Drag `Bootstrap` prefab into the main scene.
2. In your `MonoBehaviour` code, add [System.Runtime.Serialization namespace](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization?view=net-5.0), then add [[DataMember(Name = "")\]](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datamemberattribute?view=net-5.0) to any serialized properties that you want to be adjusted at runtime.
3. Add the `GameObject` with the above `MonoBehaviour` component to the `SettingPopulateList` of `BootstrapDictionary` on the `Bootstrap` prefab.
4. Enter the play mode and hit `Generate Bootstrap File`. A `bootstrapSettings.json` will be created at either `StreamingAsset` or [Persistent Data Path](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) based on your choice. Stop the play mode.
5. You are all set. Enter the play mode again. Hit `G` key to toggle the runtime inspector to change and save settings on the fly. You can use `runtimeEditorKeyStroke` property to change the key binding for the bootstrap inspector.

### Tips

1. Bootstrap works better with reference types (class) than value types (struct). You can always create a `[Serializable]` class container to group similar settings together (regardless of reference types or value types) for better organizations.