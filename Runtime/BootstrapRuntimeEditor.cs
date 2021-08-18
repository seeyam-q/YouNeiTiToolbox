using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RuntimeInspectorNamespace;

namespace FortySevenE.Bootstrapper
{
    [ExecuteInEditMode]
    public class BootstrapRuntimeEditor : MonoBehaviour
    {
        [SerializeField] private Dropdown _settingKeysDropdown;
        [SerializeField] private Text _info;
        [SerializeField] private RuntimeInspector _runtimeInspector;
        [SerializeField] private Button _saveSettingsButton;
        [SerializeField] private Button _loadSettingsButton;

        public bool HasInit { get; private set; }

        public RuntimeInspector Inspector => _runtimeInspector;

        private Canvas _canvas;

        private Coroutine _showInfoCoroutine;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            //Set a higher sorting order so it will render on the top
            _canvas.sortingOrder = Byte.MaxValue;

            //Hide the editor by default
            enabled = false;
        }

        private void Start()
        {
            if (BootstrapDictionary.Instance != null)
            {
                BootstrapDictionary.Instance.BootstrapLoaded.AddListener(RefreshUI);

                //Calling this function manually despite the event subscription because the first boostrap load event will be called before Start()
                RefreshUI();
            }
        }

        private void OnDropdownValueChanged (int index)
        {
            UpdateInspector();
        }

        private void RefreshUI()
        {
            if (BootstrapDictionary.Instance != null)
            {
                _settingKeysDropdown.onValueChanged.RemoveAllListeners();
                _loadSettingsButton.onClick.RemoveAllListeners();
                _saveSettingsButton.onClick.RemoveAllListeners();

                if (BootstrapDictionary.Instance.BootstrapRuntimeAppliedSettings.Count > 0)
                {
                    List<string> bootstrapRuntimeSettingKeys = new List<string>();
                    foreach (BootstrapRuntimeAppliedSetting runtimeAppliedSetting in BootstrapDictionary.Instance.BootstrapRuntimeAppliedSettings)
                    {
                        bootstrapRuntimeSettingKeys.Add(runtimeAppliedSetting.key);
                    }

                    _settingKeysDropdown.ClearOptions();
                    _settingKeysDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
                    _settingKeysDropdown.AddOptions(bootstrapRuntimeSettingKeys);
                }
                else
                {
                    ShowInfo("No bootstrap settings found. Is it active in editor?");
                    Debug.LogWarningFormat("[{0}] No bootstrap setting has been applied runtime. Is bootstrap active in editor?", GetType().Name);
                }

                _loadSettingsButton.onClick.AddListener(OnLoadButtonClicked);
                _saveSettingsButton.onClick.AddListener(OnSaveButtonClicked);

                UpdateInspector();
            }
        }

        private void OnDestroy()
        {
            _settingKeysDropdown.onValueChanged.RemoveAllListeners();
            _loadSettingsButton.onClick.RemoveAllListeners();
            _saveSettingsButton.onClick.RemoveAllListeners();

            if (BootstrapDictionary.Instance != null)
            {
                BootstrapDictionary.Instance.BootstrapLoaded.RemoveAllListeners();
            }
        }

        private void OnEnable()
        {
            _canvas.enabled = true;
            _runtimeInspector.enabled = true;
            UpdateInspector();
        }

        private void OnDisable()
        {
            _canvas.enabled = false;
            _runtimeInspector.enabled = false;
        }

        private void OnSaveButtonClicked()
        {
            BootstrapDictionary.Instance.SaveAllSettings();
            ShowInfo("Saved!");
        }

        private void OnLoadButtonClicked ()
        {
            BootstrapDictionary.Instance.LoadBootstrapSettingsFromFile();
            BootstrapDictionary.Instance.PopulateSettingsToComponents();
        }

        private void UpdateInspector()
        {
            if (BootstrapDictionary.Instance != null)
            {
                if (_settingKeysDropdown.value < BootstrapDictionary.Instance.BootstrapRuntimeAppliedSettings.Count)
                {
                    BootstrapRuntimeAppliedSetting runtimeAppliedSetting = BootstrapDictionary.Instance.BootstrapRuntimeAppliedSettings[_settingKeysDropdown.value];
                    if (!runtimeAppliedSetting.fieldInfo.FieldType.IsValueType)
                    {
                        _runtimeInspector.Inspect(runtimeAppliedSetting.fieldInfo.GetValue(runtimeAppliedSetting.component));
                        if (typeof(IDictionary).IsAssignableFrom(runtimeAppliedSetting.fieldInfo.FieldType))
                        {
                            ShowInfo("Cannot show Dictionary at runtime. Please change it from bootstrap file.");
                        }
                        else
                        {
                            ShowInfo("");
                        }
                    }
                    else
                    {
                        _runtimeInspector.Inspect(runtimeAppliedSetting.component);
                        ShowInfo("");
                    }
                    _runtimeInspector.RefreshDelayed();
                }
            }
        }

        public void ShowInfo(string text)
        {
            _info.text = text;
        }
    }
}