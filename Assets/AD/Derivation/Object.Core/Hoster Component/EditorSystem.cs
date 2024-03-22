using System;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.HosterSystem;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AD.Experimental.GameEditor
{
    public interface ICatchCameraRayUpdate : IADEventSystemHandler
    {
        void OnRayCatching();
    }

    public class EditorSystem : MonoBehaviour, IADSystem,IMainHoster
    {
        #region IADSystem
        public IADArchitecture Architecture { get; set; } = null;

        public Dictionary<IHosterTag, IHosterComponent> HosterComponents { get; set; } = new();

        public HierarchyItem MatchItem
        {
            get => MatchItems.Count == 0 ? null : MatchItems[0];
            set
            {
                if (MatchItems.Count == 0) MatchItems.Add(value);
                else MatchItems[0] = value;
            }
        }
        public List<HierarchyItem> MatchItems { get; private set; } = new();

        public ICanSerializeOnCustomEditor MatchTarget => this;

        public bool IsOpenListView { get => false; set { } }
        public int SerializeIndex { get => 1000031; set { } }
        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

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

        public enum CoreCameraDragObjectType
        {
            None,EditorGroup,Collider
        }

        public CoreCameraDragObjectType CoreCameraDragObjectEnable = CoreCameraDragObjectType.EditorGroup;

        #endregion

        #region ValueIndexSystem

        public ADSerializableDictionary<string, float> ValueSource = new();

        #endregion

        #region Mono

        [Tooltip(" «∑Ò∆Ù”√ƒ¨»œ◊¢≤·"), SerializeField] private bool IsRegisterSystemOnGameEditorApp = true;

        private void Start()
        {
            if (IsRegisterSystemOnGameEditorApp)
                GameEditorApp.instance.RegisterSystem(this);
        }

        public bool IsEnableDrag = true;

        public void LateUpdate()
        {
            if (IsEnableDrag)
            {
                if (CoreCameraDragObjectEnable == CoreCameraDragObjectType.EditorGroup) CoreCameraDragObject();
                else if (CoreCameraDragObjectEnable == CoreCameraDragObjectType.Collider) CoreCameraDragCollider();
            }
        }

        private void CoreCameraDragObject()
        {
            if (CoreCamera != null && CoreCamera.FoucsOneTarget != null && Mouse.current.leftButton.isPressed)
            {
                GameObject FoucesOneObject = CoreCamera.FoucsOneTarget.transform.parent.gameObject;
                if (!FoucesOneObject.TryGetComponent(out ColliderLayer colliderLayer)) return;

                Transform FoucesOne = colliderLayer.ParentGroup.transform;
                Transform CoreOne = CoreCamera.Core.transform;
                DragingFocusObject(FoucesOne, CoreOne);
            }
        }

        private void CoreCameraDragCollider()
        {
            if (CoreCamera != null && CoreCamera.FoucsOneTarget != null && Mouse.current.leftButton.isPressed)
            {
                GameObject FoucesOneObject = CoreCamera.FoucsOneTarget;

                Transform FoucesOne = FoucesOneObject.transform;
                Transform CoreOne = CoreCamera.Core.transform;
                DragingFocusObject(FoucesOne, CoreOne);
            }
        }

        private void DragingFocusObject(Transform FocusOne, Transform CoreOne)
        {
            ADEventSystemExtension.Execute<ICatchCameraRayUpdate>(FocusOne.gameObject, null, (T, P) => T.OnRayCatching());

            Vector2 dragVec = TouchPanel.DeltaDragVec;
            float DragDelta = DragSpeed * Time.deltaTime;
            if (CoreCamera.Is2D)
            {
                if (!Keyboard.current.wKey.isPressed && !Keyboard.current.sKey.isPressed && !Keyboard.current.upArrowKey.isPressed && !Keyboard.current.downArrowKey.isPressed)
                    FocusOne.position += DragDelta * dragVec.x * CoreOne.right;
                if (!Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed && !Keyboard.current.leftArrowKey.isPressed && !Keyboard.current.rightArrowKey.isPressed)
                    FocusOne.position += DragDelta * dragVec.y * CoreOne.up;
            }
            else
            {
                if (Keyboard.current.spaceKey.isPressed || Keyboard.current.leftShiftKey.isPressed)
                {
                    FocusOne.position += DragSpeed * Time.deltaTime * dragVec.y * CoreOne.forward;
                }
                else
                {
                    if (!Keyboard.current.wKey.isPressed && !Keyboard.current.sKey.isPressed && !Keyboard.current.upArrowKey.isPressed && !Keyboard.current.downArrowKey.isPressed)
                        FocusOne.position += DragDelta * dragVec.x * CoreOne.right;
                    if (!Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed && !Keyboard.current.leftArrowKey.isPressed && !Keyboard.current.rightArrowKey.isPressed)
                        FocusOne.position += DragDelta * dragVec.y * CoreOne.up;
                }
            }
        }

        #endregion

        public void Init()
        {
            IsEnableDrag = true;
            Architecture.RegisterCommand<EnableEditorSystemForDragObject>();
            Architecture.RegisterCommand<DisableEditorSystemForDragObject>();
        }

        #region IMainHoster

        public T AddHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (HosterComponents.TryGetValue(HosterSystem.HosterSystem.ObtainKey<T>(), out var component)) return (T)component;
            else
            {
                T temp = new T();
                temp.SetParent(this);
                HosterComponents.Add(HosterSystem.HosterSystem.ObtainKey<T>(), temp);
                MatchPropertiesEditors.Add(temp);
                return temp;
            }
        }

        public int AddHosterComponents(params Type[] components)
        {
            int count = 0;
            foreach (var componentType in components)
            {
                if (!HosterComponents.TryGetValue(HosterSystem.HosterSystem.ObtainKey(componentType), out var component))
                {
                    IHosterComponent temp = componentType.CreateInstance<IHosterComponent>();

                    temp.SetParent(this);
                    HosterComponents.Add(HosterSystem.HosterSystem.ObtainKey(componentType), temp);
                    MatchPropertiesEditors.Add(temp);
                    count++;
                }
            }
            return count;
        }

        public T GetHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (HosterComponents.TryGetValue(HosterSystem.HosterSystem.ObtainKey<T>(), out var component)) return (T)component;
            else return default;
        }

        public bool RemoveHosterComponent<T>() where T : IHosterComponent, new()
        {
            if (this.HosterComponents.ContainsKey(HosterSystem.HosterSystem.ObtainKey<T>()))
            {
                if (!MatchPropertiesEditors.Remove(HosterComponents[HosterSystem.HosterSystem.ObtainKey<T>()])) return false;
                if (HosterComponents.Remove(HosterSystem.HosterSystem.ObtainKey<T>())) return true;
                foreach (var component in HosterComponents)
                {
                    component.Value.Enable = false;
                }
            }
            return false;
        }

        public bool RemoveHosterComponentByKey<Key>() where Key : IHosterTag, new()
        {
            if (this.HosterComponents.ContainsKey(HosterExtension.StaticTags[typeof(Key)]))
            {
                if (!MatchPropertiesEditors.Remove(HosterComponents[HosterExtension.StaticTags[typeof(Key)]])) return false;
                if (HosterComponents.Remove(HosterExtension.StaticTags[typeof(Key)])) return true;
                foreach (var component in HosterComponents)
                {
                    component.Value.Enable = false;
                }
            }
            return false;
        }

        public void OnSerialize(HierarchyItem MatchItem)
        {
            MatchItem.SetTitle("Editor System");
        }

        public List<ICanSerializeOnCustomEditor> GetChilds()
        {
            return null;
        }

        public void ClickOnLeft()
        {

        }

        public void ClickOnRight()
        {

        }

        public void DoUpdate()
        {

        }

        #endregion

    }

    public class EnableEditorSystemForDragObject : ADCommand
    {
        public override void OnExecute()
        {
            Architecture.GetSystem<EditorSystem>().IsEnableDrag = true;
        }
    }

    public class DisableEditorSystemForDragObject : ADCommand
    {
        public override void OnExecute()
        {
            Architecture.GetSystem<EditorSystem>().IsEnableDrag = false;
        }
    }
}
