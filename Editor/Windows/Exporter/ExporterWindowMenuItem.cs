using System;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal class ExporterWindowMenuItem : UnityEditor.Editor
    {
        public TreasuredScene Scene { get; private set; }

        public virtual string DisplayName
        {
            get
            {
                return ObjectNames.NicifyVariableName(this.GetType().Name.Replace("MenuItem", ""));
            }
        }

        protected SerializedObject serializedScene;

        public void SetScene(TreasuredScene scene)
        {
            this.Scene = scene;
            this.serializedScene = new SerializedObject(scene);
        }
    }
}
