using UnityEngine;
using UnityEditor;

namespace Treasured.SDKEditor
{
    internal class TreasuredDataEditorSettings : ScriptableObject
    {
        // Editor Config
        [SerializeField]
        [AssetSelector(false)]
        private string _startUpData;

        // Export Config
        [SerializeField]
        private bool _showInExplorer = true;

        [SerializeField]
        [AssetSelector(true)]
        private string _defaultOutputFolder;

        public string StartUpDataAssetGUID { get => _startUpData; }
        public bool ShowInExplorer { get => _showInExplorer; set => _showInExplorer = value; }
        public string DefaultOutputFolderGUID { get => _defaultOutputFolder; }
    }
}