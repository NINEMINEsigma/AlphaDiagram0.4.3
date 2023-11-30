using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseSubmitBehaviour : MonoBehaviour, ISubmitHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<BaseEventData> OnSubmitEvent;

        public void OnSubmit(BaseEventData eventData)
        {
            OnSubmitEvent?.Invoke(eventData);
        }
    }
}
