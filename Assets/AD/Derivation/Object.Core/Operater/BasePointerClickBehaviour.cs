using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BasePointerClickBehaviour : MonoBehaviour, IPointerClickHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnPointerClickEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickEvent?.Invoke(eventData);
        }
    }
}
