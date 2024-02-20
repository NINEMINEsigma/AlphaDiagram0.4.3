using System;
using System.Collections;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static AD.Utility.Object.CameraCore;
using static AD.Utility.RayExtension;

namespace AD.Utility.Object
{
    [Serializable]
    public class CameraCoreInfomation
    {
        public GameObject Target, PastOneTarget, FoucsOneTarget;
        public RayInfo RayForm;

        public RayBehavour RayBehavourForm;

        public AnimationCurve FollowCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float dalte = 0;
        public Vector3 PastPosition = Vector3.zero;
        public Vector3 PastEulerAngle = Vector3.zero;

        public float moveSpeed = 2.5f, rotionSpeed = 1;

        public TouchPanelRotatingButton UserButton = TouchPanelRotatingButton.Right;
    }

    public class CameraCore : ADController, ICanInitialize
    {
        public bool Is2D = false;
        public Camera Core;
        [SerializeField] private TouchPanel touchPanel;
        public TouchPanel TouchPanel
        {
            get => touchPanel;
            set
            {
                if (TouchPanel != null)
                {
                    touchPanel.OnEvent.RemoveListener(Rotating);
                }
                touchPanel = value;
                if (TouchPanel != null)
                {
                    touchPanel.OnEvent.AddListener(Rotating);
                }
            }
        }
        [SerializeField] private CameraCoreInfomation Infomation = new();

        public GameObject Target
        {
            get => Infomation.Target;
            set => Infomation.Target = value;
        }
        public GameObject PastOneTarget
        {
            get => Infomation.PastOneTarget;
            set => Infomation.PastOneTarget = value;
        }
        public GameObject FoucsOneTarget
        {
            get => Infomation.FoucsOneTarget;
            set => Infomation.FoucsOneTarget = value;
        }
        public RayInfo RayForm
        {
            get => Infomation.RayForm;
            set => Infomation.RayForm = value;
        }

        public RayBehavour RayBehavourForm
        {
            get => Infomation.RayBehavourForm;
            set => Infomation.RayBehavourForm = value;
        }

        public AnimationCurve FollowCurve
        {
            get => Infomation.FollowCurve;
            set => Infomation.FollowCurve = value;
        }
        public float dalte
        {
            get => Infomation.dalte;
            set => Infomation.dalte = value;
        }
        public Vector3 PastPosition
        {
            get => Infomation.PastPosition;
            set => Infomation.PastPosition = value;
        }
        public Vector3 PastEulerAngle
        {
            get => Infomation.PastEulerAngle;
            set => Infomation.PastEulerAngle = value;
        }

        public float moveSpeed
        {
            get => Infomation.moveSpeed;
            set => Infomation.moveSpeed = value;
        }
        public Vector3 moveSpeedVec = Vector3.one;
        public float rotionSpeed
        {
            get => Infomation.rotionSpeed;
            set => Infomation.rotionSpeed = value;
        }
        public Vector2 rotionSpeedVec = Vector2.one;

        public float near2DPanelZ = 10, far2DPanelZ = 50;
        public XYZA xyza = XYZA.Z;

        public enum XYZA
        {
            X, Y, Z
        }

        private bool CanIDragRotating = true;

        public TouchPanelRotatingButton UserButton
        {
            get => Infomation.UserButton;
            set => Infomation.UserButton = value;
        }

        public enum RayBehavour
        {
            None, Follow, JustCatch
        }

        public enum TouchPanelRotatingButton
        {
            Left, Right
        }

        private void Start()
        {
            RayForm = Core.GetRay();
            RayForm.mask = LayerMask.GetMask("Default");
            UpdateDirty();
            if (TouchPanel != null && !Is2D) TouchPanel.OnEvent.AddListener(Rotating);
        }

        public static bool IsLockKeyBoardDetectForMove = false;
        public bool IsLockKeyBoardDetectForMoveMyselfClone;

        public void SetCameraCoreLockKeyBoardDetect(bool boolen)
        {
            IsLockKeyBoardDetectForMove = boolen;
        }

        void Update()
        {
            IsLockKeyBoardDetectForMoveMyselfClone = IsLockKeyBoardDetectForMove;
            NotNoneMode(); FollowMode(); ClearCatch();
            CheakCanIRotating();
            if (Mouse.current.leftButton.isPressed == false)
            {
                //Detect Clear
                if (IsLockKeyBoardDetectForMove == false &&
                    PrimitiveExtension.ExecuteAny(ForwardMove(), BackMove(), LeftMove(), RightMove(), UpMove(), DownMove(), DragMove2D()))
                    ClearDirty();
            }
            if (Keyboard.current.zKey.isPressed && Keyboard.current.xKey.isPressed && Mouse.current.leftButton.wasPressedThisFrame) UndoPast();
        }

        private void CheakCanIRotating()
        {
            //No FoucsOneTarget and UserButton is Pressed
            CanIDragRotating = FoucsOneTarget == null && (UserButton == TouchPanelRotatingButton.Left ? Mouse.current.leftButton : Mouse.current.rightButton).isPressed;
        }

        private void ClearCatch()
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                PastOneTarget = FoucsOneTarget;
                var targetOOO = Target == null ? FoucsOneTarget : Target;
                if (targetOOO != null && targetOOO.transform.parent && targetOOO.transform.parent.TryGetComponent(out ColliderLayer layer)) layer.ParentGroup.DoUpdate(this, false);
                FoucsOneTarget = null;
            }
        }

        private Coroutine coroutiner;

        private void FollowMode()
        {
            if (RayBehavourForm == RayBehavour.Follow && Mouse.current.leftButton.wasPressedThisFrame && Keyboard.current.cKey.isPressed)
            {
                TryStartCoroutineMove();
            }
        }

        public void TryStartCoroutineMove()
        {
            if (Target != null)
            {
                PastOneTarget = Target;
                if (coroutiner != null) StopCoroutine(coroutiner);
                coroutiner = StartCoroutine(Move());
            }
        }

        public ADEvent<GameObject, RayInfo> CatchFocusEvent = new();

        private void NotNoneMode()
        {
            if (RayBehavourForm == RayBehavour.None || Mouse.current.leftButton.wasReleasedThisFrame) return;
            (RayForm = Core.RayCatchUpdate()).Update(false);
            if (RayForm.NearestRaycastHitForm.collider != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    FoucsOneTarget = RayForm.NearestRaycastHitForm.collider.gameObject;
                    CatchFocusEvent.Invoke(FoucsOneTarget, RayForm);
                }
                var cat = RayForm.NearestRaycastHitForm.collider.gameObject;
                var targetOOO = Target == null ? FoucsOneTarget : Target;
                if (targetOOO != null && targetOOO.transform.parent && targetOOO.transform.parent.TryGetComponent(out ColliderLayer layer))
                    layer.ParentGroup.DoUpdate(this, true);
                Target = cat;
            }
            else
            {
                var targetOOO = Target == null ? FoucsOneTarget : Target;
                if (targetOOO != null && targetOOO.transform.parent && targetOOO.transform.parent.TryGetComponent(out ColliderLayer layer))
                    layer.ParentGroup.DoUpdate(this, FoucsOneTarget != null);
                Target = null;
            }
        }


        #region Transform

        private void ClearDirty()
        {
            UpdateDirty();
        }

        private void UndoPast()
        {
            Core.transform.position = PastPosition;
            Core.transform.eulerAngles = PastEulerAngle;
        }

        private void UpdateDirty()
        {
            PastPosition = Core.transform.position;
            PastEulerAngle = Core.transform.transform.eulerAngles;
        }

        private void Rotating(Vector2 dragVec)
        {
            if (CanIDragRotating)
                Core.transform.localEulerAngles =
                    Core.transform.localEulerAngles
                    .AddX(rotionSpeedVec.y * dragVec.y * Time.deltaTime * rotionSpeed)
                    .AddY(-rotionSpeedVec.x * dragVec.x * Time.deltaTime * rotionSpeed);
        }

        private bool DragMove2D()
        {
            if (Mouse.current.middleButton.isPressed)
            {
                Core.transform.position -= moveSpeedVec.y * moveSpeed * Time.deltaTime * Core.transform.up * Mouse.current.delta.ReadValue().y;
                Core.transform.position -= moveSpeedVec.x * moveSpeed * Time.deltaTime * Core.transform.right * Mouse.current.delta.ReadValue().x;
                return true;
            }
            return false;
        }

        private bool DownMove()
        {
            if (Is2D)
            {
                if (!Keyboard.current.sKey.isPressed) return false;
                Core.transform.position -= moveSpeedVec.y * moveSpeed * Time.deltaTime * Core.transform.up;
                return true;
            }
            else
            {
                if (!Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.qKey.isPressed) return false;
                Core.transform.position -= moveSpeedVec.y * moveSpeed * Time.deltaTime * Core.transform.up;
                return true;
            }
        }

        private bool UpMove()
        {
            if (Is2D)
            {
                if (!Keyboard.current.wKey.isPressed) return false;
                Core.transform.position += moveSpeedVec.y * moveSpeed * Time.deltaTime * Core.transform.up;
                return true;
            }
            else
            {
                if (!Keyboard.current.spaceKey.isPressed && !Keyboard.current.eKey.isPressed) return false;
                Core.transform.position += moveSpeedVec.y * moveSpeed * Time.deltaTime * Core.transform.up;
                return true;
            }
        }

        private bool RightMove()
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                Core.transform.position += moveSpeedVec.x * moveSpeed * Time.deltaTime * Core.transform.right;
                return true;
            }
            return false;
        }

        private bool LeftMove()
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                Core.transform.position -= moveSpeedVec.x * moveSpeed * Time.deltaTime * Core.transform.right;
                return true;
            }
            return false;
        }

        private bool When2DContronZ(Vector3 dvalue)
        {
            float near = near2DPanelZ, far = far2DPanelZ;
            if (near2DPanelZ > far2DPanelZ)
            {
                near = far2DPanelZ;
                far = near2DPanelZ;
            }
            switch (xyza)
            {
                case XYZA.X:
                    {
                        if (Core.transform.position.x + dvalue.x < near)
                        {
                            Core.transform.position = Core.transform.position.SetX(near);
                            return false;
                        }
                    }
                    break;
                case XYZA.Y:
                    {
                        if (Core.transform.position.y + dvalue.y < near)
                        {
                            Core.transform.position = Core.transform.position.SetY(near);
                            return false;
                        }
                    }
                    break;
                case XYZA.Z:
                    {
                        if (Core.transform.position.z + dvalue.z < near)
                        {
                            Core.transform.position = Core.transform.position.SetZ(near);
                            return false;
                        };
                    }
                    break;
            }
            switch (xyza)
            {
                case XYZA.X:
                    {
                        if (Core.transform.position.x + dvalue.x > far)
                        {
                            Core.transform.position = Core.transform.position.SetX(far);
                            return false;
                        }
                    }
                    break;
                case XYZA.Y:
                    {
                        if (Core.transform.position.y + dvalue.y > far)
                        {
                            Core.transform.position = Core.transform.position.SetY(far);
                            return false;
                        }
                    }
                    break;
                case XYZA.Z:
                    {
                        if (Core.transform.position.z + dvalue.z > far)
                        {
                            Core.transform.position = Core.transform.position.SetZ(far);
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }

        private bool BackMove()
        {
            if (Is2D)
            {
                float value = Mouse.current.scroll.ReadValue().y;
                if (value < 0)
                {
                    var dvalue = 0.12f * moveSpeed * moveSpeedVec.z * Time.deltaTime * value * Core.transform.forward;
                    if (!When2DContronZ(dvalue)) return false;
                    Core.transform.position += dvalue;
                    return true;
                }
                return false;
            }
            else
            {
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    Core.transform.position -= moveSpeedVec.z * moveSpeed * Time.deltaTime * Core.transform.forward;
                    return true;
                }
                return false;
            }
        }

        private bool ForwardMove()
        {
            if (Is2D)
            {
                float value = Mouse.current.scroll.ReadValue().y;
                if (value > 0)
                {
                    var dvalue = 0.12f * moveSpeed * moveSpeedVec.z * Time.deltaTime * value * Core.transform.forward;
                    if (!When2DContronZ(dvalue)) return false;
                    Core.transform.position += dvalue;
                    return true;
                }
                return false;
            }
            else
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                {
                    Core.transform.position += moveSpeedVec.z * moveSpeed * Time.deltaTime * Core.transform.forward;
                    return true;
                }
                return false;
            }
        }

        #endregion

        public override void Init()
        {
            RayForm = null;
            Target = null;
            PastEulerAngle = Vector3.zero;
            PastPosition = Vector3.zero;
        }

        private IEnumerator Move()
        {
            dalte = 1;
            UpdateDirty();
            Vector3 startPosition = Core.transform.position;
            GameObject focus = PastOneTarget;
            if (Is2D)
            {
                Vector3 oPanelVec = Vector3.zero;
                switch (xyza)
                {
                    case XYZA.X:
                        oPanelVec = focus.transform.position.SetX(startPosition.x);
                        break;
                    case XYZA.Y:
                        oPanelVec = focus.transform.position.SetY(startPosition.y);
                        break;
                    case XYZA.Z:
                        oPanelVec = focus.transform.position.SetZ(startPosition.z);
                        break;
                }
                while (dalte > 0)
                {
                    dalte = Mathf.Max(0, dalte - Time.deltaTime);
                    Core.transform.position = Vector3.Lerp(startPosition, oPanelVec, FollowCurve.Evaluate(1 - dalte));
                    yield return null;
                }
                Core.transform.position = Vector3.Lerp(startPosition, oPanelVec, 1);
            }
            else
            {
                Core.transform.LookAt(Target.transform);
                if (Core.transform.localEulerAngles.x > 190) Core.transform.localEulerAngles = Core.transform.localEulerAngles.AddX(-1);
                if (Core.transform.localEulerAngles.x < 170) Core.transform.localEulerAngles = Core.transform.localEulerAngles.AddX(1);
                Vector3 fposition = focus.transform.position;
                while (dalte > 0)
                {
                    dalte = Mathf.Max(0, dalte - Time.deltaTime);
                    Core.transform.position = Vector3.Lerp(startPosition, fposition - Core.transform.forward * 10, FollowCurve.Evaluate(1 - dalte));
                    yield return null;
                }
                Core.transform.position = Vector3.Lerp(startPosition, fposition - Core.transform.forward * 10, 1);
            }
        }
    }
}
