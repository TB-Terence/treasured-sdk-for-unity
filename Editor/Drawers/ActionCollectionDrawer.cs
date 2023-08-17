using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ActionCollection))]
    public class ActionCollectionDrawer : PropertyDrawer
    {
        private Dictionary<string, ActionListDrawer<ScriptableAction>> actionListDrawers = new Dictionary<string, ActionListDrawer<ScriptableAction>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            actionListDrawers[property.propertyPath].OnGUI(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!actionListDrawers.ContainsKey(property.propertyPath))
            {
                actionListDrawers[property.propertyPath] = new ActionListDrawer<ScriptableAction>(property.serializedObject, property.FindPropertyRelative("_actions"), "Actions");
            }
            return actionListDrawers[property.propertyPath].reorderableList.GetHeight();
        }
    }
}
