using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class JsonPropertyOverwrite : Attribute
    {
        public string CustomPropertyName;
        public string FieldName;
        
        public JsonPropertyOverwrite(string customPropertyName, string fieldName)
        {
            CustomPropertyName = customPropertyName;
            FieldName = fieldName;
        }
    }
}
