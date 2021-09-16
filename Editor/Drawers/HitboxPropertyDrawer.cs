using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(Hitbox))]
    public class HitboxPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty centerProp = property.FindPropertyRelative("center");
            SerializedProperty sizeProp = property.FindPropertyRelative("size");
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width - 40, 20), property.isExpanded, "Hitbox", true);
            if (GUI.Button(new Rect(position.xMax - 40, position.y, 20, 20), EditorGUIUtility.TrIconContent("BoxCollider Icon", "Use box collider value(in world space) from selected game gbject."), EditorStyles.label))
            {
                if (Selection.activeGameObject)
                {
                    BoxCollider[] colliders = Selection.activeGameObject.GetComponents<BoxCollider>();
                    if (colliders.Length == 1)
                    {
                        centerProp.vector3Value = colliders[0].bounds.center;
                        sizeProp.vector3Value = colliders[0].size;
                        GUI.FocusControl(null);
                    }
                    else if (colliders.Length > 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.allowDuplicateNames = true;
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            int index = i;
                            menu.AddItem(new GUIContent($"center: {colliders[index].center} | size: {colliders[index].size}"), false, () =>
                            {
                                centerProp.vector3Value = colliders[index].bounds.center;
                                sizeProp.vector3Value = colliders[index].size;
                                GUI.FocusControl(null);
                                property.serializedObject.ApplyModifiedProperties();
                            });
                        }
                        menu.ShowAsContext();
                    }
                }
                else
                {
                    Debug.LogWarning("No game object is selected.");
                }
            }
            if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("RaycastCollider Icon", "Place hitbox on floor."), EditorStyles.label))
            {
                if (property.serializedObject.targetObject is Hotspot obj && Physics.Raycast(obj.cameraTransform.position, obj.cameraTransform.position - obj.cameraTransform.position + Vector3.down, out var hit))
                {
                    if (sizeProp.vector3Value == Vector3.zero)
                    {
                        sizeProp.vector3Value = Vector3.one;
                    }
                    float sizeHeight = sizeProp.vector3Value.y;
                    centerProp.vector3Value = hit.point + new Vector3(0, sizeHeight / 2, 0);
                }
            }
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect centerRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 20, position.width, 20), new GUIContent("Center", "Position of the hitbox in world space."));
                centerProp.vector3Value = EditorGUI.Vector3Field(centerRect, "", centerProp.vector3Value);
                Rect sizeRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 20), new GUIContent("Size", "Size of the hitbox."));
                sizeProp.vector3Value = EditorGUI.Vector3Field(sizeRect, "", sizeProp.vector3Value);
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
