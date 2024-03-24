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

        private void OnClickWhenCurrentWasPressLeftListener()
        {
            if (Selected)
                OnClickWhenCurrentWasPressLeft.Invoke(TargetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3().SetZ(Distance)));
        }

        private void OnClickWhenCurrentWasPressRightListener()
        {
            if (Selected)
                OnClickWhenCurrentWasPressLeft.Invoke(TargetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3().SetZ(Distance)));
        }

        protected virtual void Start()
        {
            ADUI.Initialize(this);
            if (TargetCamera == null) TargetCamera = Camera.main;
            LeftRegisterInfo = ADGlobalSystem.AddListener(Mouse.current.leftButton, OnClickWhenCurrentWasPressLeftListener, AD.PressType.ThisFramePressed);
            RightRegisterInfo = ADGlobalSystem.AddListener(Mouse.current.rightButton, OnClickWhenCurrentWasPressRightListener, AD.PressType.ThisFramePressed);
        }
        protected virtual void OnDestroy()
        {
            ADUI.DestroyADUI(this);
            LeftRegisterInfo?.UnRegister();
            RightRegisterInfo?.UnRegister();
        }

        private void LateUpdate()
        {
            if (_Past != Mouse.current.position.ReadValue())
            {
                _Past = Mouse.current.position.ReadValue();
                IsPointerMoving = true;
            }
            else IsPointerMoving = false;
        }

        public override void InitializeContext()
        {
            base.InitializeContext();
            Context.OnBeginDragEvent = InitializeContextSingleEvent(Context.OnBeginDragEvent, OnBeginDrag);
            Context.OnDragEvent = InitializeContextSingleEvent(Context.OnDragEvent, OnDrag);
            Context.OnEndDragEvent = InitializeContextSingleEvent(Context.OnEndDragEvent, OnEndDrag);
        }

        [SerializeField] private Vector2 _Drag = Vector2.zero;
        private Vector2 _Past;

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
