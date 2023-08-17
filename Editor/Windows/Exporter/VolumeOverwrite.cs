using System;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(VolumeOverwrite))]
    class VolumeOverwrite : ExporterWindowMenuItem
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Volume Parameter Overwrites", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                foreach (var overwrite in Scene.cubemapExporter.OverwritableComponents)
                {
                        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(overwrite.Key.Name), EditorStyles.boldLabel);
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        foreach (var parameter in overwrite.Value)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                parameter.Enabled = EditorGUILayout.ToggleLeft($"{ObjectNames.NicifyVariableName(parameter.FieldName)}{(parameter.GlobalOnly ? "(Global)" : String.Empty)}", parameter.Enabled);
                                using (new EditorGUI.DisabledGroupScope(!parameter.Enabled))
                                {
                                    switch (parameter.OverwriteValue)
                                    {
                                        case Enum e:
                                            parameter.OverwriteValue = EditorGUILayout.EnumPopup((Enum)parameter.OverwriteValue);
                                            break;
                                        case float f:
                                            parameter.OverwriteValue = EditorGUILayout.FloatField((float)parameter.OverwriteValue);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
