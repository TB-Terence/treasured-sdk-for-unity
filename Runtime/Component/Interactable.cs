using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("")]
    public sealed class Interactable : TreasuredObject, IDataComponent<InteractableData>
    {
        [SerializeField]
        private InteractableData _data = new InteractableData();

        public override TreasuredObjectData Data
        {
            get
            {
                return _data;
            }
        }

        InteractableData IDataComponent<InteractableData>.Data => _data;

        public void BindData(InteractableData data)
        {
            gameObject.name = data.Name;
            gameObject.transform.position = data.Transform.Position;
            gameObject.transform.eulerAngles = data.Transform.Rotation;
            Hitbox.center = data.Hitbox.Center;
            Hitbox.size = data.Hitbox.Size;
            _data = data;
        }

        private Interactable() { }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        void Reset()
        {
            _data?.Validate();
        }
    }
}
