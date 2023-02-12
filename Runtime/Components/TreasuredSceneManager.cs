using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class TreasuredSceneManager : MonoBehaviour
    {
        public SceneInfo sceneInfo;

        private void OnValidate()
        {
            
        }
    }

    [System.Serializable]
    public class SceneInfo
    {
        [SerializeField]
        [ReadOnly]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get => _id; }

        [RequiredField]
        [Tooltip("The name of the individual, company or organization.")]
        public string creator;
        [RequiredField]
        [TextArea(2, 2)]
        public string title;
        [RequiredField]
        [TextArea(3, 5)]
        public string description;

        [JsonProperty("loader")]
        public TemplateLoader templateLoader;
    }
}
