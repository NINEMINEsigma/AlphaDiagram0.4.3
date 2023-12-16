using UnityEngine;
using AD.Experimental.GameEditor;
using AD.BASE;

namespace AD.Experimental.HosterSystem.Diagram
{
    public class TransformDiagramKey : IHosterTag { }

    public class TransformDiagram : BaseDiagram
    {
        public TransformDiagram() { }

        public TransformDiagram(IMainHoster that,Transform transform)
        {
            this.ThatTransform = transform;
            this.Parent = that;
        }

        private Transform ThatTransform;

        public override bool IsDirty { get => false; set { } } 
        public override int SerializeIndex { get => -1; set => throw new ADException("Not Support"); }

        public override void OnSerialize()
        {
            PropertiesLayout.SetUpPropertiesLayout(this);
            MatchItem.SetTitle("Transform");
            PropertiesLayout.Transform(this.ThatTransform);
            PropertiesLayout.ApplyPropertiesLayout();
        }

        #region Command

        public override void SetParent(IMainHoster Parent)
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

        #endregion
    }
}
