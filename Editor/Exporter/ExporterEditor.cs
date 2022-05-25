using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Exporter), true)]
    internal class ExporterEditor : UnityEditor.Editor
    {
        static Dictionary<Type, List<MethodInfo>> s_contextMenus = new Dictionary<Type, List<MethodInfo>>();
        [InitializeOnLoadMethod]
        static void CacheExporterContextMenu()
        {
            // TODO: Add support for base type methods
            var menuItems = UnityEditor.TypeCache.GetMethodsWithAttribute<ContextMenu>().Where(x => typeof(Exporter).IsAssignableFrom(x.DeclaringType));
            foreach (var menuItem in menuItems)
            {
                if (!s_contextMenus.ContainsKey(menuItem.DeclaringType))
                {
                    s_contextMenus.Add(menuItem.DeclaringType, new List<MethodInfo>());
                }
                s_contextMenus[menuItem.DeclaringType].Add(menuItem);
            }
        }

        internal class ExporterScope : GUI.Scope
        {
            public ExporterScope(Exporter exporter)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                exporter.enabled = EditorGUILayout.ToggleLeft(ObjectNames.NicifyVariableName(exporter.GetType().Name), exporter.enabled, EditorStyles.boldLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    if (exporter.enabled)
                    {
                        exporter.OnEnabled();
                    }
                    else
                    {
                        exporter.OnDisabled();
                    }
                }
                GUILayout.FlexibleSpace();
                if (s_contextMenus.TryGetValue(exporter.GetType(), out var menuItems))
                {
                    if (GUILayout.Button(GUIIcons.menu, EditorStyles.label))
                    {
                        GenericMenu menu = new GenericMenu();
                        foreach (var menuItem in menuItems)
                        {
                            ContextMenu m = menuItem.GetCustomAttribute<ContextMenu>();
                            menu.AddItem(new GUIContent(m == null || string.IsNullOrWhiteSpace(m.menuItem) ? ObjectNames.NicifyVariableName(menuItem.Name) : m.menuItem), false, () => menuItem.Invoke(exporter, null));
                        }
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();
            }

            protected override void CloseScope()
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        public virtual void OnPreferenceGUI(SerializedObject serializedObject)
        {
            OnInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUIUtils.DrawPropertiesExcluding(serializedObject, "m_Script");
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
