using System;
using UnityEngine;
using UnityEngine.Events;

namespace AD.UI
{
    public class VectorBaseUI : PropertyModule
    {
        public InputField X, Y, Z, W;

        public UnityAction<float> x, y, z, w;
        public Func<Vector2> xy;
        public Func<Vector3> xyz;
        public Func<Vector4> xyzw;
        public Func<float> ux, uy, uz, uw;

        public string past;
        public int curselect = 0;

        protected override void Start()
        {
            base.Start();
            SetupCall(X, x, 1); SetupCall(Y, y, 2); SetupCall(Z, z, 3); SetupCall(W, w, 4);
        }

        private void LateUpdate()
        {
            if (curselect == 0)
            {
                if (ux != null) X.SetText(ux.Invoke().ToString());
                if (uy != null) Y.SetText(uy.Invoke().ToString());
                if (uz != null) Z.SetText(uz.Invoke().ToString());
                if (uw != null) W.SetText(uw.Invoke().ToString());
                if (xy != null)
                {
                    var cxy = xy.Invoke();
                    X.SetText(cxy.x.ToString());
                    Y.SetText(cxy.y.ToString());
                }
                if (xyz != null)
                {
                    var cxyz = xyz.Invoke();
                    X.SetText(cxyz.x.ToString());
                    Y.SetText(cxyz.y.ToString());
                    Z.SetText(cxyz.z.ToString());
                }
                if (xyzw != null)
                {
                    var cxyzw = xyzw.Invoke();
                    X.SetText(cxyzw.x.ToString());
                    Y.SetText(cxyzw.y.ToString());
                    Z.SetText(cxyzw.z.ToString());
                    W.SetText(cxyzw.w.ToString());
                }
            }
            /*
            switch (curselect)
            {
                case 1:
                    {
                        if (uy != null) Y.SetText(uy.Invoke().ToString());
                        if (uz != null) Z.SetText(uz.Invoke().ToString());
                        if (uw != null) W.SetText(uw.Invoke().ToString());
                    }
                    break;
                case 2:
                    {
                        if (ux != null) Y.SetText(ux.Invoke().ToString());
                        if (uz != null) Z.SetText(uz.Invoke().ToString());
                        if (uw != null) W.SetText(uw.Invoke().ToString());
                    }
                    break;
                case 3:
                    {
                        if (ux != null) X.SetText(ux.Invoke().ToString());
                        if (uy != null) Y.SetText(uy.Invoke().ToString());
                        if (uw != null) W.SetText(uw.Invoke().ToString());
                    }
                    break;
                case 4:
                    {
                        if (ux != null) X.SetText(ux.Invoke().ToString());
                        if (uy != null) Y.SetText(uy.Invoke().ToString());
                        if (uz != null) Z.SetText(uz.Invoke().ToString());
                    }
                    break;
            }
            */
        }

        private void SetupCall(InputField target, UnityAction<float> targetCall, int curs)
        {
            if (target != null && targetCall != null)
            {
                target.AddListener(T =>
                {
                    past = T;
                    curselect = curs;
                }, PressType.OnSelect);
                target.AddListener(T =>
                {
                    if (float.TryParse(target.text, out float value)) targetCall.Invoke(value);
                    else target.SetText(past);
                    curselect = 0;
                }, PressType.OnEnd);
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

        public VectorBaseUI InitValue(params float[] args)
        {
            if (args.Length >= 2)
            {
                X.SetText(args[0].ToString());
                Y.SetText(args[1].ToString());
            }
            if (args.Length > 2) Z.SetText(args[2].ToString());
            if (args.Length > 3) W.SetText(args[3].ToString());
            return this;
        }

    }
}
