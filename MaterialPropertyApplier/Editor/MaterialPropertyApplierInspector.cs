using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FortySevenE
{
    [CustomEditor(typeof(MaterialPropertyApplier))]
    public class MaterialPropertyApplierInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as MaterialPropertyApplier;

            base.OnInspectorGUI();
            if (!script.ValidateInterfaceReferences())
            {
                base.OnInspectorGUI();
            }
        }
    }
}