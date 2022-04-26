using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class AboutWindow : EditorWindow
    {

        [MenuItem("Tools/Treasured/About", priority = 11)] // every 11 increments in priority adds a separator
        static void ShowAboutWindow()
        {
            var window = EditorWindow.GetWindow<AboutWindow>(true);
            window.titleContent = new GUIContent("About Treasured SDK for Unity");
            window.position = new Rect(200, 200, 300, 300);
            window.Show();
        }

        private void OnGUI()
        {
            //EditorGUILayout.LabelField(new GUIContent(Styles.TreasuredIcon), GUILayout.MinWidth(100), GUILayout.MinHeight(100));
            EditorGUILayout.LabelField(new GUIContent("Version"), new GUIContent(TreasuredMap.Version));
            if (EditorGUILayoutUtils.Link(new GUIContent("Website"), new GUIContent("https://treasured.ca/")))
            {
                Application.OpenURL("https://treasured.ca/");
            }
            if (EditorGUILayoutUtils.Link(new GUIContent("Email"), new GUIContent("team@treasured.ca")))
            {
                Application.OpenURL("mailto:team@treasured.ca");
            }
        }
    }
}