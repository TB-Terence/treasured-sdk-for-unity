using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Treasured.UnitySdk.Editor
{
    internal abstract class TreasuredEditor<T> : UnityEditor.Editor where T : MonoBehaviour
    {
        protected static readonly Color LabelColor = Color.white;

        private static readonly string[] Excludes = new string[] { "m_Script" };

        private Dictionary<string, Action> _beforePropertyField = new Dictionary<string, Action>();
        private Dictionary<string, Action> _afterPropertyField = new Dictionary<string, Action>();

        protected T Target { get; set; }

        /// <summary>
        /// Use Init instead.
        /// </summary>
        private void OnEnable()
        {
            Target = target as T;
            Init();
        }

        /// <summary>
        /// Used to initialize the Editor.
        /// </summary>
        protected virtual void Init() { }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator().Copy();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                if (Excludes.Contains(iterator.name))
                {
                    continue;
                }
                if (_beforePropertyField.TryGetValue(iterator.name, out Action beforeDrawer))
                {
                    beforeDrawer.Invoke();
                }
                EditorGUILayout.PropertyField(iterator);
                if (_afterPropertyField.TryGetValue(iterator.name, out Action afterDrawer))
                {
                    afterDrawer.Invoke();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            Handles.color = LabelColor;
            Handles.BeginGUI();
            Handles.Label(Target.transform.position, Target.gameObject.name);
            Handles.EndGUI();
        }

        protected void InjectDrawerBefore(string propertyName, Action drawer)
        {
            _beforePropertyField[propertyName] = drawer;
        }

        protected void InjectDrawerAfter(string propertyName, Action drawer)
        {
            _afterPropertyField[propertyName] = drawer;
        }
    }
}
