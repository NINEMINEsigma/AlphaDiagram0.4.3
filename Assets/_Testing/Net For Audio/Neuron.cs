using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.Neuron
{
    [Serializable]
    public class DestroyException : ADException
    {
        public DestroyException() : base("This Asset has been destroy") { }
    }

    public abstract class VectorAsset : IComparable<VectorAsset>, IEquatable<VectorAsset>
    {
        public abstract VectorAsset Add(VectorAsset _Right);

        public abstract int CompareTo(VectorAsset other);
        public abstract bool Equals(VectorAsset other);

        public bool IsDestory = false;

        public virtual void Destory()
        {
            IsDestory = true;
        }

        public static implicit operator bool(VectorAsset other) { return other != null && other.IsDestory; }
        public static VectorAsset operator +(VectorAsset a, VectorAsset b) { return a.Add(b); }
    }

    public abstract class Processor<_Variant>
    {
        public abstract _Variant Compute(_Variant source);
    }

    [Serializable]
    public abstract class Neuron<_Variant, _Processor>
#if NEURON_MONO
        : MonoBehaviour
#endif
        where _Processor : Processor<_Variant>, new()
    {
#if NEURON_MONO
        public virtual void Start()
#else
        public Neuron()
#endif
        {
            this.instanceIndex = App.instance.InstanceTotalGenerated++;
            this.Processor = new _Processor();
        }

        public ulong instanceIndex { get; private set; }
        public _Processor Processor;
        public _Variant Data;
        public float weight = 0.36781f;

        public List<Neuron<_Variant, _Processor>> Childs = new();

        public ADEvent<_Variant> OnUpdate;

        protected virtual _Variant ObtainResult()
        {
            if (Childs.Count == 0) return Data;
            _Variant NewData = default(_Variant);
            foreach (var child in Childs)
            {
                NewData = CountAdd(NewData, child.ObtainResult(), child.weight);
            }
            NewData = this.Processor.Compute(NewData);
            OnUpdate.Invoke(NewData);
            return Data = NewData;
        }

        protected abstract _Variant CountAdd(_Variant a, _Variant b, float weight);

        private void OnDestroy()
        {
            Processor = default;
            Data = default;
        }
    }
}
