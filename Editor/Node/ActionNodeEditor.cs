using Treasured.Actions;
using UnityEngine;
using XNodeEditor;
using static XNode.Node;

namespace Treasured.UnitySdk
{
    [CustomNodeEditor(typeof(ActionNode))]
    public class ActionNodeEditor : XNodeEditor.NodeEditor
    {
        public sealed override void OnBodyGUI()
        {
            OnBodyHeaderGUI();
            OnBodyContentGUI();
            OnBodyFooterGUI();
        }

        public virtual void OnBodyHeaderGUI()
        {
            foreach (var port in target.Ports)
            {
                if (port.IsDynamic || 
                    (NodeEditorUtilities.GetCachedAttrib(target.GetType(), port.fieldName, out OutputAttribute outputAttr) && outputAttr.dynamicPortList) || 
                    (NodeEditorUtilities.GetAttrib(target.GetType(), out HideBasePortAttribute hideAttr) && port.fieldName == hideAttr.PortName))
                {
                    continue;
                }
                switch (port.direction)
                {
                    case XNode.NodePort.IO.Input:
                        NodeEditorGUILayout.PortField(new GUIContent(port.fieldName), target.GetInputPort(port.fieldName));
                        break;
                    case XNode.NodePort.IO.Output:
                        NodeEditorGUILayout.PortField(new GUIContent(port.fieldName), target.GetOutputPort(port.fieldName));
                        break;
                }
            }
        }

        public virtual void OnBodyContentGUI()
        {
            base.OnBodyGUI();
        }

        public virtual void OnBodyFooterGUI()
        {
        }

    }
}
