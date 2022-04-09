using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class TreasuredMapValidator : Validator
    {
        private static RequiredFieldValidator s_requiredFieldValidator_map;
        private TreasuredMap _map;
        public TreasuredMapValidator(TreasuredMap map) : base(map)
        {
            this._map = map;
            s_requiredFieldValidator_map = new RequiredFieldValidator(map);
        }

        public override List<ValidationResult> GetValidationResults()
        {
            var results = base.GetValidationResults();
            // Check for missing required fields
            results.AddRange(s_requiredFieldValidator_map.GetValidationResults());
            results.AddRange(GetSelectObjectReferenceValidationResults());
            results.AddRange(GetHotspotPathValidationResult());
            return results;
        }

        List<ValidationResult> GetSelectObjectReferenceValidationResults()
        {
            List<ValidationResult> results = new List<ValidationResult>();
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

        List<ValidationResult> GetHotspotPathValidationResult(){
            List<ValidationResult> results = new List<ValidationResult>();
            var hotspots = _map.Hotspots;
            if (hotspots.Length > 2)
            {
                for (int i = 1; i < hotspots.Length; i++)
                {
                    var previous = hotspots[i - 1];
                    var current = hotspots[i];
                    if(Physics.Linecast(previous.Camera.transform.position, current.Camera.transform.position, out RaycastHit hit))
                    {
                        results.Add(new ValidationResult()
                        {
                            name = "Collider blocking path",
                            description = $"Collider blocking path between hotspot <{previous.name}> and <{current.name}>. The object is {hit.collider.gameObject.name}",
                            context = hit.collider.gameObject,
                            type = ValidationResult.ValidationResultType.Warning
                        });
                    }
                }
            }
            return results;
        }
    }
}
