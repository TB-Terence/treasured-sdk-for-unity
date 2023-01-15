using UnityEngine;
using UnityEditor;
using Treasured.Actions;

namespace Treasured.UnitySdk
{
    [CustomNodeEditor(typeof(StartTourNode))]
    public class StartTourNodeEditor : ActionNodeEditor
    {
        public override void OnBodyContentGUI()
        {
            StartTourNode startTourNode = (StartTourNode)target;
            EditorGUILayout.PrefixLabel(new GUIContent("Target"));
            if (EditorGUILayout.DropdownButton(new GUIContent(startTourNode.target ? (startTourNode.target as GuidedTour)?.title : "Select Tour"), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var tour in GuidedTourGraphEditor.Current?.tours)
                {
                    menu.AddItem(new GUIContent(tour.title), false, () =>
                    {
                        startTourNode.target = tour;
                    });
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Create New Tour"), false, () =>
                {
                    GuidedTour tour = ScriptableObject.CreateInstance<GuidedTour>();
                    tour.name += GuidedTourGraphEditor.Current?.tours.Count;
                    GuidedTourGraphEditor.Current?.tours.Add(tour);
                    startTourNode.target = tour;
                    serializedObject.ApplyModifiedProperties();
                });
                menu.ShowAsContext();
            }
        }
    }
}
