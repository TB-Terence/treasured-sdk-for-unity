using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk.Export
{
    public class JsonExportProcess : ExportProcess
    {
        public Formatting formatting = Formatting.Indented;
        [Tooltip("Convert to Three JS Transform")]
        public bool convertTransform = true;
    }
}
