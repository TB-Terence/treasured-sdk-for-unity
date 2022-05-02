using System.Linq;
using UnityEditor;

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
    }
}
