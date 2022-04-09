using System.Collections.Generic;
using System.Reflection;

namespace Treasured.UnitySdk
{
    public class RequiredFieldValidator : Validator
    {
        public RequiredFieldValidator(object target) : base(target) { }

        public override List<ValidationResult> GetValidationResults()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            FieldInfo[] fieldInfos = ReflectionUtilities.GetSeriliazedFieldsWithAttribute<RequiredFieldAttribute>(target);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                var value = fieldInfo.GetValue(target);
                if(value is string str)
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        results.Add(new ValidationResult()
                        {
                            name = "Missing Required Field.",
                            description = $"`{fieldInfo.Name}` field is required but missing(empty).",
                            type = ValidationResult.ValidationResultType.Error
                        });
                    }
                }
            }
            return results;
        }
    }
}
