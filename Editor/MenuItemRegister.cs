﻿using System;
using System.Reflection;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Class to register GameObject menu items.
    /// </summary>
    internal static class MenuItemRegister
    {
        #region Tools
        [MenuItem("Tools/Treasured/Upgrade Treasured CLI", priority = 99)]
        static void UpgradeTreasuredCLI()
        {
            EditorUtility.DisplayProgressBar("Installing Treasured CLI", "Installing the Treasured CLI from npm", 0.5f);

            try
            {
                using (Process process = ProcessUtilities.CreateProcess("npm install -g @treasured/cli"))
                {
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        UnityEngine.Debug.Log(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError(error);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                return;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayProgressBar("Installing Treasured CLI", "Installing the Treasured CLI from npm", 1f);
            EditorUtility.ClearProgressBar();

            // Get version of Treasured CLI
            string version = "";
            try
            {
                using (Process process = ProcessUtilities.CreateProcess("treasured --version"))
                {
                    process.Start();

                    process.WaitForExit();
                    version = process.StandardOutput.ReadToEnd();

                    string error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError(error);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                return;
            }

            // Show message
            if (!string.IsNullOrEmpty(version))
            {
                EditorUtility.DisplayDialog("Treasured CLI installed",
                    "Treasured CLI version " + version + " installed successfully", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Treasured CLI installation failed",
                    "Treasured CLI installation failed. Please try again.", "OK");
            }
        }

        [MenuItem("Tools/Treasured/Upgrade to Latest(Stable)", priority = 99)]
        static void UpgradeToStableVersion()
        {
            Client.Add("https://github.com/TB-Terence/treasured-sdk-for-unity.git#upm");
        }

        [MenuItem("Tools/Treasured/Upgrade to Latest(Experimental)", priority = 99)]
        static void UpgradeToExperimentalVersion()
        {
            Client.Add("https://github.com/TB-Terence/treasured-sdk-for-unity.git#exp");
        }
        #endregion

        static bool IsTreasuredSceneSelected()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredScene>();
        }

        static void CreateNew<T>() where T : TreasuredObject
        {
            TreasuredScene scene = Selection.activeGameObject?.GetComponentInParent<TreasuredScene>();
            scene?.CreateObject<T>();
        }

        [MenuItem("GameObject/Treasured/Sound Source", false, 49)]
        static void CreateSoundSource()
        {
            CreateNew<SoundSource>();
        }

        [MenuItem("GameObject/Treasured/Sound Source", true, 49)]
        static bool CanCreateSoundSource()
        {
            return IsTreasuredSceneSelected();
        }

        [MenuItem("GameObject/Treasured/Hotspot", false, 49)]
        static void CreateHotspot()
        {
            CreateNew<Hotspot>();
        }

        [MenuItem("GameObject/Treasured/Hotspot", true, 49)]
        static bool CanCreateHotspot()
        {
            return IsTreasuredSceneSelected();
        }

        [MenuItem("GameObject/Treasured/Interactable", false, 49)]
        static void CreateInteractable()
        {
            CreateNew<Interactable>();
        }

        [MenuItem("GameObject/Treasured/Interactable", true, 49)]
        static bool CanCreateInteractable()
        {
            return IsTreasuredSceneSelected();
        }

        [MenuItem("GameObject/Treasured/Video Renderer", false, 49)]
        static void CreateVideoRenderer()
        {
            CreateNew<VideoRenderer>();
        }

        [MenuItem("GameObject/Treasured/Video Renderer", true, 49)]
        static bool CanCreateVideoRenderer()
        {
            return IsTreasuredSceneSelected();
        }

        [MenuItem("GameObject/Treasured/HTML Embed", false, 49)]
        static void CreateHTMLEmbed()
        {
            CreateNew<HTMLEmbed>();
        }

        [MenuItem("GameObject/Treasured/HTML Embed", true, 49)]
        static bool CanCreateHTMLEmbed()
        {
            return IsTreasuredSceneSelected();
        }

        [MenuItem("GameObject/Treasured/Empty Scene", false, 49)]
        static void CreateEmptyScene()
        {
            GameObject map = new GameObject("Treasured Scene", typeof(TreasuredScene));
            if (Selection.activeGameObject)
            {
                map.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Empty Scene", true, 49)]
        static bool CanCreateEmptyScene()
        {
            if (Selection.activeGameObject == null)
            {
                return true;
            }

            return !Selection.activeGameObject.GetComponentInParent<TreasuredScene>();
        }
    }
}