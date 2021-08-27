using Treasured.SDK;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(AssetSelectorAttribute))]
    public class AssetSelectorAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, "Asset Selector Attribute can only be used on string type.");
            }
            AssetSelectorAttribute attr = attribute as AssetSelectorAttribute;
            EditorGUI.BeginProperty(position, label, property);
            Rect rect = EditorGUI.PrefixLabel(position, label);
            GUI.TextField(new Rect(rect.x, rect.y, rect.width - 36, rect.height), AssetDatabase.GUIDToAssetPath(property.stringValue));
            if (GUI.Button(new Rect(rect.x + rect.width - 36, rect.y, 18, rect.height), EditorGUIUtility.TrIconContent("FolderOpened Icon", $"Select {(attr.IsFolder ? "folder" : "asset")}"), EditorStyles.label))
            {
                string selectedPath = attr.IsFolder ? EditorUtility.OpenFolderPanel("Choose folder", Application.dataPath, "") : EditorUtility.OpenFilePanel("Choose asset", Application.dataPath, "asset");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (!selectedPath.StartsWith(Application.dataPath))
                    {
                        Debug.LogWarning($"The selected {(attr.IsFolder ? "folder" : "asset")} is outside the Asset folder.");
                    }
                    else
                    {
                        property.stringValue = AssetDatabase.AssetPathToGUID(selectedPath.Substring(Application.dataPath.Length - 6));
                        GUI.FocusControl(null); // clear focus on textfield if text is cleared
                    }
                }
            }
            if (GUI.Button(new Rect(rect.x + rect.width - 18, rect.y, 18, rect.height), EditorGUIUtility.TrIconContent("winbtn_win_close", "Reset"), EditorStyles.label))
            {
                property.stringValue = "";
            }
            EditorGUI.EndProperty();
        }
    }

}