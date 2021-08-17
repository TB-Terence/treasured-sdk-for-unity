﻿using UnityEngine;
using UnityEditor;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(Hitbox))]
    public class HitboxPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty centerProp = property.FindPropertyRelative("_center");
            SerializedProperty sizeProp = property.FindPropertyRelative("_size");
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, property.isExpanded ? position.width - 40 : position.width , 20), property.isExpanded, "Hitbox", true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                centerProp.vector3Value = EditorGUI.Vector3Field(new Rect(position.x, position.y + 20, position.width, 20), new GUIContent("Center", "Position offset of the hitbox relative to the position of the transform."),  centerProp.vector3Value);
                sizeProp.vector3Value = EditorGUI.Vector3Field(new Rect(position.x, position.y + 40, position.width, 20), new GUIContent("Size", "Size of the hitbox."),  sizeProp.vector3Value);
                EditorGUI.indentLevel--;
                if (GUI.Button(new Rect(position.xMax - 40, position.y, 20, 20), EditorGUIUtility.TrIconContent("BoxCollider Icon", "Use Selected Game Object Collider"), EditorStyles.label))
                {
                    if(Selection.activeGameObject)
                    {

                        Collider[] colliders = Selection.activeGameObject.GetComponents<Collider>();
                        if (colliders.Length == 1)
                        {
                            centerProp.vector3Value = colliders[0].bounds.center;
                            sizeProp.vector3Value = colliders[0].bounds.size;
                            GUI.FocusControl(null);
                        }
                        else if (colliders.Length > 1 )
                        {
                            GenericMenu menu = new GenericMenu();
                            for (int i = 0; i < colliders.Length; i++)
                            {
                                menu.AddItem(new GUIContent(colliders[i].GetType().Name), false, () =>
                                {
                                    centerProp.vector3Value = colliders[i].bounds.center;
                                    sizeProp.vector3Value = colliders[i].bounds.size;
                                    GUI.FocusControl(null);
                                });
                            }
                            menu.ShowAsContext();
                        }
                    }
                }
                if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("RaycastCollider Icon", "Raycast to Floor"), EditorStyles.label))
                {
                    string path = property.propertyPath;
                    path = $"{path.Substring(0, path.LastIndexOf('.'))}._transform._position";
                    SerializedProperty positionProp = property.serializedObject.FindProperty(path);
                    if (positionProp != null)
                    {
                        Ray ray = new Ray(positionProp.vector3Value, positionProp.vector3Value + Vector3.down * 100);
                        if (Physics.Raycast(ray, out var hit))
                        {
                            centerProp.vector3Value = hit.transform.position;
                        }
                        else
                        {
                            centerProp.vector3Value = Vector3.zero;
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? 60 : 20;
        }
    }
}
