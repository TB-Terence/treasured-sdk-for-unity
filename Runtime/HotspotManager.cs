using UnityEngine;

namespace Treasured.ExhibitX
{
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public sealed class HotspotManager : MonoBehaviour
    {
        private static bool s_initialized;
        private static HotspotManager s_intance;

        public static HotspotManager Instance
        {
            get
            {
                if (!s_initialized)
                {
                    s_intance = GameObject.FindObjectOfType<HotspotManager>();
                    s_initialized = true;
                }
                return s_intance;
            }
        }

        [SerializeField]
        private bool _loop = true;

        /// <summary>
        /// Defines if the last active hotspot should be connected to the first active hotspot.
        /// </summary>
        public bool Loop
        {
            get
            {
                return _loop;
            }
            set
            {
                _loop = value;
            }
        }

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            if (s_intance == null)
            {
                s_intance = this;
                s_initialized = true;
            }
            else
            {
                if (s_intance != this)
                {
                    DestroyImmediate(this.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            s_initialized = false;
        }
    }
}
