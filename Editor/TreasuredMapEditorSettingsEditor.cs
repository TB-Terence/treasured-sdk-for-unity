using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(TreasuredMapEditorSettings))]
    internal sealed class TreasuredMapEditorSettingsEditor : Editor
    {
        static class Styles
        {
            public static readonly GUIContent DefaultOutputFolder = EditorGUIUtility.TrTextContent("Default Output Folder", "Default output folder in project root.");
            public static readonly GUIContent DefaultOutputFolderPath = EditorGUIUtility.TrTextContent(TreasuredMapExporter.DefaultOutputFolder);
            public static readonly GUIContent OpenInExplorerIcon = EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show In Explorer");

        }

        //private static Dictionary<string, FieldInfo> fieldInfos = new Dictionary<string, FieldInfo>();

        private SerializedProperty startUpAsset;
        private SerializedProperty cameraColor;
        private SerializedProperty hitboxColor;

        private void OnEnable()
        {
            startUpAsset = serializedObject.FindProperty(nameof(startUpAsset));
            cameraColor = serializedObject.FindProperty(nameof(cameraColor));
            hitboxColor = serializedObject.FindProperty(nameof(hitboxColor));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField(Styles.DefaultOutputFolder, Styles.DefaultOutputFolderPath);
            }
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            if (GUI.Button(new Rect(buttonRect.xMax - 22, buttonRect.y, 20, 20), Styles.OpenInExplorerIcon, EditorStyles.label))
            {
                Directory.CreateDirectory(TreasuredMapExporter.DefaultOutputFolderPath);
                Application.OpenURL(TreasuredMapExporter.DefaultOutputFolderPath);
            }

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChild = true;
            while(iterator.NextVisible(enterChild))
            {
                enterChild = false;
                if (iterator.name.Equals("m_Script"))
                {
                    continue;
                }
                EditorGUILayout.PropertyField(iterator, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
