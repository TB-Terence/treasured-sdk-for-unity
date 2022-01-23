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

namespace Treasured.UnitySdk
{
    internal class ImageUtilies
    {
        private static string processName = "cmd.exe";
        private static readonly string TreasuredPluginsFolder = Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins");
        private static string ktx2Converter = Path.Combine(TreasuredPluginsFolder,
                                                           "Ktx2Converter.bat").Replace(" ", "^ ");
        private static string toktx = Path.Combine(TreasuredPluginsFolder, "toktx.exe").Replace(" ", "^ ");

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
                    Debug.Log($"Error encoding WebP image: {e.Message}");
                    return;
                }
            }
        }

        /// <summary>
        /// Encode PNG images to KTX2 format
        /// </summary>
        /// <param name="rootDirectory">Root Directory of PNG images folder</param>
        private static void EncodeToKTX2(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                Debug.LogError("KTX2 encoding failed! RootDirectory is empty.");
                return;
            }

            var modifiedDirectory = rootDirectory.Replace(" ", "^ ");

            var argumentBuilder = new StringBuilder();
            argumentBuilder.Append("/K ");
            argumentBuilder.Append(ktx2Converter);
            argumentBuilder.Append($" \"{toktx}\"");
            argumentBuilder.Append($" \"{modifiedDirectory}\"");

            var startInfo = new ProcessStartInfo(processName, argumentBuilder.ToString());
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            using var ktxProcess = new Process() { StartInfo = startInfo };
            ktxProcess.Start();
            try
            {
                ktxProcess.WaitForExit();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            finally
            {
                ktxProcess.Dispose();
            }
        }

        public static void Encode(Texture2D texture, string directory, string fileName, ImageFormat format, int imageQualityPercentage = 100)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException();
            }
            string extension = format.ToString().ToLower();
            string path = $"{directory}/{fileName}.{extension}";
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
                case ImageFormat.Ktx2:
                    //EncodeToKTX2(directory);
                    File.WriteAllBytes(path, bytes);
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
