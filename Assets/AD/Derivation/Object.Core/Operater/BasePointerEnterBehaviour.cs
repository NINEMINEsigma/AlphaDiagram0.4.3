using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BasePointerEnterBehaviour : MonoBehaviour, IPointerEnterHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnPointerEnterEvent;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterEvent?.Invoke(eventData);
        }
    }
}
