using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk.Validation
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
            results.AddRange(GetGuidedTourSrcValidationResult());
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
                        type = ValidationResult.ValidationResultType.Error,
                        target = obj
                    });
                }
                foreach (var group in obj.OnClick)
                {
                    foreach (var action in group.Actions)
                    {
                        if (action is SelectObjectAction soa)
                        {
                            if (soa.target == null || (soa.target != null && !soa.target.gameObject.activeSelf))
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Missing reference",
                                    description = $"The target for Select-Object action is inactive OR is not assigned for {obj.name}.",
                                    type = ValidationResult.ValidationResultType.Error,
                                    target = soa.target,
                                });
                            }
                            else if (soa.target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Invalid reference",
                                    description = $"The target set for Select-Object action does not belong to the same map.",
                                    type = ValidationResult.ValidationResultType.Error,
                                    target = soa.target,
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
                            if (soa.target == null || (soa.target != null && !soa.target.gameObject.activeSelf))
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Missing reference",
                                    description = $"The target for OnHover-Object action is inactive OR is not assigned for {obj.name}.",
                                    type = ValidationResult.ValidationResultType.Error,
                                    target = soa.target
                                });
                            }
                            else if (soa.target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                            {
                                results.Add(new ValidationResult()
                                {
                                    name = "Invalid reference",
                                    description = $"The target set for OnHover-Object action does not belong to the same map.",
                                    type = ValidationResult.ValidationResultType.Error,
                                    target = soa.target
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
                for (int i = 0; i < hotspots.Length; i++)
                {
                    var current = hotspots[i];
                    var next = hotspots[(i + 1) % hotspots.Length]; // ensure last go to first
                    if(Physics.Linecast(current.Camera.transform.position, next.Camera.transform.position, out RaycastHit hit))
                    {
                        results.Add(new ValidationResult()
                        {
                            name = "Collider blocking path",
                            description = $"Collider blocking path between hotspot <{current.name}> and <{next.name}>. The game object blocking the path: <{hit.collider.gameObject.name}>",
                            target = hit.collider.gameObject,
                            targets = new UnityEngine.Object[] {current, next },
                            type = ValidationResult.ValidationResultType.Warning
                        });
                    }
                }
            }
            return results;
        }

        List<ValidationResult> GetGuidedTourSrcValidationResult()
        {
            if (!_map.features.guidedTour)
            {
                return null;
            }
            List<ValidationResult> results = new List<ValidationResult>();
            var tours = _map.graph.tours;
            foreach (var tour in tours)
            {
                foreach (var action in tour.actionScripts)
                {
                    
                    if ((action is AudioAction audioAction && string.IsNullOrWhiteSpace(audioAction.src)) || (action is EmbedAction embedAction && string.IsNullOrWhiteSpace(embedAction.src)))
                    {
                        ValidationResult validationResult = new ValidationResult()
                        {
                            name = "Required Field",
                            description = $"Src is not provided for [{action.Type} action] in [{tour.title}] tour.",
                            type = ValidationResult.ValidationResultType.Error
                        };
                        results.Add(validationResult);
                    }
                    
                }
            }
            return results;
        }
    }
}
