using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("customEmbedCode")]
    public class CustomEmbedCodeAction : ScriptableAction
    {
        public string title;
        [TextArea(3, 5)]
        public string description;
        //[Url]
        //public string previewUrl;
        [JsonIgnore]
        [Label("Code (deprecated, use html instead)")]
        [TextArea(3, 5)]
        public string code;
        public CustomHTML html;


        //[JsonProperty("scripts")]
        //public string[] Scripts
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(code))
        //        {
        //            return new string[0];
        //        }
        //        var doc = new HtmlDocument();
        //        doc.LoadHtml(code);
        //        List<string> scripts = new List<string>();
        //        var scriptNodes = doc.DocumentNode.SelectNodes("//script");
        //        // SelectNodes returns null when length is 0
        //        if (scriptNodes != null)
        //        {
        //            foreach (var scriptNode in scriptNodes)
        //            {
        //                var src = scriptNode.GetAttributeValue("src", "");
        //                if (!string.IsNullOrEmpty(src)) scripts.Add(src);
        //            }
        //        }
        //        return scripts.ToArray();
        //    }
        //}
    }
}
