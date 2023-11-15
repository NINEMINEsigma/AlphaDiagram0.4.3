using AD.BASE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class BasePointerEnterBehaviour : MonoBehaviour, IPointerEnterHandler
    {
        public ADOrderlyEvent<PointerEventData> OnPointerEnterEvent;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterEvent?.Invoke(eventData);
        }
    }

    public class BasePointerExitBehaviour : MonoBehaviour, IPointerExitHandler
    {
        public ADOrderlyEvent<PointerEventData> OnPointerExitEvent;

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitEvent?.Invoke(eventData);
        }
    }

    public class BasePointerDownBehaviour : MonoBehaviour, IPointerDownHandler
    {
        public ADOrderlyEvent<PointerEventData> OnPointerDownEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke(eventData);
        }
    }

    public class BasePointerUpBehaviour : MonoBehaviour, IPointerUpHandler
    {
        public ADOrderlyEvent<PointerEventData> OnPointerUpEvent;

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke(eventData);
        }
    }

    public class BasePointerClickBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public ADOrderlyEvent<PointerEventData> OnPointerClickEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickEvent?.Invoke(eventData);
        }
    }

    public class BaseInitializePotentialDragBehaviour : MonoBehaviour, IInitializePotentialDragHandler
    {
        public ADOrderlyEvent<PointerEventData> OnInitializePotentialDragEvent;

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            OnInitializePotentialDragEvent?.Invoke(eventData);
        }
    }

    public class BaseBeginDragBehaviour : MonoBehaviour, IBeginDragHandler
    {
        public ADOrderlyEvent<PointerEventData> OnBeginDragEvent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(eventData);
        }
    }

    public class BaseDragBehaviour : MonoBehaviour, IDragHandler
    {
        public ADOrderlyEvent<PointerEventData> OnDragEvent;

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
        }
    }

    public class BaseEndDragBehaviour : MonoBehaviour, IEndDragHandler
    {
        public ADOrderlyEvent<PointerEventData> OnEndDragEvent;

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData);
        }
    }

    public class BaseDropBehaviour : MonoBehaviour, IDropHandler
    {
        public ADOrderlyEvent<PointerEventData> OnDropEvent;

        public void OnDrop(PointerEventData eventData)
        {
            OnDropEvent?.Invoke(eventData);
        }
    }

    public class BaseScrollBehaviour : MonoBehaviour, IScrollHandler
    {
        public ADOrderlyEvent<PointerEventData> OnScrollEvent;

        public void OnScroll(PointerEventData eventData)
        {
            OnScrollEvent?.Invoke(eventData);
        }
    }

    public class BaseUpdateSelectedBehaviour : MonoBehaviour, IUpdateSelectedHandler
    {
        public ADOrderlyEvent<BaseEventData> OnUpdateSelectedEvent;

        public void OnUpdateSelected(BaseEventData eventData)
        {
            OnUpdateSelectedEvent?.Invoke(eventData);
        }
    }

    public class BaseSelectBehaviour : MonoBehaviour, ISelectHandler
    {
        public ADOrderlyEvent<BaseEventData> OnSelectEvent;

        public void OnSelect(BaseEventData eventData)
        {
            OnSelectEvent?.Invoke(eventData);
        }
    }

    public class BaseDeselectBehaviour : MonoBehaviour, IDeselectHandler
    {
        public ADOrderlyEvent<BaseEventData> OnDeselectEvent;

        public void OnDeselect(BaseEventData eventData)
        {
            OnDeselectEvent?.Invoke(eventData);
        }
    }

    public class BaseMoveBehaviour : MonoBehaviour, IMoveHandler
    {
        public ADOrderlyEvent<AxisEventData> OnMoveEvent;

        public void OnMove(AxisEventData eventData)
        {
            OnMoveEvent?.Invoke(eventData);
        }
    }

    public class BaseSubmitBehaviour : MonoBehaviour, ISubmitHandler
    {
        public ADOrderlyEvent<BaseEventData> OnSubmitEvent;

        public void OnSubmit(BaseEventData eventData)
        {
            OnSubmitEvent?.Invoke(eventData);
        }
    }

    public class BaseCancelBehaviour : MonoBehaviour, ICancelHandler
    {
        public ADOrderlyEvent<BaseEventData> OnCancelEvent;

        public void OnCancel(BaseEventData eventData)
        {
            OnCancelEvent?.Invoke(eventData);
        }
    }

    public class BehaviourContext : MonoBehaviour,
        ICanvasRaycastFilter
    {
        public ADOrderlyEvent<PointerEventData> OnBeginDragEvent
        {
            get
            {
                var cat = this.GetComponent<BaseBeginDragBehaviour>();
                if(cat==null) return null;
                return cat.OnBeginDragEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseBeginDragBehaviour>();
                cat.OnBeginDragEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnDragEvent
        {
            get
            {
                var cat = this.GetComponent<BaseDragBehaviour>();
                if (cat == null) return null;
                return cat.OnDragEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseDragBehaviour>();
                cat.OnDragEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnDropEvent
        {
            get
            {
                var cat = this.GetComponent<BaseDropBehaviour>();
                if (cat == null) return null;
                return cat.OnDropEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseDropBehaviour>();
                cat.OnDropEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnEndDragEvent
        {
            get
            {
                var cat = this.GetComponent<BaseEndDragBehaviour>();
                if (cat == null) return null;
                return cat.OnEndDragEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseEndDragBehaviour>();
                cat.OnEndDragEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnInitializePotentialDragEvent
        {
            get
            {
                var cat = this.GetComponent<BaseInitializePotentialDragBehaviour>();
                if (cat == null) return null;
                return cat.OnInitializePotentialDragEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseInitializePotentialDragBehaviour>();
                cat.OnInitializePotentialDragEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnPointerClickEvent
        {
            get
            {
                var cat = this.GetComponent<BasePointerClickBehaviour>();
                if (cat == null) return null;
                return cat.OnPointerClickEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BasePointerClickBehaviour>();
                cat.OnPointerClickEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnPointerDownEvent
        {
            get
            {
                var cat = this.GetComponent<BasePointerDownBehaviour>();
                if (cat == null) return null;
                return cat.OnPointerDownEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BasePointerDownBehaviour>();
                cat.OnPointerDownEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnPointerEnterEvent
        {
            get
            {
                var cat = this.GetComponent<BasePointerEnterBehaviour>();
                if (cat == null) return null;
                return cat.OnPointerEnterEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BasePointerEnterBehaviour>();
                cat.OnPointerEnterEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnPointerExitEvent
        {
            get
            {
                var cat = this.GetComponent<BasePointerExitBehaviour>();
                if (cat == null) return null;
                return cat.OnPointerExitEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BasePointerExitBehaviour>();
                cat.OnPointerExitEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnPointerUpEvent
        {
            get
            {
                var cat = this.GetComponent<BasePointerUpBehaviour>();
                if (cat == null) return null;
                return cat.OnPointerUpEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BasePointerUpBehaviour>();
                cat.OnPointerUpEvent = value;
            }
        }
        public ADOrderlyEvent<PointerEventData> OnScrollEvent
        {
            get
            {
                var cat = this.GetComponent<BaseScrollBehaviour>();
                if (cat == null) return null;
                return cat.OnScrollEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseScrollBehaviour>();
                cat.OnScrollEvent = value;
            }
        }

        public ADOrderlyEvent<BaseEventData> OnCancelEvent
        {
            get
            {
                var cat = this.GetComponent<BaseCancelBehaviour>();
                if (cat == null) return null;
                return cat.OnCancelEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseCancelBehaviour>();
                cat.OnCancelEvent = value;
            }
        }
        public ADOrderlyEvent<BaseEventData> OnDeselectEvent
        {
            get
            {
                var cat = this.GetComponent<BaseDeselectBehaviour>();
                if (cat == null) return null;
                return cat.OnDeselectEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseDeselectBehaviour>();
                cat.OnDeselectEvent = value;
            }
        }
        public ADOrderlyEvent<BaseEventData> OnSelectEvent
        {
            get
            {
                var cat = this.GetComponent<BaseSelectBehaviour>();
                if (cat == null) return null;
                return cat.OnSelectEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseSelectBehaviour>();
                cat.OnSelectEvent = value;
            }
        }
        public ADOrderlyEvent<BaseEventData> OnSubmitEvent
        {
            get
            {
                var cat = this.GetComponent<BaseSubmitBehaviour>();
                if (cat == null) return null;
                return cat.OnSubmitEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseSubmitBehaviour>();
                cat.OnSubmitEvent = value;
            }
        }
        public ADOrderlyEvent<BaseEventData> OnUpdateSelectedEvent
        {
            get
            {
                var cat = this.GetComponent<BaseUpdateSelectedBehaviour>();
                if (cat == null) return null;
                return cat.OnUpdateSelectedEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseUpdateSelectedBehaviour>();
                cat.OnUpdateSelectedEvent = value;
            }
        }

        public ADOrderlyEvent<AxisEventData> OnMoveEvent
        {
            get
            {
                var cat = this.GetComponent<BaseMoveBehaviour>();
                if (cat == null) return null;
                return cat.OnMoveEvent;
            }
            set
            {
                var cat = this.GetOrAddComponent<BaseMoveBehaviour>();
                cat.OnMoveEvent = value;
            }
        }

        public delegate bool HowSetupRaycastLocationValid(Vector2 sp, Camera eventCamera);
        public HowSetupRaycastLocationValid locationValid;

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return locationValid?.Invoke(sp, eventCamera) ?? true;
        }
    }
}
