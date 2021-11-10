﻿using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal abstract class ExportProcess
    {
        public virtual bool Enabled { get; set; } = true;
        public virtual string DisplayName
        {
            get
            {
                string typeName = GetType().Name;
                if (typeName.EndsWith("ExportProcess"))
                {
                    typeName = typeName.Substring(0, typeName.Length - 13);
                }
                return ObjectNames.NicifyVariableName(typeName);
            }
        }
        public bool Expanded { get; set; } = true;
        public abstract void Export(string rootDirectory, TreasuredMap map);
        public virtual void OnGUI(TreasuredMap map) { }
    }
}
