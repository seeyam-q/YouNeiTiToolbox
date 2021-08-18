using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FortySevenE.Bootstrapper
{
    public enum BootstrapFileLocation
    {
        StreamingAssets,
        PersistentDataPath,
        Absolute
    }

    public enum BoostrapFileFormat
    {
        JSON,
        XML
    }

    public class BootstrapRuntimeAppliedSetting
    {
        public string key;
        public Component component;
        public FieldInfo fieldInfo;

        public BootstrapRuntimeAppliedSetting (string key, Component appliedComponent, FieldInfo appliedFieldInfo)
        {
            this.key = key;
            this.component = appliedComponent;
            this.fieldInfo = appliedFieldInfo;
        }
    }

    public class BootstrapDictionary : MonoBehaviour
    {
        public static BootstrapDictionary Instance;

        [Header("Bootstrap Config")]

        [SerializeField] protected bool _activeInEditor = default;
        public bool ActiveInEditor { get { return _activeInEditor; } }

        [SerializeField] protected BootstrapFileLocation _bootstrapFileDirectory = default;
        [SerializeField] protected string _bootstrapFile = "bootstrapSettings.json";
        [SerializeField] KeyCode _runtimeEditorKeyStroke = KeyCode.G;
        [Tooltip("Two-touch tap X times to toggle runtime editor")]
        [SerializeField] int _runtimeEditorTwoTouchTapCount = 3;
        [SerializeField] protected BootstrapRuntimeEditor _runtimeEditor;
        public BootstrapRuntimeEditor RuntimeEditor { get { return _runtimeEditor; } }

        [Header("Populate settings to the fields if their attribute [DataMember(Name)] matches the key")]
        [SerializeField] protected GameObject[] _settingPopulateList = default;

        private List<BootstrapRuntimeAppliedSetting> _bootstrapRuntimeAppliedSettings = new List<BootstrapRuntimeAppliedSetting>();
        public ReadOnlyCollection<BootstrapRuntimeAppliedSetting> BootstrapRuntimeAppliedSettings { get { return _bootstrapRuntimeAppliedSettings.AsReadOnly(); } }

        [Header("Callbacks")]
        public UnityEvent BootstrapLoaded;

        private Dictionary<string, object> _bootstrapSettingDictionary;
        private string _bootstrapRawText;
        private IBootstrapRawTextParser _bootstrapRawTextParser;

        public static string GetCmdLineArg(string commandLineKey)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(commandLineKey, StringComparison.InvariantCultureIgnoreCase) || args[i].Equals("-" + commandLineKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((i + 1) < args.Length)
                    {
                        return args[i + 1];
                    }
                }
            }

            return null;
        }

        public static bool HasCmdLineArg(string commandLineArg)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(commandLineArg, StringComparison.InvariantCultureIgnoreCase) || args[i].Equals("-" + commandLineArg, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject);

                _bootstrapSettingDictionary = new Dictionary<string, object>();

                string bootstrapFileName = GetCmdLineArg("bootstrap");
                if (!string.IsNullOrWhiteSpace(bootstrapFileName))
                {
                    _bootstrapFile = bootstrapFileName;
                }

                LoadBootstrapSettingsFromFile();
#if UNITY_EDITOR
                if (ActiveInEditor)
                {
                    PopulateSettingsToComponents();
                }
#else
                PopulateSettingsToComponents();
#endif
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.B))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    SaveToFile();
                }

                if (Input.GetKeyDown(KeyCode.L))
                {
                    LoadBootstrapSettingsFromFile();
                }
            }

            if (Input.GetKeyDown(_runtimeEditorKeyStroke))
            {
                if (RuntimeEditor != null)
                {
                    RuntimeEditor.enabled = !RuntimeEditor.enabled;
                }
            }

            if (Input.touchCount == 2)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Ended && 
                    Input.GetTouch(0).tapCount == _runtimeEditorTwoTouchTapCount)
                {
                    RuntimeEditor.enabled = !RuntimeEditor.enabled;
                }
            }
        }

        public string GetBootstrapReadAbsPath()
        {
            string bootstrapFilePath = "";
            if (_bootstrapFileDirectory != BootstrapFileLocation.StreamingAssets)
            {
                switch (_bootstrapFileDirectory)
                {
                    case BootstrapFileLocation.Absolute:
                        bootstrapFilePath = _bootstrapFile;
                        break;
                    case BootstrapFileLocation.PersistentDataPath:
                        bootstrapFilePath = Path.Combine(Application.persistentDataPath, _bootstrapFile);
                        break;
                }
                if (!File.Exists(bootstrapFilePath))
                {
                    Debug.LogWarning($"No bootstrap file {_bootstrapFile} found in {bootstrapFilePath}, reverting to Streaming Assets.");
                    bootstrapFilePath = Path.Combine(Application.streamingAssetsPath, Path.GetFileName(_bootstrapFile));
                }
            }
            else
            {
                bootstrapFilePath = Path.Combine(Application.streamingAssetsPath, _bootstrapFile);
            }

            return bootstrapFilePath;
        }

        public string GetBootstrapWriteAbsPath()
        {
            string bootstrapFilePath = "";

            switch (_bootstrapFileDirectory)
            {
                case BootstrapFileLocation.Absolute:
                    bootstrapFilePath = _bootstrapFile;
                    break;
                case BootstrapFileLocation.PersistentDataPath:
                    bootstrapFilePath = Path.Combine(Application.persistentDataPath, _bootstrapFile);
                    break;
                case BootstrapFileLocation.StreamingAssets:
                    bootstrapFilePath = Path.Combine(Application.streamingAssetsPath, _bootstrapFile);
                    break;
            }

            return bootstrapFilePath;
        }

        public void LoadBootstrapSettingsFromFile ()
        {
            switch (Path.GetExtension(_bootstrapFile).ToLower())
            {
                case ".json":
                    _bootstrapRawTextParser = new BootstrapJsonParser();
                    break;
                default:
                    Debug.LogWarning("Cannot recoginize bootstrap file format, bootstrap settings will not be loaded");
                    _bootstrapRawTextParser = new BootstrapJsonParser();
                    break;
            }

            string bootstrapFilePath = GetBootstrapReadAbsPath();

            if (File.Exists(bootstrapFilePath))
            {
                _bootstrapRawText = File.ReadAllText(bootstrapFilePath);

                if (_bootstrapRawTextParser.TryGetSettingDictionary(_bootstrapRawText, ref _bootstrapSettingDictionary))
                {
                    Debug.LogFormat($"<color=green><b>{bootstrapFilePath}</b> Loaded</color>");
                    if (BootstrapLoaded != null)
                    {
                        BootstrapLoaded.Invoke();
                    }
                }
                else
                {
                    Debug.LogError("Bootstrap cannot be parsed");
                }
            }
            else
            {
                Debug.LogWarning("No bootstrap file found at " + bootstrapFilePath);
            }
        }

        public void SaveSetting(string key, object setting, bool overwriteIfExist = true, bool saveToFile = true)
        {
            if (_bootstrapSettingDictionary == null)
            {
                _bootstrapSettingDictionary = new Dictionary<string, object>();
            }
            if (!_bootstrapSettingDictionary.ContainsKey(key))
            {
                _bootstrapSettingDictionary.Add(key, setting);
            }
            else
            {
                if (overwriteIfExist)
                {
                    _bootstrapSettingDictionary[key] = setting;
                }
                else
                {
                    Debug.Log(key + " already exsits in the bootstrap. Enable overwriteIfExist in the function if overwrite is needed");
                }
            }

            if (saveToFile)
            {
                SaveToFile();
            }
        }

        public object GetSetting(string key, Type type)
        {
            object setting = null;
            if (_bootstrapSettingDictionary != null)
            {
                if (_bootstrapSettingDictionary.ContainsKey(key))
                {
                    if (_bootstrapSettingDictionary[key] == null)
                    {
                        return null;
                    }
                    else
                    {
                        //Since _bootstrapSettingDictionary treats all of its value as typeOf(object), IBootstrapRawTextParser will not serialize them into the specific settings type and will still treat them as raw text.
                        //(IBootstrapRawTextParser does not know what type each setting value is when parsing them from the bootstrap raw text)
                        //Therefore, we need to serialize the raw setting values to their desiginated type when some other class tries to get it.
                        //The _bootstrapSettingDictionary will also holds the setting's reference so it can keep tracking the changes of the setting in case these changes need to be saved into file.
                        if (_bootstrapSettingDictionary[key].GetType() == type)
                        {
                            setting = _bootstrapSettingDictionary[key];
                        }
                        else
                        {
                            if (_bootstrapRawTextParser.TryParseRawText(_bootstrapSettingDictionary[key].ToString(), type, out setting))
                            {
                                _bootstrapSettingDictionary[key] = setting;
                            }
                            else
                            {
                                Debug.LogWarning("The setting of key " + key + " is not " + type.ToString());
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(key + " could not be found in Bootstrap");
                }
            }
            else
            {
                Debug.LogWarning("Bootstrap dictionary has not been init yet.");
            }

            return setting;
        }

        public void PopulateSettingsToComponents()
        {
            _bootstrapRuntimeAppliedSettings.Clear();

            foreach (GameObject go in _settingPopulateList)
            {
                foreach (Component component in go.GetComponents<Component>())
                {
                    if (component is Transform)
                    {
                        continue;
                    }

                    foreach (FieldInfo fieldInfo in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        string settingKey = null;
                        if (fieldInfo.GetCustomAttribute<DataMemberAttribute>(true) != null)
                        {
                            settingKey = fieldInfo.GetCustomAttribute<DataMemberAttribute>(true).Name;
                        }
                        else if (fieldInfo.GetCustomAttribute<XmlElementAttribute>() != null)
                        {
                            settingKey = fieldInfo.GetCustomAttribute<XmlElementAttribute>().ElementName;
                        }
                        else if (fieldInfo.GetCustomAttribute<JsonPropertyAttribute>() != null)
                        {
                            settingKey = fieldInfo.GetCustomAttribute<JsonPropertyAttribute>().PropertyName;
                        }


                        if (settingKey != null)
                        {
                            if (_bootstrapSettingDictionary.ContainsKey(settingKey))
                            {
                                object settingValue = GetSetting(settingKey, fieldInfo.FieldType);
                                fieldInfo.SetValue(component, settingValue);
                                Debug.LogFormat("<b>{0}</b> in <b>{1}</b> has been replaced with the new value from {2}", fieldInfo.Name, component.name, _bootstrapFile);
                                _bootstrapRuntimeAppliedSettings.Add(new BootstrapRuntimeAppliedSetting(settingKey, component, fieldInfo));
                            }
                        }
                    }
                }
            }
        }

        public void SaveAllSettings()
        {
            //if (!Application.isPlaying)
            //{
            //    Debug.LogWarning("Can only be used in play mode, as it uses reflections");
            //}

            foreach (GameObject go in _settingPopulateList)
            {
                foreach (Component component in go.GetComponents<Component>())
                {
                    if (component is Transform)
                    {
                        continue;
                    }

                    foreach (FieldInfo fieldInfo in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        string settingKey = null;
                        if (fieldInfo.GetCustomAttribute<DataMemberAttribute>(true) != null)
                        {
                            settingKey = fieldInfo.GetCustomAttribute<DataMemberAttribute>(true).Name;
                        }
                        else if (fieldInfo.GetCustomAttribute<XmlElementAttribute>() != null)
                        {
                            settingKey = fieldInfo.GetCustomAttribute<XmlElementAttribute>().ElementName;
                        }
                        else if (fieldInfo.GetCustomAttribute<JsonPropertyAttribute>() != null)
                        {
                            settingKey = fieldInfo.GetCustomAttribute<JsonPropertyAttribute>().PropertyName;
                        }


                        if (settingKey != null)
                        {
                            object settingValue = fieldInfo.GetValue(component);
                            SaveSetting(settingKey, settingValue, overwriteIfExist: true, saveToFile: false);
                        }
                    }
                }
            }

            SaveToFile();
        }

        private void SaveToFile(string overrideFullPath = null)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Can only save bootstrap in the play mode.");
                return;
            }

            string _bootstrapRawText = _bootstrapRawTextParser.SerializeDictionary(_bootstrapSettingDictionary);
            if (!string.IsNullOrEmpty(_bootstrapRawText))
            {
                var bootstrapFilePath = overrideFullPath == null ? GetBootstrapWriteAbsPath() : overrideFullPath;
                var bootstrapDir = new FileInfo(bootstrapFilePath)?.Directory?.FullName;
                if (bootstrapDir == null)
                {
                    Debug.LogError($"Bootstrap save failed - {bootstrapFilePath} not valid" );
                    return;
                }

                if (!Directory.Exists(bootstrapDir))
                {
                    Directory.CreateDirectory(bootstrapDir);
                }

                File.WriteAllText(bootstrapFilePath, _bootstrapRawText);

                Debug.Log("Bootstrap saved to <color=blue>" + bootstrapFilePath + "</color>");
            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(BootstrapDictionary))]
    public class BootstrapDictionaryInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save Bootstrap Settings"))
            {
                ((BootstrapDictionary)target).SaveAllSettings();
            }
        }
    }
    #endif
}
