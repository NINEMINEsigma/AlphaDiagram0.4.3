using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseMoveBehaviour : MonoBehaviour, IMoveHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<AxisEventData> OnMoveEvent;

        public void OnMove(AxisEventData eventData)
        {
            OnMoveEvent?.Invoke(eventData);
        }
    }
}
