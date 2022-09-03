using System;
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
        [MenuItem("Tools/Treasured/Upgrade Treasured CLI", priority = 99)]
        static void UpgradeTreasuredCLI()
        {
            EditorUtility.DisplayProgressBar("Installing Treasured CLI", "Installing the Treasured CLI from npm", 0.5f);
                            
            try
            {
                using (Process process = new Process()) {
                    process.StartInfo.FileName = "npm";
                    process.StartInfo.Arguments = "install -g @treasured/cli";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    
                    process.Start();
            
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    UnityEngine.Debug.Log(output);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            EditorUtility.DisplayProgressBar("Installing Treasured CLI", "Installing the Treasured CLI from npm", 1f);
            EditorUtility.ClearProgressBar();

            // Get version of Treasured CLI
            string version = "";
            try
            {
                using (Process process = new Process()) {
                    process.StartInfo.FileName = "treasured";
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    
                    process.Start();
                    
                    process.WaitForExit();
                    version = process.StandardOutput.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            // Show message
            if (!string.IsNullOrEmpty(version))
            {
                EditorUtility.DisplayDialog("Treasured CLI installed", "Treasured CLI version " + version + " installed successfully", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Treasured CLI installation failed", "Treasured CLI installation failed. Please try again.", "OK");
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

        static bool IsTreasuredMapSelected()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }

        static void CreateNew<T>() where T : TreasuredObject
        {
            TreasuredMap map = Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
            map?.CreateObject<T>();
        }

        [MenuItem("GameObject/Treasured/Sound Source", false, 49)]
        static void CreateSoundSource()
        {
            CreateNew<SoundSource>();
        }

        [MenuItem("GameObject/Treasured/Sound Source", true, 49)]
        static bool CanCreateSoundSource()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Hotspot", false, 49)]
        static void CreateHotspot()
        {
            CreateNew<Hotspot>();
        }

        [MenuItem("GameObject/Treasured/Hotspot", true, 49)]
        static bool CanCreateHotspot()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Interactable", false, 49)]
        static void CreateInteractable()
        {
            CreateNew<Interactable>();
        }

        [MenuItem("GameObject/Treasured/Interactable", true, 49)]
        static bool CanCreateInteractable()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Video Renderer", false, 49)]
        static void CreateVideoRenderer()
        {
            CreateNew<VideoRenderer>();
        }

        [MenuItem("GameObject/Treasured/Video Renderer", true, 49)]
        static bool CanCreateVideoRenderer()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/HTML Embed", false, 49)]
        static void CreateHTMLEmbed()
        {
            CreateNew<HTMLEmbed>();
        }

        [MenuItem("GameObject/Treasured/HTML Embed", true, 49)]
        static bool CanCreateHTMLEmbed()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Empty Map", false, 49)]
        static void CreateEmptyMap()
        {
            GameObject map = new GameObject("Treasured Map", typeof(TreasuredMap));
            if (Selection.activeGameObject)
            {
                map.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Empty Map", true, 49)]
        static bool CanCreateEmptyMap()
        {
            if (Selection.activeGameObject == null)
            {
                return true;
            }
            return !Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
        }
    }
}
