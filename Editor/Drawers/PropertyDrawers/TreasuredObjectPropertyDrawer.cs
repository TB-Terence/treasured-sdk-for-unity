using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
#if UNITY_2020_3_OR_NEWER
    [CustomPropertyDrawer(typeof(SDK.TreasuredObject))]
#endif
    public class TreasuredObjectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty idProp = property.FindPropertyRelative("_id");
            SerializedProperty nameProp = property.FindPropertyRelative("_name");
            SerializedProperty descriptionProp = property.FindPropertyRelative("_description");
            SerializedProperty transformProp = property.FindPropertyRelative("_transform");
            SerializedProperty hitboxProp = property.FindPropertyRelative("_hitbox");
            SerializedProperty onSelectedProp = property.FindPropertyRelative("_onSelected");

            if (string.IsNullOrEmpty(idProp.stringValue))
            {
                idProp.stringValue = Guid.NewGuid().ToString();
            }

            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, 18), property.isExpanded, string.IsNullOrEmpty(nameProp.stringValue) ? label : new GUIContent(nameProp.stringValue), true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 20, position.width, 18), idProp);
                }

                EditorGUI.PropertyField(new Rect(position.x, position.y + 40, position.width, 18), nameProp);

                float descriptionPropHeight = EditorGUI.GetPropertyHeight(descriptionProp);
                EditorGUI.PropertyField(new Rect(position.x, position.y + 60, position.width, descriptionPropHeight), descriptionProp);

                float transformPropHeight = EditorGUI.GetPropertyHeight(transformProp);
                EditorGUI.PropertyField(new Rect(position.x, position.y + 60 + descriptionPropHeight, position.width, transformPropHeight), transformProp, true);

                float hitboxPropHeight = EditorGUI.GetPropertyHeight(hitboxProp);
                EditorGUI.PropertyField(new Rect(position.x, position.y + 60 + descriptionPropHeight + transformPropHeight, position.width, hitboxPropHeight), hitboxProp, true);

                float onSelectedPropHeight = EditorGUI.GetPropertyHeight(onSelectedProp);
                EditorGUI.PropertyField(new Rect(position.x, position.y + 62 + descriptionPropHeight + hitboxPropHeight + transformPropHeight, position.width, onSelectedPropHeight), onSelectedProp, true);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !property.isExpanded ? 20 : EditorGUI.GetPropertyHeight(property, true);
        }
    }
}