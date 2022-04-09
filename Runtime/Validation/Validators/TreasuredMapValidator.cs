using System.Collections.Generic;

namespace Treasured.UnitySdk
{
    public sealed class TreasuredMapValidator : Validator
    {
        private static RequiredFieldValidator s_requiredFieldValidator;
        private TreasuredMap _map;
        public TreasuredMapValidator(TreasuredMap map) : base(map)
        {
            this._map = map;
            s_requiredFieldValidator = new RequiredFieldValidator(map);
        }

        public override List<ValidationResult> GetValidationResults()
        {
            var results = base.GetValidationResults();
            // Check for missing required fields
            results.AddRange(s_requiredFieldValidator.GetValidationResults());
            // Check for action references
            var treasuredObjects = _map.GetComponentsInChildren<TreasuredObject>();
            foreach (var obj in treasuredObjects)
            {
                if (obj is Hotspot hotspot && hotspot.Camera == null)
                {
                    results.Add(new ValidationResult()
                    {
                        name = "Missing reference for Action",
                        description = $"Camera Transform is not assigned for {obj.name}.",
                        type = ValidationResult.ValidationResultType.Error
                    });
                }
                foreach (var group in obj.OnClick)
                {
                    foreach (var action in group.Actions)
                    {
                        if (action is SelectObjectAction soa)
                        {
                            if (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf))
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Missing reference",
                                    description = $"The target for Select-Object action is inactive OR is not assigned for {obj.name}.",
                                    type = ValidationResult.ValidationResultType.Error
                                });
                            }
                            else if (soa.Target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Invalid reference",
                                    description = $"The target set for Select-Object action does not belong to the same map.",
                                    type = ValidationResult.ValidationResultType.Error
                                });
                            }
                        }
                    }
                }
                foreach (var group in obj.OnHover)
                {
                    foreach (var action in group.Actions)
                    {
                        if (action is SelectObjectAction soa)
                        {
                            if (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf))
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Missing reference",
                                    description = $"The target for OnHover-Object action is inactive OR is not assigned for {obj.name}.",
                                    type = ValidationResult.ValidationResultType.Error
                                });
                            }
                            else if (soa.Target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Invalid reference",
                                    description = $"The target set for OnHover-Object action does not belong to the same map.",
                                    type = ValidationResult.ValidationResultType.Error
                                });
                            }
                        }
                    }
                }
            }
            return results;
        }
    }
}
