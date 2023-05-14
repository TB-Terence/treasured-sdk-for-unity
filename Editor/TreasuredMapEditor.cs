﻿using System;
using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(TreasuredMap))]
    class TreasuredMapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Deprecated Component, use Treasured Scene instead", MessageType.Error);
            if (GUILayout.Button("Convert", GUILayout.Height(48)))
            {
                var map = (TreasuredMap)target;
                var scene = map.gameObject.AddComponent<TreasuredScene>();
                Convert(map, scene);
                DestroyImmediate(map);
            }
        }

        void Convert(TreasuredMap map, TreasuredScene scene)
        {
            if (map.name == "Treasured Map")
            {
                map.name = "Treasured Scene";
            }
            scene.creator = map.Author;
            scene.title = map.Title;
            scene.description = map.Description;

            scene.sceneInfo.backgroundMusicInfo.remoteUri = map.audioUrl;

            scene.themeInfo.darkMode = map.uiSettings.darkMode;
            scene.themeInfo.templateLoader = new TemplateLoader();
            scene.themeInfo.templateLoader.template = map.templateLoader.template;
            scene.themeInfo.templateLoader.imageUrl = map.templateLoader.imageUrl;
            scene.themeInfo.templateLoader.autoCameraRotation = map.templateLoader.autoCameraRotation;

            scene.features = map.features;

            scene.exportSettings = ScriptableObject.CreateInstance<ExportSettings>();
            scene.exportSettings.folderName = map.exportSettings.folderName;
            scene.exportSettings.optimizeScene = map.exportSettings.optimizeScene;
        }

    }
}
