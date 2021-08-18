using UnityEngine;

namespace Treasured.SDKEditor
{
    public class AssetSelectorAttribute : PropertyAttribute
    {
        public bool IsFolder { get; set; }

        public AssetSelectorAttribute(bool isFolder)
        {
            IsFolder = isFolder;
        }
    }
}