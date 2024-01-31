using UnityEngine;
using UnityEngine.EventSystems;
using AD.BASE;
using UnityEngine.InputSystem;
using AD.Utility;

namespace AD.UI
{
    [AddComponentMenu("UI/AD/TouchPanel", 100)]
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
            if (TargetCamera == null) TargetCamera = Camera.main;
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
            LeftRegisterInfo.UnRegister();
            RightRegisterInfo.UnRegister();
        }

        private void LateUpdate()
        {
            IsPointerMoving = false;
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
        public bool IsPointerMoving = false;

        public Vector2 DeltaDragVec => IsPointerMoving ? dragVec : Vector2.zero;

        public void RemoveAllListeners()
        {
            OnEvent.RemoveAllListeners();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragVec = Vector2.zero;
            IsPointerMoving = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragVec = eventData.delta;
            IsPointerMoving = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragVec = Vector2.zero;
            IsPointerMoving = false;
        }

        public RegisterInfo LeftRegisterInfo, RightRegisterInfo;
    }
}
