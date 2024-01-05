using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.UI;
using AD.Utility;
using Mono.Cecil.Cil;
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

        public override void Destory()
        {
            base.Destory();
            this.value = 0;
            this.OnChange.RemoveAllListeners();
        }
    }*/

    public sealed class DataProcessor : Processor<float>
    {
        public override float Compute(float source)
        {
            return Sigmoid(source);
        }

        public static float Sigmoid(float v)
        {
            return 1.0f / Mathf.Exp((-v) / T);
        }

        /// <summary>
        /// Î±ÎÂ¶È
        /// </summary>
        public static float T = 0.637582f;
    }

    [Serializable]
    public class LinearNeuron : Neuron<float, DataProcessor>, ICanSerializeOnCustomEditor
    {
        public bool IsMainTop = false;
        public MeshRenderer meshRenderer;

        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
        public ICanSerializeOnCustomEditor ParentTarget { get; set ; }

        public int SerializeIndex => (int)this.instanceIndex;

#if NEURON_MONO
        private IEnumerator InitializeMonoMode()
        {
            yield return new WaitForEndOfFrame();
            MatchHierarchyEditor = new HierarchyBlock<LinearNeuron>(this, this.instanceIndex.ToString());
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
            foreach (var tchild in transform)
            {
                tchild.As<Transform>().GetComponent<LinearNeuron>().SetParent(this);
            }
            ADGlobalSystem.instance.StartCoroutine(InitializeMonoMode());
#else
        public Neuron()
        {
            meshRenderer.material = new Material(meshRenderer.material);
            HierarchyItem.MaxOpenSingleItemSum = 35;
            MatchHierarchyEditor = new HierarchyBlock<LinearNeuron>(this, this.instanceIndex.ToString());
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new PropertiesBlock<LinearNeuron>(this,"Info",0)
            };
            if (IsMainTop) GameEditorApp.instance.GetController<Hierarchy>().AddOnTop(this.MatchHierarchyEditor);
#endif
        }

        private void DoUpdate()
        {
            meshRenderer.material.color = ColorExtension.LerpTo(Color.white, Color.black, Data);
        }

        public void ClickOnLeft()
        {

        }

        public void ClickOnRight()
        {
            this.ObtainResult();
        }

        protected override float ObtainResult()
        {
            base.ObtainResult();
            DoUpdate();
            return this.Data;
        }

        public List<LinearNeuron> NeuronChilds => Childs.GetSubList<Neuron<float, DataProcessor>, LinearNeuron>();

        [ADSerialize(layer = "Info", index = 0, message = "Value", methodName = ADSerializeAttribute.DefaultKey)]
        public float NeuronValue
        {
            get => base.Data;
            set
            {
                base.Data = value;
            }
        }

        public List<ICanSerializeOnCustomEditor> GetChilds()
        {
            return Childs.GetSubList<Neuron<float, DataProcessor>, ICanSerializeOnCustomEditor>();
        }

        protected override float CountAdd(float a, float b)
        {
            return a + b;
        }
    }
}
