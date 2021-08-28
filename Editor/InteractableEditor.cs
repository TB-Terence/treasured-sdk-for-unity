using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk;
using System.Linq;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(Interactable))]
    internal class InteractableEditor : TreasuredEditor<Interactable>
    {
        protected override void Init()
        {
            base.Init();
            Tools.hidden = true;
        }

        private void OnDisable()
        {
            Tools.hidden = false; // show the transform tools for other game object
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
        }
    }
}
