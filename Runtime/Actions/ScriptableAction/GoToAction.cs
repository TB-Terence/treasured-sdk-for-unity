using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("goTo")]
    [CreateActionGroup(typeof(SetCameraRotationAction))]
    [CreateActionGroup(typeof(PanAction))]
    public class GoToAction : ScriptableAction
    {
        [JsonIgnore]
        public Hotspot target;

        [JsonProperty("hotspotId")]
        string TargetId
        {
            get
            {
                return target?.Id;
            }
        }
    }
}
