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
