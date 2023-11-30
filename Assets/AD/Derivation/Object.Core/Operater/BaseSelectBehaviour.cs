using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseSelectBehaviour : MonoBehaviour, ISelectHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<BaseEventData> OnSelectEvent;

        public void OnSelect(BaseEventData eventData)
        {
            OnSelectEvent?.Invoke(eventData);
        }
    }
}
