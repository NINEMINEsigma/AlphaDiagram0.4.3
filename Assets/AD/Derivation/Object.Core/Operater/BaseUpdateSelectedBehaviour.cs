using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseUpdateSelectedBehaviour : MonoBehaviour, IUpdateSelectedHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<BaseEventData> OnUpdateSelectedEvent;

        public void OnUpdateSelected(BaseEventData eventData)
        {
            OnUpdateSelectedEvent?.Invoke(eventData);
        }
    }
}
