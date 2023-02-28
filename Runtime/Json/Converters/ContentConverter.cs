using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Treasured.UnitySdk
{
    public class ContentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VideoContent) || objectType == typeof(AudioContent) || objectType == typeof(ImageContent);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case VideoContent video:
                    writer.WriteStartObject();
                    writer.WritePropertyName(nameof(VideoRenderer.src));
                    writer.WriteValue(video.Uri);
                    writer.WritePropertyName(nameof(VideoRenderer.volume));
                    writer.WriteValue(video.volume);
                    writer.WritePropertyName(nameof(VideoRenderer.loop));
                    writer.WriteValue(video.loop);
                    writer.WritePropertyName(nameof(VideoRenderer.autoplay));
                    writer.WriteValue(video.autoplay);
                    writer.WriteEndObject();
                    break;
                case AudioContent audio:
                    writer.WriteStartObject();
                    writer.WritePropertyName("src");
                    writer.WriteValue(audio.Uri);
                    writer.WritePropertyName("volume");
                    writer.WriteValue(audio.volume);
                    writer.WritePropertyName("loop");
                    writer.WriteValue(audio.loop);
                    writer.WritePropertyName("autoplay");
                    writer.WriteValue(audio.autoplay);
                    writer.WritePropertyName("muted");
                    writer.WriteValue(audio.muted);
                    writer.WriteEndObject();
                    break;
                case ImageContent image:
                    break;
                default:
                    break;
            }
        }
    }
}
