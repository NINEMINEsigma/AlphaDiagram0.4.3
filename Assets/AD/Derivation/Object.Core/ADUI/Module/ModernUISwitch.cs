using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class ModernUISwitch : ADUI,IBoolButton
    {
        // Events
        public ADEvent OnEvents = new();
        public ADEvent OffEvents = new();
        public ADEvent<bool> SwitchEvents = new();

        // Settings
        private bool _isOn = false;
        public bool isOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                if (_isOn == value) return;
                _isOn = value;
                SwitchEvents.Invoke(value);
                if (value)
                {
                    SwitchAnimator.Play("Switch On");
                    OnEvents.Invoke();
                }
                else
                {
                    SwitchAnimator.Play("Switch Off");
                    OffEvents.Invoke();
                }
            }
        }
        public bool enableSwitchSounds = false;
        public bool useHoverSound = true;
        public bool useClickSound = true;

        // Resources
        private Animator switchAnimator;
        public Animator SwitchAnimator
        {
            get
            {
                if (switchAnimator == null) switchAnimator = gameObject.GetComponent<Animator>();
                return switchAnimator;
            }
        }
        public UnityEngine.UI.Button switchButton;
        public UnityEngine.UI.Button SwitchButton
        {
            get
            {
                if (switchButton == null)
                {

                    switchButton = gameObject.GetComponent<UnityEngine.UI.Button>();
                    switchButton.onClick.AddListener(AnimateSwitch);

                    if (enableSwitchSounds == true && useClickSound == true)
                    {
                        switchButton.onClick.AddListener(delegate
                        {
                            soundSource.PlayOneShot(clickSound);
                        });
                    }
                }
                return switchButton;
            }
        }
        public AudioSource soundSource;

        // Audio
        public AudioClip hoverSound;
        public AudioClip clickSound;

        void OnEnable()
        {
            if (SwitchAnimator == null)
                return;

            if (isOn == true)
            {
                SwitchAnimator.Play("Switch On");
            }
            else
            {
                SwitchAnimator.Play("Switch Off");
            }
        }

        public void Init()
        {
            _isOn = false;
        }

        private void AnimateSwitch(bool boolen)
        {
            isOn = boolen;
        }

        public void AnimateSwitch()
        {
            AnimateSwitch(isOn);
        }

        public void OnClick()
        {
            AnimateSwitch(!isOn);
        }

        public void UpdateUI()
        {
            if (isOn == true && SwitchAnimator != null && SwitchAnimator.gameObject.activeInHierarchy == true)
            {
                isOn = true;
                SwitchAnimator.Play("Switch On");
            }

            else if (isOn == false && SwitchAnimator != null && SwitchAnimator.gameObject.activeInHierarchy == true)
            {
                isOn = false;
                SwitchAnimator.Play("Switch Off");
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (enableSwitchSounds == true && useHoverSound == true && SwitchButton.interactable == true)
                soundSource.PlayOneShot(hoverSound);
        }

        public IBoolButton AddListener(UnityAction<bool> action)
        {
            SwitchEvents.AddListener(action);
            return this;
        }

        public IBoolButton RemoveListener(UnityAction<bool> action)
        {
            SwitchEvents.RemoveListener(action);
            return this;
        }
    }
}
