using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal abstract class TreasuredObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Move Scene View", GUILayout.Height(24f)))
            {
                (target as TreasuredObject)?.TryInvokeMethods("OnSceneViewFocus");
            }
            EditorGUILayout.Space(4);
        }
    }
}
