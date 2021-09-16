using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(SelectObjectAction))]
    public class SelectObjectActionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty targetId = property.FindPropertyRelative("_targetId");

            position.height = 18;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, 
                ObjectNames.NicifyVariableName(property.managedReferenceFullTypename.Substring(property.managedReferenceFullTypename.LastIndexOf('.') + 1)), true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                position.y += 20;
                Rect rect = EditorGUI.PrefixLabel(position, new GUIContent("Target Id"));
                if (EditorGUI.DropdownButton(rect, new GUIContent(GetObjectName(targetId)), FocusType.Passive))
                {
                    ShowTargetList(property);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? 40 : 20;
        }

        void ShowTargetList(SerializedProperty property)
        {
            if(!(property.serializedObject.targetObject is ObjectBase obj) || obj.Map == null)
            {
                return;
            }
            GenericMenu menu = new GenericMenu();
            foreach (var o in obj.Map.GetObjects())
            {
                menu.AddItem(new GUIContent($"{(o.GetType() == typeof(Hotspot) ? "Hotspots" : "Interactables")}/{o.name}"), false, () =>
                {
                    SerializedProperty targetId = property.FindPropertyRelative("_targetId");
                    targetId.stringValue = o.Id;
                    targetId.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        string GetObjectName(SerializedProperty property)
        {
            ObjectBase obj = property.serializedObject.targetObject as ObjectBase;
            if (obj && obj.Map != null && obj.Map.TryGetObject(property.stringValue, out ObjectBase result))
            {
                return result.name;
            }
            return string.Empty;
        }
    }
}
