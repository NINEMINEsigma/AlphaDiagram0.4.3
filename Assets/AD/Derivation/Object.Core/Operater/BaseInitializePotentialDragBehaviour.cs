using AD.BASE;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BaseInitializePotentialDragBehaviour : MonoBehaviour, IInitializePotentialDragHandler, IBehaviourOperator
    {
        public ADOrderlyEvent<PointerEventData> OnInitializePotentialDragEvent;

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            OnInitializePotentialDragEvent?.Invoke(eventData);
        }
    }
}
