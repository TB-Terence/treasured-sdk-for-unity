using System;

namespace Treasured.UnitySdk
{
    [API("startTour")]
    public class StartTourAction : ScriptableAction
    {
        [JsonPropertyOverwrite("name", "title")]
        public GuidedTour target;
    }
}
