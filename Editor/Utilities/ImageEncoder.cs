using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using System;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ImageEncoder
    {
        /// <summary>
        /// Encode Image in WebP format
        /// </summary>
        /// <param name="inputImageBytes">Image Byte array data</param>
        /// <param name="outputPath">Output path to save encoded image</param>
        /// <param name="imageQualityPercentage">Image quality percentage ranging from 1% - 100%</param>
        internal static void EncodeToWEBP(byte[] inputImageBytes, string outputPath, int imageQualityPercentage)
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
    }
}
