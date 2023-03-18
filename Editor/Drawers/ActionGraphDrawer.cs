using System;
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
        private struct Tab
        {
            public string name;
            public ActionListDrawer<ScriptableAction> drawer;
        }

        private List<Tab> tabs;
        private List<string> names;
        private int _selectedIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (tabs == null)
            {
                tabs = new List<Tab>();
                names = new List<string>();
                SerializedProperty groups = property.FindPropertyRelative("_groups");
                for(int i = 0; i < groups.arraySize; i++)
                {
                    SerializedProperty collection = groups.GetArrayElementAtIndex(i);
                    string name = ObjectNames.NicifyVariableName(collection.FindPropertyRelative("name").stringValue);
                    names.Add(name);
                    tabs.Add(new Tab()
                    {
                        name = name,
                        drawer = new ActionListDrawer<ScriptableAction>(collection.serializedObject, collection.FindPropertyRelative("_actions"), "")
                    });
                }
            }
            _selectedIndex = GUI.SelectionGrid(position, _selectedIndex, tabs.Select(t => t.name).ToArray(), tabs.Count, TreasuredMapEditor.Styles.TabButton);
            if (_selectedIndex >= 0 && _selectedIndex < tabs.Count)
            {
                tabs[_selectedIndex].drawer.OnGUILayout();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // this is actually the tab button height
            return 32;
        }
    }
}
