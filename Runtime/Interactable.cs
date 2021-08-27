using System;
using UnityEngine;

namespace Treasured.SDK
{
    public class Interactable : TObject
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }
    }

    public sealed class InteractableData : TObjectData
    {
        public InteractableData(Interactable interactable)
        {
            this._id = interactable.Id;
            this.Description = interactable.Description;
            this.OnSelected = interactable.OnSelected;
        }
    }
}
