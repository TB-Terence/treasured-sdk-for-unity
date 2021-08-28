using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    internal class TreasuredDataEditorSettings : ScriptableObject
    {
        // Editor Config
        [SerializeField]
        [AssetSelector()]
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