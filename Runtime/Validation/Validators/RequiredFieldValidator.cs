using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Treasured.UnitySdk.Validation
{
    public class RequiredFieldValidator : Validator
    {
        public RequiredFieldValidator(object target) : base(target) { }

        public override List<ValidationResult> GetValidationResults()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            foreach (var reference in ReflectionUtils.GetSeriliazedFieldReferencesWithAttribute<RequiredFieldAttribute>(target))
            {
                if (reference.IsNull())
                {
                    var fieldName = NicifyVariableName(reference.fieldInfo.Name);
                    results.Add(new ValidationResult()
                    {
                        name = "Missing Required Field",
                        description = $"`{fieldName}` of `{reference.fieldInfo.DeclaringType.Name}` is required, but it's either missing or unassigned.",
                        type = ValidationResult.ValidationResultType.Error
                    });
                }
            }
            return results;
        }

        static string NicifyVariableName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            value = value.Replace("_", " ").Trim();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && i > 0 && !char.IsUpper(value[i - 1]))
                {
                    sb.Append(' ');
                }
                sb.Append(i == 0 ? char.ToUpper(value[i]) : value[i]);
            }
            return sb.ToString();
        }
    }
}
