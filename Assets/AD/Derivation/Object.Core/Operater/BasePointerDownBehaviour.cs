using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BasePointerDownBehaviour : MonoBehaviour, IPointerDownHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnPointerDownEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke(eventData);
        }
    }
}
