using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class TreasuredScene : MonoBehaviour
    {
        [Serializable]
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

            public bool loop = true;

            public AudioContent backgroundMusic;
        }

        [Serializable]
        public class StyleInfo
        {
            [JsonProperty("loader")]
            public TemplateLoader templateLoader;
            public bool darkMode = false;
        }

        public SceneInfo sceneInfo;
        public StyleInfo styleInfo;

        private void OnEnable()
        {
            sceneInfo ??= new SceneInfo();
            styleInfo ??= new StyleInfo();
        }
    }
}
