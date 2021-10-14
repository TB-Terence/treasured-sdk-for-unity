using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class SerializedPropertyExtensionMethods
    {
        static void ValidateArray(this SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String || !property.isArray)
            {
                throw new ArgumentException($"The property {{{property.name}}} is not type of array.");
            }
        }

        public static SerializedProperty AppendLast(this SerializedProperty arrayProperty)
        {
            arrayProperty.ValidateArray();
            arrayProperty.arraySize++;
            return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        }

        public static bool TryAppendScriptableObject(this SerializedProperty arrayProperty, out SerializedProperty newElement, out ScriptableObject scriptableObject)
        {
            newElement = AppendLast(arrayProperty);
            scriptableObject = null;
            // TODO: Cache this maybe?
            Type type = arrayProperty.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = null;
            while (type != null) // search in base class
            {
                fieldInfo = type.GetField(arrayProperty.propertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    break;
                }
                type = type.BaseType;
            }
            Type arrayType = fieldInfo.GetValue(arrayProperty.serializedObject.targetObject).GetType();
            if (arrayType.IsGenericType && arrayType.GenericTypeArguments.Length == 1)
            {
                Type elementType = arrayType.GenericTypeArguments[0];
                if (typeof(MonoBehaviour).IsAssignableFrom(arrayProperty.serializedObject.targetObject.GetType()))
                {
                    newElement.objectReferenceValue = ScriptableObject.CreateInstance(elementType);
                }
                else
                {
                    string mainAssetPath = AssetDatabase.GetAssetPath(arrayProperty.serializedObject.targetObject);
                    if (EditorUtility.IsPersistent(arrayProperty.serializedObject.targetObject))
                    {
                        scriptableObject = ScriptableObject.CreateInstance(elementType);
                        scriptableObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                        string[] names = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath).Where(x => x.GetType() == elementType).Select(x => x.name).ToArray();
                        scriptableObject.name = ObjectNames.GetUniqueName(names, elementType.Name);
                        AssetDatabase.AddObjectToAsset(scriptableObject, mainAssetPath);
                        AssetDatabase.SaveAssets();
                        newElement.objectReferenceValue = scriptableObject;
                        arrayProperty.serializedObject.ApplyModifiedProperties();
                        return true;
                    }
                }
                
            }
            return false;
        }

        private static SerializedProperty AppendScriptableObject(this SerializedProperty arrayProperty, Type type, bool hide = true)
        {
            SerializedProperty lastElement = AppendLast(arrayProperty);
            string mainAssetPath = AssetDatabase.GetAssetPath(arrayProperty.serializedObject.targetObject);
            if (EditorUtility.IsPersistent(arrayProperty.serializedObject.targetObject))
            {
                ScriptableObject obj = ScriptableObject.CreateInstance(type);
                if (hide)
                {
                    obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                }
                string[] names = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath).Where(x => x.GetType() == type).Select(x => x.name).ToArray();
                obj.name = ObjectNames.GetUniqueName(names, type.Name);
                AssetDatabase.AddObjectToAsset(obj, mainAssetPath);
                AssetDatabase.SaveAssets();
                lastElement.objectReferenceValue = obj;
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }
            return lastElement;
        }

        public static SerializedProperty AppendManagedObject(this SerializedProperty property, Type type)
        {
            SerializedProperty lastElement = property.AppendLast();
            if (lastElement.propertyType != SerializedPropertyType.ManagedReference)
            {
                property.RemoveElementAtIndex(property.arraySize - 1);
                throw new ArgumentException($"Type dismatch {property.displayName} is not type of managed reference.");
            }
            lastElement.managedReferenceValue = Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
            return lastElement;
        }

        public static void RemoveElementAtIndex(this SerializedProperty property, int index)
        {
            property.ValidateArray();
            SerializedProperty element = property.GetArrayElementAtIndex(index);
            switch (element.propertyType)
            {
                case SerializedPropertyType.ManagedReference:
                    element.managedReferenceValue = null;
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (element.objectReferenceValue != null && EditorUtility.IsPersistent(property.serializedObject.targetObject))
                    {
                        AssetDatabase.RemoveObjectFromAsset(element.objectReferenceValue);
                        AssetDatabase.SaveAssets();
                        property.DeleteArrayElementAtIndex(index);
                    }
                    break;
            }
            property.DeleteArrayElementAtIndex(index);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
