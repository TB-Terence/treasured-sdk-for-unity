using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public struct HTMLRect
    {
        public int top;
        public int left;
        public int width;
        public int height;
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
    }
}
