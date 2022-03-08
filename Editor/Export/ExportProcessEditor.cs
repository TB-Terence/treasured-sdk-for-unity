using UnityEditor;

namespace Treasured.UnitySdk.Export
{
    [CustomEditor(typeof(ExportProcess), true)]
    internal class ExportProcessEditor : UnityEditor.Editor
    {
        public virtual void OnPreferenceGUI()
        {

        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtilities.DrawPropertiesExcluding(serializedObject, "m_Script", "enable");
        }
    }
}
