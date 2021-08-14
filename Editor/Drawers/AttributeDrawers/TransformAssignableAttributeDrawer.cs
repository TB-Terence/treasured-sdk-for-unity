using UnityEngine;
using UnityEditor;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(TransformAssignableAttribute))]
    public class TransformAssignableAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property, label, true);
            Event e = Event.current;
            if (position.Contains(e.mousePosition) && (e.type == EventType.DragUpdated || e.type == EventType.DragPerform))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    OnDrop(property);
                    e.Use();
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, property.hasChildren);
        }

        private void OnDrop(SerializedProperty property)
        {
            Transform transform = null;
            if (DragAndDrop.objectReferences[0] is GameObject go)
            {
                transform = go.transform;
            }
            else if (DragAndDrop.objectReferences[0] is Transform trans)
            {
                transform = trans;
            }
            if (transform != null)
            {
                if (fieldInfo.FieldType == typeof(Vector3))
                {
                    property.vector3Value = transform.position;
                }
                else if (fieldInfo.FieldType == typeof(Hitbox))
                {
                    SerializedProperty position = property.FindPropertyRelative("_position");
                    SerializedProperty rotation = property.FindPropertyRelative("_rotation");
                    position.vector3Value = transform.position;
                    rotation.vector3Value = transform.eulerAngles;
                }
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
        }
    }
}
