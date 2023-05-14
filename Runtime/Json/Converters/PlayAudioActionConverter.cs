using System;
using System.IO;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Treasured.UnitySdk
{
    public class PlayAudioActionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PlayAudioAction);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is PlayAudioAction newAction)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("id");
                writer.WriteValue(newAction.Id);

                writer.WritePropertyName("type");
                writer.WriteValue(newAction.Type);

                writer.WritePropertyName("position");
                writer.WriteValue(newAction.position);

                writer.WritePropertyName("src");
                if (newAction.audioClip != null)
                {
                    string path = string.Empty;
#if UNITY_EDITOR
                    path = Path.GetFileName(AssetDatabase.GetAssetPath(newAction.audioClip));
#endif
                    writer.WriteValue("audios/" + path);
                }
                else
                {
                    writer.WriteValue(newAction.src);
                }

                writer.WritePropertyName("volume");
                writer.WriteValue(newAction.volume);

                writer.WriteEndObject();
            }
        }
    }
}
