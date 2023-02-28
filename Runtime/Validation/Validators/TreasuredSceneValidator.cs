using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk.Validation
{
    public sealed class TreasuredSceneValidator : Validator
    {
        private static RequiredFieldValidator s_requiredFieldValidator_map;
        private TreasuredMap _map;
        public TreasuredSceneValidator(TreasuredMap map) : base(map)
        {
            this._map = map;
            s_requiredFieldValidator_map = new RequiredFieldValidator(map);
        }

        public override List<ValidationResult> GetValidationResults()
        {
            var results = base.GetValidationResults();
            results.AddRange(GetProjectSettingsValidationResult());
            // Check for missing required fields
            results.AddRange(s_requiredFieldValidator_map.GetValidationResults());
            results.AddRange(GetSelectObjectReferenceValidationResults());
            results.AddRange(GetHotspotPathValidationResult());
            results.AddRange(GetGuidedTourSrcValidationResult());
            return results;
        }

        IEnumerable<ValidationResult> GetProjectSettingsValidationResult()
        {
            if (TreasuredSDKPreferences.Instance.customExportFolder.StartsWith(Application.dataPath))
            {
                yield return new ValidationResult()
                {
                    name = "Export folder not recommended",
                    description = "The export folder is currently set to a folder inside the Asset folder, which is not recommended.",
                    type = ValidationResult.ValidationResultType.Warning,
                    resolvers = new ValidationResult.ValidationResolver[]
                    {
                        new ValidationResult.ValidationResolver()
                        {
                            text = "Change Export Folder",
                            onResolve = () =>
                            {
#if UNITY_2018_3_OR_NEWER && UNITY_EDITOR
                                UnityEditor.SettingsService.OpenUserPreferences("Preferences/Treasured SDK");
#endif
                            }
                        }
                    }
                };
            }
        }

        IEnumerable<ValidationResult> GetSelectObjectReferenceValidationResults()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            var treasuredObjects = _map.GetComponentsInChildren<TreasuredObject>();
            foreach (var obj in treasuredObjects)
            {
                if (obj is Hotspot hotspot && hotspot.Camera == null)
                {
                    results.Add(new ValidationResult()
                    {
                        name = "Missing camera reference for Hotspot",
                        description = $"Camera Transform is not assigned for {obj.name}.",
                        type = ValidationResult.ValidationResultType.Error,
                        resolvers = new ValidationResult.ValidationResolver[]
                        {
                            new ValidationResult.ValidationResolver()
                            {
                                text = $"Ping '{obj.name}'",
                                onResolve = () =>
                                {
#if UNITY_EDITOR
                                    UnityEditor.EditorGUIUtility.PingObject(obj);
#endif
                                }
                            }
                        }
                    });
                }
                //foreach (var group in obj.OnClick)
                //{
                //    foreach (var action in group.Actions)
                //    {
                //        if (action is SelectObjectAction soa)
                //        {
                //            if (soa.target == null || (soa.target != null && !soa.target.gameObject.activeSelf))
                //            {
                //                results.Add(new ValidationResult()
                //                {
                //                    name = "Missing reference",
                //                    description = $"The target for Select-Object action is inactive OR is not assigned for {obj.name}.",
                //                    type = ValidationResult.ValidationResultType.Error,
                //                    target = soa.target,
                //                });
                //            }
                //            else if (soa.target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                //            {
                //                results.Add(new ValidationResult()
                //                {
                //                    name = "Invalid reference",
                //                    description = $"The target set for Select-Object action does not belong to the same map.",
                //                    type = ValidationResult.ValidationResultType.Error,
                //                    target = soa.target,
                //                });
                //            }
                //        }
                //    }
                //}
                //foreach (var group in obj.OnHover)
                //{
                //    foreach (var action in group.Actions)
                //    {
                //        if (action is SelectObjectAction soa)
                //        {
                //            if (soa.target == null || (soa.target != null && !soa.target.gameObject.activeSelf))
                //            {
                //                results.Add(new ValidationResult()
                //                {
                //                    name = "Missing reference",
                //                    description = $"The target for OnHover-Object action is inactive OR is not assigned for {obj.name}.",
                //                    type = ValidationResult.ValidationResultType.Error,
                //                    target = soa.target
                //                });
                //            }
                //            else if (soa.target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                //            {
                //                results.Add(new ValidationResult()
                //                {
                //                    name = "Invalid reference",
                //                    description = $"The target set for OnHover-Object action does not belong to the same map.",
                //                    type = ValidationResult.ValidationResultType.Error,
                //                    target = soa.target
                //                });
                //            }
                //        }
                //    }
                //}
            }
            return results;
        }

        IEnumerable<ValidationResult> GetHotspotPathValidationResult(){
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
                            description = $"Collider blocking path between hotspot <{current.name}> and <{next.name}>. Move the game objects for better user experience.",
                            type = ValidationResult.ValidationResultType.Warning,
#if UNITY_EDITOR
                            resolvers = new ValidationResult.ValidationResolver[] {
                                new ValidationResult.ValidationResolver()
                                {
                                    text = $"Ping '{current.name}(Hotspot 1)'",
                                    onResolve = () => { UnityEditor.EditorGUIUtility.PingObject(current); }
                                },
                                new ValidationResult.ValidationResolver()
                                {
                                    text = $"Ping '{hit.collider.gameObject.name}(Collider)'",
                                    onResolve = () => { UnityEditor.EditorGUIUtility.PingObject(hit.collider.gameObject); }
                                },
                                 new ValidationResult.ValidationResolver()
                                {
                                    text = $"Ping '{next.name}(Hotspot 2)'",
                                    onResolve = () => { UnityEditor.EditorGUIUtility.PingObject(current); }
                                },
#endif
                            }
                        });
                    }
                }
            }
            return results;
        }

        IEnumerable<ValidationResult> GetGuidedTourSrcValidationResult()
        {
            if (!_map.features.guidedTour)
            {
                return Enumerable.Empty<ValidationResult>();
            }
            List<ValidationResult> results = new List<ValidationResult>();
            var tours = _map.graph.tours;
            foreach (var tour in tours)
            {
                if (tour.actionScripts == null) continue;
                foreach (var action in tour.actionScripts)
                {
                    if ((action is AudioAction audioAction && string.IsNullOrWhiteSpace(audioAction.src)) || (action is Actions.EmbedAction embedAction && string.IsNullOrWhiteSpace(embedAction.src)))
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
