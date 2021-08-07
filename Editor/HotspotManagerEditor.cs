using Treasured.ExhibitX;
using UnityEditor;
using UnityEngine;

namespace Treasured.ExhibitXEditor
{
    [CustomEditor(typeof(HotspotManager))]
    public class HotspotManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Hotspot Manager Window", GUILayout.Height(36)))
            {
                TreasuredToolkitWindow.OpenWindow();
            }
        }

    }
}
