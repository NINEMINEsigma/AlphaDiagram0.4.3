using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseBeginDragBehaviour : MonoBehaviour, IBeginDragHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnBeginDragEvent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(eventData);
        }
    }
}
