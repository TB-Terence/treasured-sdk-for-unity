using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ObjectList))]
    class ObjectList : ExporterWindowMenuItem
    {
        public enum GroupToggleState
        {
            None,
            All,
            Mixed
        }

        ObjectListState[] objectListStates;
        private static readonly Type[] ObjectTypes = new Type[] { typeof(Hotspot), typeof(Interactable), typeof(SoundSource), typeof(VideoRenderer), typeof(HTMLEmbed) };

        public class ObjectListState
        {
            public Type type;
            public TreasuredScene scene;
            public List<TreasuredObject> objects;
            public Vector2 scrollPosition;
            public GroupToggleState toggleState;
            public bool enableAll;

            public void UpdateObjectList()
            {
                if (this.objects == null)
                {
                    this.objects = new List<TreasuredObject>();
                }
                var objects = scene.GetComponentsInChildren(type, true);
                foreach (var obj in objects)
                {
                    if (!this.objects.Contains(obj))
                    {
                        this.objects.Add((TreasuredObject)obj);
                    }
                }
            }
        }

        static class Styles
        {
            public static readonly GUIContent[] icons = new GUIContent[]
            {
                EditorGUIUtility.TrIconContent(Resources.Load<Texture2D>("Hotspot"), "Hotspot"),
                EditorGUIUtility.TrIconContent("GameObject On Icon", "Interactable"),
                EditorGUIUtility.TrIconContent("d_SceneViewAudio", "Sound Source"),
                EditorGUIUtility.TrIconContent("d_Profiler.Video", "Video Renderer"),
                EditorGUIUtility.TrIconContent("d_BuildSettings.WebGL", "HTML Embed")
            };

            public static readonly GUIContent plus = EditorGUIUtility.TrIconContent("CreateAddNew", "Create New");

            public static readonly GUIContent[] mode = new GUIContent[]
            {
                plus,
                EditorGUIUtility.TrIconContent("d_Transform Icon", "Edit"),
            };

            public static readonly GUIContent logo = new GUIContent(Resources.Load<Texture2D>("Treasured_Logo"));

            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle selectedObjectLabel = new GUIStyle(objectLabel)
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    background = Texture2D.grayTexture
                }
            };

            public static readonly GUIStyle wordWrapCenteredGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true
            };
        }

        int selectedTypeIndex = 0;

        public override void OnInspectorGUI()
        {
            if (objectListStates == null)
            {
                objectListStates = new ObjectListState[ObjectTypes.Length];
                for (int i = 0; i < ObjectTypes.Length; i++)
                {
                    objectListStates[i] = new ObjectListState()
                    {
                        type = ObjectTypes[i],
                        scene = Scene
                    };
                    objectListStates[i].UpdateObjectList();
                }
            }
            selectedTypeIndex = GUILayout.SelectionGrid(selectedTypeIndex, Styles.icons, Styles.icons.Length, GUILayout.Height(32f), GUILayout.MaxWidth(240));
            var state = objectListStates[selectedTypeIndex];
            if (state.objects.Count > 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        new GUIContent(state.type == typeof(Hotspot) ? "Order" : string.Empty,
                            state.type == typeof(Hotspot)
                                ? "The order of the Hotspot for the Guide Tour."
                                : string.Empty), GUILayout.Width(58));
                    EditorGUILayout.LabelField(new GUIContent("Name"), GUILayout.Width(64));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    int activeCount = state.objects.Count(x => x.gameObject.activeSelf);
                    if (activeCount == state.objects.Count)
                    {
                        state.toggleState = GroupToggleState.All;
                        state.enableAll = true;
                    }
                    else
                    {
                        state.toggleState = activeCount == 0
                            ? GroupToggleState.None
                            : GroupToggleState.Mixed;
                        state.enableAll = false;
                    }

                    EditorGUI.showMixedValue = state.toggleState == GroupToggleState.Mixed;
                    GUILayout.Space(3);
                    EditorGUI.BeginChangeCheck();
                    state.enableAll = EditorGUILayout.ToggleLeft(GUIContent.none, state.enableAll);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var obj in state.objects)
                        {
                            obj.gameObject.SetActive(state.enableAll);
                        }
                    }

                    EditorGUI.showMixedValue = false;
                }
            }
            if (state.objects.Count == 0)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(
                    $"No {ObjectNames.NicifyVariableName(state.type.Name)} Found",
                    Styles.wordWrapCenteredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }
            else
            {
                using (var scope = new EditorGUILayout.ScrollViewScope(state.scrollPosition))
                {
                    state.scrollPosition = scope.scrollPosition;
                    for (int index = 0; index < state.objects.Count; index++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            TreasuredObject current = state.objects[index];
                            using (new EditorGUILayout.VerticalScope())
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    // TODO: width 40 only show up to 10000
                                    EditorGUI.BeginChangeCheck();
                                    bool active = EditorGUILayout.Toggle(GUIContent.none,
                                        current.gameObject.activeSelf, GUILayout.Width(20));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        current.gameObject.SetActive(active);
                                    }

                                    EditorGUILayout.LabelField($"{index + 1}", GUILayout.Width(32));
                                    using (var hs = new EditorGUILayout.HorizontalScope())
                                    {
                                        using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                                        {
                                            EditorGUILayout.LabelField(
                                                new GUIContent(current.gameObject.name, current.Id));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
