using UnityEngine;
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
                Catcher.x = T;
                action.Invoke(Catcher);
            };
            y = T =>
            {
                Catcher.y = T;
                action.Invoke(Catcher);
            };
            base.Start();
            Add(0, X.gameObject);
            Add(1, Y.gameObject);
        }
    }
}
