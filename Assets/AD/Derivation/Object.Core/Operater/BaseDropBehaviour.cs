using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseDropBehaviour : MonoBehaviour, IDropHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnDropEvent;

        public void OnDrop(PointerEventData eventData)
        {
            OnDropEvent?.Invoke(eventData);
        }
    }
}
