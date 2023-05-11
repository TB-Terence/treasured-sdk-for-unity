using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ScriptableActionCollection), true)]
    public class ScriptableActionCollectionEditor : UnityEditor.Editor
    {
        private ActionListDrawer<ScriptableAction> listDrawer;
        ReorderableList reorderableList;

        private void OnEnable()
        {
            listDrawer = new ActionListDrawer<ScriptableAction>(serializedObject, serializedObject.FindProperty("_actions"), "Actions");
        }

        public override void OnInspectorGUI()
        {
            listDrawer.OnGUILayout();
        }
    }

    [CustomPropertyDrawer(typeof(ScriptableActionCollection), true)]
    public class ScriptableActionCollectionDrawer : UnityEditor.PropertyDrawer
    {
        private ActionListDrawer<ScriptableAction> listDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (listDrawer == null)
            {
                listDrawer = new ActionListDrawer<ScriptableAction>(property.serializedObject, property.FindPropertyRelative("_actions"), property.displayName);
            }
            listDrawer.OnGUI(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return listDrawer == null || listDrawer.reorderableList == null ? 0 : listDrawer.reorderableList.GetHeight();
        }
    }
}
