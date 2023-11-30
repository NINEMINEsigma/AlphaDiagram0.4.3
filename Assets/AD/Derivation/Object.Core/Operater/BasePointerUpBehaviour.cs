using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BasePointerUpBehaviour : MonoBehaviour, IPointerUpHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnPointerUpEvent;

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke(eventData);
        }
    }
}
