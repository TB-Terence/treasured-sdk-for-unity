using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Treasured.UnitySdk
{
    internal class MaskExportProcess : ExportProcess
    {
        private static readonly int[] builtInLayers = new int[] { 0, 1, 2, 4, 5 };

        private static Material objectIdConverter;
        private static int colorId;

        public override bool Enabled { get; set; } = false;

        private CubemapFormat cubemapFormat = CubemapFormat.SixFaces;
        private int qualityPercentage = 75;


        public override void OnGUI(SerializedObject serializedObject)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                qualityPercentage = EditorGUILayout.IntSlider(new GUIContent("Quality Percentage"), qualityPercentage, 1, 100);
                GUILayout.Label("%");
            }
        }

        public override void Export(string rootDirectory, TreasuredMap map)
        {
            SerializedObject serializedObject = new SerializedObject(map);
            if (builtInLayers.Contains(serializedObject.FindProperty("_interactableLayer").intValue))
            {
                Debug.LogError("Can not use a built-in layer as the Interactable Layer.");
                return;
            }
            var hotspots = ValidateHotspots(map);
            int interactableLayer = new SerializedObject(map).FindProperty("_interactableLayer").intValue; // single layer
            Color maskColor = map.MaskColor;
            Color backgroundColor = Color.black;

            Dictionary<Renderer, Material> defaultMaterials = new Dictionary<Renderer, Material>();
            Dictionary<Renderer, int> defaultLayers = new Dictionary<Renderer, int>();
            Dictionary<TreasuredObject, bool> defaultObjectStates = new Dictionary<TreasuredObject, bool>();
            List<GameObject> tempHotspots = new List<GameObject>();
            var cameraGO = new GameObject("Cubemap Camera"); // creates a temporary camera with some default settings.
            cameraGO.hideFlags = HideFlags.DontSave;
            var camera = cameraGO.AddComponent<Camera>();
            var cameraData = cameraGO.AddComponent<HDAdditionalCameraData>();
            if (cameraData == null)
            {
                throw new MissingComponentException("Missing HDAdditionalCameraData component");
            }
            camera.cullingMask = 1 << interactableLayer;
            ImageFormat imageFormat = map.Format;
            //  If imageFormat is KTX2 then export images as png and then later convert them to KTX2 format  
            ImageFormat imageFormatParser = (imageFormat == ImageFormat.KTX2) ? ImageFormat.PNG : imageFormat;

            try
            {
                TreasuredObject[] objects = map.GetComponentsInChildren<TreasuredObject>();
                foreach (var obj in objects)
                {
                    defaultObjectStates[obj] = obj.gameObject.activeSelf;
                }
                if (objectIdConverter == null)
                {
                    objectIdConverter = new Material(Shader.Find("Hidden/ObjectId"));
                }


                #region HDRP camera settings
                cameraData.backgroundColorHDR = backgroundColor;
                cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                cameraData.volumeLayerMask = 0; // ensure no volume effects will affect the object id color
                cameraData.probeLayerMask = 0; // ensure no probe effects will affect the object id color
                #endregion
                int size = Mathf.Min(Mathf.NextPowerOfTwo((int)ImageQuality.Low), 8192);
                Cubemap cubemap = new Cubemap(size * 6, TextureFormat.ARGB32, false);
                Texture2D texture = new Texture2D(cubemap.width * (cubemapFormat == CubemapFormat.Single ? 6 : 1), cubemap.height, TextureFormat.ARGB32, false);

                // Create tempory hotspot object
                foreach (var hotspot in hotspots)
                {
                    GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tempGO.hideFlags = HideFlags.HideAndDontSave;
                    tempGO.transform.SetParent(hotspot.Hitbox.transform);
                    tempGO.transform.localPosition = Vector3.zero;
                    tempGO.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
                    tempGO.layer = interactableLayer;
                    tempHotspots.Add(tempGO);
                }

                // Set Object Color
                for (int i = 0; i < objects.Length; i++)
                {
                    TreasuredObject obj = objects[i];
                    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    foreach (var renderer in renderers)
                    {
                        defaultLayers[renderer] = renderer.gameObject.layer;
                        renderer.gameObject.layer = interactableLayer;
                        defaultMaterials[renderer] = renderer.sharedMaterial;
                        renderer.sharedMaterial = objectIdConverter;
                        renderer.GetPropertyBlock(mpb);
                        mpb.SetColor("_IdColor", maskColor);
                        renderer.SetPropertyBlock(mpb);
                    }
                }


                for (int index = 0; index < hotspots.Length; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Mask ({index + 1}/{hotspots.Length})";
                    string progressText = $"Generating data for {current.name}...";
                    current.gameObject.SetActive(true);
                    foreach (var obj in objects)
                    {
                        obj.gameObject.SetActive(true);
                    }
                    var visibleTargets = current.VisibleTargets;
                    foreach (var invisibleTarget in objects.Except(visibleTargets))
                    {
                        invisibleTarget.gameObject.SetActive(false);
                    }
                    camera.transform.SetPositionAndRotation(current.Camera.transform.position, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemap))
                    {
                        throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
                    }
                    var di = Directory.CreateDirectory(Path.Combine(rootDirectory, "images", current.Id).Replace('/', '\\'));
                    for (int i = 0; i < 6; i++)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                        {
                            throw new TreasuredException("Export canceled", "Export canceled by the user.");
                        }
                        texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                        ImageUtilies.FlipPixels(texture, true, imageFormat != ImageFormat.KTX2);
                        ImageUtilies.Encode(texture, di.FullName, "mask_" + SimplifyCubemapFace((CubemapFace)i), imageFormatParser, qualityPercentage);
                    }
                }

            }
            finally
            {
                foreach (var kvp in defaultMaterials)
                {
                    kvp.Key.sharedMaterial = kvp.Value;
                }
                // Restore layer
                foreach (var kvp in defaultLayers)
                {
                    kvp.Key.gameObject.layer = kvp.Value;
                }

                foreach (var kvp in defaultObjectStates)
                {
                    kvp.Key.gameObject.SetActive(kvp.Value);
                }

                foreach (var tempHotspot in tempHotspots)
                {
                    GameObject.DestroyImmediate(tempHotspot);
                }

                if (cameraGO != null)
                {
                    GameObject.DestroyImmediate(cameraGO);
                    cameraGO = null;
                }

                if (objectIdConverter != null)
                {
                    GameObject.DestroyImmediate(objectIdConverter);
                    objectIdConverter = null;
                }

                if (imageFormat == ImageFormat.KTX2)
                {
                    EditorUtility.DisplayCancelableProgressBar("Encoding Masks To KTX format", "Encoding in progress..", 0.5f);
                    ImageUtilies.Encode(null, rootDirectory, null, ImageFormat.KTX2);
                }
            }
        }

        private Hotspot[] ValidateHotspots(TreasuredMap map)
        {
            var hotspots = map.Hotspots;
            if (hotspots == null || hotspots.Length == 0)
            {
                throw new InvalidOperationException("No active hotspots.");
            }
            return hotspots;
        }

        private string SimplifyCubemapFace(CubemapFace cubemapFace)
        {
            switch (cubemapFace)
            {
                case CubemapFace.PositiveX:
                    return "px";
                case CubemapFace.NegativeX:
                    return "nx";
                case CubemapFace.PositiveY:
                    return "py";
                case CubemapFace.NegativeY:
                    return "ny";
                case CubemapFace.PositiveZ:
                    return "pz";
                case CubemapFace.NegativeZ:
                    return "nz";
                case CubemapFace.Unknown:
                default:
                    return "unknown";
            }
        }
    }
}
