using AD.BASE;
using UnityEngine;
using UnityEngine.Events;

namespace AD.UI
{
    public class VectorBaseUI : PropertyModule
    {
        public InputField X, Y, Z, W;

        public UnityAction<float> x, y, z, w;

        public string past;

        protected override void Start()
        {
            base.Start();
            SetupCall(X, x); SetupCall(Y, y); SetupCall(Z, z); SetupCall(W, w);
        }

        private void SetupCall(InputField target, UnityAction<float> targetCall)
        {
            if (target != null && targetCall != null)
            {
                target.AddListener(T => past = T, InputField.PressType.OnSelect);
                target.AddListener(T =>
                {
                    if (float.TryParse(target.text, out float value)) targetCall.Invoke(value);
                    else target.SetText(past);
                }, InputField.PressType.OnEnd);
            }
        }

        protected override void LetChildAdd(GameObject child)
        {
            child.SetActive(true);
        }

        protected override void LetChildDestroy(GameObject child)
        {
            child.SetActive(false);
        }

    }
}
