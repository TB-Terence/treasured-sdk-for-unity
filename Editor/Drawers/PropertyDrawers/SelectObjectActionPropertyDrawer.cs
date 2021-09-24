using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk.Editor
{
    [CustomPropertyDrawer(typeof(SelectObjectAction))]
    public class SelectObjectActionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = 18;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded,
                ObjectNames.NicifyVariableName(property.managedReferenceFullTypename.Substring(property.managedReferenceFullTypename.LastIndexOf('.') + 1)), true);

            if (property.isExpanded)
            {
                SerializedProperty target = property.FindPropertyRelative("_target");
                EditorGUI.indentLevel++;
                position.y += 20;
                EditorGUI.BeginChangeCheck();
                var newObject = EditorGUI.ObjectField(position, new GUIContent("Target"), target.objectReferenceValue, typeof(TreasuredObject), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newObject is TreasuredObject obj && obj.gameObject.scene != null)
                    {
                        if (property.serializedObject.targetObject is TreasuredObject actionOwner)
                        {
                            if (newObject == actionOwner)
                            {
                                Debug.LogError("Can not assign self as the target.");
                            }
                            else
                            {
                                TreasuredMap map1 = (property.serializedObject.targetObject as TreasuredObject).Map;
                                TreasuredMap map2 = obj.Map;
                                if (map1.Equals(map2))
                                {
                                    target.objectReferenceValue = newObject;
                                }
                                else
                                {
                                    Debug.LogError("The object you are trying to assign does not belong to the same Map.");
                                }
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? 40 : 20;
        }
    }
}
