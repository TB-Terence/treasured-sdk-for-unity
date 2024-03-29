using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Treasured.UnitySdk
{
    public class VideoExporter : Exporter
    {
        public override DirectoryInfo CreateExportDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(base.CreateExportDirectoryInfo().FullName, "videos"));
        }

        public override void Export()
        {
            var videoFiles = new HashSet<string>();
            var videoDirectory = CreateExportDirectoryInfo().FullName;
            var rootDirectory = Directory.GetCurrentDirectory();

            foreach (var obj in Scene.GetComponentsInChildren<TreasuredObject>())
            {
                if (obj is VideoRenderer videoRenderer)
                {
                    if (videoRenderer.videoInfo.asset == null)
                    {
                        continue;
                    }

                    if (videoFiles.Contains(videoRenderer.videoInfo.asset.name))
                    {
                        continue;
                    }
                    //  Copy video clip to the videos folder
#if UNITY_EDITOR
                    var path = AssetDatabase.GetAssetPath(videoRenderer.videoInfo.asset);
                    FileUtil.ReplaceFile(Path.Combine(rootDirectory, path).ToOSSpecificPath(),
                        Path.Combine(videoDirectory, Path.GetFileName(path)).ToOSSpecificPath());

                    videoFiles.Add(videoRenderer.videoInfo.asset.name);
#endif
                }
            }
        }
    }
}
