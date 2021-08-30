using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditorInternal;
using Treasured.SDK;

namespace Treasured.UnitySdk.Editor
{
    internal abstract class TreasuredObjectEditor<T, D> : TreasuredEditor<T> where T : TreasuredObject, IDataComponent<D> where D : TreasuredObjectData
    {
        private static readonly string[] actionTypes = new string[]
        {
            "openLink",
            "showText",
            "playAudio",
            "playVideo",
            "selectObject"
        };

        protected ReorderableList list;

        protected SerializedProperty idProp;
        protected SerializedProperty descriptionProp;
        protected SerializedProperty onSelectedProp;

        protected override void Init()
        {
            base.Init();
            idProp = serializedObject.FindProperty("_data._id");
            descriptionProp = serializedObject.FindProperty("_data._description");
            onSelectedProp = serializedObject.FindProperty("_data._onSelected");
            list = new ReorderableList(serializedObject, onSelectedProp);
            list.drawHeaderCallback = OnDrawHeader;
            list.drawElementCallback = OnDrawElement;
            list.elementHeightCallback = GetElementHeight;
            list.onAddDropdownCallback = OnAddDropDownCallback;
            list.list = Target.Data.OnSelected;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.PropertyField(idProp);
            }
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField(new GUIContent("Name"), Target.gameObject.name);
            if (EditorGUI.EndChangeCheck() && newName.Length > 0)
            {
                Undo.RecordObject(Target.gameObject, $"Rename {ObjectNames.NicifyVariableName(typeof(T).Name)}");
                Target.gameObject.name = newName;
            }
            EditorGUILayout.PropertyField(descriptionProp);
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        void OnDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, new GUIContent("On Selected"));
        }

        void OnDrawFooter(Rect rect)
        {

        }

        void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.xMin += 10;
            SerializedProperty element = onSelectedProp.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
        }

        float GetElementHeight(int index)
        {
            return EditorGUI.GetPropertyHeight(onSelectedProp.GetArrayElementAtIndex(index));
        }

        void OnAddDropDownCallback(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < actionTypes.Length; i++)
            {
                string type = actionTypes[i];
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type)), false, () =>
                {
                    list.list.Add(new TreasuredAction() 
                    { 
                        Type = type
                    });
                });
            }
            menu.DropDown(buttonRect);
        }
    }
}
