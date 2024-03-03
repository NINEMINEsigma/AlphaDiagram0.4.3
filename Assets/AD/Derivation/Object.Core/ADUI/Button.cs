using System;
using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AD.UI
{
    [Serializable]
    [AddComponentMenu("UI/AD/Button", 100)]
    public class Button : ADUI, IButton
    {
        public enum ButtonAnimatorMode
        {
            BOOL,
            STRING
        }

        public const string ButtonAnimatorModeIsBoolString = "ButtonAnimatorModeIsBool";

        //[Header("Animator")]
        public Animator animator = null;
        public ButtonAnimatorMode ChooseMode = ButtonAnimatorMode.BOOL;
        public string AnimatorBoolString = "IsClick";
        public string AnimatorONString = "In", AnimatorOFFString = "Out";
        //[Header("Event")]
        public ADEvent OnClick = new ADEvent(), OnRelease = new ADEvent();
        //[Header("Setting")]
        [SerializeField]
        private bool _IsClick = false;
        public bool IsClick
        {
            get
            {
                return _IsClick;
            }
            set
            {
                if (animator != null && IsKeepState)
                {
                    animator.speed = AnimationSpeed;
                    switch (ChooseMode)
                    {
                        case ButtonAnimatorMode.BOOL:
                            {
                                animator.SetBool(AnimatorBoolString, value);
                                animator.SetBool(ButtonAnimatorModeIsBoolString, true);
                            }
                            break;
                        case ButtonAnimatorMode.STRING:
                            {
                                animator.Play(value ? AnimatorONString : AnimatorOFFString);
                                animator.SetBool(ButtonAnimatorModeIsBoolString, false);
                            }
                            break;
                    }
                }
                _IsClick = value && IsKeepState;
                if (value) OnClick.Invoke();
                else OnRelease.Invoke();
            }
        }
        //[Tooltip("false时不会触发任何动画也不会保持按下的状态")]
        public bool IsKeepState = false;
        public float AnimationSpeed = 1;
        public TMP_Text title; 

        public Button()
        {
            ElementArea = "Button";
        }

        protected virtual void Start()
        {
            AD.UI.ADUI.Initialize(this);
        }

        protected virtual void OnDestroy()
        {
            AD.UI.ADUI.DestroyADUI(this);
        }

        public override void InitializeContext()
        {
            base.InitializeContext();
            Context.OnPointerClickEvent = InitializeContextSingleEvent(Context.OnPointerClickEvent, OnPointerClick);
        }

        public static AD.UI.Button Generate(string name, string buttonText, Transform parent = null)
        {
            AD.UI.Button button = GameObject.Instantiate(ADGlobalSystem.instance._Button, parent);
            button.name = name;
            button.transform.SetParent(parent, false);
            button.transform.localPosition = Vector3.zero;
            button.SetTitle(buttonText);
            return button;
        }

        public static AD.UI.Button Generate(string name, Transform parent = null)
        {
            return Generate(name, name, parent);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IsClick = !IsClick;
        }

        public Button AddListener(UnityAction action, AD.PressType type = AD.PressType.ThisFramePressed)
        {
            if (type == AD.PressType.ThisFramePressed) OnClick.AddListener(action);
            else if (type == AD.PressType.ThisFrameReleased) OnRelease.AddListener(action);
            else AD.ADGlobalSystem.AddMessage("You try to add worry listener");
            return this;
        }

        public Button RemoveListener(UnityAction action, AD.PressType type = AD.PressType.ThisFramePressed)
        {
            if (type == AD.PressType.ThisFramePressed) OnClick.RemoveListener(action);
            else if (type == AD.PressType.ThisFrameReleased) OnRelease.RemoveListener(action);
            else AD.ADGlobalSystem.AddMessage("You try to remove worry listener");
            return this;
        }

        public Button RemoveAllListeners(AD.PressType type = AD.PressType.ThisFramePressed)
        {
            if (type == AD.PressType.ThisFramePressed) OnClick.RemoveAllListeners();
            else if (type == AD.PressType.ThisFrameReleased) OnRelease.RemoveAllListeners();
            return this;
        }

        public AD.UI.Button SetTitle(string title)
        {
            if (this.title != null)
                this.title.text = title;
            return this;
        }

        IButton IButton.SetTitle(string title)
        {
            return this.SetTitle(title);
        }

        public AD.UI.Button SetView(Sprite image)
        {
            if (this.TryGetComponent<ViewController>(out var viewC))
            {
                viewC.CurrentImage = image;
            }
            else if (this.TryGetComponent<Image>(out var imageC))
            {
                imageC.sprite = image;
            }
            return this;
        }

        IButton IButton.AddListener(UnityAction action)
        {
            return AddListener(action);
        }

        IButton IButton.RemoveListener(UnityAction action)
        {
            return RemoveListener(action);
        }

        IButton IButton.RemoveAllListeners()
        {
            return RemoveAllListeners();
        }
    }
}
