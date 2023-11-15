using UnityEngine;
using UnityEngine.EventSystems;
using AD.BASE;
using UnityEngine.InputSystem;
using AD.Utility;

namespace AD.UI
{
    public class TouchPanel : AD.UI.ADUI
    {
        public TouchPanel()
        {
            ElementArea = "TouchPanel";
        }

        public Camera TargetCamera;
        public float Distance;

        protected virtual void Start()
        {
            ADUI.Initialize(this);
            LeftRegisterInfo = ADGlobalSystem.AddListener(Mouse.current.leftButton,
                () =>
                {
                    if (Selected)
                        OnClickWhenCurrentWasPressLeft.Invoke(TargetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3().SetZ(Distance)));
                }, PressType.ThisFramePressed);
            RightRegisterInfo = ADGlobalSystem.AddListener(Mouse.current.rightButton,
                () =>
                {
                    if (Selected)
                        OnClickWhenCurrentWasPressRight.Invoke(TargetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3().SetZ(Distance)));
                }, PressType.ThisFramePressed);
        }
        protected virtual void OnDestroy()
        {
            ADUI.Destory(this);
        }

        public override void InitializeContext()
        {
            base.InitializeContext();
            Context.OnBeginDragEvent = InitializeContextSingleEvent(Context.OnBeginDragEvent, OnBeginDrag);
            Context.OnDragEvent = InitializeContextSingleEvent(Context.OnDragEvent, OnDrag);
            Context.OnEndDragEvent = InitializeContextSingleEvent(Context.OnEndDragEvent, OnEndDrag);
        }

        [SerializeField] private Vector2 _Drag = Vector2.zero;

        public ADEvent<Vector2> OnEvent = new();
        public ADEvent<Vector3> OnClickWhenCurrentWasPressLeft = new(), OnClickWhenCurrentWasPressRight = new();

        public Vector2 dragVec
        {
            get { return _Drag; }
            set { _Drag = value; OnEvent.Invoke(value); }
        }

        public void RemoveAllListeners()
        {
            OnEvent.RemoveAllListeners();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragVec = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragVec = eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragVec = Vector2.zero;
        }

        public RegisterInfo LeftRegisterInfo, RightRegisterInfo;
    }
}
