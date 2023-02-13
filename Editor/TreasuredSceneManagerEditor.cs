using System;
using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk.Utilities;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(TreasuredSceneManager))]
    public class TreasuredSceneManagerEditor : Editor
    {
        public static class Styles
        {
            public static readonly GUIContent alignView = EditorGUIUtility.TrTextContent("Align View");
            public static readonly GUIContent snapAllToGround = EditorGUIUtility.TrTextContent("Snap All on Ground");
            public static readonly GUIContent selectAll = EditorGUIUtility.TrTextContent("Select All");

            public static readonly GUIContent searchObjects =
                EditorGUIUtility.TrTextContent("Search", "Search objects by Id or name");

            public static readonly GUIContent folderOpened =
                EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show in Explorer");

            public static readonly GUIContent search = EditorGUIUtility.TrIconContent("Search Icon");

            public static readonly GUIContent plus = EditorGUIUtility.TrIconContent("Toolbar Plus");
            public static readonly GUIContent minus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove");

            public static readonly GUIStyle logoText = new GUIStyle(EditorStyles.boldLabel)
            {
                wordWrap = false,
                fontSize = 18,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };

            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle button = new GUIStyle("label")
            {
                margin = new RectOffset(),
                padding = new RectOffset(),
            };

            public static readonly GUIStyle iconButton = new GUIStyle("button")
            {
                margin = new RectOffset(0, 0, 0, 8),
                fixedHeight = 24,
                fontStyle = FontStyle.Bold,
            };

            public static readonly GUIStyle dataDisplayBox = new GUIStyle("box")
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 8, 8, 8),
                alignment = TextAnchor.UpperLeft,
                fontSize = 10
            };

            public static readonly GUIStyle centeredLabel = new GUIStyle("label")
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
            };

            public static readonly GUIStyle exportButton = new GUIStyle("button")
            {
                padding = new RectOffset(8, 8, 8, 8),
                fontStyle = FontStyle.Bold,
            };

            public static readonly GUIStyle noLabel = new GUIStyle("label")
            {
                fixedWidth = 1
            };

            public static readonly GUIContent ToolDescription = new GUIContent("Treasured is a tool to help you create and export your Unity scenes to the web. For more information, visit treasured.dev for more info");

            private static GUIStyle tabButton;

            public static GUIStyle TabButton
            {
                get
                {
                    if (tabButton == null)
                    {
                        tabButton = new GUIStyle(EditorStyles.toolbarButton)
                        {
                            fixedHeight = 32,
                            fontStyle = FontStyle.Bold,
                            padding = new RectOffset(8, 8, 0, 0),
                            normal =
                            {
                                background = Texture2D.blackTexture
                            },
                            onNormal =
                            {
                                background = CreateTexture2D(1, 1,
                                    new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.3f))
                            }
                        };
                    }

                    return tabButton;
                }
            }

            private static GUIStyle borderlessBoxOdd;

            /// <summary>
            /// Box without margin
            /// </summary>
            public static GUIStyle BorderlessBoxOdd
            {
                get
                {
                    if (borderlessBoxOdd == null)
                    {
                        borderlessBoxOdd = new GUIStyle("box")
                        {
                            margin = new RectOffset(0, 0, 2, 0),
                            padding = new RectOffset(0, 0, 6, 6),
                            normal = new GUIStyleState()
                            {
                                background = CreateTexture2D(1, 1, new Color(1, 1, 1, 0.05f))
                            }
                        };
                    }

                    return borderlessBoxOdd;
                }
            }

            private static GUIStyle borderlessBoxEven;

            /// <summary>
            /// Box without margin
            /// </summary>
            public static GUIStyle BorderlessBoxEven
            {
                get
                {
                    if (borderlessBoxEven == null)
                    {
                        borderlessBoxEven = new GUIStyle("box")
                        {
                            margin = new RectOffset(0, 0, 2, 0),
                            padding = new RectOffset(0, 0, 6, 6)
                        };
                    }

                    return borderlessBoxEven;
                }
            }

            public static Texture2D CreateTexture2D(int width, int height, Color color)
            {
                Color[] colors = Enumerable.Repeat(color, width * height).ToArray();
                Texture2D texture = new Texture2D(width, height);
                texture.SetPixels(colors);
                texture.Apply();
                return texture;
            }
        }

        class SerializedPropertyGroup
        {
            public List<SerializedProperty> properties;
        }

        public class TabEditor
        {
            public class TabPage
            {
                public string tabName;
                public List<SerializedPropertyInfo> serializedProperties;
            }
            public SerializedObject serializedObject;
            Dictionary<string, List<SerializedPropertyInfo>> _tabPages = new Dictionary<string, List<SerializedPropertyInfo>>();
            public int selectedTabIndex;
            
            public TabEditor(SerializedObject serializedObject)
            {
                this.serializedObject = serializedObject;
                Initialize();
            }

            void Initialize()
            {
                var properties = EditorReflectionUtilities.GetSerializedProperties(serializedObject.targetObject);
                foreach (var property in properties)
                {
                    GroupAttribute groupAttribute = property.fieldInfo.GetCustomAttribute<GroupAttribute>();
                    if (groupAttribute != null)
                    {
                        AddToPage(string.IsNullOrEmpty(groupAttribute.GroupName) ? property.serializedProperty.displayName : groupAttribute.GroupName, property);
                    }
                    else
                    {
                        AddToPage("root", property);
                    }
                }
            }

            void AddToPage(string groupName, SerializedPropertyInfo serializedPropertyInfo)
            {
                if (string.IsNullOrEmpty(groupName))
                {
                    groupName = "root";
                }
                if (!_tabPages.ContainsKey(groupName))
                {
                    _tabPages.Add(groupName, new List<SerializedPropertyInfo>());
                }
                _tabPages[groupName].Add(serializedPropertyInfo);
            }

            public void OnGUI()
            {
                string[] tabs = _tabPages.Keys.ToArray();
                selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabs, GUILayout.Height(32f));
                foreach (var group in _tabPages)
                {
                    if (group.Key == tabs[selectedTabIndex])
                    {
                        foreach (var property in group.Value)
                        {
                            EditorGUIUtils.DrawPropertyWithoutFoldout(property.serializedProperty);
                        }
                    }
                }
            }
        }

        private TabEditor _tabEditor;

        private void OnEnable()
        {
            _tabEditor = new TabEditor(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            DrawHeaderGUI();
            //EditorGUIUtils.DrawProperties(serializedObject);
            _tabEditor.OnGUI();
        }

        private void DrawHeaderGUI()
        {
            Texture2D TreasuredLogo = Resources.Load<Texture2D>("Treasured_Logo");
            EditorGUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(TreasuredLogo, GUILayout.Width(42f), GUILayout.Height(42f));
                GUILayout.Space(4);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(12);
                    GUILayout.Label("Treasured Unity SDK", Styles.logoText);
                    GUILayout.Space(12);
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);
            GUILayout.Label(Styles.ToolDescription, Styles.centeredLabel);
            GUILayout.Space(10);
        }
    }
}
