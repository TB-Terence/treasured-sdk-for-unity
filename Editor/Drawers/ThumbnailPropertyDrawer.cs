using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    //[CustomPropertyDrawer(typeof(Thumbnail))]
    public class ThumbnailPropertyDrawer : PropertyDrawer
    {
        static readonly float SingleLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty type = property.FindPropertyRelative(nameof(Thumbnail.type));
            Rect typeRect = new Rect(position.x, headerRect.yMax, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(typeRect, type);
            switch (type.enumValueIndex)
            {
                case (int)ThumbnailType.FromHotspot:
                    //SerializedProperty hotspotIndex = property.FindPropertyRelative(nameof(Thumbnail.hotspotIndex));
                    //if(Selection.activeGameObject.TryGetComponent<TreasuredScene>(out var scene) && scene != null)
                    //{
                    //    string[] hotspotNames = scene.Hotspots.Select(x => x.name).ToArray();
                    //    if (EditorGUI.DropdownButton(new Rect(position.x, typeRect.yMax, position.width, EditorGUIUtility.singleLineHeight), new GUIContent(hotspotNames[hotspotIndex.intValue]), FocusType.Passive))
                    //    {
                    //        HotspotSelectionPopup hotspotSelectionPopup = new HotspotSelectionPopup();
                    //    }
                    //    //hotspotIndex.intValue = EditorGUI.Popup(new Rect(position.x, typeRect.yMax, position.width, EditorGUIUtility.singleLineHeight), "Hotspot", hotspotIndex.intValue, scene.Hotspots.Select(x => x.name).ToArray());
                    //}
                    break;
                //case (int)Thumbnail.ThumbnailType.Custom:
                //    SerializedProperty imageInfo = property.FindPropertyRelative(nameof(Thumbnail.customImage));
                //    float height = EditorGUI.GetPropertyHeight(imageInfo);
                //    EditorGUI.PropertyField(new Rect(position.x, typeRect.yMax, position.width, height), imageInfo, true);
                //    break;
            }
            EditorGUI.indentLevel--;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //return 0;
            SerializedProperty type = property.FindPropertyRelative(nameof(Thumbnail.type));
            switch (type.enumValueIndex)
            {
                case (int)ThumbnailType.FromCurrentView:
                    return SingleLineHeight * 2 - EditorGUIUtility.standardVerticalSpacing;
                case (int)ThumbnailType.FromHotspot:
                    return SingleLineHeight * 3 - EditorGUIUtility.standardVerticalSpacing;
                //case (int)Thumbnail.ThumbnailType.Custom:
                //    SerializedProperty imageInfo = property.FindPropertyRelative(nameof(Thumbnail.customImage));
                //    return SingleLineHeight * 2 + EditorGUI.GetPropertyHeight(imageInfo);
                default:
                    return base.GetPropertyHeight(property, label);
            }
        }

        class HotspotSelectionPopup : PopupWindowContent
        {
            public override void OnGUI(Rect rect)
            {

            }
        }
    }
}
