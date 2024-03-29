﻿using Newtonsoft.Json;
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
        [HideInInspector]
        public bool isDefault = false;
        public string title = "New Tour";
        [TextArea(3, 5)]
        public string description = "";
        [TextArea(3, 5)]
        public string thumbnailUrl = "";
        [JsonProperty("code")]
        [SerializeReference]
        public ActionCollection actions = new ActionCollection();
    }
}
