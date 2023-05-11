using System;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class UISettings
    {
        public bool showHotspotButtons = true;
        public bool showInteractableButtons = true;
        public bool fadeOutButtons = true;
        public bool showPreviewOnNav = false;
        public bool projectDomeOntoGeometry = true;
        public bool showOnboarding = false;
        public bool showCursor = true;
        [UnityEngine.HideInInspector]
        public bool darkMode = false;
    }
}
