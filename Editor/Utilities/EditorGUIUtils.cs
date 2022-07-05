using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Utilities
{
    internal sealed class EditorGUIUtils
    {
        public static void DrawPropertiesExcluding(SerializedObject serializedObject, params string[] propertyToExclude)
        {
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
