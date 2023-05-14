using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that plays audio from a source.
    /// </summary>
    public class PlayAudioAction : Action
    {
        [FormerlySerializedAs("_position")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EmbedPosition position = EmbedPosition.TopRight;

        [JsonIgnore][OnValueChanged("AudioClipChanged")]
        public AudioClip audioClip;
        
        //[Url]
        [EnableIf("audioClip")]
        [FormerlySerializedAs("_src")]
        public string src;

        [Range(0, 100)]
        [FormerlySerializedAs("_volume")]
        public int volume = 100;

        public void AudioClipChanged()
        {
            if (!audioClip.IsNullOrNone())
            {
                src = "audios/" + audioClip.name;
            }
            else
            {
                src = string.Empty;
            }
        }
            
        internal override ScriptableAction ConvertToScriptableAction()
        {
            AudioAction action = new AudioAction();
            action.src = src;
            action.volume = volume;
            return action;
        }
    }
}
