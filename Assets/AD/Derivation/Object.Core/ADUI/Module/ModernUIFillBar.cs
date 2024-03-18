using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AD.UI
{
    public interface IHaveValue
    {
        float value { get; }
        float Value { get; }
        float GetValue();
    }

    public class ModernUIFillBar : PropertyModule, IHaveValue, INumericManager<float>
    {
        public ModernUIFillBar()
        {
            this.ElementArea = nameof(ModernUIFillBar);
        }


        // Content
        private float lastPercent;
        [Range(0, 1)] public float currentPercent;
        public float minValue = 0;
        public float maxValue = 100;
        public ADEvent<float> OnValueChange = new();
        public ADEvent<float> OnEndChange = new();
        public ADEvent<float> OnTransValueChange = new();
        public ADEvent<float> OnEndTransChange = new();

        public float value => (maxValue - minValue) * currentPercent + minValue;
        public float Value => (maxValue - minValue) * currentPercent + minValue;
        // Resources
        public Image loadingBar;
        public TextMeshProUGUI textPercent;
        public TextMeshProUGUI textValue;

        public bool IsLockByScript = true;

        // Settings  
        public bool IsPercent = true;
        public bool IsInt = false;
        public float DragChangeSpeed = 0.5f;
        public string NumericManagerName = "DefaultFillBar";

        public override void InitializeContext()
        {
            base.InitializeContext();
            Context.OnDragEvent = InitializeContextSingleEvent(Context.OnDragEvent, OnDrag);
        }

        public void SetValue(float t)
        {
            SetPerecent((t - minValue) / (maxValue - minValue));
        }

        public void SetPerecent(float t)
        {
            if (IsLockByScript) currentPercent = Mathf.Clamp(t, 0, 1);
            else loadingBar.fillAmount = t;
        }

        public void SetPerecent(float t, float a, float b)
        {
            if (IsLockByScript) currentPercent = Mathf.Clamp(t, 0, 1);
            else loadingBar.fillAmount = t;
            minValue = a;
            maxValue = b;
            IsInt = false;
        }

        public void SetPerecent(float t, int a, int b)
        {
            if (IsLockByScript) currentPercent = Mathf.Clamp(t, 0, 1);
            else loadingBar.fillAmount = t;
            minValue = a;
            maxValue = b;
            IsInt = true;
        }

        public void OnDrag(PointerEventData data)
        {
            if (!IsLockByScript) loadingBar.fillAmount += data.delta.x * Time.deltaTime * DragChangeSpeed;
        }

        protected override void Start()
        {
            base.Start();
            ADGlobalSystem.instance.FloatValues.TryGetValue(this.NumericManagerName + "_Percent", out float value);
            this.currentPercent = loadingBar.fillAmount = this.lastPercent = value;
            textPercent.text = currentPercent.ToString("F2") + (IsPercent ? "%" : "");
            textValue.text = GetValue().ToString("F2");
        }

        private bool IsUpdateAndInvoke = true;
        public void LateUpdate()
        {
            if (IsLockByScript) loadingBar.fillAmount = Mathf.Clamp(currentPercent, 0, 1);
            else currentPercent = loadingBar.fillAmount;

            if (currentPercent == lastPercent)
            {
                if (!IsUpdateAndInvoke)
                {
                    IsUpdateAndInvoke = true;
                    OnEndChange.Invoke(currentPercent);
                    OnEndTransChange.Invoke(Value);
                }
                return;
            }

            IsUpdateAndInvoke = false;
            lastPercent = currentPercent;
            OnValueChange.Invoke(currentPercent);
            OnTransValueChange.Invoke(Value);

            textPercent.text = currentPercent.ToString("F2") + (IsPercent ? "%" : "");
            textValue.text = GetValue().ToString("F2");
        }

        public void UpdateWithInvoke()
        {
            if (IsLockByScript) loadingBar.fillAmount = Mathf.Clamp(currentPercent, 0, 1);
            else currentPercent = loadingBar.fillAmount;

            if (currentPercent == lastPercent)
            {
                if (!IsUpdateAndInvoke)
                {
                    IsUpdateAndInvoke = true;
                }
                return;
            }

            IsUpdateAndInvoke = false;
            lastPercent = currentPercent;

            textPercent.text = currentPercent.ToString("F2") + (IsPercent ? "%" : "");
            textValue.text = GetValue().ToString("F2");
        }

        public float GetValue()
        {
            return IsInt ? (int)value : value;
        }

        public int GetIntValue()
        {
            return (int)value;
        }

        public void Set(float min, float max)
        {
            currentPercent = 0;
            minValue = min;
            maxValue = max;
        }

        public void Set(float percent, float min, float max)
        {
            currentPercent = percent;
            minValue = min;
            maxValue = max;
        }

        public void Init()
        {
            if (ADGlobalSystem.instance.FloatValues.TryGetValue(this.NumericManagerName + "_Percent", out this.currentPercent)) return;
            currentPercent = 0;
            minValue = 0;
            maxValue = 1;
        }

        public void NumericManager(float value)
        {
            SetValue_NumericManagerName(this.NumericManagerName + "_Percent", this.currentPercent);
            SetValue_NumericManagerName(this.NumericManagerName, this.Value);
        }
    }
}

