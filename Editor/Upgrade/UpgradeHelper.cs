﻿using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class UpgradeHelper
    {
        [MenuItem("CONTEXT/TreasuredMap/Upgrade to Version 0.5.0.0")]
        static void Upgrade(MenuCommand command)
        {
            TreasuredMap map = (TreasuredMap)command.context;
            if (map == null || map.Data == null)
            {
                return;
            }
            SerializedObject serializedObject = new SerializedObject(map);
            serializedObject.FindProperty("_id").stringValue = map.Data.Id;
            serializedObject.FindProperty("_title").stringValue = map.Data.Title;
            serializedObject.FindProperty("_description").stringValue = map.Data.Description;
            serializedObject.FindProperty("_loop").boolValue = map.Data.Loop;
            foreach (var hotspot in map.Hotspots)
            {
                if (hotspot.Data == null)
                {
                    continue;
                }
                SerializedObject obj = new SerializedObject(hotspot);
                SerializedProperty data = obj.FindProperty("_data");

                SerializedProperty oldId = data.FindPropertyRelative("_id");
                SerializedProperty newId = obj.FindProperty("_id");

                MigrateAction(data.FindPropertyRelative("_onSelected"), obj.FindProperty("_onSelected"));

                newId.stringValue = oldId.stringValue;

                obj.ApplyModifiedProperties();
            }
            foreach (var interactable in map.Interactables)
            {
                SerializedObject obj = new SerializedObject(interactable);
                SerializedProperty data = obj.FindProperty("_data");
                SerializedProperty oldId = data.FindPropertyRelative("_id");
                SerializedProperty newId = obj.FindProperty("_id");

                MigrateAction(data.FindPropertyRelative("_onSelected"), obj.FindProperty("_onSelected"));

                newId.stringValue = oldId.stringValue;

                obj.ApplyModifiedProperties();
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.DisplayDialog("Completed", "Upgrade completed, to ensure all data is moved over enable `Debug` view on the Inspector.", "Ok");
        }

        static void MigrateAction(SerializedProperty oldOnSelected, SerializedProperty newOnSelected)
        {
            newOnSelected.arraySize = 0;
            for (int i = 0; i < oldOnSelected.arraySize; i++)
            {
                SerializedProperty oldElement = oldOnSelected.GetArrayElementAtIndex(i);
                SerializedProperty newElement = null;

                SerializedProperty _type = oldElement.FindPropertyRelative("_type");
                SerializedProperty _id = oldElement.FindPropertyRelative("_id");

                //SerializedProperty _src;
                //SerializedProperty _targetId;
                //SerializedProperty _displayMode = DisplayMode.Right;
                //SerializedProperty _content;
                //SerializedProperty _style;
                //SerializedProperty _volume = 100;
                switch (_type.stringValue)
                {
                    case "openLink":
                        newElement = newOnSelected.AppendManagedObject(typeof(OpenLinkAction));
                        newElement.FindPropertyRelative("_src").stringValue = oldElement.FindPropertyRelative("_src").stringValue;
                        break;
                    case "showText":
                        newElement = newOnSelected.AppendManagedObject(typeof(ShowTextAction));
                        newElement.FindPropertyRelative("_content").stringValue = oldElement.FindPropertyRelative("_content").stringValue;
                        break;
                    case "playAudio":
                        newElement = newOnSelected.AppendManagedObject(typeof(PlayAudioAction));
                        newElement.FindPropertyRelative("_volume").stringValue = oldElement.FindPropertyRelative("_volume").stringValue;
                        break;
                    case "playVideo":
                        newElement = newOnSelected.AppendManagedObject(typeof(OpenLinkAction));

                        break;
                    case "selectObject":
                        newElement = newOnSelected.AppendManagedObject(typeof(SelectObjectAction));
                        newElement.FindPropertyRelative("_targetId").stringValue = oldElement.FindPropertyRelative("_targetId").stringValue;
                        break;
                }
                if (newElement != null)
                {
                    newElement.FindPropertyRelative("_id").stringValue = _id.stringValue;
                }
            }
        }
    }
}
