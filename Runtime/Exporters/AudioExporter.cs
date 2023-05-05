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
                foreach (var actionGroup in obj.OnClick)
                {
                    foreach (var action in actionGroup.Actions)
                    {
                        if (action is PlayAudioAction playAudioAction)
                        {
                            if (playAudioAction.audioClip == null)
                            {
                                continue;
                            }

                            if (audioFiles.Contains(playAudioAction.audioClip.name))
                            {
                                continue;
                            }

                            //  Copy audio clip to the audios folder
#if UNITY_EDITOR
                            var path = AssetDatabase.GetAssetPath(playAudioAction.audioClip);
                            FileUtil.ReplaceFile(Path.Combine(rootDirectory, path).ToOSSpecificPath(),
                                Path.Combine(audioDirectory, Path.GetFileName(path)).ToOSSpecificPath());

                            audioFiles.Add(playAudioAction.audioClip.name);
#endif
                        }
                    }
                }

                foreach (var actionGroup in obj.OnHover)
                {
                    foreach (var action in actionGroup.Actions)
                    {
                        if (action is PlayAudioAction playAudioAction)
                        {
                            if (playAudioAction.audioClip == null)
                            {
                                continue;
                            }

                            if (audioFiles.Contains(playAudioAction.audioClip.name))
                            {
                                continue;
                            }

                            //  Copy audio clip to the audios folder
#if UNITY_EDITOR
                            var path = AssetDatabase.GetAssetPath(playAudioAction.audioClip);
                            FileUtil.ReplaceFile(Path.Combine(rootDirectory, path).ToOSSpecificPath(),
                                Path.Combine(audioDirectory, Path.GetFileName(path).ToOSSpecificPath()));
                            
                            audioFiles.Add(playAudioAction.audioClip.name);
#endif
                        }
                    }
                }
            }
        }
    }
}
