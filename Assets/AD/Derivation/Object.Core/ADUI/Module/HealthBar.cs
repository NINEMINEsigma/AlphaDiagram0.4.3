using System.Collections;
using AD.Utility;
using UnityEngine;

namespace AD.UI
{
    public class HealthBar : PropertyModule
    {
        protected override void Start()
        {
            base.Start();
            this.Add(0, Background.gameObject);
            this.Add(1, Undertone.gameObject);
            this.Add(2, DevelopmentFill.gameObject);
            Undertone.fillAmount = DevelopmentFill.fillAmount = currentProcess = process;
        }

        protected override void LetChildDestroy(GameObject child)
        {

        }

        protected override void LetChildAdd(GameObject child)
        {

        }

        public UnityEngine.UI.Image Background, Undertone, DevelopmentFill;
        public EaseCurve EaseCurve = new();
        public float EaseTime = 1;

        [SerializeField] private float process = 1, currentProcess = 1;

        public float Process
        {
            get => process;
            set
            {
                if (process != value)
                {
                    if (coroutine != null) StopCoroutine(coroutine);
                    if (value < process)
                    {
                        coroutine = StartCoroutine(Changing(process, value, EaseCurve, EaseTime));
                    }
                    process = DevelopmentFill.fillAmount = value;
                }
            }
        }
        public float CurrentProcess
        {
            get => currentProcess;
        }

        private Coroutine coroutine;

        private IEnumerator Changing(float startProcess, float targetProcess, EaseCurve easeCurve, float easeTime)
        {
            float currentEaseTime = easeTime;
            while (currentEaseTime > 0)
            {
                currentProcess = Undertone.fillAmount = Mathf.Lerp(targetProcess, startProcess, easeCurve.Evaluate(currentEaseTime / easeTime, true));
                yield return null;
                currentEaseTime -= Time.deltaTime;
            }
            Undertone.fillAmount = targetProcess;
            coroutine = null;
        }

        public void AddProcess(float value)
        {
            Process += value;
        }

        public void SetProcess(float value)
        {
            Process = value;
        }
    }
}
