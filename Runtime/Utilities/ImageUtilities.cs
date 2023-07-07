using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Color = UnityEngine.Color;

namespace Treasured.UnitySdk
{
    internal class ImageUtilies
    {
#if UNITY_STANDALONE_WIN
        private static readonly string TreasuredPluginsFolder = Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins/Win");

        private static string ktx2Converter = Path.Combine(TreasuredPluginsFolder,
            "Ktx2Converter.bat").ToOSSpecificPath();

        private static string toktx = Path.Combine(TreasuredPluginsFolder, "toktx.exe").ToOSSpecificPath();

#elif UNITY_STANDALONE_OSX
        private static readonly string TreasuredPluginsFolder = Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins/OSX");

        private static string ktx2Converter = Path.Combine(TreasuredPluginsFolder,
                                                           "Ktx2Converter.sh").ToOSSpecificPath();

        private static string toktx = Path.Combine(TreasuredPluginsFolder, "toktx").ToOSSpecificPath();
#endif

        /// <summary>
        /// Encode Image in WebP format
        /// </summary>
        /// <param name="inputImageBytes">Image Byte array data</param>
        /// <param name="outputPath">Output path to save encoded image</param>
        /// <param name="imageQualityPercentage">Image quality percentage ranging from 1% - 100%</param>
        private static void EncodeToWEBP(byte[] inputImageBytes, string outputPath, int imageQualityPercentage)
        {
            ISupportedImageFormat webpFormat = new WebPFormat { Quality = imageQualityPercentage };

            if (inputImageBytes == null)
            {
                Debug.Log($"Error encoding WebP image: Image data is empty");
                return;
            }

            using (MemoryStream imageData = new MemoryStream(inputImageBytes))
            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
            {
                try
                {
                    imageFactory.Load(imageData).Format(webpFormat).Save(outputPath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw new TreasuredException ("Encoding Error", $"Unable to encode in WebP format: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Convert all PNG files under root directory to KTX2
        /// </summary>
        /// <param name="rootDirectory">Root Directory of PNG images folder</param>
        [Obsolete]
        public static void ConvertToKTX2(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                Debug.LogError("KTX2 encoding failed! RootDirectory is empty.");
                return;
            }

            var argument = $"(\"{ktx2Converter}\" \"{toktx}\" \"{rootDirectory}\")";

            var ktxProcess = ProcessUtilities.CreateProcess(argument);
            ktxProcess.Start();
            try
            {
#if UNITY_EDITOR
#if UNITY_STANDALONE_WIN
                while (!ktxProcess.StandardOutput.EndOfStream)
                {
                    if (int.TryParse(ktxProcess.StandardOutput.ReadLine(), out int percentage))
                    {
                        if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Encoding", $"Please wait. Encoding to KTX2 format.", percentage / 100f))
                        {
                            ProcessUtilities.KillProcess(ktxProcess);
                            break;
                        }
                    }
                }
#elif UNITY_STANDALONE_OSX
                string stdOutput = ktxProcess.StandardOutput.ReadToEnd();
                ktxProcess.WaitForExit();
                if (!string.IsNullOrEmpty(stdOutput))
                {
                    UnityEngine.Debug.Log(stdOutput);
                }
#endif
#endif
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            finally
            {
                ktxProcess?.Dispose();
            }
        }

        public static void Encode(Texture2D texture, string directory, string fileName, ImageFormat format, int imageQualityPercentage = 100)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException();
            }
            string extension = format.ToString().ToLower();
            string path = $"{directory}/{fileName}.{extension}".ToOSSpecificPath();
            byte[] bytes = format == ImageFormat.JPG ? texture.EncodeToJPG(imageQualityPercentage) : texture.EncodeToPNG();
            switch (format)
            {
                case ImageFormat.JPG:
                case ImageFormat.PNG:
                    File.WriteAllBytes(path, bytes);
                    break;
                case ImageFormat.WEBP:
                    EncodeToWEBP(bytes, path, imageQualityPercentage);
                    break;
            }
        }

        public static void FlipPixels(Texture2D texture, bool flipX, bool flipY)
        {
            texture.SetPixels32(FlipPixels(texture.GetPixels32(), texture.width, texture.height, flipX, flipY));
            texture.Apply();
        }

        public static Color32[] FlipPixels(Color32[] colors, int width, int height, bool flipX, bool flipY)
        {
            var flippedPixels = Enumerable.Range(0, width * height).Select(index =>
            {
                int x = index % width;
                int y = index / height;
                if (flipX)
                    x = width - 1 - x;

                if (flipY)
                    y = height - 1 - y;

                return colors[y * width + x];
            });
            return flippedPixels.ToArray();
        }

        public static Color[] FlipPixels(Color[] colors, int width, int height, bool flipX, bool flipY)
        {
            var flippedPixels = Enumerable.Range(0, width * height).Select(index =>
            {
                int x = index % width;
                int y = index / height;
                if (flipX)
                    x = width - 1 - x;

                if (flipY)
                    y = height - 1 - y;

                return colors[y * width + x];
            });
            return flippedPixels.ToArray();
        }
    }
}
