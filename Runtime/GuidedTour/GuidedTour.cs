using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class GuidedTour : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get { return _id; } }
        public string title = "New Tour";
        [TextArea(3, 5)]
        public string description;
        public string thumbnailUrl;
        public ScriptableActionCollection actionScripts;
    }
}
