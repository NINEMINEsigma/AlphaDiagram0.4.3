using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseScrollBehaviour : MonoBehaviour, IScrollHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnScrollEvent;

        public void OnScroll(PointerEventData eventData)
        {
            OnScrollEvent?.Invoke(eventData);
        }
    }
}
