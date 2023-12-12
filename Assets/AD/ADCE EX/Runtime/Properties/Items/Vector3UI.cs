using System.Numerics;
using UnityEngine.Events;

namespace AD.UI
{
    public class Vector3UI : VectorBaseUI
    {
        public UnityAction<Vector3> action;

        public Vector3 Catcher;

        protected override void Start()
        {
            x = T =>
            {
                Catcher.X = T;
                action.Invoke(Catcher);
            };
            y = T =>
            {
                Catcher.Y = T;
                action.Invoke(Catcher);
            };
            z = T =>
            {
                Catcher.Z = T;
                action.Invoke(Catcher);
            };
            base.Start();
            Add(0, X.gameObject);
            Add(1, Y.gameObject);
            Add(2, Z.gameObject);
        }
    }
}
