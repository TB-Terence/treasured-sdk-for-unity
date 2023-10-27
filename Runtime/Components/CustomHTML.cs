using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public struct HTMLRect
    {
        public enum Style
        {
            Predefined,
            Customize
        }
        public Style style;
        [ShowIf("!IsPredefined")]
        //[ExportIf("!IsPredefined")]
        public int top;
        [ShowIf("!IsPredefined")]
       // [ExportIf("!IsPredefined")]
        public int left;
        [ShowIf("!IsPredefined")]
       // [ExportIf("!IsPredefined")]
        public int width;
        [ShowIf("!IsPredefined")]
       // [ExportIf("!IsPredefined")]
        public int height;
        [ShowIf("IsPredefined")]
        //[ExportIf("IsPredefined")]
        public WidgetPosition position;

        bool IsPredefined()
        {
            return style == Style.Predefined;
        }
    }

    [Serializable]
    public struct ScriptNode
    {
        public string src;
        public string inner;
    }

    [Serializable]
    public class CustomHTML
    {
        [TextArea]
        [Tooltip("Custom script in the head HTML")]
        public string headHTML;
        [TextArea]
        public string bodyHTML;
        public HTMLRect position;

        public List<ScriptNode> ScriptNodes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(headHTML))
                {
                    return new List<ScriptNode>();
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(headHTML);
                List<ScriptNode> scripts = new List<ScriptNode>();
                var scriptNodes = doc.DocumentNode.SelectNodes("//script");
                // SelectNodes returns null when length is 0
                if (scriptNodes != null)
                {
                    foreach (var scriptNode in scriptNodes)
                    {
                        var src = scriptNode.GetAttributeValue("src", "");
                        var node = new ScriptNode()
                        {
                            src = src,
                            inner = scriptNode.InnerHtml

                        };
                        scripts.Add(node);
                    }
                }
                return scripts;
            }
        }

        public string Src
        {
            get
            {
                string result = string.Empty;
                if (string.IsNullOrWhiteSpace(bodyHTML))
                {
                    return result;
                }
                else if (IsValidUrl(bodyHTML))
                {
                    return bodyHTML;
                }
                else
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(bodyHTML);
                    // SelectNodes returns null when length is 0
                    var nodes = doc.DocumentNode.SelectNodes("//iframe");
                    return nodes != null ? nodes.FirstOrDefault()?.GetAttributeValue("src", result) : result;
                }
            }
        }

        bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && 
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
