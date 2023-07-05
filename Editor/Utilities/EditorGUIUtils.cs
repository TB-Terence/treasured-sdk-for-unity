using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Utilities
{
    internal sealed class EditorGUIUtils
    {
        public static readonly string[] ExcludingProperties = new string[] { "m_Script" };

        /// <summary>
        /// Draw properties excluding the ones defined in <see cref="ExcludingProperties"/>
        /// </summary>
        /// <param name="serializedObject"></param>
        public static void DrawProperties(SerializedObject serializedObject)
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ExcludingProperties);
            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawPropertyWithoutFoldout(SerializedProperty serializedProperty)
        {
            SerializedProperty iterator = serializedProperty.Copy();
            iterator.serializedObject.Update();
            SerializedProperty endProperty = serializedProperty.GetEndProperty();
            bool enterChildren = true;
            using (new EditorGUILayout.VerticalScope())
            {
                while (iterator.NextVisible(enterChildren))
                {
                    if (endProperty != null && iterator.propertyPath == endProperty.propertyPath)
                    {
                        break;
                    }
                    enterChildren = false;
                    if (!ExcludingProperties.Contains(iterator.name))
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                        if (GUI.changed)
                        {
                            iterator.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }

        public static void DrawPropertiesExcluding(SerializedObject serializedObject, params string[] propertyToExclude)
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public static float DrawPropertiesExcluding(Rect rect, SerializedObject serializedObject, params string[] propertyToExclude)
        {
            float totalHeight = 0;
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    var height = EditorGUI.GetPropertyHeight(iterator);
                    Rect pRect = new Rect(rect.x, rect.y + totalHeight, rect.width, height);
                    EditorGUI.PropertyField(pRect, iterator, true);
                    totalHeight += height + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return totalHeight;
        }

        public static float GetPropertiesHeightExcluding(SerializedObject serializedObject, params string[] propertyToExclude)
        {
            float totalHeight = 0;
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    var height = EditorGUI.GetPropertyHeight(iterator);
                    totalHeight += height + 2;
                }
            }
            return totalHeight;
        }
    }
}
