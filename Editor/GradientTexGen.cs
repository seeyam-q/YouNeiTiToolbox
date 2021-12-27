using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.IO;
using System.Collections;

namespace mattatz.Utils
{

    public class GradientTexGen
    {

        public static Texture2D CreateHorizontal(Gradient grad, int width = 32, int height = 1)
        {
            var gradTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            gradTex.filterMode = FilterMode.Bilinear;
            float inv = 1f / (width);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var t = x * inv;
                    Color col = grad.Evaluate(t);
                    gradTex.SetPixel(x, y, col);
                }
            }
            gradTex.Apply();
            return gradTex;
        }

        public static Texture2D CreateVertical(Gradient grad, int width = 32, int height = 1)
        {
            var gradTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            gradTex.filterMode = FilterMode.Bilinear;
            float inv = 1f / (height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var t = y * inv;
                    Color col = grad.Evaluate(t);
                    gradTex.SetPixel(x, y, col);
                }
            }
            gradTex.Apply();
            return gradTex;
        }
    }

#if UNITY_EDITOR
    public class GradientTexCreator : EditorWindow
    {

        enum GradientDir 
        {
            Horizontal,
            Vertical
        }

        [SerializeField] Gradient gradient;

        static int width = 128;
        static int height = 16;
        static GradientDir direction;
        static string fileName = "Gradient";
        static string texSavePath;

        [MenuItem("Tools/47E/GradientTex")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(GradientTexCreator));
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            SerializedObject so = new SerializedObject(this);
            EditorGUILayout.PropertyField(so.FindProperty("gradient"), true, null);
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("width", GUILayout.Width(80f));
                int.TryParse(GUILayout.TextField(width.ToString(), GUILayout.Width(120f)), out width);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("height", GUILayout.Width(80f));
                int.TryParse(GUILayout.TextField(height.ToString(), GUILayout.Width(120f)), out height);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("direction", GUILayout.Width(80f));
                direction = (GradientDir)EditorGUILayout.Popup((int)direction, new string[] { "Horizontal", "Vertical" });
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("name", GUILayout.Width(80f));
                fileName = GUILayout.TextField(fileName, GUILayout.Width(120f));
                GUILayout.Label(".png");
            }

            if (GUILayout.Button("Save"))
            {
                texSavePath = EditorUtility.SaveFolderPanel("Select an output path", "", "");
                if (texSavePath.Length > 0)
                {
                    Texture2D tex = default;
                    switch (direction)
                    {
                        case GradientDir.Horizontal:
                            tex = GradientTexGen.CreateHorizontal(gradient, width, height);
                            break;
                        case GradientDir.Vertical:
                            tex = GradientTexGen.CreateVertical(gradient, width, height);
                            break;
                    }
                    byte[] pngData = tex.EncodeToPNG();
                    File.WriteAllBytes(texSavePath + "/" + fileName + ".png", pngData);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
#endif

}