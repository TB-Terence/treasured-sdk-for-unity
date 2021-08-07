using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Treasured.ExhibitX;

namespace Treasured.ExhibitXEditor
{
    [CustomEditor(typeof(Hotspot))]
    public class HotspotEditor : UnityEditor.Editor
    {
        private bool _interactionFoldout = true;

        private Dictionary<UnityEngine.Object, bool> _foldouts = new Dictionary<UnityEngine.Object, bool>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawInteractions(serializedObject.FindProperty("interactions"));
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawInteractions(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            _interactionFoldout = EditorGUILayout.Foldout(_interactionFoldout, new GUIContent($"Interactions ({property.arraySize})"), true);
            if (GUILayout.Button(EditorGUIUtils.IconContent("Toolbar Plus More", "Create Interaction"), GUI.skin.label, GUILayout.MaxWidth(20)))
            {
                GenericMenu menu = new GenericMenu();
                var types = TypeCache.GetTypesDerivedFrom<InteractionData>();
                foreach (var type in types)
                {
                    string name = ObjectNames.NicifyVariableName(type.Name);
                    menu.AddItem(new GUIContent(name), false, () =>
                    {
                        property.arraySize++;
                        property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = ScriptableObject.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }
            if (GUILayout.Button(new GUIContent("X", "Remove All"), GUI.skin.label, GUILayout.MaxWidth(20)))
            {
                property.arraySize = 0;
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
            if (_interactionFoldout)
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    DrawElement(property, i);
                }
            }
        }

        private void DrawElement(SerializedProperty list, int index)
        {
            SerializedObject obj = new SerializedObject(list.GetArrayElementAtIndex(index).objectReferenceValue);
            SerializedProperty element = obj.GetIterator();
            if (!_foldouts.ContainsKey(element.serializedObject.targetObject))
            {
                _foldouts[element.serializedObject.targetObject] = true;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            _foldouts[element.serializedObject.targetObject] = EditorGUILayout.Foldout(_foldouts[element.serializedObject.targetObject], ObjectNames.NicifyVariableName(element.serializedObject.targetObject.GetType().Name), true);
            if (GUILayout.Button(new GUIContent("↑", index == 0 ? "Move to Last" : "Move Up"), GUI.skin.label, GUILayout.MaxWidth(16)))
            {
                if (index == 0)
                {
                    list.MoveArrayElement(index, list.arraySize - 1);
                }
                else
                {
                    list.MoveArrayElement(index, index - 1);
                }
            }
            if (GUILayout.Button(new GUIContent("↓", index == list.arraySize - 1 ? "Move to First" : "Move Down"), GUI.skin.label, GUILayout.MaxWidth(16)))
            {
                if (index == list.arraySize - 1)
                {
                    list.MoveArrayElement(index, 0);
                }
                else
                {
                    list.MoveArrayElement(index, index + 1);
                }
            }
            if (GUILayout.Button(EditorGUIUtils.IconContent("Toolbar Minus", "Remove"), GUI.skin.label, GUILayout.MaxWidth(16)))
            {
                if (obj.targetObject != null)
                {
                    list.DeleteArrayElementAtIndex(index); // Set the element at index to null
                }
                list.DeleteArrayElementAtIndex(index);
            }
            EditorGUILayout.EndHorizontal();
            if (_foldouts[element.serializedObject.targetObject])
            {
                element.Next(true);
                while (element.NextVisible(false))
                {
                    if (element.name == "m_Script")
                    {
                        continue;
                    }
                    EditorGUILayout.PropertyField(element);
                }
            }
            if (GUI.changed)
            {
                element.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
        }
    }
}

