using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Interactable")]
    public sealed class Interactable : TreasuredObject, IDataComponent<InteractableData>
    {
        [Obsolete]
        [SerializeField]
        private InteractableData _data = new InteractableData();

        [Obsolete]
        [JsonIgnore]
        public override TreasuredObjectData Data
        {
            get
            {
                return _data;
            }
        }

        [JsonIgnore]
        InteractableData IDataComponent<InteractableData>.Data => _data;

        public void BindData(InteractableData data)
        {
            gameObject.name = data.Name;
            gameObject.transform.position = data.Transform.Position;
            gameObject.transform.eulerAngles = data.Transform.Rotation;
            BoxCollider.center = data.Hitbox.Center - data.Transform.Position;
            BoxCollider.size = data.Hitbox.Size;
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
