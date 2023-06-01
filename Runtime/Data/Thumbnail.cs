using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class Thumbnail
    {
        public enum ThumbnailType
        {
            FromCurrentView,
            FromHotspot,
            Custom
        }

        [JsonIgnore]
        public ThumbnailType type;
        [JsonIgnore]
        public int hotspotIndex;
        [JsonIgnore]
        public ImageInfo customImage;

        [JsonProperty]
        string Path
        {
            get
            {
                switch (type)
                {
                    case ThumbnailType.Custom:
                        return customImage.Path;
                    default:
                        return "thumbnail.jpg";
                }
            }
        }
    }
}
