﻿using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("Media")]
    public abstract class EmbedAction : ActionBase
    {
        [SerializeField]
        private EmbedPosition _position = EmbedPosition.TopRight;

        public EmbedPosition Position { get => _position; set => _position = value; }
    }
}
