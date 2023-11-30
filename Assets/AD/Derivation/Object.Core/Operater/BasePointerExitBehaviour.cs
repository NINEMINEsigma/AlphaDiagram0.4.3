using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BasePointerExitBehaviour : MonoBehaviour, IPointerExitHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnPointerExitEvent;

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitEvent?.Invoke(eventData);
        }
    }
}
