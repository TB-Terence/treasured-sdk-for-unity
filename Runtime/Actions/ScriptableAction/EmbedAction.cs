using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("embed")]
    public class EmbedAction : ScriptableAction
    {
        [Url]
        [JsonIgnore]
        public string src;
        [JsonProperty("src")]
        public string Src
        {
            get
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return src;
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(code);
                var iframe = doc.DocumentNode.SelectSingleNode("//iframe");
                return iframe == null ? "" : iframe.GetAttributeValue("src", "");
            }
        }
        public string title;
        [TextArea(3, 5)]
        public string description;
        [Url]
        public string previewUrl;
        public EmbedPosition position = EmbedPosition.Fullscreen;
        [TextArea(3, 5)]
        [JsonIgnore]
        public string code;
        [JsonProperty("code")]
        public string Code
        {
            get
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(code);
                doc.DocumentNode?.FirstChild?.SetAttributeValue("display", "block");
                doc.DocumentNode?.FirstChild?.SetAttributeValue("width", "100%");
                Debug.LogError(doc.DocumentNode.OuterHtml);
                return doc.DocumentNode.OuterHtml;
            }
        }
        public string[] scripts = new string[0];
    }
}
