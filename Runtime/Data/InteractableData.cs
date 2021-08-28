namespace Treasured.UnitySdk
{
    public sealed class InteractableData : TreasuredObjectData
    {
        public InteractableData(Interactable interactable)
        {
            this._id = interactable.Id;
            this.Description = interactable.Description;
            this.OnSelected = interactable.OnSelected;
        }
    }
}
