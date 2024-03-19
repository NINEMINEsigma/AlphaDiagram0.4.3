using AD.BASE;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AD.UI
{
    public class ModernUISwitch : ADUI,IBoolButton,INumericManager<bool>
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
        public string NumericManagerName = DefaultNumericManagerName;

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

        private void Start()
        {
            ADUI.Initialize(this);
            SwitchButton.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            ADUI.DestroyADUI(this);
        }

        public void Init()
        {
            isOn = false;
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

        public void NumericManager(bool value)
        {
            if(ADGlobalSystem.instance.IntValues.TryGetValue(this.NumericManagerName,out var intvalue))
            {
                if (isOn && intvalue == 0)
                    SetValue_NumericManagerName(this.NumericManagerName, 1);
                if (!isOn && intvalue != 0)
                    SetValue_NumericManagerName(this.NumericManagerName, 0);
            }
            else SetValue_NumericManagerName(this.NumericManagerName, this.isOn ? 1 : 0);
        }

        protected override void HowSetupByNumericManager()
        {
            if (GetValue_NumericManagerName(NumericManagerName, out int value))
                this.isOn = value != 0;
        }
    }
}
