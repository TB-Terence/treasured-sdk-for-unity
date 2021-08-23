using UnityEngine;
using UnityEditor;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(Hitbox))]
    public class HitboxPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isHotspot = property.propertyPath.StartsWith("_hotspots");
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty centerProp = property.FindPropertyRelative("_center");
            SerializedProperty sizeProp = property.FindPropertyRelative("_size");
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, property.isExpanded ? position.width - 40 : position.width , 20), property.isExpanded, "Hitbox", true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect centerRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 20, position.width, 20), new GUIContent("Center", "Position of the hitbox in world space."));
                centerProp.vector3Value = EditorGUI.Vector3Field(centerRect, "",  centerProp.vector3Value);
                Rect sizeRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 20), new GUIContent("Size", "Size of the hitbox."));
                sizeProp.vector3Value = EditorGUI.Vector3Field(sizeRect, "",  sizeProp.vector3Value);
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
                if (isHotspot)
                {
                    if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("RaycastCollider Icon", "Raycast to Floor"), EditorStyles.label))
                    {
                        string path = property.propertyPath;
                        path = $"{path.Substring(0, path.LastIndexOf('.'))}._transform._position";
                        SerializedProperty positionProp = property.serializedObject.FindProperty(path);
                        if (positionProp != null)
                        {
                            if (Physics.Raycast(positionProp.vector3Value, positionProp.vector3Value - positionProp.vector3Value + Vector3.down, out var hit, 100))
                            {
                                if (sizeProp.vector3Value == Vector3.zero)
                                {
                                    sizeProp.vector3Value = Vector3.one;
                                }
                                float sizeHeight = sizeProp.vector3Value.y;
                                centerProp.vector3Value = hit.point + new Vector3(0, sizeHeight / 2, 0);
                            }
                            else
                            {
                                Debug.LogWarning($"No hit found. Make sure the position is above ground and the ground has a collider component. Maximum distance: 100");
                            }
                        }
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, 20), EditorGUIUtility.TrIconContent("Transform Icon", "Use Selected Game Object Transform"), EditorStyles.label))
                    {
                        if (Selection.activeGameObject == null)
                        {
                            Debug.LogWarning("No game object is selected.");
                        }
                        else
                        {
                            centerProp.vector3Value = Selection.activeGameObject.transform.position;
                            GUI.FocusControl(null);
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
