using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(HotspotGroup))]
    public class HotspotGroupEditor : Editor
    {
        private static readonly string[] Excludes = new string[] { "m_Script" };

        private static EditorCoroutine _guideTourCoroutine;
        private static Transform _currentHotspot;
        //private static double _nextTime;

        private HotspotGroup _hotspotGroup;

        private float _interval = 3;
        private HideFlags[] _childFlags;

     

        private void OnEnable()
        {
            _hotspotGroup = target as HotspotGroup;
            Tools.hidden = true;
        }

        private void OnDisable()
        {
            if (_guideTourCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_guideTourCoroutine);
            }
            Tools.hidden = false; // show the transform tools for other game object
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(false))
            {
                if (Excludes.Contains(iterator.name))
                {
                    continue;
                }
                EditorGUILayout.PropertyField(iterator);
            }
            serializedObject.ApplyModifiedProperties();
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create Hotspot"))
                    {
                        CreateHotspot(_hotspotGroup.transform);
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    _interval = EditorGUILayout.FloatField("Interval", _interval);
                    using (new EditorGUI.DisabledGroupScope(_guideTourCoroutine != null))
                    {
                        if (GUILayout.Button("Start"))
                        {
                            if (_guideTourCoroutine != null)
                            {
                                EditorCoroutineUtility.StopCoroutine(_guideTourCoroutine);
                            }
                            _guideTourCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(StartGuideTour());
                        }
                    }
                    using (new EditorGUI.DisabledGroupScope(_guideTourCoroutine == null))
                    {
                        if (GUILayout.Button("Stop"))
                        {
                            EditorCoroutineUtility.StopCoroutine(_guideTourCoroutine);
                            _guideTourCoroutine = null;
                            _currentHotspot = null;
                        }
                    }
                }
            }
            //if (_guideTourCoroutine != null && _currentHotspot != null)
            //{
            //    double timeLeft = _nextTime - EditorApplication.timeSinceStartup;
            //    EditorGUILayout.LabelField(new GUIContent("Viewing"), new GUIContent(_currentHotspot.name));
            //    using (var scope = new GUILayout.HorizontalScope(GUILayout.MinHeight(20)))
            //    {
            //        EditorGUILayout.LabelField(new GUIContent("Time left"));
            //        EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), (float)(timeLeft / _interval), $"{timeLeft:F1} s");
            //    }
            //    this.Repaint();
            //}
        }

        private HideFlags[] GetChildFlags(Transform transform)
        {
            HideFlags[] flags = new HideFlags[transform.childCount];
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                flags[i] = transform.parent.GetChild(i).hideFlags;
            }
            return flags;
        }

        IEnumerator StartGuideTour()
        {
            _childFlags = GetChildFlags(_hotspotGroup.transform);
           // _hotspotGroup.gameObject.hideFlags = HideFlags.NotEditable;
            if (_hotspotGroup.transform.childCount == 0)
            {
                yield break;
            }
            for (int i = 0; i < _hotspotGroup.transform.childCount; i++)
            {
                _hotspotGroup.transform.GetChild(i).hideFlags = HideFlags.NotEditable;
            }
            for (int i = 0; i < _hotspotGroup.transform.childCount; i++)
            {
                _currentHotspot = _hotspotGroup.transform.GetChild(i);
                _currentHotspot.MoveSceneView();
                //_nextTime = EditorApplication.timeSinceStartup + _interval;
                yield return new EditorWaitForSeconds(_interval);
            }
            _hotspotGroup.gameObject.hideFlags = HideFlags.None;
            _guideTourCoroutine = null;
            for (int i = 0; i < _hotspotGroup.transform.childCount; i++)
            {
                _hotspotGroup.transform.GetChild(i).hideFlags = _childFlags[i];
            }
        }

        private void OnSceneGUI()
        {
            var hotspots = _hotspotGroup.gameObject.GetComponentsInChildren<Hotspot>();
            Vector3 hotspotSize = new Vector3(0.5f, 0.5f, 0.5f);
            for (int i = 0; i < hotspots.Length; i++)
            {
                Hotspot current = hotspots[i];
                Hotspot next = hotspots[(i + 1) % hotspots.Length];

                Gizmos.color = Color.red;
                Handles.DrawWireCube(current.transform.position, hotspotSize);

                Vector3 nextDirection = next.transform.position - current.transform.position;
                Handles.color = Color.green;
                Handles.ArrowHandleCap(0, current.transform.position, Quaternion.LookRotation(nextDirection), 1, EventType.Repaint);
                Handles.color = Color.white;
                Handles.DrawDottedLine(current.transform.position, next.transform.position, 5);
            }
        }

        static void CreateHotspot(Transform parent)
        {
            if (!parent)
            {
                return;
            }
            GameObject hotspot = new GameObject("New Hotspot", typeof(Hotspot));
            hotspot.transform.SetParent(parent);
        }

        [MenuItem("GameObject/Treasuerd/Create Hotspot", false, 51)]
        static void CreateHotspot()
        {
            CreateHotspot(Selection.activeTransform);
        }

        [MenuItem("GameObject/Treasuerd/Create Hotspot", true, 51)]
        static bool CanCreateHotspot()
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponent<HotspotGroup>();
        }
    }
}
