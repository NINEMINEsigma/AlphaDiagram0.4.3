using UnityEngine;
using UnityEngine.Events;

namespace AD.UI
{
    public class Vector4UI : VectorBaseUI
    {
        public UnityAction<Vector4> action;

        public Vector4 Catcher;

        protected override void Start()
        {
            x = T =>
            {
                Catcher.x = T;
                action.Invoke(Catcher);
            };
            y = T =>
            {
                Catcher.y = T;
                action.Invoke(Catcher);
            };
            z = T =>
            {
                Catcher.z = T;
                action.Invoke(Catcher);
            };
            w = T =>
            {
                Catcher.w = T;
                action.Invoke(Catcher);
            };
            base.Start();
            Add(0, X.gameObject);
            Add(1, Y.gameObject);
            Add(2, Z.gameObject);
            Add(3, W.gameObject);
        }
    }
}
