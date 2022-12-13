using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnValueChangedAttribute attribute = (OnValueChangedAttribute)base.attribute;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                var target = EditorUtils.GetTargetObjectWithProperty(property);

                var callbackMethod = EditorUtils.GetMethod(target, attribute.CallbackName);
                if (callbackMethod != null && callbackMethod.ReturnType == typeof(void)
                                           && callbackMethod.GetParameters().Length == 0)
                {
                    callbackMethod.Invoke(target, new object[] { });
                }
                else
                {
                    string warning = string.Format(
                        "{0} can invoke only methods with 'void' return type and 0 parameters",
                        attribute.GetType().Name);

                    Debug.LogWarning(warning, property.serializedObject.targetObject);
                }

            }
        }
    }
}
