using System.Numerics;
using UnityEngine.Events;

namespace AD.UI
{
    public class Vector2UI : VectorBaseUI
    {
        public UnityAction<Vector2> action;

        public Vector2 Catcher;

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
            base.Start();
            Add(0, X.gameObject);
            Add(1, Y.gameObject);
        }
    }
}
