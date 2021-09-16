using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class AboutWindow : EditorWindow
    {
        private void OnGUI()
        {
            //EditorGUILayout.LabelField(new GUIContent(Styles.TreasuredIcon), GUILayout.MinWidth(100), GUILayout.MinHeight(100));
            EditorGUILayout.LabelField(new GUIContent("Version"), TreasuredMap.Version);
            if (EditorGUIUtils.Link(new GUIContent("Website"), new GUIContent("https://treasured.ca/")))
            {
                Application.OpenURL("https://treasured.ca/");
            }
            if (EditorGUIUtils.Link(new GUIContent("Email"), new GUIContent("team@treasured.ca")))
            {
                Application.OpenURL("mailto:team@treasured.ca");
            }
        }
    }
}
