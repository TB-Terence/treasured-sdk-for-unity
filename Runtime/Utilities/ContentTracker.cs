using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Treasured.UnitySdk
{
    public static class ContentTracker
    {
        private static bool _isTracking = false;
        private static HashSet<string> s_existingFiles = new HashSet<string>();
        private static HashSet<string> s_existingFolders = new HashSet<string>();

        public static void TrackFolder(string folderPath)
        {
            if (!_isTracking)
            {
                return;
            }
            string path = folderPath.Replace('/', '\\');
            s_existingFolders.Remove(path);
            s_existingFiles.RemoveWhere(x => x.StartsWith(path));
        }

        public static void TrackFolder(DirectoryInfo directoryInfo)
        {
            TrackFolder(directoryInfo.FullName);
        }

        public static void TrackFile(string folderPath)
        {
            if (!_isTracking)
            {
                return;
            }
            string path = folderPath.Replace('/', '\\');
            s_existingFiles.Remove(path);
            DirectoryInfo directory = new DirectoryInfo(path);
            while(directory.Parent != null)
            {
                s_existingFolders.Remove(directory.FullName);
                directory = directory.Parent;
            }
        }

        public static void TrackFile(FileInfo fileInfo)
        {
            TrackFile(fileInfo.FullName);
        }

        /// <summary>
        /// Tracks current state of the root folder.
        /// </summary>
        /// <param name="root"></param>
        public static void BeginTrack(string root)
        {
            new HashSet<string>();
            s_existingFiles = new HashSet<string>(Directory.GetFiles(root, "*.*", SearchOption.AllDirectories).Select(x => x.Replace('/', '\\')));
            s_existingFolders = new HashSet<string>(Directory.GetDirectories(root, "*.*", SearchOption.AllDirectories).Select(x => x.Replace('/', '\\')));
            _isTracking = true;
        }

        /// <summary>
        /// Remove files that that are added 
        /// </summary>
        public static void EndTrack()
        {
            if (!_isTracking)
            {
                return;
            }
            /* TODO: add ability to remove redundant files of current exporter if it's enabled
            foreach (string folder in s_existingFolders)
            {
            }
            foreach (string file in s_existingFiles)
            {
            }
            */
            _isTracking = false;
        }
    }
}
