using System;
using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AD.UI
{
    [Serializable]
    [AddComponentMenu("UI/AD/Toggle", 100)]
    public class Toggle : AD.UI.ADUI,IBoolButton
    {
        public class UnRegisterInfo
        {
            public UnRegisterInfo(UnityEngine.Events.UnityAction<bool> action, Toggle toggle)
            {
                this.action = action;
                this.toggle = toggle;
            }

            private UnityEngine.Events.UnityAction<bool> action = null;
            private Toggle toggle = null;

            public void UnRegister()
            {
                toggle.RemoveListener(this.action);
                action = null;
            }

            public void RegisterAsNew(UnityEngine.Events.UnityAction<bool> action)
            {
                toggle.RemoveListener(this.action);
                toggle.AddListener(action); 
                this.action = action;
            }
        }

        #region Attribute

        public UnityEngine.UI.Image background;
        public UnityEngine.UI.Image tab;
        public UnityEngine.UI.Image mark;
        public TMP_Text title;

        public BindProperty<bool> BoolProperty = new();
        public bool isOn
        {
            get { return BoolProperty.Get(); }
            set
            {
                if (value != BoolProperty.Get())
                {
                    BoolProperty.Set(value);
                    tab.gameObject.SetActive(!value);
                    mark.gameObject.SetActive(value);
                    actions.Invoke(value);
                }
            }
        }

        public void Bind(BindProperty<bool> property)
        {
            BoolProperty = property;
            isOn = BoolProperty.Get();
        }

        private RegisterInfo __unregisterInfo;

        [SerializeField]private ADEvent<bool> actions = new ADEvent<bool>();

        #endregion 

        public Toggle()
        {
            ElementArea = "Toggle";
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this);
            __unregisterInfo = ADGlobalSystem.AddListener(Mouse.current.leftButton, () =>
            {
                if (!Selected) return;
                isOn = !isOn;
            }, AD.PressType.ThisFramePressed); 
        }
        protected void OnDestroy()
        {
            __unregisterInfo?.UnRegister();
            AD.UI.ADUI.DestroyADUI(this);
        }

        public void Init()
        {
            BoolProperty.Set(false);
            tab.gameObject.SetActive(true);
            mark.gameObject.SetActive(false);
        }

        #region Function

        public static AD.UI.Toggle Generate(string name = "New Toggle", Transform parent = null)
        {
            AD.UI.Toggle toggle = GameObject.Instantiate(ADGlobalSystem.instance._Toggle);
            toggle.transform.SetParent(parent, false);
            toggle.transform.localPosition = Vector3.zero;
            toggle.name = name;
            return toggle;
        }

        public AD.UI.Toggle AddListener(UnityEngine.Events.UnityAction<bool> action)
        {
            actions.AddListener(action);
            return this;
        }
        public AD.UI.Toggle AddListener(UnityEngine.Events.UnityAction<bool> action, out UnRegisterInfo info)
        {
            info = new UnRegisterInfo(action, this);
            actions.AddListener(action);
            return this;
        }
        public AD.UI.Toggle RemoveListener(UnityEngine.Events.UnityAction<bool> action)
        {
            actions.RemoveListener(action); 
            return this;
        }

        public AD.UI.Toggle SetTitle(string title)
        {
            this.title.text = title;
            return this; 
        }

        IBoolButton IBoolButton.AddListener(UnityAction<bool> action)
        {
            return this.AddListener(action);
        }

        IBoolButton IBoolButton.RemoveListener(UnityAction<bool> action)
        {
            return this.RemoveListener(action);
        }

        #endregion

    }
}
