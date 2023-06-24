//using UnityEngine;
//using UnityEditor;

//namespace Treasured.UnitySdk
//{
//    [CustomPropertyDrawer(typeof(StartTourAction))]
//    public class StartTourActionDrawer : PropertyDrawer
//    {
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            EditorGUI.BeginProperty(position, label, property);
//            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
//            if (property.isExpanded)
//            {
//                SerializedProperty targetProperty = property.FindPropertyRelative(nameof(StartTourAction.target));
//                Rect buttonRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Target"));
//                if (EditorGUI.DropdownButton(buttonRect, new GUIContent(targetProperty.objectReferenceValue ? (targetProperty.objectReferenceValue as GuidedTour)?.title : "Select Tour"), FocusType.Passive))
//                {
//                    GenericMenu menu = new GenericMenu();
//                    foreach (var tour in GuidedTourGraphEditor.Current?.tours)
//                    {
//                        menu.AddItem(new GUIContent(tour.title), false, () =>
//                        {
//                            targetProperty.objectReferenceValue = tour;
//                            targetProperty.serializedObject.ApplyModifiedProperties();
//                        });
//                    }
//                    menu.ShowAsContext();
//                }
//            }
//            EditorGUI.EndProperty();
//        }

//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            return base.GetPropertyHeight(property, label) + (property.isExpanded ? EditorGUIUtility.singleLineHeight * 1 + EditorGUIUtility.standardVerticalSpacing : 0);
//        }
//    }
//}
