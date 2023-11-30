using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseDragBehaviour : MonoBehaviour, IDragHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnDragEvent;

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
        }
    }
}
