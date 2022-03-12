using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Exporter), true)]
    internal class ExporterEditor : Editor
    {
        public virtual void OnPreferenceGUI()
        {

        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtilities.DrawPropertiesExcluding(serializedObject, "m_Script");
        }
    }
}
