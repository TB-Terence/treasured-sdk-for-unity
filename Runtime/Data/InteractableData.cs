using Newtonsoft.Json;
using Treasured.SDK;

namespace Treasured.UnitySdk
{
    public sealed class InteractableData : TreasuredObjectData
    {
        private InteractableData(Interactable interactable) : base(interactable)
        {

        }

        [JsonConstructor]
        private InteractableData(string id) : base(id)
        {

        }

        public static implicit operator InteractableData(Interactable data)
        {
            return new InteractableData(data);
        }
    }
}
