﻿using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Treasured.UnitySdk
{
    public struct SerializedPropertyInfo
    {
        public FieldInfo fieldInfo;
        public SerializedProperty serializedProperty;
        public object declaringObject;
    }

    public static class EditorReflectionUtilities
    {
        private static readonly string[] s_propertyToExclude = new string[] { "m_Script" };

        public static List<SerializedPropertyInfo> GetSerializedProperties(UnityEngine.Object obj)
        {
            var serializedObject = new SerializedObject(obj);
            var serializedProperties = new List<SerializedPropertyInfo>();
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);

            var fields = ReflectionUtilities.GetSerializableFieldInfoValuePair(obj, true);
            var fieldDict = new Dictionary<string, FieldInfoValuePair>();
            foreach (var field in fields)
            {
                fieldDict[field.FieldInfo.Name] = field;
            }

            while (iterator.Next(true))
            {
                if (s_propertyToExclude.Contains(iterator.name))
                {
                    continue;
                }
                var serializedProperty = iterator.Copy();
                if (fieldDict.ContainsKey(serializedProperty.name))
                {
                    var field = fieldDict[serializedProperty.name];
                    serializedProperties.Add(new SerializedPropertyInfo(serializedProperty, field.FieldInfo, field.DeclaringObject));
                }
            }

            return serializedProperties;
        }

        public static List<SerializedPropertyInfo> GetSerializedPropertiesWithAttribute<T>(UnityEngine.Object obj) where T : Attribute
        {
            var serializedObject = new SerializedObject(obj);
            var serializedProperties = new List<SerializedPropertyInfo>();
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);

            var fields = ReflectionUtilities.GetSerializableFieldInfoValuePairWithAttribute<T>(obj, true);
            var fieldDict = new Dictionary<string, FieldInfoValuePair>();
            foreach (var field in fields)
            {
                fieldDict[field.FieldInfo.Name] = field;
            }

            while (iterator.Next(true))
            {
                if (s_propertyToExclude.Contains(iterator.name))
                {
                    continue;
                }
                var serializedProperty = iterator.Copy();
                if (fieldDict.ContainsKey(serializedProperty.name))
                {
                    var field = fieldDict[serializedProperty.name];
                    serializedProperties.Add(new SerializedPropertyInfo(serializedProperty, field.FieldInfo, field.DeclaringObject));
                }
            }

            return serializedProperties;
        }

        public class SerializedPropertyInfo
        {
            public readonly SerializedProperty serializedProperty;
            public readonly FieldInfo fieldInfo;
            public readonly object declaringObject;

            public SerializedPropertyInfo(SerializedProperty property, FieldInfo fieldInfo, object declaringObject)
            {
                this.serializedProperty = property;
                this.fieldInfo = fieldInfo;
                this.declaringObject = declaringObject;
            }

            public object GetValue()
            {
                return fieldInfo.GetValue(declaringObject);
            }
        }
    }
}
