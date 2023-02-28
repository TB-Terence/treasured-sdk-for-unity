using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal abstract class TreasuredObjectEditor : Editor
    {
        public static class Styles
        {
            public static readonly GUIContent missingMapComponent = EditorGUIUtility.TrTextContent("Missing Treasured Map Component in parent.", "", "Warning");
        }

        protected ComponentCardAttribute componentCardAttribute;
        protected TreasuredMap map;
        private Texture2D _icon;

        protected virtual void OnEnable()
        {
            map = (target as TreasuredObject).Map;
            componentCardAttribute = (ComponentCardAttribute)this.GetType().GetCustomAttributes(typeof(ComponentCardAttribute), false).FirstOrDefault();
            _icon = Resources.Load<Texture2D>(componentCardAttribute.iconName);
        }

        public override void OnInspectorGUI()
        {
            if (map == null)
            {
                EditorGUILayout.LabelField(Styles.missingMapComponent);
                return;
            }
            if (componentCardAttribute != null)
            {
                EditorGUILayoutUtils.ComponentCard(_icon, componentCardAttribute.name, componentCardAttribute.description, componentCardAttribute.helpUrl);
            }
        }
    }
}
