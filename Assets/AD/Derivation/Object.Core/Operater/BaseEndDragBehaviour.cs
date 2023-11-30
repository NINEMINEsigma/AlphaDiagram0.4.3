using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseEndDragBehaviour : MonoBehaviour, IEndDragHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnEndDragEvent;

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData);
        }
    }
}
