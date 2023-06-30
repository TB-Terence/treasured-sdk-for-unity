using System;

namespace Treasured.UnitySdk
{
    internal class ExporterWindowMenuItem : UnityEditor.Editor
    {
        public TreasuredScene Scene { get; private set; }


        public void SetScene(TreasuredScene scene)
        {
            this.Scene = scene;
        }
    }
}
