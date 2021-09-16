using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(TransformData))]
    public class TransformDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty positionProp = property.FindPropertyRelative("position");
            SerializedProperty eulerAnglesProp = property.FindPropertyRelative("eulerAngles");
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width - 20, 20), property.isExpanded, "Transform", true);
            if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("Transform Icon", "Set to Selected Game Object Transform"), EditorStyles.label))
            {
                if (Selection.activeGameObject == null)
                {
                    Debug.LogWarning("No game object is selected.");
                }
                else
                {
                    positionProp.vector3Value = Selection.activeGameObject.transform.position;
                    eulerAnglesProp.vector3Value = Selection.activeGameObject.transform.rotation.eulerAngles;
                    GUI.FocusControl(null);
                }
            }

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect postitionRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 20, position.width, 20), new GUIContent("Position", "Position of the transform in world space."));
                positionProp.vector3Value = EditorGUI.Vector3Field(postitionRect, "", positionProp.vector3Value);
                Rect rotationRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 20), new GUIContent("Rotation", "Rotation of the transform."));
                eulerAnglesProp.vector3Value = EditorGUI.Vector3Field(rotationRect, "", eulerAnglesProp.vector3Value);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? 60 : 20;
        }
    }
}
