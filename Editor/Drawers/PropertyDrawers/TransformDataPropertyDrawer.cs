using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using Treasured.UnitySdk;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(TransformData))]
    public class TransformDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty positionProp = property.FindPropertyRelative("_position");
            SerializedProperty rotationProp = property.FindPropertyRelative("_rotation");
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, property.isExpanded ? position.width - 20 : position.width , 20), property.isExpanded, "Transform", true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect postitionRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 20, position.width, 20), new GUIContent("Position", "Position of the transform in world space."));
                positionProp.vector3Value = EditorGUI.Vector3Field(postitionRect, "", positionProp.vector3Value);
                Rect rotationRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 20), new GUIContent("Rotation", "Rotation of the transform."));
                rotationProp.vector3Value = EditorGUI.Vector3Field(rotationRect,"",  rotationProp.vector3Value);
                EditorGUI.indentLevel--;
                if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("Transform Icon", "Use Selected Game Object Transform"), EditorStyles.label))
                {
                    if(Selection.activeGameObject == null)
                    {
                        Debug.LogWarning("No game object is selected.");
                    }
                    else
                    {
                        positionProp.vector3Value = Selection.activeGameObject.transform.position;
                        rotationProp.vector3Value = Selection.activeGameObject.transform.rotation.eulerAngles;
                        GUI.FocusControl(null);
                    }
                }
                //using (new EditorGUI.DisabledGroupScope(true))
                //{
                //    if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("UnityEditor.SceneView", "Use Scene View Camera Transform"), EditorStyles.label))
                //    {
                //        positionProp.vector3Value = SceneView.lastActiveSceneView.camera.transform.position;
                //        rotationProp.vector3Value = SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles;
                //        GUI.FocusControl(null);
                //    }
                //}
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? 60 : 20;
        }
    }
}
