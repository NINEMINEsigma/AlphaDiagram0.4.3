using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AD.UI
{
    public interface IHaveValue
    {
        float value { get; }
        float Value { get; }
        float GetValue();
    }

    public class ModernUIFillBar : PropertyModule, IHaveValue
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

        public void LateUpdate()
        {
            if (currentPercent == lastPercent) return;

            lastPercent = currentPercent;
            OnValueChange.Invoke(currentPercent);

            if (IsLockByScript) loadingBar.fillAmount = Mathf.Clamp(currentPercent, 0, 1);
            else currentPercent = loadingBar.fillAmount;

            textPercent.text = currentPercent.ToString("F2") + (IsPercent ? "%" : "");
            textValue.text = GetValue().ToString("F2");
        }

        public float GetValue()
        {
            return IsInt ? (int)value : value;
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
            currentPercent = 0;
            minValue = 0;
            maxValue = 1;
        }
    }
}

