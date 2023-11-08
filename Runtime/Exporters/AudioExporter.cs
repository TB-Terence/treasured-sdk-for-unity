using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Treasured.UnitySdk
{
    public class AudioExporter : Exporter
    {
        static HashSet<string> fileNames = new HashSet<string>();
        public override DirectoryInfo CreateExportDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(base.CreateExportDirectoryInfo().FullName, "audios"));
        }

        public override void Export()
        {
            fileNames.Clear();
            var audioDirectory = CreateExportDirectoryInfo().FullName;
            var rootDirectory = Application.dataPath;

            AudioInfo bgmInfo = this.Scene.sceneInfo.backgroundMusicInfo;
            ExportAudio(rootDirectory, audioDirectory, this.Scene.sceneInfo.backgroundMusicInfo);

            foreach (var obj in Scene.GetComponentsInChildren<TreasuredObject>())
            {
                foreach (var graph in obj.actionGraph.GetGroups())
                {
                    foreach (var action in graph)
                    {
                        if (action is AudioAction audioAction)
                        {
                            ExportAudio(rootDirectory, audioDirectory, audioAction.audioInfo);
                        }
                    }
                }
            }
        }

        void ExportAudio(string rootDirectory, string audioDirectory, AudioInfo info)
        {
            if (!info.IsLocalContent() || fileNames.Contains(info.asset.name)) { return; }
            //  Copy audio clip to the audios folder
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(info.asset);
            FileUtil.ReplaceFile(Path.Combine(rootDirectory, Path.GetFileName(path)).ToOSSpecificPath(),
                Path.Combine(audioDirectory, Path.GetFileName(path)).ToOSSpecificPath());

            fileNames.Add(info.asset.name);
#endif
        }
    }
}
