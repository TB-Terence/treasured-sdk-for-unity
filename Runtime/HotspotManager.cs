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

        public bool loop = true;

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
