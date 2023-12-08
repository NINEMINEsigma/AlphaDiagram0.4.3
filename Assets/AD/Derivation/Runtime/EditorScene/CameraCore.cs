using System.Collections;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using static AD.Utility.RayExtension;

namespace AD.Utility.Object
{
    public class CameraCore : MonoBehaviour, ICanInitialize
    {
        public Camera Core;
        public GameObject Target, PastOneTarget, FoucsOneTarget;
        public RayInfo RayForm;

        public TouchPanel TouchPanel;

        public RayBehavour RayBehavourForm;

        public AnimationCurve FollowCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float dalte = 0;
        public Vector3 PastPosition = Vector3.zero;
        public Vector3 PastEulerAngle = Vector3.zero;

        public float moveSpeed = 2.5f, rotionSpeed = 1;

        public enum RayBehavour
        {
            None, Follow, JustCatch
        }

        private void Start()
        {
            RayForm = Core.GetRay();
            RayForm.mask = LayerMask.GetMask("Default");
            UpdateDirty();
        }

        void Update()
        {
            NotNoneMode(); FollowMode(); ClearCatch();
            if (Mouse.current.leftButton.isPressed != false)
            {
                if (FoucsOneTarget == null) Rotating();
            }
            else
            {
                bool isClearDirty = PrimitiveExtension.ExecuteAny(ForwardMove(), BackMove(), LeftMove(), RightMove(), UpMove(), DownMove());
                if (isClearDirty) ClearDirty();
            }
            if (
#if UNITY_EDITOR
                Keyboard.current.zKey.isPressed && Mouse.current.leftButton.wasPressedThisFrame
#else
                Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.zKey.isPressed && Mouse.current.leftButton.wasPressedThisFrame
#endif
                ) UndoPast();
        }

        private void ClearCatch()
        {
            if(Mouse.current.leftButton.wasReleasedThisFrame)
            {
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
            if (RayBehavourForm == RayBehavour.None) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                RayForm = Core.RayCatchUpdate();
                RayForm.Update();
                if (RayForm.NearestRaycastHitForm.collider != null)
                {
                    FoucsOneTarget = PastOneTarget = Target = RayForm.NearestRaycastHitForm.collider.gameObject;
                }
            }
            else
            {
                RayForm = Core.RayCatchUpdate();
                RayForm.Update();
                if (RayForm.NearestRaycastHitForm.collider != null && RayForm.NearestRaycastHitForm.collider.gameObject.TryGetComponent(out ColliderLayer layer))
                {
                    if (Mouse.current.leftButton.isPressed)
                        Target = RayForm.NearestRaycastHitForm.collider.gameObject;
                    layer.ParentGroup.DoUpdate(this);
                }
                else Target = null;
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

        private void Rotating()
        {
            if (TouchPanel != null)
            {
                Core.transform.localEulerAngles
                    = new Vector3(Core.transform.localEulerAngles.x + TouchPanel.dragVec.y * Time.deltaTime * rotionSpeed
                    , Core.transform.localEulerAngles.y - TouchPanel.dragVec.x * Time.deltaTime * rotionSpeed,
                    Core.transform.localEulerAngles.z);
            }
        }

        private bool DownMove()
        {
            if (!Keyboard.current.leftShiftKey.isPressed) return false;
            Core.transform.position -= moveSpeed * Time.deltaTime * Core.transform.up;
            return true;
        }

        private bool UpMove()
        {
            if (!Keyboard.current.spaceKey.isPressed) return false;
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

        public void Init()
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
