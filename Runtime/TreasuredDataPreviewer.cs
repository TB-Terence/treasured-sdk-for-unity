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
                        GameObject go = new GameObject("Treasured Data Previewer");
                        go.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
                        _instance = go.AddComponent<TreasuredDataPreviewer>();
                    }
                }
                return _instance;
            }
        }

        private TreasuredData _data;

        public TreasuredData Data { get => _data; set => _data = value; }

        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                GameObject.DestroyImmediate(this.gameObject);
            }
        }
    }
}
