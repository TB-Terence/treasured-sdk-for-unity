﻿using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Hitbox))]
    internal class HitboxEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // No GUI
        }
    }
}
