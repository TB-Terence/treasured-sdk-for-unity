﻿using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class VideoRenderer : TreasuredObject
    {
        #region
        [SerializeField]
        private bool _lockAspectRatio;
        [SerializeField]
        private string _aspectRatio = "16:9";
        [SerializeField]
        [Url]
        private string _src;
        #endregion

        [JsonIgnore]
        public bool LockAspectRatio => _lockAspectRatio;
        [JsonIgnore]
        public float AspectRatio
        {
            get
            {
                string[] ratios = _aspectRatio.Split(':', 'x');
                return float.Parse(ratios[0]) / float.Parse(ratios[1]);
            }
        }
        public string Src => _src;
    }
}
