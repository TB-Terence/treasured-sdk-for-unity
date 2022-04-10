using System.Collections.Generic;
using System.Globalization;

namespace Treasured.UnitySdk.Validation
{
    public class RequiredFieldValidator : Validator
    {
        public RequiredFieldValidator(object target) : base(target) { }

        public override List<ValidationResult> GetValidationResults()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            foreach (var reference in ReflectionUtilities.GetSeriliazedFieldReferencesWithAttribute<RequiredFieldAttribute>(target))
            {
                if (reference.Value is string str)
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        TextInfo info = CultureInfo.CurrentCulture.TextInfo;
                        var fieldName = info.ToTitleCase(reference.fieldInfo.Name.Replace("_", string.Empty));
                        results.Add(new ValidationResult()
                        {
                            name = "Missing Required Field",
                            description = $"`{fieldName}` field is required but is missing or empty.",
                            type = ValidationResult.ValidationResultType.Error
                        });
                    }
                }
            }
            return results;
        }
    }
}
