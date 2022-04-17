﻿using UnityEngine;
using Newtonsoft.Json;

namespace Treasured.UnitySdk
{
    [CreateAssetMenu(fileName = "New Icon", menuName = "Treasured/Create Icon")]
    public class IconAsset : ScriptableObject
    {
        [JsonIgnore]
        public Texture2D icon;
        [TextArea(3, 20)]
        public string svg;
    }
}
