using System.Collections.Generic;

namespace Treasured.UnitySdk.Validation
{
    public abstract class Validator
    {
        public object target;

        public Validator(object target)
        {
            this.target = target;
        }

        public virtual List<ValidationResult> GetValidationResults()
        {
            return new List<ValidationResult>();
        }
    }
}
