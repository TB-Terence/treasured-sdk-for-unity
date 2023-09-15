using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    public enum CaptureMethodType
    {
        /// <summary>
        /// Capture camera view from the first Hotspot
        /// </summary>
        FirstHotspot,
        /// <summary>
        /// Capture camera view from current Scene View, this will be what you see on your Scene View when you click on the Export button
        /// </summary>
        CurrentSceneView,
        /// <summary>
        /// Capture camera view from selected Hotspot
        /// </summary>
        FromHotspot
    }

    [Serializable]
    public class Thumbnail
    {
        [JsonIgnore]
        [Description(
            "Capture camera view from the first Hotspot", 
            "Capture camera view from current Scene View, this will be what you see on your Scene View when you click on the Export button",
            "Capture camera view from selected Hotspot"
            )]
        public CaptureMethodType type = CaptureMethodType.FirstHotspot;
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
            return type == CaptureMethodType.FromHotspot;
        }
    }
}
