using System.Collections.Generic;
using System.Linq;
using Treasured.Actions;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ActionGraph))]
    public class ActionGraphDrawer : PropertyDrawer
    {
        private class Tab
        {
            public string name;
            public SerializedProperty serializedProperty;
        }

        private List<Tab> tabs;
        private int selectedIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            selectedIndex = GUI.SelectionGrid(new Rect(position.x, position.y, position.width, 32), selectedIndex, tabs.Select(t => t.name).ToArray(), tabs.Count, TreasuredSceneEditor.Styles.TabButton);
            if (selectedIndex >= 0 && selectedIndex < tabs.Count)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + 32, position.width, GetPropertyHeight(property, label) - 32), tabs[selectedIndex].serializedProperty, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (tabs == null)
            {
                tabs = new List<Tab>();
                SerializedProperty groups = property.FindPropertyRelative("_groups");
                for (int i = 0; i < groups.arraySize; i++)
                {
                    SerializedProperty collection = groups.GetArrayElementAtIndex(i);
                    var tab = new Tab();
                    tab.name = ObjectNames.NicifyVariableName(collection.FindPropertyRelative("name").stringValue);
                    tab.serializedProperty = collection;
                    tabs.Add(tab);
                }
            }
            // 32 is tab button height
            return 32 + EditorGUI.GetPropertyHeight(tabs[selectedIndex].serializedProperty);
        }
    }
}
