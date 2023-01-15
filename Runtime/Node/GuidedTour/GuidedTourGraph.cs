using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.GuidedTour
{
    public class GuidedTourGraph : XNode.NodeGraph
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class CreateTourAttribute : Attribute
        {
            public string[] TourNames { get; set; }
            public CreateTourAttribute(params string[] tourNames)
            {
                TourNames = tourNames;
            }
        }

        [SerializeField]
        [HideInInspector]
        private UnityEngine.Object _owner;
        public UnityEngine.Object Owner { get { return _owner; } internal set { _owner = value; } }

        public static GuidedTourGraph Create(UnityEngine.Object owner)
        {
            GuidedTourGraph graph = ScriptableObject.CreateInstance<GuidedTourGraph>();
            graph._owner = owner;
            return graph;
        }
    }
}
