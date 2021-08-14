using UnityEngine;

namespace Treasured.SDK
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public sealed class TreasuredDataPreviewer : MonoBehaviour
    {
        
        private static TreasuredDataPreviewer _instance;
        public static TreasuredDataPreviewer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<TreasuredDataPreviewer>();
                    if(_instance == null)
                    {
                        new GameObject("Treasured Data Previewer", typeof(TreasuredDataPreviewer));
                    }
                }
                return _instance;
            }
        }

        [SerializeField]
        private TreasuredData _data;

        public TreasuredData Data { get => _data; set => _data = value; }

        private void Awake()
        {
            gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild;
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                GameObject.DestroyImmediate(this.gameObject);
            }
        }
    }
}
