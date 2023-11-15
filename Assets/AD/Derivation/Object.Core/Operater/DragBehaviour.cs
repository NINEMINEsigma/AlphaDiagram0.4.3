using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.UI
{
    [RequireComponent(typeof(BehaviourContext))]
    public class DragBehaviour : MonoBehaviour
    {
        [SerializeField] private BehaviourContext Context = null;
        public BehaviourContext GetBehaviourContext()
        {
            Context ??= this.GetOrAddComponent<BehaviourContext>();
            return Context;
        }
        public BehaviourContext DragBehaviourContext
        {
            get 
            { 
                Context ??= this.GetOrAddComponent<BehaviourContext>(); 
                return Context; 
            }
        }

        public bool isCanDrag = true;

        public void SetDragAble(bool isCanDrag)
        {
            this.isCanDrag = isCanDrag;
        }

        public void Init(RectTransform DragObjectInternal)
        {
            this.DragObjectInternal = DragObjectInternal;
            this.DragAreaInternal = transform.parent.transform as RectTransform;

            DragBehaviourContext.OnBeginDragEvent ??= new();
            DragBehaviourContext.OnDragEvent ??= new();

            DragBehaviourContext.OnBeginDragEvent.RemoveListener(this.OnBeginDrag);
            DragBehaviourContext.OnBeginDragEvent.AddListener(this.OnBeginDrag);
            DragBehaviourContext.OnDragEvent.RemoveListener(this.OnDrag);
            DragBehaviourContext.OnDragEvent.AddListener(this.OnDrag);
            DragBehaviourContext.locationValid = IsRaycastLocationValid;
        }

        public void Init(RectTransform DragObjectInternal, RectTransform DragAreaInternal)
        {
            this.DragObjectInternal = DragObjectInternal;
            this.DragAreaInternal = DragAreaInternal;

            DragBehaviourContext.OnBeginDragEvent ??= new();
            DragBehaviourContext.OnDragEvent ??= new();

            DragBehaviourContext.OnBeginDragEvent.RemoveListener(this.OnBeginDrag);
            DragBehaviourContext.OnBeginDragEvent.AddListener(this.OnBeginDrag);
            DragBehaviourContext.OnDragEvent.RemoveListener(this.OnDrag);
            DragBehaviourContext.OnDragEvent.AddListener(this.OnDrag);
            DragBehaviourContext.locationValid = IsRaycastLocationValid;
        }

        public bool topOnClick = true;

        private Vector2 originalLocalPointerPosition;
        private Vector3 originalPanelLocalPosition;

        private RectTransform DragObjectInternal;

        private RectTransform DragAreaInternal;

        public void OnBeginDrag(PointerEventData data)
        {
            if (!isCanDrag) return;
            originalPanelLocalPosition = DragObjectInternal.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(DragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);
            gameObject.transform.SetAsLastSibling();

            if (topOnClick == true)
                DragObjectInternal.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData data)
        {
            if (!isCanDrag) return;
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DragAreaInternal, data.position, data.pressEventCamera, out localPointerPosition))
            {
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                DragObjectInternal.localPosition = originalPanelLocalPosition + offsetToOriginal;
            }

            ClampToArea();
        }

        private void ClampToArea()
        {
            Vector3 pos = DragObjectInternal.localPosition;

            Vector3 minPosition = DragAreaInternal.rect.min - DragObjectInternal.rect.min;
            Vector3 maxPosition = DragAreaInternal.rect.max - DragObjectInternal.rect.max;

            pos.x = Mathf.Clamp(DragObjectInternal.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(DragObjectInternal.localPosition.y, minPosition.y, maxPosition.y);

            DragObjectInternal.localPosition = pos;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return isCanDrag || transform.childCount != 0;
        }
    }
}
