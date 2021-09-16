using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class WindowContextMenu
    {
        [MenuItem("Plugins/Treasured/Map Editor", priority = 0)] // every 11 increments in priority adds a separator
        static void ShowTreasuredMapEditorWindow()
        {
            var window = EditorWindow.GetWindow<TreasuredMapEditorWindow>();
            window.titleContent = new GUIContent("Treasured Map Editor");
            window.Show();
        }

        [MenuItem("Plugins/Treasured/Upload", priority = 11)]
        static void ShowUploadWindow()
        {
            var window = EditorWindow.GetWindow<UploadWindow>();
            window.titleContent = new GUIContent("Upload");
            window.Show();
        }

        //[MenuItem("Plugins/Treasured/Guide")]
        static void OpenGuide()
        {
            // TODO : Add guide page
        }

        [MenuItem("Plugins/Treasured/About", priority = 22)]
        static void ShowAboutWindow()
        {
            var window = EditorWindow.GetWindow<AboutWindow>(true);
            window.titleContent = new GUIContent("About Treasured SDK for Unity");
            window.position = new Rect(200, 200, 300, 300);
            window.Show();
        }
    }
}
