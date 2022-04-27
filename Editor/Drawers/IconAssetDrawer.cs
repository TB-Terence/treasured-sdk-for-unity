using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(IconAsset))]
    internal class IconAssetDrawer : Editor
    {
        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if ((target as IconAsset).icon.IsNullOrNone())
            {
                return;
            }
            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(r, (target as IconAsset).icon, ScaleMode.ScaleToFit);
            }
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }
    }
}
