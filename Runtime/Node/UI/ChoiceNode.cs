using System;
using UnityEngine;

namespace Treasured.Actions
{
    [CreateNodeMenu("Treasured/Inputs/Choice")]
    [HideBasePort("next")]
    public class ChoiceNode : ActionNode
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class ChoiceOptionAttribute : PropertyAttribute
        {
        }

        [TextArea]
        public string message;

        [Output(dynamicPortList = true)]
        [ChoiceOption]
        public string[] choices;
    }
}
