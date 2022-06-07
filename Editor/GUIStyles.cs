using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal class GUIStyles
    {
        private static GUIStyles _instance;
        public static GUIStyles Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GUIStyles();
                }
                return _instance;
            }
        }

        private Dictionary<string, GUIStyle> _styles = new Dictionary<string, GUIStyle>();

        public GUIStyle this[string name]
        {
            get
            {
                return _styles.TryGetValue(name, out var result) ? result : null;
            }
            set
            {
                _styles[name] = value;
            }
        }

        public GUIStyles()
        {
            this["centeredLabel"] = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.BoldAndItalic
            };
        }
    }
}
