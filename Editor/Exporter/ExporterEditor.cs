using Treasured.UnitySdk.Utilities;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Exporter), true)]
    internal class ExporterEditor : UnityEditor.Editor
    {
        public virtual void OnPreferenceGUI()
        {

        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtils.DrawPropertiesExcluding(serializedObject, "m_Script");
        }
    }
}
