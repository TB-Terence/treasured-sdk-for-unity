using System;
using System.Collections.Generic;
using System.Linq;

namespace Treasured.UnitySdk.Validation
{
    public class ValidationException : Exception
    {
        public readonly List<ValidationResult> results;
        public readonly List<ValidationResult> errors;
        public readonly List<ValidationResult> warnings;
        public readonly List<ValidationResult> infos;

        public ValidationException(List<ValidationResult> validationResults)
        {
            results = validationResults;
            errors = validationResults.Where(result => result.type == ValidationResult.ValidationResultType.Error).ToList();
            warnings = validationResults.Where(result => result.type == ValidationResult.ValidationResultType.Warning).ToList();
            infos = validationResults.Where(result => result.type == ValidationResult.ValidationResultType.Info).ToList();
        }
    }
}
