using System.Collections.Generic;
using UnityEngine;

namespace Treasured.ExhibitX
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    public class Hotspot : MonoBehaviour
    {
        [SerializeReference]
        public List<InteractionData> interactions = new List<InteractionData>();

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!transform.parent || !this.isActiveAndEnabled)
            {
                return;
            }
            int nextIndex = NextEnabled(this.transform.GetSiblingIndex());
            if(nextIndex != -1)
            {
                UnityEditor.Handles.color = new Color(0.36f, 0.35f, 1f);
                Transform next = this.transform.parent.GetChild(nextIndex);
                UnityEditor.Handles.DrawDottedLine(this.transform.position, next.position, 3);
                UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.up, 0.5f);
                UnityEditor.Handles.color = Color.green;
                Vector3 lookRotation = next.position - this.transform.position;
                if(lookRotation != Vector3.zero)
                {
                    UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(lookRotation), 2, EventType.Repaint);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.Label(this.transform.position, transform.name);
        }

        private int NextEnabled(int startIndex)
        {
            int next = HotspotManager.Instance && HotspotManager.Instance.loop && startIndex == this.transform.parent.childCount - 1 ? 0 : startIndex + 1;
            if(next > transform.parent.childCount - 1 || next == this.transform.GetSiblingIndex())
            {
                return -1;
            }
            GameObject go = transform.parent.GetChild(next).gameObject;
            if (go.TryGetComponent<Hotspot>(out var hotspot) && hotspot.isActiveAndEnabled)
            {
                return next;
            }
            return NextEnabled(next);
        }
    }
#endif
}