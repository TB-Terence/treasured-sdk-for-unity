using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(Thumbnail))]
    public class ThumbnailPropertyDrawer : PropertyDrawer
    {
        static readonly float SingleLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty type = property.FindPropertyRelative(nameof(Thumbnail.type));
            Rect typeRect = new Rect(position.x, headerRect.yMax, position.width, EditorGUIUtility.singleLineHeight);
            switch (type.enumValueIndex)
            {
                case (int)Thumbnail.ThumbnailType.FromHotspot:
                    SerializedProperty hotspotIndex = property.FindPropertyRelative(nameof(Thumbnail.hotspotIndex));
                    EditorGUI.PropertyField(new Rect(position.x, typeRect.yMax, position.width, EditorGUIUtility.singleLineHeight), hotspotIndex);
                    break;
                case (int)Thumbnail.ThumbnailType.Custom:
                    SerializedProperty imageInfo = property.FindPropertyRelative(nameof(Thumbnail.customImage));
                    float height = EditorGUI.GetPropertyHeight(imageInfo);
                    EditorGUI.PropertyField(new Rect(position.x, typeRect.yMax, position.width, height), imageInfo, true);
                    break;
            }
            EditorGUI.PropertyField(typeRect, type);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty type = property.FindPropertyRelative(nameof(Thumbnail.type));
            switch (type.enumValueIndex)
            {
                case (int)Thumbnail.ThumbnailType.FromCurrentView:
                    return SingleLineHeight * 2;
                case (int)Thumbnail.ThumbnailType.FromHotspot:
                    return SingleLineHeight * 3;
                case (int)Thumbnail.ThumbnailType.Custom:
                    SerializedProperty imageInfo = property.FindPropertyRelative(nameof(Thumbnail.customImage));
                    return SingleLineHeight * 2 + EditorGUI.GetPropertyHeight(imageInfo);
                default:
                    return base.GetPropertyHeight(property, label);
            }
        }
    }
}
