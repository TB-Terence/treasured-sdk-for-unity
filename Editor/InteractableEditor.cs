using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System.Linq;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(Interactable))]
    internal class InteractableEditor : TreasuredEditor<Interactable>
    {
        private void OnDisable()
        {
            //Tools.hidden = false; // show the transform tools for other game object
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
