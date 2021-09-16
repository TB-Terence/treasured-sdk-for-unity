using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class TreasuredMapExporter
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true,
            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                args.ErrorContext.Handled = true;
            },
            //Converters = new JsonConverter[]
            //{
            //    //new StringEnumConverter(new CamelCaseNamingStrategy()),
            //    //new ScriptableObjectConverter(),
            ////    new InteractionConverter() // used by deserialize
            //}
        };

        public static void Export(TreasuredMap map, string folderName)
        {
            if(map == null)
            {
                return;
            }
            DirectoryInfo outputDirectory;
            try
            {
                outputDirectory = Directory.CreateDirectory($"{DefaultOutputFolderPath}/{folderName}");
            }
            catch(ArgumentException)
            {
                Debug.LogError($"Invalid folder name : {folderName}");
                return;
            }
            ExportJson(map, outputDirectory);
            Application.OpenURL(DefaultOutputFolderPath);
        }

        static void ExportJson(TreasuredMap map, DirectoryInfo outputDirectory)
        {
            string jsonPath = Path.Combine(outputDirectory.FullName, "data.json");
            string json = JsonConvert.SerializeObject(map, Formatting.Indented, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }

    }
}
