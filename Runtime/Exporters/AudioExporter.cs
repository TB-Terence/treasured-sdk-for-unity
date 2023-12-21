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

            ExportAudio(rootDirectory, audioDirectory, this.Scene.sceneInfo.backgroundMusicInfo);

            foreach (var obj in Scene.GetComponentsInChildren<TreasuredObject>())
            {
                foreach (var collection in obj.actionGraph.GetGroups())
                {
                    ExportAudioRecursivelyImpl(rootDirectory, audioDirectory, collection);
                }
            }
        }

        void ExportAudio(string rootDirectory, string audioDirectory, AudioInfo info)
        {
            if (!info.IsLocalContent() || fileNames.Contains(info.asset.name)) { return; }
            //  Copy audio clip to the audios folder
#if UNITY_EDITOR
            var src = AssetDatabase.GetAssetPath(info.asset);
            var dst = Path.Combine(audioDirectory, Path.GetFileName(src).Replace(' ', '-')).ToOSSpecificPath();
            FileUtil.ReplaceFile(src, dst);
            fileNames.Add(info.asset.name);
#endif
        }

        void ExportAudioRecursivelyImpl(string rootDirectory, string audioDirectory, ActionCollection actionCollection)
        {
            foreach (ScriptableAction sa in actionCollection)
            {
                if (sa is AudioAction audioAction)
                {
                    ExportAudio(rootDirectory, audioDirectory, audioAction.audioInfo);
                }
                else if (sa is GroupAction groupAction)
                {
                    ExportAudioRecursivelyImpl(rootDirectory, audioDirectory,  groupAction.actions);
                }
            }
        }
    }
}
