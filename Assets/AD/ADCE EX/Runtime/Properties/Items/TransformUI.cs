using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.UI
{
    public class TransformUI : PropertyModule
    {
        public Text Title;
        public ModernUIInputField Position, Rotation, Scale;
        public Transform Current;
        public int WitchInput = 0;
        public string Past;

        protected override void Start()
        {
            base.Start();
            Position.AddListener(T =>
            {
                WitchInput = 1;
                Past = T;
            }, ModernUIInputField.PressType.OnSelect);
            Rotation.AddListener(T =>
            {
                WitchInput = 2;
                Past = T;
            }, ModernUIInputField.PressType.OnSelect);
            Scale.AddListener(T =>
            {
                WitchInput = 3;
                Past = T;
            }, ModernUIInputField.PressType.OnSelect);

            //TODO
            Position.AddListener(T =>
            {
                WitchInput = 1;
                Past = T;
            }, ModernUIInputField.PressType.OnSelect);
            Rotation.AddListener(T =>
            {
                WitchInput = 2;
                Past = T;
            }, ModernUIInputField.PressType.OnSelect);
            Scale.AddListener(T =>
            {
                WitchInput = 3;
                Past = T;
            }, ModernUIInputField.PressType.OnSelect);
        }

        public void DoUpdate(Transform transform)
        {
            Title.SetText("Transform");

        }

    }
}
