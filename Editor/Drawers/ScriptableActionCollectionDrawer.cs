using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ScriptableActionCollection), true)]
    public class ScriptableActionCollectionDrawer : PropertyDrawer
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