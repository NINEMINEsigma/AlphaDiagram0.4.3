using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseCancelBehaviour : MonoBehaviour, ICancelHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<BaseEventData> OnCancelEvent;

        public void OnCancel(BaseEventData eventData)
        {
            OnCancelEvent?.Invoke(eventData);
        }
    }
}
