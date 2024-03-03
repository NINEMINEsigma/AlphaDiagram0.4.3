using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.Neuron.AudioSampling
{
    /*
    public sealed class Data : VectorAsset, IProperty_Value<float>
    {
        public ADEvent<float> OnChange = new();

        private float value;

        public float Value
        {
            get { return value; }
            set { OnChange.Invoke(this.value = value); }
        }

        public override int CompareTo(VectorAsset other)
        {
            App.Assert(this);
            if (other == null) throw new ArgumentNullException();
            if (other.GetType() != typeof(Data)) throw new ArgumentException("Difference Type");
            App.Assert(other);
            return (int)(this.As<Data>().Value - other.As<Data>().Value);
        }

        public override bool Equals(VectorAsset other)
        {
            App.Assert(this);
            if (other == null) throw new ArgumentNullException();
            if (other.GetType() != typeof(Data)) throw new ArgumentException("Difference Type");
            App.Assert(other);
            return other.As<Data>().Value == this.As<Data>().Value;
        }

        public Data(float value = 0)
        {
            this.value = value;
        }

        public override void DestroyADUI()
        {
            base.DestroyADUI();
            this.value = 0;
            this.OnChange.RemoveAllListeners();
        }
    }*/

    [Serializable]
    public sealed class DataProcessor : Processor<float>
    {
        public override float Compute(float source)
        {
            return Sigmoid(source);
        }

        public float Sigmoid(float v)
        {
            return 1.0f / (1 + Mathf.Exp(-(v / T)));
        }

        /// <summary>
        /// Î±ÎÂ¶È
        /// </summary>
        public float T = 0.637582f;
    }

    [Serializable]
    public class LinearNeuron : Neuron<float, DataProcessor>, ICanSerializeOnCustomEditor
    {
        public bool IsMainTop = false;
        public MeshRenderer meshRenderer;

        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        public int SerializeIndex => (int)this.instanceIndex;

#if NEURON_MONO
        private IEnumerator InitializeMonoMode()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            MatchHierarchyEditor = new HierarchyBlock<LinearNeuron>(this, this.instanceIndex.ToString());
            this.gameObject.name = this.instanceIndex.ToString();
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new PropertiesBlock<LinearNeuron>(this,"Info",0)
            };
            if (IsMainTop) GameEditorApp.instance.GetController<Hierarchy>().AddOnTop(this.MatchHierarchyEditor);
        }
#endif

#if NEURON_MONO
        public override void Start()
        {
            base.Start();
            ADGlobalSystem.instance.StartCoroutine(InitializeMonoMode());
#else
        public LinearNeuron() :base()
        {
            MatchHierarchyEditor = new HierarchyBlock<LinearNeuron>(this, this.instanceIndex.ToString());
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new PropertiesBlock<LinearNeuron>(this,"Info",0)
            };
            if (IsMainTop) GameEditorApp.instance.GetController<Hierarchy>().AddOnTop(this.MatchHierarchyEditor);
#endif
            meshRenderer.material = new Material(meshRenderer.material);
            HierarchyItem.MaxOpenSingleItemSum = 10;
        }

        private void DoUpdate()
        {
            meshRenderer.material.color = ColorExtension.LerpTo(Color.black, Color.white, (this.Childs.Count == 0) ? Data : ((Data - 0.5f) * 2));
        }

        public void ClickOnLeft()
        {
            this.ObtainResult();
        }

        public void ClickOnRight()
        {
        }

        protected override float ObtainResult()
        {
            base.ObtainResult();
            //Debug.Log(this.instanceIndex.ToString()+"/"+ Data.ToString());
            DoUpdate();
            return this.Data;
        }

        public List<LinearNeuron> NeuronChilds => Childs.GetSubList<Neuron<float, DataProcessor>, LinearNeuron>();

        [ADSerialize(layer = "Info", index = 0, message = "$0,1,Value")]
        public float NeuronValue
        {
            get => ObtainResult();
            set
            {
                base.Data = value;
                GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors.All(T =>
                {
                    T.MatchTarget.As<LinearNeuron>().ObtainResult();
                    return true;
                });
                //ObtainResult();
            }
        }

        [ADSerialize(layer = "Info", index = 0, message = "$0,3,T")]
        public float NeuronT
        {
            get => Processor.T;
            set
            {
                Processor.T = value;
                GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors.All(T =>
                {
                    T.MatchTarget.As<LinearNeuron>().ObtainResult();
                    return true;
                });
                //ObtainResult();
            }
        }
        [ADSerialize(layer = "Info", index = 2, message = "Weight")]
        public float NeuronWeight
        {
            get => base.weight;
            set { base.weight = value; }
        }

        public List<ICanSerializeOnCustomEditor> GetChilds()
        {
            return Childs.GetSubList<Neuron<float, DataProcessor>, ICanSerializeOnCustomEditor>();
        }

        protected override float CountAdd(float a, float b, float weight)
        {
            return a + b * weight;
        }
    }
}
