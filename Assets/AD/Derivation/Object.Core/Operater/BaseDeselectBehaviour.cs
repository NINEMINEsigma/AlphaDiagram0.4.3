using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseDeselectBehaviour : MonoBehaviour, IDeselectHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<BaseEventData> OnDeselectEvent;

        public void OnDeselect(BaseEventData eventData)
        {
            OnDeselectEvent?.Invoke(eventData);
        }
    }
}
