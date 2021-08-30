using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("")]
    public class Interactable : TreasuredObject
    {
        [SerializeField]
        private InteractableData _data;
        public InteractableData Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
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

        public override void LoadFromData(TreasuredObjectData data)
        {
            base.LoadFromData(data);
            _data = (InteractableData)data;
        }
    }
}
