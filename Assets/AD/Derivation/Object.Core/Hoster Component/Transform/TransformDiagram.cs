using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AD.Experimental.GameEditor;
using AD.BASE;
using AD.Utility;

namespace AD.Experimental.HosterSystem.Diagram
{
    public class TransformDiagram : IHosterComponent
    {
        public TransformDiagram(IMainHoster that,Transform transform)
        {
            this.ThatTransform = transform;
            this.Parent = that;
        }

        private Transform ThatTransform;
        public IMainHoster Parent { get; private set; }

        public bool Enable { get; set; }
        public PropertiesItem MatchItem { get; set; }

        public ICanSerializeOnCustomEditor MatchTarget => Parent;

        public bool IsDirty { get => false; set { } } 
        public int SerializeIndex { get => -1; set => throw new ADException("Not Support"); }

        public void DoCleanup() { }

        public void DoSetup() { }

        public void DoUpdate() { }

        public void OnSerialize()
        {
            PropertiesLayout.Transform(this.ThatTransform);
        }

        public void SetParent(IMainHoster Parent)
        {
            if (Parent is HosterSystem hosterSystem)
            {
                this.Parent = hosterSystem;
                this.ThatTransform = hosterSystem.transform;
            }
            else if (Parent is HosterBase hosterBase)
            {
                this.Parent = hosterBase;
                this.ThatTransform = hosterBase.EditGroup.transform;
            }
            else
            {
                throw new ADException("Not Support");
            }
        }
    }
}
