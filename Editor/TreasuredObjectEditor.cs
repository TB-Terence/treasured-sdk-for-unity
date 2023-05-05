using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal abstract class TreasuredObjectEditor : Editor
    {
        protected ComponentCardAttribute componentCardAttribute;
        protected TreasuredScene scene;
        private Texture2D _icon;

        protected virtual void OnEnable()
        {
            scene = (target as TreasuredObject).Scene;
            componentCardAttribute = (ComponentCardAttribute)this.GetType().GetCustomAttributes(typeof(ComponentCardAttribute), false).FirstOrDefault();
            _icon = Resources.Load<Texture2D>(componentCardAttribute.iconName);
        }

        public override void OnInspectorGUI()
        {
            if (scene == null)
            {
                EditorGUILayout.HelpBox("Missing Treasured Scene Component in parent.", MessageType.Error);
                return;
            }
            if (componentCardAttribute != null)
            {
                EditorGUILayoutUtils.ComponentCard(_icon, componentCardAttribute.name, componentCardAttribute.description, componentCardAttribute.helpUrl);
            }
        }
    }
}
