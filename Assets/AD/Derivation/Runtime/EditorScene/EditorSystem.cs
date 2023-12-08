using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AD.Experimental.GameEditor
{
    public class EditorSystem : MonoBehaviour, IADSystem
    {
        #region IADSystem
        public IADArchitecture Architecture { get; protected set; } = null;

        public IADArchitecture ADInstance()
        {
            return Architecture;
        }

        public void RegisterCommand<T>() where T : class, IADCommand, new()
        {
            Architecture.RegisterCommand<T>();
        }
        public void UnRegisterCommand<T>() where T : class, IADCommand, new()
        {
            Architecture.UnRegister<T>();
        }
        public void SendCommand<T>() where T : class, IADCommand, new()
        {
            Architecture.SendCommand<T>();
        }

        public void SetArchitecture(IADArchitecture target)
        {
            Architecture = target;
        }

        public T GetController<T>() where T : class, IADController, new()
        {
            return Architecture.GetController<T>();
        }
        #endregion

        #region Resources

        public CameraCore CoreCamera;
        public float DragSpeed = 5;

        public TouchPanel TouchPanel;

        #endregion

        #region Mono

        private void Start()
        {
            GameEditorApp.instance.RegisterSystem(this);
        }

        public void LateUpdate()
        {
            CoreCameraDragObject();
        }

        private void CoreCameraDragObject()
        {
            if (CoreCamera != null && CoreCamera.FoucsOneTarget != null && Mouse.current.leftButton.isPressed)
            {
                GameObject FoucesOneObject = CoreCamera.FoucsOneTarget.transform.parent.gameObject;
                if (!FoucesOneObject.TryGetComponent(out ColliderLayer colliderLayer)) return;

                Transform FoucesOne = colliderLayer.ParentGroup.transform;
                Transform CoreOne = CoreCamera.Core.transform;

                if (Keyboard.current.spaceKey.isPressed || Keyboard.current.leftShiftKey.isPressed)
                {
                    FoucesOne.position += DragSpeed * Time.deltaTime * TouchPanel.dragVec.y * CoreOne.forward;
                }
                else
                {
                    if (!Keyboard.current.wKey.isPressed && !Keyboard.current.sKey.isPressed && !Keyboard.current.upArrowKey.isPressed && !Keyboard.current.downArrowKey.isPressed)
                        FoucesOne.position += DragSpeed * Time.deltaTime * TouchPanel.dragVec.x * CoreOne.right;
                    if (!Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed && !Keyboard.current.leftArrowKey.isPressed && !Keyboard.current.rightArrowKey.isPressed)
                        FoucesOne.position += DragSpeed * Time.deltaTime * TouchPanel.dragVec.y * CoreOne.up;
                }
            }
        }

        #endregion

        public void Init()
        {

        }
    }
}
