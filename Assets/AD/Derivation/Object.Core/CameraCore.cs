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
        public float rotionSpeed
        {
            get => Infomation.rotionSpeed;
            set => Infomation.rotionSpeed = value;
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
            TouchPanel.OnEvent.AddListener(Rotating);
        }

        public static bool IsLockKeyBoardDetectForMove = false;

        public void SetCameraCoreLockKeyBoardDetect(bool boolen)
        {
            IsLockKeyBoardDetectForMove = boolen;
        }

        void Update()
        {
            NotNoneMode(); FollowMode(); ClearCatch();
            CheakCanIRotating();
            if (Mouse.current.leftButton.isPressed == false)
            {
                //Detect Clear
                if (IsLockKeyBoardDetectForMove == false &&
                    PrimitiveExtension.ExecuteAny(ForwardMove(), BackMove(), LeftMove(), RightMove(), UpMove(), DownMove())) 
                    ClearDirty();
            }
            if (
#if UNITY_EDITOR
                Keyboard.current.zKey.isPressed && Mouse.current.leftButton.wasPressedThisFrame
#else
                Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.zKey.isPressed && Mouse.current.leftButton.wasPressedThisFrame
#endif
                ) UndoPast();
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
                if (targetOOO != null && targetOOO.transform.parent.TryGetComponent(out ColliderLayer layer)) layer.ParentGroup.DoUpdate(this, false);
                FoucsOneTarget = null;
            }
        }

        private void FollowMode()
        {
            if (RayBehavourForm == RayBehavour.Follow && Mouse.current.leftButton.wasPressedThisFrame && Keyboard.current.cKey.isPressed)
            {
                if (Target != null)
                {
                    StopCoroutine(nameof(Move));
                    StartCoroutine(Move());
                }
            }
        }

        private void NotNoneMode()
        {
            if (RayBehavourForm == RayBehavour.None || Mouse.current.leftButton.wasReleasedThisFrame) return;
            (RayForm = Core.RayCatchUpdate()).Update(false);
            if (RayForm.NearestRaycastHitForm.collider != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    FoucsOneTarget = RayForm.NearestRaycastHitForm.collider.gameObject;
                }
                var cat = RayForm.NearestRaycastHitForm.collider.gameObject;
                var targetOOO = Target == null ? FoucsOneTarget : Target;
                if (targetOOO != null && targetOOO.transform.parent.TryGetComponent(out ColliderLayer layer)) layer.ParentGroup.DoUpdate(this, true);
                Target = cat;
            }
            else
            {
                var targetOOO = Target == null ? FoucsOneTarget : Target;
                if (targetOOO != null && targetOOO.transform.parent.TryGetComponent(out ColliderLayer layer)) layer.ParentGroup.DoUpdate(this, FoucsOneTarget != null);
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
                Core.transform.localEulerAngles = Core.transform.localEulerAngles.AddX(dragVec.y * Time.deltaTime * rotionSpeed).AddY(-dragVec.x * Time.deltaTime * rotionSpeed);
        }

        private bool DownMove()
        {
            if (!Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.qKey.isPressed) return false;
            Core.transform.position -= moveSpeed * Time.deltaTime * Core.transform.up;
            return true;
        }

        private bool UpMove()
        {
            if (!Keyboard.current.spaceKey.isPressed && !Keyboard.current.eKey.isPressed) return false;
            Core.transform.position += moveSpeed * Time.deltaTime * Core.transform.up;
            return true;
        }

        private bool RightMove()
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                Core.transform.position += moveSpeed * Time.deltaTime * Core.transform.right;
                return true;
            }
            return false;
        }

        private bool LeftMove()
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                Core.transform.position -= moveSpeed * Time.deltaTime * Core.transform.right;
                return true;
            }
            return false;
        }

        private bool BackMove()
        {
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                Core.transform.position -= moveSpeed * Time.deltaTime * Core.transform.forward;
                return true;
            }
            return false;
        }

        private bool ForwardMove()
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                Core.transform.position += moveSpeed * Time.deltaTime * Core.transform.forward;
                return true;
            }
            return false;
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
            Core.transform.LookAt(Target.transform);
            if (Core.transform.localEulerAngles.x > 190) Core.transform.localEulerAngles = Core.transform.localEulerAngles.AddX(-10);
            if (Core.transform.localEulerAngles.x < 170) Core.transform.localEulerAngles = Core.transform.localEulerAngles.AddX(10);
            while (dalte > 0)
            {
                dalte = Mathf.Max(0, dalte - Time.deltaTime);
                Core.transform.position = Vector3.Lerp(Core.transform.position, PastOneTarget.transform.position - Core.transform.forward * 10, FollowCurve.Evaluate(1 - dalte));
                yield return null;
            }
            Core.transform.position = Vector3.Lerp(Core.transform.position, PastOneTarget.transform.position - Core.transform.forward * 10, 1);
        }
    }
}
