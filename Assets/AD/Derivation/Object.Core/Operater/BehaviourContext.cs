using AD.BASE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public interface IBehaviourOperator { }

    /// <summary>
    /// 禁止在Awake时刻使用BehaviourContext
    /// </summary>
    public class BehaviourContext : MonoBehaviour,
        ICanvasRaycastFilter
    {
        public ADOrderlyEvent<PointerEventData> OnBeginDragEvent
        {
            get
            {
                if (!TryGetComponent<BaseBeginDragBehaviour>(out var cat)) return null;
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
                if (!this.TryGetComponent<BaseDragBehaviour>(out var cat)) return null;
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
                if (!this.TryGetComponent<BaseDropBehaviour>(out var cat)) return null;
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
                if (!this.TryGetComponent<BaseEndDragBehaviour>(out var cat)) return null;
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
                if (!this.TryGetComponent<BaseInitializePotentialDragBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BasePointerClickBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BasePointerDownBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BasePointerEnterBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BasePointerExitBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BasePointerUpBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseScrollBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseCancelBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseDeselectBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseSelectBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseSubmitBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseUpdateSelectedBehaviour>(out var cat))
                    return null;
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
                if (!this.TryGetComponent<BaseMoveBehaviour>(out var cat))
                    return null;
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

        private void Awake()
        {
            foreach (var item in GetComponents<IBehaviourOperator>())
            {
                Destroy(item.As<MonoBehaviour>());
            }
        }
        private void OnDestroy()
        {
            foreach (var item in GetComponents<IBehaviourOperator>())
            {
                Destroy(item.As<MonoBehaviour>());
            }
        }
    }
}
