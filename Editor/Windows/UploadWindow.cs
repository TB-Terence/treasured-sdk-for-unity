using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class UploadWindow : EditorWindow
    {
        [MenuItem("Tools/Treasured/Upload", priority = 0)]
        public static UploadWindow ShowUploadWindow()
        {
            var window = EditorWindow.GetWindow<UploadWindow>();
            window.titleContent = new GUIContent("Upload");
            window.Show();
            return window;
        }
    }
}