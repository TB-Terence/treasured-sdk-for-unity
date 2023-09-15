using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk.Actions
{
    [API("showPreview")]
    public class ShowPreviewAction : ScriptableAction
    {
        [JsonIgnore]
        public TreasuredObject target;

        [JsonProperty]
        string targetId
        {
            get
            {
                return target?.Id;
            }
        }
    }
}
