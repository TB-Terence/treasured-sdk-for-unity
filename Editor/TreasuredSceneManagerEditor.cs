using System;
using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk.Utilities;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(TreasuredSceneManager))]
    public class TreasuredSceneManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUIUtils.DrawProperties(serializedObject);
        }
    }
}
