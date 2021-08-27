using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Treasured.SDKEditor
{
    internal abstract class TreasuredEditor<T> : Editor where T : MonoBehaviour
    {
        protected static readonly Color LabelColor = Color.white;

        private static readonly string[] Excludes = new string[] { "m_Script" };

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
                EditorGUILayout.PropertyField(iterator);
            }
            iterator.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            Handles.color = LabelColor;
            Handles.BeginGUI();
            Handles.Label(Target.transform.position, Target.gameObject.name);
            Handles.EndGUI();
        }
    }
}
