using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    public enum ThumbnailType
    {
        FromCurrentView,
        FromHotspot,
        //Custom
    }

    [Serializable]
    public class Thumbnail
    {
        

        [JsonIgnore]
        public ThumbnailType type;
        //[JsonIgnore]
        //public int hotspotIndex;
        [ShowIf("IsFromHotspot")]
        [JsonIgnore]
        public Hotspot hotspot;
        //[JsonIgnore]
        //public ImageInfo customImage;

        [JsonProperty]
        string Path
        {
            get
            {
                switch (type)
                {
                    //case ThumbnailType.Custom:
                    //    return customImage.Path;
                    default:
                        return "thumbnail.webp";
                }
            }
        }

        bool IsFromHotspot()
        {
            return type == ThumbnailType.FromHotspot;
        }
    }
}
