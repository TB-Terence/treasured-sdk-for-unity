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
        public override DirectoryInfo CreateExportDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(base.CreateExportDirectoryInfo().FullName, "audios"));
        }

        public override void Export()
        {
            var audioFiles = new HashSet<string>();
            var audioDirectory = CreateExportDirectoryInfo().FullName;
            var rootDirectory = Directory.GetCurrentDirectory();

            foreach (var obj in Scene.GetComponentsInChildren<TreasuredObject>())
            {
                foreach (var graph in obj.actionGraph.GetGroups())
                {
                    foreach (var action in graph)
                    {
                        if (action is AudioAction audioAction)
                        {
                            if (!audioAction.audioInfo.IsLocalContent())
                            {
                                continue;
                            }

                            if (audioFiles.Contains(audioAction.audioInfo.asset.name))
                            {
                                continue;
                            }

                            //  Copy audio clip to the audios folder
#if UNITY_EDITOR
                            var path = AssetDatabase.GetAssetPath(audioAction.audioInfo.asset);
                            FileUtil.ReplaceFile(Path.Combine(rootDirectory, path).ToOSSpecificPath(),
                                Path.Combine(audioDirectory, Path.GetFileName(path)).ToOSSpecificPath());

                            audioFiles.Add(audioAction.audioInfo.asset.name);
#endif
                        }
                    }
                }
            }
        }
    }
}
