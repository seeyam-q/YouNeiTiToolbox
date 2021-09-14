using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FortySevenE.Bootstrapper;

[CustomEditor(typeof(BootstrapDictionary))]
public class BootstrapDictionaryInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Bootstrap File"))
        {
            ((BootstrapDictionary)target).UpdateBootstrapFileFromSettingPopulateList();
        }
    }
}
