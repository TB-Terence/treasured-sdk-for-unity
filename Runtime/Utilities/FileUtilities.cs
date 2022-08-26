using System;
using System.IO;

namespace Treasured.UnitySdk
{
    public static class FileUtilities
    {
        public static string ToOSSpecificPath(this string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
