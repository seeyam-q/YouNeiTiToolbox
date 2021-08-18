
/*=================== Replace with Prefab ===================
Unity Forum Community Thread https://forum.unity.com/threads/replace-game-object-with-prefab.24311/
Tested in 2018.4.19f1, 2019.3.6f1, 2020.1.12f1
Should work in pre-2018.3 versions with old prefab workflow (Needs testing)
Changelog and contributors:
v1.0.0 (2010-03) Original CopyComponents by Michael L. Croswell for Colorado Game Coders, LLC
v1.1.0 (2011-06) by Kristian Helle Jespersen
v1.2.0 (2015-04) by Connor Cadellin McKee for Excamedia
v1.3.0 (2015-04) by Fernando Medina (fermmmm)
v1.4.0 (2015-07) by Julien Tonsuso (www.julientonsuso.com)
v1.5.0 (2017-06) by Alex Dovgodko
                 Changed into editor window and added instant preview in scene view
v1.6.0 (2018-03) by ???
                 Made changes to make things work with Unity 5.6.1
v1.7.0 (2018-05) by Carlos Diosdado (hypertectonic)
                 Added link to community thread, booleans to choose if scale and rotation are applied, mark scene as dirty, changed menu item
v1.8.0 (2018-??) by Virgil Iordan
                 Added KeepPlaceInHierarchy
v1.9.0 (2019-01) by Dev Bye-A-Jee, Sanjay Sen & Nick Rodriguez for Ravensbourne University London
                 Added unique numbering identifier in the hierarchy to each newly instantiated prefab, also accounts for existing numbers
v2.0.0 (2020-03-22) by Zan Kievit
                    Made compatible with the new Prefab system of Unity 2018. Made more user friendly and added undo.
v2.1.0 (2020-03-22) by Carlos Diosdado (hypertectonic)
                    Added options to use as a utility window (show from right click in the hierarchy), min/max window size,
                    backwards compatibility for old prefab system, works with prefabs selected in project browser, fixed not replacing prefabs,
                    added version numbers, basic documentation, Community namespace to avoid conflicts, cleaned up code for readability.
v2.2.0 (2020-03-22) by GGHitman
                    Add Search by tag or by layer
                    the object will replace the tag and the layer of the original object
                    compare and exchange materials
v.2.3.0 (2020-10-20) by Jade Annand
                     Added recursive game object, component, field and value copying.
v.2.4.0 (2020-11-9) by Zan Kievit
                    Fixed Rename errors. Added Numbering Schemes from project settings to Rename, dynamic preview of Rename, improved overal UX.
                    Added NaturalComparer class from https://www.codeproject.com/Articles/22517/Natural-Sort-Comparer for human readable sorting.
 
Known Errors: None
============================================================*/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace FortySevenE
{
    /// <summary>
    /// An editor tool to replace selected GameObjects with a specified Prefab.
    /// </summary>
    public class ReplaceWithPrefab : EditorWindow
    {
        public GameObject prefab = null;
        public GameObject oldPrefab = null;
        public List<GameObject> objectsToReplace = new List<GameObject>();
        public List<GameObject> newObjects = new List<GameObject>();
        public List<string> objectPreview = new List<string>();
        public bool editMode = false;

        public struct ReplacementPreferences
        {
            public bool renameObjects;
            public bool orderHierarchyToPreview;
            public bool applyRotation;
            public bool applyScale;
            public bool keepChildText;
        }

        public ReplacementPreferences replacementPreferences;

        NamingScheme namingScheme;
        public enum NamingScheme
        {
            SpaceParenthesis,
            Dot,
            Underscore
        }

        public bool SearchWithTag = false;
        public string TagForSearch = "Untagged";
        public GameObject[] searchResult;

        public bool SearchWithLayer = false;
        public int LayerForSearch;


        private Vector2 windowMinSize = new Vector2(450, 300);
        private Vector2 windowMaxSize = new Vector2(800, 1000);
        private Vector2 scrollPosition;

        private static readonly IDictionary<System.Type, IComponentCopier> componentCopiers = new Dictionary<System.Type, IComponentCopier>();
        private static readonly IDictionary<System.Type, ISet<string>> componentPartAvoiders = new Dictionary<System.Type, ISet<string>>();

        static ReplaceWithPrefab()
        {
            RegisterComponentCopiers();
            RegisterComponentPartAvoiders();
        }

        /// <summary>
        /// Gets or creates a new Replace with Prefab window.
        /// </summary>
        [MenuItem("Tools/47E/Replace with Prefab")]
        static void StartWindow()
        {
            ReplaceWithPrefab window = (ReplaceWithPrefab)GetWindow(typeof(ReplaceWithPrefab));
            window.Show();

            window.titleContent = new GUIContent("Replace with Prefab");
            window.minSize = window.windowMinSize;
            window.maxSize = window.windowMaxSize;
        }

        public ReplaceWithPrefab()
        {
            replacementPreferences.renameObjects = false;
            replacementPreferences.orderHierarchyToPreview = false;
            replacementPreferences.applyRotation = true;
            replacementPreferences.applyScale = true;
            replacementPreferences.keepChildText = true;
        }

        /// <summary>
        /// Handles getting the selected objects when the selection changes.
        /// </summary>
        void OnSelectionChange()
        {
            GetSelection();
            Repaint();
        }

        /// <summary>
        /// Draws the window content: object list, configuration and execution buttons.
        /// </summary>
        void OnGUI()
        {
            #region Draw Top Buttons

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                editMode = GUILayout.Toggle(editMode, new GUIContent("Start replacing", "Start using this feature"), EditorStyles.toggle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            #endregion

            #region "TAG LAYER"

            SearchWithTag = GUILayout.Toggle(!SearchWithLayer ? SearchWithTag : false, "Apply Search By Tag", EditorStyles.toggle);
            SearchWithLayer = GUILayout.Toggle(!SearchWithTag ? SearchWithLayer : false, "Apply Search By Layer");

            if (SearchWithTag)
            {
                GUILayout.Space(5);
                TagForSearch = EditorGUILayout.TagField("Set tag :  ", TagForSearch);
            }
            else if (SearchWithLayer)
            {
                GUILayout.Space(5);
                LayerForSearch = EditorGUILayout.LayerField("Set layer :  ", LayerForSearch);
            }
            #endregion "TAG LAYER"

            if (GUI.changed)
            {
                if (editMode)
                    GetSelection();
                else
                    ResetPreview();
            }

            GUILayout.Space(10);
            if (editMode)
            {
                SetNamingScheme();
                RenamePreview();

                #region Draw Prefab and List

                GUILayout.Label("Prefab: ", EditorStyles.boldLabel);
                prefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), true);
                if (oldPrefab != prefab)
                {
                    GetSelection();
                    oldPrefab = prefab;
                }
                GUILayout.Space(10);

                if (prefab != null)
                {
                    if (objectsToReplace.Count > 0)
                    {
                        GUILayout.Label(new GUIContent("Objects to be Replaced:", (!SearchWithTag && !SearchWithLayer) ? "Multi-select all the objects you want to replace with your Prefab" : ""), EditorStyles.boldLabel);

                        objectPreview.Sort(new NaturalComparer());

                        scrollPosition = GUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox);
                        {
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            {
                                string previewText = "No Preview";

                                if (replacementPreferences.renameObjects && !replacementPreferences.orderHierarchyToPreview)
                                    previewText = "Preview of Renaming";
                                else if (replacementPreferences.orderHierarchyToPreview && !replacementPreferences.renameObjects)
                                    previewText = "Preview of Hierarchy Order";
                                else if (replacementPreferences.orderHierarchyToPreview && replacementPreferences.renameObjects)
                                    previewText = "Preview of Renaming and Hierarchy Order";

                                GUILayout.Label(previewText, EditorStyles.miniLabel);
                            }
                            GUILayout.EndHorizontal();

                            foreach (string go in objectPreview)
                            {
                                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                                GUILayout.Label(go);
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.Space(2);
                        }
                        GUILayout.EndScrollView();

                        GUILayout.Space(5);

                        replacementPreferences.renameObjects = GUILayout.Toggle(replacementPreferences.renameObjects, "Rename replaced objects", EditorStyles.toggle);
                        replacementPreferences.orderHierarchyToPreview = GUILayout.Toggle(replacementPreferences.orderHierarchyToPreview, "Oder hierarchy to preview", EditorStyles.toggle);
                        GUILayout.Space(10);
                        replacementPreferences.applyRotation = GUILayout.Toggle(replacementPreferences.applyRotation, "Apply rotation", EditorStyles.toggle);
                        replacementPreferences.applyScale = GUILayout.Toggle(replacementPreferences.applyScale, "Apply scale", EditorStyles.toggle);
                        replacementPreferences.keepChildText = GUILayout.Toggle(replacementPreferences.keepChildText, "Keep Text of UGUI Text", EditorStyles.toggle);
                    }
                    else if (!SearchWithTag && !SearchWithLayer)
                    {
                        GUILayout.Label(new GUIContent("Multi-select all the objects you want to replace with your Prefab"), EditorStyles.boldLabel);
                    }
                }
                else
                {
                    GUILayout.Label("Select a Prefab to replace objects with", EditorStyles.boldLabel);
                }
                #endregion

                #region Draw Bottom Buttons

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    if (prefab != null && objectsToReplace.Count > 0)
                    {
                        if (GUILayout.Button("Apply"))
                        {
                            foreach (GameObject go in objectsToReplace)
                            {
                                Replace(go);
                                DestroyImmediate(go);
                            }
                            if (replacementPreferences.renameObjects)
                            {
                                Rename();
                            }
                            editMode = false;
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Important so that we don't forget to save!
                        }
                        else if (GUILayout.Button("Cancel"))
                        {
                            objectsToReplace.Clear();
                            objectPreview.Clear();
                            ResetPreview();
                            prefab = null;
                        }
                    }
                }
                GUILayout.EndHorizontal();
                #endregion
            }
            else
            {
                objectsToReplace.Clear();
                objectPreview.Clear();
                newObjects.Clear();
                prefab = null;
            }
        }

        /// <summary>
        /// Renames the gameObjects, adding numbering following the Naming Scheme Set in the Project Settings.
        /// It checks for already used numbers.
        /// </summary>
        void Rename()
        {
            int count = 0;
            List<int> ExistingNumbers = new List<int>();

            SetExistingNumbers(newObjects, ExistingNumbers, namingScheme);

            //Apply new names
            foreach (GameObject go in newObjects)
            {
                count = GetCount(count, ExistingNumbers);
                switch (namingScheme)
                {
                    case NamingScheme.SpaceParenthesis:
                        go.name = prefab.name + " (" + count + ")";
                        break;
                    case NamingScheme.Dot:
                        go.name = prefab.name + "." + count;
                        break;
                    case NamingScheme.Underscore:
                        go.name = prefab.name + "_" + count;
                        break;
                }
            }
        }

        /// <summary>
        /// Renames the list of names, adding numbering following the Naming Scheme Set in the Project Settings.
        /// It checks for already used numbers.
        /// </summary>
        void RenamePreview()
        {
            int count = 0;
            List<int> ExistingNumbers = new List<int>();
            objectPreview.Clear();

            if (replacementPreferences.renameObjects)
            {
                SetExistingNumbers(objectsToReplace, ExistingNumbers, namingScheme);
            }
            //Apply new names
            for (int n = 0; n < objectsToReplace.Count; n++)
            {
                if (replacementPreferences.renameObjects)
                {
                    count = GetCount(count, ExistingNumbers);
                    switch (namingScheme)
                    {
                        case NamingScheme.SpaceParenthesis:
                            objectPreview.Add(prefab.name + " (" + count + ")");
                            break;
                        case NamingScheme.Dot:
                            objectPreview.Add(prefab.name + "." + count);
                            break;
                        case NamingScheme.Underscore:
                            objectPreview.Add(prefab.name + "_" + count);
                            break;
                    }
                }
                else if (!replacementPreferences.renameObjects)
                    objectPreview.Add(objectsToReplace[n].name);
            }
        }

        /// <summary>
        /// Set existing numbers based on the naming scheme set in the project settings
        /// </summary>
        /// <param name="objects">The list of objects to get the names from</param>
        /// <param name="ExistingNumbers">The list of ExistingNumbers to set</param>
        /// <param name="namingScheme">The naming scheme to use</param>
        void SetExistingNumbers(List<GameObject> objects, List<int> ExistingNumbers, NamingScheme namingScheme)
        {
            foreach (GameObject obj in objects)
            {
                string name = obj.name;
                if (name.Contains(prefab.name) && name.Any(char.IsDigit))
                {
                    int num = 0;
                    switch (namingScheme)
                    {
                        case NamingScheme.SpaceParenthesis:
                            char[] charsToTrim = { '(', ')' };
                            num = GetExistingNumber(name, name.Split(' '), charsToTrim);
                            break;
                        case NamingScheme.Dot:
                            num = GetExistingNumber(name, name.Split('.'));
                            break;
                        case NamingScheme.Underscore:
                            num = GetExistingNumber(name, name.Split('_'));
                            break;
                    }
                    if (num != 0)
                        ExistingNumbers.Add(num);
                    else
                        Debug.LogError("The selected object cannot be renamed, as there are several naming schemes used");
                }
            }
        }

        /// <summary>
        /// Finds the "space" character in the name to identify where the number is, then if needed, strips provided extra characters.
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="splitChars">The string array</param>
        /// <param name="charsToTrim">The extra characters to trim before converting the string to an int</param>
        int GetExistingNumber(string name, string[] splitChars, char[] charsToTrim = null)
        {
            int count = 1;

            if (splitChars.Length > 1)
            {
                string digit = splitChars[1]; // substring which contains number

                //Get the substring that contains digits
                while (GetDigits(digit) == "")
                {
                    count++;
                    digit = GetDigits(splitChars[count]);
                }

                if (charsToTrim != null)
                    digit = digit.Trim(charsToTrim);

                return int.Parse(GetDigits(digit)); // convert string to number
            }
            else
            {
                return int.Parse(GetDigits(name));
            }
        }

        /// <summary>
        /// The number to give the
        /// </summary>
        /// <param name="count">The name of the object</param>
        /// <param name="ExistingNumbers">The existing numbers to keep</param>
        int GetCount(int count, List<int> ExistingNumbers)
        {
            if (ExistingNumbers.Count > 0)
            {
                int i = 0;
                while (i < ExistingNumbers.Count)
                {
                    if (count == ExistingNumbers[i])
                    {
                        count++;
                        i = 0;
                        return count;
                    }
                    else
                        i++;
                }
            }
            count++;
            return count;
        }

        /// <summary>
        /// Replaces a given gameObject with a previously chosen prefab.
        /// </summary>
        /// <param name="obj">The gameObject to replace.</param>
        void Replace(GameObject obj)
        {
            GameObject newObject;

            newObject = PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(prefab)) as GameObject;

            if (newObject == null) // if the prefab is chosen from the project browser and not the hierarchy, it is null
                newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            newObject.transform.SetParent(obj.transform.parent, true);

            newObjects.Add(newObject);

            CopyContentsToNew(obj, newObject);

            Undo.RegisterCreatedObjectUndo(newObject, "Replaced Objects");
            Undo.DestroyObjectImmediate(obj);
        }

        private void CopyContentsToNew(GameObject oldObject, GameObject newObject)
        {
            newObject.tag = oldObject.tag;
            newObject.layer = oldObject.layer;

            Component[] components = oldObject.GetComponents(typeof(Component));
            foreach (Component component in components)
            {
                IComponentCopier copier = null;
                System.Type componentType = component.GetType();
                while (copier == null)
                {
                    if (componentCopiers.ContainsKey(componentType))
                        copier = componentCopiers[componentType];
                    else if (componentType.BaseType == typeof(System.Object) ||
                        componentType.BaseType == null)
                        copier = defaultComponentCopier;
                    else
                        componentType = componentType.BaseType; // Components may be derivatives, so look up the tree.
                }
                copier.CopyComponent(replacementPreferences, component, newObject);
            }
            int childCount = oldObject.transform.childCount;
            for (int i = 0; i < oldObject.transform.childCount; i++)
            {
                GameObject child = oldObject.transform.GetChild(i).gameObject;
                GameObject newChild;
                Transform newChildTransform = newObject.transform.Find(child.name);
                if (newChildTransform == null)
                {
                    newChild = new GameObject(child.name);
                    newChild.transform.SetParent(newObject.transform, false);
                }
                else
                    newChild = newChildTransform.gameObject;
                CopyContentsToNew(child, newChild);
            }
        }

        /// <summary>
        /// Gets the currently selected game objects.
        /// </summary>
        void GetSelection()
        {
            SetNamingScheme();

            if (editMode && Selection.activeGameObject != null && (prefab == null || (!SearchWithTag && !SearchWithLayer)))
            {
                if (prefab == null) // get the prefab 1st
                {
                    PrefabAssetType t = PrefabUtility.GetPrefabAssetType(Selection.activeGameObject);

                    if (t == PrefabAssetType.Regular || t == PrefabAssetType.Variant)
                    {
                        prefab = Selection.activeGameObject;
                        oldPrefab = Selection.activeGameObject;
                    }
                }
                else // get the other objects
                {
                    ResetPreview();
                    objectPreview.Clear();
                    objectsToReplace.Clear();
                    foreach (var obj in Selection.gameObjects)
                    {
                        if (obj != prefab)
                            objectsToReplace.Add(obj);
                    }
                }
            }
            if (editMode && prefab != null && (SearchWithTag || SearchWithLayer))
            {
                if (SearchWithTag)
                {
                    ResetPreview();
                    objectPreview.Clear();
                    objectsToReplace.Clear();
                    GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (var gg in allGameObjects)
                    {
                        if (gg.tag == TagForSearch)
                        {
                            if (gg != prefab)
                                objectsToReplace.Add(gg);
                        }
                    }
                }
                else if (SearchWithLayer)
                {
                    ResetPreview();
                    objectPreview.Clear();
                    objectsToReplace.Clear();
                    GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (var gg in allGameObjects)
                    {
                        if (gg.layer == LayerForSearch)
                        {
                            if (gg != prefab)
                                objectsToReplace.Add(gg);
                        }
                    }
                }
            }
            else if (editMode && Selection.activeGameObject == null && prefab != null && !SearchWithTag && !SearchWithLayer)
            {
                ResetPreview();
                objectPreview.Clear();
                objectsToReplace.Clear();
            }
        }

        void SetNamingScheme()
        {
#if UNITY_2020_OR_NEWER
            namingScheme = EditorSettings.gameObjectNamingScheme;
#else
            namingScheme = NamingScheme.SpaceParenthesis;
#endif
        }

        /// <summary>
        /// Resets the gameObject preview.
        /// </summary>
        void ResetPreview()
        {
            if (newObjects != null)
            {
                foreach (GameObject go in newObjects)
                {
                    DestroyImmediate(go);
                }
            }
            newObjects.Clear();
        }

        /// <summary>
        /// Handles window destruction.
        /// </summary>
        void OnDestroy()
        {
            ResetPreview();
        }

        /// <summary>
        /// Takes all digits from a string and returns them as one string.
        /// </summary>
        /// <param name="text">The string to get the digits from</param>
        /// <returns>A string of digits</returns>
        string GetDigits(string text)
        {
            string digits = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    digits += text[i];
                }
            }
            return digits;
        }

        /// <summary>
        /// ASCII comparer class
        /// </summary>
        public class NaturalComparer : Comparer<string>, IDisposable
        {
            private Dictionary<string, string[]> table;

            public NaturalComparer()
            {
                table = new Dictionary<string, string[]>();
            }

            public void Dispose()
            {
                table.Clear();
                table = null;
            }

            public override int Compare(string x, string y)
            {
                if (x == y)
                {
                    return 0;
                }
                string[] x1, y1;
                if (!table.TryGetValue(x, out x1))
                {
                    x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                    table.Add(x, x1);
                }
                if (!table.TryGetValue(y, out y1))
                {
                    y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
                    table.Add(y, y1);
                }

                for (int i = 0; i < x1.Length && i < y1.Length; i++)
                {
                    if (x1[i] != y1[i])
                    {
                        return PartCompare(x1[i], y1[i]);
                    }
                }
                if (y1.Length > x1.Length)
                {
                    return 1;
                }
                else if (x1.Length > y1.Length)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            private static int PartCompare(string left, string right)
            {
                int x, y;
                if (!int.TryParse(left, out x))
                {
                    return left.CompareTo(right);
                }

                if (!int.TryParse(right, out y))
                {
                    return left.CompareTo(right);
                }

                return x.CompareTo(y);
            }
        }

        /// <summary>
        /// Register component-specific copy objects for any components that require special handling.
        /// </summary>
        static void RegisterComponentCopiers()
        {
            componentCopiers.Add(typeof(Transform), new TransformComponentCopier());
            componentCopiers.Add(typeof(MeshRenderer), new MeshRendererComponentCopier());
            componentCopiers.Add(typeof(SkinnedMeshRenderer), new SkinnedMeshRendererComponentCopier());
        }

        /// <summary>
        /// Register component-specific property names to avoid copying in a default manner.
        /// </summary>
        static void RegisterComponentPartAvoiders()
        {
            ISet<string> transformAvoiders = new HashSet<string>
            {
                "localRotation",
                "localScale",
                "name",
                "parent"
            };
            componentPartAvoiders.Add(typeof(Transform), transformAvoiders);
        }

        // The interface for component-specific copiers from old to new GameObjects.
        public interface IComponentCopier
        {
            void CopyComponent(ReplacementPreferences replacementPreferences, Component original, GameObject newObject);
        }

        // For anything that does not have a component-specific copier, or anything that does but wants to include default copy behaviour.
        public class DefaultComponentCopier : IComponentCopier
        {
            public void CopyComponent(ReplacementPreferences replacementPreferences, Component original, GameObject newObject)
            {
                System.Type type = original.GetType();
                ISet<string> partAvoiders = componentPartAvoiders.ContainsKey(type) ? componentPartAvoiders[type] : null;
                var dst = newObject.GetComponent(type);
                if (!dst)
                    dst = newObject.AddComponent(type);
                var fields = type.GetFields();
                foreach (var field in fields)
                {
                    if (field.IsStatic)
                        continue;
                    if (partAvoiders != null && partAvoiders.Contains(field.Name))
                        continue;
                    field.SetValue(dst, field.GetValue(original));
                }
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    if (!prop.CanWrite || prop.Name == "name" || prop.Name == "parent")
                        continue;
                    if (partAvoiders != null && partAvoiders.Contains(prop.Name))
                        continue;
                    prop.SetValue(dst, prop.GetValue(original));
                }
                // NOTE: Some properties are references to other things and a prefab replacement can break them.
                // TODO: Should we record any reference types in order to map them to new references later?
            }
        }

        // Shared instance of default copier.
        public static DefaultComponentCopier defaultComponentCopier = new DefaultComponentCopier();

        /// <summary>
        /// Transform-specific component copier.
        /// </summary>
        public class TransformComponentCopier : IComponentCopier
        {
            public void CopyComponent(ReplacementPreferences replacementPreferences, Component original, GameObject newObject)
            {
                Transform oldTransform = (Transform)original;
                if (replacementPreferences.applyRotation)
                    newObject.transform.localRotation = oldTransform.localRotation;

                if (replacementPreferences.applyScale)
                    newObject.transform.localScale = oldTransform.localScale;

                if (!replacementPreferences.renameObjects)
                    newObject.transform.name = oldTransform.name;

                if (replacementPreferences.keepChildText)
                {
                    var oldTextComponents = oldTransform.GetComponentsInChildren<Text>();
                    var newTextComponents = newObject.GetComponentsInChildren<Text>();
                    foreach (var oldText in oldTextComponents)
                    {
                        foreach (var newText in newTextComponents)
                        {
                            if (newText.gameObject.name == oldText.gameObject.name)
                            {
                                newText.text = oldText.text;
                            }
                        }
                    }
                }

                if (!replacementPreferences.orderHierarchyToPreview)
                    newObject.transform.SetSiblingIndex(oldTransform.GetSiblingIndex());
                defaultComponentCopier.CopyComponent(replacementPreferences, original, newObject);
            }
        }

        /// <summary>
        /// Special for-purpose component copier for mesh renderer.
        /// </summary>
        public class MeshRendererComponentCopier : IComponentCopier
        {
            public void CopyComponent(ReplacementPreferences replacementPreferences, Component original, GameObject newObject)
            {
                MeshRenderer meshRenderer = (MeshRenderer)original;
                MeshRenderer newMeshRenderer = newObject.GetComponent<MeshRenderer>();
                // QUESTION Should we instantiate one if one is not present?
                if (newMeshRenderer)
                {
                    if (meshRenderer.sharedMaterials.Length == newMeshRenderer.sharedMaterials.Length)
                    {
                        Material[] CacheMaterials = new Material[meshRenderer.sharedMaterials.Length];
                        for (int a = 0; a < meshRenderer.sharedMaterials.Length; a++)
                            CacheMaterials[a] = meshRenderer.sharedMaterials[a];
                        for (int b = 0; b < CacheMaterials.Length; b++)
                            newMeshRenderer.sharedMaterials[b] = CacheMaterials[b];
                    }
                }
            }
        }

        /// <summary>
        /// Special for-purpose component copier for skinned mesh renderer.
        /// </summary>
        public class SkinnedMeshRendererComponentCopier : IComponentCopier
        {
            public void CopyComponent(ReplacementPreferences replacementPreferences, Component original, GameObject newObject)
            {
                SkinnedMeshRenderer meshRenderer = (SkinnedMeshRenderer)original;
                SkinnedMeshRenderer newMeshRenderer = newObject.GetComponent<SkinnedMeshRenderer>();
                // QUESTION Should we instantiate one if one is not present?
                if (newMeshRenderer)
                {
                    if (meshRenderer.sharedMaterials.Length == newMeshRenderer.sharedMaterials.Length)
                    {
                        Material[] CacheMaterials = new Material[meshRenderer.sharedMaterials.Length];
                        for (int a = 0; a < meshRenderer.sharedMaterials.Length; a++)
                            CacheMaterials[a] = meshRenderer.sharedMaterials[a];
                        for (int b = 0; b < CacheMaterials.Length; b++)
                            newMeshRenderer.sharedMaterials[b] = CacheMaterials[b];
                    }
                }
            }
        }
    }
}
